
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace PeterPetkov.QuickMultitouchTest
{
    public partial class QuickMultitouch : WMTouchForm
    {
        private string helpMsg = "S - Save\r\nEsc - Exit app\r\n+ - Increase stroke size\r\n- - Decrease stroke size\r\nC - Clear screen\r\n? - Help\r\n";

        private TouchColor touchColor;
        private CollectionOfStrokes FinishedStrokes;
        private CollectionOfStrokes ActiveStrokes;
        private float penWidth = 25.0f;
        Graphics graphics;

        public QuickMultitouch()
        {
            InitializeComponent();
            touchColor = new TouchColor();
            ActiveStrokes = new CollectionOfStrokes();
            FinishedStrokes = new CollectionOfStrokes();
            Touchdown += OnTouchDownHandler;
            Touchup += OnTouchUpHandler;
            TouchMove += OnTouchMoveHandler;
            Paint += new PaintEventHandler(this.OnPaintHandler);
            KeyUp += MTScratchpadWMTouchForm_KeyUp;

            this.BackColor = SystemColors.Window;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            graphics = this.CreateGraphics();
        }

        private void MTScratchpadWMTouchForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.S:
                    Save();
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    penWidth += 10f;
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    penWidth -= 10f;
                    break;
                case Keys.C:
                    ClearScreen();
                    break;
                case Keys.OemQuestion:
                    MessageBox.Show(this, helpMsg, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    //MessageBox.Show(e.KeyCode.ToString());
                    break;
            }
        }

        private void Save()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var bmp = new Bitmap(this.Width, this.Height))
                {
                    this.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    bmp.Save(saveFileDialog.FileName);
                }
            }
        }

        private void ClearScreen()
        {
            ActiveStrokes.Clear();
            FinishedStrokes.Clear();
            this.Invalidate();
        }

        private void OnTouchDownHandler(object sender, WMTouchEventArgs e)
        {

            Debug.Assert(ActiveStrokes.Get(e.Id) == null);
            Stroke newStroke = new Stroke(penWidth);
            newStroke.Color = touchColor.GetColor(e.IsPrimaryContact);
            newStroke.Id = e.Id;
            ActiveStrokes.Add(newStroke);
        }

        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            Stroke stroke = ActiveStrokes.Remove(e.Id);
            Debug.Assert(stroke != null);
            FinishedStrokes.Add(stroke);
            Invalidate();
        }
        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {

            Stroke stroke = ActiveStrokes.Get(e.Id);
            Debug.Assert(stroke != null);
            stroke.Add(new Point(e.LocationX, e.LocationY));
            stroke.DrawLast(graphics);
        }

        private void OnPaintHandler(object sender, PaintEventArgs e)
        {
            FinishedStrokes.Draw(e.Graphics);
            ActiveStrokes.Draw(e.Graphics);
        }
    }

    public class TouchColor
    {

        public TouchColor()
        {
        }

        public Color GetColor(bool primary)
        {
            if (primary)
            {

                return Color.Black;
            }
            else
            {

                Color color = secondaryColors[idx];
                idx = (idx + 1) % secondaryColors.Length;

                return color;
            }
        }

        static private Color[] secondaryColors =
        {
            Color.Red,
            Color.LawnGreen,
            Color.Blue,
            Color.Cyan,
            Color.Magenta,
            Color.Yellow
        };
        private int idx = 0;
    }
}