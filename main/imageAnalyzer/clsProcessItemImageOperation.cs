using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemBitwise;

namespace imageAnalyzer
{
    internal class clsProcessItemImageOperation : clsProcessZItem
    {
        public enum OPERATION_OPTION { ADD, SUBTRACT, MULTIPLY, DIVIDE, MAX, MIN, ABS, ABSDIFF }

        [Category("변수")]
        [TypeConverter(typeof(OPERATION_OPTION))]
        public OPERATION_OPTION type { get; set; } = OPERATION_OPTION.ABS;


        public clsProcessItemImageOperation() : base()
        {
            name = "OP";

            frameIn.Add(new clsDataIn(this, "in1"));
            frameIn.Add(new clsDataIn(this, "in2"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("type", (int)type);
        }

        public override void loadItem(SQLITEINI f)
        {
            type = (OPERATION_OPTION)f.readValuei("type");
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
                    clsDataIn i1 = getInFrameByName("in1");
                    clsDataIn i2 = getInFrameByName("in2");
                    clsDataOut o = getFrameOutByName("out");

                    if (type == OPERATION_OPTION.ABS)
                    {
                        Mat t = i1.frame;

                        if (t.Width == 0 || t.Height == 0) t = i2.frame;

                        if (t.Width == 0 || t.Height == 0) return;

                        frameProcess = Cv2.Abs(t);

                    }
                    else if (i1.frame.Width <= 0 || i1.frame.Height <= 0 || i2.frame.Width <= 0 || i2.frame.Height <= 0)
                    {
                        return;
                    }

                    if (type == OPERATION_OPTION.ABS) Cv2.Add(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.SUBTRACT) Cv2.Subtract(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.MULTIPLY) Cv2.Multiply(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.DIVIDE) Cv2.Divide(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.MAX) Cv2.Max(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.MIN) Cv2.Min(i1.frame, i2.frame, frameProcess);
                    if (type == OPERATION_OPTION.ABSDIFF) Cv2.Absdiff(i1.frame, i2.frame, frameProcess);

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
            string paramIn01 = getOutDataCsName(items, "in1");
            string paramIn02 = getOutDataCsName(items, "in2");
            string paramOut01 = "iop" + depth.ToString("00") + guid.Substring(0, 4);


            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0 && :paramIn02.Width > 0 && :paramIn02.Height > 0)
                    {                       
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.ABS) Cv2.Add(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.SUBTRACT) Cv2.Subtract(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.MULTIPLY) Cv2.Multiply(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.DIVIDE) Cv2.Divide(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.MAX) Cv2.Max(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.MIN) Cv2.Min(:paramIn01, :paramIn02, :paramOut01);
                        if (OPERATION_OPTION.:type == OPERATION_OPTION.ABSDIFF) Cv2.Absdiff(:paramIn01, :paramIn02, :paramOut01);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramIn02", paramIn02);
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
