using System.Xml.Serialization;

namespace AwwScrap.UserConfig.Settings
{
    [XmlRoot(nameof(UserSettings), IsNullable = true)]
    public class UserSettings
    {
        [XmlElement(nameof(SettingsDescription))]
        public string SettingsDescription;

        [XmlElement(nameof(BaseAwwScrapScalar))]
        public string BaseAwwScrapScalar;

        [XmlElement(nameof(ScrapMassScalar))]
        public string ScrapMassScalar;

        [XmlElement(nameof(ScrapProductionTimeScalar))]
        public string ScrapProductionTimeScalar;

        [XmlElement(nameof(ScrapVolumeScalar))]
        public string ScrapVolumeScalar;

        [XmlElement(nameof(ScrapUnknownItems))]
        public bool ScrapUnknownItems;
    }
}