using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace FO.CLS.UTIL
{
    public class ReportBandInfo
    {
        public Panel pnlLayout = null;
        public Panel pnlBand = null;
        public DataTable dtBand = null;
        public IOrderedEnumerable<Control> controlsInLabel = null;

        public ReportBandInfo(Panel pnl, DataTable dt)
        {
            pnlBand = pnl;
            dtBand = dt;

            if(dtBand == null)
                dtBand = new DataTable();

            if(pnlBand != null)
            {
                controlsInLabel = from n in pnlBand.Controls.Cast<Control>()
                                  where n is Label || n is PictureBox
                                  orderby n.Name
                                  select n;

            }
        }
    }

    public class ReportPageInfo
    {
        public Panel pnlLayout = null;
        public bool incPageCount = false;

        public ReportBandInfo bandPageHeader = null;
        public ReportBandInfo bandTableHeader = null;
        public ReportBandInfo bandTableData = null;
        //public ReportBandInfo bandTableSummary = null;    // 테이블 아래쪽에 합계나오는것, 자동계산 되도록 누가좀 해줘
        public ReportBandInfo bandPageFooter = null;

        // pnlTableData 에서 몇개 출렸했는지 저장
        public int workDataRowIndex = 0;

        public ReportPageInfo(Panel layout
                            , bool _incPageCount
                            , ReportBandInfo pageHeader
                            , ReportBandInfo tableHeader
                            , ReportBandInfo tableData
                            //, ReportBandInfo tableSummary
                            , ReportBandInfo pageFooter)
        {
            pnlLayout = layout;
            incPageCount = _incPageCount;

            bandPageHeader = pageHeader;
            bandTableHeader = tableHeader;
            bandTableData = tableData;
            //bandTableSummary = tableSummary;
            bandPageFooter = pageFooter;

            if(bandPageHeader == null) bandPageHeader = new ReportBandInfo(null, null);
            if(bandTableHeader == null) bandTableHeader = new ReportBandInfo(null, null);
            if(bandTableData == null) bandTableData = new ReportBandInfo(null, null);
            //if(bandTableSummary == null) bandTableSummary = new ReportBandInfo(null, null);
            if(bandPageFooter == null) bandPageFooter = new ReportBandInfo(null, null);

            bandPageHeader.pnlLayout = pnlLayout;
            bandTableHeader.pnlLayout = pnlLayout;
            bandTableData.pnlLayout = pnlLayout;
            //bandTableSummary.pnlLayout = pnlLayout;
            bandPageFooter.pnlLayout = pnlLayout;

            workDataRowIndex = 0;
        }
    }


    public class FOREPORT
    {
        // 출력할 리포트
        ReportPageInfo[] reportInfo = null;
        int reportIndex = 0;

        // 페이지 번호
        int pageNow = 0;

        // 테이블의 데이터 rowNumber, 매페이지 마다 초기화 되는것
        int rowNumberPerPage = 0;

        // 그래픽 구성요소
        Pen pen = new Pen(Color.Black, 0.1f);
        Font fontDraw = null;
        SolidBrush brBackColor = new SolidBrush(Color.Black);
        SolidBrush brPen = new SolidBrush(Color.Black);

        // pixel -> mm 변환, 나중에 제대로 만들기
        float pixelTomm = 2.7f;


        public void printReport(ReportPageInfo[] _pageInfo)
        {
            reportInfo = _pageInfo;

            PrintDialog pd = new PrintDialog();

            pd.AllowPrintToFile = false;
            pd.UseEXDialog = true;

            if(pd.ShowDialog() == DialogResult.OK)
            {
                PrintDocument doc = new PrintDocument();
                PrintPreviewDialog ppv = new PrintPreviewDialog();

                doc.DefaultPageSettings = pd.PrinterSettings.DefaultPageSettings;
                ppv.Document = doc;

                doc.PrintPage += printPage;
                doc.BeginPrint += printBegin;

                ((Form)ppv).WindowState = FormWindowState.Maximized;

                ppv.ShowDialog();
            }
        }

        private void printBegin(object sender, PrintEventArgs e)
        {
            reportIndex = 0;
            pageNow = 1;

            foreach(var item in reportInfo)
            {
                item.workDataRowIndex = 0;
            }
        }

        private void setGraphics(Graphics g)
        {
            // 그래픽 기본 설정
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.High;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PageUnit = GraphicsUnit.Millimeter;
        }

        private Image imageFromDBColumn(object obj)
        {
            Image r = null;

            if(obj != System.DBNull.Value)
            {
                byte[] byteData = (byte[])obj;

                if(byteData.Length > 0)
                {
                    System.IO.MemoryStream msData = new System.IO.MemoryStream(byteData);
                    r = Image.FromStream(msData);
                }
            }

            return r;
        }

        private void drawLabel(Graphics g, DataRow dr, Control c, Panel layout, float marginTop
                              , ref SolidBrush brBackColor, ref SolidBrush penBrush, ref Pen pen, ref Font font)
        {
            RectangleF rect =  makeCellOutline( c, layout, marginTop);

            if(c is PictureBox)
            {
                PictureBox p = (PictureBox)c;

                string tag = p.Tag.ToString();

                tag = tag.Replace(":", "");
                tag = tag.Trim();

                Image img = imageFromDBColumn(dr[tag]);

                g.DrawImage(img, rect);
            }

            if(c is Label)
            {
                Label l = (Label)c;

                if(l.BackColor != Color.Transparent)
                {
                    brBackColor = makeBrush(brBackColor, l);

                    g.FillRectangle(brBackColor, rect);
                }

                if(l.BorderStyle != BorderStyle.None)
                {
                    g.DrawRectangles(pen, new RectangleF[] { rect });
                }

                if(l.ForeColor != penBrush.Color)
                {
                    penBrush = new SolidBrush(l.ForeColor);
                }

                StringFormat format = getAlignment(l);

                font = makeFont(font, l);

                if(l.Text.Length > 0)
                {
                    string str = l.Text;
                    string temp;

                    if(checkKeyword(str, out temp))     // 예약어 확인
                        str = temp;

                    else if(str[0] == ':' && dr != null)
                    {
                        string col = str;

                        col = col.Replace(":", "");
                        col = col.Trim();

                        if(dr[col] != DBNull.Value)
                            str = dr[col].ToString();

                    }

                    RectangleF rectDrawString =  makeCellStringRect( c, layout, marginTop);

                    g.DrawString(str, font, penBrush, rectDrawString, format);
                }
            }
        }

        private bool checkKeyword(string key, out string returnString)
        {
            returnString = string.Empty;

            try
            {
                string key2 = key.ToUpper().Trim();

                if(key2 == "@PAGECOUNT")
                {
                    returnString = pageNow.ToString();

                    return true;
                }

                if(key2 == "@ROWNUM")
                {
                    returnString = rowNumberPerPage.ToString();

                    return true;
                }

                // @DATE:YYYY-MM-DD     : 뒤쪽이 데이트 포맷
                if(key2.Contains("@DATE"))
                {
                    string[] t = key.Split(':');

                    returnString = DateTime.Now.ToString(t[1]);

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private RectangleF makeCellOutline(Control c, Panel layout, float marginTop)
        {
            float x = (float)((c.Left + layout.Left ) /pixelTomm);
            float y = (float)((c.Top  + layout.Top + marginTop)/pixelTomm);
            float w = (float)((c.Width)/pixelTomm);
            float h = (float)((c.Height)/pixelTomm);

            return new RectangleF(x, y, w, h);
        }

        private RectangleF makeCellStringRect(Control c, Panel layout, float marginTop)
        {
            float x = (float)((c.Left + layout.Left + c.Padding.Left) /pixelTomm);
            float y = (float)((c.Top  + layout.Top + c.Padding.Top + marginTop)/pixelTomm);
            float w = (float)((c.Width - c.Padding.Right)/pixelTomm);
            float h = (float)((c.Height - c.Padding.Bottom)/pixelTomm);

            return new RectangleF(x, y, w, h);
        }

        private SolidBrush makeBrush(SolidBrush old, Label l)
        {
            if(old == null || old.Color != l.BackColor)
                return new SolidBrush(l.BackColor);

            return old;
        }

        private Font makeFont(Font old, Label l)
        {
            if(old == null || (old.Name != l.Font.Name || old.Size != l.Font.Size || old.Bold != l.Font.Bold))
                return new Font(l.Font.Name, l.Font.Size, FontStyle.Bold, GraphicsUnit.Millimeter);

            return old;
        }

        private StringFormat getAlignment(Label l)
        {
            StringFormat format = new StringFormat();

            if(l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.MiddleLeft || l.TextAlign == ContentAlignment.BottomLeft)
                format.Alignment = StringAlignment.Near;

            if(l.TextAlign == ContentAlignment.TopCenter || l.TextAlign == ContentAlignment.MiddleCenter || l.TextAlign == ContentAlignment.BottomCenter)
                format.Alignment = StringAlignment.Center;

            if(l.TextAlign == ContentAlignment.TopRight || l.TextAlign == ContentAlignment.MiddleRight || l.TextAlign == ContentAlignment.BottomRight)
                format.Alignment = StringAlignment.Far;


            if(l.TextAlign == ContentAlignment.TopLeft || l.TextAlign == ContentAlignment.TopCenter || l.TextAlign == ContentAlignment.TopRight)
                format.LineAlignment = StringAlignment.Near;

            if(l.TextAlign == ContentAlignment.MiddleLeft || l.TextAlign == ContentAlignment.MiddleCenter || l.TextAlign == ContentAlignment.MiddleRight)
                format.LineAlignment = StringAlignment.Center;

            if(l.TextAlign == ContentAlignment.BottomLeft || l.TextAlign == ContentAlignment.BottomCenter || l.TextAlign == ContentAlignment.BottomRight)
                format.LineAlignment = StringAlignment.Far;

            return format;
        }

        private float drawBand(Graphics g, ReportBandInfo band, float marginTop)
        {
            float r = 0;

            if(band != null)
            {
                DataRow dr = null;

                if(band.dtBand.Rows.Count > 0)
                    dr = band.dtBand.Rows[0];

                if(band.controlsInLabel != null)
                {
                    foreach(Control c in band.controlsInLabel)
                    {
                        drawLabel(g, dr, c, band.pnlLayout, marginTop, ref brBackColor, ref brPen, ref pen, ref fontDraw);
                    }
                }

                if(band.pnlBand != null)
                    r = band.pnlBand.Height;
            }

            return r;
        }

        private void printPage(object o, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            setGraphics(g);

            ReportPageInfo rpt = reportInfo[reportIndex];

            float marginLeft = rpt.pnlLayout.Left;
            float marginTop = rpt.pnlLayout.Top;
            float marginRight = rpt.pnlLayout.Left + rpt.pnlLayout.Width;
            float marginBottom = rpt.pnlLayout.Top + rpt.pnlLayout.Height;
            float footerTop = marginBottom;
            float tableDataBandHeight = 0;

            bool hasMorePage = false;

            if(rpt.bandPageFooter.pnlBand != null)
                footerTop = marginBottom - rpt.bandPageFooter.pnlBand.Height;

            if(rpt.bandTableData.pnlBand != null)
                tableDataBandHeight = rpt.bandTableData.pnlBand.Height;

            // -----------------------------------------------------------------------------------
            // 아래쪽은 mm단위이기 때문에 pixelTomm으로 나눠서 그리기

            // 페이지 헤더
            marginTop += drawBand(g, rpt.bandPageHeader, marginTop);

            // 테이블 헤더
            marginTop += drawBand(g, rpt.bandTableHeader, marginTop);

            // 테이블 데이터
            if(rpt.bandTableData != null)
            {
                rowNumberPerPage = 1;

                for(int i = 0; i < rpt.bandTableData.dtBand.Rows.Count && rpt.workDataRowIndex < rpt.bandTableData.dtBand.Rows.Count; i++)
                {
                    // 이번에 그릴경우 푸터와 겹치게 되면 그리지 않음
                    if(marginTop + tableDataBandHeight > footerTop)
                    {
                        hasMorePage = true;
                        break;
                    }

                    DataRow dr = rpt.bandTableData.dtBand.Rows[rpt.workDataRowIndex];

                    foreach(Label l in rpt.bandTableData.controlsInLabel)
                    {
                        drawLabel(g, dr, l, rpt.bandTableData.pnlLayout, marginTop, ref brBackColor, ref brPen, ref pen, ref fontDraw);
                    }

                    rpt.workDataRowIndex++;

                    marginTop += tableDataBandHeight;

                    rowNumberPerPage++;
                }
            }

            // 테이블 합계
            //marginTop += drawBand(g, rpt.bandTableSummary, marginTop);


            // 페이지 푸터
            drawBand(g, rpt.bandPageFooter, footerTop);


            if(rpt.incPageCount)
                pageNow++;

            // 리포트 하나를 모두 출력한경우
            if(hasMorePage == false)
            {
                reportIndex++;

                // 다음 리포트가 있는지 확인
                if(reportIndex < reportInfo.Length)
                {
                    hasMorePage = true;
                }
            }

            e.HasMorePages = hasMorePage;
        }

    }
}
