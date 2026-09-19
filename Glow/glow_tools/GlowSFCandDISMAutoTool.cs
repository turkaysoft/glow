using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowSFCandDISMAutoTool : Form{
        // FIELDS
        // ======================================================================================================
        private Stopwatch sadtStopwatch;
        private Timer sadtUiTimer;
        private bool processStatus = true;
        private string titleMessage;
        private string processStatusMessage;
        // Compiled regexes. DISM/SFC always return English output.
        private static readonly Regex sadtProgressRegex = new Regex(@"(\d{1,3}(?:\.\d{1,2})?)%", RegexOptions.Compiled);
        private static readonly Regex sadtRepairableRegex = new Regex(@"repairable", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex sadtSfcRepairedRegex = new Regex(@"successfully repaired", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex sadtSfcFailedRegex = new Regex(@"unable to fix|unable to repair|but was unable|could not perform|could not repair", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex sadtRestoreFailedRegex = new Regex(@"restore.*failed|restore operation failed|error:\s*0x", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private sealed class SadtCommand{
            public readonly string FileName;
            public readonly string Arguments;
            public readonly string Display;
            public SadtCommand(string fileName, string args, string display){ FileName = fileName; Arguments = args; Display = display; }
        }
        public GlowSFCandDISMAutoTool() { InitializeComponent(); }
        // DYNAMIC THEME VOID
        // ======================================================================================================
        public void GTool_SADT_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                Back_Panel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                SADT_L1.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                SADT_L2.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                SADT_L3.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                SADT_L4.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                SADT_StartBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                SADT_StartBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                SADT_StartBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                SADT_StartBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                SADT_StartBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(SADT_StartBtn, GlowMain.theme == 1 ? Properties.Resources.ct_fix_light : Properties.Resources.ct_fix_dark, 18, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                TSSettingsModule software_read_settings = new TSSettingsModule(ts_sf);
                //
                titleMessage = string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_title"), Application.ProductName);
                Text = titleMessage;
                //
                SADT_L1.Text = software_lang.TSReadLangs("DISMandSFCTool", "sadt_sub_title");
                if (!GlowMain.SFCandDISMprocessStatus){
                    SADT_L2.Text = software_lang.TSReadLangs("DISMandSFCTool", "sadt_description");
                }
                SADT_L3.Text = software_lang.TSReadLangs("DISMandSFCTool", "sadt_last_repair_time");
                //
                string lastFixDate = software_read_settings.TSReadSettings(ts_settings_container, "SADTime");
                SADT_L4.Text = !string.IsNullOrWhiteSpace(lastFixDate) ? lastFixDate : software_lang.TSReadLangs("DISMandSFCTool", "sadt_not_start");
                SADT_StartBtn.Text = " " + software_lang.TSReadLangs("DISMandSFCTool", "sadt_start_engine");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_SADT_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowSFCandDISMAutoTool_Load(object sender, EventArgs e){
            try{
                GTool_SADT_Preloader();
                sadtStopwatch = new Stopwatch();
                sadtUiTimer = new Timer{
                    Interval = 500
                };
                sadtUiTimer.Tick += (s, ev) => {
                    if (!sadtStopwatch.IsRunning) return;
                    var eTime = sadtStopwatch.Elapsed;
                    this.Text = titleMessage + $" - {eTime:hh\\:mm\\:ss}";
                    int msToNextSecond = 1000 - eTime.Milliseconds;
                    sadtUiTimer.Interval = Math.Max(50, msToNextSecond);
                };
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowSFCandDISMAutoTool_Load()"); }
            }
        }
        // SFC AND DISM AUTO TOOL START ENGINE BTN
        // ======================================================================================================
        private void SADT_StartBtn_Click(object sender, EventArgs e){
            try{
                if (GlowMain.SFCandDISMprocessStatus){
                    return;
                }
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                DialogResult sadt_start_check = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_engine_start_notification"), "\n\n"));
                if (sadt_start_check == DialogResult.Yes){
                    SADT_StartBtn.Enabled = false;
                    GlowMain.SFCandDISMprocessStatus = true;
                    Task sadt_engine_bg = Task.Run(SadtEngine);
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "SADT_StartBtn_Click()"); }
            }
        }
        // SFC AND DISM AUTO TOOL ENGINE
        // ======================================================================================================
        private void SadtEngine(){
            processStatus = true;
            processStatusMessage = string.Empty;
            //
            var repairedCommands = new List<string>();
            bool isComponentStoreCorrupt = false;
            //
            try{
                sadtStopwatch?.Restart();
            }catch { }
            try{
                if (sadtUiTimer != null) BeginInvoke(new Action(() => { try { sadtUiTimer.Start(); } catch { } }));
            }catch { }
            //
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            string process_message = software_lang.TSReadLangs("DISMandSFCTool", "sadt_current_running_pro");
            //
            try{
                var engineCommands = new[] {
                    new SadtCommand("DISM.exe", "/Online /Cleanup-Image /CheckHealth", "DISM /Online /Cleanup-Image /CheckHealth"),
                    new SadtCommand("DISM.exe", "/Online /Cleanup-Image /ScanHealth", "DISM /Online /Cleanup-Image /ScanHealth"),
                    new SadtCommand("DISM.exe", "/Online /Cleanup-Image /RestoreHealth", "DISM /Online /Cleanup-Image /RestoreHealth"),
                    new SadtCommand("sfc.exe", "/scannow", "sfc /scannow")
                };
                //
                GlowMain.SFCandDISMprocessStatus = true;
                UpdateSafeEnabled(SADT_StartBtn, false);
                //
                foreach (var cmd in engineCommands){
                    UpdateSafeText(SADT_L2, string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_current_running"), cmd.Display, "\n\n"));
                    //
                    Encoding encoding = cmd.Display.StartsWith("sfc", StringComparison.OrdinalIgnoreCase) ? Encoding.Unicode : Encoding.Default;
                    int exitCode = RunSadtProcess(cmd.FileName, cmd.Arguments, encoding, cmd.Display, process_message, out string combinedOutput);
                    //
                    if (exitCode != 0){
                        processStatus = false;
                        processStatusMessage = "[Exit Code: " + exitCode + "]\n\n" + TruncateSadtOutput(combinedOutput);
                        break;
                    }
                    bool isDism = cmd.Display.StartsWith("DISM", StringComparison.OrdinalIgnoreCase);
                    bool isSfc = cmd.Display.StartsWith("sfc", StringComparison.OrdinalIgnoreCase);
                    // DISM
                    if (isDism){
                        if (cmd.Arguments.IndexOf("/CheckHealth", StringComparison.OrdinalIgnoreCase) >= 0
                            || cmd.Arguments.IndexOf("/ScanHealth", StringComparison.OrdinalIgnoreCase) >= 0){
                            if (sadtRepairableRegex.IsMatch(combinedOutput)){
                                isComponentStoreCorrupt = true;
                            }
                        }else if (cmd.Arguments.IndexOf("/RestoreHealth", StringComparison.OrdinalIgnoreCase) >= 0){
                            if (sadtRestoreFailedRegex.IsMatch(combinedOutput)){
                                processStatus = false;
                                processStatusMessage = TruncateSadtOutput(combinedOutput);
                                break;
                            }
                            if (isComponentStoreCorrupt){
                                repairedCommands.Add(cmd.Display);
                            }
                        }
                    } // SFC
                    else if (isSfc){
                        if (sadtSfcFailedRegex.IsMatch(combinedOutput)){
                            processStatus = false;
                            processStatusMessage = TruncateSadtOutput(combinedOutput);
                            break;
                        }
                        if (sadtSfcRepairedRegex.IsMatch(combinedOutput)){
                            repairedCommands.Add(cmd.Display);
                        }
                    }
                    if (!processStatus) break;
                }
                //
                if (processStatus){
                    string current_time = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
                    UpdateSafeText(SADT_L4, current_time);
                    try { new TSSettingsModule(ts_sf).TSWriteSettings(ts_settings_container, "SADTime", current_time); }
                    catch (Exception settingEx) { if (GlowMain.debug_status) { TSErrorLog.LogException(settingEx, "SadtEngine() Settings Save"); } }
                }
            }catch (Exception ex){
                processStatus = false;
                processStatusMessage = TruncateSadtOutput(ex.Message.Trim());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "SadtEngine()"); }
            }finally{
                try{ sadtStopwatch?.Stop(); }catch { }
                try{ if (sadtUiTimer != null) BeginInvoke(new Action(() => { try { sadtUiTimer.Stop(); } catch { } })); }catch { }
                GlowMain.SFCandDISMprocessStatus = false;
                //
                UpdateSafeEnabled(SADT_StartBtn, true);
                UpdateSafeText(this, titleMessage);
                UpdateSafeText(SADT_L2, software_lang.TSReadLangs("DISMandSFCTool", "sadt_description"));
                //
                TimeSpan totalTime = TimeSpan.Zero;
                try{ if (sadtStopwatch != null) totalTime = sadtStopwatch.Elapsed; }catch { }
                string timeStr = string.Format("{0:hh\\:mm\\:ss}", totalTime);
                //
                if (processStatus){
                    string finalMsg = string.Empty;
                    if (repairedCommands.Count > 0){
                        finalMsg = string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_process_success"), "\n\n", timeStr);
                        finalMsg += "\n\n" + software_lang.TSReadLangs("DISMandSFCTool", "sadt_repair_codes") + "\n\n";
                        foreach (var cmd in repairedCommands){
                            finalMsg += "- " + cmd + "\n";
                        }
                    }else{
                        finalMsg = software_lang.TSReadLangs("DISMandSFCTool", "sadt_no_repair_needed");
                        finalMsg += "\n\n" + string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_total_time"), timeStr);
                    }
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, finalMsg);
                }else{
                    TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("DISMandSFCTool", "sadt_process_failed"), "\n\n", processStatusMessage, "\n\n", timeStr));
                }
            }
        }
        private static string TruncateSadtOutput(string output){
            if (string.IsNullOrEmpty(output)) return string.Empty;
            const int maxLen = 4000;
            string trimmed = output.Trim();
            if (trimmed.Length <= maxLen) return trimmed;
            return "[...]\n" + trimmed.Substring(trimmed.Length - maxLen);
        }
        private int RunSadtProcess(string fileName, string arguments, Encoding encoding, string displayCmd, string progressFormat, out string combinedOutput){
            combinedOutput = string.Empty;
            using (var processRepair = new Process()){
                processRepair.StartInfo = new ProcessStartInfo{
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = encoding,
                    StandardErrorEncoding = encoding,
                    CreateNoWindow = true
                };
                processRepair.Start();
                var fullStdOut = new StringBuilder(8192);
                Task<string> stderrTask = Task.Factory.StartNew(() => {
                    try{ return processRepair.StandardError.ReadToEnd(); }
                    catch{ return string.Empty; }
                });
                try{
                    char[] chunk = new char[2048];
                    var window = new StringBuilder(640);
                    int lastPercent = -1;
                    long lastUiTicks = 0;
                    int readCount;
                    StreamReader reader = processRepair.StandardOutput;
                    while ((readCount = reader.Read(chunk, 0, chunk.Length)) > 0){
                        fullStdOut.Append(chunk, 0, readCount);
                        window.Append(chunk, 0, readCount);
                        if (window.Length > 600) window.Remove(0, window.Length - 600);
                        long now = DateTime.UtcNow.Ticks;
                        if (now - lastUiTicks < 500L * 10000L) continue;
                        string windowStr = window.ToString();
                        MatchCollection matches = sadtProgressRegex.Matches(windowStr);
                        if (matches.Count == 0) continue;
                        Match last = matches[matches.Count - 1];
                        string num = last.Groups[1].Value;
                        int dot = num.IndexOf('.');
                        string intPart = dot >= 0 ? num.Substring(0, dot) : num;
                        if (!int.TryParse(intPart, out int pct)) continue;
                        if (pct == lastPercent) continue;
                        lastPercent = pct;
                        lastUiTicks = now;
                        try{ UpdateSafeText(SADT_L2, string.Format(progressFormat, displayCmd, "\n\n", "\n", last.Groups[1].Value + "%")); }
                        catch { }
                    }
                }catch (Exception readEx){
                    try{ if (!processRepair.HasExited){ processRepair.Kill(); processRepair.WaitForExit(5000); } }catch { }
                    throw new Exception("Process read failed: " + displayCmd, readEx);
                }
                try{ processRepair.WaitForExit(); }catch { }
                string stderr = string.Empty;
                try{
                    if (stderrTask.Wait(15000)){
                        stderr = stderrTask.Result ?? string.Empty;
                    }
                }catch { stderr = string.Empty; }
                combinedOutput = (fullStdOut.ToString() + "\n" + stderr).Trim();
                try{ return processRepair.HasExited ? processRepair.ExitCode : -1; }
                catch{ return -1; }
            }
        }
        // SAFE TEXT UI THREAD
        // ======================================================================================================
        private void UpdateSafeText(Form renderForm, string renderMessage){
            if (renderForm.InvokeRequired){
                renderForm.BeginInvoke(new Action(() => renderForm.Text = renderMessage));
            }else{
                renderForm.Text = renderMessage;
            }
        }
        private void UpdateSafeText(Label renderLabel, string renderMessage){
            if (renderLabel.InvokeRequired){
                renderLabel.BeginInvoke(new Action(() => renderLabel.Text = renderMessage));
            }else{
                renderLabel.Text = renderMessage;
            }
        }
        private void UpdateSafeEnabled(Button renderBtn, bool renderMode){
            if (renderBtn.InvokeRequired){
                renderBtn.BeginInvoke(new Action(() => renderBtn.Enabled = renderMode));
            }else{
                renderBtn.Enabled = renderMode;
            }
        }
        // CLOSE CHECK STATUS
        // ======================================================================================================
        private void GlowSFCandDISMAutoTool_FormClosing(object sender, FormClosingEventArgs e){
            if (GlowMain.SFCandDISMprocessStatus){
                e.Cancel = true;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_dism_and_sfc_prs_msg"));
            }
        }
    }
}