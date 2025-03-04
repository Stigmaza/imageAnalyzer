using FO.CLS.UTIL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace imageAnalyzer
{
    public class clsProcessManager
    {
        public string scantime { get; set; }

        public List<clsProcessZItem> processList = new List<clsProcessZItem>();

        public clsProcessManager()
        {
            scantime = "-1";
        }

        public void addProcess(clsProcessZItem process, int x = 0, int y = 0)
        {
            process.updatePosition(x, y);
            processList.Add(process);
        }

        public clsProcessZItem addProcess(string processName, string name, string guid)
        {
            Type customerType = Type.GetType(processName);

            object obj = Activator.CreateInstance(customerType);

            clsProcessZItem item = (clsProcessZItem)obj;

            item.name = name;
            item.guid = guid;

            processList.Add(item);

            return item;
        }

        public void reorderProcessDepth()
        {
            for (int i = 0; i < processList.Count; i++)
            {
                processList[i].reOrderDepth(0);
            }
        }

        public clsProcessZItem getProcessByGuid(string id)
        {
            clsProcessZItem r = null;

            foreach (var t in processList.Where(x => x.guid == id))
            {
                r = t;
                break;
            }

            return r;
        }

        public void addNextProcess(string guidFrom, string outName, string guidTo, string inName)
        {
            clsProcessZItem itemFrom = getProcessByGuid(guidFrom);
            clsProcessZItem itemTo = getProcessByGuid(guidTo);

            itemFrom.addNextProcess(outName, itemTo, inName);
        }

        public void save(string path)
        {
            SQLITEINI f = new SQLITEINI("", path);

            f.createNew(true);

            for (int i = 0; i < processList.Count; i++)
            {
                f.setTable("ini");

                f.WriteValue("item_" + i.ToString(), processList[i].guid);

                saveSingleItem(f, processList[i]);
            }
        }

        public void saveSingleItem(SQLITEINI f, clsProcessZItem item)
        {
            string table = "item_" + item.guid;
            f.createTable(table);
            f.setTable(table);

            Type type = item.GetType();

            f.WriteValue("name", item.name);
            f.WriteValue("item_type", type.FullName);

            f.WriteValue("positions_x", item.bounds.Left);
            f.WriteValue("positions_y", item.bounds.Top);

            saveConnection(f, item);

            item.saveItem(f);
        }

        public void saveConnection(SQLITEINI f, clsProcessZItem item)
        {
            for (int inIndex = 0; inIndex < item.frameIn.Count; inIndex++)
            {
                f.WriteValue("in_" + inIndex.ToString(), item.frameIn[inIndex].name);
            }

            for (int outIndex = 0; outIndex < item.frameOut.Count; outIndex++)
            {
                f.WriteValue("out_" + outIndex.ToString(), item.frameOut[outIndex].name);

                for (int toIndex = 0; toIndex < item.frameOut[outIndex].dataTo.Count; toIndex++)
                {
                    f.WriteValue("out_" + outIndex.ToString() + "_connect_" + toIndex.ToString(), item.frameOut[outIndex].dataTo[toIndex].parent.guid);
                    f.WriteValue("out_" + outIndex.ToString() + "_connect_" + toIndex.ToString() + "_target", item.frameOut[outIndex].dataTo[toIndex].name);
                }
            }
        }

        public void load(string path)
        {
            processList.Clear();

            SQLITEINI f = new SQLITEINI("", path);

            for (int i = 0; i < 1000; i++)
            {
                f.setTable("ini");

                string guid = f.readValue("item_" + i.ToString());

                if (guid.Length == 0)
                    break;

                loadSingleItem(f, guid);
            }

            foreach (var item in processList)
            {
                foreach (var con in item.frameOut)
                {
                    for (int i = 0; i < con.dataToGuid.Count; i++)
                    {
                        addNextProcess(item.guid, con.name, con.dataToGuid[i], con.dataToInName[i]);
                    }
                }
            }

            reorderProcessDepth();

            resetZorder();
        }

        public void loadSingleItem(SQLITEINI f, string guid)
        {
            f.setTable("item_" + guid);

            string name = f.readValue("name");
            string type = f.readValue("item_type");
            int x = f.readValuei("positions_x");
            int y = f.readValuei("positions_y");

            clsProcessZItem item = addProcess(type, name, guid);

            item.updatePosition(x, y);

            loadConnection(f, item);

            item.loadItem(f);
        }

        public void loadConnection(SQLITEINI f, clsProcessZItem item)
        {
            for (int inIndex = 0; inIndex < 100; inIndex++)
            {
                string inName = f.readValue("in_" + inIndex.ToString());

                if (inName.Length == 0) break;

                item.frameIn[inIndex].name = inName;
            }

            for (int outIndex = 0; outIndex < 100; outIndex++)
            {
                string outName = f.readValue("out_" + outIndex.ToString());

                if (outName.Length == 0) break;

                item.frameOut[outIndex].name = outName;

                for (int toIndex = 0; toIndex < 100; toIndex++)
                {
                    string toGuid = f.readValue("out_" + outIndex.ToString() + "_connect_" + toIndex.ToString());
                    string toTarget = f.readValue("out_" + outIndex.ToString() + "_connect_" + toIndex.ToString() + "_target");

                    if (toGuid.Length == 0) break;

                    item.frameOut[outIndex].addDataTo(toGuid, toTarget);
                }
            }
        }

        public void callInit()
        {
            for (int i = 0; i < processList.Count; i++)
            {
                processList[i].init();
            }
        }

        public void callProcess()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            stopwatch.Start();

            clsProcessZItem lastWorkItem = null;

            try
            {
                for (int i = 0; i < processList.Count; i++)
                {
                    foreach (var t in processList.Where(x => x.depth == i))
                    {
                        lastWorkItem = t;
                        t.process();
                        t.afterProcess();
                    }
                }

                stopwatch.Stop();

                scantime = stopwatch.ElapsedMilliseconds.ToString("N0");
            }
            catch (Exception e)
            {
                throw new Exception(lastWorkItem.name + Environment.NewLine + lastWorkItem.guid + Environment.NewLine + e.ToString());
            }
        }

        public void callFinalizer()
        {
            foreach (var t in processList)
            {
                t.finalize();
            }
        }


        // ------------------------------------------------------------------------------------------------------------

        List<clsProcessZItem> selectedProcess = new List<clsProcessZItem>();
        clsDataPoint selectedPoint = null;
        clsDataPoint mousePoint = null;


        public void draw(Graphics g)
        {

            foreach (var item in processList.OrderByDescending(item => item.zOrder))
            {
                item.drawBody(g);
            }

            foreach (var item in processList.OrderByDescending(item => item.zOrder))
            {
                item.drawConnection(g);
            }
        }

        public void resetZorder(clsProcessZItem item = null)
        {
            int zorderBegin = 0;

            if (item != null)
                zorderBegin = 1;

            for (int i = 0; i < processList.Count; i++)
            {
                processList[i].zOrder = i + zorderBegin;
            }

            if (item != null)
                item.zOrder = 0;

        }

        public void clearCardSelect()
        {
            selectedProcess.Clear();

            foreach (var item in processList)
            {
                item.selected = false;
            }

        }

        public void clearConnectionSelect()
        {
            selectedPoint = null;

            foreach (var item in processList)
            {
                item.clearPointSelected();
            }
        }

        public void clearConnectTo()
        {
            mousePoint = null;

            foreach (var item in processList)
            {
                item.clearTempConnection();
            }
        }

        public clsDataPoint selectConnectPoint(MouseEventArgs e, int marginx = 10, int marginy = 10)
        {
            foreach (var item in processList)
            {
                clsDataPoint con = item.hitTestPoint(e.Location, marginx, marginy);

                if (con != null)
                {
                    return con;
                }
            }

            return null;
        }

        public clsProcessZItem selectProcessCard(MouseEventArgs e)
        {
            foreach (var item in processList.OrderBy(item => item.zOrder))
            {
                if (item.hitTestProcess(e.Location))
                {
                    return item;
                }
            }

            return null;
        }

        public clsProcessZItem selectProcessItemByConPoint(clsDataPoint p)
        {
            foreach (var item in processList)
            {
                if (p is clsDataIn @in) if (item.frameIn.IndexOf(@in) >= 0) return item;

                if (p is clsDataOut @out) if (item.frameOut.IndexOf(@out) >= 0) return item;
            }

            return null;
        }

        public clsProcessZItem selectProcess(MouseEventArgs e)
        {
            clsProcessZItem tProcess = selectProcessCard(e);

            return tProcess;
        }

        public void onMouseDown(MouseEventArgs e, bool appendSelect)
        {

            clsDataPoint tPoint = selectConnectPoint(e);

            if (tPoint != null)
            {
                if (selectedPoint != null)
                    selectedPoint.selected = false;

                clsProcessZItem process = selectProcessItemByConPoint(tPoint);

                if (process.selected == false)
                {
                    clearCardSelect();

                    clearConnectionSelect();

                    clearConnectTo();
                }

                tPoint.selected = true;
                process.selected = true;

                selectedPoint = tPoint;
                selectedProcess.Add(process);

            }
            else
            {
                bool startGroupDrag = false;

                clsProcessZItem tProcess = selectProcessCard(e);

                if (selectedProcess.Contains(tProcess) == false)
                {
                    if (appendSelect == false)
                    {
                        clearCardSelect();

                        clearConnectionSelect();

                        clearConnectTo();
                    }

                    if (tProcess != null)
                    {
                        tProcess.selected = true;

                        tProcess.dragStartPoint = e.Location;

                        tProcess.dragDelta = tProcess.getDeltaPoint(e.Location);

                        resetZorder(tProcess);

                        selectedProcess.Add(tProcess);

                        if (appendSelect)
                        {
                            startGroupDrag = true;

                        }
                    }
                }
                else
                {
                    startGroupDrag = true;

                    if(selectedPoint != null)
                        selectedPoint.selected = false;

                    selectedPoint = null;
                }

                if (startGroupDrag)
                {
                    foreach (var item in selectedProcess)
                    {
                        item.dragStartPoint = e.Location;

                        item.dragDelta = item.getDeltaPoint(e.Location);

                        resetZorder(tProcess);
                    }
                }
            }
        }

        public void onMouseMove(MouseEventArgs e)
        {
            if (selectedPoint != null)
            {
                if (mousePoint == null)
                    mousePoint = selectedPoint.addConnectToTemp(e.Location.X, e.Location.Y);

                clsDataPoint temp = selectConnectPoint(e);

                if (temp != null)
                    mousePoint.center = temp.center;
                else
                    mousePoint.center = e.Location;
            }

            else if (selectedProcess.Count > 0)
            {
                foreach (var item in selectedProcess)
                {
                    item.updatePosition(item.dragStartPoint, e.Location, item.dragDelta);
                }
            }
        }

        public void onMouseUp(MouseEventArgs e)
        {
            if (selectedPoint != null)
            {
                clsDataPoint temp = selectConnectPoint(e);

                if (temp != null)
                {
                    if (selectedPoint.type != temp.type)
                    {
                        clsProcessZItem process = selectProcessItemByConPoint(temp);

                        if (process != null && selectedProcess.Count > 0)
                        {
                            disconnectDataIn(process, temp.name);

                            foreach (var item in selectedProcess)
                            {
                                item.addNextProcess(selectedPoint.name, process, temp.name);
                            }
                        }
                    }
                }
            }

            clearConnectTo();
        }

        public void selectProcessCardByRectangle(Rectangle rect)
        {
            foreach (var item in processList)
            {
                if (rect.IntersectsWith(item.bounds))
                {
                    item.selected = true;

                    selectedProcess.Add(item);
                }
            }
        }

        public bool isPointSelected()
        {
            return selectedPoint == null ? false : true;
        }

        public clsDataPoint getSelectedPoint()
        {
            return selectedPoint;
        }

        public bool isProcessSelected()
        {
            return selectedProcess.Count > 0 ? true : false;
        }

        public clsProcessZItem getSelectedProcess()
        {
            if (selectedProcess.Count > 0)
                return selectedProcess[0];

            return null;
        }

        public void removeSelectedProcess()
        {
            foreach (var item in selectedProcess)
            {
                string guid = item.guid;

                foreach (var t in item.frameIn)
                {
                    disconnectDataIn(item, t.name);
                }

                processList.RemoveAll(x => x.guid == guid);
            }

            selectedProcess.Clear();
        }

        public void disconnectDataIn(clsProcessZItem item, string name)
        {
            foreach (var process in processList)
            {
                process.clearDataOut(item.guid, name);
            }
        }

        public void findOutConnection(List<clsProcessZItem> r, clsProcessZItem item)
        {
            for (int i = 0; i < processList.Count; i++)
            {
                if (processList[i].isNextItem(item))
                {
                    //if (r.Select(x => x.guid == processList[i].guid).Any() == false)
                    {
                        r.Add(processList[i]);

                        findOutConnection(r, processList[i]);
                    }
                }
            }
        }

        public List<clsProcessZItem> makeTree(clsProcessZItem item)
        {
            List<clsProcessZItem> r = new List<clsProcessZItem>();

            findOutConnection(r, item);

            r.Add(item);

            r = r.GroupBy(x => x.depth).Select(group => group.First()).ToList();

            r.Sort((x, y) => x.depth.CompareTo(y.depth));

            return r;
        }

        public string generateCode(List<clsProcessZItem> items)
        {
            List<string> r = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                string t = items[i].generateCode(items);

                r.Add(t);
            }

            return string.Join(Environment.NewLine, r);
        }

        public void clearFromConnection(clsProcessZItem item, string name)
        {
            for (int i = 0; i < processList.Count; i++)
            {
                processList[i].removeDataOutConnection(item, name);
            }
        }
    }
}
