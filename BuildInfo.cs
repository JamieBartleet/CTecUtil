using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil
{
    public static class BuildInfo
    {
        private static readonly VersionInfo _versionInfo = ParseProductVersionString(Assembly.GetExecutingAssembly());

        public static string   BuildVersion { get { return _versionInfo.Version;         } }
        public static DateTime BuildDate    { get { return _versionInfo.BuildDate.Value; } }
        public static string   BuildYear    { get { return _versionInfo.BuildYear;       } }


        public class VersionInfo
        {
            public string    Version   { get; set; }
            public DateTime? BuildDate { get; set; }
            public string    BuildYear { get; set; }
        }


        public static VersionInfo ParseProductVersionString(Assembly assembly)
        {
            VersionInfo result = new();

            var ver = assembly.GetName().Version;
            result.Version = ver.Major + "." + ver.Minor + "." + ver.Build;

            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            //NB: ProductVersion string is in format v.v.v+BuildDate:yyyymmddhhmmsszz
            var version_build = versionInfo.ProductVersion.Split("+");

            //if (version_build.Length > 0)
            //    result.Version = version_build[0] + "." + versionInfo.ProductBuildPart;

            if (version_build.Length > 1)
            {
                var buildDateStr = version_build[1].Split("=");

                if (buildDateStr.Length > 1)
                {
                    var dateStr = buildDateStr[1];
                    result.BuildDate = DateTime.Parse(dateStr);
                    result.BuildYear = buildDateStr[1].Substring(0, 4);
                }
            }

            return result;
        }
    }
}
