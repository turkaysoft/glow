using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowBenchMemoryTool : Form{
        // VARIABLES
        // ======================================================================================================
        private long RAMBench_TargetMemoryUsage, RAMBench_totalAllocated = 0;
        private const long RAMBench_ReserveBytes = 1024L * 1024L * 1024L;
        private const int RAMBench_AllocationSize = 100 * 1024 * 1024, RAMBench_SleepTime = 150; // 1 = 100 MB | 2 = 0.15 seconds
        private readonly List<byte[]> RAMBench_allocations = new List<byte[]>();
        private readonly object RAMBench_lock = new object();
        private CancellationTokenSource RAMBench_cancellationTokenSource;
        private bool RAMBench_dynamicMemoryUsage = true, RAMBench_stopMode = false;
        public GlowBenchMemoryTool(){
            InitializeComponent();
            //
            Bench_TLP.Columns.Add("RAMPref", "Pref");
            Bench_TLP.Columns.Add("RAMValue", "Value");
            //
            Bench_TLP.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
            Bench_TLP.Rows.Add("RAMSpec1", "RAMVal1");
            Bench_TLP.Rows.Add("RAMSpec2", "RAMVal2");
            Bench_TLP.Rows.Add("RAMSpec3", "RAMVal3");
            Bench_TLP.Rows.Add("RAMSpec4", "RAMVal4");
            foreach (DataGridViewColumn columnPadding in Bench_TLP.Columns){
                int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
            }
            //
            Bench_TLP.ColumnHeadersVisible = false;
            Bench_TLP.AllowUserToResizeColumns = false;
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, Bench_TLP, new object[] { true });
        }
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_BenchRAM_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                Bench_MStart.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStart.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_MStart.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStart.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStart.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                Bench_MStop.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStop.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_MStop.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStop.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_MStop.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                Bench_TLP.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TLP.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_TLP.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TLP.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TLP.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_TLP.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_TLP.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TLP.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TLP.ReadOnly = true;
                //
                TSImageRenderer(Bench_MStart, GlowMain.theme == 1 ? Properties.Resources.ct_test_start_light : Properties.Resources.ct_test_start_dark, 18, ContentAlignment.MiddleRight);
                TSImageRenderer(Bench_MStop, GlowMain.theme == 1 ? Properties.Resources.ct_test_stop_light : Properties.Resources.ct_test_stop_dark, 18, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("BenchRAM", "br_title"), Application.ProductName);
                //
                Bench_TLP.Rows[0].Cells[0].Value = software_lang.TSReadLangs("BenchRAM", "br_ram_total");
                Bench_TLP.Rows[1].Cells[0].Value = software_lang.TSReadLangs("BenchRAM", "br_ram_used");
                Bench_TLP.Rows[2].Cells[0].Value = software_lang.TSReadLangs("BenchRAM", "br_ram_empty");
                Bench_TLP.Rows[3].Cells[0].Value = software_lang.TSReadLangs("BenchRAM", "br_ram_allocated");
                //
                Bench_MStart.Text = " " + software_lang.TSReadLangs("BenchRAM", "br_start");
                Bench_MStop.Text = " " + software_lang.TSReadLangs("BenchRAM", "br_stop");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_BenchRAM_Preloader()"); }
            }
        }
        private void Bench_TLP_SelectionChanged(object sender, EventArgs e){
            Bench_TLP.ClearSelection();
        }
        // MAIN LOAD
        // ======================================================================================================
        private void GlowBenchMemoryTool_Load(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Bench_TLP.Rows[3].Cells[1].Value = software_lang.TSReadLangs("BenchRAM", "br_await_start");
                //
                GTool_BenchRAM_Preloader();
                //
                Task.Run(() => Dynamic_ram_status());
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Bench_ram_settings()"); }
            }
        }
        // DYNAMIC RAM STATUS
        // ======================================================================================================
        private async Task Dynamic_ram_status(){
            try{
                using (var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem")){
                    do{
                        ManagementObjectCollection results = null;
                        ManagementObject get_ram_info = null;
                        try{
                            results = search_os.Get();
                            get_ram_info = results.Cast<ManagementObject>().FirstOrDefault();
                            if (get_ram_info == null){
                                await Task.Delay(1000 - DateTime.Now.Millisecond);
                                continue;
                            }
                            ulong total_ram = (ulong)get_ram_info["TotalVisibleMemorySize"] * 1024;
                            ulong usable_ram = (ulong)get_ram_info["FreePhysicalMemory"] * 1024;
                            ulong used_ram = total_ram >= usable_ram ? total_ram - usable_ram : 0;
                            double usage_ram_percentage = total_ram > 0 ? (double)used_ram / total_ram * 100 : 0;
                            if (IsDisposed || !IsHandleCreated){
                                return;
                            }
                            if (Bench_TLP.InvokeRequired){
                                Bench_TLP.BeginInvoke((Action)(() =>{
                                    if (IsDisposed || !IsHandleCreated){
                                        return;
                                    }
                                    Bench_TLP.Rows[0].Cells[1].Value = TS_FormatSize(total_ram);
                                    Bench_TLP.Rows[1].Cells[1].Value = $"{TS_FormatSize(used_ram)} - {usage_ram_percentage:0.00}%";
                                    Bench_TLP.Rows[2].Cells[1].Value = TS_FormatSize(usable_ram);
                                    Bench_TLP.ClearSelection();
                                }));
                            }else{
                                Bench_TLP.Rows[0].Cells[1].Value = TS_FormatSize(total_ram);
                                Bench_TLP.Rows[1].Cells[1].Value = $"{TS_FormatSize(used_ram)} - {usage_ram_percentage:0.00}%";
                                Bench_TLP.Rows[2].Cells[1].Value = TS_FormatSize(usable_ram);
                                Bench_TLP.ClearSelection();
                            }
                            await Task.Delay(1000 - DateTime.Now.Millisecond);
                        }finally{
                            try{ get_ram_info?.Dispose(); }catch { }
                            try{ results?.Dispose(); }catch { }
                        }
                    }while (RAMBench_dynamicMemoryUsage);
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Dynamic_ram_status()"); }
            }
        }
        // TIMER
        // ======================================================================================================
        private async Task BenchTimer(){
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            string titleFormat = software_lang.TSReadLangs("BenchRAM", "br_title");
            string elapsedTimeFormat = software_lang.TSReadLangs("BenchRAM", "br_elapsed_time");
            try{
                while (GlowMain.RAMbenchMode){
                    //
                    TimeSpan elapsed = stopwatch.Elapsed;
                    int fh_second = (int)elapsed.TotalSeconds % 60;
                    int fh_minute = (int)(elapsed.TotalMinutes % 60);
                    int fh_hour = (int)(elapsed.TotalHours);
                    //
                    if (InvokeRequired){
                        BeginInvoke(new Action(() => {
                            if (IsDisposed || !IsHandleCreated)
                                return;
                            Text = string.Format(titleFormat, Application.ProductName) + " | " + elapsedTimeFormat + " " + string.Format("{0:D2}:{1:D2}:{2:D2}", fh_hour, fh_minute, fh_second);
                        }));
                    }else{
                        if (IsDisposed || !IsHandleCreated)
                            return;
                        Text = string.Format(titleFormat, Application.ProductName) + " | " + elapsedTimeFormat + " " + string.Format("{0:D2}:{1:D2}:{2:D2}", fh_hour, fh_minute, fh_second);
                    }
                    //
                    await Task.Delay(1000);
                }
                try{
                    TSGetLangs software_lang_end = new TSGetLangs(GlowMain.lang_path);
                    string titleFormatEnd = software_lang_end.TSReadLangs("BenchRAM", "br_title");
                    if (IsDisposed || !IsHandleCreated)
                        return;
                    if (InvokeRequired){
                        BeginInvoke(new Action(() => {
                            if (IsDisposed || !IsHandleCreated)
                                return;
                            Text = string.Format(titleFormatEnd, Application.ProductName);
                        }));
                    }else{
                        Text = string.Format(titleFormatEnd, Application.ProductName);
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "BenchTimer() - Text Render"); }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "BenchTimer()"); }
            }
        }
        // START ENGINE
        // ======================================================================================================
        private async void Bench_MStart_Click(object sender, EventArgs e){
            try{
                if (GlowMain.RAMbenchMode) return;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                DialogResult warning_test = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("BenchRAM", "br_warning"), "\n\n", "\n"));
                if (warning_test == DialogResult.Yes){
                    try{ RAMBench_cancellationTokenSource?.Dispose(); }catch { }
                    RAMBench_cancellationTokenSource = new CancellationTokenSource();
                    //
                    RAMBench_totalAllocated = 0;
                    RAMBench_stopMode = false;
                    lock (RAMBench_lock){ RAMBench_allocations.Clear(); }
                    //
                    ulong usable_ram = 0;
                    using (var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                    using (ManagementObjectCollection results = search_os.Get()){
                        var get_ram_info = results.Cast<ManagementObject>().FirstOrDefault();
                        if (get_ram_info == null){
                            try{ RAMBench_cancellationTokenSource.Dispose(); RAMBench_cancellationTokenSource = null; }catch { }
                            return;
                        }
                        using (get_ram_info){
                            usable_ram = (ulong)get_ram_info["FreePhysicalMemory"] * 1024;
                        }
                    }
                    long target = (long)usable_ram - RAMBench_ReserveBytes;
                    RAMBench_TargetMemoryUsage = Math.Max(0, target);
                    //
                    Bench_MStart.Enabled = false;
                    Bench_MStop.Enabled = true;
                    GlowMain.RAMbenchMode = true;
                    //
                    _ = Task.Run(async () => await BenchTimer());
                    await Task.Run(() => RAMBenchmarkEngine(RAMBench_cancellationTokenSource.Token));
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Bench_MStart_Click()"); }
            }
        }
        // STOP ENGINE
        // ======================================================================================================
        private void Bench_MStop_Click(object sender, EventArgs e){
            try{
                RAMBenchStop();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Bench_MStop_Click()"); }
            }
        }
        private void RAMBenchStop(){
            try{ RAMBench_cancellationTokenSource?.Cancel(); }catch { }
            GlowMain.RAMbenchMode = false;
            RAMBench_stopMode = true;
            Bench_MStart.Enabled = true;
            Bench_MStop.Enabled = false;
            _ = Task.Run(() => Gc_run());
        }
        // GC RUN
        // ======================================================================================================
        private void Gc_run(){
            lock (RAMBench_lock){ RAMBench_allocations.Clear(); }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        // RAM BENCHMARK ENGINE
        // ======================================================================================================
        private async Task RAMBenchmarkEngine(CancellationToken cancellationToken){
            try{
                using (var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreePhysicalMemory FROM Win32_OperatingSystem")){
                    while (RAMBench_totalAllocated < RAMBench_TargetMemoryUsage && !cancellationToken.IsCancellationRequested){
                        long current_available = 0;
                        using (ManagementObjectCollection results = search_os.Get()){
                            var obj = results.Cast<ManagementObject>().FirstOrDefault();
                            if (obj == null) break;
                            using (obj){
                                current_available = (long)((ulong)obj["FreePhysicalMemory"] * 1024);
                            }
                        }
                        if (current_available <= RAMBench_ReserveBytes){
                            break;
                        }
                        byte[] allocation = new byte[RAMBench_AllocationSize];
                        for (int i = 0; i < RAMBench_AllocationSize; i += 4096){
                            allocation[i] = 1;
                        }
                        lock (RAMBench_lock){ RAMBench_allocations.Add(allocation); }
                        RAMBench_totalAllocated += RAMBench_AllocationSize;
                    long totalMemoryDuring = GC.GetTotalMemory(false);
                    if (IsDisposed || !IsHandleCreated){
                        break;
                    }
                    BeginInvoke((Action)(() =>{
                        if (IsDisposed || !IsHandleCreated){
                            return;
                        }
                        Bench_TLP.Rows[3].Cells[1].Value = TS_FormatSize(totalMemoryDuring);
                    }));
                    await Task.Delay(RAMBench_SleepTime);
                    }
                }
            }finally{
                GlowMain.RAMbenchMode = false;
                try{ RAMBench_cancellationTokenSource?.Dispose(); RAMBench_cancellationTokenSource = null; } catch { }
                _ = Task.Run(() => Gc_run());
                if (!IsDisposed && IsHandleCreated){
                    BeginInvoke((Action)(() =>{
                        if (IsDisposed || !IsHandleCreated){
                            return;
                        }
                        Bench_MStart.Enabled = true;
                        Bench_MStop.Enabled = false;
                        TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                        Text = string.Format(software_lang.TSReadLangs("BenchRAM", "br_title"), Application.ProductName);
                        Bench_TLP.Rows[3].Cells[1].Value = software_lang.TSReadLangs("BenchRAM", "br_await_start");
                        if (!RAMBench_stopMode){
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("BenchRAM", "br_end"));
                        }else{
                            RAMBench_stopMode = false;
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("BenchRAM", "br_stopped"));
                        }
                    }));
                }
            }
        }
        // EXIT
        // ======================================================================================================
        private void GlowBenchMemory_FormClosing(object sender, FormClosingEventArgs e){
            if (GlowMain.RAMbenchMode){
                e.Cancel = true;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_benchmark_ram_prs_msg"));
            }else{
                RAMBench_dynamicMemoryUsage = false;
                RAMBenchStop();
            }
        }
    }
}