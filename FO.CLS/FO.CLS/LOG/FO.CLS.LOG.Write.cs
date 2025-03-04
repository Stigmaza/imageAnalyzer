using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace FO.CLS.LOG
{
    public class Write
    {
        #region 변수
        public string LogFolderPathString = string.Empty;

        public ListBox logList;
        #endregion

        #region 생성자
        public Write(ListBox _logList = null)
        {
            LogFolderPathString = Directory.GetCurrentDirectory() + "\\Log";

            logList = _logList;
        }
        #endregion

        #region 메서드
        /// <summary>
        /// 로그 쓰기
        /// </summary>
        /// <param name="screenname"></param>
        /// <param name="logData"></param>
        public void WriteLog(string screenname, string logData)
        {
            try
            {
                // 디렉토리 생성 : 로그 폴더/화면명
                string directoryPathString = string.Format(@"{0}\{1}", LogFolderPathString, screenname);

                DirectoryInfo di = new DirectoryInfo(directoryPathString);

                if(!di.Exists)
                {
                    Directory.CreateDirectory(directoryPathString);
                }

                // 파일 생성
                string filePathString = string.Format(@"{0}\{1}.log", directoryPathString, DateTime.Now.ToString("yyyy-MM-dd"));

                FileInfo fi = new FileInfo(filePathString);

                StreamWriter sw = new StreamWriter(filePathString, true);
                string strToLog = string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), logData);
                sw.WriteLine(strToLog);
                sw.Close();

                string strToLogShort = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), logData);

                if(logList != null)
                {
                    logList.Invoke((MethodInvoker)delegate ()
                    {
                        //logList.Items.Insert(0, strToLogShort);

                        Console.WriteLine(strToLogShort);

                        int index = logList.Items.Add(strToLogShort);

                        logList.SelectedIndex = index;

                        if(logList.Items.Count > 100)
                            logList.Items.RemoveAt(0);

                    });

                }
            }
            catch //(Exception)
            {
                //throw;
            }
        }

        public void WriteLog(string logData)
        {
            WriteLog(Application.ProductName, logData);
        }

        /// <summary>
        /// 로그 파일 압축
        /// </summary>
        public void LogFileCompression()
        {
            // 로그 폴더 내의 폴더내 로그 파일 압축
            try
            {
                List<string> lstFolder = new List<string>();

                lstFolder = GetFolderListFromLogFolder();

                for(int i = 0; i < lstFolder.Count; i++)
                {
                    List<string> lstLogFile = GetLogFileListFromFolder(lstFolder[i]);

                    for(int j = 0; j < lstLogFile.Count; j++)
                    {
                        // 파일 확장자 제외 이름 
                        string fileNameOnly = lstLogFile[j].Substring(0, lstLogFile[j].Length - 4);

                        // 디렉토리 포함 파일명
                        string fileFullName = LogFolderPathString + "/" + lstFolder[i] + "/" + lstLogFile[j];

                        // 압축 파일명
                        string compressionPath = LogFolderPathString + "/" + lstFolder[i] + "/" + fileNameOnly + ".zip";

                        // 압축 파일 생성
                        using(FileStream fileStream = new FileStream(@compressionPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            using(ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
                            {
                                // 파일 압축 - 각 파일명으로
                                zipArchive.CreateEntryFromFile(fileFullName, lstLogFile[j]);

                                WriteLog("File Zip OK -> File Name " + lstFolder[i] + " : " + lstLogFile[j]);

                                // 압축 완료 해당 파일 삭제
                                File.Delete(fileFullName);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLog("LogFileCompression Exception - " + ex.ToString());

                throw ex;
            }
        }

        /// <summary>
        /// 로그 폴더 파일 목록 가져오기
        /// </summary>
        /// <returns>폴더 목록</returns>
        private List<String> GetFolderListFromLogFolder()
        {
            List<string> lstFolderList = new List<string>();

            try
            {
                string directoryPathString = string.Format(@"{0}", LogFolderPathString);

                DirectoryInfo di = new DirectoryInfo(directoryPathString);

                foreach(var item in di.GetDirectories())
                {
                    lstFolderList.Add(item.Name);
                }
            }

            catch(Exception ex)
            {
                WriteLog("GetFolderListFromLogFolder Exception - " + ex.ToString());
                throw ex;
            }

            return lstFolderList;
        }

        /// <summary>
        /// 폴더 내 Log 파일 목록 가져오기
        /// </summary>
        /// <returns>파일 목록</returns>
        private List<String> GetLogFileListFromFolder(string folderName)
        {
            List<string> lstFileList = new List<string>();

            try
            {
                string nowDate = DateTime.Now.ToString("yyyy-MM-dd");

                string directoryPathString = string.Format(@"{0}\{1}", LogFolderPathString, folderName);

                DirectoryInfo di = new DirectoryInfo(directoryPathString);

                foreach(var item in di.GetFiles())
                {
                    // 오늘 날짜 제외하고 로그파일 목록 가져오기
                    if(item.Extension.ToLower().CompareTo(".log") == 0
                        && item.Name.CompareTo(nowDate) == -1)
                    {
                        lstFileList.Add(item.Name);
                    }
                }
            }
            catch(Exception ex)
            {
                WriteLog("GetLogFileListFromFolder Exception - " + ex.ToString());
                throw ex;
            }

            return lstFileList;
        }
        #endregion
    }
}
