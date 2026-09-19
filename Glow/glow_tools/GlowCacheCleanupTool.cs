using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowCacheCleanupTool : Form{
        public GlowCacheCleanupTool(){
            InitializeComponent();
            //
            CCTTable.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
            CCTTable.Columns.Add("CleanupName", "Name");
            CCTTable.Columns.Add("CleanupPath", "Path");
            CCTTable.Columns.Add("CleanupSize", "Size");
        }
        // CLEAN SYSTEM
        // ======================================================================================================
        readonly List<string> cct_path_list = new List<string>(){
            Environment.ExpandEnvironmentVariables(@"%SystemRoot%\TEMP"),
            Environment.ExpandEnvironmentVariables("%TEMP%"),
            Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Prefetch"),
            Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\Windows\Explorer"),
            Environment.ExpandEnvironmentVariables(@"%SystemRoot%\SoftwareDistribution\Download"),
        };
        private readonly List<long> CCleanup_pathSizes = new List<long>();
        private string CCleanup_cctTitle;
        private volatile bool CCleanup_aRefresh = true, CCleanup_aRefreshRepeat = false;
        private readonly SemaphoreSlim _sizeCheckLock = new SemaphoreSlim(1, 1);
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_CacheCleanup_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                BG_Panel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                CCTTable.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                CCTTable.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                CCTTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                CCTTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                CCTTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                CCTTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCTTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCTTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                CCTTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCTTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                CCT_SelectLabel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                CCT_SelectLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                //
                CCT_StartBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCT_StartBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                CCT_StartBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCT_StartBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                CCT_StartBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(CCT_StartBtn, GlowMain.theme == 1 ? Properties.Resources.ct_clean_light : Properties.Resources.ct_clean_dark, 22, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("CacheCleanupTool", "cct_title"), Application.ProductName);
                //
                CCTTable.Columns[0].HeaderText = software_lang.TSReadLangs("CacheCleanupTool", "cct_h_info_feature");
                CCTTable.Columns[1].HeaderText = software_lang.TSReadLangs("CacheCleanupTool", "cct_h_info_path");
                CCTTable.Columns[2].HeaderText = software_lang.TSReadLangs("CacheCleanupTool", "cct_h_info_size");
                //
                CCT_StartBtn.Text = " " + software_lang.TSReadLangs("CacheCleanupTool", "cct_clean");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_CacheCleanup_Preloader()"); }
            }
        }
        // CCT LOAD
        // ======================================================================================================
        private async void GlowCacheCleanupTool_Load(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                // GET THEME
                GTool_CacheCleanup_Preloader();
                List<string> cct_folder_name = new List<string>(){
                    software_lang.TSReadLangs("CacheCleanupTool", "cct_p_system_temp"),
                    software_lang.TSReadLangs("CacheCleanupTool", "cct_p_temp_user"),
                    software_lang.TSReadLangs("CacheCleanupTool", "cct_p_windows_temp"),
                    software_lang.TSReadLangs("CacheCleanupTool", "cct_p_windows_icon_temp"),
                    software_lang.TSReadLangs("CacheCleanupTool", "cct_p_windows_update_temp")
                };
                for (int i = 0; i <= cct_path_list.Count - 1; i++){
                    CCTTable.Rows.Add(cct_folder_name[i], cct_path_list[i], software_lang.TSReadLangs("CacheCleanupTool", "cct_refreshing_title"));
                }
                CCTTable.Columns[0].Width = (int)(175 * this.DeviceDpi / 96f);
                CCTTable.Columns[2].Width = (int)(125 * this.DeviceDpi / 96f);
                foreach (DataGridViewColumn CCT_Column in CCTTable.Columns){
                    CCT_Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                foreach (DataGridViewColumn columnPadding in CCTTable.Columns){
                    int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                    columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
                }
                CCTTable.ClearSelection();
                CCleanup_cctTitle = string.Format(software_lang.TSReadLangs("CacheCleanupTool", "cct_title"), Application.ProductName);
                // START FOLDER SIZE CHECK ALGORITHM & START AUTO FOLDER SIZE ALGORITHM
                await CheckFolderSizesAsync();
                _ = AutoFolderSizeRefreshAsync();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowCacheCleanupTool_Load()"); }
            }
        }
        // CHECK FOLDER SIZE ALGORITHM
        // ======================================================================================================
        private void Check_folder_sizes(){
            try{
                CCleanup_pathSizes.Clear();
                string explorerDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");
                string iconCacheDb = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IconCache.db");
                foreach (var path in cct_path_list){
                    CCleanup_pathSizes.Add(GetPathSizeSafe(path, explorerDir, iconCacheDb));
                }
                for (int i = 0; i < CCleanup_pathSizes.Count && i < CCTTable.Rows.Count; i++){
                    int index = i;
                    long size = CCleanup_pathSizes[i];
                    try{
                        if (CCTTable.InvokeRequired){
                            CCTTable.BeginInvoke(new Action(() => {
                                try{
                                    if (IsDisposed || !IsHandleCreated) return;
                                    if (index < CCTTable.Rows.Count) CCTTable.Rows[index].Cells[2].Value = TS_FormatSize(size);
                                }catch { }
                            }));
                        }else{
                            CCTTable.Rows[index].Cells[2].Value = TS_FormatSize(size);
                        }
                    }catch { }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Check_folder_sizes()"); }
            }
            finally { CCleanup_pathSizes.Clear(); }
        }
        private static long GetPathSizeSafe(string path, string explorerDir, string iconCacheDb){
            long total = 0;
            try{
                if (string.Equals(path, explorerDir, StringComparison.OrdinalIgnoreCase)){
                    if (Directory.Exists(explorerDir)){
                        try{
                            foreach (var f in Directory.EnumerateFiles(explorerDir, "iconcache*", SearchOption.TopDirectoryOnly)){
                                try { total += new FileInfo(f).Length; } catch { }
                            }
                        }catch { }
                        try{
                            foreach (var f in Directory.EnumerateFiles(explorerDir, "thumbcache*", SearchOption.TopDirectoryOnly)){
                                try { total += new FileInfo(f).Length; } catch { }
                            }
                        }catch { }
                    }
                    if (File.Exists(iconCacheDb)){
                        try { total += new FileInfo(iconCacheDb).Length; } catch { }
                    }
                }else if (Directory.Exists(path)){
                    var stack = new Stack<string>();
                    stack.Push(path);
                    while (stack.Count > 0){
                        string dir = stack.Pop();
                        string[] files = null;
                        try{ files = Directory.GetFiles(dir); }catch{ continue; }
                        if (files != null){
                            foreach (var f in files){
                                try { total += new FileInfo(f).Length; } catch { }
                            }
                        }
                        string[] subDirs = null;
                        try{ subDirs = Directory.GetDirectories(dir); }catch{ continue; }
                        if (subDirs != null){
                            foreach (var d in subDirs){
                                try{
                                    var di = new DirectoryInfo(d);
                                    if ((di.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                                    stack.Push(d);
                                }catch { }
                            }
                        }
                    }
                }else if (File.Exists(path)){
                    try { total = new FileInfo(path).Length; } catch { }
                }
            }catch { }
            return total;
        }
        // SELECT LABEL WRITE PATH
        // ======================================================================================================
        private void CCTTable_CellClick(object sender, DataGridViewCellEventArgs e){
            try{
                if (e.RowIndex >= 0){
                    TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                    DataGridViewRow get_selected_path = CCTTable.Rows[e.RowIndex];
                    string pathText = string.Format(software_lang.TSReadLangs("CacheCleanupTool", "cct_selected_path"), get_selected_path.Cells[1].Value.ToString());
                    if (CCT_SelectLabel.InvokeRequired){
                        CCT_SelectLabel.Invoke(new Action(() => CCT_SelectLabel.Text = pathText));
                    }else{
                        CCT_SelectLabel.Text = pathText;
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CCTTable_CellClick()"); }
            }
        }
        // START CLEAN BTN
        // ======================================================================================================
        private async void CCT_StartBtn_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (CCTTable.SelectedCells.Count > 0){
                    var selectedPaths = new List<string>();
                    foreach (DataGridViewCell cell in CCTTable.SelectedCells){
                        try{
                            string p = CCTTable.Rows[cell.RowIndex].Cells[1].Value.ToString().Trim();
                            if (!string.IsNullOrWhiteSpace(p) && !selectedPaths.Contains(p)) selectedPaths.Add(p);
                        }catch { }
                    }
                    if (selectedPaths.Count == 0){
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("CacheCleanupTool", "cct_check_select_clean_patch_info"));
                        return;
                    }
                    DialogResult cct_check_delete_notifi = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("CacheCleanupTool", "cct_check_delete_notification"), string.Join("\n", selectedPaths.ToArray()), "\n\n"));
                    if (cct_check_delete_notifi == DialogResult.Yes){
                        await Cleanup_engine(selectedPaths);
                    }
                }else{
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("CacheCleanupTool", "cct_check_select_clean_patch_info"));
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "CCT_StartBtn_Click()"); }
            }
        }
        // CLEANUP ENGINE
        // ======================================================================================================
        private async Task Cleanup_engine(List<string> target_paths){
            int failedFiles = 0;
            string firstPath = target_paths.Count > 0 ? target_paths[0] : string.Empty;
            try{
                string explorerDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer");
                string iconCacheDb = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IconCache.db");
                bool needExplorerRestart = false;
                foreach (var tp in target_paths){
                    if (string.Equals(tp, explorerDir, StringComparison.OrdinalIgnoreCase)){ needExplorerRestart = true; break; }
                }
                if (needExplorerRestart){
                    await RestartExplorerForCacheCleanupAsync();
                }
                foreach (var target_path in target_paths){
                    try{
                        if (string.Equals(target_path, explorerDir, StringComparison.OrdinalIgnoreCase)){
                            if (Directory.Exists(explorerDir)){
                                foreach (var f in Directory.EnumerateFiles(explorerDir, "iconcache*", SearchOption.TopDirectoryOnly)){
                                    try { File.Delete(f); } catch { failedFiles++; }
                                }
                                foreach (var f in Directory.EnumerateFiles(explorerDir, "thumbcache*", SearchOption.TopDirectoryOnly)){
                                    try { File.Delete(f); } catch { failedFiles++; }
                                }
                            }
                            if (File.Exists(iconCacheDb)){
                                try { File.Delete(iconCacheDb); } catch { failedFiles++; }
                            }
                        }else if (File.Exists(target_path)){
                            try { File.Delete(target_path); } catch { failedFiles++; }
                        }else if (Directory.Exists(target_path)){
                            DeleteDirContentsSafe(new DirectoryInfo(target_path), ref failedFiles);
                        }
                    }catch (Exception exInner){
                        failedFiles++;
                        if (GlowMain.debug_status) { TSErrorLog.LogException(exInner, "Cleanup_engine() - " + target_path); }
                    }
                }
                CCleanup_aRefreshRepeat = true;
                await CheckFolderSizesAsync();
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (!IsDisposed && IsHandleCreated){
                    int failed = failedFiles;
                    BeginInvoke(new Action(() => {
                        CCTTable.ClearSelection();
                        // Same text, warning icon when some locked files were skipped (no new lang keys).
                        TS_MessageBoxEngine.TS_MessageBox(this, failed == 0 ? 1 : 2, string.Format(software_lang.TSReadLangs("CacheCleanupTool", "cct_delete_success_notification"), firstPath));
                    }));
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Cleanup_engine()"); }
            }
        }
        private static void DeleteDirContentsSafe(DirectoryInfo di, ref int failedFiles){
            FileInfo[] files;
            try { files = di.GetFiles(); } catch { failedFiles++; return; }
            if (files != null){
                foreach (var f in files) { try { f.Delete(); } catch { failedFiles++; } }
            }
            DirectoryInfo[] dirs;
            try { dirs = di.GetDirectories(); } catch { failedFiles++; return; }
            if (dirs != null){
                foreach (var d in dirs){
                    try{
                        if ((d.Attributes & FileAttributes.ReparsePoint) != 0){
                            d.Delete();
                        }else{
                            d.Delete(true);
                        }
                    }catch { failedFiles++; }
                }
            }
        }
        // SECURE EXPLORER RESET
        // ======================================================================================================
        private async Task RestartExplorerForCacheCleanupAsync(){
            string winExplorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            try{
                await Task.Run(() => {
                    try{
                        foreach (var p in Process.GetProcessesByName("explorer")){
                            try{
                                string exePath = p.MainModule?.FileName;
                                if (!string.IsNullOrWhiteSpace(exePath) && string.Equals(exePath, winExplorerPath, StringComparison.OrdinalIgnoreCase)){
                                    try { p.Kill(); } catch { }
                                }
                            }catch{ }
                        }
                    }catch{ }
                });
                await Task.Delay(2000);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "RestartExplorerForCacheCleanupAsync()"); }
            }
            _ = Task.Run(async () => {
                try{
                    await Task.Delay(2500);
                    // App runs elevated: start explorer unelevated so the shell keeps the user token.
                    bool started = false;
                    try{
                        using (var p = Process.Start(new ProcessStartInfo("runas", "/trustlevel:0x20000 \"" + winExplorerPath + "\"") { UseShellExecute = false, CreateNoWindow = true })){
                            started = p != null;
                        }
                    }catch { started = false; }
                    if (!started){
                        try { Process.Start(new ProcessStartInfo(winExplorerPath) { UseShellExecute = true }); } catch { }
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "RestartExplorerForCacheCleanupAsync() - Timer Reset"); }
                }
            });
        }
        // AUTO REFRESH FOLDER SIZE FUNCTION
        // ======================================================================================================
        private async Task AutoFolderSizeRefreshAsync(){
            try{
                int RefreshIntervalInSeconds = 15;
                var softwareLang = new TSGetLangs(GlowMain.lang_path);
                var refreshTitleFormat = softwareLang.TSReadLangs("CacheCleanupTool", "cct_refresh_title").Trim();
                var refreshingTitle = softwareLang.TSReadLangs("CacheCleanupTool", "cct_refreshing_title").Trim();
                while (CCleanup_aRefresh){
                    for (int i = RefreshIntervalInSeconds; i >= 0; i--){
                        if (CCleanup_aRefreshRepeat){
                            CCleanup_aRefreshRepeat = false;
                            break;
                        }
                        if (!IsDisposed && IsHandleCreated){
                            BeginInvoke(new Action(() => Text = i != 0 ? $"{CCleanup_cctTitle} | {string.Format(refreshTitleFormat, i)}" : $"{CCleanup_cctTitle} | {refreshingTitle}"));
                        }
                        if (i == 0)
                            await CheckFolderSizesAsync();
                        await Task.Delay(1000);
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "AutoFolderSizeRefreshAsync()"); }
            }
        }
        private async Task CheckFolderSizesAsync(){
            if (!await _sizeCheckLock.WaitAsync(0)) return;
            try{
                await Task.Run(() => Check_folder_sizes());
            }
            finally{
                _sizeCheckLock.Release();
            }
        }
        // BEFORE CCT TOOL EXIT AUTO REFRESH STOP
        // ======================================================================================================
        private void GlowCacheCleanupTool_FormClosing(object sender, FormClosingEventArgs e) { CCleanup_aRefresh = false; }
    }
}