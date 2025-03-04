using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static imageAnalyzer.clsProcessItemImageOperation;

namespace imageAnalyzer
{
    public class clsProcessItemBinary : clsProcessZItem
    {
        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public double thresh { get; set; } = 150;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public double maxval { get; set; } = 255;

        [Category("변수")]
        [TypeConverter(typeof(ThresholdTypes))]
        public ThresholdTypes type { get; set; } = ThresholdTypes.Binary;

        public override int rangeMax(string name)
        {
            if (name == "thresh" || name == "maxval")
            {
                return 255;
            }

            return 0;
        }

        public clsProcessItemBinary() : base()
        {
            name = "BIN";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("thresh", thresh);
            f.WriteValue("maxval", maxval);
            f.WriteValue("type", (int)type);
        }

        public override void loadItem(SQLITEINI f)
        {
            thresh = f.readValued("thresh");
            maxval = f.readValued("maxval");
            type = (ThresholdTypes)f.readValued("type");
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

                    Cv2.Threshold(i.frame, frameProcess, thresh, maxval, type);

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
            string paramOut01 = "bin" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.Threshold(:paramIn01, :paramOut01, :thresh, :maxval, ThresholdTypes.:type);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":thresh", thresh.ToString());
            code = code.Replace(":maxval", maxval.ToString());
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
