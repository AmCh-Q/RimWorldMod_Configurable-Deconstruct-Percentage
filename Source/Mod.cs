using RimWorld;
using UnityEngine;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
    public class ConfigurableDeconstructPercentage : Mod
    {
        public ConfigurableDeconstructPercentage(ModContentPack content)
            : base(content)
        {
            GetSettings<DeconstructSettings>();
            LongEventHandler.QueueLongEvent(delegate
            {
                DeconstructSettings.UpdateDeconstructionFraction();
#if !v1_0 && !v1_1
                Patch.ApplyPatches();
#endif
            }, "ConfigurableDeconstructPercentage", false, null);
        }
        private static void AddNumSettings(Listing_Standard ls, ref int percent, string labelKey)
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
            string buffer = num.ToString();
            ls.TextFieldNumeric(ref num, ref buffer, 0f, 100f);
            percent = num;
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Label("DecPerc.RestartNReq".Translate());
            AddNumSettings(ls, ref DeconstructSettings.DeconstructPercent,
                "DeconstructPercent");
#if !v1_0 && !v1_1
            AddNumSettings(ls, ref DeconstructSettings.FailReturnPercent,
                "FailReturnPercent");
            AddNumSettings(ls, ref DeconstructSettings.DestroyReturnPercent,
                "DestroyReturnPercent");
            AddNumSettings(ls, ref DeconstructSettings.MinimumReturn,
                "MinimumReturn");
#endif
            ls.End();
        }
        public override string SettingsCategory()
            => "DecPerc.Name".Translate();
    }

    public partial class DeconstructSettings : ModSettings
    {
        public static int DeconstructPercent = 100;
#if !v1_0 && !v1_1
        public static int FailReturnPercent = 100;
        public static int DestroyReturnPercent = 100;
        public static int MinimumReturn = 1;
#endif
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref DeconstructPercent, nameof(DeconstructPercent), 100);
#if !v1_0 && !v1_1
            Scribe_Values.Look(ref FailReturnPercent, nameof(FailReturnPercent), 80);
            Scribe_Values.Look(ref DestroyReturnPercent, nameof(DestroyReturnPercent), 50);
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
#elif v1_1
            foreach (BuildableDef allDef in DefDatabase<BuildableDef>.AllDefs)
                allDef.resourcesFractionWhenDeconstructed = DeconstructPercent * 0.01f;
#endif
            Log.Message(string.Concat(
                "[Configurable Deconstruct Percentage]: ",
                nameof(DeconstructPercent), " = " , DeconstructPercent.ToString(), "%, "
#if !v1_0 && !v1_1
                , nameof(FailReturnPercent), " = ", FailReturnPercent.ToString(), "%, ",
                nameof(DestroyReturnPercent), " = ", DestroyReturnPercent.ToString(), "%, ",
                nameof(MinimumReturn), " = ", MinimumReturn.ToString()
#endif
            ));
        }
    }
}
