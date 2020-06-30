using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CEC.Blazor.Components;

namespace CEC.Blazor.Utilities
{
/// <summary>
/// Static class to output debug information
/// Controlled by the Debug Property
/// Use a directive wrapper if you want to tuern it on or off with DEBUG directive
/// </summary>
    public class DebugUtilities
    {
        /// <summary>
        /// Static that controls if the utilities output any debug information
        /// </summary>
        public static bool Debug { get; set; } = false;


        public static void DebugOutput(BaseEventArgs e)
        {
            if (Debug)
            {
                System.Diagnostics.Debug.WriteLine(GetOutput(e));
                var spacer = string.Empty;
                var counter = 1;
                foreach (var ev in e.Events)
                {
                    spacer += "  ";
                    System.Diagnostics.Debug.WriteLine(string.Concat(spacer, counter, " - ", GetOutput(ev)));
                    counter++;
                }
                //Console.WriteLine("## StackTrace: '{0}'", Environment.StackTrace);
            }
        }

        public static void DebugOutput(string message)
        {
            if (Debug)
            {
                var output = new StringBuilder(string.Concat(DateTime.Now.ToLongTimeString(), " - ", message, " =>"));
                System.Diagnostics.Debug.WriteLine(output);
            }
        }
        
        private static string GetOutput(BaseEventArgs e)
        {
            var classname = e.This.GetType().Name;
            var functionname = string.IsNullOrEmpty(e.FunctionName) ? "No Function Defined" : e.FunctionName;
            var GUID = e.This is IGuidComponent ? ((IGuidComponent)e.This).GUID.ToString() : string.Empty;
            var output = new StringBuilder(string.Concat(DateTime.Now.ToLongTimeString(), " - ", classname, "=>", functionname));
            if (!string.IsNullOrEmpty(e.Message)) output.Append(string.Concat(" - ", e.Message));
            if (e.Caller != null) output.Append(string.Concat(" ##### Writer Class: ", e.Caller.GetType().ToString()));
            if (!string.IsNullOrEmpty(GUID)) output.Append(string.Concat(" Component GUID: ", GUID));
            return output.ToString().Trim();
        }

        public static List<string> GetClasses(string nameSpace)
        {
            var x = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => t.IsClass && t.Namespace == nameSpace).ToList();

            if (x != null) return x.Where(t => !t.Name.Contains("<")).Select(t => t.Name).ToList();
            else return new List<string>();
        }

    }
}
