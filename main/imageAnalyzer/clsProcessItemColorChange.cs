using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace imageAnalyzer
{
    public class clsProcessItemColorChange : clsProcessZItem
    {
        public enum COLOR_TO { BGR2GRAY = 6, GRAY2BGR = 8, BGR2YCrCb = 36, YCrCb2BGR = 38, BGR2HSV = 40, BGR2Lab = 44, HSV2BGR = 54, Lab2BGR = 56 }

        [Category("변수")]
        [TypeConverter(typeof(COLOR_TO))]
        public COLOR_TO type { get; set; } = COLOR_TO.BGR2GRAY;

        public clsProcessItemColorChange() : base()
        {
            name = "COLOR CHANGE";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("color_type", (int)type);
        }

        public override void loadItem(SQLITEINI f)
        {
            type = (COLOR_TO)f.readValuei("color_type");
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

                    Cv2.CvtColor(i.frame, frameProcess, (ColorConversionCodes)type);

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
            string paramOut01 = "cc" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.CvtColor(:paramIn01, :paramOut01, ColorConversionCodes.:type);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);
            code = code.Replace(":type", type.ToString());

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
