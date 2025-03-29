using System.Text;
using AwwScrap.Common.BaseClasses;
using AwwScrap.Common.Extensions;
using AwwScrap.UserConfig.Settings;
using Sandbox.ModAPI;
using VRage.Scripting;
using VRageRender;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace AwwScrap.UserConfig.Controller
{
    public class SettingsController : BaseXmlUserSettings
    {
        private UserSettings _userSettings;
        private readonly StringBuilder _sb = new StringBuilder();

        public bool ScrapUnknownItems => _userSettings.ScrapUnknownItems;

        public SettingsController(string modName) : base(modName) { }

        public void InitializeServer()
        {
            _userSettings = Get<UserSettings>();
            SettingsMapper();
            CleanUserSettings();
            Set(_userSettings);
            WriteToSandbox();
        }

        public void InitializeClient()
        {
            ReadFromSandbox();
        }

        private void WriteToSandbox()
        {
            // Server only --

            MyAPIGateway.Utilities.SetVariable<float>("AwwScrap_BaseAwwScrapScalar", DefaultSettings.BaseAwwScrapScalar);
            MyAPIGateway.Utilities.SetVariable<float>("AwwScrap_ScrapMassScalar", DefaultSettings.ScrapMassScalar);
            MyAPIGateway.Utilities.SetVariable<float>("AwwScrap_ScrapProductionTimeScalar", DefaultSettings.ScrapProductionTimeScalar);
            MyAPIGateway.Utilities.SetVariable<float>("AwwScrap_ScrapVolumeScalar", DefaultSettings.ScrapVolumeScalar);
            MyAPIGateway.Utilities.SetVariable<bool>("AwwScrap_ScrapUnknownItems", DefaultSettings.ScrapUnknownItems);
            MyAPIGateway.Utilities.SetVariable<bool>("AwwScrap_SurvivalKitRecycling", DefaultSettings.SurvivalKitRecycling);
        }

        private void ReadFromSandbox()
        {
            // Client only --

            float scalar;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_BaseAwwScrapScalar", out scalar))
            {
                DefaultSettings.BaseAwwScrapScalar = scalar;
            }

            scalar = 0;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_ScrapMassScalar", out scalar))
            {
                DefaultSettings.ScrapMassScalar = scalar;
            }

            scalar = 0;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_ScrapProductionTimeScalar", out scalar))
            {
                DefaultSettings.ScrapProductionTimeScalar = scalar;
            }

            scalar = 0;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_ScrapVolumeScalar", out scalar))
            {
                DefaultSettings.ScrapVolumeScalar = scalar;
            }

            bool bools;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_ScrapUnknownItems", out bools))
            {
                DefaultSettings.ScrapUnknownItems = bools;
            }

            bools = false;
            if (MyAPIGateway.Utilities.GetVariable("AwwScrap_SurvivalKitRecycling", out bools))
            {
                DefaultSettings.SurvivalKitRecycling = bools;
            }
        }

        private void CleanUserSettings()
        {
            // Nothing to do here, just leaving it in as a reminder.
        }

        public override string ToString()
        {
            _sb.AppendLine();
            _sb.AppendLine();
            return _sb.ToString();
        }

        private void AppendToLog(string str1, string str2, int messageNumber)
        {
            switch (messageNumber)
            {
                case 1:
                    _sb.AppendLine($"{str1} parsed! {str2}");
                    break;
                case 2:
                    _sb.AppendLine($"{str1} was within expected range! {str2}");
                    break;
                case 3:
                    _sb.AppendLine($"{str1} was not within expected range! {str2}");
                    break;
                case 4:
                    _sb.AppendLine($"{str1} failed to parse: {str2}");
                    break;
            }
        }

        protected sealed override void SettingsMapper()
        {
            _sb.AppendLine();
            _sb.AppendLine();

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
                AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), baseAwwScrapScalar.ToString(), 1);
                // must be between 0.01f and 0.9f
                if (baseAwwScrapScalar >= 0.01f && baseAwwScrapScalar <= 0.9f)
                {
                    DefaultSettings.BaseAwwScrapScalar = baseAwwScrapScalar;
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);

            }
            else
            {
                AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.BaseAwwScrapScalar = DefaultSettings.BaseAwwScrapScalar.ToString().ToLower();
            }

            float scrapMassScalar;
            if (float.TryParse(_userSettings.ScrapMassScalar, out scrapMassScalar))
            {
                AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), scrapMassScalar.ToString(), 1);
                // must be between 0.01f and 1.0f
                if (scrapMassScalar >= 0.01f && scrapMassScalar <= 1.0f)
                {
                    DefaultSettings.ScrapMassScalar = scrapMassScalar;
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(DefaultSettings.ScrapMassScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapMassScalar = DefaultSettings.ScrapMassScalar.ToString().ToLower();
            }

            float scrapProductionTimeScalar;
            if (float.TryParse(_userSettings.ScrapProductionTimeScalar, out scrapProductionTimeScalar))
            {
                AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), scrapProductionTimeScalar.ToString(), 1);
                // must be between 0.01f and 100.0f
                if (scrapProductionTimeScalar >= 0.01f && scrapProductionTimeScalar <= 100.0f)
                {
                    DefaultSettings.ScrapProductionTimeScalar = scrapProductionTimeScalar;
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                }
                else
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(baseAwwScrapScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapProductionTimeScalar = DefaultSettings.ScrapProductionTimeScalar.ToString().ToLower();
            }

            float scrapVolumeScalar;
            if (float.TryParse(_userSettings.ScrapVolumeScalar, out scrapVolumeScalar))
            {
                AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), scrapVolumeScalar.ToString(), 1);
                // must be between 0.01f and 1.0f
                if (scrapVolumeScalar >= 0.01f && scrapVolumeScalar <= 1.0f)
                {
                    DefaultSettings.ScrapVolumeScalar = scrapVolumeScalar;
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 2);
                } else
                    AppendToLog(nameof(DefaultSettings.BaseAwwScrapScalar), DefaultSettings.BaseAwwScrapScalar.ToString(), 3);
            }
            else
            {
                AppendToLog(nameof(DefaultSettings.ScrapVolumeScalar), _userSettings.BaseAwwScrapScalar, 4);
                _userSettings.ScrapVolumeScalar = DefaultSettings.ScrapVolumeScalar.ToString().ToLower();
            }

            try
            {
                DefaultSettings.ScrapUnknownItems = _userSettings.ScrapUnknownItems;
                AppendToLog(nameof(DefaultSettings.ScrapUnknownItems), DefaultSettings.ScrapUnknownItems.ToString(), 1);
                // must be true or false
                AppendToLog(nameof(DefaultSettings.ScrapUnknownItems), DefaultSettings.ScrapUnknownItems.ToString(), 3);
            }
            catch
            {
                AppendToLog(nameof(DefaultSettings.ScrapUnknownItems), _userSettings.ScrapUnknownItems.ToSingleChar(), 4);
                _userSettings.ScrapUnknownItems = DefaultSettings.ScrapUnknownItems;
            }

            try
            {
                DefaultSettings.SurvivalKitRecycling = _userSettings.SurvivalKitRecycling;
                AppendToLog(nameof(DefaultSettings.SurvivalKitRecycling), DefaultSettings.SurvivalKitRecycling.ToString(), 1);
                // must be true or false
                AppendToLog(nameof(DefaultSettings.SurvivalKitRecycling), DefaultSettings.SurvivalKitRecycling.ToString(), 3);
            }
            catch
            {
                AppendToLog(nameof(DefaultSettings.SurvivalKitRecycling), _userSettings.SurvivalKitRecycling.ToSingleChar(), 4);
                _userSettings.SurvivalKitRecycling = DefaultSettings.SurvivalKitRecycling;
            }
        }
    }
}