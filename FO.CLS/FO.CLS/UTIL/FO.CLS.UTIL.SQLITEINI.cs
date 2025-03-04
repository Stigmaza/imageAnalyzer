using System;
using System.Data;
using System.Data.Entity.Migrations.Model;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace FO.CLS.UTIL
{
    public class SQLITEINI
    {
        string sqlitePath;
        SQLiteConnection connection;

        string prefix = "";
        string table = "ini";

        public SQLITEINI(string prefix = "", string filePath = "")
        {
            if (filePath == "")
            {
                sqlitePath = Application.StartupPath + "\\ini.sqlite";
            }
            else
            {
                sqlitePath = filePath;
            }            

            connection = new SQLiteConnection("Data Source=" + sqlitePath + ";Version=3;");

            createNew(false);

            this.prefix = prefix;
        }

        public void createNew(bool deleteIfExists)
        {
            if (File.Exists(sqlitePath))
            {
                if (deleteIfExists)
                {
                    GC.Collect();
                    File.Delete(sqlitePath);
                }
                else
                {
                    return;
                }
            }

            SQLiteConnection.CreateFile(sqlitePath);

            createTable("ini");
        }

        public void setTable(string table)
        {
            this.table = table;
        }

        public void createTable(string table)
        {
            try
            {
                connection.Open();

                string sql = @"
                    create table `:table` (key varchar(200), value varchar(200));

                    CREATE UNIQUE INDEX `PK_:table` ON `:table` (
	                    `key`
                    );
                ";

                sql = sql.Replace(":table", table);

                SQLiteCommand command = new SQLiteCommand(sql, connection);

                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }
        }

        public void setPrefix(string prefix)
        {
            this.prefix = prefix;
        }

        //~SQLITEINI()
        //{
        //    try
        //    {
        //        connection?.Close();
        //        GC.Collect();
        //    }
        //    catch
        //    {

        //    }
        //}

        public string readValue(string key, string defaultValue = "")
        {
            key = this.prefix + key;

            connection.Open();

            string sql = @"
                            SELECT value
                              FROM `:table`
                             WHERE key = ':key'
                            ";

            sql = sql.Replace(":key", key);
            sql = sql.Replace(":table", table);

            SQLiteDataAdapter command = new SQLiteDataAdapter(sql, connection);

            DataSet dataSet = new DataSet();

            command.Fill(dataSet);
            DataTable dataTable = dataSet.Tables[0];

            connection.Close();

            string r = defaultValue;

            if (dataTable.Rows.Count > 0)
                r = dataTable.Rows[0]["value"].ToString();

            return r;
        }

        public int readValuei(string key, int defaultValue = 0)
        {
            string t = readValue(key, defaultValue.ToString());

            if (int.TryParse(t, out int i) == false)
            {
                return defaultValue;
            }

            return i;
        }

        public double readValued(string key, double defaultValue = 0)
        {
            string t = readValue(key, defaultValue.ToString());

            if (double.TryParse(t, out double i) == false)
            {
                return defaultValue;
            }

            return i;
        }

        public bool readValueb(string key, bool defaultValue = false)
        {
            string t = readValue(key , "0");

            if (t == "1") return true;

            return false;
        }

        public void WriteValue(string key, int value)
        {
            WriteValue(key, value.ToString());
        }

        public void WriteValue(string key, double value)
        {
            WriteValue(key, value.ToString());
        }

        public void WriteValue(string key, bool value)
        {
            WriteValue(key, value == true ? "1" : "0"); 
        }

        public void WriteValue(string key, string value)
        {
            key = this.prefix + key;

            connection.Open();

            string sql = @"
                            UPDATE `:table`
                               SET value = ':value'
                             WHERE key = ':key'
                            ";

            sql = sql.Replace(":value", value);
            sql = sql.Replace(":table", table);
            sql = sql.Replace(":key", key);

            SQLiteCommand sqliteCommand = new SQLiteCommand(sql, connection);

            int affectedRow = sqliteCommand.ExecuteNonQuery();

            if (affectedRow == 0)
            {
                sql = @"
                        INSERT
                          INTO `:table`
                        VALUES (':key', ':value')
                        ";

                sql = sql.Replace(":value", value);
                sql = sql.Replace(":table", table);
                sql = sql.Replace(":key", key);

                sqliteCommand = new SQLiteCommand(sql, connection);
                sqliteCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        public DataTable select(string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                connection.Open();

                SQLiteDataAdapter command = new SQLiteDataAdapter(sql, connection);

                DataSet dataSet = new DataSet();

                command.Fill(dataSet);
                dataTable = dataSet.Tables[0];
            }
            finally
            {
                connection.Close();
            }

            return dataTable;
        }

        public void nonquery(string sql)
        {

        }
    }
}
