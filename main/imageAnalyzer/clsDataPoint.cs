using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace imageAnalyzer
{
    public class clsDataPoint
    {
        public enum POINT_TYPE { NONE, IN, OUT, TEMP }

        public string name { get; set; }

        public string csname { get; set; }

        public Mat frame;

        public bool inOccupie = false;

        public clsProcessZItem parent;

        // use when saved file load
        public List<string> dataToGuid;
        public List<string> dataToInName;


        public Point center { get; set; }

        public static int diameter { get; set; } = 12;

        public Rectangle boundRect { get; set; }

        public POINT_TYPE type { get; set; }

        public List<clsDataPoint> dataTo;

        

        public bool selected { get; set; }

        public clsDataPoint()
        {
            type = POINT_TYPE.NONE;
            selected = false;
        }

        public clsDataPoint( Point ptCenter, POINT_TYPE _type)
        {
            type = _type;

            center = ptCenter;
            
            boundRect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
        }

        public void initGdi()
        {
            this.name = name;
            this.center = center;

            this.csname = "";

            this.dataTo = new List<clsDataPoint>();

            this.type = type;

        }

        public void updatePosition(Rectangle parentBound, int max, int now)
        {
            if (type == POINT_TYPE.NONE)
                return;

            int circleSpacing = parentBound.Height / max;
            int y = parentBound.Top + now * circleSpacing + circleSpacing / 2;

            Point ptCenter;

            if (type == POINT_TYPE.IN)
            {
                ptCenter = new Point(parentBound.Left, y);
            }
            else
            {
                ptCenter = new Point(parentBound.Right, y);
            }

            center = ptCenter;

            boundRect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);

        }


        public void drawBody(Graphics g)
        {
            Brush brush;

            if (selected)
                brush = new SolidBrush(Color.Red);
            else
                brush = new SolidBrush(Color.Black);

            g.FillEllipse(brush, center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);

            brush.Dispose();
        }
        public void drawConnection(Graphics g)
        {
            for (int i = 0; i < dataTo.Count; i++)
            {
                Point start = center;
                Point end = dataTo[i].center;

                Point controlPoint1 = new Point((start.X + end.X) / 2, start.Y);
                Point controlPoint2 = new Point((start.X + end.X) / 2, end.Y);

                g.DrawBezier(Pens.Red, start, controlPoint1, controlPoint2, end);
            }
        }


        public bool hitTest(Point location, int marginx, int marginy)
        {
            Rectangle t = new Rectangle(boundRect.Left - marginx, boundRect.Top - marginy, boundRect.Width + marginx * 2, boundRect.Height + marginy * 2);

            if (t.Contains(location))
            {
                return true;
            }

            return false;
        }

        public void updatePosition(int deltaX, int deltaY)
        {
            center = new Point(center.X + deltaX, center.Y + deltaY);

            boundRect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
        }

        public clsDataPoint addConnectToTemp(int x, int y)
        {
            clsDataPoint item = new clsDataPoint( new Point(x, y), POINT_TYPE.TEMP);

            dataTo.Add(item);

            return item;
        }
    }
}
