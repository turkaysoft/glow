using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using Microsoft.Win32;
using System.Management;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// TS Modules
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowHSAuditorTool : Form{
        // HSA STATUS HELPER
        // ======================================================================================================
        public static class HSA_StatusHelper{
            // Status
            public const int STATUS_UNKNOWN = 0;
            public const int STATUS_ENABLED = 1;
            public const int STATUS_DISABLED = 2;
            // Colors
            private static Color _colorPending;
            private static Color _colorSuccess;
            private static Color _colorRed;
            private static Color _colorDefault;
            private static int _currentTheme = 1;
            // Texts
            private static string _textEnabled = "N/A";
            private static string _textDisabled = "N/A";
            private static string _textUnknown = "N/A";
            private static string _textChecking = "N/A";
            private static string _textError = "N/A";
            // Update Color Dynamic
            public static void UpdateColors(int theme){
                _currentTheme = theme;
                // _colorPending = TS_ThemeEngine.ColorMode(theme, "AccentPurple");
                _colorPending = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                _colorSuccess = TS_ThemeEngine.ColorMode(theme, "AccentGreen");
                _colorRed = TS_ThemeEngine.ColorMode(theme, "AccentRed");
                _colorDefault = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
            }
            // Update Text Dynamic
            public static void UpdateTexts(string enabledText, string disabledText, string unknownText, string checkingText, string errorText){
                _textEnabled = enabledText;
                _textDisabled = disabledText;
                _textUnknown = unknownText;
                _textChecking = checkingText;
                _textError = errorText;
            }
            // Status Color Change
            public static Color GetStatusColor(int status){
                if (status == STATUS_ENABLED)
                    return _colorSuccess;
                else if (status == STATUS_DISABLED)
                    return _colorRed;
                else if (status == STATUS_UNKNOWN)
                    return _colorPending;
                else
                    return _colorDefault;
            }
            // Status Image Change
            public static Image GetStatusIcon(int status){
                bool isLightTheme = (_currentTheme == 1);
                if (status == STATUS_ENABLED)
                    return isLightTheme ? Properties.Resources.ct_success_light : Properties.Resources.ct_success_dark;
                else if (status == STATUS_DISABLED)
                    return isLightTheme ? Properties.Resources.ct_fail_light : Properties.Resources.ct_fail_dark;
                else
                    return null;
            }
            // Status Text Change
            public static string GetStatusText(int status, string customText = null){
                if (!string.IsNullOrEmpty(customText))
                    return customText;
                if (status == STATUS_ENABLED)
                    return _textEnabled;
                else if (status == STATUS_DISABLED)
                    return _textDisabled;
                else if (status == STATUS_UNKNOWN)
                    return _textUnknown;
                else
                    return _textError;
            }
            // Dynamic Change Status
            public static string GetCheckingText(){
                return _textChecking;
            }
            public static string GetErrorText(){
                return _textError;
            }
            public static void UpdateStatusPanel(TSCustomPanel statusPanel, int status){
                if (statusPanel == null) return;
                statusPanel.BackColor = GetStatusColor(status);
                statusPanel.Visible = true;
            }
            public static void UpdateStatusLabel(Label label, int status, string customText = null){
                if (label == null) return;
                label.Text = GetStatusText(status, customText);
                label.ForeColor = GetStatusColor(status);
            }
            public static void UpdateStatusIcon(PictureBox pictureBox, int status){
                if (pictureBox == null) return;
                var icon = GetStatusIcon(status);
                if (icon != null){
                    TSImageRenderer(pictureBox, icon, 0, ContentAlignment.MiddleCenter);
                    pictureBox.Visible = true;
                }else{
                    pictureBox.Image = null;
                    pictureBox.Visible = false;
                }
            }
            // Update Component
            public static void UpdateComponent(TSCustomPanel statusPanel, Label statusLabel, PictureBox statusIcon, int status, string customText = null){
                UpdateStatusPanel(statusPanel, status);
                UpdateStatusLabel(statusLabel, status, customText);
                UpdateStatusIcon(statusIcon, status);
            }
        }
        // VARIABLES
        // ======================================================================================================
        private const int STATUS_UNKNOWN = 0;
        private const int STATUS_ENABLED = 1;
        private const int STATUS_DISABLED = 2;
        // FIELDS
        private string _osVersionText = null;
        private int _OS_Status = STATUS_UNKNOWN;
        private int _UEFISecureBoot_Status = STATUS_UNKNOWN;
        private int _TPM_Status = STATUS_UNKNOWN;
        private int _VBS_Status = STATUS_UNKNOWN;
        private int _HVCI_Status = STATUS_UNKNOWN;
        private int _IOMMU_Status = STATUS_UNKNOWN;
        // LANGUAGE
        private TSGetLangs _software_lang;
        // CONSTRUCTOR
        // ======================================================================================================
        public GlowHSAuditorTool(){ InitializeComponent(); Button_Export.Enabled = false; }
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_HSAuditor_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                // Update Theme Color
                HSA_StatusHelper.UpdateColors(GlowMain.theme);
                // Load Languages
                _software_lang = new TSGetLangs(GlowMain.lang_path);
                HSA_StatusHelper.UpdateTexts(
                    _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_status_enabled"),
                    _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_status_disabled"),
                    _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_status_unknown"),
                    _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_status_checking"),
                    _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_status_error")
                );
                // Text
                Text = string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_title"), Application.ProductName);
                // Back Color
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                // TOOLTIP
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                // Panels
                foreach (Control control in this.Controls){
                    if (control is TSCustomPanel panel){
                        panel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                    }
                }
                // Labels
                var leftLabels = new Dictionary<Label, string>{
                    { Label_OS, "hsa_os" },
                    { Label_UEFISecureBoot, "hsa_uefi_secure_boot" },
                    { Label_TPM, "hsa_tpm" },
                    { Label_VBS, "hsa_vbs" },
                    { Label_HVCI, "hsa_hvci" },
                    { Label_IOMMU, "hsa_iommu" }
                };
                foreach (var kvp in leftLabels){
                    kvp.Key.Text = _software_lang.TSReadLangs("HardwareSecurityAuditorTool", kvp.Value);
                    kvp.Key.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                }
                //
                TSImageRenderer(Button_Check, GlowMain.theme == 1 ? Properties.Resources.ct_check_mark_light : Properties.Resources.ct_check_mark_dark, 19, ContentAlignment.MiddleRight);
                TSImageRenderer(Button_Export, GlowMain.theme == 1 ? Properties.Resources.ct_export_light : Properties.Resources.ct_export_dark, 19, ContentAlignment.MiddleRight);
                //
                Button_Check.Text = " " + _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_check_button");
                Button_Export.Text = " " + _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_button");
                //
                foreach (Control control in this.Controls){
                    if (control is TSCustomButton button){
                        button.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        button.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                        button.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        button.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        button.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                    }
                }
                // Set Tooltip
                TooltipTextLoader();
                // Refresh UI
                RefreshAllUI();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_HSAuditor_Preloader()"); }
            }
        }
        // TOOLTIP SETTINGS
        // ======================================================================================================
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e){ e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
        // TOOLTIP DYNAMIC TEXT LOADER
        private void TooltipTextLoader(){
            try{
                string os_text = _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_tooltip_text_os");
                string other_text = _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_tooltip_text_other");
                MainToolTip.RemoveAll();
                MainToolTip.SetToolTip(Panel_OS, os_text);
                MainToolTip.SetToolTip(Panel_UEFISecureBoot, string.Format(other_text, Label_UEFISecureBoot.Text.Trim()));
                MainToolTip.SetToolTip(Panel_TPM, string.Format(other_text, Label_TPM.Text.Trim()));
                MainToolTip.SetToolTip(Panel_VBS, string.Format(other_text, Label_VBS.Text.Trim()));
                MainToolTip.SetToolTip(Panel_HVCI, string.Format(other_text, Label_HVCI.Text.Trim()));
                MainToolTip.SetToolTip(Panel_IOMMU, string.Format(other_text, Label_IOMMU.Text.Trim()));
            }catch(Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "TooltipTextLoader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowHSAuditorTool_Load(object sender, EventArgs e){
            GTool_HSAuditor_Preloader();
        }
        // REFRESH UI
        // ======================================================================================================
        private void RefreshAllUI(){
            UpdateUI("OS", _OS_Status, _osVersionText);
            UpdateUI("SecureBoot", _UEFISecureBoot_Status);
            UpdateUI("TPM", _TPM_Status);
            UpdateUI("VBS", _VBS_Status);
            UpdateUI("HVCI", _HVCI_Status);
            UpdateUI("IOMMU", _IOMMU_Status);
        }
        // SET ALL CONTENT CHECKING
        // ======================================================================================================
        private void SetAllContentToChecking(){
            try{
                string checkingText = HSA_StatusHelper.GetCheckingText();
                var labels = new[] { Label_OS_V, Label_UEFISecureBoot_V, Label_TPM_V, Label_VBS_V, Label_HVCI_V, Label_IOMMU_V };
                var panels = new[] { Panel_OS_Status, Panel_UEFISecureBoot_Status, Panel_TPM_Status, Panel_VBS_Status, Panel_HVCI_Status, Panel_IOMMU_Status };
                var icons = new[] { PB_OS, PB_UEFISecureBoot, PB_TPM, PB_VBS, PB_HVCI, PB_IOMMU };
                foreach (var lbl in labels){
                    lbl.Text = checkingText;
                    lbl.ForeColor = HSA_StatusHelper.GetStatusColor(STATUS_UNKNOWN);
                }
                foreach (var panel in panels){
                    panel.BackColor = HSA_StatusHelper.GetStatusColor(STATUS_UNKNOWN);
                    panel.Visible = true;
                }
                foreach (var icon in icons){
                    icon.Image = null;
                    icon.Visible = false;
                }
            }catch(Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "SetAllContentToChecking()"); }
            }
        }
        // RUN CHECK BUTTON EVENT
        // ======================================================================================================
        private async void Button_Check_Click(object sender, EventArgs e){
            try{
                Button_Check.Enabled = false;
                Button_Export.Enabled = false;
                Button_Check.Text = " " + HSA_StatusHelper.GetCheckingText();
                //
                SetAllContentToChecking();
                //
                await LoadModule();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Button_Check_Click()"); }
            }
            finally{
                Button_Check.Enabled = true;
                Button_Export.Enabled = true;
                Button_Check.Text = " " + _software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_check_button");
                RefreshAllUI();
            }
        }
        // LOAD MODULE
        // ======================================================================================================
        private async Task LoadModule(){
            var start_modules = new List<Task>{
                Task.Run(() => Get_OS_Version()),
                Task.Run(() => Get_UEFISecureBoot_Status()),
                Task.Run(() => Get_TPM_Status()),
                Task.Run(() => Get_VBS_Status()),
                Task.Run(() => Get_HVCI_Status()),
                Task.Run(() => Get_IOMMU_Status())
            };
            await Task.WhenAll(start_modules);
        }
        // UPDATE UI THREAD SAFE
        // ======================================================================================================
        private void UpdateUI(string component, int status, string customText = null){
            if (InvokeRequired)
                Invoke(new Action(() => UpdateUIInternal(component, status, customText)));
            else
                UpdateUIInternal(component, status, customText);
        }
        // UPDATE UI INTERNAL
        // ======================================================================================================
        private void UpdateUIInternal(string component, int status, string customText = null){
            switch (component){
                case "OS":
                    _OS_Status = status;
                    if (!string.IsNullOrEmpty(customText))
                        _osVersionText = customText;
                    HSA_StatusHelper.UpdateComponent(Panel_OS_Status, Label_OS_V, PB_OS, status, _osVersionText);
                    break;
                case "SecureBoot":
                    _UEFISecureBoot_Status = status;
                    HSA_StatusHelper.UpdateComponent(Panel_UEFISecureBoot_Status, Label_UEFISecureBoot_V, PB_UEFISecureBoot, status);
                    break;
                case "TPM":
                    _TPM_Status = status;
                    HSA_StatusHelper.UpdateComponent(Panel_TPM_Status, Label_TPM_V, PB_TPM, status, customText);
                    break;
                case "VBS":
                    _VBS_Status = status;
                    HSA_StatusHelper.UpdateComponent(Panel_VBS_Status, Label_VBS_V, PB_VBS, status, customText);
                    break;
                case "HVCI":
                    _HVCI_Status = status;
                    HSA_StatusHelper.UpdateComponent(Panel_HVCI_Status, Label_HVCI_V, PB_HVCI, status);
                    break;
                case "IOMMU":
                    _IOMMU_Status = status;
                    HSA_StatusHelper.UpdateComponent(Panel_IOMMU_Status, Label_IOMMU_V, PB_IOMMU, status);
                    break;
            }
        }
        // OS
        // ======================================================================================================
        private void Get_OS_Version(){
            try{
                string rawOsName = "";
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                using (ManagementObjectCollection osResults = searcher.Get()){
                    foreach (ManagementObject os in osResults.Cast<ManagementObject>()){
                        using (os){
                            if (os["Caption"] != null){
                                rawOsName = os["Caption"].ToString();
                                break;
                            }
                        }
                    }
                }
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")){
                    string displayVersion = key?.GetValue("DisplayVersion", "")?.ToString()?.Trim() ?? "";
                    int buildNumber = Convert.ToInt32(key?.GetValue("CurrentBuildNumber", "0") ?? "0");
                    string finalOutput = rawOsName.Replace("Microsoft", "").Trim();
                    finalOutput = Regex.Replace(finalOutput, @"\s+", " ");
                    if (!string.IsNullOrEmpty(displayVersion) && !finalOutput.Contains(displayVersion))
                        finalOutput = $"{finalOutput} {displayVersion}";
                    int status = buildNumber >= 26200 ? STATUS_ENABLED : STATUS_DISABLED;
                    UpdateUI("OS", status, finalOutput);
                }
            }catch (Exception ex){
                UpdateUI("OS", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_OS_Version()"); }
            }
        }
        // UEFI Secure Boot
        // ======================================================================================================
        private void Get_UEFISecureBoot_Status(){
            try{
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State")){
                    object rawValue = key?.GetValue("UEFISecureBootEnabled");
                    if (rawValue != null){
                        try{
                            bool status = Convert.ToBoolean(rawValue);
                            UpdateUI("SecureBoot", status ? STATUS_ENABLED : STATUS_DISABLED);
                        }catch{
                            UpdateUI("SecureBoot", STATUS_UNKNOWN);
                        }
                    }else{
                        UpdateUI("SecureBoot", STATUS_UNKNOWN);
                    }
                }
            }catch (Exception ex){
                UpdateUI("SecureBoot", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_UEFISecureBoot_Status()"); }
            }
        }
        // TPM
        // ======================================================================================================
        private void Get_TPM_Status(){
            try{
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TPM")){
                    if (key == null){
                        UpdateUI("TPM", STATUS_DISABLED);
                        return;
                    }
                }
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftTpm", "SELECT IsActivated_InitialValue, SpecVersion FROM Win32_Tpm"))
                using (ManagementObjectCollection tpmResults = searcher.Get()){
                    bool tpmFound = false;
                    foreach (ManagementObject query in tpmResults.Cast<ManagementObject>()){
                        using (query){
                            tpmFound = true;
                            bool isActivated = Convert.ToBoolean(query["IsActivated_InitialValue"]);
                            string specVersion = Convert.ToString(query["SpecVersion"]) ?? "";
                            bool isTPM20 = specVersion.Contains("2.0");
                            if (isActivated && isTPM20){
                                UpdateUI("TPM", STATUS_ENABLED, "2.0");
                            }else if (isActivated && !isTPM20){
                                UpdateUI("TPM", STATUS_DISABLED, "1.2");
                            }else{
                                UpdateUI("TPM", STATUS_DISABLED);
                            }
                        }
                    }
                    if (!tpmFound){
                        UpdateUI("TPM", STATUS_DISABLED);
                    }
                }
            }catch (Exception ex){
                UpdateUI("TPM", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_TPM_Status()"); }
            }
        }
        // VBS
        // ======================================================================================================
        private void Get_VBS_Status(){
            try{
                using (var searcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\DeviceGuard", "SELECT VirtualizationBasedSecurityStatus FROM Win32_DeviceGuard"))
                using (ManagementObjectCollection vbsResults = searcher.Get()){
                    foreach (ManagementObject query in vbsResults.Cast<ManagementObject>()){
                        using (query){
                            if (query["VirtualizationBasedSecurityStatus"] != null){
                                int status = Convert.ToInt32(query["VirtualizationBasedSecurityStatus"]);
                                UpdateUI("VBS", status == 2 ? STATUS_ENABLED : STATUS_DISABLED);
                                return;
                            }
                        }
                    }
                }
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceGuard")){
                    if (key != null){
                        object rawValue = key.GetValue("EnableVirtualizationBasedSecurity");
                        if (rawValue != null && Convert.ToInt32(rawValue) == 1){
                            UpdateUI("VBS", STATUS_ENABLED);
                            return;
                        }
                    }
                }
                UpdateUI("VBS", STATUS_DISABLED);
            }catch (Exception ex){
                UpdateUI("VBS", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_VBS_Status()"); }
            }
        }
        // HVCI
        // ======================================================================================================
        private void Get_HVCI_Status(){
            try{
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity")){
                    bool enabled = false;
                    object raw = key?.GetValue("Enabled");
                    if (raw != null && int.TryParse(raw.ToString(), out int val))
                        enabled = (val == 1);
                    UpdateUI("HVCI", enabled ? STATUS_ENABLED : STATUS_DISABLED);
                }
            }catch (Exception ex){
                UpdateUI("HVCI", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_HVCI_Status()"); }
            }
        }
        // IOMMU
        // ======================================================================================================
        private void Get_IOMMU_Status(){
            try{
                int status = STATUS_DISABLED;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\DeviceGuard", "SELECT AvailableSecurityProperties FROM Win32_DeviceGuard"))
                using (ManagementObjectCollection iommuResults = searcher.Get()){
                    foreach (ManagementObject queryObj in iommuResults.Cast<ManagementObject>()){
                        using (queryObj){
                            // WMI returns UInt16[], not int[] - check generically.
                            if (queryObj["AvailableSecurityProperties"] is Array availableProps){
                                foreach (var prop in availableProps){
                                    try{
                                        if (Convert.ToInt32(prop) == 3){
                                            status = STATUS_ENABLED;
                                            break;
                                        }
                                    }catch { }
                                }
                                if (status == STATUS_ENABLED) break;
                            }
                        }
                    }
                }
                UpdateUI("IOMMU", status);
            }catch (Exception ex){
                UpdateUI("IOMMU", STATUS_UNKNOWN, HSA_StatusHelper.GetErrorText());
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Get_IOMMU_Status()"); }
            }
        }
        // EXPORT REPORT
        // ======================================================================================================
        private void Button_Export_Click(object sender, EventArgs e){
            try{
                Label[] label_status = new Label[6] { Label_OS, Label_UEFISecureBoot, Label_TPM, Label_VBS, Label_HVCI, Label_IOMMU };
                Label[] label_result = new Label[6] { Label_OS_V, Label_UEFISecureBoot_V, Label_TPM_V, Label_VBS_V, Label_HVCI_V, Label_IOMMU_V };
                using (SaveFileDialog saveFileDialog = new SaveFileDialog()){
                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.Filter = $"{_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_file_type")} (*.txt)|*.txt";
                    saveFileDialog.Title = string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "has_export_file_title"), Application.ProductName);
                    saveFileDialog.FileName = $"{string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_file_name"), Application.ProductName)}.txt";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK){
                        string filePath = saveFileDialog.FileName;
                        int maxLabelLength = 0;
                        foreach (Label lbl in label_status){
                            if (lbl.Text.Length > maxLabelLength){
                                maxLabelLength = lbl.Text.Length;
                            }
                        }
                        string titleText = string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_file_name"), Application.ProductName);
                        int maxLineLength = titleText.Length;
                        for (int i = 0; i < label_status.Length; i++){
                            int currentLineLength = maxLabelLength + 3 + label_result[i].Text.Length;
                            if (currentLineLength > maxLineLength){
                                maxLineLength = currentLineLength;
                            }
                        }
                        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8)){
                            writer.WriteLine(titleText + Environment.NewLine);
                            writer.WriteLine(new string('-', maxLineLength));
                            writer.WriteLine();
                            for (int i = 0; i < label_status.Length; i++){
                                string alignedLabel = label_status[i].Text.PadRight(maxLabelLength);
                                string lineContent = $"{alignedLabel} : {label_result[i].Text}";
                                if (i == label_status.Length - 1){
                                    writer.Write(lineContent);
                                }else{
                                    writer.WriteLine(lineContent);
                                }
                            }
                        }
                        DialogResult askToOpen = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_file_msg_success"), filePath, "\n\n"));
                        if (askToOpen == DialogResult.Yes){
                            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                        }
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "RunClickEventGoToSide()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(_software_lang.TSReadLangs("HardwareSecurityAuditorTool", "hsa_export_file_msg_failed"), "\n\n", ex.Message));
            }
        }
        // DOUBLE CLICK TO BROWSER OPEN
        // ======================================================================================================
        private void Panel_OS_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(0); }
        private void Panel_UEFISecureBoot_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(1); }
        private void Panel_TPM_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(2); }
        private void Panel_VBS_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(3); }
        private void Panel_HVCI_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(4); }
        private void Panel_IOMMU_DoubleClick(object sender, EventArgs e){ RunClickEventGoToSide(5); }
        // HELPERS
        // ======================================================================================================
        private void RunClickEventGoToSide(int site_mode){
            try{
                string prompt = "";
                switch (site_mode){
                    case 0:
                        // Windows 11 25H2 or later upgrade
                        prompt = "How to safely upgrade to Windows 11 25H2 step by step";
                        break;
                    case 1:
                        // Enabled UEFI Secure Boot
                        prompt = "How to enable UEFI Secure Boot in BIOS Windows 11";
                        break;
                    case 2:
                        // Enabled TPM 2.0
                        prompt = "How to enable TPM 2.0 Intel PTT AMD fTPM in BIOS";
                        break;
                    case 3:
                        // Enable VBS Virtualization Based Security
                        prompt = "How to enable VBS Virtualization Based Security Windows 11";
                        break;
                    case 4:
                        // Enabled HVCI Memory Integrity Core Isolation
                        prompt = "How to enable HVCI Memory Integrity Core Isolation Windows 11";
                        break;
                    case 5:
                        // Enabled IOMMU AMD-Vi Intel VT-d
                        prompt = "How to enable IOMMU AMD-Vi Intel VT-d SVM in BIOS";
                        break;
                }
                string search_browser_q = $"https://www.google.com/search?q={Uri.EscapeDataString(prompt)}";
                Process.Start(new ProcessStartInfo(search_browser_q){
                    UseShellExecute = true
                });
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "RunClickEventGoToSide()"); }
            }
        }
    }
}