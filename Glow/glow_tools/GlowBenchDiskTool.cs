using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowBenchDiskTool : Form{
        // VARIABLES
        // ======================================================================================================
        private Task DISKBench_benchmarkTask;
        private bool DISKBench_isBenchmarking = false, DISKBench_speedMode = true, DISKBench_stopMode = false;
        private ulong DISKBench_maxReadSpeed = 0, DISKBench_maxWriteSpeed = 0;
        private string DISKBench_benchmarkFilePath, DISKBench_selectDisk, DISKBench_globalTimer;
        private readonly int[] DISKBench_sizesInGB = { 1, 5, 10, 15, 20, 25, 32, 64, 128 }, DISKBench_bufferSizesInKB = { 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        private readonly List<string> DISKBench_benchmarkDiskList = new List<string>();
        private readonly List<double> DISKBench_benchmarkDiskListFreeSpace = new List<double>();
        private readonly List<string> DISKBench_benchmarkDiskListType = new List<string>();
        private readonly List<long> DISKBench_testSizes = new List<long>();
        public GlowBenchDiskTool(){ InitializeComponent(); }
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_BenchDISK_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                BackPanel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                Bench_P1.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P2.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P3.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P4.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P5.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P6.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_P7.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_DiskSelector.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_DiskSelector_List.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_DiskSelector_List.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_DiskSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_DiskSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_DiskSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_DiskSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_DiskSelector_List.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_DiskSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_DiskSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_DiskSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_DiskSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_DiskSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_DiskSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_DiskSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_SizeSelector.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_SizeSelector_List.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_SizeSelector_List.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_SizeSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_SizeSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_SizeSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_SizeSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_SizeSelector_List.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_SizeSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_SizeSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_SizeSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_SizeSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_SizeSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_SizeSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_SizeSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_SizeCustom.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_SizeCustom.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                //
                Bench_BufferSelector.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_BufferSelector_List.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_BufferSelector_List.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_BufferSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_BufferSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_BufferSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_BufferSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_BufferSelector_List.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_BufferSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                Bench_BufferSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Bench_BufferSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_BufferSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                Bench_BufferSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_BufferSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_BufferSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Bench_L_WriteSpeed.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_L_WriteSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_R_ReadSpeed.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_R_ReadSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_L_Max_WriteSpeed.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_L_Max_WriteSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                Bench_R_Max_ReadSpeed.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                Bench_R_Max_ReadSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
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
                Text = string.Format(software_lang.TSReadLangs("BenchDisk", "bd_title"), Application.ProductName);
                Bench_DiskSelector.Text = software_lang.TSReadLangs("BenchDisk", "bd_select_disk");
                Bench_SizeSelector.Text = software_lang.TSReadLangs("BenchDisk", "bd_test_size");
                Bench_BufferSelector.Text = software_lang.TSReadLangs("BenchDisk", "bd_test_buffer_size");
                //
                Bench_L_WriteSpeed.Text = software_lang.TSReadLangs("BenchDisk", "bd_time_write");
                Bench_L_Max_WriteSpeed.Text = software_lang.TSReadLangs("BenchDisk", "bd_time_max_write");
                Bench_R_ReadSpeed.Text = software_lang.TSReadLangs("BenchDisk", "bd_time_read");
                Bench_R_Max_ReadSpeed.Text = software_lang.TSReadLangs("BenchDisk", "bd_time_max_read");
                //
                if (Bench_SizeSelector_List.Items.Count > 0){
                    Bench_SizeSelector_List.Items[Bench_SizeSelector_List.Items.Count - 1] = software_lang.TSReadLangs("BenchDisk", "bd_test_size_custom");
                }
                //
                Bench_Start.Text = " " + software_lang.TSReadLangs("BenchDisk", "bd_start");
                Bench_Stop.Text = " " + software_lang.TSReadLangs("BenchDisk", "bd_stop");
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "GTool_BenchDISK_Preloader()");
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowBenchDiskTool_Load(object sender, EventArgs e){
            try{
                RefreshDriveList();
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                foreach (int size in DISKBench_sizesInGB){
                    Bench_SizeSelector_List.Items.Add(size + " GB");
                }
                Bench_SizeSelector_List.Items.Add(software_lang.TSReadLangs("BenchDisk", "bd_test_size_custom"));
                if (Bench_SizeSelector_List.Items.Count > 1){
                    if (Bench_SizeSelector_List.InvokeRequired){
                        Bench_SizeSelector_List.Invoke((Action)(() => Bench_SizeSelector_List.SelectedIndex = 1));
                    }else{
                        Bench_SizeSelector_List.SelectedIndex = 1;
                    }
                }
                DISKBench_testSizes.Clear();
                foreach (int size in DISKBench_sizesInGB){
                    DISKBench_testSizes.Add(size);
                }
                foreach (int bufferSize in DISKBench_bufferSizesInKB){
                    Bench_BufferSelector_List.Items.Add(bufferSize + " KB");
                }
                if (Bench_BufferSelector_List.Items.Count > 7){
                    Bench_BufferSelector_List.SelectedIndex = 7;
                }
                Bench_L_WriteSpeed_V.Text = software_lang.TSReadLangs("BenchDisk", "bd_start_test_await");
                Bench_L_Max_WriteSpeed_V.Text = software_lang.TSReadLangs("BenchDisk", "bd_start_test_await");
                Bench_R_ReadSpeed_V.Text = software_lang.TSReadLangs("BenchDisk", "bd_start_test_await");
                Bench_R_Max_ReadSpeed_V.Text = software_lang.TSReadLangs("BenchDisk", "bd_start_test_await");
                GTool_BenchDISK_Preloader();
                try{
                    Task start_disk_engine_x64 = Task.Run(() => { Disk_engine(); });
                }
                catch (Exception ex) { TSErrorLog.LogException(ex, "Start Disk_engine task"); }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "GlowBenchDiskTool_Load()");
            }
        }
        // DISK LIST
        // ======================================================================================================
        private void RefreshDriveList(){
            try{
                Bench_DiskSelector_List.SelectedIndexChanged -= Bench_DiskSelector_List_SelectedIndexChanged;
            }catch { }
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            var items = new List<string>();
            var diskNames = new List<string>();
            var diskFree = new List<double>();
            var diskTypes = new List<string>();
            try{
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives){
                    try{
                        string vol = string.Empty;
                        try{ vol = drive.VolumeLabel; }catch { vol = string.Empty; }
                        string totalText;
                        try{ totalText = TS_FormatSize(drive.TotalSize); }
                        catch{ totalText = "?"; }
                        string driveInfo;
                        if (string.IsNullOrWhiteSpace(vol)){
                            driveInfo = $"{software_lang.TSReadLangs("BenchDisk", "bd_select_local_disk")} ({drive.Name.Replace("\\", string.Empty)}) - {totalText}";
                        }else{
                            driveInfo = $"{vol} ({drive.Name.Replace("\\", string.Empty)}) - {totalText}";
                        }
                        items.Add(driveInfo);
                        diskNames.Add(drive.Name);
                        double freeSpace = 0;
                        try{
                            freeSpace = drive.IsReady ? (drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0) : 0;
                        }catch { freeSpace = 0; }
                        diskFree.Add(freeSpace);
                        string dtype = "unknown";
                        try{ dtype = drive.DriveType.ToString().ToLower().Trim(); }catch { dtype = "unknown"; }
                        diskTypes.Add(dtype);
                    }catch (Exception exInner){
                        TSErrorLog.LogException(exInner, "RefreshDriveList - single drive");
                    }
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "RefreshDriveList - DriveInfo");
            }
            if (Bench_DiskSelector_List.InvokeRequired){
                Bench_DiskSelector_List.Invoke((Action)(() =>{
                    Bench_DiskSelector_List.Items.Clear();
                    foreach (var it in items) Bench_DiskSelector_List.Items.Add(it);
                    if (Bench_DiskSelector_List.Items.Count > 0) Bench_DiskSelector_List.SelectedIndex = 0;
                }));
            }else{
                Bench_DiskSelector_List.Items.Clear();
                foreach (var it in items) Bench_DiskSelector_List.Items.Add(it);
                if (Bench_DiskSelector_List.Items.Count > 0) Bench_DiskSelector_List.SelectedIndex = 0;
            }
            DISKBench_benchmarkDiskList.Clear();
            DISKBench_benchmarkDiskListFreeSpace.Clear();
            DISKBench_benchmarkDiskListType.Clear();
            DISKBench_benchmarkDiskList.AddRange(diskNames);
            DISKBench_benchmarkDiskListFreeSpace.AddRange(diskFree);
            DISKBench_benchmarkDiskListType.AddRange(diskTypes);
            if (Bench_DiskSelector_List.SelectedIndex >= 0 && Bench_DiskSelector_List.SelectedIndex < DISKBench_benchmarkDiskList.Count){
                DISKBench_selectDisk = DISKBench_benchmarkDiskList[Bench_DiskSelector_List.SelectedIndex].Trim().Replace("\\", string.Empty);
            }else{
                DISKBench_selectDisk = string.Empty;
            }
            try{
                Bench_DiskSelector_List.SelectedIndexChanged += Bench_DiskSelector_List_SelectedIndexChanged;
            }catch { }
        }
        private void RefreshFreeSpaceSnapshot(){
            try{
                for (int i = 0; i < DISKBench_benchmarkDiskList.Count; i++){
                    try{
                        var di = new DriveInfo(DISKBench_benchmarkDiskList[i]);
                        if (i < DISKBench_benchmarkDiskListFreeSpace.Count){
                            DISKBench_benchmarkDiskListFreeSpace[i] = di.IsReady ? (di.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0) : 0;
                        }
                    }catch { }
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "RefreshFreeSpaceSnapshot()");
            }
        }
        // START BTN
        // ======================================================================================================
        private void Bench_Start_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                RefreshFreeSpaceSnapshot();
                if (Bench_DiskSelector_List.SelectedIndex < 0 || Bench_DiskSelector_List.SelectedIndex >= DISKBench_benchmarkDiskList.Count){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_select_disk"));
                    return;
                }
                int customIndex = Math.Max(0, Bench_SizeSelector_List.Items.Count - 1);
                if (Bench_SizeSelector_List.SelectedIndex == customIndex){
                    if (!string.IsNullOrWhiteSpace(Bench_SizeCustom.Text)){
                        if (double.TryParse(Bench_SizeCustom.Text.Trim(), out double customVal) && customVal >= 10 && customVal <= 256){
                            double reqGB = customVal;
                            int diskIdx = Bench_DiskSelector_List.SelectedIndex;
                            if (diskIdx >= 0 && diskIdx < DISKBench_benchmarkDiskListFreeSpace.Count && DISKBench_benchmarkDiskListFreeSpace[diskIdx] > reqGB){
                                Check_info_user_warning(diskIdx);
                            }else{
                                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_low_space"));
                            }
                        }else{
                            TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_space_req"));
                            Bench_SizeCustom.Text = string.Empty;
                            Bench_SizeCustom.Focus();
                        }
                    }else{
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_space_req"));
                        Bench_SizeCustom.Focus();
                    }
                }else{
                    if (Bench_SizeSelector_List.SelectedIndex >= 0 && Bench_SizeSelector_List.SelectedIndex < DISKBench_sizesInGB.Length){
                        double reqGB = DISKBench_sizesInGB[Bench_SizeSelector_List.SelectedIndex];
                        int diskIdx = Bench_DiskSelector_List.SelectedIndex;
                        if (diskIdx >= 0 && diskIdx < DISKBench_benchmarkDiskListFreeSpace.Count && DISKBench_benchmarkDiskListFreeSpace[diskIdx] > reqGB){
                            Check_info_user_warning(diskIdx);
                        }else{
                            TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_low_space"));
                        }
                    }
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Bench_Start_Click");
            }
        }
        // CHECK DISK USER INFO
        // ======================================================================================================
        private void Check_info_user_warning(int info_mode){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                DialogResult success_warning;
                var warnings = new Dictionary<string, (string MessageKey, bool StartEngine)>{
                    { "cdrom", ( "bd_disk_cdrom", true ) },
                    { "fixed", ( "bd_disk_fixed", true ) },
                    { "network", ( "bd_disk_network", true ) },
                    { "norootdirectory", ( "bd_disk_nrd", false ) },
                    { "ram", ( "bd_disk_ram", false ) },
                    { "removable", ( "bd_disk_removable", true ) },
                    { "unknown", ( "bd_disk_unknown", false ) }
                };
                string mode = (info_mode >= 0 && info_mode < DISKBench_benchmarkDiskListType.Count) ? DISKBench_benchmarkDiskListType[info_mode] : "unknown";
                string messageKey = warnings.ContainsKey(mode) ? warnings[mode].MessageKey : warnings["unknown"].MessageKey;
                bool startEngine = warnings.ContainsKey(mode) && warnings[mode].StartEngine;
                string message = string.Format(software_lang.TSReadLangs("BenchDisk", messageKey), "\n\n", "\n\n", "\n", "\n\n");
                string caption = software_lang.TSReadLangs("BenchDisk", "bd_start_engine_disk");
                if (Bench_DiskSelector_List.SelectedItem != null){
                    caption += " " + Bench_DiskSelector_List.SelectedItem.ToString().Trim();
                }
                if (!startEngine){
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, message, caption);
                    return;
                }
                success_warning = TS_MessageBoxEngine.TS_MessageBox(this, 6, message, caption);
                if (success_warning == DialogResult.Yes){
                    Start_engine();
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Check_info_user_warning()");
            }
        }
        private void Bench_DiskSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int idx = Bench_DiskSelector_List.SelectedIndex;
                if (idx < 0 || idx >= DISKBench_benchmarkDiskList.Count){
                    DISKBench_selectDisk = string.Empty;
                    return;
                }
                DISKBench_selectDisk = DISKBench_benchmarkDiskList[idx].Trim().Replace("\\", string.Empty);
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Bench_DiskSelector_List_SelectedIndexChanged()");
            }
        }
        // TIMER
        // ======================================================================================================
        private async Task BenchTimerAsync(){
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TSGetLangs g_lang = new TSGetLangs(GlowMain.lang_path);
            string elapsedTimeFormat = g_lang.TSReadLangs("BenchDisk", "bt_elapsed_time");
            try{
                while (GlowMain.DISKbenchMode){
                    TimeSpan elapsed = stopwatch.Elapsed;
                    int fh_second = (int)elapsed.TotalSeconds % 60;
                    int fh_minute = (int)(elapsed.TotalMinutes % 60);
                    int fh_hour = (int)(elapsed.TotalHours);
                    DISKBench_globalTimer = $"{elapsedTimeFormat} {fh_hour:D2}:{fh_minute:D2}:{fh_second:D2}";
                    await Task.Delay(1000);
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "BenchTimerAsync()");
            }
        }
        // START ENGINE
        // ======================================================================================================
        private void Start_engine(){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (Bench_DiskSelector_List.SelectedIndex < 0 || Bench_DiskSelector_List.SelectedIndex >= DISKBench_benchmarkDiskList.Count)
                    return;
                GlowMain.DISKbenchMode = true;
                string selectedDrive = DISKBench_benchmarkDiskList[Bench_DiskSelector_List.SelectedIndex];
                DISKBench_speedMode = true;
                DISKBench_isBenchmarking = true;
                DISKBench_maxReadSpeed = 0;
                DISKBench_maxWriteSpeed = 0;
                TSGetLangs speedLang = new TSGetLangs(GlowMain.lang_path);
                string awaitText = speedLang.TSReadLangs("BenchDisk", "bd_start_test_await");
                Bench_L_WriteSpeed_V.SetTextSafe(awaitText);
                Bench_L_Max_WriteSpeed_V.SetTextSafe(awaitText);
                Bench_R_ReadSpeed_V.SetTextSafe(awaitText);
                Bench_R_Max_ReadSpeed_V.SetTextSafe(awaitText);
                try{
                    int selSizeIndex = Bench_SizeSelector_List.SelectedIndex;
                    string customSizeText = Bench_SizeCustom.Text ?? string.Empty;
                    int bufferIndex = Bench_BufferSelector_List.SelectedIndex;
                    DISKBench_benchmarkTask = Task.Run(async () => await RunBenchmarkAsync(selectedDrive, selSizeIndex, customSizeText, bufferIndex));
                }catch (Exception ex){
                    DISKBench_isBenchmarking = false;
                    TSErrorLog.LogException(ex, "Start_engine - Task.Run");
                    TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("BenchDisk", "bd_test_not_start"), "\n\n", ex.Message));
                    return;
                }
                Bench_Start.SetEnabledSafe(false);
                Bench_Stop.SetEnabledSafe(true);
                Bench_DiskSelector_List.SetEnabledSafe(false);
                Bench_SizeSelector_List.SetEnabledSafe(false);
                Bench_SizeCustom.SetEnabledSafe(false);
                Bench_BufferSelector_List.SetEnabledSafe(false);
                DISKBench_stopMode = false;
                if (!DISKBench_speedMode){
                    DISKBench_speedMode = true;
                    Task.Run(() => { Disk_engine(); });
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Start_engine()");
            }
        }
        // GB TO BYTE
        static long GigabytesToBytes(double gigabytes){
            return (long)(gigabytes * 1024 * 1024 * 1024);
        }
        // KB TO BYTE
        static byte[] KilobytesToBytes(double kilobytes){
            return new byte[(long)(kilobytes * 1024)];
        }
        // UPDATE PROGRESS
        private void UpdateProgress(double progress){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                string title = string.Format(software_lang.TSReadLangs("BenchDisk", "bd_title"), Application.ProductName) + " - " + DISKBench_globalTimer + " - " + progress.ToString("0") + "%";
                this.SetTextSafe(title);
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "UpdateProgress()");
            }
        }
        private void ResetBenchUiAfterEarlyExit(){
            try{
                DISKBench_isBenchmarking = false;
                GlowMain.DISKbenchMode = false;
                if (!IsDisposed && IsHandleCreated){
                    this.ExecuteSafe(() =>{
                        Bench_Start.SetEnabledSafe(true);
                        Bench_Stop.SetEnabledSafe(false);
                        Bench_DiskSelector_List.SetEnabledSafe(true);
                        Bench_SizeSelector_List.SetEnabledSafe(true);
                        Bench_SizeCustom.SetEnabledSafe(true);
                        Bench_BufferSelector_List.SetEnabledSafe(true);
                    });
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "ResetBenchUiAfterEarlyExit()");
            }
        }
        // DISK BENCHMARK
        // ======================================================================================================
        private async Task RunBenchmarkAsync(string selectedDrive, int selSizeIndex, string customSizeText, int bufferIndex){
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            try{
                DISKBench_benchmarkFilePath = Path.Combine(selectedDrive, "GlowBenchDiskTestFile_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".glow");
                long fileSizeInBytes = 0;
                int global_buffer = 0;
                int customIndex = Math.Max(0, Bench_SizeSelector_List.Items.Count - 1);
                if (selSizeIndex >= 0 && selSizeIndex < DISKBench_sizesInGB.Length){
                    fileSizeInBytes = GigabytesToBytes(DISKBench_sizesInGB[selSizeIndex]);
                }else if (selSizeIndex == customIndex){
                    if (!double.TryParse(customSizeText.Trim(), out double parsed) || parsed < 10 || parsed > 256){
                        ResetBenchUiAfterEarlyExit();
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_test_custom_invalid_size"));
                        return;
                    }
                    fileSizeInBytes = GigabytesToBytes(parsed);
                }
                if (fileSizeInBytes <= 0){
                    ResetBenchUiAfterEarlyExit();
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("BenchDisk", "bd_test_file_size_not_calc"));
                    return;
                }
                if (bufferIndex >= 0 && bufferIndex < DISKBench_bufferSizesInKB.Length){
                    global_buffer = DISKBench_bufferSizesInKB[bufferIndex];
                }
                if (global_buffer <= 0) global_buffer = 1024;
                byte[] buffer = KilobytesToBytes(global_buffer);
                var timerTask = BenchTimerAsync();
                this.ExecuteSafe(() =>{ Bench_R_ReadSpeed_V.Text = "0.0 MB/s"; });
                // WRITE
                long bytesWrittenTotal = 0;
                Stopwatch swWrite = Stopwatch.StartNew();
                using (FileStream fs = new FileStream(DISKBench_benchmarkFilePath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough)){
                    long bytesWritten = 0;
                    double lastProgress = 0;
                    long lastUiBytes = 0;
                    TimeSpan lastUiTime = TimeSpan.Zero;
                    while (bytesWritten < fileSizeInBytes && DISKBench_isBenchmarking){
                        int bufferSize = (int)Math.Min(buffer.Length, fileSizeInBytes - bytesWritten);
                        fs.Write(buffer, 0, bufferSize);
                        bytesWritten += bufferSize;
                        double progress = (double)bytesWritten / fileSizeInBytes * 100;
                        if (progress - lastProgress >= 1){
                            UpdateProgress(progress);
                            lastProgress = progress;
                        }
                        TimeSpan nowW = swWrite.Elapsed;
                        if ((nowW - lastUiTime).TotalSeconds >= 1){
                            double instSec = Math.Max(0.001, (nowW - lastUiTime).TotalSeconds);
                            double instW = ((bytesWritten - lastUiBytes) / (1024.0 * 1024.0)) / instSec;
                            lastUiBytes = bytesWritten;
                            lastUiTime = nowW;
                            ulong instWb = (ulong)(instW * 1024 * 1024);
                            if (instWb > DISKBench_maxWriteSpeed) DISKBench_maxWriteSpeed = instWb;
                            this.ExecuteSafe(() =>{
                                Bench_L_WriteSpeed_V.Text = string.Format("{0:F1} MB/s", instW);
                                Bench_L_Max_WriteSpeed_V.Text = string.Format("{0:F1} MB/s", DISKBench_maxWriteSpeed / (1024.0 * 1024.0));
                            });
                        }
                        if (!GlowMain.DISKbenchMode) break;
                    }
                    fs.Flush();
                    bytesWrittenTotal = bytesWritten;
                }
                swWrite.Stop();
                double writeMBps = (bytesWrittenTotal / (1024.0 * 1024.0)) / Math.Max(0.0001, swWrite.Elapsed.TotalSeconds);
                await Task.Delay(1000);
                // READ (skip if user stopped during the pause)
                long totalBytesRead = 0;
                double readMBps = 0;
                if (DISKBench_isBenchmarking && GlowMain.DISKbenchMode && File.Exists(DISKBench_benchmarkFilePath)){
                    this.ExecuteSafe(() =>{ Bench_L_WriteSpeed_V.Text = "0.0 MB/s"; });
                    Stopwatch swRead = Stopwatch.StartNew();
                    double lastProgressRead = 0;
                    long lastUiReadBytes = 0;
                    TimeSpan lastUiReadTime = TimeSpan.Zero;
                    using (FileStream fs = new FileStream(DISKBench_benchmarkFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length, FileOptions.SequentialScan)){
                        int bytesRead;
                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0 && DISKBench_isBenchmarking){
                            totalBytesRead += bytesRead;
                            double progress = fileSizeInBytes > 0 ? (double)totalBytesRead / fileSizeInBytes * 100 : 0;
                            if (progress - lastProgressRead >= 1){
                                UpdateProgress(progress);
                                lastProgressRead = progress;
                            }
                            TimeSpan nowR = swRead.Elapsed;
                            if ((nowR - lastUiReadTime).TotalSeconds >= 1){
                                double instSec = Math.Max(0.001, (nowR - lastUiReadTime).TotalSeconds);
                                double instR = ((totalBytesRead - lastUiReadBytes) / (1024.0 * 1024.0)) / instSec;
                                lastUiReadBytes = totalBytesRead;
                                lastUiReadTime = nowR;
                                ulong instRb = (ulong)(instR * 1024 * 1024);
                                if (instRb > DISKBench_maxReadSpeed) DISKBench_maxReadSpeed = instRb;
                                this.ExecuteSafe(() =>{
                                    Bench_R_ReadSpeed_V.Text = string.Format("{0:F1} MB/s", instR);
                                    Bench_R_Max_ReadSpeed_V.Text = string.Format("{0:F1} MB/s", DISKBench_maxReadSpeed / (1024.0 * 1024.0));
                                });
                            }
                            if (!GlowMain.DISKbenchMode) break;
                        }
                    }
                    swRead.Stop();
                    readMBps = (totalBytesRead / (1024.0 * 1024.0)) / Math.Max(0.0001, swRead.Elapsed.TotalSeconds);
                }
                if (File.Exists(DISKBench_benchmarkFilePath)){
                    try { File.Delete(DISKBench_benchmarkFilePath); }catch { }
                }
                if (!IsDisposed && IsHandleCreated){
                    this.ExecuteSafe(() =>{
                        Text = string.Format(software_lang.TSReadLangs("BenchDisk", "bd_title"), Application.ProductName);
                        GlowMain.DISKbenchMode = false;
                        double finalWrite = writeMBps;
                        double finalRead = readMBps;
                        if (!DISKBench_stopMode){
                            string resultMsg = software_lang.TSReadLangs("BenchDisk", "bd_result_success");
                            resultMsg += "\n\n" + Bench_L_WriteSpeed.Text + " " + string.Format("{0:F1} MB/s", finalWrite);
                            resultMsg += "\n" + Bench_R_ReadSpeed.Text + " " + string.Format("{0:F1} MB/s", finalRead);
                            if ((ulong)(finalRead * 1024 * 1024) > DISKBench_maxReadSpeed) DISKBench_maxReadSpeed = (ulong)(finalRead * 1024 * 1024);
                            if ((ulong)(finalWrite * 1024 * 1024) > DISKBench_maxWriteSpeed) DISKBench_maxWriteSpeed = (ulong)(finalWrite * 1024 * 1024);
                            Bench_L_WriteSpeed_V.SetTextSafe(string.Format("{0:F1} MB/s", finalWrite));
                            Bench_R_ReadSpeed_V.SetTextSafe(string.Format("{0:F1} MB/s", finalRead));
                            Bench_L_Max_WriteSpeed_V.SetTextSafe(string.Format("{0:F1} MB/s", DISKBench_maxWriteSpeed / (1024.0 * 1024.0)));
                            Bench_R_Max_ReadSpeed_V.SetTextSafe(string.Format("{0:F1} MB/s", DISKBench_maxReadSpeed / (1024.0 * 1024.0)));
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, resultMsg);
                            string awaitText = software_lang.TSReadLangs("BenchDisk", "bd_start_test_await");
                            Bench_L_WriteSpeed_V.SetTextSafe(awaitText);
                            Bench_L_Max_WriteSpeed_V.SetTextSafe(awaitText);
                            Bench_R_ReadSpeed_V.SetTextSafe(awaitText);
                            Bench_R_Max_ReadSpeed_V.SetTextSafe(awaitText);
                        }else{
                            DISKBench_stopMode = false;
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("BenchDisk", "bd_result_exit"));
                        }
                    });
                }
                DISKBench_isBenchmarking = false;
                if (!IsDisposed && IsHandleCreated){
                    this.ExecuteSafe(() =>{
                        Bench_Start.SetEnabledSafe(true);
                        Bench_Stop.SetEnabledSafe(false);
                        Bench_DiskSelector_List.SetEnabledSafe(true);
                        Bench_SizeSelector_List.SetEnabledSafe(true);
                        Bench_SizeCustom.SetEnabledSafe(true);
                        Bench_BufferSelector_List.SetEnabledSafe(true);
                    });
                }
            }catch (Exception ex){
                DISKBench_isBenchmarking = false;
                TSErrorLog.LogException(ex, "RunBenchmarkAsync()");
                if (!IsDisposed && IsHandleCreated){
                    this.ExecuteSafe(() =>{
                        TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("BenchDisk", "bd_test_fail"), "\n\n", ex.Message));
                        Bench_Start.SetEnabledSafe(true);
                        Bench_Stop.SetEnabledSafe(false);
                        Bench_DiskSelector_List.SetEnabledSafe(true);
                        Bench_SizeSelector_List.SetEnabledSafe(true);
                        Bench_SizeCustom.SetEnabledSafe(true);
                        Bench_BufferSelector_List.SetEnabledSafe(true);
                    });
                }
            }
            finally{
                try{
                    GlowMain.DISKbenchMode = false;
                    if (!string.IsNullOrEmpty(DISKBench_benchmarkFilePath) && File.Exists(DISKBench_benchmarkFilePath)){
                        File.Delete(DISKBench_benchmarkFilePath);
                    }
                }catch{ }
            }
        }
        // DISK ENGINE
        // ======================================================================================================
        private async void Disk_engine(){
            try{
                while (DISKBench_speedMode){
                    if (IsDisposed || !IsHandleCreated)
                        break;
                    try{
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, DiskReadBytesPersec, DiskWriteBytesPersec FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk"))
                        using (ManagementObjectCollection results = searcher.Get()){
                            Dictionary<string, (ulong readSpeed, ulong writeSpeed)> diskData = new Dictionary<string, (ulong, ulong)>();
                            foreach (ManagementObject obj in results.Cast<ManagementObject>()){
                                using (obj){
                                    string diskName = (string)obj["Name"];
                                    if (string.IsNullOrWhiteSpace(diskName)) continue;
                                    ulong diskReadSpeed = 0;
                                    ulong diskWriteSpeed = 0;
                                    try{
                                        diskReadSpeed = Convert.ToUInt64(obj["DiskReadBytesPersec"]);
                                        diskWriteSpeed = Convert.ToUInt64(obj["DiskWriteBytesPersec"]);
                                    }catch { }
                                    if (diskName.Trim() != "_Total"){
                                        diskData[diskName] = (diskReadSpeed, diskWriteSpeed);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(DISKBench_selectDisk) && GlowMain.DISKbenchMode){
                                string wanted = " " + DISKBench_selectDisk.Trim();
                                var selectedDisks = diskData.Where(kvp => (" " + kvp.Key).IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0);
                                foreach (var diskEntry in selectedDisks){
                                    var (readSpeed, writeSpeed) = diskEntry.Value;
                                    float diskReadSpeedMB = (float)readSpeed / (1024f * 1024f);
                                    float diskWriteSpeedMB = (float)writeSpeed / (1024f * 1024f);
                                    if (IsDisposed || !IsHandleCreated) break;
                                    this.ExecuteSafe(() =>{
                                        if (readSpeed > DISKBench_maxReadSpeed) DISKBench_maxReadSpeed = readSpeed;
                                        if (writeSpeed > DISKBench_maxWriteSpeed) DISKBench_maxWriteSpeed = writeSpeed;
                                        Bench_R_ReadSpeed_V.Text = $"{diskReadSpeedMB:F1} MB/s";
                                        Bench_R_Max_ReadSpeed_V.Text = $"{(DISKBench_maxReadSpeed / (1024f * 1024f)):F1} MB/s";
                                        Bench_L_WriteSpeed_V.Text = $"{diskWriteSpeedMB:F1} MB/s";
                                        Bench_L_Max_WriteSpeed_V.Text = $"{(DISKBench_maxWriteSpeed / (1024f * 1024f)):F1} MB/s";
                                    });
                                }
                            }
                        }
                    }catch (Exception ex){
                        TSErrorLog.LogException(ex, "Disk_engine loop.");
                    }
                    await Task.Delay(1000);
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Disk_engine()");
            }
        }
        // STOP BENCHMARK
        // ======================================================================================================
        private void Bench_Stop_Click(object sender, EventArgs e){
            Stop_engine();
        }
        private async void Stop_engine(){
            GlowMain.DISKbenchMode = false;
            DISKBench_isBenchmarking = false;
            DISKBench_stopMode = true;
            if (DISKBench_benchmarkTask != null){
                try{
                    await DISKBench_benchmarkTask;
                }catch (Exception ex){
                    TSErrorLog.LogException(ex, "Stop_engine - awaiting benchmark task.");
                }
            }
            Bench_Start.SetEnabledSafe(true);
            Bench_Stop.SetEnabledSafe(false);
            Bench_DiskSelector_List.SetEnabledSafe(true);
            Bench_SizeSelector_List.SetEnabledSafe(true);
            Bench_SizeCustom.SetEnabledSafe(true);
            Bench_BufferSelector_List.SetEnabledSafe(true);
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            Bench_L_WriteSpeed_V.SetTextSafe(software_lang.TSReadLangs("BenchDisk", "bd_start_test_await"));
            Bench_L_Max_WriteSpeed_V.SetTextSafe(software_lang.TSReadLangs("BenchDisk", "bd_start_test_await"));
            Bench_R_ReadSpeed_V.SetTextSafe(software_lang.TSReadLangs("BenchDisk", "bd_start_test_await"));
            Bench_R_Max_ReadSpeed_V.SetTextSafe(software_lang.TSReadLangs("BenchDisk", "bd_start_test_await"));
            if (!string.IsNullOrEmpty(DISKBench_benchmarkFilePath) && File.Exists(DISKBench_benchmarkFilePath)){
                try { File.Delete(DISKBench_benchmarkFilePath); }catch { }
                this.SetTextSafe(string.Format(software_lang.TSReadLangs("BenchDisk", "bd_title"), Application.ProductName));
                TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("BenchDisk", "bd_result_exit"));
            }else{
                this.SetTextSafe(string.Format(software_lang.TSReadLangs("BenchDisk", "bd_title"), Application.ProductName));
            }
        }
        // CUSTOM SIZE CHANGE
        // ======================================================================================================
        private void Bench_SizeSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                if (Bench_SizeSelector_List.SelectedIndex == Bench_SizeSelector_List.Items.Count - 1){
                    Bench_SizeCustom.SetVisibleSafe(true);
                }else{
                    Bench_SizeCustom.SetVisibleSafe(false);
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "Bench_SizeSelector_List_SelectedIndexChanged()");
            }
        }
        // NUMERIC INPUT
        // ======================================================================================================
        private void Bench_SizeCustom_KeyPress(object sender, KeyPressEventArgs e){
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)){
                e.Handled = true;
            }
        }
        // EXIT STOP ENGINE
        // ======================================================================================================
        private void GlowBenchDisk_FormClosing(object sender, FormClosingEventArgs e){
            try{
                if (GlowMain.DISKbenchMode){
                    e.Cancel = true;
                    TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_benchmark_disk_prs_msg"));
                }else{
                    DISKBench_speedMode = false;
                    Stop_engine();
                }
            }catch (Exception ex){
                TSErrorLog.LogException(ex, "GlowBenchDisk_FormClosing()");
            }
        }
    }
}