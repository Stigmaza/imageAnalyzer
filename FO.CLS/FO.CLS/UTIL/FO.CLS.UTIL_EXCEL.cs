using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;
using TextBox = System.Windows.Forms.TextBox;

namespace FO.CLS.UTIL
{
    public partial class FOEXCEL: Form
    {
        public FOEXCEL()
        {
            InitializeComponent();
        }


        // 엑셀  Object 해제
        private void ReleaseObject(Object obj)
        {
            try
            {
                if(obj != null)
                {
                    // 액셀 객체 해제
                    Marshal.ReleaseComObject(obj);

                    obj = null;
                }
            }
            catch(Exception ex)
            {
                obj = null;
                throw ex;
            }
            finally
            {
                // 가비지 수집
                GC.Collect();
            }
        }


        [DllImport("User32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);
        private static void KillExcel(Excel.Application theApp)
        {
            try
            {
                int id = 0;

                IntPtr intptr = new IntPtr(theApp.Hwnd);
                System.Diagnostics.Process p = null;

                GetWindowThreadProcessId(intptr, out id);
                p = System.Diagnostics.Process.GetProcessById(id);
                if(p != null)
                {
                    p.Kill();
                    p.Dispose();
                }
            }
            catch//(Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show("KillExcel:" + ex.Message);
            }
        }

        public TextBox[] getAllTextBoxFromPanel(Panel pnl)
        {
            IOrderedEnumerable<Control> controlsInLabel =  from n in pnl.Controls.Cast<Control>()
                                                           where n is TextBox
                                                           orderby n.Name
                                                           select n;

            return Array.ConvertAll<Control, TextBox>(controlsInLabel.ToArray(), item => (TextBox)item);
        }
        private int countVisibleColumn(DataGridView dgv)
        {
            int columnCount = 0;

            // 보여지는 컬럼 갯수 세기
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                    columnCount++;
            }

            return columnCount;
        }
        public string[] makeColumnHeaderToArray(DataGridView dgv)
        {
            string r = string.Empty;
            string comma = string.Empty;

            // 보여지는 갯수만큼 배열 할당
            int[] columnArray = new int[countVisibleColumn(dgv)];

            // 디스플레이 인덱스에 맞춰 컬럼 번호 설정
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    //columnArray[i] = dgv.Columns[i].DisplayIndex;

                    r = r + comma + dgv.Columns[i].HeaderText;
                    comma = ",";
                }
            }

            // 컬럼헤더 문자열로 조합
            //for(int i = 0; i < columnArray.Length; i++)
            //{
            //    r = r + comma + dgv.Columns[columnArray[i]].HeaderText;

            //    comma = ",";
            //}

