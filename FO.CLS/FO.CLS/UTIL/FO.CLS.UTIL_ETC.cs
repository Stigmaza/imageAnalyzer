using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Action = System.Action;
using Application = System.Windows.Forms.Application;
using CheckBox = System.Windows.Forms.CheckBox;
using DataTable = System.Data.DataTable;
using Label = System.Windows.Forms.Label;
using ListBox = System.Windows.Forms.ListBox;
using Point = System.Drawing.Point;
using TextBox = System.Windows.Forms.TextBox;

namespace FO.CLS.UTIL
{
    public class FOETC
    {
        const string BACKUPED_DISPLAY_MEMBER = "BACKUPED_DISPLAY_MEMBER";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public enum LV { UP, DOWN };

        // ----------------
        // 라벨등을 눌러서 메인폼 스크롤 가능하도록
        Form scrollMainForm;
        bool scrollOn;
        Point scrollPos;

        // ---------------------

        public class VKeyboard
        {
            [DllImport("User32.DLL")]
            public static extern Boolean PostMessage(Int32 hWnd, Int32 Msg, Int32 wParam, Int32 lParam);
            public const Int32 WM_USER = 1024;
            public const Int32 WM_CSKEYBOARD = WM_USER + 192;
            public const Int32 WM_CSKEYBOARDMOVE = WM_USER + 193;
            public const Int32 WM_CSKEYBOARDRESIZE = WM_USER + 197;

            static Process keyboardPs = null;

            public void show()
            {
                //if(keyboardPs == null)
                {
                    string filePath;
                    if(Environment.Is64BitOperatingSystem)
                    {
                        filePath = Path.Combine(Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "winsxs"),
                            "amd64_microsoft-windows-osk_*")[0],
                            "osk.exe");
                    }
                    else
                    {
                        filePath = @"C:\windows\system32\osk.exe";
                    }
                    if(File.Exists(filePath))
                    {
                        keyboardPs = Process.Start(filePath);
                    }
                }
            }
            public void hide()
            {
                try
                {
                    if(keyboardPs != null)
                    {
                        keyboardPs.Kill();
                    }
                }
                catch
                {
                    //log.WriteLog("except except : " + ex.ToString()); // 자세한 정보
                    //MessageBox.Show(ex.Message);    // 간략한 정보
                }
                finally
                {
                    keyboardPs = null;
                }
            }

