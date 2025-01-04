using System.Globalization;
using RimWorld;
using UnityEngine;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
	public class Settings : ModSettings
	{
		public static int DeconstructPercent = 100;
#if !v1_0 && !v1_1
		public static int FailReturnPercent = 100;
		public static int DestroyReturnPercent = 100;
		public static int CancelBuildingPercent = 100;
		public static int CancelUnfinishedPercent = 100;
		public static int MinimumReturn = 1;
#endif
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref DeconstructPercent, nameof(DeconstructPercent), 100);
#if !v1_0 && !v1_1
			Scribe_Values.Look(ref FailReturnPercent, nameof(FailReturnPercent), 80);
			Scribe_Values.Look(ref DestroyReturnPercent, nameof(DestroyReturnPercent), 50);
			Scribe_Values.Look(ref CancelBuildingPercent, nameof(CancelBuildingPercent), 100);
			Scribe_Values.Look(ref CancelUnfinishedPercent, nameof(CancelUnfinishedPercent), 90);
			Scribe_Values.Look(ref MinimumReturn, nameof(MinimumReturn), 1);
#endif
			UpdateDeconstructionFraction();
		}
		internal static void UpdateDeconstructionFraction()
		{
#if v1_0
			foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
				allDef.resourcesFractionWhenDeconstructed = DeconstructPercent * 0.01f;
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
				allDef.resourcesFractionWhenDeconstructed = DeconstructPercent * 0.01f;
#else
			foreach (BuildableDef allDef in DefDatabase<BuildableDef>.AllDefs)
				allDef.resourcesFractionWhenDeconstructed = DeconstructPercent * 0.01f;
#endif
			Log.Message(string.Concat(
				"[Configurable Deconstruct Percentage]: ",
				nameof(DeconstructPercent), " = ",
				DeconstructPercent.ToString(CultureInfo.InvariantCulture), "%, "
#if !v1_0 && !v1_1
				,
				nameof(FailReturnPercent), " = ",
				FailReturnPercent.ToString(CultureInfo.InvariantCulture), "%, ",
				nameof(DestroyReturnPercent), " = ",
				DestroyReturnPercent.ToString(CultureInfo.InvariantCulture), "%, ",
				nameof(CancelBuildingPercent), " = ",
				CancelBuildingPercent.ToString(CultureInfo.InvariantCulture), "%, ",
				nameof(CancelUnfinishedPercent), " = ",
				CancelUnfinishedPercent.ToString(CultureInfo.InvariantCulture), "%, ",
				nameof(MinimumReturn), " = ",
				MinimumReturn.ToString(CultureInfo.InvariantCulture)
#endif
			));
		}
	}
}
