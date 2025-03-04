using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemSobel;

namespace imageAnalyzer
{
    internal class clsProcessItemLaplacian : clsProcessZItem
    {
        public enum LAPLACIAN_OPTION { CV_8U = 0, CV_8S = 1, CV_16U = 2, CV_16S = 3, CV_32S = 4, CV_32F = 5, CV_64F = 6 }

        [Category("변수")]
        [TypeConverter(typeof(LAPLACIAN_OPTION))]
        public LAPLACIAN_OPTION ddepth { get; set; } = LAPLACIAN_OPTION.CV_64F;

        [Category("변수")]
        [TypeConverter(typeof(BorderTypes))]
        public BorderTypes borderType { get; set; } = BorderTypes.Default;

        [Category("변수")]
        public int kernel_size { get; set; } = 3;

        [Category("변수")]
        public int kernel_scale { get; set; } = 1;

        [Category("변수")]
        public int kernel_delta { get; set; } = 0;

        public clsProcessItemLaplacian() : base()
        {
            name = "LAPLACIAN";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("ddepth", (int)ddepth);
            f.WriteValue("borderType", (int)borderType);

            f.WriteValue("kernel_size", kernel_size);
            f.WriteValue("kernel_scale", kernel_scale);
            f.WriteValue("kernel_delta", kernel_delta);
        }

        public override void loadItem(SQLITEINI f)
        {
            ddepth = (LAPLACIAN_OPTION)f.readValuei("ddepth");
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

                    Cv2.Laplacian(i.frame, frameProcess, (MatType)(int)ddepth, ksize: kernel_size, scale: kernel_scale, delta: kernel_delta, borderType);

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
            string paramOut01 = "lap" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.Laplacian(:paramIn01, :paramOut01, MatType.:ddepth, ksize: :kernel_size, scale: :kernel_scale, delta: :kernel_delta, BorderTypes.:borderType);

                        :paramOut01.ConvertTo(:paramOut01, MatType.CV_8UC1);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

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
