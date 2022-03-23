//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FTD2XX_NET;

//namespace CTecUtil.Ftdi
//{
//    public class FtdiComms
//    {
//        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
//        FTDI Ftdi1 = new FTDI();
//        string comstr;
//        UInt32 ftdiDeviceCount;

//        public void GetFtdiPort()
//        {
//            List<string> info = new();

//            ftdiDeviceCount = 0;

//            Ftdi1.ResetPort();

//            // Determine the number of FTDI devices connected to the machine
//            ftStatus = Ftdi1.GetNumberOfDevices(ref ftdiDeviceCount);

//            // Check status
//            if (ftStatus == FTDI.FT_STATUS.FT_OK)
//            {
//                info.Add("Number of FTDI devices: " + ftdiDeviceCount.ToString());
//            }
//            else
//            {
//                return;
//            }

//            if (ftdiDeviceCount == 0)
//            {
//                //alt_open_btn.Text = "Search for FTDI";
//                info.Add("No FTDI device found!");
//                return;
//            }

//            // Allocate storage for device info list
//            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

//            // Populate our device list
//            ftStatus = Ftdi1.GetDeviceList(ftdiDeviceList);

//            if (ftStatus == FTDI.FT_STATUS.FT_OK)
//            {
//                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
//                {
//                    info.Add("Device Index: " + i.ToString());
//                    info.Add("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
//                    info.Add("Type: " + ftdiDeviceList[i].Type.ToString());
//                    info.Add("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
//                    info.Add("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
//                    info.Add("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
//                    info.Add("Description: " + ftdiDeviceList[i].Description.ToString());
//                    info.Add("");
//                }
//            }
//            // Open first device in our list by serial number
//            ftStatus = Ftdi1.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
//            if (ftStatus != FTDI.FT_STATUS.FT_OK)
//            {
//                info.Add("Failed to open device (error " + ftStatus.ToString() + ")");
//                return;
//            }
//            Ftdi1.GetCOMPort(out comstr);
//            info.Add("Com Port: " + comstr);
//            ftStatus = Ftdi1.Close();
//            info.Add("Open VIRTU Found On " + comstr);
//            if (ftStatus != FTDI.FT_STATUS.FT_OK)
//            {
//                info.Add("Failed to open device (error " + ftStatus.ToString() + ")");
//                return;
//            }

//        }


//    }
//}
