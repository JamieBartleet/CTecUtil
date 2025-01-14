namespace CTecUtil
{
    public class Enums
    {
        public static string SupportedAppsToString(SupportedApps app)
            => app switch
            {
                SupportedApps.XFP     => "XfpTools",
                SupportedApps.ZFP     => "ZfpTools",
                SupportedApps.Quantec => "Quantec",
                _                     => "Unknown C-Tec app",
            };


        public static string CommsDirectionToString(CommsDirection direction)
            => direction switch
            {
                CommsDirection.Idle     => Cultures.Resources.Comms_Direction_Idle,
                CommsDirection.Upload   => Cultures.Resources.Comms_Direction_Upload,
                CommsDirection.Download => Cultures.Resources.Comms_Direction_Download,
            };
    }
}
