using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace HealOnKill
{
    public class HealOnKillSettings : AttributeGlobalSettings<HealOnKillSettings>
    {
        public override string Id => "HealOnKillSettings_v2";
        public override string DisplayName => "Heal On Kill";
        public override string FolderName => "HealOnKill";
        public override string FormatType => "json";

        // =====================
        // GENERAL
        // =====================

        [SettingPropertyBool("Enable Mod", Order = 0)]
        [SettingPropertyGroup("General")]
        public bool EnableMod { get; set; } = true;

        [SettingPropertyBool("Allow NPC Heal", Order = 1,
            HintText = "If enabled, allied troops also gain life leech.")]
        [SettingPropertyGroup("General")]
        public bool AllowNpcHeal { get; set; } = true;


        // =====================
        // LIFE LEECH VALUES
        // =====================

        [SettingPropertyFloatingInteger("Player Life Leech %", 0f, 1f, Order = 0,
            HintText = "Percentage of inflicted damage converted to HP for the player.")]
        [SettingPropertyGroup("Life Leech")]
        public float PlayerLifeLeech { get; set; } = 0.15f;

        [SettingPropertyFloatingInteger("Troop Life Leech %", 0f, 1f, Order = 1,
            HintText = "Percentage of inflicted damage converted to HP for allied troops.")]
        [SettingPropertyGroup("Life Leech")]
        public float TroopLifeLeech { get; set; } = 0.15f;


        // =====================
        // LOGGING
        // =====================

        [SettingPropertyBool("Player Log", Order = 0)]
        [SettingPropertyGroup("Logging")]
        public bool LogPlayer { get; set; } = true;

        [SettingPropertyBool("Troop Log", Order = 1)]
        [SettingPropertyGroup("Logging")]
        public bool LogTroops { get; set; } = true;


        // =====================
        // MÉTODOS AUXILIARES
        // =====================

        public float GetLifeLeechFor(bool isPlayer)
        {
            return isPlayer ? PlayerLifeLeech : TroopLifeLeech;
        }
    }
}
