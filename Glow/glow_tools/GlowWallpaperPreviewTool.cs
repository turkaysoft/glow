using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowWallpaperPreviewTool : Form{
        public GlowWallpaperPreviewTool(){
            InitializeComponent();
            //
            GTool_WP_Preloader();
            //
            ImageDGV.ColumnHeadersVisible = false;
            ImageDGV.Columns.Add("WallpaperPref", "Pref");
            ImageDGV.Columns.Add("WallpaperVal", "Value");
            ImageDGV.Columns[0].Width = (int)(175 * this.DeviceDpi / 96f);
            ImageDGV.RowTemplate.Height = (int)(32 * this.DeviceDpi / 96f);
            foreach (DataGridViewColumn columnPadding in ImageDGV.Columns){
                int scaledPadding = (int)(3 * this.DeviceDpi / 96f);
                columnPadding.DefaultCellStyle.Padding = new Padding(scaledPadding, 0, 0, 0);
            }
        }
        // THEME SETTINGS
        // ======================================================================================================
        public void GTool_WP_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                ImageDGV.EnableHeadersVisualStyles = false;
                ImageDGV.BackgroundColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                ImageDGV.GridColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "SelectBoxBorderColor");
                ImageDGV.DefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                ImageDGV.DefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                ImageDGV.AlternatingRowsDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                ImageDGV.ColumnHeadersDefaultCellStyle.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                ImageDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                ImageDGV.ColumnHeadersDefaultCellStyle.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                ImageDGV.DefaultCellStyle.SelectionBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                ImageDGV.DefaultCellStyle.SelectionForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                ImageDGV.ReadOnly = true;
                //
                foreach (Control ui_buttons in BackPanel.Controls){
                    if (ui_buttons is Button open_btn){
                        open_btn.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                        open_btn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        open_btn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        open_btn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_AccentColor");
                        open_btn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "AccentColorHover");
                    }
                }
                //
                TSImageRenderer(BtnWallpaperLocationBtn, GlowMain.theme == 1 ? Properties.Resources.ct_link_mc_light : Properties.Resources.ct_link_mc_dark, 18, ContentAlignment.MiddleRight);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_title"), Application.ProductName);
                BtnWallpaperLocationBtn.Text = " " + software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_btn");
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_WP_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowWallpaperPreviewTool_Load(object sender, EventArgs e){
            try{
                Task.Run(AsyncLoadWallpaper);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowWallpaperPreviewTool_Load()"); }
            }
        }
        private void AsyncLoadWallpaper(){
            try{
                if (string.IsNullOrWhiteSpace(GlowMain.wp_rotate) || !File.Exists(GlowMain.wp_rotate)){
                    if (!IsDisposed && IsHandleCreated){
                        BeginInvoke(new Action(() => {
                            if (IsDisposed || !IsHandleCreated) return;
                            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                            TS_MessageBoxEngine.TS_MessageBox(this, 2, software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_open_error"));
                            Close();
                        }));
                    }
                    return;
                }
                GetWallpaperInfo(GlowMain.wp_rotate);
                byte[] bytes = File.ReadAllBytes(GlowMain.wp_rotate);
                Image img;
                using (var ms = new MemoryStream(bytes))
                using (var temp = Image.FromStream(ms)){
                    img = new Bitmap(temp);
                }
                if (WPImage.IsHandleCreated && !WPImage.IsDisposed){
                    WPImage.BeginInvoke(new Action(() =>{
                        try{
                            if (WPImage.IsDisposed || !WPImage.IsHandleCreated){ img.Dispose(); return; }
                            SetPictureBoxImage(WPImage, img);
                        }finally{
                            try{ img.Dispose(); }catch { }
                        }
                    }));
                }else{
                    img.Dispose();
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "AsyncLoadWallpaper()"); }
            }
        }
        private void GetWallpaperInfo(string path){
            if (!File.Exists(path)) return;
            string res = "-";
            try{
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs, false, false)){
                    res = $"{img.Width}x{img.Height}";
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GetWallpaperInfo()"); }
            }
            if (ImageDGV.IsDisposed) return;
            Action addRows = () => AddRowsToDGV(new FileInfo(path), res);
            if (ImageDGV.InvokeRequired)
                ImageDGV.BeginInvoke(addRows);
            else
                addRows();
        }
        private void AddRowsToDGV(FileInfo fi, string img_res){
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_file_name"), Path.GetFileNameWithoutExtension(fi.FullName));
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_extension"), fi.Extension);
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_resolution"), img_res);
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_size"), TS_FormatSize(fi.Length));
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_file_location"), fi.DirectoryName);
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_creation_date"), fi.CreationTime.ToString());
            ImageDGV.Rows.Add(software_lang.TSReadLangs("WallpaperPreviewTool", "wpt_change_date"), fi.LastWriteTime.ToString());
        }
        // OPEN WALLPAPER TO EXPLORER
        // ======================================================================================================
        private void BtnWallpaperLocationBtn_Click(object sender, EventArgs e){
            try{
                string wallpaper_start_path = string.Format("/select, \"{0}\"", GlowMain.wp_rotate.Trim().Replace("/", @"\"));
                ProcessStartInfo psi = new ProcessStartInfo("explorer.exe", wallpaper_start_path);
                Process.Start(psi);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "BtnWallpaperLocationBtn_Click()"); }
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                TS_MessageBoxEngine.TS_MessageBox(this, 3, software_lang.TSReadLangs("Os_Content", "os_c_wallpaper_open_error"));
            }
        }
        // CLEAR SELECTION
        // ======================================================================================================
        private void ImageDGV_SelectionChanged(object sender, EventArgs e){
            ImageDGV.ClearSelection();
        }
        // DISPOSE AND EXIT
        // ======================================================================================================
        private void GlowWallpaperPreview_FormClosing(object sender, FormClosingEventArgs e){
            SetPictureBoxImage(WPImage, null);
        }
    }
}