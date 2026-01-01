using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsCleaner
{
    /// <summary>
    /// Contrôle personnalisé pour une barre de progression colorée
    /// avec affichage du pourcentage
    /// </summary>
    public class ColoredProgressBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private Color _barColor = Color.FromArgb(0, 120, 215);

        [Category("Behavior")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        /// <summary>Valeur minimale de la barre</summary>
        public int Minimum { get => _minimum; set { _minimum = value; Invalidate(); } }

        [Category("Behavior")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        /// <summary>Valeur maximale de la barre</summary>
        public int Maximum { get => _maximum; set { _maximum = Math.Max(1, value); Invalidate(); } }

        [Category("Behavior")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        /// <summary>Valeur courante de la barre</summary>
        public int Value { get => _value; set { _value = Math.Min(Math.Max(value, Minimum), Maximum); Invalidate(); } }

        [Category("Appearance")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        /// <summary>Couleur de remplissage de la barre</summary>
        public Color BarColor { get => _barColor; set { _barColor = value; Invalidate(); } }

        /// <summary>
        /// Initialise une nouvelle instance de la barre de progression
        /// </summary>
        public ColoredProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Height = 18;
        }

        /// <summary>
        /// Peint le contrôle avec la barre et le pourcentage
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(BackColor);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var rect = ClientRectangle;
            using var borderPen = new Pen(Color.FromArgb(100, ForeColor));
            using var backBrush = new SolidBrush(ControlPaint.Dark(BackColor));
            g.FillRectangle(backBrush, rect);

            if (Maximum <= Minimum) return;
            float range = Maximum - Minimum;
            float pct = (Value - Minimum) / range;
            int fillWidth = (int)(rect.Width * pct);

            var fillRect = new Rectangle(rect.X, rect.Y, fillWidth, rect.Height);
            using var fillBrush = new SolidBrush(BarColor);
            g.FillRectangle(fillBrush, fillRect);

            g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);

            // Afficher le pourcentage au centre
            int percentage = (int)(pct * 100);
            string percentText = $"{percentage}%";
            using var font = new Font("Segoe UI", 10, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            var textSize = g.MeasureString(percentText, font);
            var textRect = new RectangleF(
                (rect.Width - textSize.Width) / 2,
                (rect.Height - textSize.Height) / 2,
                textSize.Width,
                textSize.Height
            );
            g.DrawString(percentText, font, textBrush, textRect);
        }
    }
}
