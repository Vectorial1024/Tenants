namespace Tenants
{
    internal static class SettingsHelper
    {
        public static TenantsSettings LatestVersion;

        public static void Reset()
        {
            LatestVersion.Reset();
        }
    }
}