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
    internal class clsProcessItemResize : clsProcessZItem
    {
        [Category("변수")]
        public double sizeto_width { get; set; } = 255;

        [Category("변수")]
        public double sizeto_height { get; set; } = 255;

        public clsProcessItemResize() : base()
        {
            name = "RESIZE";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("sizeto_width", sizeto_width);
            f.WriteValue("sizeto_height", sizeto_height);
        }

        public override void loadItem(SQLITEINI f)
        {
            sizeto_width = f.readValuei("sizeto_width");
            sizeto_height = f.readValuei("sizeto_height");
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

                    Cv2.Resize(i.frame, frameProcess, new Size(sizeto_width, sizeto_height));

                    o.frame = frameProcess;
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
            string paramOut01 = "resize" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.Resize(:paramIn01, :paramOut01, new Size(:sizeto_width, :sizeto_height));
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":sizeto_width", sizeto_width.ToString());
            code = code.Replace(":sizeto_height", sizeto_height.ToString());

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
