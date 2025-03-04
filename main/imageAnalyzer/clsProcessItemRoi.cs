using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace imageAnalyzer
{
    public class clsProcessItemRoi : clsProcessZItem
    {
        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int x { get; set; } = 0;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int y { get; set; } = 0;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int width { get; set; } = 100;

        [Category("변수")]
        [Editor(typeof(uiEditorslide), typeof(UITypeEditor))]
        public int height { get; set; } = 100;

        private Rect rectMax = new Rect(0, 0, 0, 0);

        public override int rangeMax(string name)
        {
            if (name == "x" || name == "width")
            {
                return rectMax.Width;
            }
            if (name == "y" || name == "height")
            {
                return rectMax.Height;
            }

            return 0;
        }

        public clsProcessItemRoi() : base()
        {
            name = "ROI";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("x", x);
            f.WriteValue("y", y);
            f.WriteValue("width", width);
            f.WriteValue("height", height);
        }

        public override void loadItem(SQLITEINI f)
        {
            x = f.readValuei("x");
            y = f.readValuei("y");
            width = f.readValuei("width");
            height = f.readValuei("height");
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

                int p1 = x;
                int p2 = y;
                int p3 = width;
                int p4 = height;

                if (p1 < 0) p1 = 0;

                if (i.frame.Width < p1 + p3)
                    p3 = i.frame.Width - p1;

                if (p2 < 0) p2 = 0;

                if (i.frame.Height < p2 + p4)
                    p4 = i.frame.Height - p2;

                rectMax.Width = i.frame.Width;
                rectMax.Height = i.frame.Height;

                Rect rect = new Rect(p1, p2, p3, p4);
                frameProcess = i.frame.SubMat(rect);

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
            string paramOut01 = "imageRoi" + depth.ToString("00") + guid.Substring(0, 4);
                        
            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        int p1 = :x;
                        int p2 = :y;
                        int p3 = :width;
                        int p4 = :height;

                        if (p1 < 0) p1 = 0;

                        if (:paramIn01.Width < p1 + p3)
                            p3 = :paramIn01.Width - p1;

                        if (p2 < 0) p2 = 0;

                        if (:paramIn01.Height < p2 + p4)
                            p4 = :paramIn01.Height - p2;

                        Rect rect = new Rect(p1, p2, p3, p4);

                        :paramOut01 = :paramIn01.SubMat(rect);
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":x", x.ToString());
            code = code.Replace(":y", y.ToString());
            code = code.Replace(":width", width.ToString());
            code = code.Replace(":height", height.ToString());

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
