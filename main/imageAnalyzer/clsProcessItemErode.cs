using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemErode;

namespace imageAnalyzer
{
    internal class clsProcessItemErode : clsProcessZItem
    {
        public enum ERODE_TYPE { Rect, Cross, Ellipse }


        [Category("변수")]
        [TypeConverter(typeof(ERODE_TYPE))]
        public ERODE_TYPE type { get; set; } = ERODE_TYPE.Rect;


        [Category("변수")]
        public int kernel_width { get; set; } = 5;


        [Category("변수")]
        public int kernel_height { get; set; } = 5;


        public clsProcessItemErode() : base()
        {
            name = "침식";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("type", (int)type);
            f.WriteValue("kernel_width", kernel_width);
            f.WriteValue("kernel_height", kernel_height);
        }

        public override void loadItem(SQLITEINI f)
        {
            type = (ERODE_TYPE)f.readValuei("thresh");
            kernel_width = f.readValuei("kernel_width");
            kernel_height = f.readValuei("kernel_height");
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

                Mat kernel = Cv2.GetStructuringElement((MorphShapes)type, new Size(kernel_width, kernel_height));

                Cv2.Erode(i.frame, frameProcess, kernel);

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
            string paramOut01 = "erode" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Mat kernel = Cv2.GetStructuringElement(MorphShapes.:type, new Size(:kernel_width, :kernel_height));

                        Cv2.Erode(:paramIn01, :paramOut01, kernel);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":type", type.ToString());
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
