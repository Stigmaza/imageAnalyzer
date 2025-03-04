using System;
using System.Data;
using System.Data.OleDb;

namespace FO.CLS.DB
{
    public class Access
    {
        #region 변수
        private OleDbConnection _oleDbConnection;
        #endregion

        #region 생성자
        public Access()
        {
            _oleDbConnection = new OleDbConnection();
        }
        #endregion

        #region 메서드
        /// <summary>
        /// 데이터 베이스 연동
        /// </summary>
        /// <param name="path">파일 경로</param>
        /// <param name="extension">파일 확장자</param>
        /// <returns></returns>
        public bool Connect(string path, string extension)
        {
            bool bRet = false;

            try
            {
                string connectionString = string.Empty;

                switch(extension)
                {
                    case "mdb":
                        //connectionString = string.Format("Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0};Mode=Share Exclusive;", path);
                        connectionString = string.Format("Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0};", path);
                        break;

                    case "accdb":
                        connectionString = string.Format("Provider = Microsoft.ACE.OLEDB.12.0; Data Source = {0};", path);
                        break;

                    default:
                        connectionString = string.Format("Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0};", path);
                        break;
                }

                if(_oleDbConnection == null)
                {
                    _oleDbConnection = new OleDbConnection();
                }

                // 연동 상태 체크 -> Open 이면 Close
                if(_oleDbConnection.State == ConnectionState.Open)
                {
                    _oleDbConnection.Close();
                }

                // DB 연동
                _oleDbConnection.ConnectionString = connectionString;
                _oleDbConnection.Open();

                bRet = _oleDbConnection.State.Equals(ConnectionState.Open);
            }
            catch
            {
                _oleDbConnection = null;

                bRet = false;
            }

            return bRet;
        }

        /// <summary>
        /// 데이터 베이스 연동 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if(_oleDbConnection == null) return;

                _oleDbConnection.Close();
            }
            catch
            {
                _oleDbConnection = null;
            }
        }

        /// <summary>
        /// 데이터 베이스 연동 상태 Get
        /// </summary>
        /// <returns>연동상태</returns>
        public bool IsConnect()
        {
            try
            {
                bool bRet = false;

                if(_oleDbConnection == null)
                {
                    return bRet;
                }

                bRet = _oleDbConnection.State.Equals(ConnectionState.Open);

                return bRet;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///  Table Select
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns>Select 결과 DataTable</returns>
        public DataTable Select(string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(sql, _oleDbConnection);
                DataSet dataSet = new DataSet();

                oleDbDataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
            }
            catch(OleDbException sqlex)
            {
                Console.WriteLine("Select OleDbException - " + sqlex.ToString());

                dataTable = new DataTable();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Select Exception - " + ex.ToString());

                dataTable = new DataTable();
            }

            return dataTable;
        }

        /// <summary>
        /// Table Insert Or Update
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns>반영된 Row 수</returns>
        public int Command(string sql)
        {
            int iRet = 0;

            try
            {
                OleDbCommand oleDbCommand = new OleDbCommand(sql, _oleDbConnection);
                iRet = oleDbCommand.ExecuteNonQuery();
            }
            catch(OleDbException sqlex)
            {
                Console.WriteLine("Select OleDbException - " + sqlex.ToString());

                iRet = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Select Exception - " + ex.ToString());

                iRet = 0;
            }

            return iRet;
        }
        #endregion
    }
}
