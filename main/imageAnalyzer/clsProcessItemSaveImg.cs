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
    public class clsProcessItemSaveImg : clsProcessZItem
    {
        public enum SAVE_OPTION { ALWAYS, INTERVAL, NOT_EXISTS }

        [Category("변수")]
        [Editor(typeof(uiEditorFileSave), typeof(UITypeEditor))]
        public string pathToSave { get; set; } = "";

        [Category("변수")]
        [TypeConverter(typeof(SAVE_OPTION))]
        public SAVE_OPTION saveOption { get; set; } = SAVE_OPTION.ALWAYS;

        [Category("변수")]
        public int saveInterval { get; set; } = 5;

        public clsProcessItemSaveImg() : base()
        {
            name = "SAVE_IMG";

            frameIn.Add(new clsDataIn(this, "in"));


            base.initGdi();
        }

        public override void saveItem(SQLITEINI f)
        {
            f.WriteValue("pathToSave", pathToSave);
            f.WriteValue("saveOption", (int)saveOption);
            f.WriteValue("saveInterval", saveInterval);
        }

        public override void loadItem(SQLITEINI f)
        {
            pathToSave = f.readValue("pathToSave");
            saveOption = (SAVE_OPTION)f.readValuei("saveOption");
            saveInterval = f.readValuei("saveInterval");
        }


        public override void init()
        {
            base.init();
        }

        DateTime timeFileSave = DateTime.MinValue;

        public override void process()
        {
            try
            {
                clsDataIn i = getInFrameByName("in");

                if (i.frame.Width <= 0 || i.frame.Height <= 0) return;

                frameProcess = i.frame;

                bool bSave = false;

                if (saveOption == SAVE_OPTION.ALWAYS)
                {
                    bSave = true;
                }

                if (saveOption == SAVE_OPTION.INTERVAL)
                {
                    if (timeFileSave < DateTime.Now)
                    {
                        bSave = true;
                        timeFileSave = DateTime.Now.AddSeconds(saveInterval);
                    }
                }

                if (saveOption == SAVE_OPTION.NOT_EXISTS)
                {
                    if (File.Exists(pathToSave) == false)
                        bSave = true;
                }

                if (bSave)
                {
                    Cv2.ImWrite(pathToSave, frameProcess);
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

            string code = @"
                {
                    string pathToSave = @"":pathToSave"";

                    if (:paramIn01.Width > 0 && :paramIn01.Height > 0)
                    {
                        if (File.Exists(pathToSave) == false)
                            bSave = true;

                        if (bSave)
                        {
                            Cv2.ImWrite(pathToSave, :paramIn01);
                        }
                    }
                }
            ";

            code = code.Replace(":paramIn01", paramIn01);
            code = code.Replace(":pathToSave", pathToSave);

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
