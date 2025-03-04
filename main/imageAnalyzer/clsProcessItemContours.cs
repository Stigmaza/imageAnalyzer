using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static imageAnalyzer.clsProcessItemScharr;

namespace imageAnalyzer
{
    internal class clsProcessItemContours : clsProcessZItem
    {
        [Category("변수")]
        [TypeConverter(typeof(RetrievalModes))]
        public RetrievalModes mode { get; set; } = RetrievalModes.Tree;

        [Category("변수")]
        [TypeConverter(typeof(ContourApproximationModes))]
        public ContourApproximationModes method { get; set; } = ContourApproximationModes.ApproxTC89KCOS;

        [Category("변수")]
        public int arcLength_min { get; set; } = 100;
        [Category("변수")]
        public int area_min { get; set; } = 1000;
        [Category("변수")]
        public int point_min { get; set; } = 5;

        [Category("변수")]
        public bool drawCenter { get; set; } = true;

        public clsProcessItemContours() : base()
        {
            name = "CONTOURS";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("mode", (int)mode);
            f.WriteValue("method", (int)method);


            f.WriteValue("arcLength_min", (int)arcLength_min);
            //f.WriteValue("arcLength_max", (int)arcLength_max);

            f.WriteValue("area_min", (int)area_min);
            //f.WriteValue("area_max", (int)area_max);

            f.WriteValue("point_min", (int)point_min);
            //f.WriteValue("point_max", (int)point_max);
        }

        public override void loadItem(SQLITEINI f)
        {
            mode = (RetrievalModes)f.readValuei("mode");
            method = (ContourApproximationModes)f.readValuei("method");

            arcLength_min = f.readValuei("arcLength_min", 100);
            //arcLength_max = f.readValuei("arcLength_max", 500);

            area_min = f.readValuei("area_min", 1000);
            //area_max = f.readValuei("area_max", 5000);

            point_min = f.readValuei("point_min", 5);
            //point_max = f.readValuei("point_max", 25);
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

                    i.frame.CopyTo(frameProcess);

                    Point[][] contours;
                    HierarchyIndex[] hierarchy;
                    List<Point[]> result = new List<Point[]>();

                    Cv2.FindContours(frameProcess, out contours, out hierarchy, mode, method);

                    foreach (Point[] p in contours)
                    {
                        double length = Cv2.ArcLength(p, true);
                        double area = Cv2.ContourArea(p, true);

                        if (length < arcLength_min && area < area_min && p.Length < point_min) continue;

                        //if (length > arcLength_max || area > area_max || p.Length > point_max) continue;

                        result.Add(p);

                        bool convex = Cv2.IsContourConvex(p);
                        Point[] hull = Cv2.ConvexHull(p, true);
                        Cv2.DrawContours(frameProcess, new Point[][] { hull }, -1, Scalar.White, 1);

                        if (drawCenter)
                        {
                            Moments moments = Cv2.Moments(p, false);
                            Cv2.Circle(frameProcess, (int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00), 5, Scalar.Black, -1);
                        }
                    }

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
            string paramOut01 = "contours" + depth.ToString("00") + guid.Substring(0, 4);
            string paramResult = "result_" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                List<Point[]> :paramResult = new List<Point[]>();

                {
                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        :paramIn01.CopyTo(:paramOut01);

                        Point[][] contours;
                        HierarchyIndex[] hierarchy;
                        
                        Cv2.FindContours(:paramOut01, out contours, out hierarchy, RetrievalModes.:mode, ContourApproximationModes.:method);

                        foreach (Point[] p in contours)
                        {
                            double length = Cv2.ArcLength(p, true);
                            double area = Cv2.ContourArea(p, true);

                            if (length < :arcLength_min && area < :area_min && p.Length < :point_min) continue;

                            :paramResult.Add(p);

                            bool convex = Cv2.IsContourConvex(p);
                            Point[] hull = Cv2.ConvexHull(p, true);
                            Cv2.DrawContours(:paramOut01, new Point[][] { hull }, -1, Scalar.White, 1);

                            if(:drawCenter)
                            {
                                Moments moments = Cv2.Moments(p, false);
                                Cv2.Circle(:paramOut01, (int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00), 5, Scalar.Black, -1);
                            }
                        }
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":paramOut01", paramOut01);
            code = code.Replace(":paramResult", paramResult);

            code = code.Replace(":mode", mode.ToString());
            code = code.Replace(":method", method.ToString());

            code = code.Replace(":arcLength_min", arcLength_min.ToString());
            code = code.Replace(":area_min", area_min.ToString());
            code = code.Replace(":point_min", point_min.ToString());

            code = code.Replace(":drawCenter", drawCenter.ToString().ToLower());

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
