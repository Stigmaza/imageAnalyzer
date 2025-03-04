using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.LineDescriptor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace imageAnalyzer
{
    public class clsProcessItemCam : clsProcessZItem
    {
        VideoCapture video = null;


        [Category("변수")]
        public int cameraIndex { get; set; } = 1;

        [Category("변수")]
        public int frameWidth { get; set; } = 1920;

        [Category("변수")]
        public int frameHeight { get; set; } = 1080;

        public clsProcessItemCam() : base()
        {
            name = "카메라입력";

            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("cameraIndex", cameraIndex);
            f.WriteValue("frameWidth", frameWidth);
            f.WriteValue("frameHeight", frameHeight);
        }

        public override void loadItem(SQLITEINI f)
        {
            cameraIndex = f.readValuei("cameraIndex");
            frameWidth = f.readValuei("frameWidth");
            frameHeight = f.readValuei("frameHeight");
        }

        public override void init()
        {
            base.init();

            video = new VideoCapture(cameraIndex);

            video.FrameWidth = frameWidth;
            video.FrameHeight = frameHeight;
        }

        public override void process()
        {
            try
            {
                clsDataOut o = getFrameOutByName("out");

                video.Read(frameProcess);

                o.frame = frameProcess;
            }
            catch
            {
                onErrorProcess();
            }
        }
        public override string generateCode(List<clsProcessZItem> items)
        {
            string paramOut01 = "cam" + depth.ToString("00") + guid.Substring(0, 4);

            clsDataOut o01 = getFrameOutByName("out");
            o01.csname = paramOut01;

            string code = @"
                VideoCapture camReader = new VideoCapture(:cameraIndex);

                Mat :paramOut01 = new Mat();

                {
                    camReader.Read(:paramOut01);                 
                }
            ";

            code = code.Replace(":paramOut01", paramOut01);

            code = code.Replace(":cameraIndex", cameraIndex.ToString());

            return code;
        }

        public override void afterProcess()
        {
            base.afterProcess();
        }

        public override void finalize()
        {
            base.finalize();
            video.Release();
        }
    }
}
