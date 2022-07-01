﻿using CTecUtil.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CTecUtil
{
    /// <summary>
    /// Legacy Registry class, included only for compatibility with pre-release versions
    /// </summary>
    public class Registry
    {
        /// <summary>
        /// Initialise the CTecUtil.Registry class.
        /// </summary>
        /// <param name="productName">The software's name ("QuantecTools", XFPTools", etc.)</param>
        public static void Initialise(string productName)
        {
            _productName = productName;
            _initialised = true;
        }


        private const string _portSetting = "port";


        public static SerialPortSettings ReadSerialPortSettings()
        {
            SerialPortSettings result = new();

            var keyData = (string)readSubKey(RegistryKeyNames.SerialPortKey);
            if (!string.IsNullOrEmpty(keyData))
            {
                var settings = keyData.Split(new char[] { ',' });

                foreach (var s in settings)
                {
                    var param = s.Split(new char[] { '=' });
                    if (param.Length > 1)
                    {
                        switch (param?[0].ToLower())
                        {
                            case _portSetting: result.PortName = param[1]; break;
                        }
                    }
                }
            }

            return result;
        }


        private static bool _initialised;
        private static string _productName;
        private const string _rootKey = @"SOFTWARE\CTec\";


        /// <summary>
        /// Read registry entry from HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="subkeyName"></param>
        private static object readSubKey(string subkeyName, object defaultValue = null)
        {
            if (!_initialised) throw new Exception("CTecUtil: the Registry class has not been initialised.");

            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_rootKey + _productName);
                var result = key.GetValue(subkeyName, defaultValue);
                key.Close();
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}