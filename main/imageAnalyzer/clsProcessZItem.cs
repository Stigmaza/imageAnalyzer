using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace imageAnalyzer
{

    public interface ISaveItem
    {
        void saveItem(SQLITEINI f);
    }

    public interface ILoadItem
    {
        void loadItem(SQLITEINI f);
    }

    public interface IInit
    {
        void init();
    }

    public interface IProcess
    {
        void process();
    }

    public interface IAfterProcess
    {
        void afterProcess();
    }

    public interface IFinalize
    {
        void finalize();
    }

    public interface IRangeMax
    {
        int rangeMax(string name);
    }

    public class clsProcessZItem : IInit, IProcess, IAfterProcess, IFinalize, ISaveItem, ILoadItem, IRangeMax
    {
        protected FOETC etc = new FOETC();

        [Category("객체속성")]
        public string name { get; set; }

        [Category("객체속성")]
        [ReadOnly(true)]
        public string guid { get; set; }

        [Browsable(false)]
        public int depth { get; set; } = -1;

        // -------------------------------

        [Browsable(false)]
        public List<clsDataIn> frameIn { get; set; } = new List<clsDataIn>();

        [ReadOnly(true)]
        public Mat frameProcess { get; set; } = new Mat();

        [Browsable(false)]
        public List<clsDataOut> frameOut { get; set; } = new List<clsDataOut>();

        //[Browsable(false)]
        //public List<clsDataOut> frameOut { get; set; } = new List<clsDataOut>();

        public virtual void saveItem(SQLITEINI f)
        {
        }

        public virtual void loadItem(SQLITEINI f)
        {
        }

        public virtual void init()
        {
        }

        public virtual void process()
        {
        }

        public virtual string generateCode(List<clsProcessZItem> items)
        {
            return string.Empty;
        }

        public virtual void afterProcess()
        {
            foreach (var item in frameOut)
            {
                item.transData();
            }

            if (frameProcess.Width > 0 && frameProcess.Height > 0 )
                contentImage = BitmapConverter.ToBitmap(frameProcess);
        }

        public virtual void finalize()
        {
        }

        public virtual int rangeMax(string name)
        {
            return 1000;
        }

        public bool isNextItem(clsProcessZItem item)
        {
            for (int frameIndex = 0; frameIndex < frameOut.Count; frameIndex++)
            {
                List<clsDataPoint> dataTo = frameOut[frameIndex].dataTo;
                for (int toIndex = 0; toIndex < dataTo.Count; toIndex++)
                {
                    if( dataTo[toIndex].parent.guid == item.guid )
                        return true;
                }
            }

            return false;
        }

        public string getOutDataCsName(clsProcessZItem dstItem, string dstInName)
        {
            if (this.guid != dstItem.guid)
            {
                for (int frameIndex = 0; frameIndex < frameOut.Count; frameIndex++)
                {
                    List<clsDataPoint> dataTo = frameOut[frameIndex].dataTo;
                    for (int toIndex = 0; toIndex < dataTo.Count; toIndex++)
                    {
                        if (dataTo[toIndex].parent.guid == dstItem.guid)
                        {
                            if (dataTo[toIndex].name == dstInName)
                                return frameOut[frameIndex].csname;
                        }
                    }
                }
            }
            return string.Empty;
        }

        public string getOutDataCsName(List<clsProcessZItem> srcItems, string dstInName )
        {
            for (int i = 0; i < srcItems.Count; i++)
            {
                string t = srcItems[i].getOutDataCsName(this, dstInName);

                if (t != string.Empty)
                    return t;
            }

            return string.Empty;
        }

        // ---------------------------------------------------------------------------------------------

        public clsProcessZItem()
        {
            depth = -1;

            guid = newGuid();
        }

        private string newGuid()
        {
            Guid guid = Guid.NewGuid();
            string shortGuid = Convert.ToBase64String(guid.ToByteArray());
            shortGuid = shortGuid.Replace("/", "").Replace("+", "").Substring(0, 10);

            return shortGuid;
        }

        public clsDataIn getInFrameByName(string name)
        {
            foreach (var item in frameIn.Where(x => x.name == name))
            {
                return item;
            }

            return null;
        }

        public clsDataOut getFrameOutByName(string name)
        {
            foreach (var item in frameOut.Where(x => x.name == name))
            {
                return item;
            }

            return null;
        }

        public void addNextProcess(string outName, clsProcessZItem item, string inName)
        {
            clsDataOut o = getFrameOutByName(outName);

            if (o != null)
            {
                clsDataIn i = item.getInFrameByName(inName);

                o.addDataTo(i);
            }
        }

        public void removeDataOutConnection(clsDataPoint con)
        {
            for (int i = 0; i < frameOut.Count; i++)
            {
                if (frameOut[i] == con)
                {
                    frameOut[i].dataTo.Clear();

                    return;
                }
            }
        }

        public void removeDataOutConnection(clsProcessZItem nextProcess, string name)
        {

            foreach (var item in frameOut)
            {
                int t = item.dataTo.RemoveAll(x => x.parent != null && x.parent.guid == nextProcess.guid && x.name == name);
            }

        }

        public void reOrderDepth(int d)
        {
            d = Math.Max(this.depth, d);

            this.depth = d;

            int depthNext = d + 1;

            foreach (var dataOut in frameOut)
            {
                foreach (var dataIn in dataOut.dataTo)
                {
                    if (dataIn.parent != null)
                    {
                        int t = dataIn.parent.depth;

                        if (t < depthNext)
                        {
                            t = depthNext;

                            dataIn.parent.depth = t;

                            dataIn.parent.reOrderDepth(t);
                        }
                    }
                }
            }
        }        

        protected void onErrorProcess()
        {
            int width = 150;
            int height = 100;

            frameProcess = new Mat(new OpenCvSharp.Size(width, height), MatType.CV_8UC3, Scalar.White);

            OpenCvSharp.Point pt1 = new OpenCvSharp.Point(0, 0);
            OpenCvSharp.Point pt2 = new OpenCvSharp.Point(width, height);
            OpenCvSharp.Point pt3 = new OpenCvSharp.Point(0, height);
            OpenCvSharp.Point pt4 = new OpenCvSharp.Point(width, 0);

            Cv2.Line(frameProcess, pt1, pt2, new Scalar(0, 0, 255), 2);
            Cv2.Line(frameProcess, pt3, pt4, new Scalar(0, 0, 255), 2);
        }

        // ---------------------------------------------------------------------------------------------

        public int zOrder = -1;

        public const int carcWidth = 300;
        public const int cardHeight= 170;

        [Browsable(false)]
        public Rectangle bounds { get; set; }
        private Font titleFont { get; set; }
        private Brush titleBrushSelected { get; set; }
        private Brush titleBrushUnSelected { get; set; }
        private Pen borderPen { get; set; }

        [Browsable(false)]
        public Image contentBefore { get; set; }

        [Browsable(false)]
        public Image contentImage { get; set; }

        protected void initGdi()
        {
            titleFont = new Font("굴림", 12, FontStyle.Bold);
            titleBrushSelected = Brushes.Black;
            titleBrushUnSelected = Brushes.White;
            borderPen = new Pen(Color.DarkGray, 2);
            contentBefore = new Bitmap(100, 100);
            contentImage = new Bitmap(100, 100);
        }

        public void updatePosition(int x, int y)
        {
            bounds = new Rectangle(x, y, (int)(carcWidth), (int)(cardHeight));

            for (int i = 0; i < frameIn.Count; i++)
            {
                frameIn[i].updatePosition(this.bounds, frameIn.Count, i);
            }

            for (int i = 0; i < frameOut.Count; i++)
            {
                frameOut[i].updatePosition(this.bounds, frameOut.Count, i);
            }
        }

        private void drawShadow(Graphics g)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddRectangle(new Rectangle(bounds.X + 5, bounds.Y + 5, bounds.Width, bounds.Height));
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(60, Color.Black);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        private void drawRoundedRectangle(Graphics g, Brush brush, Rectangle bounds, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);
                path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);
                path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
                g.DrawPath(borderPen, path);
            }
        }


        private void drawInoutPoint(Graphics g)
        {
            foreach (var item in frameIn)
            {
                item.drawBody(g);
            }

            foreach (var item in frameOut)
            {
                item.drawBody(g);
            }
        }

        public void drawBody(Graphics g)
        {
            drawShadow(g);

            int titleHeight = (int)g.MeasureString(name, titleFont).Height;

            Rectangle titleRect = new Rectangle(bounds.Left + 10, bounds.Top + 10, bounds.Width - 20, titleHeight);

            if (selected)
            {
                drawRoundedRectangle(g, new SolidBrush(Color.FromArgb(0, 255, 0)), bounds, 10);
                g.DrawString(name, titleFont, titleBrushSelected, titleRect);
            }
            else
            {
                drawRoundedRectangle(g, new SolidBrush(Color.FromArgb(0, 120, 215)), bounds, 10);
                g.DrawString(name, titleFont, titleBrushUnSelected, titleRect);
            }            

            if (contentImage != null)
            {
                Rectangle contentRect = new Rectangle(bounds.Left + 10, bounds.Top + titleHeight + 20, bounds.Width - 20, bounds.Height - titleHeight - 30);
                g.DrawImage(contentImage, contentRect);
            }

            drawInoutPoint(g);
        }

        public void drawConnection(Graphics g)
        {
            foreach (var item in frameIn)
            {
                item.drawConnection(g);
            }

            foreach (var item in frameOut)
            {
                item.drawConnection(g);
            }
        }

        public void clearPointSelected()
        {
            foreach (var item in frameIn)
            {
                item.selected = false;
            }

            foreach (var item in frameOut)
            {
                item.selected = false;
            }
        }

        public void clearTempConnection()
        {
            List<clsDataPoint> listToDelte = new List<clsDataPoint>();

            foreach (var item in frameIn)
            {
                item.dataTo.RemoveAll(x => x.type == clsDataPoint.POINT_TYPE.TEMP);
            }

            foreach (var item in frameOut)
            {
                item.dataTo.RemoveAll(x => x.type == clsDataPoint.POINT_TYPE.TEMP);
            }
        }

        public void clearDataOut(string guid, string name)
        {
            foreach (var item in frameOut)
            {
                item.dataTo.RemoveAll(x=> x.parent != null && x.parent.guid == guid && x.name == name);
            }
        }

        // ---------------------------------------------------------------------------------------------

        public Point dragStartPoint = Point.Empty;
        public Point dragDelta = Point.Empty;
        public bool selected { get; set; }

        public bool hitTestProcess(Point location)
        {
            return bounds.Contains(location);
        }

        public clsDataPoint hitTestPoint(Point location, int marginx = 0, int marginy = 0)
        {
            foreach (var item in frameIn)
            {
                if (item.hitTest(location, marginx, marginy))
                {
                    return item;
                }
            }

            foreach (var item in frameOut)
            {
                if (item.hitTest(location, marginx, marginy))
                {
                    return item;
                }
            }

            return null;
        }

        public Point getDeltaPoint(Point location)
        {
            return new Point(location.X - bounds.X, location.Y - bounds.Y);
        }

        public void updatePosition(Point ptOld, Point ptNew, Point delta)
        {
            int deltaX = bounds.Left;
            int deltaY = bounds.Top;

            bounds = new Rectangle(ptNew.X - delta.X, ptNew.Y - delta.Y, bounds.Width, bounds.Height);

            deltaX = bounds.X - deltaX;
            deltaY = bounds.Y - deltaY;

            foreach (var item in frameIn)
            {
                item.updatePosition(deltaX, deltaY);
            }

            foreach (var item in frameOut)
            {
                item.updatePosition(deltaX, deltaY);
            }
        }

    }
}
