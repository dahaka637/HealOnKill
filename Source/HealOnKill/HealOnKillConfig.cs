using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace HealOnKill
{
    [Serializable]
    public class HealOnKillConfig
    {
        // Master enable
        public bool EnableMod { get; set; } = true;

        // Core settings
        public float LifeLeechPercent { get; set; } = 0.15f;
        public float KillHealPercent { get; set; } = 0.15f;
        public bool AllowNpcHeal { get; set; } = true;
        public bool HealOnlyOnKill { get; set; } = false;

        // Legacy toggle (mantido por compatibilidade; se só ele vier true, loga tudo)
        public bool ShowFeedback { get; set; } = false;

        // New: separate log toggles
        public bool LogPlayer { get; set; } = true;
        public bool LogTroops { get; set; } = true;

        // Extra flat healing
        public float TroopExtraHeal { get; set; } = 0f;
        public float PlayerExtraHeal { get; set; } = 0f;

        // Colors (ARGB hex)
        public string PlayerLogColor { get; set; } = "FF00FF00";
        public string TroopLogColor { get; set; } = "FF00B7FF";

        public uint GetPlayerLogColorOr(uint fallback) => ParseArgbHex(PlayerLogColor, fallback);
        public uint GetTroopLogColorOr(uint fallback) => ParseArgbHex(TroopLogColor, fallback);

        private static uint ParseArgbHex(string hex, uint fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return fallback;

            string s = hex.Trim();

            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(2);

            if (s.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(1);

            if (s.Length != 8)
                return fallback;

            if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
                return value;

            return fallback;
        }

        public static HealOnKillConfig Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HealOnKillConfig));
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        return (HealOnKillConfig)serializer.Deserialize(fs);
                }
            }
            catch
            {
                // fallback
            }

            return new HealOnKillConfig();
        }
    }
}
