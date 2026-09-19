using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
// TS MODULES
using static Glow.TSModules;

namespace Glow.glow_tools{
    public partial class GlowStuckPixelFixerTool : Form{
        // DYNAMIC VARIABLES
        // ======================================================================================================
        public string testFieldCountMaxText;
        public GlowStuckPixelFixerTool(){
            InitializeComponent();
            //
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }
        // PRE-LOAD
        // ======================================================================================================
        public void GTool_StuckPixel_Preloader(){
            try{
                TSThemeModeHelper.InitializeThemeForForm(this);
                // TEXT
                // ----------------------
                TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
                Text = string.Format(software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "ht_stuck_pixel_fixer_tool"), Application.ProductName);
                testFieldCountMaxText = software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_max_field_warning");
                // THEME
                MainToolTip.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                MainToolTip.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                //
                foreach (Control ctrl in this.Controls){
                    if (ctrl is TSStuckPixelTest testBox){
                        testBox.headerPanel.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                        testBox.timeLabel.ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1");
                        foreach (Control child in testBox.headerPanel.Controls){
                            if (child is Button btn){
                                btn.BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                                btn.FlatAppearance.BorderColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor");
                                btn.FlatAppearance.MouseDownBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                                btn.FlatAppearance.MouseOverBackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor2");
                            }
                        }
                        TSImageRenderer(testBox.colorButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_color_light : Properties.Resources.ct_spf_color_dark, 12, ContentAlignment.MiddleCenter);
                        TSImageRenderer(testBox.swapperButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_toggle_light : Properties.Resources.ct_spf_toggle_dark, 12, ContentAlignment.MiddleCenter);
                        TSImageRenderer(testBox.addButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_add_light : Properties.Resources.ct_spf_add_dark, 12, ContentAlignment.MiddleCenter);
                        TSImageRenderer(testBox.closeButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_close_light : Properties.Resources.ct_spf_close_dark, 14, ContentAlignment.MiddleCenter);
                        //
                        MainToolTip.SetToolTip(testBox.colorButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_color"));
                        MainToolTip.SetToolTip(testBox.swapperButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_swapper"));
                        MainToolTip.SetToolTip(testBox.addButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_generate"));
                        MainToolTip.SetToolTip(testBox.closeButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_close_generate"));
                    }
                }
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GTool_StuckPixel_Preloader()"); }
            }
        }
        // LOAD
        // ======================================================================================================
        private void GlowStuckPixelFixerTool_Load(object sender, EventArgs e){
            try{
                float scaleFactor = this.DeviceDpi / 96f;
                AddNewTestBox((Screen.PrimaryScreen.Bounds.Width - (int)(225 * scaleFactor)) / 2, (Screen.PrimaryScreen.Bounds.Height - (int)(200 * scaleFactor)) / 2);
                GTool_StuckPixel_Preloader();
            }catch (Exception ex){
                if (GlowMain.debug_status) { TSErrorLog.LogException(ex, "GlowStuckPixelFixerTool_Load()"); }
            }
        }
        // ADD NEW TEST BOX
        // ======================================================================================================
        public void AddNewTestBox(int x, int y){
            var box = new TSStuckPixelTest(this){
                Location = new Point(x, y)
            };
            this.Controls.Add(box);
            box.BringToFront();
        }
        // CHECK CONTROLS
        // ======================================================================================================
        public bool HasAnyTestBox(){
            foreach (Control ctrl in this.Controls){
                if (ctrl is TSStuckPixelTest) return true;
            }
            return false;
        }
        // TOOLTIP RE RENDER
        // ======================================================================================================
        private void MainToolTip_Draw(object sender, DrawToolTipEventArgs e){ e.DrawBackground(); e.DrawBorder(); e.DrawText(); }
    }
    public class TSStuckPixelTest : Panel{
        // VARIABLES
        // ----------------------------
        public readonly Panel headerPanel;
        public readonly Label timeLabel;
        public readonly Button colorButton;
        public readonly Button swapperButton;
        public readonly Button closeButton;
        public readonly Button addButton;
        //
        private readonly Stopwatch stopwatch;
        private readonly GlowStuckPixelFixerTool parentForm;
        private readonly Random random;
        //
        private bool dragging = false;
        private Point dragStart;
        //
        private bool resizingRight = false;
        private bool resizingBottom = false;
        private bool resizingCorner = false;
        private Point resizeStart;
        private Size originalSize;
        private byte[] randomBuffer;
        //
        private Bitmap frame;
        private readonly object frameLock = new object();
        private Rectangle cachedArea = Rectangle.Empty;
        readonly int fieldLimit = 16;
        //
        private readonly System.Timers.Timer elapsedTimer;
        private readonly CancellationTokenSource renderCts;
        private readonly Task renderTask;
        //
        public TSStuckPixelTest(GlowStuckPixelFixerTool parent){
            // PREVENT SET
            // ----------------------------
            parentForm = parent;
            float scaleFactor = this.DeviceDpi / 96f;
            this.Size = new Size(
                (int)(225 * scaleFactor),
                (int)(200 * scaleFactor)
            );
            this.BackColor = Color.Black;
            // DOUBLE BUFFERING
            // ----------------------------
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            // HEADER PANEL CREATE
            // ----------------------------
            headerPanel = new Panel{
                Dock = DockStyle.Top,
                Height = (int)(25 * scaleFactor),
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor"),
                Cursor = Cursors.SizeAll
            };
            // TIMER LABEL
            // ----------------------------
            timeLabel = new Label{
                Text = "00:00:00",
                ForeColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_LabelColor1"),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Left,
                Padding = new Padding(2, 0, 0, 0)
            };
            // COLOR BUTTON
            // ----------------------------
            colorButton = new Button{
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor"),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = (int)(25 * scaleFactor)
            };
            colorButton.FlatAppearance.BorderSize = 0;
            colorButton.Click += (s, e) =>{
                using (ColorDialog colorDialog = new ColorDialog()){
                    colorDialog.AllowFullOpen = false;
                    colorDialog.FullOpen = false;
                    colorDialog.AnyColor = true;
                    colorDialog.SolidColorOnly = true;
                    colorDialog.Color = parentForm.BackColor;
                    Color[] baseColors = new Color[]{
                        Color.Black,
                        Color.White,
                        Color.Red,
                        Color.Green,
                        Color.Blue,
                        Color.Orange,
                        Color.Yellow,
                        Color.Purple
                    };
                    Color[] lighterColors = baseColors.Select(c => ControlPaint.Light(c, 0.5f)).ToArray();
                    colorDialog.CustomColors = baseColors.Concat(lighterColors).Select(c => ColorTranslator.ToOle(c)).ToArray();
                    if (colorDialog.ShowDialog() == DialogResult.OK){
                        parentForm.BackColor = colorDialog.Color;
                    }
                }
            };
            // TOGGLE BUTTON
            // ----------------------------
            swapperButton = new Button{
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor"),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = (int)(25 * scaleFactor)
            };
            swapperButton.FlatAppearance.BorderSize = 0;
            swapperButton.Click += (s, e) => {
                if (headerPanel.Dock == DockStyle.Top)
                    headerPanel.Dock = DockStyle.Bottom;
                else
                    headerPanel.Dock = DockStyle.Top;
                UpdateCachedArea();
            };
            // ADD
            // ----------------------------
            addButton = new Button{
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor"),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = (int)(25 * scaleFactor)
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) => {
                int currentCount = parentForm.Controls.OfType<TSStuckPixelTest>().Count();
                if (currentCount >= fieldLimit){
                    TS_MessageBoxEngine.TS_MessageBox(parentForm, 2, string.Format(parentForm.testFieldCountMaxText, fieldLimit, "\n\n"));
                    return;
                }
                parentForm.AddNewTestBox(this.Left + (int)(25 * scaleFactor), this.Top + (int)(25 * scaleFactor));
            };
            // CLOSE
            // ----------------------------
            closeButton = new Button{
                BackColor = TS_ThemeEngine.ColorMode(GlowMain.theme, "TSBT_BGColor"),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right,
                Width = (int)(25 * scaleFactor)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => {
                this.Dispose();
                if (!parentForm.HasAnyTestBox())
                    parentForm.Close();
            };
            // CONTROLS ADD
            // ----------------------------
            headerPanel.Controls.Add(timeLabel);
            headerPanel.Controls.Add(colorButton);
            headerPanel.Controls.Add(swapperButton);
            headerPanel.Controls.Add(addButton);
            headerPanel.Controls.Add(closeButton);
            // SET IMAGE BUTTON
            // ----------------------------
            TSImageRenderer(colorButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_color_light : Properties.Resources.ct_spf_color_dark, 12, ContentAlignment.MiddleCenter);
            TSImageRenderer(swapperButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_toggle_light : Properties.Resources.ct_spf_toggle_dark, 12, ContentAlignment.MiddleCenter);
            TSImageRenderer(addButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_add_light : Properties.Resources.ct_spf_add_dark, 12, ContentAlignment.MiddleCenter);
            TSImageRenderer(closeButton, GlowMain.theme == 0 ? Properties.Resources.ct_spf_close_light : Properties.Resources.ct_spf_close_dark, 14, ContentAlignment.MiddleCenter);
            // SET HOVER TEXT
            // ----------------------------
            TSGetLangs software_lang = new TSGetLangs(GlowMain.lang_path);
            parentForm.MainToolTip.SetToolTip(colorButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_color"));
            parentForm.MainToolTip.SetToolTip(swapperButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_swapper"));
            parentForm.MainToolTip.SetToolTip(addButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_new_generate"));
            parentForm.MainToolTip.SetToolTip(closeButton, software_lang.TSReadLangs("MonitorStuckPixelFixerTool", "mspf_close_generate"));
            // RENDER HEADER PANEL
            // ----------------------------
            this.Controls.Add(headerPanel);
            // EVENTS
            // ----------------------------
            headerPanel.MouseDown += Header_MouseDown;
            headerPanel.MouseMove += Header_MouseMove;
            headerPanel.MouseUp += Header_MouseUp;
            timeLabel.MouseDown += Header_MouseDown;
            timeLabel.MouseMove += Header_MouseMove;
            timeLabel.MouseUp += Header_MouseUp;
            this.MouseDown += TestBox_MouseDown;
            this.MouseMove += TestBox_MouseMove;
            this.MouseUp += TestBox_MouseUp;
            // TIMER LAUNCH
            // ----------------------------
            random = new Random();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            elapsedTimer = new System.Timers.Timer(1000);
            elapsedTimer.Elapsed += (s, e) => {
                try{
                    if (timeLabel.IsDisposed || !timeLabel.IsHandleCreated)
                        return;
                    timeLabel.BeginInvoke((Action)(() => {
                        if (timeLabel.IsDisposed)
                            return;
                        timeLabel.Text = stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                    }));
                }catch { }
            };
            elapsedTimer.Start();
            // RUN TEST
            // ----------------------------
            renderCts = new CancellationTokenSource();
            var token = renderCts.Token;
            UpdateCachedArea();
            renderTask = Task.Run(() => {
                Stopwatch frameTimer = Stopwatch.StartNew();
                const double targetFrameTime = 1000.0 / 60; // 60 FPS
                while (!token.IsCancellationRequested && !this.IsDisposed){
                    try{ UpdateFrame(); }catch { }
                    try{
                        if (this.IsHandleCreated && !this.IsDisposed){
                            this.BeginInvoke(new Action(() => {
                                if (this.IsDisposed || !this.IsHandleCreated)
                                    return;
                                UpdateCachedArea();
                                Rectangle area;
                                lock (frameLock){ area = cachedArea; }
                                if (area.Width > 0 && area.Height > 0)
                                    this.Invalidate(area);
                            }));
                        }
                    }catch { }
                    double elapsed = frameTimer.Elapsed.TotalMilliseconds;
                    double sleepTime = targetFrameTime - elapsed;
                    if (sleepTime > 0)
                        Thread.Sleep((int)sleepTime);
                    frameTimer.Restart();
                }
            }, token);
        }
        // RENDER AREA (UI THREAD ONLY) + UPDATER FRAME (WORKER THREAD)
        // ======================================================================================================
        private void UpdateCachedArea(){
            try{
                int topOffset = headerPanel.Dock == DockStyle.Top ? headerPanel.Height : 0;
                int bottomOffset = headerPanel.Dock == DockStyle.Bottom ? headerPanel.Height : 0;
                Rectangle area = new Rectangle(0, topOffset, this.Width, this.Height - topOffset - bottomOffset);
                lock (frameLock){
                    cachedArea = (area.Width > 0 && area.Height > 0) ? area : Rectangle.Empty;
                }
            }catch { }
        }
        private void UpdateFrame(){
            Rectangle area;
            lock (frameLock){ area = cachedArea; }
            if (area.Width <= 0 || area.Height <= 0)
                return;
            lock (frameLock){
                if (frame == null || frame.Width != area.Width || frame.Height != area.Height){
                    frame?.Dispose();
                    frame = new Bitmap(area.Width, area.Height, PixelFormat.Format24bppRgb);
                    randomBuffer = new byte[frame.Width * frame.Height * 3];
                }
                random.NextBytes(randomBuffer);
                BitmapData data = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.WriteOnly, frame.PixelFormat);
                int stride = data.Stride;
                IntPtr scan0 = data.Scan0;
                unsafe{
                    byte* ptr = (byte*)scan0;
                    fixed (byte* bufferPtr = randomBuffer){
                        byte* src = bufferPtr;
                        for (int y = 0; y < frame.Height; y++){
                            byte* row = ptr + (y * stride);
                            Buffer.MemoryCopy(src, row, stride, frame.Width * 3);
                            src += frame.Width * 3;
                        }
                    }
                }
                frame.UnlockBits(data);
            }
        }
        // RENDER FRAME
        // ======================================================================================================
        protected override void OnPaint(PaintEventArgs e){
            base.OnPaint(e);
            int topOffset = headerPanel.Dock == DockStyle.Top ? headerPanel.Height : 0;
            int bottomOffset = headerPanel.Dock == DockStyle.Bottom ? headerPanel.Height : 0;
            Rectangle area = new Rectangle(0, topOffset, this.Width, this.Height - topOffset - bottomOffset);
            lock (frameLock){
                if (frame != null)
                    e.Graphics.DrawImage(frame, area);
            }
        }
        // ALL TEST RENDER CHANGER FUNCTIONS
        // ======================================================================================================
        private void Header_MouseDown(object sender, MouseEventArgs e){
            if (e.Button != MouseButtons.Left) return;
            dragging = true;
            dragStart = e.Location;
            try{ headerPanel.Capture = true; }catch { }
        }
        private void Header_MouseMove(object sender, MouseEventArgs e){
            if (dragging){
                if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.None){
                    dragging = false;
                    try{ headerPanel.Capture = false; }catch { }
                    return;
                }
                var newPos = this.Location;
                newPos.Offset(e.X - dragStart.X, e.Y - dragStart.Y);
                if (newPos.X < 0) newPos.X = 0;
                if (newPos.Y < 0) newPos.Y = 0;
                if (newPos.X + this.Width > parentForm.ClientSize.Width)
                    newPos.X = parentForm.ClientSize.Width - this.Width;
                if (newPos.Y + this.Height > parentForm.ClientSize.Height)
                    newPos.Y = parentForm.ClientSize.Height - this.Height;
                this.Location = newPos;
            }
        }
        private void Header_MouseUp(object sender, MouseEventArgs e){
            dragging = false;
            try{ headerPanel.Capture = false; }catch { }
        }
        private void TestBox_MouseDown(object sender, MouseEventArgs e){
            if (e.Button == MouseButtons.Left){
                if (IsInCorner(e.Location)) resizingCorner = true;
                else if (IsOnRightEdge(e.Location)) resizingRight = true;
                else if (IsOnBottomEdge(e.Location)) resizingBottom = true;
                resizeStart = e.Location;
                originalSize = this.Size;
                try{ this.Capture = true; }catch { }
            }
        }
        private void TestBox_MouseMove(object sender, MouseEventArgs e){
            if (resizingRight || resizingBottom || resizingCorner){
                if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.None){
                    resizingRight = false;
                    resizingBottom = false;
                    resizingCorner = false;
                    this.Cursor = Cursors.Default;
                    try{ this.Capture = false; }catch { }
                    return;
                }
                int newWidth = originalSize.Width;
                int newHeight = originalSize.Height;
                float scaleFactor = this.DeviceDpi / 96f;
                if (resizingRight || resizingCorner)
                    newWidth = Math.Max((int)(200 * scaleFactor), originalSize.Width + (e.X - resizeStart.X));
                if (resizingBottom || resizingCorner)
                    newHeight = Math.Max((int)(175 * scaleFactor), originalSize.Height + (e.Y - resizeStart.Y));
                if (this.Left + newWidth > parentForm.ClientSize.Width)
                    newWidth = parentForm.ClientSize.Width - this.Left;
                if (this.Top + newHeight > parentForm.ClientSize.Height)
                    newHeight = parentForm.ClientSize.Height - this.Top;
                this.Size = new Size(newWidth, newHeight);
                UpdateCachedArea();
            }else{
                if (IsInCorner(e.Location)) this.Cursor = Cursors.SizeNWSE;
                else if (IsOnRightEdge(e.Location)) this.Cursor = Cursors.SizeWE;
                else if (IsOnBottomEdge(e.Location)) this.Cursor = Cursors.SizeNS;
                else this.Cursor = Cursors.Default;
            }
        }
        private void TestBox_MouseUp(object sender, MouseEventArgs e){
            resizingRight = false;
            resizingBottom = false;
            resizingCorner = false;
            this.Cursor = Cursors.Default;
            try{ this.Capture = false; }catch { }
            UpdateCachedArea();
        }
        private bool IsOnRightEdge(Point p) => Math.Abs(p.X - this.Width) <= 6;
        private bool IsOnBottomEdge(Point p) => Math.Abs(p.Y - this.Height) <= 6;
        private bool IsInCorner(Point p) => IsOnRightEdge(p) && IsOnBottomEdge(p);
        protected override void Dispose(bool disposing){
            if (disposing){
                try{
                    renderCts?.Cancel();
                }catch (Exception) { }
                try{
                    renderCts?.Dispose();
                }catch (Exception) { }
                try{
                    elapsedTimer?.Stop();
                    elapsedTimer?.Dispose();
                }catch (Exception) { }
                try{
                    stopwatch?.Stop();
                }catch (Exception) { }
                try{
                    lock (frameLock){
                        frame?.Dispose();
                        frame = null;
                    }
                }catch (Exception) { }
                try{
                    randomBuffer = null;
                }catch (Exception) { }
            }
            base.Dispose(disposing);
        }
    }
}