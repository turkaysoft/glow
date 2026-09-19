using System;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowNetworkFixTool : Form{
        public GlowNetworkFixTool(){ InitializeComponent(); }
        private bool nftRunning = false;
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_NetworkFix_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                Panel_BG.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                NFT_TitleLabel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                NFT_TitleLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                //
                NFT_ResultList.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                NFT_ResultList.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                NFT_ResultList.SelectedBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                NFT_ResultList.SelectedForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                //
                NFT_StartBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                NFT_StartBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                NFT_StartBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                NFT_StartBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                NFT_StartBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(NFT_StartBtn, GlowMain.theme == 1 ? Properties.Resources.ct_fix_light : Properties.Resources.ct_fix_dark, 18, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_title"), Application.ProductName);
                //
                NFT_StartBtn.Text = " " + software_lang.TSReadLangs("NetworkFixTool", "nft_process_start_btn");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_NetworkFix_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowNetworkFixTool_Load(object sender, EventArgs e){
            try{
                GTool_NetworkFix_Preloader();
                //
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                NFT_TitleLabel.Text = software_lang.TSReadLangs("NetworkFixTool", "nft_title_label_before_start");
                this.FormClosing += new FormClosingEventHandler(GlowNetworkFixTool_FormClosing);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowNetworkFixTool_Load()"); }
            }
        }
        private void GlowNetworkFixTool_FormClosing(object sender, FormClosingEventArgs e){
            if (nftRunning){
                e.Cancel = true;
            }
        }
        // RESULT LIST CLEAR SELECTION
        // ======================================================================================================
        private void NFT_ResultList_SelectedIndexChanged(object sender, EventArgs e){
            NFT_ResultList.SelectedIndex = -1;
            NFT_ResultList.ClearSelected();
        }
        // NETWORK FIX ENGINE STARTER BTN
        // ======================================================================================================
        private async void NFT_StartBtn_Click(object sender, EventArgs e){
            try{
                if (nftRunning) return;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                DialogResult start_engine_query = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_start_query"), "\n"));
                if (start_engine_query == DialogResult.Yes){
                    NFT_TitleLabel.Text = software_lang.TSReadLangs("NetworkFixTool", "nft_title_label_in_process");
                    await Start_network_fix_engine_async();
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "NFT_StartBtn_Click()"); }
            }
        }
        // NETWORK FIX ENGINE STARTER
        // ======================================================================================================
        private async Task Start_network_fix_engine_async(){
            nftRunning = true;
            try{
                NFT_ResultList.Items.Clear();
                NFT_StartBtn.Enabled = false;
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                NFT_TitleLabel.Text = software_lang.TSReadLangs("NetworkFixTool", "nft_title_label_in_process");
                await Ts_RunNetworkFixCommandAsync("netsh", "winsock reset");
                await Ts_RunNetworkFixCommandAsync("netsh", "int ip reset");
                await Ts_RunNetworkFixCommandAsync("ipconfig", "/release");
                await Ts_RunNetworkFixCommandAsync("ipconfig", "/renew");
                await Ts_RunNetworkFixCommandAsync("ipconfig", "/flushdns");
                if (this.IsDisposed || !this.IsHandleCreated) return;
                NFT_TitleLabel.Text = software_lang.TSReadLangs("NetworkFixTool", "nft_title_label_after_end");
                DialogResult end_engine_query = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_after_query"), "\n\n", "\n\n", "\n\n"));
                if (end_engine_query == DialogResult.Yes){
                    try{
                        ProcessStartInfo pc_restart_query = new ProcessStartInfo{
                            FileName = "shutdown",
                            Arguments = "/r /t 0",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        using (Process pc_restart_starter = Process.Start(pc_restart_query)){
                            if (pc_restart_starter != null){
                                await Task.Run(() => pc_restart_starter.WaitForExit());
                            }
                        }
                    }catch (Exception){
                        if (this.IsDisposed || !this.IsHandleCreated) return;
                        TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_after_restart_info"), "\n"));
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Start_network_fix_engine_async()"); }
            }finally{
                nftRunning = false;
                try{
                    if (!this.IsDisposed && this.IsHandleCreated){
                        NFT_StartBtn.Enabled = true;
                    }
                }catch { }
            }
        }
        // NETWORK FIX ENGINE
        // ======================================================================================================
        private async Task Ts_RunNetworkFixCommandAsync(string get_command, string get_arguments){
            string currentLangPath = GlowMain.lang_path;
            Encoding oemEncoding;
            try{ oemEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage); }
            catch{ oemEncoding = Encoding.Default; }
            int exitCode = -1;
            string stdoutText = string.Empty;
            string stderrText = string.Empty;
            bool startFailed = false;
            string startError = string.Empty;
            try{
                await Task.Run(() => {
                    ProcessStartInfo start_network_fix_process = new ProcessStartInfo{
                        FileName = get_command,
                        Arguments = get_arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = oemEncoding,
                        StandardErrorEncoding = oemEncoding,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (Process network_fix_runner = Process.Start(start_network_fix_process)){
                        if (network_fix_runner == null){
                            startFailed = true;
                            return;
                        }
                        Task<string> stderrTask = Task.Factory.StartNew(() => {
                            try{ return network_fix_runner.StandardError.ReadToEnd(); }
                            catch{ return string.Empty; }
                        });
                        try{ stdoutText = network_fix_runner.StandardOutput.ReadToEnd() ?? string.Empty; }
                        catch{ stdoutText = string.Empty; }
                        try{ network_fix_runner.WaitForExit(); }catch { }
                        try{
                            stderrText = stderrTask.Wait(15000) ? (stderrTask.Result ?? string.Empty) : string.Empty;
                        }catch{ stderrText = string.Empty; }
                        try{ exitCode = network_fix_runner.HasExited ? network_fix_runner.ExitCode : -1; }
                        catch{ exitCode = -1; }
                    }
                });
            }catch (Exception ex){
                startFailed = true;
                startError = ex.Message.Trim();
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Ts_RunNetworkFixCommandAsync()"); }
            }
            try{
                if (this.IsDisposed || !this.IsHandleCreated) return;
                TSGetLangs software_lang = new TSGetLangs(currentLangPath);
                string displayErr;
                if (startFailed){
                    displayErr = !string.IsNullOrWhiteSpace(startError) ? ToSingleLine(startError) : ("ExitCode: " + exitCode);
                    this.BeginInvoke(new Action(() => {
                        if (this.IsDisposed || !this.IsHandleCreated) return;
                        NFT_ResultList.Items.Add(string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_code_transfer_error"), get_command, get_arguments, displayErr));
                    }));
                    return;
                }
                if (exitCode == 0){
                    this.BeginInvoke(new Action(() => {
                        if (this.IsDisposed || !this.IsHandleCreated) return;
                        NFT_ResultList.Items.Add(string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_code_transfer"), get_command, get_arguments));
                    }));
                }else{
                    string raw = !string.IsNullOrWhiteSpace(stderrText) ? stderrText : (!string.IsNullOrWhiteSpace(stdoutText) ? stdoutText : ("ExitCode: " + exitCode));
                    displayErr = ToSingleLine(raw);
                    this.BeginInvoke(new Action(() => {
                        if (this.IsDisposed || !this.IsHandleCreated) return;
                        NFT_ResultList.Items.Add(string.Format(software_lang.TSReadLangs("NetworkFixTool", "nft_process_code_transfer_error"), get_command, get_arguments, displayErr));
                    }));
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Ts_RunNetworkFixCommandAsync() UI"); }
            }
        }
        private static string ToSingleLine(string text){
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string first = lines.Length > 0 ? lines[0].Trim() : text.Trim();
            const int maxLen = 300;
            if (first.Length <= maxLen) return first;
            return first.Substring(0, maxLen) + "...";
        }
    }
}