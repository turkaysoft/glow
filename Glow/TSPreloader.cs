using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
// TS Modules
using static Glow.TSModules;

namespace Glow{
    public partial class TSPreloader : Form{
        // VARIABLES
        // ======================================================================================================
        private string load_text;
        public TSPreloader(){
            InitializeComponent();
            //
            LabelDeveloper.Text = Application.CompanyName;
            LabelSoftware.Text = Application.ProductName;
            LabelVersion.Text = TS_VersionEngine.TS_SofwareVersion(1);
            LabelCopyright.Text = TS_SoftwareCopyrightDate.ts_scd_preloader;
            //
            PanelImg.Padding = new Padding(0, 0, 0, 0);
            ImageWelcome.BackgroundImage = Properties.Resources.ts_preloader;
            ImageWelcome.BackgroundImageLayout = ImageLayout.Zoom;
            //
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, PanelLoaderFE, new object[] { true });
        }
        // ENABLE EDGE WHEN BORDER IS CLOSED FOR WINDOWS 11
        // ======================================================================================================
        protected override void OnHandleCreated(EventArgs e){
            base.OnHandleCreated(e);
            if (Program.windows_mode == 1){
                int preference = (int)DWM_WINDOW_CORNER_PREFERENCE.Round;
                DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
        }
        // LOAD
        // ======================================================================================================
        private async void TSPreloader_Load(object sender, EventArgs e){
            if (!SoftwarePreloader()){
                Application.Exit();
                return;
            }
            SoftwareSetLaunch();
            if (Program.ts_pre_debug_mode == true){
                LabelLoader.Text = "Loading - 50%";
                PanelLoaderFE.Width = (int)(PanelLoaderBG.Width * 0.5);
            }else{
                await LoadBootstrapper();
            }
        }
        // SOFTWARE PRELOADER
        // ======================================================================================================
        private bool SoftwarePreloader(){
            try{
                // CHECK LANGS FOLDER
                if (!Directory.Exists(ts_lf)){
                    SoftwarePrelaoderAlert(0);
                    return false;
                }
                // CHECK LANGS FILE
                var lang_files = Directory.GetFiles(ts_lf, "*.ini");
                if (lang_files.Length == 0){
                    SoftwarePrelaoderAlert(1);
                    return false;
                }
                // CHECK ENGLISH LANG FILE
                if (!File.Exists(ts_lang_en)){
                    SoftwarePrelaoderAlert(2);
                    return false;
                }
                // Ensure settings file exists and add missing keys (file may or may not exist)
                EnsureSettingsFileAndSchema();
                return true;
            }catch (IOException ioEx){
                LogError(ioEx);
                return false;
            }catch (UnauthorizedAccessException uaEx){
                LogError(uaEx);
                return false;
            }catch (Exception ex){
                LogError(ex);
                return false;
            }
        }
        // Ensure the configuration file exists and complete the necessary configuration keys.
        // ======================================================================================================
        private void EnsureSettingsFileAndSchema(){
            if (!File.Exists(ts_sf)){
                var dir = Path.GetDirectoryName(ts_sf);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(ts_sf, string.Empty);
            }
            string uiLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.Trim();
            TSSettingsModule settings = new TSSettingsModule(ts_sf);
            var defaults = GetDefaultSettings(uiLang).ToList();
            foreach (var (key, valueFactory) in defaults){
                EnsureSettingKey(settings, ts_settings_container, key, valueFactory());
            }
            settings.TSOrderSectionKeys(ts_settings_container, defaults.Select(x => x.Key));
        }
        // Make sure the adjustment key is present
        // ======================================================================================================
        private void EnsureSettingKey(TSSettingsModule settings, string section, string key, string defaultValue){
            try{
                string current = settings.TSReadSettings(section, key);
                if (string.IsNullOrWhiteSpace(current)){
                    settings.TSWriteSettings(section, key, defaultValue);
                }
            }catch (Exception ex){
                LogError(ex);
            }
        }
        // DEFAULT SETTINGS
        // ======================================================================================================
        /*
            -- ThemeStatus
            ---------------------------
            0 = Dark Theme
            1 = Light Theme
            2 = System Theme

            -- LanguageStatus
            ---------------------------
            Moved to TSModules.cs

            -- StartupStatus
            ---------------------------
            1 = Full Screen
            0 = Windowed

            -- HidingStatus
            ---------------------------
            1 = On
            0 = Off

            -- DebugMode
            ---------------------------
            1 = On
            0 = Off
        */
        private IEnumerable<(string Key, Func<string> ValueFactory)> GetDefaultSettings(string uiLang){
            // GLOBAL
            yield return ("ThemeStatus", () => Convert.ToString(TSThemeModeHelper.GetSystemTheme(2)));
            yield return ("LanguageStatus", () => TSPreloaderSetDefaultLanguage(uiLang));
            yield return ("StartupStatus", () => "0");
            // APPLICATION SPECIFIC
            yield return ("HidingStatus", () => "0");
            yield return ("DebugMode", () => "1");
        }
        // PRELOAD ALERT
        // ======================================================================================================
        private void SoftwarePrelaoderAlert(int pre_mode){
            string set_message = string.Empty;
            switch (pre_mode){
                case 0:
                    set_message = $"{ts_lf} folder not found.\nThe folder seems to be missing.\n\nWould you like to view and download the latest version of {Application.ProductName} again?";
                    break;
                case 1:
                    set_message = $"No language file found.\nThere seems to be a problem with the language files.\n\nWould you like to view and download the latest version of {Application.ProductName} again?";
                    break;
                case 2:
                    set_message = $"English language (English.ini) is a compulsory language. The English.ini file seems to be missing.\n\nWould you like to view and download the latest version of {Application.ProductName} again?";
                    break;
            }
            if (!string.IsNullOrEmpty(set_message)){
                DialogResult open_last_release = TS_MessageBoxEngine.TS_MessageBox(this, 7, set_message);
                if (open_last_release == DialogResult.Yes){
                    Process.Start(new ProcessStartInfo(TS_LinkSystem.github_link_lr){ UseShellExecute = true });
                }else{
                    Application.Exit();
                }
            }
        }
        // ERROR LOG FUNCTION
        // ======================================================================================================
        private void LogError(Exception ex){
            TS_MessageBoxEngine.TS_MessageBox(this, 3, ex.Message);
        }
        // BOOTSTRAPPER PRELOADER
        // ======================================================================================================
        public void SoftwareSetLaunch(){
            try{
                TSSettingsModule software_read_settings = new TSSettingsModule(ts_sf);
                //
                int theme_mode = int.TryParse(software_read_settings.TSReadSettings(ts_settings_container, "ThemeStatus"), out int the_status) && (the_status == 0 || the_status == 1 || the_status == 2) ? the_status : 1;
                theme_mode = TSThemeModeHelper.GetSystemTheme(theme_mode);
                //
                BackColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_BGColor");
                PanelTxt.BackColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_BGColor");
                LabelDeveloper.ForeColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_LabelColor1");
                LabelSoftware.ForeColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_AccentColor");
                LabelVersion.ForeColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_LabelColor2");
                PanelLoaderBG.BackColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_BGColor2");
                PanelLoaderFE.BackColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_AccentColor");
                LabelLoader.ForeColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_LabelColor1");
                LabelCopyright.ForeColor = TS_ThemeEngine.ColorMode(theme_mode, "TSBT_LabelColor2");
                // DICTIONARY SYSTEM FOR LANGUAGE
                string lang_mode = software_read_settings.TSReadSettings(ts_settings_container, "LanguageStatus") ?? "en";
                string lang_file;
                bool isFileExist = AllLanguageFiles.ContainsKey(lang_mode) && File.Exists(AllLanguageFiles[lang_mode]);
                //
                if (isFileExist){
                    lang_file = AllLanguageFiles[lang_mode];
                }else{
                    lang_file = AllLanguageFiles.ContainsKey("en") && File.Exists(AllLanguageFiles["en"]) ? AllLanguageFiles["en"] : ts_lang_en;
                    lang_mode = "en";
                }
                //
                try{
                    if (!isFileExist){
                        TSSettingsModule software_setting_save = new TSSettingsModule(ts_sf);
                        software_setting_save.TSWriteSettings(ts_settings_container, "LanguageStatus", lang_mode);
                    }
                }catch (Exception ex){
                    LogError(ex);
                }
                //
                TSGetLangs software_lang = new TSGetLangs(lang_file);
                Text = string.Format(software_lang.TSReadLangs("TSPreloader", "tsbt_title"), Application.CompanyName);
                load_text = software_lang.TSReadLangs("TSPreloader", "tsbt_load");
                // DELETE OLD UPDATER EXE
                try{
                    if (File.Exists(Program.updater_old_exe_name)){
                        File.Delete(Program.updater_old_exe_name);
                    }
                }catch (Exception){ }
            }catch (Exception){ }
        }
        // PROGRESS BAR & PROGRESS TEXT PROCESS
        // ======================================================================================================
        private void TSProgressExecutive(int get_per){
            if (InvokeRequired){
                BeginInvoke(new Action<int>(TSProgressExecutive), get_per);
                return;
            }
            PanelLoaderFE.Width = (int)(PanelLoaderBG.Width * (get_per / 100.0));
            LabelLoader.Text = string.Format("{0} - {1}%", load_text, get_per);
        }
        // LOAD ANIMATION
        // ======================================================================================================
        private async Task LoadBootstrapper(){
            try{
                int progress_interval = 0;
                int progress_increment = 10;
                int progress_delay = 10;
                TSProgressExecutive(0);
                while (progress_interval < 100){
                    progress_interval += progress_increment;
                    if (progress_interval > 100)
                        progress_interval = 100;
                    TSProgressExecutive(progress_interval);
                    await Task.Delay(progress_delay);
                }
            }catch (OperationCanceledException){
                return;
            }
            if (IsDisposed || !IsHandleCreated){
                return;
            }
            var glow = new GlowMain();
            glow.Show();
            Hide();
        }
    }
}