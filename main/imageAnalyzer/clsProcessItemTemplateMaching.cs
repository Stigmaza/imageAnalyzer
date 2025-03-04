using FO.CLS.UTIL;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace imageAnalyzer
{
    public class clsProcessItemTemplateMaching : clsProcessZItem
    {

        [Category("변수")]
        [Editor(typeof(uiEditorTextEditor), typeof(UITypeEditor))]
        public string imgPathToFind { get; set; } = "";


        public clsProcessItemTemplateMaching() : base()
        {
            name = "TM";

            frameIn.Add(new clsDataIn(this, "in"));
            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("imgPathToFind", imgPathToFind);
        }

        public override void loadItem(SQLITEINI f)
        {
            imgPathToFind = f.readValue("imgPathToFind");
        }

        public override void init()
        {
            base.init();
        }

        private Mat rotationImage(Mat img, float angle)
        {
            // 회전 중심점 계산
            Point2f center = new Point2f(img.Cols / 2.0f, img.Rows / 2.0f);

            // 회전 변환 행렬 계산
            Mat M = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            // 회전 후의 이미지 크기 계산
            RotatedRect rotatedRect = new RotatedRect(center, img.Size(), angle);
            Rect bbox = rotatedRect.BoundingRect();

            // 변환 행렬 조정
            M.Set<double>(0, 2, M.At<double>(0, 2) + bbox.Width / 2.0 - center.X);
            M.Set<double>(1, 2, M.At<double>(1, 2) + bbox.Height / 2.0 - center.Y);

            // 결과 이미지 생성 및 회전 적용
            Mat dst = new Mat();
            Cv2.WarpAffine(img, dst, M, bbox.Size);

            return dst;
        }

        private int loadImage(List<Mat> matList, string imgPath )
        {
            string[] lines = imgPath.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                if (File.Exists(lines[i]))
                {
                    matList[i] = Cv2.ImRead(lines[i]);
                }
            }

            if( matList.Count == lines.Count())
                return matList.Count;

            return 0;
        }

        public override void process()
        {
            List<Mat> matList = new List<Mat>();

            try
            {
                clsDataIn i = getInFrameByName("in");
                clsDataOut o = getFrameOutByName("out");

                if (i.frame.Width <= 0 || i.frame.Height <= 0) return;

                if (loadImage(matList, imgPathToFind) == 0)
                {
                    onErrorProcess();
                    return;
                }

                /*
                Cv2.CvtColor(imgTarget, imgTarget, ColorConversionCodes.BGR2GRAY);

                float angleMax = 100;

                var threshold = 0.7;
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;

                for (float angle = 0; angle < angleMax; angle++)
                {
                    bool find = false;

                    Mat frameWork = i.frame;
                    Mat frameFind = rotationImage(imgTarget, angle);

                    Cv2.MatchTemplate(frameWork, imgTarget, frameProcess, TemplateMatchModes.CCoeffNormed);

                    Cv2.MinMaxLoc(frameWork, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= threshold)
                    {
                        Rect rect = new Rect(maxloc.X, maxloc.Y, imgTarget.Width, imgTarget.Height);
                        Cv2.Rectangle(frameWork, rect, new OpenCvSharp.Scalar(0, 0, 255), 2);

                        find = true;
                    }
                    else
                    {
                        Rect rect = new Rect(0, 0, 10, 10);

                        Cv2.Rectangle(frameWork, rect, new OpenCvSharp.Scalar(0, 0, 255), 1);
                    }

                    if (find)
                    {
                        o.frame = frameWork;
                        break;
                    }
                }
                */
            }
            catch
            {
                onErrorProcess();
            }
            finally
            {
                foreach (var item in matList)
                {
                    item.Dispose();
                }

                matList.Clear();
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
