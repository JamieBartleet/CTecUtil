using System;

namespace CTecUtil.StandardPanelDataTypes
{
    /// <summary>
    /// Defines an element of a DateTime
    /// </summary>
    public class DateElement : IComparable
    {
        public string Part { get; set; }
        public int    Value { get; set; }
        public int    Index { get; set; }
        public int    Length { get; set; }

        public byte ToByte(int datePart) => (byte)(datePart % Math.Pow(10, Length));

        int IComparable.CompareTo(object obj)
        {
            if (obj is not DateElement otherData)
                return 0;
            return otherData.Index.CompareTo(Index);
        }
    }
}
