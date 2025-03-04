using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static imageAnalyzer.clsProcessItemImageOperation;

namespace imageAnalyzer
{
    internal class clsProcessItemCanny : clsProcessZItem
    {
        [Category("변수")]
        public int thresholdLow { get; set; } = 100;

        [Category("변수")]
        public int thresholdHigh { get; set; } = 200;

        [Category("변수")]
        public int kernelSize { get; set; } = 3;

        [Category("변수")]
        public bool l2Gradientt { get; set; } = false;

        public clsProcessItemCanny() : base()
        {
            name = "CANNY";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("thresholdLow", thresholdLow);
            f.WriteValue("thresholdHigh", thresholdHigh);
            f.WriteValue("kernelSize", kernelSize);
            f.WriteValue("l2Gradientt", l2Gradientt);
        }

        public override void loadItem(SQLITEINI f)
        {
            thresholdLow = f.readValuei("thresholdLow");
            thresholdHigh = f.readValuei("thresholdHigh");
            kernelSize = f.readValuei("kernelSize");
            l2Gradientt = f.readValueb("l2Gradientt");
        }

        public override void init()
        {
            base.init();
        }

        public override void process()
        {
            try
            {
                {
                    clsDataIn i = getInFrameByName("in");
                    clsDataOut o = getFrameOutByName("out");

                    if (i.frame.Width <= 0 || i.frame.Height <= 0) return;

                    Cv2.Canny(i.frame, frameProcess, thresholdLow, thresholdHigh, kernelSize, l2Gradientt);

                    o.frame = frameProcess;
                }
                {
                }
            }
            catch
            {
                onErrorProcess();
            }
        }

        public override string generateCode(List<clsProcessZItem> items)
        {
            string paramIn01 = getOutDataCsName(items, "in");
            string paramOut01 = "canny" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.Canny(:paramIn01, :paramOut01, :thresholdLow, :thresholdHigh, :kernelSize, :l2Gradientt);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":thresholdLow", thresholdLow.ToString());
            code = code.Replace(":thresholdHigh", thresholdHigh.ToString());
            code = code.Replace(":kernelSize", kernelSize.ToString());
            code = code.Replace(":l2Gradientt", l2Gradientt.ToString().ToLower());

            return code;
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
