using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace imageAnalyzer
{
    internal class clsProcessItemMedianBlur : clsProcessZItem
    {

        [Category("변수")]
        public int kernel_size { get; set; } = 9;

        public clsProcessItemMedianBlur() : base()
        {
            name = "MEDIAN";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("kernel_size", kernel_size);
        }

        public override void loadItem(SQLITEINI f)
        {
            kernel_size = f.readValuei("kernel_size");
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

                Cv2.MedianBlur(i.frame, frameProcess, kernel_size);

                o.frame = frameProcess;
            }
            catch
            {
                onErrorProcess();
            }
        }

        public override string generateCode(List<clsProcessZItem> items)
        {
            string paramIn01 = getOutDataCsName(items, "in");
            string paramOut01 = "mblur" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.MedianBlur(:paramIn01, :paramOut01, :kernel_size);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":kernel_size", kernel_size.ToString());

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
