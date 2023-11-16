using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing; 


namespace ZzCleaner
{
    internal class CustomCheckedListBox : CheckedListBox
    {
        internal CustomCheckedListBox() {
            DoubleBuffered = true; 
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        private Color customHighlightColor = Color.FromArgb(60, 60, 60); // Your custom highlight color

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            
            e.DrawBackground();

            if (SelectionMode != SelectionMode.None && (e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                using (Brush brush = new SolidBrush(customHighlightColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
                e.DrawFocusRectangle();
            }

            //SystemColors.Highlight = Color.Gray;
            this.CheckOnClick = true;
            DrawItemEventArgs e2 =
            new DrawItemEventArgs
            (
                e.Graphics,
                e.Font,
                new Rectangle(e.Bounds.Location, e.Bounds.Size),
                e.Index,
                e.State & ~DrawItemState.Selected, 
                e.ForeColor,
                e.BackColor
            );  
            
            OnDrawItem(e2);
        }
    }
}
