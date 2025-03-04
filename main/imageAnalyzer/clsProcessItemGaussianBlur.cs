using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemGaussianBlur : clsProcessZItem
    {

        [Category("변수")]
        public int kernel_width { get; set; } = 15;


        [Category("변수")]
        public int kernel_height { get; set; } = 15;

        public clsProcessItemGaussianBlur() : base()
        {
            name = "GAUSSIAN";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("kernel_width", kernel_width);
            f.WriteValue("kernel_height", kernel_height);
        }

        public override void loadItem(SQLITEINI f)
        {
            kernel_width = f.readValuei("kernel_width", 15);
            kernel_height = f.readValuei("kernel_height", 15);
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

                Cv2.GaussianBlur(i.frame, frameProcess, new Size(kernel_width, kernel_height), 0);

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
            string paramOut01 = "blur" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.GaussianBlur(:paramIn01, :paramOut01, new Size(:kernel_width, :kernel_height), 0);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":kernel_width", kernel_width.ToString());
            code = code.Replace(":kernel_height", kernel_height.ToString());

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
