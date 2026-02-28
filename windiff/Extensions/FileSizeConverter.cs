using System.Globalization;

namespace windiff.Extensions
{
    public class FileSizeConverter
    {
        public int Precision { get; set; } = 2;

        //将字节数转换成人类可读的大小字符串（支持 IEC 1024 与 SI 1000）。
        string[] IECUnits = new[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
        string[] SIUnits = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public string? ConvertOrDefault(long? value, bool isIECUnit = true)
        {
            try
            {
                return Convert(value, isIECUnit);
            }
            catch
            {
                return null;
            }
        }
        public string? Convert(long? value, bool isIECUnit = true)
        {
            if (value == null) return null;

            if (isIECUnit)
            {
                var units = IECUnits;
                var unitBase = 1024;

                return Format(value.Value, units, unitBase);
            }
            else
            {
                var units = SIUnits;
                var unitBase = 1000;

                return Format(value.Value, units, unitBase);
            }
        }
        string Format(long value, string[] units, int unitBase)
        {
            var size = System.Convert.ToDouble(value);
            var unitIndex = 0;

            while (size >= unitBase && unitIndex < units.Length - 1)
            {
                size /= unitBase;
                unitIndex++;
            }

            var unit = units[unitIndex];
            var number = size.ToString($"N{Precision}", CultureInfo.CurrentCulture);

            return $"{number} {unit}";
        }
    }
}
