using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
    internal static class Patch
    {
        public static readonly Harmony harmony
            = new Harmony(id: "AmCh.ConfigurableDeconstructPercentage");
        public static void ApplyPatches()
        {
            MethodInfo original = typeof(GenLeaving)
                .GetMethod("GetBuildingResourcesLeaveCalculator", 
                BindingFlags.Static | BindingFlags.NonPublic);
            harmony.Patch(original, transpiler: Methods.transpiler);
        }
    }

    public static class Methods
    {
        public static readonly HarmonyMethod
            transpiler = new HarmonyMethod((
                (Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>)
                Transpiler).Method);

        public static readonly Func<int, int>
            d_DeconstructReturnCount = DeconstructReturnCount,
            d_DestroyReturnCount = DestroyReturnCount,
            d_FailReturnCount = FailReturnCount;

        public static int DeconstructReturnCount(int count)
            => ReturnCount(count, DeconstructSettings.DeconstructPercent);
        public static int DestroyReturnCount(int count)
            => ReturnCount(count, DeconstructSettings.DestroyReturnPercent);
        public static int FailReturnCount(int count)
            => ReturnCount(count, DeconstructSettings.FailReturnPercent);

        public static int ReturnCount(int count, int returnPct)
        {
            if (count == 0 || returnPct == 100)
                return count;
            int res = GenMath.RoundRandom(count * returnPct * 0.01f);
            if (res < DeconstructSettings.MinimumReturn)
                res = DeconstructSettings.MinimumReturn;
            if (res > count)
                return count;
            return res;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            ReadOnlyCollection<CodeInstruction> instructionList = instructions.ToList().AsReadOnly();
            int state = -1;
            DestroyMode currSectionMode = DestroyMode.Vanish;
            Label[] jumpTable = null;
            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction codeInstruction = instructionList[i];
                // If we are at switch, grab the jumpTable
                if (codeInstruction.opcode == OpCodes.Switch && state == -1)
                {
                    jumpTable = codeInstruction.operand as Label[];
                    state = 0;
                }
                // Skip until we are past the switch
                else if (state == -1)
                {
                    yield return codeInstruction;
                    continue;
                }
                // If we entered the section of a new DestroyMode
                // As indicated by jumpTable
                // update currSectionMode and increment count of seen modes
                foreach (DestroyMode mode in Enum.GetValues(typeof(DestroyMode)))
                {
                    if (codeInstruction.labels.Contains(jumpTable[(int)mode]))
                    {
                        currSectionMode = mode;
                        state++;
                        break;
                    }
                }
                // If we are in DestroyMode.Deconstruct
                // And instructions are LoadType -> Ldftn -> new Func<int,int>
                if (currSectionMode == DestroyMode.Deconstruct
                    && i < instructionList.Count - 2
                    && instructionList[i + 1].opcode == OpCodes.Ldftn
                    && instructionList[i + 2].opcode == OpCodes.Newobj)
                {
                    // Replace the 3 instructions by loading d_DeconstructReturnCount
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        typeof(Methods).GetField(nameof(d_DeconstructReturnCount))
                        ).WithLabels(codeInstruction.ExtractLabels());
                    i += 2;
                    continue;
                }
                // If we are in the beginning of 
                // DestroyMode.KillFinalize or DestroyMode.FailConstruction
                // Grab their fieldInfo and write it with our delegates
                // No need to replace the methods
                if (codeInstruction.opcode == OpCodes.Ldsfld)
                {
                    if (codeInstruction.labels.Contains(jumpTable[(int)DestroyMode.KillFinalize]))
                        (codeInstruction.operand as FieldInfo).SetValue(null, d_DestroyReturnCount);
                    else if (codeInstruction.labels.Contains(jumpTable[(int)DestroyMode.FailConstruction]))
                        (codeInstruction.operand as FieldInfo).SetValue(null, d_FailReturnCount);
                }
                // Pass method normally
                yield return codeInstruction;
            }
            if (state != 9)
                Log.Error("[Configurable Deconstruct Percentage]: " +
                    "GetBuildingResourcesLeaveCalculator patching failed, " +
                    "expected state to be 9, got " + state);
        }
    }
}
