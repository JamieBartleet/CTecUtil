using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using CTecUtil.UI;

namespace CTecUtil.IO
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TextFile
    {
        public enum FileReadResult
        {
            Ok,
            CouldNotParseFile,
            DataIsWrongType,
            ErrorReadingFile,
        }


        public static string JsonFileExt = ".json";


        public static string FilePath { get; set; }
        public static string Filter { get; set; }

        internal static string FileFolder { get; set; }
        internal static string FileName { get => Path.GetFileName(FilePath); }

        protected static string dataFileDirectory() => string.IsNullOrEmpty(FilePath) ? Environment.CurrentDirectory : Path.GetDirectoryName(FilePath);


        public static bool OpenFile()
        {
            var dlgOpenFile = new OpenFileDialog()
            {
                InitialDirectory = FileFolder,
                Filter = Filter
            };

            if (dlgOpenFile.ShowDialog() == true)
            {
                FilePath = dlgOpenFile.FileName;
                FileFolder = dataFileDirectory();
                CTecUtil.Debug.WriteLine("Opening file: " + FilePath);
                return true;
            }

            return false;
        }


        public static string SaveFile(string data)
        {
            if (data is null)
                return null;

            if (string.IsNullOrEmpty(FilePath))
                return SaveFileAs(data);
            else
            {
                CTecUtil.Debug.WriteLine("Writing file: " + FilePath);
                using var Writer = new StreamWriter(FilePath);
                Writer.Write(data);
                return FilePath;
            }
        }

        public static string SaveFile(string data, string filePath)
        {
            if (data is null || string.IsNullOrEmpty(filePath))
                return null;

            CTecUtil.Debug.WriteLine("Writing file: " + filePath);
            try
            {
                using var Writer = new StreamWriter(filePath);
                Writer.Write(data);
                return FilePath;
            }
            catch (Exception ex)
            {
                CTecUtil.Debug.WriteLine(ex.Message);
            }

            return null;
        }


        public static string SaveFileAs(string data)
        {
            if (data is null)
                return null;

            SaveFileDialog dlgSaveFile = new SaveFileDialog()
            {
                Filter           = Filter,
                InitialDirectory = FileFolder,
                FileName         = FileName,
                Title            = Cultures.Resources.File_Save_As
            };

            if (dlgSaveFile.ShowDialog() == true)
            {
                FilePath = dlgSaveFile.FileName;
                FileFolder = dataFileDirectory();
                return SaveFile(data);
            }
            return null;
        }


        /// <summary>
        /// Ensure FilePath's extension is .json
        /// </summary>
        protected static string JsonFileName(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            var idx = FilePath.LastIndexOf(ext);
            return FilePath.Substring(0, idx) + JsonFileExt;
        }

    }
}
