using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.UserConfig.Settings;
using VRage.Scripting;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace AwwScrap.UserConfig.Controller
{
    public class SettingsController : BaseXmlUserSettings
    {
        private UserSettings _userSettings;

        public SettingsController(string modName) : base(modName) { }

        public void Initialize()
        {
            _userSettings = Get<UserSettings>();
            SettingsMapper();
            CleanUserSettings();
            Set(_userSettings);
        }

        private void CleanUserSettings()
        {
            // Nothing to do here, just leaving it in as a reminder.
        }

        private StringBuilder sb = new StringBuilder();

        public override string ToString()
        {
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        private void AppendToLog(string str1, string str2, int messageNumber)
        {
            switch (messageNumber)
            {
                case 1:
                    sb.AppendLine($"{str1} parsed! {str2}");
                    break;
                case 2:
                    sb.AppendLine($"{str1} was within expected range! {str2}");
                    break;
                case 3:
                    sb.AppendLine($"{str1} was not within expected range! {str2}");
                    break;
                case 4:
                    sb.AppendLine($"{str1} failed to parse: {str2}");
                    break;
            }
        }

        protected sealed override void SettingsMapper()
        {
            sb.AppendLine();
            sb.AppendLine();

            if (_userSettings == null)
            {
                _userSettings = new UserSettings();
                _userSettings = DefaultSettings.CopyTo(_userSettings);
                return;
            }

            _userSettings.SettingsDescription = DefaultSettings.SettingsDescription;

            float baseAwwScrapScalar;
            if (float.TryParse(_userSettings.BaseAwwScrapScalar, out baseAwwScrapScalar))
            {
                AppendToLog(nameof(baseAwwScrapScalar), baseAwwScrapScalar.ToString(), 1);
                // must be between 0.01f and 0.9f
                if (baseAwwScrapScalar >= 0.01f && baseAwwScrapScalar <= 0.9f)
                {
                    DefaultSettings.BaseAwwScrapScalar = baseAwwScrapScalar;
                    AppendToLog(nameof(baseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(baseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);

            }
            else
            {
                AppendToLog(nameof(baseAwwScrapScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.BaseAwwScrapScalar = DefaultSettings.BaseAwwScrapScalar.ToString().ToLower();
            }

            float scrapMassScalar;
            if (float.TryParse(_userSettings.ScrapMassScalar, out scrapMassScalar))
            {
                AppendToLog(nameof(scrapMassScalar), scrapMassScalar.ToString(), 1);
                // must be between 0.01f and 1.0f
                if (scrapMassScalar >= 0.01f && scrapMassScalar <= 1.0f)
                {
                    DefaultSettings.ScrapMassScalar = scrapMassScalar;
                    AppendToLog(nameof(scrapMassScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(scrapMassScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(baseAwwScrapScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapMassScalar = DefaultSettings.ScrapMassScalar.ToString().ToLower();
            }

            float scrapProductionTimeScalar;
            if (float.TryParse(_userSettings.ScrapProductionTimeScalar, out scrapProductionTimeScalar))
            {
                AppendToLog(nameof(scrapProductionTimeScalar), scrapProductionTimeScalar.ToString(), 1);
                // must be between 0.01f and 100.0f
                if (scrapProductionTimeScalar >= 0.01f && scrapProductionTimeScalar <= 100.0f)
                {
                    DefaultSettings.ScrapProductionTimeScalar = scrapProductionTimeScalar;
                    AppendToLog(nameof(scrapProductionTimeScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(scrapProductionTimeScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(baseAwwScrapScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapProductionTimeScalar = DefaultSettings.ScrapProductionTimeScalar.ToString().ToLower();
            }

            float scrapVolumeScalar;
            if (float.TryParse(_userSettings.ScrapVolumeScalar, out scrapVolumeScalar))
            {
                AppendToLog(nameof(scrapVolumeScalar), scrapVolumeScalar.ToString(), 1);
                // must be between 0.01f and 1.0f
                if (scrapVolumeScalar >= 0.01f && scrapVolumeScalar <= 1.0f)
                {
                    DefaultSettings.ScrapVolumeScalar = scrapVolumeScalar;
                    AppendToLog(nameof(scrapVolumeScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                } else
                    AppendToLog(nameof(scrapVolumeScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(scrapVolumeScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapVolumeScalar = DefaultSettings.ScrapVolumeScalar.ToString().ToLower();
            }

        }
    }
}