using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemBlank : clsProcessZItem
    {
        public clsProcessItemBlank() : base()
        {
            name = "BLANK";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            //f.WriteValue("thresh", thresh);
        }

        public override void loadItem(SQLITEINI f)
        {
            //thresh = f.readValued("thresh");
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


                o.frame = frameProcess;
            }
            catch
            {
                onErrorProcess();
            }
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
