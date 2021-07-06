using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using System.IO;

namespace WpfMSSQLtoSQLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ColumnInfo> columnsInfo;

        private List<ConstraintInfo> constraintInfo;

        private List<string> sqlTableName;

        private readonly string connectionStringMSSql = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True";

        private readonly string connectionStringMSQLite = @"Data Source=d:\MyProgram\Web\Html\Birdwatching\assets\Data\{0}.sqlite";


        /// <summary>
        /// Loading schema from MS SQL database.
        /// </summary>
        /// <param name="MSSqlDbName">MS SQL database name.</param>
        /// <param name="SqliteDbName">The name and path to the Sqlite database file.</param>
        private void ImportDBInfo(string MSSqlDbName, string SqliteDbName)
        {
            DataTable columnsTable;
            DataTable allIndexColumnsSchemaTable;

            using (SqlConnection MSSQLconnection = new(string.Format(connectionStringMSSql, MSSqlDbName)))
            {

                MSSQLconnection.Open();

                columnsTable = MSSQLconnection.GetSchema("Columns");

                allIndexColumnsSchemaTable = MSSQLconnection.GetSchema("IndexColumns");

                MSSQLconnection.Close();
            }

            columnsInfo = (from info in columnsTable.AsEnumerable()
                           select new ColumnInfo(
                                    info["TABLE_CATALOG"],
                                    info["TABLE_NAME"],
                                    info["COLUMN_NAME"],
                                    info["DATA_TYPE"],
                                    info["CHARACTER_MAXIMUM_LENGTH"],
                                    info["COLUMN_DEFAULT"],
                                    info["IS_NULLABLE"],
                                    info["NUMERIC_PRECISION"],
                                    info["NUMERIC_SCALE"]
                           )).ToList();

            constraintInfo = (from constraint in allIndexColumnsSchemaTable.AsEnumerable()
                              select new ConstraintInfo(
                                constraint["table_name"],
                                constraint["column_name"],
                                constraint["constraint_name"],
                                constraint["KeyType"]
                              )).ToList();

            // sqlTableName = (from info in constraintInfo select info.TableName).ToList();

            sqlTableName = constraintInfo.Select(info => info.TableName).ToList();

            BtnCreateScript.Content = $"Создать скрипт для таблицы {sqlTableName.First()}";

            TbxScript.Text = "Data loading completed!";

            beginIndex = 0;

            BtnCreateScript.Content = $"Создать скрипт для таблицы {sqlTableName[beginIndex]}";

            BtnCreateTable.Content = $"Создать таблицу";

            BtnCreateTable.IsEnabled = false;

            BtnCreateScript.IsEnabled = true;

            BtnCopyTable.IsEnabled = true;
        }

        private int beginIndex;

        /// <summary>
        /// Creates a script to create a Sqlite database table.
        /// </summary>
        private void CreateTableScript()
        {
            string createSQliteTable = "CREATE TABLE '{0}' (";

            string currentTable = "";

            string currentColumn = "";

            string nextTable = "";

            for (int i = beginIndex; i != columnsInfo.Count; i++)
            {
                nextTable = columnsInfo[i].TableName;

                if (currentTable == "")
                {
                    currentTable = nextTable;

                    TbxScript.Text = string.Format(createSQliteTable, nextTable);
                }

                if (nextTable != currentTable)
                {
                    beginIndex = i;

                    break;
                }
                else
                {
                    TbxScript.Text += (currentColumn != "") ? currentColumn + "," : "";
                }

                currentColumn = string.Format("\n{0} {1}", SetColumnNameAndType(columnsInfo[i]),
                                                           (columnsInfo[i].IsNullable == "NO")
                                                                    ? "NOT NULL"
                                                                    : "NULL");
            }

            if (currentTable == nextTable)
            {
                beginIndex = 0;

                nextTable = sqlTableName[beginIndex];
            }

            TbxScript.Text += $"{currentColumn}\n);\n";

            BtnCreateTable.IsEnabled = true;

            BtnCreateTable.Content = $"Создать таблицу {currentTable}";

            BtnCreateScript.Content = $"Создать скрипт для таблицы {nextTable}";

            return;

            // Returns the name and type of the column.
            string SetColumnNameAndType(ColumnInfo info)
            {
                string constraintName = GetConstraintName(info);

                string type = ConvertType(info.DataType);

                if (constraintName != null && constraintName.StartsWith("PK", StringComparison.OrdinalIgnoreCase))
                {
                    string attributePK = (type == "INTEGER") ? "AUTOINCREMENT" : "";

                    return $"'{info.ColumnName}' {type} PRIMARY KEY {attributePK} UNIQUE";
                }

                return $"'{info.ColumnName}' {type}";
            }

            // Returns the constraint name for the specified column.
            string GetConstraintName(ColumnInfo column)
            {
                foreach (ConstraintInfo c in constraintInfo)
                {
                    if (c.IsTableColumn(column.TableName, column.ColumnName))
                    {
                        return c.ConstraintName;
                    }
                }

                return null;
            }

            // Converting data types from MS SQL to Sqlite types.
            string ConvertType(string type) => type.ToUpper() switch
            {
                "INT"
             or "INTEGER"
             or "BIT" => "INTEGER",
                "NVARCHAR"
             or "VARCHAR"
             or "DECIMAL"
             or "DATE" => "TEXT",
                "DOUBLE" => "REAL",
                _ => ""
            };
        }

        private void CreateTable()
        {
            using SqliteConnection connection = new(connectionString: string.Format(connectionStringMSQLite, TbxConnectionStringSQLite.Text));

            SqliteCommand command = new()
            {
                Connection = connection,
                CommandText = TbxScript.Text
            };

            try
            {
                connection.Open();

                int res = command.ExecuteNonQuery();
            }
            catch (SqliteException se)
            {
                MessageBox.Show(se.Message);

                return;
            }
            finally
            {
                connection.Close();
            }

            TbxScript.Text += "\nThe table was created successfully!\n";
        }


        private class ChangeData
        {
            private readonly Dictionary<string, List<string>> Values = new();

            private readonly string loremIpsumString;

            private readonly List<string> loremIpsumWords;

            private readonly List<string> cities;

            public ChangeData()
            {
                using (StreamReader sr = new StreamReader("loremIpsum.txt"))
                {
                    loremIpsumString = sr.ReadToEnd();
                }

                loremIpsumWords = loremIpsumString.Split(new[] { ' ', ',', '.', '\n', '\r' }).Where(s => s.Length > 2).Select(s => s.Trim()).ToList();

                cities = loremIpsumWords.Where(s => s.Length > 5).Select(s => s).ToList();
            }

            public string GetNewData(string columnName, string data)
            {
                Random rnd = new();
                string res;

                if (Values.ContainsKey(columnName) == false)
                {
                    Values.Add(columnName, new List<string>());
                }

                switch (columnName)
                {
                    case "NameCompany":

                        string[] sa = new string[rnd.Next(2, 5)];

                        while (true)
                        {
                            for (int i = 0; i != sa.Length; i++)
                            {
                                sa[i] = loremIpsumWords[rnd.Next(loremIpsumWords.Count - 1)];
                            }

                            res = string.Join(' ', sa);

                            res = res[..1].ToUpper() + res[1..].ToLower();

                            if (Values[columnName].Contains(res) == false)
                            {
                                Values[columnName].Add(res);

                                break;
                            }
                        }

                        return res;

                    case "UNP":

                        while (true)
                        {

                            res = rnd.Next(999999999).ToString("000000000");

                            if (Values[columnName].Contains(res) == false)
                            {
                                Values[columnName].Add(res);

                                break;
                            }
                        }

                        return res;

                    case "city":

                        res = cities[rnd.Next(cities.Count - 1)];

                        res = res[..1].ToUpper() + res[1..].ToLower();

                        return res;

                    case "region":

                        return "";

                    case "account":
                    case "account1":

                        while (true)
                        {

                            int n = rnd.Next(999999999);

                            res = $"BY{rnd.Next(99):00}{rnd.Next(999):000}{(char)rnd.Next((int)'A', (int)'Z')}" +
                                $"{(char)rnd.Next((int)'A', (int)'Z')}{(char)rnd.Next((int)'A', (int)'Z')}{n:000000000}0";

                            if (Values[columnName].Contains(res) == false)
                            {
                                Values[columnName].Add(res);

                                break;
                            }
                        }

                        return res;

                    case "phoneNumber":
                    case "fax":

                        while (true)
                        {

                            int n = rnd.Next(1000000, 9999999);

                            res = $"+{rnd.Next(999):###}-{rnd.Next(10, 99):(##)}-{n:###-##-##}";

                            if (Values[columnName].Contains(res) == false)
                            {
                                Values[columnName].Add(res);

                                break;
                            }
                        }

                        return res;

                    case "mail":

                        while (true)
                        {
                            res = $"{loremIpsumWords[rnd.Next(loremIpsumWords.Count - 1)]}@{loremIpsumWords[rnd.Next(loremIpsumWords.Count - 1)]}." +
                                  $"{loremIpsumWords.Where(s => s.Length <= 3).First()}";

                            if (Values[columnName].Contains(res) == false)
                            {
                                Values[columnName].Add(res);

                                break;
                            }
                        }

                        return res;

                    case "Id":

                        return data.ToString();

                    default:

                        return data;
                }
            }
        }

        /// <summary>
        /// Copying data from MS SQL to Sqlite database.
        /// </summary>
        private void CopyData(Func<string, string, string> changeData)
        {
            DataSet dataSet = new();

            // Loading data from the database.
            try
            {
                using SqlConnection MSSQLconnection = new(string.Format(connectionStringMSSql, TbxConnectionStringMSSQL.Text));

                MSSQLconnection.Open();

                foreach (string name in sqlTableName)
                {

                    SqlDataAdapter sqlDataAdapter = new($"SELECT * FROM {name};", MSSQLconnection);

                    DataTable dt = new DataTable(name);

                    sqlDataAdapter.Fill(dt);

                    dataSet.Tables.Add(dt);
                }

                MSSQLconnection.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);

                return;
            }

            // Writing data to the database.
            try
            {
                using SqliteConnection sqliteConnection = new(string.Format(connectionStringMSQLite, TbxConnectionStringSQLite.Text));

                SqliteCommand sqliteCommand = new();

                sqliteCommand.Connection = sqliteConnection;

                TbxScript.Text = "";

                sqliteConnection.Open();

                using (SqliteTransaction transaction = sqliteConnection.BeginTransaction())
                {
                    StringBuilder columns = new();
                    StringBuilder values = new();

                    sqliteCommand.Transaction = transaction;

                    foreach (DataTable dt in dataSet.Tables)
                    {
                        DateTime beginTime = DateTime.Now;

                        TbxScript.Text += $"\nStarting filling the database table {dt.TableName}";

                        columns.Clear();

                        string[] sa = new string[dt.Columns.Count];

                        // Set column names
                        for (int i = 0; i != dt.Columns.Count; i++)
                        {
                            sa[i] = dt.Columns[i].ColumnName;
                        }

                        columns.AppendJoin(',', sa);

                        foreach (DataRow row in dt.Rows)
                        {
                            // Set values
                            for (int i = 0; i != dt.Columns.Count; i++)
                            {
                                sa[i] = $"'{changeData(dt.Columns[i].ColumnName, GetDataSqliteType(row[i]))}'";
                            }

                            values.Clear();

                            values.AppendJoin(',', sa);

                            sqliteCommand.CommandText = $"INSERT INTO {dt.TableName}({columns}) VALUES({values})";

                            sqliteCommand.ExecuteNonQuery();
                        }

                        TimeSpan timeSpan = DateTime.Now - beginTime;

                        TbxScript.Text += $"\nAdded {dt.Rows.Count} records. Time = {timeSpan.TotalMilliseconds} ms\n";
                    }

                    transaction.Commit();
                }

                sqliteConnection.Close();

                BtnCopyTable.IsEnabled = false;

            }
            catch (SqliteException ex)
            {
                MessageBox.Show(ex.Message);
            }

            return;

            // Function for formatting values of some types.
            string GetDataSqliteType(object data)
            {
                Type type = data.GetType();

                if (type == typeof(DateTime))
                {
                    return ((DateTime)data).ToString("yyyy-MM-dd");
                }

                if (type == typeof(bool))
                {
                    return ((bool)data) ? "1" : "0";
                }

                if (type == typeof(decimal))
                {
                    return ((decimal)data).ToString("0.00#");
                }

                return data.ToString();
            }
        }

    }
}
