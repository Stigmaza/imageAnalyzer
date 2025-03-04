using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemColorChange;

namespace imageAnalyzer
{
    internal class clsProcessItemRange : clsProcessZItem
    {

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_1_min { get; set; }

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_1_max { get; set; } = 255;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_2_min { get; set; }

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_2_max { get; set; } = 255;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_3_min { get; set; }

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int ch_3_max { get; set; } = 255;

        public override int rangeMax(string name)
        {
            return 255;
        }

        public clsProcessItemRange() : base()
        {
            name = "RANGE";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("ch_1_min", ch_1_min);
            f.WriteValue("ch_1_max", ch_1_max);

            f.WriteValue("ch_2_min", ch_2_min);
            f.WriteValue("ch_2_max", ch_2_max);

            f.WriteValue("ch_3_min", ch_3_min);
            f.WriteValue("ch_3_max", ch_3_max);
        }

        public override void loadItem(SQLITEINI f)
        {
            ch_1_min = f.readValuei("ch_1_min");
            ch_1_max = f.readValuei("ch_1_max");

            ch_2_min = f.readValuei("ch_2_min");
            ch_2_max = f.readValuei("ch_2_max");

            ch_3_min = f.readValuei("ch_3_min");
            ch_3_max = f.readValuei("ch_3_max");
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

                contentBefore = BitmapConverter.ToBitmap(i.frame);

                Scalar low = new Scalar(ch_1_min, ch_2_min, ch_3_min);
                Scalar hi = new Scalar(ch_1_max, ch_2_max, ch_3_max);

                Cv2.InRange(i.frame, low, hi, frameProcess);

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
            string paramOut01 = "range" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Scalar low = new Scalar(:ch_1_min, :ch_2_min, :ch_3_min);
                        Scalar hi = new Scalar(:ch_1_max, :ch_2_max, :ch_3_max);

                        Cv2.InRange(:paramIn01, low, hi, :paramOut01);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":ch_1_min", ch_1_min.ToString());
            code = code.Replace(":ch_2_min", ch_2_min.ToString());
            code = code.Replace(":ch_3_min", ch_3_min.ToString());

            code = code.Replace(":ch_1_max", ch_1_max.ToString());
            code = code.Replace(":ch_2_max", ch_2_max.ToString());
            code = code.Replace(":ch_3_max", ch_3_max.ToString());

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
