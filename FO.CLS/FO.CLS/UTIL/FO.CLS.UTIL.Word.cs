using FO.CLS.DB;
using FO.CLS.LOG;
using System;
using System.Data;

namespace FO.CLS.UTIL
{
    public class Word
    {
        #region 상수 및 변수

        private string[] languageNameString = new string[] { "Korean", "English", "Chinese", "Vietnamese" };

        // XML
        SQLITEINI FOCLSUTILXml = new SQLITEINI();

        // My sql 연동
        MySQL FOCLSDBMySql = new MySQL();
        bool _isFOCDMySqlConnectFlag = false;

        // LOG
        Write FOCLSLOGWrite = new Write(null);

        // 단어 저장 테이블
        public DataTable dataTableWord = new DataTable();
        #endregion

        #region 생성자
        public Word()
        {

        }

        #endregion

        #region 메서드
        // DB 연동
        private void DBConnect()
        {
            string dbip = FOCLSUTILXml.readValue("MYSQL_IP", FOCLSDBMySql.DEFAULT_MYSQL_IP);
            string dbPort = FOCLSUTILXml.readValue("MYSQL_PORT", FOCLSDBMySql.DEFAULT_MYSQL_PORT);
            string dbId = FOCLSUTILXml.readValue("MYSQL_ID", FOCLSDBMySql.DEFAULT_MYSQL_ID);
            string dbPW = FOCLSUTILXml.readValue("MYSQL_PASSWORD", FOCLSDBMySql.DEFAULT_MYSQL_PW);
            string dbName = FOCLSUTILXml.readValue("MYSQL_NAME", FOCLSDBMySql.DEFAULT_MYSQL_DBNAME);

            if(FOCLSDBMySql.IsConnect() == false)
            {
                _isFOCDMySqlConnectFlag = FOCLSDBMySql.Connect(dbip, dbPort, dbName, dbId, dbPW);

                if(_isFOCDMySqlConnectFlag)
                {
                    _isFOCDMySqlConnectFlag = true;

                    FOCLSLOGWrite.WriteLog("Word", "Mysql DB Connect");
                }
                else
                {
                    FOCLSLOGWrite.WriteLog("Word", "Mysql DB Disconnect");
                }
            }
        }

        // 단어 검색
        public void WordSelect()
        {
            try
            {
                string sql = string.Empty;

                // XML 파일에서 LANGUAGE_SWITCH Read
                int languageSW = Convert.ToInt32(FOCLSUTILXml.readValue("LANGUAGE_SWITCH", "0"));

                string language = languageNameString[languageSW];

                DBConnect();

                switch(language.ToUpper())
                {
                    case "KOREAN":
                        sql = "SELECT T.word_code, T.code_korean AS WORD ";
                        break;

                    case "ENGLISH":
                        sql = "SELECT T.word_code, T.code_english AS WORD ";
                        break;

                    case "CHINES":
                        sql = "SELECT T.word_code, T.code_chines AS WORD ";
                        break;

                    case "VIETNAMESE":
                        sql = "SELECT T.word_code, T.code_Vietnamese AS WORD ";
                        break;

                    default:
                        sql = "SELECT T.word_code, T.code_english AS WORD ";
                        break;
                }

                sql += " FROM dictionary_table T "
                    + " WHERE T.use_flag = 'Y'";

                dataTableWord = FOCLSDBMySql.Select(sql);
            }
            catch(Exception ex)
            {
                FOCLSLOGWrite.WriteLog("Word", "WordSelect Exception - " + ex.ToString());
                throw ex;
            }
            finally
            {
                FOCLSDBMySql.Disconnect();
            }


        }

        /// <summary>
        /// 단어 Raed
        /// </summary>
        /// <param name="wordCode">단어코드</param>
        /// <returns>읽어온 값</returns>
        public string ReadWord(string wordCode)
        {
            try
            {
                if(string.IsNullOrEmpty(wordCode))
                {
                    return "WordCode is empty.";
                }

                if(dataTableWord == null || dataTableWord.Rows.Count == 0)
                {
                    return wordCode;
                }

                string filterSting = string.Format("{0} = '{1}'", "word_code", wordCode.ToUpper());
                DataRow[] dataRows = dataTableWord.Select(filterSting);

                if(!string.IsNullOrEmpty(dataRows[0]["word"].ToString()))
                {
                    string result = dataRows[0]["word"].ToString();

                    return result;
                }

                return wordCode;
            }
            catch(Exception)
            {
                return "Word Error"; ;
            }


        }
        #endregion
    }
}
