using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Data;

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

        private static ObjectDataProvider _provider;
        public static ObjectDataProvider ResourceProvider
        {
            get
            {
                if (_provider == null)
                    _provider = (ObjectDataProvider)System.Windows.Application.Current.FindResource("CTecControls");
                return _provider;
            }
        }

        public static void ChangeCulture(CultureInfo culture)
        {
            if (culture == null)
                return;

            Cultures.Resources.Culture = culture;
            ResourceProvider.Refresh();
            Registry.WriteSubKey(Registry.Keys.CultureKey, culture.Name);
        }
    }
}
