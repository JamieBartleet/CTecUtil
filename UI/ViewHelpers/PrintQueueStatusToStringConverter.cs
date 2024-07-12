using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CTecUtil.UI.ViewHelpers
{
    [ValueConversion(typeof(string), typeof(PrintQueueStatus))]
    public sealed class PrintQueueStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is PrintQueueStatus s ? s switch 
                                             {
                                                 //PrintQueueStatus.Busy             => Cultures.Resources.PrintQueueStatus_Busy,
                                                 PrintQueueStatus.DoorOpen         => Cultures.Resources.PrintQueueStatus_DoorOpen,
                                                 PrintQueueStatus.Error            => Cultures.Resources.PrintQueueStatus_Error,
                                                 PrintQueueStatus.Initializing     => Cultures.Resources.PrintQueueStatus_Initialising,
                                                 //PrintQueueStatus.IOActive         => Cultures.Resources.PrintQueueStatus_IOActive,
                                                 PrintQueueStatus.ManualFeed       => Cultures.Resources.PrintQueueStatus_ManualFeed,
                                                 //PrintQueueStatus.None             => Cultures.Resources.PrintQueueStatus_None,
                                                 PrintQueueStatus.NotAvailable     => Cultures.Resources.PrintQueueStatus_NotAvailable,
                                                 PrintQueueStatus.NoToner          => Cultures.Resources.PrintQueueStatus_NoToner,
                                                 PrintQueueStatus.Offline          => Cultures.Resources.PrintQueueStatus_Offline,
                                                 PrintQueueStatus.OutOfMemory      => Cultures.Resources.PrintQueueStatus_OutOfMemory,
                                                 PrintQueueStatus.OutputBinFull    => Cultures.Resources.PrintQueueStatus_OutputBinFull,
                                                 PrintQueueStatus.PagePunt         => Cultures.Resources.PrintQueueStatus_PagePunt,
                                                 PrintQueueStatus.PaperJam         => Cultures.Resources.PrintQueueStatus_PaperJam,
                                                 PrintQueueStatus.PaperOut         => Cultures.Resources.PrintQueueStatus_PaperOut,
                                                 PrintQueueStatus.PaperProblem     => Cultures.Resources.PrintQueueStatus_PaperProblem,
                                                 PrintQueueStatus.Paused           => Cultures.Resources.PrintQueueStatus_Paused,
                                                 //PrintQueueStatus.PendingDeletion  => Cultures.Resources.PrintQueueStatus_PendingDeletion,
                                                 PrintQueueStatus.PowerSave        => Cultures.Resources.PrintQueueStatus_PowerSave,
                                                 //PrintQueueStatus.Printing         => Cultures.Resources.PrintQueueStatus_Printing,
                                                 //PrintQueueStatus.Processing       => Cultures.Resources.PrintQueueStatus_Processing,
                                                 PrintQueueStatus.ServerUnknown    => Cultures.Resources.PrintQueueStatus_ServerUnknown,
                                                 PrintQueueStatus.TonerLow         => Cultures.Resources.PrintQueueStatus_TonerLow,
                                                 PrintQueueStatus.UserIntervention => Cultures.Resources.PrintQueueStatus_UserIntervention,
                                                 //PrintQueueStatus.Waiting          => Cultures.Resources.PrintQueueStatus_Waiting,
                                                 PrintQueueStatus.WarmingUp => Cultures.Resources.PrintQueueStatus_WarmingUp,
                                                 _                                 => Cultures.Resources.PrintQueueStatus_Ready
                                                 } : Cultures.Resources.PrintQueueStatus_NotAvailable;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}