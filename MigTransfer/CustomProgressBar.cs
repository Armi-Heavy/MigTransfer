using System;
using System.Drawing;
using System.Windows.Forms;

public class CustomProgressBar : ProgressBar
{
    public CustomProgressBar() => this.SetStyle(ControlStyles.UserPaint, true);

    protected override void OnPaint(PaintEventArgs e)
    {
        var rect = this.ClientRectangle;
        var g = e.Graphics;

        ProgressBarRenderer.DrawHorizontalBar(g, rect);
        rect.Inflate(-3, -3);

        if (this.Value > 0)
        {
            var clip = new Rectangle(rect.X, rect.Y, (int)Math.Round((float)this.Value / this.Maximum * rect.Width), rect.Height);
            using (var brush = new SolidBrush(this.ForeColor))
            {
                g.FillRectangle(brush, clip);
            }
        }
    }
}
