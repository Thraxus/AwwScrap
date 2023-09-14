using System.Text;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace AwwScrap.UserConfig.Settings
{
    public static class DefaultSettings
    {
        public const string SettingsDescription =
            "\n\t\t1) BaseAwwScrapScalar default is 0.9f [Floating Point].  Value must be between 0.01f and 0.9f.  This controls the max return rate of resources from scrap with a 4x yield vanilla refinery." +
            "\n\t\t1) ScrapMassScalar default is 0.8f [Floating Point].  Value must be between 0.01f and 1.0f.  This is the ratio of scrap mass to component mass." +
            "\n\t\t1) ScrapProductionTimeScalar default is 0.75f [Floating Point].  Value must be between 0.01f and 100.0f.  This is the ratio of scrap production time to component production time." +
            "\n\t\t1) ScrapVolumeScalar default is 0.7f [Floating Point].  Value must be between 0.01f and 1.0f.  This is the ratio of scrap volume to component volume." +
            "\n\t";

        // User set settings

        public static float BaseAwwScrapScalar = 0.9f;
        public static float ScrapMassScalar = 0.80f;
        public static float ScrapProductionTimeScalar = 0.75f;
        public static float ScrapVolumeScalar = 0.70f;
        
        // Mod hardcoded settings
        public static float ScrapScalar => (BaseAwwScrapScalar / 2);
        
        public static UserSettings CopyTo(UserSettings userSettings)
        {
            userSettings.SettingsDescription= SettingsDescription;
            userSettings.BaseAwwScrapScalar = BaseAwwScrapScalar.ToString().ToLower();
            userSettings.ScrapMassScalar = ScrapMassScalar.ToString().ToLower();
            userSettings.ScrapProductionTimeScalar = ScrapProductionTimeScalar.ToString().ToLower();
            userSettings.ScrapVolumeScalar = ScrapVolumeScalar.ToString().ToLower();
            return userSettings;
        }

        public static StringBuilder PrintSettings()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0, -2}AwwScrap Settings", " ");
            sb.AppendLine("__________________________________________________");
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] BaseAwwScrapScalar", " ", BaseAwwScrapScalar);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] ScrapScalar", " ", ScrapScalar);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] ScrapMassScalar", " ", ScrapMassScalar);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] ScrapProductionTimeScalar", " ", ScrapProductionTimeScalar);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] ScrapVolumeScalar", " ", ScrapVolumeScalar);
            sb.AppendLine();
            sb.AppendLine();
            return sb;
        }
    }
}