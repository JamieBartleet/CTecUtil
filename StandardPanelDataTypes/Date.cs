using System;
using System.Collections.Generic;

namespace CTecUtil.StandardPanelDataTypes
{
    public class Date
    {
        public Date(DateTime value) => Value = value;


        public DateTime Value { get; set; }


        /// <summary>
        /// Convert Date to a three-byte array consisting of year (2 digits), month and day.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() => ToByteArray("yyMMdd");

        /// <summary>
        /// Convert Date to a byte array according to the format specified.
        /// </summary>
        /// <param name="format">String specifying the format and order of the resultant byte array,<br/>
        /// e.g. for the Date 7/8/2024 13:47, "yyMMddhhmmss" would yield [24,8,7,1,47,0]; "yyyyMMddHHmmss" would yield [232,7,8,7,13,47,0].<br/><br/>
        /// Valid elements are:<br/>
        ///   yyyy : 2 bytes for 4-digit year<br/>
        ///   yy   : 1 byte for 2-digit year<br/>
        ///   MM   : month<br/>
        ///   dd   : day<br/>
        ///   HH   : hour (24h)<br/>
        ///   hh   : hour (12h)<br/>
        ///   mm   : minute<br/>
        ///   ss   : seconds<br/>
        /// </param>
        /// <returns></returns>
        public byte[] ToByteArray(string format)
        {
            var dateParts = new List<DateElement>()
            {
                new() { Part = "yr4", Value = Value.Year,       Index = format.IndexOf("yyyy"), Length = 2 },
                new() { Part = "yr2", Value = Value.Year % 100, Index = format.IndexOf("yy"),   Length = 1 },
                new() { Part = "mon", Value = Value.Month,      Index = format.IndexOf("MM"),   Length = 1 },
                new() { Part = "day", Value = Value.Day,        Index = format.IndexOf("dd"),   Length = 1 },
                new() { Part = "h24", Value = Value.Hour,       Index = format.IndexOf("HH"),   Length = 1 },
                new() { Part = "h12", Value = Value.Hour % 12,  Index = format.IndexOf("hh"),   Length = 1 },
                new() { Part = "min", Value = Value.Minute,     Index = format.IndexOf("mm"),   Length = 1 },
                new() { Part = "sec", Value = Value.Second,     Index = format.IndexOf("ss"),   Length = 1 },
            };

            for (int i = dateParts.Count - 1; i >= 0; i--)
                if (dateParts[i].Index < 0)
                    dateParts.RemoveAt(i);

            byte[] result = null;
            for (int i = 0; i < dateParts.Count; i++)
            {
                var bytes = ByteArrayProcessing.IntToByteArray(dateParts[i].Value, dateParts[i].Length);
                if (result is null)
                    result = bytes;
                else 
                    result = ByteArrayProcessing.CombineByteArrays(result, bytes);
            }
            return result;
        }


        /// <summary>
        /// Parse a Date from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static Date Parse(byte[] data, Func<byte[], bool> typeCheck, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data)??false)
                return null;

            try
            {
                if (data.Length > startOffset + 2)
                    return new(new(data[startOffset] + DateTime.Now.Year / 100 * 100, data[startOffset + 1], data[startOffset + 2]));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(Parse) + " failed: " + ex.ToString());
            }

            return new Date(new DateTime(0,0,0,0,0,0));
        }
    }
}
