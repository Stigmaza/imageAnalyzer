using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageAnalyzer
{
    internal class clsProcessItemChannelJoin : clsProcessZItem
    {
        private Mat[] splitChannel = new Mat[3];

        public clsProcessItemChannelJoin() : base()
        {
            name = "JOIN";

            frameIn.Add(new clsDataIn(this, "in_a"));
            frameIn.Add(new clsDataIn(this, "in_b"));
            frameIn.Add(new clsDataIn(this, "in_c"));

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
                {
                    clsDataIn ia = getInFrameByName("in_a");
                    clsDataIn ib = getInFrameByName("in_b");
                    clsDataIn ic = getInFrameByName("in_c");

                    clsDataOut o = getFrameOutByName("out");

                    if ((ia.frame.Width != 0 && ia.frame.Height != 0) || (ib.frame.Width != 0 && ib.frame.Height != 0) || (ic.frame.Width != 0 && ic.frame.Height != 0))
                    {
                        if (ia.frame.Width == 0 || ia.frame.Height == 0)
                        {
                            if (ib.frame.Width != 0 && ib.frame.Height != 0) ia.frame = new Mat(ib.frame.Size(), ib.frame.Type());
                            else if (ic.frame.Width != 0 && ic.frame.Height != 0) ia.frame = new Mat(ic.frame.Size(), ic.frame.Type());
                        }

                        if (ib.frame.Width == 0 || ib.frame.Height == 0)
                        {
                            if (ia.frame.Width != 0 && ia.frame.Height != 0) ib.frame = new Mat(ia.frame.Size(), ia.frame.Type());
                            else if (ic.frame.Width != 0 && ic.frame.Height != 0) ib.frame = new Mat(ic.frame.Size(), ic.frame.Type());
                        }

                        if (ic.frame.Width == 0 || ic.frame.Height == 0)
                        {
                            if (ia.frame.Width != 0 && ia.frame.Height != 0) ic.frame = new Mat(ia.frame.Size(), ia.frame.Type());
                            else if (ib.frame.Width != 0 && ib.frame.Height != 0) ic.frame = new Mat(ib.frame.Size(), ib.frame.Type());
                        }

                        Mat[] channels = new Mat[3];

                        channels[0] = ia.frame;
                        channels[1] = ib.frame;
                        channels[2] = ic.frame;

                        Cv2.Merge(channels, frameProcess);

                        o.frame = frameProcess;
                    }
                    else
                    {
                        onErrorProcess();
                    }
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
            string paramIn01 = getOutDataCsName(items, "in_a");
            string paramIn02 = getOutDataCsName(items, "in_b");
            string paramIn03 = getOutDataCsName(items, "in_c");

            string paramOut01 = "channelJoin" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            if (paramIn01 == string.Empty) paramIn01 = "new Mat()";
            if (paramIn02 == string.Empty) paramIn02 = "new Mat()";
            if (paramIn03 == string.Empty) paramIn03 = "new Mat()";

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    Mat workIn01 = :paramIn01;
                    Mat workIn02 = :paramIn02;
                    Mat workIn03 = :paramIn03;

                    if ((workIn01.Width != 0 && workIn01.Height != 0) || (workIn02.Width != 0 && workIn02.Height != 0) && (workIn03.Width != 0 && workIn03.Height != 0))
                    {
                        if (workIn01.Width == 0 || workIn01.Height == 0)
                        {
                            if (workIn02.Width != 0 && workIn02.Height != 0) workIn01 = new Mat(workIn02.Size(), workIn02.Type());
                            else if (workIn03.Width != 0 && workIn03.Height != 0) workIn01 = new Mat(workIn03.Size(), workIn03.Type());
                        }

                        if (workIn02.Width == 0 || workIn02.Height == 0)
                        {
                            if (workIn01.Width != 0 && workIn01.Height != 0) workIn02 = new Mat(workIn01.Size(), workIn01.Type());
                            else if (workIn03.Width != 0 && workIn03.Height != 0) workIn02 = new Mat(workIn03.Size(), workIn03.Type());
                        }

                        if (workIn03.Width == 0 || workIn03.Height == 0)
                        {
                            if (workIn01.Width != 0 && workIn01.Height != 0) workIn03 = new Mat(workIn01.Size(), workIn01.Type());
                            else if (workIn02.Width != 0 && workIn02.Height != 0) workIn03 = new Mat(workIn02.Size(), workIn02.Type());
                        }

                        Mat[] channels = new Mat[3];

                        channels[0] = workIn01;
                        channels[1] = workIn02;
                        channels[2] = workIn03;

                        Cv2.Merge(channels, frameProcess);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramIn02", paramIn02);
            code = code.Replace(":paramIn03", paramIn03);

            code = code.Replace(":paramOut01", paramOut01);

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
