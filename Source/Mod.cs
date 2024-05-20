using System;
using UnityEngine;
using Verse;

namespace Configurable_Deconstruct_Percentage
{
    public class ConfigurableDeconstructPercentage : Mod
    {
        private readonly DeconstructSettings settings;
        public ConfigurableDeconstructPercentage(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<DeconstructSettings>();
            LongEventHandler.QueueLongEvent(delegate
            {
                settings.ModifyDeconstructionFraction();
            }, "Config_Deconstruct_Percentage", false, null);
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            int deconstructionPercent = settings.DeconstructionPercent;
            ls.Label("DecPerc.Setting".Translate(deconstructionPercent), 
                -1f, "DecPerc.RestartNReq".Translate());
            deconstructionPercent = (int)Math.Round(ls.Slider(deconstructionPercent, 0f, 100f));
            string buffer = deconstructionPercent.ToString();
            ls.TextFieldNumeric(ref deconstructionPercent, ref buffer, 0f, 100f);
            settings.DeconstructionPercent = deconstructionPercent;
            ls.End();
        }
        public override string SettingsCategory()
        {
            return "DecPerc.Name".Translate();
        }
    }

    public class DeconstructSettings : ModSettings
    {
        public int DeconstructionPercent = 100;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref DeconstructionPercent, "Deconstruct_Percent", 0);
            base.ExposeData();
            ModifyDeconstructionFraction();
        }
        internal void ModifyDeconstructionFraction()
        {
#if v1_0
            foreach (TerrainDef allDef in DefDatabase<TerrainDef>.AllDefs)
                allDef.resourcesFractionWhenDeconstructed = DeconstructionPercent * 0.01f;
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
                allDef.resourcesFractionWhenDeconstructed = DeconstructionPercent * 0.01f;
#else
            foreach (BuildableDef allDef in DefDatabase<BuildableDef>.AllDefs)
                allDef.resourcesFractionWhenDeconstructed = DeconstructionPercent * 0.01f;
#endif
            Log.Message(
                "[Configurable Deconstruct Percentage]: Deconstruction return percentage modified to " 
                + DeconstructionPercent.ToString() + "%");
        }
    }
}