            return r.Split(',');
        }

        public string[] makeColumnBindPropertyToArray(DataGridView dgv)
        {
            string r = string.Empty;
            string comma = string.Empty;

            // 보여지는 갯수만큼 배열 할당
            int[] columnArray = new int[countVisibleColumn(dgv)];

            // 디스플레이 인덱스에 맞춰 컬럼 번호 설정
            for(int i = 0; i < dgv.Columns.Count; i++)
            {
                if(dgv.Columns[i].Visible)
                {
                    //columnArray[i] = dgv.Columns[i].DisplayIndex;

                    r = r + comma + dgv.Columns[i].DataPropertyName;

                    comma = ",";
                }
            }

            //for(int i = 0; i < columnArray.Length; i++)
            //{
            //    r = r + comma + dgv.Columns[columnArray[i]].DataPropertyName;

            //    comma = ",";
            //}

            return r.Split(',');
        }

        public void export(string title, DataGridView dgv, string pathToSave = "")
        {
            lblExcelMsg.Text = title;
            lblExcelCnt.Text = "- / -";

            CenterToParent();
            this.Show();

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                FOETC etc = new FOETC();

                string[] columnHeader = makeColumnHeaderToArray(dgv);
                string[] columnBinding = makeColumnBindPropertyToArray(dgv);
                DataTable dt = dgv.DataSource as DataTable;

                string folderPath = @Application.StartupPath + "/BackUp/";
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                string fullPath = folderPath + "\\" + fileName; //Path.Combine(_sSavePath, _sFileName);


                if(pathToSave.Length > 0)
                {
                    folderPath = Path.GetDirectoryName(pathToSave);
                    fullPath = pathToSave;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                if(!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                // --------------------------------------------------------------

                // 워크북 추가
                workbook = excelApp.Workbooks.Add(1);

                // 엑셀 첫번재 워크시트 가져오기
                worksheet = workbook.Worksheets.get_Item(1) as Excel.Worksheet;

                // 컬럼 이름 쓰기
                for(int i = 0; i < columnHeader.Length; i++)
                {
                    worksheet.Cells[1, i + 1] = columnHeader[i];
                    worksheet.Cells[1, i + 1].Font.Size = 10;
                    worksheet.Cells[1, i + 1].Borders.Weight = Excel.XlBorderWeight.xlThin;
                }

                // 데이터 쓰기
                for(int j = 0; j < dt.Rows.Count; j++)
                {
                    for(int i = 0; i < columnBinding.Length; i++)
                    {
                        string temp = etc.toStrDef(dt.Rows[j][columnBinding[i]], " ");

                        worksheet.Cells[j + 2, i + 1] = temp;
                        worksheet.Cells[j + 2, i + 1].Font.Size = 10;
                        worksheet.Cells[j + 2, i + 1].Borders.Weight = Excel.XlBorderWeight.xlThin;
                    }

                    lblExcelCnt.Text = (j + 1).ToString() + " / " + dt.Rows.Count.ToString();
                    Application.DoEvents();
                }

                worksheet.Columns.AutoFit(); // 열 너비 자동 맞춤

                workbook.SaveAs(fullPath); // 엑셀 파일 저장
                workbook.Close(true);
                excelApp.Quit();

                //log.WriteLog("AuxInfo Excel Export OK - " + _sFilePath + "\n Row Count - " + listView1.Items.Count);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                //ReleaseObject(worksheet);
                //ReleaseObject(workbook);
                //ReleaseObject(excelApp);

                KillExcel(excelApp);

                this.Close();
            }
        }

        public string openDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Application.StartupPath;
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Excel files (*.xls,*.xlsx)|*.xls;*.xlsx;|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if(openFileDialog.ShowDialog() == DialogResult.OK)
                return openFileDialog.FileName;

            return string.Empty;
        }

        public void import(string title, DataGridView dgv, string sheet = "Sheet1")
        {
            string filePath = openDialog();

            if(filePath == string.Empty)
                return;

            lblExcelMsg.Text = title;
            lblExcelCnt.Text = "- / -";

            CenterToParent();
            this.Show();

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                //string[] columns = makeColumnHeaderToArray(dgv);
                string[] binding = makeColumnBindPropertyToArray(dgv);

                DataTable dt = dgv.DataSource as DataTable;

                if(dt != null)
                    dt.Clear();

                workbook = excelApp.Workbooks.Open(Filename: filePath);
                worksheet = workbook.Worksheets.get_Item(sheet);

                Excel.Range range = worksheet.UsedRange;

                // 헤더 제외하고 처리
                for(int i = 2; i <= range.Rows.Count; i++)
                {
                    //if(range.Cells[i, 2].Value2 != null)
                    {
                        DataRow dataRow = dt.NewRow();

                        for(int k = 0; k < binding.Length; k++)
                        {
                            if(range.Cells[i, 1 + k].Value2 != null)
                                dataRow[binding[k]] = range.Cells[i, 1 + k].Value2.ToString();
                        }

                        dt.Rows.Add(dataRow);
                    }

                    lblExcelCnt.Text = (i - 1).ToString() + " / " + (range.Rows.Count - 1).ToString();
                    Application.DoEvents();
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                //ReleaseObject(worksheet);
                //ReleaseObject(workbook);
                //ReleaseObject(excelApp);

                KillExcel(excelApp);

                this.Close();
            }
        }

        private string rowToString(Excel.Range range, int index)
        {
            string r = string.Empty;
            string c = string.Empty;

            for(int x = 0; x <= 200; x++)
            {
                if(range.Cells[index, 1 + x].Value2 == null)
                    break;


                string t = range.Cells[index, 1 + x].Value2.ToString();

                r = r + c + t;
                c = ",";
            }

            return r;
        }

        public DataTable import(string filePath, string sheet = "Sheet1")
        {
            DataTable dt = new DataTable();

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                workbook = excelApp.Workbooks.Open(Filename: filePath);
                //worksheet = workbook.Worksheets.get_Item(sheet);
                worksheet = workbook.Worksheets[1] as Microsoft.Office.Interop.Excel.Worksheet;

                Excel.Range range = worksheet.UsedRange;



                //string columns = rowToString(range,1);
                //string[] binding = columns.Split(',');

                //DataTable tempT = new DataTable();

                //for(int i = 0; i < binding.Length; i++)
                //{
                //    tempT.Columns.Add(binding[i]);
                //}

                //// 헤더 제외하고 처리
                //for(int i = 2; i <= range.Rows.Count; i++)
                //{
                //    //if(range.Cells[i, 2].Value2 != null)
                //    {
                //        DataRow dataRow = tempT.NewRow();

                //        for(int k = 0; k < binding.Length; k++)
                //        {
                //            if(range.Cells[i, 1 + k].Value2 != null)
                //                dataRow[binding[k]] = range.Cells[i, 1 + k].Value2.ToString();
                //        }

                //        tempT.Rows.Add(dataRow);
                //    }

                //    //lblExcelCnt.Text = (i - 1).ToString() + " / " + (range.Rows.Count - 1).ToString();
                //    Application.DoEvents();
                //}

                object[,] value = range.Value;

                int columnsCount = value.GetLength(1);
                for(var colCnt = 1; colCnt <= columnsCount; colCnt++)
                {
                    dt.Columns.Add((string)value[1, colCnt], typeof(string));
                }

                int rowsCount = value.GetLength(0);
                for(var rowCnt = 2; rowCnt <= rowsCount; rowCnt++)
                {
                    var dataRow = dt.NewRow();
                    for(var colCnt = 1; colCnt <= columnsCount; colCnt++)
                    {
                        string t = "";

                        if(value[rowCnt, colCnt] != null)
                            t = value[rowCnt, colCnt].ToString();

                        dataRow[colCnt - 1] = t;
                    }
                    dt.Rows.Add(dataRow);
                }

                return dt;
            }
            catch
            {
                throw;
            }
            finally
            {
                //ReleaseObject(worksheet);
                //ReleaseObject(workbook);
                //ReleaseObject(excelApp);

                KillExcel(excelApp);
            }
        }

    }
}
