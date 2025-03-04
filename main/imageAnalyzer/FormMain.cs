using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using static imageAnalyzer.clsProcessItemBitwise;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace imageAnalyzer
{
    public partial class FormMain : Form
    {
        clsProcessManager processList = new clsProcessManager();

        clsCameraReader clsCameraReader = new clsCameraReader();


        bool bBoxSelectItem = false;
        bool bDragItem = false;
        bool bPanning = false;
        bool moveAfterMouseDown = false;

        Point dragStartPoint = Point.Empty;
        Rectangle selectBox;

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CenterToScreen();

            clsCameraReader.setup01CameraInfo(listBox1);

            pictureBox.Width = 5000;
            pictureBox.Height = 5000;
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);

            pictureBox.Paint += PictureBox_Paint;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            processList.draw(e.Graphics);

            if (bBoxSelectItem)
            {
                Graphics g = e.Graphics;

                using (Pen pen = new Pen(Color.Red, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(pen, selectBox);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (clsCameraReader.isRun() == false)
                clsCameraReader.start(processList);
            else
                clsCameraReader.stop();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            clsCameraReader.stop(true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(button4, button4.Width / 2, button4.Height);
            pictureBox.Invalidate();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            processList.save("abc.sqlite");

            pictureBox.Invalidate();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            processList.load("abc.sqlite");

            pictureBox.Invalidate();
        }

        private void pbHome_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bool shiftPressed = Control.ModifierKeys == Keys.Shift;

                processList.onMouseDown(e, shiftPressed);

                if (processList.isProcessSelected())
                {
                    propertyGrid1.SelectedObject = processList.getSelectedProcess();

                    bBoxSelectItem = false;
                    bDragItem = true;
                    bPanning = false;
                }
                else
                {
                    bBoxSelectItem = true;
                    bDragItem = true;
                    bPanning = false;

                    selectBox = new Rectangle(e.Location.X, e.Location.Y, 0, 0);
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                bBoxSelectItem = false;
                bDragItem = false;
                bPanning = true;
            }

            moveAfterMouseDown = false;
            dragStartPoint = new Point(e.Location.X, e.Location.Y);

            pictureBox.Invalidate();
        }

        private void pbHome_MouseMove(object sender, MouseEventArgs e)
        {
            if (bBoxSelectItem)
            {
                int x = Math.Min(e.X, dragStartPoint.X);
                int y = Math.Min(e.Y, dragStartPoint.Y);
                int width = Math.Abs(e.X - dragStartPoint.X);
                int height = Math.Abs(e.Y - dragStartPoint.Y);

                selectBox = new Rectangle(x, y, width, height);
            }

            if (bDragItem)
            {
                if (processList.isProcessSelected())
                {
                    processList.onMouseMove(e);
                }
            }

            if (bPanning)
            {
                Cursor = Cursors.Hand;

                int deltaX = dragStartPoint.X - e.Location.X;
                int deltaY = dragStartPoint.Y - e.Location.Y;

                panel.AutoScrollPosition = new Point(Math.Abs(panel.AutoScrollPosition.X) + deltaX, Math.Abs(panel.AutoScrollPosition.Y) + deltaY);
            }

            moveAfterMouseDown = true;
            pictureBox.Invalidate();
        }

        private void pbHome_MouseUp(object sender, MouseEventArgs e)
        {
            processList.onMouseUp(e);

            if (bBoxSelectItem)
            {
                processList.selectProcessCardByRectangle(selectBox);
            }

            bBoxSelectItem = false;
            bDragItem = false;
            bPanning = false;

            Cursor = Cursors.Default;

            pictureBox.Invalidate();

            if (moveAfterMouseDown == false)
            {
                bool showMenu = false;

                if (processList.isPointSelected())
                {
                    연결점삭제ToolStripMenuItem.Visible = true;

                    showMenu = true;
                }
                else
                {
                    연결점삭제ToolStripMenuItem.Visible = false;

                    clsProcessZItem item = processList.getSelectedProcess();

                    if (item != null && e.Button == MouseButtons.Right)
                        showMenu = true;
                }

                if (showMenu)
                    contextMenuStrip2.Show(pictureBox, e.Location);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "scan time : " + processList.scantime + " ms";

            pictureBox.Invalidate();

            if (clsCameraReader.isRun())
                button1.Text = "중지";
            else
                button1.Text = "시작";
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cAMERAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemCam(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 잘라내기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemRoi(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void gRAYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemColorChange(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 이진화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemBinary(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 템플릿ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemTemplateMaching(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 이미지저장ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemSaveImg(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 채널분리ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemChannelSplit(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 미리보기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemPreview(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 파일읽기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemLoadImg(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 색범위ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemRange(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 이미지연산ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemImageOperation(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 비트연산ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemBitwise(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 사용자01ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemUser01(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 침식ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemErode(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 팽창ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemDilate(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 크기변경ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemResize(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 가우시안블러ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemGaussianBlur(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 소벨필ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemSobel(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 샤르ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemScharr(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 라플라시안ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemLaplacian(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 메디안블러ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemMedianBlur(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 양방향흐림ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemBilateralFilter(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 박스필터ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemBoxFilter(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 캐니ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemCanny(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemContours(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 사용자02ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemUser02(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void 평준화ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemEqualizer(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void cLAHEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemClahe(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void nOMALIZEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessNomalize(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }
        private void 채널병합ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.addProcess(new clsProcessItemChannelJoin(), -panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
        }

        private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                clsProcessZItem item = processList.selectProcess(e);

                FormPreview p = new FormPreview(item);

                p.ShowDialog();
            }
        }

        private void 소스생성ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //clsProcessZItem item = processList.selectProcess(e);

            //Clipboard.SetText(item.generateCode());

            //MessageBox.Show("클립보드에 저장되었습니다.");

            // --------------------------------------------------------

            //clsProcessZItem item = processList.getSelectedProcess();

            //if (item != null)
            //{
            //    textBox1.Text = item.generateCode(null);
            //}

            // --------------------------------------------------------

            clsProcessZItem item = processList.getSelectedProcess();

            List<clsProcessZItem> list = processList.makeTree(item);

            List<string> r = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                string t = list[i].depth.ToString("00") + " " + list[i].name;

                r.Add(t);
            }

            //textBox1.Text = processList.generateCode(list);

            Clipboard.SetText(processList.generateCode(list));

            MessageBox.Show("클립보드에 복사");
        }

        private void 삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processList.removeSelectedProcess();
            pictureBox.Invalidate();
        }

        private void 연결점삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clsDataPoint con = processList.getSelectedPoint();

            if (con != null)
            {
                clsProcessZItem process = processList.getSelectedProcess();

                if (process != null)
                {
                    if (con is clsDataOut)
                    {
                        process.removeDataOutConnection(con);
                    }
                    else
                    {
                        processList.clearFromConnection(process, con.name);
                    }
                }
            }
        }
    }
}
