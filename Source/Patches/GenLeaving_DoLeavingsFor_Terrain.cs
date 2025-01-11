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
	public static class GenLeaving_DoLeavingsFor_Terrain
	{
		public static void Patch(Harmony harmony)
		{
			MethodInfo original = typeof(GenLeaving)
				.GetMethod(nameof(GenLeaving.DoLeavingsFor),
				BindingFlags.Static | BindingFlags.Public, null,
				[typeof(TerrainDef), typeof(IntVec3), typeof(Map)], null);
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
			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction codeInstruction = instructionList[i];
				yield return codeInstruction;
				if (state == -1 && i + 5 < instructionList.Count
					&& codeInstruction.LoadsField(
						typeof(ThingDefCountClass)
						.GetField(nameof(ThingDefCountClass.count)))
					&& instructionList[i + 1].opcode == OpCodes.Conv_R4
					&& instructionList[i + 3].LoadsField(
						typeof(BuildableDef)
						.GetField(nameof(BuildableDef.resourcesFractionWhenDeconstructed)))
					&& instructionList[i + 4].opcode == OpCodes.Mul
					&& instructionList[i + 5].Calls(
						((Func<float, int>)GenMath.RoundRandom).Method))
				{
					state = 0;
					yield return new CodeInstruction(OpCodes.Call,
						((Func<int, int>)ReturnsCount.DeconstructReturnCount).Method);
					i += 5;
				}
			}
			if (state != 0)
				Log.Error("[Configurable Deconstruct Percentage]: " +
					"GenLeaving_DoLeavingsFor_Terrain patching failed, " +
					"expected state to be 0, got " + state);
		}
	}
}
#endif
