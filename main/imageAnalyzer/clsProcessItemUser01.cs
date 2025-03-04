using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemUser01 : clsProcessZItem
    {
        public clsProcessItemUser01() : base()
        {
            name = "USER01";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            //f.WriteValue("thresh", thresh);
        }

        public override void loadItem(SQLITEINI f)
        {
            //thresh = f.readValued("thresh");
        }


        public override void init()
        {
            base.init();
        }

        public override void process()
        {
            try
            {
                clsDataIn i = getInFrameByName("in");
                clsDataOut o = getFrameOutByName("out");

                if (i.frame.Width <= 0 || i.frame.Height <= 0) return;

                frameProcess = i.frame;

                int rectWidth = 200;
                int rectHeight = 100;
                Point topLeft = new Point((frameProcess.Width - rectWidth) / 2, (frameProcess.Height - rectHeight) / 2);
                Point bottomRight = new Point(topLeft.X + rectWidth, topLeft.Y + rectHeight);

                Cv2.Rectangle(frameProcess, topLeft, bottomRight, new Scalar(0, 0, 255), 4);

                o.frame = frameProcess;
            }
            catch
            {
                onErrorProcess();
            }
        }

        public override void afterProcess()
        {
            base.afterProcess();
        }

        public override void finalize()
        {
            base.finalize();
        }
    }
}
