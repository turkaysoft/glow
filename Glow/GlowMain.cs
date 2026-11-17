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

namespace Glow
{
    public partial class GlowMain : Form
    {
        public GlowMain()
        {
            InitializeComponent();
            // LANGUAGE SET TAGS
            // ==================
            arabicToolStripMenuItem.Tag = "ar";
            chineseToolStripMenuItem.Tag = "zh";
            chineseTaiwanToolStripMenuItem.Tag = "zh-tw";
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
            slovenianToolStripMenuItem.Tag = "sl";
            spanishToolStripMenuItem.Tag = "es";
            turkishToolStripMenuItem.Tag = "tr";
            // LANGUAGE SET EVENTS
            // ==================
            arabicToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            chineseToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            chineseTaiwanToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
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
            slovenianToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            spanishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            turkishToolStripMenuItem.Click += LanguageToolStripMenuItem_Click;
            // DYNAMIC THEME LISTENER
            // ==================
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
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
        public static bool CPUbenchMode = false, DISKbenchMode = false, RAMbenchMode = false, BTfinderMode = false, SFCandDISMprocessStatus = false, PortScannerMode = false, DNSTestMode = false;
        public static bool debug_status;
        // VARIABLES
        // ======================================================================================================
        private int menu_btns = 1, startup_status, stealth_status;
        private int _lastSysTheme = -1;
        private int _lastSysAccentArgb = int.MinValue;
        private long _lastSysApplyTicks = 0;
        private System.Windows.Forms.Timer _sysAccentRetryTimer;
        private int _sysAccentRetryCount = 0;
        private volatile bool loop_status = true;
        private bool laptop_mode = false;
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private readonly List<Task> _bgTasks = new List<Task>();
        private readonly object _bgTasksLock = new object();
        private bool _shutdownStarted = false;
        private void TrackBg(Task bgTask)
        {
            if (bgTask == null) return;
            lock (_bgTasksLock) { _bgTasks.Add(bgTask); }
        }
        private string iapps_unknown;
        private decimal battery_fullChargedCapacity_mWh = 0;
        private const decimal battery_MIN_POWER_THRESHOLD = 0.1m;
        private static bool HasSystemBattery()
        {
            try
            {
                BatteryChargeStatus status = SystemInformation.PowerStatus.BatteryChargeStatus;
                if (status.HasFlag(BatteryChargeStatus.Unknown)) return false;
                return !status.HasFlag(BatteryChargeStatus.NoSystemBattery);
            }
            catch { return false; }
        }
        private readonly List<Label> allCopyableLabels = new List<Label>();
        // STEALTH MODE CONFIG
        // ======================================================================================================
        private static readonly int sm_star_count = 8;
        private static string sm_text;
        // ======================================================================================================
        // UI COLORS
        private readonly List<Color> btn_colors_active = new List<Color>() { Color.Transparent };
        private static readonly List<Color> header_colors = new List<Color>() { Color.Transparent, Color.Transparent, Color.Transparent };
        // HEADER SETTINGS
        // ======================================================================================================
        private class HeaderMenuColors : ToolStripProfessionalRenderer
        {
            public HeaderMenuColors() : base(new HeaderColors()) { }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) { e.ArrowColor = header_colors[1]; base.OnRenderArrow(e); }
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float dpiScale = TSDpiHelper.Scale(g.DpiX);
                Rectangle rect = e.ImageRectangle;
                using (Pen anti_alias_pen = new Pen(header_colors[2], 2.2f * dpiScale))
                {
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
        private class HeaderColors : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => header_colors[0];
            public override Color MenuStripGradientEnd => header_colors[0];
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
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e) { e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
        // DYNAMIC CLICK AND COPY SYSTEM
        // ======================================================================================================
        private void TSCAC_Properties()
        {
            try
            {
                var OSLabels = new Label[] { OS_Name_V, OS_Manufacturer_V, OS_SystemVersion_V, OS_SystemArchitectural_V, OS_ExperienceVersion_V, OS_Country_V, OS_TimeZone_V, OS_SystemTime_V, OS_SystemWorkTime_V, OS_LastBootTime_V, OS_Install_V, OS_SystemLastShutDown_V, OS_PrimaryOS_V, OS_PortableOS_V, OS_FastBoot_V, OS_AVProgram_V, OS_FirewallProgram_V, OS_AntiSpywareProgram_V, OS_WinDefCoreIsolation_V, OS_ActivePower_V, OS_ActivePowerGUID_V, OS_ActivePowerScreenTimeOutP_V, OS_ActivePowerScreenTimeOutB_V, OS_ActivePowerSleepTimeP_V, OS_ActivePowerSleepTimeB_V, OS_MSOfficeVersion_V, OS_WinActiveChannel_V, OS_WinLicenseTime_V, OS_NETFrameworkVersion_V, OS_Minidump_V, OS_BSODDate_V };
                var MBLabels = new Label[] { MB_MotherBoardName_V, MB_MotherBoardMan_V, MB_SystemModelMan_V, MB_SystemModelFamily_V, MB_SystemFamily_V, MB_SystemModel_V, MB_Chipset_V, MB_BiosManufacturer_V, MB_BiosDate_V, MB_BiosVersion_V, MB_SmBiosVersion_V, MB_BiosMode_V, MB_LastBIOSTime_V, MB_SecureBoot_V, MB_TPMStatus_V, MB_TPMPPIVersion_V, MB_TPMPPIAltVersion_V, MB_TPMMan_V, MB_TPMManVersion_V, MB_TPMManFullVersion_V, MB_TPMManPublisher_V };
                var CPULabels = new Label[] { CPU_Manufacturer_V, CPU_Architectural_V, CPU_Speed_V, CPU_CurrentSpeed_V, CPU_CoreCount_V, CPU_ActiveCoreCount_V, CPU_LogicalCore_V, CPU_L1_V, CPU_L2_V, CPU_L3_V, CPU_Family_V, CPU_Slot_V, CPU_SocketDefinition_V, CPU_Virtualization_V, CPU_Usage_V, CPU_Process_V, CPU_Threads_V, CPU_Handles_V, CPU_Features_V, };
                var RAMLabels = new Label[] { RAM_TotalRAM_V, RAM_UsageRAMCount_V, RAM_EmptyRamCount_V, RAM_TotalVirtualRam_V, RAM_UsageVirtualRam_V, RAM_EmptyVirtualRam_V, RAM_SlotStatus_V, RAM_Amount_V, RAM_Type_V, RAM_Frequency_V, RAM_Volt_V, RAM_FormFactor_V, RAM_Manufacturer_V, RAM_BankLabel_V, RAM_DataWidth_V, RAM_BellekType_V };
                var GPULabels = new Label[] { GPU_Manufacturer_V, GPU_VRAM_V, GPU_Version_V, GPU_DriverDate_V, GPU_Status_V, GPU_DeviceID_V, GPU_DacType_V, GPU_GraphicDriversName_V, GPU_DirectXAll_V, GPU_DirectXMax_V, GPU_InfFileName_V, GPU_INFSectionFile_V, GPU_CurrentColor_V, GPU_MonitorUserFriendlyName_V, GPU_MonitorManName_V, GPU_MonitorProductCodeID_V, GPU_MonitorConType_V, GPU_MonitorManfDate_V, GPU_MonitorManfDateWeek_V, GPU_MonitorHID_V, GPU_MonitorResLabel_V, GPU_MonitorVirtualRes_V, GPU_MonitorBounds_V, GPU_MonitorWorking_V, GPU_ScreenRefreshRate_V, GPU_ScreenBit_V, GPU_MonitorPrimary_V };
                var DISKLabels = new Label[] { DISK_Model_V, DISK_Man_V, DISK_VolumeID_V, DISK_VolumeName_V, DISK_Firmware_V, DISK_Size_V, DISK_UsedSpace_V, DISK_FreeSpace_V, DISK_FileSystem_V, DISK_FormattingType_V, DISK_Type_V, DISK_DriveType_V, DISK_InterFace_V, DISK_PartitionCount_V, DISK_MediaLoaded_V, DISK_MediaStatus_V, DISK_Health_V, DISK_Boot_V, DISK_Bootable_V, DISK_BitLockerStatus_V, DISK_BitLockerConversionStatus_V, DISK_BitLockerEncryptMehod_V, DISK_DriveCompressed_V };
                var NETLabels = new Label[] { NET_LT_Device_V, NET_LT_BandWidth_V, NET_NetMan_V, NET_DriverVersion_V, NET_DriverDate_V, NET_ServiceName_V, NET_AdapterType_V, NET_Physical_V, NET_DeviceID_V, NET_ConnectionType_V, NET_Dhcp_status_V, NET_DHCPFirstIpTime_V, NET_DHCPLastIpTime_V, NET_LocalConSpeed_V };
                var USBLabels = new Label[] { USB_ConName_V, USB_ConMan_V, USB_ConDeviceID_V, USB_ConPNPDeviceID_V, USB_ConDeviceStatus_V, USB_DeviceName_V, USB_DeviceMan_V, USB_DriverVersion_V, USB_DriverDate_V, USB_InfFile_V, USB_DeviceID_V, USB_HardwareID_V };
                var SOUNDLabels = new Label[] { SOUND_DeviceName_V, SOUND_DeviceManufacturer_V, SOUND_DriverVersion_V, SOUND_DriverDate_V, SOUND_DeviceID_V, SOUND_PNPDeviceID_V, SOUND_DeviceStatus_V };
                var BATTERYLabels = new Label[] { BATTERY_Status_V, BATTERY_Model_V, BATTERY_Chemistry_V, BATTERY_DesignCapacity_V, BATTERY_FullChargeCapacity_V, BATTERY_RemainingChargeCapacity_V, BATTERY_Voltage_V, BATTERY_ChargePower_V, BATTERY_ChargeCurrent_V, BATTERY_DeChargePower_V, BATTERY_DeChargeCurrent_V };
                var STEALTHLabels = new Label[] { OS_SystemUser_V, OS_ComputerName_V, OS_SavedUser_V, OS_DeviceID_V, OS_Serial_V, OS_WinMachineID_V, OS_WinKey_V, OS_WinLicenseURL_V, OS_WinLicenseVerifiURL_V, OS_Wallpaper_V, MB_DeviceSerialNumber_V, MB_MotherBoardSerial_V, MB_SystemSKU_V, MB_TPMManID_V, CPU_SerialName_V, RAM_Serial_V, RAM_PartNumber_V, GPU_MonitorSerialNumberID_V, DISK_Serial_V, DISK_VolumeSerial_V, NET_MacAdress_V, NET_Guid_V, NET_IPv4Adress_V, NET_IPv6Adress_V, NET_SubnetMask_V, NET_P_IP_Adress_V, NET_P_ISP_V, NET_DNS_v4_1_V, NET_DNS_v4_2_V, NET_DNS_v6_1_V, NET_DNS_v6_2_V, NET_LT_LocalIP_V, NET_LT_GatewayIP_V, NET_Dhcp_server_V, USB_DeviceGUID_V, BATTERY_Serial_V };
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
                if (stealth_status != 1)
                {
                    allCopyableLabels.AddRange(STEALTHLabels);
                }
                //
                TSInitLabels(allCopyableLabels);
                TSSetTooltips(allCopyableLabels);
                if (stealth_status == 1)
                {
                    TSDisableCopyableLabels(STEALTHLabels);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "TSCAC_Properties()"); }
            }
        }
        private void TSInitLabels(IEnumerable<Label> labelList)
        {
            foreach (Label label in labelList)
            {
                label.ExecuteSafe(() =>
                {
                    label.Cursor = Cursors.Hand;
                    label.DoubleClick -= TSLabel_DoubleClick;
                    label.DoubleClick += TSLabel_DoubleClick;
                });
            }
        }
        private void TSDisableCopyableLabels(IEnumerable<Label> labelList)
        {
            foreach (Label label in labelList)
            {
                label.ExecuteSafe(() =>
                {
                    label.Cursor = Cursors.Default;
                    label.DoubleClick -= TSLabel_DoubleClick;
                    MainToolTip.SetToolTip(label, null);
                });
            }
        }
        private void TSSetTooltips(IEnumerable<Label> labelList)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string tooltipText = software_lang.TSReadLangs("ContentCrossFeature", "ccf_copy_hover");
            foreach (Label label in labelList)
            {
                label.ExecuteSafe(() =>
                {
                    MainToolTip.SetToolTip(label, tooltipText);
                });
            }
        }
        private void TSLabel_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (sender is Label label)
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    Clipboard.SetText(label.Text);
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("ContentCrossFeature", "ccf_copy_message"), label.Text));
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "TSLabel_DoubleClick()"); }
            }
        }
        // UI DPI (shared standard: TSDpiHelper)
        // ======================================================================================================
        private void ApplyDpiScaling()
        {
            if (IsDisposed || Disposing) return;
            try
            {
                ScaleMainTables();
                TSDpiHelper.ScaleToolStripRecursive(HeaderMenu, this.DeviceDpi);
                try
                {
                    OSD_TextBoxClearBtn.Height = OSD_TextBox.Height + 2;
                    SERVICE_TextBoxClearBtn.Height = SERVICE_TextBox.Height + 2;
                    INSTAPPS_TextBoxClearBtn.Height = INSTAPPS_TextBox.Height + 2;
                }
                catch { }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "ApplyDpiScaling()"); }
            }
        }
        private void ScaleMainTables(int dpiOverride = 0)
        {
            try
            {
                TSDpiHelper.ScaleDataGridView(OSD_DataMainTable, new[] { 160, 120, 120, 120, 100, 100 }, 32, dpiOverride: dpiOverride);
                TSDpiHelper.ScaleDataGridView(SERVICE_DataMainTable, new[] { 160, 120, 120, 120, 100, 100 }, 32, dpiOverride: dpiOverride);
                TSDpiHelper.ScaleDataGridView(INSTAPPS_DataMainTable, new[] { 65, 175, 100, 125, 75, 75, 150 }, 36, skipColumn: 0, dpiOverride: dpiOverride);
                RefreshInstappsIcons(dpiOverride);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "ScaleMainTables()"); }
            }
        }
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            try
            {
                this.SuspendLayout();
                ApplyDpiScaling();
                if (e.DeviceDpiNew != e.DeviceDpiOld)
                    Theme_engine(theme);
                MainContent?.PerformLayout();
                try { MainContent?.SelectedTab?.Invalidate(true); } catch { }
                this.ResumeLayout(true);
                this.PerformLayout();
                this.Invalidate(true);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OnDpiChanged()"); }
            }
        }
        // LOAD SOFTWARE SETTINGS
        // ======================================================================================================
        private async Task RunSoftwareEngine()
        {
            HeaderMenu.Cursor = Cursors.Hand;
            ApplyDpiScaling();
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
            // ALL DGV WIDTH - HEIGHT - PADDING (applied after columns are created)
            // ======================================================================================================
            ApplyDpiScaling();
            // SET DOUBLE BUFFER
            // ======================================================================================================
            typeof(TabControl).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, MainContent, new object[] { true });
            foreach (TabPage tp in MainContent.TabPages)
            {
                typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, tp, new object[] { true });
                try { var _ = tp.Handle; } catch { }
            }
            //
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, OSD_DataMainTable, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, SERVICE_DataMainTable, new object[] { true });
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, INSTAPPS_DataMainTable, new object[] { true });
            typeof(FlowLayoutPanel).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, EXPORT_CB_FLP, new object[] { true });
            // THEME - LANG - STARTUP - STEALTH - DEBUG MODE PRELOADER
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
                { "zh-tw", (ts_lang_zh_tw, chineseTaiwanToolStripMenuItem, File.Exists(ts_lang_zh_tw)) },
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
                { "sl", (ts_lang_sl, slovenianToolStripMenuItem, File.Exists(ts_lang_sl)) },
                { "es", (ts_lang_es, spanishToolStripMenuItem, File.Exists(ts_lang_es)) },
                { "tr", (ts_lang_tr, turkishToolStripMenuItem, File.Exists(ts_lang_tr)) },
            };
            foreach (var langLoader in languageFiles) { langLoader.Value.menuItem.Enabled = langLoader.Value.fileExists; }
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
            string stealth_mode = software_read_settings.TSReadSettings(ts_settings_container, "StealthModeStatus");
            stealth_status = int.TryParse(stealth_mode, out int ste_status) && (ste_status == 0 || ste_status == 1) ? ste_status : 0;
            stealthModeOffToolStripMenuItem.Checked = stealth_status == 0;
            stealthModeOnToolStripMenuItem.Checked = stealth_status == 1;
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            sm_text = new string('●', sm_star_count) + $" [ {software_lang.TSReadLangs("StealthMode", "sm_on_ui")} ]";
            //
            string debug_mode = software_read_settings.TSReadSettings(ts_settings_container, "DebugModeStatus");
            debug_status = debug_mode == "1";
            debugOffToolStripMenuItem.Checked = !debug_status;
            debugOnToolStripMenuItem.Checked = debug_status;
            if (debug_status) { TSLogger.Enable(true, Program.glow_console_debug_mode); }
            // PARALLEL LOADER START
            await GlowBootstrapper();
        }
        // TS PARALLEL RUN SECTION MODULE
        // ======================================================================================================
        private async Task RunSec(Action __action)
        {
            try
            {
                await Task.Run(__action);
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown, do not log.
            }
            catch (Exception ex)
            {
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }
        }
        private async Task RunSec(Func<Task> __action)
        {
            try
            {
                await Task.Run(async () => await __action());
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown, do not log.
            }
            catch (Exception ex)
            {
                if (debug_status) TSErrorLog.LogException(ex, "RunSec");
            }
        }
        private void SetDebugMenuState(bool enabled)
        {
            debugOnToolStripMenuItem.SetEnabledSafe(enabled);
            debugOffToolStripMenuItem.SetEnabledSafe(enabled);
            debugZIPToolStripMenuItem.SetEnabledSafe(enabled);
            debugDeleteToolStripMenuItem.SetEnabledSafe(enabled);
        }
        // TASK ALL PROCESS
        // ======================================================================================================
        private async Task GlowBootstrapper()
        {
            // PROPERTIES
            // ---------------
            var btTime = Stopwatch.StartNew();
            var tList = new List<Task>();
            // RUN PARALLEL START
            // ---------------
            try
            {
                TSLogger.Log("<--------------------------->");
                TSLogger.Log($"<--- {TS_VersionEngine.TS_SoftwareVersion(0)} Boot Has Started --->");
                TSLogger.Log("<--------------------------->");
                //
                tList.Add(RunSec(() => Os()));
                tList.Add(RunSec(() => Mb()));
                tList.Add(RunSec(() => Cpu()));
                tList.Add(RunSec(() => Ram()));
                tList.Add(RunSec(() => Gpu()));
                tList.Add(RunSec(() => Disk()));
                tList.Add(RunSec(() => Network()));
                tList.Add(RunSec(() => Usb()));
                tList.Add(RunSec(() => Sound()));
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
            try
            {
                laptop_mode = HasSystemBattery();
                if (laptop_mode)
                {
                    Battery_visible_on();
                    tList.Add(RunSec(Battery));
                    TrackBg(RunSec(() => BatteryBgProcess(_shutdownCts.Token)));
                }
                else
                {
                    Battery_visible_off();
                    BATTERY_RotateBtn.SetEnabledSafe(true);
                    BATTERY.SetEnabledSafe(true);
                    if (debug_status)
                    {
                        TSLogger.Log("<--- Battery Section Loaded / Desktop Mode --->");
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); }
            }
            try
            {
                tList.Add(RunSec(() => InstalledDrivers()));
                tList.Add(RunSec(() => InstalledServices()));
                tList.Add(RunSec(() => InstalledApps()));
                // //
                TrackBg(RunSec(() => OsBgProcess(_shutdownCts.Token)));
                TrackBg(RunSec(() => ProcessorBgProcess(_shutdownCts.Token)));
                TrackBg(RunSec(() => CpuBgProcess(_shutdownCts.Token)));
                TrackBg(RunSec(() => RamBgProcess(_shutdownCts.Token)));
                TrackBg(RunSec(() => NetBgProcess(_shutdownCts.Token)));
                tList.Add(RunSec(() => NetBGProcessGateway()));
                TrackBg(RunSec(() => DriversBgProcess(_shutdownCts.Token)));
                TrackBg(RunSec(() => ServicesBgProcess(_shutdownCts.Token)));
                // //
                tList.Add(RunSec(() => ExportModsAdd()));
                tList.Add(RunSec(() => ExportModsFilterAdd()));
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
            lock (_bgTasksLock) { _bgTasks.AddRange(tList); }
            // RUN PARALLEL PROCESS END
            // ---------------
            try
            {
                await Task.WhenAll(tList);
                //
                EXPORT.SetEnabledSafe(true);
                PRINT_RotateBtn.SetEnabledSafe(true);
                //
                btTime.Stop();
                if (debug_status)
                {
                    TSLogger.Log("<--- Export Section Loaded --->");
                    TSLogger.Log("<--------------------------->");
                    TSLogger.Log("<--- ALL TASKS COMPLETED --->");
                    TSLogger.Log($"<--- Total Load Time: {btTime.Elapsed.TotalSeconds:F2} seconds --->");
                    TSLogger.Log("<--------------------------->");
                    // UNLOCK LOG FILE
                    TSLogger.UnlockFile();
                }
                // DEBUG MODE MENU ENABLE
                SetDebugMenuState(true);
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GlowBootstrapper()"); } }
        }
        // SOFTWARE LOAD
        // ======================================================================================================
        private void Glow_Load(object sender, EventArgs e)
        {
            Text = TS_VersionEngine.TS_SoftwareVersion(0);
            // LAUNCH PROCESS
            // ====================================
            TrackBg(RunSoftwareEngine());
            // SOFTWARE UPDATE CHECK NATIVE
            // ====================================
            try { TrackBg(Software_update_check(0)); } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "Glow_Load()"); } }
            // DYNAMIC MULTI CLICK COPYABLE LABELS
            // ====================================
            TSCAC_Properties();
        }
        #region OS_Section
        // OPERATING SYSTEM
        // ======================================================================================================
        readonly List<string> minidump_files_list = new List<string>();
        readonly List<string> minidump_files_date_list = new List<string>();
        readonly string mdp_1 = Path.Combine(Program.Windows_disk, @"Windows\Minidump");
        readonly string mdp_2 = Path.Combine(Program.Windows_disk, @"Windows\memory.dmp");
        private void Os()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            using (ManagementObjectSearcher search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT RegisteredUser, Caption, Manufacturer, OSArchitecture, SerialNumber, LastBootUpTime, Primary, PortableOperatingSystem FROM Win32_OperatingSystem"))
            using (ManagementObjectSearcher search_av = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM AntiVirusProduct"))
            using (ManagementObjectSearcher search_fw = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM FirewallProduct"))
            using (ManagementObjectSearcher search_as = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT displayName, pathToSignedProductExe, pathToSignedReportingExe FROM AntiSpywareProduct"))
            {
            try
            {
                // SYSTEM USER
                if (stealth_status != 1)
                {
                    OS_SystemUser_V.SetTextSafe(Environment.UserName);
                }
                else
                {
                    OS_SystemUser_V.SetTextSafe(sm_text);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // PC NAME
                if (stealth_status != 1)
                {
                    OS_ComputerName_V.SetTextSafe(Dns.GetHostName());
                }
                else
                {
                    OS_ComputerName_V.SetTextSafe(sm_text);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            using (var osCol = search_os.Get())
            {
                foreach (ManagementObject query_os_rotate in osCol.Cast<ManagementObject>())
                {
                    using (query_os_rotate)
                    {
                        try
                        {
                            // REGISTERED USER
                            string os_saved_user = Convert.ToString(query_os_rotate["RegisteredUser"]).Trim();
                            if (!string.IsNullOrEmpty(os_saved_user))
                            {
                                if (stealth_status != 1)
                                {
                                    OS_SavedUser_V.SetTextSafe(os_saved_user);
                                }
                                else
                                {
                                    OS_SavedUser_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                OS_SavedUser_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS NAME
                            string os_name = Convert.ToString(query_os_rotate["Caption"]);
                            OS_Name_V.SetTextSafe(os_name);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS MANUFACTURER
                            OS_Manufacturer_V.SetTextSafe(Convert.ToString(query_os_rotate["Manufacturer"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS VERSION + ESU
                            const string regPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                            string displayVersion = Convert.ToString(Registry.GetValue(regPath, "DisplayVersion", ""))?.Trim();
                            string build = Convert.ToString(Registry.GetValue(regPath, "CurrentBuild", ""))?.Trim();
                            string ubr = Convert.ToString(Registry.GetValue(regPath, "UBR", ""))?.Trim();
                            //
                            bool isEsu = false;
                            try
                            {
                                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows\ConsumerESU", false))
                                {
                                    object value = key?.GetValue("ESUEligibilityResult");
                                    if (value is int v)
                                    {
                                        isEsu = (v == 1);
                                    }
                                    else if (value is byte[] bytes && bytes.Length >= 4)
                                    {
                                        isEsu = (BitConverter.ToInt32(bytes, 0) == 1);
                                    }
                                }
                            }
                            catch { }
                            if (!isEsu)
                            {
                                try
                                {
                                    using (ManagementObjectSearcher esuSearcher = new ManagementObjectSearcher("SELECT Name FROM SoftwareLicensingProduct WHERE Name LIKE '%ESU%'"))
                                    using (var esuCol = esuSearcher.Get())
                                    {
                                        foreach (ManagementObject esuObj in esuCol.Cast<ManagementObject>())
                                        {
                                            using (esuObj)
                                            {
                                                string esuName = Convert.ToString(esuObj["Name"]) ?? string.Empty;
                                                if (esuName.IndexOf("ESU", StringComparison.OrdinalIgnoreCase) >= 0)
                                                {
                                                    isEsu = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                                }
                            }
                            if (!string.IsNullOrEmpty(displayVersion) && !string.IsNullOrEmpty(build) && !string.IsNullOrEmpty(ubr))
                            {
                                if (Program.Windows_mode == 0 && isEsu)
                                {
                                    OS_SystemVersion_V.SetTextSafe($"{displayVersion} ({build}.{ubr}) (ESU)");
                                }
                                else
                                {
                                    OS_SystemVersion_V.SetTextSafe($"{displayVersion} ({build}.{ubr})");
                                }
                            }
                            else
                            {
                                OS_SystemVersion_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS ARCHITECTURE
                            string system_bit = Regex.Replace(Convert.ToString(query_os_rotate["OSArchitecture"]), @"\D", string.Empty).Trim();
                            OS_SystemArchitectural_V.SetTextSafe(system_bit + " " + software_lang.TSReadLangs("Os_Content", "os_c_bit") + " - " + string.Format("(x{0})", system_bit));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS DEVICE ID
                            string os_device_id = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\SQMClient", "MachineId", "").ToString().Trim();
                            if (!string.IsNullOrEmpty(os_device_id))
                            {
                                string os_device_id_replacer = os_device_id.Replace("{", string.Empty).Replace("}", string.Empty);
                                if (stealth_status != 1)
                                {
                                    OS_DeviceID_V.SetTextSafe(os_device_id_replacer);
                                }
                                else
                                {
                                    OS_DeviceID_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                OS_DeviceID_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS SERIAL
                            string os_serial = Convert.ToString(query_os_rotate["SerialNumber"]).Trim();
                            if (stealth_status != 1)
                            {
                                OS_Serial_V.SetTextSafe(os_serial);
                            }
                            else
                            {
                                OS_Serial_V.SetTextSafe(sm_text);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS MACHINE ID
                            string os_machine_id = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", "MachineGuid", "").ToString().Trim();
                            if (!string.IsNullOrEmpty(os_machine_id))
                            {
                                if (stealth_status != 1)
                                {
                                    OS_WinMachineID_V.SetTextSafe(os_machine_id);
                                }
                                else
                                {
                                    OS_WinMachineID_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                OS_WinMachineID_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // WINDOWS EXPERIENCE VERSION
                            List<Version> exp_ver_list = new List<Version>();
                            using (var get_exp_values = new ManagementObjectSearcher("root\\CIMV2", "SELECT Version FROM Win32_InstalledStoreProgram WHERE Name LIKE 'MicrosoftWindows.Client.CBS%'"))
                            using (var expCol = get_exp_values.Get())
                            {
                                foreach (ManagementObject exp_get in expCol.Cast<ManagementObject>())
                                {
                                    using (exp_get)
                                    {
                                        var vers_obj = exp_get["Version"];
                                        if (vers_obj == null)
                                            continue;
                                        if (Version.TryParse(vers_obj.ToString(), out Version parsed_exp_vers))
                                            exp_ver_list.Add(parsed_exp_vers);
                                    }
                                }
                            }
                            if (exp_ver_list.Count == 0)
                            {
                                try
                                {
                                    string systemAppsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SystemApps");
                                    foreach (string cbsDir in Directory.EnumerateDirectories(systemAppsDir, "MicrosoftWindows.Client.CBS*"))
                                    {
                                        try
                                        {
                                            string manifestPath = Path.Combine(cbsDir, "AppxManifest.xml");
                                            if (!File.Exists(manifestPath)) continue;
                                            using (var reader = System.Xml.XmlReader.Create(manifestPath))
                                            {
                                                while (reader.Read())
                                                {
                                                    if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.EndsWith("Identity", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        string verAttr = reader.GetAttribute("Version");
                                                        if (!string.IsNullOrEmpty(verAttr) && Version.TryParse(verAttr, out Version manifestVers))
                                                            exp_ver_list.Add(manifestVers);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                                }
                            }
                            string expVersion = exp_ver_list.Count > 0 ? exp_ver_list.Max().ToString() : software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                            OS_ExperienceVersion_V.SetTextSafe(expVersion);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // SYSTEM COUNTRY AND LANGUAGE
                            CultureInfo culture_info = CultureInfo.InstalledUICulture;
                            OS_Country_V.SetTextSafe(culture_info.DisplayName.Trim());
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // SYSTEM TIME ZONE
                            TimeZone os_time_zone = TimeZone.CurrentTimeZone;
                            TimeSpan utcOffset = os_time_zone.GetUtcOffset(DateTime.Now);
                            string offsetText = (utcOffset < TimeSpan.Zero ? "-" : "+") + utcOffset.Duration().ToString(@"hh\:mm");
                            OS_TimeZone_V.SetTextSafe(os_time_zone.StandardName + " (UTC" + offsetText + ")");
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS LAST BOOT
                            string last_bt = Convert.ToString(query_os_rotate["LastBootUpTime"]);
                            DateTime last_boot_time = ManagementDateTimeConverter.ToDateTime(last_bt);
                            OS_LastBootTime_V.SetTextSafe($"{last_boot_time:dd.MM.yyyy} - {last_boot_time:HH:mm:ss}");
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // OS SHUTDOWN TIME
                            string sd_time_path = @"System\CurrentControlSet\Control\Windows";
                            using (RegistryKey sd_time_key = Registry.LocalMachine.OpenSubKey(sd_time_path))
                            {
                                object sd_time_raw = sd_time_key?.GetValue("ShutdownTime");
                                if (sd_time_raw is byte[] sd_time_val && sd_time_val.Length >= 8)
                                {
                                    long sd_time_as_long = BitConverter.ToInt64(sd_time_val, 0);
                                    DateTime shut_down_time = DateTime.FromFileTime(sd_time_as_long);
                                    OS_SystemLastShutDown_V.SetTextSafe(shut_down_time.ToString("dd.MM.yyyy - HH:mm:ss"));
                                }
                                else
                                {
                                    OS_SystemLastShutDown_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // PRIMARY OS STATUS
                            bool system_primary_os_status = Convert.ToBoolean(query_os_rotate["Primary"]);
                            if (system_primary_os_status == true)
                            {
                                OS_PrimaryOS_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_yes"));
                            }
                            else if (system_primary_os_status == false)
                            {
                                OS_PrimaryOS_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_no"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // PORTABLE OS STATUS
                            bool system_portable_status = Convert.ToBoolean(query_os_rotate["PortableOperatingSystem"]);
                            if (system_portable_status == true)
                            {
                                OS_PortableOS_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_yes"));
                            }
                            else if (system_portable_status == false)
                            {
                                OS_PortableOS_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_no"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                        try
                        {
                            // FASTBOOT STATUS
                            RegistryKey get_fastboot_status = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power");
                            string get_fs_val = get_fastboot_status?.GetValue("HiberbootEnabled")?.ToString().Trim();
                            string langKey = get_fs_val == "1" ? "os_c_fastboot_active" : get_fs_val == "0" ? "os_c_fastboot_deactive" : "os_c_unknown";
                            OS_FastBoot_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", langKey));
                            get_fastboot_status?.Close();
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                        }
                    }
                }
            }
            try
            {
                // SYSTEM ANTI-VIRUS PRODUCT
                List<string> av_list = new List<string>();
                using (var avCol = search_av.Get())
                {
                    foreach (ManagementObject query_av in avCol.Cast<ManagementObject>())
                    {
                        using (query_av)
                        {
                            string av_path = Convert.ToString(query_av["pathToSignedProductExe"]).Trim();
                            av_path = Environment.ExpandEnvironmentVariables(av_path);
                            if (string.IsNullOrEmpty(av_path) || !File.Exists(av_path))
                            {
                                av_path = Convert.ToString(query_av["pathToSignedReportingExe"]).Trim();
                                av_path = Environment.ExpandEnvironmentVariables(av_path);
                            }
                            if (!string.IsNullOrEmpty(av_path) && File.Exists(av_path))
                            {
                                string av_name = Convert.ToString(query_av["DisplayName"]).Trim();
                                if (!string.IsNullOrEmpty(av_name) && !av_list.Contains(av_name, StringComparer.OrdinalIgnoreCase)) av_list.Add(av_name);
                            }
                        }
                    }
                }
                if (av_list.Count < 1)
                {
                    av_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_anti_virus_count"));
                }
                string av_list_split = string.Join(" - ", av_list);
                OS_AVProgram_V.SetTextSafe(av_list_split.Trim());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // SYSTEM FIREWALL PRODUCT
                List<string> fw_list = new List<string>();
                using (var fwCol = search_fw.Get())
                {
                    foreach (ManagementObject query_fw in fwCol.Cast<ManagementObject>())
                    {
                        using (query_fw)
                        {
                            string fw_path = Convert.ToString(query_fw["pathToSignedProductExe"]).Trim();
                            fw_path = Environment.ExpandEnvironmentVariables(fw_path);
                            if (string.IsNullOrEmpty(fw_path) || !File.Exists(fw_path))
                            {
                                fw_path = Convert.ToString(query_fw["pathToSignedReportingExe"]).Trim();
                                fw_path = Environment.ExpandEnvironmentVariables(fw_path);
                            }
                            if (!string.IsNullOrEmpty(fw_path) && File.Exists(fw_path))
                            {
                                string fw_name = Convert.ToString(query_fw["DisplayName"]).Trim();
                                if (!string.IsNullOrEmpty(fw_name) && !fw_list.Contains(fw_name, StringComparer.OrdinalIgnoreCase)) fw_list.Add(fw_name);
                            }
                        }
                    }
                }
                if (fw_list.Count < 1)
                {
                    fw_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_firewall_count"));
                }
                string fw_list_split = string.Join(" - ", fw_list);
                OS_FirewallProgram_V.SetTextSafe(fw_list_split.Trim());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // SYSTEM ANTI-SPYWARE PRODUCT
                List<string> as_list = new List<string>();
                using (var asCol = search_as.Get())
                {
                    foreach (ManagementObject query_as in asCol.Cast<ManagementObject>())
                    {
                        using (query_as)
                        {
                            string as_path = Convert.ToString(query_as["pathToSignedProductExe"]).Trim();
                            as_path = Environment.ExpandEnvironmentVariables(as_path);
                            if (string.IsNullOrEmpty(as_path) || !File.Exists(as_path))
                            {
                                as_path = Convert.ToString(query_as["pathToSignedReportingExe"]).Trim();
                                as_path = Environment.ExpandEnvironmentVariables(as_path);
                            }
                            if (!string.IsNullOrEmpty(as_path) && File.Exists(as_path))
                            {
                                string as_name = Convert.ToString(query_as["DisplayName"]).Trim();
                                if (!string.IsNullOrEmpty(as_name) && !as_list.Contains(as_name, StringComparer.OrdinalIgnoreCase)) as_list.Add(as_name);
                            }
                        }
                    }
                }
                if (as_list.Count < 1)
                {
                    as_list.Add(software_lang.TSReadLangs("Os_Content", "os_c_anti_spyware_count"));
                }
                string as_list_split = string.Join(" - ", as_list);
                OS_AntiSpywareProgram_V.SetTextSafe(as_list_split.Trim());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // WINDOWS DEFENDER CORE ISOLATION STATUS
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"))
                {
                    string val = key?.GetValue("Enabled")?.ToString()?.Trim() ?? "0";
                    string langKey = val == "1" ? "os_c_win_core_isolation_active" : "os_c_win_core_isolation_deactive";
                    OS_WinDefCoreIsolation_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", langKey));
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // ACTIVE POWER PLAN
                string powerOutput;
                string activePlanName = null;
                string activePlanGuid = null;
                //
                using (var get_power_mode = new Process())
                {
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
                foreach (var target_power_line in split_power_line)
                {
                    if (target_power_line.Contains("GUID:") && target_power_line.Trim().EndsWith("*"))
                    {
                        var planMatch = Regex.Match(target_power_line, @"\((.*?)\)");
                        if (planMatch.Success)
                        {
                            activePlanName = planMatch.Groups[1].Value;
                        }
                        var guidMatch = Regex.Match(target_power_line, @"GUID:\s*([a-fA-F0-9\-]{36})");
                        if (guidMatch.Success)
                        {
                            activePlanGuid = guidMatch.Groups[1].Value;
                        }
                        break;
                    }
                }
                //
                OS_ActivePower_V.SetTextSafe(activePlanName != null ? $"{activePlanName}" : software_lang.TSReadLangs("Os_Content", "os_c_a_power_null"));
                OS_ActivePowerGUID_V.SetTextSafe(activePlanGuid != null ? $"{activePlanGuid}" : software_lang.TSReadLangs("Os_Content", "os_c_a_power_null"));
                bool isLaptop = HasSystemBattery();
                // SCREEN CLOSE AND SLEEP TIME
                if (!string.IsNullOrEmpty(activePlanGuid))
                {
                    // TIMEOUT
                    int acTimeout = SCQueryTimeout(activePlanGuid, "SUB_VIDEO", "VIDEOIDLE", true);
                    OS_ActivePowerScreenTimeOutP_V.SetTextSafe(SCFormatTimeout(acTimeout));
                    if (isLaptop)
                    {
                        int dcTimeout = SCQueryTimeout(activePlanGuid, "SUB_VIDEO", "VIDEOIDLE", false);
                        OS_ActivePowerScreenTimeOutB_V.SetTextSafe(SCFormatTimeout(dcTimeout));
                    }
                    else
                    {
                        OS_ActivePowerScreenTimeOutB_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_a_power_desktop"));
                    }
                    // SLEE TIME
                    int acSleep = SCQueryTimeout(activePlanGuid, "SUB_SLEEP", "STANDBYIDLE", true);
                    OS_ActivePowerSleepTimeP_V.SetTextSafe(SCFormatTimeout(acSleep));
                    if (isLaptop)
                    {
                        int dcSleep = SCQueryTimeout(activePlanGuid, "SUB_SLEEP", "STANDBYIDLE", false);
                        OS_ActivePowerSleepTimeB_V.SetTextSafe(SCFormatTimeout(dcSleep));
                    }
                    else
                    {
                        OS_ActivePowerSleepTimeB_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_a_power_desktop"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // MS OFFICE VERSION
                StartMsOfficeVersionCheck();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // WINDOWS LICENSE KEY
                string win_key = (TSWindowsProductKey.GetWindowsProductKey() ?? "NO_KEY").Trim();
                if (!string.IsNullOrEmpty(win_key) && win_key != "NS_OS" && win_key != "NO_SUB_KEY" && win_key != "NO_KEY")
                {
                    if (stealth_status != 1)
                    {
                        OS_WinKey_V.SetTextSafe(win_key);
                    }
                    else
                    {
                        OS_WinKey_V.SetTextSafe(sm_text);
                    }
                }
                else
                {
                    switch (win_key)
                    {
                        case "NS_OS":
                            OS_WinKey_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_license_unsupported_os"));
                            break;
                        default:
                            OS_WinKey_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_license_null"));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                try { OS_WinKey_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_license_null")); } catch { }
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // WINDOWS LICSENSE TYPE
                StartWindowsLicenseTypeCheck();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // WINDOWS LICENSE TIME
                Task.Run(() => WinLicenseTime());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // WINDOWS LICENSE URL & VERIFICATION URL
                Task.Run(() => ExtractHostsSLMGR(RunSlmgrDlv(out _, out _))).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        if (debug_status && t.Exception != null)
                            TSErrorLog.LogException(t.Exception, "Os()");
                        return;
                    }
                    List<string> get_hosts_url = t.Result;
                    string unknown = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                    string host1 = get_hosts_url.Count > 0 ? get_hosts_url[0].Trim() : unknown;
                    string host2 = get_hosts_url.Count > 1 ? get_hosts_url[1].Trim() : unknown;
                    if (stealth_status != 1)
                    {
                        OS_WinLicenseURL_V.SetTextSafe(host1);
                        OS_WinLicenseVerifiURL_V.SetTextSafe(host2);
                    }
                    else
                    {
                        string hidden1 = get_hosts_url.Count > 0 ? sm_text : unknown;
                        string hidden2 = get_hosts_url.Count > 1 ? sm_text : unknown;
                        OS_WinLicenseURL_V.SetTextSafe(hidden1);
                        OS_WinLicenseVerifiURL_V.SetTextSafe(hidden2);
                    }
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // .NET FRAMEWORK VERSION
                // https://learn.microsoft.com/tr-tr/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
                using (RegistryKey get_net_version = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                {
                    if (get_net_version != null)
                    {
                        object releaseKey = get_net_version.GetValue("Release");
                        if (releaseKey != null)
                        {
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
                            foreach (var kvp in netFrameworkVersions.OrderByDescending(k => k.Key))
                            {
                                if (releaseKeyValue >= kvp.Key)
                                {
                                    version = kvp.Value;
                                    break;
                                }
                            }
                            OS_NETFrameworkVersion_V.SetTextSafe(version);
                        }
                        else
                        {
                            OS_NETFrameworkVersion_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_net_framework_null"));
                        }
                    }
                    else
                    {
                        OS_NETFrameworkVersion_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_net_framework_null"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // OS BLUESCREEN CHECK
                // Check Folder
                string[] minidump_files = new string[0];
                bool minidumpFolderExists = Directory.Exists(mdp_1);
                if (minidumpFolderExists)
                {
                    // Check minidump count
                    try { minidump_files = Directory.GetFiles(mdp_1, "*.dmp", SearchOption.AllDirectories); }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
                    }
                    int minidump_count = minidump_files.Length;
                    if (minidump_count > 0)
                    {
                        for (int i = 0; i <= minidump_files.Length - 1; i++)
                        {
                            minidump_files_list.Add(minidump_files[i]);
                        }
                    }
                    // Check memory.dmp file
                    if (File.Exists(mdp_2))
                    {
                        minidump_files_list.Add(mdp_2);
                    }
                }
                else
                {
                    // Check memory.dmp file
                    if (File.Exists(mdp_2))
                    {
                        minidump_files_list.Add(mdp_2);
                    }
                }
                // no memory.dmp and no folder
                if (minidump_files_list.Count > 0)
                {
                    OS_MinidumpOpen.SetVisibleSafe(true);
                    OS_BSoDZIP.SetVisibleSafe(true);
                    OS_Minidump_V.SetTextSafe(string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_detect"), minidump_files_list.Count));
                    bool folderHasFiles = minidumpFolderExists && minidump_files.Length > 0;
                    bool memDumpListed = minidump_files_list.Any(f => string.Equals(Path.GetFileName(f), Path.GetFileName(mdp_2), StringComparison.OrdinalIgnoreCase));
                    OS_MinidumpOpen.ExecuteSafe(() =>
                    {
                        MainToolTip.SetToolTip(OS_MinidumpOpen, folderHasFiles ? string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_folder"), mdp_1) : (memDumpListed && File.Exists(mdp_2)) ? string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_file"), mdp_2) : (minidumpFolderExists && !folderHasFiles) ? software_lang.TSReadLangs("Os_Content", "os_c_bsod_folder_open_error") : software_lang.TSReadLangs("Os_Content", "os_c_bsod_dump_file_open_error"));
                        MainToolTip.SetToolTip(OS_BSoDZIP, software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_folder"));
                    });
                }
                else
                {
                    OS_MinidumpOpen.SetVisibleSafe(false);
                    OS_BSoDZIP.SetVisibleSafe(false);
                    OS_Minidump_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect"));
                    OS_BSODDate_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_bsod_not_detect"));
                }
                // Check minidump files count
                if (minidump_files_list.Count > 0)
                {
                    // Get minidump date
                    for (int i = 0; i <= minidump_files_list.Count - 1; i++)
                    {
                        DateTime modification = File.GetLastWriteTime(minidump_files_list[i]);
                        minidump_files_date_list.Add(modification.ToString());
                    }
                    // Reverse date
                    minidump_files_date_list.Reverse();
                    // Start dynamic Last BSoD Date
                    StartBsodTimeDynamicCheck();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            try
            {
                // DESKTOP WALLPAPER
                StringBuilder wallpaperPath = new StringBuilder(260);
                bool wallpaperResult = SystemParametersInfo(0x0073, 260, wallpaperPath, 0);
                if (wallpaperResult && File.Exists(wallpaperPath.ToString()))
                {
                    wp_rotate = wallpaperPath.ToString().Trim();
                    string wp_resolution;
                    using (var fs = new FileStream(wp_rotate, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var image = Image.FromStream(fs, false, false))
                    {
                        wp_resolution = $"{image.Width}x{image.Height}";
                    }
                    FileInfo fileInfo = new FileInfo(wp_rotate);
                    string fileName = Path.GetFileName(wp_rotate);
                    string formattedSize = TS_FormatSize(Convert.ToDouble(fileInfo.Length));
                    string wallpaperText;
                    if (stealth_status != 1)
                    {
                        wallpaperText = $"{fileName} - {wp_resolution} - {formattedSize}";
                    }
                    else
                    {
                        wallpaperText = sm_text;
                    }
                    OS_Wallpaper_V.SetTextSafe(wallpaperText);
                    OS_Wallpaper_V.ExecuteSafe(() =>
                    {
                        MainToolTip.SetToolTip(OS_WallpaperOpen, software_lang.TSReadLangs("Os_Content", "os_c_open_wallpaper"));
                        MainToolTip.SetToolTip(OS_WallpaperPreview, software_lang.TSReadLangs("Os_Content", "os_c_preview_wallpaper"));
                    });
                }
                else
                {
                    OS_Wallpaper_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_not_detect"));
                    OS_WallpaperOpen.SetVisibleSafe(false);
                    OS_WallpaperPreview.SetVisibleSafe(false);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Os()"); }
            }
            // OS PROCESS END ENABLED
            OS_RotateBtn.SetEnabledSafe(true);
            OS.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- Operating System Section Loaded --->");
            }
        }
        private async Task OsBgProcess(CancellationToken shutdownToken)
        {
            var software_lang = new TSGetLangs(lang_path);
            while (!shutdownToken.IsCancellationRequested && loop_status)
            {
                try
                {
                    OS_SystemTime_V.SetTextSafe(DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss"));
                    using (var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT FreeVirtualMemory, TotalVirtualMemorySize, InstallDate, LastBootUpTime FROM Win32_OperatingSystem"))
                    {
                        UpdateSystemInfoBG(search_os, software_lang);
                    }
                    int msToNextSecond = 1000 - DateTime.Now.Millisecond;
                    if (msToNextSecond <= 0) msToNextSecond = 10;
                    await Task.Delay(msToNextSecond, shutdownToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "OsBgProcess()"); }
                }
            }
        }
        private void UpdateSystemInfoBG(ManagementObjectSearcher search_os, TSGetLangs software_lang)
        {
            try
            {
                string oi_year = software_lang.TSReadLangs("Os_Content", "os_c_year");
                string oi_month = software_lang.TSReadLangs("Os_Content", "os_c_month");
                string oi_day = software_lang.TSReadLangs("Os_Content", "os_c_day");
                string oi_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
                string oi_minute = software_lang.TSReadLangs("Os_Content", "os_c_minute");
                string oi_second = software_lang.TSReadLangs("Os_Content", "os_c_second");
                string oi_ago = software_lang.TSReadLangs("Os_Content", "os_c_ago");
                //
                using (var collection = search_os.Get())
                {
                    foreach (ManagementObject query_os in collection.Cast<ManagementObject>())
                    {
                        using (query_os)
                        {
                            double freeVirtualBytes = 0;
                            double totalVirtualBytes = 0;
                            try
                            {
                                var freeVmObj = query_os["FreeVirtualMemory"];
                                var totalVmObj = query_os["TotalVirtualMemorySize"];
                                if (freeVmObj != null) freeVirtualBytes = Convert.ToDouble(freeVmObj) * 1024d;
                                if (totalVmObj != null) totalVirtualBytes = Convert.ToDouble(totalVmObj) * 1024d;
                                if (totalVirtualBytes < freeVirtualBytes) totalVirtualBytes = freeVirtualBytes;
                            }
                            catch { }
                            //
                            RAM_EmptyVirtualRam_V.SetTextSafe(TS_FormatSize(freeVirtualBytes));
                            RAM_UsageVirtualRam_V.SetTextSafe(TS_FormatSize(Math.Max(0, totalVirtualBytes - freeVirtualBytes)));
                            //
                            DateTime now = DateTime.Now;
                            try
                            {
                                var installObj = query_os["InstallDate"];
                                if (installObj != null)
                                {
                                    DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(installObj.ToString());
                                    if (now < osInstallDate)
                                    {
                                        OS_Install_V.SetTextSafe($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm} - (0 {oi_minute} {oi_ago})");
                                    }
                                    else
                                    {
                                        int totalMonths = (now.Year - osInstallDate.Year) * 12 + (now.Month - osInstallDate.Month);
                                        if (now.Day < osInstallDate.Day) totalMonths--;
                                        if (totalMonths < 0) totalMonths = 0;
                                        int years = totalMonths / 12;
                                        int months = totalMonths % 12;
                                        DateTime anchor = osInstallDate.AddMonths(years * 12 + months);
                                        if (anchor > now)
                                        {
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
                                        OS_Install_V.SetTextSafe($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm} - ({installDuration} {oi_ago})");
                                    }
                                }
                                else
                                {
                                    OS_Install_V.SetTextSafe("-");
                                }
                            }
                            catch
                            {
                                OS_Install_V.SetTextSafe("-");
                            }
                            try
                            {
                                var bootObj = query_os["LastBootUpTime"];
                                if (bootObj != null)
                                {
                                    DateTime bootDate = ManagementDateTimeConverter.ToDateTime(bootObj.ToString());
                                    if (now < bootDate)
                                    {
                                        OS_SystemWorkTime_V.SetTextSafe($"0 {oi_second}");
                                    }
                                    else
                                    {
                                        int totalMonths = (now.Year - bootDate.Year) * 12 + (now.Month - bootDate.Month);
                                        if (now.Day < bootDate.Day) totalMonths--;
                                        if (totalMonths < 0) totalMonths = 0;
                                        DateTime anchor = bootDate.AddMonths(totalMonths);
                                        if (anchor > now)
                                        {
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
                                        OS_SystemWorkTime_V.SetTextSafe(uptimeParts.Count > 0 ? string.Join(", ", uptimeParts) : $"0 {oi_second}");
                                    }
                                }
                                else
                                {
                                    OS_SystemWorkTime_V.SetTextSafe($"0 {oi_second}");
                                }
                            }
                            catch
                            {
                                OS_SystemWorkTime_V.SetTextSafe($"0 {oi_second}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "UpdateSystemInfoBG()"); }
            }
        }
        // SYSTEM SCREEN CLOSE AND SLEEP TIMER CONVERTER
        private static string SCFormatTimeout(int seconds)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string time_never = software_lang.TSReadLangs("Os_Content", "os_c_a_power_never");
            string time_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
            string time_miniutes = software_lang.TSReadLangs("Os_Content", "os_c_minute");
            //
            if (seconds < 0) return software_lang.TSReadLangs("Os_Content", "os_c_unknown");
            if (seconds == 0) return time_never;
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            string result_timeout = "";
            if (h > 0) result_timeout += $"{h} {time_hour}";
            if (m > 0) result_timeout += (result_timeout != "" ? " " : "") + $"{m} {time_miniutes}";
            return result_timeout;
        }
        // SYSTEM SCREEN CLOSE AND SLEEP GET DATA
        private static int SCQueryTimeout(string planGuid, string subgroupAlias, string settingAlias, bool acPower)
        {
            string cmd = $"/QUERY {planGuid} {subgroupAlias} {settingAlias}";
            string get_time_output;
            using (var getTimeProcess = new Process())
            {
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
            foreach (var splitLine in get_time_output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                if (splitLine.Contains(searchTime))
                {
                    string[] timeParts = splitLine.Split(new[] { ':' }, 2);
                    if (timeParts.Length < 2) continue;
                    string convertTime = timeParts[1].Trim();
                    if (convertTime.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        convertTime = convertTime.Substring(2);
                    if (int.TryParse(convertTime, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int parsedTime))
                        return parsedTime;
                    if (int.TryParse(convertTime, out parsedTime))
                        return parsedTime;
                }
            }
            return -1;
        }
        // GET MS OFFICE VERSION
        private async void StartMsOfficeVersionCheck()
        {
            try
            {
                await Ms_office_version();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StartMsOfficeVersionCheck()"); }
            }
        }
        private async Task Ms_office_version()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                //
                string LicensedText = software_lang.TSReadLangs("Os_Content", "os_c_office_licensed");
                string GraceText = software_lang.TSReadLangs("Os_Content", "os_c_office_grace");
                string OtherText = software_lang.TSReadLangs("Os_Content", "os_c_office_other");
                string NotificationsText = software_lang.TSReadLangs("Os_Content", "os_c_office_notifications");
                string SubscriptionText = software_lang.TSReadLangs("Os_Content", "os_c_office_subscription");
                string OfficeNotFoundText = software_lang.TSReadLangs("Os_Content", "os_c_office_not_found");
                //
                var productsList = await Task.Run(() =>
                {
                    var found = new List<(string family, string license, bool isGraceEdition, bool isLicensed)>();
                    // Map OSPP license name to a friendly product family (covers Office 2010-2024 and Microsoft 365).
                    string MapOfficeFamily(string licenseName)
                    {
                        if (string.IsNullOrWhiteSpace(licenseName)) return OtherText;
                        string upper = licenseName.ToUpperInvariant();
                        if (upper.Contains("O365") || upper.Contains("M365"))
                        {
                            if (upper.Contains("VISIO")) return "Visio (Microsoft 365)";
                            if (upper.Contains("PROJECT")) return "Project (Microsoft 365)";
                            if (upper.Contains("BUSINESS")) return "Microsoft 365 Business";
                            if (upper.Contains("HOMEPREM") || upper.Contains("FAMILY")) return "Microsoft 365 Family";
                            if (upper.Contains("EDU")) return "Microsoft 365 Education";
                            if (upper.Contains("PERSONAL")) return "Microsoft 365 Personal";
                            return "Microsoft 365 Apps";
                        }
                        string ver = "";
                        if (upper.Contains("OFFICE26")) ver = "Office 2026";
                        else if (upper.Contains("OFFICE24")) ver = "Office 2024";
                        else if (upper.Contains("OFFICE21")) ver = "Office 2021";
                        else if (upper.Contains("OFFICE19")) ver = "Office 2019";
                        else if (upper.Contains("OFFICE16")) ver = "Office 2016";
                        else if (upper.Contains("OFFICE15")) ver = "Office 2013";
                        else if (upper.Contains("OFFICE14")) ver = "Office 2010";
                        if (ver.Length == 0)
                        {
                            int comma = licenseName.IndexOf(',');
                            return (comma > 0 ? licenseName.Substring(0, comma) : licenseName).Trim();
                        }
                        string year = ver.Substring(7);
                        if (upper.Contains("VISIO")) return "Visio " + year + " Pro";
                        if (upper.Contains("PROJECT")) return "Project " + year + " Pro";
                        if (upper.Contains("PROPLUS") || upper.Contains("PROFESSIONAL")) return ver + " Pro Plus";
                        if (upper.Contains("STANDARD")) return ver + " Standard";
                        if (upper.Contains("HOME") && upper.Contains("BUSINESS")) return ver + " Home and Business";
                        if (upper.Contains("PERSONAL")) return ver + " Personal";
                        if (upper.Contains("BUSINESS")) return ver + " Business";
                        if (upper.Contains("HOME") || upper.Contains("STUDENT")) return ver + " Home";
                        return ver;
                    }
                    // Map raw OSPP status to a readable string. Never leak raw ---STATUS--- text.
                    string MapOfficeStatus(string status)
                    {
                        string s = (status ?? "").ToUpperInvariant();
                        if (s.Contains("UNLICENSED")) return NotificationsText;
                        if (s.Contains("LICENSED")) return LicensedText;
                        if (s.Contains("GRACE")) return GraceText;
                        if (s.Contains("NOTIFICATION")) return NotificationsText;
                        return OtherText;
                    }
                    bool IsOfficeLicensed(string status)
                    {
                        string s = (status ?? "").ToUpperInvariant();
                        return s.Contains("LICENSED") && !s.Contains("UNLICENSED");
                    }
                    // Map OSPP channel description (covers Retail/Volume/OEM/Subscription for all versions).
                    string MapOfficeChannel(string desc)
                    {
                        string d = (desc ?? "").ToUpperInvariant();
                        if (d.Contains("TIMEBASED") || d.Contains("SUBSCRIPTION")) return SubscriptionText;
                        if (d.Contains("VOLUME") || d.Contains("KMS") || d.Contains("MAK") || d.Contains("_VL")) return "Volume";
                        if (d.Contains("OEM")) return "OEM";
                        if (d.Contains("RETAIL")) return "Retail";
                        return OtherText;
                    }
                    //
                    var baseDirs = new[]{
                        $@"{Program.Windows_disk}Program Files\Microsoft Office",
                        $@"{Program.Windows_disk}Program Files (x86)\Microsoft Office",
                        $@"{Program.Windows_disk}Program Files\Microsoft Office\root",
                        $@"{Program.Windows_disk}Program Files (x86)\Microsoft Office\root",
                        $@"{Program.Windows_disk}Common Files\Microsoft Shared\Office16",
                        $@"{Program.Windows_disk}Common Files (x86)\Microsoft Shared\Office16"
                    };
                    //
                    IEnumerable<string> FindOsppFiles()
                    {
                        var results = new List<string>();
                        var seenOspp = new HashSet<string>(StringComparer.Ordinal);
                        foreach (var baseDir in baseDirs)
                        {
                            if (!Directory.Exists(baseDir)) continue;
                            try
                            {
                                string directPath = Path.Combine(baseDir, "ospp.vbs");
                                if (File.Exists(directPath)) results.Add(directPath);
                                // Depth Limit: 6
                                GetAllDirectoriesWithDepthLimit(baseDir, 6, results, seenOspp);
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) TSErrorLog.LogException(ex, "Ms_office_version() - FindOsppFiles");
                            }
                        }
                        return results;
                    }
                    //
                    void GetAllDirectoriesWithDepthLimit(string path, int maxDepth, List<string> results, HashSet<string> seen)
                    {
                        var stack = new Stack<(string Path, int Depth)>();
                        stack.Push((path, 0));
                        while (stack.Count > 0)
                        {
                            var (current, depth) = stack.Pop();
                            if (depth >= maxDepth) continue;
                            try
                            {
                                foreach (var dir in Directory.GetDirectories(current))
                                {
                                    string candidate = Path.Combine(dir, "ospp.vbs");
                                    if (File.Exists(candidate) && seen.Add(candidate))
                                    {
                                        results.Add(candidate);
                                    }
                                    stack.Push((dir, depth + 1));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) TSErrorLog.LogException(ex, $"GetAllDirectoriesWithDepthLimit - {current}");
                            }
                        }
                    }
                    //
                    var osppFiles = FindOsppFiles().Distinct();
                    foreach (var osppPath in osppFiles)
                    {
                        try
                        {
                            var psi = new ProcessStartInfo("cscript.exe", $"\"{osppPath}\" /dstatus")
                            {
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                StandardOutputEncoding = Encoding.UTF8
                            };
                            //
                            using (var process = Process.Start(psi))
                            {
                                if (process == null) continue;
                                string output = process.StandardOutput.ReadToEnd();
                                process.WaitForExit();
                                //
                                string currentName = null;
                                string currentDesc = null;
                                //
                                var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var rawLine in lines)
                                {
                                    string line = rawLine.Trim();
                                    if (string.IsNullOrEmpty(line)) continue;
                                    if (line.StartsWith("LICENSE NAME:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentName = line.Substring(13).Trim();
                                        currentDesc = null;
                                    }
                                    else if (line.StartsWith("LICENSE DESCRIPTION:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentDesc = line.Substring(20).Trim();
                                    }
                                    else if (line.StartsWith("LICENSE STATUS:", StringComparison.OrdinalIgnoreCase) && currentName != null)
                                    {
                                        string status = line.Substring(15).Trim();
                                        string family = MapOfficeFamily(currentName);
                                        string readable = MapOfficeStatus(status);
                                        string channel = MapOfficeChannel(currentDesc);
                                        bool graceEd = currentName.ToUpperInvariant().Contains("GRACE");
                                        bool licensed = IsOfficeLicensed(status);
                                        found.Add((family, $"{readable} / {channel}", graceEd, licensed));
                                        currentName = null;
                                        currentDesc = null;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) TSErrorLog.LogException(ex, $"Ms_office_version() - osppPath: {osppPath}");
                        }
                    }
                    //
                    return found;
                });
                var entries = productsList.ToList();
                // Drop stale Grace leftovers when the same family has a licensed product (typical on healthy Microsoft 365 installs).
                if (entries.Any(e => e.isLicensed))
                {
                    entries = entries.Where(e => e.isLicensed || !e.isGraceEdition || !entries.Any(o => o.isLicensed && o.family == e.family)).ToList();
                }
                string displayText;
                if (!entries.Any())
                {
                    string ctrFallback = GetClickToRunOfficeLabel();
                    displayText = string.IsNullOrEmpty(ctrFallback) ? OfficeNotFoundText : ctrFallback;
                }
                else
                {
                    var groups = entries.GroupBy(p => p.family);
                    var bestEntries = new List<string>();
                    //
                    foreach (var group in groups)
                    {
                        var items = group.ToList();
                        var licensedItems = items.Where(i => i.license.StartsWith(LicensedText)).Select(i => i.license).Distinct().ToList();
                        if (licensedItems.Any())
                        {
                            foreach (var lic in licensedItems)
                            {
                                bestEntries.Add($"{group.Key} ({lic})");
                            }
                            continue;
                        }
                        var graceItems = items.Where(i => i.license.StartsWith(GraceText)).Select(i => i.license).Distinct().ToList();
                        if (graceItems.Any())
                        {
                            foreach (var gr in graceItems)
                            {
                                bestEntries.Add($"{group.Key} ({gr})");
                            }
                            continue;
                        }
                        var notifItems = items.Where(i => i.license.StartsWith(NotificationsText)).Select(i => i.license).Distinct().ToList();
                        if (notifItems.Any())
                        {
                            foreach (var n in notifItems)
                            {
                                bestEntries.Add($"{group.Key} ({n})");
                            }
                            continue;
                        }
                        var otherItems = items.Select(i => i.license).Distinct();
                        foreach (var other in otherItems)
                        {
                            bestEntries.Add($"{group.Key} ({other})");
                        }
                    }
                    displayText = string.Join(", ", bestEntries);
                }
                OS_MSOfficeVersion_V.SetTextSafe(displayText);
            }
            catch (Exception ex)
            {
                if (debug_status) TSErrorLog.LogException(ex, "Ms_office_version()");
            }
        }
        // CLICK-TO-RUN PRODUCT LABEL FALLBACK (covers Microsoft 365 and Office 2016-2024 when OSPP reports nothing).
        private string GetClickToRunOfficeLabel()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office\ClickToRun\Configuration"))
                {
                    if (key == null) return "";
                    string ids = Convert.ToString(key.GetValue("ProductReleaseIds")) ?? "";
                    var tokens = ids.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Where(t => t.Length > 0).ToList();
                    if (!tokens.Any()) return "";
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    string other = software_lang.TSReadLangs("Os_Content", "os_c_office_other");
                    var labels = new List<string>();
                    foreach (var token in tokens)
                    {
                        string upper = token.ToUpperInvariant();
                        string label;
                        if (upper.Contains("O365") || upper.Contains("M365") || upper.Contains("365"))
                        {
                            if (upper.Contains("BUSINESS")) label = "Microsoft 365 Business";
                            else if (upper.Contains("HOMEPREM") || upper.Contains("FAMILY")) label = "Microsoft 365 Family";
                            else if (upper.Contains("EDU")) label = "Microsoft 365 Education";
                            else if (upper.Contains("PERSONAL")) label = "Microsoft 365 Personal";
                            else label = "Microsoft 365 Apps";
                        }
                        else if (upper.Contains("2026")) label = "Office 2026";
                        else if (upper.Contains("2024")) label = "Office 2024";
                        else if (upper.Contains("2021")) label = "Office 2021";
                        else if (upper.Contains("2019")) label = "Office 2019";
                        else if (upper.Contains("2016")) label = "Office 2016";
                        else if (upper.Contains("MONDO")) label = "Office Mondo";
                        else label = token;
                        string channel = "Retail";
                        if (upper.Contains("VOLUME")) channel = "Volume";
                        else if (upper.Contains("OEM")) channel = "OEM";
                        labels.Add($"{label} ({other} / {channel})");
                    }
                    return string.Join(", ", labels.Distinct());
                }
            }
            catch { return ""; }
        }
        // WIN LICENSE TYPE
        private async void StartWindowsLicenseTypeCheck()
        {
            try
            {
                await Win_license_type();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StartWindowsLicenseTypeCheck()"); }
            }
        }
        private async Task Win_license_type()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string licenseType = software_lang.TSReadLangs("Os_Content", "os_c_unknown");
                await Task.Run(() =>
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Description, LicenseStatus, PartialProductKey, Name FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                    using (ManagementObjectCollection results = searcher.Get())
                    {
                        string fallbackType = null;
                        foreach (ManagementObject obj in results.Cast<ManagementObject>())
                        {
                            using (obj)
                            {
                                string description = obj["Description"] != null ? obj["Description"].ToString() : string.Empty;
                                string name = obj["Name"] != null ? obj["Name"].ToString() : string.Empty;
                                if (string.IsNullOrWhiteSpace(description))
                                    continue;
                                if (!name.Contains("Windows"))
                                    continue;
                                string current;
                                if (description.IndexOf("RETAIL", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "Retail";
                                else if (description.IndexOf("OEM_DM", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "OEM: DM";
                                else if (description.IndexOf("OEM_COA", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "OEM: COA";
                                else if (description.IndexOf("OEM", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "OEM";
                                else if (description.IndexOf("VOLUME_MAK", StringComparison.OrdinalIgnoreCase) >= 0 || description.IndexOf("MAK", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "Volume: MAK";
                                else if (description.IndexOf("GVLK", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "GVLK";
                                else if (description.IndexOf("VOLUME_KMSCLIENT", StringComparison.OrdinalIgnoreCase) >= 0 || description.IndexOf("KMS", StringComparison.OrdinalIgnoreCase) >= 0)
                                    current = "Volume: KMS";
                                else
                                    current = description;
                                if (fallbackType == null) fallbackType = current;
                                int licStatus = -1;
                                try { if (obj["LicenseStatus"] != null) licStatus = Convert.ToInt32(obj["LicenseStatus"]); } catch { licStatus = -1; }
                                if (licStatus == 1)
                                {
                                    licenseType = current;
                                    break;
                                }
                            }
                        }
                        if (licenseType == software_lang.TSReadLangs("Os_Content", "os_c_unknown") && fallbackType != null)
                            licenseType = fallbackType;
                    }
                });
                OS_WinActiveChannel_V.SetTextSafe(licenseType);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Win_license_type()"); }
            }
        }
        // WIN LICENSE TIME
        private void WinLicenseTime()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string oi_year = software_lang.TSReadLangs("Os_Content", "os_c_year");
                string oi_month = software_lang.TSReadLangs("Os_Content", "os_c_month");
                string oi_day = software_lang.TSReadLangs("Os_Content", "os_c_day");
                string oi_hour = software_lang.TSReadLangs("Os_Content", "os_c_hour");
                string oi_then = software_lang.TSReadLangs("Os_Content", "os_c_then");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LicenseStatus, GracePeriodRemaining, PartialProductKey, Name FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results.Cast<ManagementObject>())
                    {
                        using (obj)
                        {
                            string entryName = obj["Name"] != null ? obj["Name"].ToString() : string.Empty;
                            if (entryName.IndexOf("Windows", StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                            int licenseStatus = obj["LicenseStatus"] != null ? Convert.ToInt32(obj["LicenseStatus"]) : -1;
                            int graceMinutes = obj["GracePeriodRemaining"] != null ? Convert.ToInt32(obj["GracePeriodRemaining"]) : 0;
                            if (licenseStatus != 1)
                            {
                                OS_WinLicenseTime_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_not_active"));
                                return;
                            }
                            if (graceMinutes == 0)
                            {
                                OS_WinLicenseTime_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_permanent"));
                                return;
                            }
                            DateTime now = DateTime.UtcNow;
                            DateTime expiry = now.AddMinutes(graceMinutes);
                            int years = expiry.Year - now.Year;
                            int months = expiry.Month - now.Month;
                            int days = expiry.Day - now.Day;
                            int hours = expiry.Hour - now.Hour;
                            if (hours < 0)
                            {
                                hours += 24;
                                days--;
                            }
                            if (days < 0)
                            {
                                months--;
                                days += DateTime.DaysInMonth(now.Year, now.Month);
                            }
                            if (months < 0)
                            {
                                months += 12;
                                years--;
                            }
                            string result = string.Join(" ", new[]{
                            years  > 0 ? $"{years} {oi_year}" : null,
                            months > 0 ? $"{months} {oi_month}" : null,
                            days   > 0 ? $"{days} {oi_day}" : null,
                            hours  > 0 ? $"{hours} {oi_hour}" : null
                        }.Where(x => x != null));
                            if (string.IsNullOrEmpty(result))
                            {
                                result = $"<1 {oi_hour}";
                            }
                            OS_WinLicenseTime_V.SetTextSafe($"{result} {oi_then}");
                            return;
                        }
                    }
                }
                OS_WinLicenseTime_V.SetTextSafe(software_lang.TSReadLangs("Os_Content", "os_c_unknown"));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "WinLicenseTime()"); }
            }
        }
        // BSOD TIME DYNAMIC
        private void StartBsodTimeDynamicCheck()
        {
            try
            {
                TrackBg(RunSec(() => Bsod_time_dynamic(_shutdownCts.Token)));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StartBsodTimeDynamicCheck()"); }
            }
        }
        private async Task Bsod_time_dynamic(CancellationToken shutdownToken)
        {
            try
            {
                var software_lang = new TSGetLangs(lang_path);
                DateTime last_bsod_x64 = Convert.ToDateTime(minidump_files_date_list[0]);
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    try
                    {
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
                        OS_BSODDate_V.SetTextSafe(bsodFinalText);
                        //
                        int msToNextSecond = 1000 - DateTime.Now.Millisecond;
                        if (msToNextSecond <= 0) msToNextSecond = 10;
                        await Task.Delay(msToNextSecond, shutdownToken);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "Bsod_time_dynamic()"); }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Bsod_time_dynamic()"); }
            }
        }
        // WINDOWS LICENSE & VERIFICATION URL CHECK MODULE
        static string RunSlmgrDlv(out int exitCode, out bool timedOut)
        {
            exitCode = -1;
            timedOut = false;
            Encoding oemEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            string windir = Environment.GetEnvironmentVariable("windir");
            if (string.IsNullOrWhiteSpace(windir))
                windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (string.IsNullOrWhiteSpace(windir))
                windir = Path.Combine(Program.Windows_disk, "Windows");
            string slmgrPath = Path.Combine(windir, "System32", "slmgr.vbs");
            ProcessStartInfo win_lic_url_listener = new ProcessStartInfo
            {
                FileName = Path.Combine(windir, "System32", "cscript.exe"),
                Arguments = "//Nologo \"" + slmgrPath + "\" /dlv",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = oemEncoding,
                StandardErrorEncoding = oemEncoding
            };
            try
            {
                using (Process p = Process.Start(win_lic_url_listener))
                {
                    if (p == null)
                        return string.Empty;
                    string stdout = p.StandardOutput.ReadToEnd();
                    string stderr = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(8000))
                    {
                        timedOut = true;
                        try { p.Kill(); } catch { }
                    }
                    else
                    {
                        exitCode = p.ExitCode;
                    }
                    return (stdout + "\n" + stderr).Trim();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        static List<string> ExtractHostsSLMGR(string text)
        {
            List<string> result = new List<string>();
            HashSet<string> unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(text))
                return result;
            string workingText = text;
            Regex urlLikeRegex = new Regex(@"\b(?<url>(?:https?://|www\.)[^\s""'<>]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in urlLikeRegex.Matches(workingText))
            {
                string raw = m.Groups["url"].Value.TrimEnd('.', ',', ';', ':', ')', ']', '}', '>', '"', '\'');
                string normalized = raw.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? "http://" + raw : raw;
                if (Uri.TryCreate(normalized, UriKind.Absolute, out Uri uri))
                {
                    AddHostSLMGR(uri.Host, unique, result);
                }
            }
            workingText = urlLikeRegex.Replace(workingText, " ");
            Regex hostRegex = new Regex(@"(?<![/\\@\w-])(?<host>(?:www\.)?(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,})(?::(?<port>\d{1,5}))?(?![\w-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach (Match m in hostRegex.Matches(workingText))
            {
                string host = m.Groups["host"].Value;
                AddHostSLMGR(host, unique, result);
            }
            Regex ipv4Regex = new Regex(@"(?<![\d.])(?<ip>(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(?:\.(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})(?![\d.])", RegexOptions.CultureInvariant);
            foreach (Match m in ipv4Regex.Matches(workingText))
            {
                AddHostSLMGR(m.Groups["ip"].Value, unique, result);
            }
            return result;
        }
        static void AddHostSLMGR(string host, HashSet<string> unique, List<string> result)
        {
            if (string.IsNullOrWhiteSpace(host))
                return;
            host = host.Trim().TrimEnd('.');
            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                host = host.Substring(4);
            if (unique.Add(host))
            {
                string checkHost = host;
                int colonIndex = checkHost.IndexOf(':');
                if (colonIndex > -1)
                    checkHost = checkHost.Substring(0, colonIndex);
                if (checkHost.IndexOf("microsoft.com", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    host += string.Format(" ({0})", software_lang.TSReadLangs("Os_Content", "os_c_official_server"));
                }
                result.Add(host);
            }
        }
        // MINIDIUMP FOLDER OPEN
        private void OS_MinidumpOpen_Click(object sender, EventArgs e)
        {
            try
            {
                // OPEN MINIDUMP FOLDER
                bool hasMinidumpFolder = Directory.Exists(mdp_1);
                bool hasMemoryDumpFile = File.Exists(mdp_2);
                bool hasMemoryDump = minidump_files_list.Any(f => string.Equals(Path.GetFileName(f), Path.GetFileName(mdp_2), StringComparison.OrdinalIgnoreCase));
                bool isFolderEmpty = true;
                if (hasMinidumpFolder)
                {
                    var files = Directory.GetFiles(mdp_1);
                    var directories = Directory.GetDirectories(mdp_1);
                    isFolderEmpty = (files.Length == 0 && directories.Length == 0);
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (hasMinidumpFolder && !isFolderEmpty)
                {
                    Process.Start("explorer.exe", mdp_1);
                }
                else if (hasMemoryDump && hasMemoryDumpFile)
                {
                    Process.Start("explorer.exe", $"/select,\"{mdp_2}\"");
                }
                else
                {
                    if (hasMinidumpFolder && isFolderEmpty)
                    {
                        TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_bsod_folder_open_error"));
                    }
                    else
                    {
                        TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_bsod_dump_file_open_error"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OS_MinidumpOpen_Click()"); }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_bsod_folder_open_error"));
            }
        }
        // BSOD ZIP COMPRESS
        private async void OS_BSoDZIP_Click(object sender, EventArgs e)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try
            {
                if (stealth_status == 1)
                {
                    DialogResult raw_warn = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("StealthMode", "sm_raw_export_warn"), "\n\n"));
                    if (raw_warn != DialogResult.Yes) { return; }
                }
                DialogResult pack_query = TS_MessageBoxEngine.TS_MessageBox(this, 5, software_lang.TSReadLangs("Os_Content", "os_c_bsod_query"));
                if (pack_query == DialogResult.Yes)
                {
                    string minidump_target_file = "GlowMinidumpTempFile";
                    if (Directory.Exists(minidump_target_file))
                    {
                        Directory.Delete(minidump_target_file, true);
                    }
                    Directory.CreateDirectory(minidump_target_file);
                    string minidump_zip_file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Application.ProductName + "_Minidump_" + GetExportHostName() + "_" + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + ".zip");
                    // COPY ASYNC
                    int currentFile = 0;
                    Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_progress_copy"), TS_VersionEngine.TS_SoftwareVersion(0));
                    foreach (string file_path in minidump_files_list)
                    {
                        string file_name = Path.GetFileName(file_path);
                        string target_file_path = Path.Combine(minidump_target_file, file_name);
                        await Task.Run(() => File.Copy(file_path, target_file_path, true));
                        currentFile++;
                    }
                    // ZIP ASYNC
                    Text = string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_progress_compress"), TS_VersionEngine.TS_SoftwareVersion(0));
                    await Task.Run(() =>
                    {
                        using (FileStream zipToOpen = new FileStream(minidump_zip_file, FileMode.Create))
                        {
                            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                            {
                                currentFile = 0;
                                foreach (string file_path in minidump_files_list)
                                {
                                    string file_name = Path.GetFileName(file_path);
                                    string target_file_path = Path.Combine(minidump_target_file, file_name);
                                    ZipArchiveEntry zip_entry = archive.CreateEntry(file_name, CompressionLevel.NoCompression);
                                    using (var entryStream = zip_entry.Open())
                                    using (var fileStream = new FileStream(target_file_path, FileMode.Open, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                    currentFile++;
                                }
                            }
                        }
                    });
                    if (Directory.Exists(minidump_target_file))
                    {
                        Directory.Delete(minidump_target_file, true);
                    }
                    Text = TS_VersionEngine.TS_SoftwareVersion(0);
                    DialogResult open_minidump_zip_target = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_success"), minidump_zip_file, "\n\n"));
                    if (open_minidump_zip_target == DialogResult.Yes)
                    {
                        string open_mdzt = string.Format("/select, \"{0}\"", minidump_zip_file.Trim().Replace("/", @"\"));
                        ProcessStartInfo psi = new ProcessStartInfo("explorer.exe", open_mdzt);
                        Process.Start(psi);
                    }
                }
            }
            catch (Exception ex)
            {
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_error"), "\n", ex.Message));
            }
        }
        // OPEN WALLPAPER
        private void OS_WallpaperOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string wallpaper_start_path = string.Format("/select, \"{0}\"", wp_rotate.Trim().Replace("/", @"\"));
                ProcessStartInfo psi = new ProcessStartInfo("explorer.exe", wallpaper_start_path);
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OS_WallpaperOpen_Click()"); }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_open_error"));
            }
        }
        // OPEN WALLPAPER PREVIEW
        private void OS_WallpaperPreview_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowWallpaperPreviewTool>("glow_wallpaper_preview_tool", "wpt_name");
        }
        // OPEN WINDOWS RECOVERY MODE
        private void OS_RecoveryModeBtn_Click(object sender, EventArgs e)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                DialogResult recovery_check = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("Os_Content", "os_c_recovery_mode_msg"), "\n\n", "\n\n"));
                if (recovery_check == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/r /o /f /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OS_RecoveryModeBtn_Click()"); }
            }
        }
        #endregion
        #region MB_Section
        // MB
        // ======================================================================================================
        private void Mb()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            using (ManagementObjectSearcher search_bb = new ManagementObjectSearcher("root\\CIMV2", "SELECT Product, Manufacturer, SerialNumber FROM Win32_BaseBoard"))
            using (ManagementObjectSearcher search_cs = new ManagementObjectSearcher("root\\CIMV2", "SELECT SystemFamily, SystemSKUNumber FROM Win32_ComputerSystem"))
            using (ManagementObjectSearcher search_bios = new ManagementObjectSearcher("root\\CIMV2", "SELECT Manufacturer, ReleaseDate, Caption, SMBIOSMajorVersion, SMBIOSMinorVersion, SerialNumber FROM Win32_BIOS"))
            using (ManagementObjectSearcher search_tpm = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftTpm", "SELECT IsActivated_InitialValue, SpecVersion, PhysicalPresenceVersionInfo, ManufacturerIdTxt, ManufacturerId, ManufacturerVersion, ManufacturerVersionFull20, ManufacturerVersionInfo FROM Win32_Tpm"))
            {
            using (var bbCol = search_bb.Get())
            {
                foreach (ManagementObject query_bb_rotate in bbCol.Cast<ManagementObject>())
                {
                    using (query_bb_rotate)
                    {
                        try
                        {
                            // MB NAME
                            MB_MotherBoardName_V.SetTextSafe(Convert.ToString(query_bb_rotate["Product"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // MB MAN
                            MB_MotherBoardMan_V.SetTextSafe(Convert.ToString(query_bb_rotate["Manufacturer"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // MB SERIAL
                            string mb_serial = Convert.ToString(query_bb_rotate["SerialNumber"]).Trim();
                            if (!string.IsNullOrEmpty(mb_serial))
                            {
                                if (stealth_status != 1)
                                {
                                    MB_MotherBoardSerial_V.SetTextSafe(mb_serial);
                                }
                                else
                                {
                                    MB_MotherBoardSerial_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                MB_MotherBoardSerial_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                    }
                }
            }
            try
            {
                using (RegistryKey get_bios_data = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS"))
                {
                    if (get_bios_data == null)
                    {
                        MB_SystemModelMan_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                        MB_SystemModelFamily_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                        MB_SystemModel_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                    }
                    else if (!string.IsNullOrEmpty(get_bios_data.ToString()))
                    {
                        // SYSTEM MODEL MANUFACTURER
                        try
                        {
                            string systemManufacturer = Convert.ToString(get_bios_data.GetValue("SystemManufacturer"));
                            if (!string.IsNullOrEmpty(systemManufacturer))
                            {
                                MB_SystemModelMan_V.SetTextSafe(systemManufacturer.ToString().Trim());
                            }
                            else
                            {
                                MB_SystemModelMan_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        // SYSTEM MODEL FAMILY
                        try
                        {
                            string systemFamily = Convert.ToString(get_bios_data.GetValue("SystemFamily"));
                            if (!string.IsNullOrEmpty(systemFamily))
                            {
                                MB_SystemModelFamily_V.SetTextSafe(systemFamily.ToString().Trim());
                            }
                            else
                            {
                                MB_SystemModelFamily_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        // SYSTEM MODEL
                        try
                        {
                            string systemProductName = Convert.ToString(get_bios_data.GetValue("SystemProductName"));
                            if (!string.IsNullOrEmpty(systemProductName))
                            {
                                MB_SystemModel_V.SetTextSafe(systemProductName.ToString().Trim());
                            }
                            else
                            {
                                MB_SystemModel_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                    }
                    else
                    {
                        MB_SystemModelMan_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                        MB_SystemModelFamily_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                        MB_SystemModel_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            using (var csCol = search_cs.Get())
            {
                foreach (ManagementObject query_cs in csCol.Cast<ManagementObject>())
                {
                    using (query_cs)
                    {
                        // SYSTEM FAMILY
                        try
                        {
                            string system_family = Convert.ToString(query_cs["SystemFamily"])?.Trim() ?? string.Empty;
                            string system_family_check = system_family.ToLower();
                            if (string.IsNullOrEmpty(system_family_check) || system_family_check == "default string" || system_family_check == "to be filled by o.e.m." || system_family_check == "not specified" || system_family_check == "not available" || system_family_check == "none")
                            {
                                MB_SystemFamily_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                            else
                            {
                                MB_SystemFamily_V.SetTextSafe(system_family);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        // SYSTEM SKU
                        try
                        {
                            string system_sku = Convert.ToString(query_cs["SystemSKUNumber"])?.Trim() ?? string.Empty;
                            string system_sku_check = system_sku.ToLower();
                            bool skuPlaceholder = string.IsNullOrEmpty(system_sku_check) || system_sku_check == "default string" || system_sku_check == "sku" || system_sku_check == "to be filled by o.e.m." || system_sku_check == "not specified" || system_sku_check == "not available" || system_sku_check == "none" || system_sku_check.Trim('0').Length == 0;
                            if (!skuPlaceholder)
                            {
                                if (stealth_status != 1)
                                {
                                    MB_SystemSKU_V.SetTextSafe(system_sku);
                                }
                                else
                                {
                                    MB_SystemSKU_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                MB_SystemSKU_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                    }
                }
            }
            using (var biosCol = search_bios.Get())
            {
                foreach (ManagementObject query_bios_rotate in biosCol.Cast<ManagementObject>())
                {
                    using (query_bios_rotate)
                    {
                        try
                        {
                            // BIOS MAN
                            MB_BiosManufacturer_V.SetTextSafe(Convert.ToString(query_bios_rotate["Manufacturer"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // BIOS DATE
                            string bios_date = Convert.ToString(query_bios_rotate["ReleaseDate"]);
                            DateTime bios_date_last = ManagementDateTimeConverter.ToDateTime(bios_date);
                            MB_BiosDate_V.SetTextSafe($"{bios_date_last:dd.MM.yyyy}");
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // BIOS VERSION
                            MB_BiosVersion_V.SetTextSafe(Convert.ToString(query_bios_rotate["Caption"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // SM-BIOS MAJOR MINOR
                            object sm_bios_major = query_bios_rotate["SMBIOSMajorVersion"];
                            object sm_bios_minor = query_bios_rotate["SMBIOSMinorVersion"];
                            MB_SmBiosVersion_V.SetTextSafe(sm_bios_major.ToString() + "." + sm_bios_minor.ToString());
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                        try
                        {
                            // BIOS SERIAL NUMBER
                            string mb_device_serial = Convert.ToString(query_bios_rotate["SerialNumber"]);
                            if (!string.IsNullOrEmpty(mb_device_serial))
                            {
                                if (stealth_status != 1)
                                {
                                    MB_DeviceSerialNumber_V.SetTextSafe(mb_device_serial);
                                }
                                else
                                {
                                    MB_DeviceSerialNumber_V.SetTextSafe(sm_text);
                                }
                            }
                            else
                            {
                                MB_DeviceSerialNumber_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                        }
                    }
                }
            }
            try
            {
                // MB SECURE BOOT
                object sbValue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State", "UEFISecureBootEnabled", null);
                if (sbValue == null)
                {
                    MB_SecureBoot_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected"));
                }
                else
                {
                    bool mb_secure_boot_status = Convert.ToBoolean(sbValue);
                    if (mb_secure_boot_status == true)
                    {
                        MB_SecureBoot_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_on"));
                    }
                    else if (mb_secure_boot_status == false)
                    {
                        MB_SecureBoot_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_off"));
                    }
                }
            }
            catch (Exception ex)
            {
                try { MB_SecureBoot_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_not_detected")); } catch { }
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            try
            {
                using (var tpmCol = search_tpm.Get())
                {
                    foreach (ManagementObject query_tpm in tpmCol.Cast<ManagementObject>())
                    {
                        using (query_tpm)
                        {
                            try
                            {
                                // TPM VERSION
                                bool tpm_status = Convert.ToBoolean(query_tpm["IsActivated_InitialValue"]);
                                string tpm_version = Convert.ToString(query_tpm["SpecVersion"]) ?? "";
                                if (tpm_status == true && !string.IsNullOrEmpty(tpm_version))
                                {
                                    MB_TPMStatus_V.SetTextSafe(string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_active"), tpm_version.Split(',')[0].Trim()));
                                }
                                else
                                {
                                    MB_TPMStatus_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM PPI VERSION
                                string tpm_ppi_version = Convert.ToString(query_tpm["PhysicalPresenceVersionInfo"]).Trim();
                                if (!string.IsNullOrEmpty(tpm_ppi_version))
                                {
                                    MB_TPMPPIVersion_V.SetTextSafe(tpm_ppi_version);
                                }
                                else
                                {
                                    MB_TPMPPIVersion_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM PPI ALT VERSION
                                bool tpm_status = Convert.ToBoolean(query_tpm["IsActivated_InitialValue"]);
                                string tpm_version = Convert.ToString(query_tpm["SpecVersion"]) ?? "";
                                if (tpm_status == true && !string.IsNullOrEmpty(tpm_version))
                                {
                                    string[] parts = tpm_version.Split(',');
                                    if (parts.Length >= 3)
                                    {
                                        MB_TPMPPIAltVersion_V.SetTextSafe(parts[2].Trim());
                                    }
                                    else
                                    {
                                        MB_TPMPPIAltVersion_V.SetTextSafe("N/A");
                                    }
                                }
                                else
                                {
                                    MB_TPMPPIAltVersion_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM MANUFACTURER ID TXT
                                string tpm_man_id_txt = Convert.ToString(query_tpm["ManufacturerIdTxt"])?.Trim() ?? string.Empty;
                                var tpmManMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
                        { "INTC", "Intel" },
                        { "IFX", "Infineon" },
                        { "STM", "STMicroelectronics" },
                        { "NTC", "Nuvoton" },
                        { "AMD", "AMD" },
                        { "ATML", "Atmel (Microchip)" },
                        { "SMSC", "SMSC (Microchip)" },
                        { "QCOM", "Qualcomm" }
                    };
                                if (!string.IsNullOrEmpty(tpm_man_id_txt))
                                {
                                    if (tpmManMap.TryGetValue(tpm_man_id_txt, out string tpmManName))
                                    {
                                        MB_TPMMan_V.SetTextSafe(string.Format("{0} ({1})", tpmManName, tpm_man_id_txt));
                                    }
                                    else
                                    {
                                        MB_TPMMan_V.SetTextSafe(tpm_man_id_txt);
                                    }
                                }
                                else
                                {
                                    MB_TPMMan_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM MANUFACTURER ID
                                string tpm_man_id = Convert.ToString(query_tpm["ManufacturerId"]).Trim();
                                if (!string.IsNullOrEmpty(tpm_man_id))
                                {
                                    if (stealth_status != 1)
                                    {
                                        MB_TPMManID_V.SetTextSafe(tpm_man_id);
                                    }
                                    else
                                    {
                                        MB_TPMManID_V.SetTextSafe(sm_text);
                                    }
                                }
                                else
                                {
                                    MB_TPMManID_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM MANUFACTURER VERSION
                                string tpm_man_version = Convert.ToString(query_tpm["ManufacturerVersion"]).Trim();
                                if (!string.IsNullOrEmpty(tpm_man_version))
                                {
                                    MB_TPMManVersion_V.SetTextSafe(tpm_man_version);
                                }
                                else
                                {
                                    MB_TPMManVersion_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM MANUFACTURER VERSION FULL
                                string tpm_man_version_full = Convert.ToString(query_tpm["ManufacturerVersionFull20"]).Trim();
                                if (!string.IsNullOrEmpty(tpm_man_version_full))
                                {
                                    MB_TPMManFullVersion_V.SetTextSafe(tpm_man_version_full);
                                }
                                else
                                {
                                    MB_TPMManFullVersion_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                            try
                            {
                                // TPM MANUFACTURER INFO
                                string tpm_man_version_info = Convert.ToString(query_tpm["ManufacturerVersionInfo"]).Trim();
                                if (!string.IsNullOrEmpty(tpm_man_version_info))
                                {
                                    MB_TPMManPublisher_V.SetTextSafe(tpm_man_version_info);
                                }
                                else
                                {
                                    MB_TPMManPublisher_V.SetTextSafe(software_lang.TSReadLangs("Mb_Content", "mb_c_tpm_off"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Mb() -> TPM Query"); }
            }
            // TPM MODE CHECK WRAPPER
            try
            {
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
                foreach (var tpm_control in tpm_controls)
                {
                    tpm_control.ExecuteSafe(() =>
                    {
                        if (tpm_control.Text == "N/A" || string.IsNullOrEmpty(tpm_control.Text))
                        {
                            tpm_control.Text = tpmOffMessage;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            try
            {
                // LAST BIOS TIME | POSTTime
                int last_bios_time = Convert.ToInt32(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "FwPOSTTime", ""));
                string last_bios_time_x64 = string.Format("{0:0.0}", TimeSpan.FromMilliseconds(last_bios_time).TotalSeconds).Replace(",", ".");
                MB_LastBIOSTime_V.SetTextSafe(string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_boot_time"), last_bios_time_x64));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Mb()"); }
            }
            // MB PROCESS END ENABLED
            MB_RotateBtn.SetEnabledSafe(true);
            MB.SetEnabledSafe(true);
            MB_BIOSUpdateBtn.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- Motherboard Section Loaded --->");
            }
        }
        private void MB_BIOSUpdateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string bios_prompt = "";
                if (laptop_mode == true)
                {
                    bios_prompt = string.Format("{0} {1} {2}", MB_SystemModelMan_V.Text.Trim(), MB_SystemModel_V.Text.Trim(), "BIOS Update");
                }
                else if (laptop_mode == false)
                {
                    bios_prompt = string.Format("{0} {1} {2}", MB_MotherBoardMan_V.Text.Trim(), MB_MotherBoardName_V.Text.Trim(), "BIOS Update");
                }
                string search_browser_q = $"https://www.google.com/search?q={Uri.EscapeDataString(bios_prompt)}";
                Process.Start(new ProcessStartInfo(search_browser_q) { UseShellExecute = true });
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "MB_BIOSUpdateBtn_Click()"); } }
        }
        private void MB_BIOSOpenBtn_Click(object sender, EventArgs e)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (MB_BiosMode_V.Text == "N/A")
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_not_set"));
                    return;
                }
                if (!MB_BiosMode_V.Text.Equals("UEFI", StringComparison.OrdinalIgnoreCase))
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_uefi_check"));
                    return;
                }
                DialogResult check_bios_open = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("Mb_Content", "mb_c_bios_open_check"), "\n\n", "\n\n"));
                if (check_bios_open == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        Arguments = "/r /fw /t 0",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }
            catch (Exception ex)
            {
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
        readonly List<double> cpu_base_speed_list = new List<double>();
        readonly List<string> cpu_current_speed_list = new List<string>();
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
        private static string _cachedChipset = null;
        private static string DetectChipsetFromPci()
        {
            if (_cachedChipset != null) return _cachedChipset;
            try
            {
                string bestVen = null;
                int bestDev = -1, bestPri = 99;
                bool sparkPlatform = false;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, PNPDeviceID, HardwareID, CompatibleID FROM Win32_PnPEntity WHERE PNPDeviceID LIKE 'PCI%'"))
                using (var pciCol = searcher.Get())
                {
                    foreach (ManagementObject mo in pciCol.Cast<ManagementObject>())
                    {
                        using (mo)
                        {
                            try
                            {
                                string pnp = Convert.ToString(mo["PNPDeviceID"]) ?? string.Empty;
                                try
                                {
                                    Match svM = Regex.Match(pnp, @"VEN_([0-9A-Fa-f]{4})");
                                    Match sdM = Regex.Match(pnp, @"DEV_([0-9A-Fa-f]{4})");
                                    if (svM.Success && sdM.Success && svM.Groups[1].Value.Equals("10DE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int sd = Convert.ToInt32(sdM.Groups[1].Value, 16);
                                        if (sd == 0x22CE || sd == 0x2E12) sparkPlatform = true;
                                    }
                                }
                                catch { }
                                var ids = new List<string>();
                                try
                                {
                                    if (mo["HardwareID"] is string[] hw) ids.AddRange(hw);
                                    else if (mo["HardwareID"] != null) ids.Add(mo["HardwareID"].ToString());
                                }
                                catch { }
                                try
                                {
                                    if (mo["CompatibleID"] is string[] cp) ids.AddRange(cp);
                                    else if (mo["CompatibleID"] != null) ids.Add(mo["CompatibleID"].ToString());
                                }
                                catch { }
                                string allIds = string.Join(" ", ids);
                                int pri = allIds.IndexOf("CC_0601", StringComparison.OrdinalIgnoreCase) >= 0 ? 0
                                    : allIds.IndexOf("CC_0C05", StringComparison.OrdinalIgnoreCase) >= 0 ? 1
                                    : allIds.IndexOf("CC_0600", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 : -1;
                                if (pri < 0 || pri >= bestPri) continue;
                                Match venM = Regex.Match(pnp, @"VEN_([0-9A-Fa-f]{4})");
                                Match devM = Regex.Match(pnp, @"DEV_([0-9A-Fa-f]{4})");
                                if (!venM.Success || !devM.Success) continue;
                                bestPri = pri;
                                bestVen = venM.Groups[1].Value.ToUpperInvariant();
                                bestDev = Convert.ToInt32(devM.Groups[1].Value, 16);
                                if (bestPri == 0) break;
                            }
                            catch { continue; }
                        }
                    }
                }
                string result = MapPciChipset(bestVen, bestDev);
                if (string.IsNullOrEmpty(result) && sparkPlatform) result = "NVIDIA GB10 Grace Blackwell (DGX Spark)";
                _cachedChipset = result ?? string.Empty;
                return _cachedChipset;
            }
            catch { return string.Empty; }
        }
        // Chipset PCI ID data source: https://pci-ids.ucw.cz/ (pci.ids, version 2026.09.25)
        // Raw file: https://github.com/pciutils/pciids/blob/master/pci.ids
        private static string MapPciChipset(string venHex, int dev)
        {
            if (string.IsNullOrEmpty(venHex) || dev < 0) return null;
            var vendors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
                { "8086", "Intel" }, { "1022", "AMD" }, { "1002", "AMD" }, { "10DE", "NVIDIA" },
                { "1039", "SiS" }, { "1106", "VIA" }, { "1B21", "ASMedia" },
                { "17CB", "Qualcomm" }, { "14C3", "MediaTek" }, { "106B", "Apple" }
            };
            if (!vendors.TryGetValue(venHex, out string vendor)) return null;
            if (venHex.Equals("8086", StringComparison.OrdinalIgnoreCase))
            {
                // Only currently used IDs are included (Sandy Bridge / 2011 and newer).
                var intelMap = new Dictionary<int, string>{
                    { 0x1C40, "Intel 6 Series/C200" }, { 0x1C41, "Intel 6 Series Mobile SFF" }, { 0x1C42, "Intel 6 Series/C200" },
                    { 0x1C43, "Intel Mobile 6 Series" }, { 0x1C44, "Intel Z68" }, { 0x1C45, "Intel 6 Series/C200" },
                    { 0x1C46, "Intel P67" }, { 0x1C47, "Intel UM67" }, { 0x1C48, "Intel 6 Series/C200" },
                    { 0x1C49, "Intel HM65" }, { 0x1C4A, "Intel H67" }, { 0x1C4B, "Intel HM67" }, { 0x1C4C, "Intel Q65" },
                    { 0x1C4D, "Intel QS67" }, { 0x1C4E, "Intel Q67" }, { 0x1C4F, "Intel QM67" }, { 0x1C50, "Intel B65" },
                    { 0x1C51, "Intel 6 Series/C200" }, { 0x1C52, "Intel C202" }, { 0x1C53, "Intel 6 Series/C200" },
                    { 0x1C54, "Intel C204" }, { 0x1C55, "Intel 6 Series/C200" }, { 0x1C56, "Intel C206" },
                    { 0x1C57, "Intel 6 Series/C200" }, { 0x1C58, "Intel B65" }, { 0x1C59, "Intel HM67" },
                    { 0x1C5A, "Intel Q67" }, { 0x1C5B, "Intel 6 Series/C200" }, { 0x1C5C, "Intel H61" },
                    { 0x1C5D, "Intel 6 Series/C200" }, { 0x1C5E, "Intel 6 Series/C200" }, { 0x1C5F, "Intel 6 Series/C200" },
                    { 0x1D40, "Intel X79 / C600" }, { 0x1D41, "Intel X79 / C600" },
                    { 0x1E41, "Intel 7 Series" }, { 0x1E42, "Intel 7 Series" }, { 0x1E43, "Intel 7 Series" },
                    { 0x1E44, "Intel Z77" }, { 0x1E45, "Intel 7 Series" }, { 0x1E46, "Intel Z75" },
                    { 0x1E47, "Intel Q77" }, { 0x1E48, "Intel Q75" }, { 0x1E49, "Intel B75" }, { 0x1E4A, "Intel H77" },
                    { 0x1E4B, "Intel 7 Series" }, { 0x1E4C, "Intel 7 Series" }, { 0x1E4D, "Intel 7 Series" },
                    { 0x1E4E, "Intel 7 Series" }, { 0x1E4F, "Intel 7 Series" }, { 0x1E50, "Intel 7 Series" },
                    { 0x1E51, "Intel 7 Series" }, { 0x1E52, "Intel 7 Series" }, { 0x1E53, "Intel C216" },
                    { 0x1E54, "Intel 7 Series" }, { 0x1E55, "Intel QM77" }, { 0x1E56, "Intel QS77" },
                    { 0x1E57, "Intel HM77" }, { 0x1E58, "Intel UM77" }, { 0x1E59, "Intel HM76" },
                    { 0x1E5A, "Intel 7 Series" }, { 0x1E5B, "Intel UM77" }, { 0x1E5C, "Intel 7 Series" },
                    { 0x1E5D, "Intel HM75" }, { 0x1E5E, "Intel HM70" }, { 0x1E5F, "Intel NM70" },
                    { 0x8C40, "Intel 8 Series/C220" }, { 0x8C41, "Intel 8 Series Mobile" }, { 0x8C42, "Intel 8 Series/C220" },
                    { 0x8C43, "Intel 8 Series/C220" }, { 0x8C44, "Intel Z87" }, { 0x8C45, "Intel 8 Series/C220" },
                    { 0x8C46, "Intel Z85" }, { 0x8C47, "Intel 8 Series/C220" }, { 0x8C48, "Intel 8 Series/C220" },
                    { 0x8C49, "Intel HM86" }, { 0x8C4A, "Intel H87" }, { 0x8C4B, "Intel HM87" },
                    { 0x8C4C, "Intel Q85" }, { 0x8C4D, "Intel 8 Series/C220" }, { 0x8C4E, "Intel Q87" },
                    { 0x8C4F, "Intel QM87" }, { 0x8C50, "Intel B85" }, { 0x8C51, "Intel 8 Series/C220" },
                    { 0x8C52, "Intel C222" }, { 0x8C53, "Intel 8 Series/C220" }, { 0x8C54, "Intel C224" },
                    { 0x8C55, "Intel 8 Series/C220" }, { 0x8C56, "Intel C226" }, { 0x8C57, "Intel 8 Series/C220" },
                    { 0x8C58, "Intel 8 Series WS" }, { 0x8C59, "Intel 8 Series/C220" }, { 0x8C5A, "Intel 8 Series/C220" },
                    { 0x8C5B, "Intel 8 Series/C220" }, { 0x8C5C, "Intel H81" }, { 0x8C5D, "Intel 8 Series/C220" },
                    { 0x8C5E, "Intel 8 Series/C220" }, { 0x8C5F, "Intel 8 Series/C220" },
                    { 0x8D40, "Intel X99 / C610" }, { 0x8D41, "Intel X99 / C610" }, { 0x8D42, "Intel X99 / C610" },
                    { 0x8D43, "Intel X99 / C610" }, { 0x8D44, "Intel X99 / C610" }, { 0x8D45, "Intel X99 / C610" },
                    { 0x8D46, "Intel X99 / C610" }, { 0x8D47, "Intel X99 / C610" }, { 0x8D48, "Intel X99 / C610" },
                    { 0x8D49, "Intel X99 / C610" }, { 0x8D4A, "Intel X99 / C610" }, { 0x8D4B, "Intel X99 / C610" },
                    { 0x8D4C, "Intel X99 / C610" }, { 0x8D4D, "Intel X99 / C610" }, { 0x8D4E, "Intel X99 / C610" },
                    { 0x8D4F, "Intel X99 / C610" },
                    { 0x9C40, "Intel 8 Series (Mobile)" }, { 0x9C41, "Intel 8 Series (Mobile)" },
                    { 0x9C42, "Intel 8 Series (Mobile)" }, { 0x9C43, "Intel 8 Series (Mobile)" },
                    { 0x9C44, "Intel 8 Series (Mobile)" }, { 0x9C45, "Intel 8 Series (Mobile)" },
                    { 0x9C46, "Intel 8 Series (Mobile)" }, { 0x9C47, "Intel 8 Series (Mobile)" },
                    { 0x8CC1, "Intel 9 Series" }, { 0x8CC2, "Intel 9 Series" }, { 0x8CC3, "Intel HM97" },
                    { 0x8CC4, "Intel Z97" }, { 0x8CC5, "Intel QM97" }, { 0x8CC6, "Intel H97" },
                    { 0x9CC1, "Intel 9 Series (Mobile)" }, { 0x9CC2, "Intel 9 Series (Mobile)" },
                    { 0x9CC3, "Intel 9 Series (Mobile)" }, { 0x9CC5, "Intel 9 Series (Mobile)" },
                    { 0x9CC6, "Intel 9 Series (Mobile)" }, { 0x9CC7, "Intel 9 Series (Mobile)" },
                    { 0x9CC9, "Intel 9 Series (Mobile)" },
                    { 0x0F12, "Intel Bay Trail SoC (SMBus)" }, { 0x19DC, "Intel Atom C3000 SoC" },
                    { 0x2292, "Intel Braswell SoC (SMBus)" },
                    { 0x1B81, "Intel C740 (Emmitsburg)" },
                    { 0x5AE8, "Intel Apollo Lake" }, { 0x31E8, "Intel Gemini Lake" },
                    { 0x4B00, "Intel Elkhart Lake" }, { 0x4D87, "Intel Jasper Lake" },
                    { 0x9D43, "Intel 100 Series (Mobile)" }, { 0x9D46, "Intel 100 Series (Mobile)" },
                    { 0x9D48, "Intel 100 Series (Mobile)" }, { 0x9D4E, "Intel 100 Series (Mobile)" },
                    { 0x9D50, "Intel 100 Series (Mobile)" }, { 0x9D56, "Intel 100 Series (Mobile)" },
                    { 0x9D58, "Intel 100 Series (Mobile)" },
                    { 0x9D84, "Intel 200 Series (Mobile, Cannon Point-LP)" },
                    { 0xA140, "Intel 100 Series" }, { 0xA141, "Intel 100 Series" }, { 0xA142, "Intel 100 Series" },
                    { 0xA143, "Intel H110" }, { 0xA144, "Intel H170" }, { 0xA145, "Intel Z170" },
                    { 0xA146, "Intel Q170" }, { 0xA147, "Intel Q150" }, { 0xA148, "Intel B150" },
                    { 0xA149, "Intel C236" }, { 0xA14A, "Intel C232" }, { 0xA14B, "Intel 100 Series" },
                    { 0xA14C, "Intel 100 Series" }, { 0xA14D, "Intel QM170" }, { 0xA14E, "Intel HM170" },
                    { 0xA14F, "Intel 100 Series" },
                    { 0xA150, "Intel CM236" }, { 0xA151, "Intel 100 Series" }, { 0xA152, "Intel HM175" },
                    { 0xA153, "Intel QM175" }, { 0xA154, "Intel CM238" }, { 0xA155, "Intel 100 Series" },
                    { 0xA156, "Intel 100 Series" }, { 0xA157, "Intel 100 Series" }, { 0xA158, "Intel 100 Series" },
                    { 0xA159, "Intel 100 Series" }, { 0xA15A, "Intel 100 Series" }, { 0xA15B, "Intel 100 Series" },
                    { 0xA15C, "Intel 100 Series" }, { 0xA15D, "Intel 100 Series" }, { 0xA15E, "Intel 100 Series" },
                    { 0xA15F, "Intel 100 Series" },
                    { 0xA1C1, "Intel C621" }, { 0xA1C2, "Intel C622" }, { 0xA1C3, "Intel C624" },
                    { 0xA1C4, "Intel C625" }, { 0xA1C5, "Intel C626" }, { 0xA1C6, "Intel C627" },
                    { 0xA1C7, "Intel C628" }, { 0xA1CA, "Intel C629" },
                    { 0xA242, "Intel C624 (Super)" }, { 0xA243, "Intel C627 (Super)" }, { 0xA244, "Intel C621 (Super)" },
                    { 0xA245, "Intel C627 (Super)" }, { 0xA246, "Intel C628 (Super)" },
                    { 0xA2C4, "Intel H270" }, { 0xA2C5, "Intel Z270" }, { 0xA2C6, "Intel Q270" },
                    { 0xA2C7, "Intel Q250" }, { 0xA2C8, "Intel B250" }, { 0xA2C9, "Intel Z370" },
                    { 0xA2D2, "Intel X299" }, { 0xA2D3, "Intel C422" },
                    { 0xA303, "Intel H310" }, { 0xA304, "Intel H370" }, { 0xA305, "Intel Z390" },
                    { 0xA306, "Intel Q370" }, { 0xA307, "Intel Q360" }, { 0xA308, "Intel B360" },
                    { 0xA309, "Intel C246" }, { 0xA30A, "Intel C242" }, { 0xA30C, "Intel QM370" },
                    { 0xA30D, "Intel HM370" }, { 0xA30E, "Intel CM246" },
                    { 0xA313, "Intel Cannon Lake" }, { 0xA3C8, "Intel B460" }, { 0xA3DA, "Intel H410" },
                    { 0x0284, "Intel 400 Series (Mobile)" }, { 0x0285, "Intel 400 Series (Mobile)" },
                    { 0x0684, "Intel H470" }, { 0x0685, "Intel Z490" }, { 0x0687, "Intel Q470" },
                    { 0x068C, "Intel QM480" }, { 0x068D, "Intel HM470" }, { 0x068E, "Intel WM490" },
                    { 0x0697, "Intel W480" },
                    { 0x3482, "Intel 400 Series (Mobile, Ice Lake-LP)" },
                    { 0x3882, "Intel Ice Lake (Server)" },
                    { 0xA082, "Intel 500 Series (Mobile, Tiger Lake-LP)" },
                    { 0x4384, "Intel Q570" }, { 0x4385, "Intel Z590" }, { 0x4386, "Intel H570" },
                    { 0x4387, "Intel B560" }, { 0x4388, "Intel H510" }, { 0x4389, "Intel WM590" },
                    { 0x438A, "Intel QM580" }, { 0x438B, "Intel HM570" }, { 0x438C, "Intel C252" },
                    { 0x438D, "Intel C256" }, { 0x438E, "Intel H310D" }, { 0x438F, "Intel W580" },
                    { 0x4390, "Intel RM590E" }, { 0x4391, "Intel R580E" },
                    { 0x5181, "Intel Alder Lake-P" }, { 0x5182, "Intel Alder Lake-P" },
                    { 0x5187, "Intel Alder Lake" }, { 0x519D, "Intel Raptor Lake" },
                    { 0x5481, "Intel Alder Lake-N" },
                    { 0x5795, "Intel Xeon 6 P-Core (Granite Rapids)" },
                    { 0x7A04, "Intel Z790" }, { 0x7A05, "Intel H770" }, { 0x7A06, "Intel B760" },
                    { 0x7A0C, "Intel HM770" }, { 0x7A0D, "Intel WM790" }, { 0x7A13, "Intel C266" },
                    { 0x7A14, "Intel C262" }, { 0x7A83, "Intel Q670" },
                    { 0x7A84, "Intel Z690" }, { 0x7A85, "Intel H670" }, { 0x7A86, "Intel B660" },
                    { 0x7A87, "Intel H610" }, { 0x7A88, "Intel W680" }, { 0x7A8A, "Intel W790" },
                    { 0x7A8C, "Intel HM670" }, { 0x7A8D, "Intel WM690" },
                    { 0x7702, "Intel Arrow Lake-H" }, { 0x7730, "Intel Arrow Lake-H" },
                    { 0x7202, "Intel Core Ultra 200H" }, { 0x7203, "Intel Core Ultra 200V" },
                    { 0xAE10, "Intel Arrow Lake-HX" },
                    { 0x7E01, "Intel Meteor Lake-P" }, { 0x7E02, "Intel Meteor Lake-H" },
                    { 0xA806, "Intel Lunar Lake-M" }, { 0xA807, "Intel Core Ultra 200V" },
                    { 0x7F03, "Intel Q870" }, { 0x7F04, "Intel Z890" }, { 0x7F06, "Intel B860" },
                    { 0x7F07, "Intel H810" }, { 0x7F08, "Intel W880" }, { 0x7F0C, "Intel HM870" },
                    { 0x7F0D, "Intel WM880" },
                    { 0xE300, "Intel Panther Lake" }, { 0xE302, "Intel Panther Lake" },
                    { 0xE31F, "Intel Panther Lake" }, { 0xE400, "Intel Panther Lake" },
                    { 0xE402, "Intel Panther Lake" }, { 0xE41F, "Intel Panther Lake" }
                };
                if (intelMap.TryGetValue(dev, out string intelName)) return intelName;
                return string.Format("{0} (ID {1:X4})", vendor, dev);
            }
            if (venHex.Equals("1022", StringComparison.OrdinalIgnoreCase) || venHex.Equals("1002", StringComparison.OrdinalIgnoreCase))
            {
                // Only currently used IDs are included.
                var amdMap = new Dictionary<int, string>{
                    { 0x780E, "AMD FCH" }, { 0x790B, "AMD FCH (SMBus)" }, { 0x790E, "AMD FCH" }
                };
                if (amdMap.TryGetValue(dev, out string amdName)) return amdName;
                return string.Format("{0} (ID {1:X4})", vendor, dev);
            }
            if (venHex.Equals("10DE", StringComparison.OrdinalIgnoreCase))
            {
                // Only currently used IDs are included.
                var nvMap = new Dictionary<int, string>{
                    { 0x22CE, "NVIDIA GB10 Grace Blackwell (DGX Spark)" },
                    { 0x2E12, "NVIDIA GB10 Grace Blackwell (DGX Spark)" }
                };
                if (nvMap.TryGetValue(dev, out string nvName)) return nvName;
                return vendor;
            }
            if (venHex.Equals("1039", StringComparison.OrdinalIgnoreCase)) return vendor;
            if (venHex.Equals("1106", StringComparison.OrdinalIgnoreCase)) return vendor;
            if (venHex.Equals("1B21", StringComparison.OrdinalIgnoreCase)) return "ASMedia";
            return vendor;
        }
        [DllImport("kernel32.dll")]
        private static extern bool IsProcessorFeaturePresent(int feature);
        private struct CpuFeatureEntry { public string Name; public int Id; }
        private static string _cachedCpuFeatures = null;
        private static string GetCpuSupportedFeatures()
        {
            if (_cachedCpuFeatures != null) return _cachedCpuFeatures;
            try
            {
                // CPU feature data source: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-isprocessorfeaturepresent
                // Only flags shown by CPU-Z that are also queryable here (modern processors).
                var table = new List<CpuFeatureEntry>{
                    new CpuFeatureEntry { Name = "MMX", Id = 3 },
                    new CpuFeatureEntry { Name = "SSE", Id = 6 }, new CpuFeatureEntry { Name = "SSE2", Id = 10 },
                    new CpuFeatureEntry { Name = "SSE3", Id = 13 }, new CpuFeatureEntry { Name = "SSSE3", Id = 36 },
                    new CpuFeatureEntry { Name = "SSE4.1", Id = 37 }, new CpuFeatureEntry { Name = "SSE4.2", Id = 38 },
                    new CpuFeatureEntry { Name = "AVX", Id = 39 }, new CpuFeatureEntry { Name = "AVX2", Id = 40 },
                    new CpuFeatureEntry { Name = "AVX512F", Id = 41 }
                };
                var on = new List<string>();
                foreach (var f in table)
                {
                    try { if (IsProcessorFeaturePresent(f.Id)) on.Add(f.Name); } catch { }
                }
                on.Sort(StringComparer.OrdinalIgnoreCase);
                _cachedCpuFeatures = on.Count > 0 ? string.Join(", ", on.ToArray()) : string.Empty;
                return _cachedCpuFeatures;
            }
            catch { return string.Empty; }
        }
        private static string TSExtractAssocDeviceId(string assocPath)
        {
            try
            {
                if (string.IsNullOrEmpty(assocPath)) return null;
                int q1 = assocPath.IndexOf("=\"", StringComparison.Ordinal);
                if (q1 < 0) return null;
                int q2 = assocPath.IndexOf("\"", q1 + 2, StringComparison.Ordinal);
                if (q2 <= q1 + 1) return null;
                return assocPath.Substring(q1 + 2, q2 - q1 - 2).Trim();
            }
            catch { return null; }
        }
        private void Cpu()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            using (ManagementObjectSearcher search_process = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, Manufacturer, Architecture, MaxClockSpeed, CurrentClockSpeed, NumberOfCores, NumberOfEnabledCore, ThreadCount, DeviceID, L2CacheSize, L3CacheSize, ProcessorId, Description, SocketDesignation, UpgradeMethod, VirtualizationFirmwareEnabled FROM Win32_Processor"))
            using (ManagementObjectSearcher search_cs = new ManagementObjectSearcher("root\\CIMV2", "SELECT HypervisorPresent FROM Win32_ComputerSystem"))
            {
            bool hvPresent = false;
            try
            {
                using (var hvPreCol = search_cs.Get())
                {
                    foreach (ManagementObject query_hv_pre in hvPreCol.Cast<ManagementObject>())
                    {
                        using (query_hv_pre)
                        {
                            if (query_hv_pre["HypervisorPresent"] == null) continue;
                            hvPresent = Convert.ToBoolean(query_hv_pre["HypervisorPresent"]);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            Dictionary<string, List<ManagementObject>> bulkCacheByCpu = null;
            var bulkCacheOwned = new List<ManagementObject>();
            try
            {
                var cacheByDevice = new Dictionary<string, ManagementObject>(StringComparer.OrdinalIgnoreCase);
                using (var allCache = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID, Level, MaxCacheSize FROM Win32_CacheMemory"))
                using (var allCacheCol = allCache.Get())
                {
                    foreach (ManagementObject c in allCacheCol.Cast<ManagementObject>())
                    {
                        string cid = (Convert.ToString(c["DeviceID"]) ?? string.Empty).Trim();
                        if (!string.IsNullOrEmpty(cid) && !cacheByDevice.ContainsKey(cid)) { cacheByDevice[cid] = c; bulkCacheOwned.Add(c); }
                        else { try { c.Dispose(); } catch { } }
                    }
                }
                if (cacheByDevice.Count > 0)
                {
                    bulkCacheByCpu = new Dictionary<string, List<ManagementObject>>(StringComparer.OrdinalIgnoreCase);
                    using (var assocPM = new ManagementObjectSearcher("root\\CIMV2", "SELECT Antecedent, Dependent FROM Win32_AssociatedProcessorMemory"))
                    using (var assocPMCol = assocPM.Get())
                    {
                        foreach (ManagementObject a in assocPMCol.Cast<ManagementObject>())
                        {
                            using (a)
                            {
                                string cacheId = TSExtractAssocDeviceId(Convert.ToString(a["Antecedent"]) ?? string.Empty);
                                string cpuId = TSExtractAssocDeviceId(Convert.ToString(a["Dependent"]) ?? string.Empty);
                                if (!string.IsNullOrEmpty(cacheId) && !string.IsNullOrEmpty(cpuId) && cacheByDevice.TryGetValue(cacheId, out var co))
                                {
                                    if (!bulkCacheByCpu.TryGetValue(cpuId, out var cl)) bulkCacheByCpu[cpuId] = cl = new List<ManagementObject>();
                                    if (!cl.Contains(co)) cl.Add(co);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu() - bulk cache prefetch"); }
                bulkCacheByCpu = null;
            }
            using (var procCol = search_process.Get())
            {
                foreach (ManagementObject query_process_rotate in procCol.Cast<ManagementObject>())
                {
                    using (query_process_rotate)
                    {
                        try
                        {
                            // CPU NAME
                            string cpu_name = Convert.ToString(query_process_rotate["Name"])?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(cpu_name)) cpu_name = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            CPU_Selector_List.AddItemSafe(cpu_name);
                        }
                        catch (Exception ex)
                        {
                            CPU_Selector_List.AddItemSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU MANUFACTURER AND CHIPSET CHECK
                            string cpuManufacturer = Convert.ToString(query_process_rotate["Manufacturer"])?.Trim() ?? string.Empty;
                            string displayManufacturer;
                            string mbChipset;
                            if (cpuManufacturer.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "Intel Corporation";
                                mbChipset = "Intel";
                            }
                            else if (cpuManufacturer.IndexOf("Advanced Micro", StringComparison.OrdinalIgnoreCase) >= 0 || cpuManufacturer.IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "Advanced Micro Devices, Inc.";
                                mbChipset = "AMD";
                            }
                            else if (cpuManufacturer.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "NVIDIA Corporation";
                                mbChipset = "NVIDIA";
                            }
                            else if (cpuManufacturer.IndexOf("Qualcomm", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "Qualcomm Technologies, Inc.";
                                mbChipset = "Qualcomm";
                            }
                            else if (cpuManufacturer.IndexOf("MediaTek", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "MediaTek Inc.";
                                mbChipset = "MediaTek";
                            }
                            else if (cpuManufacturer.IndexOf("Apple", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                displayManufacturer = "Apple Inc.";
                                mbChipset = "Apple";
                            }
                            else
                            {
                                displayManufacturer = cpuManufacturer;
                                mbChipset = cpuManufacturer;
                            }
                            try
                            {
                                string pciChipset = DetectChipsetFromPci();
                                if (!string.IsNullOrEmpty(pciChipset)) mbChipset = pciChipset;
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                            }
                            cpu_man_list.Add(displayManufacturer);
                            CPU_Manufacturer_V.SetTextSafe(displayManufacturer);
                            MB_Chipset_V.SetTextSafe(mbChipset);
                        }
                        catch (Exception ex)
                        {
                            string cpuUnknown = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            cpu_man_list.Add(cpuUnknown);
                            if (cpu_man_list.Count == 1)
                            {
                                CPU_Manufacturer_V.SetTextSafe(cpuUnknown);
                                MB_Chipset_V.SetTextSafe(cpuUnknown);
                            }
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
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
                            string archText = archMap.ContainsKey(arch) ? archMap[arch] : $"{software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown")} ({arch})";
                            cpu_arch_list.Add(archText);
                            CPU_Architectural_V.SetTextSafe(archText);
                        }
                        catch (Exception ex)
                        {
                            cpu_arch_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_arch_list.Count == 1) CPU_Architectural_V.SetTextSafe(cpu_arch_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU SPEED
                            double cpu_max_speed = 0;
                            try { cpu_max_speed = Convert.ToDouble(query_process_rotate["MaxClockSpeed"]); } catch { cpu_max_speed = 0; }
                            cpu_base_speed_list.Add(cpu_max_speed);
                            if (cpu_max_speed <= 0)
                            {
                                cpu_speed_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            }
                            else if (cpu_max_speed > 1024)
                            {
                                cpu_speed_list.Add(string.Format("{0:0.00} GHz", cpu_max_speed / 1000));
                            }
                            else
                            {
                                cpu_speed_list.Add(cpu_max_speed.ToString() + " MHz");
                            }
                            CPU_Speed_V.SetTextSafe(cpu_speed_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_base_speed_list.Add(0);
                            cpu_speed_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_speed_list.Count == 1) CPU_Speed_V.SetTextSafe(cpu_speed_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU CURRENT SPEED
                            double cpu_cur_speed = 0;
                            try { cpu_cur_speed = Convert.ToDouble(query_process_rotate["CurrentClockSpeed"]); } catch { cpu_cur_speed = 0; }
                            if (cpu_cur_speed <= 0)
                            {
                                cpu_current_speed_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            }
                            else if (cpu_cur_speed > 1024)
                            {
                                cpu_current_speed_list.Add(string.Format("{0:0.00} GHz", cpu_cur_speed / 1000));
                            }
                            else
                            {
                                cpu_current_speed_list.Add(cpu_cur_speed.ToString() + " MHz");
                            }
                            CPU_CurrentSpeed_V.SetTextSafe(cpu_current_speed_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_current_speed_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_current_speed_list.Count == 1) CPU_CurrentSpeed_V.SetTextSafe(cpu_current_speed_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU CORES
                            string core_count = Convert.ToString(query_process_rotate["NumberOfCores"]);
                            if (string.IsNullOrEmpty(core_count)) core_count = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            cpu_core_list.Add(core_count);
                            CPU_CoreCount_V.SetTextSafe(cpu_core_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_core_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_core_list.Count == 1) CPU_CoreCount_V.SetTextSafe(cpu_core_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU ACTIVE CORES
                            string active_core_count = Convert.ToString(query_process_rotate["NumberOfEnabledCore"]);
                            if (string.IsNullOrEmpty(active_core_count)) active_core_count = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            cpu_active_core_list.Add(active_core_count);
                            CPU_ActiveCoreCount_V.SetTextSafe(cpu_active_core_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_active_core_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_active_core_list.Count == 1) CPU_ActiveCoreCount_V.SetTextSafe(cpu_active_core_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU LOGICAL CORES
                            string thread_count = Convert.ToString(query_process_rotate["ThreadCount"]);
                            string __thread_count = "";
                            if (!string.IsNullOrEmpty(thread_count))
                            {
                                __thread_count = thread_count;
                            }
                            else
                            {
                                __thread_count = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            }
                            cpu_logical_core_list.Add(__thread_count);
                            CPU_LogicalCore_V.SetTextSafe(cpu_logical_core_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_logical_core_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_logical_core_list.Count == 1) CPU_LogicalCore_V.SetTextSafe(cpu_logical_core_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // L1 - L2 - L3 CACHE
                            string procId = query_process_rotate["DeviceID"].ToString(); // CPU0
                            string cleanId = new string(procId.Where(char.IsDigit).ToArray());
                            cpu_slot_list.Add(cleanId.Trim());
                            CPU_Slot_V.SetTextSafe(cpu_slot_list[0]);
                            //
                            double cpu_l1_total = 0;
                            double cpu_l2_total = 0;
                            double cpu_l3_total = 0;
                            //
                            var l3_candidates = new List<double>();
                            //
                            List<ManagementObject> cacheObjs = null;
                            if (bulkCacheByCpu != null && bulkCacheByCpu.TryGetValue(procId, out var bcl) && bcl != null && bcl.Count > 0)
                            {
                                cacheObjs = new List<ManagementObject>(bcl.Count);
                                foreach (var bc in bcl) { try { cacheObjs.Add((ManagementObject)bc.Clone()); } catch { } }
                            }
                            if (cacheObjs != null && cacheObjs.Count > 0)
                            {
                                foreach (ManagementObject cache in cacheObjs)
                                {
                                    using (cache)
                                    {
                                        if (cache["Level"] == null || cache["MaxCacheSize"] == null)
                                            continue;
                                        if (!int.TryParse(cache["Level"].ToString(), out int level))
                                            continue;
                                        if (!double.TryParse(cache["MaxCacheSize"].ToString(), out double kb))
                                            continue;
                                        double bytes = kb * 1024; // KB to bytes
                                        switch (level)
                                        {
                                            case 3: cpu_l1_total += bytes; break; // Primary
                                            case 4: cpu_l2_total += bytes; break; // Secondary
                                            case 5: l3_candidates.Add(bytes); break; // Tertiary (fallback)
                                        }
                                    }
                                }
                            }
                            else
                                using (ManagementObjectSearcher cacheSearcher = new ManagementObjectSearcher("root\\CIMV2", $"ASSOCIATORS OF {{Win32_Processor.DeviceID='{procId}'}} WHERE ResultClass = Win32_CacheMemory"))
                                using (var cacheCol = cacheSearcher.Get())
                                {
                                    foreach (ManagementObject cache in cacheCol.Cast<ManagementObject>())
                                    {
                                        using (cache)
                                        {
                                            if (cache["Level"] == null || cache["MaxCacheSize"] == null)
                                                continue;
                                            if (!int.TryParse(cache["Level"].ToString(), out int level))
                                                continue;
                                            if (!double.TryParse(cache["MaxCacheSize"].ToString(), out double kb))
                                                continue;
                                            double bytes = kb * 1024; // KB to bytes
                                            switch (level)
                                            {
                                                case 3: cpu_l1_total += bytes; break; // Primary
                                                case 4: cpu_l2_total += bytes; break; // Secondary
                                                case 5: l3_candidates.Add(bytes); break; // Tertiary (fallback)
                                            }
                                        }
                                    }
                                }
                            double procL3Bytes = 0;
                            if (query_process_rotate["L3CacheSize"] != null && double.TryParse(query_process_rotate["L3CacheSize"].ToString(), out double l3Kb) && l3Kb > 0)
                            {
                                procL3Bytes = l3Kb * 1024;
                            }
                            if (procL3Bytes > 0)
                            {
                                cpu_l3_total = procL3Bytes;
                            }
                            else
                            {
                                if (l3_candidates.Count == 1)
                                {
                                    cpu_l3_total = l3_candidates[0];
                                }
                                else if (l3_candidates.Count > 1)
                                {
                                    bool allSame = l3_candidates.All(x => Math.Abs(x - l3_candidates[0]) < 1);
                                    cpu_l3_total = allSame ? l3_candidates.Max() : l3_candidates.Sum();
                                }
                            }
                            //
                            cpu_l1_size_list.Add(TS_FormatSize(cpu_l1_total));
                            CPU_L1_V.SetTextSafe(cpu_l1_size_list[0]);
                            //
                            cpu_l2_size_list.Add(TS_FormatSize(cpu_l2_total));
                            CPU_L2_V.SetTextSafe(cpu_l2_size_list[0]);
                            //
                            cpu_l3_size_list.Add(TS_FormatSize(cpu_l3_total));
                            CPU_L3_V.SetTextSafe(cpu_l3_size_list[0]);
                        }
                        catch (Exception ex)
                        {
                            string cpuCacheUnknown = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            while (cpu_l1_size_list.Count < cpu_slot_list.Count) cpu_l1_size_list.Add(cpuCacheUnknown);
                            while (cpu_l2_size_list.Count < cpu_slot_list.Count) cpu_l2_size_list.Add(cpuCacheUnknown);
                            while (cpu_l3_size_list.Count < cpu_slot_list.Count) cpu_l3_size_list.Add(cpuCacheUnknown);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU SERIAL ID
                            string cpu_serial = Convert.ToString(query_process_rotate["ProcessorId"])?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(cpu_serial)) cpu_serial = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            if (stealth_status != 1)
                            {
                                cpu_unique_id_list.Add(cpu_serial);
                            }
                            else
                            {
                                cpu_unique_id_list.Add(sm_text);
                            }
                            CPU_SerialName_V.SetTextSafe(cpu_unique_id_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_unique_id_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_unique_id_list.Count == 1) CPU_SerialName_V.SetTextSafe(cpu_unique_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU FAMILY
                            string cpu_description = Convert.ToString(query_process_rotate["Description"]);
                            string cpu_arch = Regex.Replace(cpu_description ?? "", "Intel64", "Intel X64", RegexOptions.IgnoreCase);
                            cpu_arch = Regex.Replace(cpu_arch, "AMD64", "AMD X64", RegexOptions.IgnoreCase);
                            Match cpu_fms = Regex.Match(cpu_description ?? "", @"Family\s+(\d+)\s+Model\s+(\d+)\s+Stepping\s+(\d+)", RegexOptions.IgnoreCase);
                            string cpu_family_text;
                            if (cpu_fms.Success)
                            {
                                string cpu_arch_base = Regex.Replace(cpu_arch, @"\s*Family\s+\d+\s+Model\s+\d+\s+Stepping\s+\d+", "", RegexOptions.IgnoreCase).Trim();
                                cpu_family_text = $"{cpu_arch_base} ({software_lang.TSReadLangs("Cpu_Content", "cpu_c_family")} {cpu_fms.Groups[1].Value}, {software_lang.TSReadLangs("Cpu_Content", "cpu_c_model")} {cpu_fms.Groups[2].Value}, {software_lang.TSReadLangs("Cpu_Content", "cpu_c_stage")} {cpu_fms.Groups[3].Value})";
                            }
                            else
                            {
                                cpu_family_text = cpu_arch.Replace("Family", software_lang.TSReadLangs("Cpu_Content", "cpu_c_family")).Replace("Model", software_lang.TSReadLangs("Cpu_Content", "cpu_c_model")).Replace("Stepping", software_lang.TSReadLangs("Cpu_Content", "cpu_c_stage"));
                            }
                            cpu_family_list.Add(cpu_family_text);
                            CPU_Family_V.SetTextSafe(cpu_family_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_family_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_family_list.Count == 1) CPU_Family_V.SetTextSafe(cpu_family_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU SOCKET
                            string cpu_socket_definition = Convert.ToString(query_process_rotate["SocketDesignation"]);
                            if (string.IsNullOrEmpty(cpu_socket_definition)) cpu_socket_definition = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                            // Soldered detection (updated Oct 2026):
                            // 1) Socket name contains BGA / FPx / FTx / FLx -> soldered. (Intel BGA, AMD FP5/FP6/FP7/FP8/FP11/FT3/FT4/FL1; number-independent.)
                            // 2) Socket name contains LGA/PGA/AM2-AM5/SP3-SP7/TRX-WRX/FMx/FS1/G34/C32 -> socketed. (AM4/AM5 are desktop, never in laptops.)
                            //    NOTE: bare "Socket" word is not a signal ("CPU Socket - U3E1" is generic).
                            // 3) For generic codes (U3E1, Unknown...) soldered only if UpgradeMethod is in the SMBIOS BGA list. (6=None alone does not mean soldered.)
                            // 4) If still ambiguous, check vendor + CPU name: Apple Silicon / NVIDIA / Qualcomm / MediaTek -> soldered; Ampere -> socketed; Intel/AMD mobile suffix -> soldered.
                            bool cpu_soldered = false;
                            try
                            {
                                string sockTrim = (cpu_socket_definition ?? "").Trim();
                                string sockUpper = sockTrim.ToUpperInvariant();
                                string cpuNameForSocket = Convert.ToString(query_process_rotate["Name"]) ?? "";
                                string cpuManForSocket = Convert.ToString(query_process_rotate["Manufacturer"]) ?? "";
                                string cpuFullUpper = (cpuManForSocket + " " + cpuNameForSocket).ToUpperInvariant();
                                bool isVirtualCpu = cpuFullUpper.Contains("VIRTUAL") || cpuFullUpper.Contains("VMWARE") || cpuFullUpper.Contains("QEMU")
                                    || cpuFullUpper.Contains("HYPER-V") || cpuFullUpper.Contains("HYPERV") || cpuFullUpper.Contains("PARALLELS")
                                    || cpuFullUpper.Contains("KVM") || cpuFullUpper.Contains(" XEN ") || cpuFullUpper.Contains("BOCHS");
                                bool socketSaysSoldered = sockUpper.Contains("BGA")
                                    || Regex.IsMatch(sockTrim, @"\bF[PT]\d", RegexOptions.IgnoreCase)
                                    || Regex.IsMatch(sockTrim, @"\bFL\d", RegexOptions.IgnoreCase);
                                bool socketSaysSocketed = sockUpper.Contains("LGA")
                                    || sockUpper.Contains("PGA")
                                    || sockUpper.Contains("AM2") || sockUpper.Contains("AM3") || sockUpper.Contains("AM4") || sockUpper.Contains("AM5")
                                    || sockUpper.Contains("SP3") || sockUpper.Contains("SP5") || sockUpper.Contains("SP6") || sockUpper.Contains("SP7")
                                    || sockUpper.Contains("TRX") || sockUpper.Contains("TR5") || sockUpper.Contains("STR5") || sockUpper.Contains("WRX")
                                    || sockUpper.Contains("FM1") || sockUpper.Contains("FM2") || sockUpper.Contains("FS1")
                                    || sockUpper.Contains("G34") || sockUpper.Contains("C32");
                                if (socketSaysSoldered)
                                {
                                    cpu_soldered = true;
                                }
                                else if (socketSaysSocketed)
                                {
                                    cpu_soldered = false;
                                }
                                else if (!isVirtualCpu)
                                {
                                    bool upgradeIsBga = false;
                                    if (query_process_rotate["UpgradeMethod"] != null)
                                    {
                                        try
                                        {
                                            ushort cpu_upgrade = Convert.ToUInt16(query_process_rotate["UpgradeMethod"]);
                                            // SMBIOS BGA socket values (dmidecode table, BGA codes in 0x20-0x4E range)
                                            switch (cpu_upgrade)
                                            {
                                                case 32:
                                                case 34:
                                                case 35:
                                                case 36:
                                                case 46:
                                                case 47:
                                                case 48:
                                                case 51:
                                                case 52:
                                                case 53:
                                                case 58:
                                                case 59:
                                                case 60:
                                                case 65:
                                                case 66:
                                                case 67:
                                                case 68:
                                                case 72:
                                                case 76:
                                                case 77:
                                                case 78:
                                                    upgradeIsBga = true;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        catch { }
                                    }
                                    if (upgradeIsBga)
                                    {
                                        cpu_soldered = true;
                                    }
                                    else
                                    {
                                        // Vendor + name rules (Oct 2026: Apple M1-M5, NVIDIA GB10/N1/N1X/Spark, Snapdragon X, Core Ultra V/H/U, Ryzen U/H/HS/HX/Z1-Z2)
                                        bool manApple = cpuFullUpper.Contains("APPLE");
                                        bool manNvidia = cpuFullUpper.Contains("NVIDIA");
                                        bool manQualcomm = cpuFullUpper.Contains("QUALCOMM");
                                        bool manMediatek = cpuFullUpper.Contains("MEDIATEK") || cpuFullUpper.Contains("MTK");
                                        bool manAmpere = cpuFullUpper.Contains("AMPERE") || cpuFullUpper.Contains("ALTRA");
                                        if (manApple && Regex.IsMatch(cpuNameForSocket, @"\bM[1-9]\b", RegexOptions.IgnoreCase))
                                        {
                                            cpu_soldered = true; // All Apple Silicon (M1-M5, Pro/Max/Ultra) is soldered
                                        }
                                        else if (manNvidia || Regex.IsMatch(cpuNameForSocket, @"\bGB1\d\d?|GRACE|BLACKWELL|\bN1X?\b|TEGRA|ORIN|THOR|DGX\s+SPARK|\bSPARK\b", RegexOptions.IgnoreCase))
                                        {
                                            cpu_soldered = true; // GB10/Spark, N1/N1X, Grace, Tegra/Orin/Thor: all BGA
                                        }
                                        else if (manQualcomm || Regex.IsMatch(cpuNameForSocket, @"\bSNAPDRAGON|ORYON|\bX1[EP]\b|\bX2\b|8CX|\b8C\b|\b7C\b|\bSQ[123]\b", RegexOptions.IgnoreCase))
                                        {
                                            cpu_soldered = true; // Snapdragon X Elite/Plus, 8cx, Oryon, SQ1-3: all soldered
                                        }
                                        else if (manMediatek || Regex.IsMatch(cpuNameForSocket, @"\bKOMPANIO|DIMENSITY\b", RegexOptions.IgnoreCase))
                                        {
                                            cpu_soldered = true;
                                        }
                                        else if (manAmpere)
                                        {
                                            cpu_soldered = false; // Ampere Altra/One: server LGA socketed
                                        }
                                        else
                                        {
                                            // Intel mobile (soldered): 12650H, 1255U, 1360P, 1065G7, N100/N305, 155H/258V/285HX, Atom, Celeron/Pentium N/J
                                            bool intelMobile = Regex.IsMatch(cpuNameForSocket, @"-\s*\d{3,5}\s*(HX|HK|HQ|HS|HE|HL|H|U|P|UL|Y|V)\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\b\d{4}G[147]\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\bN1\d\d\b|\bN3\d\d\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\b\d{4}(MQ|MX)\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\bW-\d{4,5}M\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\b(ATOM|CELERON\s+[NJ]|PENTIUM\s+[NJ])\b", RegexOptions.IgnoreCase);
                                            // AMD mobile (soldered): HS/HX/HX3D without brand check; single-letter U/H requires Ryzen/Athlon (avoid desktop G/GE/X/X3D)
                                            bool amdMobileUnamb = Regex.IsMatch(cpuNameForSocket, @"\b\d{3,4}\s*(HX3D|HS|HX)\b", RegexOptions.IgnoreCase)
                                                || Regex.IsMatch(cpuNameForSocket, @"\bZ1(\s+EXTREME)?\b|\bZ2(\s+(GO|EXTREME))?\b", RegexOptions.IgnoreCase);
                                            bool amdMobileSingle = Regex.IsMatch(cpuNameForSocket, @"\b(RYZEN|ATHLON)\b", RegexOptions.IgnoreCase)
                                                && Regex.IsMatch(cpuNameForSocket, @"\b\d{3,4}\s*(U|H)\b", RegexOptions.IgnoreCase);
                                            if (intelMobile || amdMobileUnamb || amdMobileSingle)
                                            {
                                                cpu_soldered = true;
                                            }
                                            // Desktop (12400F/12900K/265K, 7600/7950X/8700G) and unknown ARM: stays false, no guessing.
                                        }
                                    }
                                }
                            }
                            catch { }
                            if (cpu_soldered && cpu_socket_definition != software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"))
                            {
                                cpu_socket_definition = $"{cpu_socket_definition} ({software_lang.TSReadLangs("Cpu_Content", "cpu_c_soldered")})";
                            }
                            cpu_socket_list.Add(cpu_socket_definition);
                            CPU_SocketDefinition_V.SetTextSafe(cpu_socket_list[0]);
                        }
                        catch (Exception ex)
                        {
                            cpu_socket_list.Add(software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown"));
                            if (cpu_socket_list.Count == 1) CPU_SocketDefinition_V.SetTextSafe(cpu_socket_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                        try
                        {
                            // CPU VIRTUALIZATION
                            if (Program.Windows_mode == 0)
                            {
                                bool cpu_virtual_mod = false;
                                try { cpu_virtual_mod = Convert.ToBoolean(query_process_rotate["VirtualizationFirmwareEnabled"]); } catch { cpu_virtual_mod = false; }
                                cpu_virtual_mod = cpu_virtual_mod || hvPresent;
                                if (cpu_virtual_mod == true)
                                {
                                    CPU_Virtualization_V.SetTextSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_active"));
                                }
                                else if (cpu_virtual_mod == false)
                                {
                                    CPU_Virtualization_V.SetTextSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_disabled"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                        }
                    }
                }
            }
            try
            {
                if (bulkCacheOwned != null)
                {
                    foreach (var bo in bulkCacheOwned) { try { bo.Dispose(); } catch { } }
                    bulkCacheOwned.Clear();
                }
            }
            catch { }
            try
            {
                // CPU VIRTUALIZATION
                if (Program.Windows_mode == 1)
                {
                    bool virtFound = false;
                    try
                    {
                        using (ManagementObjectSearcher search_proc_virt = new ManagementObjectSearcher("root\\CIMV2", "SELECT VirtualizationFirmwareEnabled FROM Win32_Processor"))
                        using (var virtCol = search_proc_virt.Get())
                        {
                            foreach (ManagementObject query_virt in virtCol.Cast<ManagementObject>())
                            {
                                using (query_virt)
                                {
                                    if (query_virt["VirtualizationFirmwareEnabled"] == null) continue;
                                    bool cpu_virtual_mod = Convert.ToBoolean(query_virt["VirtualizationFirmwareEnabled"]) || hvPresent;
                                    CPU_Virtualization_V.SetTextSafe(cpu_virtual_mod ? software_lang.TSReadLangs("Cpu_Content", "cpu_c_active") : software_lang.TSReadLangs("Cpu_Content", "cpu_c_disabled"));
                                    virtFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
                    }
                    if (!virtFound)
                    {
                        using (var hvCol = search_cs.Get())
                        {
                            foreach (ManagementObject query_hv in hvCol.Cast<ManagementObject>())
                            {
                                using (query_hv)
                                {
                                    if (query_hv["HypervisorPresent"] == null) continue;
                                    bool cpu_virtual_mod = Convert.ToBoolean(query_hv["HypervisorPresent"]);
                                    if (cpu_virtual_mod == true)
                                    {
                                        CPU_Virtualization_V.SetTextSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_active"));
                                    }
                                    else if (cpu_virtual_mod == false)
                                    {
                                        CPU_Virtualization_V.SetTextSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_disabled"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            // Test Data
            // ---------------------
            if (debug_status && Program.glow_console_debug_mode)
            {
                CPU_Selector_List.AddItemSafe("AMD Ryzen 9 7950X3D");
                cpu_man_list.Add("AMD");
                cpu_arch_list.Add("x64 (64 Bit)");
                cpu_speed_list.Add("4.2 GHz");
                cpu_base_speed_list.Add(4200);
                cpu_current_speed_list.Add("4.2 GHz");
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
            try
            {
                // AUTO SELECT SLOT 0
                CPU_Selector_List.ExecuteSafe(() =>
                {
                    if (CPU_Selector_List.Items.Count > 0)
                    {
                        CPU_Selector_List.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            // CPU SUPPORTED FEATURES (system-wide, same for every CPU slot)
            try
            {
                string cpuFeatures = GetCpuSupportedFeatures();
                CPU_Features_V.SetTextSafe(string.IsNullOrEmpty(cpuFeatures) ? software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown") : cpuFeatures);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Cpu()"); }
            }
            // CPU PROCESS END ENABLED
            CPU_RotateBtn.SetEnabledSafe(true);
            CPU.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- Processor Section Loaded --->");
            }
        }
        private async Task ProcessorBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    DateTime startTime = DateTime.Now;
                    long processCount = 0;
                    long totalThreads = 0;
                    long totalHandles = 0;
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT NumberOfProcesses FROM Win32_OperatingSystem"))
                        using (var collection = searcher.Get())
                        {
                            foreach (ManagementObject mo in collection.Cast<ManagementObject>())
                            {
                                using (mo)
                                {
                                    try { processCount = Convert.ToInt64(mo["NumberOfProcesses"]); } catch { }
                                    break;
                                }
                            }
                        }
                        var allProcesses = Process.GetProcesses();
                        foreach (Process proc in allProcesses)
                        {
                            try
                            {
                                totalThreads += proc.Threads.Count;
                                totalHandles += proc.HandleCount;
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); }
                            }
                            finally { proc.Dispose(); }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); }
                    }
                    CPU_Process_V.SetTextSafe(processCount.ToString("N0"));
                    CPU_Threads_V.SetTextSafe(totalThreads.ToString("N0"));
                    CPU_Handles_V.SetTextSafe(totalHandles.ToString("N0"));
                    int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    int nextDelay = Math.Max(10, 1000 - elapsed);
                    try
                    {
                        await Task.Delay(nextDelay, shutdownToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "ProcessorBgProcess()"); }
            }
        }
        // CPU ENGINE
        // ======================================================================================================
        private async Task CpuBgProcess(CancellationToken shutdownToken)
        {
            PerformanceCounter cpuCounter = null;
            PerformanceCounter cpuPerfCounter = null;
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try
            {
                CPU_Usage_V.SetTextSafe(software_lang.TSReadLangs("Cpu_Content", "cpu_c_loading"));
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                cpuCounter.NextValue();
                try
                {
                    cpuPerfCounter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
                    cpuPerfCounter.NextValue();
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                    try { cpuPerfCounter?.Dispose(); } catch { }
                    cpuPerfCounter = null;
                }
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    DateTime startTime = DateTime.Now;
                    float cpuUsage = 0f;
                    try
                    {
                        cpuUsage = cpuCounter.NextValue();
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                        cpuUsage = 0f;
                    }
                    CPU_Usage_V.SetTextSafe(string.Format("{0:F1}%", cpuUsage));
                    try
                    {
                        float perfPct = 0f;
                        bool perfOk = false;
                        if (cpuPerfCounter != null)
                        {
                            try
                            {
                                perfPct = cpuPerfCounter.NextValue();
                                perfOk = perfPct > 0;
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                            }
                        }
                        List<double> wmiSpeeds = null;
                        if (!perfOk && !shutdownToken.IsCancellationRequested)
                        {
                            try
                            {
                                wmiSpeeds = new List<double>();
                                using (var cpuSpeedSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT CurrentClockSpeed FROM Win32_Processor"))
                                using (var col = cpuSpeedSearcher.Get())
                                {
                                    foreach (ManagementObject mo in col.Cast<ManagementObject>())
                                    {
                                        using (mo)
                                        {
                                            double v = 0;
                                            try { v = Convert.ToDouble(mo["CurrentClockSpeed"]); } catch { v = 0; }
                                            wmiSpeeds.Add(v);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                            }
                        }
                        string cpuUnknown = software_lang.TSReadLangs("Cpu_Content", "cpu_c_unknown");
                        CPU_Selector_List.ExecuteSafe(() =>
                        {
                            try
                            {
                                int sel = CPU_Selector_List.SelectedIndex;
                                if (sel < 0 || sel >= cpu_current_speed_list.Count) return;
                                double curMhz = 0;
                                if (perfOk && sel < cpu_base_speed_list.Count && cpu_base_speed_list[sel] > 0)
                                {
                                    curMhz = cpu_base_speed_list[sel] * perfPct / 100.0;
                                }
                                else if (wmiSpeeds != null && sel < wmiSpeeds.Count)
                                {
                                    curMhz = wmiSpeeds[sel];
                                }
                                string formatted;
                                if (curMhz <= 0) formatted = cpuUnknown;
                                else if (curMhz > 1024) formatted = string.Format("{0:0.00} GHz", curMhz / 1000);
                                else formatted = curMhz.ToString("0") + " MHz";
                                cpu_current_speed_list[sel] = formatted;
                                CPU_CurrentSpeed_V.Text = formatted;
                            }
                            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                        });
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
                    }
                    int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    int nextDelay = Math.Max(10, 1000 - elapsed);
                    try
                    {
                        await Task.Delay(nextDelay, shutdownToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "CpuBgProcess()"); }
            }
            finally
            {
                cpuCounter?.Dispose();
                try { cpuPerfCounter?.Dispose(); } catch { }
            }
        }
        private void CPU_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int cpu_slot = CPU_Selector_List.SelectedIndex;
                if (cpu_slot < 0 || cpu_man_list.Count <= cpu_slot) return;
                try { CPU_Manufacturer_V.Text = cpu_man_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Architectural_V.Text = cpu_arch_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_Speed_V.Text = cpu_speed_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
                try { CPU_CurrentSpeed_V.Text = cpu_slot < cpu_current_speed_list.Count ? cpu_current_speed_list[cpu_slot] : cpu_speed_list[cpu_slot]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
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
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "CPU_Selector_List_SelectedIndexChanged()"); }
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
        private void Ram()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            double usableRamBytes = 0;
            double physicalRamBytes = 0;
            using (ManagementObjectSearcher search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, TotalVirtualMemorySize FROM Win32_OperatingSystem"))
            using (ManagementObjectSearcher search_pm = new ManagementObjectSearcher("root\\CIMV2", "SELECT BankLabel, Capacity, SMBIOSMemoryType, MemoryType, Speed, ConfiguredClockSpeed, ConfiguredVoltage, MinVoltage, MaxVoltage, FormFactor, SerialNumber, Manufacturer, TotalWidth, DeviceLocator, PartNumber FROM Win32_PhysicalMemory"))
            {
            using (var ramOsCol = search_os.Get())
            {
                foreach (ManagementObject query_os_rotate in ramOsCol.Cast<ManagementObject>())
                {
                    using (query_os_rotate)
                    {
                        try
                        {
                            // TOTAL RAM
                            double total_bytes = Convert.ToDouble(query_os_rotate["TotalVisibleMemorySize"]) * 1024;
                            usableRamBytes = total_bytes;
                            //
                            string usableStr = TS_FormatSize(total_bytes);
                            //
                            string avabilable_string = software_lang.TSReadLangs("Ram_Content", "ram_c_available");

                            RAM_TotalRAM_V.SetTextSafe(string.Format("{0} {1}", avabilable_string, usableStr));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // TOTAL VIRTUAL RAM
                            double total_virtual_ram = Convert.ToDouble(query_os_rotate["TotalVirtualMemorySize"]) * 1024;
                            RAM_TotalVirtualRam_V.SetTextSafe(TS_FormatSize(total_virtual_ram));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                    }
                }
            }
            using (var ramPmCol = search_pm.Get())
            {
                var ramSlotSeen = new HashSet<string>(ram_slot_list);
                foreach (ManagementObject queryObj in ramPmCol.Cast<ManagementObject>())
                {
                    using (queryObj)
                    {
                        try
                        {
                            // RAM AMOUNT
                            string ram_count = Convert.ToString(queryObj["BankLabel"]);
                            string ram_locator_name = Convert.ToString(queryObj["DeviceLocator"]);
                            if (string.IsNullOrEmpty(ram_count))
                            {
                                string ramSlotEntry = string.IsNullOrEmpty(ram_locator_name) ? software_lang.TSReadLangs("Ram_Content", "ram_c_no_bank_label") : ram_locator_name;
                                ram_slot_list.Add(ramSlotEntry);
                                ramSlotSeen.Add(ramSlotEntry);
                            }
                            else if (ramSlotSeen.Contains(ram_count) && !string.IsNullOrEmpty(ram_locator_name))
                            {
                                string ramSlotEntry = ram_count + " (" + ram_locator_name + ")";
                                ram_slot_list.Add(ramSlotEntry);
                                ramSlotSeen.Add(ramSlotEntry);
                            }
                            else
                            {
                                ram_slot_list.Add(ram_count);
                                ramSlotSeen.Add(ram_count);
                            }
                        }
                        catch (Exception ex)
                        {
                            ram_slot_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_no_bank_label"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM SLOT COUNT
                            ram_slot_count.Add(Convert.ToString(queryObj["Capacity"]));
                            RAM_SlotStatus_V.SetTextSafe(ram_slot_count.Count + " " + software_lang.TSReadLangs("Ram_Content", "ram_c_slot_count"));
                        }
                        catch (Exception ex)
                        {
                            ram_slot_count.Add(string.Empty);
                            RAM_SlotStatus_V.SetTextSafe(ram_slot_count.Count + " " + software_lang.TSReadLangs("Ram_Content", "ram_c_slot_count"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM CAPACITY
                            double ram_amount = Convert.ToDouble(queryObj["Capacity"]);
                            physicalRamBytes += ram_amount;
                            ram_amount_list.Add(TS_FormatSize(ram_amount));
                            RAM_Amount_V.SetTextSafe(ram_amount_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_amount_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_amount_list.Count == 1) RAM_Amount_V.SetTextSafe(ram_amount_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // MEMORY TYPE
                            int sm_bios_memory_type = 0;
                            int memory_type = 0;
                            try { sm_bios_memory_type = Convert.ToInt32(queryObj["SMBIOSMemoryType"]); } catch { sm_bios_memory_type = 0; }
                            try { memory_type = Convert.ToInt32(queryObj["MemoryType"]); } catch { memory_type = 0; }
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
                        { 27, "LPDDR" },
                        { 28, "LPDDR2" },
                        { 29, "LPDDR3" },
                        { 30, "LPDDR4" },
                        { 31, "Logical non-volatile device" },
                        { 32, "HBM" },
                        { 33, "HBM2" },
                        { 34, "DDR5" },
                        { 35, "LPDDR5" },
                        { 36, "HBM3" },
                        { 37, "MRDIMM" }
                    };
                            bool smBiosInformative = sm_bios_memory_type != 0 && sm_bios_memory_type != 1 && memoryTypes.ContainsKey(sm_bios_memory_type);
                            bool memInformative = memory_type != 0 && memory_type != 1 && memoryTypes.ContainsKey(memory_type);
                            if ((smBiosInformative && memoryTypes.TryGetValue(sm_bios_memory_type, out string ramTypeMessage)) || (memInformative && memoryTypes.TryGetValue(memory_type, out ramTypeMessage)))
                            {
                                ram_type_list.Add(ramTypeMessage);
                            }
                            else if (memoryTypes.TryGetValue(sm_bios_memory_type, out ramTypeMessage) && sm_bios_memory_type != 0)
                            {
                                ram_type_list.Add(ramTypeMessage);
                            }
                            else
                            {
                                ram_type_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            RAM_Type_V.SetTextSafe(ram_type_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_type_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_type_list.Count == 1) RAM_Type_V.SetTextSafe(ram_type_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM SPEED
                            double configuredSpeed = 0;
                            double ratedSpeed = 0;
                            try { configuredSpeed = Convert.ToDouble(queryObj["ConfiguredClockSpeed"]); } catch { configuredSpeed = 0; }
                            try { ratedSpeed = Convert.ToDouble(queryObj["Speed"]); } catch { ratedSpeed = 0; }
                            double ram_speed = configuredSpeed > 0 ? configuredSpeed : ratedSpeed;
                            if (ram_speed <= 0)
                            {
                                ram_frekans_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                ram_frekans_list.Add(string.Format("{0} MT/s ({1} MHz)", ram_speed, ram_speed / 2.0));
                            }
                            RAM_Frequency_V.SetTextSafe(ram_frekans_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_frekans_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_frekans_list.Count == 1) RAM_Frequency_V.SetTextSafe(ram_frekans_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM VOLTAGE
                            double ramVolt = 0;
                            string[] voltKeys = new string[] { "ConfiguredVoltage", "MinVoltage", "MaxVoltage" };
                            foreach (string voltKey in voltKeys)
                            {
                                try
                                {
                                    string voltStr = queryObj[voltKey]?.ToString();
                                    if (!string.IsNullOrEmpty(voltStr) && double.TryParse(voltStr, out double parsedVolt) && parsedVolt > 0)
                                    {
                                        ramVolt = parsedVolt;
                                        break;
                                    }
                                }
                                catch { continue; }
                            }
                            if (ramVolt <= 0)
                            {
                                ram_voltage_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                ram_voltage_list.Add(string.Format("{0:0.00} " + software_lang.TSReadLangs("Ram_Content", "ram_c_voltage"), ramVolt / 1000.0));
                            }
                            RAM_Volt_V.SetTextSafe(ram_voltage_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_voltage_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_voltage_list.Count == 1) RAM_Volt_V.SetTextSafe(ram_voltage_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
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
                        { 23, "LGA" },
                        { 24, "FB-DIMM" }
                    };
                            if (formFactorDict.TryGetValue(form_factor, out string formFactorValue))
                            {
                                ram_form_factor.Add(formFactorValue);
                            }
                            else
                            {
                                ram_form_factor.Add(formFactorDict[0]);
                            }
                            RAM_FormFactor_V.SetTextSafe(ram_form_factor[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_form_factor.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_form_factor.Count == 1) RAM_FormFactor_V.SetTextSafe(ram_form_factor[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM SERIAL
                            string ram_serial = Convert.ToString(queryObj["SerialNumber"]).Trim();
                            if (string.IsNullOrEmpty(ram_serial) || ram_serial.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
                            {
                                ram_serial_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                if (stealth_status == 1)
                                {
                                    ram_serial_list.Add(sm_text);
                                }
                                else
                                {
                                    ram_serial_list.Add(ram_serial);
                                }
                            }
                            RAM_Serial_V.SetTextSafe(ram_serial_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_serial_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_serial_list.Count == 1) RAM_Serial_V.SetTextSafe(ram_serial_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM MAN (WMI Manufacturer = JEDEC JEP-106 ID or plain text)
                            string ram_man_raw = Convert.ToString(queryObj["Manufacturer"])?.Trim() ?? string.Empty;
                            string ram_man_key = ram_man_raw.ToUpperInvariant().Replace("0X", "").Trim();
                            // Verified against JEP106BI: prefix = continuation count (bank-1), last byte = ID
                            Dictionary<string, string> manufacturerDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
                        { "00CE", "Samsung" },      // Bank1 CE
                        { "80CE", "Samsung" },      // parity variant
                        { "00AD", "SK Hynix" },     // Bank1 AD
                        { "80AD", "SK Hynix" },
                        { "002C", "Micron Technology" }, // Bank1 2C
                        { "802C", "Micron Technology" },
                        { "2C00", "Micron Technology" }, // byte-swap variant (some BIOSes)
                        { "017A", "Apacer" },       // Bank2 7A
                        { "0198", "Kingston" },     // Bank2 98 (incl. HyperX)
                        { "014F", "Transcend" },    // Bank2 4F
                        { "01BA", "PNY" },          // Bank2 BA
                        { "029E", "Corsair" },      // Bank3 9E
                        { "02FE", "Elpida" },       // Bank3 FE
                        { "02B5", "SpecTek" },      // Bank3 B5
                        { "030B", "Nanya" },        // Bank4 0B
                        { "0325", "Kingmax" },      // Bank4 25
                        { "04CB", "A-DATA" },       // Bank5 CB
                        { "04CD", "G.Skill" },      // Bank5 CD
                        { "04B0", "OCZ" },          // Bank5 B0
                        { "04EF", "Team Group" },   // Bank5 EF
                        { "04F1", "Toshiba" },      // Bank5 F1 (Kioxia)
                        { "059B", "Crucial" },      // Bank6 9B
                        { "859B", "Crucial" },      // parity variant
                        { "0502", "Patriot Memory" }, // Bank6 02
                        { "065E", "Lexar" },        // Bank7 5E
                        { "06D3", "Silicon Power" }, // Bank7 D3
                        { "0746", "Gloway" },       // Bank8 46
                        { "08F1", "Asgard" },       // Bank9 F1
                        { "09CD", "Longsys" },      // Bank10 CD
                        { "00C1", "Infineon" }      // Bank1 C1
                    };
                            string detectedManufacturer;
                            if (string.IsNullOrEmpty(ram_man_key) || ram_man_key.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase)
                                || ram_man_key == "0000" || ram_man_key == "FFFF" || ram_man_key == "7F7F" || ram_man_key == "7FFF")
                            {
                                detectedManufacturer = software_lang.TSReadLangs("Ram_Content", "ram_c_unknown");
                            }
                            else if (manufacturerDict.TryGetValue(ram_man_key, out string manufacturerName))
                            {
                                detectedManufacturer = manufacturerName;
                            }
                            else if (ram_man_key.Length > 4)
                            {
                                // Some BIOSes return a combined ID like "80AD000080AD": try first/last 4 chars
                                string first4 = ram_man_key.Substring(0, 4);
                                string last4 = ram_man_key.Substring(ram_man_key.Length - 4, 4);
                                if (manufacturerDict.TryGetValue(first4, out manufacturerName) || manufacturerDict.TryGetValue(last4, out manufacturerName))
                                {
                                    detectedManufacturer = manufacturerName;
                                }
                                else
                                {
                                    detectedManufacturer = ram_man_raw.Trim();
                                }
                            }
                            else if (ram_man_key.Length == 4)
                            {
                                // Try byte-swap variant (e.g. CE00 -> 00CE, AD00 -> 00AD)
                                string swapped = ram_man_key.Substring(2, 2) + ram_man_key.Substring(0, 2);
                                if (manufacturerDict.TryGetValue(swapped, out manufacturerName))
                                {
                                    detectedManufacturer = manufacturerName;
                                }
                                else
                                {
                                    detectedManufacturer = ram_man_raw.Trim();
                                }
                            }
                            else
                            {
                                detectedManufacturer = ram_man_raw.Trim();
                            }
                            ram_manufacturer_list.Add(detectedManufacturer);
                            RAM_Manufacturer_V.SetTextSafe(detectedManufacturer);
                        }
                        catch (Exception ex)
                        {
                            ram_manufacturer_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_manufacturer_list.Count == 1) RAM_Manufacturer_V.SetTextSafe(ram_manufacturer_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM BANK LABEL
                            string bank_label = Convert.ToString(queryObj["BankLabel"]);
                            if (string.IsNullOrEmpty(bank_label))
                            {
                                ram_bank_label_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                ram_bank_label_list.Add(bank_label);
                            }
                            RAM_BankLabel_V.SetTextSafe(ram_bank_label_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_bank_label_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_bank_label_list.Count == 1) RAM_BankLabel_V.SetTextSafe(ram_bank_label_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM TOTAL WIDTH
                            string ram_data_width = Convert.ToString(queryObj["TotalWidth"]);
                            if (string.IsNullOrEmpty(ram_data_width))
                            {
                                ram_data_width_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                ram_data_width_list.Add(ram_data_width + " Bit");
                            }
                            RAM_DataWidth_V.SetTextSafe(ram_data_width_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_data_width_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_data_width_list.Count == 1) RAM_DataWidth_V.SetTextSafe(ram_data_width_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // RAM LOCATOR
                            string ram_locator = Convert.ToString(queryObj["DeviceLocator"]);
                            if (string.IsNullOrEmpty(ram_locator))
                            {
                                bellek_type_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                bellek_type_list.Add(ram_locator);
                            }
                            RAM_BellekType_V.SetTextSafe(bellek_type_list[0]);
                        }
                        catch (Exception ex)
                        {
                            bellek_type_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (bellek_type_list.Count == 1) RAM_BellekType_V.SetTextSafe(bellek_type_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                        try
                        {
                            // PART NUMBER
                            string part_number = Convert.ToString(queryObj["PartNumber"]).Trim();
                            if (string.IsNullOrEmpty(part_number))
                            {
                                ram_part_number_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            }
                            else
                            {
                                if (stealth_status == 1)
                                {
                                    ram_part_number_list.Add(sm_text);
                                }
                                else
                                {
                                    ram_part_number_list.Add(part_number);
                                }
                            }
                            RAM_PartNumber_V.SetTextSafe(ram_part_number_list[0]);
                        }
                        catch (Exception ex)
                        {
                            ram_part_number_list.Add(software_lang.TSReadLangs("Ram_Content", "ram_c_unknown"));
                            if (ram_part_number_list.Count == 1) RAM_PartNumber_V.SetTextSafe(ram_part_number_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
                        }
                    }
                }
            }
            if (physicalRamBytes > 0)
            {
                string availableText = software_lang.TSReadLangs("Ram_Content", "ram_c_available");
                string usableText = usableRamBytes > 0 ? TS_FormatSize(usableRamBytes) : software_lang.TSReadLangs("Ram_Content", "ram_c_unknown");
                RAM_TotalRAM_V.SetTextSafe(string.Format("{0} ({1} {2})", TS_FormatSize(physicalRamBytes), availableText, usableText));
            }
            // RAM SELECT
            try
            {
                string ram_unknown = software_lang.TSReadLangs("Ram_Content", "ram_c_unknown");
                int ram_amount = ram_slot_list.Count - 1;
                for (int rs = 0; rs <= ram_amount; rs++)
                {
                    string sel_man = rs < ram_manufacturer_list.Count ? ram_manufacturer_list[rs] : ram_unknown;
                    string sel_amt = rs < ram_amount_list.Count ? ram_amount_list[rs] : ram_unknown;
                    RAM_Selector_List.AddItemSafe(string.Format("{0} #{1} - {2} / {3}", software_lang.TSReadLangs("Ram_Content", "ram_c_ram_slot_select"), (rs + 1), sel_man, sel_amt));
                }
                RAM_Selector_List.ExecuteSafe(() =>
                {
                    if (RAM_Selector_List.Items.Count > 0)
                    {
                        RAM_Selector_List.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Ram()"); }
            }
            // RAM PROCESS END ENABLED
            RAM_RotateBtn.SetEnabledSafe(true);
            RAM.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- RAM Section Loaded --->");
            }
        }
        private void RAM_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int ram_slot = RAM_Selector_List.SelectedIndex;
                if (ram_slot < 0 || ram_manufacturer_list.Count <= ram_slot) return;
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
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "RAM_Selector_List_SelectedIndexChanged()"); }
        }
        private async Task RamBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    DateTime startTime = DateTime.Now;
                    ulong total = 0;
                    ulong free = 0;
                    try
                    {
                        using (var search_os = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                        using (var collection = search_os.Get())
                        {
                            foreach (ManagementObject get_ram_info in collection.Cast<ManagementObject>())
                            {
                                using (get_ram_info)
                                {
                                    total = (ulong)get_ram_info["TotalVisibleMemorySize"] * 1024;
                                    free = (ulong)get_ram_info["FreePhysicalMemory"] * 1024;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "RamBgProcess() - WMI Query"); }
                    }
                    if (total > 0)
                    {
                        ulong used = total - free;
                        double usedRatio = (double)used / total * 100;
                        RAM_ProgressBGPanel.ExecuteSafe(() =>
                        {
                            RAM_UsageRAMCount_V.Text = TS_FormatSize(used);
                            RAM_EmptyRamCount_V.Text = TS_FormatSize(free);
                            RAM_ProgressFEPanel.Height = (int)(RAM_ProgressBGPanel.Height * (usedRatio / 100.0));
                            RAM_ProgressLabel.Text = $"{usedRatio:F0}%";
                            RAM_ProgressLabel.Top = RAM_ProgressBGPanel.Top + RAM_ProgressFEPanel.Top + 6;
                        });
                    }
                    int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    int nextDelay = Math.Max(10, 1000 - elapsed);
                    try
                    {
                        await Task.Delay(nextDelay, shutdownToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "RamBgProcess()"); }
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
        private async Task Gpu()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var gpuSelectorItems = new List<string>();
            using (ManagementObjectSearcher search_vc = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, AdapterCompatibility, DriverVersion, DriverDate, Availability, PNPDeviceID, AdapterDACType, InstalledDisplayDrivers, InfFilename, InfSection, CurrentNumberOfColors FROM Win32_VideoController"))
            {
                using (var collection = search_vc.Get())
                {
                    foreach (ManagementObject query_vc_rotate in collection.Cast<ManagementObject>())
                    {
                        using (query_vc_rotate)
                        {
                            try
                            {
                                // GET GPU NAME AND MATCH MULTI GPU
                                string gpu_name = (Convert.ToString(query_vc_rotate["Name"]) ?? "").Trim();
                                if (gpu_name.Length == 0)
                                {
                                    continue;
                                }
                                if (!gpuNameCounters.TryGetValue(gpu_name, out int count))
                                {
                                    count = 0;
                                }
                                count++;
                                gpuNameCounters[gpu_name] = count;
                                if (count == 1)
                                {
                                    gpuSelectorItems.Add(gpu_name);
                                    GPU_Selector_List.AddItemSafe(gpu_name);
                                }
                                else if (count == 2)
                                {
                                    int existingIndex = gpuSelectorItems.FindIndex(item => string.Equals(item, gpu_name, StringComparison.OrdinalIgnoreCase));
                                    if (existingIndex >= 0)
                                    {
                                        gpuSelectorItems[existingIndex] = gpu_name + " #1";
                                    }
                                    else
                                    {
                                        gpuSelectorItems.Add(gpu_name + " #1");
                                    }
                                    gpuSelectorItems.Add(gpu_name + " #2");
                                    bool found = false;
                                    GPU_Selector_List.ExecuteSafe(() =>
                                    {
                                        for (int i = 0; i < GPU_Selector_List.Items.Count; i++)
                                        {
                                            if (string.Equals(GPU_Selector_List.Items[i]?.ToString(), gpu_name, StringComparison.OrdinalIgnoreCase))
                                            {
                                                GPU_Selector_List.Items[i] = gpu_name + " #1";
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found)
                                        {
                                            GPU_Selector_List.Items.Add(gpu_name + " #1");
                                        }
                                        GPU_Selector_List.Items.Add(gpu_name + " #2");
                                    });
                                }
                                else
                                {
                                    gpuSelectorItems.Add(gpu_name + " #" + count);
                                    GPU_Selector_List.AddItemSafe(gpu_name + " #" + count);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU MAN
                                string gpu_man = Convert.ToString(query_vc_rotate["AdapterCompatibility"]).Trim();
                                if (!string.IsNullOrEmpty(gpu_man))
                                {
                                    gpu_man_list.Add(gpu_man);
                                    GPU_Manufacturer_V.SetTextSafe(gpu_man_list[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU DRIVER VERSION
                                string driver_version = Convert.ToString(query_vc_rotate["DriverVersion"]);
                                if (!string.IsNullOrEmpty(driver_version))
                                {
                                    gpu_driver_version_list.Add(driver_version);
                                    GPU_Version_V.SetTextSafe(gpu_driver_version_list[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU DRIVER DATE
                                string gpu_date = Convert.ToString(query_vc_rotate["DriverDate"]);
                                if (!string.IsNullOrEmpty(gpu_date))
                                {
                                    DateTime gpu_date_last = ManagementDateTimeConverter.ToDateTime(gpu_date);
                                    gpu_driver_date_list.Add($"{gpu_date_last:dd.MM.yyyy}");
                                    GPU_DriverDate_V.SetTextSafe(gpu_driver_date_list[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
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
                                if (gpuStatusMessages.TryGetValue(gpu_status, out string gpuStatusMessage))
                                {
                                    gpu_status_list.Add(gpuStatusMessage);
                                }
                                else
                                {
                                    gpu_status_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                                }
                                GPU_Status_V.SetTextSafe(gpu_status_list[0]);
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU DEVICE ID
                                string gpu_device_id = Convert.ToString(query_vc_rotate["PNPDeviceID"]).Trim();
                                if (!string.IsNullOrEmpty(gpu_device_id))
                                {
                                    char[] split_char = { '\\' };
                                    string[] gpu_device_split = gpu_device_id.Trim().Split(split_char);
                                    gpu_device_id_list.Add($"{gpu_device_split[0]}\\{gpu_device_split[1]}");
                                }
                                else
                                {
                                    gpu_device_id_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                                }
                                GPU_DeviceID_V.SetTextSafe(gpu_device_id_list[0]);
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU DAC TYPE
                                string adaptor_dac_type = Convert.ToString(query_vc_rotate["AdapterDACType"]);
                                if (string.IsNullOrEmpty(adaptor_dac_type))
                                {
                                    gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                                }
                                else
                                {
                                    if (adaptor_dac_type == "Integrated RAMDAC")
                                    {
                                        gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_integrated_ramdac"));
                                    }
                                    else if (adaptor_dac_type == "Internal")
                                    {
                                        gpu_dac_type_list.Add(software_lang.TSReadLangs("Gpu_Content", "gpu_c_internal_ramdac"));
                                    }
                                    else
                                    {
                                        gpu_dac_type_list.Add(adaptor_dac_type);
                                    }
                                }
                                GPU_DacType_V.SetTextSafe(gpu_dac_type_list[0]);
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU DIRECTX DRIVERS
                                var grouped = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                                var drivers = (query_vc_rotate["InstalledDisplayDrivers"] ?? "").ToString().Split(',');
                                foreach (var p in drivers)
                                {
                                    var path = p.Trim();
                                    if (string.IsNullOrEmpty(path)) continue;
                                    int lastSlash = path.LastIndexOf('\\');
                                    var fileName = lastSlash >= 0 && lastSlash < path.Length - 1 ? path.Substring(lastSlash + 1) : path;
                                    if (string.IsNullOrEmpty(fileName) || fileName.IndexOfAny(new[] { '<', '>' }) >= 0)
                                        continue;
                                    var directory = lastSlash > 0 ? path.Substring(0, lastSlash) : "";
                                    int secondLastSlash = directory.LastIndexOf('\\');
                                    var key = secondLastSlash >= 0 ? directory.Substring(secondLastSlash + 1) : directory;
                                    if (string.IsNullOrEmpty(key))
                                    {
                                        key = "GPUDrivers_Cache";
                                    }
                                    if (!grouped.TryGetValue(key, out var set))
                                        grouped[key] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                    set.Add(fileName);
                                }
                                gpu_drivers_list.AddRange(grouped.Values.Select(dlls => string.Join(", ", dlls.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))));
                                if (gpu_drivers_list.Count > 0)
                                {
                                    GPU_GraphicDriversName_V.SetTextSafe(gpu_drivers_list[0]);
                                }
                                else
                                {
                                    GPU_GraphicDriversName_V.SetTextSafe(software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown"));
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU INF FILE NAME
                                string gpu_inf_file = Convert.ToString(query_vc_rotate["InfFilename"]);
                                if (!string.IsNullOrEmpty(gpu_inf_file))
                                {
                                    gpu_inf_file_list.Add(gpu_inf_file);
                                    GPU_InfFileName_V.SetTextSafe(gpu_inf_file_list[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // GPU INF FILE GPU INFO PARTITION
                                string gpu_inf_section = Convert.ToString(query_vc_rotate["InfSection"]);
                                if (!string.IsNullOrEmpty(gpu_inf_section))
                                {
                                    gpu_inf_file_section_list.Add(gpu_inf_section);
                                    GPU_INFSectionFile_V.SetTextSafe(gpu_inf_file_section_list[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                            try
                            {
                                // CURRENT NUMBER OF COLORS
                                long gpu_current_color = Convert.ToInt64(query_vc_rotate["CurrentNumberOfColors"]);
                                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                                string formattedNumber = gpu_current_color.ToString("N0", currentCulture);
                                gpu_current_colors_list.Add(formattedNumber);
                                GPU_CurrentColor_V.SetTextSafe(gpu_current_colors_list[0]);
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                            }
                        }
                    }
                }
            }
            // MONITOR INFORMATIONS
            // ======================================
            try
            {
                List<string> edid_model_list = new List<string>();
                List<string> edid_manufacturer_list = new List<string>();
                List<string> edid_product_code_list = new List<string>();
                List<string> edid_serial_list = new List<string>();
                List<string> edid_manuf_list = new List<string>();
                List<string> edid_manuf_week_list = new List<string>();
                List<string> edid_manuf_hid_list = new List<string>();
                List<int> edid_panel_bpc_list = new List<int>();
                string local_display_name = software_lang.TSReadLangs("Gpu_Content", "gpu_c_d_name");
                string unknown_message = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                using (var searchMonitorInfo = new ManagementObjectSearcher(@"root\WMI", "SELECT InstanceName, UserFriendlyName, ManufacturerName, ProductCodeID, SerialNumberID, YearOfManufacture, WeekOfManufacture FROM WmiMonitorID"))
                {
                    using (var collection = searchMonitorInfo.Get())
                    {
                        foreach (ManagementObject mo in collection.Cast<ManagementObject>())
                        {
                            using (mo)
                            {
                                string instanceName = "", model = "", manufacturer = "", productCode = "", serial = "", manuf_week = "";
                                UInt16 manuf_year = 0;
                                try { instanceName = mo["InstanceName"]?.ToString().Trim() ?? ""; } catch { }
                                try { model = GetMonitorFromUShortArray((ushort[])mo["UserFriendlyName"]); } catch { }
                                try { manufacturer = GetMonitorFromUShortArray((ushort[])mo["ManufacturerName"]); } catch { }
                                try { productCode = GetMonitorFromUShortArray((ushort[])mo["ProductCodeID"]); } catch { }
                                try { serial = GetMonitorFromUShortArray((ushort[])mo["SerialNumberID"]); } catch { }
                                try { manuf_year = (UInt16)mo["YearOfManufacture"]; } catch { }
                                try { manuf_week = TSGetFormattedWeekInfo(manuf_year, Convert.ToInt32(Convert.ToUInt32(mo["WeekOfManufacture"]))); } catch { }
                                if (string.IsNullOrWhiteSpace(model)) model = local_display_name;
                                edid_model_list.Add(model.Trim('\0').Trim());
                                edid_manufacturer_list.Add(string.IsNullOrWhiteSpace(manufacturer) ? unknown_message : manufacturer.Trim());
                                edid_product_code_list.Add(string.IsNullOrWhiteSpace(productCode) ? unknown_message : productCode.Trim());
                                edid_serial_list.Add(string.IsNullOrWhiteSpace(serial) ? unknown_message : serial.Trim());
                                edid_manuf_list.Add(manuf_year == 0 ? unknown_message : manuf_year.ToString().Trim());
                                edid_manuf_week_list.Add(string.IsNullOrWhiteSpace(manuf_week) ? unknown_message : manuf_week.Trim());
                                edid_manuf_hid_list.Add(string.IsNullOrWhiteSpace(instanceName) ? unknown_message : instanceName.Trim());
                                int panelBpc = 0;
                                try { panelBpc = GetEdidPanelBitsPerColor(instanceName); } catch { panelBpc = 0; }
                                edid_panel_bpc_list.Add(panelBpc);
                            }
                        }
                    }
                }
                var edidSeg1List = new List<string>(edid_manuf_hid_list.Count);
                foreach (var hid in edid_manuf_hid_list)
                {
                    string edidSeg = null;
                    if (!string.IsNullOrWhiteSpace(hid))
                    {
                        string[] hp = hid.Split('\\');
                        if (hp.Length > 1) edidSeg = hp[1].Trim();
                    }
                    edidSeg1List.Add(edidSeg);
                }
                var videoModeMapByKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var videoModeMapStatic = new Dictionary<long, string>{
                    { -2, software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown") },
                    { -1, software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown") },
                    { 0, "HD15 (VGA)" }, { 1, "S-Video" }, { 2, "Composite Video" }, { 3, "Component Video" },
                    { 4, "DVI" }, { 5, "HDMI" }, { 6, "LVDS / MIPI DSI" }, { 8, "D-Jpn" }, { 9, "SDI" },
                    { 10, string.Format("DisplayPort ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_external")) },
                    { 11, string.Format("DisplayPort ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_embedded")) },
                    { 12, string.Format("UDI ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_external")) },
                    { 13, string.Format("UDI ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_embedded")) },
                    { 14, "SDTV Dongle" }, { 15, string.Format("Miracast ({0})", software_lang.TSReadLangs("Gpu_Content", "gpu_c_wireless")) },
                    { 16, "Indirect Wired" }, { unchecked((int)0x80000000), software_lang.TSReadLangs("Gpu_Content", "gpu_c_d_name") }
                };
                using (var connectionParamsSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT InstanceName, VideoOutputTechnology FROM WmiMonitorConnectionParams"))
                {
                    using (var collection = connectionParamsSearcher.Get())
                    {
                        foreach (ManagementObject mp in collection.Cast<ManagementObject>())
                        {
                            using (mp)
                            {
                                try
                                {
                                    string inst = mp["InstanceName"]?.ToString() ?? "";
                                    long video_mode = Convert.ToInt64(mp["VideoOutputTechnology"]);
                                    string videoModeRender = videoModeMapStatic.TryGetValue(video_mode, out var vm) ? vm : "eDP";
                                    string instNorm = inst.Trim();
                                    if (!string.IsNullOrEmpty(instNorm))
                                    {
                                        if (!videoModeMapByKey.ContainsKey(instNorm))
                                        {
                                            videoModeMapByKey[instNorm] = videoModeRender;
                                        }
                                    }
                                    string[] parts = instNorm.Split('\\');
                                    if (parts.Length > 1)
                                    {
                                        string shortId = parts[1].Trim();
                                        if (!string.IsNullOrEmpty(shortId) && !videoModeMapByKey.ContainsKey(shortId))
                                        {
                                            videoModeMapByKey[shortId] = videoModeRender;
                                        }
                                    }
                                    if (parts.Length > 2)
                                    {
                                        string tail = parts[parts.Length - 1].Trim();
                                        if (!string.IsNullOrEmpty(tail) && !videoModeMapByKey.ContainsKey(tail))
                                        {
                                            videoModeMapByKey[tail] = videoModeRender;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    var screen = Screen.AllScreens[i];
                    var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)) };
                    EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
                    DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                    d.cb = Marshal.SizeOf(d);
                    EnumDisplayDevices(screen.DeviceName, 0, ref d, 0);
                    string deviceKey = d.DeviceID ?? "";
                    int matchedIndex = -1;
                    string devKeyPart = deviceKey.Contains("\\") ? deviceKey.Split('\\')[1].Trim().ToUpperInvariant() : deviceKey.Trim().ToUpperInvariant();
                    for (int j = 0; j < edid_manuf_hid_list.Count; j++)
                    {
                        string instanceName = edid_manuf_hid_list[j] ?? "";
                        if (string.IsNullOrWhiteSpace(instanceName)) continue;
                        if (instanceName.Equals(deviceKey, StringComparison.OrdinalIgnoreCase) || instanceName.IndexOf(devKeyPart, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            matchedIndex = j;
                            break;
                        }
                        string edidSeg = (j >= 0 && j < edidSeg1List.Count) ? edidSeg1List[j] : null;
                        if (edidSeg != null && edidSeg.Equals(devKeyPart, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedIndex = j;
                            break;
                        }
                    }
                    string resolvedVideoOutput = null;
                    if (matchedIndex != -1)
                    {
                        string instCandidate = edid_manuf_hid_list[matchedIndex] ?? "";
                        if (!string.IsNullOrWhiteSpace(instCandidate) && videoModeMapByKey.TryGetValue(instCandidate, out var v1))
                        {
                            resolvedVideoOutput = v1;
                        }
                        else
                        {
                            string[] p = instCandidate.Split('\\');
                            if (p.Length > 1 && videoModeMapByKey.TryGetValue(p[1], out var v2))
                            {
                                resolvedVideoOutput = v2;
                            }
                        }
                    }
                    if (resolvedVideoOutput == null)
                    {
                        string[] dkParts = deviceKey.Split('\\');
                        if (dkParts.Length > 1 && videoModeMapByKey.TryGetValue(dkParts[1], out var vv))
                        {
                            resolvedVideoOutput = vv;
                        }
                        else if (dkParts.Length > 2 && videoModeMapByKey.TryGetValue(dkParts[dkParts.Length - 1], out var vv2))
                        {
                            resolvedVideoOutput = vv2;
                        }
                    }
                    if (resolvedVideoOutput == null)
                    {
                        resolvedVideoOutput = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                    }
                    gpu_monitor_user_friendly_name_list.Add(matchedIndex != -1 ? edid_model_list[matchedIndex] : local_display_name);
                    gpu_monitor_manufacturer_list.Add(matchedIndex != -1 ? edid_manufacturer_list[matchedIndex] : unknown_message);
                    gpu_monitor_product_code_id_list.Add(matchedIndex != -1 ? edid_product_code_list[matchedIndex] : unknown_message);
                    if (matchedIndex == -1 || stealth_status != 1)
                    {
                        gpu_monitor_serial_number_id_list.Add(matchedIndex != -1 ? edid_serial_list[matchedIndex] : unknown_message);
                    }
                    else
                    {
                        gpu_monitor_serial_number_id_list.Add(sm_text);
                    }
                    gpu_monitor_manuf_list.Add(matchedIndex != -1 ? edid_manuf_list[matchedIndex] : unknown_message);
                    gpu_monitor_manuf_week_list.Add(matchedIndex != -1 ? edid_manuf_week_list[matchedIndex] : unknown_message);
                    gpu_monitor_hid_list.Add(matchedIndex != -1 ? edid_manuf_hid_list[matchedIndex] : unknown_message);
                    gpu_monitor_con_type_list.Add(resolvedVideoOutput);
                    gpu_monitor_bounds_list.Add(GPUFormatScreenInfo(screen.Bounds));
                    gpu_monitor_work_list.Add(GPUFormatScreenInfo(screen.WorkingArea));
                    gpu_monitor_primary_list.Add(GPUFormatPrimaryScreen(screen.Primary));
                    gpu_monitor_res_list.Add(dm.dmPelsWidth + " x " + dm.dmPelsHeight);
                    gpu_monitor_virtual_res_list.Add(screen.Bounds.Width + " x " + screen.Bounds.Height);
                    gpu_monitor_refresh_rate_list.Add(dm.dmDisplayFrequency + " Hz");
                    int screenPanelBpc = (matchedIndex != -1 && matchedIndex < edid_panel_bpc_list.Count) ? edid_panel_bpc_list[matchedIndex] : 0;
                    if (screenPanelBpc > 0)
                    {
                        gpu_monitor_bit_deep_list.Add(dm.dmBitsPerPel + " Bit (" + screenPanelBpc + " bpc)");
                    }
                    else
                    {
                        gpu_monitor_bit_deep_list.Add(dm.dmBitsPerPel + " Bit");
                    }
                    string monitorSelectorMessage = string.Format("{0} #{1} - {2}", software_lang.TSReadLangs("Gpu_Content", "gpu_c_monitor_select"), i + 1, gpu_monitor_user_friendly_name_list[i]);
                    GPU_MonitorSelector_List.AddItemSafe(monitorSelectorMessage);
                    if (screen.Primary)
                    {
                        int primaryIndex = i;
                        GPU_MonitorSelector_List.ExecuteSafe(() =>
                        {
                            if (GPU_MonitorSelector_List.Items.Count > primaryIndex)
                            {
                                GPU_MonitorSelector_List.SelectedIndex = primaryIndex;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            Task directXGpuInfoTask = Task.CompletedTask;
            try
            {
                // GET DIRECTX GPU INFO
                gpu_vram_list.Clear();
                gpu_dxfl_list.Clear();
                gpu_dxfl_max_list.Clear();
                directXGpuInfoTask = LoadDirectXGPUInfo(gpuSelectorItems.ToArray());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            // GPU SELECT
            try
            {
                GPU_Selector_List.ExecuteSafe(() =>
                {
                    if (GPU_Selector_List.Items.Count > 0)
                    {
                        GPU_Selector_List.SelectedIndex = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            // GPU NAVIGATION AND PAGE ENABLED WHILE DIRECTX INFO LOADS
            GPU_RotateBtn.SetEnabledSafe(true);
            GPU.SetEnabledSafe(true);
            try
            {
                await directXGpuInfoTask;
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Gpu()"); }
            }
            if (debug_status)
            {
                TSLogger.Log("<--- GPU Section Loaded --->");
            }
        }
        // GPU VRAM
        // ======================================================================================================
        private async Task LoadDirectXGPUInfo(IReadOnlyList<string> gpuSelectorItems)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            //
            int n = gpuSelectorItems?.Count ?? 0;
            while (gpu_vram_list.Count < n) gpu_vram_list.Add(string.Empty);
            while (gpu_dxfl_list.Count < n) gpu_dxfl_list.Add(string.Empty);
            while (gpu_dxfl_max_list.Count < n) gpu_dxfl_max_list.Add(string.Empty);
            //
            await Task.Run(() =>
            {
                string dxdiagXml = Path.Combine(Path.GetTempPath(), $"dxdiag_{Guid.NewGuid()}.xml");
                //
                try
                {
                    var proc = new Process
                    {
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
                    for (int di = 0; di < displayNodes.Count; di++)
                    {
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
                        if (!string.IsNullOrWhiteSpace(dxfl))
                        {
                            int incMajor = -1, incMinor = -1, incCount = 0;
                            {
                                string[] parts = dxfl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                incCount = parts.Length;
                                for (int i = 0; i < parts.Length; i++)
                                {
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
                                    if (maj > incMajor || (maj == incMajor && min > incMinor))
                                    {
                                        incMajor = maj;
                                        incMinor = min;
                                    }
                                }
                            }
                            //
                            int curMajor = -1, curMinor = -1, curCount = 0;
                            if (!string.IsNullOrWhiteSpace(agg.bestDxfl))
                            {
                                string[] parts = agg.bestDxfl.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                curCount = parts.Length;
                                for (int i = 0; i < parts.Length; i++)
                                {
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
                                    if (maj > curMajor || (maj == curMajor && min > curMinor))
                                    {
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
                    int currentCount = gpuSelectorItems?.Count ?? 0;
                    while (gpu_vram_list.Count < currentCount) gpu_vram_list.Add(string.Empty);
                    while (gpu_dxfl_list.Count < currentCount) gpu_dxfl_list.Add(string.Empty);
                    while (gpu_dxfl_max_list.Count < currentCount) gpu_dxfl_max_list.Add(string.Empty);
                    foreach (var kv in gpuAgg)
                    {
                        string gpuKey = kv.Key;
                        string gpuName = gpuKey;
                        int bar = gpuKey.IndexOf('|');
                        if (bar > 0) gpuName = gpuKey.Substring(0, bar);
                        string dxDevId = (bar > 0 && bar + 1 < gpuKey.Length) ? gpuKey.Substring(bar + 1) : string.Empty;
                        if (dxDevId.StartsWith("i:", StringComparison.OrdinalIgnoreCase)) dxDevId = string.Empty;
                        //
                        long vramBytes = kv.Value.vramMaxBytes;
                        string bestDxfl = kv.Value.bestDxfl;
                        //
                        int bestIndex = -1;
                        int bestScore = int.MaxValue;
                        //
                        var candidateIndices = new List<int>();
                        for (int i = 0; i < currentCount; i++)
                        {
                            string itemNameRaw = gpuSelectorItems[i] ?? "";
                            string itemBase = StripGpuIndexSuffix(itemNameRaw);
                            if (itemBase.Equals(gpuName, StringComparison.OrdinalIgnoreCase))
                                candidateIndices.Add(i);
                        }
                        //
                        if (candidateIndices.Count == 1)
                        {
                            int only = candidateIndices[0];
                            bool slotFilled = (only >= 0 && only < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[only])) || (only >= 0 && only < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[only]));
                            if (slotFilled) continue;
                        }
                        //
                        if (candidateIndices.Count > 0)
                        {
                            string dxSig = ExtractPciSignature(dxDevId);
                            if (!string.IsNullOrEmpty(dxSig))
                            {
                                for (int k = 0; k < candidateIndices.Count; k++)
                                {
                                    int idx = candidateIndices[k];
                                    if (idx >= 0 && idx < gpu_vram_list.Count && idx < gpu_device_id_list.Count && string.IsNullOrWhiteSpace(gpu_vram_list[idx]))
                                    {
                                        string wmiSig = ExtractPciSignature(gpu_device_id_list[idx]);
                                        if (string.Equals(wmiSig, dxSig, StringComparison.OrdinalIgnoreCase))
                                        {
                                            bestIndex = idx;
                                            break;
                                        }
                                    }
                                }
                            }
                            //
                            if (bestIndex < 0)
                            {
                                for (int k = 0; k < candidateIndices.Count; k++)
                                {
                                    int idx = candidateIndices[k];
                                    if (idx >= 0 && idx < gpu_vram_list.Count && string.IsNullOrWhiteSpace(gpu_vram_list[idx]))
                                    {
                                        bestIndex = idx;
                                        break;
                                    }
                                }
                            }
                            //
                            if (bestIndex < 0)
                            {
                                string a = gpuName.ToLowerInvariant();
                                for (int k = 0; k < candidateIndices.Count; k++)
                                {
                                    int i = candidateIndices[k];
                                    string itemNameRaw = gpuSelectorItems[i] ?? "";
                                    string itemLower = itemNameRaw.ToLowerInvariant();
                                    if (Math.Abs(a.Length - itemLower.Length) >= bestScore) continue;
                                    int score = LevenshteinDistance(a, itemLower);
                                    if (score < bestScore)
                                    {
                                        bestScore = score;
                                        bestIndex = i;
                                    }
                                }
                            }
                        }
                        else
                        {
                            string a = gpuName.ToLowerInvariant();
                            for (int i = 0; i < currentCount; i++)
                            {
                                string itemName = gpuSelectorItems[i] ?? "";
                                string itemLower = itemName.ToLowerInvariant();
                                if (Math.Abs(a.Length - itemLower.Length) >= bestScore) continue;
                                int score = LevenshteinDistance(a, itemLower);
                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestIndex = i;
                                }
                            }
                        }
                        if (bestIndex >= 0 && bestIndex < gpu_vram_list.Count && bestIndex < gpu_dxfl_list.Count && bestIndex < gpu_dxfl_max_list.Count)
                        {
                            string dynamicStr = TS_FormatSize(vramBytes);
                            double roundedBytes = 0;
                            //
                            if (vramBytes >= 1024 * 1024 * 1024)
                            {
                                double vramGB = vramBytes / (1024.0 * 1024 * 1024);
                                double totalGBRounded = Math.Round(vramGB, 0, MidpointRounding.AwayFromZero);
                                roundedBytes = totalGBRounded * 1024 * 1024 * 1024;
                            }
                            else
                            {
                                double vramMB = vramBytes / (1024.0 * 1024);
                                double totalMBRounded = Math.Round(vramMB, 0, MidpointRounding.AwayFromZero);
                                roundedBytes = totalMBRounded * 1024 * 1024;
                            }
                            //
                            string totalStr = TS_FormatSize(roundedBytes);
                            if (totalStr != dynamicStr)
                            {
                                string available_string = software_lang.TSReadLangs("Gpu_Content", "gpu_c_available");
                                gpu_vram_list[bestIndex] = string.Format("{0} ({1} {2})", totalStr, available_string, dynamicStr);
                            }
                            else
                            {
                                gpu_vram_list[bestIndex] = totalStr;
                            }
                            //
                            if (!string.IsNullOrWhiteSpace(bestDxfl))
                            {
                                gpu_dxfl_list[bestIndex] = bestDxfl;
                                string maxFl = bestDxfl.Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                                int secondDot = maxFl.IndexOf('.', maxFl.IndexOf('.') + 1);
                                if (secondDot > 0) maxFl = maxFl.Substring(0, secondDot);
                                gpu_dxfl_max_list[bestIndex] = maxFl;
                            }
                            else
                            {
                                string unk = software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                                gpu_dxfl_list[bestIndex] = unk;
                                gpu_dxfl_max_list[bestIndex] = unk;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "LoadDirectXGPUInfo()"); }
                }
                finally
                {
                    try { if (File.Exists(dxdiagXml)) File.Delete(dxdiagXml); } catch { }
                }
            });
            GPU_VRAM_V.ExecuteSafe(() =>
            {
                int gpu_select = GPU_Selector_List.SelectedIndex;
                GPU_VRAM_V.Text = (gpu_select >= 0 && gpu_select < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[gpu_select])) ? gpu_vram_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                GPU_DirectXAll_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[gpu_select])) ? gpu_dxfl_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
                GPU_DirectXMax_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_max_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_max_list[gpu_select])) ? gpu_dxfl_max_list[gpu_select] : software_lang.TSReadLangs("Gpu_Content", "gpu_c_unknown");
            });
        }
        // VRAM AND FEATURE LEVELS PARSER ALGORITHM
        // ======================================================================================================
        private static string ExtractPciSignature(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return null;
            Match ven = Regex.Match(deviceId, @"VEN_([0-9A-Fa-f]{4})");
            if (!ven.Success) return null;
            Match dev = Regex.Match(deviceId, @"DEV_([0-9A-Fa-f]{4})");
            if (!dev.Success) return null;
            string sig = ("VEN_" + ven.Groups[1].Value + "&DEV_" + dev.Groups[1].Value).ToUpperInvariant();
            Match subsys = Regex.Match(deviceId, @"SUBSYS_([0-9A-Fa-f]{8})");
            if (subsys.Success)
                sig += ("&SUBSYS_" + subsys.Groups[1].Value).ToUpperInvariant();
            return sig;
        }
        private static string StripGpuIndexSuffix(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            int hash = s.LastIndexOf('#');
            if (hash > 0)
            {
                string tail = s.Substring(hash + 1).Trim();
                if (tail.Length > 0)
                {
                    bool allDigits = true;
                    for (int i = 0; i < tail.Length; i++)
                    {
                        if (!char.IsDigit(tail[i])) { allDigits = false; break; }
                    }
                    if (allDigits)
                        return s.Substring(0, hash).TrimEnd();
                }
            }
            return s.Trim();
        }
        private static bool TryParseMb(string text, out int mb)
        {
            mb = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();
            if (!text.EndsWith("MB", StringComparison.OrdinalIgnoreCase)) return false;
            string num = text.Substring(0, text.Length - 2).Trim();
            return int.TryParse(num, out mb);
        }
        private static string ExtractFeatureLevelsFromDisplayDevice(XmlNode displayDeviceNode)
        {
            if (displayDeviceNode == null) return string.Empty;
            return displayDeviceNode.SelectSingleNode("FeatureLevels")?.InnerText?.Trim() ?? displayDeviceNode.SelectSingleNode("Feature Levels")?.InnerText?.Trim() ?? string.Empty;
        }
        private static string NormalizeFeatureLevels(string raw)
        {
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
        private int LevenshteinDistance(string a, string b)
        {
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;
            if (a.Length < b.Length)
            {
                (b, a) = (a, b);
            }
            int[] prev = new int[b.Length + 1];
            int[] curr = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++) prev[j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                    curr[j] = Math.Min(Math.Min(prev[j] + 1, curr[j - 1] + 1), prev[j - 1] + cost);
                }
                (curr, prev) = (prev, curr);
            }
            return prev[b.Length];
        }
        // WEEKLY NUMBER CONVERTER
        // ======================================================================================================
        static string TSGetFormattedWeekInfo(int year, int weekNumber)
        {
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
        private void GPU_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int gpu_select = GPU_Selector_List.SelectedIndex;
                if (gpu_select < 0 || gpu_select >= GPU_Selector_List.Items.Count) return;
                try
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    string loading = software_lang.TSReadLangs("Gpu_Content", "gpu_c_loading");
                    GPU_Manufacturer_V.Text = (gpu_select < gpu_man_list.Count) ? gpu_man_list[gpu_select] : string.Empty;
                    GPU_VRAM_V.Text = (gpu_select >= 0 && gpu_select < gpu_vram_list.Count && !string.IsNullOrWhiteSpace(gpu_vram_list[gpu_select]) && !string.Equals(gpu_vram_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_vram_list[gpu_select] : loading;
                    GPU_DirectXAll_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_list[gpu_select]) && !string.Equals(gpu_dxfl_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_dxfl_list[gpu_select] : loading;
                    GPU_DirectXMax_V.Text = (gpu_select >= 0 && gpu_select < gpu_dxfl_max_list.Count && !string.IsNullOrWhiteSpace(gpu_dxfl_max_list[gpu_select]) && !string.Equals(gpu_dxfl_max_list[gpu_select], loading, StringComparison.Ordinal)) ? gpu_dxfl_max_list[gpu_select] : loading;
                }
                catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_Version_V.Text = gpu_driver_version_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DriverDate_V.Text = gpu_driver_date_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_Status_V.Text = gpu_status_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DeviceID_V.Text = gpu_device_id_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_DacType_V.Text = gpu_dac_type_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_GraphicDriversName_V.Text = gpu_drivers_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_InfFileName_V.Text = gpu_inf_file_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_INFSectionFile_V.Text = gpu_inf_file_section_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
                try { GPU_CurrentColor_V.Text = gpu_current_colors_list[gpu_select]; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
        }
        private void GPU_MonitorSelector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int monitor_select = GPU_MonitorSelector_List.SelectedIndex;
                if (monitor_select < 0 || monitor_select >= GPU_MonitorSelector_List.Items.Count) return;
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
            }
            catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "GPU_Selector_List_SelectedIndexChanged()"); } }
        }
        // FORMATED SCREEN INFO
        // ======================================================================================================
        public static string GetMonitorFromUShortArray(ushort[] array)
        {
            if (array == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var v in array)
            {
                if (v > 0 && v <= char.MaxValue)
                    sb.Append((char)v);
            }
            return sb.ToString().Trim();
        }
        private static int GetEdidPanelBitsPerColor(string wmiInstanceName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(wmiInstanceName)) return 0;
                string norm = wmiInstanceName.Trim();
                string[] parts = norm.Split('\\');
                if (parts.Length < 3) return 0;
                string vendor = parts[1].Trim();
                string device = parts[2].Trim();
                int uscore = device.LastIndexOf('_');
                if (uscore > 0) device = device.Substring(0, uscore);
                if (string.IsNullOrEmpty(vendor) || string.IsNullOrEmpty(device)) return 0;
                string subPath = @"SYSTEM\CurrentControlSet\Enum\DISPLAY\" + vendor + @"\" + device + @"\Device Parameters";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subPath, false))
                {
                    if (key == null) return 0;
                    if (!(key.GetValue("EDID") is byte[] edid) || edid.Length < 0x15) return 0;
                    byte b = edid[0x14];
                    if ((b & 0x80) == 0) return 0;
                    int code = (b >> 4) & 0x07;
                    switch (code)
                    {
                        case 0: return 8;
                        case 1: return 6;
                        case 2: return 8;
                        case 3: return 10;
                        case 4: return 12;
                        case 5: return 14;
                        case 6: return 16;
                        default: return 0;
                    }
                }
            }
            catch { return 0; }
        }
        private string GPUFormatScreenInfo(Rectangle bounds)
        {
            return $"{bounds.Width} x {bounds.Height} (X: {bounds.X}, Y: {bounds.Y})";
        }
        private string GPUFormatPrimaryScreen(bool isPrimary)
        {
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
        private void Disk()
        {
            // DISK COUNTER
            int disk_ssd_count = 0;
            int disk_hdd_count = 0;
            int disk_usb_count = 0;
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string Normalize(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                s = s.Trim().ToLowerInvariant().Replace("ı", "i");
                var sb = new StringBuilder(s.Length);
                bool prevSpace = false;
                foreach (char ch in s)
                {
                    char c = ch;
                    if (c == '_' || c == '-' || c == '/' || c == '\\' || c == '.' || c == ',' || c == ';' || c == ':' || c == '(' || c == ')' || c == '[' || c == ']') c = ' ';
                    if (char.IsWhiteSpace(c))
                    {
                        if (!prevSpace)
                        {
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
            bool ContainsIgnoreCase(string haystack, string needle)
            {
                if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(needle)) return false;
                return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            bool IsUselessManufacturer(string manNorm)
            {
                if (string.IsNullOrEmpty(manNorm)) return true;
                string[] junk = { "nvme", "usb", "ata", "sata", "scsi", "msft", "microsoft", "standard", "generic" };
                for (int k = 0; k < junk.Length; k++)
                {
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
                new KeyValuePair<string[], string>(new[]{ "biostar" }, "Biostar"),
                new KeyValuePair<string[], string>(new[]{ "buffalo" }, "Buffalo"),
                new KeyValuePair<string[], string>(new[]{ "busbi" }, "Busbi"),
                new KeyValuePair<string[], string>(new[]{ "buslink" }, "Buslink"),
                new KeyValuePair<string[], string>(new[]{ "centon" }, "Centon"),
                new KeyValuePair<string[], string>(new[]{ "cm storm", "cmstorm" }, "CM Storm"),
                new KeyValuePair<string[], string>(new[]{ "codegen" }, "Codegen"),
                new KeyValuePair<string[], string>(new[]{ "colorful" }, "Colorful"),
                new KeyValuePair<string[], string>(new[]{ "corsair" }, "Corsair"),
                new KeyValuePair<string[], string>(new[]{ "crucial" }, "Crucial"),
                new KeyValuePair<string[], string>(new[]{ "datawrite" }, "DataWrite"),
                new KeyValuePair<string[], string>(new[]{ "duracell" }, "Duracell"),
                new KeyValuePair<string[], string>(new[]{ "dynamode" }, "Dynamode"),
                new KeyValuePair<string[], string>(new[]{ "emtec" }, "Emtec"),
                new KeyValuePair<string[], string>(new[]{ "excelstor" }, "ExcelStor"),
                new KeyValuePair<string[], string>(new[]{ "ezcool" }, "EZcool"),
                new KeyValuePair<string[], string>(new[]{ "fuji" }, "Fuji"),
                new KeyValuePair<string[], string>(new[]{ "fujitsu" }, "Fujitsu"),
                new KeyValuePair<string[], string>(new[]{ "geil" }, "GeIL"),
                new KeyValuePair<string[], string>(new[]{ "gigabyte" }, "Gigabyte"),
                new KeyValuePair<string[], string>(new[]{ "goodram" }, "Goodram"),
                new KeyValuePair<string[], string>(new[]{ "hi level", "hi-level" }, "Hi-Level"),
                new KeyValuePair<string[], string>(new[]{ "hikvision" }, "Hikvision"),
                new KeyValuePair<string[], string>(new[]{ "hitachi" }, "Hitachi"),
                new KeyValuePair<string[], string>(new[]{ "hp" }, "HP"),
                new KeyValuePair<string[], string>(new[]{ "imation" }, "Imation"),
                new KeyValuePair<string[], string>(new[]{ "intel" }, "Intel"),
                new KeyValuePair<string[], string>(new[]{ "intenso" }, "Intenso"),
                new KeyValuePair<string[], string>(new[]{ "iomega" }, "Iomega"),
                new KeyValuePair<string[], string>(new[]{ "james donkey" }, "James Donkey"),
                new KeyValuePair<string[], string>(new[]{ "kanguru" }, "Kanguru"),
                new KeyValuePair<string[], string>(new[]{ "kingmax" }, "Kingmax"),
                new KeyValuePair<string[], string>(new[]{ "kingston" }, "Kingston"),
                new KeyValuePair<string[], string>(new[]{ "kingspec" }, "Kingspec"),
                new KeyValuePair<string[], string>(new[]{ "kioxia" }, "Kioxia"),
                new KeyValuePair<string[], string>(new[]{ "kodak" }, "Kodak"),
                new KeyValuePair<string[], string>(new[]{ "lacie" }, "LaCie"),
                new KeyValuePair<string[], string>(new[]{ "lenovo" }, "Lenovo"),
                new KeyValuePair<string[], string>(new[]{ "leven" }, "Leven"),
                new KeyValuePair<string[], string>(new[]{ "lexar" }, "Lexar"),
                new KeyValuePair<string[], string>(new[]{ "liteon", "lite-on" }, "Lite-On"),
                new KeyValuePair<string[], string>(new[]{ "maxtor" }, "Maxtor"),
                new KeyValuePair<string[], string>(new[]{ "maxell" }, "Maxell"),
                new KeyValuePair<string[], string>(new[]{ "memorex" }, "Memorex"),
                new KeyValuePair<string[], string>(new[]{ "micron" }, "Micron"),
                new KeyValuePair<string[], string>(new[]{ "minox" }, "Minox"),
                new KeyValuePair<string[], string>(new[]{ "mio" }, "Mio"),
                new KeyValuePair<string[], string>(new[]{ "mld" }, "MLD"),
                new KeyValuePair<string[], string>(new[]{ "msi" }, "MSI"),
                new KeyValuePair<string[], string>(new[]{ "mushkin" }, "Mushkin"),
                new KeyValuePair<string[], string>(new[]{ "nec" }, "NEC"),
                new KeyValuePair<string[], string>(new[]{ "neo forza", "neoforza" }, "Neo Forza"),
                new KeyValuePair<string[], string>(new[]{ "netac" }, "Netac"),
                new KeyValuePair<string[], string>(new[]{ "ocz" }, "OCZ"),
                new KeyValuePair<string[], string>(new[]{ "olympus" }, "Olympus"),
                new KeyValuePair<string[], string>(new[]{ "optiarc" }, "Optiarc"),
                new KeyValuePair<string[], string>(new[]{ "panasonic" }, "Panasonic"),
                new KeyValuePair<string[], string>(new[]{ "patriot" }, "Patriot"),
                new KeyValuePair<string[], string>(new[]{ "philips" }, "Philips"),
                new KeyValuePair<string[], string>(new[]{ "pinnacle" }, "Pinnacle"),
                new KeyValuePair<string[], string>(new[]{ "pioneer" }, "Pioneer"),
                new KeyValuePair<string[], string>(new[]{ "plextor" }, "Plextor"),
                new KeyValuePair<string[], string>(new[]{ "pny", "pnypqi" }, "PNY"),
                new KeyValuePair<string[], string>(new[]{ "qnap" }, "QNAP"),
                new KeyValuePair<string[], string>(new[]{ "quantum" }, "Quantum"),
                new KeyValuePair<string[], string>(new[]{ "ramsta" }, "Ramsta"),
                new KeyValuePair<string[], string>(new[]{ "ricoh" }, "Ricoh"),
                new KeyValuePair<string[], string>(new[]{ "rokit" }, "Rokit"),
                new KeyValuePair<string[], string>(new[]{ "samsung" }, "Samsung"),
                new KeyValuePair<string[], string>(new[]{ "sandisk" }, "SanDisk"),
                new KeyValuePair<string[], string>(new[]{ "seagate" }, "Seagate"),
                new KeyValuePair<string[], string>(new[]{ "sigma" }, "Sigma"),
                new KeyValuePair<string[], string>(new[]{ "siliconpower", "silicon power" }, "Silicon Power"),
                new KeyValuePair<string[], string>(new[]{ "sk hynix", "skhynix", "hynix" }, "SK hynix"),
                new KeyValuePair<string[], string>(new[]{ "smartbuy" }, "SmartBuy"),
                new KeyValuePair<string[], string>(new[]{ "sony" }, "Sony"),
                new KeyValuePair<string[], string>(new[]{ "super talent", "supertalent" }, "Super Talent"),
                new KeyValuePair<string[], string>(new[]{ "team" }, "Team"),
                new KeyValuePair<string[], string>(new[]{ "teac" }, "TEAC"),
                new KeyValuePair<string[], string>(new[]{ "toshiba" }, "Toshiba"),
                new KeyValuePair<string[], string>(new[]{ "transcend" }, "Transcend"),
                new KeyValuePair<string[], string>(new[]{ "traxdata" }, "Traxdata"),
                new KeyValuePair<string[], string>(new[]{ "trimble" }, "Trimble"),
                new KeyValuePair<string[], string>(new[]{ "turbox" }, "Turbox"),
                new KeyValuePair<string[], string>(new[]{ "verbatim" }, "Verbatim"),
                new KeyValuePair<string[], string>(new[]{ "verico" }, "Verico"),
                new KeyValuePair<string[], string>(new[]{ "viking" }, "Viking"),
                new KeyValuePair<string[], string>(new[]{ "western digital", "wd black", "wd_black", "wdblack", "wd" }, "Western Digital"),
                new KeyValuePair<string[], string>(new[]{ "xerox" }, "Xerox"),
                new KeyValuePair<string[], string>(new[]{ "xmedia" }, "XMedia"),
                new KeyValuePair<string[], string>(new[]{ "zotac" }, "Zotac"),
                new KeyValuePair<string[], string>(new[]{ "zoostorm" }, "Zoostorm")
            };
            var brandRulesNorm = new List<KeyValuePair<string[], string>>(brandRules.Count);
            foreach (var bre in brandRules)
            {
                var normAliases = new string[bre.Key.Length];
                for (int nai = 0; nai < bre.Key.Length; nai++) normAliases[nai] = Normalize(bre.Key[nai]);
                brandRulesNorm.Add(new KeyValuePair<string[], string>(normAliases, bre.Value));
            }
            string DetectBrand(string modelNorm, string manNorm)
            {
                for (int r = 0; r < brandRulesNorm.Count; r++)
                {
                    var aliases = brandRulesNorm[r].Key;
                    var display = brandRulesNorm[r].Value;
                    for (int a = 0; a < aliases.Length; a++)
                    {
                        string an = aliases[a];
                        if (!string.IsNullOrEmpty(manNorm) && ContainsIgnoreCase(manNorm, an))
                            return display;
                        if (!string.IsNullOrEmpty(modelNorm) && ContainsIgnoreCase(modelNorm, an))
                            return display;
                    }
                }
                return string.Empty;
            }
            int ParseDiskIndexFromAssocPath(string assocPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(assocPath)) return -1;
                    int h = assocPath.IndexOf("Disk #", StringComparison.OrdinalIgnoreCase);
                    if (h < 0) return -1;
                    int s = h + 6;
                    int e = s;
                    while (e < assocPath.Length && char.IsDigit(assocPath[e])) e++;
                    if (e > s && int.TryParse(assocPath.Substring(s, e - s), out int di)) return di;
                }
                catch { }
                return -1;
            }
            string ExtractQuotedId(string assocPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(assocPath)) return null;
                    int q1 = assocPath.IndexOf("=\"", StringComparison.Ordinal);
                    if (q1 < 0) return null;
                    int q2 = assocPath.IndexOf("\"", q1 + 2, StringComparison.Ordinal);
                    if (q2 <= q1 + 1) return null;
                    return assocPath.Substring(q1 + 2, q2 - q1 - 2).Trim();
                }
                catch { return null; }
            }
            string ExtractStorageObjectId(string assocPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(assocPath)) return null;
                    int m = assocPath.IndexOf("ObjectId=\"", StringComparison.Ordinal);
                    if (m < 0) return null;
                    string inner = assocPath.Substring(m + 10);
                    if (inner.EndsWith("\"", StringComparison.Ordinal)) inner = inner.Substring(0, inner.Length - 1);
                    if (inner.Length == 0) return null;
                    var sb = new StringBuilder(inner.Length);
                    int i = 0;
                    while (i < inner.Length)
                    {
                        if (inner[i] == '\\' && i + 1 < inner.Length && (inner[i + 1] == '\\' || inner[i + 1] == '"'))
                        {
                            sb.Append(inner[i + 1]);
                            i += 2;
                        }
                        else
                        {
                            sb.Append(inner[i]);
                            i++;
                        }
                    }
                    string result = sb.ToString();
                    return string.IsNullOrEmpty(result) ? null : result;
                }
                catch { return null; }
            }
            Dictionary<int, List<ManagementObject>> bulkLogicalByDisk = null;
            Dictionary<int, List<ManagementObject>> bulkVolumesByDisk = null;
            var bulkDiskOwned = new List<ManagementObject>();
            List<ManagementObject> bulkPhysDisks = null;
            try
            {
                var seenDisks = new HashSet<int>();
                try
                {
                    var diskIndexToLogicalIds = new Dictionary<int, List<string>>();
                    using (var assocLP = new ManagementObjectSearcher("root\\CIMV2", "SELECT Antecedent, Dependent FROM Win32_LogicalDiskToPartition"))
                    using (var assocLPCol = assocLP.Get())
                    {
                        foreach (ManagementObject assoc in assocLPCol.Cast<ManagementObject>())
                        {
                            using (assoc)
                            {
                                string ant = Convert.ToString(assoc["Antecedent"]) ?? string.Empty;
                                string dep = Convert.ToString(assoc["Dependent"]) ?? string.Empty;
                                int di = ParseDiskIndexFromAssocPath(ant);
                                string lid = ExtractQuotedId(dep);
                                if (di >= 0 && !string.IsNullOrEmpty(lid))
                                {
                                    if (!diskIndexToLogicalIds.TryGetValue(di, out var idList)) diskIndexToLogicalIds[di] = idList = new List<string>();
                                    if (!idList.Contains(lid, StringComparer.OrdinalIgnoreCase)) idList.Add(lid);
                                }
                            }
                        }
                    }
                    if (diskIndexToLogicalIds.Count > 0)
                    {
                        var logicalById = new Dictionary<string, ManagementObject>(StringComparer.OrdinalIgnoreCase);
                        using (var allLD = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDisk"))
                        using (var allLDCol = allLD.Get())
                        {
                            foreach (ManagementObject ld in allLDCol.Cast<ManagementObject>())
                            {
                                string id = (Convert.ToString(ld["DeviceID"]) ?? string.Empty).Trim();
                                if (!string.IsNullOrEmpty(id) && !logicalById.ContainsKey(id))
                                {
                                    logicalById[id] = ld;
                                    bulkDiskOwned.Add(ld);
                                }
                                else { try { ld.Dispose(); } catch { } }
                            }
                        }
                        bulkLogicalByDisk = new Dictionary<int, List<ManagementObject>>();
                        foreach (var kv in diskIndexToLogicalIds)
                        {
                            var objList = new List<ManagementObject>();
                            foreach (var lid in kv.Value)
                            {
                                if (logicalById.TryGetValue(lid, out var lo)) objList.Add(lo);
                            }
                            if (objList.Count > 0) bulkLogicalByDisk[kv.Key] = objList;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "Disk() - bulk CIMV2 prefetch"); }
                    bulkLogicalByDisk = null;
                }
                try
                {
                    var partDiskByObjectId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    using (var allPart = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT DiskNumber, ObjectId FROM MSFT_Partition"))
                    using (var allPartCol = allPart.Get())
                    {
                        foreach (ManagementObject p in allPartCol.Cast<ManagementObject>())
                        {
                            using (p)
                            {
                                string oid = (Convert.ToString(p["ObjectId"]) ?? string.Empty).Trim();
                                int dn = -1;
                                try { dn = Convert.ToInt32(p["DiskNumber"]); } catch { }
                                if (!string.IsNullOrEmpty(oid) && dn >= 0 && !partDiskByObjectId.ContainsKey(oid)) partDiskByObjectId[oid] = dn;
                            }
                        }
                    }
                    if (partDiskByObjectId.Count > 0)
                    {
                        var volDiskByObjectId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        using (var assocPV = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT Partition, Volume FROM MSFT_PartitionToVolume"))
                        using (var assocPVCol = assocPV.Get())
                        {
                            foreach (ManagementObject assoc in assocPVCol.Cast<ManagementObject>())
                            {
                                using (assoc)
                                {
                                    string partOid = ExtractStorageObjectId(Convert.ToString(assoc["Partition"]) ?? string.Empty);
                                    string volOid = ExtractStorageObjectId(Convert.ToString(assoc["Volume"]) ?? string.Empty);
                                    if (!string.IsNullOrEmpty(partOid) && !string.IsNullOrEmpty(volOid) && partDiskByObjectId.TryGetValue(partOid, out int dn) && !volDiskByObjectId.ContainsKey(volOid))
                                        volDiskByObjectId[volOid] = dn;
                                }
                            }
                        }
                        if (volDiskByObjectId.Count > 0)
                        {
                            bulkVolumesByDisk = new Dictionary<int, List<ManagementObject>>();
                            using (var allVol = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Volume"))
                            using (var allVolCol = allVol.Get())
                            {
                                foreach (ManagementObject volume in allVolCol.Cast<ManagementObject>())
                                {
                                    string oid = (Convert.ToString(volume["ObjectId"]) ?? string.Empty).Trim();
                                    if (!string.IsNullOrEmpty(oid) && volDiskByObjectId.TryGetValue(oid, out int dn))
                                    {
                                        if (!bulkVolumesByDisk.TryGetValue(dn, out var vList)) bulkVolumesByDisk[dn] = vList = new List<ManagementObject>();
                                        vList.Add(volume);
                                        bulkDiskOwned.Add(volume);
                                    }
                                    else { try { volume.Dispose(); } catch { } }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "Disk() - bulk Storage prefetch"); }
                    bulkVolumesByDisk = null;
                }
                try
                {
                    var physList = new List<ManagementObject>();
                    using (var allPhys = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT DeviceID, MediaType, SerialNumber, FriendlyName FROM MSFT_PhysicalDisk"))
                    using (var allPhysCol = allPhys.Get())
                    {
                        foreach (ManagementObject pd in allPhysCol.Cast<ManagementObject>())
                        {
                            physList.Add(pd);
                        }
                    }
                    if (physList.Count > 0) bulkPhysDisks = physList;
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "Disk() - bulk PhysicalDisk prefetch"); }
                    bulkPhysDisks = null;
                }
                using (var get_drives = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive"))
                using (var drivesCol = get_drives.Get())
                {
                    foreach (ManagementObject drive_info in drivesCol.Cast<ManagementObject>())
                    {
                        using (drive_info)
                        {
                            int diskIndex;
                            try { diskIndex = Convert.ToInt32(drive_info["Index"]); }
                            catch { continue; }
                            if (!seenDisks.Add(diskIndex))
                                continue;
                            var logicalDisks = new List<ManagementObject>();
                            var storageVolumes = new List<ManagementObject>();
                            List<ManagementObject> bulkL = null;
                            bulkLogicalByDisk?.TryGetValue(diskIndex, out bulkL);
                            if (bulkL != null && bulkL.Count > 0)
                            {
                                logicalDisks.AddRange(bulkL);
                            }
                            else
                                try
                                {
                                    var disk_part_text_query = $"associators of {{{drive_info.Path.RelativePath}}} where AssocClass = Win32_DiskDriveToDiskPartition";
                                    using (var disk_part_query = new ManagementObjectSearcher(disk_part_text_query))
                                    using (var partCol = disk_part_query.Get())
                                    {
                                        foreach (ManagementObject disk_partition in partCol.Cast<ManagementObject>())
                                        {
                                            using (disk_partition)
                                            {
                                                var logical_disk_text_query = $"associators of {{{disk_partition.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition";
                                                using (var logical_disk_query = new ManagementObjectSearcher(logical_disk_text_query))
                                                using (var ldCol = logical_disk_query.Get())
                                                {
                                                    logicalDisks.AddRange(ldCol.Cast<ManagementObject>());
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
                                }
                            var seenVolumes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            List<ManagementObject> bulkV = null;
                            bulkVolumesByDisk?.TryGetValue(diskIndex, out bulkV);
                            if (bulkV != null && bulkV.Count > 0)
                            {
                                foreach (ManagementObject volume in bulkV)
                                {
                                    string volumeKey = Convert.ToString(volume["ObjectId"])?.Trim();
                                    if (string.IsNullOrEmpty(volumeKey))
                                        volumeKey = Convert.ToString(volume["Path"])?.Trim();
                                    if (!string.IsNullOrEmpty(volumeKey) && seenVolumes.Add(volumeKey))
                                        storageVolumes.Add(volume);
                                }
                            }
                            else
                                try
                                {
                                    using (var partitionQuery = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT * FROM MSFT_Partition WHERE DiskNumber={diskIndex}"))
                                    using (var msftPartCol = partitionQuery.Get())
                                    {
                                        foreach (ManagementObject partition in msftPartCol.Cast<ManagementObject>())
                                        {
                                            using (partition)
                                            {
                                                var volumeQueryText = $"associators of {{{partition.Path.RelativePath}}} where AssocClass = MSFT_PartitionToVolume";
                                                using (var volumeQuery = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", volumeQueryText))
                                                using (var volCol = volumeQuery.Get())
                                                {
                                                    foreach (ManagementObject volume in volCol.Cast<ManagementObject>())
                                                    {
                                                        string volumeKey = Convert.ToString(volume["ObjectId"])?.Trim();
                                                        if (string.IsNullOrEmpty(volumeKey))
                                                            volumeKey = Convert.ToString(volume["Path"])?.Trim();
                                                        if (!string.IsNullOrEmpty(volumeKey) && seenVolumes.Add(volumeKey))
                                                            storageVolumes.Add(volume);
                                                        else
                                                        {
                                                            try { volume.Dispose(); } catch { }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Disk() - MSFT_Volume"); }
                                }
                            // DISK CAPTION
                            try
                            {
                                var disk_caption = Convert.ToString(drive_info["Caption"])?.Trim();
                                if (!string.IsNullOrEmpty(disk_caption))
                                    DISK_Selector_List.AddItemSafe(disk_caption);
                                else
                                    DISK_Selector_List.AddItemSafe(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch (Exception ex)
                            {
                                if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
                            }
                            // DISK MANUFACTURER & MODEL
                            try
                            {
                                using (var q = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT Model, Manufacturer FROM MSFT_Disk WHERE Number={diskIndex}"))
                                using (var msftDiskCol = q.Get())
                                {
                                    foreach (ManagementObject row in msftDiskCol.Cast<ManagementObject>())
                                    {
                                        using (row)
                                        {
                                            var modelRaw = Convert.ToString(row["Model"])?.Trim();
                                            var model = !string.IsNullOrEmpty(modelRaw) ? modelRaw : software_lang.TSReadLangs("StorageContent", "se_c_unknown");
                                            disk_model_list.Add(model);
                                            var manRaw = Convert.ToString(row["Manufacturer"])?.Trim() ?? string.Empty;
                                            string modelNorm = Normalize(modelRaw ?? "");
                                            string manNorm = Normalize(manRaw);
                                            if (IsUselessManufacturer(manNorm))
                                                manNorm = string.Empty;
                                            string detected = DetectBrand(modelNorm, manNorm);
                                            if (!string.IsNullOrEmpty(detected))
                                            {
                                                disk_man_list.Add(detected);
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(manRaw) && !IsUselessManufacturer(Normalize(manRaw)))
                                                    disk_man_list.Add(manRaw);
                                                else
                                                    disk_man_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                disk_model_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_man_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK VOLUME ID
                            try
                            {
                                var volumeIds = logicalDisks.Select(ld => Convert.ToString(ld["Name"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).Select(s => s + @"\").ToList();
                                disk_volume_id_list.Add(volumeIds.Count > 0 ? string.Join(", ", volumeIds) : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_volume_id_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK VOLUME NAME
                            try
                            {
                                var volumeNames = logicalDisks.Select(ld => Convert.ToString(ld["VolumeName"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                                var volumeNameSet = new HashSet<string>(volumeNames, StringComparer.OrdinalIgnoreCase);
                                volumeNames.AddRange(storageVolumes.Select(v => Convert.ToString(v["FileSystemLabel"])?.Trim()).Where(s => !string.IsNullOrEmpty(s) && volumeNameSet.Add(s)));
                                disk_volume_name_list.Add(volumeNames.Count > 0 ? string.Join(", ", volumeNames) : software_lang.TSReadLangs("StorageContent", "se_c_local_disk"));
                            }
                            catch
                            {
                                disk_volume_name_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_local_disk"));
                            }
                            // DISK FIRMWARE VERSION
                            try
                            {
                                var disk_firmware_version = Convert.ToString(drive_info["FirmwareRevision"])?.Trim();
                                disk_firmware_list.Add(!string.IsNullOrEmpty(disk_firmware_version) ? disk_firmware_version : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_firmware_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK SERIAL NUMBER
                            try
                            {
                                var disk_serial_number = new string((Convert.ToString(drive_info["SerialNumber"]) ?? string.Empty).Where(c => !char.IsControl(c)).ToArray()).Trim();
                                if (!string.IsNullOrEmpty(disk_serial_number))
                                {
                                    if (stealth_status != 1)
                                        disk_serial_list.Add(disk_serial_number);
                                    else
                                        disk_serial_list.Add(sm_text);
                                }
                                else
                                {
                                    disk_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                }
                            }
                            catch
                            {
                                disk_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK VOLUME SERIAL NUMBER
                            try
                            {
                                var volSerials = logicalDisks.Select(ld =>
                                {
                                    string raw = Convert.ToString(ld["VolumeSerialNumber"])?.Trim();
                                    if (!string.IsNullOrEmpty(raw) && raw.Length == 8)
                                        return raw.Insert(4, "-").ToUpper();
                                    return raw;
                                }).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                                if (volSerials.Count == 0)
                                {
                                    disk_volume_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                }
                                else
                                {
                                    if (stealth_status != 1)
                                        disk_volume_serial_list.Add(string.Join(", ", volSerials));
                                    else
                                        disk_volume_serial_list.Add(sm_text);
                                }
                            }
                            catch
                            {
                                disk_volume_serial_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK SPACE & FREE & USED
                            try
                            {
                                double diskSizeBytes = Convert.ToDouble(drive_info["Size"]);
                                double freeBytes = 0;
                                double usedBytes = 0;
                                if (storageVolumes.Count > 0)
                                {
                                    foreach (var volume in storageVolumes)
                                    {
                                        double vSize = 0, vRem = 0;
                                        try { vSize = Convert.ToDouble(volume["Size"]); } catch { vSize = 0; }
                                        try { vRem = Convert.ToDouble(volume["SizeRemaining"]); } catch { vRem = 0; }
                                        if (vSize < 0) vSize = 0;
                                        if (vRem < 0) vRem = 0;
                                        if (vRem > vSize) vRem = vSize;
                                        freeBytes += vRem;
                                        usedBytes += vSize - vRem;
                                    }
                                }
                                else
                                {
                                    foreach (var ld in logicalDisks)
                                    {
                                        double vSize = 0, vRem = 0;
                                        try { vSize = Convert.ToDouble(ld["Size"]); } catch { vSize = 0; }
                                        try { vRem = Convert.ToDouble(ld["FreeSpace"]); } catch { vRem = 0; }
                                        if (vSize < 0) vSize = 0;
                                        if (vRem < 0) vRem = 0;
                                        if (vRem > vSize) vRem = vSize;
                                        freeBytes += vRem;
                                        usedBytes += vSize - vRem;
                                    }
                                }
                                if (usedBytes < 0) usedBytes = 0;
                                disk_total_space_list.Add(TS_FormatSize(diskSizeBytes));
                                disk_used_space_list.Add(TS_FormatSize(usedBytes));
                                disk_free_space_list.Add(TS_FormatSize(freeBytes));
                            }
                            catch
                            {
                                disk_total_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_used_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_free_space_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK FILE SYSTEM
                            try
                            {
                                var fileSystems = logicalDisks.Select(ld => Convert.ToString(ld["FileSystem"])?.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                                var fileSystemSet = new HashSet<string>(fileSystems, StringComparer.OrdinalIgnoreCase);
                                fileSystems.AddRange(storageVolumes.Select(v => Convert.ToString(v["FileSystem"])?.Trim()).Where(s => !string.IsNullOrEmpty(s) && fileSystemSet.Add(s)));
                                disk_file_system_list.Add(fileSystems.Count > 0 ? string.Join(", ", fileSystems) : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_file_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK FORMATTING SYSTEM
                            try
                            {
                                bool fmtFound = false;
                                using (var disk_part_search = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT PartitionStyle FROM MSFT_Disk WHERE Number={diskIndex}"))
                                using (var partStyleCol = disk_part_search.Get())
                                {
                                    foreach (ManagementObject dp_search in partStyleCol.Cast<ManagementObject>())
                                    {
                                        using (dp_search)
                                        {
                                            var disk_part_style = Convert.ToInt32(dp_search["PartitionStyle"]);
                                            switch (disk_part_style)
                                            {
                                                case 0: disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); break;
                                                case 1: disk_formatting_system_list.Add("MBR"); break;
                                                case 2: disk_formatting_system_list.Add("GPT"); break;
                                                default: disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); break;
                                            }
                                            fmtFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!fmtFound) disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_formatting_system_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK TYPE + COUNTERS
                            try
                            {
                                var disk_media_type = Convert.ToString(drive_info["MediaType"])?.ToLower().Trim() ?? "";
                                string wmiDiskSerial = (Convert.ToString(drive_info["SerialNumber"]) ?? string.Empty).Trim();
                                string wmiDiskModel = (Convert.ToString(drive_info["Model"]) ?? string.Empty).Trim();
                                ManagementObject typeTarget = null;
                                ManagementObject typeFallbackById = null;
                                if (bulkPhysDisks != null)
                                {
                                    foreach (ManagementObject search_disk_t in bulkPhysDisks)
                                    {
                                        string candSerial = (Convert.ToString(search_disk_t["SerialNumber"]) ?? string.Empty).Trim();
                                        string candModel = (Convert.ToString(search_disk_t["FriendlyName"]) ?? string.Empty).Trim();
                                        int candId = -1;
                                        try { candId = Convert.ToInt32(search_disk_t["DeviceID"]); } catch { candId = -1; }
                                        bool isFallback = (candId == diskIndex && typeFallbackById == null);
                                        bool isTarget = (typeTarget == null && !string.IsNullOrEmpty(wmiDiskSerial) && candSerial.Equals(wmiDiskSerial, StringComparison.OrdinalIgnoreCase))
                                            || (typeTarget == null && !string.IsNullOrEmpty(wmiDiskModel) && candModel.Equals(wmiDiskModel, StringComparison.OrdinalIgnoreCase));
                                        if (isFallback && typeFallbackById == null) typeFallbackById = (ManagementObject)search_disk_t.Clone();
                                        else if (isTarget) typeTarget = (ManagementObject)search_disk_t.Clone();
                                    }
                                }
                                else
                                    using (var search_disk_type = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT DeviceID, MediaType, SerialNumber, FriendlyName FROM MSFT_PhysicalDisk"))
                                    using (var physCol = search_disk_type.Get())
                                    {
                                        foreach (ManagementObject search_disk_t in physCol.Cast<ManagementObject>())
                                        {
                                            string candSerial = (Convert.ToString(search_disk_t["SerialNumber"]) ?? string.Empty).Trim();
                                            string candModel = (Convert.ToString(search_disk_t["FriendlyName"]) ?? string.Empty).Trim();
                                            int candId = -1;
                                            try { candId = Convert.ToInt32(search_disk_t["DeviceID"]); } catch { candId = -1; }
                                            bool isFallback = (candId == diskIndex && typeFallbackById == null);
                                            bool isTarget = (typeTarget == null && !string.IsNullOrEmpty(wmiDiskSerial) && candSerial.Equals(wmiDiskSerial, StringComparison.OrdinalIgnoreCase))
                                                || (typeTarget == null && !string.IsNullOrEmpty(wmiDiskModel) && candModel.Equals(wmiDiskModel, StringComparison.OrdinalIgnoreCase));
                                            if (isFallback && typeFallbackById == null) typeFallbackById = search_disk_t;
                                            else if (isTarget) typeTarget = search_disk_t;
                                            else { try { search_disk_t.Dispose(); } catch { } }
                                        }
                                    }
                                ManagementObject typeRow = typeTarget ?? typeFallbackById;
                                try
                                {
                                    if (typeRow != null)
                                    {
                                        {
                                            var search_disk_t = typeRow;
                                            var disk_type = Convert.ToInt32(search_disk_t["MediaType"]);
                                            switch (disk_type)
                                            {
                                                case 0:
                                                    switch (disk_media_type)
                                                    {
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
                                            // DRIVE TYPE Text Mapping
                                            switch (disk_media_type)
                                            {
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
                                        }
                                    }
                                    else
                                    {
                                        disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                        disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                    }
                                }
                                finally
                                {
                                    try { typeTarget?.Dispose(); } catch { }
                                    if (!ReferenceEquals(typeFallbackById, typeTarget)) { try { typeFallbackById?.Dispose(); } catch { } }
                                }
                            }
                            catch
                            {
                                disk_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_drive_type_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK INTERFACE
                            try
                            {
                                bool ifaceFound = false;
                                using (var disk_interface = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT BusType FROM MSFT_Disk WHERE Number={diskIndex}"))
                                using (var busCol = disk_interface.Get())
                                {
                                    foreach (ManagementObject disk_int_query in busCol.Cast<ManagementObject>())
                                    {
                                        using (disk_int_query)
                                        {
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
                                    { 17, "NVMe" }
                                };
                                            if (busTypeMessages.TryGetValue(di_query, out string busTypeMessage))
                                                disk_interface_list.Add(busTypeMessage);
                                            else
                                                disk_interface_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                            ifaceFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!ifaceFound) disk_interface_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_interface_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK PARTITION COUNT
                            try
                            {
                                var disk_partitions = Convert.ToString(drive_info["Partitions"])?.Trim();
                                disk_partition_list.Add(!string.IsNullOrEmpty(disk_partitions) ? disk_partitions : software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            catch
                            {
                                disk_partition_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK MEDIA LOADED
                            try
                            {
                                var disk_media_loaded = Convert.ToBoolean(drive_info["MediaLoaded"]);
                                disk_media_loaded_list.Add(disk_media_loaded ? software_lang.TSReadLangs("StorageContent", "se_c_disk_write_and_read") : software_lang.TSReadLangs("StorageContent", "se_c_disk_not_write_and_read"));
                            }
                            catch { disk_media_loaded_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); }
                            // DISK STATUS
                            try
                            {
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
                            }
                            catch
                            {
                                disk_media_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_disk_status_non"));
                            }
                            // DISK HEALTH + BOOT FLAGS
                            try
                            {
                                bool healthFound = false;
                                using (var search_disk_inf_4 = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT HealthStatus, BootFromDisk, IsBoot FROM MSFT_Disk WHERE Number={diskIndex}"))
                                using (var healthCol = search_disk_inf_4.Get())
                                {
                                    foreach (ManagementObject sdi_4 in healthCol.Cast<ManagementObject>())
                                    {
                                        using (sdi_4)
                                        {
                                            var disk_health = Convert.ToInt32(sdi_4["HealthStatus"]);
                                            switch (disk_health)
                                            {
                                                case 0: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_healt_good")); break;
                                                case 1: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_healt_running_and_error")); break;
                                                case 2: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_defective")); break;
                                                default: disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown")); break;
                                            }
                                            disk_boot_list.Add(Convert.ToBoolean(sdi_4["BootFromDisk"]) ? software_lang.TSReadLangs("StorageContent", "se_c_main_boot_disk") : software_lang.TSReadLangs("StorageContent", "se_c_not_main_boot_disk"));
                                            disk_bootable_list.Add(Convert.ToBoolean(sdi_4["IsBoot"]) ? software_lang.TSReadLangs("StorageContent", "se_c_bootable_disk") : software_lang.TSReadLangs("StorageContent", "se_c_not_bootable_disk"));
                                            healthFound = true;
                                            break;
                                        }
                                    }
                                }
                                if (!healthFound)
                                {
                                    disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                    disk_boot_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                    disk_bootable_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                }
                            }
                            catch
                            {
                                disk_health_status_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_boot_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                                disk_bootable_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // DISK COMPRESSED
                            try
                            {
                                bool anyCompressed = logicalDisks.Any(ld =>
                                {
                                    try { return Convert.ToBoolean(ld["Compressed"]); }
                                    catch { return false; }
                                });
                                disk_drive_compressed_list.Add(anyCompressed ? software_lang.TSReadLangs("StorageContent", "se_c_compressed") : software_lang.TSReadLangs("StorageContent", "se_c_not_compressed"));
                            }
                            catch
                            {
                                disk_drive_compressed_list.Add(software_lang.TSReadLangs("StorageContent", "se_c_unknown"));
                            }
                            // MB BIOS TYPE
                            try
                            {
                                using (var search_bios_type = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", $"SELECT PartitionStyle FROM MSFT_Disk WHERE Number={diskIndex}"))
                                using (var biosStyleCol = search_bios_type.Get())
                                {
                                    foreach (ManagementObject sbt in biosStyleCol.Cast<ManagementObject>())
                                    {
                                        using (sbt)
                                        {
                                            var osLetter = Program.Windows_disk.Replace("\\", string.Empty).Trim();
                                            bool isOsOnThisDisk = logicalDisks.Any(ld =>
                                            {
                                                try
                                                {
                                                    var name = Convert.ToString(ld["Name"])?.Trim();
                                                    return !string.IsNullOrEmpty(name) && string.Equals(name, osLetter, StringComparison.OrdinalIgnoreCase);
                                                }
                                                catch { return false; }
                                            });
                                            if (isOsOnThisDisk)
                                            {
                                                var disk_style = Convert.ToInt32(sbt["PartitionStyle"]);
                                                string biosModeText = disk_style == 2 ? "UEFI" : disk_style == 1 ? software_lang.TSReadLangs("Mb_Content", "mb_c_old") : software_lang.TSReadLangs("StorageContent", "se_c_unknown");
                                                MB_BiosMode_V.SetTextSafe(biosModeText);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            catch { }
                            // DISPOSE PER-DISK WMI OBJECTS
                            try
                            {
                                foreach (var ld in logicalDisks) { try { ld.Dispose(); } catch { } }
                                foreach (var sv in storageVolumes) { try { sv.Dispose(); } catch { } }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
            }
            try
            {
                if (bulkDiskOwned != null)
                {
                    foreach (var bo in bulkDiskOwned) { try { bo.Dispose(); } catch { } }
                    bulkDiskOwned.Clear();
                }
                if (bulkPhysDisks != null)
                {
                    foreach (var pd in bulkPhysDisks) { try { pd.Dispose(); } catch { } }
                    bulkPhysDisks.Clear();
                }
            }
            catch { }
            // BITLOCKER (DISK-LEVEL AGGREGATE)
            var bulkBitLockerOwned = new List<ManagementObject>();
            Dictionary<string, List<ManagementObject>> bulkBitLockerByLetter = null;
            try
            {
                try
                {
                    using (var allBL = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", "SELECT DriveLetter, ProtectionStatus, ConversionStatus, EncryptionMethod FROM Win32_EncryptableVolume"))
                    using (var allBLCol = allBL.Get())
                    {
                        var blMap = new Dictionary<string, List<ManagementObject>>(StringComparer.OrdinalIgnoreCase);
                        foreach (ManagementObject blObj in allBLCol.Cast<ManagementObject>())
                        {
                            string dl = (Convert.ToString(blObj["DriveLetter"]) ?? string.Empty).Trim();
                            if (!string.IsNullOrEmpty(dl))
                            {
                                if (!blMap.TryGetValue(dl, out var blList)) blMap[dl] = blList = new List<ManagementObject>();
                                blList.Add(blObj);
                                bulkBitLockerOwned.Add(blObj);
                            }
                            else { try { blObj.Dispose(); } catch { } }
                        }
                        if (blMap.Count > 0) bulkBitLockerByLetter = blMap;
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "Disk() - bulk BitLocker prefetch"); }
                    bulkBitLockerByLetter = null;
                }
                for (int i = 0; i <= disk_volume_id_list.Count - 1; i++)
                {
                    var letters = disk_volume_id_list[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Replace("\\", "")).Where(x => x.Length >= 2 && x[1] == ':').Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    string unkP = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_unknown");
                    string unkC = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_status_unknown");
                    string unkE = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_em_pass_unknown");
                    if (letters.Count == 0)
                    {
                        disk_bitlocker_status_list.Add(unkP);
                        disk_bitlocker_conversionstatus_list.Add(unkC);
                        disk_bitlocker_encryptionmethod_list.Add(unkE);
                        continue;
                    }
                    var perVolProtection = new List<string>();
                    var perVolConversion = new List<string>();
                    var perVolEncryption = new List<string>();
                    foreach (var letter in letters)
                    {
                        try
                        {
                            List<ManagementObject> results = null;
                            if (bulkBitLockerByLetter != null)
                            {
                                bulkBitLockerByLetter.TryGetValue(letter, out results);
                            }
                            else
                            {
                                using (var get_bitlocker_status = new ManagementObjectSearcher("root\\CIMV2\\Security\\MicrosoftVolumeEncryption", $"SELECT ProtectionStatus, ConversionStatus, EncryptionMethod FROM Win32_EncryptableVolume WHERE DriveLetter = '{letter}'"))
                                {
                                    results = get_bitlocker_status.Get().Cast<ManagementObject>().ToList();
                                }
                            }
                            if (results == null || results.Count == 0)
                            {
                                perVolProtection.Add(unkP);
                                perVolConversion.Add(unkC);
                                perVolEncryption.Add(unkE);
                                continue;
                            }
                            foreach (var query_bitlocker in results)
                            {
                                using (query_bitlocker)
                                {
                                    // PROTECTION STATUS
                                    try
                                    {
                                        string bl_protection_status = Convert.ToString(query_bitlocker["ProtectionStatus"]);
                                        var protectionStatusMessages = new Dictionary<int, string>{
                                            { 0, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_off") },
                                            { 1, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_ps_on") },
                                            { 2, unkP }
                                        };
                                        if (!string.IsNullOrEmpty(bl_protection_status))
                                        {
                                            int v = Convert.ToInt32(bl_protection_status);
                                            perVolProtection.Add(protectionStatusMessages.TryGetValue(v, out string msg) ? msg : unkP);
                                        }
                                        else
                                        {
                                            perVolProtection.Add(unkP);
                                        }
                                    }
                                    catch { perVolProtection.Add(unkP); }
                                    // CONVERSION STATUS
                                    try
                                    {
                                        string bl_conversion_status = Convert.ToString(query_bitlocker["ConversionStatus"]);
                                        var conversionStatusMessages = new Dictionary<int, string>{
                                            { 0, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_no_pass") },
                                            { 1, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_yes_pass") },
                                            { 2, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_encrypt_continue") },
                                            { 3, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_decrypt_continue") },
                                            { 4, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_encrypt_paused") },
                                            { 5, software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_cs_decrypt_paused") }
                                        };
                                        if (!string.IsNullOrEmpty(bl_conversion_status))
                                        {
                                            int v = Convert.ToInt32(bl_conversion_status);
                                            perVolConversion.Add(conversionStatusMessages.TryGetValue(v, out string msg) ? msg : unkC);
                                        }
                                        else
                                        {
                                            perVolConversion.Add(unkC);
                                        }
                                    }
                                    catch { perVolConversion.Add(unkC); }
                                    // ENCRYPTION METHOD
                                    try
                                    {
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
                                        if (!string.IsNullOrEmpty(bl_encryption_method))
                                        {
                                            int v = Convert.ToInt32(bl_encryption_method);
                                            perVolEncryption.Add(encryptionMethodMessages.TryGetValue(v, out string msg) ? msg : unkE);
                                        }
                                        else
                                        {
                                            perVolEncryption.Add(unkE);
                                        }
                                    }
                                    catch { perVolEncryption.Add(unkE); }
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            perVolProtection.Add(unkP);
                            perVolConversion.Add(unkC);
                            perVolEncryption.Add(unkE);
                        }
                    }
                    bool isUsbOrExternal = false;
                    try
                    {
                        string t = (disk_type_list.Count > i ? (disk_type_list[i] ?? "") : "").ToLowerInvariant();
                        isUsbOrExternal = t.Contains("usb") || t.Contains("external");
                    }
                    catch { }
                    string ReduceToSingle(List<string> vals, string mixedLabel, string unknownValue)
                    {
                        vals = vals.Where(v => !string.IsNullOrEmpty(v)).ToList();
                        if (vals.Count == 0) return unknownValue;
                        var filtered = vals.Where(v => !string.Equals(v, unknownValue, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (filtered.Count == 0) return unknownValue;
                        return filtered.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1 ? filtered[0] : mixedLabel;
                    }
                    string mixedText = software_lang.TSReadLangs("StorageContent", "se_c_bitlocker_mixed");
                    string p = ReduceToSingle(perVolProtection, mixedText, unkP);
                    string c = ReduceToSingle(perVolConversion, mixedText, unkC);
                    string e = ReduceToSingle(perVolEncryption, mixedText, unkE);
                    if (isUsbOrExternal)
                    {
                        if (p == mixedText) p = unkP;
                        if (c == mixedText) c = unkC;
                        if (e == mixedText) e = unkE;
                    }
                    disk_bitlocker_status_list.Add(p);
                    disk_bitlocker_conversionstatus_list.Add(c);
                    disk_bitlocker_encryptionmethod_list.Add(e);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Disk()"); }
            }
            try
            {
                if (bulkBitLockerOwned != null)
                {
                    foreach (var bo in bulkBitLockerOwned) { try { bo.Dispose(); } catch { } }
                    bulkBitLockerOwned.Clear();
                }
            }
            catch { }
            // SELECT DISK & UI RENDER
            DISK_Selector_List.ExecuteSafe(() =>
            {
                try
                {
                    int c_index = disk_volume_id_list.FindIndex(x => x.Contains(Program.Windows_disk));
                    if (DISK_Selector_List.Items.Count > 0)
                    {
                        if (c_index != -1 && c_index < DISK_Selector_List.Items.Count)
                        {
                            DISK_Selector_List.SelectedIndex = c_index;
                        }
                        else
                        {
                            DISK_Selector_List.SelectedIndex = 0;
                        }
                    }
                }
                catch (Exception)
                {
                    if (DISK_Selector_List.Items.Count > 0) DISK_Selector_List.SelectedIndex = 0;
                }
                DISK_TTLP_P1_L2.Text = disk_ssd_count.ToString();
                DISK_TTLP_P2_L2.Text = disk_hdd_count.ToString();
                DISK_TTLP_P3_L2.Text = disk_usb_count.ToString();
                DISK_TTLP_P4_L2.Text = DISK_Selector_List.Items.Count.ToString();
            });
            // DISK PROCESS END ENABLED
            DISK_RotateBtn.SetEnabledSafe(true);
            DISK.SetEnabledSafe(true);
            if (debug_status)
            {
                TSLogger.Log("<--- Storage Section Loaded --->");
            }
        }
        // DISK RIGHT PROGRESS FUNCTION
        private void Disk_progress_function(int _pb_disk)
        {
            try
            {
                if (_pb_disk < 0 || _pb_disk >= disk_total_space_list.Count || _pb_disk >= disk_free_space_list.Count) return;
                string totalDiskSpaceStr = disk_total_space_list[_pb_disk];
                string freeDiskSpaceStr = disk_free_space_list[_pb_disk];
                long ConvertToMB(string sizeStr)
                {
                    sizeStr = sizeStr.Trim().ToUpper();
                    string pattern = @"([\d.,]+)\s*(B|KB|MB|GB|TB|PB|EB|ZB|YB)";
                    Match match = Regex.Match(sizeStr, pattern);
                    if (!match.Success) return 0;
                    string numericValueStr = match.Groups[1].Value.Replace(',', '.');
                    string unit = match.Groups[2].Value;
                    double numericValue = double.Parse(numericValueStr, CultureInfo.InvariantCulture);
                    string[] unitScale = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
                    int unitIndex = Array.IndexOf(unitScale, unit);
                    double sizeInMB = unitIndex >= 2 ? numericValue * Math.Pow(1024, unitIndex - 2) : numericValue / Math.Pow(1024, 2 - unitIndex);
                    return (long)Math.Floor(sizeInMB);
                }
                long totalDiskSpaceMB = ConvertToMB(totalDiskSpaceStr);
                long freeDiskSpaceMB = ConvertToMB(freeDiskSpaceStr);
                if (totalDiskSpaceMB <= 0) return;
                long usedDiskSpaceMB = totalDiskSpaceMB - freeDiskSpaceMB;
                int usagePercentage = (int)((double)usedDiskSpaceMB / totalDiskSpaceMB * 100);
                DISK_PBar_FE.ExecuteSafe(() =>
                {
                    DISK_PBar_FE.Height = (int)(DISK_PBar_BG.Height * (usagePercentage / 100.0));
                    DISK_PBar_Label.Text = usagePercentage + "%";
                    if (usagePercentage <= 7)
                    {
                        DISK_PBar_Label.Top = DISK_PBar_BG.Top + DISK_PBar_FE.Top - 2;
                    }
                    else
                    {
                        DISK_PBar_Label.Top = DISK_PBar_BG.Top + DISK_PBar_FE.Top + 6;
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Disk_progress_function()"); }
            }
        }
        private void DISK_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int disk_percent = DISK_Selector_List.SelectedIndex;
                if (disk_percent == -1) return;
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
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "DISK_Selector_List_SelectedIndexChanged()"); }
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
        readonly List<string> network_subnet_mask_list = new List<string>();
        readonly List<string> network_dns_v4_1_list = new List<string>();
        readonly List<string> network_dns_v4_2_list = new List<string>();
        readonly List<string> network_dns_v6_1_list = new List<string>();
        readonly List<string> network_dns_v6_2_list = new List<string>();
        private static bool IsValidIPv4SubnetMask(IPAddress address)
        {
            if (address == null || address.AddressFamily != AddressFamily.InterNetwork) return false;

            byte[] bytes = address.GetAddressBytes();
            uint mask = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
            if (mask == 0 || mask == uint.MaxValue) return mask == uint.MaxValue;

            uint inverted = ~mask;
            return (inverted & (inverted + 1)) == 0;
        }
        // DNS PROVIDER CLASS
        // -----------------------------------------
        public class DnsProvider
        {
            public string Name { get; }
            public List<string> IPv4 { get; }
            public List<string> IPv6 { get; }
            public int Id { get; }
            public List<string> DnsAddresses
            {
                get
                {
                    var all = new List<string>(IPv4.Count + IPv6.Count);
                    all.AddRange(IPv4);
                    all.AddRange(IPv6);
                    return all;
                }
            }
            public DnsProvider(string name, List<string> ipv4, List<string> ipv6, int id)
            {
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
            new DnsProvider("CleanBrowsing",
                new List<string> { "185.228.168.9", "185.228.169.9" },
                new List<string> { "2a0d:2a00:1::2", "2a0d:2a00:2::2" }, 1
            ),
            new DnsProvider("Cloudflare",
                new List<string> { "1.1.1.1", "1.0.0.1" },
                new List<string> { "2606:4700:4700::1111", "2606:4700:4700::1001" }, 2
            ),
            new DnsProvider("Comodo",
                new List<string> { "8.26.56.26", "8.20.247.20" },
                new List<string>(), 3
            ),
            new DnsProvider("Control D",
                new List<string> { "76.76.2.0", "76.76.10.0" },
                new List<string> { "2606:1a40::0", "2606:1a40:1::0" }, 4
            ),
            new DnsProvider("DNS.WATCH",
                new List<string> { "84.200.69.80", "84.200.70.40" },
                new List<string> { "2001:1608:10:25::1c04:b12f", "2001:1608:10:25::9249:d69b" }, 5
            ),
            new DnsProvider("Google",
                new List<string> { "8.8.8.8", "8.8.4.4" },
                new List<string> { "2001:4860:4860::8888", "2001:4860:4860::8844" }, 6
            ),
            new DnsProvider("Lumen DNS",
                new List<string> { "4.2.2.1", "4.2.2.2" },
                new List<string>(), 7
            ),
            new DnsProvider("NextDNS",
                new List<string> { "45.90.28.0", "45.90.30.0" },
                new List<string> { "2a07:a8c0::", "2a07:a8c1::" }, 8
            ),
            new DnsProvider("OpenDNS",
                new List<string> { "208.67.222.222", "208.67.220.220" },
                new List<string> { "2620:119:35::35", "2620:119:53::53" }, 9
            ),
            new DnsProvider("Quad9",
                new List<string> { "9.9.9.9", "149.112.112.112" },
                new List<string> { "2620:fe::fe", "2620:fe::9" }, 10
            ),
            new DnsProvider("Yandex DNS",
                new List<string> { "77.88.8.8", "77.88.8.1" },
                new List<string> { "2a02:6b8::feed:0ff", "2a02:6b8:0:1::feed:0ff" }, 11
            ),
        };
        private async Task Network()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            using (ManagementObjectSearcher search_na = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, MACAddress, Manufacturer, ServiceName, AdapterType, PhysicalAdapter, PNPDeviceID, GUID, NetConnectionID, Index, Speed FROM Win32_NetworkAdapter", queryOptions))
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceName, DriverVersion, DriverDate FROM Win32_PnPSignedDriver WHERE DeviceClass='Net'", queryOptions))
            {
            var driverDict = new Dictionary<string, ManagementObject>(StringComparer.OrdinalIgnoreCase);
            using (var results = searcher.Get())
            {
                foreach (ManagementObject d in results.Cast<ManagementObject>())
                {
                    string deviceName = d["DeviceName"]?.ToString() ?? "";
                    string key = Net_replacer(deviceName);
                    if (!string.IsNullOrEmpty(key) && !driverDict.ContainsKey(key))
                    {
                        driverDict[key] = d;
                    }
                    else
                    {
                        try { d.Dispose(); } catch { }
                    }
                }
            }
            var nicByGuid = new Dictionary<string, NetworkInterface>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    string nid = (ni.Id ?? "").Trim().Trim('{', '}');
                    if (!string.IsNullOrEmpty(nid) && !nicByGuid.ContainsKey(nid)) nicByGuid[nid] = ni;
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Network() -> NIC prefetch"); }
            }
            using (var naCol = search_na.Get())
            {
                foreach (ManagementObject query_na_rotate in naCol.Cast<ManagementObject>())
                {
                    using (query_na_rotate)
                    {
                        try
                        {
                            // NET NAME
                            NET_Selector_List.AddItemSafe(Convert.ToString(query_na_rotate["Name"]));
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Add Selector Item"); }
                        }
                        try
                        {
                            // MAC ADDRESS
                            string mac_adress = Convert.ToString(query_na_rotate["MACAddress"]);
                            if (string.IsNullOrEmpty(mac_adress))
                            {
                                network_mac_adress_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_mac_adress"));
                            }
                            else
                            {
                                if (stealth_status != 1)
                                {
                                    network_mac_adress_list.Add(mac_adress);
                                }
                                else
                                {
                                    network_mac_adress_list.Add(sm_text);
                                }
                            }
                            if (network_mac_adress_list.Count > 0) NET_MacAdress_V.SetTextSafe(network_mac_adress_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> MAC Address"); }
                        }
                        try
                        {
                            // NET MAN
                            string net_man = Convert.ToString(query_na_rotate["Manufacturer"]);
                            if (string.IsNullOrEmpty(net_man))
                            {
                                network_man_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_provider"));
                            }
                            else
                            {
                                network_man_list.Add(net_man);
                            }
                            if (network_man_list.Count > 0) NET_NetMan_V.SetTextSafe(network_man_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Manufacturer"); }
                        }
                        try
                        {
                            // DRIVER INFO
                            string netName = Net_replacer(Convert.ToString(query_na_rotate["Name"]) ?? "");
                            string unknown = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                            string driverVersion = unknown;
                            string driverDate = unknown;
                            if (driverDict.TryGetValue(netName, out var driverMatch))
                            {
                                driverVersion = Convert.ToString(driverMatch["DriverVersion"]) ?? unknown;
                                string dateRaw = Convert.ToString(driverMatch["DriverDate"]);
                                if (!string.IsNullOrEmpty(dateRaw))
                                {
                                    try
                                    {
                                        DateTime dt = ManagementDateTimeConverter.ToDateTime(dateRaw);
                                        driverDate = dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        driverDate = unknown;
                                    }
                                }
                            }
                            network_driver_version_list.Add(driverVersion);
                            network_driver_date_list.Add(driverDate);
                        }
                        catch
                        {
                            string unknown = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                            network_driver_version_list.Add(unknown);
                            network_driver_date_list.Add(unknown);
                        }
                        try
                        {
                            // SERVICE NAME
                            string service_name = Convert.ToString(query_na_rotate["ServiceName"]);
                            if (string.IsNullOrEmpty(service_name))
                            {
                                network_service_name_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_service_name"));
                            }
                            else
                            {
                                network_service_name_list.Add(service_name);
                            }
                            if (network_service_name_list.Count > 0) NET_ServiceName_V.SetTextSafe(network_service_name_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Service Name"); }
                        }
                        try
                        {
                            // NET ADAPTER TYPE
                            string adaptor_type = Convert.ToString(query_na_rotate["AdapterType"]);
                            if (string.IsNullOrEmpty(adaptor_type))
                            {
                                network_adaptor_type_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_adaptor_type"));
                            }
                            else
                            {
                                network_adaptor_type_list.Add(adaptor_type);
                            }
                            if (network_adaptor_type_list.Count > 0) NET_AdapterType_V.SetTextSafe(network_adaptor_type_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Adapter Type"); }
                        }
                        try
                        {
                            // NET PHYSICAL
                            bool net_physical = Convert.ToBoolean(query_na_rotate["PhysicalAdapter"]);
                            network_physical_list.Add(net_physical ? software_lang.TSReadLangs("Network_Content", "nk_c_yes") : software_lang.TSReadLangs("Network_Content", "nk_c_no"));
                            if (network_physical_list.Count > 0) NET_Physical_V.SetTextSafe(network_physical_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Physical Status"); }
                        }
                        try
                        {
                            // NETWORK DEVICE ID
                            string network_device_id = Convert.ToString(query_na_rotate["PNPDeviceID"])?.Trim() ?? "";
                            if (!string.IsNullOrEmpty(network_device_id))
                            {
                                string[] network_device_split = network_device_id.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                if (network_device_split.Length >= 2)
                                    network_device_id_list.Add($"{network_device_split[0]}\\{network_device_split[1]}");
                                else
                                    network_device_id_list.Add(network_device_id);
                            }
                            else
                            {
                                network_device_id_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_hardware_id"));
                            }
                            if (network_device_id_list.Count > 0) NET_DeviceID_V.SetTextSafe(network_device_id_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> PNPDeviceID"); }
                        }
                        try
                        {
                            // NET GUID
                            string guid = Convert.ToString(query_na_rotate["GUID"]);
                            if (string.IsNullOrEmpty(guid))
                            {
                                network_guid_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_guid"));
                            }
                            else
                            {
                                string net_guid_replacer = guid.Replace("{", string.Empty).Replace("}", string.Empty);
                                if (stealth_status != 1)
                                {
                                    network_guid_list.Add(net_guid_replacer);
                                }
                                else
                                {
                                    network_guid_list.Add(sm_text);
                                }
                            }
                            if (network_guid_list.Count > 0) NET_Guid_V.SetTextSafe(network_guid_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> GUID"); }
                        }
                        try
                        {
                            // NET CONNECTION TYPE
                            string net_con_id = Convert.ToString(query_na_rotate["NetConnectionID"]);
                            if (string.IsNullOrEmpty(net_con_id))
                            {
                                network_connection_type_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_connection_type"));
                            }
                            else
                            {
                                network_connection_type_list.Add(net_con_id);
                            }
                            if (network_connection_type_list.Count > 0) NET_ConnectionType_V.SetTextSafe(network_connection_type_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Connection ID"); }
                        }
                        // NETWORK ADAPTER CONFIG SECTION
                        try
                        {
                            var get_na_index = query_na_rotate["Index"];
                            using (ManagementObjectSearcher search_nac = new ManagementObjectSearcher("root\\CIMV2", $"SELECT DHCPEnabled, DHCPServer, DHCPLeaseObtained, DHCPLeaseExpires, IPAddress, IPSubnet FROM Win32_NetworkAdapterConfiguration WHERE Index={get_na_index}"))
                            using (var nacCol = search_nac.Get())
                            {
                                foreach (ManagementObject query_nac_rotate in nacCol.Cast<ManagementObject>())
                                {
                                    using (query_nac_rotate)
                                    {
                                        try
                                        {
                                            // DHCP STATUS
                                            bool dhcp_enabled = Convert.ToBoolean(query_nac_rotate["DHCPEnabled"]);
                                            network_dhcp_status_list.Add(dhcp_enabled ? software_lang.TSReadLangs("Network_Content", "nk_c_active") : software_lang.TSReadLangs("Network_Content", "nk_c_out_of_order"));
                                            if (network_dhcp_status_list.Count > 0) NET_Dhcp_status_V.SetTextSafe(network_dhcp_status_list[0]);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> DHCP Status"); }
                                        }
                                        try
                                        {
                                            // DHCP SERVER STATUS
                                            string dhcp_server = Convert.ToString(query_nac_rotate["DHCPServer"]);
                                            if (string.IsNullOrEmpty(dhcp_server))
                                            {
                                                network_dhcp_server_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                                            }
                                            else
                                            {
                                                if (stealth_status != 1)
                                                {
                                                    network_dhcp_server_list.Add(dhcp_server);
                                                }
                                                else
                                                {
                                                    network_dhcp_server_list.Add(sm_text);
                                                }
                                            }
                                            if (network_dhcp_server_list.Count > 0) NET_Dhcp_server_V.SetTextSafe(network_dhcp_server_list[0]);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> DHCP Server"); }
                                        }
                                        try
                                        {
                                            // DHCP SERVER FIRST CONNECT TIME
                                            string dhcp_first_ip_time = Convert.ToString(query_nac_rotate["DHCPLeaseObtained"]);
                                            if (string.IsNullOrEmpty(dhcp_first_ip_time))
                                            {
                                                network_dhcp_first_ip_time_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                                            }
                                            else
                                            {
                                                DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(dhcp_first_ip_time);
                                                network_dhcp_first_ip_time_list.Add($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm:ss} ");
                                            }
                                            if (network_dhcp_first_ip_time_list.Count > 0) NET_DHCPFirstIpTime_V.SetTextSafe(network_dhcp_first_ip_time_list[0]);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> DHCP Lease Obtained"); }
                                        }
                                        try
                                        {
                                            // DHCP SERVER LAST CONNECT TIME
                                            string dhcp_last_ip_time = Convert.ToString(query_nac_rotate["DHCPLeaseExpires"]);
                                            if (string.IsNullOrEmpty(dhcp_last_ip_time))
                                            {
                                                network_dhcp_last_ip_time_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                                            }
                                            else
                                            {
                                                DateTime osInstallDate = ManagementDateTimeConverter.ToDateTime(dhcp_last_ip_time);
                                                network_dhcp_last_ip_time_list.Add($"{osInstallDate:dd.MM.yyyy} - {osInstallDate:HH:mm:ss} ");
                                            }
                                            if (network_dhcp_last_ip_time_list.Count > 0) NET_DHCPLastIpTime_V.SetTextSafe(network_dhcp_last_ip_time_list[0]);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> DHCP Lease Expires"); }
                                        }
                                        try
                                        {
                                            // IP SUBNET MASK
                                            string unknownMask = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                                            var subnetMasks = new List<string>();
                                            if (query_nac_rotate["IPSubnet"] is string[] ipSubnets)
                                            {
                                                foreach (string subnet in ipSubnets)
                                                {
                                                    if (IPAddress.TryParse(subnet, out IPAddress parsedSubnet) && IsValidIPv4SubnetMask(parsedSubnet) && !subnetMasks.Contains(parsedSubnet.ToString()))
                                                        subnetMasks.Add(parsedSubnet.ToString());
                                                }
                                            }
                                                if (stealth_status != 1)
                                                {
                                                    network_subnet_mask_list.Add(subnetMasks.Count > 0 ? string.Join(", ", subnetMasks) : unknownMask);
                                                }
                                                else
                                                {
                                                    network_subnet_mask_list.Add(sm_text);
                                                }
                                        }
                                        catch (Exception ex)
                                        {
                                            network_subnet_mask_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_unknown"));
                                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Subnet Mask"); }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Configuration Inner Loop"); }
                        }
                        // CONNECTION SPEED SECTION
                        try
                        {
                            string local_con_speed = Convert.ToString(query_na_rotate["Speed"]);
                            bool speed_unknown = string.IsNullOrEmpty(local_con_speed) || local_con_speed == "Unknown" || local_con_speed == "0" || local_con_speed == "4294967295" || local_con_speed == "9223372036854775807" || local_con_speed == "18446744073709551615";
                            if (!speed_unknown && double.TryParse(local_con_speed, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed_bps) && parsed_bps > 1e15)
                            {
                                speed_unknown = true;
                            }
                            if (speed_unknown)
                            {
                                network_connection_speed_list.Add(software_lang.TSReadLangs("Network_Content", "nk_c_not_connect"));
                            }
                            else
                            {
                                double raw_speed_bps = Convert.ToDouble(local_con_speed);
                                double mbps = raw_speed_bps / 1_000_000.0;
                                double mb_s = mbps / 8.0;
                                string part1 = (mbps >= 1000) ? $"{(mbps / 1000.0):0.##} Gbps" : $"{mbps:0} Mbps";
                                string part2 = (mb_s >= 1024.0) ? $"{(mb_s / 1024.0):0.##} GB/s" : $"{mb_s:0.##} MB/s";
                                network_connection_speed_list.Add($"{part1} - ({part2})");
                            }
                            if (network_connection_speed_list.Count > 0) NET_LocalConSpeed_V.SetTextSafe(network_connection_speed_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Connection Speed"); }
                        }
                        // IPV4 & IPV6 SECTION
                        try
                        {
                            var get_na_index = query_na_rotate["Index"];
                            string unknownIp = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                            using (ManagementObjectSearcher query_na_ip = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index={get_na_index}"))
                            using (var ipCol = query_na_ip.Get())
                            {
                                foreach (ManagementObject search_ip in ipCol.Cast<ManagementObject>())
                                {
                                    using (search_ip)
                                    {
                                        if (search_ip["IPAddress"] == null)
                                        {
                                            network_ipv4_list.Add(unknownIp);
                                            network_ipv6_list.Add(unknownIp);
                                        }
                                        else
                                        {
                                            string[] arrIPAddress = (string[])(search_ip["IPAddress"]);
                                            var v4Addrs = new List<string>();
                                            var v4Seen = new HashSet<string>();
                                            var v6Global = new List<string>();
                                            var v6GlobalSeen = new HashSet<string>();
                                            var v6LinkLocal = new List<string>();
                                            var v6LinkSeen = new HashSet<string>();
                                            foreach (string ipAddress in arrIPAddress)
                                            {
                                                if (IPAddress.TryParse(ipAddress, out IPAddress address))
                                                {
                                                    if (address.AddressFamily == AddressFamily.InterNetwork)
                                                    {
                                                        if (IPAddress.IsLoopback(address) || ipAddress == "0.0.0.0") continue;
                                                        if (v4Seen.Add(ipAddress)) v4Addrs.Add(ipAddress);
                                                    }
                                                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                                                    {
                                                        if (address.Equals(IPAddress.IPv6Loopback) || address.Equals(IPAddress.IPv6Any) || address.IsIPv6Multicast) continue;
                                                        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
                                                        {
                                                            if (v6LinkSeen.Add(ipAddress)) v6LinkLocal.Add(ipAddress);
                                                        }
                                                        else
                                                        {
                                                            if (v6GlobalSeen.Add(ipAddress)) v6Global.Add(ipAddress);
                                                        }
                                                    }
                                                }
                                            }
                                            if (stealth_status == 1)
                                            {
                                                network_ipv4_list.Add(v4Addrs.Count > 0 || v6Global.Count > 0 || v6LinkLocal.Count > 0 ? sm_text : unknownIp);
                                                network_ipv6_list.Add(v6Global.Count > 0 || v6LinkLocal.Count > 0 || v4Addrs.Count > 0 ? sm_text : unknownIp);
                                            }
                                            else
                                            {
                                                network_ipv4_list.Add(v4Addrs.Count > 0 ? string.Join(", ", v4Addrs) : unknownIp);
                                                var v6Shown = v6Global.Count > 0 ? v6Global : v6LinkLocal;
                                                network_ipv6_list.Add(v6Shown.Count > 0 ? string.Join(", ", v6Shown) : unknownIp);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                string unknownIp = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                                network_ipv4_list.Add(unknownIp);
                                network_ipv6_list.Add(unknownIp);
                            }
                            catch { }
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> IP Resolution"); }
                        }
                        // PER-ADAPTER DNS SECTION
                        try
                        {
                            string adapterGuid = Convert.ToString(query_na_rotate["GUID"]) ?? "";
                            string guidNorm = adapterGuid.Trim().Trim('{', '}');
                            string dns41 = "", dns42 = "", dns61 = "", dns62 = "";
                            try
                            {
                                NetworkInterface nic = null;
                                if (!string.IsNullOrEmpty(guidNorm)) nicByGuid.TryGetValue(guidNorm, out nic);
                                if (nic != null)
                                {
                                    var dnsList = nic.GetIPProperties().DnsAddresses.Where(d => d != null && !d.Equals(IPAddress.Any) && !d.Equals(IPAddress.IPv6Any) && !d.ToString().StartsWith("fec0:0:0:ffff", StringComparison.OrdinalIgnoreCase)).Select(d => d.ToString()).Distinct().ToList();
                                    var dnsV4 = dnsList.Where(s => IPAddress.TryParse(s, out IPAddress a) && a.AddressFamily == AddressFamily.InterNetwork).ToList();
                                    var dnsV6 = dnsList.Where(s => IPAddress.TryParse(s, out IPAddress a) && a.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                                    dns41 = dnsV4.ElementAtOrDefault(0) ?? "";
                                    dns42 = dnsV4.ElementAtOrDefault(1) ?? "";
                                    dns61 = dnsV6.ElementAtOrDefault(0) ?? "";
                                    dns62 = dnsV6.ElementAtOrDefault(1) ?? "";
                                }
                            }
                            catch { }
                            network_dns_v4_1_list.Add(dns41);
                            network_dns_v4_2_list.Add(dns42);
                            network_dns_v6_1_list.Add(dns61);
                            network_dns_v6_2_list.Add(dns62);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                network_dns_v4_1_list.Add("");
                                network_dns_v4_2_list.Add("");
                                network_dns_v6_1_list.Add("");
                                network_dns_v6_2_list.Add("");
                            }
                            catch { }
                            if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Per-Adapter DNS"); }
                        }
                    }
                }
            }
            // GET PUBLIC IP & ISP AND LOCATION
            try
            {
                string u_info = software_lang.TSReadLangs("Network_Content", "nk_c_unknown");
                string ni_connection = software_lang.TSReadLangs("Network_Content", "nk_c_not_internet_connection");
                if (stealth_status == 1)
                {
                    NET_P_IP_Adress_V.SetTextSafe(sm_text);
                    NET_P_ISP_V.SetTextSafe(sm_text);
                }
                else
                {
                    bool isConnected = await IsNetworkAvailable().ConfigureAwait(false);
                    if (!isConnected)
                    {
                        NET_P_IP_Adress_V.SetTextSafe(ni_connection);
                        NET_P_ISP_V.SetTextSafe(ni_connection);
                    }
                    else
                    {
                        var get_info = await GetPublicIpInfoAsync().ConfigureAwait(false);
                        if (get_info != null && !string.IsNullOrEmpty(get_info.IP))
                        {
                            string ipType = "";
                            if (IPAddress.TryParse(get_info.IP, out IPAddress parsedIp))
                            {
                                if (parsedIp.AddressFamily == AddressFamily.InterNetwork) { ipType = "(IPv4)"; }
                                else if (parsedIp.AddressFamily == AddressFamily.InterNetworkV6) { ipType = "(IPv6)"; }
                            }
                            NET_P_IP_Adress_V.SetTextSafe($"{get_info.IP} {ipType}");
                            NET_P_ISP_V.SetTextSafe(get_info.ISP ?? u_info);
                        }
                        else
                        {
                            NET_P_IP_Adress_V.SetTextSafe(u_info);
                            NET_P_ISP_V.SetTextSafe(u_info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Public IP Api"); }
            }
            try
            {
                // GET DNS ADDRESS (per-adapter, selector follows)
                this.ExecuteSafe(() =>
                {
                    try
                    {
                        TSGetLangs dns_lang = new TSGetLangs(lang_path);
                        if (network_dns_v4_1_list.Count > 0)
                        {
                            SetDnsLabel(NET_DNS_v4_1_V, network_dns_v4_1_list[0], dns_lang);
                            SetDnsLabel(NET_DNS_v4_2_V, network_dns_v4_2_list.Count > 0 ? network_dns_v4_2_list[0] : "", dns_lang);
                            SetDnsLabel(NET_DNS_v6_1_V, network_dns_v6_1_list.Count > 0 ? network_dns_v6_1_list[0] : "", dns_lang);
                            SetDnsLabel(NET_DNS_v6_2_V, network_dns_v6_2_list.Count > 0 ? network_dns_v6_2_list[0] : "", dns_lang);
                        }
                        else
                        {
                            UpdateDnsLabels(NET_DNS_v4_1_V, NET_DNS_v4_2_V, NET_DNS_v6_1_V, NET_DNS_v6_2_V);
                        }
                    }
                    catch
                    {
                        UpdateDnsLabels(NET_DNS_v4_1_V, NET_DNS_v4_2_V, NET_DNS_v6_1_V, NET_DNS_v6_2_V);
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Network() -> DNS Update"); }
            }
            // NETWORK SELECT & INDEX SELECTION
            NET_Selector_List.ExecuteSafe(() =>
            {
                try
                {
                    var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback).OrderByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet).ThenByDescending(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211).Select(ni => Net_replacer(ni.Description).ToLower()).ToList();
                    bool found = false;
                    for (int i = 0; i < NET_Selector_List.Items.Count; i++)
                    {
                        string listItem = Net_replacer(NET_Selector_List.Items[i].ToString()).ToLower();
                        if (activeInterfaces.Any(activeName => listItem.Contains(activeName) || activeName.Contains(listItem)))
                        {
                            NET_Selector_List.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }
                    if (!found && NET_Selector_List.Items.Count > 0)
                    {
                        NET_Selector_List.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "Network() -> Selector List Index Set"); }
                }
            });
            // NETWORK PROCESS END ENABLED
            NET_RotateBtn.SetEnabledSafe(true);
            NETWORK.SetEnabledSafe(true);
            try { foreach (var kv in driverDict) { try { kv.Value?.Dispose(); } catch { } } driverDict.Clear(); } catch { }
            }
            if (debug_status)
            {
                TSLogger.Log("<--- Network Section Loaded --->");
            }
        }
        private void NET_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int network_select = NET_Selector_List.SelectedIndex;
                if (network_select == -1) return;
                TSGetLangs net_lang = new TSGetLangs(lang_path);
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
                try { NET_SubnetMask_V.Text = network_subnet_mask_list[network_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { SetDnsLabel(NET_DNS_v4_1_V, network_select < network_dns_v4_1_list.Count ? network_dns_v4_1_list[network_select] : "", net_lang); } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { SetDnsLabel(NET_DNS_v4_2_V, network_select < network_dns_v4_2_list.Count ? network_dns_v4_2_list[network_select] : "", net_lang); } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { SetDnsLabel(NET_DNS_v6_1_V, network_select < network_dns_v6_1_list.Count ? network_dns_v6_1_list[network_select] : "", net_lang); } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
                try { SetDnsLabel(NET_DNS_v6_2_V, network_select < network_dns_v6_2_list.Count ? network_dns_v6_2_list[network_select] : "", net_lang); } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "NET_Selector_List_SelectedIndexChanged()"); }
        }
        private async Task NetBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                var software_lang = new TSGetLangs(lang_path);
                string activeAdapter = GetActiveNetworkAdapter();
                string not_connect = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                if (string.IsNullOrEmpty(activeAdapter))
                {
                    SetUI_NotConnected(not_connect);
                    return;
                }
                var category = new PerformanceCounterCategory("Network Interface");
                string[] instanceNames = category.GetInstanceNames();
                string perfCounterAdapterName = instanceNames.FirstOrDefault(name => Net_replacer(name) == Net_replacer(activeAdapter));
                if (perfCounterAdapterName == null)
                {
                    SetUI_NotConnected(not_connect);
                    return;
                }
                using (var bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", perfCounterAdapterName))
                using (var bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", perfCounterAdapterName))
                using (var bandwidthCounter = new PerformanceCounter("Network Interface", "Current Bandwidth", perfCounterAdapterName))
                {
                    double rawBandwidth = bandwidthCounter.NextValue() / 1_000_000.0;
                    string bandwidthText = rawBandwidth >= 1000 ? $"{(rawBandwidth / 1000.0):0.##} Gbps" : $"{(int)Math.Round(rawBandwidth)} Mbps";
                    NET_LT_Device_V.SetTextSafe(activeAdapter);
                    NET_LT_BandWidth_V.SetTextSafe(bandwidthText);
                    while (!shutdownToken.IsCancellationRequested && loop_status)
                    {
                        DateTime startTime = DateTime.Now;
                        double mbpsSent = bytesSentCounter.NextValue() * 8 / 1_000_000.0;
                        double mbpsReceived = bytesReceivedCounter.NextValue() * 8 / 1_000_000.0;
                        NET_LT_UL2.SetTextSafe(FormatSpeed(mbpsSent));
                        NET_LT_DL2.SetTextSafe(FormatSpeed(mbpsReceived));
                        int elapsed = (int)(DateTime.Now - startTime).TotalMilliseconds;
                        int nextDelay = Math.Max(10, 1000 - elapsed);
                        try
                        {
                            await Task.Delay(nextDelay, shutdownToken);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
                string FormatSpeed(double mbps)
                {
                    if (mbps >= 1000)
                        return $"{(mbps / 1000.0):0.00} Gbps / ({(mbps / 8 / 1000.0):0.00} GB/s)";
                    return $"{mbps:0.00} Mbps / ({(mbps / 8):0.00} MB/s)";
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "NetBgProcess()"); }
            }
        }
        private static string GetActiveNetworkAdapter()
        {
            try
            {
                using (ManagementObjectSearcher searcher_active_net = new ManagementObjectSearcher("root\\CIMV2", "SELECT Name, Index FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2"))
                using (ManagementObjectCollection adapterCol = searcher_active_net.Get())
                {
                    foreach (ManagementObject search_ip_enabled in adapterCol.Cast<ManagementObject>())
                    {
                        using (search_ip_enabled)
                        {
                            object idxObj;
                            string adapterName;
                            try
                            {
                                idxObj = search_ip_enabled["Index"];
                                adapterName = search_ip_enabled["Name"]?.ToString()?.Trim();
                            }
                            catch { continue; }
                            if (string.IsNullOrEmpty(adapterName)) continue;
                            bool hasIp = false;
                            using (ManagementObjectSearcher configSearcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT IPEnabled FROM Win32_NetworkAdapterConfiguration WHERE Index={idxObj} AND IPEnabled=True"))
                            using (ManagementObjectCollection cfgCol = configSearcher.Get())
                            {
                                foreach (ManagementObject configObj in cfgCol.Cast<ManagementObject>())
                                {
                                    using (configObj) { hasIp = true; break; }
                                }
                            }
                            if (hasIp) return adapterName;
                        }
                    }
                }
            }
            catch { }
            return null;
        }
        private void SetUI_NotConnected(string not_connect)
        {
            NET_LT_Device_V.SetTextSafe(not_connect);
            NET_LT_BandWidth_V.SetTextSafe(not_connect);
            NET_LT_UL2.SetTextSafe(not_connect);
            NET_LT_DL2.SetTextSafe(not_connect);
        }
        // GET PC IP & GATEWAY IP
        private string cached_gateway_ip = string.Empty;
        private void NetBGProcessGateway()
        {
            try
            {
                var getLocalNetInfo = GetLocalNetworkInfo();
                UpdateNetworkUi(getLocalNetInfo);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "NetBGProcessGateway()"); }
            }
        }
        private void UpdateNetworkUi(GetLocalNetInfo getLocalNetInfo)
        {
            if (getLocalNetInfo != null)
            {
                cached_gateway_ip = getLocalNetInfo.GatewayIp != null ? getLocalNetInfo.GatewayIp.ToString() : string.Empty;
                if (stealth_status != 1)
                {
                    NET_LT_LocalIP_V.SetTextSafe(getLocalNetInfo.LocalIp.ToString());
                    NET_LT_GatewayIP_V.SetTextSafe(cached_gateway_ip);
                }
                else
                {
                    NET_LT_LocalIP_V.SetTextSafe(sm_text);
                    NET_LT_GatewayIP_V.SetTextSafe(sm_text);
                }
                NET_RotateGateway.SetVisibleSafe(true);
            }
            else
            {
                cached_gateway_ip = string.Empty;
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string not_connect = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                NET_LT_LocalIP_V.SetTextSafe(not_connect);
                NET_LT_GatewayIP_V.SetTextSafe(not_connect);
                NET_RotateGateway.SetVisibleSafe(false);
            }
        }
        public class GetLocalNetInfo
        {
            public IPAddress LocalIp { get; set; }
            public IPAddress GatewayIp { get; set; }
        }

        public static GetLocalNetInfo GetLocalNetworkInfo()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet && ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211) continue;
                var ipProps = ni.GetIPProperties();
                var localIp = ipProps.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
                if (localIp == null) continue;
                var gateway = ipProps.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
                if (gateway == null) continue;
                return new GetLocalNetInfo
                {
                    LocalIp = localIp,
                    GatewayIp = gateway
                };
            }
            return null;
        }
        private void NET_RotateGateway_Click(object sender, EventArgs e)
        {
            try
            {
                string gatewayText = (cached_gateway_ip ?? string.Empty).Trim();
                if (!IPAddress.TryParse(gatewayText, out _)) { return; }
                var gateway_url = $"http://{gatewayText}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = gateway_url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "NET_RotateGateway_Click()"); }
            }
        }

        // CHECK PRIVATE IP & ISP AND REGION INFO
        private static readonly HttpClient GetPublicIPInfo = CreatePublicIpClient();
        private static HttpClient CreatePublicIpClient()
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/155.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
            client.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
            return client;
        }
        static async Task<GetIpInfo> GetPublicIpInfoAsync()
        {
            try
            {
                string get_data = await GetPublicIPInfo.GetStringAsync("https://ipinfo.io/json").ConfigureAwait(false);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                dynamic serialize_data = serializer.Deserialize<dynamic>(get_data);
                if (serialize_data != null && serialize_data.ContainsKey("ip"))
                {
                    return new GetIpInfo
                    {
                        IP = serialize_data["ip"],
                        ISP = serialize_data["org"],
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public class GetIpInfo
        {
            public string IP { get; set; }
            public string ISP { get; set; }
        }

        // CHECK DNS
        private void UpdateDnsLabels(Label labelDns1, Label labelDns2, Label labelDns3, Label labelDns4)
        {
            try
            {
                string ipv4_1 = "";
                string ipv4_2 = "";
                string ipv6_1 = "";
                string ipv6_2 = "";
                var allInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToList();
                NetworkInterface activeInterface = allInterfaces.Where(IsUsableInterface).OrderByDescending(GetInterfacePriority).FirstOrDefault();
                bool hasNetwork = activeInterface != null;
                bool hasDns = false;
                if (hasNetwork)
                {
                    var ipProps = activeInterface.GetIPProperties();
                    var dnsList = ipProps.DnsAddresses.Where(d => d != null && !d.Equals(IPAddress.Any) && !d.Equals(IPAddress.IPv6Any) && !d.ToString().StartsWith("fec0:0:0:ffff", StringComparison.OrdinalIgnoreCase)).ToList();
                    hasDns = dnsList.Any();
                    var ipv4 = dnsList.Where(d => d.AddressFamily == AddressFamily.InterNetwork).ToList();
                    var ipv6 = dnsList.Where(d => d.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                    ipv4_1 = ipv4.ElementAtOrDefault(0)?.ToString() ?? "";
                    ipv4_2 = ipv4.ElementAtOrDefault(1)?.ToString() ?? "";
                    ipv6_1 = ipv6.ElementAtOrDefault(0)?.ToString() ?? "";
                    ipv6_2 = ipv6.ElementAtOrDefault(1)?.ToString() ?? "";
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string notConnectedText = software_lang.TSReadLangs("Network_Content", "nk_c_not_connect");
                string dnsNotAssignedText = software_lang.TSReadLangs("Network_Content", "nk_c_dns_not");
                if (!hasNetwork)
                {
                    labelDns1.SetTextSafe(notConnectedText);
                    labelDns2.SetTextSafe(notConnectedText);
                    labelDns3.SetTextSafe(notConnectedText);
                    labelDns4.SetTextSafe(notConnectedText);
                    return;
                }
                if (!hasDns)
                {
                    labelDns1.SetTextSafe(dnsNotAssignedText);
                    labelDns2.SetTextSafe(dnsNotAssignedText);
                    labelDns3.SetTextSafe(dnsNotAssignedText);
                    labelDns4.SetTextSafe(dnsNotAssignedText);
                    return;
                }
                SetDnsLabel(labelDns1, ipv4_1, software_lang);
                SetDnsLabel(labelDns2, ipv4_2, software_lang);
                SetDnsLabel(labelDns3, ipv6_1, software_lang);
                SetDnsLabel(labelDns4, ipv6_2, software_lang);
            }
            catch (Exception ex)
            {
                if (debug_status)
                    TSErrorLog.LogException(ex, "UpdateDnsLabels()");
            }
        }
        private bool IsUsableInterface(NetworkInterface ni)
        {
            var props = ni.GetIPProperties();
            return props.GatewayAddresses.Any(g => g?.Address != null && !g.Address.Equals(IPAddress.Any));
        }
        private int GetInterfacePriority(NetworkInterface ni)
        {
            switch (ni.NetworkInterfaceType)
            {
                case NetworkInterfaceType.Ethernet: return 3;
                case NetworkInterfaceType.Wireless80211: return 2;
                default: return 1;
            }
        }
        private void SetDnsLabel(Label label, string dns, TSGetLangs software_lang)
        {
            if (!string.IsNullOrEmpty(dns))
            {
                if (stealth_status == 1)
                {
                    label.SetTextSafe(sm_text);
                }
                else
                {
                    if (IPAddress.TryParse(dns, out IPAddress ip))
                    {
                        DnsProvider provider = null;
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            provider = DnsProviders.FirstOrDefault(p => p.IPv4.Contains(dns));
                        }
                        else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            provider = DnsProviders.FirstOrDefault(p => p.IPv6.Contains(dns));
                        }
                        label.SetTextSafe(provider != null ? $"{dns} ({provider.Name})" : dns);
                    }
                    else
                    {
                        label.SetTextSafe(dns);
                    }
                }
            }
            else
            {
                label.SetTextSafe(software_lang.TSReadLangs("Network_Content", "nk_c_dns_not"));
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
        private void Usb()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            USB_Selector_List.ExecuteSafe(() => USB_Selector_List.Items.Clear());
            USB_DeviceSelector_List.ExecuteSafe(() => USB_DeviceSelector_List.Items.Clear());
            usb_controller_name_list.Clear();
            usb_controller_manufacturer_list.Clear();
            usb_controller_device_id_list.Clear();
            usb_controller_pnp_device_id_list.Clear();
            usb_controller_device_status_list.Clear();
            usb_device_name_list.Clear();
            usb_device_man_list.Clear();
            usb_device_driver_version_list.Clear();
            usb_device_driver_date_list.Clear();
            usb_device_inf_file_list.Clear();
            usb_device_id_list.Clear();
            usb_device_hardware_id_list.Clear();
            usb_device_guid_list.Clear();
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            using (ManagementObjectSearcher search_usb_controller = new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption, Name, Manufacturer, DeviceID, PNPDeviceID, Status FROM Win32_USBController", queryOptions))
            using (ManagementObjectSearcher search_usb_device = new ManagementObjectSearcher("root\\CIMV2", "SELECT FriendlyName, DeviceName, Manufacturer, DriverVersion, DriverDate, InfName, DeviceID, HardWareID, ClassGuid FROM Win32_PnPSignedDriver WHERE DeviceID LIKE 'USB%'", queryOptions))
            {
            using (var usbConCol = search_usb_controller.Get())
            {
                foreach (ManagementObject query_usb_con in usbConCol.Cast<ManagementObject>())
                {
                    using (query_usb_con)
                    {
                        // USB CON CAPTION
                        try
                        {
                            string usb_con_caption = Convert.ToString(query_usb_con["Caption"]).Trim();
                            if (!string.IsNullOrEmpty(usb_con_caption))
                            {
                                USB_Selector_List.AddItemSafe(usb_con_caption);
                            }
                            else
                            {
                                USB_Selector_List.AddItemSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                            }
                        }
                        catch (Exception ex)
                        {
                            USB_Selector_List.AddItemSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Caption"); }
                        }
                        // USB CON NAME
                        try
                        {
                            string usb_con_name = Convert.ToString(query_usb_con["Name"]).Trim();
                            if (!string.IsNullOrEmpty(usb_con_name))
                            {
                                usb_controller_name_list.Add(usb_con_name);
                            }
                            else
                            {
                                usb_controller_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_controller_name_list.Count > 0)
                            {
                                USB_ConName_V.SetTextSafe(usb_controller_name_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_controller_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_controller_name_list.Count == 1) USB_ConName_V.SetTextSafe(usb_controller_name_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Name"); }
                        }
                        // USB CON MANUFACTURER
                        try
                        {
                            string usb_con_man = Convert.ToString(query_usb_con["Manufacturer"]).Trim();
                            if (!string.IsNullOrEmpty(usb_con_man))
                            {
                                usb_controller_manufacturer_list.Add(usb_con_man);
                            }
                            else
                            {
                                usb_controller_manufacturer_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_controller_manufacturer_list.Count > 0)
                            {
                                USB_ConMan_V.SetTextSafe(usb_controller_manufacturer_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_controller_manufacturer_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_controller_manufacturer_list.Count == 1) USB_ConMan_V.SetTextSafe(usb_controller_manufacturer_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Manufacturer"); }
                        }
                        // USB CON DEVICE ID
                        try
                        {
                            string usb_con_device_id = Convert.ToString(query_usb_con["DeviceID"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(usb_con_device_id))
                            {
                                string[] usb_con_device_split = usb_con_device_id.Split('\\');
                                usb_controller_device_id_list.Add(usb_con_device_split.Length >= 2 ? $"{usb_con_device_split[0]}\\{usb_con_device_split[1]}" : usb_con_device_id);
                            }
                            else
                            {
                                usb_controller_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_controller_device_id_list.Count > 0)
                            {
                                USB_ConDeviceID_V.SetTextSafe(usb_controller_device_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_controller_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_controller_device_id_list.Count == 1) USB_ConDeviceID_V.SetTextSafe(usb_controller_device_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> DeviceID"); }
                        }
                        // USB CON PNP DEVICE ID
                        try
                        {
                            string usb_con_pnp_device_id = Convert.ToString(query_usb_con["PNPDeviceID"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(usb_con_pnp_device_id))
                            {
                                string[] usb_con_pnp_device_split = usb_con_pnp_device_id.Split('\\');
                                usb_controller_pnp_device_id_list.Add(usb_con_pnp_device_split.Length >= 2 ? $"{usb_con_pnp_device_split[0]}\\{usb_con_pnp_device_split[1]}" : usb_con_pnp_device_id);
                            }
                            else
                            {
                                usb_controller_pnp_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_controller_pnp_device_id_list.Count > 0)
                            {
                                USB_ConPNPDeviceID_V.SetTextSafe(usb_controller_pnp_device_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_controller_pnp_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_controller_pnp_device_id_list.Count == 1) USB_ConPNPDeviceID_V.SetTextSafe(usb_controller_pnp_device_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> PNPDeviceID"); }
                        }
                        // USB CON DEVICE STATUS
                        try
                        {
                            string usb_con_device_status = (Convert.ToString(query_usb_con["Status"]) ?? string.Empty).Trim().ToLowerInvariant();
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
                            if (!string.IsNullOrEmpty(usb_con_device_status))
                            {
                                if (usbConDeviceStatuses.TryGetValue(usb_con_device_status, out string statusKey))
                                {
                                    usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", statusKey));
                                }
                                else
                                {
                                    usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                                }
                            }
                            else
                            {
                                usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_controller_device_status_list.Count > 0)
                            {
                                USB_ConDeviceStatus_V.SetTextSafe(usb_controller_device_status_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_controller_device_status_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_controller_device_status_list.Count == 1) USB_ConDeviceStatus_V.SetTextSafe(usb_controller_device_status_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Status"); }
                        }
                    }
                }
            }
            using (var usbDevCol = search_usb_device.Get())
            {
                foreach (ManagementObject usb_device in usbDevCol.Cast<ManagementObject>())
                {
                    using (usb_device)
                    {
                        // USB FRIENDLY NAME
                        try
                        {
                            string usb_caption = Convert.ToString(usb_device["FriendlyName"]).Trim();
                            if (!string.IsNullOrEmpty(usb_caption))
                            {
                                USB_DeviceSelector_List.AddItemSafe(usb_caption);
                            }
                            else
                            {
                                string usb_caption_2 = Convert.ToString(usb_device["DeviceName"]).Trim();
                                if (!string.IsNullOrEmpty(usb_caption_2))
                                {
                                    USB_DeviceSelector_List.AddItemSafe(usb_caption_2);
                                }
                                else
                                {
                                    USB_DeviceSelector_List.AddItemSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            USB_DeviceSelector_List.AddItemSafe(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown_device"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> FriendlyName"); }
                        }
                        // USB DEVICE NAME
                        try
                        {
                            string usb_device_name = Convert.ToString(usb_device["FriendlyName"])?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(usb_device_name)) usb_device_name = Convert.ToString(usb_device["DeviceName"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(usb_device_name))
                            {
                                usb_device_name_list.Add(usb_device_name);
                            }
                            else
                            {
                                usb_device_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_name_list.Count > 0)
                            {
                                USB_DeviceName_V.SetTextSafe(usb_device_name_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_name_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_name_list.Count == 1) USB_DeviceName_V.SetTextSafe(usb_device_name_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> DeviceName"); }
                        }
                        // USB MANUFACTURER
                        try
                        {
                            string usb_manufacturer = Convert.ToString(usb_device["Manufacturer"]).Trim();
                            if (!string.IsNullOrEmpty(usb_manufacturer))
                            {
                                usb_device_man_list.Add(usb_manufacturer);
                            }
                            else
                            {
                                usb_device_man_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_man_list.Count > 0)
                            {
                                USB_DeviceMan_V.SetTextSafe(usb_device_man_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_man_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_man_list.Count == 1) USB_DeviceMan_V.SetTextSafe(usb_device_man_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Manufacturer"); }
                        }
                        // USB DRIVER VERSION
                        try
                        {
                            string usb_driver_version = Convert.ToString(usb_device["DriverVersion"]).Trim();
                            if (!string.IsNullOrEmpty(usb_driver_version))
                            {
                                usb_device_driver_version_list.Add(usb_driver_version);
                            }
                            else
                            {
                                usb_device_driver_version_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_driver_version_list.Count > 0)
                            {
                                USB_DriverVersion_V.SetTextSafe(usb_device_driver_version_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_driver_version_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_driver_version_list.Count == 1) USB_DriverVersion_V.SetTextSafe(usb_device_driver_version_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> DriverVersion"); }
                        }
                        // USB DRIVER DATE
                        try
                        {
                            string usb_driver_date = Convert.ToString(usb_device["DriverDate"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(usb_driver_date))
                            {
                                try
                                {
                                    DateTime driverDate = ManagementDateTimeConverter.ToDateTime(usb_driver_date);
                                    usb_device_driver_date_list.Add(driverDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
                                }
                                catch
                                {
                                    usb_device_driver_date_list.Add(usb_driver_date);
                                }
                            }
                            else
                            {
                                usb_device_driver_date_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_driver_date_list.Count > 0)
                            {
                                USB_DriverDate_V.SetTextSafe(usb_device_driver_date_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_driver_date_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_driver_date_list.Count == 1) USB_DriverDate_V.SetTextSafe(usb_device_driver_date_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> DriverDate"); }
                        }
                        // USB INF FILE NAME
                        try
                        {
                            string usb_inf_name = Convert.ToString(usb_device["InfName"]).Trim();
                            if (!string.IsNullOrEmpty(usb_inf_name))
                            {
                                usb_device_inf_file_list.Add(usb_inf_name);
                            }
                            else
                            {
                                usb_device_inf_file_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_inf_file_list.Count > 0)
                            {
                                USB_InfFile_V.SetTextSafe(usb_device_inf_file_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_inf_file_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_inf_file_list.Count == 1) USB_InfFile_V.SetTextSafe(usb_device_inf_file_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> InfName"); }
                        }
                        // USB DEVICE ID
                        try
                        {
                            string usb_device_id = Convert.ToString(usb_device["DeviceID"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(usb_device_id))
                            {
                                string[] usb_con_device_split = usb_device_id.Split('\\');
                                usb_device_id_list.Add(usb_con_device_split.Length >= 2 ? $"{usb_con_device_split[0]}\\{usb_con_device_split[1]}" : usb_device_id);
                            }
                            else
                            {
                                usb_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_id_list.Count > 0)
                            {
                                USB_DeviceID_V.SetTextSafe(usb_device_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_id_list.Count == 1) USB_DeviceID_V.SetTextSafe(usb_device_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> DeviceID (Device)"); }
                        }
                        // USB HARDWARE ID
                        try
                        {
                            object usb_hw_obj = usb_device["HardWareID"];
                            string usb_hardware_id = (usb_hw_obj is string[] usb_hw_arr) ? (usb_hw_arr.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?.Trim() ?? string.Empty) : (Convert.ToString(usb_hw_obj)?.Trim() ?? string.Empty);
                            if (!string.IsNullOrEmpty(usb_hardware_id))
                            {
                                string[] usb_con_device_split = usb_hardware_id.Split('\\');
                                usb_device_hardware_id_list.Add(usb_con_device_split.Length >= 2 ? $"{usb_con_device_split[0]}\\{usb_con_device_split[1]}" : usb_hardware_id);
                            }
                            else
                            {
                                usb_device_hardware_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            if (usb_device_hardware_id_list.Count > 0)
                            {
                                USB_HardwareID_V.SetTextSafe(usb_device_hardware_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_hardware_id_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_hardware_id_list.Count == 1) USB_HardwareID_V.SetTextSafe(usb_device_hardware_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> HardWareID"); }
                        }
                        // USB DEVICE GUID
                        try
                        {
                            string usb_device_guid = Convert.ToString(usb_device["ClassGuid"])?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(usb_device_guid))
                            {
                                usb_device_guid_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            }
                            else
                            {
                                string usb_guid_replacer = usb_device_guid.Replace("{", string.Empty).Replace("}", string.Empty);
                                if (stealth_status != 1)
                                {
                                    usb_device_guid_list.Add(usb_guid_replacer.ToUpper());
                                }
                                else
                                {
                                    usb_device_guid_list.Add(sm_text);
                                }
                            }
                            if (usb_device_guid_list.Count > 0)
                            {
                                USB_DeviceGUID_V.SetTextSafe(usb_device_guid_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            usb_device_guid_list.Add(software_lang.TSReadLangs("Usb_Content", "usb_c_unknown"));
                            if (usb_device_guid_list.Count == 1) USB_DeviceGUID_V.SetTextSafe(usb_device_guid_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> ClassGuid"); }
                        }
                    }
                }
            }
            USB_Selector_List.ExecuteSafe(() =>
            {
                try { if (USB_Selector_List.Items.Count > 0) USB_Selector_List.SelectedIndex = 0; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Select Controller Index"); } }
            });
            USB_DeviceSelector_List.ExecuteSafe(() =>
            {
                try { if (USB_DeviceSelector_List.Items.Count > 0) USB_DeviceSelector_List.SelectedIndex = 0; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "Usb() -> Select Device Index"); } }
            });
            // USB PROCESS END ENABLED
            USB_RotateBtn.SetEnabledSafe(true);
            USB.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- USB Section Loaded --->");
            }
        }
        private void USB_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int usb_con_select = USB_Selector_List.SelectedIndex;
                if (usb_con_select < 0 || usb_con_select >= USB_Selector_List.Items.Count) return;
                if (usb_con_select >= usb_controller_name_list.Count || usb_con_select >= usb_controller_manufacturer_list.Count || usb_con_select >= usb_controller_device_id_list.Count || usb_con_select >= usb_controller_pnp_device_id_list.Count || usb_con_select >= usb_controller_device_status_list.Count) return;
                try { USB_ConName_V.Text = usb_controller_name_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConMan_V.Text = usb_controller_manufacturer_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConDeviceID_V.Text = usb_controller_device_id_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConPNPDeviceID_V.Text = usb_controller_pnp_device_id_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
                try { USB_ConDeviceStatus_V.Text = usb_controller_device_status_list[usb_con_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_Selector_List_SelectedIndexChanged()"); }
        }
        private void USB_DeviceSelector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int usb_select = USB_DeviceSelector_List.SelectedIndex;
                if (usb_select < 0 || usb_select >= USB_DeviceSelector_List.Items.Count) return;
                if (usb_select >= usb_device_name_list.Count || usb_select >= usb_device_man_list.Count || usb_select >= usb_device_driver_version_list.Count || usb_select >= usb_device_driver_date_list.Count || usb_select >= usb_device_inf_file_list.Count || usb_select >= usb_device_id_list.Count || usb_select >= usb_device_hardware_id_list.Count || usb_select >= usb_device_guid_list.Count) return;
                try { USB_DeviceName_V.Text = usb_device_name_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceMan_V.Text = usb_device_man_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DriverVersion_V.Text = usb_device_driver_version_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DriverDate_V.Text = usb_device_driver_date_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_InfFile_V.Text = usb_device_inf_file_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceID_V.Text = usb_device_id_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_HardwareID_V.Text = usb_device_hardware_id_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
                try { USB_DeviceGUID_V.Text = usb_device_guid_list[usb_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "USB_DeviceSelector_List_SelectedIndexChanged()"); }
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
        private void Sound()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            SOUND_Selector_List.ExecuteSafe(() => SOUND_Selector_List.Items.Clear());
            sound_device_name_list.Clear();
            sound_device_manufacturer_list.Clear();
            sound_device_driver_version_list.Clear();
            sound_device_driver_date_list.Clear();
            sound_device_id_list.Clear();
            sound_pnp_device_id_list.Clear();
            sound_device_status_list.Clear();
            EnumerationOptions queryOptions = new EnumerationOptions { ReturnImmediately = true, Rewindable = false, DirectRead = true };
            using (ManagementObjectSearcher search_sound = new ManagementObjectSearcher("root\\CIMV2", "SELECT Caption, Name, Manufacturer, PNPDeviceID, DeviceID, Status FROM Win32_SoundDevice", queryOptions))
            using (ManagementObjectSearcher soundSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT DeviceID, DriverVersion, DriverDate FROM Win32_PnPSignedDriver", queryOptions))
            {
            var soundDriversDict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            using (var results = soundSearcher.Get())
            {
                foreach (ManagementObject d in results.Cast<ManagementObject>())
                {
                    using (d)
                    {
                        string deviceID = Convert.ToString(d["DeviceID"])?.Trim() ?? "";
                        if (!string.IsNullOrEmpty(deviceID) && !soundDriversDict.ContainsKey(deviceID))
                        {
                            soundDriversDict[deviceID] = new string[] { Convert.ToString(d["DriverVersion"])?.Trim() ?? "", Convert.ToString(d["DriverDate"])?.Trim() ?? "" };
                        }
                    }
                }
            }
            using (var soundCol = search_sound.Get())
            {
                foreach (ManagementObject query_sound in soundCol.Cast<ManagementObject>())
                {
                    using (query_sound)
                    {
                        // SOUND DEVICE CAPTION
                        try
                        {
                            string sound_caption = Convert.ToString(query_sound["Caption"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(sound_caption))
                            {
                                SOUND_Selector_List.AddItemSafe(sound_caption);
                            }
                            else
                            {
                                SOUND_Selector_List.AddItemSafe(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown_device"));
                            }
                        }
                        catch (Exception ex)
                        {
                            SOUND_Selector_List.AddItemSafe(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown_device"));
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Caption"); }
                        }
                        // SOUND DEVICE NAME
                        try
                        {
                            string sound_name = Convert.ToString(query_sound["Name"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(sound_name))
                            {
                                sound_device_name_list.Add(sound_name);
                            }
                            else
                            {
                                sound_device_name_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            }
                            if (sound_device_name_list.Count > 0)
                            {
                                SOUND_DeviceName_V.SetTextSafe(sound_device_name_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            sound_device_name_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            if (sound_device_name_list.Count == 1) SOUND_DeviceName_V.SetTextSafe(sound_device_name_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Name"); }
                        }
                        // SOUND DEVICE MANUFACTURER
                        try
                        {
                            string sound_manfuacturer = Convert.ToString(query_sound["Manufacturer"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(sound_manfuacturer))
                            {
                                sound_device_manufacturer_list.Add(sound_manfuacturer);
                            }
                            else
                            {
                                sound_device_manufacturer_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            }
                            if (sound_device_manufacturer_list.Count > 0)
                            {
                                SOUND_DeviceManufacturer_V.SetTextSafe(sound_device_manufacturer_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            sound_device_manufacturer_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            if (sound_device_manufacturer_list.Count == 1) SOUND_DeviceManufacturer_V.SetTextSafe(sound_device_manufacturer_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Manufacturer"); }
                        }
                        // SOUND DRIVER VERSION & DATE
                        try
                        {
                            string unknown = software_lang.TSReadLangs("Sound_Content", "sound_c_unknown");
                            string sound_pnp_device_id = Convert.ToString(query_sound["PNPDeviceID"])?.Trim() ?? "";
                            string driverVersion = unknown;
                            string driverDate = unknown;
                            if (!string.IsNullOrEmpty(sound_pnp_device_id) && soundDriversDict.TryGetValue(sound_pnp_device_id, out string[] driverMatch))
                            {
                                if (!string.IsNullOrEmpty(driverMatch[0])) driverVersion = driverMatch[0];
                                if (!string.IsNullOrEmpty(driverMatch[1]))
                                {
                                    try
                                    {
                                        DateTime dt = ManagementDateTimeConverter.ToDateTime(driverMatch[1]);
                                        driverDate = dt.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                                    }
                                    catch
                                    {
                                        driverDate = driverMatch[1];
                                    }
                                }
                            }
                            sound_device_driver_version_list.Add(driverVersion);
                            sound_device_driver_date_list.Add(driverDate);
                            if (sound_device_driver_version_list.Count > 0) SOUND_DriverVersion_V.SetTextSafe(sound_device_driver_version_list[0]);
                            if (sound_device_driver_date_list.Count > 0) SOUND_DriverDate_V.SetTextSafe(sound_device_driver_date_list[0]);
                        }
                        catch (Exception ex)
                        {
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Driver Info"); }
                            string unknown = software_lang.TSReadLangs("Sound_Content", "sound_c_unknown");
                            sound_device_driver_version_list.Add(unknown);
                            sound_device_driver_date_list.Add(unknown);
                        }
                        // SOUND DEVICE ID
                        try
                        {
                            string sound_device_id = Convert.ToString(query_sound["DeviceID"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(sound_device_id))
                            {
                                string[] sound_device_split = sound_device_id.Split('\\');
                                sound_device_id_list.Add(sound_device_split.Length >= 2 ? $"{sound_device_split[0]}\\{sound_device_split[1]}" : sound_device_id);
                            }
                            else
                            {
                                sound_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            }
                            if (sound_device_id_list.Count > 0)
                            {
                                SOUND_DeviceID_V.SetTextSafe(sound_device_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            sound_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            if (sound_device_id_list.Count == 1) SOUND_DeviceID_V.SetTextSafe(sound_device_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> DeviceID"); }
                        }
                        // SOUND PNP DEVICE ID
                        try
                        {
                            string sound_pnp_device_id = Convert.ToString(query_sound["PNPDeviceID"])?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(sound_pnp_device_id))
                            {
                                string[] sound_pnp_device_split = sound_pnp_device_id.Split('\\');
                                sound_pnp_device_id_list.Add(sound_pnp_device_split.Length >= 2 ? $"{sound_pnp_device_split[0]}\\{sound_pnp_device_split[1]}" : sound_pnp_device_id);
                            }
                            else
                            {
                                sound_pnp_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            }
                            if (sound_pnp_device_id_list.Count > 0)
                            {
                                SOUND_PNPDeviceID_V.SetTextSafe(sound_pnp_device_id_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            sound_pnp_device_id_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            if (sound_pnp_device_id_list.Count == 1) SOUND_PNPDeviceID_V.SetTextSafe(sound_pnp_device_id_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> PNPDeviceID"); }
                        }
                        // SOUND DEVICE STATUS
                        try
                        {
                            string sound_device_status = (Convert.ToString(query_sound["Status"]) ?? string.Empty).Trim().ToLowerInvariant();
                            if (!string.IsNullOrEmpty(sound_device_status))
                            {
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
                                if (soundDeviceStatuses.TryGetValue(sound_device_status, out string statusKey))
                                {
                                    sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", statusKey));
                                }
                                else
                                {
                                    sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                                }
                            }
                            else
                            {
                                sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            }
                            if (sound_device_status_list.Count > 0)
                            {
                                SOUND_DeviceStatus_V.SetTextSafe(sound_device_status_list[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            sound_device_status_list.Add(software_lang.TSReadLangs("Sound_Content", "sound_c_unknown"));
                            if (sound_device_status_list.Count == 1) SOUND_DeviceStatus_V.SetTextSafe(sound_device_status_list[0]);
                            if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Status"); }
                        }
                    }
                }
            }
            SOUND_Selector_List.ExecuteSafe(() =>
            {
                try { if (SOUND_Selector_List.Items.Count > 0) SOUND_Selector_List.SelectedIndex = 0; } catch (Exception ex) { if (debug_status) { TSErrorLog.LogException(ex, "Sound() -> Select Index"); } }
            });
            // SOUND PROCESS END ENABLED
            SOUND_RotateBtn.SetEnabledSafe(true);
            SOUND.SetEnabledSafe(true);
            }
            if (debug_status)
            {
                TSLogger.Log("<--- Sound Section Loaded --->");
            }
        }
        private void SOUND_Selector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int sound_select = SOUND_Selector_List.SelectedIndex;
                if (sound_select < 0 || sound_select >= SOUND_Selector_List.Items.Count) return;
                if (sound_select >= sound_device_name_list.Count || sound_select >= sound_device_manufacturer_list.Count || sound_select >= sound_device_driver_version_list.Count || sound_select >= sound_device_driver_date_list.Count || sound_select >= sound_device_id_list.Count || sound_select >= sound_pnp_device_id_list.Count || sound_select >= sound_device_status_list.Count) return;
                try { SOUND_DeviceName_V.Text = sound_device_name_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceManufacturer_V.Text = sound_device_manufacturer_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DriverVersion_V.Text = sound_device_driver_version_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DriverDate_V.Text = sound_device_driver_date_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceID_V.Text = sound_device_id_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_PNPDeviceID_V.Text = sound_pnp_device_id_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
                try { SOUND_DeviceStatus_V.Text = sound_device_status_list[sound_select]; } catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
            }
            catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "SOUND_Selector_List_SelectedIndexChanged()"); }
        }
        #endregion
        #region BATTERY_Section
        // BATTERY
        // ======================================================================================================
        private void Battery()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                using (ManagementObjectSearcher get_battery_static_data = new ManagementObjectSearcher("root\\WMI", "SELECT DeviceName, SerialNumber, Chemistry, DesignedCapacity FROM BatteryStaticData"))
                {
                bool chemFound = false;
                using (var battStatCol = get_battery_static_data.Get())
                {
                    foreach (ManagementObject batteryStatic in battStatCol.Cast<ManagementObject>())
                    {
                        using (batteryStatic)
                        {
                            chemFound = true;
                            try
                            {
                                // BATTERY NAME
                                string battery_name = batteryStatic["DeviceName"]?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(battery_name))
                                {
                                    BATTERY_Model_V.SetTextSafe(battery_name);
                                }
                                else
                                {
                                    BATTERY_Model_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                }
                            }
                            catch (Exception ex)
                            {
                                BATTERY_Model_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> Name"); }
                            }
                            try
                            {
                                // BATTERY SERIAL
                                string battery_serial = batteryStatic["SerialNumber"]?.ToString()?.Trim();
                                if (!string.IsNullOrEmpty(battery_serial))
                                {
                                    if (stealth_status != 1)
                                    {
                                        BATTERY_Serial_V.SetTextSafe(battery_serial);
                                    }
                                    else
                                    {
                                        BATTERY_Serial_V.SetTextSafe(sm_text);
                                    }
                                }
                                else
                                {
                                    BATTERY_Serial_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                }
                            }
                            catch (Exception ex)
                            {
                                BATTERY_Serial_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> Serial"); }
                            }
                            try
                            {
                                // BATTERY CHEMISTRY
                                string chemistryRaw = null;
                                try
                                {
                                    object chemObj = batteryStatic["Chemistry"];
                                    if (chemObj != null && chemObj != DBNull.Value)
                                    {
                                        byte[] chemistryBytes = BitConverter.GetBytes(Convert.ToUInt32(chemObj));
                                        chemistryRaw = Encoding.ASCII.GetString(chemistryBytes).Trim('\0').Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> Chemistry Parse"); }
                                }
                                if (!string.IsNullOrEmpty(chemistryRaw))
                                {
                                    string chemNorm = chemistryRaw.ToUpperInvariant().Replace("-", "").Replace(" ", "");
                                    string chemDisplay;
                                    if (chemNorm.Contains("LIP")) chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_lithium_polymer");
                                    else if (chemNorm.StartsWith("LI")) chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_lithium_ion");
                                    else if (chemNorm.Contains("NIMH")) chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_nickel_metal_hydride");
                                    else if (chemNorm.Contains("NICD") || chemNorm.Contains("NICA")) chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_nickel_cadmium");
                                    else if (chemNorm.Contains("PBAC") || chemNorm.Contains("LEAD") || chemNorm == "PBA") chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_lead_acid");
                                    else if (chemNorm.Contains("ZNAR") || chemNorm.Contains("ZINC")) chemDisplay = software_lang.TSReadLangs("Battery_Content", "by_c_zinc_air");
                                    else chemDisplay = chemistryRaw;
                                    BATTERY_Chemistry_V.SetTextSafe(chemDisplay);
                                }
                                else
                                {
                                    using (ManagementObjectSearcher battery_sv2_chemistry = new ManagementObjectSearcher("root\\CIMV2", "SELECT Chemistry FROM Win32_Battery"))
                                    using (var chemCol = battery_sv2_chemistry.Get())
                                    {
                                        foreach (ManagementObject b_sv2_chemistry in chemCol.Cast<ManagementObject>())
                                        {
                                            using (b_sv2_chemistry)
                                            {
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
                                                if (batteryTypes.TryGetValue(battery_structure, out string batteryKey))
                                                {
                                                    BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", batteryKey));
                                                }
                                                else
                                                {
                                                    BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> Chemistry"); }
                            }
                            try
                            {
                                // BATTERY DESIGN CAPACITY (ham deger mWh)
                                decimal designedCapacity_mWh = Convert.ToDecimal(batteryStatic["DesignedCapacity"]);
                                if (designedCapacity_mWh <= 0)
                                {
                                    BATTERY_DesignCapacity_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                }
                                else
                                {
                                    BATTERY_DesignCapacity_V.SetTextSafe($"{designedCapacity_mWh:N0} mWh");
                                }
                            }
                            catch (Exception ex)
                            {
                                BATTERY_DesignCapacity_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> DesignCapacity"); }
                            }
                        }
                    }
                }
                if (!chemFound)
                {
                    try
                    {
                        using (ManagementObjectSearcher battery_sv2_chemistry = new ManagementObjectSearcher("root\\CIMV2", "SELECT Chemistry FROM Win32_Battery"))
                        using (var chemFbCol = battery_sv2_chemistry.Get())
                        {
                            bool win32Found = false;
                            foreach (ManagementObject b_sv2_chemistry in chemFbCol.Cast<ManagementObject>())
                            {
                                using (b_sv2_chemistry)
                                {
                                    win32Found = true;
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
                                    if (batteryTypes.TryGetValue(battery_structure, out string batteryKey))
                                    {
                                        BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", batteryKey));
                                    }
                                    else
                                    {
                                        BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                                    }
                                    break;
                                }
                            }
                            if (!win32Found) BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                        }
                    }
                    catch (Exception ex)
                    {
                        BATTERY_Chemistry_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> Chemistry Fallback"); }
                    }
                }
                try
                {
                    // BATTERY FULL CHARGE CAPACITY (ham deger mWh)
                    battery_fullChargedCapacity_mWh = 0;
                    using (ManagementObjectSearcher get_battery_full_charge_capacity = new ManagementObjectSearcher("root\\WMI", "SELECT FullChargedCapacity FROM BatteryFullChargedCapacity"))
                    {
                    bool fullFound = false;
                    using (var fullCol = get_battery_full_charge_capacity.Get())
                    {
                        foreach (ManagementObject batteryFullCharge in fullCol.Cast<ManagementObject>())
                        {
                            using (batteryFullCharge)
                            {
                                fullFound = true;
                                long fullChargedCapacityRaw = Convert.ToInt64(batteryFullCharge["FullChargedCapacity"]);
                                battery_fullChargedCapacity_mWh = fullChargedCapacityRaw;
                                break;
                            }
                        }
                    }
                    if (fullFound && battery_fullChargedCapacity_mWh > 0)
                    {
                        BATTERY_FullChargeCapacity_V.SetTextSafe($"{battery_fullChargedCapacity_mWh:N0} mWh");
                    }
                    else
                    {
                        battery_fullChargedCapacity_mWh = 0;
                        BATTERY_FullChargeCapacity_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                    }
                    }
                }
                catch (Exception ex)
                {
                    battery_fullChargedCapacity_mWh = 0;
                    BATTERY_FullChargeCapacity_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_unknown"));
                    if (debug_status) { TSErrorLog.LogException(ex, "Battery() -> FullChargeCapacity"); }
                }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Battery()"); }
            }
            BATTERY_RotateBtn.SetEnabledSafe(true);
            BATTERY.SetEnabledSafe(true);
            if (debug_status)
            {
                TSLogger.Log("<--- Battery Section Loaded --->");
            }
        }
        private void Battery_visible_off()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            BATTERY_Status_V.SetTextSafe(software_lang.TSReadLangs("Battery_Content", "by_c_battery_not_detect"));
            battery_panel_1.ExecuteSafe(() =>
            {
                battery_panel_1.Height = TSDpiHelper.ScalePx(47, TSDpiHelper.EffectiveDpi(this));
            });
            battery_panel_2.SetVisibleSafe(false);
            BATTERY_Model.SetVisibleSafe(false);
            BATTERY_Model_V.SetVisibleSafe(false);
            BATTERY_Serial.SetVisibleSafe(false);
            BATTERY_Serial_V.SetVisibleSafe(false);
            BATTERY_Chemistry.SetVisibleSafe(false);
            BATTERY_Chemistry_V.SetVisibleSafe(false);
            BATTERY_DesignCapacity.SetVisibleSafe(false);
            BATTERY_DesignCapacity_V.SetVisibleSafe(false);
            BATTERY_FullChargeCapacity.SetVisibleSafe(false);
            BATTERY_FullChargeCapacity_V.SetVisibleSafe(false);
            BATTERY_RemainingChargeCapacity.SetVisibleSafe(false);
            BATTERY_RemainingChargeCapacity_V.SetVisibleSafe(false);
            BATTERY_Voltage.SetVisibleSafe(false);
            BATTERY_Voltage_V.SetVisibleSafe(false);
            BATTERY_ChargePower.SetVisibleSafe(false);
            BATTERY_ChargePower_V.SetVisibleSafe(false);
            BATTERY_ChargeCurrent.SetVisibleSafe(false);
            BATTERY_ChargeCurrent_V.SetVisibleSafe(false);
            BATTERY_DeChargePower.SetVisibleSafe(false);
            BATTERY_DeChargePower_V.SetVisibleSafe(false);
            BATTERY_DeChargeCurrent.SetVisibleSafe(false);
            BATTERY_DeChargeCurrent_V.SetVisibleSafe(false);
            BATTERY_ReportBtn.SetVisibleSafe(false);
            BATTERY_PBG_Panel.SetVisibleSafe(false);
            BATTERY_PFE_Panel.SetVisibleSafe(false);
            BATTERY_ProgressLabel.SetVisibleSafe(false);
        }
        private void Battery_visible_on()
        {
            // Restore the panel height from DPI (any shrinkage heals itself)
            battery_panel_1.ExecuteSafe(() =>
            {
                battery_panel_1.Height = TSDpiHelper.ScalePx(277, TSDpiHelper.EffectiveDpi(this));
            });
            BATTERY_Model.SetVisibleSafe(true);
            BATTERY_Model_V.SetVisibleSafe(true);
            BATTERY_Serial.SetVisibleSafe(true);
            BATTERY_Serial_V.SetVisibleSafe(true);
            BATTERY_Chemistry.SetVisibleSafe(true);
            BATTERY_Chemistry_V.SetVisibleSafe(true);
            BATTERY_DesignCapacity.SetVisibleSafe(true);
            BATTERY_DesignCapacity_V.SetVisibleSafe(true);
            BATTERY_FullChargeCapacity.SetVisibleSafe(true);
            BATTERY_FullChargeCapacity_V.SetVisibleSafe(true);
            BATTERY_RemainingChargeCapacity.SetVisibleSafe(true);
            BATTERY_RemainingChargeCapacity_V.SetVisibleSafe(true);
            BATTERY_Voltage.SetVisibleSafe(true);
            BATTERY_Voltage_V.SetVisibleSafe(true);
            BATTERY_ChargePower.SetVisibleSafe(true);
            BATTERY_ChargePower_V.SetVisibleSafe(true);
            BATTERY_ChargeCurrent.SetVisibleSafe(true);
            BATTERY_ChargeCurrent_V.SetVisibleSafe(true);
            BATTERY_DeChargePower.SetVisibleSafe(true);
            BATTERY_DeChargePower_V.SetVisibleSafe(true);
            BATTERY_DeChargeCurrent.SetVisibleSafe(true);
            BATTERY_DeChargeCurrent_V.SetVisibleSafe(true);
            BATTERY_ReportBtn.SetVisibleSafe(true);
            BATTERY_PBG_Panel.SetVisibleSafe(true);
            BATTERY_PFE_Panel.SetVisibleSafe(true);
            BATTERY_ProgressLabel.SetVisibleSafe(true);
        }
        private async Task BatteryBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    PowerStatus power = SystemInformation.PowerStatus;
                    var software_lang = new TSGetLangs(lang_path);
                    string batteryUnknown = software_lang.TSReadLangs("Battery_Content", "by_c_unknown");
                    float rawPct = power.BatteryLifePercent;
                    bool pctKnown = rawPct >= 0f && rawPct <= 1f;
                    float batteryPercent = pctKnown ? rawPct * 100f : -1f;
                    string batteryStatusText = pctKnown ? string.Format("{0}%", batteryPercent.ToString("F0").Trim()) : batteryUnknown;
                    decimal remainingCapacity_mWh = 0;
                    decimal voltage_V = 0;
                    decimal chargePower_W = 0;
                    decimal dischargePower_W = 0;
                    bool remOk = false, voltOk = false;
                    using (var get_dynamic_battery_info = new ManagementObjectSearcher("root\\WMI", "SELECT RemainingCapacity, Voltage, ChargeRate, DischargeRate FROM BatteryStatus"))
                    using (var battCol = get_dynamic_battery_info.Get())
                    {
                        foreach (ManagementObject batteryStatus in battCol.Cast<ManagementObject>())
                        {
                            using (batteryStatus)
                            {
                                try { remainingCapacity_mWh = Convert.ToInt64(batteryStatus["RemainingCapacity"]); remOk = remainingCapacity_mWh >= 0; } catch { remOk = false; }
                                try { voltage_V = Convert.ToInt32(batteryStatus["Voltage"]) / 1000m; voltOk = voltage_V > 0; } catch { voltOk = false; }
                                try
                                {
                                    long rawCharge = Convert.ToInt32(batteryStatus["ChargeRate"]);
                                    long rawDischarge = Convert.ToInt32(batteryStatus["DischargeRate"]);
                                    chargePower_W = rawCharge > 0 ? rawCharge / 1000m : 0;
                                    dischargePower_W = rawDischarge > 0 ? rawDischarge / 1000m : 0;
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess() -> Loop Data Fetch"); }
                                }
                            }
                        }
                    }
                    BATTERY_RotateBtn.SetTextSafe($"  {software_lang.TSReadLangs("LeftMenu", "left_battery")} - {batteryStatusText}");
                    float barPct = pctKnown ? Math.Max(0f, Math.Min(100f, batteryPercent)) : 0f;
                    BATTERY_PBG_Panel.ExecuteSafe(() =>
                    {
                        BATTERY_PFE_Panel.Height = Math.Max(0, (int)(BATTERY_PBG_Panel.Height * (barPct / 100.0)));
                        BATTERY_ProgressLabel.Text = batteryStatusText;
                        BATTERY_ProgressLabel.Top = BATTERY_PBG_Panel.Top + BATTERY_PFE_Panel.Top + 6;
                    });
                    BATTERY_RemainingChargeCapacity_V.SetTextSafe(remOk ? $"{remainingCapacity_mWh:N0} mWh" : batteryUnknown);
                    BATTERY_Voltage_V.SetTextSafe(voltOk ? $"{voltage_V:N1} V" : batteryUnknown);
                    BATTERY_ChargePower_V.SetTextSafe(chargePower_W > 0 ? $"{chargePower_W:N1} W" : software_lang.TSReadLangs("Battery_Content", "by_c_not_charging"));
                    BATTERY_ChargeCurrent_V.SetTextSafe((chargePower_W > 0 && voltOk) ? $"{(chargePower_W / voltage_V):N1} A" : software_lang.TSReadLangs("Battery_Content", "by_c_not_charging"));
                    BATTERY_DeChargePower_V.SetTextSafe(dischargePower_W > 0 ? $"{dischargePower_W:N1} W" : software_lang.TSReadLangs("Battery_Content", "by_c_not_discharging"));
                    BATTERY_DeChargeCurrent_V.SetTextSafe((dischargePower_W > 0 && voltOk) ? $"{(dischargePower_W / voltage_V):N1} A" : software_lang.TSReadLangs("Battery_Content", "by_c_not_discharging"));
                    string finalStatusString = "";
                    bool lineOnline = false, lineOffline = false;
                    try { lineOnline = power.PowerLineStatus == PowerLineStatus.Online; lineOffline = power.PowerLineStatus == PowerLineStatus.Offline; } catch { }
                    if (battery_fullChargedCapacity_mWh <= 0)
                    {
                        finalStatusString = software_lang.TSReadLangs("Battery_Content", "by_c_not_full_charge_capacity_get");
                    }
                    else if (lineOnline && remOk)
                    {
                        decimal remainingToFull = battery_fullChargedCapacity_mWh - remainingCapacity_mWh;
                        if (remainingToFull > 0 && chargePower_W >= battery_MIN_POWER_THRESHOLD)
                        {
                            var t = TimeSpan.FromHours((double)(remainingToFull / chargePower_W));
                            finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_charging"), batteryStatusText, (int)Math.Floor(t.TotalHours), t.Minutes);
                        }
                        else if (remainingToFull <= 0)
                        {
                            finalStatusString = software_lang.TSReadLangs("Battery_Content", "by_c_full_capacity");
                        }
                        else
                        {
                            finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_fixed_capacity"), batteryStatusText);
                        }
                    }
                    else if (lineOffline && remOk)
                    {
                        if (dischargePower_W >= battery_MIN_POWER_THRESHOLD && remainingCapacity_mWh > 0)
                        {
                            var t = TimeSpan.FromHours((double)(remainingCapacity_mWh / dischargePower_W));
                            finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_discharging"), batteryStatusText, (int)Math.Floor(t.TotalHours), t.Minutes);
                        }
                        else
                        {
                            finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_fixed_capacity"), batteryStatusText);
                        }
                    }
                    else if (chargePower_W >= battery_MIN_POWER_THRESHOLD && remOk)
                    {
                        decimal remainingToFull = battery_fullChargedCapacity_mWh - remainingCapacity_mWh;
                        if (remainingToFull > 0 && chargePower_W > 0)
                        {
                            var t = TimeSpan.FromHours((double)(remainingToFull / chargePower_W));
                            finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_charging"), batteryStatusText, (int)Math.Floor(t.TotalHours), t.Minutes);
                        }
                        else
                        {
                            finalStatusString = software_lang.TSReadLangs("Battery_Content", "by_c_full_capacity");
                        }
                    }
                    else if (dischargePower_W >= battery_MIN_POWER_THRESHOLD && dischargePower_W > 0 && remOk && remainingCapacity_mWh > 0)
                    {
                        var t = TimeSpan.FromHours((double)(remainingCapacity_mWh / dischargePower_W));
                        finalStatusString = string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_discharging"), batteryStatusText, (int)Math.Floor(t.TotalHours), t.Minutes);
                    }
                    else
                    {
                        finalStatusString = pctKnown && batteryPercent < 100 ? string.Format(software_lang.TSReadLangs("Battery_Content", "by_c_fixed_capacity"), batteryStatusText) : software_lang.TSReadLangs("Battery_Content", "by_c_full_capacity");
                    }
                    BATTERY_Status_V.SetTextSafe(finalStatusString);
                    try
                    {
                        await Task.Delay(1000, shutdownToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BatteryBgProcess()"); }
            }
        }
        readonly string battery_report_path = Path.Combine(Path.GetTempPath(), "battery-report.html");
        private async void BATTERY_ReportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                TSGetLangs battery_stealth_lang = new TSGetLangs(lang_path);
                if (stealth_status == 1)
                {
                    DialogResult raw_warn = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(battery_stealth_lang.TSReadLangs("StealthMode", "sm_raw_export_warn"), "\n\n"));
                    if (raw_warn != DialogResult.Yes) { return; }
                }
                try
                {
                    if (File.Exists(battery_report_path))
                        File.Delete(battery_report_path);
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BATTERY_ReportBtn_Click()"); }
                }
                string reportWorkDir = Path.GetDirectoryName(battery_report_path);
                using (Process.Start(new ProcessStartInfo("powercfg", "/batteryreport") { CreateNoWindow = true, UseShellExecute = false, WorkingDirectory = reportWorkDir })) { }
                await StartBatteryReportProcessAsync();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BATTERY_ReportBtn_Click()"); }
            }
        }
        private async Task StartBatteryReportProcessAsync()
        {
            try
            {
                await Task.Run(async () => await Battery_report_check_process_async());
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StartBatteryReportProcessAsync()"); }
            }
        }
        private async Task Battery_report_check_process_async()
        {
            try
            {
                const int reportWaitTimeoutMs = 60000;
                const int lockWaitTimeoutMs = 30000;
                var waitTimeout = Stopwatch.StartNew();
                while (!File.Exists(battery_report_path))
                {
                    if (waitTimeout.ElapsedMilliseconds >= reportWaitTimeoutMs)
                    {
                        if (debug_status) { TSLogger.Log("<--- Battery report was not created within timeout, giving up. --->"); }
                        return;
                    }
                    await Task.Delay(500);
                }
                waitTimeout.Restart();
                bool fileReady = false;
                while (!fileReady)
                {
                    if (waitTimeout.ElapsedMilliseconds >= lockWaitTimeoutMs)
                    {
                        if (debug_status) { TSLogger.Log("<--- Battery report file stayed locked within timeout, giving up. --->"); }
                        return;
                    }
                    try
                    {
                        using (var stream = new FileStream(battery_report_path, FileMode.Open, FileAccess.Read, FileShare.None)) { }
                        fileReady = true;
                    }
                    catch { await Task.Delay(200); }
                }
                string new_battery_report_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{Application.ProductName}_{GetExportHostName()}_battery_report_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.html");
                int duplicateCounter = 1;
                string base_report_path = new_battery_report_path;
                while (File.Exists(new_battery_report_path))
                {
                    string dir = Path.GetDirectoryName(base_report_path);
                    string name = Path.GetFileNameWithoutExtension(base_report_path);
                    new_battery_report_path = Path.Combine(dir, $"{name}_{duplicateCounter}.html");
                    duplicateCounter++;
                }
                try
                {
                    File.Move(battery_report_path, new_battery_report_path);
                }
                catch (IOException)
                {
                    try
                    {
                        File.Copy(battery_report_path, new_battery_report_path, true);
                        try { File.Delete(battery_report_path); } catch { }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "Battery_report_check_process_async()"); }
                        return;
                    }
                }
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string messageText = string.Format(software_lang.TSReadLangs("Battery_Content", "by_report_create_message"), new_battery_report_path, "\n\n");
                DialogResult br_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, messageText);
                if (br_message == DialogResult.Yes)
                {
                    Process.Start(new_battery_report_path);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Battery_report_check_process_async()"); }
            }
        }
        #endregion
        #region OSD_Section
        // OSD
        // ======================================================================================================
        private Task InstalledDrivers()
        {
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
            string L(string key)
            {
                if (!langCache.TryGetValue(key, out string val))
                {
                    val = software_lang.TSReadLangs("Osd_Content", key);
                    langCache[key] = val;
                }
                return val;
            }
            try
            {
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions
                {
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT PathName, DisplayName, ServiceType, StartMode, State FROM Win32_SystemDriver");
                var driverRows = new List<string[]>();
                using (var searcher = new ManagementObjectSearcher(scope, query, options))
                using (var results = searcher.Get())
                {
                    foreach (ManagementBaseObject d in results)
                    {
                        using (d)
                        {
                            string rawPath = (d["PathName"] as string ?? "").Replace("\"", "").Replace("\\??\\", "").Trim();
                            int argIdx = rawPath.IndexOf(" -", StringComparison.Ordinal);
                            if (argIdx < 0) argIdx = rawPath.IndexOf(" /", StringComparison.Ordinal);
                            string path = argIdx > 0 ? rawPath.Substring(0, argIdx).Trim() : rawPath;
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
                    }
                }
                //
                driverRows.Sort((a, b) => string.Compare(a[6], b[6], StringComparison.OrdinalIgnoreCase));
                //
                OSD_DataMainTable.ExecuteSafe(() =>
                {
                    BulkAddRowsFast(driverRows, software_lang);
                });
            }
            catch (Exception ex)
            {
                OSD_DataMainTable.ExecuteSafe(() =>
                {
                    try
                    {
                        OSD_DataMainTable.Rows.Clear();
                        OSD_TYSS_V.Text = "0";
                    }
                    catch { }
                });
                if (debug_status) TSErrorLog.LogException(ex, "InstalledDrivers()");
            }
            finally
            {
                osdLoaded = true;
                OSD_DataMainTable.ExecuteSafe(() =>
                {
                    OSD_RotateBtn.SetEnabledSafe(true);
                    DRIVERS.SetEnabledSafe(true);
                    OSD_DataMainTable.PerformLayout();
                    OSD_DataMainTable.Invalidate();
                });
                if (debug_status) TSLogger.Log("<--- Installed Drivers Section Loaded --->");
            }
            return Task.CompletedTask;
        }
        private void BulkAddRowsFast(List<string[]> driverRows, TSGetLangs software_lang)
        {
            OSD_DataMainTable.SuspendLayout();
            try
            {
                OSD_DataMainTable.Rows.Clear();
                string unknownValue = software_lang.TSReadLangs("Osd_Content", "osd_c_unknown");
                int count = driverRows.Count;
                int dpiHeight = OSD_DataMainTable.RowTemplate.Height;
                DataGridViewRow[] rowsArray = new DataGridViewRow[count];
                for (int i = 0; i < count; i++)
                {
                    string[] data = driverRows[i];
                    for (int j = 0; j < data.Length && j < 6; j++)
                    {
                        if (string.IsNullOrWhiteSpace(data[j])) data[j] = unknownValue;
                    }
                    DataGridViewRow gridRow = new DataGridViewRow();
                    gridRow.CreateCells(OSD_DataMainTable, data.Take(6).ToArray());
                    gridRow.Height = dpiHeight;
                    rowsArray[i] = gridRow;
                }
                OSD_DataMainTable.Rows.AddRange(rowsArray);
                OSD_DataMainTable.ClearSelection();
                OSD_TYSS_V.Text = count.ToString();
                RebuildGridStripe(OSD_DataMainTable);
            }
            finally
            {
                OSD_DataMainTable.ResumeLayout();
            }
        }
        private System.Windows.Forms.Timer OSD_filterTimer;
        private System.Windows.Forms.Timer SERVICE_filterTimer;
        private System.Windows.Forms.Timer INSTAPPS_filterTimer;
        private readonly Dictionary<DataGridView, Dictionary<DataGridViewRow, int>> gridStripeMaps = new Dictionary<DataGridView, Dictionary<DataGridViewRow, int>>();
        private readonly HashSet<DataGridView> stripeSortedWired = new HashSet<DataGridView>();
        private void ApplyGridFilter(DataGridView grid, string searchText)
        {
            bool isEmpty = string.IsNullOrEmpty(searchText);
            string[] searchWords = isEmpty ? new string[0] : searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            grid.CurrentCell = null;
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (isEmpty)
                {
                    row.Visible = true;
                    continue;
                }
                var cellVal = row.Cells[1].Value?.ToString();
                if (string.IsNullOrEmpty(cellVal))
                {
                    row.Visible = false;
                    continue;
                }
                bool isMatch = true;
                foreach (var searchWord in searchWords)
                {
                    if (cellVal.IndexOf(searchWord, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        isMatch = false;
                        break;
                    }
                }
                row.Visible = isMatch;
            }
            if (grid.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Visible)
                    {
                        grid.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            RebuildGridStripe(grid);
        }
        private void ScrollGridToFirstVisibleRow(DataGridView grid)
        {
            try
            {
                if (grid == null || grid.IsDisposed || grid.Rows.Count == 0) return;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Visible && !row.IsNewRow)
                    {
                        grid.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "ScrollGridToFirstVisibleRow()"); }
            }
        }
        private void RebuildGridStripe(DataGridView grid)
        {
            try
            {
                if (grid == null || grid.IsDisposed) return;
                if (stripeSortedWired.Add(grid))
                {
                    grid.Sorted += (s, ev) =>
                    {
                        try { RebuildGridStripe(grid); grid.Invalidate(); } catch { }
                    };
                }
                var map = new Dictionary<DataGridViewRow, int>();
                int stripe = 0;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Visible) map[row] = stripe++;
                }
                gridStripeMaps[grid] = map;
            }
            catch { }
        }
        private void ApplyGridStripePaint(DataGridView grid, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count) return;
            DataGridViewRow row = grid.Rows[e.RowIndex];
            if (!row.Visible) return;
            int stripe = 0;
            if (gridStripeMaps.TryGetValue(grid, out var map) && map != null && map.TryGetValue(row, out int s)) stripe = s;
            if (stripe % 2 == 0)
            {
                row.DefaultCellStyle.BackColor = grid.DefaultCellStyle.BackColor;
            }
            else
            {
                row.DefaultCellStyle.BackColor = grid.AlternatingRowsDefaultCellStyle.BackColor != Color.Empty ? grid.AlternatingRowsDefaultCellStyle.BackColor : grid.DefaultCellStyle.BackColor;
            }
        }
        private void OSD_TextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OSD_TextBoxClearBtn.Enabled = !string.IsNullOrEmpty(OSD_TextBox.Text.Trim());
                if (OSD_filterTimer == null)
                {
                    OSD_filterTimer = this.components != null ? new System.Windows.Forms.Timer(this.components) : new System.Windows.Forms.Timer();
                    OSD_filterTimer.Interval = 250;
                    OSD_filterTimer.Tick += (s, ev) =>
                    {
                        try
                        {
                            OSD_filterTimer.Stop();
                            ApplyGridFilter(OSD_DataMainTable, OSD_TextBox.Text.Trim());
                        }
                        catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "OSD_TextBox_TextChanged()"); }
                    };
                }
                OSD_filterTimer.Stop();
                OSD_filterTimer.Start();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_TextBox_TextChanged()"); }
            }
        }
        private void OSD_DataMainTable_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                ApplyGridStripePaint(OSD_DataMainTable, e);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_DataMainTable_RowPrePaint()"); }
            }
        }
        private void OSD_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string firstCellText = string.Empty;
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= OSD_DataMainTable.Rows.Count) return;
                var clickedRow = OSD_DataMainTable.Rows[e.RowIndex];
                if (clickedRow.IsNewRow || clickedRow.Cells.Count < 6) return;
                var cells = clickedRow.Cells;
                firstCellText = cells[0].Value?.ToString() ?? string.Empty;
                Clipboard.SetText(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", firstCellText, cells[1].Value?.ToString() ?? string.Empty, cells[2].Value?.ToString() ?? string.Empty, cells[3].Value?.ToString() ?? string.Empty, cells[4].Value?.ToString() ?? string.Empty, cells[5].Value?.ToString() ?? string.Empty));
                TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("Osd_Content", "osd_c_copy_success"), firstCellText));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_DataMainTable_CellDoubleClick()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Osd_Content", "osd_c_copy_error"), firstCellText));
            }
        }
        private void OSD_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (OSD_DataMainTable.SelectedRows.Count > 0)
                {
                    OSD_DataMainTable.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void OSD_TextBoxClearBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OSD_TextBox.Text = string.Empty;
                OSD_TextBox.Focus();
                if (OSD_DataMainTable.Rows.Count > 0) ScrollGridToFirstVisibleRow(OSD_DataMainTable);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_TextBoxClearBtn_Click()"); }
            }
        }
        private void OSD_SortMode_CheckedChanged(object sender, EventArgs e)
        {
            var sortCol = OSD_DataMainTable.Columns[1];
            var prevMode = sortCol.SortMode;
            try
            {
                sortCol.SortMode = DataGridViewColumnSortMode.Automatic;
                if (OSD_SortMode.CheckState == CheckState.Checked)
                {
                    OSD_DataMainTable.Sort(sortCol, ListSortDirection.Descending);
                }
                else if (OSD_SortMode.CheckState == CheckState.Unchecked)
                {
                    OSD_DataMainTable.Sort(sortCol, ListSortDirection.Ascending);
                }
                OSD_DataMainTable.ClearSelection();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "OSD_SortMode_CheckedChanged()"); }
            }
            finally
            {
                try { sortCol.SortMode = prevMode; } catch { }
            }
        }
        private volatile bool osdLoaded = false;
        private async Task DriversBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                await Task.Delay(5000, shutdownToken);
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    try
                    {
                        if (!this.IsDisposed && !this.Disposing && osdLoaded)
                        {
                            RefreshDriverStates(shutdownToken);
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "DriversBgProcess()"); }
                    }
                    try
                    {
                        await Task.Delay(5000, shutdownToken);
                    }
                    catch (OperationCanceledException) { break; }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DriversBgProcess()"); }
            }
        }
        private void RefreshDriverStates(CancellationToken shutdownToken)
        {
            try
            {
                if (this.IsDisposed || this.Disposing) return;
                if (shutdownToken.IsCancellationRequested || !loop_status) return;
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string stoppedText = software_lang.TSReadLangs("Osd_Content", "osd_c_stopped");
                string runningText = software_lang.TSReadLangs("Osd_Content", "osd_c_working");
                string unknownText = software_lang.TSReadLangs("Osd_Content", "osd_c_unknown");
                var stateByDisp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var stateByFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions
                {
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT DisplayName, PathName, State FROM Win32_SystemDriver");
                using (var searcher = new ManagementObjectSearcher(scope, query, options))
                using (var results = searcher.Get())
                {
                    foreach (ManagementBaseObject d in results)
                    {
                        using (d)
                        {
                            if (shutdownToken.IsCancellationRequested || !loop_status) break;
                            string disp = (d["DisplayName"] as string ?? "").Trim();
                            string rawPath = (d["PathName"] as string ?? "").Replace("\"", "").Replace("\\??\\", "").Trim();
                            int argIdx = rawPath.IndexOf(" -", StringComparison.Ordinal);
                            if (argIdx < 0) argIdx = rawPath.IndexOf(" /", StringComparison.Ordinal);
                            string path = argIdx > 0 ? rawPath.Substring(0, argIdx).Trim() : rawPath;
                            string file = "";
                            try { file = Path.GetFileName(path); } catch { file = ""; }
                            string state = (d["State"] as string ?? "").Trim();
                            if (!string.IsNullOrEmpty(disp) && !stateByDisp.ContainsKey(disp)) stateByDisp[disp] = state;
                            if (!string.IsNullOrEmpty(file) && !stateByFile.ContainsKey(file)) stateByFile[file] = state;
                        }
                    }
                }
                if (shutdownToken.IsCancellationRequested || !loop_status) return;
                if (stateByDisp.Count == 0 && stateByFile.Count == 0) return;
                OSD_DataMainTable.ExecuteSafe(() =>
                {
                    try
                    {
                        if (OSD_DataMainTable.Rows.Count == 0) return;
                        if (OSD_DataMainTable.IsDisposed || this.IsDisposed || this.Disposing) return;
                        foreach (DataGridViewRow row in OSD_DataMainTable.Rows)
                        {
                            if (row.IsNewRow || row.Cells.Count < 6) continue;
                            string fileVal = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty;
                            string dispVal = row.Cells[2].Value?.ToString()?.Trim() ?? string.Empty;
                            string rawState = null;
                            if (!string.IsNullOrEmpty(dispVal) && !stateByDisp.TryGetValue(dispVal, out rawState))
                            {
                                if (!string.IsNullOrEmpty(fileVal)) stateByFile.TryGetValue(fileVal, out rawState);
                            }
                            else if (string.IsNullOrEmpty(dispVal) && !string.IsNullOrEmpty(fileVal))
                            {
                                stateByFile.TryGetValue(fileVal, out rawState);
                            }
                            if (rawState == null) continue;
                            string newState = rawState == "Running" ? runningText : rawState == "Stopped" ? stoppedText : unknownText;
                            var cell = row.Cells[5];
                            string cur = cell.Value?.ToString();
                            if (!string.Equals(cur, newState, StringComparison.Ordinal)) cell.Value = newState;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "RefreshDriverStates() -> UI Update"); }
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "RefreshDriverStates()"); }
            }
        }
        #endregion
        #region SERVICES_Section
        // GS SERVICE
        // ======================================================================================================
        private Task InstalledServices()
        {
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
            string L(string key)
            {
                if (!langCache.TryGetValue(key, out string val))
                {
                    val = software_lang.TSReadLangs("Services_Content", key);
                    langCache[key] = val;
                }
                return val;
            }
            try
            {
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions
                {
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT PathName, Name, DisplayName, ServiceType, StartMode, State FROM Win32_Service");
                var serviceRows = new List<string[]>();
                //
                using (var searcher = new ManagementObjectSearcher(scope, query, options))
                using (var results = searcher.Get())
                {
                    foreach (ManagementBaseObject s in results)
                    {
                        using (s)
                        {
                            string rawPath = (s["PathName"] as string ?? "").Replace("\"", "").Replace("\\??\\", "").Trim();
                            int argIdx = rawPath.IndexOf(" -", StringComparison.Ordinal);
                            if (argIdx < 0) argIdx = rawPath.IndexOf(" /", StringComparison.Ordinal);
                            string path = argIdx > 0 ? rawPath.Substring(0, argIdx).Trim() : rawPath;
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
                    }
                }
                //
                serviceRows.Sort((a, b) => string.Compare(a[6], b[6], StringComparison.OrdinalIgnoreCase));
                //
                SERVICE_DataMainTable.ExecuteSafe(() =>
                {
                    BulkAddServiceRowsFast(serviceRows, software_lang);
                });
            }
            catch (Exception ex)
            {
                SERVICE_DataMainTable.ExecuteSafe(() =>
                {
                    try
                    {
                        SERVICE_DataMainTable.Rows.Clear();
                        SERVICE_TYS_V.Text = "0";
                    }
                    catch { }
                });
                if (debug_status) TSErrorLog.LogException(ex, "InstalledServices()");
            }
            finally
            {
                servicesLoaded = true;
                SERVICE_DataMainTable.ExecuteSafe(() =>
                {
                    SERVICES_RotateBtn.SetEnabledSafe(true);
                    SERVICES.SetEnabledSafe(true);
                    SERVICE_DataMainTable.PerformLayout();
                    SERVICE_DataMainTable.Invalidate();
                });
                if (debug_status) TSLogger.Log("<--- Installed Services Section Loaded --->");
            }
            return Task.CompletedTask;
        }
        private void BulkAddServiceRowsFast(List<string[]> serviceRows, TSGetLangs software_lang)
        {
            SERVICE_DataMainTable.SuspendLayout();
            try
            {
                SERVICE_DataMainTable.Rows.Clear();
                string unknownValue = software_lang.TSReadLangs("Services_Content", "ss_c_unknown");
                int count = serviceRows.Count;
                int dpiHeight = SERVICE_DataMainTable.RowTemplate.Height;
                DataGridViewRow[] rowsArray = new DataGridViewRow[count];
                for (int i = 0; i < count; i++)
                {
                    string[] rowData = serviceRows[i];
                    for (int j = 0; j < rowData.Length && j < 6; j++)
                    {
                        if (string.IsNullOrWhiteSpace(rowData[j])) rowData[j] = unknownValue;
                    }
                    DataGridViewRow gridRow = new DataGridViewRow();
                    gridRow.CreateCells(SERVICE_DataMainTable, rowData.Take(6).ToArray());
                    gridRow.Height = dpiHeight;
                    rowsArray[i] = gridRow;
                }
                SERVICE_DataMainTable.Rows.AddRange(rowsArray);
                SERVICE_DataMainTable.ClearSelection();
                SERVICE_TYS_V.Text = count.ToString();
                RebuildGridStripe(SERVICE_DataMainTable);
            }
            finally
            {
                SERVICE_DataMainTable.ResumeLayout();
            }
        }
        private void Services_SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SERVICE_TextBoxClearBtn.Enabled = !string.IsNullOrEmpty(SERVICE_TextBox.Text.Trim());
                if (SERVICE_filterTimer == null)
                {
                    SERVICE_filterTimer = this.components != null ? new System.Windows.Forms.Timer(this.components) : new System.Windows.Forms.Timer();
                    SERVICE_filterTimer.Interval = 250;
                    SERVICE_filterTimer.Tick += (s, ev) =>
                    {
                        try
                        {
                            SERVICE_filterTimer.Stop();
                            ApplyGridFilter(SERVICE_DataMainTable, SERVICE_TextBox.Text.Trim());
                        }
                        catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "Services_SearchTextBox_TextChanged()"); }
                    };
                }
                SERVICE_filterTimer.Stop();
                SERVICE_filterTimer.Start();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Services_SearchTextBox_TextChanged()"); }
            }
        }
        private void SERVICE_DataMainTable_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                ApplyGridStripePaint(SERVICE_DataMainTable, e);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_DataMainTable_RowPrePaint()"); }
            }
        }
        private void SERVICE_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            string firstCellText = string.Empty;
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= SERVICE_DataMainTable.Rows.Count) return;
                var clickedRow = SERVICE_DataMainTable.Rows[e.RowIndex];
                if (clickedRow.IsNewRow || clickedRow.Cells.Count < 6) return;
                var cells = clickedRow.Cells;
                firstCellText = cells[0].Value?.ToString() ?? string.Empty;
                Clipboard.SetText(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", firstCellText, cells[1].Value?.ToString() ?? string.Empty, cells[2].Value?.ToString() ?? string.Empty, cells[3].Value?.ToString() ?? string.Empty, cells[4].Value?.ToString() ?? string.Empty, cells[5].Value?.ToString() ?? string.Empty));
                TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("Services_Content", "ss_c_copy_success"), firstCellText));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_DataMainTable_CellDoubleClick()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("Services_Content", "ss_c_copy_error"), firstCellText));
            }
        }
        private void SERVICE_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (SERVICE_DataMainTable.SelectedRows.Count > 0)
                {
                    SERVICE_DataMainTable.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void SERVICE_TextBoxClearBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SERVICE_TextBox.Text = string.Empty;
                SERVICE_TextBox.Focus();
                if (SERVICE_DataMainTable.Rows.Count > 0) ScrollGridToFirstVisibleRow(SERVICE_DataMainTable);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICE_TextBoxClearBtn_Click()"); }
            }
        }
        private void SERVICES_SortMode_CheckedChanged(object sender, EventArgs e)
        {
            var sortCol = SERVICE_DataMainTable.Columns[1];
            var prevMode = sortCol.SortMode;
            try
            {
                sortCol.SortMode = DataGridViewColumnSortMode.Automatic;
                if (SERVICE_SortMode.CheckState == CheckState.Checked)
                {
                    SERVICE_DataMainTable.Sort(sortCol, ListSortDirection.Descending);
                }
                else if (SERVICE_SortMode.CheckState == CheckState.Unchecked)
                {
                    SERVICE_DataMainTable.Sort(sortCol, ListSortDirection.Ascending);
                }
                SERVICE_DataMainTable.ClearSelection();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SERVICES_SortMode_CheckedChanged()"); }
            }
            finally
            {
                try { sortCol.SortMode = prevMode; } catch { }
            }
        }
        private volatile bool servicesLoaded = false;
        private async Task ServicesBgProcess(CancellationToken shutdownToken)
        {
            try
            {
                await Task.Delay(5000, shutdownToken);
                while (!shutdownToken.IsCancellationRequested && loop_status)
                {
                    try
                    {
                        if (!this.IsDisposed && !this.Disposing && servicesLoaded)
                        {
                            RefreshServiceStates(shutdownToken);
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "ServicesBgProcess()"); }
                    }
                    try
                    {
                        await Task.Delay(5000, shutdownToken);
                    }
                    catch (OperationCanceledException) { break; }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "ServicesBgProcess()"); }
            }
        }
        private void RefreshServiceStates(CancellationToken shutdownToken)
        {
            try
            {
                if (this.IsDisposed || this.Disposing) return;
                if (shutdownToken.IsCancellationRequested || !loop_status) return;
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                string stoppedText = software_lang.TSReadLangs("Services_Content", "ss_c_stopped");
                string runningText = software_lang.TSReadLangs("Services_Content", "ss_c_working");
                string unknownText = software_lang.TSReadLangs("Services_Content", "ss_c_unknown");
                var stateByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var scope = new ManagementScope(@"\\.\root\CIMV2");
                scope.Connect();
                var options = new EnumerationOptions
                {
                    ReturnImmediately = true,
                    Rewindable = false,
                    DirectRead = true
                };
                var query = new ObjectQuery("SELECT Name, State FROM Win32_Service");
                using (var searcher = new ManagementObjectSearcher(scope, query, options))
                using (var results = searcher.Get())
                {
                    foreach (ManagementBaseObject s in results)
                    {
                        using (s)
                        {
                            if (shutdownToken.IsCancellationRequested || !loop_status) break;
                            string name = (s["Name"] as string ?? "").Trim();
                            string state = (s["State"] as string ?? "").Trim();
                            if (!string.IsNullOrEmpty(name) && !stateByName.ContainsKey(name)) stateByName[name] = state;
                        }
                    }
                }
                if (shutdownToken.IsCancellationRequested || !loop_status) return;
                if (stateByName.Count == 0) return;
                SERVICE_DataMainTable.ExecuteSafe(() =>
                {
                    try
                    {
                        if (SERVICE_DataMainTable.Rows.Count == 0) return;
                        if (SERVICE_DataMainTable.IsDisposed || this.IsDisposed || this.Disposing) return;
                        foreach (DataGridViewRow row in SERVICE_DataMainTable.Rows)
                        {
                            if (row.IsNewRow || row.Cells.Count < 6) continue;
                            string nameVal = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(nameVal)) continue;
                            if (!stateByName.TryGetValue(nameVal, out string rawState)) continue;
                            string newState = rawState == "Running" ? runningText : rawState == "Stopped" ? stoppedText : unknownText;
                            var cell = row.Cells[5];
                            string cur = cell.Value?.ToString();
                            if (!string.Equals(cur, newState, StringComparison.Ordinal)) cell.Value = newState;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "RefreshServiceStates() -> UI Update"); }
                    }
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "RefreshServiceStates()"); }
            }
        }
        #endregion
        #region INSTAPPS
        private Task InstalledApps()
        {
            try
            {
                LoadInstalledApplications();
                INSTAPPS_DataMainTable.ExecuteSafe(() =>
                {
                    INSTAPPS_TYUS_V.SetTextSafe(INSTAPPS_DataMainTable.Rows.Count.ToString());
                    INSTAPPS_DataMainTable.ClearSelection();
                });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "InstalledApps()"); }
            }
            finally
            {
                INSTAPPS_DataMainTable.ExecuteSafe(() =>
                {
                    INSTALLED_RotateBtn.SetEnabledSafe(true);
                    INSTAPPS.SetEnabledSafe(true);
                    INSTAPPS_DataMainTable.PerformLayout();
                    INSTAPPS_DataMainTable.Invalidate();
                });
                if (debug_status)
                {
                    TSLogger.Log("<--- Installed Application Section Loaded --->");
                }
            }
            return Task.CompletedTask;
        }
        private void INSTAPPS_TextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                INSTAPPS_TextBoxClearBtn.Enabled = !string.IsNullOrEmpty(INSTAPPS_TextBox.Text.Trim());
                if (INSTAPPS_filterTimer == null)
                {
                    INSTAPPS_filterTimer = this.components != null ? new System.Windows.Forms.Timer(this.components) : new System.Windows.Forms.Timer();
                    INSTAPPS_filterTimer.Interval = 250;
                    INSTAPPS_filterTimer.Tick += (s, ev) =>
                    {
                        try
                        {
                            INSTAPPS_filterTimer.Stop();
                            ApplyGridFilter(INSTAPPS_DataMainTable, INSTAPPS_TextBox.Text.Trim());
                        }
                        catch (Exception ex) { if (debug_status) TSErrorLog.LogException(ex, "INSTAPPS_TextBox_TextChanged()"); }
                    };
                }
                INSTAPPS_filterTimer.Stop();
                INSTAPPS_filterTimer.Start();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_TextBox_TextChanged()"); }
            }
        }
        private void INSTAPPS_DataMainTable_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                ApplyGridStripePaint(INSTAPPS_DataMainTable, e);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_DataMainTable_RowPrePaint()"); }
            }
        }
        private void INSTAPPS_DataMainTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= INSTAPPS_DataMainTable.Rows.Count) return;
            var clickedRow = INSTAPPS_DataMainTable.Rows[e.RowIndex];
            if (clickedRow.IsNewRow || clickedRow.Cells.Count <= 6) return;
            //
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            var selected_row = clickedRow;
            //
            DialogResult open_apps_folder = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Instapps_Content", "ia_open_apps"), selected_row.Cells[1].Value));
            if (open_apps_folder == DialogResult.Yes)
            {
                var installPath = selected_row.Cells[6].Value as string;
                if (string.IsNullOrWhiteSpace(installPath) || installPath == iapps_unknown || !Directory.Exists(installPath))
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Instapps_Content", "ia_not_path"));
                    return;
                }
                try
                {
                    var exePath = Directory.GetFiles(installPath, "*.exe").FirstOrDefault();
                    if (string.IsNullOrEmpty(exePath))
                    {
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Instapps_Content", "ia_not_exe"));
                        return;
                    }
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{exePath}\"") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_DataMainTable_CellDoubleClick()"); }
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("Instapps_Content", "ia_file_path_not_oppened"), ex.Message));
                }
            }
        }
        private void INSTAPPS_DataMainTable_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (INSTAPPS_DataMainTable.SelectedRows.Count > 0)
                {
                    INSTAPPS_DataMainTable.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_DataMainTable_ColumnHeaderMouseClick()"); }
            }
        }
        private void INSTAPPS_TextBoxClearBtn_Click(object sender, EventArgs e)
        {
            try
            {
                INSTAPPS_TextBox.Text = string.Empty;
                INSTAPPS_TextBox.Focus();
                if (INSTAPPS_DataMainTable.Rows.Count > 0) ScrollGridToFirstVisibleRow(INSTAPPS_DataMainTable);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_TextBoxClearBtn_Click()"); }
            }
        }
        private void INSTAPPS_SortMode_CheckedChanged(object sender, EventArgs e)
        {
            var sortCol = INSTAPPS_DataMainTable.Columns[1];
            var prevMode = sortCol.SortMode;
            try
            {
                sortCol.SortMode = DataGridViewColumnSortMode.Automatic;
                if (INSTAPPS_SortMode.CheckState == CheckState.Checked)
                {
                    INSTAPPS_DataMainTable.Sort(sortCol, ListSortDirection.Descending);
                }
                else if (INSTAPPS_SortMode.CheckState == CheckState.Unchecked)
                {
                    INSTAPPS_DataMainTable.Sort(sortCol, ListSortDirection.Ascending);
                }
                INSTAPPS_DataMainTable.ClearSelection();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "INSTAPPS_SortMode_CheckedChanged()"); }
            }
            finally
            {
                try { sortCol.SortMode = prevMode; } catch { }
            }
        }
        public class InstalledAppConfig
        {
            public string DisplayIcon { get; set; }
            public Icon AppIcon { get; set; }
            public string TargetExePath { get; set; }
            public string Name { get; set; }
            public string Publisher { get; set; }
            public DateTime? InstallDate { get; set; }
            public long? Size { get; set; }
            public string Version { get; set; }
            public string InstallLocation { get; set; }
            public List<string> ExeFileCache { get; set; }
        }
        public static class AppWindowIcon
        {
            [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
            private static extern uint ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
            [DllImport("user32.dll")]
            private static extern bool DestroyIcon(IntPtr hIcon);
            public static Icon ExtractIconFromFile(string filePath, int index, bool large = true)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return null;
                }
                IntPtr[] largeIcons = large ? new IntPtr[1] : null;
                IntPtr[] smallIcons = !large ? new IntPtr[1] : null;
                uint count = ExtractIconEx(filePath, index, largeIcons, smallIcons, 1);
                if (count == 0)
                {
                    return null;
                }
                IntPtr hIcon = large ? largeIcons[0] : smallIcons[0];
                if (hIcon == IntPtr.Zero)
                    return null;
                try
                {
                    return (Icon)Icon.FromHandle(hIcon).Clone();
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }
        private static void ParseDisplayIcon(string displayIconRaw, out string iconPath, out int iconIndex)
        {
            iconIndex = 0;
            iconPath = string.Empty;
            if (string.IsNullOrWhiteSpace(displayIconRaw))
            {
                return;
            }
            string s = Environment.ExpandEnvironmentVariables(displayIconRaw).Trim();
            if (s.StartsWith("@", StringComparison.Ordinal))
            {
                s = s.Substring(1).Trim();
            }
            s = s.Trim().Trim('"');
            int comma = s.LastIndexOf(',');
            if (comma > 0 && comma < s.Length - 1)
            {
                string pathPart = s.Substring(0, comma).Trim().Trim('"');
                string indexPart = s.Substring(comma + 1).Trim();
                if (int.TryParse(indexPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx))
                {
                    iconPath = pathPart;
                    iconIndex = idx;
                    return;
                }
            }
            iconPath = s;
            iconIndex = 0;
        }
        private Image _defaultIconCache;
        private Image GetDefaultIcon()
        {
            if (_defaultIconCache != null)
            {
                return (Image)_defaultIconCache.Clone();
            }
            try
            {
                using (var ico = AppWindowIcon.ExtractIconFromFile("shell32.dll", 2, large: true))
                {
                    if (ico != null)
                    {
                        _defaultIconCache = ico.ToBitmap();
                        return (Image)_defaultIconCache.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "GetDefaultIcon()"); }
            }
            Bitmap bmpFallback = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmpFallback))
            {
                g.Clear(TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor"));
                g.DrawRectangle(Pens.Black, 4, 4, 24, 24);
            }
            _defaultIconCache = bmpFallback;
            return (Image)_defaultIconCache.Clone();
        }
        private void LoadInstalledApplications()
        {
            var apps = GetInstalledApplications();
            INSTAPPS_DataMainTable.ExecuteSafe(() =>
            {
                INSTAPPS_DataMainTable.SuspendLayout();
                try
                {
                    foreach (DataGridViewRow oldRow in INSTAPPS_DataMainTable.Rows)
                    {
                        try { (oldRow.Cells[0].Value as Image)?.Dispose(); } catch { }
                        try { (oldRow.Tag as Image)?.Dispose(); } catch { }
                    }
                }
                catch { }
                INSTAPPS_DataMainTable.Rows.Clear();
            });
            var rowsToAdd = new List<object[]>();
            var iconMasters = new List<Image>();
            foreach (var app in apps)
            {
                Image iconImage = null;
                if (!string.IsNullOrWhiteSpace(app.DisplayIcon))
                {
                    try
                    {
                        ParseDisplayIcon(app.DisplayIcon, out string iconPath, out int iconIndex);
                        if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
                        {
                            string ext = Path.GetExtension(iconPath);
                            if (string.Equals(ext, ".ico", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var ico = new Icon(iconPath)) { iconImage = ico.ToBitmap(); }
                            }
                            else
                            {
                                using (var ico = AppWindowIcon.ExtractIconFromFile(iconPath, iconIndex, large: true))
                                {
                                    if (ico != null) iconImage = ico.ToBitmap();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) TSErrorLog.LogException(ex, "LoadInstalledApplications() - DisplayIcon");
                    }
                }
                if (iconImage == null && !string.IsNullOrEmpty(app.TargetExePath))
                {
                    try
                    {
                        using (Icon icon = Icon.ExtractAssociatedIcon(app.TargetExePath))
                        {
                            if (icon != null) iconImage = icon.ToBitmap();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) TSErrorLog.LogException(ex, "LoadInstalledApplications() - TargetExeIcon");
                    }
                }
                if (iconImage == null) { iconImage = GetDefaultIcon(); }
                Image masterIcon = iconImage;
                Image cellIcon = ResizeDGIcon(masterIcon, 30, this.DeviceDpi);
                iconMasters.Add(masterIcon);
                var nameText = string.IsNullOrWhiteSpace(app.Name) ? iapps_unknown : app.Name;
                var publisherText = string.IsNullOrWhiteSpace(app.Publisher) ? iapps_unknown : app.Publisher;
                string installDateText = app.InstallDate.HasValue ? app.InstallDate.Value.ToString("dd.MM.yyyy") : (GetInstallDateFromLocation(app.InstallLocation, app.TargetExePath, app.ExeFileCache) ?? iapps_unknown);
                var sizeText = app.Size.HasValue ? TS_FormatSize(app.Size.Value * 1024) : iapps_unknown;
                var versionText = string.IsNullOrWhiteSpace(app.Version) ? iapps_unknown : app.Version;
                string installLocationText = !string.IsNullOrWhiteSpace(app.InstallLocation) ? TS_NormalizePath(app.InstallLocation) : iapps_unknown;
                rowsToAdd.Add(new object[] { cellIcon, nameText, publisherText, installDateText, sizeText, versionText, installLocationText });
            }
            INSTAPPS_DataMainTable.ExecuteSafe(() =>
            {
                try
                {
                    foreach (var row in rowsToAdd)
                    {
                        INSTAPPS_DataMainTable.Rows.Add(row);
                    }
                    int tagCount = Math.Min(iconMasters.Count, INSTAPPS_DataMainTable.Rows.Count);
                    for (int i = 0; i < tagCount; i++) INSTAPPS_DataMainTable.Rows[i].Tag = iconMasters[i];
                    RebuildGridStripe(INSTAPPS_DataMainTable);
                }
                finally
                {
                    INSTAPPS_DataMainTable.ResumeLayout();
                }
            });
        }
        // Regenerates cell icons from the master on DPI change (no disk/registry access, selection preserved)
        private void RefreshInstappsIcons(int dpiOverride = 0)
        {
            try
            {
                if (IsDisposed || Disposing || INSTAPPS_DataMainTable.IsDisposed) return;
                int dpi = dpiOverride > 0 ? dpiOverride : TSDpiHelper.EffectiveDpi(INSTAPPS_DataMainTable);
                if (dpi <= 0) dpi = TSDpiHelper.BaseDpi;
                foreach (DataGridViewRow row in INSTAPPS_DataMainTable.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (!(row.Tag is Image master)) continue;
                    Image old = row.Cells[0].Value as Image;
                    Image fresh = ResizeDGIcon(master, 30, dpi);
                    row.Cells[0].Value = fresh;
                    if (old != null && !ReferenceEquals(old, master)) old.Dispose();
                }
                INSTAPPS_DataMainTable.Invalidate();
            }
            catch (Exception ex)
            {
                if (debug_status) TSErrorLog.LogException(ex, "RefreshInstappsIcons()");
            }
        }
        private List<InstalledAppConfig> GetInstalledApplications()
        {
            var apps = new List<InstalledAppConfig>();
            var registryViews = new[] { RegistryView.Registry64, RegistryView.Registry32 };
            var registryRoots = new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
            foreach (var view in registryViews)
            {
                foreach (var root in registryRoots)
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(root, view))
                    using (var uninstallKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                    {
                        if (uninstallKey == null) continue;
                        foreach (var subkeyName in uninstallKey.GetSubKeyNames())
                        {
                            using (var subkey = uninstallKey.OpenSubKey(subkeyName))
                            {
                                if (subkey == null) continue;
                                var name = (subkey.GetValue("DisplayName") as string)?.Trim();
                                if (string.IsNullOrWhiteSpace(name)) continue;
                                var displayIcon = (subkey.GetValue("DisplayIcon") as string)?.Trim()?.Trim('"');
                                var publisher = (subkey.GetValue("Publisher") as string)?.Trim();
                                var installLocationRaw = (subkey.GetValue("InstallLocation") as string)?.Trim();
                                var installLocation = installLocationRaw?.Trim('"');
                                var version = (subkey.GetValue("DisplayVersion") as string)?.Trim();
                                var installDateRaw = (subkey.GetValue("InstallDate") as string)?.Trim();
                                var uninstallString = (subkey.GetValue("UninstallString") as string)?.Trim();
                                if (string.IsNullOrWhiteSpace(installLocation))
                                {
                                    if (!string.IsNullOrWhiteSpace(displayIcon))
                                    {
                                        ParseDisplayIcon(displayIcon, out string iconPath, out _);
                                        installLocation = ExtractDirFromPathString(iconPath);
                                    }
                                    if (string.IsNullOrWhiteSpace(installLocation) && !string.IsNullOrWhiteSpace(uninstallString))
                                    {
                                        if (uninstallString.IndexOf("MsiExec", StringComparison.OrdinalIgnoreCase) == -1)
                                        {
                                            installLocation = ExtractDirFromPathString(uninstallString);
                                        }
                                    }
                                }
                                string targetExe = null;
                                if (!string.IsNullOrWhiteSpace(displayIcon))
                                {
                                    ParseDisplayIcon(displayIcon, out string iconPath, out _);
                                    if (!string.IsNullOrWhiteSpace(iconPath) && iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(iconPath))
                                    {
                                        targetExe = iconPath;
                                    }
                                }
                                if (string.IsNullOrEmpty(targetExe) && !string.IsNullOrWhiteSpace(uninstallString))
                                {
                                    var cleanUninstallPath = ExtractFilePathFromCmd(uninstallString);
                                    if (!string.IsNullOrEmpty(cleanUninstallPath) && !cleanUninstallPath.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase) && !cleanUninstallPath.EndsWith("unins000.exe", StringComparison.OrdinalIgnoreCase) && File.Exists(cleanUninstallPath))
                                    {
                                        targetExe = cleanUninstallPath;
                                    }
                                }
                                List<string> cachedExeFiles = null;
                                if (string.IsNullOrEmpty(targetExe) && !string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
                                {
                                    try
                                    {
                                        cachedExeFiles = Directory.EnumerateFiles(installLocation, "*.exe").Where(p => !p.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase) && !p.EndsWith("unins000.exe", StringComparison.OrdinalIgnoreCase)).ToList();
                                        targetExe = cachedExeFiles.FirstOrDefault(p => !string.IsNullOrEmpty(name) && Path.GetFileNameWithoutExtension(p).IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) ?? cachedExeFiles.FirstOrDefault();
                                    }
                                    catch { cachedExeFiles = null; }
                                }
                                if (string.IsNullOrWhiteSpace(publisher) && !string.IsNullOrEmpty(targetExe) && File.Exists(targetExe))
                                {
                                    try
                                    {
                                        var versionInfo = FileVersionInfo.GetVersionInfo(targetExe);
                                        if (!string.IsNullOrWhiteSpace(versionInfo.CompanyName))
                                        {
                                            publisher = versionInfo.CompanyName.Trim();
                                        }
                                    }
                                    catch { }
                                }
                                DateTime? installDate = null;
                                if (!string.IsNullOrWhiteSpace(installDateRaw))
                                {
                                    if (installDateRaw.Length == 8 && DateTime.TryParseExact(installDateRaw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                                        installDate = parsedDate;
                                    else if (installDateRaw.Length >= 10 && DateTime.TryParse(installDateRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate2))
                                        installDate = parsedDate2;
                                }
                                var sizeValue = subkey.GetValue("EstimatedSize");
                                long? sizeKb = null;
                                if (sizeValue != null)
                                {
                                    try { sizeKb = Convert.ToInt64(sizeValue); } catch { }
                                }
                                if (!sizeKb.HasValue || sizeKb.Value == 0)
                                {
                                    long? folderSizeBytes = IsSystemDirectory(installLocation) ? (long?)null : CalculateFolderSizeInBytes(installLocation);
                                    if (folderSizeBytes.HasValue)
                                    {
                                        sizeKb = folderSizeBytes.Value / 1024;
                                    }
                                }
                                apps.Add(new InstalledAppConfig
                                {
                                    DisplayIcon = displayIcon,
                                    AppIcon = null,
                                    Name = name,
                                    Publisher = publisher,
                                    InstallDate = installDate,
                                    Size = sizeKb,
                                    Version = version,
                                    InstallLocation = installLocation,
                                    TargetExePath = targetExe,
                                    ExeFileCache = cachedExeFiles
                                });
                            }
                        }
                    }
                }
            }
            return apps.GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.OrderByDescending(a => string.IsNullOrEmpty(a.Version) ? 0 : 1).ThenByDescending(a => a.Size ?? -1L).ThenBy(a => a.InstallLocation ?? string.Empty, StringComparer.OrdinalIgnoreCase).First()).OrderBy(a => TSNaturalSortKey(a.Name)).ToList();
        }
        private static string ExtractFilePathFromCmd(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return null;
            string cleaned = rawPath.Trim();
            if (cleaned.StartsWith("\"", StringComparison.Ordinal))
            {
                int nextQuote = cleaned.IndexOf("\"", 1);
                if (nextQuote > 1) return cleaned.Substring(1, nextQuote - 1);
            }
            else
            {
                int exeIdx = cleaned.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (exeIdx > 0) return cleaned.Substring(0, exeIdx + 4);
            }
            return cleaned;
        }
        private static string ExtractDirFromPathString(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return null;
            string cleaned = rawPath.Trim();
            if (cleaned.StartsWith("\"", StringComparison.Ordinal))
            {
                int nextQuote = cleaned.IndexOf("\"", 1);
                if (nextQuote > 1)
                {
                    cleaned = cleaned.Substring(1, nextQuote - 1);
                }
            }
            else
            {
                int exeIdx = cleaned.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (exeIdx > 0)
                {
                    cleaned = cleaned.Substring(0, exeIdx + 4);
                }
            }
            try
            {
                cleaned = Environment.ExpandEnvironmentVariables(cleaned).Trim().Trim('"');
                if (File.Exists(cleaned))
                {
                    return Path.GetDirectoryName(cleaned);
                }
                if (Directory.Exists(cleaned))
                {
                    return cleaned;
                }
                string dir = Path.GetDirectoryName(cleaned);
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                {
                    return dir;
                }
            }
            catch { }
            return null;
        }
        private static bool IsSystemDirectory(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return false;
                string full = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar);
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (string.IsNullOrWhiteSpace(winDir)) return false;
                string fp = Path.GetFullPath(winDir).TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar);
                if (string.IsNullOrEmpty(fp)) return false;
                if (full.Equals(fp, StringComparison.OrdinalIgnoreCase)) return true;
                if (full.StartsWith(fp + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
        private long? CalculateFolderSizeInBytes(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                    return null;
                long totalBytes = 0;
                var sw = Stopwatch.StartNew();
                CalculateFolderSizeRecursive(folderPath, ref totalBytes, sw, 2000);
                if (sw.ElapsedMilliseconds > 2000) return null;
                return totalBytes;
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "CalculateFolderSizeInBytes() - " + folderPath); }
                return null;
            }
        }
        private void CalculateFolderSizeRecursive(string folderPath, ref long totalBytes, Stopwatch sw, int limitMs, HashSet<string> visited = null)
        {
            try
            {
                if (visited == null) visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string fullPath;
                try { fullPath = Path.GetFullPath(folderPath); } catch { return; }
                if (!visited.Add(fullPath)) return;
                foreach (var file in Directory.EnumerateFiles(folderPath))
                {
                    if (sw.ElapsedMilliseconds > limitMs) return;
                    try
                    {
                        totalBytes += new FileInfo(file).Length;
                    }
                    catch { }
                }
                foreach (var subDir in Directory.EnumerateDirectories(folderPath))
                {
                    if (sw.ElapsedMilliseconds > limitMs) return;
                    try
                    {
                        CalculateFolderSizeRecursive(subDir, ref totalBytes, sw, limitMs, visited);
                    }
                    catch { }
                }
            }
            catch { }
        }
        private string GetInstallDateFromLocation(string installLocation, string preCalculatedExe, IReadOnlyList<string> precomputedExeFiles = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(installLocation)) return null;
                if (!string.IsNullOrEmpty(preCalculatedExe) && File.Exists(preCalculatedExe))
                {
                    var creationTime = File.GetCreationTime(preCalculatedExe);
                    if (creationTime.Year > 2000 && creationTime.Year < 2099)
                        return creationTime.ToString("dd.MM.yyyy");
                    var lastWriteTime = File.GetLastWriteTime(preCalculatedExe);
                    if (lastWriteTime.Year > 2000 && lastWriteTime.Year < 2099)
                        return lastWriteTime.ToString("dd.MM.yyyy");
                }
                if (!Directory.Exists(installLocation)) return null;
                var exePath = precomputedExeFiles != null
                    ? precomputedExeFiles.FirstOrDefault(p => !p.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase) && !p.EndsWith("unins000.exe", StringComparison.OrdinalIgnoreCase))
                    : Directory.EnumerateFiles(installLocation, "*.exe").FirstOrDefault(p => !p.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase) && !p.EndsWith("unins000.exe", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    var creationTime = File.GetCreationTime(exePath);
                    if (creationTime.Year > 2000 && creationTime.Year < 2099)
                        return creationTime.ToString("dd.MM.yyyy");
                }
                var dirCreation = Directory.GetCreationTime(installLocation);
                if (dirCreation.Year > 2000 && dirCreation.Year < 2099)
                    return dirCreation.ToString("dd.MM.yyyy");
                return null;
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "GetInstallDateFromLocation()"); }
                return null;
            }
        }
        private void INSTAPPS_DataMainTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);
                if (e.Value is Image img)
                {
                    float scale = TSDpiHelper.Scale(TSDpiHelper.EffectiveDpi(this));
                    int iconSize = Math.Max(1, (int)Math.Round(30 * scale));
                    int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;
                    var rect = new Rectangle(x, y, iconSize, iconSize);
                    var g = e.Graphics;
                    var oldInterp = g.InterpolationMode;
                    var oldComp = g.CompositingQuality;
                    var oldPixel = g.PixelOffsetMode;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    try { g.DrawImage(img, rect); }
                    finally
                    {
                        g.InterpolationMode = oldInterp;
                        g.CompositingQuality = oldComp;
                        g.PixelOffsetMode = oldPixel;
                    }
                }
                e.Handled = true;
            }
        }
        #endregion
        // BUTTONS ROTATE
        // ======================================================================================================
        private void Active_page(object btn_target)
        {
            Button active_btn = null;
            Disabled_page();
            if (btn_target != null)
            {
                if (active_btn != (Button)btn_target)
                {
                    active_btn = (Button)btn_target;
                    active_btn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                    active_btn.Cursor = Cursors.Default;
                }
            }
        }
        private void Disabled_page()
        {
            foreach (Control disabled_btn in LeftMenuPanel.Controls)
            {
                disabled_btn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
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
        private void MainContent_Selecting(object sender, TabControlCancelEventArgs e)
        {
            try
            {
                if (!e.TabPage.Enabled)
                    e.Cancel = true;
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "MainContent_Selecting()"); }
            }
        }
        private void MainContent_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
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
                if (tabButtons.TryGetValue(MainContent.SelectedIndex, out Button btn))
                {
                    Active_page(btn);
                    menu_btns = MainContent.SelectedIndex + 1;
                    UpdateHeaderByIndex(MainContent.SelectedIndex);
                }
                // Refresh the selected page's DPI/layout (it may have missed the DPI change while hidden)
                try
                {
                    int pageDpi = this.DeviceDpi;
                    ScaleMainTables(pageDpi > 0 ? pageDpi : 0);
                    var selectedTab = MainContent.SelectedTab;
                    if (selectedTab != null && !selectedTab.IsDisposed)
                    {
                        selectedTab.PerformLayout();
                        selectedTab.Invalidate(true);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "MainContent_SelectedIndexChanged()"); }
            }
        }
        // DYNAMIC LEFT MENU PRELOADER
        private void Left_menu_preloader(int target_menu, object sender)
        {
            try
            {
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
                if (menu_btns != target_menu && menuTargets.TryGetValue(target_menu, out var target))
                {
                    MainContent.SelectedTab = target.tab;
                    if (!btn_colors_active.Contains(target.rotateBtn.BackColor))
                        Active_page(sender);
                    HeaderText.Text = software_lang.TSReadLangs("Header", target.headerKey);
                }
                menu_btns = target_menu;
                Header_image_reloader(menu_btns);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Left_menu_preloader()"); }
            }
        }
        private void UpdateHeaderByIndex(int index)
        {
            var keys = new[]{
                "header_os", "header_mb", "header_cpu", "header_ram", "header_gpu",
                "header_storage", "header_network", "header_usb", "header_sound",
                "header_battery", "header_installed_drivers", "header_installed_services",
                "header_installed_apps", "header_export"
            };
            if (index >= 0 && index < keys.Length)
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                HeaderText.Text = software_lang.TSReadLangs("Header", keys[index]);
                Header_image_reloader(index + 1);
            }
        }
        // INTERFACE NAVIGATION: Prevents focus traps and enables cyclic page switching with arrow keys
        // ======================================================================================================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            {
                int currentIndex = MainContent.SelectedIndex;
                int totalTabs = MainContent.TabCount;
                if (keyData == Keys.Down || keyData == Keys.Right)
                {
                    MainContent.SelectedIndex = (currentIndex + 1) % totalTabs;
                }
                else if (keyData == Keys.Up || keyData == Keys.Left)
                {
                    MainContent.SelectedIndex = (currentIndex - 1 + totalTabs) % totalTabs;
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        // LANGUAGES SETTINGS
        // ======================================================================================================
        private ToolStripMenuItem selected_lang = null;
        private void Select_lang_active(object target_lang)
        {
            if (target_lang == null)
                return;
            ToolStripMenuItem clicked_lang = (ToolStripMenuItem)target_lang;
            if (selected_lang == clicked_lang)
                return;
            Select_lang_deactive();
            selected_lang = clicked_lang;
            selected_lang.Checked = true;
        }
        private void Select_lang_deactive()
        {
            foreach (ToolStripMenuItem disabled_lang in languageToolStripMenuItem.DropDownItems)
            {
                disabled_lang.Checked = false;
            }
        }
        // LANG SWAP
        // ======================================================================================================
        private void LanguageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string langCode)
                {
                    if (lang != langCode && AllLanguageFiles.ContainsKey(langCode))
                    {
                        Lang_preload(AllLanguageFiles[langCode], langCode);
                        Select_lang_active(sender);
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "LanguageToolStripMenuItem_Click()"); }
            }
        }
        private void Lang_preload(string lang_type, string lang_code)
        {
            Lang_engine(lang_type, lang_code);
            try
            {
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "LanguageStatus", lang_code);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Lang_preload()"); }
            }
            // LANG CHANGE NOTIFICATION
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            DialogResult lang_change_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("LangChange", "lang_change_notification"), "\n\n", "\n\n"));
            if (lang_change_message == DialogResult.Yes) { Application.Restart(); }
        }
        private void Lang_engine(string lang_type, string lang_code)
        {
            try
            {
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
                if (headers.TryGetValue(menu_btns, out string headerKey))
                {
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
                chineseTaiwanToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_zh_tw");
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
                slovenianToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_sl");
                spanishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_es");
                turkishToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderLangs", "lang_tr");
                // STARTUP VIEW
                startupToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_start");
                windowedToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_windowed");
                fullScreenToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderViewMode", "header_view_mode_full_screen");
                // STEALTH MODE
                stealthModeToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_stealth_mode");
                stealthModeOnToolStripMenuItem.Text = software_lang.TSReadLangs("StealthMode", "sm_on");
                stealthModeOffToolStripMenuItem.Text = software_lang.TSReadLangs("StealthMode", "sm_off");
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
                hsAuditorTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_hardware_security_auditor_tool");
                bluetoothFinderToolToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderTools", "ht_bluetooth_finder_tool");
                dnsTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_dns_test_tool");
                networkFixTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_network_fix_tool");
                showWiFiPasswordTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_show_wifi_password_tool");
                portScannerTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_port_scanner_tool");
                monitorTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test");
                monitorDeadPixelTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dead_pixel");
                monitorDynamicRangeTestTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dynamic_range");
                monitorStuckPixelFixerTool.Text = software_lang.TSReadLangs("HeaderTools", "ht_stuck_pixel_fixer_tool");
                // DONATE
                donateToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_donate");
                // ABOUT
                aboutToolStripMenuItem.Text = software_lang.TSReadLangs("HeaderMenu", "header_menu_about");
                // TOP HIDE MENU
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
                OS_WinMachineID.Text = software_lang.TSReadLangs("OperatingSystem", "os_win_machine_id");
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
                if (minidump_files_list.Count > 0)
                {
                    MainToolTip.SetToolTip(OS_MinidumpOpen, Directory.Exists(mdp_1) ? string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_folder"), mdp_1) : string.Format(software_lang.TSReadLangs("Os_Content", "os_c_bsod_open_file"), mdp_2));
                    MainToolTip.SetToolTip(OS_BSoDZIP, software_lang.TSReadLangs("Os_Content", "os_c_bsod_zip_folder"));
                }
                if (!string.IsNullOrEmpty(wp_rotate))
                {
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
                CPU_CurrentSpeed.Text = software_lang.TSReadLangs("Processor", "pr_cpu_current_speed");
                CPU_CoreCount.Text = software_lang.TSReadLangs("Processor", "pr_cpu_core_count");
                CPU_ActiveCoreCount.Text = software_lang.TSReadLangs("Processor", "pr_cpu_active_core_count");
                CPU_LogicalCore.Text = software_lang.TSReadLangs("Processor", "pr_cpu_logical_core_count");
                CPU_L1.Text = software_lang.TSReadLangs("Processor", "pr_l1_cache_size");
                CPU_L2.Text = software_lang.TSReadLangs("Processor", "pr_l2_cache_size");
                CPU_L3.Text = software_lang.TSReadLangs("Processor", "pr_l3_cache_size");
                CPU_SerialName.Text = software_lang.TSReadLangs("Processor", "pr_cpu_processor_id");
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
                CPU_Features.Text = software_lang.TSReadLangs("Processor", "pr_cpu_features");
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
                if (!string.IsNullOrEmpty(NET_LT_GatewayIP_V.Text.Trim()))
                {
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
                NET_SubnetMask.Text = software_lang.TSReadLangs("Network", "nk_subnet_mask");
                NET_DNS_v4_1.Text = software_lang.TSReadLangs("Network", "nk_v4_dns1");
                NET_DNS_v4_2.Text = software_lang.TSReadLangs("Network", "nk_v4_dns2");
                NET_DNS_v6_1.Text = software_lang.TSReadLangs("Network", "nk_v6_dns1");
                NET_DNS_v6_2.Text = software_lang.TSReadLangs("Network", "nk_v6_dns2");
                NET_P_IP_Adress.Text = software_lang.TSReadLangs("Network", "nk_p_ip");
                NET_P_ISP.Text = software_lang.TSReadLangs("Network", "nk_p_isp");
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
                if (EXPORT_Selector_List.Items.Count > 0)
                {
                    EXPORT_Selector_List.Items[0] = software_lang.TSReadLangs("Export", "e_mode_txt");
                    EXPORT_Selector_List.Items[1] = software_lang.TSReadLangs("Export", "e_mode_html");
                    EXPORT_Selector_List.Items[2] = software_lang.TSReadLangs("Export", "e_mode_markdown");
                    if (EXPORT_Selector_List.Items.Count > 3)
                        EXPORT_Selector_List.Items[3] = software_lang.TSReadLangs("Export", "e_mode_json");
                }
                if (EXPORT_ModeSelector_List.Items.Count > 0)
                {
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
                // OTHER PAGE DYNAMIC LOADER
                Glow_OtherPage_DynamicLoader();
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Lang_engine()"); }
            }
        }
        // THEME SETTINGS
        // ======================================================================================================
        private ToolStripMenuItem selected_theme = null;
        private void Select_theme_active(object target_theme)
        {
            if (target_theme == null)
                return;
            ToolStripMenuItem clicked_theme = (ToolStripMenuItem)target_theme;
            if (selected_theme == clicked_theme)
                return;
            Select_theme_deactive();
            selected_theme = clicked_theme;
            selected_theme.Checked = true;
        }
        private void Select_theme_deactive()
        {
            foreach (ToolStripMenuItem theme in themeToolStripMenuItem.DropDownItems)
            {
                theme.Checked = false;
            }
        }
        // THEME SWAP
        // ======================================================================================================
        private void SystemThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Already in system mode: re-resolve cheaply (dedup inside) instead of full re-theme.
                if (themeSystem == 2) { TSUseSystemTheme(); return; }
                themeSystem = 2; Theme_engine(TSThemeModeHelper.GetSystemTheme(2)); SaveTheme(2); Select_theme_active(sender);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SystemThemeToolStripMenuItem_Click()"); }
            }
        }
        private void LightThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (themeSystem != 2 && theme == 1) return;
                themeSystem = 0; Theme_engine(1); SaveTheme(1); Select_theme_active(sender);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "LightThemeToolStripMenuItem_Click()"); }
            }
        }
        private void DarkThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (themeSystem != 2 && theme == 0) return;
                themeSystem = 0; Theme_engine(0); SaveTheme(0); Select_theme_active(sender);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DarkThemeToolStripMenuItem_Click()"); }
            }
        }
        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            TSUseSystemTheme();
        }
        private const int WM_THEMECHANGED = 0x031A;
        private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            try
            {
                if (themeSystem != 2 || IsDisposed || Disposing) return;
                if (m.Msg == WM_DWMCOLORIZATIONCOLORCHANGED || m.Msg == WM_THEMECHANGED)
                {
                    if (IsHandleCreated && !IsDisposed && !Disposing)
                    {
                        try { BeginInvoke(new Action(() => TSUseSystemTheme())); } catch { }
                    }
                }
            }
            catch { }
        }
        private void TSUseSystemTheme()
        {
            if (themeSystem != 2) return;
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => TSUseSystemTheme())); } catch { }
                return;
            }
            int sysTheme;
            int accentArgb;
            try { sysTheme = TSThemeModeHelper.GetSystemTheme(2); }
            catch { return; }
            try { accentArgb = TS_ThemeEngine.TSAccentHelper.GetSystemAccentBase().ToArgb(); }
            catch { accentArgb = int.MinValue; }
            if (sysTheme == theme && sysTheme == _lastSysTheme && accentArgb == _lastSysAccentArgb) return;
            if (sysTheme == theme && sysTheme == _lastSysTheme)
            {
                // Coalesce rapid accent bursts: max one full re-theme per ~350ms.
                // Re-queued calls re-read the live accent, so the newest color is never lost.
                long nowTicks = DateTime.UtcNow.Ticks;
                if (nowTicks - _lastSysApplyTicks < 350 * TimeSpan.TicksPerMillisecond)
                {
                    try { BeginInvoke(new Action(() => TSUseSystemTheme())); } catch { }
                    return;
                }
            }
            Theme_engine(sysTheme);
            // Palette can lag the picked color: re-resolve shortly after fallback.
            _sysAccentRetryCount = 0;
            if (themeSystem == 2 && TS_ThemeEngine.LastResolveWasFallback) ScheduleAccentRetry();
        }
        private void ScheduleAccentRetry()
        {
            if (IsDisposed || Disposing) return;
            if (_sysAccentRetryCount >= 3) return;
            _sysAccentRetryCount++;
            try
            {
                if (_sysAccentRetryTimer == null)
                {
                    _sysAccentRetryTimer = new System.Windows.Forms.Timer(this.components);
                    _sysAccentRetryTimer.Tick += SysAccentRetry_Tick;
                }
                _sysAccentRetryTimer.Interval = 800;
                _sysAccentRetryTimer.Stop();
                _sysAccentRetryTimer.Start();
            }
            catch { }
        }
        private void SysAccentRetry_Tick(object sender, EventArgs e)
        {
            try
            {
                _sysAccentRetryTimer?.Stop();
                if (themeSystem != 2 || IsDisposed || Disposing || !IsHandleCreated) { _sysAccentRetryCount = 0; return; }
                Theme_engine(theme);
                if (TS_ThemeEngine.LastResolveWasFallback) ScheduleAccentRetry();
                else _sysAccentRetryCount = 0;
            }
            catch { _sysAccentRetryCount = 0; }
        }
        private void SaveTheme(int ts)
        {
            // SAVE CURRENT THEME
            try
            {
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "ThemeStatus", Convert.ToString(ts));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "SaveTheme()"); }
            }
        }
        // THEME ENGINE
        // ======================================================================================================
        private void Theme_engine(int ts)
        {
            try
            {
                try { TSDpiHelper.ScaleToolStripRecursive(HeaderMenu, this.DeviceDpi); } catch { }
                theme = ts;
                // SYSTEM ACCENT: system mode only; manual themes keep fixed accents.
                if (themeSystem == 2) { TS_ThemeEngine.SetSystemAccentEnabled(true); }
                else { TS_ThemeEngine.ClearSystemAccent(); }
                try
                {
                    if (themeSystem == 2)
                    {
                        _lastSysTheme = ts;
                        _lastSysApplyTicks = DateTime.UtcNow.Ticks;
                        _lastSysAccentArgb = TS_ThemeEngine.LastSystemAccentArgb;
                    }
                    else
                    {
                        _lastSysTheme = -1;
                        _lastSysAccentArgb = int.MinValue;
                        _lastSysApplyTicks = 0;
                    }
                }
                catch { }
                //
                TSThemeModeHelper.SetThemeMode(ts == 0);
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                if (theme == 1)
                {
                    // LEFT MENU LOGO CHANGE
                    if (Program.Windows_mode == 1)
                    {
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w11_light, 18, ContentAlignment.MiddleLeft);
                    }
                    else
                    {
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
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(stealthModeToolStripMenuItem, Properties.Resources.tm_stealth_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(debugModeToolStripMenuItem, Properties.Resources.tm_debug_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(checkForUpdatesToolStripMenuItem, Properties.Resources.tm_update_light, 0, ContentAlignment.MiddleCenter);
                    // TOOLS
                    TSImageRenderer(toolsToolStripMenuItem, Properties.Resources.tm_tools_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(sFCandDISMAutoTool, Properties.Resources.cx_sfc_and_dism_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchCPUTool, Properties.Resources.cx_bench_cpu_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchRAMTool, Properties.Resources.cx_bench_ram_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchDiskTool, Properties.Resources.cx_bench_disk_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(cacheCleaningTool, Properties.Resources.cx_cache_clean_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(systemIdAnalysisTool, Properties.Resources.cx_system_id_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(hsAuditorTool, Properties.Resources.tm_hsa_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(bluetoothFinderToolToolStripMenuItem, Properties.Resources.cx_bt_finder_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(dnsTestTool, Properties.Resources.cx_dns_test_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(networkFixTool, Properties.Resources.cx_network_fix_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(showWiFiPasswordTool, Properties.Resources.cx_swpt_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(portScannerTool, Properties.Resources.cx_port_scanner_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorTestTool, Properties.Resources.cx_test_monitor_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorDeadPixelTestTool, Properties.Resources.cx_test_dead_pixel_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorDynamicRangeTestTool, Properties.Resources.cx_test_dynamic_range_light, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorStuckPixelFixerTool, Properties.Resources.cx_stuck_pixel_fixer_light, 0, ContentAlignment.MiddleCenter);
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
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_light, 0, ContentAlignment.MiddleCenter);
                    // ABOUT
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_light, 0, ContentAlignment.MiddleCenter);
                    // SCROLL TOP
                    TSImageRenderer(OS_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(MB_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(CPU_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(RAM_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(GPU_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(DISK_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(NET_ScrollTop, Properties.Resources.ct_scroll_top_light, 22, ContentAlignment.MiddleCenter);
                }
                else if (theme == 0)
                {
                    // LEFT MENU LOGO CHANGE
                    if (Program.Windows_mode == 1)
                    {
                        TSImageRenderer(OS_RotateBtn, Properties.Resources.lm_os_w11_dark, 18, ContentAlignment.MiddleLeft);
                    }
                    else
                    {
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
                    TSImageRenderer(settingsToolStripMenuItem, Properties.Resources.tm_settings_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(themeToolStripMenuItem, Properties.Resources.tm_theme_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(languageToolStripMenuItem, Properties.Resources.tm_language_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(startupToolStripMenuItem, Properties.Resources.tm_startup_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(stealthModeToolStripMenuItem, Properties.Resources.tm_stealth_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(debugModeToolStripMenuItem, Properties.Resources.tm_debug_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(checkForUpdatesToolStripMenuItem, Properties.Resources.tm_update_dark, 0, ContentAlignment.MiddleCenter);
                    // TOOLS
                    TSImageRenderer(toolsToolStripMenuItem, Properties.Resources.tm_tools_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(sFCandDISMAutoTool, Properties.Resources.cx_sfc_and_dism_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchCPUTool, Properties.Resources.cx_bench_cpu_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchRAMTool, Properties.Resources.cx_bench_ram_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(benchDiskTool, Properties.Resources.cx_bench_disk_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(cacheCleaningTool, Properties.Resources.cx_cache_clean_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(systemIdAnalysisTool, Properties.Resources.cx_system_id_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(hsAuditorTool, Properties.Resources.tm_hsa_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(bluetoothFinderToolToolStripMenuItem, Properties.Resources.cx_bt_finder_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(dnsTestTool, Properties.Resources.cx_dns_test_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(networkFixTool, Properties.Resources.cx_network_fix_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(showWiFiPasswordTool, Properties.Resources.cx_swpt_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(portScannerTool, Properties.Resources.cx_port_scanner_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorTestTool, Properties.Resources.cx_test_monitor_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorDeadPixelTestTool, Properties.Resources.cx_test_dead_pixel_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorDynamicRangeTestTool, Properties.Resources.cx_test_dynamic_range_dark, 0, ContentAlignment.MiddleCenter);
                    TSImageRenderer(monitorStuckPixelFixerTool, Properties.Resources.cx_stuck_pixel_fixer_dark, 0, ContentAlignment.MiddleCenter);
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
                    TSImageRenderer(donateToolStripMenuItem, Properties.Resources.tm_donate_dark, 0, ContentAlignment.MiddleCenter);
                    // ABOUT
                    TSImageRenderer(aboutToolStripMenuItem, Properties.Resources.tm_about_dark, 0, ContentAlignment.MiddleCenter);
                    // SCROLL TOP
                    TSImageRenderer(OS_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(MB_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(CPU_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(RAM_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(GPU_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(DISK_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                    TSImageRenderer(NET_ScrollTop, Properties.Resources.ct_scroll_top_dark, 22, ContentAlignment.MiddleCenter);
                }
                // OTHER PAGE DYNAMIC LOADER
                Glow_OtherPage_DynamicLoader();
                // HEADER
                Header_image_reloader(menu_btns);
                header_colors[0] = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                header_colors[1] = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                header_colors[2] = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                HeaderMenu.Renderer = new HeaderMenuColors();
                // ACTIVE BTN 
                btn_colors_active[0] = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // TOOLTIP
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                // HEADER MENU
                var bg = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                var fg = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                Header_InPanel.BackColor = bg;
                HeaderText.ForeColor = fg;
                HeaderMenu.ForeColor = fg;
                HeaderMenu.BackColor = bg;
                SetMenuStripColors(HeaderMenu, bg, fg);
                // LEFT MENU
                LeftMenuPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OS_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                MB_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                CPU_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                RAM_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                DISK_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                NET_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SOUND_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                BATTERY_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OSD_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SERVICES_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                INSTALLED_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                PRINT_RotateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                // LEFT MENU BORDER
                OS_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                MB_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                CPU_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                RAM_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                DISK_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                NET_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SOUND_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                BATTERY_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OSD_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SERVICES_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                INSTALLED_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                PRINT_RotateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                // LEFT MENU MOUSE HOVER
                OS_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                MB_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                CPU_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SOUND_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                BATTERY_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICES_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTALLED_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                PRINT_RotateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // LEFT MENU MOUSE DOWN
                OS_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                MB_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                CPU_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SOUND_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                BATTERY_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICES_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTALLED_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                PRINT_RotateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // LEFT MENU BUTTON TEXT COLOR
                OS_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICES_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTALLED_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                PRINT_RotateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                // CONTENT BG
                BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OS.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                MB.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                CPU.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NETWORK.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SOUND.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                BATTERY.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DRIVERS.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICES.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // OS
                os_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                os_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                os_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                os_panel_4.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                os_panel_5.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                os_panel_6.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                OS_SystemUser.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemUser_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ComputerName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ComputerName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SavedUser.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SavedUser_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Name.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Name_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SystemVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SystemArchitectural.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemArchitectural_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinMachineID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinMachineID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ExperienceVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ExperienceVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Country.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Country_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_TimeZone.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_TimeZone_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SystemTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SystemWorkTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemWorkTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_LastBootTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_LastBootTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_SystemLastShutDown.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_SystemLastShutDown_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Install.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Install_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_PrimaryOS.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_PrimaryOS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_PortableOS.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_PortableOS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_FastBoot.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_FastBoot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_AVProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_AVProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_FirewallProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_FirewallProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_AntiSpywareProgram.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_AntiSpywareProgram_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinDefCoreIsolation.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinDefCoreIsolation_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePowerGUID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePowerGUID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePowerScreenTimeOutP.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePowerScreenTimeOutP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePowerScreenTimeOutB.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePowerScreenTimeOutB_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePowerSleepTimeP.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePowerSleepTimeP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_ActivePowerSleepTimeB.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_ActivePowerSleepTimeB_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_MSOfficeVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_MSOfficeVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinLicenseTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinLicenseTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinKey.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinKey_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinActiveChannel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinActiveChannel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinLicenseURL.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinLicenseURL_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_WinLicenseVerifiURL.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_WinLicenseVerifiURL_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_NETFrameworkVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_NETFrameworkVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Minidump.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Minidump_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_BSODDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_BSODDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_Wallpaper.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OS_Wallpaper_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                OS_RecoveryModeBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OS_RecoveryModeBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OS_RecoveryModeBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                // MB
                mb_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                mb_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                mb_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                MB_BIOSUpdateBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                MB_BIOSUpdateBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSUpdateBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                MB_BIOSOpenBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                MB_BIOSOpenBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BIOSOpenBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                MB_MotherBoardName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_MotherBoardName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_MotherBoardMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_MotherBoardMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SystemModelMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SystemModelMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SystemModelFamily.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SystemModelFamily_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SystemFamily.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SystemFamily_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SystemModel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SystemModel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_DeviceSerialNumber.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_DeviceSerialNumber_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_MotherBoardSerial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_MotherBoardSerial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SystemSKU.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SystemSKU_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_Chipset.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_Chipset_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BiosManufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_BiosManufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BiosDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_BiosDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BiosVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_BiosVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SmBiosVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SmBiosVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_BiosMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_BiosMode_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_LastBIOSTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_LastBIOSTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_SecureBoot.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_SecureBoot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMPPIVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMPPIVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMPPIAltVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMPPIAltVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMManID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMManID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMManVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMManVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMManFullVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMManFullVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                MB_TPMManPublisher.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                MB_TPMManPublisher_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // CPU
                cpu_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                cpu_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                CPU_Name.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                CPU_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                CPU_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                CPU_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                CPU_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                CPU_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                CPU_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                CPU_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Architectural.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Architectural_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Speed.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Speed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_CurrentSpeed.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_CurrentSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_CoreCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_CoreCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_ActiveCoreCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_ActiveCoreCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_LogicalCore.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_LogicalCore_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_L1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_L2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_L3.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_L3_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_SerialName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_SerialName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Family.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Family_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Slot.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Slot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_SocketDefinition.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_SocketDefinition_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                CPU_AllCPUHead.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Virtualization.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Virtualization_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Usage.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Usage_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Process.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Process_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Threads.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Threads_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Handles.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Handles_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                CPU_Features.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                CPU_Features_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // RAM
                ram_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                ram_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                RAM_TotalRAM.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_TotalRAM_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_UsageRAMCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_UsageRAMCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_EmptyRamCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_EmptyRamCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_TotalVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_TotalVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_UsageVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_UsageVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_EmptyVirtualRam.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_EmptyVirtualRam_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_SlotStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_SlotStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                RAM_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                RAM_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                RAM_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                RAM_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                RAM_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                RAM_Amount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Amount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Type.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Type_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Frequency.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Frequency_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Volt.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Volt_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_FormFactor.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_FormFactor_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_BankLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_BankLabel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_DataWidth.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_DataWidth_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_BellekType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_BellekType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_PartNumber.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                RAM_PartNumber_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                RAM_ProgressBGPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                RAM_ProgressFEPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                RAM_ProgressLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // GPU
                gpu_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                gpu_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                GPU_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                GPU_Manufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Manufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_VRAM.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_VRAM_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_Version.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Version_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_Status.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_Status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_DacType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_DacType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_GraphicDriversName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_GraphicDriversName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_DirectXAll.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_DirectXAll_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_DirectXMax.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_DirectXMax_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_InfFileName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_InfFileName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_INFSectionFile.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_INFSectionFile_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_CurrentColor.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_CurrentColor_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                GPU_MonitorSelector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_MonitorSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                GPU_MonitorSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_MonitorSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                GPU_MonitorSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_MonitorSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                GPU_MonitorSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                GPU_MonitorUserFriendlyName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorUserFriendlyName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorManName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorManName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorProductCodeID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorProductCodeID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorSerialNumberID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorSerialNumberID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorConType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorConType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorManfDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorManfDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorManfDateWeek.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorManfDateWeek_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorHID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorHID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorResLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorResLabel_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorVirtualRes.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorVirtualRes_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorBounds.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorBounds_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorWorking.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorWorking_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_ScreenRefreshRate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_ScreenRefreshRate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_ScreenBit.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_ScreenBit_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                GPU_MonitorPrimary.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                GPU_MonitorPrimary_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // DISK
                disk_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                disk_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                disk_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                DISK_TTLP_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_TTLP_P1_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_TTLP_P2_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_TTLP_P3_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_TTLP_P4_L1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                //
                DISK_TLP_PB_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_TLP_PB_2.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentPurple");
                DISK_TLP_PB_3.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentRed");
                DISK_TLP_PB_4.BackColor = TS_ThemeEngine.ColorMode(theme, "AccentGreen");
                DISK_TTLP_P1_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_TTLP_P2_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentPurple");
                DISK_TTLP_P3_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentRed");
                DISK_TTLP_P4_L2.ForeColor = TS_ThemeEngine.ColorMode(theme, "AccentGreen");
                //
                DISK_TTLP_Panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_TTLP_Panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_TTLP_Panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_TTLP_Panel_4.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                DISK_PBar_BG.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_PBar_FE.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_PBar_Label.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                DISK_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                DISK_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                DISK_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                DISK_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                DISK_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                DISK_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                DISK_Model.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Model_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Man.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Man_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_VolumeID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_VolumeID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_VolumeName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_VolumeName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Firmware.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Firmware_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_VolumeSerial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_VolumeSerial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Size.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Size_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_UsedSpace.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_UsedSpace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_FreeSpace.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_FreeSpace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_FileSystem.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_FileSystem_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_FormattingType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_FormattingType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Type.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Type_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_DriveType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_DriveType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_InterFace.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_InterFace_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_PartitionCount.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_PartitionCount_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_MediaLoaded.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_MediaLoaded_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_MediaStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_MediaStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Health.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Health_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Boot.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Boot_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_Bootable.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_Bootable_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_BitLockerStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_BitLockerStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_BitLockerConversionStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_BitLockerConversionStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_BitLockerEncryptMehod.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_BitLockerEncryptMehod_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                DISK_DriveCompressed.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                DISK_DriveCompressed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // NETWORK
                network_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                network_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                network_panel_3.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                NET_LT_Device.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_Device_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LT_BandWidth.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_BandWidth_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LT_LocalIP.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_LocalIP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LT_GatewayIP.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_GatewayIP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LT_P1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_LT_P2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_LT_DL1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_DL2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LT_UL1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LT_UL2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                NET_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                NET_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                NET_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                NET_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                NET_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                NET_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                NET_MacAdress.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_MacAdress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_NetMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_NetMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_ServiceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_ServiceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_AdapterType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_AdapterType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_Physical.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Physical_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_Guid.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Guid_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_ConnectionType.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_ConnectionType_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_Dhcp_status.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Dhcp_status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_Dhcp_server.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_Dhcp_server_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DHCPFirstIpTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DHCPFirstIpTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DHCPLastIpTime.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DHCPLastIpTime_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_LocalConSpeed.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_LocalConSpeed_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_IPv4Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_IPv4Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_IPv6Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_IPv6Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_SubnetMask.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_SubnetMask_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DNS_v4_1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DNS_v4_1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DNS_v4_2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DNS_v4_2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DNS_v6_1.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DNS_v6_1_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_DNS_v6_2.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_DNS_v6_2_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_P_IP_Adress.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_P_IP_Adress_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                NET_P_ISP.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                NET_P_ISP_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // USB
                usb_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                usb_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                USB_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                USB_ConName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_ConName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_ConMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_ConMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_ConDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_ConDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_ConPNPDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_ConPNPDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_ConDeviceStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_ConDeviceStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                USB_DeviceSelector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_DeviceSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                USB_DeviceSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_DeviceSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                USB_DeviceSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_DeviceSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                USB_DeviceSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DeviceSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                USB_DeviceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DeviceMan.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceMan_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_InfFile.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_InfFile_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_HardwareID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_HardwareID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                USB_DeviceGUID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                USB_DeviceGUID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // SOUND
                sound_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                SOUND_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SOUND_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SOUND_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SOUND_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SOUND_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                SOUND_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                SOUND_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                //
                SOUND_DeviceName.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DeviceName_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_DeviceManufacturer.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DeviceManufacturer_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_DriverVersion.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DriverVersion_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_DriverDate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DriverDate_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_DeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_PNPDeviceID.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_PNPDeviceID_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SOUND_DeviceStatus.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SOUND_DeviceStatus_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // BATTERY
                battery_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                battery_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                BATTERY_Status.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_Status_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_Model.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_Model_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_Serial.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_Serial_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_Chemistry.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_Chemistry_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_DesignCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_DesignCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_FullChargeCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_FullChargeCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_RemainingChargeCapacity.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_RemainingChargeCapacity_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_Voltage.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_Voltage_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ChargePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_ChargePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ChargeCurrent.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_ChargeCurrent_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_DeChargePower.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_DeChargePower_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_DeChargeCurrent.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                BATTERY_DeChargeCurrent_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                BATTERY_ReportBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                BATTERY_ReportBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ReportBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ReportBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ReportBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                BATTERY_PBG_Panel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                BATTERY_PFE_Panel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                BATTERY_ProgressLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                // INSTALLED DRIVERS
                osd_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OSD_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_TYSS.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_TYSS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_SearchDriverLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                OSD_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                OSD_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OSD_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                OSD_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                OSD_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                OSD_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                OSD_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                OSD_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // SERVICES
                service_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SERVICE_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICE_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICE_TYS.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICE_TYS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_SearchLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICE_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICE_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                SERVICE_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICE_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICE_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                SERVICE_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SERVICE_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                SERVICE_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                SERVICE_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                SERVICE_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                SERVICE_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                SERVICE_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // INSTALLED APPS
                instapps_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                INSTAPPS_TextBox.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS_TextBox.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTAPPS_TYUS.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTAPPS_TYUS_V.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_SearchAppsLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTAPPS_TextBoxClearBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_TextBoxClearBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_TextBoxClearBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                INSTAPPS_SortMode.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTAPPS_SortMode.CheckedColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_SortMode.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS_SortMode.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                INSTAPPS_DataMainTable.BackgroundColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                INSTAPPS_DataMainTable.GridColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                INSTAPPS_DataMainTable.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                INSTAPPS_DataMainTable.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                INSTAPPS_DataMainTable.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_DataMainTable.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                INSTAPPS_DataMainTable.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                INSTAPPS_DataMainTable.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                // PRINT
                print_panel_1.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                print_panel_2.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                EXPORT_Selector.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_Selector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_Selector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                EXPORT_Selector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                EXPORT_Selector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_Selector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_Selector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_Selector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_Selector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_Selector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.DisabledArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_Selector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                EXPORT_ProgressBGPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_ProgressFEPanel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_ProgessLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                //
                Export_ModeTitle.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                //
                EXPORT_ModeSelector_List.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_ModeSelector_List.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_ModeSelector_List.HoverBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_ModeSelector_List.ButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                EXPORT_ModeSelector_List.ArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_ModeSelector_List.HoverButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                EXPORT_ModeSelector_List.BorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_ModeSelector_List.FocusedBorderColor = TS_ThemeEngine.ColorMode(theme, "SelectBoxBorderColor");
                EXPORT_ModeSelector_List.HoverForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_ModeSelector_List.SelectedBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_ModeSelector_List.SelectedForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_ModeSelector_List.DisabledBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_ModeSelector_List.DisabledForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_ModeSelector_List.DisabledArrowColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                EXPORT_ModeSelector_List.DisabledButtonColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor");
                //
                EXPORT_CB_FLP.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                foreach (Control c in EXPORT_CB_FLP.Controls)
                {
                    if (c is TSCustomCheckBox cb)
                    {
                        cb.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                        cb.CheckedColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                        cb.CheckMarkColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                        cb.UncheckedBorderColor = TS_ThemeEngine.ColorMode(theme, "CheckBoxUnCheckBorderColor");
                    }
                }
                //
                EXPORT_StartEngineBtn.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_StartEngineBtn.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_StartEngineBtn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                //
                EXPORT_Donate.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_Donate.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_Donate.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_Donate.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                EXPORT_Donate.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                EXPORT_DonateLabel.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                EXPORT_DonateLabel.ForeColor = TS_ThemeEngine.ColorMode(theme, "TSBT_LabelColor1");
                // SCROLL TOP
                var st_fg = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                var st_bg = TS_ThemeEngine.ColorMode(theme, "TSBT_AccentColor");
                var st_hover = TS_ThemeEngine.ColorMode(theme, "AccentColorHover");
                new[] { OS_ScrollTop, CPU_ScrollTop, MB_ScrollTop, RAM_ScrollTop, GPU_ScrollTop, DISK_ScrollTop, NET_ScrollTop }.ToList().ForEach(btn =>
                {
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
                if (buttonMap.TryGetValue(menu_btns, out Button selectedButton))
                {
                    selectedButton.BackColor = TS_ThemeEngine.ColorMode(theme, "TSBT_BGColor2");
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Theme_engine()"); }
            }
        }
        private void SetMenuStripColors(MenuStrip menuStrip, Color bgColor, Color fgColor)
        {
            if (menuStrip == null) return;
            foreach (ToolStripItem item in menuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void SetMenuItemColors(ToolStripMenuItem menuItem, Color bgColor, Color fgColor)
        {
            if (menuItem == null) return;
            menuItem.BackColor = bgColor;
            menuItem.ForeColor = fgColor;
            foreach (ToolStripItem item in menuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem subMenuItem)
                {
                    SetMenuItemColors(subMenuItem, bgColor, fgColor);
                }
            }
        }
        private void SetContextMenuColors(ContextMenuStrip contextMenu, Color bgColor, Color fgColor)
        {
            if (contextMenu == null) return;
            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    SetMenuItemColors(menuItem, bgColor, fgColor);
                }
            }
        }
        private void Header_image_reloader(int hi_value)
        {
            try
            {
                var imageMap = new Dictionary<int, string>{
                    { 1, (theme == 1) ? (Program.Windows_mode == 1 ? "lm_os_w11_light" : "lm_os_w10_light") : (Program.Windows_mode == 1 ? "lm_os_w11_dark" : "lm_os_w10_dark") },
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
                if (imageMap.TryGetValue(hi_value, out string imageName))
                {
                    TSImageRenderer(HeaderImage, (Image)Properties.Resources.ResourceManager.GetObject(imageName), 0, ContentAlignment.MiddleCenter);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Header_image_reloader()"); }
            }
        }
        // MODULES PAGE DYNAMIC UI
        // ======================================================================================================
        private void Glow_OtherPage_DynamicLoader()
        {
            try
            {
                var glow_other_pages = new (string name, Func<object> createTool, Action<object> applySettings)[]{
                    ("glow_sfc_and_dism_tool", () => new GlowSFCandDISMAutoTool(), tool => ((GlowSFCandDISMAutoTool)tool).GTool_SADT_Preloader()),
                    ("glow_bench_cpu_tool", () => new GlowBenchCPUTool(), tool => ((GlowBenchCPUTool)tool).GTool_BenchCPU_Preloader()),
                    ("glow_bench_ram_tool", () => new GlowBenchMemoryTool(), tool => ((GlowBenchMemoryTool)tool).GTool_BenchRAM_Preloader()),
                    ("glow_bench_disk_tool", () => new GlowBenchDiskTool(), tool => ((GlowBenchDiskTool)tool).GTool_BenchDISK_Preloader()),
                    ("glow_cache_cleanup_tool", () => new GlowCacheCleanupTool(), tool => ((GlowCacheCleanupTool)tool).GTool_CacheCleanup_Preloader()),
                    ("glow_system_id_analysis_tool", () => new GlowSystemIDAnalysisTool(), tool => ((GlowSystemIDAnalysisTool)tool).GTool_SystemIA_Preloader()),
                    ("glow_hardware_security_auditor_tool", () => new GlowHSAuditorTool(), tool => ((GlowHSAuditorTool)tool).GTool_HSAuditor_Preloader()),
                    ("glow_bluetooth_finder_tool", () => new GlowBluetoothFinderTool(), tool => ((GlowBluetoothFinderTool)tool).GTool_BTFinder_Preloader()),
                    ("glow_dns_test_tool", () => new GlowDNSTestTool(), tool => ((GlowDNSTestTool)tool).GTool_DNSTest_Preloader()),
                    ("glow_network_fix_tool", () => new GlowNetworkFixTool(), tool => ((GlowNetworkFixTool)tool).GTool_NetworkFix_Preloader()),
                    ("glow_show_wifi_password_tool", () => new GlowShowWiFiPasswordTool(), tool => ((GlowShowWiFiPasswordTool)tool).GTool_SWP_Preloader()),
                    ("glow_port_scanner_tool", () => new GlowPortScannerTool(), tool => ((GlowPortScannerTool)tool).GTool_PortScanner_Preloader()),
                    ("glow_monitor_test_engine_dead_pixel", () => new GlowMonitorTestTool(), tool => ((GlowMonitorTestTool)tool).GTool_MonitorTest_Preloader()),
                    ("glow_monitor_test_engine_dynamic_range", () => new GlowMonitorTestTool(), tool => ((GlowMonitorTestTool)tool).GTool_MonitorTest_Preloader()),
                    ("glow_monitor_stuck_pixel_fixer", () => new GlowStuckPixelFixerTool(), tool => ((GlowStuckPixelFixerTool)tool).GTool_StuckPixel_Preloader()),
                    ("glow_wallpaper_preview_tool", () => new GlowWallpaperPreviewTool(), tool => ((GlowWallpaperPreviewTool)tool).GTool_WP_Preloader()),
                    ("glow_about", () => new GlowAbout(), tool => ((GlowAbout)tool).About_Preloader()),
                };
                foreach (var (toolName, _, applySettings) in glow_other_pages)
                {
                    try
                    {
                        Form openTool = Application.OpenForms[toolName];
                        if (openTool != null)
                        {
                            applySettings(openTool);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debug_status) { TSErrorLog.LogException(ex, "Glow_other_page_dynamic_ui()"); }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Glow_other_page_dynamic_ui()"); }
            }
        }
        // SCROLL TOP MODULE
        // ======================================================================================================
        private CancellationTokenSource _resizeCts;
        private void MainContent_Resize(object sender, EventArgs e)
        {
            try { _resizeCts?.Cancel(); } catch { }
            try { _resizeCts?.Dispose(); } catch { }
            _resizeCts = new CancellationTokenSource();
            var token = _resizeCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, token);
                    if (token.IsCancellationRequested) return;
                    MainContent.ExecuteSafe(() =>
                    {
                        UpdateScrollTopButtonVisibility();
                    });
                }
                catch (TaskCanceledException) { }
            });
        }
        private void UpdateScrollTopButtonVisibility()
        {
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
            })
            {
                btn.SetVisibleSafe(hasScroll);
            }
        }
        private bool _isScrolling = false;
        private async void ScrollTop_Click(object sender, EventArgs e)
        {
            if (_isScrolling) return;
            try
            {
                _isScrolling = true;
                await TS_ScrollTop();
            }
            finally
            {
                _isScrolling = false;
            }
        }
        private async Task TS_ScrollTop()
        {
            var tab = MainContent.SelectedTab;
            if (tab == null) return;
            int start = tab.VerticalScroll.Value;
            if (start <= 0) return;
            int duration = 250;
            var sw = Stopwatch.StartNew();
            while (true)
            {
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
        private ToolStripMenuItem selected_startup_mode = null;
        private void Select_startup_mode_active(object target_startup_mode)
        {
            if (target_startup_mode == null)
                return;
            ToolStripMenuItem clicked_startup_mode = (ToolStripMenuItem)target_startup_mode;
            if (selected_startup_mode == clicked_startup_mode)
                return;
            Select_startup_mode_deactive();
            selected_startup_mode = clicked_startup_mode;
            selected_startup_mode.Checked = true;
        }
        private void Select_startup_mode_deactive()
        {
            foreach (ToolStripMenuItem disabled_startup in startupToolStripMenuItem.DropDownItems)
            {
                disabled_startup.Checked = false;
            }
        }
        private void WindowedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (startup_status != 0) { startup_status = 0; Startup_mode_settings("0"); Select_startup_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "WindowedToolStripMenuItem_Click()"); }
            }
        }
        private void FullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (startup_status != 1) { startup_status = 1; Startup_mode_settings("1"); Select_startup_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "FullScreenToolStripMenuItem_Click()"); }
            }
        }
        private void Startup_mode_settings(string get_startup_value)
        {
            try
            {
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "StartupStatus", get_startup_value);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Startup_mode_settings()"); }
            }
        }
        // STEALTH MODE
        // ======================================================================================================
        private ToolStripMenuItem selected_stealth_mode = null;
        private void Select_stealth_mode_active(object target_stealth_mode)
        {
            if (target_stealth_mode == null)
                return;
            ToolStripMenuItem clicked_stealth_mode = (ToolStripMenuItem)target_stealth_mode;

            if (selected_stealth_mode == clicked_stealth_mode)
                return;
            Select_stealth_mode_deactive();
            selected_stealth_mode = clicked_stealth_mode;
            selected_stealth_mode.Checked = true;
        }
        private void Select_stealth_mode_deactive()
        {
            foreach (ToolStripMenuItem disabled_stealth in stealthModeToolStripMenuItem.DropDownItems)
            {
                disabled_stealth.Checked = false;
            }
        }
        private void StealthModeOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (stealth_status != 1) { stealth_status = 1; Stealth_mode_settings("1"); Select_stealth_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StealthModeOnToolStripMenuItem_Click()"); }
            }
        }
        private void StealthModeOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (stealth_status != 0) { stealth_status = 0; Stealth_mode_settings("0"); Select_stealth_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "StealthModeOffToolStripMenuItem_Click()"); }
            }
        }
        private void Stealth_mode_settings(string get_stealth_value)
        {
            try
            {
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "StealthModeStatus", get_stealth_value);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Stealth_mode_settings()"); }
            }
            // STEALTH MODE CHANGE NOTIFICATION
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            DialogResult stealth_mode_change_message = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("StealthModeChange", "sm_change_notification"), "\n\n", "\n\n"));
            if (stealth_mode_change_message == DialogResult.Yes) { Application.Restart(); }
        }
        // DEBUG MODE
        // ======================================================================================================
        // OPEN DEBUG FOLDER
        private void DebugFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir))
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = logDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DebugFolderToolStripMenuItem_Click()"); }
            }
        }
        // COMPRESS ZIP ALL DEBUG FILE
        private void DebugZIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try
            {
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir))
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                string currentLog = TSLogger.CurrentLogFile;
                var logFiles = Directory.EnumerateFiles(logDir, "*.log", SearchOption.AllDirectories).ToList();
                if (logFiles.Count == 0)
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_file_not_found"));
                    return;
                }
                //
                var confirm = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_check"), "\n\n"));
                if (confirm != DialogResult.Yes)
                    return;
                //
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string zipPath = Path.Combine(desktop, $"Glow_Logs_{DateTime.Now:dd.MM.yyyy_HH.mm.ss}.zip");
                //
                if (File.Exists(zipPath))
                {
                    string zpDir = Path.GetDirectoryName(zipPath);
                    string zpName = Path.GetFileNameWithoutExtension(zipPath);
                    string zpExt = Path.GetExtension(zipPath);
                    for (int i = 2; ; i++)
                    {
                        string candidate = Path.Combine(zpDir, $"{zpName}_{i}{zpExt}");
                        if (!File.Exists(candidate))
                        {
                            zipPath = candidate;
                            break;
                        }
                    }
                }
                string basePath = logDir.EndsWith("\\") ? logDir : logDir + "\\";
                var usedEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    foreach (var file in logFiles)
                    {
                        string entryName = file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? file.Substring(basePath.Length) : Path.GetFileName(file);
                        entryName = entryName.Replace("\\", "/");
                        if (usedEntries.Contains(entryName))
                        {
                            string entryDir = Path.GetDirectoryName(entryName)?.Replace('\\', '/') ?? "";
                            string entryFile = Path.GetFileNameWithoutExtension(entryName);
                            string entryExt = Path.GetExtension(entryName);
                            for (int i = 2; ; i++)
                            {
                                string candidate = string.IsNullOrEmpty(entryDir) ? $"{entryFile}_{i}{entryExt}" : $"{entryDir}/{entryFile}_{i}{entryExt}";
                                if (!usedEntries.Contains(candidate))
                                {
                                    entryName = candidate;
                                    break;
                                }
                            }
                        }
                        usedEntries.Add(entryName);
                        if (!string.IsNullOrEmpty(currentLog) && string.Equals(file, currentLog, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                                using (var entryStream = entry.Open())
                                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    fs.CopyTo(entryStream);
                                }
                            }
                            catch (Exception exFile)
                            {
                                if (debug_status) { TSErrorLog.LogException(exFile, "DebugZIP (CurrentLogStreamCopy)"); }
                            }
                            continue;
                        }
                        try
                        {
                            zip.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                        }
                        catch (IOException)
                        {
                            try
                            {
                                var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                                using (var entryStream = entry.Open())
                                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    fs.CopyTo(entryStream);
                                }
                            }
                            catch (Exception exFile)
                            {
                                if (debug_status) { TSErrorLog.LogException(exFile, "DebugZIP (FallbackStreamCopy)"); }
                            }
                        }
                    }
                }
                var openResult = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_success"), zipPath, "\n\n"));
                if (openResult == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{zipPath}\"", UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DebugZIPToolStripMenuItem_Click()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_zip_failed"), "\n\n", ex.Message));
            }
        }
        // DELETE ALL DEBUG FILE
        private void DebugDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try
            {
                string logDir = TSLogger.LogDirectory;
                if (!Directory.Exists(logDir))
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_folder_not_found"));
                    return;
                }
                var confirm = TS_MessageBoxEngine.TS_MessageBox(this, 6, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_check"), "\n\n"));
                if (confirm != DialogResult.Yes)
                    return;
                TSLogger.UnlockFile();
                int deletedCount = 0;
                foreach (var file in Directory.EnumerateFiles(logDir, "*.log", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch { }
                }
                try
                {
                    Directory.Delete(logDir, false);
                }
                catch { }
                TS_MessageBoxEngine.TS_MessageBox(this, 1, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_success"), deletedCount));
            }
            catch (Exception ex)
            {
                if (debug_status)
                {
                    TSErrorLog.LogException(ex, "DebugDeleteToolStripMenuItem_Click()");
                }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("HeaderDebugMode", "header_debug_mode_delete_failed"), "\n\n", ex.Message));
            }
        }
        private ToolStripMenuItem selected_debug_mode = null;
        private void Select_debug_mode_active(object target_debug_mode)
        {
            if (target_debug_mode == null)
                return;
            ToolStripMenuItem clicked_debug_mode = (ToolStripMenuItem)target_debug_mode;
            if (selected_debug_mode == clicked_debug_mode)
                return;
            Select_debug_mode_deactive();
            selected_debug_mode = clicked_debug_mode;
            selected_debug_mode.Checked = true;
        }
        private void Select_debug_mode_deactive()
        {
            foreach (ToolStripMenuItem disabled_debug in debugModeToolStripMenuItem.DropDownItems)
            {
                disabled_debug.Checked = false;
            }
        }
        private void DebugOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!debug_status) { debug_status = true; TSLogger.Enable(true, Program.glow_console_debug_mode); Debug_mode_settings("1"); Select_debug_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DebugOnToolStripMenuItem_Click()"); }
            }
        }
        private void DebugOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (debug_status) { debug_status = false; TSLogger.Enable(false, Program.glow_console_debug_mode); Debug_mode_settings("0"); Select_debug_mode_active(sender); }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DebugOffToolStripMenuItem_Click()"); }
            }
        }
        private void Debug_mode_settings(string get_debug_value)
        {
            try
            {
                TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                software_setting_save.TSWriteSettings(ts_settings_container, "DebugModeStatus", get_debug_value);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Debug_mode_settings()"); }
            }
        }
        // UPDATE CHECK ENGINE
        // ======================================================================================================
        private void CheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                _ = Software_update_check(1);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "CheckForUpdatesToolStripMenuItem_Click()"); }
            }
        }
        public async Task Software_update_check(int _check_update_ui)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                SetUpdateMenuEnabled(false);
                if (!await IsNetworkAvailable())
                {
                    if (_check_update_ui == 1)
                    {
                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_connection"), "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                    }
                    return;
                }
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.UseProxy = false;
                    using (HttpClient httpClient = new HttpClient(handler))
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(15);
                        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true, MustRevalidate = true };
                        httpClient.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
                        string versionUrl = TS_LinkSystem.github_link_lv;
                        versionUrl += (versionUrl.Contains("?") ? "&" : "?") + "_ts=" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string response = await httpClient.GetStringAsync(versionUrl);
                        string firstLine = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)[0];
                        string client_version_raw = TS_VersionParser.ParseUINormalize(Application.ProductVersion);
                        string last_version_raw = TS_VersionParser.ParseUINormalize(firstLine.Split(new[] { '=' }, 2)[1].Trim());
                        Version client_ver = Version.Parse(client_version_raw);
                        Version last_ver = Version.Parse(last_version_raw);
                        if (client_ver < last_ver)
                        {
                            if (_shutdownStarted || isExiting || IsDisposed || Disposing) return;
                            DialogResult info_update = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_available"), Application.ProductName, "\n\n", client_version_raw, "\n", last_version_raw, "\n\n"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                            if (info_update == DialogResult.Yes)
                            {
                                try
                                {
                                    string updaterPath = Path.Combine(Application.StartupPath, Program.updater_exe_name);
                                    if (File.Exists(updaterPath))
                                    {
                                        string procName = Path.GetFileNameWithoutExtension(updaterPath);
                                        bool isRunning = Process.GetProcessesByName(procName).Length > 0;
                                        if (!isRunning)
                                        {
                                            Process.Start(new ProcessStartInfo(updaterPath) { UseShellExecute = true, Arguments = $"-app={Application.ProductName}" });
                                        }
                                        else
                                        {
                                            TS_MessageBoxEngine.TS_MessageBox(this, 1, software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_c_running"), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        }
                                        try { Close(); } catch { }
                                        return;
                                    }
                                    else
                                    {
                                        TS_MessageBoxEngine.TS_MessageBox(this, 2, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_ts_updater_not_available"), Program.updater_exe_name), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                                        Process.Start(new ProcessStartInfo(TS_LinkSystem.github_link_lr) { UseShellExecute = true });
                                        try { Close(); } catch { }
                                        return;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (debug_status)
                                        TSErrorLog.LogException(ex, $"{Program.updater_exe_name} launch block.");
                                }
                            }
                        }
                        else if (_check_update_ui == 1)
                        {
                            string update_msg = client_ver == last_ver ? string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_not_available"), Application.ProductName, "\n", client_version_raw) : string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_newer"), "\n\n", $"v{client_version_raw}");
                            TS_MessageBoxEngine.TS_MessageBox(this, 1, update_msg, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status)
                {
                    TSErrorLog.LogException(ex, "Software_update_check()");
                }
                if (_check_update_ui == 1 && !IsDisposed && !Disposing)
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_error"), "\n\n", ex.Message), string.Format(software_lang.TSReadLangs("SoftwareUpdate", "su_title"), Application.ProductName));
                }
            }
            finally
            {
                SetUpdateMenuEnabled(true);
            }
        }
        private void SetUpdateMenuEnabled(bool enabled)
        {
            try
            {
                if (IsDisposed || Disposing) return;
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => checkForUpdatesToolStripMenuItem.Enabled = enabled));
                }
                else
                {
                    checkForUpdatesToolStripMenuItem.Enabled = enabled;
                }
            }
            catch { }
        }
        // EXPORT PAGE - SUPPORT SECTION
        // ======================================================================================================
        private void EXPORT_Donate_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(TS_LinkSystem.ts_donate) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "EXPORT_Donate_Click()"); }
            }
        }
        // PRINT ENGINE
        // ======================================================================================================
        // Enumerations Print Engine
        private enum ExportSectionType
        {
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
        private enum ExportFormat
        {
            Txt = 0,
            Html = 1,
            Markdown = 2,
            Json = 3
        }
        private enum RenderFormat
        {
            Txt,
            Html,
            Markdown,
            Json
        }
        // Models Print Engine
        private sealed class SelectorState
        {
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
        private sealed class ExportReport
        {
            public string Title { get; set; }
            public string GeneratedAt { get; set; }
            public List<ReportSection> Sections { get; } = new List<ReportSection>();
            public List<ReportFooterItem> FooterItems { get; } = new List<ReportFooterItem>();
        }
        private sealed class ReportSection
        {
            public int ProgressIndex { get; set; }
            public string Key { get; set; }
            public string Title { get; set; }
            public List<IReportBlock> Blocks { get; } = new List<IReportBlock>();
        }
        private interface IReportBlock { }
        private sealed class ReportKeyValueList : IReportBlock
        {
            public List<ReportItem> Items { get; } = new List<ReportItem>();
        }
        private sealed class ReportGroup : IReportBlock
        {
            public string Title { get; set; }
            public string HtmlTitleOverride { get; set; }
            public bool HtmlTitleIsRaw { get; set; }
            public string MarkdownTitleOverride { get; set; }
            public List<ReportItem> Items { get; } = new List<ReportItem>();
        }
        private sealed class ReportSeparator : IReportBlock
        {
            public int Length { get; set; }
        }
        private sealed class ReportTableBlock : IReportBlock
        {
            public string Title { get; set; }
            public string HtmlTitlePrefix { get; set; }
            public string HtmlTitleSuffix { get; set; }
            public bool SplitTitleByColonForHtml { get; set; }
            public List<string> Rows { get; } = new List<string>();
            public string SummaryLabel { get; set; }
            public string SummaryValue { get; set; }
            public int TxtRowSeparatorLength { get; set; } = 155;
        }
        private sealed class ReportFooterItem
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public bool IsLink { get; set; }
        }
        private sealed class ReportItem
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public string TxtValueOverride { get; set; }
            public string HtmlValueOverride { get; set; }
            public string MarkdownValueOverride { get; set; }
            private string SanitizeValue(string value)
            {
                if (string.IsNullOrEmpty(value)) return value;
                return value.Replace('●', '*');
            }
            public string GetValue(RenderFormat format)
            {
                string rawValue;
                switch (format)
                {
                    case RenderFormat.Txt:
                        rawValue = TxtValueOverride ?? Value ?? string.Empty;
                        break;
                    case RenderFormat.Html:
                        rawValue = HtmlValueOverride ?? Value ?? string.Empty;
                        break;
                    case RenderFormat.Markdown:
                        rawValue = MarkdownValueOverride ?? Value ?? string.Empty;
                        break;
                    case RenderFormat.Json:
                        rawValue = Value ?? string.Empty;
                        break;
                    default:
                        rawValue = Value ?? string.Empty;
                        break;
                }
                return SanitizeValue(rawValue);
            }
        }
        private interface IReportRenderer
        {
            string DefaultExtension { get; }
            string BuildFilter(TSGetLangs software_lang);
            string Render(ExportReport report);
        }
        // Export Section Selection Print Engine
        private HashSet<ExportSectionType> GetSelectedExportSections()
        {
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
        private void ExportModsAdd()
        {
            try
            {
                EXPORT_Selector_List.ExecuteSafe(() =>
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    EXPORT_Selector_List.Items.Clear();
                    EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_txt"));
                    EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_html"));
                    EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_markdown"));
                    EXPORT_Selector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_json"));
                    EXPORT_Selector_List.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                if (debug_status)
                {
                    TSErrorLog.LogException(ex, "ExportModsAdd()");
                }
            }
        }
        // Export UI Filter Print Engine
        private void ExportModsFilterAdd()
        {
            try
            {
                EXPORT_ModeSelector_List.ExecuteSafe(() =>
                {
                    TSGetLangs software_lang = new TSGetLangs(lang_path);
                    EXPORT_ModeSelector_List.Items.Clear();
                    EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_basic"));
                    EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_advanced"));
                    EXPORT_ModeSelector_List.Items.Add(software_lang.TSReadLangs("Export", "e_mode_filter_full"));
                    EXPORT_ModeSelector_List.SelectedIndex = 2;
                });
            }
            catch (Exception ex)
            {
                if (debug_status)
                {
                    TSErrorLog.LogException(ex, "ExportModsFilterAdd()");
                }
            }
        }
        // Export UI Dynamic Filter Print Engine
        private void EXPORT_ModeSelector_List_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
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
                foreach (var item in filterMap)
                {
                    bool shouldCheck = activeSet.Contains(item.Key);
                    if (item.Value.Checked != shouldCheck)
                    {
                        item.Value.Checked = shouldCheck;
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status)
                {
                    TSErrorLog.LogException(ex, "EXPORT_ModeSelector_List_SelectedIndexChanged()");
                }
            }
        }
        // Export UI Start Engine Print Engine
        private void EXPORT_StartEngineBtn_Click(object sender, EventArgs e)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                HashSet<ExportSectionType> selectedSections = GetSelectedExportSections();
                if (selectedSections.Count == 0)
                {
                    TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Export", "e_mode_select_one_min"));
                    return;
                }
                DialogResult start_print_info = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("Export", "e_mode_report_start_info"), EXPORT_Selector_List.SelectedItem.ToString(), "\n\n"));
                if (start_print_info == DialogResult.Yes)
                {
                    EXPORT_Selector_List.Enabled = false;
                    EXPORT_ProgressBGPanel.Visible = true;
                    EXPORT_ProgessLabel.Visible = true;
                    //
                    EXPORT_ModeSelector_List.Enabled = false;
                    EXPORT_CB_FLP.Enabled = false;
                    //
                    EXPORT_StartEngineBtn.Enabled = false;
                    try
                    {
                        Print_engine_mode((ExportFormat)EXPORT_Selector_List.SelectedIndex);
                    }
                    finally
                    {
                        EXPORT_StartEngineBtn.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "EXPORT_StartEngineBtn_Click()"); }
            }
        }
        // Export Process Print Engine
        private void Print_engine_mode(ExportFormat pe_mode)
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            try
            {
                ExportReport report = BuildExportReport();
                int totalSections = report.Sections.Count;
                int currentSection = 0;
                foreach (ReportSection section in report.Sections)
                {
                    currentSection++;
                    Print_engine_progress_update(section.Title, currentSection, totalSections);
                }
                IReportRenderer renderer = CreateRenderer(pe_mode);
                string content = renderer.Render(report);
                EXPORT_ProgessLabel.Text = software_lang.TSReadLangs("Export", "e_mode_save_await");
                EXPORT_ProgressFEPanel.Width = CalculateProgressWidth(totalSections, totalSections);
                SaveRenderedReport(renderer, software_lang, content);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Print_engine_mode()"); }
                TS_MessageBoxEngine.TS_MessageBox(this, 3, string.Format(software_lang.TSReadLangs("PrintEngine", "pe_export_info_error"), "\n", "\n\n", ex.Message));
            }
            finally
            {
                Print_after_mode();
            }
        }
        private void Print_engine_progress_update(string sectionTitle, int status, int totalPages)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (totalPages <= 0)
                    totalPages = 1;
                EXPORT_ProgessLabel.Text = string.Format(software_lang.TSReadLangs("Export", "e_mode_process"), sectionTitle, status, totalPages);
                EXPORT_ProgressFEPanel.Width = CalculateProgressWidth(status, totalPages);
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "Print_engine_progress_update()"); }
            }
        }
        private int CalculateProgressWidth(int status, int total_pages)
        {
            if (total_pages <= 0)
                return 0;
            return (int)((EXPORT_ProgressBGPanel.Width * status) / total_pages);
        }
        private void Print_after_mode()
        {
            EXPORT_Selector_List.Enabled = true;
            EXPORT_ProgressBGPanel.Visible = false;
            EXPORT_ProgressFEPanel.Width = 0;
            EXPORT_ProgessLabel.Visible = false;
            EXPORT_ModeSelector_List.Enabled = true;
            EXPORT_CB_FLP.Enabled = true;
        }
        // Rendering And Saving Print Engine
        private IReportRenderer CreateRenderer(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Txt:
                    return new TxtReportRenderer(lang_path);
                case ExportFormat.Html:
                    return new HtmlReportRenderer(theme, lang, Application.CompanyName, Application.ProductName, lang_path);
                case ExportFormat.Markdown:
                    return new MarkdownReportRenderer(lang_path);
                case ExportFormat.Json:
                    return new JsonReportRenderer();
                default:
                    return new TxtReportRenderer(lang_path);
            }
        }
        private void SaveRenderedReport(IReportRenderer renderer, TSGetLangs software_lang, string content)
        {
            using (SaveFileDialog save_engine = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = Application.ProductName + " - " + software_lang.TSReadLangs("PrintEngine", "pe_save_directory"),
                DefaultExt = renderer.DefaultExtension,
                FileName = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_name"), GetExportHostName()) + " - " + DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss"),
                Filter = renderer.BuildFilter(software_lang)
            })
            {
                if (save_engine.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(save_engine.FileName, content);
                    DialogResult glow_print_engine_query = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("PrintEngine", "pe_save_success") + "\n\n" + software_lang.TSReadLangs("PrintEngine", "pe_save_info_open"), Application.ProductName, save_engine.FileName));
                    if (glow_print_engine_query == DialogResult.Yes)
                    {
                        Process.Start(save_engine.FileName);
                    }
                }
            }
        }
        // Helpers Print Engine
        private SelectorState CaptureSelectorState()
        {
            SelectorState state = new SelectorState
            {
                CpuIndex = (CPU_Selector_List != null && CPU_Selector_List.Items.Count > 0 && CPU_Selector_List.SelectedIndex >= 0) ? CPU_Selector_List.SelectedIndex : 0,
                RamIndex = (RAM_Selector_List != null && RAM_Selector_List.Items.Count > 0 && RAM_Selector_List.SelectedIndex >= 0) ? RAM_Selector_List.SelectedIndex : 0,
                GpuIndex = (GPU_Selector_List != null && GPU_Selector_List.Items.Count > 0 && GPU_Selector_List.SelectedIndex >= 0) ? GPU_Selector_List.SelectedIndex : 0,
                MonitorIndex = (GPU_MonitorSelector_List != null && GPU_MonitorSelector_List.Items.Count > 0 && GPU_MonitorSelector_List.SelectedIndex >= 0) ? GPU_MonitorSelector_List.SelectedIndex : 0,
                DiskIndex = (DISK_Selector_List != null && DISK_Selector_List.Items.Count > 0 && DISK_Selector_List.SelectedIndex >= 0) ? DISK_Selector_List.SelectedIndex : 0,
                NetworkIndex = (NET_Selector_List != null && NET_Selector_List.Items.Count > 0 && NET_Selector_List.SelectedIndex >= 0) ? NET_Selector_List.SelectedIndex : 0,
                UsbIndex = (USB_Selector_List != null && USB_Selector_List.Items.Count > 0 && USB_Selector_List.SelectedIndex >= 0) ? USB_Selector_List.SelectedIndex : 0,
                UsbDeviceIndex = (USB_DeviceSelector_List != null && USB_DeviceSelector_List.Items.Count > 0 && USB_DeviceSelector_List.SelectedIndex >= 0) ? USB_DeviceSelector_List.SelectedIndex : 0,
                SoundIndex = (SOUND_Selector_List != null && SOUND_Selector_List.Items.Count > 0 && SOUND_Selector_List.SelectedIndex >= 0) ? SOUND_Selector_List.SelectedIndex : 0
            };
            return state;
        }
        private void RestoreSelectorState(SelectorState state)
        {
            if (CPU_Selector_List != null && CPU_Selector_List.Items.Count > 0 && state.CpuIndex < CPU_Selector_List.Items.Count)
                CPU_Selector_List.SelectedIndex = state.CpuIndex;
            if (RAM_Selector_List != null && RAM_Selector_List.Items.Count > 0 && state.RamIndex < RAM_Selector_List.Items.Count)
                RAM_Selector_List.SelectedIndex = state.RamIndex;
            if (GPU_Selector_List != null && GPU_Selector_List.Items.Count > 0 && state.GpuIndex < GPU_Selector_List.Items.Count)
                GPU_Selector_List.SelectedIndex = state.GpuIndex;
            if (GPU_MonitorSelector_List != null && GPU_MonitorSelector_List.Items.Count > 0 && state.MonitorIndex < GPU_MonitorSelector_List.Items.Count)
                GPU_MonitorSelector_List.SelectedIndex = state.MonitorIndex;
            if (DISK_Selector_List != null && DISK_Selector_List.Items.Count > 0 && state.DiskIndex < DISK_Selector_List.Items.Count)
                DISK_Selector_List.SelectedIndex = state.DiskIndex;
            if (NET_Selector_List != null && NET_Selector_List.Items.Count > 0 && state.NetworkIndex < NET_Selector_List.Items.Count)
                NET_Selector_List.SelectedIndex = state.NetworkIndex;
            if (USB_Selector_List != null && USB_Selector_List.Items.Count > 0 && state.UsbIndex < USB_Selector_List.Items.Count)
                USB_Selector_List.SelectedIndex = state.UsbIndex;
            if (USB_DeviceSelector_List != null && USB_DeviceSelector_List.Items.Count > 0 && state.UsbDeviceIndex < USB_DeviceSelector_List.Items.Count)
                USB_DeviceSelector_List.SelectedIndex = state.UsbDeviceIndex;
            if (SOUND_Selector_List != null && SOUND_Selector_List.Items.Count > 0 && state.SoundIndex < SOUND_Selector_List.Items.Count)
                SOUND_Selector_List.SelectedIndex = state.SoundIndex;
        }
        private static string HtmlSafe(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
        private string GetExportHostName()
        {
            try
            {
                if (stealth_status == 1)
                {
                    try
                    {
                        string stealthName = new TSGetLangs(lang_path).TSReadLangs("StealthMode", "sm_export_pc");
                        if (!string.IsNullOrWhiteSpace(stealthName) && stealthName != "N/A Langs")
                        {
                            foreach (char c in Path.GetInvalidFileNameChars())
                            {
                                stealthName = stealthName.Replace(c, '-');
                            }
                            stealthName = stealthName.Trim();
                            if (!string.IsNullOrWhiteSpace(stealthName))
                            {
                                return stealthName;
                            }
                        }
                    }
                    catch { }
                    return "Stealth-PC";
                }
                string host = null;
                try { host = OS_ComputerName_V.Text; } catch { }
                if (string.IsNullOrWhiteSpace(host) || host == sm_text)
                {
                    try { host = Dns.GetHostName(); } catch { host = null; }
                }
                if (string.IsNullOrWhiteSpace(host))
                {
                    return "PC";
                }
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    host = host.Replace(c, '-');
                }
                return host.Trim();
            }
            catch
            {
                return "PC";
            }
        }
        private static ReportItem KV(string label, string value)
        {
            ReportItem item = new ReportItem { Label = label ?? string.Empty, Value = value ?? string.Empty };
            return item;
        }
        private static ReportItem KV(string label, string value, string txtOverride, string htmlOverride, string markdownOverride)
        {
            ReportItem item = new ReportItem
            {
                Label = label ?? string.Empty,
                Value = value ?? string.Empty,
                TxtValueOverride = txtOverride,
                HtmlValueOverride = htmlOverride,
                MarkdownValueOverride = markdownOverride
            };
            return item;
        }
        private static ReportKeyValueList KVList(params ReportItem[] items)
        {
            ReportKeyValueList list = new ReportKeyValueList();
            foreach (ReportItem item in items)
            {
                list.Items.Add(item);
            }
            return list;
        }
        private static ReportGroup Group(string title, params ReportItem[] items)
        {
            ReportGroup group = new ReportGroup
            {
                Title = title ?? string.Empty
            };
            foreach (ReportItem item in items)
            {
                group.Items.Add(item);
            }
            return group;
        }
        private static ReportSeparator Separator(int length)
        {
            ReportSeparator separator = new ReportSeparator
            {
                Length = length
            };
            return separator;
        }
        // Report Builder Print Engine
        private ExportReport BuildExportReport()
        {
            TSGetLangs software_lang = new TSGetLangs(lang_path);
            SelectorState state = CaptureSelectorState();
            HashSet<ExportSectionType> selectedSections = GetSelectedExportSections();
            try
            {
                ExportReport report = new ExportReport
                {
                    Title = Application.ProductName + " - " + string.Format(software_lang.TSReadLangs("PrintEngine", "pe_report_title"), GetExportHostName()),
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
                report.FooterItems.Add(new ReportFooterItem
                {
                    Label = Application.ProductName + " " + software_lang.TSReadLangs("PrintEngine", "pe_version"),
                    Value = TS_VersionEngine.TS_SoftwareVersion(1)
                });
                report.FooterItems.Add(new ReportFooterItem
                {
                    Label = string.Empty,
                    Value = TS_SoftwareCopyrightDate.ts_scd_preloader
                });
                report.FooterItems.Add(new ReportFooterItem
                {
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_website"),
                    Value = TS_LinkSystem.website_link,
                    IsLink = true
                });
                report.FooterItems.Add(new ReportFooterItem
                {
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_github"),
                    Value = TS_LinkSystem.github_link,
                    IsLink = true
                });
                report.FooterItems.Add(new ReportFooterItem
                {
                    Label = software_lang.TSReadLangs("PrintEngine", "pe_donate"),
                    Value = TS_LinkSystem.ts_donate,
                    IsLink = true
                });
                return report;
            }
            finally
            {
                RestoreSelectorState(state);
            }
        }
        // Section Builders Print Engine
        private ReportSection BuildOsSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
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
                KV(OS_WinMachineID.Text, OS_WinMachineID_V.Text),
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
        private ReportSection BuildMotherboardSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
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
        private ReportSection BuildCpuSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 3,
                Key = "cpu",
                Title = software_lang.TSReadLangs("LeftMenu", "left_cpu")
            };
            if (CPU_Selector_List.Items.Count == 0)
            {
                return section;
            }
            try
            {
                int cpu_slot = CPU_Selector_List.Items.Count;
                for (int cs = 1; cs <= cpu_slot; cs++)
                {
                    CPU_Selector_List.SelectedIndex = cs - 1;
                    string cpuSelectedTitle = CPU_Selector_List.SelectedItem != null ? CPU_Selector_List.SelectedItem.ToString() : string.Empty;
                    ReportGroup group = Group(
                        software_lang.TSReadLangs("Cpu_Content", "cpu_c_print") + " " + cpuSelectedTitle,
                        KV(CPU_Name.Text, cpuSelectedTitle),
                        KV(CPU_Manufacturer.Text, CPU_Manufacturer_V.Text),
                        KV(CPU_Architectural.Text, CPU_Architectural_V.Text),
                        KV(CPU_Speed.Text, CPU_Speed_V.Text),
                        KV(CPU_CurrentSpeed.Text, CPU_CurrentSpeed_V.Text),
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
                   KV(CPU_Handles.Text, CPU_Handles_V.Text),
                   KV(CPU_Features.Text, CPU_Features_V.Text)
               ));
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BuildCpuSection()"); }
            }
            return section;
        }
        private ReportSection BuildRamSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 4,
                Key = "ram",
                Title = software_lang.TSReadLangs("LeftMenu", "left_ram")
            };
            if (RAM_Selector_List == null || RAM_Selector_List.Items.Count == 0)
            {
                return section;
            }
            section.Blocks.Add(KVList(
                KV(RAM_TotalRAM.Text, RAM_TotalRAM_V.Text),
                KV(RAM_UsageRAMCount.Text, RAM_UsageRAMCount_V.Text),
                KV(RAM_EmptyRamCount.Text, RAM_EmptyRamCount_V.Text),
                KV(RAM_TotalVirtualRam.Text, RAM_TotalVirtualRam_V.Text),
                KV(RAM_UsageVirtualRam.Text, RAM_UsageVirtualRam_V.Text),
                KV(RAM_EmptyVirtualRam.Text, RAM_EmptyVirtualRam_V.Text),
                KV(RAM_SlotStatus.Text, RAM_SlotStatus_V.Text)
            ));
            try
            {
                int ram_slot = RAM_Selector_List.Items.Count;
                for (int rs = 1; rs <= ram_slot; rs++)
                {
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
                    if (!string.IsNullOrEmpty(title) && title.Contains("-"))
                    {
                        string[] ram_explode = title.Split('-');
                        if (ram_explode.Length >= 2)
                        {
                            group.HtmlTitleOverride = HtmlSafe(ram_explode[0].Trim()) + " - </span><span>" + HtmlSafe(ram_explode[1].Trim());
                            group.HtmlTitleIsRaw = true;
                        }
                    }
                    section.Blocks.Add(group);
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BuildRamSection()"); }
            }
            return section;
        }
        private ReportSection BuildGpuSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 5,
                Key = "gpu",
                Title = software_lang.TSReadLangs("LeftMenu", "left_gpu")
            };
            if (GPU_Selector_List != null && GPU_Selector_List.Items.Count > 0)
            {
                try
                {
                    int gpu_amount = GPU_Selector_List.Items.Count;
                    for (int gpu_render = 1; gpu_render <= gpu_amount; gpu_render++)
                    {
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
                        group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Gpu_Content", "gpu_c_gpu_print")) + "</span><span>" + HtmlSafe(gpuSelectedTitle);
                        group.HtmlTitleIsRaw = true;
                        section.Blocks.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildGpuSection()"); }
                }
            }
            if (GPU_Selector_List != null && GPU_Selector_List.Items.Count > 0)
            {
                section.Blocks.Add(Separator(60));
            }
            if (GPU_MonitorSelector_List != null && GPU_MonitorSelector_List.Items.Count > 0)
            {
                try
                {
                    int screen_amount = GPU_MonitorSelector_List.Items.Count;
                    for (int sa = 1; sa <= screen_amount; sa++)
                    {
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
                        if (!string.IsNullOrEmpty(title) && title.Contains("-"))
                        {
                            string[] gpu_monitor_explode = title.Split('-');
                            if (gpu_monitor_explode.Length >= 2)
                            {
                                group.HtmlTitleOverride = HtmlSafe(gpu_monitor_explode[0].Trim()) + " - </span><span>" + HtmlSafe(gpu_monitor_explode[1].Trim());
                                group.HtmlTitleIsRaw = true;
                            }
                        }
                        section.Blocks.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildGpuSection()"); }
                }
            }
            return section;
        }
        private ReportSection BuildStorageSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 6,
                Key = "storage",
                Title = software_lang.TSReadLangs("LeftMenu", "left_storage")
            };
            if (DISK_Selector_List != null && DISK_Selector_List.Items.Count > 0)
            {
                section.Blocks.Add(Group(
                    DISK_TTLP_L1.Text,
                    KV(DISK_TTLP_P1_L1.Text, DISK_TTLP_P1_L2.Text),
                    KV(DISK_TTLP_P2_L1.Text, DISK_TTLP_P2_L2.Text),
                    KV(DISK_TTLP_P3_L1.Text, DISK_TTLP_P3_L2.Text),
                    KV(DISK_TTLP_P4_L1.Text, DISK_TTLP_P4_L2.Text)
                ));
                section.Blocks.Add(Separator(12));
                try
                {
                    int disk_amount = DISK_Selector_List.Items.Count;
                    for (int disk_render = 1; disk_render <= disk_amount; disk_render++)
                    {
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
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildStorageSection()"); }
                }
            }
            return section;
        }
        private ReportSection BuildNetworkSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 7,
                Key = "network",
                Title = software_lang.TSReadLangs("LeftMenu", "left_network")
            };
            if (NET_Selector_List != null && NET_Selector_List.Items.Count > 0)
            {
                section.Blocks.Add(KVList(
                    KV(NET_LT_Device.Text, NET_LT_Device_V.Text),
                    KV(NET_LT_BandWidth.Text, NET_LT_BandWidth_V.Text),
                    KV(NET_LT_LocalIP.Text, NET_LT_LocalIP_V.Text),
                    KV(NET_LT_GatewayIP.Text, NET_LT_GatewayIP_V.Text),
                    KV(NET_LT_DL1.Text, NET_LT_DL2.Text),
                    KV(NET_LT_UL1.Text, NET_LT_UL2.Text)
                ));
                try
                {
                    int net_amount = NET_Selector_List.Items.Count;
                    for (int net_render = 1; net_render <= net_amount; net_render++)
                    {
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
                            KV(NET_IPv6Adress.Text, NET_IPv6Adress_V.Text),
                            KV(NET_SubnetMask.Text, NET_SubnetMask_V.Text),
                            KV(NET_DNS_v4_1.Text, NET_DNS_v4_1_V.Text),
                            KV(NET_DNS_v4_2.Text, NET_DNS_v4_2_V.Text),
                            KV(NET_DNS_v6_1.Text, NET_DNS_v6_1_V.Text),
                            KV(NET_DNS_v6_2.Text, NET_DNS_v6_2_V.Text)
                        );
                        group.HtmlTitleOverride = HtmlSafe(software_lang.TSReadLangs("Network_Content", "nk_c_network_print")) + "</span><span>" + HtmlSafe(netSelectedTitle);
                        group.HtmlTitleIsRaw = true;
                        section.Blocks.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildNetworkSection()"); }
                }
                section.Blocks.Add(Group(
                    software_lang.TSReadLangs("Network_Content", "nk_c_gen_net_info"),
                    KV(NET_P_IP_Adress.Text, NET_P_IP_Adress_V.Text),
                    KV(NET_P_ISP.Text, NET_P_ISP_V.Text)
                ));
            }
            return section;
        }
        private ReportSection BuildUsbSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 8,
                Key = "usb",
                Title = software_lang.TSReadLangs("LeftMenu", "left_usb")
            };
            if (USB_Selector_List != null && USB_Selector_List.Items.Count > 0)
            {
                try
                {
                    int usb_con_amount = USB_Selector_List.Items.Count;
                    for (int usb_con_render = 1; usb_con_render <= usb_con_amount; usb_con_render++)
                    {
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
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildUsbSection()"); }
                }
            }
            if (USB_Selector_List != null && USB_Selector_List.Items.Count > 0)
            {
                section.Blocks.Add(Separator(40));
            }
            if (USB_DeviceSelector_List != null && USB_DeviceSelector_List.Items.Count > 0)
            {
                try
                {
                    int usb_amount = USB_DeviceSelector_List.Items.Count;
                    for (int usb_render = 1; usb_render <= usb_amount; usb_render++)
                    {
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
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildUsbSection()"); }
                }
            }
            return section;
        }
        private ReportSection BuildSoundSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 9,
                Key = "sound",
                Title = software_lang.TSReadLangs("LeftMenu", "left_sound")
            };
            if (SOUND_Selector_List != null && SOUND_Selector_List.Items.Count > 0)
            {
                int soundPrevIndex;
                try { soundPrevIndex = SOUND_Selector_List.SelectedIndex; } catch { soundPrevIndex = -1; }
                try
                {
                    int sound_amount = SOUND_Selector_List.Items.Count;
                    for (int sound_render = 1; sound_render <= sound_amount; sound_render++)
                    {
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
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildSoundSection()"); }
                }
                try
                {
                    if (soundPrevIndex >= 0 && soundPrevIndex < SOUND_Selector_List.Items.Count) SOUND_Selector_List.SelectedIndex = soundPrevIndex;
                }
                catch (Exception ex)
                {
                    if (debug_status) { TSErrorLog.LogException(ex, "BuildSoundSection()"); }
                }
            }
            return section;
        }
        private ReportSection BuildBatterySection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 10,
                Key = "battery",
                Title = software_lang.TSReadLangs("LeftMenu", "left_battery")
            };
            PowerStatus power_status = SystemInformation.PowerStatus;
            string battery_charging = HasSystemBattery() ? power_status.BatteryChargeStatus.ToString() : "NoSystemBattery";
            if (battery_charging == "NoSystemBattery")
            {
                section.Blocks.Add(KVList(
                    KV(BATTERY_Status.Text, BATTERY_Status_V.Text)
                ));
            }
            else
            {
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
        private ReportSection BuildInstalledDriversSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 11,
                Key = "installed_drivers",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_drivers")
            };
            ReportTableBlock table = new ReportTableBlock
            {
                Title = software_lang.TSReadLangs("PrintEngine", "pe_driver_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Osd_Content", "osd_total_installed_driver_count"),
                SummaryValue = OSD_TYSS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try
            {
                foreach (DataGridViewRow row in OSD_DataMainTable.Rows)
                {
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledDriversSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        private ReportSection BuildInstalledServicesSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 12,
                Key = "installed_services",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_services")
            };
            ReportTableBlock table = new ReportTableBlock
            {
                Title = software_lang.TSReadLangs("PrintEngine", "pe_service_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Services_Content", "ss_total_installed_service_count"),
                SummaryValue = SERVICE_TYS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try
            {
                foreach (DataGridViewRow row in SERVICE_DataMainTable.Rows)
                {
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledServicesSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        private ReportSection BuildInstalledAppsSection(TSGetLangs software_lang)
        {
            ReportSection section = new ReportSection
            {
                ProgressIndex = 13,
                Key = "installed_apps",
                Title = software_lang.TSReadLangs("LeftMenu", "left_installed_apps")
            };
            ReportTableBlock table = new ReportTableBlock
            {
                Title = software_lang.TSReadLangs("PrintEngine", "pe_instapps_sorting"),
                SummaryLabel = software_lang.TSReadLangs("Instapps_Content", "ia_total_installed_apps_count"),
                SummaryValue = INSTAPPS_TYUS_V.Text,
                SplitTitleByColonForHtml = true
            };
            try
            {
                foreach (DataGridViewRow row in INSTAPPS_DataMainTable.Rows)
                {
                    if (row.IsNewRow) continue;
                    IEnumerable<string> cellValues = row.Cells.Cast<DataGridViewCell>().Skip(1).Select(c => c.Value != null ? c.Value.ToString() : string.Empty);
                    table.Rows.Add(string.Join(" | ", cellValues));
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "BuildInstalledAppsSection()"); }
            }
            section.Blocks.Add(table);
            return section;
        }
        // Report Renderers Print Engine
        private sealed class TxtReportRenderer : IReportRenderer
        {
            private readonly string _langPath;
            public TxtReportRenderer(string langPath)
            {
                _langPath = langPath;
            }
            public string DefaultExtension
            {
                get { return "txt"; }
            }
            public string BuildFilter(TSGetLangs software_lang)
            {
                return software_lang.TSReadLangs("PrintEngine", "pe_save_txt") + " (*.txt)|*.txt";
            }
            public string Render(ExportReport report)
            {
                TSGetLangs software_lang = new TSGetLangs(_langPath);
                List<string> lines = new List<string>{
                    "<------------- " + report.Title.ToUpper() + " ------------->",
                    "<------------- " + software_lang.TSReadLangs("PrintEngine", "pe_process_time") + " " + report.GeneratedAt + " ------------->",
                    string.Empty,
                    new string('-', 60),
                    string.Empty
                };
                foreach (ReportSection section in report.Sections)
                {
                    lines.Add("<------- " + (section.Title ?? string.Empty) + " ------->");
                    lines.Add(string.Empty);
                    foreach (IReportBlock block in section.Blocks)
                    {
                        if (block is ReportKeyValueList kvList)
                        {
                            foreach (ReportItem item in kvList.Items)
                            {
                                lines.Add((item.Label ?? string.Empty) + " " + item.GetValue(RenderFormat.Txt));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportGroup group)
                        {
                            lines.Add(group.Title ?? string.Empty);
                            lines.Add(string.Empty);
                            foreach (ReportItem item in group.Items)
                            {
                                lines.Add((item.Label ?? string.Empty) + " " + item.GetValue(RenderFormat.Txt));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportSeparator separator)
                        {
                            lines.Add(new string('-', separator.Length));
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportTableBlock table)
                        {
                            lines.Add(table.Title ?? string.Empty);
                            lines.Add(string.Empty);
                            foreach (string row in table.Rows)
                            {
                                lines.Add(row);
                                lines.Add(new string('-', table.TxtRowSeparatorLength));
                            }
                            lines.Add(string.Empty);
                            lines.Add((table.SummaryLabel ?? string.Empty) + " " + (table.SummaryValue ?? string.Empty));
                            lines.Add(string.Empty);
                            continue;
                        }
                    }
                    if (lines.Count > 0 && lines[lines.Count - 1] != string.Empty)
                    {
                        lines.Add(string.Empty);
                    }
                    lines.Add(new string('-', 60));
                    lines.Add(string.Empty);
                }
                foreach (ReportFooterItem item in report.FooterItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Label))
                    {
                        lines.Add(item.Value ?? string.Empty);
                    }
                    else
                    {
                        lines.Add(item.Label + " " + (item.Value ?? string.Empty));
                    }
                }
                return string.Join(Environment.NewLine, lines);
            }
        }
        private sealed class HtmlReportRenderer : IReportRenderer
        {
            private readonly int _theme;
            private readonly string _lang;
            private readonly string _companyName;
            private readonly string _productName;
            private readonly string _langPath;
            public HtmlReportRenderer(int theme, string lang, string companyName, string productName, string langPath)
            {
                _theme = theme;
                _lang = lang;
                _companyName = companyName;
                _productName = productName;
                _langPath = langPath;
            }
            public string DefaultExtension
            {
                get { return "html"; }
            }
            public string BuildFilter(TSGetLangs software_lang)
            {
                return software_lang.TSReadLangs("PrintEngine", "pe_save_html") + " (*.html)|*.html";
            }
            public string Render(ExportReport report)
            {
                TSGetLangs software_lang = new TSGetLangs(_langPath);
                Color html_body_bg_color = TS_ThemeEngine.ColorMode(_theme, "TSBT_BGColor");
                string html_bbgc = string.Format("#{0}{1}{2}", html_body_bg_color.R.ToString("X2"), html_body_bg_color.G.ToString("X2"), html_body_bg_color.B.ToString("X2"));
                Color html_middle_bg_color = TS_ThemeEngine.ColorMode(_theme, "TSBT_BGColor2");
                string html_mbgc = string.Format("#{0}{1}{2}", html_middle_bg_color.R.ToString("X2"), html_middle_bg_color.G.ToString("X2"), html_middle_bg_color.B.ToString("X2"));
                Color html_ui_fe_color = TS_ThemeEngine.ColorMode(_theme, "TSBT_AccentColor");
                string html_uifc = string.Format("#{0}{1}{2}", html_ui_fe_color.R.ToString("X2"), html_ui_fe_color.G.ToString("X2"), html_ui_fe_color.B.ToString("X2"));
                Color html_ui_fe_hover_color = TS_ThemeEngine.ColorMode(_theme, "AccentColorHover");
                string html_uifhc = string.Format("#{0}{1}{2}", html_ui_fe_hover_color.R.ToString("X2"), html_ui_fe_hover_color.G.ToString("X2"), html_ui_fe_hover_color.B.ToString("X2"));
                string print_html_font_url = @"https://fonts.googleapis.com/css2?family=Open+Sans:ital,wght@0,300..800;1,300..800&display=swap";
                string print_html_glow_logo_url = @"https://raw.githubusercontent.com/turkaysoft/glow/main/Glow/glow_images/glow_logo/GlowLogo.ico";
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
                sb.AppendLine("\t\t*{ font-family: 'Open Sans', 'Segoe UI', system-ui, -apple-system, sans-serif; margin: 0; padding: 0; box-sizing: border-box; text-decoration: none; }");
                sb.AppendLine("\t\t:root{ --ts-bg: " + html_bbgc + "; --ts-card: " + html_mbgc + "; --ts-accent: " + html_uifc + "; --ts-accent-hover: " + html_uifhc + "; }");
                if (_theme == 1) { sb.AppendLine("\t\t:root { color-scheme: light; }"); }
                else if (_theme == 0) { sb.AppendLine("\t\t:root { color-scheme: dark; }"); }
                sb.AppendLine("\t\ta:visited{ color: var(--ts-accent-hover); }");
                sb.AppendLine("\t\t.ts_scroll_top{ background-color: var(--ts-accent); position: fixed; bottom: -100px; right: 15px; width: 45px; height: 45px; justify-content: center; align-items: center; display: flex; border-radius: 50%; border: none; cursor: pointer; z-index: 999; box-shadow: 0 4px 14px rgba(0,0,0,0.35); transition: bottom 0.2s ease, background-color 0.2s ease; }");
                sb.AppendLine("\t\t.ts_scroll_top > .ts_arrow_up { border-left: 5px solid var(--ts-card); border-top: 5px solid var(--ts-card); width: 16px; height: 16px; transform: rotate(45deg); border-radius: 2px; margin-top: 4px; }");
                sb.AppendLine("\t\t.ts_scroll_top:hover{ background-color: var(--ts-accent-hover); }");
                sb.AppendLine("\t\t.ts_scroll_top.active{ bottom: 15px; }");
                sb.AppendLine("\t\t@media (max-width: 700px){ .ts_scroll_top{ bottom: -100px; right: 10px; } .ts_scroll_top.active{ bottom: 10px; } }");
                sb.AppendLine("\t\t.ts_page_wrapper{ background-color: var(--ts-card); font-weight: 500; position: fixed; left: 15px; bottom: 15px; width: auto; height: 35px; padding: 0 8px; font-size: 16px; margin: 0; outline: none; border-radius: 8px; border: 1px solid var(--ts-accent); cursor: pointer; box-shadow: 0 4px 14px rgba(0,0,0,0.35); }");
                sb.AppendLine("\t\t@media (max-width: 700px){ .ts_page_wrapper{ left: 10px; bottom: 10px; height: 30px; border-radius: 6px; } }");
                sb.AppendLine("\t\tbody{ background-color: var(--ts-bg); padding: 5px 10px; min-height: 100vh; line-height: 1.55; justify-content: center; align-items: flex-start; display: flex; }");
                sb.AppendLine("\t\t#main_container{ width: 100%; max-width: 1280px; margin: 0 auto; height: auto; justify-content: center; align-items: center; display: flex; flex-direction: column; }");
                sb.AppendLine("\t\t#main_container > h2{ margin: 25px 0; font-weight: 500; color: var(--ts-accent); }");
                sb.AppendLine("\t\t#main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ background-color: var(--ts-card); width: 100%; height: auto; border-radius: 12px; margin: 5px 0; padding: 15px; box-sizing: border-box; display: block; word-break: break-word; box-shadow: 0 4px 18px rgba(0,0,0,0.22); }");
                sb.AppendLine("\t\t.ts_box_wrapper{ scroll-margin-top: 16px; }");
                sb.AppendLine("\t\t#main_container > .b8 > h3{ margin: 0 0 15px 0; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1){ justify-content: start; align-items: center; display: flex; gap: 25px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > img{ width: 75px; height: 75px; border-radius: 12px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text{ justify-content: center; align-items: start; display: flex; flex-direction: column; gap: 5px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > h2{ color: var(--ts-accent); font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > span{ font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text > span > label{ color: var(--ts-accent); font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h2{ color: var(--ts-accent); font-weight: 500; text-align: center; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h3{ color: var(--ts-accent); font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h4{ margin: 13px 0 0 11px; font-weight: 600; justify-content: start; align-items: center; display: flex; gap: 5px; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > h4 > span:nth-child(2){ color: var(--ts-accent); font-weight: 600; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul{ margin: 15px 0 0 30px; padding: 0; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li{ margin: 5px 0; font-weight: 500; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > span{ margin: 0 7px 0 0; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > span:nth-child(2){ color: var(--ts-accent); }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > a{ color: var(--ts-accent); text-decoration: underline; }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > ul > li > a:hover{ color: var(--ts-accent-hover); }");
                sb.AppendLine("\t\t#main_container > .ts_box_wrapper > hr{ height: 1px; background-color: var(--ts-accent); border: none; margin: 20px 0; padding: 0; }");
                sb.AppendLine("\t\t#main_container > .b15 > ul > li > span{ color: var(--ts-accent); }");
                sb.AppendLine("\t\t.ts_box_wrapper a:focus-visible, .ts_page_wrapper:focus-visible, .ts_scroll_top:focus-visible{ outline: 2px solid var(--ts-accent-hover); outline-offset: 2px; }");
                sb.AppendLine("\t\t@media (max-width: 735px){ #main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ padding: 10px; } #main_container > .ts_box_wrapper > h3{ text-align: center; } }");
                sb.AppendLine("\t\t@media (max-width: 495px){ #main_container > .ts_box_wrapper:nth-child(1){ flex-direction: column; justify-content: center; gap: 10px; } #main_container > .ts_box_wrapper:nth-child(1) > .ts_box_text{ text-align: center; align-items: center; } #main_container > .ts_box_wrapper > ul{ margin: 15px 0 0 25px; } #main_container > .ts_box_wrapper > h4{ margin: 13px 0 0 6px; } }");
                sb.AppendLine("\t\t@media (prefers-reduced-motion: reduce){ .ts_scroll_top{ transition: none; } }");
                sb.AppendLine("\t\t@media print{ .ts_scroll_top, .ts_page_wrapper{ display: none !important; } body{ padding: 0; display: block; } #main_container{ max-width: 100%; } #main_container > .b1, .b2, .b3, .b4, .b5, .b6, .b7, .b8, .b9, .b10, .b11, .b12, .b13, .b14, .b15{ box-shadow: none; break-inside: avoid; } }");
                sb.AppendLine("\t</style>");
                sb.AppendLine("\t<link rel='icon' type='image/x-icon' href='" + print_html_glow_logo_url + "'>");
                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                sb.AppendLine("\t<div class='ts_scroll_top' onclick='ts_scroll_to_up();' title='" + H(software_lang.TSReadLangs("PrintEngine", "pe_scroll_top_select")) + "'><span class='ts_arrow_up'></span></div>");
                sb.AppendLine("\t<select title='" + H(software_lang.TSReadLangs("PrintEngine", "pe_html_partition_select")) + "' class='ts_page_wrapper' id='ts_session_select'>");
                sb.AppendLine("\t\t<option value='b1'>" + H(software_lang.TSReadLangs("PrintEngine", "pe_html_header_select")) + "</option>");
                for (int i = 0; i < report.Sections.Count; i++)
                {
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
                for (int sectionIndex = 0; sectionIndex < report.Sections.Count; sectionIndex++)
                {
                    ReportSection section = report.Sections[sectionIndex];
                    string boxClass = "b" + (sectionIndex + 2).ToString();
                    sb.AppendLine("\t\t<div class='" + boxClass + " ts_box_wrapper'>");
                    sb.AppendLine("\t\t\t<h3>" + H(section.Title) + "</h3>");
                    foreach (IReportBlock block in section.Blocks)
                    {
                        if (block is ReportKeyValueList kvList)
                        {
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (ReportItem item in kvList.Items)
                            {
                                sb.AppendLine("\t\t\t\t<li><span>" + H(item.Label) + "</span><span>" + H(item.GetValue(RenderFormat.Html)) + "</span></li>");
                            }
                            sb.AppendLine("\t\t\t</ul>");
                            continue;
                        }
                        if (block is ReportGroup group)
                        {
                            string groupTitle = group.HtmlTitleOverride ?? group.Title ?? string.Empty;
                            if (group.HtmlTitleIsRaw)
                            {
                                sb.AppendLine("\t\t\t\t<h4><span>" + groupTitle + "</span></h4>");
                            }
                            else
                            {
                                sb.AppendLine("\t\t\t\t<h4><span>" + H(groupTitle) + "</span></h4>");
                            }
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (ReportItem item in group.Items)
                            {
                                sb.AppendLine("\t\t\t\t<li><span>" + H(item.Label) + "</span><span>" + H(item.GetValue(RenderFormat.Html)) + "</span></li>");
                            }
                            sb.AppendLine("\t\t\t</ul>");
                            continue;
                        }
                        if (block is ReportSeparator separator)
                        {
                            sb.AppendLine("\t\t\t<hr>");
                            continue;
                        }
                        if (block is ReportTableBlock table)
                        {
                            if (table.SplitTitleByColonForHtml)
                            {
                                string[] parts = (table.Title ?? string.Empty).Split(':');
                                string left = parts.Length > 0 ? parts[0] + ":" : string.Empty;
                                string right = parts.Length > 1 ? parts[1] : string.Empty;
                                sb.AppendLine("\t\t\t<h4><span>" + H(left) + "</span><span>" + H(right) + "</span></h4>");
                            }
                            else
                            {
                                sb.AppendLine("\t\t\t<h4><span>" + H(table.Title) + "</span></h4>");
                            }
                            sb.AppendLine("\t\t\t<ul>");
                            foreach (string row in table.Rows)
                            {
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
                foreach (ReportFooterItem item in report.FooterItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Label))
                    {
                        sb.AppendLine("\t\t\t\t<li>" + H(item.Value) + "</li>");
                    }
                    else if (item.IsLink)
                    {
                        string safeValue = H(item.Value);
                        sb.AppendLine("\t\t\t\t<li>" + H(item.Label) + " <a target='_blank' href='" + safeValue + "'>" + safeValue + "</a></li>");
                    }
                    else
                    {
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
            private static string H(string value)
            {
                return WebUtility.HtmlEncode(value ?? string.Empty);
            }
        }
        private sealed class MarkdownReportRenderer : IReportRenderer
        {
            private readonly string _langPath;
            public MarkdownReportRenderer(string langPath)
            {
                _langPath = langPath;
            }
            public string DefaultExtension
            {
                get { return "md"; }
            }
            public string BuildFilter(TSGetLangs software_lang)
            {
                return software_lang.TSReadLangs("PrintEngine", "pe_save_markdown") + " (*.md)|*.md";
            }
            public string Render(ExportReport report)
            {
                TSGetLangs software_lang = new TSGetLangs(_langPath);
                List<string> lines = new List<string> { "# " + Md(report.Title), string.Empty, "**" + Md(software_lang.TSReadLangs("PrintEngine", "pe_process_time")) + "** " + Md(report.GeneratedAt), string.Empty };
                foreach (ReportSection section in report.Sections)
                {
                    lines.Add("## " + Md(section.Title));
                    lines.Add(string.Empty);
                    foreach (IReportBlock block in section.Blocks)
                    {
                        if (block is ReportKeyValueList kvList)
                        {
                            foreach (ReportItem item in kvList.Items)
                            {
                                lines.Add("- **" + Md(item.Label) + "** " + Md(item.GetValue(RenderFormat.Markdown)));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportGroup group)
                        {
                            string groupTitle = group.MarkdownTitleOverride ?? group.Title ?? string.Empty;
                            lines.Add("### " + Md(groupTitle));
                            lines.Add(string.Empty);
                            foreach (ReportItem item in group.Items)
                            {
                                lines.Add("- **" + Md(item.Label) + "** " + Md(item.GetValue(RenderFormat.Markdown)));
                            }
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportSeparator)
                        {
                            lines.Add("---");
                            lines.Add(string.Empty);
                            continue;
                        }
                        if (block is ReportTableBlock table)
                        {
                            lines.Add("### " + Md(table.Title));
                            lines.Add(string.Empty);
                            foreach (string row in table.Rows)
                            {
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
                foreach (ReportFooterItem item in report.FooterItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Label))
                    {
                        lines.Add("- " + Md(item.Value));
                    }
                    else if (item.IsLink)
                    {
                        lines.Add("- **" + Md(item.Label) + "** [" + Md(item.Value) + "](" + (item.Value ?? string.Empty) + ")");
                    }
                    else
                    {
                        lines.Add("- **" + Md(item.Label) + "** " + Md(item.Value));
                    }
                }
                return string.Join(Environment.NewLine, lines);
            }
            private static string Md(string value)
            {
                return (value ?? string.Empty).Replace("\r", "").Replace("\n", "  \n");
            }
        }
        private sealed class JsonReportRenderer : IReportRenderer
        {
            public string DefaultExtension
            {
                get { return "json"; }
            }
            public string BuildFilter(TSGetLangs software_lang)
            {
                return software_lang.TSReadLangs("PrintEngine", "pe_save_json") + " (*.json)|*.json";
            }
            public string Render(ExportReport report)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"title\": " + J(report.Title) + ",");
                sb.AppendLine("  \"generatedAt\": " + J(report.GeneratedAt) + ",");
                sb.AppendLine("  \"sections\": [");
                for (int sectionIndex = 0; sectionIndex < report.Sections.Count; sectionIndex++)
                {
                    ReportSection section = report.Sections[sectionIndex];
                    sb.AppendLine("    {");
                    sb.AppendLine("      \"title\": " + J(section.Title) + ",");
                    sb.AppendLine("      \"blocks\": [");
                    for (int blockIndex = 0; blockIndex < section.Blocks.Count; blockIndex++)
                    {
                        sb.Append(RenderBlock(section.Blocks[blockIndex]));
                        sb.AppendLine(blockIndex < section.Blocks.Count - 1 ? "," : string.Empty);
                    }
                    sb.AppendLine("      ]");
                    sb.Append("    }");
                    sb.AppendLine(sectionIndex < report.Sections.Count - 1 ? "," : string.Empty);
                }
                sb.AppendLine("  ],");
                sb.AppendLine("  \"footer\": [");
                for (int footerIndex = 0; footerIndex < report.FooterItems.Count; footerIndex++)
                {
                    ReportFooterItem item = report.FooterItems[footerIndex];
                    sb.Append("    { \"label\": " + J(item.Label) + ", \"value\": " + J(item.Value) + " }");
                    sb.AppendLine(footerIndex < report.FooterItems.Count - 1 ? "," : string.Empty);
                }
                sb.AppendLine("  ]");
                sb.Append("}");
                return sb.ToString();
            }
            private static string RenderBlock(IReportBlock block)
            {
                if (block is ReportKeyValueList kvList)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("        { \"type\": \"keyValueList\", \"items\": [");
                    AppendItems(sb, kvList.Items, "          ");
                    sb.Append("        ] }");
                    return sb.ToString();
                }
                if (block is ReportGroup group)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("        { \"type\": \"group\", \"title\": " + J(group.Title) + ", \"items\": [");
                    AppendItems(sb, group.Items, "          ");
                    sb.Append("        ] }");
                    return sb.ToString();
                }
                if (block is ReportSeparator)
                {
                    return "        { \"type\": \"separator\" }";
                }
                if (block is ReportTableBlock table)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("        { \"type\": \"table\", \"title\": " + J(table.Title) + ", \"rows\": [");
                    for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        sb.Append("          " + J(table.Rows[rowIndex]));
                        sb.AppendLine(rowIndex < table.Rows.Count - 1 ? "," : string.Empty);
                    }
                    sb.AppendLine("        ], \"summary\": { \"label\": " + J(table.SummaryLabel) + ", \"value\": " + J(table.SummaryValue) + " } }");
                    return sb.ToString();
                }
                return "        { \"type\": \"unknown\" }";
            }
            private static void AppendItems(StringBuilder sb, List<ReportItem> items, string indent)
            {
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    ReportItem item = items[itemIndex];
                    sb.Append(indent + "{ \"label\": " + J(item.Label) + ", \"value\": " + J(item.GetValue(RenderFormat.Json)) + " }");
                    sb.AppendLine(itemIndex < items.Count - 1 ? "," : string.Empty);
                }
            }
            private static string J(string value)
            {
                if (value == null) return "\"\"";
                StringBuilder sb = new StringBuilder();
                sb.Append('"');
                foreach (char c in value)
                {
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20) sb.Append("\\u" + ((int)c).ToString("X4"));
                            else sb.Append(c);
                            break;
                    }
                }
                sb.Append('"');
                return sb.ToString();
            }
        }
        // TS TOOL LAUNCHER MODULE
        // ======================================================================================================
        private void TSToolLauncher<T>(string formName, string langKey) where T : Form, new()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (Application.OpenForms[formName] == null)
                {
                    T tool = new T { Name = formName };
                    tool.Show();
                }
                else
                {
                    if (Application.OpenForms[formName].WindowState == FormWindowState.Minimized)
                    {
                        Application.OpenForms[formName].WindowState = FormWindowState.Normal;
                    }
                    string public_message;
                    public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("HeaderTools", langKey));
                    if (formName == "glow_about")
                    {
                        public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("HeaderMenu", langKey));
                    }
                    else if (formName == "glow_wallpaper_preview_tool")
                    {
                        public_message = string.Format(software_lang.TSReadLangs("HeaderToolsInfo", "header_tool_info_notification"), software_lang.TSReadLangs("WallpaperPreviewTool", langKey));
                    }
                    TS_MessageBoxEngine.TS_MessageBox(this, 1, public_message);
                    Application.OpenForms[formName].Activate();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "TSToolLauncher()"); }
            }
        }
        // SFC AND DISM AUTO TOOL
        // ======================================================================================================
        private void SFCandDISMAutoTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowSFCandDISMAutoTool>("glow_sfc_and_dism_tool", "ht_dism_and_sfc_tool");
        }
        // CPU BENCH TOOL
        // ======================================================================================================
        private void BenchCPUTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowBenchCPUTool>("glow_bench_cpu_tool", "ht_bench_cpu");
        }
        // RAM BENCH TOOL
        // ======================================================================================================
        private void BenchRAMTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowBenchMemoryTool>("glow_bench_ram_tool", "ht_bench_ram");
        }
        // DISK BENCH TOOL
        // ======================================================================================================
        private void BenchDiskTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowBenchDiskTool>("glow_bench_disk_tool", "ht_bench_disk");
        }
        // CACHE CLEANUP TOOL
        // ======================================================================================================
        private void CacheCleaningTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowCacheCleanupTool>("glow_cache_cleanup_tool", "ht_cache_cleanup_tool");
        }
        // SYSTEM ID ANALYSIS TOOL
        // ======================================================================================================
        private void SystemIdAnalysisTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowSystemIDAnalysisTool>("glow_system_id_analysis_tool", "ht_system_id_analysis_tool");
        }
        // HARDWARE SECURITY AUDITOR TOOL
        // ======================================================================================================
        private void HsAuditorToolTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowHSAuditorTool>("glow_hardware_security_auditor_tool", "ht_hardware_security_auditor_tool");
        }
        // BLUETOOTH FINDER TOOL
        // ======================================================================================================
        private void BluetoothFinderTool(object sender, EventArgs e)
        {
            TSToolLauncher<GlowBluetoothFinderTool>("glow_bluetooth_finder_tool", "ht_bluetooth_finder_tool");
        }
        // DNS TEST TOOL
        // ======================================================================================================
        private void DnsTestTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowDNSTestTool>("glow_dns_test_tool", "ht_dns_test_tool");
        }
        // NETWORK FIX TOOL
        // ======================================================================================================
        private void NetworkFixTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowNetworkFixTool>("glow_network_fix_tool", "ht_network_fix_tool");
        }
        // SHOW WIFI PASSWORD TOOL
        // ======================================================================================================
        private void ShowWiFiPasswordTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowShowWiFiPasswordTool>("glow_show_wifi_password_tool", "ht_show_wifi_password_tool");
        }
        // PORT SCANNER TOOL
        // ======================================================================================================
        private void PortScannerTool_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowPortScannerTool>("glow_port_scanner_tool", "ht_port_scanner_tool");
        }
        // MONITOR TEST TOOL
        // ======================================================================================================
        private void MonitorDeadPixelTestTool_Click(object sender, EventArgs e)
        {
            monitor_engine_mode = 0;
            MonitorStartEnginePending();
        }
        private void MonitorDynamicRangeTestTool_Click(object sender, EventArgs e)
        {
            monitor_engine_mode = 1;
            MonitorStartEnginePending();
        }
        private void MonitorStartEnginePending()
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                if (monitor_engine_mode == 0)
                {
                    var warningMessage = string.Format(software_lang.TSReadLangs("MonitorTestTool", "mtt_epilepsy_warning"), "\n\n", software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dead_pixel"), "\n\n");
                    DialogResult mse_warning = TS_MessageBoxEngine.TS_MessageBox(this, 6, warningMessage);
                    if (mse_warning == DialogResult.Yes)
                        MonitorStartEngineSelect();
                }
                else
                {
                    MonitorStartEngineSelect();
                }
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "MonitorStartEnginePending()"); }
            }
        }
        private void MonitorStartEngineSelect()
        {
            string glow_tool_name = monitor_engine_mode == 0 ? "glow_monitor_test_engine_dead_pixel" : "glow_monitor_test_engine_dynamic_range";
            string toolKey = monitor_engine_mode == 0 ? "ht_monitor_test_dead_pixel" : "ht_monitor_test_dynamic_range";
            TSToolLauncher<GlowMonitorTestTool>(glow_tool_name, toolKey);
        }
        // MONITOR STUCK PIXEL FIXER TOOL
        // ======================================================================================================
        private void MonitorStuckPixelFixerTool(object sender, EventArgs e)
        {
            try
            {
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                DialogResult mspf_open_warning = TS_MessageBoxEngine.TS_MessageBox(this, 5, string.Format(software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_open_warning"), "\n\n", "\n\n", "\n\n"));
                if (mspf_open_warning == DialogResult.Yes)
                    TSToolLauncher<GlowStuckPixelFixerTool>("glow_monitor_stuck_pixel_fixer", "ht_stuck_pixel_fixer_tool");
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "MonitorStuckPixelFixerTool()"); }
            }
        }
        // DONATE LINK
        // ======================================================================================================
        private void DonateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(TS_LinkSystem.ts_donate) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                if (debug_status) { TSErrorLog.LogException(ex, "DonateToolStripMenuItem_Click()"); }
            }
        }
        // ABOUT
        // ======================================================================================================
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSToolLauncher<GlowAbout>("glow_about", "header_menu_about");
        }
        // EXIT
        // ======================================================================================================
        private void CancelAllTasks()
        {
            try
            {
                if (debug_status && !TSLogger.IsClosed)
                {
                    TSLogger.Log("<---------------------------->");
                    TSLogger.Log("<--- ALL TASKS TERMINATED --->");
                    TSLogger.Log("<---------------------------->");
                }
            }
            catch (Exception ex)
            {
                if (debug_status && !this.IsDisposed && !TSLogger.IsClosed)
                {
                    TSErrorLog.LogException(ex, "CancelAllTasks()");
                }
            }
        }
        private bool isExiting = false;
        private void Software_exit()
        {
            if (isExiting) return;
            isExiting = true;
            loop_status = false;
            try { _shutdownCts.Cancel(); } catch { }
            try { SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged; } catch { }
            try { if (_sysAccentRetryTimer != null) { _sysAccentRetryTimer.Stop(); _sysAccentRetryTimer.Dispose(); _sysAccentRetryTimer = null; } } catch { }
            CancelAllTasks();
            if (debug_status)
            {
                TSLogger.Log("<--- THE APPLICATION HAS BEEN SUCCESSFULLY CLOSED --->");
            }
            TSLogger.Close();
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }
        }
        private async Task ShutdownAndExitAsync()
        {
            if (_shutdownStarted) return;
            _shutdownStarted = true;
            bool toolsWereOpen = false;
            try
            {
                for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
                {
                    Form f = null;
                    try { f = Application.OpenForms[i]; } catch { }
                    if (f != null && f != this && !f.IsDisposed && !f.Disposing)
                    {
                        toolsWereOpen = true;
                        try { f.Close(); } catch { }
                    }
                }
            }
            catch { }
            loop_status = false;
            try { _shutdownCts.Cancel(); } catch { }
            try { Enabled = false; } catch { }
            try
            {
                TSGetLangs closing_lang = new TSGetLangs(lang_path);
                string closingText = closing_lang.TSReadLangs("ContentCrossFeature", "ccf_closing");
                if (string.IsNullOrWhiteSpace(closingText)) closingText = "Closing...";
                Text = TS_VersionEngine.TS_SoftwareVersion(0) + " - " + closingText;
                Cursor = Cursors.WaitCursor;
                HeaderMenu.Cursor = Cursors.WaitCursor;
            }
            catch { }
            try
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline)
                {
                    Task[] snapshot;
                    lock (_bgTasksLock) { snapshot = _bgTasks.ToArray(); }
                    Task[] pending = snapshot.Where(t => t != null && !t.IsCompleted).ToArray();
                    if (pending.Length == 0) break;
                    await Task.WhenAny(Task.WhenAll(pending), Task.Delay(500));
                }
            }
            catch { }
            try
            {
                if (toolsWereOpen) await Task.Delay(1000);
            }
            catch { }
            Software_exit();
        }
        private void Glow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CPUbenchMode || DISKbenchMode || RAMbenchMode || BTfinderMode || SFCandDISMprocessStatus || PortScannerMode || DNSTestMode)
            {
                e.Cancel = true;
                TSGetLangs software_lang = new TSGetLangs(lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("GToolsMessage", "gtm_all_prs_msg"));
            }
            else if (!isExiting)
            {
                e.Cancel = true;
                _ = ShutdownAndExitAsync();
            }
        }
    }
}
