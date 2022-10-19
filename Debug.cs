using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTecUtil
{
    /// <summary>
    /// Replicates WriteLine() and Write() functions from System.Diagnostics.Debug, prefixing the output with a timestamp.
    /// </summary>
    public class Debug
    {
        private static string debugPrefix { get => DateTime.Now + " " + String.Format("{0,-3}", Thread.CurrentThread.ManagedThreadId) + " - "; }


        /// <summary>
        /// Writes a category name and message to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        /// <param name="isNewLine"></param>
        public static void Write(string message, string category, bool isNewLine = false)
        {
            if (isNewLine) System.Diagnostics.Debug.WriteLine(debugPrefix);
            System.Diagnostics.Debug.Write(message, category);
        }
        
        
        /// <summary>
        /// Writes a category name and the value of the object's System.Object.ToString method to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="value">An object whose name is sent to the System.Diagnostics.Debug.Listeners.</param>
        /// <param name="category">A category name used to organize the output.</param>
        /// <param name="isNewLine"></param>
        public static void Write(object value, string category, bool isNewLine = false)
        {        
            if (isNewLine) System.Diagnostics.Debug.WriteLine(debugPrefix);
            System.Diagnostics.Debug.Write(value, category);
        }


        /// Writes the value of the object's System.Object.ToString method to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="value">An object whose name is sent to the System.Diagnostics.Debug.Listeners.</param>
        /// <param name="isNewLine"></param>
        public static void Write(object value, bool isNewLine = false)
        {
            if (isNewLine) System.Diagnostics.Debug.WriteLine(debugPrefix);
            System.Diagnostics.Debug.Write(value);
        }
        

        /// <summary>
        /// Writes a message to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="isNewLine">If <see langword="true"/>, the message is prefixed with a timestamp (default False).</param>
        public static void Write(string message, bool isNewLine = false)
        {
            if (isNewLine) System.Diagnostics.Debug.WriteLine(debugPrefix);
            System.Diagnostics.Debug.Write(message);
        }


        /// <summary>
        /// Writes the value of the object's System.Object.ToString method to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="value">An object whose name is sent to the System.Diagnostics.Debug.Listeners.</param>
        public static void WriteLine(object value) => System.Diagnostics.Debug.WriteLine(debugPrefix + value.ToString());


        /// <summary>
        /// Writes a new line to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        public static void WriteLine() => System.Diagnostics.Debug.WriteLine("");


        /// <summary>
        /// Writes a category name and the value of the object's System.Object.ToString method to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="value">An object whose name is sent to the System.Diagnostics.Debug.Listeners.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public static void WriteLine(object value, string category) => System.Diagnostics.Debug.WriteLine(debugPrefix + value.ToString(), category);

        
        /// <summary>
        /// Writes a message followed by a line terminator to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public static void WriteLine(string message) => System.Diagnostics.Debug.WriteLine(debugPrefix + message);


        /// <summary>
        /// Writes a formatted message followed by a line terminator to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="format">A composite format string that contains text intermixed with zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void WriteLine(string format, params object[] args) => System.Diagnostics.Debug.WriteLine(debugPrefix + string.Format(format, args));


        /// <summary>
        /// Writes a category name, message and exception details to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="ex">An Exception that was thrown.</param>
        public static void WriteLine(string message, Exception ex) => System.Diagnostics.Debug.WriteLine(debugPrefix + message + "\n" + ex.ToString());


        /// <summary>
        /// Writes a category name, message and exception details to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="ex">An Exception that was thrown.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public static void WriteLine(string message, Exception ex, string category) => System.Diagnostics.Debug.WriteLine(debugPrefix + message + "\n" + ex.ToString(), category);


        /// <summary>
        /// Writes a category name and message to the trace listeners in the System.Diagnostics.Debug.Listeners collection.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="category">A category name used to organize the output.</param>
        public static void WriteLine(string message, string category) => System.Diagnostics.Debug.WriteLine(debugPrefix + message, category);
    }
}
