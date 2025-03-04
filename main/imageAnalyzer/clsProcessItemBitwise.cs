using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static imageAnalyzer.clsProcessItemBitwise;
using static imageAnalyzer.clsProcessItemColorChange;

namespace imageAnalyzer
{
    internal class clsProcessItemBitwise : clsProcessZItem
    {
        public enum BIT_OPTION { AND, OR, XOR, NOT }

        [Category("변수")]
        [TypeConverter(typeof(BIT_OPTION))]
        public BIT_OPTION type { get; set; } = BIT_OPTION.NOT;


        public clsProcessItemBitwise() : base()
        {
            name = "BIT";

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
            type = (BIT_OPTION)f.readValuei("type");
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

                    if (type == BIT_OPTION.NOT)
                    {
                        Mat t = i1.frame;

                        if (t.Width == 0 || t.Height == 0) t = i2.frame;

                        if (t.Width == 0 || t.Height == 0) return;

                        Cv2.BitwiseNot(t, frameProcess);

                    }
                    else if (i1.frame.Width <= 0 || i1.frame.Height <= 0 || i2.frame.Width <= 0 || i2.frame.Height <= 0)
                    {
                        return;
                    }

                    if (type == BIT_OPTION.AND) Cv2.BitwiseAnd(i1.frame, i2.frame, frameProcess);
                    if (type == BIT_OPTION.OR) Cv2.BitwiseOr(i1.frame, i2.frame, frameProcess);
                    if (type == BIT_OPTION.XOR) Cv2.BitwiseXor(i1.frame, i2.frame, frameProcess);

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
            string paramOut01 = "bitwise" + depth.ToString("00") + guid.Substring(0, 4);

            if (paramIn01 == string.Empty)
                paramIn01 = "new Mat()";

            if (paramIn02 == string.Empty)
                paramIn02 = "new Mat()";

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    Mat workIn01 = :paramIn01;
                    Mat workIn02 = :paramIn02; 

                    if (BIT_OPTION.:type == BIT_OPTION.NOT)
                    {
                        Mat t = workIn01;
                        
                        if (t.Width <= 0 || t.Height <= 0 )
                            t = workIn02;

                        if (t.Width >= 0 && t.Height >= 0 )
                            Cv2.BitwiseNot(t, :paramOut01);

                    }
                    else if (workIn01.Width > 0 && workIn01.Height > 0 && workIn02.Width > 0 && workIn02.Height > 0)
                    {
                        if (BIT_OPTION.:type == BIT_OPTION.AND) Cv2.BitwiseAnd(workIn01, workIn02, :paramOut01);
                        if (BIT_OPTION.:type == BIT_OPTION.OR)   Cv2.BitwiseOr(workIn01, workIn02, :paramOut01);
                        if (BIT_OPTION.:type == BIT_OPTION.XOR) Cv2.BitwiseXor(workIn01, workIn02, :paramOut01);
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
