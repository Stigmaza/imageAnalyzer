using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;

namespace FO.CLS.DB
{
    public class Oracle
    {
        #region 상수

        //////////////////////////////////////////////////

        // DB IP 기본값
        public string DEFAULT_ORACLE_IP = "";

        // DB PORT 기본값
        public string DEFAULT_ORACLE_PORT = "";

        // DB NAME 기본값
        public string DEFAULT_ORACLE_DBNAME = "";

        // DB ID 기본값
        public string DEFAULT_ORACLE_ID = "";

        // DB PW 기본값
        public string DEFAULT_ORACLE_PW = "";

        //////////////////////////////////////////////////
        ///
        public string ODBIP { get; set; }
        public string ODBPORT { get; set; }
        public string ODB_SERVICE_NAME { get; set; }
        public string ODB_ID { get; set; }
        public string ODB_PW { get; set; }
        //private string ODBIP = string.Empty;
        //private string ODBPORT = string.Empty;
        //private string ODB_SERVICE_NAME = string.Empty;
        //private string ODB_ID = string.Empty;
        //private string ODB_PW = string.Empty;

        // DB 연결 관리
        private OracleConnection _oracleConnection;

        private OracleTransaction _oracleCommandTransaction;

        // 마지막으로 쿼리 보내고 ok일때 시간
        public DateTime lastCommTime = new DateTime(2020, 01, 01, 00, 00, 00);

        #endregion

        #region 생성자 및 메서드
        public Oracle()
        {
            _oracleConnection = new OracleConnection();

            /*
            UtilXml ux = new UtilXml();

            ODBIP = ux.GetValue("ODBIP", ODBIP);
            ODBPORT = ux.GetValue("ODBPORT", ODBPORT);
            ODB_SERVICE_NAME = ux.GetValue("ODB_SERVICE_NAME", ODB_SERVICE_NAME);
            ODB_ID = ux.GetValue("ODB_ID", ODB_ID);
            ODB_PW = ux.GetValue("ODB_PW", ODB_PW);
            */
        }

        /// <summary>
        /// 데이터 베이스 연동
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="name"></param>
        /// <param name="uid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public bool Connect(string ip, string port, string name, string uid, string pwd)
        {
            bool bRet = false;

            try
            {
                string connectionString = string.Format("Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)"
                                            + "(HOST = {0})(PORT = {1}))"
                                            + "(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {2}))); User ID = {3}; "
                                            + "Password = {4};"
                                            , ip
                                            , port
                                            , name
                                            , uid
                                            , pwd
                                            );
                //string connectionString = string.Format("Data Source = (DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)"
                //                            + "(HOST = {0})(PORT = {1}))"
                //                            + "(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = {2}))); User ID = {3}; "
                //                            + "Password = {4}"
                //                            , ip
                //                            , port
                //                            , name
                //                            , uid
                //                            , pwd
                //                            );

                if(_oracleConnection == null)
                {
                    _oracleConnection = new OracleConnection();
                }

                if(_oracleConnection.State == ConnectionState.Open)
                {
                    _oracleConnection.Close();
                }

                _oracleConnection.ConnectionString = connectionString;
                _oracleConnection.Open();

                bRet = _oracleConnection.State.Equals(ConnectionState.Open);
            }
            catch
            {
                _oracleConnection = null;

                bRet = false;
            }

            return bRet;
        }

        public bool Connect()
        {
            return Connect(ODBIP, ODBPORT, ODB_SERVICE_NAME, ODB_ID, ODB_PW);
        }

        /// <summary>
        /// 데이터 베이스 연동 해제
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if(_oracleConnection == null) return;

                _oracleConnection.Close();
            }
            catch
            {
                _oracleConnection = null;
            }
        }

        /// <summary>
        /// 현재 DB 접속 상태를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public bool IsConnect()
        {
            try
            {
                if(_oracleConnection == null) return false;

                return _oracleConnection.State.Equals(ConnectionState.Open);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable Select(string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                OracleDataAdapter oracleDataAdaper = new OracleDataAdapter(sql, _oracleConnection);
                DataSet dataset = new DataSet();

                oracleDataAdaper.Fill(dataset);
                dataTable = dataset.Tables[0];

                lastCommTime = DateTime.Now;
                return dataTable;
            }
            catch(OracleException oraclEx)
            {
                string errorMsg = sql + " [OracleException] Error Database Select " + oraclEx;
                //WriteLog(errorMsg);
                Console.WriteLine(errorMsg);

                dataTable = null;
                return dataTable;
            }
            catch(Exception Ex)
            {
                string errorMsg = sql + " [OracleException_2] Error Database Select " + Ex;
                //WriteLog(errorMsg);
                Console.WriteLine(errorMsg);

                dataTable = null;
                return dataTable;
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Insert & Delete
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool DbQuery(string sql)
        {
            OracleCommand oracleCommand = new OracleCommand(sql, _oracleConnection);

            try
            {
                //ConnectCheck();

                if(oracleCommand.ExecuteNonQuery() != 0)
                {
                    lastCommTime = DateTime.Now;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(OracleException oraclEx)
            {
                string errorMsg = sql + " [OracleException] Error Database Query " + oraclEx;
                //WriteLog(errorMsg);

                return false;
            }
            catch(Exception Ex)
            {
                string errorMsg = sql + " [OracleException_2] Error Database Query " + Ex;
                //WriteLog(errorMsg);

                return false;
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// TransactionNonQuery() 에 sql 넣기전에 begin함수를 먼저 호출해놓고
        /// TransactionNonQuery()에다가 sql 넣어야함
        /// </summary>
        /// <returns></returns>
        public bool beginTransaction()
        {
            bool iRet = false;

            //ConnectCheck();

            try
            {
                _oracleCommandTransaction = _oracleConnection.BeginTransaction();

                iRet = true;

                lastCommTime = DateTime.Now;
            }
            catch
            {
                throw;
            }

            return iRet;
        }

        /// <summary>
        /// 다중 insert 할때 nonquery에다가 여러개 넣고나서 commit을 해줘야 DB에 적용됨
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int TransactionNonQuery(string sql)
        {
            int iRet = 0;

            try
            {
                OracleCommand oracleCommand = new OracleCommand(sql, _oracleConnection);

                iRet = oracleCommand.ExecuteNonQuery();

                lastCommTime = DateTime.Now;
            }
            catch(Exception e)
            {

                MessageBox.Show(e.Message);
                throw;
            }

            return iRet;
        }

        /// <summary>
        /// 트랜잭션 COMMIT
        /// </summary>
        /// <returns></returns>
        public bool commit()
        {
            bool iRet = false;
            try
            {
                _oracleCommandTransaction.Commit();
                iRet = true;

                lastCommTime = DateTime.Now;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            finally
            {
                Disconnect();
            }

            return iRet;
        }

        /// <summary>
        /// 트랜잭션 ROLLBACK
        /// </summary>
        /// <returns></returns>    
        public bool rollback()
        {
            bool iRet = false;
            try
            {
                _oracleCommandTransaction.Rollback();
                iRet = true;

                lastCommTime = DateTime.Now;
            }
            catch
            {
                throw;
            }
            finally
            {
                Disconnect();
            }

            return iRet;
        }
        #endregion
    }
}
