using Newtonsoft.Json.Linq;
using System;

namespace CTecUtil.StandardPanelDataTypes
{
    /// <summary>
    /// Defines a time consisting of two numbers.  These represent either hours and minutes or minutes and seconds.
    /// </summary>
    public class TimeOfDay
    {
        public TimeOfDay(TimeSpan value) => Value = value;


        public TimeSpan Value { get; set; }


        /// <summary>
        /// Convert a TimeOfDay to a two-byte array
        /// </summary>
        /// <param name="hoursAndMinutes">If true (default) the result represents the hours and minutes, else it is the minutes and seconds</param>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        public byte[] ToByteArray(bool hoursAndMinutes = true) => hoursAndMinutes ? [ (byte)Value.Hours, (byte)Value.Minutes ] : [ (byte)Value.Minutes, (byte)Value.Seconds ];
#else
        public byte[] ToByteArray(bool hoursAndMinutes = true) => hoursAndMinutes ? new[] { (byte)Value.Hours, (byte)Value.Minutes } : new[] { (byte)Value.Minutes, (byte)Value.Seconds };
#endif

        /// <summary>
        /// Parse a TimeOfDay from the byte data
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static TimeOfDay Parse(byte[] data, Func<byte[], bool> typeCheck, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data)??false)
                return null;

            try
            {
                if (data.Length > startOffset + 1)
                {
                    var result = new TimeSpan(data[startOffset + 1], data[startOffset], 0);

                    //return zero if greater than 1 day
                    if (result.Subtract(new(1, 0, 0, 0)) > TimeSpan.Zero)
                        return new(TimeSpan.Zero);
                    return new(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(Parse) + " failed: " + ex.ToString());
            }

            return new TimeOfDay(new TimeSpan(0,0,0));
        }
    }
}
