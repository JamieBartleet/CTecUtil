using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Data;
using System.Reflection;

namespace CTecUtil.Cultures
{
    /// <summary>
    /// Wraps up XAML access to instance of WPFLocalize.Properties.Resources, list of available cultures, and method to change culture
    /// </summary>
    public class CultureResources
    {
        /// <summary>
        /// The Resources ObjectDataProvider uses this method to get an instance of the WPFLocalize.Properties.Resources class
        /// </summary>
        /// <returns></returns>
        public Cultures.Resources GetResourceInstance()
        {
            return new Cultures.Resources();
        }

        //private static ObjectDataProvider _provider;
        //public static ObjectDataProvider ResourceProvider
        //{
        //    get
        //    {
        //        if (_provider is null)
        //            _provider = (ObjectDataProvider)System.Windows.Application.Current.FindResource(nameof(CTecUtil));
        //        return _provider;
        //    }
        //}

        public static void ChangeCulture(CultureInfo culture)
        {
            if (culture == null)
                return;

            Cultures.Resources.Culture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            //ResourceProvider.Refresh();
        }

        
        private static List<CultureInfo> _supportedCultures = null;

        /// <summary>
        /// List of available cultures, enumerated at startup
        /// </summary>
        public static List<CultureInfo> SupportedCultures
        {
            get
            {
                if (_supportedCultures is null)
                    throw new CultureNotFoundException("SupportedCultures has not been initialised - call 'CTecUtil.Cultures.CultureResources.InitSupportedCultures(mainAssemblyName)'.");
                return _supportedCultures;
            }
        }


        /// <summary>
        /// Initialise the supported cultures list
        /// </summary>
        /// <param name="mainAssemblyName"></param>
        public static void InitSupportedCultures(string mainAssemblyName)
        {
            if (_supportedCultures is null)
            {
                _supportedCultures = new List<CultureInfo>();

                //determine which cultures are available to this application
                foreach (string dir in Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory))
                {
                    try
                    {
                        DirectoryInfo dirinfo = new DirectoryInfo(dir);

                        //is there a resources dll that originated from our assembly (i.e. not from the installer package) in this directory?
                        if (dirinfo.GetFiles(mainAssemblyName + ".resources.dll").Length > 0)
                            _supportedCultures.Add(CultureInfo.GetCultureInfo(dirinfo.Name));
                    }
                    catch (ArgumentException) //ignore exceptions generated for any unrelated directories in the bin folder
                    {
                    }
                }
            }
        }
    }
}
