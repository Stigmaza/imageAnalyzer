using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemScharr;
using static imageAnalyzer.clsProcessItemSobel;

namespace imageAnalyzer
{
    internal class clsProcessItemSobel : clsProcessZItem
    {
        public enum SOBEL_OPTION { CV_8U = 0, CV_8S = 1, CV_16U = 2, CV_16S = 3, CV_32S = 4, CV_32F = 5, CV_64F = 6 }

        public enum SOBEL_DIRECTION { HORIZONTAL, VERTICAL }

        [Category("변수")]
        [TypeConverter(typeof(SOBEL_OPTION))]
        public SOBEL_OPTION ddepth { get; set; } = SOBEL_OPTION.CV_64F;

        [Category("변수")]
        [TypeConverter(typeof(SOBEL_DIRECTION))]
        public SOBEL_DIRECTION direction { get; set; } = SOBEL_DIRECTION.HORIZONTAL;

        [Category("변수")]
        [TypeConverter(typeof(BorderTypes))]
        public BorderTypes borderType { get; set; } = BorderTypes.Default;

        [Category("변수")]
        public int kernel_size { get; set; } = 3;

        [Category("변수")]
        public int kernel_scale { get; set; } = 1;

        [Category("변수")]
        public int kernel_delta { get; set; } = 0;

        public clsProcessItemSobel() : base()
        {
            name = "SOBEL";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("ddepth", (int)ddepth);
            f.WriteValue("direction", (int)direction);
            f.WriteValue("borderType", (int)borderType);

            f.WriteValue("kernel_size", kernel_size);
            f.WriteValue("kernel_scale", kernel_scale);
            f.WriteValue("kernel_delta", kernel_delta);
        }

        public override void loadItem(SQLITEINI f)
        {
            ddepth = (SOBEL_OPTION)f.readValuei("ddepth");
            direction = (SOBEL_DIRECTION)f.readValuei("direction");
            borderType = (BorderTypes)f.readValuei("borderType");

            kernel_size = f.readValuei("kernel_size", 3);
            kernel_scale = f.readValuei("kernel_scale", 1);
            kernel_delta = f.readValuei("kernel_delta", 0);
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

                    if (direction == SOBEL_DIRECTION.HORIZONTAL) Cv2.Sobel(i.frame, frameProcess, (MatType)(int)ddepth, 1, 0, ksize: kernel_size, scale: kernel_scale, delta: kernel_delta, borderType);
                    if (direction == SOBEL_DIRECTION.VERTICAL) Cv2.Sobel(i.frame, frameProcess, (MatType)(int)ddepth, 0, 1, ksize: kernel_size, scale: kernel_scale, delta: kernel_delta, borderType);

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
            string paramOut01 = "scharr" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        if (SOBEL_DIRECTION.:direction == SOBEL_DIRECTION.HORIZONTAL) Cv2.Sobel(:paramIn01, :paramOut01, MatType.:ddepth, 1, 0, ksize: :kernel_size, scale: :kernel_scale, delta: :kernel_delta, BorderTypes.:borderType);
                        if (SOBEL_DIRECTION.:direction == SOBEL_DIRECTION.VERTICAL)   Cv2.Sobel(:paramIn01, :paramOut01, MatType.:ddepth, 0, 1, ksize: :kernel_size, scale: :kernel_scale, delta: :kernel_delta, BorderTypes.:borderType);

                        :paramOut01.ConvertTo(:paramOut01, MatType.CV_8UC1);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":direction", direction.ToString());
            code = code.Replace(":ddepth", ddepth.ToString());
            code = code.Replace(":kernel_size", kernel_size.ToString());
            code = code.Replace(":kernel_scale", kernel_scale.ToString());
            code = code.Replace(":kernel_delta", kernel_delta.ToString());
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
