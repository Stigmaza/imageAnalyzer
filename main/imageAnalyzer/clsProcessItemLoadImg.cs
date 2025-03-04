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

namespace imageAnalyzer
{
    internal class clsProcessItemLoadImg : clsProcessZItem
    {

        [Category("변수")]
        [Editor(typeof(uiEditorFileOpen), typeof(UITypeEditor))]
        public string pathToLoad { get; set; } = "";

        public clsProcessItemLoadImg() : base()
        {
            name = "load image";

            frameOut.Add(new clsDataOut(this, "out"));

            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("pathToLoad", pathToLoad);
        }

        public override void loadItem(SQLITEINI f)
        {
            pathToLoad = f.readValue("pathToLoad");
        }

        public override void init()
        {
            base.init();
        }

        public override void process()
        {
            try
            {
                clsDataOut o = getFrameOutByName("out");

                if (File.Exists(pathToLoad))
                {
                    frameProcess = Cv2.ImRead(pathToLoad);

                    o.frame = frameProcess;
                }
            }
            catch
            {
                onErrorProcess();
            }
        }

        public override string generateCode(List<clsProcessZItem> items)
        {
            string paramOut01 = "imgFromFile" + depth.ToString("00") + guid.Substring(0, 4);
            clsDataOut o01 = getFrameOutByName("out");

            o01.csname = paramOut01;

            string code = @"
                Mat :paramOut01 = new Mat();
                {
                    string pathToLoad = @"":pathToLoad"";

                    if (File.Exists(pathToLoad))
                    {
                        :paramOut01 = Cv2.ImRead(pathToLoad);
                    }
                }
            ";

            code = code.Replace(":paramOut01", paramOut01);
            code = code.Replace(":pathToLoad", pathToLoad);

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
