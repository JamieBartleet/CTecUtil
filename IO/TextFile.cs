using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using CTecUtil.UI;

namespace CTecUtil.IO
{
    public class TextFile
    {
        public static string FileFolder { get; set; }
        public static string FilePath { get; set; }
        public static string FileName { get => Path.GetFileName(FilePath); }
        public static string Filter { get; set; }

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


        public static void SaveFile(string data)
        {
            if (data is null)
                return;

            if (string.IsNullOrEmpty(FilePath))
                SaveFileAs(data);
            else
            {
                UIState.SetBusyState();
                CTecUtil.Debug.WriteLine("Writing file: " + FilePath);
                using var Writer = new StreamWriter(FilePath);
                Writer.Write(data);
            }
        }


        public static void SaveFileAs(string data)
        {
            if (data is null)
                return;

            SaveFileDialog dlgSaveFile = new SaveFileDialog()
            {
                Filter           = Filter,
                InitialDirectory = FileFolder,
                FileName         = FileName
            };

            if (dlgSaveFile.ShowDialog() == true)
            {
                FilePath = dlgSaveFile.FileName;
                FileFolder = dataFileDirectory();
                SaveFile(data);
            }
        }

    }
}
