using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ZzCleaner
{
    public partial class Form1 : Form
    {
        //Moving Form
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        //Panel Shadow
        private List<Control> shadowControls = new List<Control>();
        private Bitmap shadowBmp = null;

        //Folder path
        private string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";

        [DllImport("Gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        private FolderOrganiser folderOrganiser;

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);
        internal void HideCaret()
        {
            HideCaret(richTextBox1.Handle);
        }


        public Form1()
        {
            InitializeComponent();
            base.DoubleBuffered = true;
            shadowControls.Add(FoldersToClear);
            //Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            ApplyRoundedRegionToFoldersToClear();
            this.Refresh();
        }

        private void ApplyRoundedRegionToFoldersToClear()
        {
            int radius = 6;
            IntPtr roundedRegion = CreateRoundRectRgn(0, 0, FoldersToClear.Width, FoldersToClear.Height, 4, radius);
            Region = System.Drawing.Region.FromHrgn(roundedRegion);
            FoldersToClear.Region = Region;
            IntPtr roundedRegionBox = CreateRoundRectRgn(0, 0, richTextBox1.Width, richTextBox1.Height, 1, radius);
            richTextBox1.Region = System.Drawing.Region.FromHrgn(roundedRegionBox);
            DeleteObject(roundedRegion);
        }



        //Russian code 
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 0,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }

        private void FoldersToClear_Paint(object sender, PaintEventArgs e)
        {
            if (shadowBmp == null || shadowBmp.Size != this.Size)
            {
                shadowBmp?.Dispose();
                shadowBmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
            }
            foreach (Control control in shadowControls)
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddRectangle(new Rectangle(control.Location.X, control.Location.Y, control.Size.Width, control.Size.Height));
                    DrawShadowSmooth(gp, 80, 60, shadowBmp);
                }
                e.Graphics.DrawImage(shadowBmp, new Point(0, 0));
            }
        }

        private static void DrawShadowSmooth(GraphicsPath gp, int intensity, int radius, Bitmap dest)
        {
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                double alpha = 0;
                double astep = 0;
                double astepstep = (double)intensity / radius / (radius / 2D);
                for (int thickness = radius; thickness > 0; thickness--)
                {
                    using (Pen p = new Pen(Color.FromArgb((int)alpha, 0, 0, 0), thickness))
                    {
                        p.LineJoin = LineJoin.Round;
                        g.DrawPath(p, gp);
                    }
                    alpha += astep;
                    astep += astepstep;
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Calibri", 8))
            {
                e.Graphics.DrawString("Copyright © 2023 Aeromol Technologies", myFont, Brushes.White, new Point(165, 30));
                MessageBox.Show("Flulf");
            }
        }

        private struct Locations
        {
            public int TextBoxLocationX;
            public int TextBoxLocationY;

            public int PictureBoxLocationX;
            public int PictureBoxLocationY;
        }

        private Locations locations = new Locations
        {
            TextBoxLocationX = 90,
            TextBoxLocationY = 0,

            PictureBoxLocationX = 0,
            PictureBoxLocationY = 14
        };

        private void FolderPathRichTextBox(object item, int textBoxNumber)
        {

            RichTextBox box = new RichTextBox();
            box.Location = new Point(locations.TextBoxLocationX, locations.TextBoxLocationY);
            box.Name = "folderPathRichTextBox" + (int)(textBoxNumber + 1);
            box.Font = new Font("Arial", 7.00f, FontStyle.Italic);
            box.Text = folderPath + item.ToString();
            box.Size = new Size(170, 14);
            //box.BackColor = FoldersToClear.BackColor;
            box.BorderStyle = BorderStyle.None;
            box.ScrollBars = RichTextBoxScrollBars.None;

            box.Click += new System.EventHandler(FolderPathRichTextBox_Click);

            this.FoldersToClear.Controls.Add(box);
            box.Focus();

            locations.TextBoxLocationY += 15;
        }

        private void FolderPathRichTextBox_Click(object sender, EventArgs e)
        {
            RichTextBox changeRichTextBoxColor = sender as RichTextBox;

            changeRichTextBoxColor.BackColor = SystemColors.Window; 
            changeRichTextBoxColor.ForeColor = SystemColors.WindowText;

            
        }



        private void PictureBoxLine(int pictureBoxLineNumber)
        {
            PictureBox pictureBoxLine = new PictureBox();
            pictureBoxLine.Name = "pictureBoxLine" + (int)(pictureBoxLineNumber + 1);
            pictureBoxLine.Location = new Point(locations.PictureBoxLocationX, locations.PictureBoxLocationY);
            pictureBoxLine.BackColor = Color.FromArgb(255, 224, 192);
            pictureBoxLine.Size = new Size(257, 1);
            this.FoldersToClear.Controls.Add(pictureBoxLine);

            locations.PictureBoxLocationY += 15;
        }

        //Making directories
        private void ZzCleaner_Load(object sender, EventArgs e)
        {
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            richTextBox1_Load();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.FoldersToClear))
            {
                Properties.Settings.Default.FoldersToClear.Split(',')
                    .ToList()
                    .ForEach(item =>
                    {

                        folderOrganiser = new FolderOrganiser(item, richTextBox1);
                        bool checkbox = false;
                        if (Properties.Settings.Default.CheckedFolders.Contains(item))
                            checkbox = true;


                        if (!FoldersToClear.Items.Contains(item))
                        {
                            FoldersToClear.Items.Add(item);
                            FolderPathRichTextBox(item, FoldersToClear.Items.IndexOf(item));
                            PictureBoxLine(FoldersToClear.Items.IndexOf(item));
                            //richrichTextBox2.SelectionFont = new Font("Arial", 8.00f, FontStyle.Italic);
                            //richrichTextBox2.AppendText(folderPath + item + Environment.NewLine);
                        }

                        var index = this.FoldersToClear.Items.IndexOf(item);
                        folderOrganiser.AddItemToCleaner(item.ToString(), checkbox);
                        FoldersToClear.SetItemChecked(index, checkbox);
                    });
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var indices = this.FoldersToClear.Items.Cast<string>().ToArray();
            var checkboxValues = this.FoldersToClear.CheckedItems.Cast<string>().ToArray();
            Properties.Settings.Default.CheckedFolders = string.Join(",", checkboxValues);
            Properties.Settings.Default.FoldersToClear = string.Join(",", indices);
            Properties.Settings.Default.Save();
        }

        private void removeBtn2_Click(object sender, EventArgs e) //Add
        {
            if (richTextBox2.Text == "")
            {
                textBox1.Visible = true;
                textBox1.Text = "No Folders Choosen";
                SystemSounds.Exclamation.Play();
                return;
            }

            string folderName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(richTextBox2.Text.ToLower());

            if (FoldersToClear.Items.Contains(folderName))
            {
                textBox1.Visible = true;
                textBox1.Text = richTextBox2.Text + " Folder Is Already In";
                SystemSounds.Exclamation.Play();
                return;
            }

            if (!Regex.IsMatch(richTextBox2.Text, "^[a-zA-Z]+$"))
            {
                textBox1.Visible = true;
                textBox1.Text = "Illegal Character In Folder Name";
                SystemSounds.Exclamation.Play();
                return; 
            }

            textBox1.Visible = false;

            folderOrganiser.AddItemToCleaner(richTextBox2.Text, true);
            FoldersToClear.Items.Add(folderName);
            int indexOfFolder = FoldersToClear.Items.IndexOf(folderName);
            FoldersToClear.SetItemChecked(indexOfFolder, true);
            FolderPathRichTextBox(folderName, indexOfFolder);
            PictureBoxLine(indexOfFolder);
            richTextBox2.Text = "";
        }
        private void removeBtn1_Click(object sender, EventArgs e) //Remove
        {

            if (FoldersToClear.Items.Count != 0)
            {
                if (FoldersToClear.SelectedIndex != -1)
                {
                    int folderToDelete = FoldersToClear.SelectedIndex;
                    //Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + FoldersToClear.Items[folderToDelete].ToString())); Deleted full directory dumbass
                    folderOrganiser.RemoveItemFromCleaner(FoldersToClear.Items[folderToDelete].ToString());
                    FoldersToClear.Items.RemoveAt(folderToDelete);

                    RichTextBox richTextBoxToDelete = this.FoldersToClear.Controls.OfType<RichTextBox>().FirstOrDefault(tb => tb.Name == ("folderPathRichTextBox" + (folderToDelete + 1).ToString()));
                    this.FoldersToClear.Controls.Remove(richTextBoxToDelete);
                    locations.TextBoxLocationY -= 15; 
                    locations.PictureBoxLocationY -= 15;    

                    PictureBox pictureBoxToDelete = this.FoldersToClear.Controls.OfType<PictureBox>().FirstOrDefault(pb => pb.Name == ("pictureBoxLine" + (folderToDelete + 1).ToString()));
                    this.FoldersToClear.Controls.Remove(pictureBoxToDelete);
                }
            }
        }

        private void textBox1_TextChanged_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.ForeColor = Color.Black;

        }

        private void richTextBox2_Click(object sender, EventArgs e)
        {
            this.richTextBox2.Text = "";
            this.richTextBox2.ForeColor = Color.Black;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e) { dragging = false; }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void richTextBox1_Load()
        {
            richTextBox1.Clear();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;

            richTextBox1.SelectionFont = new Font("Arial Rounded MT Bold", 8.25f);
            richTextBox1.SelectionColor = Color.FromArgb(64, 64, 64);
            richTextBox1.AppendText("Output Log");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            base.Hide();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            base.WindowState = FormWindowState.Minimized;
        }

        private RichTextBox previousRichTextBox = null; 
        private void FoldersToClear_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            RichTextBox changeRichTextBoxColor = this.FoldersToClear.Controls.OfType<RichTextBox>().FirstOrDefault(tb => tb.Name == ("folderPathRichTextBox" + (e.Index + 1).ToString()));
            if (previousRichTextBox != null && previousRichTextBox != changeRichTextBoxColor)
            {
                previousRichTextBox.BackColor = SystemColors.Window;
                previousRichTextBox.ForeColor = SystemColors.WindowText;
            }


            if (changeRichTextBoxColor != null)
            {
                changeRichTextBoxColor.BackColor = SystemColors.Highlight;
                changeRichTextBoxColor.ForeColor = SystemColors.HighlightText;
            }

            string checkedItemName = FoldersToClear.Items[e.Index].ToString();

            if (e.NewValue != CheckState.Checked)
                folderOrganiser.ChangedItemInCleaner(checkedItemName, false);

            else if (e.NewValue != CheckState.Unchecked)
                folderOrganiser.ChangedItemInCleaner(checkedItemName, true);

            previousRichTextBox = changeRichTextBoxColor; 
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            HideCaret(); 
        }

        private void richTextBox1_MouseDown(object sender, MouseEventArgs e) //Supposed to remove blinking effect not working
        {
            richTextBox1.SelectionLength = 0;
        }

        private void richTextBox1_MouseWheel(object sender, MouseEventArgs e)
        {
           
        }
    }
}