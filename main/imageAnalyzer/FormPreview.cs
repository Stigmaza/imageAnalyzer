using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web.Hosting;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace imageAnalyzer
{
    public partial class FormPreview : Form
    {
        clsProcessZItem item;

        Point positionMouse = new Point(0, 0);

        bool dragging = false;
        Point dragStart = new Point(0, 0);
        Point dragEnd = new Point(0, 0);

        Rectangle selectBox = new Rectangle(0, 0, 0, 0);

        List<Rectangle> boxList = new List<Rectangle>();

        public FormPreview(clsProcessZItem _item)
        {
            InitializeComponent();

            item = _item;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterToParent();

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (item != null)
            {
                Graphics g = e.Graphics;

                Rectangle contentRect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);

                Image img = null;

                if (item is clsProcessItemRoi || item is clsProcessItemRange)
                    img = item.contentBefore;
                else
                    img = item.contentImage;

                int sx = scale(pictureBox1.Width, img.Width, positionMouse.X);
                int sy = scale(pictureBox1.Height, img.Height, positionMouse.Y);

                label1.Text = positionMouse.X.ToString() + ", " + positionMouse.Y.ToString() + " => " + sx.ToString() + ", " + sy.ToString();

                g.DrawImage(img, contentRect);

                if (dragging == false)
                {
                    g.DrawLine(Pens.Red, 0, positionMouse.Y, pictureBox1.Width, positionMouse.Y);

                    g.DrawLine(Pens.Red, positionMouse.X, 0, positionMouse.X, pictureBox1.Height);
                }

                g.DrawRectangle(Pens.Red, selectBox);

                foreach (var item in boxList)
                {
                    g.DrawRectangle(Pens.Red, item);
                }
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragStart = e.Location;
                dragEnd = e.Location;
            }
            else if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < boxList.Count; i++)
                {
                    if (boxList[i].Contains(e.Location))
                    {
                        boxList.RemoveAt(i);
                        break;
                    }
                }
            }

            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            positionMouse = e.Location;

            if (dragging)
            {
                dragEnd = e.Location;

                int x = Math.Min(dragEnd.X, dragStart.X);
                int y = Math.Min(dragEnd.Y, dragStart.Y);
                int width = Math.Abs(dragEnd.X - dragStart.X);
                int height = Math.Abs(dragEnd.Y - dragStart.Y);
                selectBox = new Rectangle(x, y, width, height);

            }

            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                boxList.Add(selectBox);

                selectBox = new Rectangle();
                dragging = false;
            }

            pictureBox1.Invalidate();
        }


        private int scale(float s1, float s2, float p)
        {
            if (s1 == 0)
                return 0;

            return (int)(s2 * p / s1);
        }

        private bool applyParamRoi(clsProcessItemRoi roi)
        {
            if (boxList.Count != 1)
            {
                MessageBox.Show("선택 영역은 한개여야 합니다");
                return false;
            }

            selectBox = boxList[0];

            int sx = scale(pictureBox1.Width, item.contentBefore.Width, selectBox.X);
            int sy = scale(pictureBox1.Height, item.contentBefore.Height, selectBox.Y);

            int sw = scale(pictureBox1.Width, item.contentBefore.Width, selectBox.Width);
            int sh = scale(pictureBox1.Height, item.contentBefore.Height, selectBox.Height);

            roi.x = sx;
            roi.y = sy;
            roi.width = sw;
            roi.height = sh;

            return true;
        }

        private bool applyParamRange(clsProcessItemRange range)
        {
            if (boxList.Count < 1)
            {
                MessageBox.Show("영역을 선택하세요");
                return false;
            }

            for (int boxIndex = 0; boxIndex < boxList.Count; boxIndex++)
            {
                int sx = scale(pictureBox1.Width, item.contentBefore.Width, boxList[boxIndex].X);
                int sy = scale(pictureBox1.Height, item.contentBefore.Height, boxList[boxIndex].Y);

                int sw = scale(pictureBox1.Width, item.contentBefore.Width, boxList[boxIndex].Width);
                int sh = scale(pictureBox1.Height, item.contentBefore.Height, boxList[boxIndex].Height);

                Rect rect = new Rect(sx, sy, sw, sh);

                clsDataIn inMat = range.getInFrameByName("in");

                Mat t = inMat.frame.SubMat(rect);

                Mat[] channels = Cv2.Split(t);

                for (int i = 0; i < channels.Length; i++)
                {
                    double min, max;

                    channels[i].MinMaxIdx(out min, out max);

                    // 첫번째 영역은 1:1 대입
                    if (boxIndex == 0)
                    {
                        if (i == 0)
                        {
                            range.ch_1_min = (int)min;
                            range.ch_1_max = (int)max;
                        }

                        if (i == 1)
                        {
                            range.ch_2_min = (int)min;
                            range.ch_2_max = (int)max;
                        }

                        if (i == 2)
                        {
                            range.ch_3_min = (int)min;
                            range.ch_3_max = (int)max;
                        }
                    }

                    // 이후 영역은 비교하며 대입
                    if (boxIndex > 0)
                    {
                        if (i == 0)
                        {
                            if (range.ch_1_min > (int)min) range.ch_1_min = (int)min;
                            if (range.ch_1_max < (int)max) range.ch_1_max = (int)max;
                        }

                        if (i == 1)
                        {
                            if (range.ch_2_min > (int)min) range.ch_2_min = (int)min;
                            if (range.ch_2_max < (int)max) range.ch_2_max = (int)max;
                        }

                        if (i == 2)
                        {
                            if (range.ch_3_min > (int)min) range.ch_3_min = (int)min;
                            if (range.ch_3_max < (int)max) range.ch_3_max = (int)max;
                        }
                    }
                }
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (item is clsProcessItemRoi roi)
            {
                if (applyParamRoi(roi) == false)
                    return;
            }

            if (item is clsProcessItemRange range)
            {
                if (applyParamRange(range) == false)
                    return;
            }

            DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

        }
    }
}
