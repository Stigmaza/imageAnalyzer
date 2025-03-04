using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Windows.Forms;
using FO.CLS.UTIL;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace imageAnalyzer
{
    public class clsCameraReader
    {
        clsProcessManager processList;

        string statMsg = string.Empty;


        Thread handleThread = null;
        bool exitThread = false;

        ListBox listLog = null;


        public clsCameraReader()
        {
        }

        public void setup01CameraInfo(ListBox _listLog)
        {
            listLog = _listLog;
        }

        public void start(clsProcessManager _processList)
        {
            if (handleThread == null)
            {
                exitThread = false;

                processList = _processList;

                handleThread = new Thread(threadMain);
                handleThread.IsBackground = true;
                handleThread.Start();
            }
        }

        public bool isRun()
        {
            if (handleThread != null) return true;

            return false;
        }

        public void stop(bool force = false)
        {
            exitThread = true;

            if (force)
            {
                handleThread?.Abort();
            }
            
            handleThread = null;
        }

        public void updateStatMsg(string msg)
        {
            statMsg = DateTime.Now.ToString("HH:mm:ss : ") + msg;

            //if (lbStat != null)
            //{
            //    lbStat.Invoke((MethodInvoker)delegate ()
            //    {
            //        lbStat.Text = statMsg;
            //    });
            //}

            if (listLog != null)
            {
                listLog.Invoke((MethodInvoker)delegate ()
                {
                    if (listLog.Items.Count > 300) listLog.Items.Clear();

                    bool skipInsert = false;

                    if (listLog.Items.Count > 0)
                    {
                        string msgLog = listLog.Items[0].ToString();

                        string[] t1 = msgLog.Split('[');
                        string[] t2 = statMsg.Split('[');

                        if (t1.Length == t2.Length && t1.Length == 2)
                        {
                            if (t1[1] == t2[1])
                                skipInsert = true;
                        }
                    }

                    if (skipInsert == false)
                    {
                        listLog.Items.Insert(0, statMsg);
                    }
                    else
                    {
                        listLog.Items[0] = statMsg;
                    }
                });
            }
        }



        public void threadMain()
        {
            updateStatMsg("초기화 시작");

            processList.callInit();

            updateStatMsg("초기화 완료");

            try
            {
                while (exitThread == false)
                {
                    try
                    {
                        processList.reorderProcessDepth();

                        processList.callProcess();
                    }
                    catch (Exception e)
                    {
                        updateStatMsg("except : " + e.Message);
                    }
                    finally
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch
            {

            }
            finally
            {
                processList.callFinalizer();
            }
        }


    }
}
