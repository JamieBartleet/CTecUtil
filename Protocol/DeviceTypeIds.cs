using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.Protocol
{
    public enum ApolloDeviceTypeIds
    {
        NetworkController = 0xff,

        S90_XP95_SounderController = 0x20,
        XP95A_SounderControlModule = 0x28,
        DiscoverySounder = 0x30,
        DiscoveryVoiceSounder = 0x34,
        IOUnit = 0x40,
        XP95A_MiniSwitchMonitor = 0x48,
        XP95A_SwitchMonitor = 0x50,
        XP95A_SwitchMonitorIO = 0x58,
        IonisationDetector = 0x60,
        Discovery_MOST = 0x68,
        Discovery_CODetector = 0x69,
        Discovery_COHeatDetector = 0x70,
        Discovery_COPersonalProtectionMonitor = 0x71,
        Discovery_COEnvironmentalGasMonitor = 0x79,
        S90_XP95_ZoneMonitorUnit = 0x80,
        XP95_MiniSwitchMonitor = 0x88,
        Discovery_AspiratingDetector = 0x98,
        OpticalDetectorEUR = 0xa0,
        XP95_BeamDetectorUSA = 0xa8,
        XP95_FlameDetector = 0xb0,
        Discovery_MultiSensor = 0xb8,
        XP95_MultiSensor = 0xb9,
        HeatDetector = 0xc0,
        XP95_HighTempHeatDetector = 0xc8,
        S90_ManualCallpoint = 0xe0,
        XP95A_MiniPrioritySwitchMonitor = 0xe8,
        XP95A_PrioritySwitchMonitor = 0xf0,
        ManualCallpointWithInterrupt = 0xf8,

        Delete = -1,
        Unknown = -2
    }


    public enum CastDeviceTypeIds
    {
        NetworkController = 0xff,

        OpticalDetector = 0x00,
        HeatDetector = 0x01,
        CODetector = 0x02,
        OpticalHeatDetector = 0x03,
        OpticalCODetector = 0x04,
        HeatCODetector = 0x05,
        OpticalHeatCODetector = 0x06,
        BeamDetector = 0x07,
        FlameDetector = 0x08,

        MCP = 0x20,

        MiniIO = 0x40,
        DoubleGangIOSingleChannelSounderController = 0x41,
        DoubleGangMainsIO = 0x42,
        HushButton = 0x43,
        FourChannelSounderController = 0x44,
        MultiChannelZoneMonitor = 0x45,
        DoubleGangZoneMonitor = 0x46,
        MiniIOFire2HMO = 0x47,
        DoubleGangMainsIOFire2HMO = 0x48,

        Sounder = 0x60,
        VAD = 0x61,
        SounderVAD = 0x62,
        VoiceSounder = 0x63,
        VoiceSounderVAD = 0x64,

        Delete = -1,
        Unknown = -2
    }


    public enum CastProDeviceTypeIds
    {
        NetworkController = 0xff,

        OpticalDetector = 0x00,
        HeatDetector = 0x01,
        CODetector = 0x02,
        OpticalHeatDetectorWithToneSounder = 0x03,
        OpticalHeatDetectorWithVoiceSounder = 0x04,
        OpticalHeatDetectorWithToneSounderVAD = 0x05,
        OpticalHeatDetectorWithVoiceSounderVAD = 0x06,
        OpticalHeatCODetector = 0x07,
        OpticalHeatCODetectorWithToneSounder = 0x08,
        OpticalHeatCODetectorWithVoiceSounder = 0x09,
        OpticalHeatCODetectorWithToneSounderVAD = 0x0a,
        OpticalHeatCODetectorWithVoiceSounderVAD = 0x0b,

        Delete = -1,
        Unknown = -2
    }


    public enum HochikiDeviceTypeIds
    {
        NetworkController = 0xff,

        OpticalDetector = 0xA0,
        IonisationDetector = 0x01,
        HeatDetector = 0x02,
        MultiDetector = 0x03,

        MCP = 0x04,

        AddressableBeacon = 0x05,
        MiniZoneMonitor = 0x06,
        RelayController = 0x07,
        DualSwitchController = 0x08,
        SingleIOModule = 0x09,
        DualZoneMonitor = 0x0a,
        NonLatchingIOUnit = 0x0b,
        SounderController_Sets = 0x0c,
        SounderController_Groups = 0x0d,
        RemoteIndicator = 0x0e,

        BaseWallSounder = 0x0f,
        AddressableBase = 0x10,
        MasterAddressableBase = 0x12,

        Delete = -1,
        Unknown = -2
    }


    public enum QuantecDeviceTypeIds
    {
        Callpoint = 0x00,
        CorridorDisplay = 0x03,
        MonitoringPoint = 0x05,
        OverDoorLight = 0x06,
        NetworkController = 0x07,
        RadioReceiver = 0x08,
        NotFitted = 0xfe,

        Qt412_StaffTransmitter = 0x0100,
        Qt432_PendantTransmitter = 0x0200,
        Qt6029_Qt302rx_CallPoint = 0x0400,
        Qt611_CallPoint = 0x0800,
        Qt611_DayNightSwitch = 0x1000,
        Qt611_MonitorPoint = 0x1100,
        Qt612_RelayOutput = 0x1200,
        Qt613zi_qt6882_Sounder = 0x1400,
        Qt616_FallSensor = 0x1800,

        Qt611_MultipurposeDevice = Qt611_CallPoint | Qt611_DayNightSwitch | Qt611_MonitorPoint,

        Delete = -1,
        Unknown = -2
    }
}
