#if !v1_0 && !v1_1
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
	public static class GenLeaving_GetBuildingResourcesLeaveCalculator
	{
		public static void Patch(Harmony harmony)
		{
			MethodInfo original = typeof(GenLeaving)
				.GetMethod("GetBuildingResourcesLeaveCalculator",
				BindingFlags.Static | BindingFlags.NonPublic);
			HarmonyMethod transpiler = new((
				(Func<IEnumerable<CodeInstruction>,
					IEnumerable<CodeInstruction>>)
				Transpiler).Method);
			harmony.Patch(original, transpiler: transpiler);
		}

		public static IEnumerable<CodeInstruction>
			Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			ReadOnlyCollection<CodeInstruction> instructionList
				= instructions.ToList().AsReadOnly();
			int state = -1;
			DestroyMode currSectionMode = DestroyMode.Vanish;
			Label[]? jumpTable = null;
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				if (jumpTable is null)
				{
					// Skip until we are past the switch
					// If we are at switch, grab the jumpTable
					if (codeInstruction.opcode == OpCodes.Switch)
					{
						jumpTable = codeInstruction.operand as Label[];
						state = 0;
					}
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
					}
				}
				// If we are in DestroyMode.Deconstruct
				// And instructions are LoadType -> Ldftn -> new Func<int,int>
				if (currSectionMode == DestroyMode.Deconstruct
					&& instructionList[i + 1].opcode == OpCodes.Ldftn
					&& instructionList[i + 2].opcode == OpCodes.Newobj)
				{
					// Replace the 3 instructions by loading d_DeconstructReturnCount
					yield return new CodeInstruction(OpCodes.Ldsfld,
						typeof(ReturnsCount)
						.GetField(nameof(ReturnsCount.d_DeconstructReturnCount))
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
					if (codeInstruction.labels
						.Contains(jumpTable[(int)DestroyMode.KillFinalize]))
					{
						(codeInstruction.operand as FieldInfo)!
							.SetValue(null, (Func<int, int>)ReturnsCount.DestroyReturnCount);
					}
					else if (codeInstruction.labels
						.Contains(jumpTable[(int)DestroyMode.Cancel]))
					{
						(codeInstruction.operand as FieldInfo)!
							.SetValue(null, (Func<int, int>)ReturnsCount.CancelBuildingCount);
					}
					else if (codeInstruction.labels
						.Contains(jumpTable[(int)DestroyMode.FailConstruction]))
					{
						(codeInstruction.operand as FieldInfo)!
							.SetValue(null, (Func<int, int>)ReturnsCount.FailReturnCount);
					}
				}
				// Pass instruction normally
				yield return codeInstruction;
			}
			if (state != 9)
				Log.Error("[Configurable Deconstruct Percentage]: " +
					"GenLeaving_GetBuildingResourcesLeaveCalculator patching failed, " +
					"expected state to be 9, got " + state);
		}
	}
}
#endif
