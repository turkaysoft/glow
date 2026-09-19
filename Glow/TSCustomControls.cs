// ======================================================================================================
// Türkaysoft - C# Custom Graphics UI Library
// Library Version: v2.0
// Compilation Date: 10.09.2026
// © Eray Türkay
// ======================================================================================================

// ======================================================================================================
// Current TS Custom Graphics UI Library Control Items
// ---------------------------------------------------------
// - Button
// - CheckBox
// - ComboBox
// - DateTimePicker
// - FlowLayoutPanel
// - Label
// - ListBox
// - Panel
// - RadioButton
// - TrackBar
// ======================================================================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Glow
{
    #region TS Custom Button
    public class TSCustomButton : Button
    {
        private int borderSize = 0;
        private int borderRadius = 5;
        private Color borderColor = Color.DodgerBlue;
        private Control _observedParent;
        [Category("TS Appearance")]
        public int BorderSize
        {
            get => borderSize;
            set { borderSize = Math.Max(0, value); Invalidate(); }
        }
        [Category("TS Appearance")]
        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
        }
        [Category("TS Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }
        [Category("TS Appearance")]
        public Color BackgroundColor
        {
            get => BackColor;
            set => BackColor = value;
        }
        [Category("TS Appearance")]
        public Color TextColor
        {
            get => ForeColor;
            set => ForeColor = value;
        }
        public TSCustomButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Size = new Size(150, 40);
            BackColor = Color.DodgerBlue;
            ForeColor = Color.White;
        }
        private float ScaleFactor => DeviceDpi / 96f;
        private GraphicsPath GetFigurePath(Rectangle rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            float curveSize = radius * 2f;
            if (curveSize > rect.Width) curveSize = rect.Width;
            if (curveSize > rect.Height) curveSize = rect.Height;
            if (curveSize <= 0) curveSize = 0.1f;
 
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
        private void UpdateRegion()
        {
            if (Width <= 0 || Height <= 0) return;
            float scale = DeviceDpi / 96f;
            int maxRadius = (int)(Height / scale);
            int safeRadius = Math.Min(borderRadius, maxRadius);
            int scaledBorderRadius = (int)(safeRadius * ScaleFactor);
            Rectangle rectSurface = ClientRectangle;
            Region oldRegion = this.Region;
            if (scaledBorderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, scaledBorderRadius))
                {
                    this.Region = new Region(pathSurface);
                }
            }
            else
            {
                this.Region = new Region(rectSurface);
            }
            oldRegion?.Dispose();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Parent == null) return;
            if (Width <= 0 || Height <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            int scaledBorderSize = (int)(borderSize * ScaleFactor);
            float scale = DeviceDpi / 96f;
            int maxRadius = (int)(Height / scale);
            int safeRadius = Math.Min(borderRadius, maxRadius);
            int scaledBorderRadius = (int)(safeRadius * ScaleFactor);
            Rectangle rectSurface = ClientRectangle;
            int maxBorder = Math.Min(rectSurface.Width, rectSurface.Height) / 2;
            if (scaledBorderSize > maxBorder) scaledBorderSize = maxBorder;
            if (scaledBorderSize < 0) scaledBorderSize = 0;
            RectangleF rectBorder = new RectangleF(scaledBorderSize / 2f, scaledBorderSize / 2f, rectSurface.Width - scaledBorderSize, rectSurface.Height - scaledBorderSize);
            int smoothSize = scaledBorderSize > 0 ? scaledBorderSize : 2;
            if (scaledBorderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetFigurePath(rectSurface, scaledBorderRadius))
                using (GraphicsPath pathBorder = GetFigurePath(Rectangle.Round(rectBorder), Math.Max(1, scaledBorderRadius - scaledBorderSize)))
                using (Pen penSurface = new Pen(Parent.BackColor, smoothSize))
                using (Pen penBorder = new Pen(borderColor, scaledBorderSize))
                {
                    g.DrawPath(penSurface, pathSurface);
                    if (scaledBorderSize >= 1)
                    {
                        g.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else
            {
                if (scaledBorderSize >= 1 && Width > 1 && Height > 1)
                {
                    using (Pen penBorder = new Pen(borderColor, scaledBorderSize))
                    {
                        penBorder.Alignment = PenAlignment.Inset;
                        g.DrawRectangle(penBorder, 0, 0, Width - 1, Height - 1);
                    }
                }
            }
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
            }
            _observedParent = Parent;
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged += Container_BackColorChanged;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
                _observedParent = null;
            }
            base.Dispose(disposing);
        }
        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom CheckBox
    public class TSCustomCheckBox : CheckBox
    {
        private Color _checkedColor = Color.DodgerBlue;
        [Category("TS Appearance")]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }
        private Color _checkMarkColor = Color.White;
        [Category("TS Appearance")]
        public Color CheckMarkColor
        {
            get => _checkMarkColor;
            set { _checkMarkColor = value; Invalidate(); }
        }
        private Color _uncheckedBackColor = Color.Transparent;
        [Category("TS Appearance")]
        public Color UncheckedBackColor
        {
            get => _uncheckedBackColor;
            set { _uncheckedBackColor = value; Invalidate(); }
        }
        private bool _drawUncheckedFill = false;
        [Category("TS Appearance")]
        public bool DrawUncheckedFill
        {
            get => _drawUncheckedFill;
            set { _drawUncheckedFill = value; Invalidate(); }
        }
        private float _borderThickness = 2f;
        [Category("TS Appearance")]
        public float BorderThickness
        {
            get => _borderThickness;
            set { _borderThickness = Math.Max(0, value); Invalidate(); }
        }
        private Color _uncheckedBorderColor = Color.Gray;
        [Category("TS Appearance")]
        public Color UncheckedBorderColor
        {
            get => _uncheckedBorderColor;
            set { _uncheckedBorderColor = value; Invalidate(); }
        }
        private float _borderRadius = 2f;
        [Category("TS Appearance")]
        public float BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }
        private float _maxBorderThickness = 4f;
        [Category("TS Appearance")]
        public float MaxBorderThickness
        {
            get => _maxBorderThickness;
            set { _maxBorderThickness = Math.Max(0, value); Invalidate(); }
        }
        private float _maxBorderRadius = 8f;
        [Category("TS Appearance")]
        public float MaxBorderRadius
        {
            get => _maxBorderRadius;
            set { _maxBorderRadius = Math.Max(0, value); Invalidate(); }
        }
        public TSCustomCheckBox()
        {
            AutoSize = true;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
            PerformLayout();
        }
        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            Invalidate();
        }
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }
        public override Size GetPreferredSize(Size proposedSize)
        {
            float dpi = DeviceDpi / 96f;
            int boxSize = (int)(16 * dpi);
            int padding = (int)(6 * dpi);
            int margin = 2;
            TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            Size textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), flags);
            int width = textSize.Width + boxSize + padding + (margin * 2);
            int height = Math.Max(textSize.Height, boxSize) + 4;
            return new Size(width, height);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (Width <= 0 || Height <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float dpi = DeviceDpi / 96f;
            float boxSize = 16f * dpi;
            float padding = 6f * dpi;
            float margin = 2f;
            bool checkOnRight = CheckAlign == ContentAlignment.MiddleRight || CheckAlign == ContentAlignment.TopRight || CheckAlign == ContentAlignment.BottomRight;
            float boxY = (Height - boxSize) / 2f;
            float boxX = checkOnRight ? Width - boxSize - margin : margin;
            Rectangle textRect = checkOnRight ? new Rectangle(0, 0, (int)(Width - boxSize - padding - margin), Height) : new Rectangle((int)(boxSize + padding + margin), 0, Width - (int)(boxSize + padding + margin), Height);
            RectangleF boxRect = new RectangleF(boxX, boxY, boxSize, boxSize);
            float radius = Math.Min(BorderRadius, MaxBorderRadius) * dpi;
            radius = Math.Min(radius, boxSize / 2f);
            if (Checked || (DrawUncheckedFill && UncheckedBackColor.A > 0))
            {
                Color fillColor = Checked ? CheckedColor : (!Enabled ? SystemColors.Control : UncheckedBackColor);
                using (var path = GetRoundedRectPath(boxRect, radius))
                using (var brush = new SolidBrush(fillColor))
                    g.FillPath(brush, path);
            }
            float border = Math.Min(BorderThickness, MaxBorderThickness) * dpi;
            if (border > 0.5f)
            {
                RectangleF borderRect = new RectangleF(boxRect.X + border / 2f, boxRect.Y + border / 2f, boxRect.Width - border, boxRect.Height - border);
                Color borderColor = Checked ? CheckedColor : (!Enabled ? SystemColors.ControlDark : UncheckedBorderColor);
                using (var path = GetRoundedRectPath(borderRect, radius))
                using (var pen = new Pen(borderColor, border))
                    g.DrawPath(pen, path);
            }
            if (Checked)
            {
                using (var pen = new Pen(CheckMarkColor, boxRect.Width * 0.18f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    float cx = boxRect.Left + boxRect.Width / 2f;
                    float cy = boxRect.Top + boxRect.Height / 2f;
                    float s = boxRect.Width * 0.5f;
                    g.DrawLines(pen, new[]
                    {
                        new PointF(cx - s * 0.5f, cy),
                        new PointF(cx - s * 0.1f, cy + s * 0.4f),
                        new PointF(cx + s * 0.5f, cy - s * 0.4f)
                    });
                }
            }
            TextFormatFlags textFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
            textFlags |= checkOnRight ? TextFormatFlags.Right : TextFormatFlags.Left;
            if (textRect.Width > 0 && textRect.Height > 0)
                TextRenderer.DrawText(g, Text, Font, textRect, Enabled ? ForeColor : SystemColors.GrayText, textFlags);
        }
        private GraphicsPath GetRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            if (radius <= 0.5f)
            {
                path.AddRectangle(rect);
                return path;
            }
            float d = radius * 2f;
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom ComboBox
    public class TSCustomComboBox : ComboBox
    {
        private Color _backColor = SystemColors.Window;
        private Color _foreColor = SystemColors.WindowText;
        private Color _buttonColor = SystemColors.ControlDark;
        private Color _arrowColor = SystemColors.WindowText;
        private Color _borderColor = SystemColors.ControlDark;
        private Color _disabledBackColor = SystemColors.Control;
        private Color _disabledForeColor = SystemColors.GrayText;
        private Color _disabledButtonColor = SystemColors.ControlDark;
        private Color _disabledArrowColor = SystemColors.GrayText;
        private Color _focusedBorderColor = Color.DodgerBlue;
        private Color _hoverBackColor = SystemColors.Window;
        private Color _hoverForeColor = SystemColors.WindowText;
        private Color _hoverButtonColor = SystemColors.ControlDark;
        private bool _isHovering;
        public TSCustomComboBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            MouseEnter += (s, e) => { _isHovering = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovering = false; Invalidate(); };
            DrawItem += TSCustomComboBox_DrawItem;
        }
        [Category("TS Appearance")]
        public override Color BackColor
        {
            get => _backColor;
            set { _backColor = value; base.BackColor = value; Invalidate(); }
        }
        [Category("TS Appearance")]
        public override Color ForeColor
        {
            get => _foreColor;
            set { _foreColor = value; base.ForeColor = value; Invalidate(); }
        }
        [Category("TS Appearance")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color ButtonColor { get => _buttonColor; set { _buttonColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color ArrowColor { get => _arrowColor; set { _arrowColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color DisabledArrowColor { get => _disabledArrowColor; set { _disabledArrowColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color DisabledBackColor { get => _disabledBackColor; set { _disabledBackColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color DisabledForeColor { get => _disabledForeColor; set { _disabledForeColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color DisabledButtonColor { get => _disabledButtonColor; set { _disabledButtonColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color FocusedBorderColor { get => _focusedBorderColor; set { _focusedBorderColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color HoverBackColor { get => _hoverBackColor; set { _hoverBackColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color HoverForeColor { get => _hoverForeColor; set { _hoverForeColor = value; Invalidate(); } }
        [Category("TS Appearance")]
        public Color HoverButtonColor { get => _hoverButtonColor; set { _hoverButtonColor = value; Invalidate(); } }
        private Color _selectedBackColor = SystemColors.Highlight;
        [Category("TS Appearance")]
        public Color SelectedBackColor { get => _selectedBackColor; set { _selectedBackColor = value; Invalidate(); } }
        private Color _selectedForeColor = SystemColors.HighlightText;
        [Category("TS Appearance")]
        public Color SelectedForeColor { get => _selectedForeColor; set { _selectedForeColor = value; Invalidate(); } }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;
            bool rtl = RightToLeft == RightToLeft.Yes;
            float scale = DeviceDpi / 96f;
            int buttonWidth = (int)(20 * scale);
            int padding = (int)(6 * scale);
            Rectangle buttonRect = rtl ? new Rectangle(0, 0, buttonWidth, rect.Height) : new Rectangle(rect.Width - buttonWidth, 0, buttonWidth, rect.Height);
            Rectangle textRect = rtl ? new Rectangle(buttonRect.Right + padding, 0, rect.Width - buttonRect.Width - (padding * 2), rect.Height) : new Rectangle(padding, 0, rect.Width - buttonRect.Width - (padding * 2), rect.Height);
            bool useHover = _isHovering && Enabled;
            Color back = !Enabled ? _disabledBackColor : useHover ? _hoverBackColor : _backColor;
            Color fore = !Enabled ? _disabledForeColor : useHover ? _hoverForeColor : _foreColor;
            Color button = !Enabled ? _disabledButtonColor : useHover ? _hoverButtonColor : _buttonColor;
            using (SolidBrush b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, rect);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            flags |= rtl ? TextFormatFlags.Right : TextFormatFlags.Left;
            if (textRect.Width > 0 && textRect.Height > 0)
                TextRenderer.DrawText(e.Graphics, Text, Font, textRect, fore, flags);
            using (SolidBrush b = new SolidBrush(button))
                e.Graphics.FillRectangle(b, buttonRect);
            float aw = 8 * scale;
            float ah = 5 * scale;
            PointF c = new PointF(buttonRect.Left + buttonRect.Width / 2f, buttonRect.Top + buttonRect.Height / 2f);
            PointF[] arrow =
            {
                new PointF(c.X - aw / 2, c.Y - ah / 2),
                new PointF(c.X + aw / 2, c.Y - ah / 2),
                new PointF(c.X, c.Y + ah / 2)
            };
            using (SolidBrush b = new SolidBrush(!Enabled ? _disabledArrowColor : _arrowColor))
                e.Graphics.FillPolygon(b, arrow);
            if (rect.Width > 1 && rect.Height > 1)
            {
                using (Pen p = new Pen(Focused ? _focusedBorderColor : _borderColor))
                    e.Graphics.DrawRectangle(p, 0, 0, rect.Width - 1, rect.Height - 1);
            }
        }
        private void TSCustomComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color back = selected ? SelectedBackColor : BackColor;
            Color fore = selected ? SelectedForeColor : ForeColor;
            using (SolidBrush b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, e.Bounds);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            flags |= (RightToLeft == RightToLeft.Yes) ? TextFormatFlags.Right : TextFormatFlags.Left;
            Rectangle itemBounds = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, Items[e.Index]?.ToString() ?? string.Empty, Font, itemBounds, fore, flags);
        }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); Invalidate(); }
        protected override void OnSelectedIndexChanged(EventArgs e) { base.OnSelectedIndexChanged(e); Invalidate(); }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Invalidate(); }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DrawItem -= TSCustomComboBox_DrawItem;
            }
            base.Dispose(disposing);
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom DateTimePicker
    public class TSCustomDateTimePicker : DateTimePicker
    {
        private Color _backColor = SystemColors.Window;
        private Color _foreColor = SystemColors.WindowText;
        private Color _buttonColor = SystemColors.ControlDark;
        private Color _borderColor = SystemColors.ControlDark;
        private Color _disabledBackColor = SystemColors.Control;
        private Color _disabledForeColor = SystemColors.GrayText;
        private Color _disabledButtonColor = SystemColors.ControlDark;
        private Color _focusedBorderColor = Color.DodgerBlue;
        public TSCustomDateTimePicker()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UpdateCalendarColors();
        }
        [Browsable(true), Category("TS Appearance")]
        public override Color BackColor
        {
            get => _backColor;
            set { _backColor = value; UpdateCalendarColors(); Invalidate(); }
        }
        private bool ShouldSerializeBackColor() => _backColor != SystemColors.Window;
        [Browsable(true), Category("TS Appearance")]
        public override Color ForeColor
        {
            get => _foreColor;
            set { _foreColor = value; UpdateCalendarColors(); Invalidate(); }
        }
        private bool ShouldSerializeForeColor() => _foreColor != SystemColors.WindowText;
        [Browsable(true), Category("TS Appearance")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Browsable(true), Category("TS Appearance")] public Color ButtonColor { get => _buttonColor; set { _buttonColor = value; Invalidate(); } }
        [Browsable(true), Category("TS Appearance")] public Color DisabledBackColor { get => _disabledBackColor; set { _disabledBackColor = value; Invalidate(); } }
        [Browsable(true), Category("TS Appearance")] public Color DisabledForeColor { get => _disabledForeColor; set { _disabledForeColor = value; Invalidate(); } }
        [Browsable(true), Category("TS Appearance")] public Color DisabledButtonColor { get => _disabledButtonColor; set { _disabledButtonColor = value; Invalidate(); } }
        [Browsable(true), Category("TS Appearance")] public Color FocusedBorderColor { get => _focusedBorderColor; set { _focusedBorderColor = value; Invalidate(); } }
        private void UpdateCalendarColors()
        {
            if (IsDisposed || Disposing) return;
            try
            {
                this.CalendarForeColor = _foreColor;
                this.CalendarMonthBackground = _backColor;
            }
            catch { }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateCalendarColors();
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = this.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;
            bool rtl = this.RightToLeft == RightToLeft.Yes;
            float scale = this.DeviceDpi / 96f;
            int buttonWidth = (int)(20 * scale);
            int padding = (int)(2 * scale);
            Rectangle buttonRect = rtl ? new Rectangle(0, 0, buttonWidth, rect.Height) : new Rectangle(rect.Width - buttonWidth, 0, buttonWidth, rect.Height);
            Rectangle textRect = rtl ? new Rectangle(buttonRect.Right + padding, 0, rect.Width - buttonRect.Width - padding, rect.Height) : new Rectangle(padding, 0, rect.Width - buttonRect.Width - padding, rect.Height);
            Color effectiveBack = this.Enabled ? _backColor : _disabledBackColor;
            Color effectiveFore = this.Enabled ? _foreColor : _disabledForeColor;
            Color effectiveButton = this.Enabled ? _buttonColor : _disabledButtonColor;
            using (SolidBrush b = new SolidBrush(effectiveBack))
                e.Graphics.FillRectangle(b, rect);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | (rtl ? TextFormatFlags.Right : TextFormatFlags.Left);
            if (textRect.Width > 0 && textRect.Height > 0)
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, effectiveFore, flags);
            using (SolidBrush b = new SolidBrush(effectiveButton))
                e.Graphics.FillRectangle(b, buttonRect);
            int arrowWidth = (int)(8 * scale);
            int arrowHeight = (int)(5 * scale);
            Point middle = new Point(buttonRect.Left + buttonRect.Width / 2, buttonRect.Top + buttonRect.Height / 2);
            Point[] arrow = {
                new Point(middle.X - arrowWidth / 2, middle.Y - arrowHeight / 2),
                new Point(middle.X + arrowWidth / 2, middle.Y - arrowHeight / 2),
                new Point(middle.X, middle.Y + arrowHeight / 2)
            };
            using (SolidBrush arrowBrush = new SolidBrush(effectiveFore))
                e.Graphics.FillPolygon(arrowBrush, arrow);
            if (rect.Width > 1 && rect.Height > 1)
            {
                using (Pen pen = new Pen(this.Focused ? _focusedBorderColor : _borderColor))
                    e.Graphics.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
            }
            if (this.Focused && this.ShowFocusCues && this.Enabled && rect.Width > 4 && rect.Height > 4)
            {
                Rectangle focusRect = new Rectangle(2, 2, rect.Width - 4, rect.Height - 4);
                ControlPaint.DrawFocusRectangle(e.Graphics, focusRect);
            }
        }
        protected override void OnValueChanged(EventArgs eventargs) { base.OnValueChanged(eventargs); Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); Invalidate(); }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom FlowLayoutPanel
    public class TSCustomFLP : FlowLayoutPanel
    {
        private int _borderRadius = 0;
        private Control _observedParent;
        private Point _savedScrollPosition;
        private bool _restoringScroll;
        [Category("TS Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = Math.Max(0, value);
                UpdateRegion();
                Invalidate();
            }
        }
        public TSCustomFLP()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
            Invalidate();
        }
        private void UpdateRegion()
        {
            float scale = DeviceDpi / 96f;
            float radius = _borderRadius * scale;
            Region oldRegion = this.Region;
            if (radius > 2 && this.Width > 0 && this.Height > 0)
            {
                Rectangle rect = ClientRectangle;
                using (GraphicsPath path = GetRoundedRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius))
                {
                    this.Region = new Region(path);
                }
            }
            else
            {
                this.Region = null;
            }
            oldRegion?.Dispose();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Parent == null) return;
            if (Width <= 0 || Height <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float scale = DeviceDpi / 96f;
            float radius = _borderRadius * scale;
            Rectangle rectSurface = ClientRectangle;
            if (radius > 2)
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(brush, rectSurface);
                }
                using (GraphicsPath pathSurface = GetRoundedRectangle(new RectangleF(rectSurface.X, rectSurface.Y, rectSurface.Width, rectSurface.Height), radius))
                using (Pen penSurface = new Pen(Parent.BackColor, 2f))
                {
                    g.DrawPath(penSurface, pathSurface);
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                    g.FillRectangle(brush, rectSurface);
            }
        }
        private GraphicsPath GetRoundedRectangle(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            float curveSize = radius * 2f;
            if (curveSize > rect.Width) curveSize = rect.Width;
            if (curveSize > rect.Height) curveSize = rect.Height;
            if (curveSize <= 0) curveSize = 0.1f;
 
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if (!_restoringScroll)
            {
                Point p = AutoScrollPosition;
                _savedScrollPosition = new Point(-p.X, -p.Y);
            }
        }
        protected override void OnResize(EventArgs e)
        {
            Point p = AutoScrollPosition;
            _savedScrollPosition = new Point(-p.X, -p.Y);
            base.OnResize(e);
            UpdateRegion();
            Invalidate();
            if (!IsHandleCreated || DesignMode) return;
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (IsDisposed || Disposing || !IsHandleCreated) return;
                    try
                    {
                        _restoringScroll = true;
                        AutoScrollPosition = _savedScrollPosition;
                    }
                    catch { }
                    finally
                    {
                        _restoringScroll = false;
                    }
                });
            }
            catch { }
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
            }
            _observedParent = Parent;
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged += Container_BackColorChanged;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
                _observedParent = null;
            }
            base.Dispose(disposing);
        }
        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom Label
    public class TSCustomLabel : Label
    {
        private int _borderRadius = 0;
        private Control _observedParent;
        [Category("TS Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = Math.Max(0, value);
                UpdateRegion();
                Invalidate();
            }
        }
        public TSCustomLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
            Invalidate();
        }
        private void UpdateRegion()
        {
            float scale = DeviceDpi / 96f;
            float radius = _borderRadius * scale;
            Region oldRegion = this.Region;
            if (radius > 2 && this.Width > 0 && this.Height > 0)
            {
                Rectangle rect = ClientRectangle;
                using (GraphicsPath path = GetRoundedRectangle(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius))
                {
                    this.Region = new Region(path);
                }
            }
            else
            {
                this.Region = null;
            }
            oldRegion?.Dispose();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Parent == null) return;
            if (Width <= 0 || Height <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float scale = DeviceDpi / 96f;
            float radius = _borderRadius * scale;
            Rectangle rectSurface = ClientRectangle;
            if (radius > 2)
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(brush, rectSurface);
                }
                using (GraphicsPath pathSurface = GetRoundedRectangle(new RectangleF(rectSurface.X, rectSurface.Y, rectSurface.Width, rectSurface.Height), radius))
                using (Pen penSurface = new Pen(Parent.BackColor, 2f))
                {
                    g.DrawPath(penSurface, pathSurface);
                }
                TextFormatFlags flags = GetTextFormatFlags();
                Rectangle textRect = new Rectangle(this.Padding.Left, this.Padding.Top, this.Width - this.Padding.Horizontal, this.Height - this.Padding.Vertical);
                if (textRect.Width > 0 && textRect.Height > 0)
                    TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, flags);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                    g.FillRectangle(brush, rectSurface);
                TextFormatFlags flags = GetTextFormatFlags();
                if (rectSurface.Width > 0 && rectSurface.Height > 0)
                    TextRenderer.DrawText(g, Text, Font, rectSurface, ForeColor, flags);
            }
        }
        private TextFormatFlags GetTextFormatFlags()
        {
            TextFormatFlags baseFlags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                    return baseFlags | TextFormatFlags.Top | TextFormatFlags.Left;
                case ContentAlignment.TopCenter:
                    return baseFlags | TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.TopRight:
                    return baseFlags | TextFormatFlags.Top | TextFormatFlags.Right;
                case ContentAlignment.MiddleLeft:
                    return baseFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                case ContentAlignment.MiddleCenter:
                    return baseFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.MiddleRight:
                    return baseFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
                case ContentAlignment.BottomLeft:
                    return baseFlags | TextFormatFlags.Bottom | TextFormatFlags.Left;
                case ContentAlignment.BottomCenter:
                    return baseFlags | TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                case ContentAlignment.BottomRight:
                    return baseFlags | TextFormatFlags.Bottom | TextFormatFlags.Right;
                default:
                    return baseFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            }
        }
        private GraphicsPath GetRoundedRectangle(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            float curveSize = radius * 2f;
            if (curveSize > rect.Width) curveSize = rect.Width;
            if (curveSize > rect.Height) curveSize = rect.Height;
            if (curveSize <= 0) curveSize = 0.1f;
 
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
            }
            _observedParent = Parent;
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged += Container_BackColorChanged;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
                _observedParent = null;
            }
            base.Dispose(disposing);
        }
        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom ListBox
    public class TSCustomListBox : ListBox
    {
        private const int LB_ADDSTRING = 0x180;
        private const int LB_INSERTSTRING = 0x181;
        private const int LB_DELETESTRING = 0x182;
        private const int LB_RESETCONTENT = 0x184;
        private const int LB_DIR = 0x18D;
        private const int LB_ADDFILE = 0x196;
        private Color _selectedBackColor = Color.DodgerBlue;
        [Category("TS Appearance")]
        public Color SelectedBackColor
        {
            get => _selectedBackColor;
            set { _selectedBackColor = value; Invalidate(); }
        }
        private Color _selectedForeColor = Color.White;
        [Category("TS Appearance")]
        public Color SelectedForeColor
        {
            get => _selectedForeColor;
            set { _selectedForeColor = value; Invalidate(); }
        }
        public TSCustomListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DoubleBuffered = true;
            UpdateItemHeight();
        }
        private void UpdateItemHeight()
        {
            float dpiScale = (float)DeviceDpi / 96f;
            this.ItemHeight = (int)((this.Font.Height + 5) * dpiScale);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateItemHeight();
            UpdateHorizontalExtent();
        }
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateItemHeight();
            UpdateHorizontalExtent();
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == LB_ADDSTRING || m.Msg == LB_INSERTSTRING || m.Msg == LB_DELETESTRING || m.Msg == LB_RESETCONTENT || m.Msg == LB_DIR || m.Msg == LB_ADDFILE)
            {
                if (!IsDisposed && !Disposing && IsHandleCreated)
                {
                    UpdateHorizontalExtent();
                }
            }
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            e.DrawBackground();
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color backColor = selected ? SelectedBackColor : this.BackColor;
            Color foreColor = selected ? SelectedForeColor : this.ForeColor;
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            Rectangle textBounds = new Rectangle(e.Bounds.X + 3, e.Bounds.Y, e.Bounds.Width - 3, e.Bounds.Height);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix;
            if (textBounds.Width > 0 && textBounds.Height > 0)
                TextRenderer.DrawText(e.Graphics, this.Items[e.Index]?.ToString() ?? string.Empty, this.Font, textBounds, foreColor, flags);
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }
        }
        public void UpdateHorizontalExtent()
        {
            if (IsDisposed || Disposing) return;
            int maxExtent = 0;
            foreach (var item in this.Items)
            {
                if (item != null)
                {
                    Size textSize = TextRenderer.MeasureText(item.ToString(), this.Font);
                    if (textSize.Width > maxExtent)
                    {
                        maxExtent = textSize.Width;
                    }
                }
            }
            this.HorizontalExtent = maxExtent + 10;
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom Panel
    public class TSCustomPanel : Panel
    {
        private int borderRadius = 10;
        private Color borderColor = Color.DodgerBlue;
        private int borderSize = 0;
        private GraphicsPath pathSurface;
        private GraphicsPath pathBorder;
        [Category("TS Appearance")]
        public int BorderRadius
        {
            get => borderRadius;
            set { borderRadius = Math.Max(0, value); RecreatePaths(); UpdateControlRegion(); this.Invalidate(); }
        }
        [Category("TS Appearance")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; this.Invalidate(); }
        }
        [Category("TS Appearance")]
        public int BorderSize
        {
            get => borderSize;
            set { borderSize = Math.Max(0, value); RecreatePaths(); this.Invalidate(); }
        }
        public TSCustomPanel()
        {
            this.BackColor = Color.White;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RecreatePaths();
            UpdateControlRegion();
        }
        private void RecreatePaths()
        {
            pathSurface?.Dispose();
            pathBorder?.Dispose();
            pathSurface = null;
            pathBorder = null;
            if (this.Width <= 0 || this.Height <= 0) return;
            float scale = this.DeviceDpi / 96f;
            float scaledBorderSize = borderSize * scale;
            float scaledRadius = borderRadius * scale;
            float maxDim = Math.Min(this.Width, this.Height);
            if (scaledBorderSize > maxDim) scaledBorderSize = maxDim;
            if (scaledBorderSize < 0) scaledBorderSize = 0;
            RectangleF rectSurface = new RectangleF(0, 0, this.Width, this.Height);
            RectangleF rectBorder = new RectangleF(scaledBorderSize / 2f, scaledBorderSize / 2f, this.Width - scaledBorderSize, this.Height - scaledBorderSize);
            if (rectBorder.Width <= 0 || rectBorder.Height <= 0)
            {
                if (scaledRadius > 2)
                {
                    pathSurface = GetRoundedRectangle(rectSurface, scaledRadius);
                }
                pathBorder = null;
                return;
            }
            if (scaledRadius > 2)
            {
                pathSurface = GetRoundedRectangle(rectSurface, scaledRadius);
                pathBorder = GetRoundedRectangle(rectBorder, Math.Max(0, scaledRadius - (scaledBorderSize / 2f)));
            }
        }
        private void UpdateControlRegion()
        {
            float scale = this.DeviceDpi / 96f;
            Region oldRegion = this.Region;
            if (pathSurface != null && (borderRadius * scale) > 2)
            {
                this.Region = new Region(pathSurface);
            }
            else
            {
                this.Region = null;
            }
            oldRegion?.Dispose();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (Width <= 0 || Height <= 0) return;
            if (this.Parent != null)
            {
                using (SolidBrush parentBrush = new SolidBrush(this.Parent.BackColor))
                {
                    g.FillRectangle(parentBrush, this.ClientRectangle);
                }
            }
            float scale = this.DeviceDpi / 96f;
            float scaledRadius = borderRadius * scale;
            if (scaledRadius > 2 && pathSurface != null)
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                    g.FillPath(brush, pathSurface);
                if (this.Parent != null)
                {
                    using (Pen penAntiAlias = new Pen(this.Parent.BackColor, 2f))
                    {
                        g.DrawPath(penAntiAlias, pathSurface);
                    }
                }
                if (borderSize > 0 && pathBorder != null)
                {
                    using (Pen penBorder = new Pen(borderColor, borderSize * scale))
                    {
                        g.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                    g.FillRectangle(brush, this.ClientRectangle);
                if (borderSize > 0 && Width > 0 && Height > 0)
                {
                    float scaledBorderSize = borderSize * scale;
                    float maxDimElse = Math.Min(Width, Height);
                    if (scaledBorderSize > maxDimElse) scaledBorderSize = maxDimElse;
                    if (this.Width - scaledBorderSize > 0 && this.Height - scaledBorderSize > 0)
                    {
                        using (Pen penBorder = new Pen(borderColor, scaledBorderSize))
                            g.DrawRectangle(penBorder, scaledBorderSize / 2f, scaledBorderSize / 2f, this.Width - scaledBorderSize, this.Height - scaledBorderSize);
                    }
                }
            }
        }
        private GraphicsPath GetRoundedRectangle(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            float curveSize = radius * 2f;
            if (curveSize > rect.Width) curveSize = rect.Width;
            if (curveSize > rect.Height) curveSize = rect.Height;
            if (curveSize <= 0) curveSize = 0.1f;
 
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
            path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
            path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
            path.CloseFigure();
            return path;
        }
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            RecreatePaths();
            UpdateControlRegion();
            this.Invalidate();
        }
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            this.Invalidate();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pathSurface?.Dispose();
                pathBorder?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom RadioButton
    public class TSCustomRadioButton : RadioButton
    {
        private Color _checkedColor = Color.DodgerBlue;
        [Category("TS Appearance")]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }
        private Color _unCheckedColor = Color.Gray;
        [Category("TS Appearance")]
        public Color UnCheckedColor
        {
            get => _unCheckedColor;
            set { _unCheckedColor = value; Invalidate(); }
        }
        public TSCustomRadioButton()
        {
            AutoSize = true;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); PerformLayout(); }
        protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Invalidate(); }
        public override Size GetPreferredSize(Size proposedSize)
        {
            float dpi = DeviceDpi / 96f;
            int rbSize = (int)(18 * dpi);
            int padding = (int)(8 * dpi);
            int margin = 2;
            TextFormatFlags flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
            Size textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), flags);
            int width = textSize.Width + rbSize + padding + (margin * 2);
            int height = Math.Max(textSize.Height, rbSize) + 6;
            return new Size(width, height);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (Width <= 0 || Height <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float dpi = DeviceDpi / 96f;
            float rbSize = 18f * dpi;
            float checkSize = 10f * dpi;
            float padding = 8f * dpi;
            float margin = 2f;
            bool rightAligned = CheckAlign == ContentAlignment.MiddleRight || CheckAlign == ContentAlignment.TopRight || CheckAlign == ContentAlignment.BottomRight;
            float rbX = rightAligned ? Width - rbSize - margin : margin;
            Rectangle textRect = rightAligned ? new Rectangle(0, 0, (int)(Width - rbSize - padding - margin), Height) : new Rectangle((int)(rbSize + padding + margin), 0, Width - (int)(rbSize + padding + margin), Height);
            RectangleF rbRect = new RectangleF(rbX, (Height - rbSize) / 2f, rbSize, rbSize);
            RectangleF checkRect = new RectangleF(rbRect.X + (rbRect.Width - checkSize) / 2f, rbRect.Y + (rbRect.Height - checkSize) / 2f, checkSize, checkSize);
            using (Pen borderPen = new Pen(Checked ? CheckedColor : UnCheckedColor, 1.6f * dpi))
            using (SolidBrush checkBrush = new SolidBrush(CheckedColor))
            {
                g.DrawEllipse(borderPen, rbRect);
                if (Checked)
                    g.FillEllipse(checkBrush, checkRect);
            }
            TextFormatFlags textFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
            textFlags |= rightAligned ? TextFormatFlags.Right : TextFormatFlags.Left;
            if (textRect.Width > 0 && textRect.Height > 0)
                TextRenderer.DrawText(g, Text, Font, textRect, Enabled ? ForeColor : SystemColors.GrayText, textFlags);
        }
    }
    #endregion
    // ======================================================================================================
    #region TS Custom TrackBar
    public class TSCustomTrackBar : Control
    {
        private int _borderRadius = 0;
        private Control _observedParent;
        [Category("TS Appearance")]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = Math.Max(0, value);
                UpdateRegion();
                Invalidate();
            }
        }
        private Color _trackColor = Color.LightGray;
        [Category("TS Appearance")] public Color TrackColor { get => _trackColor; set { _trackColor = value; Invalidate(); } }
        private Color _trackFillColor = Color.DodgerBlue;
        [Category("TS Appearance")] public Color TrackFillColor { get => _trackFillColor; set { _trackFillColor = value; Invalidate(); } }
        private float _trackHeight = 8f;
        [Category("TS Appearance")]
        public float TrackHeight
        {
            get => _trackHeight;
            set { _trackHeight = Math.Max(0, value); Invalidate(); }
        }
        private float _trackRadius = 5f;
        [Category("TS Appearance")]
        public float TrackRadius
        {
            get => _trackRadius;
            set { _trackRadius = Math.Max(0, value); Invalidate(); }
        }
        private Color _thumbColor = Color.DodgerBlue;
        [Category("TS Appearance")] public Color ThumbColor { get => _thumbColor; set { _thumbColor = value; Invalidate(); } }
        private Color _thumbHoverColor = Color.DodgerBlue;
        [Category("TS Appearance")] public Color ThumbHoverColor { get => _thumbHoverColor; set { _thumbHoverColor = value; Invalidate(); } }
        private Color _thumbPressedColor = Color.DodgerBlue;
        [Category("TS Appearance")] public Color ThumbPressedColor { get => _thumbPressedColor; set { _thumbPressedColor = value; Invalidate(); } }
        private Color _thumbBorderColor = Color.DimGray;
        [Category("TS Appearance")] public Color ThumbBorderColor { get => _thumbBorderColor; set { _thumbBorderColor = value; Invalidate(); } }
        private float _thumbRadius = 10f;
        [Category("TS Appearance")]
        public float ThumbRadius
        {
            get => _thumbRadius;
            set { _thumbRadius = Math.Max(0, value); Invalidate(); }
        }
        private float _thumbBorderThickness = 0f;
        [Category("TS Appearance")]
        public float ThumbBorderThickness
        {
            get => _thumbBorderThickness;
            set { _thumbBorderThickness = Math.Max(0, value); Invalidate(); }
        }
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private bool _hover = false;
        private bool _pressed = false;
        [Category("TS Appearance")]
        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                if (_maximum < _minimum)
                    _maximum = _minimum;
                if (_value < _minimum)
                {
                    _value = _minimum;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }
        [Category("TS Appearance")]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                if (_minimum > _maximum)
                    _minimum = _maximum;
                if (_value > _maximum)
                {
                    _value = _maximum;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            }
        }
        [Category("TS Appearance")]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Max(Minimum, Math.Min(Maximum, value));
                if (_value == newValue) return;
                _value = newValue;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private bool _vertical = false;
        [Category("TS Appearance")]
        public bool Vertical
        {
            get => _vertical;
            set { _vertical = value; Invalidate(); }
        }
        public event EventHandler ValueChanged;
        public TSCustomTrackBar()
        {
            this.Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }
        private void UpdateRegion()
        {
            float scale = DeviceDpi / 96f;
            float radius = _borderRadius * scale;
            Region oldRegion = this.Region;
            if (radius > 2 && this.Width > 0 && this.Height > 0)
            {
                Rectangle rect = ClientRectangle;
                using (GraphicsPath path = GetRoundedRectPath(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius))
                {
                    this.Region = new Region(path);
                }
            }
            else
            {
                this.Region = null;
            }
            oldRegion?.Dispose();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; UpdateValueFromMouse(e.Location); base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { if (_pressed) UpdateValueFromMouse(e.Location); base.OnMouseMove(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        private void UpdateValueFromMouse(Point location)
        {
            float dpiScale = DeviceDpi / 96f;
            float thumbR = ThumbRadius * dpiScale;
            float bThick = ThumbBorderThickness * dpiScale;
            float margin = thumbR + (bThick / 2f) + 2f;
            if (Vertical)
            {
                float usableHeight = Height - (2 * margin);
                if (usableHeight <= 0) return;
                float pos = location.Y - margin;
                float ratio = 1f - (pos / usableHeight);
                Value = Minimum + (int)Math.Round(Math.Max(0, Math.Min(1, ratio)) * (Maximum - Minimum));
            }
            else
            {
                float usableWidth = Width - (2 * margin);
                if (usableWidth <= 0) return;
                float pos = location.X - margin;
                float ratio = pos / usableWidth;
                Value = Minimum + (int)Math.Round(Math.Max(0, Math.Min(1, ratio)) * (Maximum - Minimum));
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (Width <= 0 || Height <= 0) return;
            float dpiScale = DeviceDpi / 96f;
            float radius = _borderRadius * dpiScale;
            Rectangle rectSurface = ClientRectangle;
            if (radius > 2 && Parent != null)
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    g.FillRectangle(brush, rectSurface);
                }
                using (GraphicsPath pathSurface = GetRoundedRectPath(new RectangleF(rectSurface.X, rectSurface.Y, rectSurface.Width, rectSurface.Height), radius))
                using (Pen penSurface = new Pen(Parent.BackColor, 2f))
                {
                    g.DrawPath(penSurface, pathSurface);
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                    g.FillRectangle(brush, rectSurface);
            }
            float thumbR = ThumbRadius * dpiScale;
            float bThick = ThumbBorderThickness * dpiScale;
            float trackH = TrackHeight * dpiScale;
            float trackRad = TrackRadius * dpiScale;
            float margin = thumbR + (bThick / 2f) + 2f;
            RectangleF trackRect = Vertical ? new RectangleF((Width - trackH) / 2f, margin, trackH, Height - (2 * margin)) : new RectangleF(margin, (Height - trackH) / 2f, Width - (2 * margin), trackH);
            if (trackRect.Width <= 0 || trackRect.Height <= 0) return;
            using (GraphicsPath trackPath = GetRoundedRectPath(trackRect, trackRad))
            using (SolidBrush br = new SolidBrush(TrackColor))
                g.FillPath(br, trackPath);
            float ratio = (Maximum == Minimum) ? 0 : (float)(Value - Minimum) / (Maximum - Minimum);
            if (ratio > 0)
            {
                RectangleF fillRect = Vertical ? new RectangleF(trackRect.X, trackRect.Bottom - (trackRect.Height * ratio), trackRect.Width, trackRect.Height * ratio) : new RectangleF(trackRect.X, trackRect.Y, trackRect.Width * ratio, trackRect.Height);
                using (GraphicsPath fillPath = GetRoundedRectPath(fillRect, trackRad))
                using (SolidBrush br = new SolidBrush(TrackFillColor))
                    g.FillPath(br, fillPath);
            }
            PointF thumbCenter = Vertical ? new PointF(Width / 2f, trackRect.Bottom - (trackRect.Height * ratio)) : new PointF(trackRect.X + (trackRect.Width * ratio), Height / 2f);
            RectangleF thumbRect = new RectangleF(thumbCenter.X - thumbR, thumbCenter.Y - thumbR, thumbR * 2, thumbR * 2);
            if (thumbRect.Width <= 0 || thumbRect.Height <= 0) return;
            Color activeThumbColor = _pressed ? ThumbPressedColor : (_hover ? ThumbHoverColor : ThumbColor);
            using (SolidBrush br = new SolidBrush(activeThumbColor))
                g.FillEllipse(br, thumbRect);
            if (bThick > 0)
            {
                using (Pen pen = new Pen(ThumbBorderColor, bThick))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawEllipse(pen, thumbRect);
                }
            }
        }
        private GraphicsPath GetRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            if (radius <= 0.1f) { path.AddRectangle(rect); return path; }
            float d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
            }
            _observedParent = Parent;
            if (_observedParent != null)
            {
                _observedParent.BackColorChanged += Container_BackColorChanged;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _observedParent != null)
            {
                _observedParent.BackColorChanged -= Container_BackColorChanged;
                _observedParent = null;
            }
            base.Dispose(disposing);
        }
        private void Container_BackColorChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
    #endregion
}