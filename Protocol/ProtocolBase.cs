using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CTecUtil.Protocol
{
    public class ProtocolBase
    {
        //public static string DeviceCategory(int id)          => throw new NotImplementedException("ProtocolBase.DeviceCategory");
        public static string DeviceFamily(int id)            => throw new NotImplementedException("ProtocolBase.DeviceFamily");
        public static string DeviceName(int deviceType)      => throw new NotImplementedException("ProtocolBase.DeviceName");
        public static BitmapImage DeviceIcon(int deviceType) => throw new NotImplementedException("ProtocolBase.DeviceIcon");
        public static bool IsValidDeviceType(int id)         => throw new NotImplementedException("ProtocolBase.IsValidDeviceType");
        public static bool IsInputDevice(int deviceType)     => throw new NotImplementedException("ProtocolBase.IsInputDevice");
        public static bool IsIODevice(int deviceType)        => throw new NotImplementedException("ProtocolBase.IsIODevice");
        public static bool IsZonalDevice(int deviceType)     => throw new NotImplementedException("ProtocolBase.IsZonedDevice");
        public static bool IsGroupedDevice(int deviceType)   => throw new NotImplementedException("ProtocolBase.IsGroupedDevice");
        public static bool IsAreaDevice(int deviceType)      => throw new NotImplementedException("ProtocolBase.IsAreaDevice");
    }
}
