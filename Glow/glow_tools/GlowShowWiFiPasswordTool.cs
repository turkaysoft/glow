using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowShowWiFiPasswordTool : Form{
        public GlowShowWiFiPasswordTool(){
            InitializeComponent();
            SWP_DGV.Columns.Add("col_ssid", "Network Name");
            SWP_DGV.Columns.Add("col_pass", "Network Password");
            SWP_DGV.Columns[0].Width = (int)(260 * this.DeviceDpi / 96f);
            SWP_DGV.RowTemplate.Height = (int)(28 * this.DeviceDpi / 96f);
            foreach (DataGridViewColumn columnPadding in SWP_DGV.Columns){
                int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
            }
        }
        // DYNAMIC THEME VOID
        // ======================================================================================================
        public void Swpt_theme_settings(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "ContentPanelBGColor");
                Panel_BG.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "ContentPanelBGColor");
                //
                SWP_DGV.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "PageContainerBGAndPageContentTotalColors");
                SWP_DGV.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridColor");
                SWP_DGV.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridBGColor");
                SWP_DGV.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridFEColor");
                SWP_DGV.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DataGridAlternatingColor");
                SWP_DGV.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                SWP_DGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                SWP_DGV.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageFE");
                SWP_DGV.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageBG");
                SWP_DGV.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "OSDAndServicesPageFE");
                //
                SWPT_ExportBtn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                SWPT_ExportBtn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "DynamicThemeActiveBtnBG");
                SWPT_ExportBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                SWPT_ExportBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColor");
                SWPT_ExportBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                //
                TSImageRenderer(SWPT_ExportBtn, GlowMain.theme == 1 ? Properties.Resources.ct_export_light : Properties.Resources.ct_export_dark, 18, ContentAlignment.MiddleRight);
                //
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_title"), Application.ProductName);
                //
                if (SWP_DGV.Columns.Count > 0){
                    SWP_DGV.Columns[0].HeaderText = software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_name");
                    SWP_DGV.Columns[1].HeaderText = software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_password");
                }
                //
                SWPT_ExportBtn.Text = " " + software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_export_btn");
            }catch (Exception){ }
        }
        // LOAD SWPT
        // ======================================================================================================
        private async void GlowShowWiFiPasswordTool_Load(object sender, EventArgs e){
            Swpt_theme_settings();
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_title"), Application.ProductName) + " - " + software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_load");
                await Task.Run(() => {
                    string[] wifiProfiles = Ts_ExtractWiFiProfileNames(Ts_GetWiFiPassword("netsh wlan show profile"));
                    if (wifiProfiles.Length == 0){
                        Invoke(new Action(() => {
                            TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_no_profile"), "\n", software_lang.TSReadLangs("HeaderTools", "ht_show_wifi_password_tool")));
                            Close();
                        }));
                        return;
                    }
                    Invoke(new Action(() => {
                        Text = string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_title"), Application.ProductName);
                        SWPT_ExportBtn.Enabled = true;
                        foreach (string profile in wifiProfiles){
                            string password = GetWiFiPassword(profile.Trim());
                            SWP_DGV.Rows.Add(profile.Trim(), password);
                        }
                        SWP_DGV.ClearSelection();
                    }));
                });
            }catch (Exception){ }
        }
        // GET WI-FI PASSWORD
        // ======================================================================================================
        static string GetWiFiPassword(string networkName){
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            try{
                string wifiDetails = Ts_GetWiFiPassword($"netsh wlan show profile \"{networkName}\" key=clear");
                string passwordKey = "Key Content            : ";
                int startIndex = wifiDetails.IndexOf(passwordKey);
                if (startIndex == -1)
                    return software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_no_password");
                startIndex += passwordKey.Length;
                int endIndex = wifiDetails.IndexOf("\n", startIndex);
                return wifiDetails.Substring(startIndex, endIndex - startIndex).Trim();
            }catch{
                return software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_no_read");
            }
        }
        // EXECUTE CMD INTERFACE CODE
        // ======================================================================================================
        static string Ts_GetWiFiPassword(string get_command){
            ProcessStartInfo wifi_psi = new ProcessStartInfo{
                FileName = "cmd.exe",
                Arguments = $"/c {get_command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using (Process process_result = Process.Start(wifi_psi)){
                string wifi_output_result = process_result.StandardOutput.ReadToEnd();
                process_result.WaitForExit();
                return wifi_output_result;
            }
        }
        // GET ONLY THE REQUIRED SITE WITH CMD
        // ======================================================================================================
        static string[] Ts_ExtractWiFiProfileNames(string get_wifi_profiles){
            string userProfilesSection = "User profiles";
            int startIndex = get_wifi_profiles.IndexOf(userProfilesSection);
            if (startIndex == -1){
                return new string[0];
            }
            string profileSection = get_wifi_profiles.Substring(startIndex);
            MatchCollection matches = Regex.Matches(profileSection, @"All User Profile\s*:\s*(.*)");
            string[] profileNames = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++){
                profileNames[i] = matches[i].Groups[1].Value.Trim();
            }
            return profileNames;
        }
        // COPY SELECTED WI-FI PASSWORD
        // ======================================================================================================
        private void SWP_DGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (e.RowIndex < 0)
                    return;
                DataGridViewRow row = SWP_DGV.Rows[e.RowIndex];
                if (row == null || row.Cells.Count < 2)
                    return;
                string profileName = row.Cells[0].Value?.ToString().Trim();
                string password = row.Cells[1].Value?.ToString().Trim();
                if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(password))
                    return;
                if (password == software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_no_password").Trim() || password == software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_no_read").Trim())
                    return;
                Clipboard.SetText(password);
                TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_copy_txt"), profileName));
                SWP_DGV.ClearSelection();
            }catch (Exception){ }
        }
        // EXPORT WI-FI'S PASSWORD
        // ======================================================================================================
        readonly List<string> PrintWiFiList = new List<string>();
        private void SWPT_ExportBtn_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            try{
                string profile_name = software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_name") + ":";
                string profile_password = software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_password") + ":";
                //
                PrintWiFiList.Clear();
                PrintWiFiList.Add(string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_profile_title"), Application.ProductName));
                PrintWiFiList.Add(Environment.NewLine + new string('-', 100) + Environment.NewLine);
                //
                for (int i = 0; i <= SWP_DGV.Rows.Count - 1; i++){
                    int lineLength = (i == SWP_DGV.Rows.Count - 1) ? 100 : 25;
                    PrintWiFiList.Add(profile_name + " " + SWP_DGV.Rows[i].Cells[0].Value.ToString() + "\n" + profile_password + " " + SWP_DGV.Rows[i].Cells[1].Value.ToString() + Environment.NewLine + Environment.NewLine + new string('-', lineLength) + Environment.NewLine);
                }
                //
                using (SaveFileDialog saveDlg = new SaveFileDialog{
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Application.ProductName + " - " + software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_save_directory"),
                    DefaultExt = "txt",
                    FileName = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_save_name"), software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_export_name")),
                    Filter = software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_save_txt") + " (*.txt)|*.txt"
                }){
                    if (saveDlg.ShowDialog() == DialogResult.OK){
                        string content = string.Join(Environment.NewLine, PrintWiFiList);
                        content = content.TrimEnd(Environment.NewLine.ToCharArray());
                        File.WriteAllText(saveDlg.FileName, content);
                        var res = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_save_success") + Environment.NewLine + Environment.NewLine + software_lang.TSReadLangs("ShowWiFiPasswordTool", "swpt_save_info_open"), Application.ProductName, saveDlg.FileName));
                        if (res == DialogResult.Yes)
                            Process.Start(saveDlg.FileName);
                    }
                }
            }catch (Exception){ }
        }
    }
}