            public void move(int x, int y, int w, int h)
            {
                if(keyboardPs.Handle != null)
                {
                    PostMessage(keyboardPs.Handle.ToInt32(), WM_CSKEYBOARDMOVE, x, y); // Move to 0, 0
                    PostMessage(keyboardPs.Handle.ToInt32(), WM_CSKEYBOARDRESIZE, w, h); // Resize to 600, 300
                }
            }
        }

        public VKeyboard keyboard = new VKeyboard();

        #region modaless
        /// <summary>
        /// 판넬을 팝업처럼 보이게
        /// </summary>
        /// <param name="form"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        /// <param name="ptLocation"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool showPanelModaless(Form form, Control c, Panel p, Point ptLocation, string msg = "")
        {
            Point pt = locationToControCenter(c, p);

            pt.X += ptLocation.X;
            pt.Y += ptLocation.Y;

            p.Parent = form;
            p.Location = pt;
            p.Visible = true;
            p.BringToFront();

            foreach(Control l in p.Controls)
            {
                if(l is Label)
                {
                    if(l.Tag == null)
                    {
                        l.Tag = l.Text;

                        if(msg == "")
                            msg = l.Tag as string;

                        l.Text = msg;
                    }

                    break;
                }
            }

            Application.DoEvents();

            return true;
        }

        public bool showPanelModaless(Form form, Control c, Panel p, string msg = "")
        {
            return showPanelModaless(form, c, p, new Point(0, 0), msg);
        }

        /// <summary>
        /// 지정 시간후 판넬 닫기
        /// </summary>
        /// <param name="pnl"></param>
        /// <param name="interval"></param>
        public void closePanelAfter(Panel pnl, int interval)
        {
            Task.Run(async () =>
            {
                await Task.Delay(interval);

                pnl.Invoke(new Action(() =>
                {
                    pnl.Visible = false;
                }));
            });

        }

        /// <summary>
        /// 판넬 닫기
        /// </summary>
        /// <param name="pnl"></param>
        public void closePanel(Panel pnl)
        {
            if(pnl == null) return;

            closePanelAfter(pnl, 1);
        }

        /// <summary>
        /// 메인폼 프리징 없이 시간대기
        /// </summary>
        /// <param name="d"></param>
        public void delay(int d)
        {
            DateTime max = DateTime.Now.AddMilliseconds(d);
            while(max > DateTime.Now) Application.DoEvents();
        }
        #endregion

        public DialogResult showPanelModal(Form parent, Control locationParent, Panel p, string msg = "", Point ptLocation = new Point())
        {
            Point pt = locationToControCenter(locationParent, p);

            pt.X += parent.Location.X + ptLocation.X;
            pt.Y += parent.Location.Y + ptLocation.Y;

            Form popupParent = p.Parent as Form;
            popupParent.ClientSize = p.Size;
            popupParent.StartPosition = FormStartPosition.Manual;
            popupParent.FormBorderStyle = FormBorderStyle.None;
            popupParent.Location = pt;
            //popupParent.ShowInTaskbar = true;
            popupParent.TopMost = true;


            p.Parent = popupParent;
            p.Location = new Point(0, 0);
            p.Visible = true;
            p.BringToFront();

            foreach(Control l in p.Controls)
            {
                if(l is Label)
                {
                    if(l.Tag == null)
                    {
                        l.Tag = l.Text;

                        if(msg == "")
                            msg = l.Tag as string;

                        l.Text = msg;
                    }

                    break;
                }
            }

            Application.DoEvents();

            return popupParent.ShowDialog();
        }

        /// <summary>
        /// DataGridView에 지정된 컬럼만 보이고, 나머지는 visible false 상태
        /// 리턴되는 DataTable은 컬럼형식이 모두 string
        /// 
        /// DataTable dt = mysql.Select(sql);
        ///  dtStockList = etc.dataGridFillFromDataTable(dgvStock, dt, new string[] { "MATNO", "MATCD", "LOTNO", "MATNM" });
        /// 
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="src"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public DataTable dataGridFillFromDataTable(DataGridView dgv, DataTable src, string[] columns)
        {
            DataTable r = src.Clone();

            dgv.SuspendLayout();

            try
            {
                // 임시테이블 컬럼 타입 스트링으로 변경
                foreach(DataColumn col in r.Columns)
                {
                    col.DataType = typeof(System.String);
                }

                // 내용 복사
                foreach(DataRow row in src.Rows)
                {
                    r.ImportRow(row);
                }

                // displayIndex 저장
                {
                    bool bSet = false;

                    for(int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if(dgv.Columns[i].Tag != null)
                            bSet = true;
                    }

                    // 최초 한번만 하도록
                    if(bSet == false)
                    {
                        for(int i = 0; i < dgv.Columns.Count; i++)
                        {
                            if(dgv.Columns[i].Tag == null)
                            {
                                dgv.Columns[i].Tag = dgv.Columns[i].DisplayIndex;
                            }

                        }
                    }
                }

                dgv.DataSource = r;

                // 지정된 컬럼만 남김 - 처음 실행할때 시간 많이 걸림
                for(int i = 0; i < dgv.Columns.Count; i++)
                {
                    if(dgv.Columns[i].Tag == null)
                    {
                        dgv.Columns[i].Visible = false;
                        dgv.Columns[i].DisplayIndex = dgv.Columns.Count - 1;
                    }
                }

                for(int i = 0; i < dgv.Columns.Count; i++)
                {
                    if(dgv.Columns[i].Visible)
                    {
                        int index = Convert.ToInt32(dgv.Columns[i].Tag.ToString());
                        dgv.Columns[i].DataPropertyName = columns[index];
                        dgv.Columns[i].DisplayIndex = index;
                    }
                }

                /*
                for(int i = 0; i < dgv.Columns.Count; i++)
                {
                    dgv.Columns[i].Visible = false;
                }

                for(int i = 0; i < columns.Length; i++)
                {
                    dgv.Columns[i].Visible = true;
                    dgv.Columns[i].DataPropertyName = columns[i];
                    dgv.Columns[i].DisplayIndex = i;
                }
                */

                reOrderDisplayIndex(dgv);

                dgv.ClearSelection();
                //dgv.Update(); 
            }
            catch
            {
                throw;
            }
            finally
            {
                dgv.ResumeLayout(true);
            }

            return r;
        }


        private DataGridViewColumn getDataGridViewColumnByName(DataGridView dgv, string name)
        {
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].HeaderText == name)
                {
                    return dgv.Columns[i];
                }
            }

            return null;
        }

        public void reOrderDisplayIndex(DataGridView dgv, DataTable dt = null)
        {
            if(dt == null)
                dt = (DataTable)dgv.DataSource;

            for(int i = 0; i < dt.Columns.Count; i++)
            {
                DataGridViewColumn col = getDataGridViewColumnByName(dgv, dt.Columns[i].Caption);

                if( col != null)
                    col.DisplayIndex = i;
            }
        }

        /// <summary>
        /// dataGridFillFromDataTable(gdvAlarmList, dt, "ALARMDATETIME, ALARMCODE, ALARMSTATUS, ERROR_CONTENT_KOR" );
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="src"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public DataTable dataGridFillFromDataTable(DataGridView dgv, DataTable src, string columns)
        {
            return dataGridFillFromDataTable(dgv, src, split(columns));
        }

        public void clearDataGridView(DataGridView dgv)
        {
            DataTable dt = dgv.DataSource as DataTable;

            if(dt != null)
            {
                dt.Clear();
            }
        }

        public string[] split(string str)
        {
            string[] words = str.Split(',');

            for(int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
            }

            return words;
        }
        /// <summary>
        /// 지정된 컬러명으로 DataTable을 만들어 돌려준다
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public DataTable createTable(string[] columns)
        {
            DataTable temp = new DataTable();

            // 컬럼 생성
            foreach(string columnName in columns)
            {
                temp.Columns.Add(columnName);
            }

            return temp;
        }
        public DataTable createTable(string columns)
        {
            return createTable(split(columns));
        }

        /// <summary>
        /// 버튼 클릭으로 리스트에서 선택된 행의 위치를 위아래로 변경 // 
        /// direction - 0: 위, 1: 아래
        /// </summary>
        /// <param name="list"></param>
        /// <param name="direction"></param>
        public void changeListviewSelection(ListView list, LV direction)
        {
            try
            {
                if(list.Visible && list.Items.Count > 0)
                {
                    if(list.SelectedIndices.Count == 0)
                    {
                        if(direction == 0)
                            list.Items[list.Items.Count - 1].Selected = true;
                        else
                            list.Items[0].Selected = true;

                        return;
                    }

                    int index = list.SelectedIndices[0];

                    if(direction == LV.UP) index = index - 1;
                    if(direction == LV.DOWN) index = index + 1;

                    if(index >= 0 && index < list.Items.Count)
                    {
                        list.SelectedItems.Clear();
                        list.Items[index].Selected = true;
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 리스트뷰의 선택된 아이템의 위치를 위아래로 이동 //
        /// LV.UP, LP.DOWN
        /// </summary>
        /// <param name="list"></param>
        /// <param name="direction"></param>
        public void moveListViewItem(ListView list, LV direction)
        {
            try
            {
                if(list.Visible && list.Items.Count > 0)
                {
                    if(list.SelectedItems.Count == 0)
                        return;

                    int index = list.SelectedItems[0].Index;
                    var item = list.SelectedItems[0];

                    if(direction == LV.UP) index = index - 1;
                    if(direction == LV.DOWN) index = index + 1;

                    if(index >= 0 && index < list.Items.Count)
                    {
                        list.Items.Remove(item);
                        list.Items.Insert(index, item);
                        item.Selected = true;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 지정된 해상도일때 전체화면으로 만들기 //
        /// 작업표시줄 보이지 않음 //
        /// 전체화면이 됬을때 메인폼 size를 1040, 807 하고 //
        /// 메인폼에 판넬을 넣고 dock = fill로 했을때 판넬 크기가 1024*768이 됨
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void makeFullSize(Form owner, int width = 1024, int height = 768)
        {
            if(Screen.PrimaryScreen.Bounds.Width == width && Screen.PrimaryScreen.Bounds.Height == height)
            {
                owner.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                owner.WindowState = FormWindowState.Maximized;
            }
        }


        /// <summary>
        /// 실행중인 프로세스 찾아서 맨앞으로 나오도록
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool findProcess(string filename)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filename));

            foreach(Process p in processes)
            {
                IntPtr windowHandle = p.MainWindowHandle;

                SetForegroundWindow(p.MainWindowHandle);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 프로그램 실행
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool startProcess(string filename)
        {
            if(findProcess(filename))
                return true;

            Process psRecipe = new Process();
            psRecipe.StartInfo.FileName = filename;
            psRecipe.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);

            if(File.Exists(filename))
            {
                psRecipe.Start();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 지정된 문자열을 따옴표 문자열로 변경 // 
        /// QuotedString("123") -> '123'
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string QuotedString(string str)
        {
            return "'" + str + "'";
        }

        public string QuotedString(int str)
        {
            return QuotedString(str.ToString());
        }

        public string QuotedString(object row)
        {
            return QuotedString(toStrDef(row));
        }

        public string qs(string str)
        {
            return QuotedString(str);
        }

        public string qs(int str)
        {
            return QuotedString(str.ToString());
        }

        public string qs(object row)
        {
            return QuotedString(toStrDef(row));
        }

        public string RemoveComma(string str)
        {
            return str.Replace(",", "");
        }

        /// <summary>
        /// 문자열을 딕셔너리로 만들어 반환   //
        //  "a=123,b=456,c=789" -> dict["a"], dict["b"]
        /// </summary>
        /// <param name="dictString"></param>
        /// <param name="sep"></param>
        /// <returns></returns>
        public Dictionary<string, string> stringToDictionary(string dictString, char sep = ',')
        {
            return dictString.Split(sep)
                             .Select(pp => pp.Trim().Split('='))
                             .ToDictionary(pp => pp[0], pp => pp[1]);
        }

        /// <summary>
        /// form 기준으로 지정된 컴포넌트의 location을 구한다
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public Point locationToFormLocation(Control con)
        {
            if(con is Form)
            {
                return new Point(0, 0);
            }

            Point loc1 = con.Location;
            Point loc2 = (con.Parent != null) ? con.Parent.PointToScreen(loc1) : con.PointToScreen(loc1);

            return con.FindForm().PointToClient(loc2);
        }

        /// <summary>
        /// panel컴포넌트를 parent컴포넌트의 중간위치에 가도록 위치계산
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="panel"></param>
        /// <returns></returns>
        public Point locationToControCenter(Control parent, Control panel)
        {
            Point pt = locationToFormLocation(parent);

            pt.X = pt.X + (parent.Width / 2) - (panel.Width / 2);
            pt.Y = pt.Y + (parent.Height / 2) - (panel.Height / 2);

            return pt;
        }

        /// <summary>
        /// 지정된 컬럼에서 (g)가 들어간 컬럼에 쉼표 넣기 - 이벤트 메소드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void etc_cellNumberComma(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if(sender == null) return;
            if(e == null) return;

            DataGridView dgv = (DataGridView)sender;

            // 헤더 텍스트에 g가 있는것만 변환하도록
            if(dgv.Columns[e.ColumnIndex].HeaderText.IndexOf("(g)") == -1 &&
                dgv.Columns[e.ColumnIndex].HeaderText.IndexOf("(Kg)") == -1 &&
                dgv.Columns[e.ColumnIndex].HeaderText.IndexOf("%") == -1
                ) return;

            try
            {
                string num = "0.0";
                double intNum;

                if(e.Value != null)
                    num = e.Value.ToString();

                if(double.TryParse(num, out intNum))
                {
                    e.Value = string.Format("{0:#,##0.000}", intNum);
                }
                else
                {
                    e.Value = num;
                }
            }
            catch
            {
                //log.WriteLog("except except : " + ex.ToString()); // 자세한 정보
                //MessageBox.Show(ex.Message);	// 간략한 정보
            }
            finally
            {
            }
        }


        /// <summary>
        /// 지정된 컬럼에서 (g)가 들어간 컬럼에 쉼표 넣기 - 호출할 함수
        /// </summary>
        /// <param name="dgv"></param>
        public void dataGridViewComma(DataGridView dgv)
        {
            dgv.CellFormatting += this.etc_cellNumberComma;
        }
        /// <summary>
        /// 그리드뷰를 리스트뷰 처럼 보이도록
        /// </summary>
        /// <param name="dgv"></param>
        public void dataGridViewLookLikeListView(DataGridView dgv)
        {
            /*
                ///////////       수동으로 설정       ///////////
                
                아래 항목은 폼디자이너 에서 수동으로 설정한다     

                // 인디케이터 없애기
                dgv.RowHeadersVisible = false;

                // 배경 하얗게
                dataGridView1.BackgroundColor = Color.White;
                                    
                // 추가되는 레코드 높이 조절
                RowTemplate.Height
                        
                // 추가되는 레코드 수정 안되도록
                // 그리드 전체에 적용됨, 하나라도 수정이 가능해야 하면,
                // 아래항목은 true로 설정하고 컬럼별로 edit false 해야함
                RowTemplate.ReadOnly
                        
                // 셀 편집 안되게
                dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically

                ///////////       참고       ///////////

                
                // AutoSizeColumnsMode - fill 일때, 원하는 컬럼 넓이 조절하기
                columns - 컬럼 - AutoSizeMode = none, width 설정

                // 인디케이터 넓이 조절 
                dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;            
             
                // cell Click 이벤트
                private void dgvStock_CellClick(object sender, DataGridViewCellEventArgs e)
                {
                    if (sender == null) return;
                    if (e.RowIndex < 0) return;

                    DataGridView dgv = (DataGridView) sender;

                    if (dgv.Columns[e.ColumnIndex].HeaderText == "밸브")  // 컬럼 헤더 텍스트로 구분
                    {
                        DataRow dr = dtStockList.Rows[e.RowIndex];

                        frmPopup.setManulStockInfomation(dr);

                        frmPopup.showPanelAsModal(frmPopup.pnlManualValve);
                    }
                }
            */

            // 마우스 클릭으로 컬럼 넓이 조절 안되도록
            //dgv.AllowUserToResizeColumns = false;

            // 다중선택 끄기
            //dgv.MultiSelect = false;

            // 레코드 전체 선택
            //dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 컬럼 타이틀 중앙 정렬
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 짝수라인 배경색 다르게
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            // 컬럼 헤더 높이 조절 
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // 레코드 추가 
            dgv.AllowUserToAddRows = false;

            // 레코드 삭제 
            dgv.AllowUserToDeleteRows = false;

            // 레코드 높이 조절 - 폼디자이너에서 컴포넌트 만들었을때 첫번째 레코드에 대해서만
            dgv.AllowUserToResizeRows = false;

            // 레코드 높이 조절 - 추가되는 새 레코드에 대해
            dgv.RowTemplate.Resizable = DataGridViewTriState.False;

            // 컬럼 클릭시 레코드 정렬 없애기
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        public string intArrayToHexString(int[] array)
        {
            var builder = new StringBuilder();
            Array.ForEach(array, x => builder.Append(x.ToString("X4") + " "));
            string s = builder.ToString();

            return s;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// 지정된 필드를 숫자형으로 바꾸는데, 오류가 있으면 nDefaultValue로 바꿈
        /// </summary>
        /// <param name="row"></param>
        /// <param name="nDefaultValue"></param>
        /// <returns></returns>
        public int toIntDef(object row, int nDefaultValue = 0)
        {
            int nResult = nDefaultValue;

            object value = row;

            if((value is DBNull) == false && value != null)
            {
                string sValue = value.ToString();

                if(int.TryParse(sValue, out nResult) == false)
                    nResult = nDefaultValue;
            }

            return nResult;
        }
        public double toDoubleDef(object row, double nDefaultValue = 0)
        {
            double nResult = nDefaultValue;

            object value = row;

            if((value is DBNull) == false && value != null)
            {
                string sValue = value.ToString();

                if(double.TryParse(sValue, out nResult) == false)
                    nResult = nDefaultValue;
            }

            return nResult;
        }

        public double toDoubleDef(string txt, double nDefaultValue = 0)
        {
            if(double.TryParse(txt, out double nResult) == false)
            {
                nResult = nDefaultValue;
            }

            return nResult;
        }
        public int toIntDef(string txt, int nDefaultValue = 0)
        {
            if(int.TryParse(txt, out int nResult) == false)
            {
                nResult = nDefaultValue;
            }

            return nResult;
        }

        public string toStrDef(object row, string nDefaultValue = "")
        {
            string nResult = nDefaultValue;

            object value = row;

            if((value is DBNull) == false && value != null)
            {
                return value.ToString();
            }

            return nResult;
        }

        public string toStrDef(DataRow dr, string columnName, string nDefaultValue = "")
        {
            string nResult = nDefaultValue;

            try
            {
                nResult = dr[columnName].ToString();
            }
            catch
            {
            }

            return nResult;
        }

        //----------------------------------------------------------------------
        public double strToDoubleDef(string str, double def, int multiply = 1)
        {
            if(multiply == 0) multiply = 1;

            double r = def;

            if(double.TryParse(str, out r) == true)
            {
                r *= multiply;
            }

            return r;
        }

        public int strToIntDef(string str, int def = 0)
        {
            int r = def;

            if(int.TryParse(str, out r) == false)
                r = def;

            return r;
        }

        //----------------------------------------------------------------------

        public void setControlsFromDataTable(DataTable dt, int index, List<Control> controls)
        {
            if(dt == null) return;

            if(index >= dt.Rows.Count)
                return;

            for(int i = 0; i < controls.Count; i++)
            {
                string colName = controls[i].Name;

                colName = colName.Substring(3, colName.Length - 3);

                if(controls[i] is TextBox)
                {
                    TextBox item = (TextBox)controls[i];

                    try
                    {
                        item.Text = toStrDef(dt.Rows[index][colName]);
                    }
                    catch
                    {
                    }
                }
                else if(controls[i] is Label)
                {
                    Label item = (Label)controls[i];

                    try
                    {
                        item.Text = toStrDef(dt.Rows[index][colName], "");
                    }
                    catch
                    {
                    }
                }
                else if(controls[i] is CheckBox)
                {
                    CheckBox item = (CheckBox)controls[i];

                    try
                    {
                        double bin = toDoubleDef(dt.Rows[index][colName].ToString(), -999999999);

                        if(bin != -999999999)   // 들어있는 값이 숫자라면
                        {
                            if(bin == 0)        // 값이 0 이 아니면 true
                                item.Checked = false;
                            else
                                item.Checked = true;
                        }
                        else   // 숫자가 아닌게 들어있는경우
                        {
                            string yntf = toStrDef(dt.Rows[index][colName].ToString(), "X");

                            if(yntf == "Y" || yntf == "T")        // Y T 들어있는 경우 true
                                item.Checked = true;
                            else
                                item.Checked = false;
                        }
                    }
                    catch
                    {
                    }
                }
                else if(controls[i] is ComboBox)
                {
                    ComboBox item = (ComboBox)controls[i];

                    if(item.ValueMember != null && item.ValueMember.Length > 0)
                    {
                        if(dt.Columns.IndexOf(item.ValueMember) != -1)
                            lookupSelectByValue(item, toStrDef(dt.Rows[index][item.ValueMember]));
                    }
                }
            }
        }

        public void setControlsFromDataGridView(DataGridView dgv, List<Control> controls)
        {
            if(dgv == null || controls == null) return;

            DataTable dt = dgv.DataSource as DataTable;

            //int index = getGridSelectedDataGridIndex(dgv);
            int index = getGridSelectedDataTableIndex(dgv);

            if (dt == null || index < 0)
                return;

            setControlsFromDataTable(dt, index, controls);
        }

        // 컨트롤 리스트, dgv에 연결된 데이터 테이블의 컬럼명과 같은 이름, 텍스트 박스 클리어
        // -> 바인딩 되어있는 컨트롤 리셋
        public void clearTextFromColumnExist(DataGridView dgv, List<Control> controls)
        {
            if(dgv == null || controls == null) return;

            DataTable dt = dgv.DataSource as DataTable;

            if(dt == null) return;

            for(int i = 0; i < dt.Columns.Count; i++)
            {
                string columnName = dt.Columns[i].ColumnName;

                // 3은 고정이다, 네이밍 규칙 따르면 고정해도 된다
                List<Control> t = controls.Where(x => x.Name.Substring(3) == columnName).ToList();

                if(t.Count > 0)
                {
                    for(int tcount = 0; tcount < t.Count; tcount++)
                    {
                        if(t[tcount] is TextBox)
                            t[tcount].Text = "";

                        if(t[tcount] is ComboBox)
                            ((ComboBox)t[tcount]).SelectedIndex = -1;
                    }
                }
            }
        }

        public void setControlsFromDataGridView(object sender, List<Control> controls)
        {
            setControlsFromDataGridView((DataGridView)sender, controls);
        }

        public void controlsToDataTable(DataTable dt, int index, List<Control> controls)
        {
            if(index >= dt.Rows.Count)
                return;

            for(int i = 0; i < controls.Count; i++)
            {
                string colName = controls[i].Name;

                colName = colName.Substring(3, colName.Length - 3);

                if(controls[i] is TextBox)
                {
                    TextBox item = (TextBox)controls[i];

                    try
                    {
                        dt.Rows[index][colName] = item.Text;
                    }
                    catch
                    {
                    }
                }

                else if(controls[i] is CheckBox)
                {
                    CheckBox item = (CheckBox)controls[i];

                    try
                    {
                        dt.Rows[index][colName] = (item.Checked) ? "Y" : "N";
                    }
                    catch
                    {
                    }
                }
                else if(controls[i] is ComboBox)
                {
                    ComboBox item = (ComboBox)controls[i];

                    if(item.DisplayMember != null && item.DisplayMember.Length > 0)
                    {
                        if(lookupisExistBackupDisplayMember(item))
                        {
                            dt.Rows[index][item.DisplayMember] = lookupGetBackupedDisplayMember(item);
                        }
                        else
                        {
                            if(dt.Columns.IndexOf(item.DisplayMember) != -1)
                                dt.Rows[index][item.DisplayMember] = lookupDisplayString(item);
                        }
                    }

                    if(item.ValueMember != null && item.ValueMember.Length > 0)
                    {
                        if(dt.Columns.IndexOf(item.ValueMember) != -1)
                            dt.Rows[index][item.ValueMember] = lookupValueSelected(item);
                    }
                }
            }
        }

        public void controlsToDataGridView(DataGridView dgv, List<Control> controls)
        {
            if(controls == null) return;

            DataTable dt = dgv.DataSource as DataTable;

            //int index = getGridSelectedDataGridIndex(dgv);
            int index = getGridSelectedDataTableIndex(dgv);

            if (dt == null || index < 0)
                return;

            controlsToDataTable(dt, index, controls);
            dgv.Invalidate();
        }

        public void clearControlsList(List<Control> controls)
        {
            for(int i = 0; i < controls.Count; i++)
            {
                if(controls[i] is TextBox)
                {
                    TextBox item = (TextBox)controls[i];

                    item.Text = string.Empty;
                }

                else if(controls[i] is CheckBox)
                {
                    CheckBox item = (CheckBox)controls[i];

                    item.Checked = false;
                }
            }
        }

        //----------------------------------------------------------------------

        public void replaceDataGridHeaderText(DataGridView dgv, string p1, string p2)
        {
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].HeaderText = dgv.Columns[i].HeaderText.Replace(p1, p2);
            }
        }

        #region plc 관련 데이터 변환 함수

        /// <summary>
        /// plc에서 읽어온 word배열을 문자열로 변경
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        /*
            int[] arr = xgk.Read10Word("900");  // A
            string str = xgk.toStringFromPlcWordArray(arr);

            arr = xgk.toWordArrayFromString("하이ㅁ니아러ㅣㄴ마어리ㅏ넝ㅁ리ㅏ", 50); // B

            for (int i = 0; i < 50; i++)    // C
            {
                xgk.WriteWord((900 + i).ToString(), arr[i]);
            }

            // B, C의 길이가 같아야 한다
        */

        public string toStringFromPlcWordArray(int[] src, int pos, int wordLen)
        {
            // src가 word배열이라 길이 * 2
            byte[] byteArray = new byte[wordLen * 2];

            // 상하위 바이트 위치 바꿈
            for(int i = 0; i < wordLen; i++)
            {
                byteArray[i * 2 + 0] = (byte)(src[pos + i] & 0x00ff);
                byteArray[i * 2 + 1] = (byte)((src[pos + i] & 0xff00) >> 8);
            }

            string t = System.Text.Encoding.Default.GetString(byteArray);

            t = t.Replace((char)0, ' ');    // 문자가 아닌것 공백으로 변환

            return t.Trim();
        }

        /// <summary>
        /// 문자열을 word배열로 변경
        /// </summary>
        /// <param name="str">문자열</param>
        /// <param name="maxWordLen">최대길이</param>
        /// <returns></returns>
        public int[] toWordArrayFromString(string str, int maxWordLen)
        {
            // 변환과정은 BYTE로 진행 되는거라 길이 * 2
            byte[] byteArray = Enumerable.Repeat<byte>(0, maxWordLen * 2).ToArray<byte>();
            byte[] srcArray = System.Text.Encoding.Default.GetBytes(str);

            // 파라미터로 받은 문자열 복사
            for(int i = 0; i < maxWordLen * 2 && i < srcArray.Length; i++)
                byteArray[i] = srcArray[i];

            int[] intArray = new int[maxWordLen];

            // 상하위 바이트 위치 바꿈
            for(int i = 0; i < intArray.Length; i++)
            {
                intArray[i] = (byteArray[i * 2 + 1] << 8) + (byteArray[i * 2]);
            }

            return intArray;
        }

        /// <summary>
        /// plc에서 받아온 int 배열에서 지정된 위치부터 함수명의 형식으로 변경
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public short makeSigned16(int[] data, int pos)
        {
            if(pos < data.Length)
            {
                return (short)data[pos];
            }

            return short.MaxValue;
        }
        /// <summary>
        /// plc에서 받아온 int 배열에서 지정된 위치부터 함수명의 형식으로 변경
        /// plc에서 읽은 값이 100 이고 div가 10 인경우, 값 / div -> 10.0 으로 변환됨
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pos"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public string makeSigned16str(int[] data, int pos, int div = 1)
        {
            double t = makeSigned16(data, pos);

            t /= div;

            return t.ToString(getBaseFormat(div));
        }
        //----------------------------------------------------------------------
        public ushort makeUnSigned16(int[] data, int pos)
        {
            if(pos < data.Length)
            {
                return (ushort)data[pos];
            }

            return ushort.MaxValue;
        }
        public string makeUnSigned16str(int[] data, int pos, int div = 1)
        {
            double r = makeUnSigned16(data, pos);

            r /= div;

            return r.ToString(getBaseFormat(div));
        }
        //----------------------------------------------------------------------
        public double makeSigned32(int[] data, int pos)
        {
            if(pos + 1 < data.Length)
            {
                return data[pos] + (data[pos + 1] << 16);
            }

            return double.MaxValue;
        }

        public string makeSigned32str(int[] data, int pos, int div = 1)
        {
            double r = makeSigned32(data, pos);

            r /= div;

            return r.ToString(getBaseFormat(div));
        }
        //----------------------------------------------------------------------
        public uint makeUnSigned32(int[] data, int pos)
        {
            if(pos + 1 < data.Length)
            {
                return (uint)(data[pos] + (data[pos + 1] << 16));
            }

            return uint.MaxValue;
        }
        public string makeUnSigned32str(int[] data, int pos, int div = 1)
        {
            double r = makeUnSigned32(data, pos);

            r /= div;

            return r.ToString(getBaseFormat(div));
        }
        //----------------------------------------------------------------------
        private string getBaseFormat(double div)
        {
            string f = (1 / div).ToString();

            return f.Replace("1", "0");
        }
        //----------------------------------------------------------------------
        /// <summary>
        /// 기존의 make2word 함수와 같음, writeplcword 함수에서 사용하기 위해 만듬, private!!
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int[] makeTwoWordArray(double data)
        {
            int[] r = new int[2];

            int t = (int)data;

            r[0] = t & 0xffff;
            r[1] = (t >> 16) & 0xffff;

            return r;
        }
        //----------------------------------------------------------------------
        /// <summary>
        /// int[1] t = strToDoubleOneWordArray( "123.4", 10 );
        /// t[0] == 1234
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="multiply"></param>
        /// <returns></returns>
        public int[] toOneWordArray(string str, int multiply = 1)
        {
            if(multiply == 0) multiply = 1;
            int[] r = new int[1];

            double t = strToDoubleDef(str, 0, multiply);

            r[0] = (int)t;

            return r;
        }

        /// <summary>
        /// int[2] t = strToDoubleTwoWordArray( "123.4", 10 );
        /// t[0] == 1234
        /// t[1] == 0
        /// </summary>
        /// <param name="str"></param>
        /// <param name="multiply"></param>
        /// <returns></returns>
        public int[] toTwoWordArray(string str, int multiply = 1)
        {
            if(multiply == 0) multiply = 1;
            double t = strToDoubleDef(str, 0, multiply);

            return makeTwoWordArray(t);
        }
        //----------------------------------------------------------------------
        public int[] toOneWordArray(double d, int multiply = 1)
        {
            if(multiply == 0) multiply = 1;
            int[] r = new int[1];

            r[0] = (int)(d * multiply);

            return r;
        }

        public int[] toTwoWordArray(double d, int multiply = 1)
        {
            if(multiply == 0) multiply = 1;
            d *= multiply;

            return makeTwoWordArray(d);
        }

        #endregion


        public int getBitValueFromRight(int t, int index)
        {
            t >>= index;

            return (t & 0x1);
        }

        public bool getBitOnOff(int t, int index)
        {
            if(getBitValueFromRight(t, index) == 1)
                return true;

            return false;
        }

        public bool isZero(double t)
        {
            if(t == 0)
                return true;

            return false;
        }

        public bool isZero(string t)
        {
            return isZero(strToDoubleDef(t, 0));
        }

        public int hiByte(int t)
        {
            return t >> 8;
        }

        public int loByte(int t)
        {
            return t & 0xff;
        }

        public void dataTableToComboBox(DataTable dt, string colname, ComboBox cb)
        {
            cb.Items.Clear();

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                cb.Items.Add(dt.Rows[i][colname].ToString());
            }
        }

        private void lookupBackupDisplayMember(DataTable dt, string displayMember)
        {
            if(dt.Columns.IndexOf(BACKUPED_DISPLAY_MEMBER) == -1)
            {
                dt.Columns.Add(BACKUPED_DISPLAY_MEMBER);

                for(int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][BACKUPED_DISPLAY_MEMBER] = dt.Rows[i][displayMember].ToString();
                }
            }
        }

        public bool PrevInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            if(Process.GetProcessesByName(currentProcess.ProcessName, currentProcess.MachineName).Length > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> singleColumnToStringList(DataTable dt, string columnName)
        {
            List<string> r = new List<string>();

            try
            {
                List<object> t = dt.Select().Select(x => x[columnName]).ToList();
                r = t.Cast<string>().ToList();
            }
            catch
            {
            }
            return r;
        }

        public List<double> singleColumnToDoubleList(DataTable dt, string columnName)
        {
            List<double> r = new List<double>();

            try
            {
                List<object> t = dt.Select().Select(x => x[columnName]).ToList();
                r = t.Cast<double>().ToList();
            }
            catch
            {
            }
            return r;
        }

        public List<int> singleColumnToIntList(DataTable dt, string columnName)
        {
            List<int> r = new List<int>();

            try
            {
                List<object> t = dt.Select().Select(x => x[columnName]).ToList();
                r = t.Cast<int>().ToList();
            }
            catch
            {
            }
            return r;
        }

        public DataTable columnsToDataTable(DataTable dt, string columnNames, bool distinct = true)
        {
            if(dt == null || dt.Rows.Count == 0)
                return new DataTable();

            string[] t = split(columnNames);

            DataTable r = dt.DefaultView.ToTable(distinct, t);

            return r;
        }

        public string columnsToString(DataTable dt, string sep)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int i = 0; i < dt.Columns.Count; i++)
            {
                r = r + c + dt.Columns[i].ColumnName;
                c = sep;
            }

            return r;
        }

        public List<string> columnsToList(DataTable dt)
        {
            List<string> r = new List<string>();

            for(int i = 0; i < dt.Columns.Count; i++)
            {
                r.Add(dt.Columns[i].ColumnName);
            }

            return r;
        }

        public double sumColumn(DataTable dt, string column)
        {
            double r = 0;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                double t = toDoubleDef(dt.Rows[i][column]);

                r += t;
            }

            return r;
        }

        public string rowToString(DataTable dt, int index, string sep)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int i = 0; i < dt.Columns.Count; i++)
            {
                r = r + c + toStrDef(dt.Rows[index][i], "");
                c = sep;
            }

            return r;
        }

        public void dataTableToFile(DataTable dt, string path, string sep = "|")
        {
            string str = string.Empty;
            string c = string.Empty;

            str = columnsToString(dt, sep) + Environment.NewLine;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                str = str + c + rowToString(dt, i, sep);
                c = Environment.NewLine;
            }

            File.WriteAllText(path, str);
        }

        public string dataTableToString(DataTable dt, bool includeHeader, string sep)
        {
            string str = string.Empty;
            string c = string.Empty;

            if (dt != null)
            {
                if (includeHeader)
                    str = columnsToString(dt, sep) + Environment.NewLine;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    str = str + c + rowToString(dt, i, sep);
                    c = Environment.NewLine;
                }
            }

            return str;
        }

        private string rowToString(DataGridView dgv, int index, string sep)
        {
            string str = string.Empty;
            string c = string.Empty;

            for (int columnindex = 0; columnindex < dgv.Columns.Count; columnindex++)
            {
                if (dgv.Columns[columnindex].Visible)
                {
                    str = str + c + toStrDef(dgv.Rows[index].Cells[columnindex].Value);
                    c = sep;
                }
            }

            return str;
        }

        public string dataGridViewToString(DataGridView dgv, bool includeHeader, string sep)
        {
            string str = string.Empty;
            string c = string.Empty;
                        
            if (includeHeader)
            {
                for (int columnindex = 0; columnindex < dgv.Columns.Count; columnindex++)
                {
                    if (dgv.Columns[columnindex].Visible)
                    {
                        str = str + c + dgv.Columns[columnindex].HeaderText;
                        c = sep;
                    }
                }

                str += Environment.NewLine;
            }

            c = string.Empty;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                str = str + c + rowToString(dgv, i, sep);
                c = Environment.NewLine;
            }

            return str;
        }


        public DataTable dataTableFromFile(string path, Encoding endcode, char sep = '|')
        {
            if(File.Exists(path))
            {
                string[] fileContents = File.ReadAllLines(path, endcode);

                string[] columns = fileContents[0].Split(sep);

                DataTable dtR = createTable(columns);

                for(int i = 1; i < fileContents.Length; i++)
                {
                    string[] temp = fileContents[i].Split(sep);

                    DataRow row = dtR.NewRow();

                    for(int k = 0; k < columns.Length; k++)
                        row[k] = temp[k];

                    dtR.Rows.Add(row);

                }
                return dtR;
            }

            return null;
        }

        public DataTable dataTableFromFile(string path, char sep = '|')
        {
            return dataTableFromFile(path, Encoding.Default, sep);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="direction"> - 1 : 위, 1 : 아래 </param>
        public void insertRecord(object sender, bool copy, int direction = 1)
        {
            insertRecord((DataGridView)sender, copy, direction);
        }

        public void insertRecord(DataGridView dgv, bool copy, int direction = 1)
        {
            try
            {
                DataTable dt = dgv.DataSource as DataTable;

                if(dt == null) return;

                //int index = getGridSelectedDataGridIndex(dgv);
                int index = getGridSelectedDataTableIndex(dgv);

                if (index == -1)
                {
                    index = dt.Rows.Count - 1;
                }

                int copyFrom = index;

                if(direction == -1)
                {
                    direction = 0;

                    if(copy)
                        copyFrom++;
                }

                int newIndex = index + direction;

                insertRecord(dt, newIndex, (copy) ? copyFrom : -1);

                //int t = getGridSelectedDataGridIndex(dgv);

                int cindex = dgv.CurrentCell.ColumnIndex;

                dgv.ClearSelection();
                //dgv.Rows[newIndex].Selected = true;
                dgv.CurrentCell = dgv.Rows[newIndex].Cells[cindex];

                dgv.Invalidate();
            }
            catch
            {

            }
        }

        public void insertRecord(DataTable dt, int rowIndex, int copyFrom)
        {
            dt.Rows.InsertAt(dt.NewRow(), rowIndex);

            if(copyFrom != -1)
            {
                for(int i = 0; i < dt.Columns.Count; i++)
                {
                    dt.Rows[rowIndex][i] = dt.Rows[copyFrom][i];
                }
            }
        }

        private int calcNextPosition(DataTable dt, int rowIndex, int direction)
        {
            if(direction == -1)
            {
                if(rowIndex <= 0) return -1;

                return rowIndex - 1;
            }
            else // direction == 1
            {
                if(rowIndex >= dt.Rows.Count - 1) return -1;

                return rowIndex + 1;
            }
        }

        private bool isNearSelectionExist(int[] selectionList, int arrayIndex, int direction)
        {
            int nextArrayIndex = arrayIndex - 1;

            if(nextArrayIndex < 0 || nextArrayIndex >= selectionList.Length) return true;

            if(selectionList[nextArrayIndex] == selectionList[arrayIndex] + direction)
                return false;

            return true;
        }

        public bool moveDataRecord(DataTable dt, int rowIndex, int direction)
        {
            DataRow dr = dt.Rows[rowIndex];

            DataRow newRow = dt.NewRow();

            newRow.ItemArray = dr.ItemArray;

            dt.Rows.Remove(dr);

            dt.Rows.InsertAt(newRow, rowIndex + direction);

            return true;
        }

        public void moveDataRecord(DataGridView dgv, int direction)
        {
            if(dgv == null) return;
            if (dgv.SelectedRows.Count == 0) return;

            //if (dgv.CurrentCell == null) return;

            //int[] selectionList = new int[dgv.SelectedRows.Count];
            //for (int i = 0; i < dgv.SelectedRows.Count; i++)
            //{
            //    selectionList[i] = dgv.SelectedRows[i].Index;
            //}

            int rindex = dgv.CurrentCell.RowIndex;
            int cindex = dgv.CurrentCell.ColumnIndex;
                        
            int[] selectionList = getDataGridViewSelectedDataTableIndex(dgv).ToArray();

            Array.Sort(selectionList);
            if(direction > 0)
                Array.Reverse(selectionList);

            DataTable dt = dgv.DataSource as DataTable;

            for(int i = 0; i < selectionList.Length; i++)
            {
                int rowIndex = selectionList[i];
                int nextIndex = calcNextPosition(dt, rowIndex, direction);

                if(nextIndex != -1 && isNearSelectionExist(selectionList, i, direction))
                {
                    moveDataRecord(dt, rowIndex, direction);
                    selectionList[i] = nextIndex;

                    if(rowIndex == rindex)
                        dgv.CurrentCell = dgv.Rows[nextIndex].Cells[cindex];

                }
            }

            // 레코드를 새로 만들기때문에 선택한것 지우고 다시 선택한다
            dgv.ClearSelection();

            for(int i = 0; i < selectionList.Length; i++)
            {
                dgv.Rows[selectionList[i]].Selected = true;
            }


            // 영역 밖으로 나간경우 스크롤

            if(direction < 0)
            {
                if(selectionList[0] < dgv.FirstDisplayedScrollingRowIndex)
                    dgv.FirstDisplayedScrollingRowIndex = selectionList[0];
            }
            else
            {
                int displayCount = dgv.DisplayedRowCount(false) - 1;

                if(selectionList[0] > dgv.FirstDisplayedScrollingRowIndex + displayCount)
                    dgv.FirstDisplayedScrollingRowIndex = selectionList[0] - displayCount;
            }

        }

        public void deleteSelectedRow(DataGridView dgv)
        {
            if(dgv.SelectedRows == null) return;
            if(dgv.SelectedRows.Count == 0) return;

            DataTable dt = dgv.DataSource as DataTable;

            for(int i = dgv.Rows.Count - 1; i >= 0; i--)
            {
                if(dgv.Rows[i].Selected)
                    dt.Rows[i].Delete();
            }

            dt.AcceptChanges();
            dgv.ClearSelection();
        }

        public float[] getGridHeaderWidth(DataGridView dgv)
        {
            int cnt = 0;
            int idx = 0;

            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    cnt++;
                }
            }

            float[] r = new float[cnt];

            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    if(dgv.Columns[i].AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                        r[idx] = dgv.Columns[i].FillWeight;
                    else
                        r[idx] = dgv.Columns[i].Width;

                    idx++;
                }
            }

            return r;
        }

        public void setGridHeaderWidth(DataGridView dgv, float[] wid)
        {
            if(wid.Length == 0) return;

            int cnt = 0;

            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    cnt++;
                }
            }

            if(cnt != wid.Length)
                return;

            int idx = 0;
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    if(dgv.Columns[i].AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                        dgv.Columns[i].FillWeight = wid[idx];
                    else
                        dgv.Columns[i].Width = (int)wid[idx];

                    idx++;
                }
            }
        }

        public string floatArrayToString(float[] f)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int i = 0; i < f.Length; i++)
            {
                r = r + c + f[i].ToString();
                c = ", ";
            }

            return r;
        }

        public float[] stringTofloatArray(string str)
        {
            string[] s = str.Split(',');
            float[] r = new float[s.Length];

            for(int i = 0; i < s.Length; i++)
            {
                if(float.TryParse(s[i], out float f))
                    r[i] = f;
            }

            return r;
        }

        private void etc_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if(dgv != null)
            {
                SQLITEINI xml = new SQLITEINI();

                string str = floatArrayToString(getGridHeaderWidth(dgv));
                xml.WriteValue((string)dgv.Tag + "_ColumnWidth", str);
            }
        }

        /// <summary>
        /// 컬럼헤더 AutoSize 모드를 fill 로 강제변경한다 
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="path"></param>
        private void autoSaveColumnWidth(DataGridView dgv, string path)
        {
            dgv.Tag = path;

            SQLITEINI xml = new SQLITEINI();
            string t = xml.readValue(path + "_ColumnWidth", "");

            if(t != "")
                setGridHeaderWidth(dgv, stringTofloatArray(t));

            dgv.ColumnWidthChanged += etc_ColumnWidthChanged;

        }

        private void etc_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if(dgv != null)
            {
                SQLITEINI xml = new SQLITEINI();

                string str = e.Row.Height.ToString();

                for(int i = 0; i < dgv.Rows.Count; i++)
                {
                    //Console.WriteLine(dgv.Rows[i].Height);
                    dgv.Rows[i].Height = e.Row.Height;
                }

                xml.WriteValue((string)dgv.Tag + "_RowHeight", str);
            }
        }

        private void autoSaveRowHeight(DataGridView dgv, string path)
        {
            dgv.Tag = path;

            SQLITEINI xml = new SQLITEINI();
            string t = xml.readValue(path + "_RowHeight", "");

            if(int.TryParse(t, out int i))
            {
                dgv.RowTemplate.Height = i;
            }

            dgv.RowHeightChanged += etc_RowHeightChanged;

        }

        private void etc_ColumnHeadersHeightChanged(object sender, EventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if(dgv != null)
            {
                SQLITEINI xml = new SQLITEINI();

                string str = dgv.ColumnHeadersHeight.ToString();

                xml.WriteValue((string)dgv.Tag + "_ColumnHeight", str);
            }
        }
        private void autoSaveColumnHeight(DataGridView dgv, string path)
        {
            dgv.Tag = path;

            SQLITEINI xml = new SQLITEINI();
            string t = xml.readValue(path + "_ColumnHeight", "");

            if(int.TryParse(t, out int i))
            {
                dgv.ColumnHeadersHeight = i;
            }

            dgv.ColumnHeadersHeightChanged += etc_ColumnHeadersHeightChanged;

        }

        bool IsTheSameCellValue(DataGridView dgv, int column, int row)
        {
            DataGridViewCell cell1 = dgv[column, row];
            DataGridViewCell cell2 = dgv[column, row - 1];

            if(cell1.Value == null || cell2.Value == null)
            {
                return false;
            }

            bool r = cell1.Value.ToString() == cell2.Value.ToString();
            return r;
        }

        private void etc_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            bool b = true;

            if (dgv.Columns[e.ColumnIndex].ToolTipText == "1")
            {
                if(dgv.RowCount > 1)
                    e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

                if(e.RowIndex < 1 || e.ColumnIndex < 0)
                {
                    if(e.RowIndex == -1)
                        e.AdvancedBorderStyle.Bottom = dgv.AdvancedCellBorderStyle.Bottom;

                    return;
                }

                int firstColumnIndex = getFirstVisibleColumn(dgv);

                {
                    //// 처음열과 선택열까지 비교하여 머지 진행
                    for (int i = firstColumnIndex; i <= e.ColumnIndex; i++)
                    {
                        bool s1 = IsTheSameCellValue(dgv, i, e.RowIndex);
                        if (!s1) { b = false; }
                    }

                    if (b)
                    {
                        e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
                    }
                    else
                    {
                        e.AdvancedBorderStyle.Top = dgv.AdvancedCellBorderStyle.Top;
                    }
                }
                {
                    //// 처음열과 선택열만 비교하여 중간열이 달라도 선택열이 머지됨
                    //bool s1 = IsTheSameCellValue(dgv, firstColumnIndex, e.RowIndex);
                    //bool s2 = IsTheSameCellValue(dgv, e.ColumnIndex, e.RowIndex);

                    //if(s1 && s2)
                    //{
                    //    e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
                    //}
                    //else
                    //{
                    //    e.AdvancedBorderStyle.Top = dgv.AdvancedCellBorderStyle.Top;
                    //}
                }

                if (e.RowIndex + 1 == dgv.RowCount)
                    e.AdvancedBorderStyle.Bottom = dgv.AdvancedCellBorderStyle.Bottom;
            }
        }

        private void etc_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if(e.RowIndex == 0)
                return;

            DataGridView dgv = (DataGridView)sender;

            bool b = true;

            if(dgv != null)
            {
                if(dgv.Columns[e.ColumnIndex].ToolTipText == "1")
                {
                    int firstColumnIndex = getFirstVisibleColumn(dgv);

                    {
                        //// 처음열과 선택열까지 비교하여 머지 진행
                        for (int i = firstColumnIndex; i <= e.ColumnIndex; i++)
                        {
                            bool s1 = IsTheSameCellValue(dgv, i, e.RowIndex);
                            if (!s1) { b = false; }
                        }

                        if (b)
                        {
                            e.Value = "";
                            e.FormattingApplied = true;
                        }
                    }
                    {
                        //// 처음열과 선택열만 비교하여 중간열이 달라도 선택열이 머지됨
                        //bool s1 = IsTheSameCellValue(dgv, firstColumnIndex, e.RowIndex);
                        //bool s2 = IsTheSameCellValue(dgv, e.ColumnIndex, e.RowIndex);

                        //if(s1 && s2)
                        //{
                        //    e.Value = "";
                        //    e.FormattingApplied = true;
                        //}
                    }
                }
            }
        }

        public int getFirstVisibleColumn(DataGridView dgv)
        {
            for(int i = 0; i < dgv.ColumnCount; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    return i;
                }
            }

            return 0;
        }

        private int findFirstRowIndex(DataGridView dgv, int index)
        {
            int r = 0;

            for(int i = index; i >= 0; i--)
            {
                if(dgv.Rows[i].Cells[getFirstVisibleColumn(dgv)].FormattedValue.ToString() != "")
                    return i;
            }

            return r;
        }

        private int findLastRowIndex(DataGridView dgv, int index)
        {
            int r = dgv.RowCount;

            for(int i = index; i < dgv.RowCount; i++)
            {
                if(dgv.Rows[i].Cells[getFirstVisibleColumn(dgv)].FormattedValue.ToString() != "")
                    return i;
            }

            return r;
        }

        private void etc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if(dgv == null) return;

            //dgv.ClearSelection();

            int fRow = findFirstRowIndex(dgv, e.RowIndex);
            int lRow = findLastRowIndex(dgv, e.RowIndex + 1);

            for(int rIndex = fRow; rIndex < lRow; rIndex++)
            {
                for(int cIndex = 0; cIndex < dgv.ColumnCount; cIndex++)
                {
                    if(dgv.Rows[rIndex].Cells[cIndex].Visible)
                    {
                        dgv.Rows[rIndex].Cells[cIndex].Selected = true;

                        Application.DoEvents();
                    }
                }
            }

        }

        /// <summary>
        /// dataGridViewcellMerge(dgvRecipe, "No, 호량, 염조제명");
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="columnHeaderText"></param>
        public void dataGridViewcellMerge(DataGridView dgv, string columnHeaderText)
        {
            string [] ar = columnHeaderText.Split(',');

            ar = ar.Select(x => x.Trim()).ToArray();

            for(int i = 0; i < dgv.ColumnCount; i++)
            {
                if(Array.IndexOf(ar, dgv.Columns[i].HeaderText) >= 0)
                {
                    dgv.Columns[i].ToolTipText = "1";
                }
            }

            dgv.CellFormatting -= etc_CellFormatting;
            dgv.CellFormatting += etc_CellFormatting;

            dgv.CellPainting -= etc_CellPainting;
            dgv.CellPainting += etc_CellPainting;

            //dgv.CellClick += etc_CellClick; 
        }


        private void etc_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if(dgv != null)
            {
                if(dgv.RowTemplate.Resizable == DataGridViewTriState.True)
                {
                    dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                    dgv.RowTemplate.Resizable = DataGridViewTriState.False;
                    dgv.AllowUserToResizeColumns = false;

                    for(int i = 0; i < dgv.RowCount; i++)
                        dgv.Rows[0].Resizable = DataGridViewTriState.False;
                }
                else
                {
                    dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                    dgv.RowTemplate.Resizable = DataGridViewTriState.True;
                    dgv.AllowUserToResizeColumns = true;

                    for(int i = 0; i < dgv.RowCount; i++)
                        dgv.Rows[0].Resizable = DataGridViewTriState.True;
                }

            }
        }


        private void etc_CheckBoxAutoSizeCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridView dgv = (DataGridView)sender;
                if (dgv.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    int wid = e.CellBounds.Width / 2;
                    int hei = e.CellBounds.Height / 2;


                    e.PaintBackground(e.CellBounds, true);
                    ControlPaint.DrawCheckBox(e.Graphics, e.CellBounds.X + (wid / 2), e.CellBounds.Y + (hei / 2), wid, hei,
                        (bool)e.FormattedValue ? ButtonState.Checked : ButtonState.Normal);
                    e.Handled = true;
                }
            }
        }

        public void dataGridViewCheckBoxAutoSize(DataGridView dgv)
        {
            dgv.CellPainting += etc_CheckBoxAutoSizeCellPainting;
        }

        public void dataGridViewSizeChangeManager(DataGridView dgv, string saveName)
        {
            autoSaveColumnWidth(dgv, saveName);
            autoSaveRowHeight(dgv, saveName);
            autoSaveColumnHeight(dgv, saveName);

            dgv.ColumnHeaderMouseDoubleClick += etc_ColumnHeaderMouseDoubleClick;
        }

        //-------------------------------------------------------------------------


        public string getDataTableColumnData(DataTable dt, int index, string column)
        {
            if(dt == null || index == -1)
                return string.Empty;

            return toStrDef(dt.Rows[index][column], string.Empty);
        }

        // 그리드뷰의 선택된 순서를 가져온다 - 보이는 순서대로 - 컬럼 정렬했을때 이 순서로 datatable에 접근하면 엉뚱한것 건드린다
        public int getGridSelectedDataGridIndex(object sender)  
        {
            DataGridView dgv = (DataGridView) sender;

            if(dgv != null && dgv.SelectedRows != null && dgv.SelectedRows.Count == 1)
                return dgv.SelectedRows[0].Index;

            return -1;
        }

        // 그리드뷰에서 선택한 항목의 datatable의 순서를 가져온다 - 정렬했을때도 작동, 화면에 보이는 순서가 아니다
        public int getGridSelectedDataTableIndex(object sender)
        {
            DataGridView dgv = (DataGridView)sender;

            DataTable dt = (DataTable)dgv.DataSource;

            if (dt != null)
            {
                if (dgv.CurrentCell != null)
                {
                    BindingManagerBase bindingManagerBase = dgv.BindingContext[dgv.DataSource, dgv.DataMember];

                    DataRow dataRow = ((DataRowView)bindingManagerBase.Current).Row;

                    if (dataRow != null)
                    {
                        return dt.Rows.IndexOf(dataRow);
                    }
                }
            }

            return -1;
        }

        private List<int> getDataGridViewSelectedDataTableIndex(DataGridView dgv)
        {
            List<int> r = new List<int>();

            DataTable dt = (DataTable)dgv.DataSource;

            if (dt != null)
            {
                if (dgv.CurrentCell != null)
                {
                    foreach (DataGridViewRow row in dgv.SelectedRows)
                    {
                        DataRowView dataRowView = row.DataBoundItem as DataRowView;
                        if (dataRowView != null)
                        {
                            int index = dt.Rows.IndexOf(dataRowView.Row);

                            r.Add(index);
                        }
                    }
                }
            }

            return r;
        }

        public string getGridSelectedColumnData(object sender, string column)
        {
            //if(sender == null)
            //    return string.Empty;

            //DataGridView dgv = (DataGridView) sender;

            //int index = getGridSelectedRowIndex(sender);

            //return getDataTableColumnData((DataTable)dgv.DataSource, index, column);


            if (sender == null)
                return string.Empty;

            DataGridView dgv = (DataGridView)sender;

            int index = getGridSelectedDataTableIndex(sender);

            return getDataTableColumnData((DataTable)dgv.DataSource, index, column);
        }

        public void setGridSelectedColumnData(object sender, string column, string data)
        {
            if (sender == null) return;

            DataGridView dgv = (DataGridView)sender;

            DataTable dt = (DataTable)dgv.DataSource;

            if (dt == null) return;

            int index = getGridSelectedDataTableIndex(sender);

            if (index < 0 || index >= dt.Rows.Count) return;

            dt.Rows[index][column] = data;            
        }

        public int getDgvColumnIndex(DataGridView dgv, string columnName)
        {
            for(int i = 0; i < dgv.ColumnCount; i++)
            {
                if(dgv.Columns[i].HeaderText == columnName)
                {
                    return i;
                }
            }

            return -1;
        }

        public void ScrollDataGridView(DataGridView dgv, int rowIndex)
        {
            if(dgv.RowCount == 0) { return; }
            rowIndex = Math.Min(Math.Max(0, rowIndex), dgv.RowCount - 1);

            dgv.FirstDisplayedScrollingRowIndex = rowIndex;
        }

        public void dgvPageUp(DataGridView dgv)
        {
            int nextPageRowIndex = dgv.FirstDisplayedScrollingRowIndex - dgv.DisplayedRowCount(false);

            ScrollDataGridView(dgv, nextPageRowIndex);
        }

        public void dgvPageDown(DataGridView dgv)
        {
            int nextPageRowIndex = dgv.FirstDisplayedScrollingRowIndex + dgv.DisplayedRowCount(false);

            ScrollDataGridView(dgv, nextPageRowIndex);
        }

        public int dgvLocationByHeaterText(DataGridView dgv, string headerText, string value)
        {
            int columnIndex = getDgvColumnIndex(dgv, headerText);

            if(columnIndex >= 0)
            {
                for(int i = 0; i < dgv.RowCount; i++)
                {
                    if(dgv.Rows[i].Cells[columnIndex].Value.ToString() == value)
                    {
                        dgv.ClearSelection();

                        dgv.Rows[i].Selected = true;

                        ScrollDataGridView(dgv, i);

                        return i;
                    }
                }
            }

            return -1;
        }

        public int dgvGetColumnIndexByBindingName(DataGridView dgv, string name)
        {
            for(int i = 0; i < dgv.ColumnCount; i++)
            {
                if(dgv.Columns[i].DataPropertyName == name)
                    return i;
            }

            return -1;
        }


        public int dgvLocationByBindingColumnName(DataGridView dgv, string columnName, string value)
        {
            int columnIndex = dgvGetColumnIndexByBindingName(dgv, columnName);

            if(columnIndex >= 0)
            {
                for(int i = 0; i < dgv.RowCount; i++)
                {
                    if(dgv.Rows[i].Cells[columnIndex].Value.ToString() == value)
                    {
                        dgv.ClearSelection();

                        dgv.Rows[i].Selected = true;

                        ScrollDataGridView(dgv, i);

                        return i;
                    }
                }
            }

            return -1;
        }

        public int dataTableLocation(DataTable dt, string column, string data)
        {
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                if(dt.Rows[i][column].ToString() == data)
                    return i;
            }

            return -1;
        }

        public int dataTableLocation(DataTable dt, int column, string data)
        {
            for(int i = 0; i < dt.Rows.Count; i++)
            {
                if(dt.Rows[i][column].ToString() == data)
                    return i;
            }

            return -1;
        }

        public DialogResult showDialog(Form f)
        {
            DialogResult dr = f.ShowDialog();

            f.Dispose();

            return dr;
        }

        public void contolsSetWindowScroll(Form f, Control c)
        {
            scrollMainForm = f;
            c.MouseDown += (o, e) => { if(e.Button == MouseButtons.Left) { scrollOn = true; scrollPos = e.Location; } };
            c.MouseMove += (o, e) => { if(scrollOn) scrollMainForm.Location = new Point(scrollMainForm.Location.X + (e.X - scrollPos.X), scrollMainForm.Location.Y + (e.Y - scrollPos.Y)); };
            c.MouseUp += (o, e) => { if(e.Button == MouseButtons.Left) { scrollOn = false; scrollPos = e.Location; } };
        }

        public string copyListBoxtText(ListBox list)
        {
            string t = string.Empty;

            for(int i = 0; i < list.Items.Count; i++)
            {
                t = t + list.Items[i].ToString() + Environment.NewLine;
            }

            return t;
        }

        private void getControlList(Control.ControlCollection conColl, List<Control> r)
        {
            try
            {
                foreach(Control con in conColl)
                {
                    if(con.Controls.Count > 0)
                    {
                        getControlList(con.Controls, r);
                    }
                    else
                    {
                        r.Add(con);
                    }
                }
            }
            catch//(Exception ex)
            {
                //WriteLog("Main : ReSetControl() Error =[" + ex.ToString() + "]");
                //LogListAdd(ex.Message);
            }
        }

        public List<Control> getControlList(Control.ControlCollection conColl)
        {
            List<Control> r = new List<Control>();

            getControlList(conColl, r);

            return r;
        }

        /// <summary>
        /// 지정된 폼과 컨트롤 이름으로 데이터 테이블의 언어를 설정한다 -> 태그 안씀
        /// 폼 이름과 컨트롤 이름은 대문자로 한다
        /// 필요하면 언어는 더 추가한다
        /// 
        /// FORM     |CONTROL     |KOR       |ENG       |
        /// ---------+------------+----------+----------+
        /// MAINFORM | BTNNEWWORK | 신규작업 | NEW WORK |
        /// 
        /// </summary>
        /// <param name="formName"></param>
        /// <param name="r"></param>
        /// <param name="dt"></param>
        /// <param name="lang"></param>
        public void setControlMultiText(string formName, List<Control> r, DataTable dt, string lang)
        {
            if(dt.Rows.Count <= 0)
                return;

            if(lang == string.Empty)
                return;

            formName = formName.ToUpper();

            for(int i = 0; i < r.Count; i++)
            {
                try
                {
                    DataRow[] dr = dt.Select( "FORM = " + QuotedString(formName) + " AND CONTROL = " + QuotedString(r[i].Name.ToUpper()) );

                    if(dr.Length == 1)
                    {
                        string title = toStrDef(dr[0][lang]);

                        if(title != "")
                            r[i].Text = title;
                    }
                }
                catch//(Exception ex)
                {
                    //WriteLog("Main : ReSetControl() Error =[" + ex.ToString() + "]");
                    //LogListAdd(ex.Message);
                }
            }
        }

        public DataTable makeControlsTextToDataTable(string formName, Control.ControlCollection conColl)
        {
            List<Control> r = new List<Control>();
            DataTable dt = createTable( "FORM, CONTROL, KOR");

            getControlList(conColl, r);

            for(int i = 0; i < r.Count; i++)
            {
                try
                {
                    if(r[i].Text.Length > 0 && r[i].Name.Length > 0)
                    {
                        DataRow dr = dt.NewRow();

                        dr["FORM"] = formName;
                        dr["CONTROL"] = r[i].Name.ToUpper();
                        dr["KOR"] = r[i].Text;

                        dt.Rows.Add(dr);
                    }
                }
                catch//(Exception ex)
                {
                    //log.WriteLog("except except : " + ex.ToString()); // 자세한 정보
                    //MessageBox.Show(ex.Message);    // 간략한 정보
                }
                finally
                {
                }
            }

            return dt;
        }

        public string comma(int t)
        {
            return string.Format("{0:N0}", t);
        }

        public void stringToFile(string path, string text)
        {
            try
            {
                StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
                sw.Write(text + sw.NewLine);
                sw.Close();
            }
            catch
            {
            }
        }

        public void dataTableToCsv(DataTable dtDataTable, string path, char sep = ',')
        {
            try
            {
                StreamWriter sw = new StreamWriter(path, false, Encoding.Default);
                //headers    
                for(int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    string col = dtDataTable.Columns[i].ColumnName;

                    if (col.IndexOf(',') >= 0)
                        col = "\"" + col + "\"";

                    sw.Write(col);
                    if(i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(sep);
                    }
                }
                sw.Write(sw.NewLine);
                foreach(DataRow dr in dtDataTable.Rows)
                {
                    for(int i = 0; i < dtDataTable.Columns.Count; i++)
                    {
                        if(!Convert.IsDBNull(dr[i]))
                        {
                            string item = dr[i].ToString();

                            if (item.IndexOf(',') >= 0)
                                item = "\"" + item + "\"";

                            sw.Write(item);
                        }
                        if(i < dtDataTable.Columns.Count - 1)
                        {
                            sw.Write(sep);
                        }
                    }
                    sw.Write(sw.NewLine);
                }
                sw.Close();

            }
            catch
            {
            }
        }

        private List<string> splitCsvLine(string line, char sep)
        {
            List<string> r = new List<string>();

            while (line.Length > 0)
            {
                int c = line.IndexOf(sep);
                int d = line.IndexOf('\"');

                if (d == 0)
                {
                    line = line.Substring(1);

                    d = line.IndexOf('\"');

                    string t = line.Substring(0, d);

                    r.Add(t);

                    line = line.Substring(d + 2);
                }
                else if (c < d)
                {
                    string t = line.Substring(0, d - 1);

                    string[] strings = t.Split(sep);

                    r.AddRange(strings);

                    line = line.Substring(d + 1);

                    d = line.IndexOf('\"');

                    t = line.Substring(0, d);

                    r.Add(t);

                    line = line.Substring(d + 2);

                }
                else
                {
                    string[] strings = line.Split(sep);

                    r.AddRange(strings);

                    line = string.Empty;
                }

            }

            return r;
        }

        public DataTable dataTableFromCsv(string path, char sep = ',')
        {
            string[] fileContents = File.ReadAllLines(path, Encoding.Default);

            string[] columns = splitCsvLine(fileContents[0], sep).ToArray();

            DataTable dtR = createTable(columns);

            for (int i = 1; i < fileContents.Length; i++)
            {
                string[] temp = splitCsvLine(fileContents[i], sep).ToArray();

                DataRow row = dtR.NewRow();

                for (int k = 0; k < columns.Length; k++)
                    row[k] = temp[k];

                dtR.Rows.Add(row);
            }

            return dtR;
        }

        // 셋업
        public void lookupComboSetup(ComboBox cb, DataTable dt, string valueMember, string displayMember, string renameValueMember = "", string renameDisplayMember = "")
        {
            try
            {
                dt = dt.Copy();

                lookupBackupDisplayMember(dt, displayMember);

                if(renameValueMember.Length > 0)
                {
                    dt.Columns[valueMember].ColumnName = renameValueMember;
                    valueMember = renameValueMember;
                }

                if(renameDisplayMember.Length > 0)
                {
                    dt.Columns[displayMember].ColumnName = renameDisplayMember;
                    displayMember = renameDisplayMember;
                }

                cb.DataSource = dt;
                cb.DisplayMember = displayMember;
                cb.ValueMember = valueMember;
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                cb.SelectedIndex = -1;
            }
        }

        public string lookupDisplayString(ComboBox cb)
        {
            return cb.Text;
        }

        public bool lookupisExistBackupDisplayMember(ComboBox cb)
        {
            DataTable dt = (DataTable)cb.DataSource;

            if(dt.Columns.IndexOf(BACKUPED_DISPLAY_MEMBER) >= 0)
                return true;

            return false;
        }

        public string lookupGetBackupedDisplayMember(ComboBox cb)
        {
            string r = string.Empty;

            DataTable dt = (DataTable)cb.DataSource;

            if(dt != null && dt.Rows.Count > 0)
            {
                int index = cb.SelectedIndex;

                if(index >= 0 && index < dt.Rows.Count)
                {
                    r = toStrDef(dt.Rows[index][BACKUPED_DISPLAY_MEMBER]);
                }
            }

            return r;
        }

        // 선택된 항목의 value 값 가져오는것
        public string lookupValueSelected(ComboBox cb)
        {
            try
            {
                return lookupValueByIndex(cb, cb.SelectedIndex);
            }
            catch(Exception)
            {
                throw;
            }
        }

        // 주어진 인덱스의 value 가져오는것
        public string lookupValueByIndex(ComboBox cb, int index)
        {
            try
            {
                DataTable dt = (DataTable)cb.DataSource;

                if(dt == null) return "";

                if(index < 0 || index >= dt.Rows.Count) return "";

                if(cb.ValueMember != "")
                    return toStrDef(dt.Rows[index][cb.ValueMember]);
            }
            catch(Exception)
            {
                throw;
            }

            return "";
        }

        // value로 인덱스 찾는것
        public int lookupIndexByValue(ComboBox cb, string value)
        {
            try
            {
                DataTable dt = (DataTable)cb.DataSource;

                if(dt == null) return -1;

                for(int i = 0; i < dt.Rows.Count; i++)
                {
                    if(dt.Rows[i][cb.ValueMember].ToString() == value)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch(Exception)
            {
                throw;
            }
        }

        // value로 항목 선택
        public void lookupSelectByValue(ComboBox cb, string value)
        {
            try
            {
                int index = lookupIndexByValue(cb, value);

                cb.SelectedIndex = index;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public DataTable dataRowsToTable(DataRow[] drs)
        {
            DataTable dt = new DataTable();

            if(drs.Length > 0)
                dt = drs.CopyToDataTable();

            return dt;
        }

        public DataTable dataTableAppend(DataTable dtdst, DataTable dtsrc)
        {
            DataTable r = new DataTable();

            if(dtdst != null && dtdst.Columns.Count > 0 && dtsrc != null && dtsrc.Columns.Count > 0)
            {
                r = dtdst.Copy();

                dtsrc = dtsrc.Copy();

                for(int i = 0; i < dtsrc.Rows.Count; i++)
                {
                    r.ImportRow(dtsrc.Rows[i]);
                }
            }
            else
            {
                if(dtsrc == null || dtsrc.Columns.Count == 0)
                {
                    if(dtdst != null)
                        r = dtdst.Copy();
                }
                else
                {
                    if(dtsrc != null)
                        r = dtsrc.Copy();
                }
            }

            return r;
        }

        private object[] columnToArrayFromPivot(DataTable dt, string columnName, int columnIndex)
        {
            object[] r = new object[dt.Rows.Count + 1];

            r[0] = columnName;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                r[i + 1] = dt.Rows[i][columnIndex].ToString();
            }

            return r;
        }

        public DataTable pivotDataTable(DataTable dtSrc)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int i = 0; i < dtSrc.Rows.Count; i++)
            {
                string t = "DATA_" + i.ToString("00");

                r = r + c + t;
                c = ", ";
            }

            DataTable dtDst = createTable("column," + r);

            for(int i = 0; i < dtSrc.Columns.Count; i++)
            {
                object[] array = columnToArrayFromPivot(dtSrc, dtSrc.Columns[i].ColumnName, i);

                dtDst.Rows.Add(array);
            }

            return dtDst;
        }

        private object[] columnToArrayFromUnPivot(DataTable dt, int columnIndex)
        {
            object[] r = new object[dt.Rows.Count];

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                r[i] = dt.Rows[i][columnIndex].ToString();
            }

            return r;
        }

        public DataTable unpivotDataTable(DataTable dtSrc)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int i = 0; i < dtSrc.Rows.Count; i++)
            {
                string t = toStrDef(dtSrc.Rows[i][0]);

                r = r + c + t;
                c = ", ";
            }

            DataTable dtDst = createTable(r);

            for(int i = 1; i < dtSrc.Columns.Count; i++)
            {
                object[] array = columnToArrayFromUnPivot(dtSrc, i);

                dtDst.Rows.Add(array);
            }

            return dtDst;
        }

        public DataTable dataTableTypeConvert(DataTable dtSrc, int columnIndex, Type t)
        {
            DataTable dtDst = dtSrc.Clone();

            dtDst.Columns[columnIndex].DataType = t;

            for(int i = 0; i < dtSrc.Rows.Count; i++)
            {
                dtDst.Rows.Add(dtDst.NewRow());

                for(int c = 0; c < dtSrc.Columns.Count; c++)
                {
                    //if(c != columnIndex)
                    //    dtDst.Rows[i][c] = dtSrc.Rows[i][c];
                    //else
                    //    dtDst.Rows[i][c] = (t)dtSrc.Rows[i][c];

                    dtDst.Rows[i][c] = toDoubleDef(dtSrc.Rows[i][c]);
                }
            }

            return dtDst;
        }


        public string listBoxToString(ListBox box)
        {
            string r = string.Empty;

            foreach(string i in box.Items)
            {
                r = r + Environment.NewLine + i.Trim();
            }

            return r;
        }

        // -------------------------------------------------------------------------------

        List<List<DataGridView>> dgvSyncList = new List<List<DataGridView>>();

        private void dgvSyncFunction(object sender, ScrollEventArgs e)
        {
            List<DataGridView> dgvList = null;
            DataGridView dgvFind = (DataGridView)sender;

            for(int i = 0; i < dgvSyncList.Count(); i++)
            {
                List<DataGridView> t = dgvSyncList[i].Where(x => x == dgvFind).ToList();
                if(t.Count() > 0)
                {
                    dgvList = dgvSyncList[i];

                    {
                        foreach(DataGridView item in dgvList)
                        {
                            if(item != dgvFind)
                            {
                                if(e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                                {
                                    item.HorizontalScrollingOffset = dgvFind.HorizontalScrollingOffset;
                                }
                                else
                                {
                                    item.FirstDisplayedScrollingRowIndex = dgvFind.FirstDisplayedScrollingRowIndex;
                                }
                            }
                        }
                    }

                    break;
                }
            }

        }

        public void syncDgvScroll(DataGridView[] dgvList)
        {
            dgvSyncList.Add(dgvList.ToList());

            for(int i = 0; i < dgvList.Count(); i++)
            {
                dgvList[i].Scroll += dgvSyncFunction;
            }

        }

        public void makeSeqNoByKeyField(DataTable dt, string columnSeq, string keyColumn1, string keyColumn2 = "", string keyColumn3 = "")
        {
            if(dt == null || dt.Rows.Count <= 0) return;

            string key = "0sd9fj214-1`1d";
            int seq = 1;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                string tkey = dt.Rows[i][keyColumn1].ToString();

                if(keyColumn2 != "")
                    tkey += dt.Rows[i][keyColumn2].ToString();

                if(keyColumn3 != "")
                    tkey += dt.Rows[i][keyColumn3].ToString();

                if(key != tkey)
                {
                    seq = 1;
                    key = tkey;
                }

                dt.Rows[i][columnSeq] = seq.ToString();

                seq++;
            }
        }

        public void makeDiffIncSeqNoByKeyField(DataTable dt, string columnSeq, string keyColumn1, string keyColumn2 = "", string keyColumn3 = "")
        {
            if (dt == null || dt.Rows.Count <= 0) return;

            string key = "0sd9fj214-1`1d";
            int seq = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string tkey = dt.Rows[i][keyColumn1].ToString();

                if (keyColumn2 != "")
                    tkey += dt.Rows[i][keyColumn2].ToString();

                if (keyColumn3 != "")
                    tkey += dt.Rows[i][keyColumn3].ToString();

                if (key != tkey)
                {
                    seq++;
                    key = tkey;
                }

                dt.Rows[i][columnSeq] = seq.ToString();
            }
        }

        public void makeSeqNoByKeyField(DataGridView dgv, string columnSeq, string columnKey1, string keyColumn2 = "")
        {
            DataTable dt = (DataTable)dgv.DataSource;

            makeSeqNoByKeyField(dt, columnSeq, columnKey1, keyColumn2);
        }

        public void makeSeqNoWithoutKeyField(DataTable dt, string columnSeq)
        {
            if(dt == null || dt.Rows.Count <= 0) return;

            if(dt.Columns.IndexOf(columnSeq) == -1) return;

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][columnSeq] = (i + 1).ToString();
            }
        }

        public void makeSeqNoWithoutKeyField(DataGridView dgv, string columnSeq)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            makeSeqNoWithoutKeyField(dt, columnSeq);
        }

        public void columnMerge(DataTable dt, string columnTarge, string columnData, string sepString, bool mergeDataRear = false)
        {
            lookupBackupDisplayMember(dt, columnTarge);

            for(int i = 0; i < dt.Rows.Count; i++)
            {
                if(mergeDataRear)
                    dt.Rows[i][columnTarge] = dt.Rows[i][columnTarge].ToString() + sepString + dt.Rows[i][columnData].ToString();
                else
                    dt.Rows[i][columnTarge] = dt.Rows[i][columnData].ToString() + sepString + dt.Rows[i][columnTarge].ToString();
            }
        }

        public List<DataTable> dataTableDivide(DataTable dt, int count = 15)
        {
            List<DataTable> r = new List<DataTable>();

            int max = (int)(Math.Ceiling((double)dt.Rows.Count / 15));

            for(int i = 0; i < max; i++)
            {
                r.Add(dt.AsEnumerable().Skip(i * count).Take(count).CopyToDataTable());
            }

            return r;
        }

        public void moveFileToTrash(string file)
        {
            if(File.Exists(file))
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    file,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

            }
        }

        public bool ping(string ip)
        {
            try
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();

                options.DontFragment = true;

                string data = "Test";
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                PingReply reply = pingSender.Send(ip, 100, buffer, options);

                return (reply.Status == IPStatus.Success);
            }
            catch
            {

            }

            return false;
        }




    }
}
