using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowMonitorTestTool : Form{
        public GlowMonitorTestTool() { InitializeComponent(); this.ResizeRedraw = true; this.KeyPreview = true; }
        // VARIABLES
        // ======================================================================================================
        private readonly Color[] dead_pixel_colors = { Color.Black, Color.White, Color.Red, Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255) };
        private readonly Color[] dynamic_range_colors = { Color.FromArgb(34, 34, 34), Color.FromArgb(85, 85, 85), Color.White, Color.Red, Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255) };
        //
        int size_mode;
        private bool message_disposed = false;        //
        private int dead_pixel_index = 0;
        private bool dead_pixel_index_changed = false;
        private bool dead_pixel_pause_toggle = false;
        private readonly object dead_pixel_lockobj = new object();
        private Thread dead_pixel_colorChangingThread;
        private readonly int dead_pixel_color_change_interval = 4750; // ms
        private volatile bool dead_pixel_test_status = true;
        //
        private int Dynamic_range_color_count => dynamic_range_colors.Length;
        private const int dynamic_range_shade_count = 15;
        private int Dynamic_range_box_count => dynamic_range_shade_count * Dynamic_range_color_count;
        private Panel[] dynamic_range_boxs;
        //
        // RESIZE PERF
        // ======================================================================================================
        private System.Windows.Forms.Timer dynamic_range_resizeTimer;
        private int lastW = -1, lastH = -1;
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_MonitorTest_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                //
                InfoLabel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                InfoLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                if (GlowMain.monitor_engine_mode == 0){
                    Text = string.Format(software_lang.TSReadLangs("MonitorTestTool", "mtt_title"), Application.ProductName, software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dead_pixel"));
                }else if (GlowMain.monitor_engine_mode == 1){
                    BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                    Text = string.Format(software_lang.TSReadLangs("MonitorTestTool", "mtt_title"), Application.ProductName, software_lang.TSReadLangs("HeaderTools", "ht_monitor_test_dynamic_range"));
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_MonitorTest_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowMonitorTestTool_Load(object sender, EventArgs e){
            try{
                GTool_MonitorTest_Preloader();
                Monitor_test_loader(GlowMain.monitor_engine_mode);
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowMonitorTestTool_Load()"); }
            }
        }
        // MONITOR TEST LOADER
        // ======================================================================================================
        private async void Monitor_test_loader(int monitor_test_mode){
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            if (monitor_test_mode == 0){
                InfoLabel.Text = string.Format(software_lang.TSReadLangs("MonitorTestTool", "mtt_esc_info_dead_pixel"), "\n");
                Monitor_test_dead_pixel();
            }else if (monitor_test_mode == 1){
                InfoLabel.Text = software_lang.TSReadLangs("MonitorTestTool", "mtt_esc_info_dynamic_range");
                Monitor_dynamic_range_test();
            }
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            size_mode = 1;
            //
            await Task.Run(() => Message_dispose());
        }
        private void InfoLabel_Click(object sender, EventArgs e){
            DisposeLabel();
        }
        private async void Message_dispose(){
            try{
                await Task.Delay(7000);
                if (IsDisposed || !IsHandleCreated) return;
                if (InvokeRequired){
                    BeginInvoke(new MethodInvoker(DisposeLabel));
                }else{
                    DisposeLabel();
                }
            }catch { }
        }
        private void DisposeLabel(){
            if (!message_disposed && InfoLabel != null && !InfoLabel.IsDisposed){
                message_disposed = true;
                InfoLabel.Visible = false;
                InfoLabel.Enabled = false;
            }
        }
        // DEAD PIXEL TEST
        // ======================================================================================================
        private void Monitor_test_dead_pixel(){
            dead_pixel_colorChangingThread = new Thread(() => {
                try{
                    int sleepStep = 50;
                    int elapsed = 0;
                    while (dead_pixel_test_status){
                        if (!dead_pixel_pause_toggle){
                            try{
                                if (!IsDisposed && IsHandleCreated){
                                    BeginInvoke((MethodInvoker)(() => {
                                        UpdateBackgroundColor();
                                    }));
                                }
                            }catch (Exception) { }
                            while (elapsed < dead_pixel_color_change_interval){
                                Thread.Sleep(sleepStep);
                                elapsed += sleepStep;
                                lock (dead_pixel_lockobj){
                                    if (dead_pixel_index_changed){
                                        elapsed = 0;
                                        dead_pixel_index_changed = false;
                                    }
                                }
                                while (dead_pixel_pause_toggle && dead_pixel_test_status){
                                    Thread.Sleep(100);
                                }
                                if (!dead_pixel_test_status)
                                    return;
                            }
                            lock (dead_pixel_lockobj){
                                dead_pixel_index = (dead_pixel_index + 1) % dead_pixel_colors.Length;
                                elapsed = 0;
                            }
                        }else{
                            Thread.Sleep(100);
                        }
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Monitor_test_dead_pixel()"); }
                }
            }){ IsBackground = true };
            dead_pixel_colorChangingThread.Start();
            MouseDown -= MouseDown_Handler;
            MouseDown += MouseDown_Handler;
            KeyDown -= KeyDown_Handler;
            KeyDown += KeyDown_Handler;
        }
        private void MouseDown_Handler(object sender, MouseEventArgs e){
            if (e.Button == MouseButtons.Left){
                lock (dead_pixel_lockobj){
                    dead_pixel_index = (dead_pixel_index + 1) % dead_pixel_colors.Length;
                    dead_pixel_index_changed = true;
                }
                try{
                    if (!IsDisposed && IsHandleCreated){
                        BeginInvoke((MethodInvoker)(() => {
                            UpdateBackgroundColor();
                        }));
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "MouseDown_Handler()"); }
                }
            }
        }
        private void KeyDown_Handler(object sender, KeyEventArgs e){
            if (e.KeyCode == Keys.Space){
                dead_pixel_pause_toggle = !dead_pixel_pause_toggle;
                if (!dead_pixel_pause_toggle){
                    dead_pixel_index_changed = true;
                }
            }else if (e.KeyCode == Keys.Left){
                lock (dead_pixel_lockobj){
                    dead_pixel_index = 0;
                    dead_pixel_index_changed = true;
                }
                try{
                    if (!IsDisposed && IsHandleCreated){
                        BeginInvoke((MethodInvoker)(() => {
                            UpdateBackgroundColor();
                        }));
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "KeyDown_Handler()"); }
                }
            }
        }
        private void UpdateBackgroundColor(){
            try{
                if (InvokeRequired){
                    BeginInvoke(new MethodInvoker(delegate { UpdateBackgroundColor(); }));
                }else{
                    BackColor = dead_pixel_colors[dead_pixel_index];
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "UpdateBackgroundColor()"); }
            }
        }
        // DYNAMIC RANGE TEST
        // ======================================================================================================
        private void Monitor_dynamic_range_test(){
            try{
                dynamic_range_boxs = new Panel[Dynamic_range_box_count];
                for (int colorIndex = 0; colorIndex < Dynamic_range_color_count; colorIndex++){
                    for (int shadeIndex = 0; shadeIndex < dynamic_range_shade_count; shadeIndex++){
                        int index = colorIndex * dynamic_range_shade_count + shadeIndex;
                        dynamic_range_boxs[index] = new Panel{
                            BackColor = ShadeGenerator(dynamic_range_colors[colorIndex], (double)shadeIndex / (dynamic_range_shade_count - 1)),
                            BorderStyle = BorderStyle.None
                        };
                        Controls.Add(dynamic_range_boxs[index]);
                    }
                }
                // RESIZE PERF INIT
                // ======================================================================================================
                if (dynamic_range_resizeTimer == null){
                    dynamic_range_resizeTimer = new System.Windows.Forms.Timer{
                        Interval = 50
                    };
                    dynamic_range_resizeTimer.Tick += (s, e) => {
                        try{
                            dynamic_range_resizeTimer.Stop();
                            Monitor_dynamic_range_box_resize();
                        }catch (Exception) { }
                    };
                }
                Monitor_dynamic_range_box_resize();
                if (InfoLabel != null && !InfoLabel.IsDisposed)
                    InfoLabel.BringToFront();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Monitor_dynamic_range_test()"); }
            }
        }
        private void Monitor_dynamic_range_box_resize(){
            try{
                if (dynamic_range_boxs == null || dynamic_range_boxs.Length != Dynamic_range_box_count)
                    return;
                if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                    return;
                this.SuspendLayout();
                try{
                    int totalWidth = ClientSize.Width;
                    int totalHeight = ClientSize.Height;
                    int boxWidth = (int)Math.Ceiling((double)totalWidth / Dynamic_range_color_count);
                    int boxHeight = (int)Math.Ceiling((double)totalHeight / dynamic_range_shade_count);
                    for (int colorIndex = 0; colorIndex < Dynamic_range_color_count; colorIndex++){
                        for (int shadeIndex = 0; shadeIndex < dynamic_range_shade_count; shadeIndex++){
                            int index = colorIndex * dynamic_range_shade_count + shadeIndex;
                            if (index >= 0 && index < dynamic_range_boxs.Length && dynamic_range_boxs[index] != null && !dynamic_range_boxs[index].IsDisposed){
                                if (dynamic_range_boxs[index].Width != boxWidth || dynamic_range_boxs[index].Height != boxHeight)
                                    dynamic_range_boxs[index].Size = new Size(boxWidth, boxHeight);
                                Point target = new Point(colorIndex * boxWidth, shadeIndex * boxHeight);
                                if (dynamic_range_boxs[index].Location != target)
                                    dynamic_range_boxs[index].Location = target;
                            }
                        }
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Monitor_dynamic_range_box_resize()"); }
                }finally{
                    try{ this.ResumeLayout(false); }catch { }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "Monitor_dynamic_range_box_resize()"); }
            }
        }
        private Color ShadeGenerator(Color mainColor, double shadeRatio){
            double gamma = 2.2;
            double factor = Math.Pow(shadeRatio, gamma);
            int red = (int)(mainColor.R * factor);
            int green = (int)(mainColor.G * factor);
            int blue = (int)(mainColor.B * factor);
            return Color.FromArgb(Clamp(red), Clamp(green), Clamp(blue));
        }
        private int Clamp(int value) => Math.Max(0, Math.Min(255, value));
        // RESIZE
        // ======================================================================================================
        private void GlowMonitorTestEngine_Resize(object sender, EventArgs e){
            if (GlowMain.monitor_engine_mode == 1){
                try{
                    if (ClientSize.Width == lastW && ClientSize.Height == lastH)
                        return;
                    lastW = ClientSize.Width;
                    lastH = ClientSize.Height;
                    if (dynamic_range_resizeTimer != null){
                        dynamic_range_resizeTimer.Stop();
                        dynamic_range_resizeTimer.Start();
                    }else{
                        Monitor_dynamic_range_box_resize();
                    }
                }catch (Exception ex){
                    if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowMonitorTestEngine_Resize()"); }
                }
            }
        }
        // FULLSCREEN TO NORMAL
        // ======================================================================================================
        private void GlowMonitorTestEngine_KeyDown(object sender, KeyEventArgs e){
            if (e.KeyCode == Keys.F12 || e.KeyCode == Keys.Escape){
                if (size_mode == 1){
                    WindowState = FormWindowState.Normal;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    this.Icon = Properties.Resources.GlowLogo;
                    this.ShowIcon = true;
                    size_mode = 0;
                }else{
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                    size_mode = 1;
                }
            }
        }
        // EXIT
        // ======================================================================================================
        private void GlowMonitorTestEngine_FormClosing(object sender, FormClosingEventArgs e){
            dead_pixel_test_status = false;
            try{
                if (dynamic_range_resizeTimer != null){
                    dynamic_range_resizeTimer.Stop();
                    dynamic_range_resizeTimer.Dispose();
                    dynamic_range_resizeTimer = null;
                }
            }catch (Exception) { }
            if (dead_pixel_colorChangingThread != null && dead_pixel_colorChangingThread.IsAlive){
                dead_pixel_colorChangingThread.Join(250);
            }
        }
    }
}