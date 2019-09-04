using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MyVet.Common.Helpers
{
    public static class Settings
    {
        private const string _pet = "Pet";
        private static readonly string _settingsDefault = string.Empty;

        private static ISettings AppSettings => CrossSettings.Current;

        public static string Pet
        {
            get => AppSettings.GetValueOrDefault(_pet, _settingsDefault);
            set => AppSettings.AddOrUpdateValue(_pet, value);
        }
    }
}
