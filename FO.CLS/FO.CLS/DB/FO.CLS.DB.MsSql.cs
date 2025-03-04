using FO.CLS.LOG;
using FO.CLS.UTIL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FO.CLS.DB
{
    public class MsSql
    {
        #region 변수
        private SqlConnection _sqlConnection;

        private SqlTransaction SqlCommandTransaction;


        public string IP;
        public string PORT;
        public string DBNAME;
        public string ID;
        public string PW;
        private int    QUERYTIMEOUT;
        private string PREFIX = "";
        #endregion

        #region 생성자
        public MsSql(string prefix = "")
        {
            SQLITEINI xml = new SQLITEINI();

            PREFIX = prefix;
            if(PREFIX != "") PREFIX += "_";


            IP = xml.readValue(PREFIX + "IP", "");
            PORT = xml.readValue(PREFIX + "PORT", "");
            DBNAME = xml.readValue(PREFIX + "DBNAME", "");
            ID = xml.readValue(PREFIX + "ID", "");
            PW = xml.readValue(PREFIX + "PW", "");
            QUERYTIMEOUT = Convert.ToInt32(xml.readValue(PREFIX + "TIMEOUT", "300"));

            _sqlConnection = new SqlConnection();

            saveSet();
        }

        public MsSql(string ip, string port, string dbname, string id, string pw, string timeout="300")
        {
            IP = ip;
            PORT = port;
            DBNAME = dbname;
            ID = id;
            PW = pw;
            QUERYTIMEOUT = Convert.ToInt32(timeout);

            _sqlConnection = new SqlConnection();

            saveSet();
        }

        ~MsSql()
        {
            //_sqlConnection?.Close();

            //GC.Collect();
        }

        public void saveSet()
        {
            SQLITEINI xml = new SQLITEINI();

            xml.WriteValue(PREFIX + "IP", IP);
            xml.WriteValue(PREFIX + "PORT", PORT);
            xml.WriteValue(PREFIX + "DBNAME", DBNAME);
            xml.WriteValue(PREFIX + "ID", ID);
            xml.WriteValue(PREFIX + "PW", PW);
            xml.WriteValue(PREFIX + "TIMEOUT", QUERYTIMEOUT.ToString());
        }


        public string runSP(string spName, string param = "")
        {
            string r = string.Empty;

            try
            {
                string sql = "EXEC dbo." + spName + " " + param;

                //beginTransaction();

                DataTable dt = Select(sql);

                if(dt.Rows.Count == 1)
                {
                    r = dt.Rows[0][0].ToString();
                }

                //commit();

            }
            catch
            {
                //rollback();
                throw;
            }

            return r;
        }


        public string runFunc(string fcName, string param = "")
        {
            string r = string.Empty;

            try
            {
                string sql = "select dbo." + fcName + "(" + param + ")";

                DataTable dt = Select(sql);

                if(dt.Rows.Count == 1)
                {
                    r = dt.Rows[0][0].ToString();
                }
            }
            catch
            {
                throw;
            }

            return r;
        }

        #endregion

        #region  메서드
        /// <summary>
        /// 데이터 베이스 연동
        /// </summary>
        /// <param name="ip">ip</param>
        /// <param name="port">port</param>
        /// <param name="name">Database Name</param>
        /// <param name="uid">id</param>
        /// <param name="pwd">password</param>
        /// <returns>연동상태</returns>
        public bool Connect(string ip, string port, string name, string uid, string pwd)
        {
            bool bRet = false;

            if(port.Length > 0)
            {
                try
                {
                    string connectionString = string.Format("Server={0},{1};"
                                                      + " Initial Catalog={2};"
                                                      + " User ID={3};"
                                                      + " Password={4};"
                                                      , ip
                                                      , port
                                                      , name
                                                      , uid
                                                      , pwd);

                    if(_sqlConnection == null)
                    {
                        _sqlConnection = new SqlConnection();
                    }

                    // 연동 상태 체크 -> Open 이면 Close
                    if(_sqlConnection.State == ConnectionState.Open)
                    {
                        _sqlConnection.Close();
                    }

                    // DB 연동
                    _sqlConnection.ConnectionString = connectionString;
                    _sqlConnection.Open();

                    bRet = _sqlConnection.State.Equals(ConnectionState.Open);
                }
                catch
                {
                    _sqlConnection = null;

                    bRet = false;
                }
            }

            return bRet;
        }

        public bool Connect()
        {
            return Connect(IP, PORT, DBNAME, ID, PW);
        }

        /// <summary>
        /// 데이터 베이스 연동 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if(_sqlConnection == null) return;

                // 연동 해제
                _sqlConnection.Close();
            }
            catch
            {
                _sqlConnection = null;
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

                if(_sqlConnection == null)
                {
                    return bRet;
                }

                bRet = _sqlConnection.State.Equals(ConnectionState.Open);

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
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sql, _sqlConnection);

                sqlDataAdapter.SelectCommand.CommandTimeout = QUERYTIMEOUT;

                DataSet dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet);

                dataTable = dataSet.Tables[0];
            }
            catch
            {
                throw;
            }

            return dataTable;
        }

        public List<DataTable> multiSelect(string sql)
        {
            List<DataTable> r = new List<DataTable>();

            try
            {
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sql, _sqlConnection);

                sqlDataAdapter.SelectCommand.CommandTimeout = QUERYTIMEOUT;

                DataSet dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet);

                for (int i = 0; i < dataSet.Tables.Count; i++)
                {
                    r.Add(dataSet.Tables[i]);
                }
            }
            catch
            {
                throw;
            }

            return r;
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
                SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection);
                iRet = sqlCommand.ExecuteNonQuery();

            }
            catch(SqlException sqlex)
            {
                Console.WriteLine("Select MySqlException - " + sqlex.ToString());

                iRet = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Select Exception - " + ex.ToString());

                iRet = 0;
            }

            return iRet;
        }



        public bool beginTransaction()
        {
            bool iRet = false;

            try
            {
                SqlCommandTransaction = _sqlConnection.BeginTransaction();

                iRet = true;

            }
            catch
            {
                throw;
            }

            return iRet;
        }

        public int TransactionNonQuery(string sql, string[] pKey = null, object[] pValue = null)
        {
            int iRet = 0;

            try
            {
                SqlCommand mySqlCommand = new SqlCommand(sql, _sqlConnection, SqlCommandTransaction);

                if(pKey != null)
                {
                    for(int i = 0; i < pKey.Length; i++)
                    {
                        mySqlCommand.Parameters.AddWithValue(pKey[i], pValue[i]);
                    }
                }

                iRet = mySqlCommand.ExecuteNonQuery();

            }
            catch
            {
                throw;
            }

            return iRet;
        }
        public DataTable TransactionSelect(string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                SqlCommand mySqlCommand = new SqlCommand(sql, _sqlConnection, SqlCommandTransaction);

                SqlDataReader reader = mySqlCommand.ExecuteReader();

                dataTable.Load(reader);

                reader.Close();
            }
            catch
            {
                throw;
            }

            return dataTable;
        }

        public string TransactionRunFunc(string fcName, string param = "")
        {
            string r = string.Empty;

            try
            {
                string sql = "select dbo." + fcName + "(" + param + ")";

                DataTable dt = TransactionSelect(sql);

                if(dt.Rows.Count == 1)
                {
                    r = dt.Rows[0][0].ToString();
                }
            }
            catch
            {
                throw;
            }

            return r;
        }

        public string TransactionRunSP(string spName, string param = "")
        {
            string r = string.Empty;

            try
            {
                if(param.Length > 0)
                {
                    param = "'" + param + "'";
                }

                string sql = "EXEC dbo." + spName + " " + param;

                DataTable dt = TransactionSelect(sql);

                if(dt.Rows.Count == 1)
                {
                    r = dt.Rows[0][0].ToString();
                }

            }
            catch
            {
                throw;
            }

            return r;
        }

        public bool commit()
        {
            bool iRet = false;
            try
            {
                SqlCommandTransaction?.Commit();
                iRet = true;
            }
            catch
            {
                throw;
            }

            return iRet;
        }

        public bool rollback()
        {
            bool iRet = false;
            try
            {
                SqlCommandTransaction?.Rollback();
                iRet = true;
            }
            catch(Exception ex)
            {
                Write log = new Write();

                log.WriteLog("rollback : " + ex.Message);
                log.WriteLog("rollback : " + ex.StackTrace);
            }

            return iRet;
        }
        #endregion
    }
}
