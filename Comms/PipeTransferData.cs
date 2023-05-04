using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil.Comms
{
    public class PipeTransferData
    {
        public enum DataTypes
        {
            Path
        };


        public PipeTransferData(DataTypes type, string data) { DataType = type; Data = data; }

        public DataTypes DataType { get; set; }
        public string    Data { get; set; }
    }
}
