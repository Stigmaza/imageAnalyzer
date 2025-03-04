using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{

    internal class clsProcessItemBilateralFilter : clsProcessZItem
    {

        [Category("변수")]
        public int diameter { get; set; } = 9;

        [Category("변수")]
        public int sigmaColor { get; set; } = 3;

        [Category("변수")]
        public int sigmaSpace { get; set; } = 3;

        [Category("변수")]
        [TypeConverter(typeof(BorderTypes))]
        public BorderTypes borderType { get; set; } = BorderTypes.Default;

        public clsProcessItemBilateralFilter() : base()
        {
            name = "BILATERAL";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("diameter", diameter);
            f.WriteValue("sigmaColor", sigmaColor);
            f.WriteValue("sigmaSpace", sigmaSpace);
            f.WriteValue("borderType", (int)borderType);
        }

        public override void loadItem(SQLITEINI f)
        {
            diameter = f.readValuei("diameter");
            sigmaColor = f.readValuei("sigmaColor");
            sigmaSpace = f.readValuei("sigmaSpace");
            borderType = (BorderTypes) f.readValuei("borderType");
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

                    Cv2.BilateralFilter(i.frame, frameProcess, diameter, sigmaColor, sigmaSpace, borderType);

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
            string paramOut01 = "bf" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.BilateralFilter(:paramIn01, :paramOut01, :diameter, :sigmaColor, :sigmaSpace, BorderTypes.:borderType);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":diameter",     diameter.ToString());
            code = code.Replace(":sigmaColor", sigmaColor.ToString());
            code = code.Replace(":sigmaSpace", sigmaSpace.ToString());
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
