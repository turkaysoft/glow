using System;
using System.Linq;
using System.Drawing;
using System.Management;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowBenchCPUTool : Form{
        // VARIABLES
        // ======================================================================================================
        private Task[] CPUBench_taskList;
        private Stopwatch CPUBench_stopWatch;
        private bool CPUBench_isRunning = false;
        private double[] CPUBench_collector;
        private int CPUBench_singleThreadScore = 0, CPUBench_multiThreadScore = 0;
        public GlowBenchCPUTool() { InitializeComponent(); }
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_BenchCPU_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_BG_Panel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                Bench_TLP_T_P1.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP_T_P2.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP_T_P3.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP_R_P1.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TLP_R_P2.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_CPUName.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_CPUCores.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                //
                Bench_Label_RSingle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_Label_RSingleResult.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Label_RMulti.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_Label_RMultiResult.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                //
                Bench_ModeSelector.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_ModeSelector_List.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_ModeSelector_List.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_ModeSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_ModeSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_ModeSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_ModeSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_ModeSelector_List.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_ModeSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_ModeSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_ModeSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_ModeSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_ModeSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_ModeSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_ModeSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_TimeSelector.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TimeSelector_List.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TimeSelector_List.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TimeSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TimeSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TimeSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TimeSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TimeSelector_List.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_TimeSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_TimeSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_TimeSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TimeSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TimeSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_TimeSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_TimeSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_TimeCustom.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_TimeCustom.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                //
                Bench_Start.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Start.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_Start.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Start.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Start.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                Bench_Stop.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Stop.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_Stop.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Stop.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_Stop.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(Bench_Start, GlowMain.theme == 1 ? Properties.Resources.ct_test_start_light : Properties.Resources.ct_test_start_dark, 18, ContentAlignment.MiddleRight);
                TSImageRenderer(Bench_Stop, GlowMain.theme == 1 ? Properties.Resources.ct_test_stop_light : Properties.Resources.ct_test_stop_dark, 18, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("BenchCPU", "bc_title"), Application.ProductName);
                //
                Bench_ModeSelector.Text = software_lang.TSReadLangs("BenchCPU", "bc_level");
                Bench_ModeSelector_List.Items[0] = software_lang.TSReadLangs("BenchCPU", "bc_level_1");
                Bench_ModeSelector_List.Items[1] = software_lang.TSReadLangs("BenchCPU", "bc_level_2");
                Bench_ModeSelector_List.Items[2] = software_lang.TSReadLangs("BenchCPU", "bc_level_3");
                Bench_ModeSelector_List.Items[3] = software_lang.TSReadLangs("BenchCPU", "bc_level_4");
                //
                Bench_TimeSelector.Text = software_lang.TSReadLangs("BenchCPU", "bc_time");
                Bench_TimeSelector_List.Items[0] = software_lang.TSReadLangs("BenchCPU", "bc_time_1");
                Bench_TimeSelector_List.Items[1] = software_lang.TSReadLangs("BenchCPU", "bc_time_2");
                Bench_TimeSelector_List.Items[2] = software_lang.TSReadLangs("BenchCPU", "bc_time_3");
                Bench_TimeSelector_List.Items[3] = software_lang.TSReadLangs("BenchCPU", "bc_time_4");
                Bench_TimeSelector_List.Items[4] = software_lang.TSReadLangs("BenchCPU", "bc_time_5");
                Bench_TimeSelector_List.Items[5] = software_lang.TSReadLangs("BenchCPU", "bc_time_6");
                //
                Bench_Label_RSingle.Text = software_lang.TSReadLangs("BenchCPU", "bc_score_single");
                Bench_Label_RMulti.Text = software_lang.TSReadLangs("BenchCPU", "bc_score_multi");
                //
                Bench_Label_RSingleResult.Text = software_lang.TSReadLangs("BenchCPU", "bc_score_start_await");
                Bench_Label_RMultiResult.Text = software_lang.TSReadLangs("BenchCPU", "bc_score_start_await");
                //
                Bench_Start.Text = " " + software_lang.TSReadLangs("BenchCPU", "bc_start_engine");
                Bench_Stop.Text = " " + software_lang.TSReadLangs("BenchCPU", "bc_stop_engine");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_BenchCPU_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowBenchCPUTool_Load(object sender, EventArgs e){
            try{
                Cpu_bench_add_mode();
                GTool_BenchCPU_Preloader();
                // TEXT CPU NAME
                Task.Run(() => GetCPUInfo());
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowBenchCPUTool_Load()"); }
            }
        }
        // GET CPU INFO
        // ======================================================================================================
        private void GetCPUInfo(){
            try{
                var software_lang = new TSGetLangs(GlowMain.lang_path);
                if (IsDisposed || !IsHandleCreated)
                    return;
                BeginInvoke(new Action(() => {
                    Bench_CPUName.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_loading");
                    Bench_CPUCores.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_loading");
                }));
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, NumberOfCores, ThreadCount FROM Win32_Processor")){
                    ManagementObjectCollection results = null;
                    try{
                        results = searcher.Get();
                        foreach (ManagementObject queryObj in results.Cast<ManagementObject>()){
                            using (queryObj){
                                string cpuName = Convert.ToString(queryObj["Name"]).Trim();
                                string cpuCores = string.Format(software_lang.TSReadLangs("BenchCPU", "bc_core_thread"), queryObj["NumberOfCores"], queryObj["ThreadCount"]);
                                if (IsDisposed || !IsHandleCreated)
                                    return;
                                BeginInvoke(new Action(() => {
                                    Bench_CPUName.Text = cpuName;
                                    Bench_CPUCores.Text = cpuCores;
                                }));
                            }
                        }
                    }finally{
                        try{ results?.Dispose(); }catch { }
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GetCPUInfo()"); }
            }
        }
        private void Cpu_bench_add_mode(){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                //
                Bench_ModeSelector.Text = software_lang.TSReadLangs("BenchCPU", "bc_level");
                Bench_ModeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_level_1"));
                Bench_ModeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_level_2"));
                Bench_ModeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_level_3"));
                Bench_ModeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_level_4"));
                Bench_ModeSelector_List.SelectedIndex = 0;
                //
                Bench_TimeSelector.Text = software_lang.TSReadLangs("BenchCPU", "bc_time");
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_1"));
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_2"));
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_3"));
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_4"));
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_5"));
                Bench_TimeSelector_List.Items.Add(software_lang.TSReadLangs("BenchCPU", "bc_time_6"));
                Bench_TimeSelector_List.SelectedIndex = 0;
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Cpu_bench_add_mode()"); }
            }
        }
        // CUSTOM TIME MODE
        // ======================================================================================================
        private void Bench_TimeSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            if (Bench_TimeSelector_List.SelectedIndex == 5){
                Bench_TimeCustom.Visible = true;
            }else{
                Bench_TimeCustom.Visible = false;
            }
        }
        private void Bench_TimeCustom_TextBox_KeyPress(object sender, KeyPressEventArgs e){
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)){
                e.Handled = true;
            }
        }
        // START BTN
        // ======================================================================================================
        private void Bench_Start_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (Bench_ModeSelector_List.SelectedIndex == 3){
                    DialogResult info_warning_hard = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("BenchCPU", "bc_lethal_warning"), "\n\n", "\n\n", "\n\n"));
                    if (info_warning_hard == DialogResult.Yes)
                    {
                        Bench_start_engine();
                    }
                }else{
                    if (Bench_TimeSelector_List.SelectedIndex == 5){
                        if (!string.IsNullOrEmpty(Bench_TimeCustom.Text.Trim())){
                            DialogResult info_warning_normal = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("BenchCPU", "bc_test_start_warning"), "\n\n", "\n\n", "\n\n"));
                            if (info_warning_normal == DialogResult.Yes){
                                Bench_start_engine();
                            }
                        }else{
                            TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchCPU", "bc_time_custom_warning"));
                        }
                    }else{
                        DialogResult info_warning_normal = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("BenchCPU", "bc_test_start_warning"), "\n\n", "\n\n", "\n\n"));
                        if (info_warning_normal == DialogResult.Yes){
                            Bench_start_engine();
                        }
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Bench_Start_Click()"); }
            }
        }
        // TIMER
        // ======================================================================================================
        private async Task BenchTimerAsync(){
            var software_lang = new TSGetLangs(GlowMain.lang_path);
            string titleFormat = string.Format(software_lang.TSReadLangs("BenchCPU", "bc_title"), Application.ProductName);
            string elapsedTimeFormat = software_lang.TSReadLangs("BenchCPU", "bc_elapsed_time");
            try{
                while (GlowMain.CPUbenchMode){
                    if (CPUBench_stopWatch == null)
                        break;
                    TimeSpan elapsed = CPUBench_stopWatch.Elapsed;
                    int fh_second = (int)elapsed.TotalSeconds % 60;
                    int fh_minute = (int)(elapsed.TotalMinutes % 60);
                    int fh_hour = (int)elapsed.TotalHours;
                    if (IsDisposed || !IsHandleCreated)
                        break;
                    BeginInvoke(new Action(() => {
                        Text = $"{titleFormat} - {elapsedTimeFormat} {fh_hour:D2}:{fh_minute:D2}:{fh_second:D2}";
                    }));
                    await Task.Delay(1000);
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "BenchTimerAsync() - Timer"); }
            }
        }
        // CPU BENCHMARK ENGINE
        // ======================================================================================================
        private async void Bench_start_engine(){
            if (!CPUBench_isRunning){
                GlowMain.CPUbenchMode = true;
                CPUBench_isRunning = true;
                //
                Bench_Start.Enabled = false;
                Bench_Stop.Enabled = true;
                Bench_ModeSelector_List.Enabled = false;
                Bench_TimeSelector_List.Enabled = false;
                Bench_TimeCustom.Enabled = false;
                // CPU SYSTEM
                int coreCount = Environment.ProcessorCount;
                if (Bench_ModeSelector_List.SelectedIndex == 0)
                    coreCount /= 3;
                else if (Bench_ModeSelector_List.SelectedIndex == 1)
                    coreCount /= 2;
                else if (Bench_ModeSelector_List.SelectedIndex == 2)
                    coreCount -= 1;
                if (coreCount < 1)
                    coreCount = 1;
                // ENGINE STARTER
                CPUBench_stopWatch = new Stopwatch();
                CPUBench_collector = new double[coreCount];
                // REAL SCORE SAMPLER (measures actual throughput, not a static formula)
                var updateScoreTask = Task.Run(async () => {
                    double prevTotal = 0;
                    bool firstSample = true;
                    DateTime prevTime = DateTime.UtcNow;
                    while (CPUBench_isRunning && GlowMain.CPUbenchMode){
                        await Task.Delay(500);
                        if (!CPUBench_isRunning || !GlowMain.CPUbenchMode)
                            break;
                        double total = 0;
                        try{
                            double[] snapshot = CPUBench_collector;
                            if (snapshot == null)
                                continue;
                            for (int k = 0; k < snapshot.Length; k++)
                                total += snapshot[k];
                        }catch { continue; }
                        DateTime now = DateTime.UtcNow;
                        double secs = (now - prevTime).TotalSeconds;
                        prevTime = now;
                        if (firstSample || secs <= 0){ prevTotal = total; firstSample = false; continue; }
                        double delta = total - prevTotal;
                        prevTotal = total;
                        if (delta < 0) delta = 0;
                        double multiOps = delta / secs;
                        double singleOps = coreCount > 0 ? multiOps / coreCount : multiOps;
                        int singleScore = singleOps > int.MaxValue ? int.MaxValue : (int)Math.Round(singleOps);
                        int multiScore = multiOps > int.MaxValue ? int.MaxValue : (int)Math.Round(multiOps);
                        CPUBench_singleThreadScore = singleScore;
                        CPUBench_multiThreadScore = multiScore;
                        if (IsDisposed || !IsHandleCreated)
                            break;
                        try{
                            BeginInvoke(new Action(() => {
                                Bench_Label_RSingleResult.Text = singleScore.ToString("N0");
                                Bench_Label_RMultiResult.Text = multiScore.ToString("N0");
                            }));
                        }catch { }
                    }
                });
                //
                TimeSpan benchDuration = TimeSpan.FromMinutes(1);
                if (Bench_TimeSelector_List.SelectedIndex == 0)
                    benchDuration = TimeSpan.FromSeconds(30);
                else if (Bench_TimeSelector_List.SelectedIndex == 1)
                    benchDuration = TimeSpan.FromMinutes(1);
                else if (Bench_TimeSelector_List.SelectedIndex == 2)
                    benchDuration = TimeSpan.FromMinutes(15);
                else if (Bench_TimeSelector_List.SelectedIndex == 3)
                    benchDuration = TimeSpan.FromMinutes(30);
                else if (Bench_TimeSelector_List.SelectedIndex == 4)
                    benchDuration = TimeSpan.FromHours(1);
                else if (!string.IsNullOrEmpty(Bench_TimeCustom.Text)){
                    if (double.TryParse(Bench_TimeCustom.Text.Trim(), out double customMinutes) && customMinutes > 0){
                        if (customMinutes > 180) customMinutes = 180;
                        benchDuration = TimeSpan.FromMinutes(customMinutes);
                    }
                }
                DateTime endTime = DateTime.Now.Add(benchDuration);
                //
                var timerTask = BenchTimerAsync();
                //
                CPUBench_stopWatch.Start();
                // ALL CORE WITH TASKS
                CPUBench_taskList = new Task[coreCount];
                for (int i = 0; i < coreCount; i++){
                    int coreIndex = i;
                    CPUBench_taskList[i] = Task.Run(() => {
                        // ENGINE MODE
                        Random random = new Random(unchecked(Guid.NewGuid().GetHashCode() ^ (coreIndex * 997) ^ Thread.CurrentThread.ManagedThreadId));
                        while (CPUBench_isRunning && DateTime.Now < endTime){
                            double number = random.NextDouble();
                            double result = Math.Sqrt(number);
                            CPUBench_collector[coreIndex] += result;
                        }
                    });
                }
                //
                try{
                    await Task.WhenAll(CPUBench_taskList);
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CPUBench - Task.WhenAll"); }
                }
                //
                CPUBench_stopWatch.Stop();
                bool stoppedEarly = !CPUBench_isRunning || DateTime.Now < endTime;
                CPUBench_isRunning = false;
                GlowMain.CPUbenchMode = false;
                //
                try{
                    await updateScoreTask;
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CPUBench - updateScoreTask"); }
                }
                try{
                    await timerTask;
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CPUBench - timerTask"); }
                }
                //
                if (!IsDisposed && IsHandleCreated && !stoppedEarly){
                    BeginInvoke(new Action(() => {
                        Bench_Start.Enabled = true;
                        Bench_Stop.Enabled = false;
                        Bench_ModeSelector_List.Enabled = true;
                        Bench_TimeSelector_List.Enabled = true;
                        Bench_TimeCustom.Enabled = true;
                        TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                        Text = $"{string.Format(software_lang.TSReadLangs("BenchCPU", "bc_title"), Application.ProductName)} | {software_lang.TSReadLangs("BenchCPU", "bc_end_test")}";
                    }));
                }
            }
        }
        // ENGINE STOP BTN
        // ======================================================================================================
        private void Bench_Stop_Click(object sender, EventArgs e){
            Bench_stop_engine();
        }
        // ENGINE STOP MODE
        // ======================================================================================================
        private async void Bench_stop_engine(){
            if (CPUBench_isRunning){
                GlowMain.CPUbenchMode = false;
                CPUBench_isRunning = false;
                //
                Bench_Start.Enabled = true;
                Bench_Stop.Enabled = false;
                Bench_ModeSelector_List.Enabled = true;
                Bench_TimeSelector_List.Enabled = true;
                Bench_TimeCustom.Enabled = true;
                //
                try{
                    if (CPUBench_taskList != null){
                        await Task.WhenAll(CPUBench_taskList);
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Bench_stop_engine()"); }
                }
                //
                CPUBench_stopWatch?.Stop();
                //
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = $"{string.Format(software_lang.TSReadLangs("BenchCPU", "bc_title"), Application.ProductName)} | {software_lang.TSReadLangs("BenchCPU", "bc_stop_engine_message")}";
                //
                // Console.WriteLine($"Computation time: {stopwatch.Elapsed.Seconds} seconds");
            }
        }
        // EXIT
        // ======================================================================================================
        private void GlowBenchCPU_FormClosing(object sender, FormClosingEventArgs e){
            if (GlowMain.CPUbenchMode || CPUBench_isRunning){
                e.Cancel = true;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_benchmark_cpu_prs_msg"));
            }
        }
    }
}