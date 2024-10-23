using System.Text;
using AwwScrap.Common.Extensions;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace AwwScrap.UserConfig.Settings
{
    public static class DefaultSettings
    {
        public const string SettingsDescription =
            "\n\t\t1) BaseAwwScrapScalar default is 0.9f [Floating Point].  Value must be between 0.01f and 0.9f.  This controls the max return rate of resources from scrap with a 4x yield vanilla refinery." +
            "\n\t\t2) ScrapMassScalar default is 0.8f [Floating Point].  Value must be between 0.01f and 1.0f.  This is the ratio of scrap mass to component mass." +
            "\n\t\t3) ScrapProductionTimeScalar default is 0.75f [Floating Point].  Value must be between 0.01f and 100.0f.  This is the ratio of scrap production time to component production time." +
            "\n\t\t4) ScrapVolumeScalar default is 0.7f [Floating Point].  Value must be between 0.01f and 1.0f.  This is the ratio of scrap volume to component volume." +
            "\n\t\t5) ScrapUnknownItems default is true [Bool].  Value must be true or false.  This determines whether to set an unknown item to generic scrap or return the unknown item to the player (unaltered)." +
            "\n\t\t6) SurvivalKitRecycling default is true [Bool].  Value must be true or false.  This determines whether the Survival Kit is allowed to process scrap or not." +
            "\n\t";

        // User set settings

        public static float BaseAwwScrapScalar = 0.9f;
        public static float ScrapMassScalar = 0.80f;
        public static float ScrapProductionTimeScalar = 0.75f;
        public static float ScrapVolumeScalar = 0.70f;
        public static bool ScrapUnknownItems = true;
        public static bool SurvivalKitRecycling = true;

        // Mod hardcoded settings
        public static float ScrapScalar => (BaseAwwScrapScalar / 2);
        
        public static UserSettings CopyTo(UserSettings userSettings)
        {
            userSettings.SettingsDescription = SettingsDescription;
            userSettings.BaseAwwScrapScalar = BaseAwwScrapScalar.ToString().ToLower();
            userSettings.ScrapMassScalar = ScrapMassScalar.ToString().ToLower();
            userSettings.ScrapProductionTimeScalar = ScrapProductionTimeScalar.ToString().ToLower();
            userSettings.ScrapVolumeScalar = ScrapVolumeScalar.ToString().ToLower();
            userSettings.ScrapUnknownItems = ScrapUnknownItems;
            userSettings.SurvivalKitRecycling = SurvivalKitRecycling;
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
            sb.AppendFormat("{0, -4}[{1}] ScrapUnknownItems", " ", ScrapUnknownItems.ToSingleChar());
            sb.AppendLine();
            sb.AppendFormat("{0, -4}[{1}] SurvivalKitRecycling", " ", SurvivalKitRecycling.ToSingleChar());
            sb.AppendLine();
            sb.AppendLine();
            return sb;
        }
    }
}