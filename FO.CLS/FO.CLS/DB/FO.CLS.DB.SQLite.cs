using System;
using System.Data;
using System.Data.SQLite;

namespace FO.CLS.DB
{
    public class SQLite
    {
        //////////////////////////////////////////////////
        // DB IP 설정명
        public const string SET_NAME_DB_FILE_PATH = "MYSQL_IP";

        //////////////////////////////////////////////////
        ///
        #region 변수
        private SQLiteConnection _sqliteConnection;
        #endregion

        #region 생성자
        public SQLite()
        {
            _sqliteConnection = new SQLiteConnection();
        }
        #endregion

        public string filePath { get; set; }

        #region 메서드
        /// <summary>
        /// 데이터 베이스 연동
        /// </summary>
        /// <param name="path">path</param>
        /// <returns></returns>
        public bool Connect(string path)
        {
            bool bRet = false;

            try
            {
                string connectionString = string.Format("Data Source = {0}", path);

                if(_sqliteConnection == null)
                {
                    _sqliteConnection = new SQLiteConnection();
                }

                if(_sqliteConnection.State == ConnectionState.Open)
                {
                    _sqliteConnection.Close();
                }

                _sqliteConnection.ConnectionString = connectionString;
                _sqliteConnection.Open();

                bRet = _sqliteConnection.State.Equals(ConnectionState.Open);

                filePath = path;
            }
            catch(Exception)
            {
                _sqliteConnection = null;

                bRet = false;
            }

            return bRet;
        }

        public bool Connect()
        {
            return Connect(filePath);
        }
        /// <summary>
        /// 데이터 베이스 연동 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if(_sqliteConnection == null) return;

                // 연동 해제
                _sqliteConnection.Close();
            }
            catch
            {
                _sqliteConnection = null;
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

                if(_sqliteConnection == null)
                {
                    return bRet;
                }

                bRet = _sqliteConnection.State.Equals(ConnectionState.Open);

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
                SQLiteDataAdapter sqliteDataAdapter = new SQLiteDataAdapter(sql, _sqliteConnection);
                DataSet dataSet = new DataSet();

                sqliteDataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
            }
            catch(SQLiteException sqlex)
            {
                Console.WriteLine("Select SQLiteException - " + sqlex.ToString());

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
                SQLiteDataAdapter mySqlDataAdapter = new SQLiteDataAdapter(joinedSql, _sqliteConnection);
                DataSet dataSet = new DataSet();

                mySqlDataAdapter.Fill(dataSet);

                for(int i = 0; i < dataSet.Tables.Count; i++)
                {
                    dataTable[i] = dataSet.Tables[i];
                }
            }
            catch(SQLiteException sqlex)
            {
                Console.WriteLine("Select MySqlException - " + sqlex.ToString());
                return new DataTable[0];
            }
            catch(Exception ex)
            {
                Console.WriteLine("Select Exception - " + ex.ToString());
                return new DataTable[0];
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
                SQLiteCommand sqliteCommand = new SQLiteCommand(sql, _sqliteConnection);
                iRet = sqliteCommand.ExecuteNonQuery();
            }
            catch(SQLiteException sqlex)
            {
                Console.WriteLine("Select SQLiteException - " + sqlex.ToString());

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
