#if !v1_0 && !v1_1
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
	public static class UnfinishedThing_Destroy
	{
		public static void Patch(Harmony harmony)
		{
			MethodInfo original = typeof(UnfinishedThing)
				.GetMethod(nameof(UnfinishedThing.Destroy),
				BindingFlags.Instance | BindingFlags.Public);
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
				// Vanilla game is converting int count to float, multiply, then roundrandom
				// We replace these code by passing count to our method instead
				if (state == -1
					&& codeInstruction.opcode == OpCodes.Ldfld
					&& instructionList[i + 1].opcode == OpCodes.Conv_R4
					&& instructionList[i + 2].opcode == OpCodes.Ldc_R4
					&& instructionList[i + 3].opcode == OpCodes.Mul
					&& instructionList[i + 4].Calls(((Func<float, int>)GenMath.RoundRandom).Method))
				{
					state = 0;
					yield return new CodeInstruction(OpCodes.Call,
						((Func<int, int>)ReturnsCount.CancelUnfinishedCount).Method);
					i += 4;
				}
			}
			if (state != 0)
				Log.Error("[Configurable Deconstruct Percentage]: " +
					"UnfinishedThing_Destroy patching failed, " +
					"expected state to be 0, got " + state);
		}
	}
}
#endif
