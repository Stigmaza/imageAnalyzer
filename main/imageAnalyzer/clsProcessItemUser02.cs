using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemUser02 : clsProcessZItem
    {
        public clsProcessItemUser02() : base()
        {
            name = "USER02";

            frameIn.Add(new clsDataIn(this, "in1"));
            frameIn.Add(new clsDataIn(this, "in2"));

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
                clsDataIn i1 = getInFrameByName("in1");
                clsDataIn i2 = getInFrameByName("in2");
                clsDataOut o = getFrameOutByName("out");

                if (i1.frame.Width <= 0 || i1.frame.Height <= 0) return;
                if (i2.frame.Width <= 0 || i2.frame.Height <= 0) return;

                Mat binary = new Mat(); 

                Cv2.Threshold(i1.frame, binary, 254, 255, ThresholdTypes.Binary);                                                                             
                int whitePixelCount01 = Cv2.CountNonZero(binary);

                Cv2.Threshold(i2.frame, binary, 254, 255, ThresholdTypes.Binary);
                int whitePixelCount02 = Cv2.CountNonZero(binary);

                if (whitePixelCount01 > whitePixelCount02)
                {
                    frameProcess = i1.frame;
                }
                else
                {
                    frameProcess = i2.frame;
                }
                
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
