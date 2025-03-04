using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemBoxFilter : clsProcessZItem
    {
        public enum BOXFILTER_OPTION { CV_8U = 0, CV_8UC3 = 16, CV_8S = 1, CV_16U = 2, CV_16S = 3, CV_32S = 4, CV_32F = 5, CV_64F = 6 }

        [Category("변수")]
        [TypeConverter(typeof(BOXFILTER_OPTION))]
        public BOXFILTER_OPTION ddepth { get; set; } = BOXFILTER_OPTION.CV_8UC3;

        [Category("변수")]
        [TypeConverter(typeof(BorderTypes))]
        public BorderTypes borderType { get; set; } = BorderTypes.Default;

        [Category("변수")]
        public int kernel_width { get; set; } = 9;

        [Category("변수")]
        public int kernel_height { get; set; } = 9;

        [Category("변수")]
        public int anchor_x { get; set; } = -1;

        [Category("변수")]
        public int anchor_y { get; set; } = -1;

        [Category("변수")]
        public bool normalize { get; set; } = true;

        public clsProcessItemBoxFilter() : base()
        {
            name = "BOXFILTER";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("ddepth", (int)ddepth);
            f.WriteValue("borderType", (int)borderType);

            f.WriteValue("kernel_width", kernel_width);
            f.WriteValue("kernel_height", kernel_height);
            f.WriteValue("anchor_x", anchor_x);
            f.WriteValue("anchor_y", anchor_y);
            f.WriteValue("normalize", normalize);
        }

        public override void loadItem(SQLITEINI f)
        {
            ddepth = (BOXFILTER_OPTION)f.readValuei("ddepth");
            borderType = (BorderTypes)f.readValuei("borderType");

            kernel_width = f.readValuei("kernel_width");
            kernel_height = f.readValuei("kernel_height");
            anchor_x = f.readValuei("anchor_x");
            anchor_y = f.readValuei("anchor_y");
            normalize = f.readValueb("normalize");
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

                    Cv2.BoxFilter(i.frame, frameProcess, (MatType)(int)ddepth, new Size(kernel_width, kernel_height), new Point(anchor_x, anchor_y), normalize, borderType);

                    frameProcess.ConvertTo(frameProcess, MatType.CV_8UC1);

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
            string paramOut01 = "boxFilter" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.BoxFilter(:paramIn01, :paramOut01, MatType.:ddepth, new Size(:kernel_width, :kernel_height), new Point(:anchor_x, :anchor_y), :normalize, BorderTypes.:borderType);

                        :paramOut01.ConvertTo(:paramOut01, MatType.CV_8UC1);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":ddepth", ddepth.ToString());
            code = code.Replace(":kernel_width", kernel_width.ToString());
            code = code.Replace(":kernel_height", kernel_height.ToString());
            code = code.Replace(":anchor_x", anchor_x.ToString());
            code = code.Replace(":anchor_y", anchor_y.ToString());
            code = code.Replace(":normalize", normalize.ToString().ToLower());
            code = code.Replace(":borderType", borderType.ToString());

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
