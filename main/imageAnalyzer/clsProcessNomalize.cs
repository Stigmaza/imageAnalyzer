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
    internal class clsProcessNomalize : clsProcessZItem
    {
        [Category("변수")]
        public double alpha { get; set; } = 0;

        [Category("변수")]
        public double beta { get; set; } = 255;

        [Category("변수")]
        public NormTypes normtype { get; set; } = NormTypes.MinMax;


        public clsProcessNomalize() : base()
        {
            name = "NORMAL";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("alpha", (int)alpha);
            f.WriteValue("beta", (int)beta);
            f.WriteValue("normtype", (int)normtype);
        }

        public override void loadItem(SQLITEINI f)
        {
            alpha = f.readValued("alpha", 0);
            beta = f.readValued("beta", 255);
            normtype = (NormTypes)f.readValuei("normtype", 32);
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

                    Cv2.Normalize(i.frame, frameProcess, alpha, beta, normtype);

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
            string paramOut01 = "equalizer" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Cv2.Normalize(:paramIn01, :paramOut01, :alpha, :beta, NormTypes.:normtype);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":alpha", alpha.ToString());
            code = code.Replace(":beta", beta.ToString());
            code = code.Replace(":normtype", normtype.ToString());

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
