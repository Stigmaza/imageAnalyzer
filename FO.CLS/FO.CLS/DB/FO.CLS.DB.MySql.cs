using FO.CLS.UTIL;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace FO.CLS.DB
{
    public class MySQL
    {
        #region 상수
        // DB IP 설정명
        public const string SET_NAME_DB_IP       = "IP";

        // DB PORT 설정명                        
        public const string SET_NAME_DB_PORT     = "PORT";

        // DB NAME 설정명                        
        public const string SET_NAME_DB_NAME     = "NAME";

        // DB ID 설정명                          
        public const string SET_NAME_DB_ID       = "ID";

        // DB PW 설정명
        public const string SET_NAME_DB_PASSWORD = "PW";

        //////////////////////////////////////////////////

        // DB IP 기본값
        public string DEFAULT_MYSQL_IP = "fouronedb.synology.me";

        // DB PORT 기본값
        public string DEFAULT_MYSQL_PORT = "33070";

        // DB NAME 기본값
        public string DEFAULT_MYSQL_DBNAME = "samhwa";

        // DB ID 기본값
        public string DEFAULT_MYSQL_ID = "fourone";

        // DB PW 기본값
        public string DEFAULT_MYSQL_PW = "Fourone2020!!";

        //////////////////////////////////////////////////


        public string IP { get; set; }
        public string PORT { get; set; }
        public string DBNAME { get; set; }
        public string ID { get; set; }
        public string PW { get; set; }
        private string PREFIX = "";

        #endregion

        //////////////////////////////////////////////////
        ///
        #region 변수
        private MySqlConnection _mySqlConnection;

        private MySqlTransaction mySqlCommandTransaction;

        // 연결상태 - 접속이 안되거나, 쿼리시 익셉션이 떨어질때 false / db를 사용할때만 접속하기 때문에 변수를 하나더 만듬
        public bool     connected    = false;

        // 마지막으로 쿼리 보내고 ok일때 시간
        public DateTime lastCommTime = new DateTime(2020,01,01,00,00,00);

        #endregion

        #region 생성자
        public MySQL(string prefix = "")
        {
            SQLITEINI xml = new SQLITEINI();

            PREFIX = prefix;
            if(PREFIX != "") PREFIX += "_";

            IP = xml.readValue(PREFIX + SET_NAME_DB_IP, DEFAULT_MYSQL_IP);
            PORT = xml.readValue(PREFIX + SET_NAME_DB_PORT, DEFAULT_MYSQL_PORT);
            DBNAME = xml.readValue(PREFIX + SET_NAME_DB_NAME, DEFAULT_MYSQL_DBNAME);
            ID = xml.readValue(PREFIX + SET_NAME_DB_ID, DEFAULT_MYSQL_ID);
            PW = xml.readValue(PREFIX + SET_NAME_DB_PASSWORD, DEFAULT_MYSQL_PW);

            _mySqlConnection = new MySqlConnection();

            saveSet();
        }

        public void saveSet()
        {
            SQLITEINI xml = new SQLITEINI();

            xml.WriteValue(PREFIX + SET_NAME_DB_IP, IP);
            xml.WriteValue(PREFIX + SET_NAME_DB_PORT, PORT);
            xml.WriteValue(PREFIX + SET_NAME_DB_NAME, DBNAME);
            xml.WriteValue(PREFIX + SET_NAME_DB_ID, ID);
            xml.WriteValue(PREFIX + SET_NAME_DB_PASSWORD, PW);
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

            try
            {
                string connectionString = string.Format("Server={0};"
                                                      + "port={1};"
                                                      + " Database={2};"
                                                      + " Uid={3};"
                                                      + " pwd={4}; sslmode=None;"
                                                      , ip
                                                      , port
                                                      , name
                                                      , uid
                                                      , pwd);
                if(_mySqlConnection == null)
                {
                    _mySqlConnection = new MySqlConnection();
                }

                // 연동 상태 체크 -> Open 이면 Close
                if(_mySqlConnection.State == ConnectionState.Open)
                {
                    _mySqlConnection.Close();
                }

                // DB 연동
                _mySqlConnection.ConnectionString = connectionString;
                _mySqlConnection.Open();

                bRet = _mySqlConnection.State.Equals(ConnectionState.Open);

                connected = bRet;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

                connected = false;
                _mySqlConnection = null;

                bRet = false;
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
                if(_mySqlConnection == null) return;

                // 연동 해제
                _mySqlConnection.Close();
            }
            catch
            {
                _mySqlConnection = null;
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
                if(_mySqlConnection == null) return false;

                return _mySqlConnection.State.Equals(ConnectionState.Open);
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        ///  Table Select
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns>Select 결과 DataTable</returns>
        public DataTable Select(string sql)
        {
            try
            {
                DataTable dataTable = new DataTable();

                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(sql, _mySqlConnection);
                mySqlDataAdapter.SelectCommand.CommandTimeout = 500;

                DataSet dataSet = new DataSet();

                mySqlDataAdapter.Fill(dataSet);

                if(dataSet.Tables.Count > 0)
                    dataTable = dataSet.Tables[0];

                lastCommTime = DateTime.Now;
                connected = true;

                return dataTable;
            }
            catch
            {
                connected = false;
                throw;
            }
        }

        /// <summary>        
        /// 
        ///        string sql1 = " SELECT item_code as item_index , item_name " +
        ///                      " FROM item_information " +
        ///                      " WHERE item_flag = 'Y' ";
        ///
        ///        string sql2 = " SELECT concat(item_code, '22')  as item_code, item_name " +
        ///                      " FROM item_information " +
        ///                      " WHERE item_flag = 'Y' ";
        ///
        ///        string sql3 = " SELECT concat(item_code, '33')  as item_code, item_name " +
        ///                      " FROM item_information " +
        ///                      " WHERE item_flag = 'Y' ";
        ///
        ///        string sql4 = " SELECT concat(item_code, '44')  as item_code, item_name " +
        ///                      " FROM item_information " +
        ///                      " WHERE item_flag = 'Y' ";
        ///
        ///
        ///        DataTable[] table = _mysqlDB.Select(new string[] { sql1, sql2, sql3, sql4 });
        ///            
        ///        if(table.Length > 0 )
        ///        {
        ///            dataGridView2.DataSource = table[0];
        ///            dataGridView3.DataSource = table[1];
        ///
        ///            bindableListView2.DataSource = table[2];
        ///            bindableListView3.DataSource = table[3];
        ///        }
        ///
        /// </summary>
        public DataTable[] Select(string[] sqls)
        {
            int sqlCount = sqls.Length;

            if(sqlCount == 0)
                return new DataTable[0];

            var joinedSql = string.Join(";", sqls);

            DataTable[] dataTable = new DataTable[sqlCount];

            try
            {
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(joinedSql, _mySqlConnection);
                DataSet dataSet = new DataSet();

                mySqlDataAdapter.Fill(dataSet);

                for(int i = 0; i < dataSet.Tables.Count; i++)
                {
                    dataTable[i] = dataSet.Tables[i];
                }

                lastCommTime = DateTime.Now;
                connected = true;
            }
            catch
            {
                connected = false;
                throw;
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
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, _mySqlConnection);

                int r = mySqlCommand.ExecuteNonQuery();

                lastCommTime = DateTime.Now;
                connected = true;

                return r;
            }
            catch
            {
                throw;
            }
        }

        /*
                    if (_mysqlDB.Connect())
                    {
                        _mysqlDB.beginTransaction();

                        if (_mysqlDB.TransactionNonQuery(sql) > 0)
                        {
                            _mysqlDB.commit();
                        }
                        else 
                        {
                            _mysqlDB.rollback();
                        }

                        _mysqlDB.Disconnect();
                    }

         */

        public bool beginTransaction()
        {
            try
            {
                mySqlCommandTransaction = _mySqlConnection.BeginTransaction();

                lastCommTime = DateTime.Now;
                connected = true;

                return true;
            }
            catch
            {
                connected = false;
                throw;
            }
        }

        public int TransactionNonQuery(string sql, string[] pKey = null, object[] pValue = null)
        {
            try
            {
                MySqlCommand mySqlCommand = new MySqlCommand(sql, _mySqlConnection, mySqlCommandTransaction);

                if(pKey != null)
                {
                    for(int i = 0; i < pKey.Length; i++)
                    {
                        mySqlCommand.Parameters.AddWithValue(pKey[i], pValue[i]);
                    }
                }

                int r = mySqlCommand.ExecuteNonQuery();

                lastCommTime = DateTime.Now;
                connected = true;

                return r;

            }
            catch
            {
                connected = false;
                throw;
            }
        }

        public bool commit()
        {
            try
            {
                mySqlCommandTransaction.Commit();

                lastCommTime = DateTime.Now;
                connected = true;

                return true;
            }
            catch
            {
                connected = false;
                throw;
            }
        }
        public bool rollback()
        {
            try
            {
                mySqlCommandTransaction.Rollback();

                lastCommTime = DateTime.Now;
                connected = true;

                return true;
            }
            catch
            {
                connected = false;

                throw;
            }
        }


        #endregion
    }
}
