using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow{
    public class Program{
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        // ======================================================================================================
        // GLOBAL SYSTEM INFO
        public static int windows_mode = 0;
        public static string windows_disk = @"C:\";
        // ======================================================================================================
        // DEBUG MODES
        public static readonly bool ts_pre_debug_mode = false;
        public static readonly bool glow_console_debug_mode = false;
        // ======================================================================================================
        // TS UPDATER TEXT
        public static readonly string updater_exe_name = "TSUpdater.exe";
        public static readonly string updater_old_exe_name = "TSUpdater.exe.old";
        // ======================================================================================================
        [STAThread]
        static void Main(){
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            // ------------------------------------------------------------------
            // CHECK WINDOWS VERSION & OS DISK
            try{
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2","SELECT Caption FROM Win32_OperatingSystem"))
                using (var results = searcher.Get()){
                    string caption = results.Cast<ManagementObject>().Select(mo => mo["Caption"]?.ToString()).FirstOrDefault();
                    windows_mode = (caption?.IndexOf("Windows 11", StringComparison.OrdinalIgnoreCase) >= 0) ? 1 : 0;
                }
                windows_disk = Path.GetPathRoot(Environment.ExpandEnvironmentVariables("%SystemRoot%"))?.Trim();
            }catch (Exception){ }
            // ------------------------------------------------------------------
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TSPreloader());
        }
    }
}