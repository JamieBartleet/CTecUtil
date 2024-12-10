using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTecUtil.Config;

namespace CTecUtil.Printing
{
    public class PrintUtil
    {
        private static readonly string _tempPrintFolder = Path.Combine("Temp", "Print");
        private const string _tempPrintFileSuffix = ".xps";


        public static string GetTempPrintFileName(string prefix)
        {
            var tempFolder = Path.Combine(ApplicationConfigBase.AppDataFolder, _tempPrintFolder);
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "document";                
            var tempFile   = prefix /*+ "_" + DateTime.Now.ToUniversalTime().ToString("u")*/ + _tempPrintFileSuffix;
            Directory.CreateDirectory(tempFolder);
            return Path.Combine(tempFolder, tempFile);
        }
    }
}
