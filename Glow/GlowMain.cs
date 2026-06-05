// ======================================================================================================
// Glow - System Analysis Software
// © Copyright 2019-2026, Eray Türkay.
// Publisher: Türkaysoft
// Project Type: Open Source
// License: MIT License
// Website: https://turkaysoft.com
// GitHub: https://github.com/turkaysoft/glow
// ======================================================================================================

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
// TS Modules
using Glow.glow_tools;
using static Glow.TSModules;

namespace Glow{
    public partial class GlowMain : Form{
        public GlowMain(){
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            // LANGUAGE SET TAGS
            // ==================
            arabicToolStripMenuItem.Tag = "ar";
            chineseToolStripMenuItem.Tag = "zh";
            englishToolStripMenuItem.Tag = "en";
            dutchToolStripMenuItem.Tag = "nl";
            frenchToolStripMenuItem.Tag = "fr";
            germanToolStripMenuItem.Tag = "de";
            hindiToolStripMenuItem.Tag = "hi";
            italianToolStripMenuItem.Tag = "it";
            japaneseToolStripMenuItem.Tag = "ja";
            koreanToolStripMenuItem.Tag = "ko";
            polishToolStripMenuItem.Tag = "pl";
            portugueseToolStripMenuItem.Tag = "pt";
            russianToolStripMenuItem.Tag = "ru";
            spanishToolStripMenuItem.Tag = "es";
            turkishToolStripMenuItem.Tag = "tr";
            // LANGUAGE SET EVENTS
            // ==================
            arabicToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            chineseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            englishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            dutchToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            frenchToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            germanToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            hindiToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            italianToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            japaneseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            koreanToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            polishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            portugueseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            russianToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            spanishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            turkishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            // DYNAMIC THEME LISTENER
            // ==================
            SystemEvents.UserPreferenceChanged += (s, e) => TSUseSystemTheme();
            // PAGE CONTROL FOR PARALLEL LOADING PROCEDURE
            // ==================
            var rotateButtons = new[] {
                OS_RotateBtn, MB_RotateBtn, CPU_RotateBtn, RAM_RotateBtn, GPU_RotateBtn,
                DISK_RotateBtn, NET_RotateBtn, USB_RotateBtn, SOUND_RotateBtn,
                BATTERY_RotateBtn, OSD_RotateBtn, SERVICES_RotateBtn, INSTALLED_RotateBtn, PRINT_RotateBtn
            };
            foreach (var r_btns in rotateButtons) r_btns.Enabled = false;
            var rotatePages = new[] {
                /*(Control)OS,*/ (Control)MB, (Control)CPU, (Control)RAM, (Control)GPU,
                (Control)DISK, (Control)NETWORK, (Control)USB, (Control)SOUND,
                (Control)BATTERY, (Control)DRIVERS, (Control)SERVICES, (Control)INSTAPPS, (Control)EXPORT
            };
            foreach (var r_pages in rotatePages) r_pages.Enabled = false;
            // SCROLL TOP LISTENER
            MainContent.Resize += MainContent_Resize;
            MainContent.SelectedIndexChanged += (s, e) => UpdateScrollTopButtonVisibility();
            //
            OS_ScrollTop.Click += ScrollTop_Click;
            MB_ScrollTop.Click += ScrollTop_Click;
            CPU_ScrollTop.Click += ScrollTop_Click;
            RAM_ScrollTop.Click += ScrollTop_Click;
            GPU_ScrollTop.Click += ScrollTop_Click;
            DISK_ScrollTop.Click += ScrollTop_Click;
            NET_ScrollTop.Click += ScrollTop_Click;
        }
        // GLOBAL VARIABLES
        // ======================================================================================================
        public static string lang, lang_path, wp_rotate;
        public static int theme, themeSystem, monitor_engine_mode;
        public static bool CPUbenchMode = false, DISKbenchMode = false, RAMbenchMode = false, BTfinderMode = false, SFCandDISMprocessStatus = false;
        // VARIABLES
        // ======================================================================================================
        private int menu_btns = 1, menu_rp = 1, startup_status, hiding_status, hiding_mode_wrapper;
        private bool loop_status = true, laptop_mode = false, ts_token_engine_stopper = false, debug_status;
        private string iapps_unknown;
        private decimal battery_fullChargedCapacity_mWh = 0;
        private const decimal battery_MIN_POWER_THRESHOLD = 0.1m;
        private readonly List<Label> allCopyableLabels = new List<Label>();
        // VISIBLE MODE DYNAMIC STAR
        // ======================================================================================================
        private static readonly List<int> vn_range = new List<int>(){ 10, 24 };
        private static readonly Random vis_m_property = new Random();
        // ======================================================================================================
        // UI COLORS
        private readonly List<Color> btn_colors_active = new List<Color>(){ Color.Transparent };
        private static readonly List<Color> header_colors = new List<Color>() { Color.Transparent, Color.Transparent, Color.Transparent };
        // HEADER SETTINGS
        // ======================================================================================================
        private class HeaderMenuColors : ToolStripProfessionalRenderer{
            public HeaderMenuColors() : base(new HeaderColors()){ }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e){ e.ArrowColor = header_colors[1]; base.OnRenderArrow(e); }
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e){
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float dpiScale = g.DpiX / 96f;
                Rectangle rect = e.ImageRectangle;
                using (Pen anti_alias_pen = new Pen(header_colors[2], 2.2f * dpiScale)){
                    anti_alias_pen.StartCap = LineCap.Round;
                    anti_alias_pen.EndCap = LineCap.Round;
                    anti_alias_pen.LineJoin = LineJoin.Round;
                    PointF p1 = new PointF(rect.Left + rect.Width * 0.18f, rect.Top + rect.Height * 0.52f);
                    PointF p2 = new PointF(rect.Left + rect.Width * 0.38f, rect.Top + rect.Height * 0.72f);
                    PointF p3 = new PointF(rect.Left + rect.Width * 0.78f, rect.Top + rect.Height * 0.28f);
                    g.DrawLines(anti_alias_pen, new[] { p1, p2, p3 });
                }
            }
        }
        private class HeaderColors : ProfessionalColorTable{
            public override Color MenuItemSelected => header_colors[0];
            public override Color ToolStripDropDownBackground => header_colors[0];
            public override Color ImageMarginGradientBegin => header_colors[0];
            public override Color ImageMarginGradientEnd => header_colors[0];
            public override Color ImageMarginGradientMiddle => header_colors[0];
            public override Color MenuItemSelectedGradientBegin => header_colors[0];
            public override Color MenuItemSelectedGradientEnd => header_colors[0];
            public override Color MenuItemPressedGradientBegin => header_colors[0];
            public override Color MenuItemPressedGradientMiddle => header_colors[0];
            public override Color MenuItemPressedGradientEnd => header_colors[0];
            public override Color MenuItemBorder => header_colors[0];
            public override Color CheckBackground => header_colors[0];
            public override Color ButtonSelectedBorder => header_colors[0];
            public override Color CheckSelectedBackground => header_colors[0];
            public override Color CheckPressedBackground => header_colors[0];
            public override Color MenuBorder => header_colors[0];
            public override Color SeparatorLight => header_colors[1];
            public override Color SeparatorDark => header_colors[1];
        }
        // TOOLTIP SETTINGS
        // ======================================================================================================
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e){ e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
        // DYNAMIC CLICK AND COPY SYSTEM
        // ======================================================================================================
        private void TSCAC_Properties(){
            try{
                var OSLabels = new Label[] { OS_SystemUser_V, OS_ComputerName_V, OS_Name_V, OS_Manufacturer_V, OS_SystemVersion_V, OS_SystemArchitectural_V, OS_ExperienceVersion_V, OS_Country_V, OS_TimeZone_V, OS_SystemTime_V, OS_SystemWorkTime_V, OS_LastBootTime_V, OS_Install_V, OS_SystemLastShutDown_V, OS_PrimaryOS_V, OS_PortableOS_V, OS_FastBoot_V, OS_AVProgram_V, OS_FirewallProgram_V, OS_AntiSpywareProgram_V, OS_WinDefCoreIsolation_V, OS_ActivePower_V, OS_ActivePowerGUID_V, OS_ActivePowerScreenTimeOutP_V, OS_ActivePowerScreenTimeOutB_V, OS_ActivePowerSleepTimeP_V, OS_ActivePowerSleepTimeB_V, OS_MSOfficeVersion_V, OS_WinActiveChannel_V, OS_WinLicenseTime_V, OS_NETFrameworkVersion_V, OS_Minidump_V, OS_BSODDate_V };
                var MBLabels = new Label[] { MB_MotherBoardName_V, MB_MotherBoardMan_V, MB_SystemModelMan_V, MB_SystemModelFamily_V, MB_SystemFamily_V, MB_SystemModel_V, MB_Chipset_V, MB_BiosManufacturer_V, MB_BiosDate_V, MB_BiosVersion_V, MB_SmBiosVersion_V, MB_BiosMode_V, MB_LastBIOSTime_V, MB_SecureBoot_V, MB_TPMStatus_V, MB_TPMPPIVersion_V, MB_TPMPPIAltVersion_V, MB_TPMMan_V, MB_TPMManVersion_V, MB_TPMManFullVersion_V, MB_TPMManPublisher_V };
                var CPULabels = new Label[] { CPU_Manufacturer_V, CPU_Architectural_V, CPU_Speed_V, CPU_CoreCount_V, CPU_ActiveCoreCount_V, CPU_LogicalCore_V, CPU_L1_V, CPU_L2_V, CPU_L3_V, CPU_Family_V, CPU_Slot_V, CPU_SocketDefinition_V, CPU_Virtualization_V, CPU_Usage_V, CPU_Process_V, CPU_Threads_V, CPU_Handles_V, };
                var RAMLabels = new Label[] { RAM_TotalRAM_V, RAM_UsageRAMCount_V, RAM_EmptyRamCount_V, RAM_TotalVirtualRam_V, RAM_UsageVirtualRam_V, RAM_EmptyVirtualRam_V, RAM_SlotStatus_V, RAM_Amount_V, RAM_Type_V, RAM_Frequency_V, RAM_Volt_V, RAM_FormFactor_V, RAM_Manufacturer_V, RAM_BankLabel_V, RAM_DataWidth_V, RAM_BellekType_V };
                var GPULabels = new Label[] { GPU_Manufacturer_V, GPU_VRAM_V, GPU_Version_V, GPU_DriverDate_V, GPU_Status_V, GPU_DeviceID_V, GPU_DacType_V, GPU_GraphicDriversName_V, GPU_DirectXAll_V, GPU_DirectXMax_V, GPU_InfFileName_V, GPU_INFSectionFile_V, GPU_CurrentColor_V, GPU_MonitorUserFriendlyName_V, GPU_MonitorManName_V, GPU_MonitorProductCodeID_V, GPU_MonitorConType_V, GPU_MonitorManfDate_V, GPU_MonitorManfDateWeek_V, GPU_MonitorHID_V, GPU_MonitorResLabel_V, GPU_MonitorVirtualRes_V, GPU_MonitorBounds_V, GPU_MonitorWorking_V, GPU_ScreenRefreshRate_V, GPU_ScreenBit_V, GPU_MonitorPrimary_V };
                var DISKLabels = new Label[] { DISK_Model_V, DISK_Man_V, DISK_VolumeID_V, DISK_VolumeName_V, DISK_Firmware_V, DISK_Size_V, DISK_UsedSpace_V, DISK_FreeSpace_V, DISK_FileSystem_V, DISK_FormattingType_V, DISK_Type_V, DISK_DriveType_V, DISK_InterFace_V, DISK_PartitionCount_V, DISK_MediaLoaded_V, DISK_MediaStatus_V, DISK_Health_V, DISK_Boot_V, DISK_Bootable_V, DISK_BitLockerStatus_V, DISK_BitLockerConversionStatus_V, DISK_BitLockerEncryptMehod_V, DISK_DriveCompressed_V };
                var NETLabels = new Label[] { NET_LT_Device_V, NET_LT_BandWidth_V, NET_LT_LocalIP_V, NET_LT_GatewayIP_V, NET_NetMan_V, NET_DriverVersion_V, NET_DriverDate_V, NET_ServiceName_V, NET_AdapterType_V, NET_Physical_V, NET_DeviceID_V, NET_ConnectionType_V, NET_Dhcp_status_V, NET_Dhcp_server_V, NET_DHCPFirstIpTime_V, NET_DHCPLastIpTime_V, NET_LocalConSpeed_V };
                var USBLabels = new Label[] { USB_ConName_V, USB_ConMan_V, USB_ConDeviceID_V, USB_ConPNPDeviceID_V, USB_ConDeviceStatus_V, USB_DeviceName_V, USB_DeviceMan_V, USB_DriverVersion_V, USB_DriverDate_V, USB_InfFile_V, USB_DeviceID_V, USB_HardwareID_V };
                var SOUNDLabels = new Label[] { SOUND_DeviceName_V, SOUND_DeviceManufacturer_V, SOUND_DriverVersion_V, SOUND_DriverDate_V, SOUND_DeviceID_V, SOUND_PNPDeviceID_V, SOUND_DeviceStatus_V };
                var BATTERYLabels = new Label[] { BATTERY_Status_V, BATTERY_Model_V,  BATTERY_Chemistry_V, BATTERY_DesignCapacity_V, BATTERY_FullChargeCapacity_V, BATTERY_RemainingChargeCapacity_V, BATTERY_Voltage_V, BATTERY_ChargePower_V, BATTERY_ChargeCurrent_V, BATTERY_DeChargePower_V, BATTERY_DeChargeCurrent_V };
                //
                allCopyableLabels.Clear();
                allCopyableLabels.AddRange(OSLabels);
                allCopyableLabels.AddRange(MBLabels);
                allCopyableLabels.AddRange(CPULabels);
                allCopyableLabels.AddRange(RAMLabels);
                allCopyableLabels.AddRange(GPULabels);
                allCopyableLabels.AddRange(DISKLabels);
                allCopyableLabels.AddRange(NETLabels);
                allCopyableLabels.AddRange(USBLabels);
                allCopyableLabels.AddRange(SOUNDLabels);
                allCopyableLabels.AddRange(BATTERYLabels);
                //
                if (hiding_mode_wrapper != 1){
                    var HIDINGLabels = new Label[] { OS_SavedUser_V, OS_DeviceID_V, OS_Serial_V, OS_WinKey_V, OS_WinLicenseURL_V, OS_WinLicenseVerifiURL_V, OS_Wallpaper_V, MB_DeviceSerialNumber_V, MB_MotherBoardSerial_V, MB_SystemSKU_V, MB_TPMManID_V, CPU_SerialName_V, RAM_Serial_V, RAM_PartNumber_V, GPU_MonitorSerialNumberID_V, DISK_Serial_V, DISK_VolumeSerial_V, NET_MacAdress_V, NET_Guid_V, NET_IPv4Adress_V, NET_IPv6Adress_V, NET_P_IP_Adress_V, NET_P_ISP_V, NET_DNS_v4_1_V, NET_DNS_v4_2_V, NET_DNS_v6_1_V, NET_DNS_v6_2_V, USB_DeviceGUID_V, BATTERY_Serial_V };
                    allCopyableLabels.AddRange(HIDINGLabels);
                }
                //
                TSInitLabels(allCopyableLabels);
                TSSetTooltips(allCopyableLabels);
            }catch (Exception ex) {
                if (debug_status){ TSErrorLog.LogException(ex, "TSCAC_Properties()"); }
            }
        }
        private void TSInitLabels(IEnumerable<Label> labelList){
            foreach (Label label in labelList){
                label.Cursor = Cursors.Hand;
                label.DoubleClick -= TSLabel_DoubleClick;
                label.DoubleClick += TSLabel_DoubleClick;
            }
        }
        private void TSSetTooltips(IEnumerable<Label> labelList){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string tooltipText = software_lang.TSReadLangs("ContentCrossFeature", "ccf_copy_hover");
            foreach (Label label in labelList){
                MainToolTip.SetToolTip(label, tooltipText);
            }
        }
        private void TSLabel_DoubleClick(object sender, EventArgs e){
            try{
                if (sender is Label label){
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    Clipboard.SetText(label.Text);
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("ContentCrossFeature", "ccf_copy_message"), label.Text));
                }
            }catch (Exception ex){
                if (debug_status){ TSErrorLog.LogException(ex, "TSLabel_DoubleClick()"); }
            }
        }
        // UI DPI CHANGER
        // ======================================================================================================
        private int TSDPIChanger(int size){
            return (int)(size * this.DeviceDpi / 96f);
        }
        // LOAD SOFTWARE SETTINGS
        // ======================================================================================================
        private async void RunSoftwareEngine(){
            HeaderMenu.Cursor = Cursors.Hand;
            if (this.DeviceDpi / 96f >= 1.5){
                HeaderPanel.Padding = new Padding(TSDPIChanger(4), 0, 0, 0);
            }else if (this.DeviceDpi / 96f >= 1.25){
                HeaderPanel.Padding = new Padding(TSDPIChanger(3), 0, 0, 0);
            }
            if (this.DeviceDpi / 96f >= 1.25){
                OS.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                MB.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                CPU.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                RAM.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                GPU.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                DISK.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                NETWORK.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                USB.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                SOUND.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                BATTERY.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                DRIVERS.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                SERVICES.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                INSTAPPS.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
                EXPORT.Padding = new Padding(TSDPIChanger(2), TSDPIChanger(2), TSDPIChanger(0), TSDPIChanger(4));
            }
            // INSTALLED DRIVERS
            // ======================================================================================================
            for (int i = 0; i < 6; i++) OSD_DataMainTable.Columns.Add($"osd_file_{i}", "osd_variable");
            foreach (DataGridViewColumn OSD_Column in OSD_DataMainTable.Columns) OSD_Column.SortMode = DataGridViewColumnSortMode.NotSortable;
            // SERVICES
            // ======================================================================================================
            for (int i = 0; i < 6; i++) SERVICE_DataMainTable.Columns.Add($"ss_file_{i}", "ss_variable");
            foreach (DataGridViewColumn SERVICE_Column in SERVICE_DataMainTable.Columns) SERVICE_Column.SortMode = DataGridViewColumnSortMode.NotSortable;
            // INSTALLED APPS
            // ======================================================================================================
            INSTAPPS_DataMainTable.Columns.Add(new DataGridViewImageColumn { Name = "Icon", Width = 40, ImageLayout = DataGridViewImageCellLayout.Zoom });
            for (int i = 0; i < 6; i++) INSTAPPS_DataMainTable.Columns.Add($"iapps_file_{i}", "iapps_variable");
            foreach (DataGridViewColumn OSD_Column in INSTAPPS_DataMainTable.Columns) OSD_Column.SortMode = DataGridViewColumnSortMode.NotSortable;
            // ALL DGV WIDTH - HEIGHT - PADDING
            // ======================================================================================================
            int[] columnWidths = { 160, 120, 120, 120, 100, 100 };
            int[] columnWidthsApps = { 65, 175, 100, 125, 75, 75, 150 };
            for (int i = 0; i < columnWidths.Length; i++){
                OSD_DataMainTable.Columns[i].Width = (int)(columnWidths[i] * this.DeviceDpi / 96f);
                SERVICE_DataMainTable.Columns[i].Width = (int)(columnWidths[i] * this.DeviceDpi / 96f);
            }
            for (int i = 0; i < columnWidthsApps.Length; i++){
                INSTAPPS_DataMainTable.Columns[i].Width = (int)(columnWidthsApps[i] * this.DeviceDpi / 96f);
            }
            //
            OSD_DataMainTable.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
            SERVICE_DataMainTable.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
            INSTAPPS_DataMainTable.RowTemplate.Height = (int)(36 * this.DeviceDpi / 96f);
            //
            int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
            foreach (var tableMod in new[] { OSD_DataMainTable, SERVICE_DataMainTable, INSTAPPS_DataMainTable }){
                foreach (DataGridViewColumn columnPadding in tableMod.Columns){
                    if (tableMod == INSTAPPS_DataMainTable && columnPadding.Index == 0)
                        continue;
                    columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
                }
            }
            // OSD - SERVICE - INSTAPP CLEAR BTN DPI HEIGHT
            // ======================================================================================================
            OSD_TextBoxClearBtn.Height = OSD_TextBox.Height + 2;
            SERVICE_TextBoxClearBtn.Height = SERVICE_TextBox.Height + 2;
            INSTAPPS_TextBoxClearBtn.Height = INSTAPPS_TextBox.Height + 2;
            // SET DOUBLE BUFFER
            // ======================================================================================================
            typeof(TabControl).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, MainContent, new object[] { true });
            foreach (TabPage tp in MainContent.TabPages){
                typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, tp, new object[] { true });
            }
            //
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, OSD_DataMainTable, new object[]{ true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, SERVICE_DataMainTable, new object[]{ true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, INSTAPPS_DataMainTable, new object[]{ true });
            typeof(FlowLayoutPanel).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, EXPORT_CB_FLP, new object[]{ true });
            // THEME - LANG - STARTUP - HIDIN MODE PRELOADER
            // ======================================================================================================
            TSSettingsModule software_read_settings = new TSSettingsModule(ts_sf);
            //
            int theme_mode = int.TryParse(software_read_settings.TSReadSettings(ts_settings_container, "ThemeStatus"), out int the_status) && (the_status == 0 || the_status == 1 || the_status == 2) ? the_status : 1;
            if (theme_mode == 2) { themeSystem = 2; Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); } else Theme_engine(theme_mode);
            darkThemeToolStripMenuItem.Checked = theme_mode == 0;
            lightThemeToolStripMenuItem.Checked = theme_mode == 1;
            systemThemeToolStripMenuItem.Checked = theme_mode == 2;
            //
            string lang_mode = software_read_settings.TSReadSettings(ts_settings_container, "LanguageStatus");
            var languageFiles = new Dictionary<string, (object langResource, ToolStripMenuItem menuItem, bool fileExists)>{
                { "ar", (ts_lang_ar, arabicToolStripMenuItem, File.Exists(ts_lang_ar)) },
                { "zh", (ts_lang_zh, chineseToolStripMenuItem, File.Exists(ts_lang_zh)) },
                { "en", (ts_lang_en, englishToolStripMenuItem, File.Exists(ts_lang_en)) },
                { "nl", (ts_lang_nl, dutchToolStripMenuItem, File.Exists(ts_lang_nl)) },
                { "fr", (ts_lang_fr, frenchToolStripMenuItem, File.Exists(ts_lang_fr)) },
                { "de", (ts_lang_de, germanToolStripMenuItem, File.Exists(ts_lang_de)) },
                { "hi", (ts_lang_hi, hindiToolStripMenuItem, File.Exists(ts_lang_hi)) },
                { "it", (ts_lang_it, italianToolStripMenuItem, File.Exists(ts_lang_it)) },
                { "ja", (ts_lang_ja, japaneseToolStripMenuItem, File.Exists(ts_lang_ja)) },
                { "ko", (ts_lang_ko, koreanToolStripMenuItem, File.Exists(ts_lang_ko)) },
                { "pl", (ts_lang_pl, polishToolStripMenuItem, File.Exists(ts_lang_pl)) },
                { "pt", (ts_lang_pt, portugueseToolStripMenuItem, File.Exists(ts_lang_pt)) },
                { "ru", (ts_lang_ru, russianToolStripMenuItem, File.Exists(ts_lang_ru)) },
                { "es", (ts_lang_es, spanishToolStripMenuItem, File.Exists(ts_lang_es)) },
                { "tr", (ts_lang_tr, turkishToolStripMenuItem, File.Exists(ts_lang_tr)) },
            };
            foreach (var langLoader in languageFiles){ langLoader.Value.menuItem.Enabled = langLoader.Value.fileExists; }
            var (langResource, selectedMenuItem, _) = languageFiles.ContainsKey(lang_mode) ? languageFiles[lang_mode] : languageFiles["en"];
            Lang_engine(Convert.ToString(langResource), lang_mode);
            selectedMenuItem.Checked = true;
            //
            string startup_mode = software_read_settings.TSReadSettings(ts_settings_container, "StartupStatus");
            startup_status = int.TryParse(startup_mode, out int str_status) && (str_status == 0 || str_status == 1) ? str_status : 0;
            WindowState = startup_status == 1 ? FormWindowState.Maximized : FormWindowState.Normal;
            windowedToolStripMenuItem.Checked = startup_status == 0;
            fullScreenToolStripMenuItem.Checked = startup_status == 1;
            //
            string hiding_mode = software_read_settings.TSReadSettings(ts_settings_container, "HidingStatus");
            hiding_status = int.TryParse(hiding_mode, out int hid_status) && (hid_status == 0 || hid_status == 1) ? hid_status : 0;
            hiding_mode_wrapper = hiding_status;
            hidingModeOffToolStripMenuItem.Checked = hiding_status == 0;
            hidingModeOnToolStripMenuItem.Checked = hiding_status == 1;
            //
            string debug_mode = software_read_settings.TSReadSettings(ts_settings_container, "DebugMode");
            debug_status = debug_mode == "1";
            debugOffToolStripMenuItem.Checked = !debug_status;
            debugOnToolStripMenuItem.Checked = debug_status;
            if (debug_status) { TSLogger.Enable(true, Program.glow_console_debug_mode); }
            // PARALLEL LOADER START
            await GlowBootstrapper();
        }
        // TS PARALLEL RUN SECTION MODULE
        // ======================================================================================================
        private async Task RunSec(Action __action){
            try{
                await Task.Run(__action);
            }catch (OperationCanceledException ex){
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }catch (Exception ex){
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }
        }
        private async Task RunSec(Func<Task> __action){
            try{
                await Task.Run(async () => await __action());
            }catch (OperationCanceledException ex){
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }catch (Exception ex){
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }
        }
        private void SetDebugMenuState(bool enabled){
            if (InvokeRequired){
                Invoke(new Action(() =>{
                    debugOnToolStripMenuItem.Enabled = enabled;
                    debugOffToolStripMenuItem.Enabled = enabled;
                }));
                return;
            }
            debugOnToolStripMenuItem.Enabled = enabled;
            debugOffToolStripMenuItem.Enabled = enabled;
        }
        // TASK ALL PROCESS
        // ======================================================================================================
        private async Task GlowBootstrapper(){
            // PROPERTIES
            // ---------------
            var btTime = Stopwatch.StartNew();
            var tList = new List<Task>();
            // RUN PARALLEL START
            // ---------------
            try{
                tList.Add(RunSec(() => Os()));
                tList.Add(RunSec(() => Mb()));
                tList.Add(RunSec(() => Cpu()));
                tList.Add(RunSec(() => Ram()));
                tList.Add(RunSec(() => Gpu()));
                tList.Add(RunSec(() => Disk()));
                tList.Add(RunSec(() => Network()));
                tList.Add(RunSec(() => Usb()));
                tList.Add(RunSec(() => Sound()));
            }catch (Exception ex){ if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
            try{
                laptop_mode = SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery;
                if (laptop_mode){
                    Battery_visible_on();
                    tList.Add(RunSec(Battery));
                    tList.Add(RunSec(BatteryBgProcess));
                }else{
                    Battery_visible_off();
                    //
                    void EnableBatteryControls(){
                        BATTERY_RotateBtn.Enabled = true;
                        ((Control)BATTERY).Enabled = true;
                    }
                    if (MainContent.InvokeRequired) MainContent.Invoke((Action)EnableBatteryControls);
                    else EnableBatteryControls();
                    //
                    if (debug_status){
                        TSLogger.Log("<--- Battery Section Loaded / Desktop Mode --->");
                    }
                }
            }catch (Exception ex){ if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
            try{
                tList.Add(RunSec(() => InstalledDrivers()));
                tList.Add(RunSec(() => InstalledServices()));
                tList.Add(RunSec(() => InstalledApps()));
                //
                tList.Add(RunSec(() => OsBgProcess()));
                tList.Add(RunSec(() => ProcessorBgProcess()));
                tList.Add(RunSec(() => CpuBgProcess()));
                tList.Add(RunSec(() => RamBgProcess()));
                tList.Add(RunSec(() => NetBgProcess()));
                tList.Add(RunSec(() => NetBGProcessGateway()));
                //
                tList.Add(RunSec(() => ExportModsAdd()));
                tList.Add(RunSec(() => ExportModsFilterAdd()));
            }catch (Exception ex){ if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
            // RUN PARALLEL PROCESS END
            // ---------------
            try{
                await Task.WhenAll(tList);
                if (MainContent.InvokeRequired){
                    MainContent.Invoke(new Action(() => { ((Control)EXPORT).Enabled = true; PRINT_RotateBtn.Enabled = true; }));
                }else{
                    ((Control)EXPORT).Enabled = true; PRINT_RotateBtn.Enabled = true;
                }
                btTime.Stop();
                if (debug_status){
                    TSLogger.Log("<--- Export Section Loaded --->");
                    TSLogger.Log("<--------------------------->");
                    TSLogger.Log("<--- ALL TASKS COMPLETED --->");
                    TSLogger.Log($"<--- Total Load Time: {btTime.Elapsed.TotalSeconds:F2} seconds --->");
                    TSLogger.Log("<--------------------------->");
                    // FILE LOCK RELEASE
                    TSLogger.Close();
                }
                // DEBUG MODE MENU ENABLE
                SetDebugMenuState(true);
            }catch (Exception ex){ if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
        }
        // SOFTWARE LOAD
        // ======================================================================================================
        private void Glow_Load(object sender, EventArgs e){ 
            Text = TS_VersionEngine.TS_SoftwareVersion(0);
            // LAUNCH PROCESS 
            // ====================================
            RunSoftwareEngine();
            // SOFTWARE UPDATE CHECK
            // ====================================
            Task.Run(() => Software_update_check(0));
            // DYNAMIC MULTI CLICK COPYABLE LABELS
            // ====================================
            TSCAC_Properties();
        }
        #region OS_Section
        // OPERATING SYSTEM
        // ======================================================================================================
        readonly List<string> minidump_files_list = new List<string>();
        readonly List<string> minidump_files_date_list = new List<string>();
        readonly string mdp_1 = Path.Combine(Program.windows_disk, @"Windows\Minidump");
        readonly string mdp_2 = Path.Combine(Program.windows_disk, @"Windows\memory.dmp");
        private void Os(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            ManagementObjectSearcher search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT RegisteredUser, Caption, Manufacturer, OSArchitecture, SerialNumber, EncryptionLevel, LastBootUpTime, Primary, PortableOperatingSystem FROM Win32_OperatingSystem");
            ManagementObjectSearcher search_av = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM AntiVirusProduct");
            ManagementObjectSearcher search_fw = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM FirewallProduct");
            ManagementObjectSearcher search_as = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM AntiSpywareProduct");
            try{
                // SYSTEM USER
                OS_SystemUser_V.Text = Environment.UserName;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // PC NAME
                OS_ComputerName_V.Text = Dns.GetHostName();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            foreach (ManagementObject query_os_rotate in search_os.Get().Cast<ManagementObject>()){
                try{
                    // REGISTERED USER
                    string os_saved_user = Convert.ToString(query_os_rotate["RegisteredUser"]).Trim();
                    if (!string.IsNullOrEmpty(os_saved_user)){
                        if (hiding_mode_wrapper != 1){
                            OS_SavedUser_V.Text = os_saved_user;
                        }else{
                            OS_SavedUser_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        OS_SavedUser_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS NAME
                    string os_name = Convert.ToString(query_os_rotate["Caption"]);
                    OS_Name_V.Text = os_name;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS MANUFACTURER
                    OS_Manufacturer_V.Text = Convert.ToString(query_os_rotate["Manufacturer"]);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS VERSION + ESU
                    const string regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                    string displayVersion = Convert.ToString(Registry.GetValue(regPath, "DisplayVersion", ""))?.Trim();
                    string build = Convert.ToString(Registry.GetValue(regPath, "CurrentBuild", ""))?.Trim();
                    string ubr = Convert.ToString(Registry.GetValue(regPath, "UBR", ""))?.Trim();
                    //
                    bool isEsu = false;
                    try{
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows\ConsumerESU", false)){
                            object value = key?.GetValue("ESUEligibilityResult");
                            if (value is int v){
                                isEsu = (v == 1);
                            }else if (value is byte[] bytes && bytes.Length >= 4){
                                isEsu = (BitConverter.ToInt32(bytes, 0) == 1);
                            }
                        }
                    }catch { }
                    if (!string.IsNullOrEmpty(displayVersion) && !string.IsNullOrEmpty(build) && !string.IsNullOrEmpty(ubr)){
                        if (Program.windows_mode == 0 && isEsu){
                            OS_SystemVersion_V.Text = $"{displayVersion} ({build}.{ubr}) (ESU)";
                        }else{
                            OS_SystemVersion_V.Text = $"{displayVersion} ({build}.{ubr})";
                        }
                    }else{
                        OS_SystemVersion_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS ARCHITECTURE
                    string system_bit = Regex.Replace(Convert.ToString(query_os_rotate["OSArchitecture"]), @"\D", string.Empty).Trim();
                    OS_SystemArchitectural_V.Text = system_bit + " " + software_lang.TSReadLangs("Os_Content", "os_c_bit") + " - " + string.Format("(x{0})", system_bit);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS DEVICE ID
                    string os_device_id = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient", "MachineId", "").ToString().Trim();
                    if (!string.IsNullOrEmpty(os_device_id)){
                        string os_device_id_replacer = os_device_id.Replace("{", string.Empty).Replace("}", string.Empty);
                        if (hiding_mode_wrapper != 1){
                            OS_DeviceID_V.Text = os_device_id_replacer;
                        }else{
                            OS_DeviceID_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        OS_DeviceID_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS SERIAL
                    string os_serial = Convert.ToString(query_os_rotate["SerialNumber"]).Trim();
                    if (hiding_mode_wrapper != 1){
                        OS_Serial_V.Text = os_serial;
                    }else{
                        OS_Serial_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // WINDOWS EXPERIENCE VERSION
                    List<Version> exp_ver_list = new List<Version>();
                    using (var get_exp_values = new ManagementObjectSearcher("root\\CIMV2", "SELECT Version FROM Win32_InstalledStoreProgram WHERE Name LIKE 'MicrosoftWindows.Client.CBS%'")){
                        foreach (ManagementObject exp_get in get_exp_values.Get().Cast<ManagementObject>()){
                            var vers_obj = exp_get["Version"];
                            if (vers_obj == null)
                                continue;
                            if (Version.TryParse(vers_obj.ToString(), out Version parsed_exp_vers))
                                exp_ver_list.Add(parsed_exp_vers);
                        }
                    }
                    string expVersion = exp_ver_list.Count > 0 ? exp_ver_list.Max().ToString() : software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                    OS_ExperienceVersion_V.Text = expVersion;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // SYSTEM COUNTRY AND LANGUAGE
                    CultureInfo culture_info = CultureInfo.InstalledUICulture;
                    OS_Country_V.Text = culture_info.DisplayName.Trim();
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // SYSTEM TIME ZONE
                    TimeZone os_time_zone = TimeZone.CurrentTimeZone;
                    OS_TimeZone_V.Text = os_time_zone.StandardName;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS LAST BOOT
                    string last_bt = Convert.ToString(query_os_rotate["LastBootUpTime"]);
                    DateTime last_boot_time = ManagementDateTimeConverter.ToDateTime(last_bt);
                    OS_LastBootTime_V.Text = $"{last_boot_time:dd.MM.yyyy} - {last_boot_time:HH:mm:ss}";
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // OS SHUTDOWN TIME
                    string sd_time_path = @"System\CurrentControlSet\Control\Windows";
                    RegistryKey sd_time_key = Registry.LocalMachine.OpenSubKey(sd_time_path);
                    byte[] sd_time_val = (byte[])sd_time_key.GetValue("ShutdownTime");
                    sd_time_key.Close();
                    long sd_time_as_long = BitConverter.ToInt64(sd_time_val, 0);
                    DateTime shut_down_time = DateTime.FromFileTime(sd_time_as_long);
                    OS_SystemLastShutDown_V.Text = shut_down_time.ToString("dd.MM.yyyy - HH:mm:ss");
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // PRIMARY OS STATUS
                    bool system_primary_os_status = Convert.ToBoolean(query_os_rotate["Primary"]);
                    if (system_primary_os_status == true){
                        OS_PrimaryOS_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_yes");
                    }else if (system_primary_os_status == false){
                        OS_PrimaryOS_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_no");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // PORTABLE OS STATUS
                    bool system_portable_status = Convert.ToBoolean(query_os_rotate["PortableOperatingSystem"]);
                    if (system_portable_status == true){
                        OS_PortableOS_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_yes");
                    }else if (system_portable_status == false){
                        OS_PortableOS_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_no");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
                try{
                    // FASTBOOT STATUS
                    RegistryKey get_fastboot_status = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power");
                    string get_fs_val = get_fastboot_status?.GetValue("HiberbootEnabled")?.ToString().Trim();
                    string langKey = get_fs_val == "1" ? "os_c_fastboot_active" : get_fs_val == "0" ? "os_c_fastboot_deactive" : "os_c_unknown";
                    OS_FastBoot_V.Text = software_lang.TSReadLangs("Os_Content", langKey);
                    get_fastboot_status?.Close();
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                }
            }
            try{
                // SYSTEM ANTI-VIRUS PRODUCT
                List<string> av_list = new List<string>();
                foreach (ManagementObject query_av in search_av.Get().Cast<ManagementObject>()){
                    string av_path = Convert.ToString(query_av["pathToSignedProductExe"]).Trim();
                    av_path = Environment.ExpandEnvironmentVariables(av_path);
                    if (string.IsNullOrEmpty(av_path) || !File.Exists(av_path)){
                        av_path = Convert.ToString(query_av["pathToSignedReportingExe"]).Trim();
                        av_path = Environment.ExpandEnvironmentVariables(av_path);
                    }
                    if (!string.IsNullOrEmpty(av_path) && File.Exists(av_path)){
                        string av_name = Convert.ToString(query_av["DisplayName"]).Trim();
                        av_list.Add(av_name);
                    }
                }
                if (av_list.Count < 1){
                    av_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_anti_virus_count"));
                }
                string av_list_split = string.Join(" - ", av_list);
                OS_AVProgram_V.Text = av_list_split.Trim();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // SYSTEM FIREWALL PRODUCT
                List<string> fw_list = new List<string>();
                foreach (ManagementObject query_fw in search_fw.Get().Cast<ManagementObject>()){
                    string fw_path = Convert.ToString(query_fw["pathToSignedProductExe"]).Trim();
                    fw_path = Environment.ExpandEnvironmentVariables(fw_path);
                    if (string.IsNullOrEmpty(fw_path) || !File.Exists(fw_path)){
                        fw_path = Convert.ToString(query_fw["pathToSignedReportingExe"]).Trim();
                        fw_path = Environment.ExpandEnvironmentVariables(fw_path);
                    }
                    if (!string.IsNullOrEmpty(fw_path) && File.Exists(fw_path)){
                        string fw_name = Convert.ToString(query_fw["DisplayName"]).Trim();
                        fw_list.Add(fw_name);
                    }
                }
                if (fw_list.Count < 1){
                    fw_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_firewall_count"));
                }
                string fw_list_split = string.Join(" - ", fw_list);
                OS_FirewallProgram_V.Text = fw_list_split.Trim();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // SYSTEM ANTI-SPYWARE PRODUCT
                List<string> as_list = new List<string>();
                foreach (ManagementObject query_as in search_as.Get().Cast<ManagementObject>()){
                    string as_path = Convert.ToString(query_as["pathToSignedProductExe"]).Trim();
                    as_path = Environment.ExpandEnvironmentVariables(as_path);
                    if (string.IsNullOrEmpty(as_path) || !File.Exists(as_path)){
                        as_path = Convert.ToString(query_as["pathToSignedReportingExe"]).Trim();
                        as_path = Environment.ExpandEnvironmentVariables(as_path);
                    }
                    if (!string.IsNullOrEmpty(as_path) && File.Exists(as_path)){
                        string as_name = Convert.ToString(query_as["DisplayName"]).Trim();
                        as_list.Add(as_name);
                    }
                }
                if (as_list.Count < 1){
                    as_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_anti_spyware_count"));
                }
                string as_list_split = string.Join(" - ", as_list);
                OS_AntiSpywareProgram_V.Text = as_list_split.Trim();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // WINDOWS DEFENDER CORE ISOLATION STATUS
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity")){
                    string val = key?.GetValue("Enabled")?.ToString()?.Trim() ?? "0";
                    string langKey = val == "1" ? "os_c_win_core_isolation_active" : "os_c_win_core_isolation_deactive";
                    OS_WinDefCoreIsolation_V.Text = software_lang.TSReadLangs("Os_Content", langKey);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // ACTIVE POWER PLAN
                string powerOutput;
                string activePlanName = null;
                string activePlanGuid = null;
                //
                using (var get_power_mode = new Process()){
                    get_power_mode.StartInfo.FileName = "powercfg";
                    get_power_mode.StartInfo.Arguments = "/L";
                    get_power_mode.StartInfo.RedirectStandardOutput = true;
                    get_power_mode.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                    get_power_mode.StartInfo.UseShellExecute = false;
                    get_power_mode.StartInfo.CreateNoWindow = true;
                    get_power_mode.Start();
                    powerOutput = get_power_mode.StandardOutput.ReadToEnd();
                    get_power_mode.WaitForExit();
                }
                //
                var split_power_line = powerOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                //
                foreach (var target_power_line in split_power_line){
                    if (target_power_line.Contains("GUID:") && target_power_line.Trim().EndsWith("*")){
                        var planMatch = Regex.Match(target_power_line, @"\((.*?)\)");
                        if (planMatch.Success){
                            activePlanName = planMatch.Groups[1].Value;
                        }
                        var guidMatch = Regex.Match(target_power_line, @"GUID:\s*([a-fA-F0-9\-]{36})");
                        if (guidMatch.Success){
                            activePlanGuid = guidMatch.Groups[1].Value;
                        }
                        break;
                    }
                }
                //
                OS_ActivePower_V.Text = activePlanName != null ? $"{activePlanName}" : software_lang.TSReadLangs("Os_Content", "os_c_a_power_null");
                OS_ActivePowerGUID_V.Text = activePlanGuid != null ? $"{activePlanGuid}" : software_lang.TSReadLangs("Os_Content", "os_c_a_power_null");
                bool isLaptop = SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery;
                // SCREEN CLOSE AND SLEEP TIME
                if (!string.IsNullOrEmpty(activePlanGuid)){
                    // TIMEOUT
                    int acTimeout = SCQueryTimeout(activePlanGuid, "SUB_VIDEO", "VIDEOIDLE", true);
                    OS_ActivePowerScreenTimeOutP_V.Text = SCFormatTimeout(acTimeout);
                    if (isLaptop){
                        int dcTimeout = SCQueryTimeout(activePlanGuid, "SUB_VIDEO", "VIDEOIDLE", false);
                        OS_ActivePowerScreenTimeOutB_V.Text = SCFormatTimeout(dcTimeout);
                    }else{
                        OS_ActivePowerScreenTimeOutB_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_a_power_desktop");
                    }
                    // SLEE TIME
                    int acSleep = SCQueryTimeout(activePlanGuid, "SUB_SLEEP", "STANDBYIDLE", true);
                    OS_ActivePowerSleepTimeP_V.Text = SCFormatTimeout(acSleep);
                    if (isLaptop){
                        int dcSleep = SCQueryTimeout(activePlanGuid, "SUB_SLEEP", "STANDBYIDLE", false);
                        OS_ActivePowerSleepTimeB_V.Text = SCFormatTimeout(dcSleep);
                    }else{
                        OS_ActivePowerSleepTimeB_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_a_power_desktop");
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // MS OFFICE VERSION
                StartMsOfficeVersionCheck();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // WINDOWS LICENSE KEY
                string win_key = TSWindowsProductKey.GetWindowsProductKey().Trim();
                if (!string.IsNullOrEmpty(win_key) && win_key != "NS_OS" && win_key != "NO_SUB_KEY" && win_key != "NO_KEY"){
                    if (hiding_mode_wrapper != 1){
                        OS_WinKey_V.Text = win_key; 
                    }else{
                        OS_WinKey_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                    }
                }else{
                    switch (win_key){
                        case "NS_OS":
                            OS_WinKey_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_license_unsupported_os");
                            break;
                        default:
                            OS_WinKey_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_license_null");
                            break;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // WINDOWS LICSENSE TYPE
                StartWindowsLicenseTypeCheck();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // WINDOWS LICENSE TIME
                Task.Run(() => WinLicenseTime());
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // WINDOWS LICENSE URL & VERIFICATION URL
                Task.Run(() => ExtractHostsSLMGR(RunSlmgrDlv(out _, out _))).ContinueWith(t => {
                    if (t.IsFaulted){
                        if (debug_status && t.Exception != null)
                            TSErrorLog.LogException(t.Exception, "Os()");
                        return;
                    }
                    List<string> get_hosts_url = t.Result;
                    this.BeginInvoke((MethodInvoker)delegate {
                        string unknown = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                        string host1 = get_hosts_url.Count > 0 ? get_hosts_url[0].Trim() : unknown;
                        string host2 = get_hosts_url.Count > 1 ? get_hosts_url[1].Trim() : unknown;
                        if (hiding_mode_wrapper != 1){
                            OS_WinLicenseURL_V.Text = host1;
                            OS_WinLicenseVerifiURL_V.Text = host2;
                        }else{
                            string hiddenSuffix = $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                            string hidden1 = get_hosts_url.Count > 0 ? new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + hiddenSuffix : unknown;
                            string hidden2 = get_hosts_url.Count > 1 ? new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + hiddenSuffix : unknown;
                            OS_WinLicenseURL_V.Text = hidden1;
                            OS_WinLicenseVerifiURL_V.Text = hidden2;
                        }
                    });
                }, TaskScheduler.Default);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // .NET FRAMEWORK VERSION
                // https://learn.microsoft.com/tr-tr/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
                using (RegistryKey get_net_version = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\")){
                    if (get_net_version != null){
                        object releaseKey = get_net_version.GetValue("Release");
                        if (releaseKey != null){
                            int releaseKeyValue = (int)releaseKey;
                            var netFrameworkVersions = new Dictionary<int, string>{
                                { 533320, "4.8.1" },
                                { 528040, "4.8" },
                                { 461808, "4.7.2" },
                                { 461308, "4.7.1" },
                                { 460798, "4.7" },
                                { 394802, "4.6.2" },
                                { 394254, "4.6.1" },
                                { 393295, "4.6" },
                                { 379893, "4.5.2" },
                                { 378675, "4.5.1" },
                                { 378389, "4.5" }
                            };
                            string version = software_lang.TSReadLangs("Os_Content", "os_c_net_framework_null");
                            foreach (var kvp in netFrameworkVersions.OrderByDescending(k => k.Key)){
                                if (releaseKeyValue >= kvp.Key){
                                    version = kvp.Value;
                                    break;
                                }
                            }
                            OS_NETFrameworkVersion_V.Text = version;
                        }else{
                            OS_NETFrameworkVersion_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_net_framework_null");
                        }
                    }else{
                        OS_NETFrameworkVersion_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_net_framework_null");
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // OS BLUESCREEN CHECK
                // Check Folder
                if (Directory.Exists(mdp_1)){
                    // Check minidump count
                    int minidump_count = Directory.GetFiles(mdp_1, "*.dmp", SearchOption.AllDirectories).Length;
                    if (minidump_count > 0){
                        string[] minidump_files = Directory.GetFiles(mdp_1, "*.dmp", SearchOption.AllDirectories);
                        for (int i = 0; i <= minidump_files.Length - 1; i++){
                            minidump_files_list.Add(minidump_files[i]);
                        }
                    }
                    // Check memory.dmp file
                    if (File.Exists(mdp_2)){
                        minidump_files_list.Add(mdp_2);
                    }
                }else{
                    // Check memory.dmp file
                    if (File.Exists(mdp_2)){
                        minidump_files_list.Add(mdp_2);
                    }
                }
                // no memory.dmp and no folder
                if (minidump_files_list.Count > 0){
                    if (OS_MinidumpOpen.InvokeRequired){
                        OS_MinidumpOpen.Invoke((MethodInvoker)(() => {
                            OS_MinidumpOpen.Visible = true;
                            OS_BSoDZIP.Visible = true;
                            OS_Minidump_V.Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_detect"), minidump_files_list.Count);
                            bool hasMemoryDump = minidump_files_list.Any(f => string.Equals(Path.GetFileName(f), "memory.dmp", StringComparison.OrdinalIgnoreCase));
                            MainToolTip.SetToolTip(OS_MinidumpOpen, Directory.Exists(mdp_1) ? string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_folder"), mdp_1) : string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_file"), mdp_2) );
                            MainToolTip.SetToolTip(OS_BSoDZIP, software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_folder"));
                        }));
                    }else{
                        OS_MinidumpOpen.Visible = true;
                        OS_BSoDZIP.Visible = true;
                        OS_Minidump_V.Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_detect"), minidump_files_list.Count);
                        MainToolTip.SetToolTip(OS_MinidumpOpen, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_folder"), mdp_1));
                        MainToolTip.SetToolTip(OS_BSoDZIP, software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_folder"));
                    }
                }else{
                    if (OS_MinidumpOpen.InvokeRequired){
                        OS_MinidumpOpen.Invoke((MethodInvoker)(() => {
                            OS_MinidumpOpen.Visible = false;
                            OS_BSoDZIP.Visible = false;
                            OS_Minidump_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect");
                            OS_BSODDate_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect");
                        }));
                    }else{
                        OS_MinidumpOpen.Visible = false;
                        OS_BSoDZIP.Visible = false;
                        OS_Minidump_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect");
                        OS_BSODDate_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect");
                    }
                }
                // Check minidump files count
                if (minidump_files_list.Count > 0){
                    // Get minidump date
                    for (int i = 0; i <= minidump_files_list.Count - 1; i++){
                        DateTime modification = File.GetLastWriteTime(minidump_files_list[i]);
                        minidump_files_date_list.Add(modification.ToString());
                    }
                    // Reverse date
                    minidump_files_date_list.Reverse();
                    // Start dynamic Last BSoD Date
                    StartBsodTimeDynamicCheck();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try{
                // DESKTOP WALLPAPER
                StringBuilder wallpaperPath = new StringBuilder(260);
                bool wallpaperResult = SystemParametersInfo(0x0073, 260, wallpaperPath, 0);
                if (wallpaperResult && File.Exists(wallpaperPath.ToString())){
                    wp_rotate = wallpaperPath.ToString().Trim();
                    string wp_resolution;
                    using (var fs = new FileStream(wp_rotate, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var image = Image.FromStream(fs, false, false)){
                        wp_resolution = $"{image.Width}x{image.Height}";
                    }
                    FileInfo fileInfo = new FileInfo(wp_rotate);
                    string fileName = Path.GetFileName(wp_rotate);
                    string fileExtension = Path.GetExtension(wp_rotate);
                    string formattedSize = TS_FormatSize(Convert.ToDouble(fileInfo.Length));
                    string wallpaperText;
                    if (hiding_mode_wrapper != 1){
                        wallpaperText = $"{fileName} - {wp_resolution} - {formattedSize}";
                    }else{
                        wallpaperText = $"{new string('*', vis_m_property.Next(vn_range[0], vn_range[1]))}{fileExtension} - {wp_resolution} - {formattedSize} ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                    }
                    OS_Wallpaper_V.Invoke((MethodInvoker)(() => {
                        OS_Wallpaper_V.Text = wallpaperText;
                        MainToolTip.SetToolTip(OS_WallpaperOpen, software_lang.TSReadLangs("Os_Content", "os_c_open_wallpaper"));
                        MainToolTip.SetToolTip(OS_WallpaperPreview, software_lang.TSReadLangs("Os_Content", "os_c_preview_wallpaper"));
                    }));
                }else{
                    OS_Wallpaper_V.Invoke((MethodInvoker)(() => {
                        OS_Wallpaper_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_not_detect");
                        OS_WallpaperOpen.Visible = false;
                        OS_WallpaperPreview.Visible = false;
                    }));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            // OS PROCESS END ENABLED
            OS_RotateBtn.Enabled = true;
            ((Control)OS).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Operating System Section Loaded --->");
            }
        }
        private async void OsBgProcess(){
            var software_lang = new TSGetLangs(lang_path);
            var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreeVirtualMemory, TotalVirtualMemorySize, InstallDate, LastBootUpTime FROM Win32_OperatingSystem");
            while (loop_status){
                try{
                    OS_SystemTime_V.Text = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
                    UpdateSystemInfoBG(search_os, software_lang);
                    int msToNextSecond = 1000 - DateTime.Now.Millisecond;
                    if (msToNextSecond <= 0) msToNextSecond = 10;
                    await Task.Delay(msToNextSecond);
                }catch (TaskCanceledException) { break; }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "OsBgProcess()"); }
                }
            }
        }
        private void UpdateSystemInfoBG(ManagementObjectSearcher search_os, TSGetLangs software_lang){
            try{
                string oi_year = software_lang.TSReadLangs("Os_Content", "os_c_year");
                string oi_month = software_lang.TSReadLangs("Os_Content", "os_c_month");
                string oi_day = software_lang.TSReadLangs("Os_Content", "os_c_day");
                string oi_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
                string oi_minute = software_lang.TSReadLangs("Os_Content", "os_c_minute");
                string oi_second = software_lang.TSReadLangs("Os_Content", "os_c_second");
                string oi_ago = software_lang.TSReadLangs("Os_Content", "os_c_ago");
                //
                using (var collection = search_os.Get()){
                    foreach (ManagementObject query_os in collection.Cast<ManagementObject>()){
                        double freeVirtualBytes = 0;
                        double totalVirtualBytes = 0;
                        try{
                            var freeVmObj = query_os["FreeVirtualMemory"];
                            var totalVmObj = query_os["TotalVirtualMemorySize"];
                            if (freeVmObj != null) freeVirtualBytes = Convert.ToDouble(freeVmObj) * 1024d;
                            if (totalVmObj != null) totalVirtualBytes = Convert.ToDouble(totalVmObj) * 1024d;
                            if (totalVirtualBytes < freeVirtualBytes) totalVirtualBytes = freeVirtualBytes;
                        }catch{ }
                        //
                        RAM_EmptyVirtualRam_V.Text = TS_FormatSize(freeVirtualBytes);
                        RAM_UsageVirtualRam_V.Text = TS_FormatSize(Math.Max(0, totalVirtualBytes - freeVirtualBytes));
                        //
                        DateTime now = DateTime.Now;
                        try{
                            var installObj = query_os["InstallDate"];
                            if (installObj != null){
                                DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(installObj.ToString());
                                if (now < osInstallDate){
                                    OS_Install_V.Text = $"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm} - (0 {oi_minute} {oi_ago})";
                                }else{
                                    int totalMonths = (now.Year - osInstallDate.Year) * 12 + (now.Month - osInstallDate.Month);
                                    if (now.Day < osInstallDate.Day) totalMonths--;
                                    if (totalMonths < 0) totalMonths = 0;
                                    int years = totalMonths / 12;
                                    int months = totalMonths % 12;
                                    DateTime anchor = osInstallDate.AddMonths(years * 12 + months);
                                    if (anchor > now){
                                        totalMonths = Math.Max(0, totalMonths - 1);
                                        years = totalMonths / 12;
                                        months = totalMonths % 12;
                                        anchor = osInstallDate.AddMonths(years * 12 + months);
                                    }
                                    TimeSpan rest = now - anchor;
                                    int days = rest.Days;
                                    int hours = rest.Hours;
                                    int minutes = rest.Minutes;
                                    List<string> installParts = new List<string>();
                                    if (years > 0) installParts.Add($"{years} {oi_year}");
                                    if (months > 0) installParts.Add($"{months} {oi_month}");
                                    if (days > 0) installParts.Add($"{days} {oi_day}");
                                    if (hours > 0) installParts.Add($"{hours} {oi_hour}");
                                    if (minutes > 0) installParts.Add($"{minutes} {oi_minute}");
                                    string installDuration = installParts.Count > 0 ? string.Join(", ", installParts) : $"0 {oi_minute}";
                                    OS_Install_V.Text = $"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm} - ({installDuration} {oi_ago})";
                                }
                            }else{
                                OS_Install_V.Text = "-";
                            }
                        }catch{
                            OS_Install_V.Text = "-";
                        }
                        try{
                            var bootObj = query_os["LastBootUpTime"];
                            if (bootObj != null){
                                DateTime bootDate = ManagementDateTimeConverter.ToDateTime(bootObj.ToString());
                                if (now < bootDate){
                                    OS_SystemWorkTime_V.Text = $"0 {oi_second}";
                                }else{
                                    int totalMonths = (now.Year - bootDate.Year) * 12 + (now.Month - bootDate.Month);
                                    if (now.Day < bootDate.Day) totalMonths--;
                                    if (totalMonths < 0) totalMonths = 0;
                                    DateTime anchor = bootDate.AddMonths(totalMonths);
                                    if (anchor > now){
                                        totalMonths = Math.Max(0, totalMonths - 1);
                                        anchor = bootDate.AddMonths(totalMonths);
                                    }
                                    TimeSpan rest = now - anchor;
                                    List<string> uptimeParts = new List<string>();
                                    if (totalMonths > 0) uptimeParts.Add($"{totalMonths} {oi_month}");
                                    if (rest.Days > 0) uptimeParts.Add($"{rest.Days} {oi_day}");
                                    if (rest.Hours > 0) uptimeParts.Add($"{rest.Hours} {oi_hour}");
                                    if (rest.Minutes > 0) uptimeParts.Add($"{rest.Minutes} {oi_minute}");
                                    if (rest.Seconds > 0) uptimeParts.Add($"{rest.Seconds} {oi_second}");
                                    OS_SystemWorkTime_V.Text = uptimeParts.Count > 0 ? string.Join(", ", uptimeParts) : $"0 {oi_second}";
                                }
                            }else{
                                OS_SystemWorkTime_V.Text = $"0 {oi_second}";
                            }
                        }catch{
                            OS_SystemWorkTime_V.Text = $"0 {oi_second}";
                        }
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "UpdateSystemInfoBG()"); }
            }
        }
        // SYSTEM SCREEN CLOSE AND SLEEP TIMER CONVERTER
        private static string SCFormatTimeout(int seconds){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string time_never = software_lang.TSReadLangs("Os_Content", "os_c_a_power_never");
            string time_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
            string time_miniutes = software_lang.TSReadLangs("Os_Content", "os_c_minute");
            //
            if (seconds <= 0) return time_never;
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            string result_timeout = "";
            if (h > 0) result_timeout += $"{h} {time_hour}";
            if (m > 0) result_timeout += (result_timeout != "" ? " " : "") + $"{m} {time_miniutes}";
            return result_timeout;
        }
        // SYSTEM SCREEN CLOSE AND SLEEP GET DATA
        private static int SCQueryTimeout(string planGuid, string subgroupAlias, string settingAlias, bool acPower){
            string cmd = $"/QUERY {planGuid} {subgroupAlias} {settingAlias}";
            string get_time_output;
            using (var getTimeProcess = new Process()){
                getTimeProcess.StartInfo.FileName = "powercfg";
                getTimeProcess.StartInfo.Arguments = cmd;
                getTimeProcess.StartInfo.RedirectStandardOutput = true;
                getTimeProcess.StartInfo.UseShellExecute = false;
                getTimeProcess.StartInfo.CreateNoWindow = true;
                getTimeProcess.Start();
                get_time_output = getTimeProcess.StandardOutput.ReadToEnd();
                getTimeProcess.WaitForExit();
            }
            string searchTime = acPower ? "Current AC Power Setting Index:" : "Current DC Power Setting Index:";
            foreach (var splitLine in get_time_output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)){
                if (splitLine.Contains(searchTime)){
                    string convertTime = splitLine.Split(':')[1].Trim();
                    return Convert.ToInt32(convertTime, 16);
                }
            }
            return -1;
        }
        // GET MS OFFICE VERSION
        private async void StartMsOfficeVersionCheck(){
            try{
                await Ms_office_version();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "StartMsOfficeVersionCheck()"); }
            }
        }
        private async Task Ms_office_version(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string LicensedText = software_lang.TSReadLangs("Os_Content", "os_c_office_licensed");
                string GraceText = software_lang.TSReadLangs("Os_Content", "os_c_office_grace");
                string OtherText = software_lang.TSReadLangs("Os_Content", "os_c_office_other");
                string OfficeNotFoundText = software_lang.TSReadLangs("Os_Content", "os_c_office_not_found");
                var productsList = await Task.Run(() => {
                    var found = new List<(string fullName, string license)>();
                    var baseDirs = new[] {
                        $@"{Program.windows_disk}Program Files\Microsoft Office",
                        $@"{Program.windows_disk}Program Files (x86)\Microsoft Office"
                    };
                    IEnumerable<string> EnumerateDirectoriesSafe(string root, int maxDepth){
                        var dirs = new List<string>();
                        void Recurse(string path, int depth){
                            if (depth > maxDepth) return;
                            try{
                                foreach (var dir in Directory.EnumerateDirectories(path)){
                                    dirs.Add(dir);
                                    Recurse(dir, depth + 1);
                                }
                            }catch (Exception ex){
                                if (debug_status) { TSErrorLog.LogException(ex, "Ms_office_version()"); }
                            }
                        }
                        Recurse(root, 1);
                        return dirs;
                    }
                    foreach (var baseDir in baseDirs){
                        if (!Directory.Exists(baseDir)) continue;
                        try{
                            var dirs = EnumerateDirectoriesSafe(baseDir, 5).ToList();
                            foreach (var dir in dirs){
                                string osppPath = Path.Combine(dir, "ospp.vbs");
                                if (!File.Exists(osppPath)) continue;
                                var psi = new ProcessStartInfo("cscript.exe", $"\"{osppPath}\" /dstatus"){
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                };
                                using (var process = Process.Start(psi)){
                                    if (process == null) continue;
                                    string output = process.StandardOutput.ReadToEnd();
                                    process.WaitForExit();
                                    string currentName = null, currentChannel = null;
                                    foreach (string line in output.Split('\n')){
                                        string readLine = line.Trim();
                                        if (string.IsNullOrEmpty(readLine)) continue;
                                        if (readLine.StartsWith("LICENSE NAME:"))
                                            currentName = readLine.Substring(13).Trim();
                                        else if (readLine.StartsWith("LICENSE DESCRIPTION:")){
                                            string desc = readLine.Substring(20).Trim().ToUpperInvariant();
                                            if (desc.Contains("RETAIL")) currentChannel = "Retail";
                                            else if (desc.Contains("VL")) currentChannel = "Volume";
                                            else if (desc.Contains("OEM")) currentChannel = "OEM";
                                            else currentChannel = OtherText;
                                        }else if (readLine.StartsWith("LICENSE STATUS:") && currentName != null){
                                            string status = readLine.Substring(15).Trim();
                                            string readable = status.Contains("LICENSED") ? LicensedText : status.Contains("OOB_GRACE") ? GraceText : status;
                                            string version = currentName.Split(',')[0].Trim();
                                            found.Add((version, $"{readable} / {currentChannel ?? OtherText}"));
                                            currentName = null;
                                            currentChannel = null;
                                        }
                                    }
                                }
                            }
                        }catch (Exception ex){
                            if (debug_status) { TSErrorLog.LogException(ex, "Ms_office_version()"); }
                        }
                    }
                    return found;
                });
                string displayText;
                if (!productsList.Any()){
                    displayText = OfficeNotFoundText;
                }else{
                    var groupedOffice = productsList.GroupBy(p => p.fullName).Select(g => {
                        string licenseStatus = g.Select(x => x.license).Distinct().FirstOrDefault() ?? OtherText;
                        return $"{g.Key} ({licenseStatus})";
                    });
                    displayText = string.Join(", ", groupedOffice);
                }
                if (IsHandleCreated){
                    BeginInvoke(new Action(() => OS_MSOfficeVersion_V.Text = displayText));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Ms_office_version()"); }
            }
        }
        // WIN LICENSE TYPE
        private async void StartWindowsLicenseTypeCheck(){
            try{
                await Win_license_type();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "StartWindowsLicenseTypeCheck()"); }
            }
        }
        private async Task Win_license_type(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string licenseType = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                await Task.Run(() =>{
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Description, LicenseStatus, PartialProductKey, Name FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                    using (ManagementObjectCollection results = searcher.Get()){
                        foreach (ManagementObject obj in results.Cast<ManagementObject>()){
                            string description = obj["Description"] != null ? obj["Description"].ToString() : string.Empty;
                            string name = obj["Name"] != null ? obj["Name"].ToString() : string.Empty;
                            if (string.IsNullOrWhiteSpace(description))
                                continue;
                            if (!name.Contains("Windows"))
                                continue;
                            if (description.IndexOf("RETAIL", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "Retail";
                            else if (description.IndexOf("OEM_DM", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "OEM: DM";
                            else if (description.IndexOf("OEM_COA", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "OEM: COA";
                            else if (description.IndexOf("OEM", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "OEM";
                            else if (description.IndexOf("VOLUME_MAK", StringComparison.OrdinalIgnoreCase) >= 0 || description.IndexOf("MAK", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "Volume: MAK";
                            else if (description.IndexOf("VOLUME_KMSCLIENT", StringComparison.OrdinalIgnoreCase) >= 0 || description.IndexOf("KMS", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "Volume: KMS";
                            else if (description.IndexOf("GVLK", StringComparison.OrdinalIgnoreCase) >= 0)
                                licenseType = "GVLK";
                            else
                                licenseType = description;
                            break;
                        }
                    }
                });
                if (IsHandleCreated){
                    BeginInvoke(new Action(() => OS_WinActiveChannel_V.Text = licenseType));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Win_license_type()"); }
            }
        }
        // WIN LICENSE TIME
        private void WinLicenseTime(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string oi_year = software_lang.TSReadLangs("Os_Content", "os_c_year");
                string oi_month = software_lang.TSReadLangs("Os_Content", "os_c_month");
                string oi_day = software_lang.TSReadLangs("Os_Content", "os_c_day");
                string oi_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
                string oi_then = software_lang.TSReadLangs("Os_Content", "os_c_then");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LicenseStatus, GracePeriodRemaining, PartialProductKey FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                using (ManagementObjectCollection results = searcher.Get()){
                    foreach (ManagementObject obj in results.Cast<ManagementObject>()){
                        int licenseStatus = obj["LicenseStatus"] != null ? Convert.ToInt32(obj["LicenseStatus"]) : -1;
                        int graceMinutes = obj["GracePeriodRemaining"] != null ? Convert.ToInt32(obj["GracePeriodRemaining"]) : 0;
                        if (licenseStatus != 1){
                            OS_WinLicenseTime_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_not_active");
                            return;
                        }
                        if (graceMinutes == 0){
                            OS_WinLicenseTime_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_permanent");
                            return;
                        }
                        DateTime now = DateTime.UtcNow;
                        DateTime expiry = now.AddMinutes(graceMinutes);
                        int years = expiry.Year - now.Year;
                        int months = expiry.Month - now.Month;
                        int days = expiry.Day - now.Day;
                        int hours = expiry.Hour - now.Hour;
                        if (hours < 0){
                            hours += 24;
                            days--;
                        }
                        if (days < 0){
                            months--;
                            days += DateTime.DaysInMonth(now.Year, now.Month);
                        }
                        if (months < 0){
                            months += 12;
                            years--;
                        }
                        string result = string.Join(" ", new[]{
                            years  > 0 ? $"{years} {oi_year}" : null,
                            months > 0 ? $"{months} {oi_month}" : null,
                            days   > 0 ? $"{days} {oi_day}" : null,
                            hours  > 0 ? $"{hours} {oi_hour}" : null
                        }.Where(x => x != null));
                        if (string.IsNullOrEmpty(result)){
                            result = $"<1 {oi_hour}";
                        }
                        OS_WinLicenseTime_V.Text = $"{result} {oi_then}";
                        return;
                    }
                }
                OS_WinLicenseTime_V.Text = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "WinLicenseTime()"); }
            }
        }
        // BSOD TIME DYNAMIC
        private async void StartBsodTimeDynamicCheck(){
            try{
                await Task.Run(() => Bsod_time_dynamic());
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "StartBsodTimeDynamicCheck()"); }
            }
        }
        private async void Bsod_time_dynamic(){
            try{
                var software_lang = new TSGetLangs(lang_path);
                DateTime last_bsod_x64 = Convert.ToDateTime(minidump_files_date_list[0]);
                while (loop_status){
                    try{
                        DateTime now = DateTime.Now;
                        //
                        int years = now.Year - last_bsod_x64.Year;
                        int months = now.Month - last_bsod_x64.Month;
                        int days = now.Day - last_bsod_x64.Day;
                        int hours = now.Hour - last_bsod_x64.Hour;
                        int minutes = now.Minute - last_bsod_x64.Minute;
                        int seconds = now.Second - last_bsod_x64.Second;
                        //
                        if (seconds < 0) { seconds += 60; minutes--; }
                        if (minutes < 0) { minutes += 60; hours--; }
                        if (hours < 0) { hours += 24; days--; }
                        if (days < 0) { days += DateTime.DaysInMonth(now.AddMonths(-1).Year, now.AddMonths(-1).Month); months--; }
                        if (months < 0) { months += 12; years--; }
                        //
                        List<string> bsodParts = new List<string>();
                        if (years > 0) bsodParts.Add($"{years} {software_lang.TSReadLangs("Os_Content", "os_c_year")}");
                        if (months > 0) bsodParts.Add($"{months} {software_lang.TSReadLangs("Os_Content", "os_c_month")}");
                        if (days > 0) bsodParts.Add($"{days} {software_lang.TSReadLangs("Os_Content", "os_c_day")}");
                        if (hours > 0) bsodParts.Add($"{hours} {software_lang.TSReadLangs("Os_Content", "os_c_hour")}");
                        if (minutes > 0) bsodParts.Add($"{minutes} {software_lang.TSReadLangs("Os_Content", "os_c_minute")}");
                        if (seconds > 0) bsodParts.Add($"{seconds} {software_lang.TSReadLangs("Os_Content", "os_c_second")}");
                        //
                        string agoText = software_lang.TSReadLangs("Os_Content", "os_c_ago");
                        string bsodFinalText = bsodParts.Count > 0 ? $"{string.Join(", ", bsodParts)} {agoText}" : $"0 {software_lang.TSReadLangs("Os_Content", "os_c_second")} {agoText}";
                        //
                        if (IsHandleCreated){
                            BeginInvoke(new Action(() => {
                                OS_BSODDate_V.Text = bsodFinalText;
                            }));
                        }
                        //
                        int msToNextSecond = 1000 - DateTime.Now.Millisecond;
                        if (msToNextSecond <= 0) msToNextSecond = 10;
                        await Task.Delay(msToNextSecond);
                    }catch (TaskCanceledException) { break; }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Bsod_time_dynamic()"); }
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Bsod_time_dynamic()"); }
            }
        }
        // WINDOWS LICENSE & VERIFICATION URL CHECK MODULE
        static string RunSlmgrDlv(out int exitCode, out bool timedOut){
            exitCode = -1;
            timedOut = false;
            Encoding oemEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            string windir = Environment.GetEnvironmentVariable("windir");
            if (string.IsNullOrEmpty(windir))
                windir = Program.windows_disk + "Windows";
            string slmgrPath = windir + @"\System32\slmgr.vbs";
            ProcessStartInfo win_lic_url_listener = new ProcessStartInfo{
                FileName = "cscript.exe",
                Arguments = "//Nologo \"" + slmgrPath + "\" /dlv",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = oemEncoding,
                StandardErrorEncoding = oemEncoding
            };
            try{
                using (Process p = Process.Start(win_lic_url_listener)){
                    if (p == null)
                        return string.Empty;
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(8000)){
                        timedOut = true;
                        try { p.Kill(); } catch{ }
                    }else{
                        exitCode = p.ExitCode;
                    }
                    return (stdout + "\n" + stderr).Trim();
                }
            }catch (Exception ex){
                return ex.ToString();
            }
        }
        static List<string> ExtractHostsSLMGR(string text){
            List<string> result = new List<string>();
            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text))
                return result;
            string workingText = text;
            // Actual URLs
            Regex urlLikeRegex = new Regex(@"\b(?<url>(?:https?://|www\.)[^\s""'<>]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in urlLikeRegex.Matches(workingText)){
                string raw = m.Groups["url"].Value.TrimEnd('.', ',', ';', ':', ')', ']', '}', '>', '"', '\'');
                string normalized = raw.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? "http://" + raw : raw;
                if (Uri.TryCreate(normalized, UriKind.Absolute, out Uri uri)){
                    AddHostSLMGR(uri.Host, unique, result);
                }
            }
            // Clean up so that URL fragments don't match again like the host
            workingText = urlLikeRegex.Replace(workingText, " ");
            // Hostname:port or bare hostname
            Regex hostRegex = new Regex(@"(?<![/\\@\w-])(?<host>(?:www\.)?(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,})(?::(?<port>\d{1,5}))?(?![\w-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in hostRegex.Matches(workingText)){
                string host = m.Groups["host"].Value;
                AddHostSLMGR(host, unique, result);
            }
            // IPv4 Adress
            Regex ipv4Regex = new Regex(@"(?<![\d.])(?<ip>(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(?:\.(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})(?![\d.])", RegexOptions.CultureInvariant);
            foreach (Match m in ipv4Regex.Matches(workingText)){
                AddHostSLMGR(m.Groups["ip"].Value, unique, result);
            }
            return result;
        }
        static void AddHostSLMGR(string host, HashSet<string> unique, List<string> result){
            if (string.IsNullOrWhiteSpace(host))
                return;
            host = host.Trim().TrimEnd('.');
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                host = host.Substring(4);
            if (unique.Add(host)){
                string checkHost = host;
                int colonIndex = checkHost.IndexOf(':');
                if (colonIndex > -1)
                    checkHost = checkHost.Substring(0, colonIndex);
                if (checkHost.IndexOf("microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0){
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    host += string.Format(" ({0})", software_lang.TSReadLangs("Os_Content", "os_c_official_server"));
                }
                result.Add(host);
            }
        }
        // MINIDIUMP FOLDER OPEN
        private void OS_MinidumpOpen_Click(object sender, EventArgs e){
            try{
                // OPEN MINIDUMP FOLDER
                bool hasMemoryDump = minidump_files_list.Any(f => string.Equals(Path.GetFileName(f), "memory.dmp", StringComparison.OrdinalIgnoreCase));
                bool hasMinidumpFolder = Directory.Exists(mdp_1);
                bool hasMemoryDumpFile = File.Exists(mdp_2);
                if (hasMinidumpFolder){
                    Process.Start("explorer.exe", mdp_1);
                }else if (hasMemoryDump && hasMemoryDumpFile){
                    Process.Start("explorer.exe", $"/select,\"{mdp_2}\"");
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OS_MinidumpOpen_Click()"); }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_bsod_folder_open_error"));
            }
        }
        // BSOD ZIP COMPRESS
        private async void OS_BSoDZIP_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                string minidump_target_file = "GlowMinidumpTempFile";
                if (Directory.Exists(minidump_target_file)){
                    Directory.Delete(minidump_target_file, true);
                }
                Directory.CreateDirectory(minidump_target_file);
                string minidump_zip_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Application.ProductName + "_Minidump_" + Dns.GetHostName() + "_" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + ".zip");
                // COPY ASYNC
                int currentFile = 0;
                Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_progress_copy"), TS_VersionEngine.TS_SoftwareVersion(0));
                foreach (string file_path in minidump_files_list){
                    string file_name = Path.GetFileName(file_path);
                    string target_file_path = Path.Combine(minidump_target_file, file_name);
                    await Task.Run(() => File.Copy(file_path, target_file_path, true));
                    currentFile++;
                }
                // ZIP ASYNC
                Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_progress_compress"), TS_VersionEngine.TS_SoftwareVersion(0));
                await Task.Run(() => {
                    using (FileStream zipToOpen = new FileStream(minidump_zip_file, FileMode.Create)){
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create)){
                            currentFile = 0;
                            foreach (string file_path in minidump_files_list){
                                string file_name = Path.GetFileName(file_path);
                                string target_file_path = Path.Combine(minidump_target_file, file_name);
                                ZipArchiveEntry zip_entry = archive.CreateEntry(file_name, CompressionLevel.NoCompression);
                                using (var entryStream = zip_entry.Open())
                                using (var fileStream = new FileStream(target_file_path, FileMode.Open, FileAccess.Read)){
                                    fileStream.CopyTo(entryStream);
                                }
                                currentFile++;
                            }
                        }
                    }
                });
                if (Directory.Exists(minidump_target_file)){
                    Directory.Delete(minidump_target_file, true);
                }
                Text = TS_VersionEngine.TS_SoftwareVersion(0);
                DialogResult open_minidump_zip_target = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_success"), minidump_zip_file, "\n\n"));
                if (open_minidump_zip_target == DialogResult.Yes){
                    string open_mdzt = string.Format("/select, \"{0}\"", minidump_zip_file.Trim().Replace("/", @"\"));
                    ProcessStartInfo psi = new ProcessStartInfo("explorer.exe", open_mdzt);
                    Process.Start(psi);
                }
            }catch (Exception ex){
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_error"), "\n", ex.Message));
            }
        }
        // OPEN WALLPAPER
        private void OS_WallpaperOpen_Click(object sender, EventArgs e){
            try{
                string wallpaper_start_path = string.Format("/select, \"{0}\"", wp_rotate.Trim().Replace("/", @"\"));
                ProcessStartInfo psi = new ProcessStartInfo("explorer.exe", wallpaper_start_path);
                Process.Start(psi);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OS_WallpaperOpen_Click()"); }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_open_error"));
            }
        }
        // OPEN WALLPAPER PREVIEW
        private void OS_WallpaperPreview_Click(object sender, EventArgs e){
            TSToolLauncher<GlowWallpaperPreviewTool>("glow_wallpaper_preview_tool", "wpt_name");
        }
        // OPEN WINDOWS RECOVERY MODE
        private void OS_RecoveryModeBtn_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                DialogResult recovery_check = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_recovery_mode_msg"), "\n\n", "\n\n"));
                if (recovery_check == DialogResult.Yes){
                    Process.Start(new ProcessStartInfo{
                        FileName = "shutdown",
                        Arguments = "/r /o /f /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OS_RecoveryModeBtn_Click()"); }
            }
        }
        #endregion
        #region MB_Section
        // MB
        // ======================================================================================================
        private void Mb(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            ManagementObjectSearcher search_bb = new ManagementObjectSearcher("root\\CIMV2", "SELECT Product, Manufacturer, SerialNumber FROM Win32_BaseBoard");
            ManagementObjectSearcher search_cs = new ManagementObjectSearcher("root\\CIMV2", "SELECT SystemFamily, SystemSKUNumber FROM Win32_ComputerSystem");
            ManagementObjectSearcher search_bios = new ManagementObjectSearcher("root\\CIMV2", "SELECT Manufacturer, ReleaseDate, Caption, SMBIOSMajorVersion, SMBIOSMinorVersion, SerialNumber FROM Win32_BIOS");
            ManagementObjectSearcher search_tpm = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftTpm", "SELECT IsActivated_InitialValue, SpecVersion, PhysicalPresenceVersionInfo, ManufacturerIdTxt, ManufacturerId, ManufacturerVersion, ManufacturerVersionFull20, ManufacturerVersionInfo FROM Win32_Tpm");
            foreach (ManagementObject query_bb_rotate in search_bb.Get().Cast<ManagementObject>()){
                try{
                    // MB NAME
                    MB_MotherBoardName_V.Text = Convert.ToString(query_bb_rotate["Product"]);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // MB MAN
                    MB_MotherBoardMan_V.Text = Convert.ToString(query_bb_rotate["Manufacturer"]);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // MB SERIAL
                    string mb_serial = Convert.ToString(query_bb_rotate["SerialNumber"]).Trim();
                    if (!string.IsNullOrEmpty(mb_serial)){
                        if (hiding_mode_wrapper != 1){
                            MB_MotherBoardSerial_V.Text = mb_serial;
                        }else{
                            MB_MotherBoardSerial_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        MB_MotherBoardSerial_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
            }
            try{
                RegistryKey get_bios_data = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                if (!string.IsNullOrEmpty(get_bios_data.ToString())){
                    // SYSTEM MODEL MANUFACTURER
                    try{
                        string systemManufacturer = Convert.ToString(get_bios_data.GetValue("SystemManufacturer"));
                        if (!string.IsNullOrEmpty(systemManufacturer)){
                            MB_SystemModelMan_V.Text = systemManufacturer.ToString().Trim();
                        }else{
                            MB_SystemModelMan_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                    }
                    // SYSTEM MODEL FAMILY
                    try{
                        string systemFamily = Convert.ToString(get_bios_data.GetValue("SystemFamily"));
                        if (!string.IsNullOrEmpty(systemFamily)){
                            MB_SystemModelFamily_V.Text = systemFamily.ToString().Trim();
                        }else{
                            MB_SystemModelFamily_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                    }
                    // SYSTEM MODEL
                    try{
                        string systemProductName = Convert.ToString(get_bios_data.GetValue("SystemProductName"));
                        if (!string.IsNullOrEmpty(systemProductName)){
                            MB_SystemModel_V.Text = systemProductName.ToString().Trim();
                        }else{
                            MB_SystemModel_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                    }
                }else{
                    MB_SystemModelMan_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    MB_SystemModelFamily_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    MB_SystemModel_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                }
                get_bios_data.Close();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            foreach (ManagementObject query_cs in search_cs.Get().Cast<ManagementObject>()){
                // SYSTEM FAMILY
                try{
                    string system_family = Convert.ToString(query_cs["SystemFamily"]).Trim();
                    string system_family_check = system_family.ToLower();
                    if (system_family_check == "default string" || system_family_check == "to be filled by o.e.m."){
                        MB_SystemFamily_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    }else{
                        MB_SystemFamily_V.Text = system_family;
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                // SYSTEM SKU
                try{
                    string system_sku = Convert.ToString(query_cs["SystemSKUNumber"]).Trim();
                    string system_sku_check = system_sku.ToLower();
                    if (system_sku_check != "default string" && system_sku_check != "sku"){
                        if (hiding_mode_wrapper != 1){
                            MB_SystemSKU_V.Text = system_sku;
                        }else{
                            MB_SystemSKU_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        MB_SystemSKU_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
            }
            foreach (ManagementObject query_bios_rotate in search_bios.Get().Cast<ManagementObject>()){
                try{
                    // BIOS MAN
                    MB_BiosManufacturer_V.Text = Convert.ToString(query_bios_rotate["Manufacturer"]);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // BIOS DATE
                    string bios_date = Convert.ToString(query_bios_rotate["ReleaseDate"]);
                    DateTime bios_date_last = ManagementDateTimeConverter.ToDateTime(bios_date);
                    MB_BiosDate_V.Text = $"{bios_date_last:dd.MM.yyyy}";
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // BIOS VERSION
                    MB_BiosVersion_V.Text = Convert.ToString(query_bios_rotate["Caption"]);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // SM-BIOS MAJOR MINOR
                    object sm_bios_major = query_bios_rotate["SMBIOSMajorVersion"];
                    object sm_bios_minor = query_bios_rotate["SMBIOSMinorVersion"];
                    MB_SmBiosVersion_V.Text = sm_bios_major.ToString() + "." + sm_bios_minor.ToString();
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // BIOS SERIAL NUMBER
                    string mb_device_serial = Convert.ToString(query_bios_rotate["SerialNumber"]);
                    if (!string.IsNullOrEmpty(mb_device_serial)){
                        if (hiding_mode_wrapper != 1){
                            MB_DeviceSerialNumber_V.Text = mb_device_serial;
                        }else{
                            MB_DeviceSerialNumber_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        MB_DeviceSerialNumber_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
            }
            try{
                // MB SECURE BOOT
                bool mb_secure_boot_status = Convert.ToBoolean(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State", "UEFISecureBootEnabled", ""));
                if (mb_secure_boot_status == true){
                    MB_SecureBoot_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_on");
                }else if (mb_secure_boot_status == false){
                    MB_SecureBoot_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_off");
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            foreach (ManagementObject query_tpm in search_tpm.Get().Cast<ManagementObject>()){
                try{
                    // TPM VERSION
                    bool tpm_status = Convert.ToBoolean(query_tpm["IsActivated_InitialValue"]);
                    string tpm_version = Convert.ToString(query_tpm["SpecVersion"]) ?? "";
                    if (tpm_status == true && !string.IsNullOrEmpty(tpm_version)){
                        MB_TPMStatus_V.Text = string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_active"), tpm_version.Split(',')[0].Trim());
                    }else{
                        MB_TPMStatus_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM PPI VERSION
                    string tpm_ppi_version = Convert.ToString(query_tpm["PhysicalPresenceVersionInfo"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_ppi_version)){
                        MB_TPMPPIVersion_V.Text = tpm_ppi_version;
                    }else{
                        MB_TPMPPIVersion_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM PPI ALT VERSION
                    bool tpm_status = Convert.ToBoolean(query_tpm["IsActivated_InitialValue"]);
                    string tpm_version = Convert.ToString(query_tpm["SpecVersion"]) ?? "";
                    if (tpm_status == true && !string.IsNullOrEmpty(tpm_version)){
                        string[] parts = tpm_version.Split(',');
                        if (parts.Length >= 3){
                            MB_TPMPPIAltVersion_V.Text = parts[2].Trim();
                        }else{
                            MB_TPMPPIAltVersion_V.Text = "N/A";
                        }
                    }else{
                        MB_TPMPPIAltVersion_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM MANUFACTURER ID TXT
                    string tpm_man_id_txt = Convert.ToString(query_tpm["ManufacturerIdTxt"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_man_id_txt)){
                        if (tpm_man_id_txt == "INTC"){
                            MB_TPMMan_V.Text = string.Format("Intel ({0})", tpm_man_id_txt);
                        }else{
                            MB_TPMMan_V.Text = tpm_man_id_txt;
                        }
                    }else{
                        MB_TPMMan_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM MANUFACTURER ID
                    string tpm_man_id = Convert.ToString(query_tpm["ManufacturerId"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_man_id)){
                        if (hiding_mode_wrapper != 1){
                            MB_TPMManID_V.Text = tpm_man_id;
                        }else{
                            MB_TPMManID_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                        }
                    }else{
                        MB_TPMManID_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM MANUFACTURER VERSION
                    string tpm_man_version = Convert.ToString(query_tpm["ManufacturerVersion"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_man_version)){
                        MB_TPMManVersion_V.Text = tpm_man_version;
                    }else{
                        MB_TPMManVersion_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM MANUFACTURER VERSION FULL
                    string tpm_man_version_full = Convert.ToString(query_tpm["ManufacturerVersionFull20"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_man_version_full)){
                        MB_TPMManFullVersion_V.Text = tpm_man_version_full;
                    }else{
                        MB_TPMManFullVersion_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
                try{
                    // TPM MANUFACTURER INFO
                    string tpm_man_version_info = Convert.ToString(query_tpm["ManufacturerVersionInfo"]).Trim();
                    if (!string.IsNullOrEmpty(tpm_man_version_info)){
                        MB_TPMManPublisher_V.Text = tpm_man_version_info;
                    }else{
                        MB_TPMManPublisher_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                }
            }
            // TPM MODE CHECK WRAPPER
            try{
                string tpmOffMessage = software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off");
                var tpm_controls = new Control[] {
                    MB_TPMStatus_V,
                    MB_TPMPPIVersion_V,
                    MB_TPMPPIAltVersion_V,
                    MB_TPMMan_V,
                    MB_TPMManID_V,
                    MB_TPMManVersion_V,
                    MB_TPMManFullVersion_V,
                    MB_TPMManPublisher_V
                };
                foreach (var tpm_control in tpm_controls){
                    if (tpm_control.Text == "N/A" || string.IsNullOrEmpty(tpm_control.Text)){
                        tpm_control.Text = tpmOffMessage;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            try{
                // LAST BIOS TIME | POSTTime
                int last_bios_time = Convert.ToInt32(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "FwPOSTTime", ""));
                string last_bios_time_x64 = string.Format("{0:0.0}", TimeSpan.FromMilliseconds(last_bios_time).TotalSeconds).Replace(",", ".");
                MB_LastBIOSTime_V.Text = string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_boot_time"), last_bios_time_x64);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            // MB PROCESS END ENABLED
            MB_RotateBtn.Enabled = true;
            ((Control)MB).Enabled = true;
            MB_BIOSUpdateBtn.Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Motherboard Section Loaded --->");
            }
        }
        private void MB_BIOSUpdateBtn_Click(object sender, EventArgs e){
            try{
                string bios_prompt = "";
                if (laptop_mode == true){
                    bios_prompt = string.Format("{0} {1} {2}", MB_SystemModelMan_V.Text.Trim(), MB_SystemModel_V.Text.Trim(), "BIOS Update");
                }else if (laptop_mode == false){
                    bios_prompt = string.Format("{0} {1} {2}", MB_MotherBoardMan_V.Text.Trim(), MB_MotherBoardName_V.Text.Trim(), "BIOS Update");
                }
                string search_browser_q = $"https://www.google.com/search?q={Uri.EscapeDataString(bios_prompt)}";
                Process.Start(new ProcessStartInfo(search_browser_q) { UseShellExecute = true });
            }catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "MB_BIOSUpdateBtn_Click()"); } }
        }
        private void MB_BIOSOpenBtn_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (MB_BiosMode_V.Text == "N/A"){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_not_set"));
                    return;
                }
                if (!MB_BiosMode_V.Text.Equals("UEFI", StringComparison.OrdinalIgnoreCase)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_uefi_check"));
                    return;
                }
                DialogResult check_bios_open = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_check"), "\n\n", "\n\n"));
                if (check_bios_open == DialogResult.Yes){
                    Process.Start(new ProcessStartInfo{
                        FileName = "shutdown",
                        Arguments = "/r /fw /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "MB_BIOSOpenBtn_Click()"); }
            }
        }
        #endregion
        #region CPU_Section
        // CPU
        // ======================================================================================================
        readonly List<string> cpu_man_list = new List<string>();
        readonly List<string> cpu_arch_list = new List<string>();
        readonly List<string> cpu_speed_list = new List<string>();
        readonly List<string> cpu_core_list = new List<string>();
        readonly List<string> cpu_active_core_list = new List<string>();
        readonly List<string> cpu_logical_core_list = new List<string>();
        readonly List<string> cpu_l1_size_list = new List<string>();
        readonly List<string> cpu_l2_size_list = new List<string>();
        readonly List<string> cpu_l3_size_list = new List<string>();
        readonly List<string> cpu_unique_id_list = new List<string>();
        readonly List<string> cpu_family_list = new List<string>();
        readonly List<string> cpu_slot_list = new List<string>();
        readonly List<string> cpu_socket_list = new List<string>();
        private void Cpu(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            ManagementObjectSearcher search_process = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, Manufacturer, Architecture, MaxClockSpeed, NumberOfCores, NumberOfEnabledCore, ThreadCount, DeviceID, L2CacheSize, L3CacheSize, ProcessorId, Description, SocketDesignation, VirtualizationFirmwareEnabled FROM Win32_Processor");
            ManagementObjectSearcher search_cs = new ManagementObjectSearcher("root\\CIMV2", "SELECT HypervisorPresent FROM Win32_ComputerSystem");
            foreach (ManagementObject query_process_rotate in search_process.Get().Cast<ManagementObject>()){
                try{
                    // CPU NAME
                    string cpu_name = Convert.ToString(query_process_rotate["Name"]).Trim();
                    CPU_Selector_List.Items.Add(cpu_name);
                }catch (Exception ex) {
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU MANUFACTURER AND CHIPSET CHECK
                    string cpuManufacturer = Convert.ToString(query_process_rotate["Manufacturer"])?.Trim() ?? string.Empty;
                    string displayManufacturer;
                    string mbChipset;
                    if (cpuManufacturer.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0){
                        displayManufacturer = "Intel Corporation";
                        mbChipset = "Intel";
                    }else if (cpuManufacturer.IndexOf("Advanced Micro", StringComparison.OrdinalIgnoreCase) >= 0 || cpuManufacturer.IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0){
                        displayManufacturer = "Advanced Micro Devices, Inc.";
                        mbChipset = "AMD";
                    }else if (cpuManufacturer.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0){
                        displayManufacturer = "NVIDIA Corporation";
                        mbChipset = "NVIDIA";
                    }else if (cpuManufacturer.IndexOf("Apple", StringComparison.OrdinalIgnoreCase) >= 0){
                        displayManufacturer = "Apple Inc.";
                        mbChipset = "Apple";
                    }else{
                        displayManufacturer = cpuManufacturer;
                        mbChipset = cpuManufacturer;
                    }
                    cpu_man_list.Add(displayManufacturer);
                    CPU_Manufacturer_V.Text = displayManufacturer;
                    MB_Chipset_V.Text = mbChipset;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU ARCHITECTURE
                    int arch = Convert.ToInt32(query_process_rotate["Architecture"]);
                    string bitText = software_lang.TSReadLangs("Cpu_Content", "cpu_c_bit");
                    var archMap = new Dictionary<int, string>{
                        { 0, "32 " + bitText + " - (x86)" },
                        { 1, "MIPS" },
                        { 2, "Alpha" },
                        { 3, "PowerPC" },
                        { 5, "ARM32" },
                        { 6, "ia64 (Itanium)" },
                        { 9, "64 " + bitText + " - (x64)" },
                        { 12, "64 " + bitText + " - (ARM64)" }
                    };
                    string archText = archMap.ContainsKey(arch) ? archMap[arch] : $"Unknown ({arch})";
                    cpu_arch_list.Add(archText);
                    CPU_Architectural_V.Text = archText;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU SPEED
                    double cpu_max_speed = Convert.ToDouble(query_process_rotate["MaxClockSpeed"]);
                    if (cpu_max_speed > 1024){
                        cpu_speed_list.Add(string.Format("{0:0.00} GHz", cpu_max_speed / 1000));
                    }else{
                        cpu_speed_list.Add(cpu_max_speed.ToString() + " MHz");
                    }
                    CPU_Speed_V.Text = cpu_speed_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU CORES
                    string core_count = Convert.ToString(query_process_rotate["NumberOfCores"]);
                    cpu_core_list.Add(core_count);
                    CPU_CoreCount_V.Text = cpu_core_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU ACTIVE CORES
                    string active_core_count = Convert.ToString(query_process_rotate["NumberOfEnabledCore"]);
                    cpu_active_core_list.Add(active_core_count);
                    CPU_ActiveCoreCount_V.Text = cpu_active_core_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU LOGICAL CORES
                    string thread_count = Convert.ToString(query_process_rotate["ThreadCount"]);
                    string __thread_count = "";
                    if (!string.IsNullOrEmpty(thread_count)){
                        __thread_count = thread_count;
                    }else{
                        __thread_count = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                    }
                    cpu_logical_core_list.Add(__thread_count);
                    CPU_LogicalCore_V.Text = cpu_logical_core_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // L1 - L2 - L3 CACHE
                    string procId = query_process_rotate["DeviceID"].ToString(); // CPU0
                    string cleanId = new string(procId.Where(char.IsDigit).ToArray());
                    cpu_slot_list.Add(cleanId.Trim());
                    CPU_Slot_V.Text = cpu_slot_list[0];
                    //
                    double cpu_l1_total = 0;
                    double cpu_l2_total = 0;
                    double cpu_l3_total = 0;
                    //
                    var l3_candidates = new List<double>();
                    //
                    using (ManagementObjectSearcher cacheSearcher = new ManagementObjectSearcher("root\\CIMV2", $"ASSOCIATORS OF {{Win32_Processor.DeviceID='{procId}'}} WHERE ResultClass = Win32_CacheMemory")){
                        foreach (ManagementObject cache in cacheSearcher.Get().Cast<ManagementObject>()){
                            if (cache["Level"] == null || cache["MaxCacheSize"] == null)
                                continue;
                            if (!int.TryParse(cache["Level"].ToString(), out int level))
                                continue;
                            if (!double.TryParse(cache["MaxCacheSize"].ToString(), out double kb))
                                continue;
                            double bytes = kb * 1024; // KB to bytes
                            switch (level){
                                case 3: cpu_l1_total += bytes; break; // Primary
                                case 4: cpu_l2_total += bytes; break; // Secondary
                                case 5: l3_candidates.Add(bytes); break;      // Tertiary (fallback)
                            }
                        }
                    }
                    double procL3Bytes = 0;
                    if (query_process_rotate["L3CacheSize"] != null && double.TryParse(query_process_rotate["L3CacheSize"].ToString(), out double l3Kb) && l3Kb > 0){
                        procL3Bytes = l3Kb * 1024;
                    }
                    if (procL3Bytes > 0){
                        cpu_l3_total = procL3Bytes;
                    }else{
                        if (l3_candidates.Count == 1){
                            cpu_l3_total = l3_candidates[0];
                        }else if (l3_candidates.Count > 1){
                            bool allSame = l3_candidates.All(x => Math.Abs(x - l3_candidates[0]) < 1);
                            cpu_l3_total = allSame ? l3_candidates.Max() : l3_candidates.Sum();
                        }
                    }
                    //
                    cpu_l1_size_list.Add(TS_FormatSize(cpu_l1_total));
                    CPU_L1_V.Text = cpu_l1_size_list[0];
                    //
                    cpu_l2_size_list.Add(TS_FormatSize(cpu_l2_total));
                    CPU_L2_V.Text = cpu_l2_size_list[0];
                    //
                    cpu_l3_size_list.Add(TS_FormatSize(cpu_l3_total));
                    CPU_L3_V.Text = cpu_l3_size_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU SERIAL ID
                    string cpu_serial = Convert.ToString(query_process_rotate["ProcessorId"]).Trim();
                    if (hiding_mode_wrapper != 1){
                        cpu_unique_id_list.Add(cpu_serial);
                    }else{
                        cpu_unique_id_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                    }
                    CPU_SerialName_V.Text = cpu_unique_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU FAMILY
                    string cpu_description = Convert.ToString(query_process_rotate["Description"]);
                    string cpu_tanim = cpu_description.Replace("Family", software_lang.TSReadLangs("Cpu_Content", "cpu_c_family"));
                    string cpu_tanim_2 = cpu_tanim.Replace("Model", software_lang.TSReadLangs("Cpu_Content", "cpu_c_model"));
                    string cpu_tanim_3 = cpu_tanim_2.Replace("Stepping", software_lang.TSReadLangs("Cpu_Content", "cpu_c_stage"));
                    string cpu_tanim_4 = cpu_tanim_3.Replace("64", " X64");
                    cpu_family_list.Add(cpu_tanim_4);
                    CPU_Family_V.Text = cpu_family_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU SOCKET
                    string cpu_socket_definition = Convert.ToString(query_process_rotate["SocketDesignation"]);
                    cpu_socket_list.Add(cpu_socket_definition);
                    CPU_SocketDefinition_V.Text = cpu_socket_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
                try{
                    // CPU VIRTUALIZATION / W10
                    if (Program.windows_mode == 0){
                        bool cpu_virtual_mod = Convert.ToBoolean(query_process_rotate["VirtualizationFirmwareEnabled"]);
                        if (cpu_virtual_mod == true){
                            CPU_Virtualization_V.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_active");
                        }else if (cpu_virtual_mod == false){
                            CPU_Virtualization_V.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_disabled");
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                }
            }
            try{
                // CPU VIRTUALIZATION / W11
                if (Program.windows_mode == 1){
                    foreach (ManagementObject query_hv in search_cs.Get().Cast<ManagementObject>()){
                        bool cpu_virtual_mod = Convert.ToBoolean(query_hv["HypervisorPresent"]);
                        if (cpu_virtual_mod == true){
                            CPU_Virtualization_V.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_active");
                        }else if (cpu_virtual_mod == false){
                            CPU_Virtualization_V.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_disabled");
                        }
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            // Test Data
            // ---------------------
            if (debug_status && Program.glow_console_debug_mode){
                CPU_Selector_List.Items.Add("AMD Ryzen 9 7950X3D");
                cpu_man_list.Add("AMD");
                cpu_arch_list.Add("x64 (64 Bit)");
                cpu_speed_list.Add("4.2 GHz");
                cpu_core_list.Add("16");
                cpu_active_core_list.Add("16");
                cpu_logical_core_list.Add("32");
                cpu_l1_size_list.Add("1 MB");
                cpu_l2_size_list.Add("16 MB");
                cpu_l3_size_list.Add("128 MB");
                cpu_unique_id_list.Add("AMD64_F19h_7950X3D");
                cpu_family_list.Add("Family 19h");
                cpu_slot_list.Add("1");
                cpu_socket_list.Add("AM5");
            }
            // ---------------------
            try{
                // AUTO SELECT SLOT 0
                if (CPU_Selector_List.Items.Count > 0){
                    CPU_Selector_List.SelectedIndex = 0;
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            // CPU PROCESS END ENABLED
            CPU_RotateBtn.Enabled = true;
            ((Control)CPU).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Processor Section Loaded --->");
            }
        }
        private async void ProcessorBgProcess(){
            try{
                var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT NumberOfProcesses FROM Win32_OperatingSystem");
                while (loop_status){
                    DateTime startTime = DateTime.Now;
                    long processCount = 0;
                    long totalThreads = 0;
                    long totalHandles = 0;
                    try{
                        using (var collection = searcher.Get()){
                            processCount = collection.Cast<ManagementObject>().Select(mo => Convert.ToInt64(mo["NumberOfProcesses"])).FirstOrDefault();
                        }
                        var allProcesses = Process.GetProcesses();
                        foreach (Process proc in allProcesses){
                            try{
                                totalThreads += proc.Threads.Count;
                                totalHandles += proc.HandleCount;
                            }catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); } }
                            finally { proc.Dispose(); }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); }
                    }
                    if (IsHandleCreated){
                        CPU_Process_V.Text = processCount.ToString("N0");
                        CPU_Threads_V.Text = totalThreads.ToString("N0");
                        CPU_Handles_V.Text = totalHandles.ToString("N0");
                    }
                    int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    int nextDelay = Math.Max(10, 1000 - elapsed);
                    try{
                        await Task.Delay(nextDelay);
                    }catch (TaskCanceledException){
                        break;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); }
            }
        }
        // CPU ENGINE
        // ======================================================================================================
        private async void CpuBgProcess(){
            PerformanceCounter cpuCounter = null;
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                if (IsHandleCreated){
                    CPU_Usage_V.Text = software_lang.TSReadLangs("Cpu_Content", "cpu_c_loading");
                }
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
                while (loop_status){
                    DateTime startTime = DateTime.Now;
                    float cpuUsage = 0f;
                    try{
                        cpuUsage = cpuCounter.NextValue();
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                        cpuUsage = 0f;
                    }
                    if (IsHandleCreated){
                        CPU_Usage_V.Text = string.Format("{0:F1}%", cpuUsage);
                    }
                    int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    int nextDelay = Math.Max(10, 1000 - elapsed);
                    try{
                        await Task.Delay(nextDelay);
                    }catch (TaskCanceledException){
                        break;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
            }finally{
                cpuCounter?.Dispose();
            }
        }
        private void CPU_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int cpu_slot = CPU_Selector_List.SelectedIndex;
                try { CPU_Manufacturer_V.Text = cpu_man_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Architectural_V.Text = cpu_arch_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Speed_V.Text = cpu_speed_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_CoreCount_V.Text = cpu_core_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_ActiveCoreCount_V.Text = cpu_active_core_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_LogicalCore_V.Text = cpu_logical_core_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_L1_V.Text = cpu_l1_size_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_L2_V.Text = cpu_l2_size_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_L3_V.Text = cpu_l3_size_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_SerialName_V.Text = cpu_unique_id_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Family_V.Text = cpu_family_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Slot_V.Text = cpu_slot_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_SocketDefinition_V.Text = cpu_socket_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
        }
        #endregion
        #region RAM_Section
        // RAM
        // ======================================================================================================
        readonly List<string> ram_slot_count = new List<string>();
        readonly List<string> ram_slot_list = new List<string>();
        readonly List<string> ram_amount_list = new List<string>();
        readonly List<string> ram_type_list = new List<string>();
        readonly List<string> ram_frekans_list = new List<string>();
        readonly List<string> ram_voltage_list = new List<string>();
        readonly List<string> ram_form_factor = new List<string>();
        readonly List<string> ram_serial_list = new List<string>();
        readonly List<string> ram_manufacturer_list = new List<string>();
        readonly List<string> ram_bank_label_list = new List<string>();
        readonly List<string> ram_data_width_list = new List<string>();
        readonly List<string> bellek_type_list = new List<string>();
        readonly List<string> ram_part_number_list = new List<string>();
        private void Ram(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            ManagementObjectSearcher search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, TotalVirtualMemorySize FROM Win32_OperatingSystem");
            ManagementObjectSearcher search_pm = new ManagementObjectSearcher("root\\CIMV2", "SELECT BankLabel, Capacity, SMBIOSMemoryType, MemoryType, Speed, ConfiguredVoltage, FormFactor, SerialNumber, Manufacturer, TotalWidth, DeviceLocator, PartNumber FROM Win32_PhysicalMemory");
            //
            foreach (ManagementObject query_os_rotate in search_os.Get().Cast<ManagementObject>()){
                try{
                    // TOTAL RAM
                    double total_bytes = Convert.ToDouble(query_os_rotate["TotalVisibleMemorySize"]) * 1024;
                    //
                    string usableStr = TS_FormatSize(total_bytes);
                    double totalGB = total_bytes / (1024 * 1024 * 1024);
                    double installedGBValue = Math.Pow(2, Math.Round(Math.Log(totalGB, 2)));
                    double installedBytes = installedGBValue * 1024 * 1024 * 1024;
                    string installedStr = TS_FormatSize(installedBytes);
                    //
                    string avabilable_string = software_lang.TSReadLangs("Ram_Content", "ram_c_available");
                    RAM_TotalRAM_V.Text = string.Format("{0} ({1} {2})", installedStr, avabilable_string, usableStr);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // TOTAL VIRTUAL RAM
                    double total_virtual_ram = Convert.ToDouble(query_os_rotate["TotalVirtualMemorySize"]) * 1024;
                    RAM_TotalVirtualRam_V.Text = TS_FormatSize(total_virtual_ram);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
            }
            foreach (ManagementObject queryObj in search_pm.Get().Cast<ManagementObject>()){
                try{
                    // RAM AMOUNT
                    string ram_count = Convert.ToString(queryObj["BankLabel"]);
                    if (string.IsNullOrEmpty(ram_count)){
                        ram_slot_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_no_bank_label"));
                    }else{
                        ram_slot_list.Add(ram_count);
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM SLOT COUNT
                    ram_slot_count.Add(Convert.ToString(queryObj["Capacity"]));
                    RAM_SlotStatus_V.Text = ram_slot_count.Count + " " + software_lang.TSReadLangs("Ram_Content", "ram_c_slot_count");
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM CAPACITY
                    double ram_amount = Convert.ToDouble(queryObj["Capacity"]);
                    ram_amount_list.Add(TS_FormatSize(ram_amount));
                    RAM_Amount_V.Text = ram_amount_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // MEMORY TYPE
                    // https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory
                    int sm_bios_memory_type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]);
                    int memory_type = Convert.ToInt32(queryObj["MemoryType"]);
                    var memoryTypes = new Dictionary<int, string>{
                        { 0, software_lang.TSReadLangs("Ram_Content", "ram_c_unknown") },
                        { 1, software_lang.TSReadLangs("Ram_Content", "ram_c_other_memory_type") },
                        { 2, "DRAM" },
                        { 3, "Synchronous DRAM" },
                        { 4, "Cache Ram" },
                        { 5, "EDO" },
                        { 6, "EDRAM" },
                        { 7, "VRAM" },
                        { 8, "SRAM" },
                        { 9, "RAM" },
                        { 10, "ROM" },
                        { 11, "FLASH" },
                        { 12, "EEPROM" },
                        { 13, "FEPROM" },
                        { 14, "EPROM" },
                        { 15, "CDRAM" },
                        { 16, "3DRAM" },
                        { 17, "SDRAM" },
                        { 18, "SGRAM" },
                        { 19, "RDRAM" },
                        { 20, "DDR" },
                        { 21, "DDR2" },
                        { 22, "DDR2 FB-DIMM" },
                        { 24, "DDR3" },
                        { 25, "FBD2" },
                        { 26, "DDR4" },
                        { 34, "DDR5" }
                    };
                    if (memoryTypes.TryGetValue(sm_bios_memory_type, out string ramTypeMessage) || memoryTypes.TryGetValue(memory_type, out ramTypeMessage)){
                        ram_type_list.Add(ramTypeMessage);
                    }else{
                        ram_type_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }
                    RAM_Type_V.Text = ram_type_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM SPEED
                    double ram_speed = Convert.ToInt32(queryObj["Speed"]);
                    ram_frekans_list.Add(string.Format("{0} MT/s ({1} MHz)", ram_speed, ram_speed / 2));
                    RAM_Frequency_V.Text = ram_frekans_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM VOLTAGE
                    string ramVoltStr = queryObj["ConfiguredVoltage"]?.ToString();
                    if (string.IsNullOrEmpty(ramVoltStr) || !double.TryParse(ramVoltStr, out double ramVolt) || ramVolt == 0){
                        ram_voltage_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }else{
                        ram_voltage_list.Add(string.Format("{0:0.00} " + software_lang.TSReadLangs("Ram_Content", "ram_c_voltage"), ramVolt / 1000.0));
                    }
                    RAM_Volt_V.Text = ram_voltage_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // FORM FACTOR
                    int form_factor = Convert.ToInt32(queryObj["FormFactor"]);
                    Dictionary<int, string> formFactorDict = new Dictionary<int, string>{
                        { 0, software_lang.TSReadLangs("Ram_Content", "ram_c_unknown") },
                        { 1, "Other" },
                        { 2, "SIP" },
                        { 3, "DIP" },
                        { 4, "ZIP" },
                        { 5, "SOJ" },
                        { 6, "Proprietary" },
                        { 7, "SIMM" },
                        { 8, "DIMM" },
                        { 9, "TSOP" },
                        { 10, "PGA" },
                        { 11, "RIMM" },
                        { 12, "SO-DIMM" },
                        { 13, "SRIMM" },
                        { 14, "SMD" },
                        { 15, "SSMP" },
                        { 16, "QFP" },
                        { 17, "TQFP" },
                        { 18, "SOIC" },
                        { 19, "LCC" },
                        { 20, "PLCC" },
                        { 21, "BGA" },
                        { 22, "FPBGA" },
                        { 23, "LGA" }
                    };
                    //
                    if (formFactorDict.TryGetValue(form_factor, out string formFactorValue)){
                        ram_form_factor.Add(formFactorValue);
                    }else{
                        ram_form_factor.Add(formFactorDict[0]);
                    }
                    RAM_FormFactor_V.Text = ram_form_factor[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM SERIAL
                    string ram_serial = Convert.ToString(queryObj["SerialNumber"]).Trim();
                    if (string.IsNullOrEmpty(ram_serial) || ram_serial.Equals("Unknown", StringComparison.OrdinalIgnoreCase)){
                        ram_serial_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }else{
                        // Hiding mode kontrolü
                        if (hiding_mode_wrapper == 1){
                            int maskLength = vis_m_property.Next(vn_range[0], vn_range[1]);
                            ram_serial_list.Add(new string('*', maskLength) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }else{
                            ram_serial_list.Add(ram_serial);
                        }
                    }
                    RAM_Serial_V.Text = ram_serial_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM MAN
                    string ram_man = Convert.ToString(queryObj["Manufacturer"])?.Trim() ?? string.Empty;
                    Dictionary<string, string> manufacturerDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "017A", "Apacer" },
                        { "059B", "Crucial" },
                        { "04CD", "G.Skill" },
                        { "0198", "Kingston (HyperX)" },
                        { "029E", "Corsair" },
                        { "04CB", "A-DATA" },
                        { "00CE", "Samsung" },
                        { "00FE", "Micron" },
                        { "00AD", "SK Hynix" },
                        { "00B3", "Elpida" },
                        { "2C00", "Micron Technology" },
                        { "014F", "Transcend" },
                        { "1C1F", "Kingston" },
                        { "00A1", "Infineon" },
                        { "7F7F", "Silicon Power" },
                        { "0147", "Spector" },
                        { "0251", "Patriot Memory" },
                        { "0325", "PNY" },
                        { "05CD", "Team Group" },
                        { "008A", "Toshiba" },
                        { "0294", "Ocz" },
                        { "058A", "GeIL" },
                        { "069A", "Longsys (Lexar)" },
                        { "0128", "Asgard / Gloway" },
                        { "80AD", "SK Hynix" },
                        { "80CE", "Samsung" }
                    };
                    string detectedManufacturer;
                    if (string.IsNullOrEmpty(ram_man) || ram_man.Equals("unknown", StringComparison.OrdinalIgnoreCase)){
                        detectedManufacturer = software_lang.TSReadLangs("Ram_Content", "ram_c_unknown");
                    }else{
                        if (manufacturerDict.TryGetValue(ram_man, out string manufacturerName)){
                            detectedManufacturer = manufacturerName;
                        }else{
                            detectedManufacturer = ram_man;
                        }
                    }
                    ram_manufacturer_list.Add(detectedManufacturer);
                    RAM_Manufacturer_V.Text = detectedManufacturer;
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM BANK LABEL
                    string bank_label = Convert.ToString(queryObj["BankLabel"]);
                    if (string.IsNullOrEmpty(bank_label)){
                        ram_bank_label_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }else{
                        ram_bank_label_list.Add(bank_label);
                    }
                    RAM_BankLabel_V.Text = ram_bank_label_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM TOTAL WIDTH
                    string ram_data_width = Convert.ToString(queryObj["TotalWidth"]);
                    if (string.IsNullOrEmpty(ram_data_width)){
                        ram_data_width_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }else{
                        ram_data_width_list.Add(ram_data_width + " Bit");
                    }
                    RAM_DataWidth_V.Text = ram_data_width_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // RAM LOCATOR
                    bellek_type_list.Add(Convert.ToString(queryObj["DeviceLocator"]));
                    RAM_BellekType_V.Text = bellek_type_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
                try{
                    // PART NUMBER
                    string part_number = Convert.ToString(queryObj["PartNumber"]).Trim();
                    if (string.IsNullOrEmpty(part_number)){
                        ram_part_number_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                    }else{
                        if (hiding_mode_wrapper == 1){
                            int maskLength = vis_m_property.Next(vn_range[0], vn_range[1]);
                            ram_part_number_list.Add(new string('*', maskLength) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }else{
                            ram_part_number_list.Add(part_number);
                        }
                    }
                    RAM_PartNumber_V.Text = ram_part_number_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                }
            }
            // RAM SELECT
            try{
                int ram_amount = ram_slot_list.Count - 1;
                for (int rs = 0; rs <= ram_amount; rs++){
                    RAM_Selector_List.Items.Add(string.Format("{0} #{1} - {2} / {3}", software_lang.TSReadLangs("Ram_Content", "ram_c_ram_slot_select"), (rs + 1), ram_manufacturer_list[rs], ram_amount_list[rs]));
                }
                RAM_Selector_List.SelectedIndex = 0;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
            }
            // RAM PROCESS END ENABLED
            RAM_RotateBtn.Enabled = true;
            ((Control)RAM).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- RAM Section Loaded --->");
            }
        }
        private void RAM_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int ram_slot = RAM_Selector_List.SelectedIndex;
                try { RAM_Amount_V.Text = ram_amount_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_Type_V.Text = ram_type_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_Frequency_V.Text = ram_frekans_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_Volt_V.Text = ram_voltage_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_FormFactor_V.Text = ram_form_factor[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_Serial_V.Text = ram_serial_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_Manufacturer_V.Text = ram_manufacturer_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_BankLabel_V.Text = ram_bank_label_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_DataWidth_V.Text = ram_data_width_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_BellekType_V.Text = bellek_type_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
                try { RAM_PartNumber_V.Text = ram_part_number_list[ram_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
        }
        private async void RamBgProcess(){
            try{
                ManagementObjectSearcher search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                while (loop_status){
                    var get_ram_info = search_os.Get().Cast<ManagementObject>().FirstOrDefault();
                    ulong total = (ulong)get_ram_info["TotalVisibleMemorySize"] * 1024;
                    ulong free = (ulong)get_ram_info["FreePhysicalMemory"] * 1024;
                    ulong used = total - free;
                    double usedRatio = (double)used / total * 100;
                    if (IsHandleCreated){
                        BeginInvoke((Action)(() =>{
                            RAM_UsageRAMCount_V.Text = TS_FormatSize(used);
                            RAM_EmptyRamCount_V.Text = TS_FormatSize(free);
                            RAM_ProgressFEPanel.Height = (int)(RAM_ProgressBGPanel.Height * (usedRatio / 100.0));
                            RAM_ProgressLabel.Text = $"{usedRatio:0.0}%";
                            RAM_ProgressLabel.Top = RAM_ProgressFEPanel.Top + 6;
                        }));
                    }
                    await Task.Delay(1000);
                }
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "RamBgProcess()");
                }
            }
        }
        #endregion
        #region GPU_Section
        // GPU
        // ======================================================================================================
        readonly Dictionary<string, int> gpuNameCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // ------------------------------------------------------------------------------------
        readonly List<string> gpu_man_list = new List<string>();
        readonly List<string> gpu_vram_list = new List<string>();
        readonly List<string> gpu_driver_version_list = new List<string>();
        readonly List<string> gpu_driver_date_list = new List<string>();
        readonly List<string> gpu_status_list = new List<string>();
        readonly List<string> gpu_device_id_list = new List<string>();
        readonly List<string> gpu_dac_type_list = new List<string>();
        readonly List<string> gpu_drivers_list = new List<string>();
        readonly List<string> gpu_dxfl_list = new List<string>();
        readonly List<string> gpu_dxfl_max_list = new List<string>();
        readonly List<string> gpu_inf_file_list = new List<string>();
        readonly List<string> gpu_inf_file_section_list = new List<string>();
        readonly List<string> gpu_current_colors_list = new List<string>();
        // ------------------------------------------------------------------------------------
        readonly List<string> gpu_monitor_user_friendly_name_list = new List<string>();
        readonly List<string> gpu_monitor_manufacturer_list = new List<string>();
        readonly List<string> gpu_monitor_product_code_id_list = new List<string>();
        readonly List<string> gpu_monitor_serial_number_id_list = new List<string>();
        readonly List<string> gpu_monitor_con_type_list = new List<string>();
        readonly List<string> gpu_monitor_manuf_list = new List<string>();
        readonly List<string> gpu_monitor_manuf_week_list = new List<string>();
        readonly List<string> gpu_monitor_hid_list = new List<string>();
        readonly List<string> gpu_monitor_res_list = new List<string>();
        readonly List<string> gpu_monitor_virtual_res_list = new List<string>();
        readonly List<string> gpu_monitor_bounds_list = new List<string>();
        readonly List<string> gpu_monitor_work_list = new List<string>();
        readonly List<string> gpu_monitor_refresh_rate_list = new List<string>();
        readonly List<string> gpu_monitor_bit_deep_list = new List<string>();
        readonly List<string> gpu_monitor_primary_list = new List<string>();
        private void Gpu(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            ManagementObjectSearcher search_vc = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, AdapterCompatibility, DriverVersion, DriverDate, Availability, PNPDeviceID, AdapterDACType, InstalledDisplayDrivers, InfFilename, InfSection, CurrentNumberOfColors FROM Win32_VideoController");
            foreach (ManagementObject query_vc_rotate in search_vc.Get().Cast<ManagementObject>()){
                try{
                    // GET GPU NAME AND MATCH MULTI GPU
                    string gpu_name = (Convert.ToString(query_vc_rotate["Name"]) ?? "").Trim();
                    if (gpu_name.Length == 0){
                        continue;
                    }
                    if (!gpuNameCounters.TryGetValue(gpu_name, out int count)){
                        count = 0;
                    }
                    count++;
                    gpuNameCounters[gpu_name] = count;
                    if (count == 1){
                        GPU_Selector_List.Items.Add(gpu_name);
                    }
                    else if (count == 2){
                        bool found = false;
                        for (int i = 0; i < GPU_Selector_List.Items.Count; i++){
                            if (string.Equals(GPU_Selector_List.Items[i]?.ToString(), gpu_name, StringComparison.OrdinalIgnoreCase)){
                                GPU_Selector_List.Items[i] = gpu_name + " #1";
                                found = true;
                                break;
                            }
                        }
                        if (!found){
                            GPU_Selector_List.Items.Add(gpu_name + " #1");
                        }
                        GPU_Selector_List.Items.Add(gpu_name + " #2");
                    }else{
                        GPU_Selector_List.Items.Add(gpu_name + " #" + count);
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU MAN
                    string gpu_man = Convert.ToString(query_vc_rotate["AdapterCompatibility"]).Trim();
                    if (!string.IsNullOrEmpty(gpu_man)){
                        gpu_man_list.Add(gpu_man);
                        GPU_Manufacturer_V.Text = gpu_man_list[0];
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU DRIVER VERSION
                    string driver_version = Convert.ToString(query_vc_rotate["DriverVersion"]);
                    if (!string.IsNullOrEmpty(driver_version)){
                        gpu_driver_version_list.Add(driver_version);
                        GPU_Version_V.Text = gpu_driver_version_list[0];
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU DRIVER DATE
                    string gpu_date = Convert.ToString(query_vc_rotate["DriverDate"]);
                    if (!string.IsNullOrEmpty(gpu_date)){
                        DateTime gpu_date_last = ManagementDateTimeConverter.ToDateTime(gpu_date);
                        gpu_driver_date_list.Add($"{gpu_date_last:dd.MM.yyyy}");
                        GPU_DriverDate_V.Text = gpu_driver_date_list[0];
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU STATUS
                    int gpu_status = Convert.ToInt32(query_vc_rotate["Availability"]);
                    var gpuStatusMessages = new Dictionary<int, string>{
                        { 1, software_lang.TSReadLangs("Gpu_Content", "gpu_c_other") },
                        { 2, software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown") },
                        { 3, software_lang.TSReadLangs("Gpu_Content", "gpu_c_works_smoothly") },
                        { 4, software_lang.TSReadLangs("Gpu_Content", "gpu_c_warning") },
                        { 5, software_lang.TSReadLangs("Gpu_Content", "gpu_c_test") },
                        { 6, software_lang.TSReadLangs("Gpu_Content", "gpu_c_not_applicable") },
                        { 7, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_off") },
                        { 8, software_lang.TSReadLangs("Gpu_Content", "gpu_c_offline") },
                        { 9, software_lang.TSReadLangs("Gpu_Content", "gpu_c_off_duty") },
                        { 10, software_lang.TSReadLangs("Gpu_Content", "gpu_c_corrupted") },
                        { 11, software_lang.TSReadLangs("Gpu_Content", "gpu_c_not_installed") },
                        { 12, software_lang.TSReadLangs("Gpu_Content", "gpu_c_install_error") },
                        { 13, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_saving_mode") },
                        { 14, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_saving_mode_low_power") },
                        { 15, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_saving_mode_wait") },
                        { 16, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_loop") },
                        { 17, software_lang.TSReadLangs("Gpu_Content", "gpu_c_power_saving_warning") },
                        { 18, software_lang.TSReadLangs("Gpu_Content", "gpu_c_paused") },
                        { 19, software_lang.TSReadLangs("Gpu_Content", "gpu_c_not_ready") },
                        { 20, software_lang.TSReadLangs("Gpu_Content", "gpu_c_not_configured") },
                        { 21, software_lang.TSReadLangs("Gpu_Content", "gpu_c_silent_mode") }
                    };
                    if (gpuStatusMessages.TryGetValue(gpu_status, out string gpuStatusMessage)){
                        gpu_status_list.Add(gpuStatusMessage);
                    }else{
                        gpu_status_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                    }
                    GPU_Status_V.Text = gpu_status_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU DEVICE ID
                    string gpu_device_id = Convert.ToString(query_vc_rotate["PNPDeviceID"]).Trim();
                    if (!string.IsNullOrEmpty(gpu_device_id)){
                        char[] split_char = { '\\' };
                        string[] gpu_device_split = gpu_device_id.Trim().Split(split_char);
                        gpu_device_id_list.Add($"{gpu_device_split[0]}\\{gpu_device_split[1]}");
                    }else{
                        gpu_device_id_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                    }
                    GPU_DeviceID_V.Text = gpu_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU DAC TYPE
                    string adaptor_dac_type = Convert.ToString(query_vc_rotate["AdapterDACType"]);
                    if (string.IsNullOrEmpty(adaptor_dac_type)){
                        gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                    }else{
                        if (adaptor_dac_type == "Integrated RAMDAC"){
                            gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_integrated_ramdac"));
                        }else if (adaptor_dac_type == "Internal"){
                            gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_internal_ramdac"));
                        }else{
                            gpu_dac_type_list.Add(adaptor_dac_type);
                        }
                    }
                    GPU_DacType_V.Text = gpu_dac_type_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU DIRECTX DRIVERS
                    var grouped = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    var drivers = (query_vc_rotate["InstalledDisplayDrivers"] ?? "").ToString().Split(',');
                    foreach (var p in drivers){
                        var path = p.Trim();
                        if (string.IsNullOrEmpty(path)) continue;
                        int lastSlash = path.LastIndexOf('\\');
                        var fileName = lastSlash >= 0 && lastSlash < path.Length - 1 ? path.Substring(lastSlash + 1) : path;
                        if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(new[] { '<', '>' }) >= 0)
                            continue;
                        var directory = lastSlash > 0 ? path.Substring(0, lastSlash) : "";
                        int secondLastSlash = directory.LastIndexOf('\\');
                        var key = secondLastSlash >= 0 ? directory.Substring(secondLastSlash + 1) : directory;
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!grouped.TryGetValue(key, out var set))
                            grouped[key] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        set.Add(fileName);
                    }
                    gpu_drivers_list.AddRange(grouped.Values.Select(dlls => string.Join(", ", dlls.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))));
                    GPU_GraphicDriversName_V.Text = gpu_drivers_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU INF FILE NAME
                    string gpu_inf_file = Convert.ToString(query_vc_rotate["InfFilename"]);
                    if (!string.IsNullOrEmpty(gpu_inf_file)){
                        gpu_inf_file_list.Add(gpu_inf_file);
                        GPU_InfFileName_V.Text = gpu_inf_file_list[0];
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // GPU INF FILE GPU INFO PARTITION
                    string gpu_inf_section = Convert.ToString(query_vc_rotate["InfSection"]);
                    if (!string.IsNullOrEmpty(gpu_inf_section)){
                        gpu_inf_file_section_list.Add(gpu_inf_section);
                        GPU_INFSectionFile_V.Text = gpu_inf_file_section_list[0];
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
                try{
                    // CURRENT NUMBER OF COLORS
                    long gpu_current_color = Convert.ToInt64(query_vc_rotate["CurrentNumberOfColors"]);
                    CultureInfo currentCulture = CultureInfo.CurrentCulture;
                    string formattedNumber = gpu_current_color.ToString("N0", currentCulture);
                    gpu_current_colors_list.Add(formattedNumber);
                    GPU_CurrentColor_V.Text = gpu_current_colors_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                }
            }
            // MONITOR INFORMATIONS
            // ======================================
            try{
                List<string> edid_model_list = new List<string>();
                List<string> edid_manufacturer_list = new List<string>();
                List<string> edid_product_code_list = new List<string>();
                List<string> edid_serial_list = new List<string>();
                List<string> edid_manuf_list = new List<string>();
                List<string> edid_manuf_week_list = new List<string>();
                List<string> edid_manuf_hid_list = new List<string>();
                //
                string local_display_name = software_lang.TSReadLangs("Gpu_Content", "gpu_c_d_name");
                string unknown_message = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                //
                using (var searchMonitorInfo = new ManagementObjectSearcher(@"root\WMI", "SELECT InstanceName, UserFriendlyName, ManufacturerName, ProductCodeID, SerialNumberID, YearOfManufacture, WeekOfManufacture FROM WmiMonitorID")){
                    foreach (ManagementObject mo in searchMonitorInfo.Get().Cast<ManagementObject>()){
                        string instanceName = "", model = "", manufacturer = "", productCode = "", serial = "", manuf_week = "";
                        UInt16 manuf_year = 0;
                        //
                        try { instanceName = mo["InstanceName"]?.ToString().Trim() ?? ""; } catch { }
                        try { model = GetMonitorFromUShortArray((ushort[])mo["UserFriendlyName"]); } catch { }
                        try { manufacturer = GetMonitorFromUShortArray((ushort[])mo["ManufacturerName"]); } catch { }
                        try { productCode = GetMonitorFromUShortArray((ushort[])mo["ProductCodeID"]); } catch { }
                        try { serial = GetMonitorFromUShortArray((ushort[])mo["SerialNumberID"]); } catch { }
                        try { manuf_year = (UInt16)mo["YearOfManufacture"]; } catch { }
                        try { manuf_week = TSGetFormattedWeekInfo(manuf_year, Convert.ToInt32(Convert.ToUInt32(mo["WeekOfManufacture"]))); } catch { }
                        //
                        if (string.IsNullOrWhiteSpace(model)) model = local_display_name;
                        //
                        edid_model_list.Add(model.Trim('\0').Trim());
                        edid_manufacturer_list.Add(string.IsNullOrWhiteSpace(manufacturer) ? unknown_message : manufacturer.Trim());
                        edid_product_code_list.Add(string.IsNullOrWhiteSpace(productCode) ? unknown_message : productCode.Trim());
                        edid_serial_list.Add(string.IsNullOrWhiteSpace(serial) ? unknown_message : serial.Trim());
                        edid_manuf_list.Add(manuf_year == 0 ? unknown_message : manuf_year.ToString().Trim());
                        edid_manuf_week_list.Add(string.IsNullOrWhiteSpace(manuf_week) ? unknown_message : manuf_week.Trim());
                        edid_manuf_hid_list.Add(string.IsNullOrWhiteSpace(instanceName) ? unknown_message : instanceName.Trim());
                    }
                }
                //
                var connectionParamsSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT InstanceName, VideoOutputTechnology FROM WmiMonitorConnectionParams");
                var videoModeMapByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var videoModeMapStatic = new Dictionary<long, string>{
                    { -2, software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown") },
                    { -1, software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown") },
                    { 0, "HD15 (VGA)" },
                    { 1, "S-Video" },
                    { 2, "Composite Video" },
                    { 3, "Component Video" },
                    { 4, "DVI" },
                    { 5, "HDMI" },
                    { 6, "LVDS / MIPI DSI" },
                    { 8, "D-Jpn" },
                    { 9, "SDI" },
                    { 10, string.Format("DisplayPort ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_external")) },
                    { 11, string.Format("DisplayPort ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_embedded")) },
                    { 12, string.Format("UDI ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_external")) },
                    { 13, string.Format("UDI ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_embedded")) },
                    { 14, "SDTV Dongle" },
                    { 15, string.Format("Miracast ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_wireless")) },
                    { 16, "Indirect Wired" },
                    { unchecked((int)0x80000000), software_lang.TSReadLangs("Gpu_Content", "gpu_c_d_name") }
                };
                foreach (ManagementObject mp in connectionParamsSearcher.Get().Cast<ManagementObject>()){
                    try{
                        string inst = mp["InstanceName"]?.ToString() ?? "";
                        long video_mode = Convert.ToInt64(mp["VideoOutputTechnology"]);
                        string videoModeRender = videoModeMapStatic.TryGetValue(video_mode, out var vm) ? vm : "eDP";
                        string instNorm = inst.Trim();
                        if (!string.IsNullOrEmpty(instNorm)){
                            if (!videoModeMapByKey.ContainsKey(instNorm)){
                                videoModeMapByKey[instNorm] = videoModeRender;
                            }
                        }
                        string[] parts = instNorm.Split('\\');
                        if (parts.Length > 1){
                            string shortId = parts[1].Trim();
                            if (!string.IsNullOrEmpty(shortId) && !videoModeMapByKey.ContainsKey(shortId)){
                                videoModeMapByKey[shortId] = videoModeRender;
                            }
                        }
                        if (parts.Length > 2){
                            string tail = parts[parts.Length - 1].Trim();
                            if (!string.IsNullOrEmpty(tail) && !videoModeMapByKey.ContainsKey(tail)){
                                videoModeMapByKey[tail] = videoModeRender;
                            }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                    }
                }
                for (int i = 0; i < Screen.AllScreens.Length; i++){
                    var screen = Screen.AllScreens[i];
                    var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
                    EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
                    //
                    DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                    d.cb = Marshal.SizeOf(d);
                    EnumDisplayDevices(screen.DeviceName, 0, ref d, 0);
                    string deviceKey = d.DeviceID ?? "";
                    int matchedIndex = -1;
                    string devKeyPart = "";
                    if (deviceKey.Contains("\\")){
                        devKeyPart = deviceKey.Split('\\')[1].Trim().ToUpperInvariant();
                    }else{
                        devKeyPart = deviceKey.Trim().ToUpperInvariant();
                    }
                    //
                    for (int j = 0; j < edid_manuf_hid_list.Count; j++){
                        string instanceName = edid_manuf_hid_list[j] ?? "";
                        if (string.IsNullOrWhiteSpace(instanceName)){
                            continue;
                        }
                        if (instanceName.Equals(deviceKey, StringComparison.OrdinalIgnoreCase) || instanceName.IndexOf(devKeyPart, StringComparison.OrdinalIgnoreCase) >= 0){
                            matchedIndex = j;
                            break;
                        }
                        string[] p = instanceName.Split('\\');
                        if (p.Length > 1 && p[1].Trim().Equals(devKeyPart, StringComparison.OrdinalIgnoreCase)){
                            matchedIndex = j;
                            break;
                        }
                    }
                    string resolvedVideoOutput = null;
                    if (matchedIndex != -1){
                        string instCandidate = edid_manuf_hid_list[matchedIndex] ?? "";
                        if (!string.IsNullOrWhiteSpace(instCandidate) && videoModeMapByKey.TryGetValue(instCandidate, out var v1)){
                            resolvedVideoOutput = v1;
                        }else{
                            string[] p = instCandidate.Split('\\');
                            if (p.Length > 1 && videoModeMapByKey.TryGetValue(p[1], out var v2)){
                                resolvedVideoOutput = v2;
                            }
                        }
                    }
                    if (resolvedVideoOutput == null){
                        string[] dkParts = deviceKey.Split('\\');
                        if (dkParts.Length > 1 && videoModeMapByKey.TryGetValue(dkParts[1], out var vv)){
                            resolvedVideoOutput = vv;
                        }else if (dkParts.Length > 2 && videoModeMapByKey.TryGetValue(dkParts[dkParts.Length - 1], out var vv2)){
                            resolvedVideoOutput = vv2;
                        }
                    }
                    if (resolvedVideoOutput == null){
                        resolvedVideoOutput = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                    }
                    gpu_monitor_user_friendly_name_list.Add(matchedIndex != -1 ? edid_model_list[matchedIndex] : local_display_name);
                    gpu_monitor_manufacturer_list.Add(matchedIndex != -1 ? edid_manufacturer_list[matchedIndex] : unknown_message);
                    gpu_monitor_product_code_id_list.Add(matchedIndex != -1 ? edid_product_code_list[matchedIndex] : unknown_message);
                    if (matchedIndex == -1 || hiding_mode_wrapper != 1){
                        gpu_monitor_serial_number_id_list.Add(matchedIndex != -1 ? edid_serial_list[matchedIndex] : unknown_message);
                    }else{
                        gpu_monitor_serial_number_id_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                    }
                    gpu_monitor_manuf_list.Add(matchedIndex != -1 ? edid_manuf_list[matchedIndex] : unknown_message);
                    gpu_monitor_manuf_week_list.Add(matchedIndex != -1 ? edid_manuf_week_list[matchedIndex] : unknown_message);
                    gpu_monitor_hid_list.Add(matchedIndex != -1 ? edid_manuf_hid_list[matchedIndex] : unknown_message);
                    gpu_monitor_con_type_list.Add(resolvedVideoOutput);
                    //
                    gpu_monitor_bounds_list.Add(GPUFormatScreenInfo(screen.Bounds));
                    gpu_monitor_work_list.Add(GPUFormatScreenInfo(screen.WorkingArea));
                    gpu_monitor_primary_list.Add(GPUFormatPrimaryScreen(screen.Primary));
                    gpu_monitor_res_list.Add(dm.dmPelsWidth + " x " + dm.dmPelsHeight);
                    gpu_monitor_virtual_res_list.Add(screen.Bounds.Width + " x " + screen.Bounds.Height);
                    gpu_monitor_refresh_rate_list.Add(dm.dmDisplayFrequency + " Hz");
                    gpu_monitor_bit_deep_list.Add(dm.dmBitsPerPel + " Bit");
                    //
                    GPU_MonitorSelector_List.Items.Add(string.Format("{0} #{1} - {2}", software_lang.TSReadLangs("Gpu_Content", "gpu_c_monitor_select"), i + 1, gpu_monitor_user_friendly_name_list[i]));
                    if (screen.Primary){
                        GPU_MonitorSelector_List.SelectedIndex = i;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            try{
                // GET DIRECTX GPU INFO
                gpu_vram_list.Clear();
                gpu_dxfl_list.Clear();
                gpu_dxfl_max_list.Clear();
                _ = LoadDirectXGPUInfo();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            // GPU SELECT
            try { GPU_Selector_List.SelectedIndex = 0; }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            // GPU PROCESS END ENABLED
            GPU_RotateBtn.Enabled = true;
            ((Control)GPU).Enabled = true;
            if (debug_status){ 
                TSLogger.Log("<--- GPU Section Loaded --->");
            }
        }
        // GPU VRAM
        // ======================================================================================================
        private async Task LoadDirectXGPUInfo(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            //
            int n = GPU_Selector_List.Items.Count;
            while (gpu_vram_list.Count < n) gpu_vram_list.Add(string.Empty);
            while (gpu_dxfl_list.Count < n) gpu_dxfl_list.Add(string.Empty);
            while (gpu_dxfl_max_list.Count < n) gpu_dxfl_max_list.Add(string.Empty);
            //
            await Task.Run(() =>{
                string dxdiagXml = Path.Combine(Path.GetTempPath(), $"dxdiag_{Guid.NewGuid()}.xml");
                //
                try{
                    var proc = new Process{
                        StartInfo ={
                            FileName = "dxdiag.exe",
                            Arguments = $"/whql:off /x \"{dxdiagXml}\"",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();
                    //
                    var dx = new XmlDocument();
                    dx.Load(dxdiagXml);
                    XmlNodeList displayNodes = dx.SelectNodes("/DxDiag/DisplayDevices/DisplayDevice");
                    if (displayNodes == null) return;
                    //
                    var gpuAgg = new Dictionary<string, (long vramMaxBytes, string bestDxfl)>(StringComparer.OrdinalIgnoreCase);
                    for (int di = 0; di < displayNodes.Count; di++){
                        XmlNode node = displayNodes[di];
                        if (node == null) continue;
                        string gpuName = node.SelectSingleNode("CardName")?.InnerText?.Trim() ?? software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                        //
                        string devId = node.SelectSingleNode("DeviceKey")?.InnerText?.Trim() ?? node.SelectSingleNode("DeviceIdentifier")?.InnerText?.Trim() ?? node.SelectSingleNode("DeviceId")?.InnerText?.Trim() ?? node.SelectSingleNode("DeviceID")?.InnerText?.Trim();
                        //
                        string gpuKey = !string.IsNullOrWhiteSpace(devId) ? (gpuName + "|" + devId) : (gpuName + "|i:" + di);
                        //
                        long vramBytes = 0;
                        string dedicatedMemory = node.SelectSingleNode("DedicatedMemory")?.InnerText ?? "0 MB";
                        if (TryParseMb(dedicatedMemory, out int parsedMB))
                            vramBytes = (long)parsedMB * 1024 * 1024;
                        //
                        string dxfl = NormalizeFeatureLevels(ExtractFeatureLevelsFromDisplayDevice(node));
                        if (!gpuAgg.TryGetValue(gpuKey, out var agg))
                            agg = (0L, string.Empty);
                        if (vramBytes > agg.vramMaxBytes)
                            agg.vramMaxBytes = vramBytes;
                        //
                        if (!string.IsNullOrWhiteSpace(dxfl)){
                            int incMajor = -1, incMinor = -1, incCount = 0;{
                                string[] parts = dxfl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                incCount = parts.Length;
                                for (int i = 0; i < parts.Length; i++){
                                    string p = parts[i].Trim();
                                    int dot = p.IndexOf('.');
                                    if (dot <= 0) continue;
                                    //
                                    string majStr = p.Substring(0, dot);
                                    int end = dot + 1;
                                    while (end < p.Length && char.IsDigit(p[end])) end++;
                                    //
                                    string minStr = p.Substring(dot + 1, end - (dot + 1));
                                    if (!int.TryParse(majStr, out int maj)) continue;
                                    if (!int.TryParse(minStr, out int min)) continue;
                                    //
                                    if (maj > incMajor || (maj == incMajor && min > incMinor)){
                                        incMajor = maj;
                                        incMinor = min;
                                    }
                                }
                            }
                            //
                            int curMajor = -1, curMinor = -1, curCount = 0;
                            if (!string.IsNullOrWhiteSpace(agg.bestDxfl)){
                                string[] parts = agg.bestDxfl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                curCount = parts.Length;
                                for (int i = 0; i < parts.Length; i++){
                                    string p = parts[i].Trim();
                                    int dot = p.IndexOf('.');
                                    if (dot <= 0) continue;
                                    //
                                    string majStr = p.Substring(0, dot);
                                    int end = dot + 1;
                                    while (end < p.Length && char.IsDigit(p[end])) end++;
                                    //
                                    string minStr = p.Substring(dot + 1, end - (dot + 1));
                                    if (!int.TryParse(majStr, out int maj)) continue;
                                    if (!int.TryParse(minStr, out int min)) continue;
                                    //
                                    if (maj > curMajor || (maj == curMajor && min > curMinor)){
                                        curMajor = maj;
                                        curMinor = min;
                                    }
                                }
                            }
                            bool incomingBetter = string.IsNullOrWhiteSpace(agg.bestDxfl) || (incMajor > curMajor) || (incMajor == curMajor && incMinor > curMinor) || (incMajor == curMajor && incMinor == curMinor && incCount > curCount);
                            if (incomingBetter)
                                agg.bestDxfl = dxfl;
                        }
                        gpuAgg[gpuKey] = agg;
                    }
                    foreach (var kv in gpuAgg){
                        string gpuKey = kv.Key;
                        string gpuName = gpuKey;
                        int bar = gpuKey.IndexOf('|');
                        if (bar > 0) gpuName = gpuKey.Substring(0, bar);
                        //
                        long vramBytes = kv.Value.vramMaxBytes;
                        string bestDxfl = kv.Value.bestDxfl;
                        //
                        int bestIndex = -1;
                        int bestScore = int.MaxValue;
                        //
                        var candidateIndices = new List<int>();
                        for (int i = 0; i < GPU_Selector_List.Items.Count; i++){
                            string itemNameRaw = GPU_Selector_List.Items[i]?.ToString() ?? "";
                            string itemBase = StripGpuIndexSuffix(itemNameRaw);
                            if (itemBase.Equals(gpuName, StringComparison.OrdinalIgnoreCase))
                                candidateIndices.Add(i);
                        }
                        //
                        if (candidateIndices.Count == 1){
                            int only = candidateIndices[0];
                            bool slotFilled = (only >= 0 && only < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[only])) || (only >= 0 && only < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[only]));
                            if (slotFilled) continue;
                        }
                        //
                        if (candidateIndices.Count > 0){
                            for (int k = 0; k < candidateIndices.Count; k++){
                                int idx = candidateIndices[k];
                                if (idx >= 0 && idx < gpu_vram_list.Count && string.IsNullOrWhiteSpace(gpu_vram_list[idx])){
                                    bestIndex = idx;
                                    break;
                                }
                            }
                            //
                            if (bestIndex < 0){
                                string a = gpuName.ToLowerInvariant();
                                for (int k = 0; k < candidateIndices.Count; k++){
                                    int i = candidateIndices[k];
                                    string itemNameRaw = GPU_Selector_List.Items[i]?.ToString() ?? "";
                                    int score = LevenshteinDistance(a, itemNameRaw.ToLowerInvariant());
                                    if (score < bestScore){
                                        bestScore = score;
                                        bestIndex = i;
                                    }
                                }
                            }
                        }else{
                            string a = gpuName.ToLowerInvariant();
                            for (int i = 0; i < GPU_Selector_List.Items.Count; i++){
                                string itemName = GPU_Selector_List.Items[i]?.ToString() ?? "";
                                int score = LevenshteinDistance(a, itemName.ToLowerInvariant());
                                if (score < bestScore){
                                    bestScore = score;
                                    bestIndex = i;
                                }
                            }
                        }
                        if (bestIndex >= 0){
                            string dynamicStr = TS_FormatSize(vramBytes);
                            double roundedBytes = 0;
                            //
                            if (vramBytes >= 1024 * 1024 * 1024){
                                double vramGB = vramBytes / (1024.0 * 1024 * 1024);
                                double totalGBRounded = Math.Round(vramGB, 0, MidpointRounding.AwayFromZero);
                                roundedBytes = totalGBRounded * 1024 * 1024 * 1024;
                            }else{
                                double vramMB = vramBytes / (1024.0 * 1024);
                                double totalMBRounded = Math.Round(vramMB, 0, MidpointRounding.AwayFromZero);
                                roundedBytes = totalMBRounded * 1024 * 1024;
                            }
                            //
                            string totalStr = TS_FormatSize(roundedBytes);
                            if (totalStr != dynamicStr){
                                string available_string = software_lang.TSReadLangs("Gpu_Content", "gpu_c_available");
                                gpu_vram_list[bestIndex] = string.Format("{0} ({1} {2})", totalStr, available_string, dynamicStr);
                            }else{
                                gpu_vram_list[bestIndex] = totalStr;
                            }
                            //
                            if (!string.IsNullOrWhiteSpace(bestDxfl)){
                                gpu_dxfl_list[bestIndex] = bestDxfl;
                                string maxFl = bestDxfl.Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                                int secondDot = maxFl.IndexOf('.', maxFl.IndexOf('.') + 1);
                                if (secondDot > 0) maxFl = maxFl.Substring(0, secondDot);
                                gpu_dxfl_max_list[bestIndex] = maxFl;
                            }else{
                                string unk = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                                gpu_dxfl_list[bestIndex] = unk;
                                gpu_dxfl_max_list[bestIndex] = unk;
                            }
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "LoadDirectXGPUInfo()"); }
                }
                finally{
                    try { if (File.Exists(dxdiagXml)) File.Delete(dxdiagXml); } catch { }
                }
            });
            GPU_VRAM_V.Invoke(new Action(() =>{
                int gpu_select = GPU_Selector_List.SelectedIndex;
                GPU_VRAM_V.Text = (gpu_select >= 0 && gpu_select < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[gpu_select])) ? gpu_vram_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                GPU_DirectXAll_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[gpu_select])) ? gpu_dxfl_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                GPU_DirectXMax_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_max_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_max_list[gpu_select])) ? gpu_dxfl_max_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
            }));
        }
        // VRAM AND FEATURE LEVELS PARSER ALGORITHM
        // ======================================================================================================
        private static string StripGpuIndexSuffix(string s){
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            int hash = s.LastIndexOf('#');
            if (hash > 0){
                string tail = s.Substring(hash + 1).Trim();
                if (tail.Length > 0){
                    bool allDigits = true;
                    for (int i = 0; i < tail.Length; i++){
                        if (!char.IsDigit(tail[i])) { allDigits = false; break; }
                    }
                    if (allDigits)
                        return s.Substring(0, hash).TrimEnd();
                }
            }
            return s.Trim();
        }
        private static bool TryParseMb(string text, out int mb){
            mb = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();
            if (!text.EndsWith("MB", StringComparison.OrdinalIgnoreCase)) return false;
            string num = text.Substring(0, text.Length - 2).Trim();
            return int.TryParse(num, out mb);
        }
        private static string ExtractFeatureLevelsFromDisplayDevice(XmlNode displayDeviceNode){
            if (displayDeviceNode == null) return string.Empty;
            return displayDeviceNode.SelectSingleNode("FeatureLevels")?.InnerText?.Trim() ?? displayDeviceNode.SelectSingleNode("Feature Levels")?.InnerText?.Trim() ?? string.Empty;
        }
        private static string NormalizeFeatureLevels(string raw){
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;
            string s = raw.Trim();
            const string prefix = "Feature Levels:";
            if (s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                s = s.Substring(prefix.Length).Trim();
            string[] parts = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = parts[i].Trim();
            s = string.Join(", ", parts);
            s = s.Replace("_", ".");
            return s;
        }
        // LEVENSHTEIN DISTANCE ALGORITHM
        // ======================================================================================================
        private int LevenshteinDistance(string a, string b){
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++){
                for (int j = 1; j <= b.Length; j++){
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }
        // WEEKLY NUMBER CONVERTER
        // ======================================================================================================
        static string TSGetFormattedWeekInfo(int year, int weekNumber){
            var software_lang = new TSGetLangs(lang_path);
            if (weekNumber < 1 || weekNumber > 53)
                return software_lang.TSReadLangs("Gpu_Content", "gpu_c_date_valid_info");
            DateTime jan4 = new DateTime(year, 1, 4);
            int offset = ((int)jan4.DayOfWeek + 6) % 7;
            DateTime firstMonday = jan4.AddDays(-offset);
            DateTime targetMonday = firstMonday.AddDays((weekNumber - 1) * 7);
            DateTime firstDayOfMonth = new DateTime(targetMonday.Year, targetMonday.Month, 1);
            int firstDayOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
            DateTime firstMondayOfMonth = firstDayOfMonth.AddDays(firstDayOffset == 0 ? 0 : 7 - firstDayOffset);
            int weekOfMonth = targetMonday < firstMondayOfMonth ? 1 : 1 + (int)((targetMonday - firstMondayOfMonth).TotalDays / 7);
            string monthName = targetMonday.ToString("MMMM", CultureInfo.CurrentCulture);
            return string.Format(software_lang.TSReadLangs("Gpu_Content", "gpu_c_date_info"), year, monthName, weekOfMonth);
        }
        // GPU SELECT
        // ======================================================================================================
        private void GPU_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int gpu_select = GPU_Selector_List.SelectedIndex;
                try { GPU_Manufacturer_V.Text = gpu_man_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try{
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    string loading = software_lang.TSReadLangs("Gpu_Content", "gpu_c_loading");
                    GPU_VRAM_V.Text = (gpu_select >= 0 && gpu_select < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[gpu_select]) && !string.Equals(gpu_vram_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_vram_list[gpu_select] : loading;
                    GPU_DirectXAll_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[gpu_select]) && !string.Equals(gpu_dxfl_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_dxfl_list[gpu_select] : loading;
                    GPU_DirectXMax_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_max_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_max_list[gpu_select]) && !string.Equals(gpu_dxfl_max_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_dxfl_max_list[gpu_select] : loading;
                }catch (Exception ex){ if (debug_status){ TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_Version_V.Text = gpu_driver_version_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DriverDate_V.Text = gpu_driver_date_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_Status_V.Text = gpu_status_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DeviceID_V.Text = gpu_device_id_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DacType_V.Text = gpu_dac_type_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_GraphicDriversName_V.Text = gpu_drivers_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_InfFileName_V.Text = gpu_inf_file_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_INFSectionFile_V.Text = gpu_inf_file_section_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_CurrentColor_V.Text = gpu_current_colors_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
            }catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
        }
        private void GPU_MonitorSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int monitor_select = GPU_MonitorSelector_List.SelectedIndex;
                try { GPU_MonitorUserFriendlyName_V.Text = gpu_monitor_user_friendly_name_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorManName_V.Text = gpu_monitor_manufacturer_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorProductCodeID_V.Text = gpu_monitor_product_code_id_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorSerialNumberID_V.Text = gpu_monitor_serial_number_id_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorConType_V.Text = gpu_monitor_con_type_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorManfDate_V.Text = gpu_monitor_manuf_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorManfDateWeek_V.Text = gpu_monitor_manuf_week_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorHID_V.Text = gpu_monitor_hid_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorResLabel_V.Text = gpu_monitor_res_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorVirtualRes_V.Text = gpu_monitor_virtual_res_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorBounds_V.Text = gpu_monitor_bounds_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorWorking_V.Text = gpu_monitor_work_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_ScreenRefreshRate_V.Text = gpu_monitor_refresh_rate_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_ScreenBit_V.Text = gpu_monitor_bit_deep_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_MonitorPrimary_V.Text = gpu_monitor_primary_list[monitor_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
            }catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
        }
        // FORMATED SCREEN INFO
        // ======================================================================================================
        public static string GetMonitorFromUShortArray(ushort[] array){
            if (array == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var v in array){
                if (v > 0 && v <= char.MaxValue)
                    sb.Append((char)v);
            }
            return sb.ToString().Trim();
        }
        private string GPUFormatScreenInfo(Rectangle bounds){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string formatted = bounds.ToString().Replace("Width", software_lang.TSReadLangs("Gpu_Content", "gpu_c_width")).Replace("Height", software_lang.TSReadLangs("Gpu_Content", "gpu_c_height")).Replace("{", "").Replace("}", "").Replace(",", ", ").Replace("=", ": ");
            return formatted;
        }
        private string GPUFormatPrimaryScreen(bool isPrimary){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            return isPrimary ? software_lang.TSReadLangs("Gpu_Content", "gpu_c_yes") : software_lang.TSReadLangs("Gpu_Content", "gpu_c_no");
        }
        #endregion
        #region DISK_Section
        // DISK
        // ======================================================================================================
        readonly List<string> disk_man_list = new List<string>();
        readonly List<string> disk_model_list = new List<string>();
        readonly List<string> disk_volume_id_list = new List<string>();
        readonly List<string> disk_volume_name_list = new List<string>();
        readonly List<string> disk_firmware_list = new List<string>();
        readonly List<string> disk_serial_list = new List<string>();
        readonly List<string> disk_volume_serial_list = new List<string>();
        readonly List<string> disk_total_space_list = new List<string>();
        readonly List<string> disk_used_space_list = new List<string>();
        readonly List<string> disk_free_space_list = new List<string>();
        readonly List<string> disk_file_system_list = new List<string>();
        readonly List<string> disk_formatting_system_list = new List<string>();
        readonly List<string> disk_type_list = new List<string>();
        readonly List<string> disk_drive_type_list = new List<string>();
        readonly List<string> disk_interface_list = new List<string>();
        readonly List<string> disk_partition_list = new List<string>();
        readonly List<string> disk_media_loaded_list = new List<string>();
        readonly List<string> disk_media_status_list = new List<string>();
        readonly List<string> disk_health_status_list = new List<string>();
        readonly List<string> disk_boot_list = new List<string>();
        readonly List<string> disk_bootable_list = new List<string>();
        readonly List<string> disk_bitlocker_status_list = new List<string>();
        readonly List<string> disk_bitlocker_conversionstatus_list = new List<string>();
        readonly List<string> disk_bitlocker_encryptionmethod_list = new List<string>();
        readonly List<string> disk_drive_compressed_list = new List<string>();
        private void Disk(){
            // DISK COUNTER
            int disk_ssd_count = 0;
            int disk_hdd_count = 0;
            int disk_usb_count = 0;
            //
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            //
            try{
                var seenDisks = new HashSet<int>();
                //
                var get_drives = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject drive_info in get_drives.Get().Cast<ManagementObject>()){
                    int diskIndex;
                    try { diskIndex = Convert.ToInt32(drive_info["Index"]); }
                    catch { continue; }
                    //
                    if (!seenDisks.Add(diskIndex))
                        continue;
                    var logicalDisks = new List<ManagementObject>();
                    //
                    try{
                        var disk_part_text_query = $"associators of {{{drive_info.Path.RelativePath}}} where AssocClass = Win32_DiskDriveToDiskPartition";
                        using (var disk_part_query = new ManagementObjectSearcher(disk_part_text_query)){
                            foreach (ManagementObject disk_partition in disk_part_query.Get().Cast<ManagementObject>()){
                                var logical_disk_text_query = $"associators of {{{disk_partition.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                                using (var logical_disk_query = new ManagementObjectSearcher(logical_disk_text_query)){
                                    logicalDisks.AddRange(logical_disk_query.Get().Cast<ManagementObject>());
                                }
                            }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
                    }
                    // DISK CAPTION
                    try{
                        var disk_caption = Convert.ToString(drive_info["Caption"])?.Trim();
                        if (!string.IsNullOrEmpty(disk_caption))
                            DISK_Selector_List.Items.Add(disk_caption);
                        else
                            DISK_Selector_List.Items.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
                    }
                    // DISK MANUFACTURER
                    try{
                        string Normalize(string s){
                            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                            s = s.Trim().ToLowerInvariant().Replace("ı", "i");
                            var sb = new StringBuilder(s.Length);
                            bool prevSpace = false;
                            foreach (char ch in s){
                                char c = ch;
                                if (c == '_' || c == '-' || c == '/' || c == '\\' || c == '.' || c == ',' || c == ';' || c == ':' || c == '(' || c == ')' || c == '[' || c == ']') c = ' ';
                                if (char.IsWhiteSpace(c)){
                                    if (!prevSpace){
                                        sb.Append(' ');
                                        prevSpace = true;
                                    }
                                    continue;
                                }
                                sb.Append(c);
                                prevSpace = false;
                            }
                            return sb.ToString().Trim();
                        }
                        bool ContainsIgnoreCase(string haystack, string needle){
                            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle)) return false;
                            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        bool IsUselessManufacturer(string manNorm){
                            if (string.IsNullOrEmpty(manNorm)) return true;
                            string[] junk = { "nvme", "usb", "ata", "sata", "scsi", "msft", "microsoft", "standard", "generic" };
                            for (int k = 0; k < junk.Length; k++){
                                if (string.Equals(manNorm, junk[k], StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }
                            return false;
                        }
                        var brandRules = new List<KeyValuePair<string[], string>>(){
                            new KeyValuePair<string[], string>(new[]{ "acer" }, "Acer"),
                            new KeyValuePair<string[], string>(new[]{ "a data", "a-data", "adata" }, "Adata"),
                            new KeyValuePair<string[], string>(new[]{ "addlink" }, "Addlink"),
                            new KeyValuePair<string[], string>(new[]{ "alpin" }, "Alpin"),
                            new KeyValuePair<string[], string>(new[]{ "apacer" }, "Apacer"),
                            new KeyValuePair<string[], string>(new[]{ "apple" }, "Apple"),
                            new KeyValuePair<string[], string>(new[]{ "asus" }, "ASUS"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "biostar" }, "Biostar"),
                            new KeyValuePair<string[], string>(new[]{ "buffalo" }, "Buffalo"),
                            new KeyValuePair<string[], string>(new[]{ "busbi" }, "Busbi"),
                            new KeyValuePair<string[], string>(new[]{ "buslink" }, "Buslink"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "centon" }, "Centon"),
                            new KeyValuePair<string[], string>(new[]{ "cm storm", "cmstorm" }, "CM Storm"),
                            new KeyValuePair<string[], string>(new[]{ "codegen" }, "Codegen"),
                            new KeyValuePair<string[], string>(new[]{ "colorful" }, "Colorful"),
                            new KeyValuePair<string[], string>(new[]{ "corsair" }, "Corsair"),
                            new KeyValuePair<string[], string>(new[]{ "crucial" }, "Crucial"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "datawrite" }, "DataWrite"),
                            new KeyValuePair<string[], string>(new[]{ "duracell" }, "Duracell"),
                            new KeyValuePair<string[], string>(new[]{ "dynamode" }, "Dynamode"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "emtec" }, "Emtec"),
                            new KeyValuePair<string[], string>(new[]{ "excelstor" }, "ExcelStor"),
                            new KeyValuePair<string[], string>(new[]{ "ezcool" }, "EZcool"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "fuji" }, "Fuji"),
                            new KeyValuePair<string[], string>(new[]{ "fujitsu" }, "Fujitsu"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "geil" }, "GeIL"),
                            new KeyValuePair<string[], string>(new[]{ "gigabyte" }, "Gigabyte"),
                            new KeyValuePair<string[], string>(new[]{ "goodram" }, "Goodram"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "hi level", "hi-level" }, "Hi-Level"),
                            new KeyValuePair<string[], string>(new[]{ "hikvision" }, "Hikvision"),
                            new KeyValuePair<string[], string>(new[]{ "hitachi" }, "Hitachi"),
                            new KeyValuePair<string[], string>(new[]{ "hp" }, "HP"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "imation" }, "Imation"),
                            new KeyValuePair<string[], string>(new[]{ "intel" }, "Intel"),
                            new KeyValuePair<string[], string>(new[]{ "intenso" }, "Intenso"),
                            new KeyValuePair<string[], string>(new[]{ "iomega" }, "Iomega"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "james donkey" }, "James Donkey"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "kanguru" }, "Kanguru"),
                            new KeyValuePair<string[], string>(new[]{ "kingmax" }, "Kingmax"),
                            new KeyValuePair<string[], string>(new[]{ "kingston" }, "Kingston"),
                            new KeyValuePair<string[], string>(new[]{ "kingspec" }, "Kingspec"),
                            new KeyValuePair<string[], string>(new[]{ "kioxia" }, "Kioxia"),
                            new KeyValuePair<string[], string>(new[]{ "kodak" }, "Kodak"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "lacie" }, "LaCie"),
                            new KeyValuePair<string[], string>(new[]{ "lenovo" }, "Lenovo"),
                            new KeyValuePair<string[], string>(new[]{ "leven" }, "Leven"),
                            new KeyValuePair<string[], string>(new[]{ "lexar" }, "Lexar"),
                            new KeyValuePair<string[], string>(new[]{ "liteon", "lite-on" }, "Lite-On"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "maxtor" }, "Maxtor"),
                            new KeyValuePair<string[], string>(new[]{ "maxell" }, "Maxell"),
                            new KeyValuePair<string[], string>(new[]{ "memorex" }, "Memorex"),
                            new KeyValuePair<string[], string>(new[]{ "micron" }, "Micron"),
                            new KeyValuePair<string[], string>(new[]{ "minox" }, "Minox"),
                            new KeyValuePair<string[], string>(new[]{ "mio" }, "Mio"),
                            new KeyValuePair<string[], string>(new[]{ "mld" }, "MLD"),
                            new KeyValuePair<string[], string>(new[]{ "msi" }, "MSI"),
                            new KeyValuePair<string[], string>(new[]{ "mushkin" }, "Mushkin"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "nec" }, "NEC"),
                            new KeyValuePair<string[], string>(new[]{ "neo forza", "neoforza" }, "Neo Forza"),
                            new KeyValuePair<string[], string>(new[]{ "netac" }, "Netac"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "ocz" }, "OCZ"),
                            new KeyValuePair<string[], string>(new[]{ "olympus" }, "Olympus"),
                            new KeyValuePair<string[], string>(new[]{ "optiarc" }, "Optiarc"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "panasonic" }, "Panasonic"),
                            new KeyValuePair<string[], string>(new[]{ "patriot" }, "Patriot"),
                            new KeyValuePair<string[], string>(new[]{ "philips" }, "Philips"),
                            new KeyValuePair<string[], string>(new[]{ "pinnacle" }, "Pinnacle"),
                            new KeyValuePair<string[], string>(new[]{ "pioneer" }, "Pioneer"),
                            new KeyValuePair<string[], string>(new[]{ "plextor" }, "Plextor"),
                            new KeyValuePair<string[], string>(new[]{ "pny", "pnypqi" }, "PNY"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "qnap" }, "QNAP"),
                            new KeyValuePair<string[], string>(new[]{ "quantum" }, "Quantum"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "ramsta" }, "Ramsta"),
                            new KeyValuePair<string[], string>(new[]{ "ricoh" }, "Ricoh"),
                            new KeyValuePair<string[], string>(new[]{ "rokit" }, "Rokit"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "samsung" }, "Samsung"),
                            new KeyValuePair<string[], string>(new[]{ "sandisk" }, "SanDisk"),
                            new KeyValuePair<string[], string>(new[]{ "seagate" }, "Seagate"),
                            new KeyValuePair<string[], string>(new[]{ "sigma" }, "Sigma"),
                            new KeyValuePair<string[], string>(new[]{ "siliconpower", "silicon power" }, "Silicon Power"),
                            new KeyValuePair<string[], string>(new[]{ "sk hynix", "skhynix", "hynix" }, "SK hynix"),
                            new KeyValuePair<string[], string>(new[]{ "smartbuy" }, "SmartBuy"),
                            new KeyValuePair<string[], string>(new[]{ "sony" }, "Sony"),
                            new KeyValuePair<string[], string>(new[]{ "super talent", "supertalent" }, "Super Talent"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "team" }, "Team"),
                            new KeyValuePair<string[], string>(new[]{ "teac" }, "TEAC"),
                            new KeyValuePair<string[], string>(new[]{ "toshiba" }, "Toshiba"),
                            new KeyValuePair<string[], string>(new[]{ "transcend" }, "Transcend"),
                            new KeyValuePair<string[], string>(new[]{ "traxdata" }, "Traxdata"),
                            new KeyValuePair<string[], string>(new[]{ "trimble" }, "Trimble"),
                            new KeyValuePair<string[], string>(new[]{ "turbox" }, "Turbox"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "verbatim" }, "Verbatim"),
                            new KeyValuePair<string[], string>(new[]{ "verico" }, "Verico"),
                            new KeyValuePair<string[], string>(new[]{ "viking" }, "Viking"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "western digital", "wd black", "wd_black", "wdblack", "wd" }, "Western Digital"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "xerox" }, "Xerox"),
                            new KeyValuePair<string[], string>(new[]{ "xmedia" }, "XMedia"),
                            //
                            new KeyValuePair<string[], string>(new[]{ "zotac" }, "Zotac"),
                            new KeyValuePair<string[], string>(new[]{ "zoostorm" }, "Zoostorm")
                        };
                        string DetectBrand(string modelNorm, string manNorm){
                            for (int r = 0; r < brandRules.Count; r++){
                                var aliases = brandRules[r].Key;
                                var display = brandRules[r].Value;
                                for (int a = 0; a < aliases.Length; a++){
                                    string an = Normalize(aliases[a]);
                                    if (!string.IsNullOrEmpty(manNorm) && ContainsIgnoreCase(manNorm, an))
                                        return display;
                                    if (!string.IsNullOrEmpty(modelNorm) && ContainsIgnoreCase(modelNorm, an))
                                        return display;
                                }
                            }
                            return string.Empty;
                        }
                        using (var q = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT Model, Manufacturer FROM MSFT_Disk WHERE Number={diskIndex}")){
                            foreach (ManagementObject row in q.Get().Cast<ManagementObject>()){
                                // MODEL
                                var modelRaw = Convert.ToString(row["Model"]);
                                if (modelRaw != null) modelRaw = modelRaw.Trim();
                                var model = !string.IsNullOrEmpty(modelRaw) ? modelRaw : software_lang.TSReadLangs("StorageContent", "se_c_unknown");
                                //
                                disk_model_list.Add(model);
                                //
                                var manRaw = Convert.ToString(row["Manufacturer"]);
                                if (manRaw != null) manRaw = manRaw.Trim();
                                if (manRaw == null) manRaw = string.Empty;
                                string modelNorm = Normalize(modelRaw ?? "");
                                string manNorm = Normalize(manRaw);
                                if (IsUselessManufacturer(manNorm))
                                    manNorm = string.Empty;
                                // detect
                                string detected = DetectBrand(modelNorm, manNorm);
                                if (!string.IsNullOrEmpty(detected)){
                                    disk_man_list.Add(detected);
                                }else{
                                    // fallback
                                    if (!string.IsNullOrEmpty(manRaw) && !IsUselessManufacturer(Normalize(manRaw)))
                                        disk_man_list.Add(manRaw);
                                    else
                                        disk_man_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                }
                                break;
                            }
                        }
                    }catch{
                        disk_model_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_man_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK VOLUME ID
                    try{
                        var volumeIds = logicalDisks.Select(ld => Convert.ToString(ld["Name"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).Select(s => s + @"\").ToList();
                        disk_volume_id_list.Add(volumeIds.Count > 0 ? string.Join(", ", volumeIds) : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }catch{
                        disk_volume_id_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK VOLUME NAME
                    try{
                        var volumeNames = logicalDisks.Select(ld => Convert.ToString(ld["VolumeName"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        disk_volume_name_list.Add(volumeNames.Count > 0 ? string.Join(", ", volumeNames) : software_lang.TSReadLangs("StorageContent", "se_c_local_disk"));
                    }catch{
                        disk_volume_name_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_local_disk"));
                    }
                    // DISK FIRMWARE VERSION
                    try{
                        var disk_firmware_version = Convert.ToString(drive_info["FirmwareRevision"])?.Trim();
                        disk_firmware_list.Add(!string.IsNullOrEmpty(disk_firmware_version) ? disk_firmware_version : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }catch{
                        disk_firmware_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK SERIAL NUMBER
                    try{
                        var disk_serial_number = Convert.ToString(drive_info["SerialNumber"])?.Trim();
                        if (!string.IsNullOrEmpty(disk_serial_number)){
                            if (hiding_mode_wrapper != 1)
                                disk_serial_list.Add(disk_serial_number);
                            else
                                disk_serial_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }else{
                            disk_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        }
                    }catch{
                        disk_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK VOLUME SERIAL NUMBER
                    try{
                        var volSerials = logicalDisks.Select(ld => {
                            string raw = Convert.ToString(ld["VolumeSerialNumber"])?.Trim();
                            if (!string.IsNullOrEmpty(raw) && raw.Length == 8)
                                return raw.Insert(4, "-").ToUpper();
                            return raw;
                        }).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        if (volSerials.Count == 0){
                            disk_volume_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        }else{
                            if (hiding_mode_wrapper != 1)
                                disk_volume_serial_list.Add(string.Join(", ", volSerials));
                            else
                                disk_volume_serial_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }
                    }catch{
                        disk_volume_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK SPACE & FREE & USED               
                    try{
                        // TOTAL SPACE
                        double diskSizeBytes = Convert.ToDouble(drive_info["Size"]);
                        // FREE SPACE
                        double freeBytes = logicalDisks.Sum(ld => {
                            try { return Convert.ToDouble(ld["FreeSpace"]); }
                            catch { return 0d; }
                        });
                        // USED SPACE
                        double usedBytes = diskSizeBytes - freeBytes;
                        if (usedBytes < 0) usedBytes = 0;
                        disk_total_space_list.Add(TS_FormatSize(diskSizeBytes));
                        disk_used_space_list.Add(TS_FormatSize(usedBytes));
                        disk_free_space_list.Add(TS_FormatSize(freeBytes));
                    }catch{
                        disk_total_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_used_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_free_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK FILE SYSTEM
                    try{
                        var fileSystems = logicalDisks.Select(ld => Convert.ToString(ld["FileSystem"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        disk_file_system_list.Add(fileSystems.Count > 0 ? string.Join(", ", fileSystems) : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }catch{
                        disk_file_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK FORMATTING SYSTEM
                    try{
                        using (var disk_part_search = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT PartitionStyle FROM MSFT_Disk WHERE Number={diskIndex}")){
                            foreach (ManagementObject dp_search in disk_part_search.Get().Cast<ManagementObject>()){
                                var disk_part_style = Convert.ToInt32(dp_search["PartitionStyle"]);
                                switch (disk_part_style){
                                    case 0: disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); break;
                                    case 1: disk_formatting_system_list.Add("MBR"); break;
                                    case 2: disk_formatting_system_list.Add("GPT"); break;
                                }
                                break;
                            }
                        }
                    }catch{
                        disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK TYPE + COUNTERS
                    try{
                        var disk_media_type = Convert.ToString(drive_info["MediaType"])?.ToLower().Trim() ?? "";
                        using (var search_disk_type = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT MediaType FROM MSFT_PhysicalDisk WHERE DeviceID={diskIndex}")){
                            foreach (ManagementObject search_disk_t in search_disk_type.Get().Cast<ManagementObject>()){
                                var disk_type = Convert.ToInt32(search_disk_t["MediaType"]);
                                switch (disk_type){
                                    case 0:
                                        switch (disk_media_type){
                                            case "external hard disk media":
                                                disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_external_disk"));
                                                disk_usb_count++;
                                                break;
                                            case "removable media":
                                                disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_usb_disk"));
                                                disk_usb_count++;
                                                break;
                                            default:
                                                disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                                break;
                                        }
                                        break;
                                    case 3:
                                        disk_type_list.Add("HDD");
                                        disk_hdd_count++;
                                        break;
                                    case 4:
                                        disk_type_list.Add("SSD");
                                        disk_ssd_count++;
                                        break;
                                    case 5:
                                        disk_type_list.Add("SCM");
                                        break;
                                }
                                // DRIVE TYPE text
                                switch (disk_media_type){
                                    case "fixed hard disk media":
                                        disk_drive_type_list.Add(disk_type == 4 ? software_lang.TSReadLangs("StorageContent", "se_c_ssd") : software_lang.TSReadLangs("StorageContent", "se_c_fixed_drive"));
                                        break;
                                    case "external hard disk media":
                                        disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_external_drive"));
                                        break;
                                    case "removable media":
                                        disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_removable_drive"));
                                        break;
                                    default:
                                        disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                        break;
                                }
                                break;
                            }
                        }
                    }catch{
                        disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK INTERFACE
                    try{
                        using (var disk_interface = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT BusType FROM MSFT_Disk WHERE Number={diskIndex}")){
                            foreach (ManagementObject disk_int_query in disk_interface.Get().Cast<ManagementObject>()){
                                var di_query = Convert.ToInt32(disk_int_query["BusType"]);
                                var busTypeMessages = new Dictionary<int, string>{
                                    { 0, software_lang.TSReadLangs("StorageContent", "se_c_unknown") },
                                    { 1, "SCSI" },
                                    { 2, "ATAPI" },
                                    { 3, "ATA" },
                                    { 4, "1394 - IEEE 1394" },
                                    { 5, "SSA" },
                                    { 6, software_lang.TSReadLangs("StorageContent", "se_c_fiber_channel") },
                                    { 7, "USB" },
                                    { 8, "RAID" },
                                    { 9, "iSCSI" },
                                    { 10, software_lang.TSReadLangs("StorageContent", "se_c_scsi_sas") },
                                    { 11, "SATA" },
                                    { 12, software_lang.TSReadLangs("StorageContent", "se_c_secure_digital") },
                                    { 13, software_lang.TSReadLangs("StorageContent", "se_c_multi_media_card") },
                                    { 14, software_lang.TSReadLangs("StorageContent", "se_c_virtual") },
                                    { 15, software_lang.TSReadLangs("StorageContent", "se_c_file_supported_virtual") },
                                    { 16, software_lang.TSReadLangs("StorageContent", "se_c_storage_area") },
                                    { 17, "NVM-e" }
                                };
                                if (busTypeMessages.TryGetValue(di_query, out string busTypeMessage))
                                    disk_interface_list.Add(busTypeMessage);
                                else
                                    disk_interface_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                break;
                            }
                        }
                    }catch{
                        disk_interface_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK PARTITION COUNT
                    try{
                        var disk_partitions = Convert.ToString(drive_info["Partitions"])?.Trim();
                        disk_partition_list.Add(!string.IsNullOrEmpty(disk_partitions) ? disk_partitions : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }catch{
                        disk_partition_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK MEDIA LOADED
                    try{
                        var disk_media_loaded = Convert.ToBoolean(drive_info["MediaLoaded"]);
                        disk_media_loaded_list.Add(disk_media_loaded ? software_lang.TSReadLangs("StorageContent", "se_c_disk_write_and_read") : software_lang.TSReadLangs("StorageContent", "se_c_disk_not_write_and_read"));
                    }catch { disk_media_loaded_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); }
                    // DISK STATUS
                    try{
                        var disk_media_status = (Convert.ToString(drive_info["Status"]) ?? "").ToLower().Trim();
                        var mediaStatusMessages = new Dictionary<string, string>{
                            { "ok", software_lang.TSReadLangs("StorageContent", "se_c_disk_stable") },
                            { "error", software_lang.TSReadLangs("StorageContent", "se_c_disk_error") },
                            { "degraded", software_lang.TSReadLangs("StorageContent", "se_c_disk_broken") },
                            { "unknown", software_lang.TSReadLangs("StorageContent", "se_c_disk_status_non") },
                            { "pred fail", software_lang.TSReadLangs("StorageContent", "se_c_pred_fail") },
                            { "starting", software_lang.TSReadLangs("StorageContent", "se_c_starting") },
                            { "stopping", software_lang.TSReadLangs("StorageContent", "se_c_stopped") },
                            { "service", software_lang.TSReadLangs("StorageContent", "se_c_serive") },
                            { "stressed", software_lang.TSReadLangs("StorageContent", "se_c_stressed") },
                            { "nonrecover", software_lang.TSReadLangs("StorageContent", "se_c_cannot_be_fixed") },
                            { "no contact", software_lang.TSReadLangs("StorageContent", "se_c_no_contact") },
                            { "lost comm", software_lang.TSReadLangs("StorageContent", "se_c_lost_comm") }
                        };
                        if (mediaStatusMessages.TryGetValue(disk_media_status, out string message))
                            disk_media_status_list.Add(message);
                        else
                            disk_media_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_disk_status_non"));
                    }catch{
                        disk_media_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_disk_status_non"));
                    }
                    // DISK HEALTH + BOOT FLAGS
                    try{
                        using (var search_disk_inf_4 = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT HealthStatus, BootFromDisk, IsBoot FROM MSFT_Disk WHERE Number={diskIndex}")){
                            foreach (ManagementObject sdi_4 in search_disk_inf_4.Get().Cast<ManagementObject>()){
                                var disk_health = Convert.ToInt32(sdi_4["HealthStatus"]);
                                switch (disk_health){
                                    case 0: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_healt_good")); break;
                                    case 1: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_healt_running_and_error")); break;
                                    case 2: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_defective")); break;
                                    default: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); break;
                                }
                                disk_boot_list.Add(Convert.ToBoolean(sdi_4["BootFromDisk"]) ? software_lang.TSReadLangs("StorageContent", "se_c_main_boot_disk") : software_lang.TSReadLangs("StorageContent", "se_c_not_main_boot_disk"));
                                disk_bootable_list.Add(Convert.ToBoolean(sdi_4["IsBoot"]) ? software_lang.TSReadLangs("StorageContent", "se_c_bootable_disk") : software_lang.TSReadLangs("StorageContent", "se_c_not_bootable_disk"));
                                break;
                            }
                        }
                    }catch{
                        disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_boot_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                        disk_bootable_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // DISK COMPRESSED
                    try{
                        bool anyCompressed = logicalDisks.Any(ld =>{
                            try { return Convert.ToBoolean(ld["Compressed"]); }
                            catch { return false; }
                        });
                        disk_drive_compressed_list.Add(anyCompressed ? software_lang.TSReadLangs("StorageContent", "se_c_compressed") : software_lang.TSReadLangs("StorageContent", "se_c_not_compressed"));
                    }catch{
                        disk_drive_compressed_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                    }
                    // MB BIOS TYPE
                    try{
                        using (var search_bios_type = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT PartitionStyle FROM MSFT_Disk WHERE Number={diskIndex}")){
                            foreach (ManagementObject sbt in search_bios_type.Get().Cast<ManagementObject>()){
                                var osLetter = Program.windows_disk.Replace("\\", string.Empty).Trim();
                                bool isOsOnThisDisk = logicalDisks.Any(ld =>{
                                    try{
                                        var name = Convert.ToString(ld["Name"])?.Trim();
                                        return !string.IsNullOrEmpty(name) && string.Equals(name, osLetter, StringComparison.OrdinalIgnoreCase);
                                    }
                                    catch { return false; }
                                });
                                if (isOsOnThisDisk){
                                    var disk_style = Convert.ToInt32(sbt["PartitionStyle"]);
                                    if (disk_style == 1) MB_BiosMode_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_old");
                                    else if (disk_style == 2) MB_BiosMode_V.Text = "UEFI";
                                    else MB_BiosMode_V.Text = software_lang.TSReadLangs("Mb_Content", "mb_c_old");
                                }
                                break;
                            }
                        }
                    }catch { }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
            }
            // BITLOCKER (DISK-LEVEL AGGREGATE)
            try{
                for (int i = 0; i <= disk_volume_id_list.Count - 1; i++){
                    var letters = disk_volume_id_list[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Replace("\\", "")).Where(x => x.Length >= 2 && x[1] == ':').Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    //
                    string unkP = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_unknown");
                    string unkC = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_status_unknown");
                    string unkE = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_unknown");
                    //
                    if (letters.Count == 0){
                        disk_bitlocker_status_list.Add(unkP);
                        disk_bitlocker_conversionstatus_list.Add(unkC);
                        disk_bitlocker_encryptionmethod_list.Add(unkE);
                        continue;
                    }
                    //
                    var perVolProtection = new List<string>();
                    var perVolConversion = new List<string>();
                    var perVolEncryption = new List<string>();
                    //
                    foreach (var letter in letters){
                        using (var get_bitlocker_status = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", $"SELECT ProtectionStatus, ConversionStatus, EncryptionMethod FROM Win32_EncryptableVolume WHERE DriveLetter = '{letter}'")){
                            var results = get_bitlocker_status.Get().Cast<ManagementObject>().ToList();
                            if (results.Count == 0){
                                perVolProtection.Add(unkP);
                                perVolConversion.Add(unkC);
                                perVolEncryption.Add(unkE);
                                continue;
                            }
                            foreach (var query_bitlocker in results){
                                // PROTECTION STATUS
                                try{
                                    string bl_protection_status = Convert.ToString(query_bitlocker["ProtectionStatus"]);
                                    var protectionStatusMessages = new Dictionary<int, string>{
                                        { 0, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_off") },
                                        { 1, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_on") },
                                        { 2, unkP }
                                    };
                                    if (!string.IsNullOrEmpty(bl_protection_status)){
                                        int v = Convert.ToInt32(bl_protection_status);
                                        perVolProtection.Add(protectionStatusMessages.TryGetValue(v, out string msg) ? msg : unkP);
                                    }else{
                                        perVolProtection.Add(unkP);
                                    }
                                }catch { perVolProtection.Add(unkP); }
                                // CONVERSION STATUS
                                try{
                                    string bl_conversion_status = Convert.ToString(query_bitlocker["ConversionStatus"]);
                                    var conversionStatusMessages = new Dictionary<int, string>{
                                        { 0, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_no_pass") },
                                        { 1, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_yes_pass") },
                                        { 2, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_encrypt_continue") },
                                        { 3, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_decrypt_continue") },
                                        { 4, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_encrypt_paused") },
                                        { 5, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_decrypt_paused") }
                                    };
                                    if (!string.IsNullOrEmpty(bl_conversion_status)){
                                        int v = Convert.ToInt32(bl_conversion_status);
                                        perVolConversion.Add(conversionStatusMessages.TryGetValue(v, out string msg) ? msg : unkC);
                                    }else{
                                        perVolConversion.Add(unkC);
                                    }
                                }catch { perVolConversion.Add(unkC); }
                                // ENCRYPTION METHOD
                                try{
                                    string bl_encryption_method = Convert.ToString(query_bitlocker["EncryptionMethod"]);
                                    var encryptionMethodMessages = new Dictionary<int, string>{
                                        { 0, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_no_pass") },
                                        { 1, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_aes128d") },
                                        { 2, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_aes256d") },
                                        { 3, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_aes128") },
                                        { 4, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_aes256") },
                                        { 5, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_hardware") },
                                        { 6, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_xts_aes128") },
                                        { 7, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_xts_aes256d") }
                                    };
                                    if (!string.IsNullOrEmpty(bl_encryption_method)){
                                        int v = Convert.ToInt32(bl_encryption_method);
                                        perVolEncryption.Add(encryptionMethodMessages.TryGetValue(v, out string msg) ? msg : unkE);
                                    }else{
                                        perVolEncryption.Add(unkE);
                                    }
                                }catch { perVolEncryption.Add(unkE); }
                                break;
                            }
                        }
                    }
                    bool isUsbOrExternal = false;
                    try{
                        string t = (disk_type_list.Count > i ? (disk_type_list[i] ?? "") : "").ToLowerInvariant();
                        isUsbOrExternal = t.Contains("usb") || t.Contains("external");
                    }catch { }
                    string ReduceToSingle(List<string> vals, string mixedLabel, string unknownValue){
                        vals = vals.Where(v => !string.IsNullOrEmpty(v)).ToList();
                        if (vals.Count == 0) return unknownValue;
                        var filtered = vals.Where(v => !string.Equals(v, unknownValue, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (filtered.Count == 0) return unknownValue;
                        return filtered.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1 ? filtered[0] : mixedLabel;
                    }
                    //
                    string mixedText = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_mixed");
                    string p = ReduceToSingle(perVolProtection, mixedText, unkP);
                    string c = ReduceToSingle(perVolConversion, mixedText, unkC);
                    string e = ReduceToSingle(perVolEncryption, mixedText, unkE);
                    //
                    if (isUsbOrExternal){
                        if (p == mixedText) p = unkP;
                        if (c == mixedText) c = unkC;
                        if (e == mixedText) e = unkE;
                    }
                    //
                    disk_bitlocker_status_list.Add(p);
                    disk_bitlocker_conversionstatus_list.Add(c);
                    disk_bitlocker_encryptionmethod_list.Add(e);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
            }
            // SELECT DISK
            try{
                int c_index = disk_volume_id_list.FindIndex(x => x.Contains(Program.windows_disk));
                DISK_Selector_List.SelectedIndex = c_index;
                if (c_index == -1){
                    DISK_Selector_List.SelectedIndex = 0;
                }
            }catch (Exception){
                DISK_Selector_List.SelectedIndex = 0;
            }
            // DISK COUNTER RENDERER
            DISK_TTLP_P1_L2.Text = disk_ssd_count.ToString();
            DISK_TTLP_P2_L2.Text = disk_hdd_count.ToString();
            DISK_TTLP_P3_L2.Text = disk_usb_count.ToString();
            DISK_TTLP_P4_L2.Text = DISK_Selector_List.Items.Count.ToString();
            // DISK PROCESS END ENABLED
            DISK_RotateBtn.Enabled = true;
            ((Control)DISK).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Storage Section Loaded --->");
            }
        }
        // DISK RIGHT PROGRESS FUNCTION
        private void Disk_progress_function(int _pb_disk){
            try{
                string totalDiskSpaceStr = disk_total_space_list[_pb_disk];
                string freeDiskSpaceStr = disk_free_space_list[_pb_disk];
                //
                long ConvertToMB(string sizeStr){
                    sizeStr = sizeStr.Trim().ToUpper();
                    //
                    string pattern = @"([\d.,]+)\s*(B|KB|MB|GB|TB|PB|EB|ZB|YB)";
                    Match match = Regex.Match(sizeStr, pattern);
                    string numericValueStr = match.Groups[1].Value.Replace(',', '.');
                    string unit = match.Groups[2].Value;
                    //
                    double numericValue = double.Parse(numericValueStr, CultureInfo.InvariantCulture);
                    string[] unitScale = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                    int unitIndex = Array.IndexOf(unitScale, unit);
                    double sizeInMB = unitIndex >= 2 ? numericValue * Math.Pow(1024, unitIndex - 2) : numericValue / Math.Pow(1024, 2 - unitIndex);
                    //
                    return (long)Math.Floor(sizeInMB);
                }
                //
                long totalDiskSpaceMB = ConvertToMB(totalDiskSpaceStr);
                long freeDiskSpaceMB = ConvertToMB(freeDiskSpaceStr);
                long usedDiskSpaceMB = totalDiskSpaceMB - freeDiskSpaceMB;
                int usagePercentage = (int)((double)usedDiskSpaceMB / totalDiskSpaceMB * 100);
                //
                DISK_PBar_FE.Height = (int)(DISK_PBar_BG.Height * (usagePercentage / 100.0));
                DISK_PBar_Label.Text = usagePercentage + "%";
                if (usagePercentage <= 7){
                    DISK_PBar_Label.Top = DISK_PBar_FE.Top - 2;
                }else{
                    DISK_PBar_Label.Top = DISK_PBar_FE.Top + 6;
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Disk_progress_function()"); }
            }
        }
        private void DISK_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int disk_percent = DISK_Selector_List.SelectedIndex;
                try { Disk_progress_function(disk_percent); } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Model_V.Text = disk_model_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Man_V.Text = disk_man_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_VolumeID_V.Text = disk_volume_id_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_VolumeName_V.Text = disk_volume_name_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Firmware_V.Text = disk_firmware_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Serial_V.Text = disk_serial_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_VolumeSerial_V.Text = disk_volume_serial_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Size_V.Text = disk_total_space_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_UsedSpace_V.Text = disk_used_space_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_FreeSpace_V.Text = disk_free_space_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_FileSystem_V.Text = disk_file_system_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_FormattingType_V.Text = disk_formatting_system_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Type_V.Text = disk_type_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_DriveType_V.Text = disk_drive_type_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_InterFace_V.Text = disk_interface_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_PartitionCount_V.Text = disk_partition_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_MediaLoaded_V.Text = disk_media_loaded_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_MediaStatus_V.Text = disk_media_status_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Health_V.Text = disk_health_status_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Boot_V.Text = disk_boot_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_Bootable_V.Text = disk_bootable_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_BitLockerStatus_V.Text = disk_bitlocker_status_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_BitLockerConversionStatus_V.Text = disk_bitlocker_conversionstatus_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_BitLockerEncryptMehod_V.Text = disk_bitlocker_encryptionmethod_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
                try { DISK_DriveCompressed_V.Text = disk_drive_compressed_list[disk_percent]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
        }
        #endregion
        #region NETWORK_Section
        // NETWORK
        // ======================================================================================================
        readonly List<string> network_mac_adress_list = new List<string>();
        readonly List<string> network_man_list = new List<string>();
        readonly List<string> network_driver_version_list = new List<string>();
        readonly List<string> network_driver_date_list = new List<string>();
        readonly List<string> network_service_name_list = new List<string>();
        readonly List<string> network_adaptor_type_list = new List<string>();
        readonly List<string> network_physical_list = new List<string>();
        readonly List<string> network_device_id_list = new List<string>();
        readonly List<string> network_guid_list = new List<string>();
        readonly List<string> network_connection_type_list = new List<string>();
        readonly List<string> network_dhcp_status_list = new List<string>();
        readonly List<string> network_dhcp_server_list = new List<string>();
        readonly List<string> network_dhcp_first_ip_time_list = new List<string>();
        readonly List<string> network_dhcp_last_ip_time_list = new List<string>();
        readonly List<string> network_connection_speed_list = new List<string>();
        readonly List<string> network_ipv4_list = new List<string>();
        readonly List<string> network_ipv6_list = new List<string>();
        // DNS PROVIDER CLASS
        // -----------------------------------------
        public class DnsProvider{
            public string Name { get; }
            public List<string> IPv4 { get; }
            public List<string> IPv6 { get; }
            public int Id { get; }
            public List<string> DnsAddresses{
                get{
                    var all = new List<string>(IPv4.Count + IPv6.Count);
                    all.AddRange(IPv4);
                    all.AddRange(IPv6);
                    return all;
                }
            }
            public DnsProvider(string name, List<string> ipv4, List<string> ipv6, int id){
                Name = name;
                IPv4 = ipv4 ?? new List<string>();
                IPv6 = ipv6 ?? new List<string>();
                Id = id;
            }
        }
        public static readonly List<DnsProvider> DnsProviders = new List<DnsProvider>{
            new DnsProvider("AdGuard DNS",
                new List<string> { "94.140.14.14", "94.140.15.15" },
                new List<string> { "2a10:50c0::ad1:ff", "2a10:50c0::ad2:ff" }, 0
            ),
            new DnsProvider("Cloudflare",
                new List<string> { "1.1.1.1", "1.0.0.1" },
                new List<string> { "2606:4700:4700::1111", "2606:4700:4700::1001" }, 1
            ),
            new DnsProvider("Comodo",
                new List<string> { "8.26.56.26", "8.20.247.20" },
                new List<string>(), 2
            ),
            new DnsProvider("Control D",
                new List<string> { "76.76.2.0", "76.76.10.0" },
                new List<string> { "2606:1a40::2", "2606:1a40::10" }, 3
            ),
            new DnsProvider("DNS.WATCH",
                new List<string> { "84.200.69.80", "84.200.70.40" },
                new List<string> { "2001:1608:10:25::1c04:b12f", "2001:1608:10:25::9249:d69b" }, 4
            ),
            new DnsProvider("Google",
                new List<string> { "8.8.8.8", "8.8.4.4" },
                new List<string> { "2001:4860:4860::8888", "2001:4860:4860::8844" }, 5
            ),
            new DnsProvider("Lumen DNS",
                new List<string> { "4.2.2.1", "4.2.2.2" },
                new List<string>(), 6
            ),
            new DnsProvider("NextDNS",
                new List<string> { "45.90.28.0", "45.90.30.0" },
                new List<string> { "2a07:a8c0::", "2a07:a8c1::" }, 7
            ),
            new DnsProvider("OpenDNS",
                new List<string> { "208.67.222.222", "208.67.220.220" },
                new List<string> { "2620:119:35::35", "2620:119:53::53" }, 8
            ),
            new DnsProvider("Quad9",
                new List<string> { "9.9.9.9", "149.112.112.112" },
                new List<string> { "2620:fe::fe", "2620:fe::9" }, 9
            ),
            new DnsProvider("Yandex DNS",
                new List<string> { "77.88.8.8", "77.88.8.1" },
                new List<string> { "2a02:6b8::feed:0ff", "2a02:6b8:0:1::feed:0ff" }, 10
            ),
        };
        private void Network(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            ManagementObjectSearcher search_na = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, MACAddress, Manufacturer, ServiceName, AdapterType, PhysicalAdapter, PNPDeviceID, GUID, NetConnectionID, Index, Speed FROM Win32_NetworkAdapter", queryOptions);
            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceName, DriverVersion, DriverDate FROM Win32_PnPSignedDriver WHERE DeviceClass='Net'", queryOptions);
            var driverDict = new Dictionary<string, ManagementObject>(StringComparer.OrdinalIgnoreCase);
            using (var results = searcher.Get()){
                foreach (ManagementObject d in results.Cast<ManagementObject>()){
                    string deviceName = d["DeviceName"]?.ToString() ?? "";
                    string key = Net_replacer(deviceName);
                    if (!string.IsNullOrEmpty(key) && !driverDict.ContainsKey(key)){
                        driverDict[key] = d;
                    }
                }
            }
            //
            foreach (ManagementObject query_na_rotate in search_na.Get().Cast<ManagementObject>()){
                try{
                    // NET NAME
                    NET_Selector_List.Items.Add(Convert.ToString(query_na_rotate["Name"]));
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // MAC ADRESS
                    string mac_adress = Convert.ToString(query_na_rotate["MACAddress"]);
                    if (string.IsNullOrEmpty(mac_adress)){
                        network_mac_adress_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_mac_adress"));
                    }else{
                        if (hiding_mode_wrapper != 1){
                            network_mac_adress_list.Add(mac_adress);
                        }else{
                            network_mac_adress_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }
                    }
                    NET_MacAdress_V.Text = network_mac_adress_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NET MAN
                    string net_man = Convert.ToString(query_na_rotate["Manufacturer"]);
                    if (string.IsNullOrEmpty(net_man)){
                        network_man_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_provider"));
                    }else{
                        network_man_list.Add(net_man);
                    }
                    NET_NetMan_V.Text = network_man_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    string netName = Net_replacer(Convert.ToString(query_na_rotate["Name"]) ?? "");
                    string unknown = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                    string driverVersion = unknown;
                    string driverDate = unknown;
                    if (driverDict.TryGetValue(netName, out var driverMatch)){
                        driverVersion = Convert.ToString(driverMatch["DriverVersion"]) ?? unknown;
                        string dateRaw = Convert.ToString(driverMatch["DriverDate"]);
                        if (!string.IsNullOrEmpty(dateRaw)){
                            try{
                                DateTime dt = ManagementDateTimeConverter.ToDateTime(dateRaw);
                                driverDate = dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                            }catch{
                                driverDate = unknown;
                            }
                        }
                    }
                    network_driver_version_list.Add(driverVersion);
                    network_driver_date_list.Add(driverDate);
                }catch{
                    string unknown = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                    network_driver_version_list.Add(unknown);
                    network_driver_date_list.Add(unknown);
                }
                try{
                    // SERVICE NAME
                    string service_name = Convert.ToString(query_na_rotate["ServiceName"]);
                    if (string.IsNullOrEmpty(service_name)){
                        network_service_name_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_service_name"));
                    }else{
                        network_service_name_list.Add(service_name);
                    }
                    NET_ServiceName_V.Text = network_service_name_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NET ADAPTER TYPE
                    string adaptor_type = Convert.ToString(query_na_rotate["AdapterType"]);
                    if (string.IsNullOrEmpty(adaptor_type)){
                        network_adaptor_type_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_adaptor_type"));
                    }else{
                        network_adaptor_type_list.Add(adaptor_type);
                    }
                    NET_AdapterType_V.Text = network_adaptor_type_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NET PHYSICAL
                    bool net_physical = Convert.ToBoolean(query_na_rotate["PhysicalAdapter"]);
                    if (net_physical == true){
                        network_physical_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_yes"));
                    }else if (net_physical == false){
                        network_physical_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_no"));
                    }
                    NET_Physical_V.Text = network_physical_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NETWORK DEVICE ID
                    string network_device_id = Convert.ToString(query_na_rotate["PNPDeviceID"]).Trim();
                    if (!string.IsNullOrEmpty(network_device_id)){
                        char[] split_char = { '\\' };
                        string[] network_device_split = network_device_id.Trim().Split(split_char);
                        network_device_id_list.Add($"{network_device_split[0]}\\{network_device_split[1]}");
                    }else{
                        network_device_id_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_hardware_id"));
                    }
                    NET_DeviceID_V.Text = network_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NET GUID
                    string guid = Convert.ToString(query_na_rotate["GUID"]);
                    if (string.IsNullOrEmpty(guid)){
                        network_guid_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_guid"));
                    }else{
                        string net_guid_replacer = guid.Replace("{", string.Empty).Replace("}", string.Empty);
                        if (hiding_mode_wrapper != 1){
                            network_guid_list.Add(net_guid_replacer);
                        }else{
                            network_guid_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }
                    }
                    NET_Guid_V.Text = network_guid_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // NET CONNECTION TYPE
                    string net_con_id = Convert.ToString(query_na_rotate["NetConnectionID"]);
                    if (string.IsNullOrEmpty(net_con_id)){
                        network_connection_type_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_connection_type"));
                    }else{
                        network_connection_type_list.Add(net_con_id);
                    }
                    NET_ConnectionType_V.Text = network_connection_type_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                // NETWORK ADAPTER CONFIG SECTION
                try{
                    var get_na_index = query_na_rotate["Index"];
                    ManagementObjectSearcher search_nac = new ManagementObjectSearcher("root\\CIMV2", $"SELECT DHCPEnabled, DHCPServer, DHCPLeaseObtained, DHCPLeaseExpires, IPAddress FROM Win32_NetworkAdapterConfiguration WHERE Index={get_na_index}");
                    foreach (ManagementObject query_nac_rotate in search_nac.Get().Cast<ManagementObject>()){
                        try{
                            // DHCP STATUS
                            bool dhcp_enabled = Convert.ToBoolean(query_nac_rotate["DHCPEnabled"]);
                            if (dhcp_enabled == true){
                                network_dhcp_status_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_active"));
                            }else if (dhcp_enabled == false){
                                network_dhcp_status_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_out_of_order"));
                            }
                            NET_Dhcp_status_V.Text = network_dhcp_status_list[0];
                        }catch (Exception ex){
                            if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                        }
                        try{
                            // DHCP SERVER STATUS
                            string dhcp_server = Convert.ToString(query_nac_rotate["DHCPServer"]);
                            if (string.IsNullOrEmpty(dhcp_server)){
                                network_dhcp_server_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                            }else{
                                network_dhcp_server_list.Add(dhcp_server);
                            }
                            NET_Dhcp_server_V.Text = network_dhcp_server_list[0];
                        }catch (Exception ex){
                            if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                        }
                        try{
                            // DHCP SERVER FIRST CONNECT TIME
                            string dhcp_first_ip_time = Convert.ToString(query_nac_rotate["DHCPLeaseObtained"]);
                            if (string.IsNullOrEmpty(dhcp_first_ip_time)){
                                network_dhcp_first_ip_time_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                            }else{
                                DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(dhcp_first_ip_time);
                                network_dhcp_first_ip_time_list.Add($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm:ss} ");
                            }
                            NET_DHCPFirstIpTime_V.Text = network_dhcp_first_ip_time_list[0];
                        }catch (Exception ex){
                            if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                        }
                        try{
                            // DHCP SERVER LAST CONNECT TIME
                            string dhcp_last_ip_time = Convert.ToString(query_nac_rotate["DHCPLeaseExpires"]);
                            if (string.IsNullOrEmpty(dhcp_last_ip_time)){
                                network_dhcp_last_ip_time_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                            }else{
                                DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(dhcp_last_ip_time);
                                network_dhcp_last_ip_time_list.Add($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm:ss} ");
                            }
                            NET_DHCPLastIpTime_V.Text = network_dhcp_last_ip_time_list[0];
                        }catch (Exception ex){
                            if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                // NETWORK ADAPTER CONFIG SECTION
                try{
                    string local_con_speed = Convert.ToString(query_na_rotate["Speed"]);
                    if (string.IsNullOrEmpty(local_con_speed) || local_con_speed == "Unknown"){
                        network_connection_speed_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_connect"));
                    }else{
                        double raw_speed_bps = Convert.ToDouble(local_con_speed);
                        double mbps = raw_speed_bps / 1_000_000.0;
                        double mb_s = mbps / 8.0;
                        //
                        string part1; // Mbps or Gbps
                        string part2; // MB/s or GB/s
                        //
                        if (mbps >= 1000){
                            part1 = $"{(mbps / 1000.0):0.##} Gbps";
                        }else{
                            part1 = $"{mbps:0} Mbps";
                        }
                        //
                        if (mb_s >= 1024.0){
                            part2 = $"{(mb_s / 1024.0):0.##} GB/s";
                        }else{
                            part2 = $"{mb_s:0.##} MB/s";
                        }
                        //
                        string formattedSpeed = $"{part1} - ({part2})";
                        network_connection_speed_list.Add(formattedSpeed);
                    }
                    NET_LocalConSpeed_V.Text = network_connection_speed_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
                try{
                    // IPV4 & IPV6 Adress
                    var get_na_index = query_na_rotate["Index"];
                    ManagementObjectSearcher query_na_ip = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index={get_na_index}");
                    foreach (ManagementObject search_ip in query_na_ip.Get().Cast<ManagementObject>()){
                        if (search_ip["IPAddress"] == null){
                            network_ipv4_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                            network_ipv6_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                        }else{
                            string[] arrIPAddress = (string[])(search_ip["IPAddress"]);
                            foreach (string ipAddress in arrIPAddress){
                                if (IPAddress.TryParse(ipAddress, out IPAddress address)){
                                    if (address.AddressFamily == AddressFamily.InterNetwork){
                                        if (hiding_mode_wrapper == 1){
                                            int maskLength = vis_m_property.Next(vn_range[0], vn_range[1]);
                                            network_ipv4_list.Add(new string('*', maskLength) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                                        }else{
                                            network_ipv4_list.Add(ipAddress);
                                        }
                                    }else if (address.AddressFamily == AddressFamily.InterNetworkV6){
                                        if (hiding_mode_wrapper == 1){
                                            int maskLength = vis_m_property.Next(vn_range[0], vn_range[1]);
                                            network_ipv6_list.Add(new string('*', maskLength) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                                        }else{
                                            network_ipv6_list.Add(ipAddress);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
                }
            }
            try{
                // GET PUBLIC IP & ISP AND LOCATION
                string u_info = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                string ni_connection = software_lang.TSReadLangs("Network_Content", "nk_c_not_internet_connection");
                //
                if (hiding_mode_wrapper == 1){
                    NET_P_IP_Adress_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                    NET_P_ISP_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                }
                else{
                    bool isConnected = IsNetworkAvailable().ConfigureAwait(false).GetAwaiter().GetResult();
                    if (!isConnected){
                        NET_P_IP_Adress_V.Text = ni_connection;
                        NET_P_ISP_V.Text = ni_connection;
                    }else{
                        GetPublicIPInfo.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");
                        GetPublicIPInfo.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
                        GetPublicIPInfo.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
                        //
                        var get_info = GetPublicIpInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        //
                        if (get_info != null && get_info.Success){
                            NET_P_IP_Adress_V.Text = $"{get_info.IP ?? u_info} ({get_info.Type ?? u_info})";
                            NET_P_ISP_V.Text = $"{get_info.Connection?.ISP ?? u_info}";
                        }else{
                            NET_P_IP_Adress_V.Text = u_info;
                            NET_P_ISP_V.Text = u_info;
                        }
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
            }
            try{
                // GET DNS ADRESS
                UpdateDnsLabels(NET_DNS_v4_1_V, NET_DNS_v4_2_V, NET_DNS_v6_1_V, NET_DNS_v6_2_V);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
            }
            // NETWORK SELECT
            try{
                var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback).OrderByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .ThenByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211).Select(ni => Net_replacer(ni.Description).ToLower()).ToList();
                bool found = false;
                for (int i = 0; i < NET_Selector_List.Items.Count; i++){
                    string listItem = Net_replacer(NET_Selector_List.Items[i].ToString()).ToLower();
                    if (activeInterfaces.Any(activeName => listItem.Contains(activeName) || activeName.Contains(listItem))){
                        NET_Selector_List.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found && NET_Selector_List.Items.Count > 0){
                    NET_Selector_List.SelectedIndex = 0;
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Network()"); }
            }
            // NETWORK PROCESS END ENABLED
            NET_RotateBtn.Enabled = true;
            ((Control)NETWORK).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Network Section Loaded --->");
            }
        }
        private void NET_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int network_select = NET_Selector_List.SelectedIndex;
                try { NET_MacAdress_V.Text = network_mac_adress_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_NetMan_V.Text = network_man_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_DriverVersion_V.Text = network_driver_version_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_DriverDate_V.Text = network_driver_date_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_ServiceName_V.Text = network_service_name_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_AdapterType_V.Text = network_adaptor_type_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_Physical_V.Text = network_physical_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_DeviceID_V.Text = network_device_id_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_Guid_V.Text = network_guid_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_ConnectionType_V.Text = network_connection_type_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_Dhcp_status_V.Text = network_dhcp_status_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_Dhcp_server_V.Text = network_dhcp_server_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_DHCPFirstIpTime_V.Text = network_dhcp_first_ip_time_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_DHCPLastIpTime_V.Text = network_dhcp_last_ip_time_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_LocalConSpeed_V.Text = network_connection_speed_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_IPv4Adress_V.Text = network_ipv4_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { NET_IPv6Adress_V.Text = network_ipv6_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
        }
        private async void NetBgProcess(){
            try{
                var software_lang = new TSGetLangs(lang_path);
                string activeAdapter = GetActiveNetworkAdapter();
                string not_connect = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                if (string.IsNullOrEmpty(activeAdapter)){
                    SetUI_NotConnected(not_connect);
                    return;
                }
                var category = new PerformanceCounterCategory("Network Interface");
                string[] instanceNames = category.GetInstanceNames();
                string perfCounterAdapterName = instanceNames.FirstOrDefault(name => Net_replacer(name) == Net_replacer(activeAdapter));
                if (perfCounterAdapterName == null){
                    SetUI_NotConnected(not_connect);
                    return;
                }
                using (var bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", perfCounterAdapterName))
                using (var bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", perfCounterAdapterName))
                using (var bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", perfCounterAdapterName)){
                    double rawBandwidth = bandwidthCounter.NextValue() / 1_000_000.0;
                    string bandwidthText = rawBandwidth >= 1000 ? $"{(rawBandwidth / 1000.0):0.##} Gbps" : $"{(int)Math.Round(rawBandwidth)} Mbps";
                    if (IsHandleCreated){
                        BeginInvoke(new Action(() => { NET_LT_Device_V.Text = activeAdapter; NET_LT_BandWidth_V.Text = bandwidthText; }));
                    }
                    while (loop_status){
                        DateTime startTime = DateTime.Now;
                        double mbpsSent = bytesSentCounter.NextValue() * 8 / 1_000_000.0;
                        double mbpsReceived = bytesReceivedCounter.NextValue() * 8 / 1_000_000.0;
                        if (IsHandleCreated){
                            BeginInvoke(new Action(() => { NET_LT_UL2.Text = FormatSpeed(mbpsSent); NET_LT_DL2.Text = FormatSpeed(mbpsReceived); }));
                        }
                        int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                        int nextDelay = Math.Max(10, 1000 - elapsed);
                        try{
                            await Task.Delay(nextDelay);
                        }catch (TaskCanceledException){
                            break;
                        }
                    }
                }
                string FormatSpeed(double mbps){
                    if (mbps >= 1000)
                        return $"{(mbps / 1000.0):0.00} Gbps / ({(mbps / 8 / 1000.0):0.00} GB/s)";
                    return $"{mbps:0.00} Mbps / ({(mbps / 8):0.00} MB/s)";
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "NetBgProcess()"); }
            }
        }
        private static string GetActiveNetworkAdapter(){
            try{
                ManagementObjectSearcher searcher_active_net = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, Index FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2");
                foreach (ManagementObject search_ip_enabled in searcher_active_net.Get().Cast<ManagementObject>()){
                    ManagementObjectSearcher configSearcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE Index={search_ip_enabled["Index"]} AND IPEnabled=True");
                    foreach (ManagementObject configObj in configSearcher.Get().Cast<ManagementObject>()){
                        return search_ip_enabled["Name"].ToString().Trim();
                    }
                }
            }catch (Exception){ }
            return null;
        }
        private void SetUI_NotConnected(string not_connect){
            if (!IsHandleCreated) return;
            BeginInvoke(new Action(() => {
                NET_LT_Device_V.Text = not_connect;
                NET_LT_BandWidth_V.Text = not_connect;
                NET_LT_UL2.Text = not_connect;
                NET_LT_DL2.Text = not_connect;
            }));
        }
        // GET PC IP & GETWAY IP
        private void NetBGProcessGateway(){
            try{
                var getLocalNetInfo = GetLocalNetworkInfo();
                if (this.InvokeRequired){
                    this.Invoke(new Action(() => {
                        UpdateNetworkUi(getLocalNetInfo);
                    }));
                }else{
                    UpdateNetworkUi(getLocalNetInfo);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "NetBGProcessGateway()"); }
            }
        }
        private void UpdateNetworkUi(GetLocalNetInfo getLocalNetInfo){
            if (getLocalNetInfo != null){
                NET_LT_LocalIP_V.Text = getLocalNetInfo.LocalIp.ToString();
                NET_LT_GatewayIP_V.Text = getLocalNetInfo.GatewayIp.ToString();
                NET_RotateGateway.Visible = true;
            }else{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string not_connect = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                NET_LT_LocalIP_V.Text = not_connect;
                NET_LT_GatewayIP_V.Text = not_connect;
            }
        }
        public class GetLocalNetInfo{
            public IPAddress LocalIp { get; set; }
            public IPAddress GatewayIp { get; set; }
        }
        public static GetLocalNetInfo GetLocalNetworkInfo(){
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()){
                if (ni.OperationalStatus != OperationalStatus.Up){
                    continue;
                }
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet && ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211){
                    continue;
                }
                var ipProps = ni.GetIPProperties();
                var localIp = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
                if (localIp == null){
                    continue;
                }
                var gateway = ipProps.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
                if (gateway == null){
                    continue;
                }
                return new GetLocalNetInfo{
                    LocalIp = localIp,
                    GatewayIp = gateway
                };
            }
            return null;
        }
        private void NET_RotateGateway_Click(object sender, EventArgs e){
            try{
                var gateway_url = $"http://{NET_LT_GatewayIP_V.Text.Trim()}";
                Process.Start(new ProcessStartInfo{
                    FileName = gateway_url,
                    UseShellExecute = true
                }); ;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "NET_RotateGateway_Click()"); }
            }
        }
        // CHECK PRIVATE IP & ISP AND REGION INFO
        private static readonly HttpClient GetPublicIPInfo = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        static async Task<GetIpInfo> GetPublicIpInfoAsync(){
            try{
                string get_data = await GetPublicIPInfo.GetStringAsync("https://ipwho.is/").ConfigureAwait(false);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic serialize_data = serializer.Deserialize<dynamic>(get_data);
                if (serialize_data["success"] == true){
                    return new GetIpInfo{
                        Success = serialize_data["success"],
                        IP = serialize_data["ip"],
                        Type = serialize_data["type"],
                        Connection = new ConnectionIPInfo{
                            ISP = serialize_data["connection"]?["isp"]
                        }
                    };
                }
                return null;
            }catch{
                return null;
            }
        }
        public class GetIpInfo{
            public bool Success { get; set; }
            public string IP { get; set; }
            public string Type { get; set; }
            public ConnectionIPInfo Connection { get; set; }
        }
        public class ConnectionIPInfo{
            public string ISP { get; set; }
        }
        // CHECK DNS
        private void UpdateDnsLabels(Label labelDns1, Label labelDns2, Label labelDns3, Label labelDns4){
            try{
                string ipv4_1 = "";
                string ipv4_2 = "";
                string ipv6_1 = "";
                string ipv6_2 = "";
                //
                var allInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToList();
                NetworkInterface activeInterface = allInterfaces.Where(IsUsableInterface).OrderByDescending(GetInterfacePriority).FirstOrDefault();
                bool hasNetwork = activeInterface != null;
                bool hasDns = false;
                //
                if (hasNetwork){
                    var ipProps = activeInterface.GetIPProperties();
                    var dnsList = ipProps.DnsAddresses.Where(d => d != null && !d.Equals(IPAddress.Any) && !d.Equals(IPAddress.IPv6Any) && !d.ToString().StartsWith("fec0:0:0:ffff", StringComparison.OrdinalIgnoreCase)).ToList();
                    hasDns = dnsList.Any();
                    //
                    var ipv4 = dnsList.Where(d => d.AddressFamily == AddressFamily.InterNetwork).ToList();
                    var ipv6 = dnsList.Where(d => d.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                    ipv4_1 = ipv4.ElementAtOrDefault(0)?.ToString() ?? "";
                    ipv4_2 = ipv4.ElementAtOrDefault(1)?.ToString() ?? "";
                    ipv6_1 = ipv6.ElementAtOrDefault(0)?.ToString() ?? "";
                    ipv6_2 = ipv6.ElementAtOrDefault(1)?.ToString() ?? "";
                }
                //
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string notConnectedText = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                string dnsNotAssignedText = software_lang.TSReadLangs("Network_Content", "nk_c_dns_not");
                //
                if (!hasNetwork){
                    labelDns1.Text = notConnectedText;
                    labelDns2.Text = notConnectedText;
                    labelDns3.Text = notConnectedText;
                    labelDns4.Text = notConnectedText;
                    return;
                }
                if (!hasDns){
                    labelDns1.Text = dnsNotAssignedText;
                    labelDns2.Text = dnsNotAssignedText;
                    labelDns3.Text = dnsNotAssignedText;
                    labelDns4.Text = dnsNotAssignedText;
                    return;
                }
                //
                SetDnsLabel(labelDns1, ipv4_1, software_lang);
                SetDnsLabel(labelDns2, ipv4_2, software_lang);
                SetDnsLabel(labelDns3, ipv6_1, software_lang);
                SetDnsLabel(labelDns4, ipv6_2, software_lang);
            }catch (Exception ex){
                if (debug_status)
                    TSErrorLog.LogException(ex, "UpdateDnsLabels()");
            }
        }
        private bool IsUsableInterface(NetworkInterface ni){
            var props = ni.GetIPProperties();
            bool hasGateway = props.GatewayAddresses.Any(g => g?.Address != null && !g.Address.Equals(IPAddress.Any));
            return hasGateway;
        }
        private int GetInterfacePriority(NetworkInterface ni){
            switch (ni.NetworkInterfaceType){
                case NetworkInterfaceType.Ethernet:
                    return 3;
                case NetworkInterfaceType.Wireless80211:
                    return 2;
                default:
                    return 1;
            }
        }
        private void SetDnsLabel(Label label, string dns, TSGetLangs lang){
            if (!string.IsNullOrEmpty(dns)){
                if (hiding_mode_wrapper == 1){
                    int maskLength = vis_m_property.Next(vn_range[0], vn_range[1]);
                    label.Text = new string('*', maskLength) + $" ({lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                }else{
                    var ip = IPAddress.Parse(dns);
                    DnsProvider provider = null;
                    if (ip.AddressFamily == AddressFamily.InterNetwork){
                        provider = DnsProviders.FirstOrDefault(p => p.IPv4.Contains(dns));
                    }else if (ip.AddressFamily == AddressFamily.InterNetworkV6){
                        provider = DnsProviders.FirstOrDefault(p => p.IPv6.Contains(dns));
                    }
                    label.Text = provider != null ? $"{dns} ({provider.Name})" : dns;
                }
            }else{
                label.Text = lang.TSReadLangs("Network_Content", "nk_c_dns_not");
            }
        }
        #endregion
        #region USB_Section
        // USB
        // ======================================================================================================
        readonly List<string> usb_controller_name_list = new List<string>();
        readonly List<string> usb_controller_manufacturer_list = new List<string>();
        readonly List<string> usb_controller_device_id_list = new List<string>();
        readonly List<string> usb_controller_pnp_device_id_list = new List<string>();
        readonly List<string> usb_controller_device_status_list = new List<string>();
        // ----------------------------------------------------------------------
        readonly List<string> usb_device_name_list = new List<string>();
        readonly List<string> usb_device_man_list = new List<string>();
        readonly List<string> usb_device_driver_version_list = new List<string>();
        readonly List<string> usb_device_driver_date_list = new List<string>();
        readonly List<string> usb_device_inf_file_list = new List<string>();
        readonly List<string> usb_device_id_list = new List<string>();
        readonly List<string> usb_device_hardware_id_list = new List<string>();
        readonly List<string> usb_device_guid_list = new List<string>();
        private void Usb(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            ManagementObjectSearcher search_usb_controller = new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption, Name, Manufacturer, DeviceID, PNPDeviceID, Status FROM Win32_USBController", queryOptions);
            ManagementObjectSearcher search_usb_device = new ManagementObjectSearcher("root\\CIMV2", "SELECT FriendlyName, DeviceName, Manufacturer, DriverVersion, DriverDate, InfName, DeviceID, HardWareID, ClassGuid FROM Win32_PnPSignedDriver WHERE DeviceID LIKE 'USB%'", queryOptions);
            foreach (ManagementObject query_usb_con in search_usb_controller.Get().Cast<ManagementObject>()){
                // USB CON CAPTION
                try{
                    string usb_con_caption = Convert.ToString(query_usb_con["Caption"]).Trim();
                    if (!string.IsNullOrEmpty(usb_con_caption)){
                        USB_Selector_List.Items.Add(usb_con_caption);
                    }else{
                        USB_Selector_List.Items.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB CON NAME
                try{
                    string usb_con_name = Convert.ToString(query_usb_con["Name"]).Trim();
                    if (!string.IsNullOrEmpty(usb_con_name)){
                        usb_controller_name_list.Add(usb_con_name);
                    }else{
                        usb_controller_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_ConName_V.Text = usb_controller_name_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB CON MANUFACTURER
                try{
                    string usb_con_man = Convert.ToString(query_usb_con["Manufacturer"]).Trim();
                    if (!string.IsNullOrEmpty(usb_con_man)){
                        usb_controller_manufacturer_list.Add(usb_con_man);
                    }else{
                        usb_controller_manufacturer_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_ConMan_V.Text = usb_controller_manufacturer_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB CON DEVICE ID
                try{
                    string usb_con_device_id = Convert.ToString(query_usb_con["DeviceID"]).Trim();
                    if (!string.IsNullOrEmpty(usb_con_device_id)){
                        char[] split_char = { '\\' };
                        string[] usb_con_device_split = usb_con_device_id.Trim().Split(split_char);
                        usb_controller_device_id_list.Add($"{usb_con_device_split[0]}\\{usb_con_device_split[1]}");
                    }else{
                        usb_controller_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_ConDeviceID_V.Text = usb_controller_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB CON PNP DEVICE ID
                try{
                    string usb_con_pnp_device_id = Convert.ToString(query_usb_con["PNPDeviceID"]).Trim();
                    if (!string.IsNullOrEmpty(usb_con_pnp_device_id)){
                        char[] split_char = { '\\' };
                        string[] usb_con_pnp_device_split = usb_con_pnp_device_id.Trim().Split(split_char);
                        usb_controller_pnp_device_id_list.Add($"{usb_con_pnp_device_split[0]}\\{usb_con_pnp_device_split[1]}");
                    }else{
                        usb_controller_pnp_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_ConPNPDeviceID_V.Text = usb_controller_pnp_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB CON DEVICE STATUS
                try{
                    string usb_con_device_status = Convert.ToString(query_usb_con["Status"]).Trim().ToLower();
                    var usbConDeviceStatuses = new Dictionary<string, string>{
                        { "ok", "usb_c_works_smoothly" },
                        { "error", "usb_c_error" },
                        { "degraded", "usb_c_degraded" },
                        { "unknown", "usb_c_unknown" },
                        { "pred fail", "usb_c_prevention_failed" },
                        { "starting", "usb_c_starting" },
                        { "stopping", "usb_c_stopped" },
                        { "service", "usb_c_service_mode" },
                        { "stressed", "usb_c_stressed" },
                        { "nonrecover", "usb_c_unrecoverable" },
                        { "no contact", "usb_c_no_communication" },
                        { "lost comm", "usb_c_communication_loss" }
                    };
                    if (!string.IsNullOrEmpty(usb_con_device_status)){
                        if (usbConDeviceStatuses.TryGetValue(usb_con_device_status, out string statusKey)){
                            usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", statusKey));
                        }else{
                            usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                        }
                    }else{
                        usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_ConDeviceStatus_V.Text = usb_controller_device_status_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
            }
            foreach (ManagementObject usb_device in search_usb_device.Get().Cast<ManagementObject>()){
                // USB FRIENDLY NAME
                try{
                    string usb_caption = Convert.ToString(usb_device["FriendlyName"]).Trim();
                    if (!string.IsNullOrEmpty(usb_caption)){
                        USB_DeviceSelector_List.Items.Add(usb_caption);
                    }else{
                        string usb_caption_2 = Convert.ToString(usb_device["DeviceName"]).Trim();
                        if (!string.IsNullOrEmpty(usb_caption_2)){
                            USB_DeviceSelector_List.Items.Add(usb_caption_2);
                        }else{
                            USB_DeviceSelector_List.Items.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB DEVICE NAME
                try{
                    string usb_device_name = Convert.ToString(usb_device["DeviceName"]).Trim();
                    if (!string.IsNullOrEmpty(usb_device_name)){
                        usb_device_name_list.Add(usb_device_name);
                    }else{
                        usb_device_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_DeviceName_V.Text = usb_device_name_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB MANUFACTURER
                try{
                    string usb_manufacturer= Convert.ToString(usb_device["Manufacturer"]).Trim();
                    if (!string.IsNullOrEmpty(usb_manufacturer)){
                        usb_device_man_list.Add(usb_manufacturer);
                    }else{
                        usb_device_man_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_DeviceMan_V.Text = usb_device_man_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB DRIVER VERSION
                try{
                    string usb_driver_version = Convert.ToString(usb_device["DriverVersion"]).Trim();
                    if (!string.IsNullOrEmpty(usb_driver_version)){
                        usb_device_driver_version_list.Add(usb_driver_version);
                    }else{
                        usb_device_driver_version_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_DriverVersion_V.Text = usb_device_driver_version_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB DRIVER DATE
                try{
                    string usb_driver_date = usb_device["DriverDate"]?.ToString().Trim();
                    if (!string.IsNullOrEmpty(usb_driver_date)){
                        try{
                            DateTime driverDate = ManagementDateTimeConverter.ToDateTime(usb_driver_date);
                            usb_device_driver_date_list.Add(driverDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
                        }catch{
                            usb_device_driver_date_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                        }
                    }else{
                        usb_device_driver_date_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_DriverDate_V.Text = usb_device_driver_date_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB INF FILE NAME
                try{
                    string usb_inf_name = Convert.ToString(usb_device["InfName"]).Trim();
                    if (!string.IsNullOrEmpty(usb_inf_name)){
                        usb_device_inf_file_list.Add(usb_inf_name);
                    }else{
                        usb_device_inf_file_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_InfFile_V.Text = usb_device_inf_file_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB DEVICE ID
                try{
                    string usb_device_id = Convert.ToString(usb_device["DeviceID"]).Trim();
                    if (!string.IsNullOrEmpty(usb_device_id)){
                        char[] split_char = { '\\' };
                        string[] usb_con_device_split = usb_device_id.Trim().Split(split_char);
                        usb_device_id_list.Add($"{usb_con_device_split[0]}\\{usb_con_device_split[1]}");
                    }else{
                        usb_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_DeviceID_V.Text = usb_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB HARDWARE ID
                try{
                    string usb_hardware_id = Convert.ToString(usb_device["HardWareID"]).Trim();
                    if (!string.IsNullOrEmpty(usb_hardware_id)){
                        char[] split_char = { '\\' };
                        string[] usb_con_device_split = usb_hardware_id.Trim().Split(split_char);
                        usb_device_hardware_id_list.Add($"{usb_con_device_split[0]}\\{usb_con_device_split[1]}");
                    }else{
                        usb_device_hardware_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                    }
                    USB_HardwareID_V.Text = usb_device_hardware_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
                // USB DEVICE GUID
                try{
                    string usb_device_guid = Convert.ToString(usb_device["ClassGuid"]).Trim();
                    if (string.IsNullOrEmpty(usb_device_guid)){
                        usb_device_guid_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_guid"));
                    }else{
                        string usb_guid_replacer = usb_device_guid.Replace("{", string.Empty).Replace("}", string.Empty);
                        if (hiding_mode_wrapper != 1){
                            usb_device_guid_list.Add(usb_guid_replacer.ToUpper());
                        }else{
                            usb_device_guid_list.Add(new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})");
                        }
                    }
                    USB_DeviceGUID_V.Text = usb_device_guid_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
                }
            }
            try{ USB_Selector_List.SelectedIndex = 0; }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
            }
            try { USB_DeviceSelector_List.SelectedIndex = 0; }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Usb()"); }
            }
            // USB PROCESS END ENABLED
            USB_RotateBtn.Enabled = true;
            ((Control)USB).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- USB Section Loaded --->");
            }
        }
        private void USB_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int usb_con_select = USB_Selector_List.SelectedIndex;
                try { USB_ConName_V.Text = usb_controller_name_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConMan_V.Text = usb_controller_manufacturer_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConDeviceID_V.Text = usb_controller_device_id_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConPNPDeviceID_V.Text = usb_controller_pnp_device_id_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConDeviceStatus_V.Text = usb_controller_device_status_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
        }
        private void USB_DeviceSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int usb_select = USB_DeviceSelector_List.SelectedIndex;
                try { USB_DeviceName_V.Text = usb_device_name_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceMan_V.Text = usb_device_man_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DriverVersion_V.Text = usb_device_driver_version_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DriverDate_V.Text = usb_device_driver_date_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_InfFile_V.Text = usb_device_inf_file_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceID_V.Text = usb_device_id_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_HardwareID_V.Text = usb_device_hardware_id_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceGUID_V.Text = usb_device_guid_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
        }
        #endregion
        #region SOUND_Section
        // SOUND
        // ======================================================================================================
        readonly List<string> sound_device_name_list = new List<string>();
        readonly List<string> sound_device_manufacturer_list = new List<string>();
        readonly List<string> sound_device_driver_version_list = new List<string>();
        readonly List<string> sound_device_driver_date_list = new List<string>();
        readonly List<string> sound_device_id_list = new List<string>();
        readonly List<string> sound_pnp_device_id_list = new List<string>();
        readonly List<string> sound_device_status_list = new List<string>();
        private void Sound(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            ManagementObjectSearcher search_sound = new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption, Name, Manufacturer, PNPDeviceID, DeviceID, Status FROM Win32_SoundDevice", queryOptions);
            //
            var soundSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID, DriverVersion, DriverDate FROM Win32_PnPSignedDriver WHERE DeviceClass='MEDIA'", queryOptions);
            var soundDriversDict = new Dictionary<string, ManagementObject>(StringComparer.OrdinalIgnoreCase);
            using (var results = soundSearcher.Get()){
                foreach (ManagementObject d in results.Cast<ManagementObject>()){
                    string deviceID = d["DeviceID"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(deviceID) && !soundDriversDict.ContainsKey(deviceID)){
                        soundDriversDict[deviceID] = d;
                    }
                }
            }
            //
            foreach (ManagementObject query_sound in search_sound.Get().Cast<ManagementObject>()){
                // SOUND DEVICE CAPTION
                try{
                    string sound_caption = Convert.ToString(query_sound["Caption"]);
                    if (!string.IsNullOrEmpty(sound_caption)){
                        SOUND_Selector_List.Items.Add(sound_caption);
                    }else{
                        SOUND_Selector_List.Items.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown_device"));
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
                // SOUND DEVICE NAME
                try{
                    string sound_name = Convert.ToString(query_sound["Name"]);
                    if (!string.IsNullOrEmpty(sound_name)){
                        sound_device_name_list.Add(sound_name);
                    }else{
                        sound_device_name_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                    }
                    SOUND_DeviceName_V.Text = sound_device_name_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
                // SOUND DEVICE MANUFACTURER
                try{
                    string sound_manfuacturer = Convert.ToString(query_sound["Manufacturer"]);
                    if (!string.IsNullOrEmpty(sound_manfuacturer)){
                        sound_device_manufacturer_list.Add(sound_manfuacturer);
                    }else{
                        sound_device_manufacturer_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                    }
                    SOUND_DeviceManufacturer_V.Text = sound_device_manufacturer_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
                // SOUND DRIVER VERSION & DATE
                try{
                    string unknown = software_lang.TSReadLangs("Sound_Content", "sound_c_unknown");
                    string sound_name = Convert.ToString(query_sound["Name"]) ?? unknown;
                    string sound_pnp_device_id = Convert.ToString(query_sound["PNPDeviceID"])?.Trim() ?? "";
                    string driverVersion = unknown;
                    string driverDate = unknown;
                    if (!string.IsNullOrEmpty(sound_pnp_device_id) && soundDriversDict.TryGetValue(sound_pnp_device_id, out var driverMatch)){
                        driverVersion = Convert.ToString(driverMatch["DriverVersion"]) ?? unknown;
                        if (driverMatch["DriverDate"] != null){
                            try{
                                DateTime dt = ManagementDateTimeConverter.ToDateTime(driverMatch["DriverDate"].ToString());
                                driverDate = dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                            }catch{
                                driverDate = unknown;
                            }
                        }
                    }
                    sound_device_driver_version_list.Add(driverVersion);
                    sound_device_driver_date_list.Add(driverDate);
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                    string unknown = software_lang.TSReadLangs("Sound_Content", "sound_c_unknown");
                    sound_device_driver_version_list.Add(unknown);
                    sound_device_driver_date_list.Add(unknown);
                }
                // SOUND DEVICE ID
                try{
                    string sound_device_id = Convert.ToString(query_sound["DeviceID"]);
                    if (!string.IsNullOrEmpty(sound_device_id)){
                        char[] split_char = { '\\' };
                        string[] sound_device_split = sound_device_id.Trim().Split(split_char);
                        sound_device_id_list.Add($"{sound_device_split[0]}\\{sound_device_split[1]}");
                    }else{
                        sound_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                    }
                    SOUND_DeviceID_V.Text = sound_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
                // SOUND PNP DEVICE ID
                try{
                    string sound_pnp_device_id = Convert.ToString(query_sound["PNPDeviceID"]);
                    if (!string.IsNullOrEmpty(sound_pnp_device_id)){
                        char[] split_char = { '\\' };
                        string[] sound_pnp_device_split = sound_pnp_device_id.Trim().Split(split_char);
                        sound_pnp_device_id_list.Add($"{sound_pnp_device_split[0]}\\{sound_pnp_device_split[1]}");
                    }else{
                        sound_pnp_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                    }
                    SOUND_PNPDeviceID_V.Text = sound_pnp_device_id_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
                // SOUND DEVICE STATUS
                try{
                    string sound_device_status = Convert.ToString(query_sound["Status"]).Trim().ToLower();
                    if (!string.IsNullOrEmpty(sound_device_status)){
                        var soundDeviceStatuses = new Dictionary<string, string>{
                            { "ok", "sound_c_works_smoothly" },
                            { "error", "sound_c_error" },
                            { "degraded", "sound_c_degraded" },
                            { "unknown", "sound_c_unknown" },
                            { "pred fail", "sound_c_prevention_failed" },
                            { "starting", "sound_c_starting" },
                            { "stopping", "sound_c_stopped" },
                            { "service", "sound_c_service_mode" },
                            { "stressed", "sound_c_stressed" },
                            { "nonrecover", "sound_c_unrecoverable" },
                            { "no contact", "sound_c_no_communication" },
                            { "lost comm", "sound_c_communication_loss" }
                        };
                        if (soundDeviceStatuses.TryGetValue(sound_device_status, out string statusKey)){
                            sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", statusKey));
                        }else{
                            sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                        }
                    }else{
                        sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                    }
                    SOUND_DeviceStatus_V.Text = sound_device_status_list[0];
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
                }
            }
            try { SOUND_Selector_List.SelectedIndex = 0; }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Sound()"); }
            }
            // SOUND PROCESS END ENABLED
            SOUND_RotateBtn.Enabled = true;
            ((Control)SOUND).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Sound Section Loaded --->");
            }
        }
        private void SOUND_Selector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int sound_select = SOUND_Selector_List.SelectedIndex;
                try { SOUND_DeviceName_V.Text = sound_device_name_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceManufacturer_V.Text = sound_device_manufacturer_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DriverVersion_V.Text = sound_device_driver_version_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DriverDate_V.Text = sound_device_driver_date_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceID_V.Text = sound_device_id_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_PNPDeviceID_V.Text = sound_pnp_device_id_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceStatus_V.Text = sound_device_status_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
            }catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
        }
        #endregion
        #region BATTERY_Section
        // BATTERY
        // ======================================================================================================
        private void Battery(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                ManagementObjectSearcher get_battery_static_data = new ManagementObjectSearcher("root\\WMI", "SELECT DeviceName, SerialNumber, Chemistry, DesignedCapacity FROM BatteryStaticData");
                foreach (ManagementObject batteryStatic in get_battery_static_data.Get().Cast<ManagementObject>()){
                    try{
                        // BATTERY NAME
                        string battery_name = batteryStatic["DeviceName"].ToString().Trim();
                        if (!string.IsNullOrEmpty(battery_name)){
                            BATTERY_Model_V.Text = battery_name;
                        }else{
                            BATTERY_Model_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
                    }
                    try{
                        // BATTERY SERIAL
                        string battery_serial = batteryStatic["SerialNumber"].ToString().Trim();
                        if (!string.IsNullOrEmpty(battery_serial)){
                            if (hiding_mode_wrapper != 1){
                                BATTERY_Serial_V.Text = battery_serial;
                            }else{
                                BATTERY_Serial_V.Text = new string('*', vis_m_property.Next(vn_range[0], vn_range[1])) + $" ({software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on_ui")})";
                            }
                        }else{
                            BATTERY_Serial_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
                    }
                    try{
                        // BATTERY CHEMISTRY
                        byte[] chemistryBytes = BitConverter.GetBytes(Convert.ToUInt32(batteryStatic["Chemistry"]));
                        string chemistry = Encoding.ASCII.GetString(chemistryBytes).Trim('\0').Trim();
                        if (!string.IsNullOrEmpty(chemistry)){
                            BATTERY_Chemistry_V.Text = chemistry.Replace("Li-I", software_lang.TSReadLangs("Battery_Content", "by_c_lithium_ion"));
                        }else{
                            ManagementObjectSearcher battery_sv2_chemistry = new ManagementObjectSearcher("root\\CIMV2", "SELECT Chemistry FROM Win32_Battery");
                            foreach (ManagementObject b_sv2_chemistry in battery_sv2_chemistry.Get().Cast<ManagementObject>()){
                                int battery_structure = Convert.ToInt32(b_sv2_chemistry["Chemistry"]);
                                var batteryTypes = new Dictionary<int, string>{
                                    { 1, "by_c_other" },
                                    { 2, "by_c_unknown" },
                                    { 3, "by_c_lead_acid" },
                                    { 4, "by_c_nickel_cadmium" },
                                    { 5, "by_c_nickel_metal_hydride" },
                                    { 6, "by_c_lithium_ion" },
                                    { 7, "by_c_zinc_air" },
                                    { 8, "by_c_lithium_polymer" }
                                };
                                if (batteryTypes.TryGetValue(battery_structure, out string batteryKey)){
                                    BATTERY_Chemistry_V.Text = software_lang.TSReadLangs("Battery_Content", batteryKey);
                                }else{
                                    BATTERY_Chemistry_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                                }
                            }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
                    }
                    try{
                        // BATTERY DESIGN CAPACITY
                        decimal designedCapacityRaw = Convert.ToDecimal(batteryStatic["DesignedCapacity"]);
                        decimal designedCapacity_mWh = designedCapacityRaw / 1000m;
                        if (!string.IsNullOrEmpty(Convert.ToString(designedCapacity_mWh))){
                            BATTERY_DesignCapacity_V.Text = $"{designedCapacity_mWh:N3} mWh";
                        }else{
                            BATTERY_DesignCapacity_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
                    }
                }
                try{
                    // BATTERY FULL CHARAGE CAPACITY
                    ManagementObjectSearcher get_battery_full_charge_capacity = new ManagementObjectSearcher("root\\WMI", "SELECT FullChargedCapacity FROM BatteryFullChargedCapacity");
                    foreach (ManagementObject batteryFullCharge in get_battery_full_charge_capacity.Get().Cast<ManagementObject>()){
                        long fullChargedCapacityRaw = Convert.ToInt64(batteryFullCharge["FullChargedCapacity"]);
                        battery_fullChargedCapacity_mWh = fullChargedCapacityRaw / 1000m;
                        if (!string.IsNullOrEmpty(Convert.ToString(battery_fullChargedCapacity_mWh))){
                            BATTERY_FullChargeCapacity_V.Text = $"{battery_fullChargedCapacity_mWh:N3} mWh";
                        }else{
                            BATTERY_FullChargeCapacity_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                        }
                    }
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
            }
            // BATTERY PROCESS END ENABLED
            BATTERY_RotateBtn.Enabled = true;
            ((Control)BATTERY).Enabled = true;
            if (debug_status){
                TSLogger.Log("<--- Battery Section Loaded --->");
            }
        }
        private void Battery_visible_off(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            BATTERY_Status_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_battery_not_detect");
            battery_panel_1.Height = (int)(47 * this.DeviceDpi / 96f);
            battery_panel_2.Visible = false;
            //
            BATTERY_Model.Visible = false;
            BATTERY_Model_V.Visible = false;
            BATTERY_Serial.Visible = false;
            BATTERY_Serial_V.Visible = false;
            BATTERY_Chemistry.Visible = false;
            BATTERY_Chemistry_V.Visible = false;
            BATTERY_DesignCapacity.Visible = false;
            BATTERY_DesignCapacity_V.Visible = false;
            BATTERY_FullChargeCapacity.Visible = false;
            BATTERY_FullChargeCapacity_V.Visible = false;
            BATTERY_RemainingChargeCapacity.Visible = false;
            BATTERY_RemainingChargeCapacity_V.Visible = false;
            BATTERY_Voltage.Visible = false;
            BATTERY_Voltage_V.Visible = false;
            BATTERY_ChargePower.Visible = false;
            BATTERY_ChargePower_V.Visible = false;
            BATTERY_ChargeCurrent.Visible = false;
            BATTERY_ChargeCurrent_V.Visible = false;
            BATTERY_DeChargePower.Visible = false;
            BATTERY_DeChargePower_V.Visible = false;
            BATTERY_DeChargeCurrent.Visible = false;
            BATTERY_DeChargeCurrent_V.Visible = false;
            //
            BATTERY_ReportBtn.Visible = false;
            BATTERY_PBG_Panel.Visible = false;
            BATTERY_PFE_Panel.Visible = false;
            BATTERY_ProgressLabel.Visible = false;
        }
        private void Battery_visible_on(){
            BATTERY_Model.Visible = true;
            BATTERY_Model_V.Visible = true;
            BATTERY_Serial.Visible = true;
            BATTERY_Serial_V.Visible = true;
            BATTERY_Chemistry.Visible = true;
            BATTERY_Chemistry_V.Visible = true;
            BATTERY_DesignCapacity.Visible = true;
            BATTERY_DesignCapacity_V.Visible = true;
            BATTERY_FullChargeCapacity.Visible = true;
            BATTERY_FullChargeCapacity_V.Visible = true;
            BATTERY_RemainingChargeCapacity.Visible = true;
            BATTERY_RemainingChargeCapacity_V.Visible = true;
            BATTERY_Voltage.Visible = true;
            BATTERY_Voltage_V.Visible = true;
            BATTERY_ChargePower.Visible = true;
            BATTERY_ChargePower_V.Visible = true;
            BATTERY_ChargeCurrent.Visible = true;
            BATTERY_ChargeCurrent_V.Visible = true;
            BATTERY_DeChargePower.Visible = true;
            BATTERY_DeChargePower_V.Visible = true;
            BATTERY_DeChargeCurrent.Visible = true;
            BATTERY_DeChargeCurrent_V.Visible = true;
            //
            BATTERY_ReportBtn.Visible = true;
            BATTERY_PBG_Panel.Visible = true;
            BATTERY_PFE_Panel.Visible = true;
            BATTERY_ProgressLabel.Visible = true;
        }
        private async void BatteryBgProcess(){
            try{
                var get_dynamic_battery_info = new ManagementObjectSearcher("root\\WMI", "SELECT RemainingCapacity, Voltage, ChargeRate, DischargeRate FROM BatteryStatus");
                PowerStatus power = SystemInformation.PowerStatus;
                while (loop_status){
                    var batteryData = new Dictionary<string, object>();
                    try{
                        var software_lang = new TSGetLangs(lang_path);
                        float batteryPercent = power.BatteryLifePercent * 100;
                        batteryData["BatteryPercent"] = batteryPercent;
                        batteryData["BatteryStatusText"] = string.Format("{0}%", batteryPercent.ToString("F0").Trim());
                        batteryData["SoftwareLang"] = software_lang;
                        //
                        decimal remainingCapacity_mWh = 0;
                        decimal voltage_V = 0;
                        decimal chargePower_W = 0;
                        decimal dischargePower_W = 0;
                        //
                        foreach (ManagementObject batteryStatus in get_dynamic_battery_info.Get().Cast<ManagementObject>()){
                            try { remainingCapacity_mWh = Convert.ToInt64(batteryStatus["RemainingCapacity"]) / 1000m; } catch { }
                            try { voltage_V = Convert.ToInt32(batteryStatus["Voltage"]) / 1000m; } catch { }
                            try{
                                chargePower_W = Convert.ToInt32(batteryStatus["ChargeRate"]) / 1000m;
                                dischargePower_W = Convert.ToInt32(batteryStatus["DischargeRate"]) / 1000m;
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess()"); }
                            }
                        }
                        //
                        batteryData["RemainingCapacity"] = remainingCapacity_mWh;
                        batteryData["Voltage"] = voltage_V;
                        batteryData["ChargePower"] = chargePower_W;
                        batteryData["DischargePower"] = dischargePower_W;
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess()"); }
                    }
                    //
                    if (IsHandleCreated){
                        BeginInvoke(new Action(() =>{
                            try{
                                var software_lang = (TSGetLangs)batteryData["SoftwareLang"];
                                var battery_status = (string)batteryData["BatteryStatusText"];
                                var percent = (float)batteryData["BatteryPercent"];
                                var remainingCapacity = (decimal)batteryData["RemainingCapacity"];
                                var voltage = (decimal)batteryData["Voltage"];
                                var chargePower = (decimal)batteryData["ChargePower"];
                                var dischargePower = (decimal)batteryData["DischargePower"];
                                //
                                BATTERY_RotateBtn.Text = $"  {software_lang.TSReadLangs("LeftMenu", "left_battery")} - {battery_status}";
                                BATTERY_PFE_Panel.Height = (int)(BATTERY_PBG_Panel.Height * (percent / 100.0));
                                BATTERY_ProgressLabel.Text = battery_status;
                                BATTERY_ProgressLabel.Top = BATTERY_PFE_Panel.Top + 6;
                                //
                                BATTERY_RemainingChargeCapacity_V.Text = $"{remainingCapacity:N3} mWh";
                                BATTERY_Voltage_V.Text = $"{voltage:N1} V";
                                //
                                BATTERY_ChargePower_V.Text = chargePower > 0 ? $"{chargePower:N1} W" : software_lang.TSReadLangs("Battery_Content", "by_c_not_charging");
                                BATTERY_ChargeCurrent_V.Text = (chargePower > 0 && voltage > 0) ? $"{(chargePower / voltage):N1} A" : software_lang.TSReadLangs("Battery_Content", "by_c_not_charging");
                                //
                                BATTERY_DeChargePower_V.Text = dischargePower > 0 ? $"{dischargePower:N1} W" : software_lang.TSReadLangs("Battery_Content", "by_c_not_discharging");
                                BATTERY_DeChargeCurrent_V.Text = (dischargePower > 0 && voltage > 0) ? $"{(dischargePower / voltage):N1} A" : software_lang.TSReadLangs("Battery_Content", "by_c_not_discharging");
                                //
                                if (battery_fullChargedCapacity_mWh <= 0){
                                    BATTERY_Status_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_not_full_charge_capacity_get");
                                }else if (chargePower >= battery_MIN_POWER_THRESHOLD){
                                    decimal remainingToFull = battery_fullChargedCapacity_mWh - remainingCapacity;
                                    if (remainingToFull > 0){
                                        var t = TimeSpan.FromHours((double)(remainingToFull / chargePower));
                                        BATTERY_Status_V.Text = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_charging"), battery_status, t.Hours, t.Minutes);
                                    }else{
                                        BATTERY_Status_V.Text = software_lang.TSReadLangs("Battery_Content", "by_c_full_capacity");
                                    }
                                }else if (dischargePower >= battery_MIN_POWER_THRESHOLD){
                                    var t = TimeSpan.FromHours((double)(remainingCapacity / dischargePower));
                                    BATTERY_Status_V.Text = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_discharging"), battery_status, t.Hours, t.Minutes);
                                }
                                else{
                                    BATTERY_Status_V.Text = percent < 100 ? string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_fixed_capacity"), battery_status) : software_lang.TSReadLangs("Battery_Content", "by_c_full_capacity");
                                }
                            }catch (Exception ex){
                                if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess()"); }
                            }
                        }));
                    }
                    //
                    try{
                        await Task.Delay(1000);
                    }catch (TaskCanceledException){
                        break;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess()"); }
            }
        }
        readonly string battery_report_path = Application.StartupPath + @"\battery-report.html";
        private async void BATTERY_ReportBtn_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (File.Exists(battery_report_path))
                    File.Delete(battery_report_path);
                Process.Start(new ProcessStartInfo("powercfg", "/batteryreport") { CreateNoWindow = true, UseShellExecute = false });
                await StartBatteryReportProcessAsync();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BATTERY_ReportBtn_Click()"); }
            }
        }
        private async Task StartBatteryReportProcessAsync(){
            try{
                await Task.Run(async () => await Battery_report_check_process_async());
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "StartBatteryReportProcessAsync()"); }
            }
        }
        private async Task Battery_report_check_process_async(){
            try{
                while (!File.Exists(battery_report_path)){
                    await Task.Delay(500);
                }
                bool fileReady = false;
                while (!fileReady){
                    try{
                        using (var stream = new FileStream(battery_report_path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                        fileReady = true;
                    }catch { await Task.Delay(200); }
                }
                string new_battery_report_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{Application.ProductName}_{Dns.GetHostName()}_battery_report_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.html");
                File.Move(battery_report_path, new_battery_report_path);
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                DialogResult br_message = TS_MessageBoxEngine.TS_MessageBox(this, 5,
                    string.Format(software_lang.TSReadLangs("Battery_Content", "by_report_create_message"), new_battery_report_path, "\n\n")
                );
                if (br_message == DialogResult.Yes){
                    Process.Start(new_battery_report_path);
                }
            }catch (TaskCanceledException) { }
            catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Battery_report_check_process_async()"); }
            }
        }
        #endregion
        #region OSD_Section
        // OSD
        // ======================================================================================================
        private Task InstalledDrivers(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var typeMap = new Dictionary<string, string> {
                 { "Kernel Driver", "osd_c_kernel_driver" },
                 { "File System Driver", "osd_c_file_system_driver" }
            };
            var startMap = new Dictionary<string, string> {
                 { "Boot", "osd_c_boot" }, { "Manual", "osd_c_manuel" },
                 { "System", "osd_c_system" }, { "Auto", "osd_c_auto" },
                 { "Disabled", "osd_c_disabled" }
            };
            var stateMap = new Dictionary<string, string> {
                 { "Stopped", "osd_c_stopped" }, { "Running", "osd_c_working" }
            };
            var langCache = new Dictionary<string, string>();
            string L(string key){
                if (!langCache.TryGetValue(key, out string val)){
                    val = software_lang.TSReadLangs("Osd_Content", key);
                    langCache[key] = val;
                }
                return val;
            }
            try{
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions{
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT PathName, DisplayName, ServiceType, StartMode, State FROM Win32_SystemDriver");
                var searcher = new ManagementObjectSearcher(scope, query, options);
                var driverRows = new List<string[]>();
                foreach (ManagementBaseObject d in searcher.Get()){
                    string path = (d["PathName"] as string ?? "").Replace("\"", "").Replace("\\??\\", "").Trim();
                    string file = Path.GetFileName(path);
                    string disp = (d["DisplayName"] as string ?? "").Trim();
                    //
                    string typeKey = d["ServiceType"] as string ?? "";
                    string startKey = d["StartMode"] as string ?? "";
                    string stateKey = d["State"] as string ?? "";
                    //
                    string type = L(typeMap.TryGetValue(typeKey, out var tVal) ? tVal : "osd_c_unknown");
                    string start = L(startMap.TryGetValue(startKey, out var sVal) ? sVal : "osd_c_unknown");
                    string state = L(stateMap.TryGetValue(stateKey, out var stVal) ? stVal : "osd_c_unknown");
                    //
                    driverRows.Add(new string[] { path, file, disp, type, start, state, TSNaturalSortKey(file) });
                }
                //
                driverRows.Sort((a, b) => string.Compare(a[6], b[6], StringComparison.OrdinalIgnoreCase));
                //
                Action uiUpdate = () => BulkAddRowsFast(driverRows, software_lang);
                if (OSD_DataMainTable.InvokeRequired)
                    OSD_DataMainTable.Invoke(uiUpdate);
                else
                    uiUpdate();
            }catch (Exception ex){
                if (debug_status) TSErrorLog.LogException(ex, "InstalledDrivers()");
            }finally{
                Action finalizeUi = () => {
                    OSD_RotateBtn.Enabled = true;
                    ((Control)DRIVERS).Enabled = true;
                    OSD_DataMainTable.PerformLayout();
                    OSD_DataMainTable.Invalidate();
                    if (debug_status) TSLogger.Log("<--- Installed Drivers Section Loaded --->");
                };
                if (OSD_DataMainTable.InvokeRequired)
                    OSD_DataMainTable.Invoke(finalizeUi);
                else
                    finalizeUi();
            }
            return Task.CompletedTask;
        }
        private void BulkAddRowsFast(List<string[]> driverRows, TSGetLangs software_lang){
            OSD_DataMainTable.SuspendLayout();
            OSD_DataMainTable.Rows.Clear();
            string unknownValue = software_lang.TSReadLangs("Osd_Content", "osd_c_unknown");
            int count = driverRows.Count;
            int dpiHeight = OSD_DataMainTable.RowTemplate.Height;
            DataGridViewRow[] rowsArray = new DataGridViewRow[count];
            for (int i = 0; i < count; i++){
                string[] data = driverRows[i];
                for (int j = 0; j < data.Length; j++){
                    if (string.IsNullOrWhiteSpace(data[j])) data[j] = unknownValue;
                }
                DataGridViewRow gridRow = new DataGridViewRow();
                gridRow.CreateCells(OSD_DataMainTable, data);
                gridRow.Height = dpiHeight;
                rowsArray[i] = gridRow;
            }
            OSD_DataMainTable.Rows.AddRange(rowsArray);
            OSD_DataMainTable.ClearSelection();
            OSD_TYSS_V.Text = count.ToString();
            OSD_DataMainTable.ResumeLayout();
        }
        private void OSD_TextBox_TextChanged(object sender, EventArgs e){
            string searchText = OSD_TextBox.Text.Trim();
            bool isTextBoxEmpty = string.IsNullOrEmpty(searchText);
            OSD_DataMainTable.ClearSelection();
            OSD_TextBoxClearBtn.Enabled = !isTextBoxEmpty;
            if (OSD_DataMainTable.Rows.Count > 0)
                OSD_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            if (!isTextBoxEmpty){
                string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (DataGridViewRow driver_row in OSD_DataMainTable.Rows){
                    var cellValue = driver_row.Cells[1].Value?.ToString();
                    if (!string.IsNullOrEmpty(cellValue)){
                        string[] cellWords = cellValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        bool isMatch = true;
                        foreach (var searchWord in searchWords){
                            bool wordFound = false;
                            foreach (var cellWord in cellWords){
                                if (cellWord.StartsWith(searchWord, StringComparison.OrdinalIgnoreCase) || cellWord.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) >= 0){
                                    wordFound = true;
                                    break;
                                }
                            }
                            if (!wordFound){
                                isMatch = false;
                                break;
                            }
                        }
                        if (isMatch){
                            driver_row.Selected = true;
                            OSD_DataMainTable.FirstDisplayedScrollingRowIndex = driver_row.Index;
                            break;
                        }
                    }
                }
            }
        }
        private void OSD_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                if (OSD_DataMainTable.SelectedRows.Count > 0){
                    Clipboard.SetText(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", OSD_DataMainTable.Rows[e.RowIndex].Cells[0].Value.ToString(), OSD_DataMainTable.Rows[e.RowIndex].Cells[1].Value.ToString(), OSD_DataMainTable.Rows[e.RowIndex].Cells[2].Value.ToString(), OSD_DataMainTable.Rows[e.RowIndex].Cells[3].Value.ToString(), OSD_DataMainTable.Rows[e.RowIndex].Cells[4].Value.ToString(), OSD_DataMainTable.Rows[e.RowIndex].Cells[5].Value.ToString()));
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("Osd_Content", "osd_c_copy_success"), OSD_DataMainTable.Rows[e.RowIndex].Cells[0].Value));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_DataMainTable_CellDoubleClick()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Osd_Content", "osd_c_copy_error"), OSD_DataMainTable.Rows[e.RowIndex].Cells[0].Value));
            }
        }
        private void OSD_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e){
            try{
                if (OSD_DataMainTable.SelectedRows.Count > 0){
                    OSD_DataMainTable.ClearSelection();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void OSD_TextBoxClearBtn_Click(object sender, EventArgs e){
            try{
                OSD_TextBox.Text = string.Empty;
                OSD_TextBox.Focus();
                OSD_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_TextBoxClearBtn_Click()"); }
            }
        }
        private void OSD_SortMode_CheckedChanged(object sender, EventArgs e){
            try{
                if (OSD_SortMode.CheckState == CheckState.Checked){
                    OSD_DataMainTable.Sort(OSD_DataMainTable.Columns[1], ListSortDirection.Descending);
                }else if (OSD_SortMode.CheckState == CheckState.Unchecked){
                    OSD_DataMainTable.Sort(OSD_DataMainTable.Columns[1], ListSortDirection.Ascending);
                }
                OSD_DataMainTable.ClearSelection();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_SortMode_CheckedChanged()"); }
            }
        }
        #endregion
        #region SERVICES_Section
        // GS SERVICE
        // ======================================================================================================
        private Task InstalledServices(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var serviceTypeMap = new Dictionary<string, string>{
                 { "Kernel Driver", "ss_c_kernel_driver" },
                 { "File System Driver", "ss_c_file_system_service" },
                 { "Adapter", "ss_c_adaptor" },
                 { "Recognizer Driver", "ss_c_recognizer_service" },
                 { "Own Process", "ss_c_own_system" },
                 { "Share Process", "ss_c_process_sharer" },
                 { "Interactive Process", "ss_c_interactive_process" }
            };
            var startModeMap = new Dictionary<string, string>{
                 { "Boot", "ss_c_boot" }, { "Manual", "ss_c_manuel" },
                 { "System", "ss_c_system" }, { "Auto", "ss_c_auto" },
                 { "Disabled", "ss_c_disabled" }
            };
            var stateMap = new Dictionary<string, string>{
                 { "Stopped", "ss_c_stopped" }, { "Running", "ss_c_working" }
            };
            var langCache = new Dictionary<string, string>();
            string L(string key){
                if (!langCache.TryGetValue(key, out string val)){
                    val = software_lang.TSReadLangs("Services_Content", key);
                    langCache[key] = val;
                }
                return val;
            }
            try{
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions{
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT PathName, Name, DisplayName, ServiceType, StartMode, State FROM Win32_Service");
                var searcher = new ManagementObjectSearcher(scope, query, options);
                var serviceRows = new List<string[]>();
                //
                foreach (ManagementBaseObject s in searcher.Get()){
                    string path = (s["PathName"] as string ?? "").Replace("\"", "").Replace("\\??\\", "").Trim();
                    string name = (s["Name"] as string ?? "").Trim();
                    string disp = (s["DisplayName"] as string ?? "").Trim();
                    //
                    string tK = s["ServiceType"] as string ?? "";
                    string smK = s["StartMode"] as string ?? "";
                    string stK = s["State"] as string ?? "";
                    //
                    string type = L(serviceTypeMap.TryGetValue(tK, out var t) ? t : "ss_c_unknown");
                    string start = L(startModeMap.TryGetValue(smK, out var sm) ? sm : "ss_c_unknown");
                    string state = L(stateMap.TryGetValue(stK, out var st) ? st : "ss_c_unknown");
                    //
                    serviceRows.Add(new string[] { path, name, disp, type, start, state, TSNaturalSortKey(name) });
                }
                //
                serviceRows.Sort((a, b) => string.Compare(a[6], b[6], StringComparison.OrdinalIgnoreCase));
                //
                Action uiUpdate = () => BulkAddServiceRowsFast(serviceRows, software_lang);
                if (SERVICE_DataMainTable.InvokeRequired)
                    SERVICE_DataMainTable.Invoke(uiUpdate);
                else
                    uiUpdate();
            }catch (Exception ex){
                if (debug_status) TSErrorLog.LogException(ex, "InstalledServices()");
            }finally{
                Action finalizeUi = () => {
                    SERVICES_RotateBtn.Enabled = true;
                    ((Control)SERVICES).Enabled = true;
                    SERVICE_DataMainTable.PerformLayout();
                    SERVICE_DataMainTable.Invalidate();
                    if (debug_status) TSLogger.Log("<--- Installed Services Section Loaded --->");
                };
                if (SERVICE_DataMainTable.InvokeRequired)
                    SERVICE_DataMainTable.Invoke(finalizeUi);
                else
                    finalizeUi();
            }
            return Task.CompletedTask;
        }
        private void BulkAddServiceRowsFast(List<string[]> serviceRows, TSGetLangs software_lang){
            SERVICE_DataMainTable.SuspendLayout();
            SERVICE_DataMainTable.Rows.Clear();
            string unknownValue = software_lang.TSReadLangs("Services_Content", "ss_c_unknown");
            int count = serviceRows.Count;
            int dpiHeight = SERVICE_DataMainTable.RowTemplate.Height;
            DataGridViewRow[] rowsArray = new DataGridViewRow[count];
            for (int i = 0; i < count; i++){
                string[] rowData = serviceRows[i];
                for (int j = 0; j < rowData.Length; j++){
                    if (string.IsNullOrWhiteSpace(rowData[j])) rowData[j] = unknownValue;
                }
                DataGridViewRow gridRow = new DataGridViewRow();
                gridRow.CreateCells(SERVICE_DataMainTable, rowData);
                gridRow.Height = dpiHeight;
                rowsArray[i] = gridRow;
            }
            SERVICE_DataMainTable.Rows.AddRange(rowsArray);
            SERVICE_DataMainTable.ClearSelection();
            SERVICE_TYS_V.Text = count.ToString();
            SERVICE_DataMainTable.ResumeLayout();
        }
        private void Services_SearchTextBox_TextChanged(object sender, EventArgs e){
            string searchText = SERVICE_TextBox.Text.Trim();
            bool isTextBoxEmpty = string.IsNullOrEmpty(searchText);
            SERVICE_DataMainTable.ClearSelection();
            SERVICE_TextBoxClearBtn.Enabled = !isTextBoxEmpty;
            if (SERVICE_DataMainTable.Rows.Count > 0)
                SERVICE_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            if (!isTextBoxEmpty) {
                string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (DataGridViewRow service_row in SERVICE_DataMainTable.Rows){
                    var cellValue = service_row.Cells[1].Value?.ToString();
                    if (!string.IsNullOrEmpty(cellValue)){
                        string[] cellWords = cellValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        bool isMatch = true;
                        foreach (var searchWord in searchWords){
                            bool wordFound = false;
                            foreach (var cellWord in cellWords){
                                if (cellWord.StartsWith(searchWord, StringComparison.OrdinalIgnoreCase) || cellWord.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) >= 0){
                                    wordFound = true;
                                    break;
                                }
                            }
                            if (!wordFound){
                                isMatch = false;
                                break;
                            }
                        }
                        if (isMatch){
                            service_row.Selected = true;
                            SERVICE_DataMainTable.FirstDisplayedScrollingRowIndex = service_row.Index;
                            break;
                        }
                    }
                }
            }
        }
        private void SERVICE_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                if (SERVICE_DataMainTable.SelectedRows.Count > 0){
                    Clipboard.SetText(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", SERVICE_DataMainTable.Rows[e.RowIndex].Cells[0].Value.ToString(), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[1].Value.ToString(), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[2].Value.ToString(), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[3].Value.ToString(), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[4].Value.ToString(), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[5].Value.ToString()));
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("Services_Content", "ss_c_copy_success"), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[0].Value));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_DataMainTable_CellDoubleClick()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Services_Content", "ss_c_copy_error"), SERVICE_DataMainTable.Rows[e.RowIndex].Cells[0].Value));
            }
        }
        private void SERVICE_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e){
            try{
                if (SERVICE_DataMainTable.SelectedRows.Count > 0){
                    SERVICE_DataMainTable.ClearSelection();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void SERVICE_TextBoxClearBtn_Click(object sender, EventArgs e){
            try{
                SERVICE_TextBox.Text = string.Empty;
                SERVICE_TextBox.Focus();
                SERVICE_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_TextBoxClearBtn_Click()"); }
            }
        }
        private void SERVICES_SortMode_CheckedChanged(object sender, EventArgs e){
            try{
                if (SERVICE_SortMode.CheckState == CheckState.Checked){
                    SERVICE_DataMainTable.Sort(SERVICE_DataMainTable.Columns[1], ListSortDirection.Descending);
                }else if (SERVICE_SortMode.CheckState == CheckState.Unchecked){
                    SERVICE_DataMainTable.Sort(SERVICE_DataMainTable.Columns[1], ListSortDirection.Ascending);
                }
                SERVICE_DataMainTable.ClearSelection();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICES_SortMode_CheckedChanged()"); }
            }
        }
        #endregion
        #region INSTAPPS
        private Task InstalledApps(){
            try{
                LoadInstalledApplications();
                INSTAPPS_TYUS_V.Text = INSTAPPS_DataMainTable.Rows.Count.ToString();
                INSTAPPS_DataMainTable.ClearSelection();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "InstalledApps()"); }
            }
            finally{
                Action finalizeUi = () =>{
                    INSTALLED_RotateBtn.Enabled = true;
                    ((Control)INSTAPPS).Enabled = true;
                    INSTAPPS_DataMainTable.PerformLayout();
                    INSTAPPS_DataMainTable.Invalidate();
                    if (debug_status){
                        TSLogger.Log("<--- Installed Application Section Loaded --->");
                    }
                };
                if (INSTAPPS_DataMainTable.InvokeRequired)
                    INSTAPPS_DataMainTable.Invoke(finalizeUi);
                else
                    finalizeUi();
            }
            return Task.CompletedTask;
        }
        private void INSTAPPS_TextBox_TextChanged(object sender, EventArgs e){
            string searchText = INSTAPPS_TextBox.Text.Trim();
            bool isTextBoxEmpty = string.IsNullOrEmpty(searchText);
            INSTAPPS_DataMainTable.ClearSelection();
            INSTAPPS_TextBoxClearBtn.Enabled = !isTextBoxEmpty;
            if (INSTAPPS_DataMainTable.Rows.Count > 0)
                INSTAPPS_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            if (!isTextBoxEmpty){
                string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (DataGridViewRow service_row in INSTAPPS_DataMainTable.Rows){
                    var cellVal = service_row.Cells[1].Value?.ToString();
                    if (!string.IsNullOrEmpty(cellVal)){
                        string[] cellWords = cellVal.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        bool isMatch = true;
                        foreach (var searchWord in searchWords){
                            bool wordFound = false;
                            foreach (var cellWord in cellWords){
                                if (cellWord.StartsWith(searchWord, StringComparison.OrdinalIgnoreCase) || cellWord.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) >= 0){
                                    wordFound = true;
                                    break;
                                }
                            }
                            if (!wordFound){
                                isMatch = false;
                                break;
                            }
                        }
                        if (isMatch){
                            service_row.Selected = true;
                            INSTAPPS_DataMainTable.FirstDisplayedScrollingRowIndex = service_row.Index;
                            break;
                        }
                    }
                }
            }
        }
        private void INSTAPPS_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e){
            if (INSTAPPS_DataMainTable.SelectedRows.Count == 0) return;
            //
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var selected_row = INSTAPPS_DataMainTable.SelectedRows[0];
            //
            DialogResult open_apps_folder = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Instapps_Content", "ia_open_apps"), selected_row.Cells[1].Value));
            if (open_apps_folder == DialogResult.Yes){
                var installPath = selected_row.Cells[6].Value as string;
                if (string.IsNullOrWhiteSpace(installPath) || installPath == iapps_unknown || !Directory.Exists(installPath)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Instapps_Content", "ia_not_path"));
                    return;
                }
                try{
                    var exePath = Directory.GetFiles(installPath, "*.exe").FirstOrDefault();
                    if (string.IsNullOrEmpty(exePath)){
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Instapps_Content", "ia_not_exe"));
                        return;
                    }
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{exePath}\"") { UseShellExecute = true });
                }catch (Exception ex){
                    if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_DataMainTable_CellDoubleClick()"); }
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("Instapps_Content", "ia_file_path_not_oppened"), ex.Message));
                }
            }
        }
        private void INSTAPPS_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e){
            try{
                if (INSTAPPS_DataMainTable.SelectedRows.Count > 0){
                    INSTAPPS_DataMainTable.ClearSelection();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void INSTAPPS_TextBoxClearBtn_Click(object sender, EventArgs e){
            try{
                INSTAPPS_TextBox.Text = string.Empty;
                INSTAPPS_TextBox.Focus();
                INSTAPPS_DataMainTable.FirstDisplayedScrollingRowIndex = 0;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_TextBoxClearBtn_Click()"); }
            }
        }
        private void INSTAPPS_SortMode_CheckedChanged(object sender, EventArgs e){
            try{
                if (INSTAPPS_SortMode.CheckState == CheckState.Checked){
                    INSTAPPS_DataMainTable.Sort(INSTAPPS_DataMainTable.Columns[1], ListSortDirection.Descending);
                }else if (INSTAPPS_SortMode.CheckState == CheckState.Unchecked){
                    INSTAPPS_DataMainTable.Sort(INSTAPPS_DataMainTable.Columns[1], ListSortDirection.Ascending);
                }
                INSTAPPS_DataMainTable.ClearSelection();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_SortMode_CheckedChanged()"); }
            }
        }
        public class InstalledAppConfig{
            public string DisplayIcon { get; set; }
            public Icon AppIcon { get; set; }
            public string Name { get; set; }
            public string Publisher { get; set; }
            public DateTime? InstallDate { get; set; }
            public long? Size { get; set; }
            public string Version { get; set; }
            public string InstallLocation { get; set; }
        }
        public static class AppWindowIcon{
            [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
            private static extern uint ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool DestroyIcon(IntPtr hIcon);
            public static Icon ExtractIconFromFile(string filePath, int index, bool large = true){
                if (string.IsNullOrWhiteSpace(filePath)){
                    return null;
                }
                IntPtr[] largeIcons = large ? new IntPtr[1] : null;
                IntPtr[] smallIcons = !large ? new IntPtr[1] : null;
                uint count = ExtractIconEx(filePath, index, largeIcons, smallIcons, 1);
                if (count == 0){
                    return null;
                }
                IntPtr hIcon = large ? largeIcons[0] : smallIcons[0];
                if (hIcon == IntPtr.Zero)
                    return null;
                try{
                    return (Icon)Icon.FromHandle(hIcon).Clone();
                }finally{
                    DestroyIcon(hIcon);
                }
            }
        }
        private static void ParseDisplayIcon(string displayIconRaw, out string iconPath, out int iconIndex){
            iconIndex = 0;
            iconPath = string.Empty;
            if (string.IsNullOrWhiteSpace(displayIconRaw)){
                return;
            }
            string s = Environment.ExpandEnvironmentVariables(displayIconRaw).Trim();
            if (s.StartsWith("@", StringComparison.Ordinal)){
                s = s.Substring(1).Trim();
            }
            s = s.Trim().Trim('"');
            int comma = s.LastIndexOf(',');
            if (comma > 0 && comma < s.Length - 1){
                string pathPart = s.Substring(0, comma).Trim().Trim('"');
                string indexPart = s.Substring(comma + 1).Trim();
                if (int.TryParse(indexPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx)){
                    iconPath = pathPart;
                    iconIndex = idx;
                    return;
                }
            }
            iconPath = s;
            iconIndex = 0;
        }
        private Image _defaultIconCache;
        private Image GetDefaultIcon(){
            if (_defaultIconCache != null){
                return (Image)_defaultIconCache.Clone();
            }
            try{
                using (var ico = AppWindowIcon.ExtractIconFromFile("shell32.dll", 2, large: true)){
                    if (ico != null){
                        _defaultIconCache = ico.ToBitmap();
                        return (Image)_defaultIconCache.Clone();
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "GetDefaultIcon()"); }
            }
            Bitmap bmpFallback = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmpFallback)){
                g.Clear(TS_ThemeEngine.ColorMode(theme, "AccentColor"));
                g.DrawRectangle(Pens.Black, 4, 4, 24, 24);
            }
            _defaultIconCache = bmpFallback;
            return (Image)_defaultIconCache.Clone();
        }
        private void LoadInstalledApplications(){
            var apps = GetInstalledApplications();
            if (INSTAPPS_DataMainTable.InvokeRequired){
                INSTAPPS_DataMainTable.Invoke(new Action(() => INSTAPPS_DataMainTable.Rows.Clear()));
            }else{
                INSTAPPS_DataMainTable.Rows.Clear();
            }
            foreach (var app in apps){
                Image iconImage = null;
                if (!string.IsNullOrWhiteSpace(app.DisplayIcon)){
                    try{
                        ParseDisplayIcon(app.DisplayIcon, out string iconPath, out int iconIndex);
                        if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath)){
                            string ext = Path.GetExtension(iconPath);
                            if (string.Equals(ext, ".ico", StringComparison.OrdinalIgnoreCase)){
                                using (var ico = new Icon(iconPath)){
                                    iconImage = ico.ToBitmap();
                                }
                            }else{
                                using (var ico = AppWindowIcon.ExtractIconFromFile(iconPath, iconIndex, large: true)){
                                    if (ico != null){
                                        iconImage = ico.ToBitmap();
                                    }
                                }
                            }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "LoadInstalledApplications()"); }
                    }
                }
                if (iconImage == null && !string.IsNullOrWhiteSpace(app.InstallLocation) && Directory.Exists(app.InstallLocation)){
                    try{
                        var exe = Directory.EnumerateFiles(app.InstallLocation, "*.exe").FirstOrDefault(p => !p.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase) && !p.EndsWith("unins000.exe", StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(exe)){
                            using (Icon icon = Icon.ExtractAssociatedIcon(exe)){
                                if (icon != null){
                                    iconImage = icon.ToBitmap();
                                }
                            }
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "LoadInstalledApplications()"); }
                    }
                }
                if (iconImage == null){
                    iconImage = GetDefaultIcon();
                }
                iconImage = ResizeDGIcon(iconImage, 30, this.DeviceDpi);
                var nameText = string.IsNullOrWhiteSpace(app.Name) ? iapps_unknown : app.Name;
                var publisherText = string.IsNullOrWhiteSpace(app.Publisher) ? iapps_unknown : app.Publisher;
                var installDateText = app.InstallDate?.ToString("dd.MM.yyyy") ?? iapps_unknown;
                var sizeText = app.Size.HasValue ? TS_FormatSize(app.Size.Value * 1024) : iapps_unknown;
                var versionText = string.IsNullOrWhiteSpace(app.Version) ? iapps_unknown : app.Version;
                var installLocationText = string.IsNullOrWhiteSpace(app.InstallLocation) ? iapps_unknown : app.InstallLocation;
                if (INSTAPPS_DataMainTable.InvokeRequired){
                    INSTAPPS_DataMainTable.Invoke(new Action(() =>{
                        INSTAPPS_DataMainTable.Rows.Add(iconImage, nameText, publisherText, installDateText, sizeText, versionText, installLocationText);
                    }));
                }else{
                    INSTAPPS_DataMainTable.Rows.Add(iconImage, nameText, publisherText, installDateText, sizeText, versionText, installLocationText);
                }
            }
        }
        private List<InstalledAppConfig> GetInstalledApplications(){
            var apps = new List<InstalledAppConfig>();
            var registryViews = new[] { RegistryView.Registry64, RegistryView.Registry32 };
            var registryRoots = new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
            foreach (var view in registryViews){
                foreach (var root in registryRoots){
                    using (var baseKey = RegistryKey.OpenBaseKey(root, view))
                    using (var uninstallKey = baseKey.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall")){
                        if (uninstallKey == null) continue;
                        foreach (var subkeyName in uninstallKey.GetSubKeyNames()){
                            using (var subkey = uninstallKey.OpenSubKey(subkeyName)){
                                if (subkey == null) continue;
                                var displayIcon = (subkey.GetValue("DisplayIcon") as string)?.Trim()?.Trim('"');
                                var name = (subkey.GetValue("DisplayName") as string)?.Trim();
                                if (string.IsNullOrWhiteSpace(name)) continue;
                                var publisher = (subkey.GetValue("Publisher") as string)?.Trim();
                                var installLocationRaw = (subkey.GetValue("InstallLocation") as string)?.Trim();
                                var installLocation = installLocationRaw?.Trim('"');
                                var version = (subkey.GetValue("DisplayVersion") as string)?.Trim();
                                var installDateRaw = (subkey.GetValue("InstallDate") as string)?.Trim();
                                DateTime? installDate = null;
                                if (!string.IsNullOrWhiteSpace(installDateRaw) && installDateRaw.Length == 8){
                                    if (DateTime.TryParseExact(installDateRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)){
                                        installDate = parsedDate;
                                    }
                                }
                                var sizeValue = subkey.GetValue("EstimatedSize");
                                long? sizeKb = null;
                                if (sizeValue != null){
                                    try{
                                        sizeKb = Convert.ToInt64(sizeValue);
                                    }catch { }
                                }
                                apps.Add(new InstalledAppConfig{
                                    DisplayIcon = displayIcon,
                                    AppIcon = null,
                                    Name = name,
                                    Publisher = publisher,
                                    InstallDate = installDate,
                                    Size = sizeKb,
                                    Version = version,
                                    InstallLocation = installLocation
                                });
                            }
                        }
                    }
                }
            }
            return apps.GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).OrderBy(a => TSNaturalSortKey(a.Name)).ToList();
        }
        private void INSTAPPS_DataMainTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e){
            if (e.ColumnIndex == 0 && e.RowIndex >= 0){
                e.PaintBackground(e.CellBounds, true);
                if (e.Value is Image img){
                    float scale = this.DeviceDpi / 96f;
                    int iconSize = (int)(30 * scale);
                    int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;
                    var rect = new Rectangle(x, y, iconSize, iconSize);
                    e.Graphics.DrawImage(img, rect);
                }
                e.Handled = true;
            }
        }
        #endregion
        // BUTTONS ROTATE
        // ======================================================================================================
        private void Active_page(object btn_target){
            Button active_btn = null;
            Disabled_page();
            if (btn_target != null){
                if (active_btn != (Button)btn_target){
                    active_btn = (Button)btn_target;
                    active_btn.BackColor = TS_ThemeEngine.ColorMode(theme, "BtnActiveColor");
                    active_btn.Cursor = Cursors.Default;
                }
            }
        }
        private void Disabled_page(){
            foreach (Control disabled_btn in LeftMenuPanel.Controls){
                disabled_btn.BackColor = TS_ThemeEngine.ColorMode(theme, "BtnDeActiveColor");
                disabled_btn.Cursor = Cursors.Hand;
            }
        }
        // LEFT MENU SENDERS
        private void OSRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(1, sender); }
        private void MBRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(2, sender); }
        private void CPURotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(3, sender); }
        private void RAMRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(4, sender); }
        private void GPURotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(5, sender); }
        private void DISKRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(6, sender); }
        private void NETWORKRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(7, sender); }
        private void USB_RotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(8, sender); }
        private void SOUND_RotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(9, sender); }
        private void PILRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(10, sender); }
        private void OSDRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(11, sender); }
        private void ServicesRotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(12, sender); }
        private void INSTALLED_RotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(13, sender); }
        private void PRINT_RotateBtn_Click(object sender, EventArgs e) { Left_menu_preloader(14, sender); }
        // DYNAMIC ARROW KEYS ROTATE
        private void MainContent_Selecting(object sender, TabControlCancelEventArgs e){
            try{
                if (!e.TabPage.Enabled)
                    e.Cancel = true;
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "MainContent_Selecting()"); }
            }
        }
        private void MainContent_SelectedIndexChanged(object sender, EventArgs e){
            try{
                var tabButtons = new Dictionary<int, Button>{
                    { 0, OS_RotateBtn },
                    { 1, MB_RotateBtn },
                    { 2, CPU_RotateBtn },
                    { 3, RAM_RotateBtn },
                    { 4, GPU_RotateBtn },
                    { 5, DISK_RotateBtn },
                    { 6, NET_RotateBtn },
                    { 7, USB_RotateBtn },
                    { 8, SOUND_RotateBtn },
                    { 9, BATTERY_RotateBtn },
                    { 10, OSD_RotateBtn },
                    { 11, SERVICES_RotateBtn },
                    { 12, INSTALLED_RotateBtn },
                    { 13, PRINT_RotateBtn }
                };
                if (tabButtons.TryGetValue(MainContent.SelectedIndex, out Button btn)){
                    Active_page(btn);
                    menu_btns = MainContent.SelectedIndex + 1;
                    menu_rp = MainContent.SelectedIndex + 1;
                    UpdateHeaderByIndex(MainContent.SelectedIndex);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "MainContent_SelectedIndexChanged()"); }
            }
        }
        // DYNAMIC LEFT MENU PRELOADER
        private void Left_menu_preloader(int target_menu, object sender){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                var menuTargets = new Dictionary<int, (TabPage tab, Button rotateBtn, string headerKey)>{
                    { 1,  (OS, OS_RotateBtn, "header_os") },
                    { 2,  (MB, MB_RotateBtn, "header_mb") },
                    { 3,  (CPU, CPU_RotateBtn, "header_cpu") },
                    { 4,  (RAM, RAM_RotateBtn, "header_ram") },
                    { 5,  (GPU, GPU_RotateBtn, "header_gpu") },
                    { 6,  (DISK, DISK_RotateBtn, "header_storage") },
                    { 7,  (NETWORK, NET_RotateBtn, "header_network") },
                    { 8,  (USB, USB_RotateBtn, "header_usb") },
                    { 9,  (SOUND, SOUND_RotateBtn, "header_sound") },
                    { 10, (BATTERY, BATTERY_RotateBtn, "header_battery") },
                    { 11, (DRIVERS, OSD_RotateBtn, "header_installed_drivers") },
                    { 12, (SERVICES, SERVICES_RotateBtn, "header_installed_services") },
                    { 13, (INSTAPPS, INSTALLED_RotateBtn, "header_installed_apps") },
                    { 14, (EXPORT, PRINT_RotateBtn, "header_export") }
                };
                if (menu_btns != target_menu && menuTargets.TryGetValue(target_menu, out var target)){
                    MainContent.SelectedTab = target.tab;
                    if (!btn_colors_active.Contains(target.rotateBtn.BackColor))
                        Active_page(sender);
                    HeaderText.Text = software_lang.TSReadLangs("Header", target.headerKey);
                }
                menu_btns = target_menu;
                menu_rp = target_menu;
                Header_image_reloader(menu_btns);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Left_menu_preloader()"); }
            }
        }
        private void UpdateHeaderByIndex(int index){
            var keys = new[]{
                "header_os", "header_mb", "header_cpu", "header_ram", "header_gpu",
                "header_storage", "header_network", "header_usb", "header_sound",
                "header_battery", "header_installed_drivers", "header_installed_services",
                "header_installed_apps", "header_export"
            };
            if (index >= 0 && index < keys.Length){
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                HeaderText.Text = software_lang.TSReadLangs("Header", keys[index]);
                Header_image_reloader(index + 1);
            }
        }
        // INTERFACE NAVIGATION: Prevents focus traps and enables cyclic page switching with arrow keys
        // ======================================================================================================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData){
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right){
                int currentIndex = MainContent.SelectedIndex;
                int totalTabs = MainContent.TabCount;
                if (keyData == Keys.Down || keyData == Keys.Right){
                    MainContent.SelectedIndex = (currentIndex + 1) % totalTabs;
                }else if (keyData == Keys.Up || keyData == Keys.Left){
                    MainContent.SelectedIndex = (currentIndex - 1 + totalTabs) % totalTabs;
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        // LANGUAGES SETTINGS
        // ======================================================================================================
        private void Select_lang_active(object target_lang){
            ToolStripMenuItem selected_lang = null;
            Select_lang_deactive();
            if (target_lang != null){
                if (selected_lang != (ToolStripMenuItem)target_lang){
                    selected_lang = (ToolStripMenuItem)target_lang;
                    selected_lang.Checked = true;
                }
            }
        }
        private void Select_lang_deactive(){
            foreach (ToolStripMenuItem disabled_lang in languageToolStripMenuItem.DropDownItems){
                disabled_lang.Checked = false;
            }
        }
        // LANG SWAP
        // ======================================================================================================
        private void LanguageToolStripMenuItem_Click(object sender, EventArgs e){
            if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string langCode){
                if (lang != langCode && AllLanguageFiles.ContainsKey(langCode)){
                    Lang_preload(AllLanguageFiles[langCode], langCode);
                    Select_lang_active(sender);
                }
            }
        }
        private void Lang_preload(string lang_type, string lang_code){
            Lang_engine(lang_type, lang_code);
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "LanguageStatus", lang_code);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Lang_preload()"); }
            }
            // LANG CHANGE NOTIFICATION
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            DialogResult lang_change_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("LangChange", "lang_change_notification"), "\n\n", "\n\n"));
            if (lang_change_message == DialogResult.Yes){ Application.Restart(); }
        }
        private void Lang_engine(string lang_type, string lang_code){
            try{
                lang_path = lang_type;
                lang = lang_code;
                // GLOBAL ENGINE
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                // HEADER TITLE
                var headers = new Dictionary<int, string>{
                    { 1, "header_os" },
                    { 2, "header_mb" },
                    { 3, "header_cpu" },
                    { 4, "header_ram" },
                    { 5, "header_gpu" },
                    { 6, "header_storage" },
                    { 7, "header_network" },
                    { 8, "header_usb" },
                    { 9, "header_sound" },
                    { 10, "header_battery" },
                    { 11, "header_installed_drivers" },
                    { 12, "header_installed_services" },
                    { 13, "header_installed_apps" },
                    { 14, "header_export" }
                };
                if (headers.TryGetValue(menu_rp, out string headerKey)){
                    HeaderText.Text = software_lang.TSReadLangs("Header", headerKey);
                }
                // SETTINGS
                settingsToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_settings");
                // THEMES
                themeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_theme");
                lightThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_light");
                darkThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_dark");
                systemThemeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderThemes", "theme_system");
                // LANGS
                languageToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_language");
                arabicToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ar");
                chineseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_zh");
                englishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_en");
                dutchToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_nl");
                frenchToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_fr");
                germanToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_de");
                hindiToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_hi");
                italianToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_it");
                japaneseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ja");
                koreanToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ko");
                polishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_pl");
                portugueseToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_pt");
                russianToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_ru");
                spanishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_es");
                turkishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_tr");
                // STARTUP VIEW
                startupToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_start");
                windowedToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_windowed");
                fullScreenToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_full_screen");
                // HIDING MODE
                hidingModeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_privacy_mode");
                hidingModeOnToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_on");
                hidingModeOffToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderHidingMode", "header_hiding_mode_off");
                // DEBUG MODE
                debugModeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_debug_mode");
                debugOnToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_on");
                debugOffToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_off");
                debugFolderToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder");
                debugZIPToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip");
                debugDeleteToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete");
                // UPDATE CHECK
                checkForUpdatesToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_update");
                // TOOLS
                toolsToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_tools");
                sFCandDISMAutoTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_dism_and_sfc_tool");
                benchCPUTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_bench_cpu");
                benchRAMTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_bench_ram");
                benchDiskTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_bench_disk");
                cacheCleaningTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_cache_cleanup_tool");
                systemIdAnalysisTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_system_id_analysis_tool");
                screenOverlayTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_overlay");
                bluetoothFinderToolToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderTools", "ht_bluetooth_finder_tool");
                dnsTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_dns_test_tool");
                networkFixTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_network_fix_tool");
                showWiFiPasswordTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_show_wifi_password_tool");
                monitorTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test");
                monitorDeadPixelTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dead_pixel");
                monitorDynamicRangeTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dynamic_range");
                monitorStuckPixelFixerTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_stuck_pixel_fixer_tool");
                // DONATE
                donateToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_donate");
                // ABOUT
                aboutToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_about");
                // HIDING MENU
                OS.Text = software_lang.TSReadLangs("LeftMenu", "left_os");
                MB.Text = software_lang.TSReadLangs("LeftMenu", "left_mb");
                CPU.Text = software_lang.TSReadLangs("LeftMenu", "left_cpu");
                RAM.Text = software_lang.TSReadLangs("LeftMenu", "left_ram");
                GPU.Text = software_lang.TSReadLangs("LeftMenu", "left_gpu");
                DISK.Text = software_lang.TSReadLangs("LeftMenu", "left_storage");
                NETWORK.Text = software_lang.TSReadLangs("LeftMenu", "left_network");
                USB.Text = software_lang.TSReadLangs("LeftMenu", "left_usb");
                SOUND.Text = software_lang.TSReadLangs("LeftMenu", "left_sound");
                BATTERY.Text = software_lang.TSReadLangs("LeftMenu", "left_battery");
                DRIVERS.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_drivers");
                SERVICES.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_services");
                INSTAPPS.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_apps");
                EXPORT.Text = software_lang.TSReadLangs("LeftMenu", "left_export");
                // MENU
                OS_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_os");
                MB_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_mb");
                CPU_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_cpu");
                RAM_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_ram");
                GPU_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_gpu");
                DISK_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_storage");
                NET_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_network");
                USB_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_usb");
                SOUND_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_sound");
                BATTERY_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_battery");
                OSD_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_installed_drivers");
                SERVICES_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_installed_services");
                INSTALLED_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_installed_apps");
                PRINT_RotateBtn.Text = " " + " " + software_lang.TSReadLangs("LeftMenu", "left_export");
                // OS
                OS_SystemUser.Text = software_lang.TSReadLangs("OperatingSystem", "os_user");
                OS_ComputerName.Text = software_lang.TSReadLangs("OperatingSystem", "os_computer_name");
                OS_SavedUser.Text = software_lang.TSReadLangs("OperatingSystem", "os_saved_user_account");
                OS_Name.Text = software_lang.TSReadLangs("OperatingSystem", "os_operating_system");
                OS_Manufacturer.Text = software_lang.TSReadLangs("OperatingSystem", "os_operating_system_publisher");
                OS_SystemVersion.Text = software_lang.TSReadLangs("OperatingSystem", "os_system_version");
                OS_SystemArchitectural.Text = software_lang.TSReadLangs("OperatingSystem", "os_system_architecture");
                OS_DeviceID.Text = software_lang.TSReadLangs("OperatingSystem", "os_device_id");
                OS_Serial.Text = software_lang.TSReadLangs("OperatingSystem", "os_product_id");
                OS_ExperienceVersion.Text = software_lang.TSReadLangs("OperatingSystem", "os_experience_version");
                OS_Country.Text = software_lang.TSReadLangs("OperatingSystem", "os_adjustable_language");
                OS_TimeZone.Text = software_lang.TSReadLangs("OperatingSystem", "os_timezone");
                OS_SystemTime.Text = software_lang.TSReadLangs("OperatingSystem", "os_time");
                OS_SystemWorkTime.Text = software_lang.TSReadLangs("OperatingSystem", "os_system_work_time");
                OS_LastBootTime.Text = software_lang.TSReadLangs("OperatingSystem", "os_last_boot_time");
                OS_SystemLastShutDown.Text = software_lang.TSReadLangs("OperatingSystem", "os_last_shutdown_time");
                OS_Install.Text = software_lang.TSReadLangs("OperatingSystem", "os_install_date");
                OS_PrimaryOS.Text = software_lang.TSReadLangs("OperatingSystem", "os_primary_os");
                OS_PortableOS.Text = software_lang.TSReadLangs("OperatingSystem", "os_portable_os");
                OS_FastBoot.Text = software_lang.TSReadLangs("OperatingSystem", "os_fastboot_status");
                OS_AVProgram.Text = software_lang.TSReadLangs("OperatingSystem", "os_installed_anti_virus_apps");
                OS_FirewallProgram.Text = software_lang.TSReadLangs("OperatingSystem", "os_installed_firewall_apps");
                OS_AntiSpywareProgram.Text = software_lang.TSReadLangs("OperatingSystem", "os_installed_anti_spyware_apps");
                OS_WinDefCoreIsolation.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_core_isolation_status");
                OS_ActivePower.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power");
                OS_ActivePowerGUID.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power_guid");
                OS_ActivePowerScreenTimeOutP.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power_sctp");
                OS_ActivePowerScreenTimeOutB.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power_sctb");
                OS_ActivePowerSleepTimeP.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power_sptp");
                OS_ActivePowerSleepTimeB.Text = software_lang.TSReadLangs("OperatingSystem", "os_active_power_sptb");
                OS_MSOfficeVersion.Text = software_lang.TSReadLangs("OperatingSystem", "os_ms_office_version");
                OS_WinLicenseTime.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_license_time");
                OS_WinKey.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_license_key");
                OS_WinActiveChannel.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_activation_channel");
                OS_WinLicenseURL.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_license_url");
                OS_WinLicenseVerifiURL.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_license_verification_url");
                OS_NETFrameworkVersion.Text = software_lang.TSReadLangs("OperatingSystem", "os_net_framework_version");
                OS_Minidump.Text = software_lang.TSReadLangs("OperatingSystem", "os_detect_minidump_count");
                OS_BSODDate.Text = software_lang.TSReadLangs("OperatingSystem", "os_last_bsod_time");
                OS_Wallpaper.Text = software_lang.TSReadLangs("OperatingSystem", "os_wallpaper");
                OS_RecoveryModeBtn.Text = " " + software_lang.TSReadLangs("OperatingSystem", "os_recovery_mode");
                if (minidump_files_list.Count > 0){
                    MainToolTip.SetToolTip(OS_MinidumpOpen, Directory.Exists(mdp_1) ? string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_folder"), mdp_1) : string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_file"), mdp_2));
                    MainToolTip.SetToolTip(OS_BSoDZIP, software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_folder"));
                }
                if (!string.IsNullOrEmpty(wp_rotate)){
                    MainToolTip.SetToolTip(OS_WallpaperOpen, software_lang.TSReadLangs("Os_Content", "os_c_open_wallpaper"));
                    MainToolTip.SetToolTip(OS_WallpaperPreview, software_lang.TSReadLangs("Os_Content", "os_c_preview_wallpaper"));
                }
                TS_AdjustButtonWidth(OS_RecoveryModeBtn);
                // MB
                MB_BIOSUpdateBtn.Text = " " + software_lang.TSReadLangs("Motherboard", "mb_b_update_btn");
                MB_BIOSOpenBtn.Text = " " + software_lang.TSReadLangs("Motherboard", "mb_b_open_btn");
                MB_MotherBoardName.Text = software_lang.TSReadLangs("Motherboard", "mb_model");
                MB_MotherBoardMan.Text = software_lang.TSReadLangs("Motherboard", "mb_manufacturer");
                MB_SystemModelMan.Text = software_lang.TSReadLangs("Motherboard", "mb_system_model_manufacturer");
                MB_SystemModelFamily.Text = software_lang.TSReadLangs("Motherboard", "mb_system_model_family");
                MB_SystemFamily.Text = software_lang.TSReadLangs("Motherboard", "mb_system_family");
                MB_SystemModel.Text = software_lang.TSReadLangs("Motherboard", "mb_system_model");
                MB_DeviceSerialNumber.Text = software_lang.TSReadLangs("Motherboard", "mb_device_serial_number");
                MB_MotherBoardSerial.Text = software_lang.TSReadLangs("Motherboard", "mb_serial");
                MB_SystemSKU.Text = software_lang.TSReadLangs("Motherboard", "mb_system_sku");
                MB_Chipset.Text = software_lang.TSReadLangs("Motherboard", "mb_chipset");
                MB_BiosManufacturer.Text = software_lang.TSReadLangs("Motherboard", "mb_bios_publisher");
                MB_BiosDate.Text = software_lang.TSReadLangs("Motherboard", "mb_bios_date");
                MB_BiosVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_bios_version");
                MB_SmBiosVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_sm_bios_version");
                MB_BiosMode.Text = software_lang.TSReadLangs("Motherboard", "mb_bios_mode");
                MB_LastBIOSTime.Text = software_lang.TSReadLangs("Motherboard", "mb_last_bios_time");
                MB_SecureBoot.Text = software_lang.TSReadLangs("Motherboard", "mb_secure_boot_status");
                MB_TPMStatus.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_status");
                MB_TPMPPIVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_ppi_version");
                MB_TPMPPIAltVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_ppi_alt_version");
                MB_TPMMan.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_manufacturer");
                MB_TPMManID.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_manufacturer_id");
                MB_TPMManVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_manufacturer_version");
                MB_TPMManFullVersion.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_manufacturer_full_version");
                MB_TPMManPublisher.Text = software_lang.TSReadLangs("Motherboard", "mb_tpm_publisher");
                TS_AdjustButtonWidth(MB_BIOSUpdateBtn);
                TS_AdjustButtonWidth(MB_BIOSOpenBtn);
                // CPU
                CPU_Name.Text = software_lang.TSReadLangs("Processor", "pr_cpu_model");
                CPU_Manufacturer.Text = software_lang.TSReadLangs("Processor", "pr_cpu_manufacturer");
                CPU_Architectural.Text = software_lang.TSReadLangs("Processor", "pr_cpu_architecture");
                CPU_Speed.Text = software_lang.TSReadLangs("Processor", "pr_cpu_speed");
                CPU_CoreCount.Text = software_lang.TSReadLangs("Processor", "pr_cpu_core_count");
                CPU_ActiveCoreCount.Text = software_lang.TSReadLangs("Processor", "pr_cpu_active_core_count");
                CPU_LogicalCore.Text = software_lang.TSReadLangs("Processor", "pr_cpu_logical_core_count");
                CPU_L1.Text = software_lang.TSReadLangs("Processor", "pr_l1_cache_size");
                CPU_L2.Text = software_lang.TSReadLangs("Processor", "pr_l2_cache_size");
                CPU_L3.Text = software_lang.TSReadLangs("Processor", "pr_l3_cache_size");
                CPU_SerialName.Text = software_lang.TSReadLangs("Processor", "pr_unique_processor_id");
                CPU_Family.Text = software_lang.TSReadLangs("Processor", "pr_cpu_family");
                CPU_Slot.Text = software_lang.TSReadLangs("Processor", "pr_cpu_slot");
                CPU_SocketDefinition.Text = software_lang.TSReadLangs("Processor", "pr_cpu_socket_definition");
                //
                CPU_AllCPUHead.Text = software_lang.TSReadLangs("Processor", "pr_cpu_head_text");
                CPU_Virtualization.Text = software_lang.TSReadLangs("Processor", "pr_cpu_virtualization");
                CPU_Usage.Text = software_lang.TSReadLangs("Processor", "pr_cpu_usage");
                CPU_Process.Text = software_lang.TSReadLangs("Processor", "pr_cpu_process_count");
                CPU_Threads.Text = software_lang.TSReadLangs("Processor", "pr_cpu_threads_count");
                CPU_Handles.Text = software_lang.TSReadLangs("Processor", "pr_cpu_handles_count");
                // RAM
                RAM_TotalRAM.Text = software_lang.TSReadLangs("Memory", "my_total_ram_amount");
                RAM_UsageRAMCount.Text = software_lang.TSReadLangs("Memory", "my_usage_ram_amount");
                RAM_EmptyRamCount.Text = software_lang.TSReadLangs("Memory", "my_empty_ram_amount");
                RAM_TotalVirtualRam.Text = software_lang.TSReadLangs("Memory", "my_total_virtual_ram_amount");
                RAM_UsageVirtualRam.Text = software_lang.TSReadLangs("Memory", "my_usage_virtual_ram_amount");
                RAM_EmptyVirtualRam.Text = software_lang.TSReadLangs("Memory", "my_empty_virtual_ram_amount");
                RAM_SlotStatus.Text = software_lang.TSReadLangs("Memory", "my_ram_slot_fullness");
                RAM_Selector.Text = software_lang.TSReadLangs("Memory", "my_ram_slot");
                RAM_Amount.Text = software_lang.TSReadLangs("Memory", "my_ram_amount");
                RAM_Type.Text = software_lang.TSReadLangs("Memory", "my_ram_type");
                RAM_Frequency.Text = software_lang.TSReadLangs("Memory", "my_ram_frequency");
                RAM_Volt.Text = software_lang.TSReadLangs("Memory", "my_ram_voltage");
                RAM_FormFactor.Text = software_lang.TSReadLangs("Memory", "my_ram_form_factor");
                RAM_Serial.Text = software_lang.TSReadLangs("Memory", "my_ram_serial");
                RAM_Manufacturer.Text = software_lang.TSReadLangs("Memory", "my_ram_manufacturer");
                RAM_BankLabel.Text = software_lang.TSReadLangs("Memory", "my_ram_location");
                RAM_DataWidth.Text = software_lang.TSReadLangs("Memory", "my_ram_width");
                RAM_BellekType.Text = software_lang.TSReadLangs("Memory", "my_partition_type");
                RAM_PartNumber.Text = software_lang.TSReadLangs("Memory", "my_partition_number");
                // GPU
                GPU_Selector.Text = software_lang.TSReadLangs("Gpu", "gpu_model");
                GPU_Manufacturer.Text = software_lang.TSReadLangs("Gpu", "gpu_publisher");
                GPU_VRAM.Text = software_lang.TSReadLangs("Gpu", "gpu_vram");
                GPU_Version.Text = software_lang.TSReadLangs("Gpu", "gpu_driver_version");
                GPU_DriverDate.Text = software_lang.TSReadLangs("Gpu", "gpu_driver_date");
                GPU_Status.Text = software_lang.TSReadLangs("Gpu", "gpu_status");
                GPU_DeviceID.Text = software_lang.TSReadLangs("Gpu", "gpu_hardware_id");
                GPU_DacType.Text = software_lang.TSReadLangs("Gpu", "gpu_dac_type");
                GPU_GraphicDriversName.Text = software_lang.TSReadLangs("Gpu", "gpu_graphic_drivers");
                GPU_DirectXAll.Text = software_lang.TSReadLangs("Gpu", "gpu_directx_all");
                GPU_DirectXMax.Text = software_lang.TSReadLangs("Gpu", "gpu_directx_max");
                GPU_InfFileName.Text = software_lang.TSReadLangs("Gpu", "gpu_inf_file");
                GPU_INFSectionFile.Text = software_lang.TSReadLangs("Gpu", "gpu_inf_file_gpu_partition");
                GPU_CurrentColor.Text = software_lang.TSReadLangs("Gpu", "gpu_current_color");
                //
                GPU_MonitorSelector.Text = software_lang.TSReadLangs("Gpu", "gpu_monitors");
                GPU_MonitorUserFriendlyName.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_name");
                GPU_MonitorManName.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_manufacturer");
                GPU_MonitorProductCodeID.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_manufacturer_num");
                GPU_MonitorSerialNumberID.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_serial_number");
                GPU_MonitorConType.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_connect_type");
                GPU_MonitorManfDate.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_manf_date");
                GPU_MonitorManfDateWeek.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_manf_date_week");
                GPU_MonitorHID.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_hardware_id");
                GPU_MonitorResLabel.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_resolution");
                GPU_MonitorVirtualRes.Text = software_lang.TSReadLangs("Gpu", "gpu_virtual_resolution");
                GPU_MonitorBounds.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_limit");
                GPU_MonitorWorking.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_work_area_size");
                GPU_ScreenRefreshRate.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_refresh_rate");
                GPU_ScreenBit.Text = software_lang.TSReadLangs("Gpu", "gpu_monitor_bit_rate");
                GPU_MonitorPrimary.Text = software_lang.TSReadLangs("Gpu", "gpu_primary_screen");
                // DISK
                DISK_TTLP_L1.Text = software_lang.TSReadLangs("Storage", "se_t_title");
                DISK_TTLP_P1_L1.Text = software_lang.TSReadLangs("Storage", "se_t_ssd");
                DISK_TTLP_P2_L1.Text = software_lang.TSReadLangs("Storage", "se_t_hdd");
                DISK_TTLP_P3_L1.Text = software_lang.TSReadLangs("Storage", "se_t_usb");
                DISK_TTLP_P4_L1.Text = software_lang.TSReadLangs("Storage", "se_t_total");
                DISK_Selector.Text = software_lang.TSReadLangs("Storage", "se_name");
                DISK_Model.Text = software_lang.TSReadLangs("Storage", "se_model");
                DISK_Man.Text = software_lang.TSReadLangs("Storage", "se_manufacturer");
                DISK_VolumeID.Text = software_lang.TSReadLangs("Storage", "se_partition_letter");
                DISK_VolumeName.Text = software_lang.TSReadLangs("Storage", "se_partition_name");
                DISK_Firmware.Text = software_lang.TSReadLangs("Storage", "se_firmware_version");
                DISK_Serial.Text = software_lang.TSReadLangs("Storage", "se_serial");
                DISK_VolumeSerial.Text = software_lang.TSReadLangs("Storage", "se_partition_serial");
                DISK_Size.Text = software_lang.TSReadLangs("Storage", "se_size");
                DISK_UsedSpace.Text = software_lang.TSReadLangs("Storage", "se_used_space");
                DISK_FreeSpace.Text = software_lang.TSReadLangs("Storage", "se_empty_space");
                DISK_FileSystem.Text = software_lang.TSReadLangs("Storage", "se_file_system");
                DISK_FormattingType.Text = software_lang.TSReadLangs("Storage", "se_formatting_type");
                DISK_Type.Text = software_lang.TSReadLangs("Storage", "se_type");
                DISK_DriveType.Text = software_lang.TSReadLangs("Storage", "se_drive_type");
                DISK_InterFace.Text = software_lang.TSReadLangs("Storage", "se_interface_type");
                DISK_PartitionCount.Text = software_lang.TSReadLangs("Storage", "se_partition_count");
                DISK_MediaLoaded.Text = software_lang.TSReadLangs("Storage", "se_work_status");
                DISK_MediaStatus.Text = software_lang.TSReadLangs("Storage", "se_status");
                DISK_Health.Text = software_lang.TSReadLangs("Storage", "se_health");
                DISK_Boot.Text = software_lang.TSReadLangs("Storage", "se_primary_disk");
                DISK_Bootable.Text = software_lang.TSReadLangs("Storage", "se_bootable_disk");
                DISK_BitLockerStatus.Text = software_lang.TSReadLangs("Storage", "se_bitlocker_status");
                DISK_BitLockerConversionStatus.Text = software_lang.TSReadLangs("Storage", "se_bitlocker_encrypt_status");
                DISK_BitLockerEncryptMehod.Text = software_lang.TSReadLangs("Storage", "se_bitlocker_encrypt_method");
                DISK_DriveCompressed.Text = software_lang.TSReadLangs("Storage", "se_compress_status");
                // NETWORK
                NET_LT_Device.Text = software_lang.TSReadLangs("Network", "nk_live_speed_adapter");
                NET_LT_BandWidth.Text = software_lang.TSReadLangs("Network", "nk_live_speed_band_width");
                NET_LT_LocalIP.Text = software_lang.TSReadLangs("Network", "nk_local_ip_address");
                NET_LT_GatewayIP.Text = software_lang.TSReadLangs("Network", "nk_gateway_ip_address");
                if (!string.IsNullOrEmpty(NET_LT_GatewayIP_V.Text.Trim())){
                    MainToolTip.SetToolTip(NET_RotateGateway, software_lang.TSReadLangs("Network", "nk_gateway_ip_hover"));
                }
                NET_LT_DL1.Text = software_lang.TSReadLangs("Network", "nk_live_speed_download");
                NET_LT_UL1.Text = software_lang.TSReadLangs("Network", "nk_live_speed_upload");
                // 
                NET_Selector.Text = software_lang.TSReadLangs("Network", "nk_network_device");
                NET_MacAdress.Text = software_lang.TSReadLangs("Network", "nk_mac_adress");
                NET_NetMan.Text = software_lang.TSReadLangs("Network", "nk_driver_provider");
                NET_DriverVersion.Text = software_lang.TSReadLangs("Network", "nk_driver_version");
                NET_DriverDate.Text = software_lang.TSReadLangs("Network", "nk_driver_date");
                NET_ServiceName.Text = software_lang.TSReadLangs("Network", "nk_service_name");
                NET_AdapterType.Text = software_lang.TSReadLangs("Network", "nk_adaptor_type");
                NET_Physical.Text = software_lang.TSReadLangs("Network", "nk_physical_adaptor");
                NET_DeviceID.Text = software_lang.TSReadLangs("Network", "nk_network_hardware_id");
                NET_Guid.Text = software_lang.TSReadLangs("Network", "nk_guid");
                NET_ConnectionType.Text = software_lang.TSReadLangs("Network", "nk_connection_type");
                NET_Dhcp_status.Text = software_lang.TSReadLangs("Network", "nk_dhcp_status");
                NET_Dhcp_server.Text = software_lang.TSReadLangs("Network", "nk_dhcp_server");
                NET_DHCPFirstIpTime.Text = software_lang.TSReadLangs("Network", "nk_dhcp_first_ip_time");
                NET_DHCPLastIpTime.Text = software_lang.TSReadLangs("Network", "nk_dhcp_last_ip_time");
                NET_LocalConSpeed.Text = software_lang.TSReadLangs("Network", "nk_connection_speed");
                NET_IPv4Adress.Text = software_lang.TSReadLangs("Network", "nk_appointed_ipv4_adress");
                NET_IPv6Adress.Text = software_lang.TSReadLangs("Network", "nk_appointed_ipv6_adress");
                NET_P_IP_Adress.Text = software_lang.TSReadLangs("Network", "nk_p_ip");
                NET_P_ISP.Text = software_lang.TSReadLangs("Network", "nk_p_isp");
                NET_DNS_v4_1.Text = software_lang.TSReadLangs("Network", "nk_v4_dns1");
                NET_DNS_v4_2.Text = software_lang.TSReadLangs("Network", "nk_v4_dns2");
                NET_DNS_v6_1.Text = software_lang.TSReadLangs("Network", "nk_v6_dns1");
                NET_DNS_v6_2.Text = software_lang.TSReadLangs("Network", "nk_v6_dns2");
                // USB
                USB_Selector.Text = software_lang.TSReadLangs("Usb", "usb_controller");
                USB_ConName.Text = software_lang.TSReadLangs("Usb", "usb_controller_name");
                USB_ConMan.Text = software_lang.TSReadLangs("Usb", "usb_controller_publisher");
                USB_ConDeviceID.Text = software_lang.TSReadLangs("Usb", "usb_controller_hardware_id");
                USB_ConPNPDeviceID.Text = software_lang.TSReadLangs("Usb", "usb_controller_pnp_hardware_id");
                USB_ConDeviceStatus.Text = software_lang.TSReadLangs("Usb", "usb_controller_status");
                USB_DeviceSelector.Text = software_lang.TSReadLangs("Usb", "usb_device");
                USB_DeviceName.Text = software_lang.TSReadLangs("Usb", "usb_device_name");
                USB_DeviceMan.Text = software_lang.TSReadLangs("Usb", "usb_device_manufacturer");
                USB_DriverVersion.Text = software_lang.TSReadLangs("Usb", "usb_device_driver_version");
                USB_DriverDate.Text = software_lang.TSReadLangs("Usb", "usb_device_driver_date");
                USB_InfFile.Text = software_lang.TSReadLangs("Usb", "usb_device_inf_file");
                USB_DeviceID.Text = software_lang.TSReadLangs("Usb", "usb_device_device_id");
                USB_HardwareID.Text = software_lang.TSReadLangs("Usb", "usb_device_hardware_id");
                USB_DeviceGUID.Text = software_lang.TSReadLangs("Usb", "usb_device_guid");
                // SOUND
                SOUND_Selector.Text = software_lang.TSReadLangs("Sound", "sound_device");
                SOUND_DeviceName.Text = software_lang.TSReadLangs("Sound", "sound_device_name");
                SOUND_DeviceManufacturer.Text = software_lang.TSReadLangs("Sound", "sound_device_publisher");
                SOUND_DriverVersion.Text = software_lang.TSReadLangs("Sound", "sound_device_version");
                SOUND_DriverDate.Text = software_lang.TSReadLangs("Sound", "sound_device_date");
                SOUND_DeviceID.Text = software_lang.TSReadLangs("Sound", "sound_device_hardware_id");
                SOUND_PNPDeviceID.Text = software_lang.TSReadLangs("Sound", "sound_device_pnp_hardware_id");
                SOUND_DeviceStatus.Text = software_lang.TSReadLangs("Sound", "sound_device_status");
                // BATTERY
                BATTERY_Status.Text = software_lang.TSReadLangs("Battery", "by_status");
                BATTERY_Model.Text = software_lang.TSReadLangs("Battery", "by_model");
                BATTERY_Serial.Text = software_lang.TSReadLangs("Battery", "by_serial");
                BATTERY_Chemistry.Text = software_lang.TSReadLangs("Battery", "by_chemistry");
                BATTERY_DesignCapacity.Text = software_lang.TSReadLangs("Battery", "by_design_capacity");
                BATTERY_FullChargeCapacity.Text = software_lang.TSReadLangs("Battery", "by_full_charge_capacity");
                BATTERY_RemainingChargeCapacity.Text = software_lang.TSReadLangs("Battery", "by_remaining_charge_capacity");
                BATTERY_Voltage.Text = software_lang.TSReadLangs("Battery", "by_voltage");
                BATTERY_ChargePower.Text = software_lang.TSReadLangs("Battery", "by_charge_power");
                BATTERY_ChargeCurrent.Text = software_lang.TSReadLangs("Battery", "by_charge_current");
                BATTERY_DeChargePower.Text = software_lang.TSReadLangs("Battery", "by_decharge_power");
                BATTERY_DeChargeCurrent.Text = software_lang.TSReadLangs("Battery", "by_decharge_current");
                BATTERY_ReportBtn.Text = " " + software_lang.TSReadLangs("Battery", "by_report_btn");
                TS_AdjustButtonWidth(BATTERY_ReportBtn); // Dynamic Width
                // OSD
                OSD_DataMainTable.Columns[0].HeaderText = software_lang.TSReadLangs("Osd", "osd_file_path");
                OSD_DataMainTable.Columns[1].HeaderText = software_lang.TSReadLangs("Osd", "osd_file_name");
                OSD_DataMainTable.Columns[2].HeaderText = software_lang.TSReadLangs("Osd", "osd_driver_name");
                OSD_DataMainTable.Columns[3].HeaderText = software_lang.TSReadLangs("Osd", "osd_driver_type");
                OSD_DataMainTable.Columns[4].HeaderText = software_lang.TSReadLangs("Osd", "osd_start");
                OSD_DataMainTable.Columns[5].HeaderText = software_lang.TSReadLangs("Osd", "osd_status");
                OSD_SearchDriverLabel.Text = software_lang.TSReadLangs("Osd", "osd_search_driver");
                OSD_TYSS.Text = software_lang.TSReadLangs("Osd", "osd_installed_driver_count");
                OSD_SortMode.Text = software_lang.TSReadLangs("Osd", "osd_order_in_reverse");
                // SERVICES
                SERVICE_DataMainTable.Columns[0].HeaderText = software_lang.TSReadLangs("Services", "ss_file_path");
                SERVICE_DataMainTable.Columns[1].HeaderText = software_lang.TSReadLangs("Services", "ss_file_name");
                SERVICE_DataMainTable.Columns[2].HeaderText = software_lang.TSReadLangs("Services", "ss_service_name");
                SERVICE_DataMainTable.Columns[3].HeaderText = software_lang.TSReadLangs("Services", "ss_service_type");
                SERVICE_DataMainTable.Columns[4].HeaderText = software_lang.TSReadLangs("Services", "ss_start");
                SERVICE_DataMainTable.Columns[5].HeaderText = software_lang.TSReadLangs("Services", "ss_status");
                SERVICE_SearchLabel.Text = software_lang.TSReadLangs("Services", "ss_search_service");
                SERVICE_TYS.Text = software_lang.TSReadLangs("Services", "ss_installed_service_count");
                SERVICE_SortMode.Text = software_lang.TSReadLangs("Services", "ss_order_in_reverse");
                // INSTALLED APPS
                INSTAPPS_DataMainTable.Columns[0].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_icon");
                INSTAPPS_DataMainTable.Columns[1].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_name");
                INSTAPPS_DataMainTable.Columns[2].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_developer");
                INSTAPPS_DataMainTable.Columns[3].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_date");
                INSTAPPS_DataMainTable.Columns[4].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_size");
                INSTAPPS_DataMainTable.Columns[5].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_version");
                INSTAPPS_DataMainTable.Columns[6].HeaderText = software_lang.TSReadLangs("Instapps", "ia_apps_path");
                INSTAPPS_SearchAppsLabel.Text = software_lang.TSReadLangs("Instapps", "ia_search_apps");
                INSTAPPS_TYUS.Text = software_lang.TSReadLangs("Instapps", "ia_installed_apps_count");
                INSTAPPS_SortMode.Text = software_lang.TSReadLangs("Instapps", "ia_order_in_reverse");
                iapps_unknown = software_lang.TSReadLangs("Instapps_Content", "ia_unknown");
                // EXPORT
                EXPORT_Selector.Text = software_lang.TSReadLangs("Export", "e_mode_title");
                Export_ModeTitle.Text = software_lang.TSReadLangs("Export", "e_mode_filter_title");
                //
                if (EXPORT_Selector_List.Items.Count > 0){
                    EXPORT_Selector_List.Items[0] = software_lang.TSReadLangs("Export", "e_mode_txt");
                    EXPORT_Selector_List.Items[1] = software_lang.TSReadLangs("Export", "e_mode_html");
                    EXPORT_Selector_List.Items[2] = software_lang.TSReadLangs("Export", "e_mode_markdown");
                }
                if (EXPORT_ModeSelector_List.Items.Count > 0){
                    EXPORT_ModeSelector_List.Items[0] = software_lang.TSReadLangs("Export", "e_mode_filter_basic");
                    EXPORT_ModeSelector_List.Items[1] = software_lang.TSReadLangs("Export", "e_mode_filter_advanced");
                    EXPORT_ModeSelector_List.Items[2] = software_lang.TSReadLangs("Export", "e_mode_filter_full");
                }
                //
                EXPORT_CB_OS.Text = software_lang.TSReadLangs("LeftMenu", "left_os");
                EXPORT_CB_MB.Text = software_lang.TSReadLangs("LeftMenu", "left_mb");
                EXPORT_CB_CPU.Text = software_lang.TSReadLangs("LeftMenu", "left_cpu");
                EXPORT_CB_RAM.Text = software_lang.TSReadLangs("LeftMenu", "left_ram");
                EXPORT_CB_GPU.Text = software_lang.TSReadLangs("LeftMenu", "left_gpu");
                EXPORT_CB_STORAGE.Text = software_lang.TSReadLangs("LeftMenu", "left_storage");
                EXPORT_CB_NETWORK.Text = software_lang.TSReadLangs("LeftMenu", "left_network");
                EXPORT_CB_USB.Text = software_lang.TSReadLangs("LeftMenu", "left_usb");
                EXPORT_CB_SOUND.Text = software_lang.TSReadLangs("LeftMenu", "left_sound");
                EXPORT_CB_BATTERY.Text = software_lang.TSReadLangs("LeftMenu", "left_battery");
                EXPORT_CB_DRIVERS.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_drivers");
                EXPORT_CB_SERVICES.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_services");
                EXPORT_CB_APPS.Text = software_lang.TSReadLangs("LeftMenu", "left_installed_apps");
                //
                EXPORT_StartEngineBtn.Text = " " + software_lang.TSReadLangs("Export", "e_mode_report_start");
                //
                EXPORT_DonateLabel.Text = software_lang.TSReadLangs("ExportSupport", "es_text");
                EXPORT_Donate.Text = " " + " " + software_lang.TSReadLangs("ExportSupport", "es_btn_text");
                TS_AdjustButtonWidth(EXPORT_StartEngineBtn); // Dynamic Width
                TS_AdjustButtonWidth(EXPORT_Donate); // Dynamic Width
                // CHANGE COPYABLE LABELS TOOLTIP TEXT
                this.BeginInvoke(new MethodInvoker(() => { TSSetTooltips(allCopyableLabels); }));
                // SCROLL TOP
                MainToolTip.SetToolTip(OS_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(MB_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(CPU_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(RAM_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(GPU_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(DISK_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                MainToolTip.SetToolTip(NET_ScrollTop, software_lang.TSReadLangs("GToolsMessage", "gtm_scroll_top"));
                // OTHER PAGE DYNAMIC UI
                Glow_other_page_dynamic_ui();
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Lang_engine()"); }
            }
        }
        // THEME SETTINGS
        // ======================================================================================================
        private ToolStripMenuItem selected_theme = null;
        private void Select_theme_active(object target_theme){
            if (target_theme == null)
                return;
            ToolStripMenuItem clicked_theme = (ToolStripMenuItem)target_theme;
            if (selected_theme == clicked_theme)
                return;
            Select_theme_deactive();
            selected_theme = clicked_theme;
            selected_theme.Checked = true;
        }
        private void Select_theme_deactive(){
            foreach (ToolStripMenuItem theme in themeToolStripMenuItem.DropDownItems){
                theme.Checked = false;
            }
        }
        // THEME SWAP
        // ======================================================================================================
        private void SystemThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 2; Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); SaveTheme(2); Select_theme_active(sender);
        }
        private void LightThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 0; Theme_engine(1); SaveTheme(1); Select_theme_active(sender); 
        }
        private void DarkThemeToolStripMenuItem_Click(object sender, EventArgs e){
            themeSystem = 0; Theme_engine(0); SaveTheme(0); Select_theme_active(sender); 
        }
        private void TSUseSystemTheme(){ if (themeSystem == 2) Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); }
        private void SaveTheme(int ts){
            // SAVE CURRENT THEME
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "ThemeStatus", Convert.ToString(ts));
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "SaveTheme()"); }
            }
        }
        // THEME ENGINE
        // ======================================================================================================
        private void Theme_engine(int ts){
            try{
                theme = ts;
                //
                TSThemeModeHelper.SetThemeMode(ts == 0);
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                if (theme == 1){
                    // LEFT MENU LOGO CHANGE
                    if (Program.windows_mode == 1){
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w11_light, 18, ContentAlignment.MiddleLeft);
                    }else{
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w10_light, 18, ContentAlignment.MiddleLeft);
                    }
                    TSImageRenderer(MB_RotateBtn, Properties.Resources.lm_mb_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(CPU_RotateBtn, Properties.Resources.lm_cpu_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(RAM_RotateBtn, Properties.Resources.lm_ram_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(GPU_RotateBtn, Properties.Resources.lm_gpu_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(DISK_RotateBtn, Properties.Resources.lm_disk_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(NET_RotateBtn, Properties.Resources.lm_net_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(USB_RotateBtn, Properties.Resources.lm_usb_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(SOUND_RotateBtn, Properties.Resources.lm_sound_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(BATTERY_RotateBtn, Properties.Resources.lm_battery_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(OSD_RotateBtn, Properties.Resources.lm_idrivers_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(SERVICES_RotateBtn, Properties.Resources.lm_iservices_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(INSTALLED_RotateBtn, Properties.Resources.lm_iapps_light, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(PRINT_RotateBtn, Properties.Resources.lm_export_light, 18, ContentAlignment.MiddleLeft);
                    // TOP MENU LOGO CHANGE
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(hidingModeToolStripMenuItem, Properties.Resources.tm_hidden_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(debugModeToolStripMenuItem, Properties.Resources.tm_debug_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(checkForUpdatesToolStripMenuItem, Properties.Resources.tm_update_light, 0, ContentAlignment.MiddleRight);
                    // TOOLS
                    TSImageRenderer(toolsToolStripMenuItem, Properties.Resources.tm_tools_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(sFCandDISMAutoTool, Properties.Resources.cx_sfc_and_dism_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchCPUTool, Properties.Resources.cx_bench_cpu_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchRAMTool, Properties.Resources.cx_bench_ram_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchDiskTool, Properties.Resources.cx_bench_disk_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(cacheCleaningTool, Properties.Resources.cx_cache_clean_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(systemIdAnalysisTool, Properties.Resources.cx_system_id_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(screenOverlayTool, Properties.Resources.cx_overlay_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(bluetoothFinderToolToolStripMenuItem, Properties.Resources.cx_bt_finder_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(dnsTestTool, Properties.Resources.cx_dns_test_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(networkFixTool, Properties.Resources.cx_network_fix_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(showWiFiPasswordTool, Properties.Resources.cx_swpt_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorTestTool, Properties.Resources.cx_test_monitor_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorDeadPixelTestTool, Properties.Resources.cx_test_dead_pixel_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorDynamicRangeTestTool, Properties.Resources.cx_test_dynamic_range_light, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorStuckPixelFixerTool, Properties.Resources.cx_stuck_pixel_fixer_light, 0, ContentAlignment.MiddleRight);
                    // MIDDLE CONTENT LOGO CHANGE
                    TSImageRenderer(OS_MinidumpOpen, Properties.Resources.ct_link_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_BSoDZIP, Properties.Resources.ct_zip_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_WallpaperOpen, Properties.Resources.ct_link_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_WallpaperPreview, Properties.Resources.ct_image_preview_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_RecoveryModeBtn, Properties.Resources.ct_link_mc_light, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(MB_BIOSUpdateBtn, Properties.Resources.ct_mb_update_light, 16, ContentAlignment.MiddleRight);
                    TSImageRenderer(MB_BIOSOpenBtn, Properties.Resources.ct_link_mc_light, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(NET_RotateGateway, Properties.Resources.ct_link_light, 0, ContentAlignment.MiddleCenter);
                    //
                    TSImageRenderer(BATTERY_ReportBtn, Properties.Resources.ct_battery_report_light, 18, ContentAlignment.MiddleRight);
                    TSImageRenderer(EXPORT_StartEngineBtn, Properties.Resources.ct_export_light, 18, ContentAlignment.MiddleRight);
                    TSImageRenderer(EXPORT_Donate, Properties.Resources.tm_donate_mc_light, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(INSTAPPS_TextBoxClearBtn, Properties.Resources.ct_clear_light, 15, ContentAlignment.MiddleCenter);
                    TSImageRenderer(SERVICE_TextBoxClearBtn, Properties.Resources.ct_clear_light, 15, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OSD_TextBoxClearBtn, Properties.Resources.ct_clear_light, 15, ContentAlignment.MiddleCenter);
                    // DONATE
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_light, 0, ContentAlignment.MiddleRight);
                    // ABOUT
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_light, 0, ContentAlignment.MiddleRight);
                    // SCROLL TOP
                    TSImageRenderer(OS_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(MB_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(CPU_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(RAM_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(GPU_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(DISK_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(NET_ScrollTop, Properties.Resources.ct_scroll_top_light, 24, ContentAlignment.MiddleCenter);
                }else if (theme == 0){
                    // LEFT MENU LOGO CHANGE
                    if (Program.windows_mode == 1){
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w11_dark, 18, ContentAlignment.MiddleLeft);
                    }else{
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w10_dark, 18, ContentAlignment.MiddleLeft);
                    }
                    TSImageRenderer(MB_RotateBtn, Properties.Resources.lm_mb_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(CPU_RotateBtn, Properties.Resources.lm_cpu_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(RAM_RotateBtn, Properties.Resources.lm_ram_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(GPU_RotateBtn, Properties.Resources.lm_gpu_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(DISK_RotateBtn, Properties.Resources.lm_disk_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(NET_RotateBtn, Properties.Resources.lm_net_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(USB_RotateBtn, Properties.Resources.lm_usb_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(SOUND_RotateBtn, Properties.Resources.lm_sound_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(BATTERY_RotateBtn, Properties.Resources.lm_battery_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(OSD_RotateBtn, Properties.Resources.lm_idrivers_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(SERVICES_RotateBtn, Properties.Resources.lm_iservices_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(INSTALLED_RotateBtn, Properties.Resources.lm_iapps_dark, 18, ContentAlignment.MiddleLeft);
                    TSImageRenderer(PRINT_RotateBtn, Properties.Resources.lm_export_dark, 18, ContentAlignment.MiddleLeft);
                    // TOP MENU LOGO CHANGE
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(hidingModeToolStripMenuItem, Properties.Resources.tm_hidden_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(debugModeToolStripMenuItem, Properties.Resources.tm_debug_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(checkForUpdatesToolStripMenuItem, Properties.Resources.tm_update_dark, 0, ContentAlignment.MiddleRight);
                    // TOOLS
                    TSImageRenderer(toolsToolStripMenuItem, Properties.Resources.tm_tools_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(sFCandDISMAutoTool, Properties.Resources.cx_sfc_and_dism_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchCPUTool, Properties.Resources.cx_bench_cpu_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchRAMTool, Properties.Resources.cx_bench_ram_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(benchDiskTool, Properties.Resources.cx_bench_disk_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(cacheCleaningTool, Properties.Resources.cx_cache_clean_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(systemIdAnalysisTool, Properties.Resources.cx_system_id_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(screenOverlayTool, Properties.Resources.cx_overlay_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(bluetoothFinderToolToolStripMenuItem, Properties.Resources.cx_bt_finder_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(dnsTestTool, Properties.Resources.cx_dns_test_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(networkFixTool, Properties.Resources.cx_network_fix_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(showWiFiPasswordTool, Properties.Resources.cx_swpt_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorTestTool, Properties.Resources.cx_test_monitor_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorDeadPixelTestTool, Properties.Resources.cx_test_dead_pixel_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorDynamicRangeTestTool, Properties.Resources.cx_test_dynamic_range_dark, 0, ContentAlignment.MiddleRight);
                    TSImageRenderer(monitorStuckPixelFixerTool, Properties.Resources.cx_stuck_pixel_fixer_dark, 0, ContentAlignment.MiddleRight);
                    // MIDDLE CONTENT LOGO CHANGE
                    TSImageRenderer(OS_MinidumpOpen, Properties.Resources.ct_link_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_BSoDZIP, Properties.Resources.ct_zip_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_WallpaperOpen, Properties.Resources.ct_link_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_WallpaperPreview, Properties.Resources.ct_image_preview_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OS_RecoveryModeBtn, Properties.Resources.ct_link_mc_dark, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(MB_BIOSUpdateBtn, Properties.Resources.ct_mb_update_dark, 16, ContentAlignment.MiddleRight);
                    TSImageRenderer(MB_BIOSOpenBtn, Properties.Resources.ct_link_mc_dark, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(NET_RotateGateway, Properties.Resources.ct_link_dark, 0, ContentAlignment.MiddleCenter);
                    //
                    TSImageRenderer(BATTERY_ReportBtn, Properties.Resources.ct_battery_report_dark, 18, ContentAlignment.MiddleRight);
                    TSImageRenderer(EXPORT_StartEngineBtn, Properties.Resources.ct_export_dark, 18, ContentAlignment.MiddleRight);
                    TSImageRenderer(EXPORT_Donate, Properties.Resources.tm_donate_mc_dark, 18, ContentAlignment.MiddleRight);
                    //
                    TSImageRenderer(INSTAPPS_TextBoxClearBtn, Properties.Resources.ct_clear_dark, 15, ContentAlignment.MiddleCenter);
                    TSImageRenderer(SERVICE_TextBoxClearBtn, Properties.Resources.ct_clear_dark, 15, ContentAlignment.MiddleCenter);
                    TSImageRenderer(OSD_TextBoxClearBtn, Properties.Resources.ct_clear_dark, 15, ContentAlignment.MiddleCenter);
                    // DONATE
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_dark, 0, ContentAlignment.MiddleRight);
                    // ABOUT
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_dark, 0, ContentAlignment.MiddleRight);
                    // SCROLL TOP
                    TSImageRenderer(OS_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(MB_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(CPU_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(RAM_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(GPU_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(DISK_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                    TSImageRenderer(NET_ScrollTop, Properties.Resources.ct_scroll_top_dark, 24, ContentAlignment.MiddleCenter);
                }
                // OTHER PAGE DYNAMIC UI
                Glow_other_page_dynamic_ui();
                // HEADER
                Header_image_reloader(menu_btns);
                header_colors[0] = TS_ThemeEngine.ColorMode(theme, "HeaderBGColorMain");
                header_colors[1] = TS_ThemeEngine.ColorMode(theme, "HeaderFEColorMain");
                header_colors[2] = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                HeaderMenu.Renderer = new HeaderMenuColors();
                // ACTIVE BTN 
                btn_colors_active[0] = TS_ThemeEngine.ColorMode(theme, "BtnActiveColor");
                // TOOLTIP
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(theme, "HeaderFEColor");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(theme, "HeaderBGColor");
                // HEADER MENU
                var bg = TS_ThemeEngine.ColorMode(theme, "HeaderBGColor");
                var fg = TS_ThemeEngine.ColorMode(theme, "HeaderFEColor");
                Header_InPanel.BackColor = bg;
                HeaderText.ForeColor = fg;
                HeaderMenu.ForeColor = fg;
                HeaderMenu.BackColor = bg;
                SetMenuStripColors(HeaderMenu, bg, fg);
                // LEFT MENU
                LeftMenuPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                OS_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                MB_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                CPU_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                RAM_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                GPU_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                DISK_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                NET_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                USB_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                SOUND_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                BATTERY_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                OSD_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                SERVICES_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                INSTALLED_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                PRINT_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                // LEFT MENU BORDER
                OS_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                MB_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                CPU_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                RAM_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                GPU_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                DISK_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                NET_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                USB_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                SOUND_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                BATTERY_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                OSD_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                SERVICES_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                INSTALLED_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                PRINT_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuBGAndBorderColor");
                // LEFT MENU MOUSE HOVER
                OS_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                MB_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                CPU_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                RAM_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                GPU_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                DISK_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                NET_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                USB_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                SOUND_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                BATTERY_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                OSD_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                SERVICES_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                INSTALLED_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                PRINT_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                // LEFT MENU MOUSE DOWN
                OS_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                MB_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                CPU_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                RAM_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                GPU_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                DISK_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                NET_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                USB_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                SOUND_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                BATTERY_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                OSD_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                SERVICES_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                INSTALLED_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                PRINT_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                // LEFT MENU BUTTON TEXT COLOR
                OS_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                MB_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                CPU_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                RAM_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                GPU_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                DISK_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                NET_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                USB_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                SOUND_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                BATTERY_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                OSD_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                SERVICES_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                INSTALLED_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                PRINT_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonFEColor");
                // CONTENT BG
                BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                OS.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                MB.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                CPU.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                RAM.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                GPU.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DISK.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                NETWORK.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                USB.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                SOUND.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                BATTERY.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DRIVERS.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                SERVICES.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                INSTAPPS.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                EXPORT.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                // OS
                os_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                os_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                os_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                os_panel_4.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                os_panel_5.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                os_panel_6.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                OS_SystemUser.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemUser_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ComputerName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ComputerName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SavedUser.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SavedUser_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Name.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Name_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SystemVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SystemArchitectural.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemArchitectural_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ExperienceVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ExperienceVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Country.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Country_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_TimeZone.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_TimeZone_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SystemTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SystemWorkTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemWorkTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_LastBootTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_LastBootTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_SystemLastShutDown.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_SystemLastShutDown_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Install.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Install_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_PrimaryOS.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_PrimaryOS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_PortableOS.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_PortableOS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_FastBoot.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_FastBoot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_AVProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_AVProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_FirewallProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_FirewallProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_AntiSpywareProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_AntiSpywareProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinDefCoreIsolation.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinDefCoreIsolation_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePowerGUID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePowerGUID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePowerScreenTimeOutP.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePowerScreenTimeOutP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePowerScreenTimeOutB.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePowerScreenTimeOutB_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePowerSleepTimeP.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePowerSleepTimeP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_ActivePowerSleepTimeB.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_ActivePowerSleepTimeB_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_MSOfficeVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_MSOfficeVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinLicenseTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinLicenseTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinKey.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinKey_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinActiveChannel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinActiveChannel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinLicenseURL.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinLicenseURL_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_WinLicenseVerifiURL.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_WinLicenseVerifiURL_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_NETFrameworkVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_NETFrameworkVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Minidump.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Minidump_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_BSODDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_BSODDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_Wallpaper.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OS_Wallpaper_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                OS_RecoveryModeBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                OS_RecoveryModeBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                // MB
                mb_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                mb_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                mb_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                MB_BIOSUpdateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                MB_BIOSUpdateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                MB_BIOSOpenBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                MB_BIOSOpenBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                MB_MotherBoardName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_MotherBoardName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_MotherBoardMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_MotherBoardMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SystemModelMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SystemModelMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SystemModelFamily.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SystemModelFamily_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SystemFamily.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SystemFamily_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SystemModel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SystemModel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_DeviceSerialNumber.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_DeviceSerialNumber_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_MotherBoardSerial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_MotherBoardSerial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SystemSKU.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SystemSKU_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_Chipset.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_Chipset_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BiosManufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_BiosManufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BiosDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_BiosDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BiosVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_BiosVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SmBiosVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SmBiosVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_BiosMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_BiosMode_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_LastBIOSTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_LastBIOSTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_SecureBoot.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_SecureBoot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMPPIVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMPPIVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMPPIAltVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMPPIAltVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMManID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMManID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMManVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMManVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMManFullVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMManFullVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                MB_TPMManPublisher.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                MB_TPMManPublisher_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // CPU
                cpu_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                cpu_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                CPU_Name.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                CPU_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                CPU_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                CPU_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                CPU_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                CPU_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                CPU_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                CPU_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                CPU_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                CPU_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                CPU_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Architectural.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Architectural_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Speed.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Speed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_CoreCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_CoreCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_ActiveCoreCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_ActiveCoreCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_LogicalCore.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_LogicalCore_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_L1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_L2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_L3.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_L3_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_SerialName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_SerialName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Family.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Family_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Slot.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Slot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_SocketDefinition.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_SocketDefinition_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                CPU_AllCPUHead.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Virtualization.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Virtualization_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Usage.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Usage_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Process.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Process_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Threads.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Threads_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                CPU_Handles.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                CPU_Handles_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // RAM
                ram_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                ram_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                RAM_TotalRAM.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_TotalRAM_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_UsageRAMCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_UsageRAMCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_EmptyRamCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_EmptyRamCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_TotalVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_TotalVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_UsageVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_UsageVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_EmptyVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_EmptyVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_SlotStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_SlotStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                RAM_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                RAM_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                RAM_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                RAM_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                RAM_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                RAM_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                RAM_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                RAM_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                RAM_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                RAM_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                RAM_Amount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Amount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Type.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Type_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Frequency.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Frequency_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Volt.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Volt_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_FormFactor.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_FormFactor_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_BankLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_BankLabel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_DataWidth.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_DataWidth_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_BellekType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_BellekType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_PartNumber.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                RAM_PartNumber_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                RAM_ProgressBGPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                RAM_ProgressFEPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                RAM_ProgressLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // GPU
                gpu_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                gpu_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                GPU_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                GPU_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                GPU_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                GPU_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                GPU_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                GPU_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_VRAM.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_VRAM_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_Version.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_Version_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_Status.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_Status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_DacType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_DacType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_GraphicDriversName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_GraphicDriversName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_DirectXAll.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_DirectXAll_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_DirectXMax.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_DirectXMax_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_InfFileName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_InfFileName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_INFSectionFile.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_INFSectionFile_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_CurrentColor.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_CurrentColor_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                GPU_MonitorSelector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                GPU_MonitorSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_MonitorSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                GPU_MonitorSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                GPU_MonitorSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_MonitorSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                GPU_MonitorSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_MonitorSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_MonitorSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                GPU_MonitorSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                GPU_MonitorUserFriendlyName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorUserFriendlyName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorManName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorManName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorProductCodeID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorProductCodeID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorSerialNumberID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorSerialNumberID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorConType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorConType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorManfDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorManfDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorManfDateWeek.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorManfDateWeek_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorHID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorHID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorResLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorResLabel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorVirtualRes.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorVirtualRes_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorBounds.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorBounds_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorWorking.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorWorking_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_ScreenRefreshRate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_ScreenRefreshRate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_ScreenBit.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_ScreenBit_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                GPU_MonitorPrimary.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                GPU_MonitorPrimary_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // DISK
                disk_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                disk_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                disk_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                DISK_TTLP_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_TTLP_P1_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_TTLP_P2_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_TTLP_P3_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_TTLP_P4_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                //
                DISK_TLP_PB_1.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentBlue");
                DISK_TLP_PB_2.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentPurple");
                DISK_TLP_PB_3.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentRed");
                DISK_TLP_PB_4.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentGreen");
                DISK_TTLP_P1_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentBlue");
                DISK_TTLP_P2_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentPurple");
                DISK_TTLP_P3_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentRed");
                DISK_TTLP_P4_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentGreen");
                //
                DISK_TTLP_Panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DISK_TTLP_Panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DISK_TTLP_Panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DISK_TTLP_Panel_4.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                //
                DISK_PBar_BG.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                DISK_PBar_FE.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_PBar_Label.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                DISK_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                DISK_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                DISK_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                DISK_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                DISK_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                DISK_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                DISK_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                DISK_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                DISK_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                DISK_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                DISK_Model.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Model_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Man.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Man_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_VolumeID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_VolumeID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_VolumeName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_VolumeName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Firmware.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Firmware_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_VolumeSerial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_VolumeSerial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Size.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Size_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_UsedSpace.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_UsedSpace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_FreeSpace.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_FreeSpace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_FileSystem.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_FileSystem_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_FormattingType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_FormattingType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Type.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Type_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_DriveType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_DriveType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_InterFace.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_InterFace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_PartitionCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_PartitionCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_MediaLoaded.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_MediaLoaded_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_MediaStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_MediaStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Health.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Health_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Boot.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Boot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_Bootable.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_Bootable_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_BitLockerStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_BitLockerStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_BitLockerConversionStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_BitLockerConversionStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_BitLockerEncryptMehod.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_BitLockerEncryptMehod_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                DISK_DriveCompressed.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                DISK_DriveCompressed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // NETWORK
                network_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                network_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                network_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                NET_LT_Device.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_Device_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LT_BandWidth.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_BandWidth_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LT_LocalIP.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_LocalIP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LT_GatewayIP.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_GatewayIP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LT_P1.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                NET_LT_P2.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                NET_LT_DL1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_DL2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LT_UL1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LT_UL2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                NET_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                NET_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                NET_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                NET_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                NET_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                NET_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                NET_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                NET_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                NET_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                NET_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                NET_MacAdress.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_MacAdress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_NetMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_NetMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_ServiceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_ServiceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_AdapterType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_AdapterType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_Physical.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_Physical_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_Guid.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_Guid_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_ConnectionType.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_ConnectionType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_Dhcp_status.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_Dhcp_status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_Dhcp_server.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_Dhcp_server_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DHCPFirstIpTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DHCPFirstIpTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DHCPLastIpTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DHCPLastIpTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_LocalConSpeed.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_LocalConSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_IPv4Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_IPv4Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_IPv6Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_IPv6Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_P_IP_Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_P_IP_Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_P_ISP.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_P_ISP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DNS_v4_1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DNS_v4_1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DNS_v4_2.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DNS_v4_2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DNS_v6_1.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DNS_v6_1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                NET_DNS_v6_2.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                NET_DNS_v6_2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // USB
                usb_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                usb_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                USB_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                USB_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                USB_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                USB_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                USB_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                USB_ConName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_ConName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_ConMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_ConMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_ConDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_ConDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_ConPNPDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_ConPNPDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_ConDeviceStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_ConDeviceStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                USB_DeviceSelector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DeviceSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                USB_DeviceSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_DeviceSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                USB_DeviceSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                USB_DeviceSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_DeviceSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                USB_DeviceSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_DeviceSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_DeviceSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                USB_DeviceSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DeviceSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                USB_DeviceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DeviceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DeviceMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DeviceMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_InfFile.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_InfFile_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_HardwareID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_HardwareID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                USB_DeviceGUID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                USB_DeviceGUID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // SOUND
                sound_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                SOUND_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                SOUND_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                SOUND_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                SOUND_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                SOUND_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                SOUND_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                SOUND_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                SOUND_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                SOUND_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                SOUND_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                //
                SOUND_DeviceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DeviceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_DeviceManufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DeviceManufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_PNPDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_PNPDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SOUND_DeviceStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SOUND_DeviceStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // BATTERY
                battery_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                battery_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                BATTERY_Status.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_Status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_Model.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_Model_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_Chemistry.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_Chemistry_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_DesignCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_DesignCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_FullChargeCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_FullChargeCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_RemainingChargeCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_RemainingChargeCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_Voltage.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_Voltage_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ChargePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_ChargePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ChargeCurrent.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_ChargeCurrent_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_DeChargePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_DeChargePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_DeChargeCurrent.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                BATTERY_DeChargeCurrent_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                BATTERY_ReportBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                BATTERY_ReportBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ReportBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ReportBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ReportBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                BATTERY_PBG_Panel.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                BATTERY_PFE_Panel.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                BATTERY_ProgressLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                // INSTALLED DRIVERS
                osd_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                OSD_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TextBoxBGColor");
                OSD_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                OSD_TYSS.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OSD_TYSS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OSD_SearchDriverLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OSD_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                OSD_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                OSD_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OSD_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OSD_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                OSD_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                OSD_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                OSD_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                OSD_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                OSD_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                OSD_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "DataGridColor");
                OSD_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                OSD_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridFEColor");
                OSD_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridAlternatingColor");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                OSD_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                OSD_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                // SERVICES
                service_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                SERVICE_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TextBoxBGColor");
                SERVICE_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                SERVICE_TYS.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SERVICE_TYS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SERVICE_SearchLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SERVICE_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                SERVICE_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                SERVICE_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SERVICE_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SERVICE_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                SERVICE_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                SERVICE_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                SERVICE_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                SERVICE_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                SERVICE_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                SERVICE_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "DataGridColor");
                SERVICE_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                SERVICE_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridFEColor");
                SERVICE_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridAlternatingColor");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                SERVICE_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                SERVICE_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                // INSTALLED APPS
                instapps_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                INSTAPPS_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TextBoxBGColor");
                INSTAPPS_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TextBoxFEColor");
                INSTAPPS_TYUS.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                INSTAPPS_TYUS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                INSTAPPS_SearchAppsLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                INSTAPPS_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                INSTAPPS_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                INSTAPPS_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                INSTAPPS_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                INSTAPPS_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                INSTAPPS_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                INSTAPPS_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                INSTAPPS_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "DataGridColor");
                INSTAPPS_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridBGColor");
                INSTAPPS_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "DataGridFEColor");
                INSTAPPS_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "DataGridAlternatingColor");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                INSTAPPS_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageBG");
                INSTAPPS_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "OSDAndServicesPageFE");
                // PRINT
                print_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                print_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "ContentPanelBGColor");
                //
                EXPORT_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                EXPORT_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                EXPORT_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                EXPORT_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_Selector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_Selector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_Selector_List.DisabledArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_Selector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                //
                EXPORT_ProgressBGPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "PageContainerBGAndPageContentTotalColors");
                EXPORT_ProgressFEPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_ProgessLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                //
                Export_ModeTitle.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                //
                EXPORT_ModeSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_ModeSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_ModeSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_ModeSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                EXPORT_ModeSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_ModeSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                EXPORT_ModeSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_ModeSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_ModeSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_ModeSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_ModeSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_ModeSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                EXPORT_ModeSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_ModeSelector_List.DisabledArrowColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxFEColor");
                EXPORT_ModeSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor2");
                //
                EXPORT_CB_FLP.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                foreach (Control c in EXPORT_CB_FLP.Controls){
                    if (c is TSCustomCheckBox cb){
                        cb.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                        cb.CheckedColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                        cb.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBGColor");
                        cb.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                    }
                }
                //
                EXPORT_StartEngineBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                EXPORT_StartEngineBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                EXPORT_Donate.ForeColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                EXPORT_Donate.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_Donate.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_Donate.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                EXPORT_Donate.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                EXPORT_DonateLabel.BackColor = TS_ThemeEngine.ColorMode(theme, "LeftMenuButtonHoverAndMouseDownColor");
                EXPORT_DonateLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "ContentLabelLeft");
                // SCROLL TOP
                var st_fg = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                var st_bg = TS_ThemeEngine.ColorMode(theme, "AccentColor");
                var st_hover = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                new[] { OS_ScrollTop, CPU_ScrollTop, MB_ScrollTop, RAM_ScrollTop, GPU_ScrollTop, DISK_ScrollTop, NET_ScrollTop }.ToList().ForEach(btn =>{
                    btn.ForeColor = st_fg;
                    btn.BackColor = st_bg;
                    btn.FlatAppearance.BorderColor = st_bg;
                    btn.FlatAppearance.MouseDownBackColor = st_bg;
                    btn.FlatAppearance.MouseOverBackColor = st_hover;
                });
                // ROTATE MENU
                var buttonMap = new Dictionary<int, Button>{
                    { 1, OS_RotateBtn },
                    { 2, MB_RotateBtn },
                    { 3, CPU_RotateBtn },
                    { 4, RAM_RotateBtn },
                    { 5, GPU_RotateBtn },
                    { 6, DISK_RotateBtn },
                    { 7, NET_RotateBtn },
                    { 8, USB_RotateBtn },
                    { 9, SOUND_RotateBtn },
                    { 10, BATTERY_RotateBtn },
                    { 11, OSD_RotateBtn },
                    { 12, SERVICES_RotateBtn },
                    { 13, INSTALLED_RotateBtn },
                    { 14, PRINT_RotateBtn }
                };
                if (buttonMap.TryGetValue(menu_btns, out Button selectedButton)){
                    selectedButton.BackColor = TS_ThemeEngine.ColorMode(theme, "DynamicThemeActiveBtnBG");
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Theme_engine()"); }
            }
        }
        private void SetMenuStripColors(MenuStrip menuStrip, Color bgColor, Color fgColor){
            if (menuStrip == null) return;
            foreach (ToolStripItem item in menuStrip.Items){
                if (item is ToolStripMenuItem menuItem){
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void SetMenuItemColors(ToolStripMenuItem menuItem, Color bgColor, Color fgColor){
            if (menuItem == null) return;
            menuItem.BackColor = bgColor;
            menuItem.ForeColor = fgColor;
            foreach (ToolStripItem item in menuItem.DropDownItems){
                if (item is ToolStripMenuItem subMenuItem){
                    SetMenuItemColors(subMenuItem, bgColor, fgColor);
                }
            }
        }
        private void SetContextMenuColors(ContextMenuStrip contextMenu, Color bgColor, Color fgColor){
            if (contextMenu == null) return;
            foreach (ToolStripItem item in contextMenu.Items){
                if (item is ToolStripMenuItem menuItem){
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void Header_image_reloader(int hi_value){
            try{
                var imageMap = new Dictionary<int, string>{
                    { 1, (theme == 1) ? (Program.windows_mode == 1 ? "lm_os_w11_light" : "lm_os_w10_light") : (Program.windows_mode == 1 ? "lm_os_w11_dark" : "lm_os_w10_dark") },
                    { 2, theme == 1 ? "lm_mb_light" : "lm_mb_dark" },
                    { 3, theme == 1 ? "lm_cpu_light" : "lm_cpu_dark" },
                    { 4, theme == 1 ? "lm_ram_light" : "lm_ram_dark" },
                    { 5, theme == 1 ? "lm_gpu_light" : "lm_gpu_dark" },
                    { 6, theme == 1 ? "lm_disk_light" : "lm_disk_dark" },
                    { 7, theme == 1 ? "lm_net_light" : "lm_net_dark" },
                    { 8, theme == 1 ? "lm_usb_light" : "lm_usb_dark" },
                    { 9, theme == 1 ? "lm_sound_light" : "lm_sound_dark" },
                    { 10, theme == 1 ? "lm_battery_light" : "lm_battery_dark" },
                    { 11, theme == 1 ? "lm_idrivers_light" : "lm_idrivers_dark" },
                    { 12, theme == 1 ? "lm_iservices_light" : "lm_iservices_dark" },
                    { 13, theme == 1 ? "lm_iapps_light" : "lm_iapps_dark" },
                    { 14, theme == 1 ? "lm_export_light" : "lm_export_dark" }
                };
                if (imageMap.TryGetValue(hi_value, out string imageName)){
                    TSImageRenderer(HeaderImage, (Image)Properties.Resources.ResourceManager.GetObject(imageName), 0, ContentAlignment.MiddleCenter);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Header_image_reloader()"); }
            }
        }
        // MODULES PAGE DYNAMIC UI
        // ======================================================================================================
        private void Glow_other_page_dynamic_ui(){
            try{
                var glow_other_pages = new (string name, Func<object> createTool, Action<object> applySettings)[]{
                    ("glow_sfc_and_dism_tool", () => new GlowSFCandDISMAutoTool(), tool => ((GlowSFCandDISMAutoTool)tool).SADTLoadEngine()),
                    ("glow_bench_cpu_tool", () => new GlowBenchCPUTool(), tool => ((GlowBenchCPUTool)tool).Bench_cpu_theme_settings()),
                    ("glow_bench_ram_tool", () => new GlowBenchMemoryTool(), tool => ((GlowBenchMemoryTool)tool).Bench_ram_settings()),
                    ("glow_bench_disk_tool", () => new GlowBenchDiskTool(), tool => ((GlowBenchDiskTool)tool).Bench_disk_theme_settings()),
                    ("glow_cache_cleanup_tool", () => new GlowCacheCleanupTool(), tool => ((GlowCacheCleanupTool)tool).Cct_theme_settings()),
                    ("glow_system_id_analysis_tool", () => new GlowSystemIDAnalysisTool(), tool => ((GlowSystemIDAnalysisTool)tool).SIG_Preloader()),
                    ("glow_screen_overlay_tool", () => new GlowOverlayTool(), tool => ((GlowOverlayTool)tool).Screen_overlay_settings()),
                    ("glow_bluetooth_finder_tool", () => new GlowBluetoothFinderTool(), tool => ((GlowBluetoothFinderTool)tool).BTFinder_Preloader()),
                    ("glow_dns_test_tool", () => new GlowDNSTestTool(), tool => ((GlowDNSTestTool)tool).Dns_test_settings()),
                    ("glow_network_fix_tool", () => new GlowNetworkFixTool(), tool => ((GlowNetworkFixTool)tool).Nft_theme_settings()),
                    ("glow_show_wifi_password_tool", () => new GlowShowWiFiPasswordTool(), tool => ((GlowShowWiFiPasswordTool)tool).Swpt_theme_settings()),
                    ("glow_monitor_test_engine_dead_pixel", () => new GlowMonitorTestTool(), tool => ((GlowMonitorTestTool)tool).Monitor_test_engine_theme_settings()),
                    ("glow_monitor_test_engine_dynamic_range", () => new GlowMonitorTestTool(), tool => ((GlowMonitorTestTool)tool).Monitor_test_engine_theme_settings()),
                    ("glow_monitor_stuck_pixel_fixer", () => new GlowStuckPixelFixerTool(), tool => ((GlowStuckPixelFixerTool)tool).ChangeDynamicUI()),
                    ("glow_wallpaper_preview_tool", () => new GlowWallpaperPreviewTool(), tool => ((GlowWallpaperPreviewTool)tool).WP_Preview_theme_settings()),
                    ("glow_about", () => new GlowAbout(), tool => ((GlowAbout)tool).About_preloader()),
                };
                foreach (var (toolName, createTool, applySettings) in glow_other_pages){
                    try{
                        var tool = createTool();
                        tool.GetType().GetProperty("Name")?.SetValue(tool, toolName);
                        if (Application.OpenForms[toolName] != null){
                            tool = Application.OpenForms[toolName];
                            applySettings(tool);
                        }
                    }catch (Exception ex){
                        if (debug_status) { TSErrorLog.LogException(ex, "Glow_other_page_dynamic_ui()"); }
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Glow_other_page_dynamic_ui()"); }
            }
        }
        // SCROLL TOP MODULE
        // ======================================================================================================
        private CancellationTokenSource _resizeCts;
        private void MainContent_Resize(object sender, EventArgs e){
            _resizeCts?.Cancel();
            _resizeCts = new CancellationTokenSource();
            var token = _resizeCts.Token;
            _ = Task.Run(async () =>{
                try{
                    await Task.Delay(100, token);
                    if (token.IsCancellationRequested) return;
                    MainContent.Invoke((Action)UpdateScrollTopButtonVisibility);
                }catch (TaskCanceledException) { }
            });
        }
        private void UpdateScrollTopButtonVisibility(){
            var tab = MainContent.SelectedTab;
            if (tab == null) return;
            bool hasScroll = tab.DisplayRectangle.Height > tab.ClientSize.Height;
            foreach (var btn in new[]{
                OS_ScrollTop,
                CPU_ScrollTop,
                MB_ScrollTop,
                RAM_ScrollTop,
                GPU_ScrollTop,
                DISK_ScrollTop,
                NET_ScrollTop
            }){
                btn.Visible = hasScroll;
            }
        }
        private bool _isScrolling = false;
        private async void ScrollTop_Click(object sender, EventArgs e){
            if (_isScrolling) return;
            try{
                _isScrolling = true;
                await TS_ScrollTop();
            }finally{
                _isScrolling = false;
            }
        }
        private async Task TS_ScrollTop(){
            var tab = MainContent.SelectedTab;
            if (tab == null) return;
            int start = tab.VerticalScroll.Value;
            if (start <= 0) return;
            int duration = 250;
            var sw = Stopwatch.StartNew();
            while (true){
                double t = sw.Elapsed.TotalMilliseconds / duration;
                if (t >= 1.0) break;
                double eased = 1 - Math.Pow(1 - t, 3);
                int value = (int)(start * (1 - eased));
                tab.AutoScrollPosition = new Point(0, value);
                tab.VerticalScroll.Value = value;
                tab.Update();
                await Task.Yield();
            }
            tab.AutoScrollPosition = new Point(0, 0);
            tab.VerticalScroll.Value = 0;
            tab.Focus();
            tab.PerformLayout();
        }
        // STARTUP SETINGS
        // ======================================================================================================
        private void Select_startup_mode_active(object target_startup_mode){
            ToolStripMenuItem selected_startup_mode = null;
            Select_startup_mode_deactive();
            if (target_startup_mode != null){
                if (selected_startup_mode != (ToolStripMenuItem)target_startup_mode){
                    selected_startup_mode = (ToolStripMenuItem)target_startup_mode;
                    selected_startup_mode.Checked = true;
                }
            }
        }
        private void Select_startup_mode_deactive(){
            foreach (ToolStripMenuItem disabled_startup in startupToolStripMenuItem.DropDownItems){
                disabled_startup.Checked = false;
            }
        }
        private void WindowedToolStripMenuItem_Click(object sender, EventArgs e){
            if (startup_status != 0){ startup_status = 0; Startup_mode_settings("0"); Select_startup_mode_active(sender); }
        }
        private void FullScreenToolStripMenuItem_Click(object sender, EventArgs e){
            if (startup_status != 1){ startup_status = 1; Startup_mode_settings("1"); Select_startup_mode_active(sender); }
        }
        private void Startup_mode_settings(string get_startup_value){
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "StartupStatus", get_startup_value);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Startup_mode_settings()"); }
            }
        }
        // HIDING MODE
        // ======================================================================================================
        private void Select_hiding_mode_active(object target_hiding_mode){
            ToolStripMenuItem selected_hiding_mode = null;
            Select_hiding_mode_deactive();
            if (target_hiding_mode != null){
                if (selected_hiding_mode != (ToolStripMenuItem)target_hiding_mode){
                    selected_hiding_mode = (ToolStripMenuItem)target_hiding_mode;
                    selected_hiding_mode.Checked = true;
                }
            }
        }
        private void Select_hiding_mode_deactive(){
            foreach (ToolStripMenuItem disabled_hiding in hidingModeToolStripMenuItem.DropDownItems){
                disabled_hiding.Checked = false;
            }
        }
        private void HidingModeOnToolStripMenuItem_Click(object sender, EventArgs e){
            if (hiding_status != 1){ hiding_status = 1; Hiding_mode_settings("1"); Select_hiding_mode_active(sender); }
        }
        private void HidingModeOffToolStripMenuItem_Click(object sender, EventArgs e){
            if (hiding_status != 0){ hiding_status = 0; Hiding_mode_settings("0"); Select_hiding_mode_active(sender); }
        }
        private void Hiding_mode_settings(string get_hiding_value){
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "HidingStatus", get_hiding_value);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Hiding_mode_settings()"); }
            }
            // HIDING MODE CHANGE NOTIFICATION
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            DialogResult hiding_mode_change_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("HidingModeChange", "hiding_mode_change_notification"), "\n\n", "\n\n"));
            if (hiding_mode_change_message == DialogResult.Yes){ Application.Restart(); }
        }
        // DEBUG MODE
        // ======================================================================================================
        // OPEN DEBUG FOLDER
        private void DebugFolderToolStripMenuItem_Click(object sender, EventArgs e){
            try{
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir)) {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = logDir, UseShellExecute = true });
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "DebugFolderToolStripMenuItem_Click()"); }
            }
        }
        // COMPRESS ZIP ALL DEBUG FILE
        private void DebugZIPToolStripMenuItem_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                string currentLog = TSLogger.CurrentLogFile;
                var logFiles = Directory.EnumerateFiles(logDir, "*.log", SearchOption.AllDirectories).ToList();
                if (logFiles.Count == 0){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_file_not_found"));
                    return;
                }
                //
                var confirm = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_check"), "\n\n"));
                if (confirm != DialogResult.Yes)
                    return;
                //
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string zipPath = Path.Combine(desktop, $"Glow_Logs_{Dns.GetHostName()}_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.zip");
                //
                if (File.Exists(zipPath)){
                    string zpDir = Path.GetDirectoryName(zipPath);
                    string zpName = Path.GetFileNameWithoutExtension(zipPath);
                    string zpExt = Path.GetExtension(zipPath);
                    for (int i = 2; ; i++){
                        string candidate = Path.Combine(zpDir, $"{zpName}_{i}{zpExt}");
                        if (!File.Exists(candidate)){
                            zipPath = candidate;
                            break;
                        }
                    }
                }
                string basePath = logDir.EndsWith("\\") ? logDir : logDir + "\\";
                var usedEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create)){
                    foreach (var file in logFiles){
                        string entryName = file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? file.Substring(basePath.Length) : Path.GetFileName(file);
                        entryName = entryName.Replace("\\", "/");
                        if (usedEntries.Contains(entryName)){
                            string entryDir = Path.GetDirectoryName(entryName)?.Replace('\\', '/') ?? "";
                            string entryFile = Path.GetFileNameWithoutExtension(entryName);
                            string entryExt = Path.GetExtension(entryName);
                            for (int i = 2; ; i++){
                                string candidate = string.IsNullOrEmpty(entryDir) ? $"{entryFile}_{i}{entryExt}" : $"{entryDir}/{entryFile}_{i}{entryExt}";
                                if (!usedEntries.Contains(candidate)){
                                    entryName = candidate;
                                    break;
                                }
                            }
                        }
                        usedEntries.Add(entryName);
                        if (!string.IsNullOrEmpty(currentLog) && string.Equals(file, currentLog, StringComparison.OrdinalIgnoreCase)){
                            try{
                                var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                                using (var entryStream = entry.Open())
                                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                                    fs.CopyTo(entryStream);
                                }
                            }catch (Exception exFile){
                                if (debug_status) { TSErrorLog.LogException(exFile, "DebugZIP (CurrentLogStreamCopy)"); }
                            }
                            continue;
                        }
                        try{
                            zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                        }catch (IOException){
                            try{
                                var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                                using (var entryStream = entry.Open())
                                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                                    fs.CopyTo(entryStream);
                                }
                            }catch (Exception exFile){
                                if (debug_status) { TSErrorLog.LogException(exFile, "DebugZIP (FallbackStreamCopy)"); }
                            }
                        }
                    }
                }
                var openResult = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_success"), zipPath, "\n\n"));
                if (openResult == DialogResult.Yes){
                    Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{zipPath}\"", UseShellExecute = true });
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "DebugZIPToolStripMenuItem_Click()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_failed"), "\n\n", ex.Message));
            }
        }
        // DELETE ALL DEBUG FILE
        private void DebugDeleteToolStripMenuItem_Click(object sender, EventArgs e){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir)){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                var confirm = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_check"), "\n\n"));
                //
                if (confirm != DialogResult.Yes)
                    return;
                //
                string currentLog = TSLogger.CurrentLogFile;
                int deletedCount = 0;
                //
                foreach (var file in Directory.EnumerateFiles(logDir, "*.log", SearchOption.TopDirectoryOnly)){
                    if (!string.IsNullOrEmpty(currentLog) && string.Equals(file, currentLog, StringComparison.OrdinalIgnoreCase)){
                        continue;
                    }
                    try{
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                        deletedCount++;
                    }catch{ }
                }
                TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_success"), deletedCount));
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "DebugDeleteToolStripMenuItem_Click()");
                }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_failed"), "\n\n", ex.Message));
            }
        }
        private void Select_debug_mode_active(object target_debug_mode){
            ToolStripMenuItem selected_debug_mode = null;
            Select_debug_mode_deactive();
            if (target_debug_mode != null){
                if (selected_debug_mode != (ToolStripMenuItem)target_debug_mode){
                    selected_debug_mode = (ToolStripMenuItem)target_debug_mode;
                    selected_debug_mode.Checked = true;
                }
            }
        }
        private void Select_debug_mode_deactive(){
            foreach (ToolStripMenuItem disabled_debug in debugModeToolStripMenuItem.DropDownItems){
                disabled_debug.Checked = false;
            }
        }
        private void DebugOnToolStripMenuItem_Click(object sender, EventArgs e){
            if (!debug_status){ debug_status = true; TSLogger.Enable(true, Program.glow_console_debug_mode); Debug_mode_settings("1"); Select_debug_mode_active(sender); }
        }
        private void DebugOffToolStripMenuItem_Click(object sender, EventArgs e){
            if (debug_status){ debug_status = false; TSLogger.Enable(false, Program.glow_console_debug_mode); Debug_mode_settings("0"); Select_debug_mode_active(sender); }
        }
        private void Debug_mode_settings(string get_debug_value){
            try{
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "DebugMode", get_debug_value);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Debug_mode_settings()"); }
            }
        }
        // UPDATE CHECK ENGINE
        // ======================================================================================================
        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e){
            Task.Run(() => Software_update_check(1));
        }
        public async void Software_update_check(int _check_update_ui){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                SetUpdateMenuEnabled(false);
                if (!await IsNetworkAvailable()){
                    if (_check_update_ui == 1){
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_connection"), "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                    }
                    return;
                }
                using (HttpClientHandler handler = new HttpClientHandler()){
                    handler.UseProxy = false;
                    using (HttpClient httpClient = new HttpClient(handler)){
                        httpClient.Timeout = TimeSpan.FromSeconds(15);
                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue{ NoCache = true, NoStore = true, MustRevalidate = true };
                        httpClient.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
                        string versionUrl = TS_LinkSystem.github_link_lv;
                        versionUrl += (versionUrl.Contains("?") ? "&" : "?") + "_ts=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string response = await httpClient.GetStringAsync(versionUrl);
                        string firstLine = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];
                        string client_version_raw = TS_VersionParser.ParseUINormalize(Application.ProductVersion);
                        string last_version_raw = TS_VersionParser.ParseUINormalize(firstLine.Split(new[] { '=' }, 2)[1].Trim());
                        Version client_ver = Version.Parse(client_version_raw);
                        Version last_ver = Version.Parse(last_version_raw);
                        if (client_ver < last_ver){
                            DialogResult info_update = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_available"), Application.ProductName, "\n\n", client_version_raw, "\n", last_version_raw, "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                            if (info_update == DialogResult.Yes){
                                try{
                                    string updaterPath = Path.Combine(Application.StartupPath, Program.updater_exe_name);
                                    if (File.Exists(updaterPath)){
                                        string procName = Path.GetFileNameWithoutExtension(updaterPath);
                                        bool isRunning = Process.GetProcessesByName(procName).Length > 0;
                                        if (!isRunning){
                                            Process.Start(new ProcessStartInfo(updaterPath) { UseShellExecute = true, Arguments = $"-app={Application.ProductName}" });
                                        }else{
                                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_c_running"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        }
                                        Application.Exit();
                                        return;
                                    }else{
                                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_not_available"), Program.updater_exe_name), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        Process.Start(new ProcessStartInfo(TS_LinkSystem.github_link_lr) { UseShellExecute = true });
                                        Application.Exit();
                                        return;
                                    }
                                }catch (Exception ex){
                                    if (debug_status)
                                        TSErrorLog.LogException(ex, $"{Program.updater_exe_name} launch block.");
                                }
                            }
                        }else if (_check_update_ui == 1){
                            string update_msg = client_ver == last_ver ? string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_available"), Application.ProductName, "\n", client_version_raw) : string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_newer"), "\n\n", $"v{client_version_raw}");
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, update_msg, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                        }
                    }
                }
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "Software_update_check()");
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_error"), "\n\n", ex.Message), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
            }finally{
                SetUpdateMenuEnabled(true);
            }
        }
        private void SetUpdateMenuEnabled(bool enabled){
            if (InvokeRequired){
                BeginInvoke(new Action(() => checkForUpdatesToolStripMenuItem.Enabled = enabled));
            }else{
                checkForUpdatesToolStripMenuItem.Enabled = enabled;
            }
        }
        // EXPORT PAGE - SUPPORT SECTION
        // ======================================================================================================
        private void EXPORT_Donate_Click(object sender, EventArgs e){
            try{
                Process.Start(new ProcessStartInfo(TS_LinkSystem.ts_donate){ UseShellExecute = true });
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "EXPORT_Donate_Click()"); }
            }
        }
        // PRINT ENGINE
        // ======================================================================================================
        // Enumerations Print Engine
        private enum ExportSectionType{
            Os,
            Motherboard,
            Cpu,
            Ram,
            Gpu,
            Storage,
            Network,
            Usb,
            Sound,
            Battery,
            InstalledDrivers,
            InstalledServices,
            InstalledApps
        }
        private enum ExportFormat{
            Txt = 0,
            Html = 1,
            Markdown = 2
        }
        private enum RenderFormat{
            Txt,
            Html,
            Markdown
        }
        // Models Print Engine
        private sealed class SelectorState{
            public int CpuIndex { get; set; }
            public int RamIndex { get; set; }
            public int GpuIndex { get; set; }
            public int MonitorIndex { get; set; }
            public int DiskIndex { get; set; }
            public int NetworkIndex { get; set; }
            public int UsbIndex { get; set; }
            public int UsbDeviceIndex { get; set; }
            public int SoundIndex { get; set; }
        }
        private sealed class ExportReport{
            public string Title { get; set; }
            public string GeneratedAt { get; set; }
            public List<ReportSection> Sections { get; } = new List<ReportSection>();
            public List<ReportFooterItem> FooterItems { get; } = new List<ReportFooterItem>();
        }
        private sealed class ReportSection{
            public int ProgressIndex { get; set; }
            public string Key { get; set; }
            public string Title { get; set; }
            public List<IReportBlock> Blocks { get; } = new List<IReportBlock>();
        }
        private interface IReportBlock { }
        private sealed class ReportKeyValueList : IReportBlock{
            public List<ReportItem> Items { get; } = new List<ReportItem>();
        }
        private sealed class ReportGroup : IReportBlock{
            public string Title { get; set; }
            public string HtmlTitleOverride { get; set; }
            public bool HtmlTitleIsRaw { get; set; }
            public string MarkdownTitleOverride { get; set; }
            public List<ReportItem> Items { get; } = new List<ReportItem>();
        }
        private sealed class ReportSeparator : IReportBlock{
            public int Length { get; set; }
        }
        private sealed class ReportTableBlock : IReportBlock{
            public string Title { get; set; }
            public string HtmlTitlePrefix { get; set; }
            public string HtmlTitleSuffix { get; set; }
            public bool SplitTitleByColonForHtml { get; set; }
            public List<string> Rows { get; } = new List<string>();
            public string SummaryLabel { get; set; }
            public string SummaryValue { get; set; }
            public int TxtRowSeparatorLength { get; set; } = 155;
        }
        private sealed class ReportFooterItem{
            public string Label { get; set; }
            public string Value { get; set; }
            public bool IsLink { get; set; }
        }
        private sealed class ReportItem{
            public string Label { get; set; }
            public string Value { get; set; }
            public string TxtValueOverride { get; set; }
            public string HtmlValueOverride { get; set; }
            public string MarkdownValueOverride { get; set; }
            public string GetValue(RenderFormat format){
                switch (format){
                    case RenderFormat.Txt:
                        return TxtValueOverride ?? Value ?? string.Empty;
                    case RenderFormat.Html:
                        return HtmlValueOverride ?? Value ?? string.Empty;
                    case RenderFormat.Markdown:
                        return MarkdownValueOverride ?? Value ?? string.Empty;
                    default:
                        return Value ?? string.Empty;
                }
            }
        }
        private interface IReportRenderer{
            string DefaultExtension { get; }
            string BuildFilter(TSGetLangs software_lang);
            string Render(ExportReport report);
        }
        // Export Section Selection Print Engine
        private HashSet<ExportSectionType> GetSelectedExportSections(){
            HashSet<ExportSectionType> selectedSections = new HashSet<ExportSectionType>();
            if (EXPORT_CB_OS.Checked)
                selectedSections.Add(ExportSectionType.Os);
            if (EXPORT_CB_MB.Checked)
                selectedSections.Add(ExportSectionType.Motherboard);
            if (EXPORT_CB_CPU.Checked)
                selectedSections.Add(ExportSectionType.Cpu);
            if (EXPORT_CB_RAM.Checked)
                selectedSections.Add(ExportSectionType.Ram);
            if (EXPORT_CB_GPU.Checked)
                selectedSections.Add(ExportSectionType.Gpu);
            if (EXPORT_CB_STORAGE.Checked)
                selectedSections.Add(ExportSectionType.Storage);
            if (EXPORT_CB_NETWORK.Checked)
                selectedSections.Add(ExportSectionType.Network);
            if (EXPORT_CB_USB.Checked)
                selectedSections.Add(ExportSectionType.Usb);
            if (EXPORT_CB_SOUND.Checked)
                selectedSections.Add(ExportSectionType.Sound);
            if (EXPORT_CB_BATTERY.Checked)
                selectedSections.Add(ExportSectionType.Battery);
            if (EXPORT_CB_DRIVERS.Checked)
                selectedSections.Add(ExportSectionType.InstalledDrivers);
            if (EXPORT_CB_SERVICES.Checked)
                selectedSections.Add(ExportSectionType.InstalledServices);
            if (EXPORT_CB_APPS.Checked)
                selectedSections.Add(ExportSectionType.InstalledApps);
            return selectedSections;
        }
        // Export UI Print Engine
        private void ExportModsAdd(){
            try{
                if (EXPORT_Selector_List.InvokeRequired){
                    EXPORT_Selector_List.Invoke(new MethodInvoker(ExportModsAdd));
                    return;
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                EXPORT_Selector_List.Items.Clear();
                EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_txt"));
                EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_html"));
                EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_markdown"));
                EXPORT_Selector_List.SelectedIndex = 0;
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "ExportModsAdd()");
                }
            }
        }
        // Export UI Filter Print Engine
        private void ExportModsFilterAdd(){
            try{
                if (EXPORT_ModeSelector_List.InvokeRequired){
                    EXPORT_ModeSelector_List.Invoke(new MethodInvoker(ExportModsFilterAdd));
                    return;
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                EXPORT_ModeSelector_List.Items.Clear();
                EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_basic"));
                EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_advanced"));
                EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_full"));
                EXPORT_ModeSelector_List.SelectedIndex = 2;
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "ExportModsFilterAdd()");
                }
            }
        }
        // Export UI Dynamic Filter Print Engine
        private void EXPORT_ModeSelector_List_SelectedIndexChanged(object sender, EventArgs e){
            try{
                int mode = EXPORT_ModeSelector_List.SelectedIndex;
                var filterMap = new Dictionary<string, CheckBox>{
                    { "OS", EXPORT_CB_OS },
                    { "MB", EXPORT_CB_MB },
                    { "CPU", EXPORT_CB_CPU },
                    { "RAM", EXPORT_CB_RAM },
                    { "GPU", EXPORT_CB_GPU },
                    { "STORAGE", EXPORT_CB_STORAGE },
                    { "NETWORK", EXPORT_CB_NETWORK },
                    { "USB", EXPORT_CB_USB },
                    { "SOUND", EXPORT_CB_SOUND },
                    { "BATTERY", EXPORT_CB_BATTERY },
                    { "DRIVERS", EXPORT_CB_DRIVERS },
                    { "SERVICES", EXPORT_CB_SERVICES },
                    { "APPS", EXPORT_CB_APPS }
                };
                var modeMap = new Dictionary<int, string[]>{
                    { 0, new[]{ "OS", "MB", "CPU", "RAM", "GPU", "STORAGE" } },
                    { 1, new[]{ "OS", "MB", "CPU", "RAM", "GPU", "STORAGE", "NETWORK", "USB", "SOUND", "BATTERY" } },
                    { 2, filterMap.Keys.ToArray() } // ALL
                };
                var active = modeMap.ContainsKey(mode) ? modeMap[mode] : Array.Empty<string>();
                var activeSet = new HashSet<string>(active);
                foreach (var item in filterMap){
                    bool shouldCheck = activeSet.Contains(item.Key);
                    if (item.Value.Checked != shouldCheck){
                        item.Value.Checked = shouldCheck;
                    }
                }
            }catch (Exception ex){
                if (debug_status){
                    TSErrorLog.LogException(ex, "EXPORT_ModeSelector_List_SelectedIndexChanged()");
                }
            }
        }
        // Export UI Start Engine Print Engine
        private void EXPORT_StartEngineBtn_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                HashSet<ExportSectionType> selectedSections = GetSelectedExportSections();
                if (selectedSections.Count == 0){
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Export", "e_mode_select_one_min"));
                    return;
                }
                DialogResult start_print_info = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Export", "e_mode_report_start_info"), EXPORT_Selector_List.SelectedItem.ToString(), "\n\n"));
                if (start_print_info == DialogResult.Yes){
                    EXPORT_Selector_List.Enabled = false;
                    EXPORT_ProgressBGPanel.Visible = true;
                    EXPORT_ProgessLabel.Visible = true;
                    //
                    EXPORT_ModeSelector_List.Enabled = false;
                    EXPORT_CB_FLP.Enabled = false;
                    //
                    EXPORT_StartEngineBtn.Enabled = false;
                    try{
                        Print_engine_mode((ExportFormat)EXPORT_Selector_List.SelectedIndex);
                    }
                    finally{
                        EXPORT_StartEngineBtn.Enabled = true;
                    }
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "EXPORT_StartEngineBtn_Click()"); }
            }
        }
        // Export Process Print Engine
        private void Print_engine_mode(ExportFormat pe_mode){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try{
                ExportReport report = BuildExportReport();
                int totalSections = report.Sections.Count;
                int currentSection = 0;
                foreach (ReportSection section in report.Sections){
                    currentSection++;
                    Print_engine_progress_update(section.Title, currentSection, totalSections);
                }
                IReportRenderer renderer = CreateRenderer(pe_mode);
                string content = renderer.Render(report);
                EXPORT_ProgessLabel.Text = software_lang.TSReadLangs("Export", "e_mode_save_await");
                EXPORT_ProgressFEPanel.Width = CalculateProgressWidth(totalSections, totalSections);
                SaveRenderedReport(renderer, software_lang, content);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Print_engine_mode()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("PrintEngine", "pe_export_info_error"));
            }finally{
                Print_after_mode();
            }
        }
        private void Print_engine_progress_update(string sectionTitle, int status, int totalPages){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (totalPages <= 0)
                    totalPages = 1;
                EXPORT_ProgessLabel.Text = string.Format(software_lang.TSReadLangs("Export", "e_mode_process"), sectionTitle, status, totalPages);
                EXPORT_ProgressFEPanel.Width = CalculateProgressWidth(status, totalPages);
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "Print_engine_progress_update()"); }
            }
        }
        private int CalculateProgressWidth(int status, int total_pages){
            if (total_pages <= 0)
                return 0;
            return (int)((EXPORT_ProgressBGPanel.Width * status) / total_pages);
        }
        private void Print_after_mode(){
            EXPORT_Selector_List.Enabled = true;
            EXPORT_ProgressBGPanel.Visible = false;
            EXPORT_ProgressFEPanel.Width = 0;
            EXPORT_ProgessLabel.Visible = false;
            EXPORT_ModeSelector_List.Enabled = true;
            EXPORT_CB_FLP.Enabled = true;
        }
        // Rendering And Saving Print Engine
        private IReportRenderer CreateRenderer(ExportFormat format){
            switch (format){
                case ExportFormat.Txt:
                    return new TxtReportRenderer();
                case ExportFormat.Html:
                    return new HtmlReportRenderer(theme, lang, Application.CompanyName, Application.ProductName, lang_path);
                case ExportFormat.Markdown:
                    return new MarkdownReportRenderer(lang_path);
                default:
                    return new TxtReportRenderer();
            }
        }
        private void SaveRenderedReport(IReportRenderer renderer, TSGetLangs software_lang, string content){
            using (SaveFileDialog save_engine = new SaveFileDialog{
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = Application.ProductName + " - " + software_lang.TSReadLangs("PrintEngine", "pe_save_directory"),
                DefaultExt = renderer.DefaultExtension,
                FileName = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_name"), OS_ComputerName_V.Text) + " - " + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss"),
                Filter = renderer.BuildFilter(software_lang)
            }){
                if (save_engine.ShowDialog() == DialogResult.OK){
                    File.WriteAllText(save_engine.FileName, content);
                    DialogResult glow_print_engine_query = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_success") + "\n\n" + software_lang.TSReadLangs("PrintEngine", "pe_save_info_open"), Application.ProductName, save_engine.FileName));
                    if (glow_print_engine_query == DialogResult.Yes){
                        Process.Start(save_engine.FileName);
                    }
                }
            }
        }
        // Helpers Print Engine
        private SelectorState CaptureSelectorState(){
            SelectorState state = new SelectorState{
                CpuIndex = CPU_Selector_List.SelectedIndex >= 0 ? CPU_Selector_List.SelectedIndex : 0,
                RamIndex = RAM_Selector_List.SelectedIndex >= 0 ? RAM_Selector_List.SelectedIndex : 0,
                GpuIndex = GPU_Selector_List.SelectedIndex >= 0 ? GPU_Selector_List.SelectedIndex : 0,
                MonitorIndex = GPU_MonitorSelector_List.SelectedIndex >= 0 ? GPU_MonitorSelector_List.SelectedIndex : 0,
                DiskIndex = DISK_Selector_List.SelectedIndex >= 0 ? DISK_Selector_List.SelectedIndex : 0,
                NetworkIndex = NET_Selector_List.SelectedIndex >= 0 ? NET_Selector_List.SelectedIndex : 0,
                UsbIndex = USB_Selector_List.SelectedIndex >= 0 ? USB_Selector_List.SelectedIndex : 0,
                UsbDeviceIndex = USB_DeviceSelector_List.SelectedIndex >= 0 ? USB_DeviceSelector_List.SelectedIndex : 0,
                SoundIndex = SOUND_Selector_List.SelectedIndex >= 0 ? SOUND_Selector_List.SelectedIndex : 0
            };
            return state;
        }
        private void RestoreSelectorState(SelectorState state){
            CPU_Selector_List.SelectedIndex = state.CpuIndex;
            RAM_Selector_List.SelectedIndex = state.RamIndex;
            GPU_Selector_List.SelectedIndex = state.GpuIndex;
            GPU_MonitorSelector_List.SelectedIndex = state.MonitorIndex;
            DISK_Selector_List.SelectedIndex = state.DiskIndex;
            NET_Selector_List.SelectedIndex = state.NetworkIndex;
            USB_Selector_List.SelectedIndex = state.UsbIndex;
            USB_DeviceSelector_List.SelectedIndex = state.UsbDeviceIndex;
            SOUND_Selector_List.SelectedIndex = state.SoundIndex;
        }
        private static string HtmlSafe(string value){
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
        private static ReportItem KV(string label, string value){
            ReportItem item = new ReportItem { Label = label ?? string.Empty, Value = value ?? string.Empty };
            return item;
        }
        private static ReportItem KV(string label, string value, string txtOverride, string htmlOverride, string markdownOverride){
            ReportItem item = new ReportItem{
                Label = label ?? string.Empty,
                Value = value ?? string.Empty,
                TxtValueOverride = txtOverride,
                HtmlValueOverride = htmlOverride,
                MarkdownValueOverride = markdownOverride
            };
            return item;
        }
        private static ReportKeyValueList KVList(params ReportItem[] items){
            ReportKeyValueList list = new ReportKeyValueList();
            foreach (ReportItem item in items){
                list.Items.Add(item);
            }
            return list;
        }
        private static ReportGroup Group(string title, params ReportItem[] items){
            ReportGroup group = new ReportGroup{
                Title = title ?? string.Empty
            };
            foreach (ReportItem item in items){
                group.Items.Add(item);
            }
            return group;
        }
        private static ReportSeparator Separator(int length){
            ReportSeparator separator = new ReportSeparator{
                Length = length
            };
            return separator;
        }
        // Report Builder Print Engine
        private ExportReport BuildExportReport(){
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            SelectorState state = CaptureSelectorState();
            HashSet<ExportSectionType> selectedSections = GetSelectedExportSections();
            try{
                ExportReport report = new ExportReport{
                    Title = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_report_title"), OS_ComputerName_V.Text),
                    GeneratedAt = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss")
                };
                if (selectedSections.Contains(ExportSectionType.Os))
                    report.Sections.Add(BuildOsSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Motherboard))
                    report.Sections.Add(BuildMotherboardSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Cpu))
                    report.Sections.Add(BuildCpuSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Ram))
                    report.Sections.Add(BuildRamSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Gpu))
                    report.Sections.Add(BuildGpuSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Storage))
                    report.Sections.Add(BuildStorageSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Network))
                    report.Sections.Add(BuildNetworkSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Usb))
                    report.Sections.Add(BuildUsbSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Sound))
                    report.Sections.Add(BuildSoundSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.Battery))
                    report.Sections.Add(BuildBatterySection(software_lang));
                if (selectedSections.Contains(ExportSectionType.InstalledDrivers))
                    report.Sections.Add(BuildInstalledDriversSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.InstalledServices))
                    report.Sections.Add(BuildInstalledServicesSection(software_lang));
                if (selectedSections.Contains(ExportSectionType.InstalledApps))
                    report.Sections.Add(BuildInstalledAppsSection(software_lang));
                report.FooterItems.Add(new ReportFooterItem{
                    Label = Application.ProductName + " " + software_lang.TSReadLangs("PrintEngine", "pe_version"),
                    Value = TS_VersionEngine.TS_SoftwareVersion(1)
                });
                report.FooterItems.Add(new ReportFooterItem{
                    Label = string.Empty,
                    Value = TS_SoftwareCopyrightDate.ts_scd_preloader
                });
                report.FooterItems.Add(new ReportFooterItem{
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_process_time"),
                    Value = report.GeneratedAt
                });
                report.FooterItems.Add(new ReportFooterItem{
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_website"),
                    Value = TS_LinkSystem.website_link,
                    IsLink = true
                });
                report.FooterItems.Add(new ReportFooterItem{
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_github"),
                    Value = TS_LinkSystem.github_link,
                    IsLink = true
                });
                report.FooterItems.Add(new ReportFooterItem{
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_donate"),
                    Value = TS_LinkSystem.ts_donate,
                    IsLink = true
                });
                return report;
            }finally{
                RestoreSelectorState(state);
            }
        }
        // Section Builders Print Engine
        private ReportSection BuildOsSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 1,
                Key = "os",
                Title = software_lang.TSReadLangs("LeftMenu", "left_os")
            };
            section.Blocks.Add(KVList(
                KV(OS_SystemUser.Text, OS_SystemUser_V.Text),
                KV(OS_ComputerName.Text, OS_ComputerName_V.Text),
                KV(OS_SavedUser.Text, OS_SavedUser_V.Text),
                KV(OS_Name.Text, OS_Name_V.Text),
                KV(OS_Manufacturer.Text, OS_Manufacturer_V.Text),
                KV(OS_SystemVersion.Text, OS_SystemVersion_V.Text),
                KV(OS_SystemArchitectural.Text, OS_SystemArchitectural_V.Text),
                KV(OS_DeviceID.Text, OS_DeviceID_V.Text),
                KV(OS_Serial.Text, OS_Serial_V.Text),
                KV(OS_ExperienceVersion.Text, OS_ExperienceVersion_V.Text),
                KV(OS_Country.Text, OS_Country_V.Text),
                KV(OS_TimeZone.Text, OS_TimeZone_V.Text),
                KV(OS_SystemTime.Text, OS_SystemTime_V.Text),
                KV(OS_SystemWorkTime.Text, OS_SystemWorkTime_V.Text),
                KV(OS_LastBootTime.Text, OS_LastBootTime_V.Text),
                KV(OS_SystemLastShutDown.Text, OS_SystemLastShutDown_V.Text),
                KV(OS_Install.Text, OS_Install_V.Text),
                KV(OS_PrimaryOS.Text, OS_PrimaryOS_V.Text),
                KV(OS_PortableOS.Text, OS_PortableOS_V.Text),
                KV(OS_FastBoot.Text, OS_FastBoot_V.Text),
                KV(OS_AVProgram.Text, OS_AVProgram_V.Text),
                KV(OS_FirewallProgram.Text, OS_FirewallProgram_V.Text),
                KV(OS_AntiSpywareProgram.Text, OS_AntiSpywareProgram_V.Text),
                KV(OS_WinDefCoreIsolation.Text, OS_WinDefCoreIsolation_V.Text),
                KV(OS_ActivePower.Text, OS_ActivePower_V.Text),
                KV(OS_ActivePowerGUID.Text, OS_ActivePowerGUID_V.Text),
                KV(OS_ActivePowerScreenTimeOutP.Text, OS_ActivePowerScreenTimeOutP_V.Text),
                KV(OS_ActivePowerScreenTimeOutB.Text, OS_ActivePowerScreenTimeOutB_V.Text),
                KV(OS_ActivePowerSleepTimeP.Text, OS_ActivePowerSleepTimeP_V.Text),
                KV(OS_ActivePowerSleepTimeB.Text, OS_ActivePowerSleepTimeB_V.Text),
                KV(OS_MSOfficeVersion.Text, OS_MSOfficeVersion_V.Text),
                KV(OS_WinLicenseTime.Text, OS_WinLicenseTime_V.Text),
                KV(OS_WinKey.Text, OS_WinKey_V.Text),
                KV(OS_WinActiveChannel.Text, OS_WinActiveChannel_V.Text),
                KV(OS_WinLicenseURL.Text, OS_WinLicenseURL_V.Text),
                KV(OS_WinLicenseVerifiURL.Text, OS_WinLicenseVerifiURL_V.Text),
                KV(OS_NETFrameworkVersion.Text, OS_NETFrameworkVersion_V.Text),
                KV(OS_Minidump.Text, OS_Minidump_V.Text),
                KV(OS_BSODDate.Text, OS_BSODDate_V.Text),
                KV(OS_Wallpaper.Text, OS_Wallpaper_V.Text)
            ));
            return section;
        }
        private ReportSection BuildMotherboardSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 2,
                Key = "mb",
                Title = software_lang.TSReadLangs("LeftMenu", "left_mb")
            };
            section.Blocks.Add(KVList(
                KV(MB_MotherBoardName.Text, MB_MotherBoardName_V.Text),
                KV(MB_MotherBoardMan.Text, MB_MotherBoardMan_V.Text),
                KV(MB_SystemModelMan.Text, MB_SystemModelMan_V.Text),
                KV(MB_SystemModelFamily.Text, MB_SystemModelFamily_V.Text),
                KV(MB_SystemFamily.Text, MB_SystemFamily_V.Text),
                KV(MB_SystemModel.Text, MB_SystemModel_V.Text),
                KV(MB_DeviceSerialNumber.Text, MB_DeviceSerialNumber_V.Text),
                KV(MB_MotherBoardSerial.Text, MB_MotherBoardSerial_V.Text),
                KV(MB_SystemSKU.Text, MB_SystemSKU_V.Text),
                KV(MB_Chipset.Text, MB_Chipset_V.Text),
                KV(MB_BiosManufacturer.Text, MB_BiosManufacturer_V.Text),
                KV(MB_BiosDate.Text, MB_BiosDate_V.Text),
                KV(MB_BiosVersion.Text, MB_BiosVersion_V.Text),
                KV(MB_SmBiosVersion.Text, MB_SmBiosVersion_V.Text),
                KV(MB_BiosMode.Text, MB_BiosMode_V.Text),
                KV(MB_LastBIOSTime.Text, MB_LastBIOSTime_V.Text),
                KV(MB_SecureBoot.Text, MB_SecureBoot_V.Text),
                KV(MB_TPMStatus.Text, MB_TPMStatus_V.Text),
                KV(MB_TPMPPIVersion.Text, MB_TPMPPIVersion_V.Text),
                KV(MB_TPMPPIAltVersion.Text, MB_TPMPPIAltVersion_V.Text),
                KV(MB_TPMMan.Text, MB_TPMMan_V.Text),
                KV(MB_TPMManID.Text, MB_TPMManID_V.Text),
                KV(MB_TPMManVersion.Text, MB_TPMManVersion_V.Text),
                KV(MB_TPMManFullVersion.Text, MB_TPMManFullVersion_V.Text),
                KV(MB_TPMManPublisher.Text, MB_TPMManPublisher_V.Text)
            ));
            return section;
        }
        private ReportSection BuildCpuSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 3,
                Key = "cpu",
                Title = software_lang.TSReadLangs("LeftMenu", "left_cpu")
            };
            try{
                int cpu_slot = CPU_Selector_List.Items.Count;
                for (int cs = 1; cs <= cpu_slot; cs++){
                    CPU_Selector_List.SelectedIndex = cs - 1;
                    string cpuSelectedTitle = CPU_Selector_List.SelectedItem != null ? CPU_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Cpu_Content", "cpu_c_print") + " " + cpuSelectedTitle,
                        KV(CPU_Name.Text, cpuSelectedTitle),
                        KV(CPU_Manufacturer.Text, CPU_Manufacturer_V.Text),
                        KV(CPU_Architectural.Text, CPU_Architectural_V.Text),
                        KV(CPU_Speed.Text, CPU_Speed_V.Text),
                        KV(CPU_CoreCount.Text, CPU_CoreCount_V.Text),
                        KV(CPU_ActiveCoreCount.Text, CPU_ActiveCoreCount_V.Text),
                        KV(CPU_LogicalCore.Text, CPU_LogicalCore_V.Text),
                        KV(CPU_L1.Text, CPU_L1_V.Text),
                        KV(CPU_L2.Text, CPU_L2_V.Text),
                        KV(CPU_L3.Text, CPU_L3_V.Text),
                        KV(CPU_SerialName.Text, CPU_SerialName_V.Text),
                        KV(CPU_Family.Text, CPU_Family_V.Text),
                        KV(CPU_Slot.Text, CPU_Slot_V.Text),
                        KV(CPU_SocketDefinition.Text, CPU_SocketDefinition_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_print")) + "</span><span>" + HtmlSafe(cpuSelectedTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
                section.Blocks.Add(Group(
                    CPU_AllCPUHead.Text,
                    KV(CPU_Virtualization.Text, CPU_Virtualization_V.Text),
                    KV(CPU_Usage.Text, CPU_Usage_V.Text),
                    KV(CPU_Process.Text, CPU_Process_V.Text),
                    KV(CPU_Threads.Text, CPU_Threads_V.Text),
                    KV(CPU_Handles.Text, CPU_Handles_V.Text)
                ));
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildCpuSection()"); }
            }
            return section;
        }
        private ReportSection BuildRamSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 4,
                Key = "ram",
                Title = software_lang.TSReadLangs("LeftMenu", "left_ram")
            };
            section.Blocks.Add(KVList(
                KV(RAM_TotalRAM.Text, RAM_TotalRAM_V.Text),
                KV(RAM_UsageRAMCount.Text, RAM_UsageRAMCount_V.Text),
                KV(RAM_EmptyRamCount.Text, RAM_EmptyRamCount_V.Text),
                KV(RAM_TotalVirtualRam.Text, RAM_TotalVirtualRam_V.Text),
                KV(RAM_UsageVirtualRam.Text, RAM_UsageVirtualRam_V.Text),
                KV(RAM_EmptyVirtualRam.Text, RAM_EmptyVirtualRam_V.Text),
                KV(RAM_SlotStatus.Text, RAM_SlotStatus_V.Text)
            ));
            try{
                int ram_slot = RAM_Selector_List.Items.Count;
                for (int rs = 1; rs <= ram_slot; rs++){
                    RAM_Selector_List.SelectedIndex = rs - 1;
                    string title = RAM_Selector_List.SelectedItem != null ? RAM_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        title,
                        KV(RAM_Amount.Text, RAM_Amount_V.Text),
                        KV(RAM_Type.Text, RAM_Type_V.Text),
                        KV(RAM_Frequency.Text, RAM_Frequency_V.Text),
                        KV(RAM_Volt.Text, RAM_Volt_V.Text),
                        KV(RAM_FormFactor.Text, RAM_FormFactor_V.Text),
                        KV(RAM_Serial.Text, RAM_Serial_V.Text),
                        KV(RAM_Manufacturer.Text, RAM_Manufacturer_V.Text),
                        KV(RAM_BankLabel.Text, RAM_BankLabel_V.Text),
                        KV(RAM_DataWidth.Text, RAM_DataWidth_V.Text),
                        KV(RAM_BellekType.Text, RAM_BellekType_V.Text),
                        KV(RAM_PartNumber.Text, RAM_PartNumber_V.Text)
                    );
                    if (!string.IsNullOrEmpty(title) && title.Contains("-")){
                        string[] ram_explode = title.Split('-');
                        if (ram_explode.Length >= 2){
                            group.HtmlTitleOverride = HtmlSafe(ram_explode[0].Trim()) + " - </span><span>" + HtmlSafe(ram_explode[1].Trim());
                            group.HtmlTitleIsRaw = true;
                        }
                    }
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildRamSection()"); }
            }
            return section;
        }
        private ReportSection BuildGpuSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 5,
                Key = "gpu",
                Title = software_lang.TSReadLangs("LeftMenu", "left_gpu")
            };
            try{
                int gpu_amount = GPU_Selector_List.Items.Count;
                for (int gpu_render = 1; gpu_render <= gpu_amount; gpu_render++){
                    GPU_Selector_List.SelectedIndex = gpu_render - 1;
                    string gpuSelectedTitle = GPU_Selector_List.SelectedItem != null ? GPU_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Gpu_Content", "gpu_c_gpu_print") + " " + gpuSelectedTitle,
                        KV(GPU_Selector.Text, gpuSelectedTitle),
                        KV(GPU_Manufacturer.Text, GPU_Manufacturer_V.Text),
                        KV(GPU_VRAM.Text, GPU_VRAM_V.Text),
                        KV(GPU_Version.Text, GPU_Version_V.Text),
                        KV(GPU_DriverDate.Text, GPU_DriverDate_V.Text),
                        KV(GPU_Status.Text, GPU_Status_V.Text),
                        KV(GPU_DeviceID.Text, GPU_DeviceID_V.Text),
                        KV(GPU_DacType.Text, GPU_DacType_V.Text),
                        KV(GPU_GraphicDriversName.Text, GPU_GraphicDriversName_V.Text),
                        KV(GPU_DirectXAll.Text, GPU_DirectXAll_V.Text),
                        KV(GPU_DirectXMax.Text, GPU_DirectXMax_V.Text),
                        KV(GPU_InfFileName.Text, GPU_InfFileName_V.Text),
                        KV(GPU_INFSectionFile.Text, GPU_INFSectionFile_V.Text),
                        KV(GPU_CurrentColor.Text, GPU_CurrentColor_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Gpu_Content", "gpu_c_gpu_print")) +"</span><span>" + HtmlSafe(gpuSelectedTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildGpuSection()"); }
            }
            section.Blocks.Add(Separator(60));
            try{
                int screen_amount = GPU_MonitorSelector_List.Items.Count;
                for (int sa = 1; sa <= screen_amount; sa++){
                    GPU_MonitorSelector_List.SelectedIndex = sa - 1;
                    string title = GPU_MonitorSelector_List.SelectedItem != null ? GPU_MonitorSelector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        title,
                        KV(GPU_MonitorUserFriendlyName.Text, GPU_MonitorUserFriendlyName_V.Text),
                        KV(GPU_MonitorManName.Text, GPU_MonitorManName_V.Text),
                        KV(GPU_MonitorProductCodeID.Text, GPU_MonitorProductCodeID_V.Text),
                        KV(GPU_MonitorSerialNumberID.Text, GPU_MonitorSerialNumberID_V.Text),
                        KV(GPU_MonitorConType.Text, GPU_MonitorConType_V.Text),
                        KV(GPU_MonitorManfDate.Text, GPU_MonitorManfDate_V.Text),
                        KV(GPU_MonitorManfDateWeek.Text, GPU_MonitorManfDateWeek_V.Text),
                        KV(GPU_MonitorHID.Text, GPU_MonitorHID_V.Text),
                        KV(GPU_MonitorResLabel.Text, GPU_MonitorResLabel_V.Text),
                        KV(GPU_MonitorVirtualRes.Text, GPU_MonitorVirtualRes_V.Text),
                        KV(GPU_MonitorBounds.Text, GPU_MonitorBounds_V.Text),
                        KV(GPU_MonitorWorking.Text, GPU_MonitorWorking_V.Text),
                        KV(GPU_ScreenRefreshRate.Text, GPU_ScreenRefreshRate_V.Text),
                        KV(GPU_ScreenBit.Text, GPU_ScreenBit_V.Text),
                        KV(GPU_MonitorPrimary.Text, GPU_MonitorPrimary_V.Text)
                    );
                    if (!string.IsNullOrEmpty(title) && title.Contains("-")){
                        string[] gpu_monitor_explode = title.Split('-');
                        if (gpu_monitor_explode.Length >= 2){
                            group.HtmlTitleOverride = HtmlSafe(gpu_monitor_explode[0].Trim()) + " - </span><span>" + HtmlSafe(gpu_monitor_explode[1].Trim());
                            group.HtmlTitleIsRaw = true;
                        }
                    }
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildGpuSection()"); }
            }
            return section;
        }
        private ReportSection BuildStorageSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 6,
                Key = "storage",
                Title = software_lang.TSReadLangs("LeftMenu", "left_storage")
            };
            section.Blocks.Add(Group(
                DISK_TTLP_L1.Text,
                KV(DISK_TTLP_P1_L1.Text, DISK_TTLP_P1_L2.Text),
                KV(DISK_TTLP_P2_L1.Text, DISK_TTLP_P2_L2.Text),
                KV(DISK_TTLP_P3_L1.Text, DISK_TTLP_P3_L2.Text),
                KV(DISK_TTLP_P4_L1.Text, DISK_TTLP_P4_L2.Text)
            ));
            section.Blocks.Add(Separator(12));
            try{
                int disk_amount = DISK_Selector_List.Items.Count;
                for (int disk_render = 1; disk_render <= disk_amount; disk_render++){
                    DISK_Selector_List.SelectedIndex = disk_render - 1;
                    string diskSelectedTitle = DISK_Selector_List.SelectedItem != null ? DISK_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("StorageContent", "se_c_disk_print") + " " + diskSelectedTitle,
                        KV(DISK_Selector.Text, diskSelectedTitle),
                        KV(DISK_Model.Text, DISK_Model_V.Text),
                        KV(DISK_Man.Text, DISK_Man_V.Text),
                        KV(DISK_VolumeID.Text, DISK_VolumeID_V.Text),
                        KV(DISK_VolumeName.Text, DISK_VolumeName_V.Text),
                        KV(DISK_Firmware.Text, DISK_Firmware_V.Text),
                        KV(DISK_Serial.Text, DISK_Serial_V.Text),
                        KV(DISK_VolumeSerial.Text, DISK_VolumeSerial_V.Text),
                        KV(DISK_Size.Text, DISK_Size_V.Text),
                        KV(DISK_UsedSpace.Text, DISK_UsedSpace_V.Text),
                        KV(DISK_FreeSpace.Text, DISK_FreeSpace_V.Text),
                        KV(DISK_FileSystem.Text, DISK_FileSystem_V.Text),
                        KV(DISK_FormattingType.Text, DISK_FormattingType_V.Text),
                        KV(DISK_Type.Text, DISK_Type_V.Text),
                        KV(DISK_DriveType.Text, DISK_DriveType_V.Text),
                        KV(DISK_InterFace.Text, DISK_InterFace_V.Text),
                        KV(DISK_PartitionCount.Text, DISK_PartitionCount_V.Text),
                        KV(DISK_MediaLoaded.Text, DISK_MediaLoaded_V.Text),
                        KV(DISK_MediaStatus.Text, DISK_MediaStatus_V.Text),
                        KV(DISK_Health.Text, DISK_Health_V.Text),
                        KV(DISK_Boot.Text, DISK_Boot_V.Text),
                        KV(DISK_Bootable.Text, DISK_Bootable_V.Text),
                        KV(DISK_BitLockerStatus.Text, DISK_BitLockerStatus_V.Text),
                        KV(DISK_BitLockerConversionStatus.Text, DISK_BitLockerConversionStatus_V.Text),
                        KV(DISK_BitLockerEncryptMehod.Text, DISK_BitLockerEncryptMehod_V.Text),
                        KV(DISK_DriveCompressed.Text, DISK_DriveCompressed_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("StorageContent", "se_c_disk_print")) + "</span><span>" + HtmlSafe(diskSelectedTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildStorageSection()"); }
            }
            return section;
        }
        private ReportSection BuildNetworkSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 7,
                Key = "network",
                Title = software_lang.TSReadLangs("LeftMenu", "left_network")
            };
            section.Blocks.Add(KVList(
                KV(NET_LT_Device.Text, NET_LT_Device_V.Text),
                KV(NET_LT_BandWidth.Text, NET_LT_BandWidth_V.Text),
                KV(NET_LT_LocalIP.Text, NET_LT_LocalIP_V.Text),
                KV(NET_LT_GatewayIP.Text, NET_LT_GatewayIP_V.Text),
                KV(NET_LT_DL1.Text, NET_LT_DL2.Text),
                KV(NET_LT_UL1.Text, NET_LT_UL2.Text)
            ));
            try{
                int net_amount = NET_Selector_List.Items.Count;
                for (int net_render = 1; net_render <= net_amount; net_render++){
                    NET_Selector_List.SelectedIndex = net_render - 1;
                    string netSelectedTitle = NET_Selector_List.SelectedItem != null ? NET_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Network_Content", "nk_c_network_print") + " " + netSelectedTitle,
                        KV(NET_Selector.Text, netSelectedTitle),
                        KV(NET_MacAdress.Text, NET_MacAdress_V.Text),
                        KV(NET_NetMan.Text, NET_NetMan_V.Text),
                        KV(
                            NET_DriverVersion.Text,
                            NET_DriverVersion_V.Text,
                            NET_DriverVersion.Text,
                            NET_DriverVersion_V.Text,
                            NET_DriverVersion_V.Text
                        ),
                        KV(NET_DriverDate.Text, NET_DriverDate_V.Text),
                        KV(NET_ServiceName.Text, NET_ServiceName_V.Text),
                        KV(NET_AdapterType.Text, NET_AdapterType_V.Text),
                        KV(NET_Physical.Text, NET_Physical_V.Text),
                        KV(NET_DeviceID.Text, NET_DeviceID_V.Text),
                        KV(NET_Guid.Text, NET_Guid_V.Text),
                        KV(NET_ConnectionType.Text, NET_ConnectionType_V.Text),
                        KV(NET_Dhcp_status.Text, NET_Dhcp_status_V.Text),
                        KV(NET_Dhcp_server.Text, NET_Dhcp_server_V.Text),
                        KV(NET_DHCPFirstIpTime.Text, NET_DHCPFirstIpTime_V.Text),
                        KV(NET_DHCPLastIpTime.Text, NET_DHCPLastIpTime_V.Text),
                        KV(NET_LocalConSpeed.Text, NET_LocalConSpeed_V.Text),
                        KV(NET_IPv4Adress.Text, NET_IPv4Adress_V.Text),
                        KV(NET_IPv6Adress.Text, NET_IPv6Adress_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Network_Content", "nk_c_network_print")) + "</span><span>" + HtmlSafe(netSelectedTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildNetworkSection()"); }
            }
            section.Blocks.Add(KVList(
                KV(NET_P_IP_Adress.Text, NET_P_IP_Adress_V.Text),
                KV(NET_P_ISP.Text, NET_P_ISP_V.Text),
                KV(NET_DNS_v4_1.Text, NET_DNS_v4_1_V.Text),
                KV(NET_DNS_v4_2.Text, NET_DNS_v4_2_V.Text),
                KV(NET_DNS_v6_1.Text, NET_DNS_v6_1_V.Text),
                KV(NET_DNS_v6_2.Text, NET_DNS_v6_2_V.Text)
            ));
            return section;
        }
        private ReportSection BuildUsbSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 8,
                Key = "usb",
                Title = software_lang.TSReadLangs("LeftMenu", "left_usb")
            };
            try{
                int usb_con_amount = USB_Selector_List.Items.Count;
                for (int usb_con_render = 1; usb_con_render <= usb_con_amount; usb_con_render++){
                    USB_Selector_List.SelectedIndex = usb_con_render - 1;
                    string usbControllerTitle = USB_Selector_List.SelectedItem != null ? USB_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Usb_Content", "usb_c_controller_print") + " " + usbControllerTitle,
                        KV(USB_Selector.Text, usbControllerTitle),
                        KV(USB_ConName.Text, USB_ConName_V.Text),
                        KV(USB_ConMan.Text, USB_ConMan_V.Text),
                        KV(USB_ConDeviceID.Text, USB_ConDeviceID_V.Text),
                        KV(USB_ConPNPDeviceID.Text, USB_ConPNPDeviceID_V.Text),
                        KV(USB_ConDeviceStatus.Text, USB_ConDeviceStatus_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_controller_print")) + "</span><span>" + HtmlSafe(usbControllerTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildUsbSection()"); }
            }
            section.Blocks.Add(Separator(40));
            try{
                int usb_amount = USB_DeviceSelector_List.Items.Count;
                for (int usb_render = 1; usb_render <= usb_amount; usb_render++){
                    USB_DeviceSelector_List.SelectedIndex = usb_render - 1;
                    string usbDeviceTitle = USB_DeviceSelector_List.SelectedItem != null ? USB_DeviceSelector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Usb_Content", "usb_c_device_print") + " " + usbDeviceTitle,
                        KV(USB_DeviceSelector.Text, usbDeviceTitle),
                        KV(USB_DeviceName.Text, USB_DeviceName_V.Text),
                        KV(USB_DeviceMan.Text, USB_DeviceMan_V.Text),
                        KV(USB_DriverVersion.Text, USB_DriverVersion_V.Text),
                        KV(USB_DriverDate.Text, USB_DriverDate_V.Text),
                        KV(USB_InfFile.Text, USB_InfFile_V.Text),
                        KV(USB_DeviceID.Text, USB_DeviceID_V.Text),
                        KV(USB_HardwareID.Text, USB_HardwareID_V.Text),
                        KV(USB_DeviceGUID.Text, USB_DeviceGUID_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_device_print")) + "</span><span>" + HtmlSafe(usbDeviceTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildUsbSection()"); }
            }
            return section;
        }
        private ReportSection BuildSoundSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 9,
                Key = "sound",
                Title = software_lang.TSReadLangs("LeftMenu", "left_sound")
            };
            try{
                int sound_amount = SOUND_Selector_List.Items.Count;
                for (int sound_render = 1; sound_render <= sound_amount; sound_render++){
                    SOUND_Selector_List.SelectedIndex = sound_render - 1;
                    string soundSelectedTitle = SOUND_Selector_List.SelectedItem != null ? SOUND_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Sound_Content", "sound_c_print") + " " + soundSelectedTitle,
                        KV(SOUND_Selector.Text, soundSelectedTitle),
                        KV(SOUND_DeviceName.Text, SOUND_DeviceName_V.Text),
                        KV(SOUND_DeviceManufacturer.Text, SOUND_DeviceManufacturer_V.Text),
                        KV(SOUND_DriverVersion.Text, SOUND_DriverVersion_V.Text),
                        KV(SOUND_DriverDate.Text, SOUND_DriverDate_V.Text),
                        KV(SOUND_DeviceID.Text, SOUND_DeviceID_V.Text),
                        KV(SOUND_PNPDeviceID.Text, SOUND_PNPDeviceID_V.Text),
                        KV(SOUND_DeviceStatus.Text, SOUND_DeviceStatus_V.Text)
                    );
                    group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Sound_Content", "sound_c_print")) + "</span><span>" + HtmlSafe(soundSelectedTitle);
                    group.HtmlTitleIsRaw = true;
                    section.Blocks.Add(group);
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildSoundSection()"); }
            }
            return section;
        }
        private ReportSection BuildBatterySection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 10,
                Key = "battery",
                Title = software_lang.TSReadLangs("LeftMenu", "left_battery")
            };
            PowerStatus power_status = SystemInformation.PowerStatus;
            string battery_charging = power_status.BatteryChargeStatus.ToString();
            if (battery_charging == "NoSystemBattery"){
                section.Blocks.Add(KVList(
                    KV(BATTERY_Status.Text, BATTERY_Status_V.Text)
                ));
            }else{
                section.Blocks.Add(KVList(
                    KV(BATTERY_Status.Text, BATTERY_Status_V.Text),
                    KV(BATTERY_Model.Text, BATTERY_Model_V.Text),
                    KV(BATTERY_Serial.Text, BATTERY_Serial_V.Text),
                    KV(BATTERY_Chemistry.Text, BATTERY_Chemistry_V.Text),
                    KV(BATTERY_DesignCapacity.Text, BATTERY_DesignCapacity_V.Text),
                    KV(BATTERY_FullChargeCapacity.Text, BATTERY_FullChargeCapacity_V.Text),
                    KV(BATTERY_RemainingChargeCapacity.Text, BATTERY_RemainingChargeCapacity_V.Text),
                    KV(BATTERY_Voltage.Text, BATTERY_Voltage_V.Text),
                    KV(BATTERY_ChargePower.Text, BATTERY_ChargePower_V.Text),
                    KV(BATTERY_ChargeCurrent.Text, BATTERY_ChargeCurrent_V.Text),
                    KV(BATTERY_DeChargePower.Text, BATTERY_DeChargePower_V.Text),
                    KV(BATTERY_DeChargeCurrent.Text, BATTERY_DeChargeCurrent_V.Text)
                ));
            }
            return section;
        }
        private ReportSection BuildInstalledDriversSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 11,
                Key = "installed_drivers",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_drivers")
            };
            ReportTableBlock table = new ReportTableBlock{
                Title = software_lang.TSReadLangs("PrintEngine", "pe_driver_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Osd_Content", "osd_total_installed_driver_count"),
                SummaryValue = OSD_TYSS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try{
                foreach (DataGridViewRow row in OSD_DataMainTable.Rows){
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledDriversSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        private ReportSection BuildInstalledServicesSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 12,
                Key = "installed_services",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_services")
            };
            ReportTableBlock table = new ReportTableBlock{
                Title = software_lang.TSReadLangs("PrintEngine", "pe_service_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Services_Content", "ss_total_installed_service_count"),
                SummaryValue = SERVICE_TYS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try{
                foreach (DataGridViewRow row in SERVICE_DataMainTable.Rows){
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledServicesSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        private ReportSection BuildInstalledAppsSection(TSGetLangs software_lang){
            ReportSection section = new ReportSection{
                ProgressIndex = 13,
                Key = "installed_apps",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_apps")
            };
            ReportTableBlock table = new ReportTableBlock{
                Title = software_lang.TSReadLangs("PrintEngine", "pe_instapps_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Instapps_Content", "ia_total_installed_apps_count"),
                SummaryValue = INSTAPPS_TYUS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try{
                foreach (DataGridViewRow row in INSTAPPS_DataMainTable.Rows){
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Skip(1).Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledAppsSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        // Report Renderers Print Engine
        private sealed class TxtReportRenderer : IReportRenderer{
            public string DefaultExtension{
                get { return "txt"; }
            }
            public string BuildFilter(TSGetLangs software_lang){
                return software_lang.TSReadLangs("PrintEngine", "pe_save_txt") + " (*.txt)|*.txt";
            }
            public string Render(ExportReport report){
                List<string> lines = new List<string> { "<" + new string('-', 13) + " " + report.Title.ToUpper() + " " + new string('-', 13) + ">", string.Empty, new string('-', 60), string.Empty };
                foreach (ReportSection section in report.Sections){
                    lines.Add("<" + new string('-', 7) + " " + (section.Title ?? string.Empty) + " " + new string('-', 7) + ">");
                    lines.Add(string.Empty);
                    foreach (IReportBlock block in section.Blocks){
                        if (block is ReportKeyValueList kvList){
                            foreach (ReportItem item in kvList.Items){
                                lines.Add((item.Label ?? string.Empty) + " " + item.GetValue(RenderFormat.Txt));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportGroup group){
                            lines.Add(group.Title ?? string.Empty);
                            lines.Add(string.Empty);
                            foreach (ReportItem item in group.Items){
                                lines.Add((item.Label ?? string.Empty) + " " + item.GetValue(RenderFormat.Txt));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportSeparator separator){
                            lines.Add(new string('-', separator.Length));
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportTableBlock table){
                            lines.Add(table.Title ?? string.Empty);
                            lines.Add(string.Empty);
                            foreach (string row in table.Rows){
                                lines.Add(row);
                                lines.Add(new string('-', table.TxtRowSeparatorLength));
                            }
                            lines.Add(string.Empty);
                            lines.Add((table.SummaryLabel ?? string.Empty) + " " + (table.SummaryValue ?? string.Empty));
                            lines.Add(string.Empty);
                            continue;
                        }
                    }
                    if (lines.Count > 0 && lines[lines.Count - 1] != string.Empty){
                        lines.Add(string.Empty);
                    }
                    lines.Add(new string('-', 60));
                    lines.Add(string.Empty);
                }
                foreach (ReportFooterItem item in report.FooterItems){
                    if (string.IsNullOrWhiteSpace(item.Label)){
                        lines.Add(item.Value ?? string.Empty);
                    }else{
                        lines.Add(item.Label + " " + (item.Value ?? string.Empty));
                    }
                }
                return string.Join(Environment.NewLine, lines);
            }
        }
        private sealed class HtmlReportRenderer : IReportRenderer{
            private readonly int _theme;
            private readonly string _lang;
            private readonly string _companyName;
            private readonly string _productName;
            private readonly string _langPath;
            public HtmlReportRenderer(int theme, string lang, string companyName, string productName, string langPath){
                _theme = theme;
                _lang = lang;
                _companyName = companyName;
                _productName = productName;
                _langPath = langPath;
            }
            public string DefaultExtension{
                get { return "html"; }
            }
            public string BuildFilter(TSGetLangs software_lang){
                return software_lang.TSReadLangs("PrintEngine", "pe_save_html") + " (*.html)|*.html";
            }
            public string Render(ExportReport report){
                TSGetLangs software_lang = new TSGetLangs(_langPath);
                Color html_body_bg_color = TS_ThemeEngine.ColorMode(_theme, "ContentPanelBGColor");
                string html_bbgc = string.Format("#{0}{1}{2}", html_body_bg_color.R.ToString("X2"), html_body_bg_color.G.ToString("X2"), html_body_bg_color.B.ToString("X2"));
                Color html_middle_bg_color = TS_ThemeEngine.ColorMode(_theme, "PageContainerBGAndPageContentTotalColors");
                string html_mbgc = string.Format("#{0}{1}{2}", html_middle_bg_color.R.ToString("X2"), html_middle_bg_color.G.ToString("X2"), html_middle_bg_color.B.ToString("X2"));
                Color html_ui_fe_color = TS_ThemeEngine.ColorMode(_theme, "AccentColor");
                string html_uifc = string.Format("#{0}{1}{2}", html_ui_fe_color.R.ToString("X2"), html_ui_fe_color.G.ToString("X2"), html_ui_fe_color.B.ToString("X2"));
                Color html_ui_fe_hover_color = TS_ThemeEngine.ColorMode(_theme, "AccentColorHover");
                string html_uifhc = string.Format("#{0}{1}{2}", html_ui_fe_hover_color.R.ToString("X2"), html_ui_fe_hover_color.G.ToString("X2"), html_ui_fe_hover_color.B.ToString("X2"));
                string print_html_font_url =        @"https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300..800;1,300..800&display=swap";
                string print_html_glow_logo_url =   @"https://raw.githubusercontent.com/turkaysoft/glow/main/Glow/glow_images/glow_logo/GlowLogo.ico";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html lang='" + H(_lang) + "'>");
                sb.AppendLine("<head>");
                sb.AppendLine("\t<meta charset='utf-8'>");
                sb.AppendLine("\t<meta name='author' content='" + H(_companyName) + "'>");
                sb.AppendLine("\t<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                sb.AppendLine("\t<title>" + H(report.Title) + "</title>");
                sb.AppendLine("\t<style>");
                sb.AppendLine("\t\t@import url('" + print_html_font_url + "');");
                sb.AppendLine("\t\t*{ font-family: 'Open Sans', sans-serif; margin: 0; padding: 0; box-sizing: border-box; text-decoration: none; }");
                if (_theme == 1) { sb.AppendLine("\t\t:root { color-scheme: light; }"); }
                else if (_theme == 0) { sb.AppendLine("\t\t:root { color-scheme: dark; }"); }
                sb.AppendLine("\t\ta:visited{ color: " + html_uifhc + "; }");
                sb.AppendLine("\t\t.ts_scroll_top{ background-color: " + html_uifc + "; position: fixed; bottom: -100px; right: 15px; width: 45px; height: 45px; justify-content: center; align-items: center; display: flex; border-radius: 50%; cursor: pointer; z-index: 999; transition: 0.2s; }");
                sb.AppendLine("\t\t.ts_scroll_top > .ts_arrow_up { border-left: 5px solid " + html_mbgc + "; border-top: 5px solid " + html_mbgc + "; width: 16px; height: 16px; transform: rotate(45deg); border-radius: 2px; margin-top: 4px; }");
                sb.AppendLine("\t\t.ts_scroll_top:hover{ background-color: " + html_uifhc + "; }");
                sb.AppendLine("\t\t.ts_scroll_top.active{ bottom: 15px; }");
                sb.AppendLine("\t\t@media (max-width: 700px){ .ts_scroll_top{ bottom: -100px; right: 10px; } .ts_scroll_top.active{ bottom: 10px; } }");
                sb.AppendLine("\t\t.ts_page_wrapper{ background-color:" + html_mbgc + "; font-weight: 500; position:fixed; left: 15px; bottom: 15px; width: auto; height: 35px; padding: 0 5px; font-size: 16px; margin: 0; outline: none; border-radius: 5px; border: 1px solid " + html_uifc + "; transition: 0.2s; -webkit-box-shadow: 0px 0px 13px -3px rgba(0,0,0,0.7); -moz-box-shadow: 0px 0px 13px -3px rgba(0,0,0,0.7); box-shadow: 0px 0px 13px -3px rgba(0,0,0,0.7); }");
                sb.AppendLine("\t\t@media (max-width: 700px){ .ts_page_wrapper{ left: 10px; bottom: 10px; height: 30px; border-radius: 3px; } }");
                sb.AppendLine("\t\tbody{ background-color: " + html_bbgc + "; padding: 5px 10px; justify-content: center; align-items: center; display: flex; }");
                sb.AppendLine("\t\t#main_container{ width: 100%; height: auto; justify-content: center; align-items: center; display: flex; flex-direction: column; }");
                sb.AppendLine("\t\t#main_container > h2{ margin: 25px 0; font-weight: 500; color: " + html_uifc + "; }");
                sb.AppendLine("\t\t#main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ background-color: " + html_mbgc + "; width: 1250px; height: auto; border-radius: 10px; margin: 5px 0; padding: 15px; box-sizing: border-box; display: inline-block; word-break: break-word; table-layout: fixed; }");
                sb.AppendLine("\t\t#main_container > .b8 > h3{ margin: 0 0 15px 0; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1){ justify-content: start; align-items: center; display: flex; gap: 25px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > img{ width: 75px; height: 75px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text{ justify-content: center; align-items: start; display: flex; flex-direction: column; gap: 5px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > h2{ color: " + html_uifc + "; font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > span{ font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > span > label{ color: " + html_uifc + "; font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h2{ color: " + html_uifc + "; font-weight: 500; text-align: center; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h3{ color: " + html_uifc + "; font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h4{ margin: 13px 0 0 11px; font-weight: 600; justify-content: start; align-items: center; display: flex; gap: 5px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h4 > span:nth-child(2){ color: " + html_uifc + "; font-weight: 600; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul{ margin: 15px 0 0 30px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li{ margin: 5px 0; font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > span{ margin: 0 7px 0 0; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > span:nth-child(2){ color: " + html_uifc + "; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > a{ color: " + html_uifc + "; text-decoration: underline; transition: 0.3s; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > a:hover{ color: " + html_uifhc + "; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > hr{ height: 1px; background-color: " + html_uifc + "; border: none; margin: 20px 0; padding: 0; }");
                sb.AppendLine("\t\t#main_container > .b15 > ul > li > span{ color: " + html_uifc + "; }");
                sb.AppendLine("\t\t@media (max-width: 1260px){ #main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ width: 100%; } }");
                sb.AppendLine("\t\t@media (max-width: 735px){ #main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ padding: 10px; } #main_container > .ts_box_wrapper > h3{ text-align: center; } }");
                sb.AppendLine("\t\t@media (max-width: 495px){ #main_container > .ts_box_wrapper:nth-child(1){ flex-direction: column; justify-content: center; gap: 10px; } #main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text{ text-align: center; align-items: center; } #main_container > .ts_box_wrapper > ul{ margin: 15px 0 0 25px; } #main_container > .ts_box_wrapper > h4{ margin: 13px 0 0 6px; } }");
                sb.AppendLine("\t</style>");
                sb.AppendLine("\t<link rel='icon' type='image/x-icon' href='" + print_html_glow_logo_url + "'>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                sb.AppendLine("\t<div class='ts_scroll_top' onclick='ts_scroll_to_up();' title='" + H(software_lang.TSReadLangs("PrintEngine", "pe_scroll_top_select")) + "'><span class='ts_arrow_up'></span></div>");
                sb.AppendLine("\t<select title='" + H(software_lang.TSReadLangs("PrintEngine", "pe_html_partition_select")) + "' class='ts_page_wrapper' id='ts_session_select'>");
                sb.AppendLine("\t\t<option value='b1'>" + H(software_lang.TSReadLangs("PrintEngine", "pe_html_header_select")) + "</option>");
                for (int i = 0; i < report.Sections.Count; i++){
                    sb.AppendLine("\t\t<option value='b" + (i + 2).ToString() + "'>" + H(report.Sections[i].Title) + "</option>");
                }
                sb.AppendLine("\t\t<option value='b" + (report.Sections.Count + 2).ToString() + "'>" + H(software_lang.TSReadLangs("PrintEngine", "pe_html_footer_select")) + "</option>");
                sb.AppendLine("\t</select>");
                sb.AppendLine("\t<div id='main_container'>");
                sb.AppendLine("\t\t<div class='b1 ts_box_wrapper'>");
                sb.AppendLine("\t\t\t<img src='" + print_html_glow_logo_url + "' alt='" + H(string.Format(software_lang.TSReadLangs("PrintEngine", "pe_logo"), _productName)) + "'>");
                sb.AppendLine("\t\t\t<div class='ts_box_text'>");
                sb.AppendLine("\t\t\t\t<h2>" + H(report.Title.ToUpper()) + "</h2>");
                sb.AppendLine("\t\t\t\t<span>" + H(software_lang.TSReadLangs("PrintEngine", "pe_process_time")) + " <label>" + H(report.GeneratedAt) + "</label></span>");
                sb.AppendLine("\t\t\t</div>");
                sb.AppendLine("\t\t</div>");
                for (int sectionIndex = 0; sectionIndex < report.Sections.Count; sectionIndex++){
                    ReportSection section = report.Sections[sectionIndex];
                    string boxClass = "b" + (sectionIndex + 2).ToString();
                    sb.AppendLine("\t\t<div class='" + boxClass + " ts_box_wrapper'>");
                    sb.AppendLine("\t\t\t<h3>" + H(section.Title) + "</h3>");
                    foreach (IReportBlock block in section.Blocks){
                        if (block is ReportKeyValueList kvList){
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (ReportItem item in kvList.Items){
                                sb.AppendLine("\t\t\t\t<li><span>" + H(item.Label) + "</span><span>" + H(item.GetValue(RenderFormat.Html)) + "</span></li>");
                            }
                            sb.AppendLine("\t\t\t</ul>");
                            continue;
                        }
                        if (block is ReportGroup group){
                            string groupTitle = group.HtmlTitleOverride ?? group.Title ?? string.Empty;
                            if (group.HtmlTitleIsRaw){
                                sb.AppendLine("\t\t\t\t<h4><span>" + groupTitle + "</span></h4>");
                            }else{
                                sb.AppendLine("\t\t\t\t<h4><span>" + H(groupTitle) + "</span></h4>");
                            }
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (ReportItem item in group.Items){
                                sb.AppendLine("\t\t\t\t<li><span>" + H(item.Label) + "</span><span>" + H(item.GetValue(RenderFormat.Html)) + "</span></li>");
                            }
                            sb.AppendLine("\t\t\t</ul>");
                            continue;
                        }
                        if (block is ReportSeparator separator){
                            sb.AppendLine("\t\t\t<hr>");
                            continue;
                        }
                        if (block is ReportTableBlock table){
                            if (table.SplitTitleByColonForHtml){
                                string[] parts = (table.Title ?? string.Empty).Split(':');
                                string left = parts.Length > 0 ? parts[0] + ":" : string.Empty;
                                string right = parts.Length > 1 ? parts[1] : string.Empty;
                                sb.AppendLine("\t\t\t<h4><span>" + H(left) + "</span><span>" + H(right) + "</span></h4>");
                            }else{
                                sb.AppendLine("\t\t\t<h4><span>" + H(table.Title) + "</span></h4>");
                            }
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (string row in table.Rows){
                                sb.AppendLine("\t\t\t\t<li>" + H(row) + "</li>");
                            }
                            sb.AppendLine("\t\t\t</ul>");
                            sb.AppendLine("\t\t\t<h4><span>" + H(table.SummaryLabel) + "</span><span>" + H(table.SummaryValue) + "</span></h4>");
                            continue;
                        }
                    }
                    sb.AppendLine("\t\t</div>");
                }
                string footerBoxClass = "b" + (report.Sections.Count + 2).ToString();
                sb.AppendLine("\t\t<div class='" + footerBoxClass + " ts_box_wrapper'>");
                sb.AppendLine("\t\t\t<h3>" + H(string.Format(software_lang.TSReadLangs("PrintEngine", "pe_html_footer"), _productName.ToUpper())) + "</h3>");
                sb.AppendLine("\t\t\t<ul>");
                foreach (ReportFooterItem item in report.FooterItems){
                    if (string.IsNullOrWhiteSpace(item.Label)){
                        sb.AppendLine("\t\t\t\t<li>" + H(item.Value) + "</li>");
                    }else if (item.IsLink){
                        string safeValue = H(item.Value);
                        sb.AppendLine("\t\t\t\t<li>" + H(item.Label) + " <a target='_blank' href='" + safeValue + "'>" + safeValue + "</a></li>");
                    }else{
                        sb.AppendLine("\t\t\t\t<li>" + H(item.Label) + " <span>" + H(item.Value) + "</span></li>");
                    }
                }
                sb.AppendLine("\t\t\t</ul>");
                sb.AppendLine("\t\t</div>");
                sb.AppendLine("\t</div>");
                sb.AppendLine("<script>");
                sb.AppendLine("\tdocument.querySelector('select').addEventListener('change', function(){ var ts_d_id = this.value; var targetElement = document.querySelector('.' + ts_d_id); window.scrollTo({ top: targetElement.offsetTop, behavior: 'smooth' }); });");
                sb.AppendLine("\tvar ts_output = []; document.querySelectorAll('select option').forEach(function(option){ ts_output.push(option.value); });");
                sb.AppendLine("\twindow.addEventListener('scroll', function(){ if (!document.documentElement.classList.contains('animated')){ var ts_filtered = ts_output.filter(function(ts){ var targetElement = document.querySelector('.' + ts); return window.scrollY >= targetElement.offsetTop; }); if (ts_filtered.length > 0){ document.querySelector('select').value = ts_filtered[ts_filtered.length - 1]; } } });");
                sb.AppendLine("\tconst ts_scroll_top = document.querySelector('.ts_scroll_top'); window.addEventListener('scroll', function(){ ts_scroll_top.classList.toggle('active', window.scrollY > 350); });");
                sb.AppendLine("\tfunction ts_scroll_to_up(){ window.scrollTo({ top: 0, behavior: 'smooth' }); }");
                sb.AppendLine("</script>");
                sb.AppendLine("</body>");
                sb.Append("</html>");
                return sb.ToString();
            }
            private static string H(string value){
                return WebUtility.HtmlEncode(value ?? string.Empty);
            }
        }
        private sealed class MarkdownReportRenderer : IReportRenderer{
            private readonly string _langPath;
            public MarkdownReportRenderer(string langPath){
                _langPath = langPath;
            }
            public string DefaultExtension{
                get { return "md"; }
            }
            public string BuildFilter(TSGetLangs software_lang){
                return software_lang.TSReadLangs("PrintEngine", "pe_save_markdown") + " (*.md)|*.md";
            }
            public string Render(ExportReport report){
                TSGetLangs software_lang = new TSGetLangs(_langPath);
                List<string> lines = new List<string> { "# " + Md(report.Title), string.Empty, "**" + Md(software_lang.TSReadLangs("PrintEngine", "pe_process_time")) + "** " + Md(report.GeneratedAt), string.Empty };
                foreach (ReportSection section in report.Sections){
                    lines.Add("## " + Md(section.Title));
                    lines.Add(string.Empty);
                    foreach (IReportBlock block in section.Blocks){
                        if (block is ReportKeyValueList kvList){
                            foreach (ReportItem item in kvList.Items){
                                lines.Add("- **" + Md(item.Label) + "** " + Md(item.GetValue(RenderFormat.Markdown)));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportGroup group){
                            string groupTitle = group.MarkdownTitleOverride ?? group.Title ?? string.Empty;
                            lines.Add("### " + Md(groupTitle));
                            lines.Add(string.Empty);
                            foreach (ReportItem item in group.Items){
                                lines.Add("- **" + Md(item.Label) + "** " + Md(item.GetValue(RenderFormat.Markdown)));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportSeparator){
                            lines.Add("---");
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportTableBlock table){
                            lines.Add("### " + Md(table.Title));
                            lines.Add(string.Empty);
                            foreach (string row in table.Rows){
                                lines.Add("- " + Md(row));
                            }
                            lines.Add(string.Empty);
                            lines.Add("**" + Md(table.SummaryLabel) + "** " + Md(table.SummaryValue));
                            lines.Add(string.Empty);
                            continue;
                        }
                    }
                }
                lines.Add("## " + Md(software_lang.TSReadLangs("PrintEngine", "pe_html_footer_select")));
                lines.Add(string.Empty);
                foreach (ReportFooterItem item in report.FooterItems){
                    if (string.IsNullOrWhiteSpace(item.Label)){
                        lines.Add("- " + Md(item.Value));
                    }else if (item.IsLink){
                        lines.Add("- **" + Md(item.Label) + "** [" + Md(item.Value) + "](" + (item.Value ?? string.Empty) + ")");
                    }else{
                        lines.Add("- **" + Md(item.Label) + "** " + Md(item.Value));
                    }
                }
                return string.Join(Environment.NewLine, lines);
            }
            private static string Md(string value){
                return (value ?? string.Empty).Replace("\r", "").Replace("\n", "  \n");
            }
        }
        // TS TOOL LAUNCHER MODULE
        // ======================================================================================================
        private void TSToolLauncher<T>(string formName, string langKey) where T : Form, new(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                T tool = new T{ Name = formName };
                if (Application.OpenForms[formName] == null){
                    tool.Show();
                }else{
                    if (Application.OpenForms[formName].WindowState == FormWindowState.Minimized){
                        Application.OpenForms[formName].WindowState = FormWindowState.Normal;
                    }
                    string public_message;
                    public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("HeaderTools", langKey));
                    if (formName == "glow_about"){
                        public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("HeaderMenu", langKey));
                    }else if (formName == "glow_wallpaper_preview_tool"){
                        public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("WallpaperPreviewTool", langKey));
                    }
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, public_message);
                    Application.OpenForms[formName].Activate();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "TSToolLauncher()"); }
            }
        }
        // SFC AND DISM AUTO TOOL
        // ======================================================================================================
        private void SFCandDISMAutoTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowSFCandDISMAutoTool>("glow_sfc_and_dism_tool", "ht_dism_and_sfc_tool");
        }
        // CPU BENCH TOOL
        // ======================================================================================================
        private void BenchCPUTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowBenchCPUTool>("glow_bench_cpu_tool", "ht_bench_cpu");
        }
        // RAM BENCH TOOL
        // ======================================================================================================
        private void BenchRAMTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowBenchMemoryTool>("glow_bench_ram_tool", "ht_bench_ram");
        }
        // DISK BENCH TOOL
        // ======================================================================================================
        private void BenchDiskTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowBenchDiskTool>("glow_bench_disk_tool", "ht_bench_disk");
        }
        // CACHE CLEANUP TOOL
        // ======================================================================================================
        private void CacheCleaningTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowCacheCleanupTool>("glow_cache_cleanup_tool", "ht_cache_cleanup_tool");
        }
        // SYSTEM ID ANALYSIS TOOL
        // ======================================================================================================
        private void SystemIdAnalysisTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowSystemIDAnalysisTool>("glow_system_id_analysis_tool", "ht_system_id_analysis_tool");
        }
        // SCREEN OVERLAY
        // ======================================================================================================
        private void ScreenOverlayTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowOverlayTool>("glow_screen_overlay_tool", "ht_overlay");
        }
        // BLUETOOTH FINDER TOOL
        // ======================================================================================================
        private void BluetoothFinderToolToolStripMenuItem_Click(object sender, EventArgs e){
            TSToolLauncher<GlowBluetoothFinderTool>("glow_bluetooth_finder_tool", "ht_bluetooth_finder_tool");
        }
        // DNS TEST TOOL
        // ======================================================================================================
        private void DnsTestTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowDNSTestTool>("glow_dns_test_tool", "ht_dns_test_tool");
        }
        // NETWORK FIX TOOL
        // ======================================================================================================
        private void NetworkFixTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowNetworkFixTool>("glow_network_fix_tool", "ht_network_fix_tool");
        }
        // SHOW WIFI PASSWORD TOOL
        // ======================================================================================================
        private void ShowWiFiPasswordTool_Click(object sender, EventArgs e){
            TSToolLauncher<GlowShowWiFiPasswordTool>("glow_show_wifi_password_tool", "ht_show_wifi_password_tool");
        }
        // MONITOR TEST TOOL
        // ======================================================================================================
        private void MonitorDeadPixelTestTool_Click(object sender, EventArgs e){
            monitor_engine_mode = 0;
            MonitorStartEnginePending();
        }
        private void MonitorDynamicRangeTestTool_Click(object sender, EventArgs e){
            monitor_engine_mode = 1;
            MonitorStartEnginePending();
        }
        private void MonitorStartEnginePending(){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (monitor_engine_mode == 0){
                    var warningMessage = string.Format(software_lang.TSReadLangs("MonitorTestTool", "mtt_epilepsy_warning"), "\n\n", software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dead_pixel"), "\n\n");
                    DialogResult mse_warning = TS_MessageBoxEngine.TS_MessageBox(this, 6, warningMessage);
                    if (mse_warning == DialogResult.Yes)
                        MonitorStartEngineSelect();
                }else{
                    MonitorStartEngineSelect();
                }
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "MonitorStartEnginePending()"); }
            }
        }
        private void MonitorStartEngineSelect(){
            string glow_tool_name = monitor_engine_mode == 0 ? "glow_monitor_test_engine_dead_pixel" : "glow_monitor_test_engine_dynamic_range";
            string toolKey = monitor_engine_mode == 0 ? "ht_monitor_test_dead_pixel" : "ht_monitor_test_dynamic_range";
            TSToolLauncher<GlowMonitorTestTool>(glow_tool_name, toolKey);
        }
        // MONITOR STUCK PIXEL FIXER TOOL
        // ======================================================================================================
        private void MonitorStuckPixelFixerToolToolStripMenuItem_Click(object sender, EventArgs e){
            try{
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                DialogResult mspf_open_warning = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_open_warning"), "\n\n", "\n\n", "\n\n"));
                if (mspf_open_warning == DialogResult.Yes)
                    TSToolLauncher<GlowStuckPixelFixerTool>("glow_monitor_stuck_pixel_fixer", "ht_stuck_pixel_fixer_tool");
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "MonitorStuckPixelFixerToolToolStripMenuItem_Click()"); }
            }
        }
        // DONATE LINK
        // ======================================================================================================
        private void DonateToolStripMenuItem_Click(object sender, EventArgs e){
            try{
                Process.Start(new ProcessStartInfo(TS_LinkSystem.ts_donate) { UseShellExecute = true });
            }catch (Exception ex){
                if (debug_status) { TSErrorLog.LogException(ex, "DonateToolStripMenuItem_Click()"); }
            }
        }
        // ABOUT
        // ======================================================================================================
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e){
            TSToolLauncher<GlowAbout>("glow_about", "header_menu_about");
        }
        // EXIT
        // ======================================================================================================
        private void CancelAllTasks(){
            try{
                if (ts_token_engine_stopper) return;
                ts_token_engine_stopper = true;
                if (debug_status){
                    TSLogger.Log("<---------------------------->");
                    TSLogger.Log("<--- ALL TASKS TERMINATED --->");
                    TSLogger.Log("<---------------------------->");
                }
            }catch (Exception ex){
                if (debug_status && !this.IsDisposed){
                    TSErrorLog.LogException(ex, "CancelAllTasks()");
                }
            }
        }
        private bool isExiting = false;
        private void Software_exit(){
            if (isExiting) return;
            isExiting = true;
            loop_status = false;
            CancelAllTasks();
            if (Application.MessageLoop){
                Application.Exit();
            }else{
                Environment.Exit(1);
            }
        }
        private void Glow_FormClosing(object sender, FormClosingEventArgs e){
            if (CPUbenchMode || DISKbenchMode || RAMbenchMode || BTfinderMode || SFCandDISMprocessStatus){
                e.Cancel = true;
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_all_prs_msg"));
            }else{
                Software_exit();
            }
        }
    }
}