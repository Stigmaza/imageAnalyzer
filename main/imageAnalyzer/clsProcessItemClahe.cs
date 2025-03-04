using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace imageAnalyzer
{
    internal class clsProcessItemClahe : clsProcessZItem
    {
        [Category("변수")]
        public int clipLimit { get; set; } = 4;

        [Category("변수")]
        public int tileSize{ get; set; } = 8;

        public clsProcessItemClahe() : base()
        {
            name = "CLAHE";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("clipLimit", (int)clipLimit);
            f.WriteValue("tileSize", (int)tileSize);
        }

        public override void loadItem(SQLITEINI f)
        {
            clipLimit = f.readValuei("clipLimit", 40);
            tileSize = f.readValuei("tileSize", 8);
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

                    CLAHE clahe = Cv2.CreateCLAHE(clipLimit, new Size(tileSize,tileSize));

                    clahe.Apply(i.frame, frameProcess);

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
                        CLAHE clahe = Cv2.CreateCLAHE(:clipLimit, new Size(:tileSize,:tileSize));

                        clahe.Apply(:paramIn01, :paramOut01);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":clipLimit", clipLimit.ToString());
            code = code.Replace(":tileSize", tileSize.ToString());

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
