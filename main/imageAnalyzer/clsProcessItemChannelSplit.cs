using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemBitwise;
using static imageAnalyzer.clsProcessItemImageOperation;

namespace imageAnalyzer
{
    internal class clsProcessItemChannelSplit : clsProcessZItem
    {
        private Mat[] splitChannel = new Mat[3];

        public clsProcessItemChannelSplit() : base()
        {
            name = "SPLIT";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out_a"));
            frameOut.Add(new clsDataOut(this, "out_b"));
            frameOut.Add(new clsDataOut(this, "out_c"));

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
                    clsDataIn i = getInFrameByName("in");
                    clsDataOut oa = getFrameOutByName("out_a");
                    clsDataOut ob = getFrameOutByName("out_b");
                    clsDataOut oc = getFrameOutByName("out_c");

                    if (i.frame.Width <= 0 || i.frame.Height <= 0) return;

                    Cv2.Split(i.frame, out splitChannel);

                    frameProcess = i.frame;

                    oa.frame = splitChannel[0];
                    ob.frame = splitChannel[1];
                    oc.frame = splitChannel[2];
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
            string paramOut01 = "channelSplit01" + depth.ToString("00") + guid.Substring(0, 4);
            string paramOut02 = "channelSplit02" + depth.ToString("00") + guid.Substring(0, 4);
            string paramOut03 = "channelSplit03" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out_a");
            o01.csname = paramOut01;

            clsDataOut o02 = getFrameOutByName("out_b");
            o02.csname = paramOut02;

            clsDataOut o03 = getFrameOutByName("out_c");
            o03.csname = paramOut03;

            string code = @"
                Mat :paramOut01 = new Mat();
                Mat :paramOut02 = new Mat();
                Mat :paramOut03 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        Mat[] splitChannel = new Mat[3];

                        Cv2.Split(:paramIn01, out splitChannel);

                        :paramOut01 = splitChannel[0];
                        :paramOut02 = splitChannel[1];
                        :paramOut03 = splitChannel[2];
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);
            code = code.Replace(":paramOut02", paramOut02);
            code = code.Replace(":paramOut03", paramOut03);

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
