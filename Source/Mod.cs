using System.Globalization;
using System.Reflection;
using UnityEngine;
using Verse;
#if !v1_0 && !v1_1
using HarmonyLib;
#endif

[assembly: AssemblyVersion("2.1.0.0")]
namespace Configurable_Deconstruct_Percentage
{
	public class ConfigurableDeconstructPercentage : Mod
	{
#if !v1_0 && !v1_1
		public static readonly Harmony harmony
			= new(id: "AmCh.Configurable_Deconstruct_Percentage");
#endif
		public ConfigurableDeconstructPercentage(ModContentPack content)
			: base(content)
		{
			GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				Settings.UpdateDeconstructionFraction();
#if !v1_0 && !v1_1
				GenLeaving_GetBuildingResourcesLeaveCalculator.Patch(harmony);
				UnfinishedThing_Destroy.Patch(harmony);
#endif
			}, "ConfigurableDeconstructPercentage", false, null);
		}
		public override string SettingsCategory()
			=> "DecPerc.Name".Translate();
		public override void DoSettingsWindowContents(Rect inRect)
		{
			static void AddNumSettings(Listing_Standard ls, ref int percent, string labelKey)
			{
				int num = percent;
				string labelStr = ("DecPerc." + labelKey).Translate(num);
				string tipStr = ("DecPerc." + labelKey + "_Tip").Translate(num);
#if v1_0 || v1_1 || v1_2 || v1_3
				ls.Label(labelStr, -1f, tipStr);
				num = Mathf.RoundToInt(ls.Slider(num, 0f, 100f));
#else
				num = Mathf.RoundToInt(ls.SliderLabeled(labelStr, num, 0f, 100f, labelPct: 0.4f, tooltip: tipStr));
#endif
				string buffer = num.ToString(CultureInfo.InvariantCulture);
				ls.TextFieldNumeric(ref num, ref buffer, 0f, 100f);
				percent = num;
			}
			base.DoSettingsWindowContents(inRect);
			Listing_Standard ls = new();
			ls.Begin(inRect);
			ls.Label("DecPerc.RestartNReq".Translate());
			AddNumSettings(ls, ref Settings.DeconstructPercent,
				nameof(Settings.DeconstructPercent));
#if !v1_0 && !v1_1
			AddNumSettings(ls, ref Settings.FailReturnPercent,
				nameof(Settings.FailReturnPercent));
			AddNumSettings(ls, ref Settings.DestroyReturnPercent,
				nameof(Settings.DestroyReturnPercent));
			AddNumSettings(ls, ref Settings.CancelBuildingPercent,
				nameof(Settings.CancelBuildingPercent));
			AddNumSettings(ls, ref Settings.CancelUnfinishedPercent,
				nameof(Settings.CancelUnfinishedPercent));
			AddNumSettings(ls, ref Settings.MinimumReturn,
				nameof(Settings.MinimumReturn));
#endif
			ls.End();
		}
	}
}
