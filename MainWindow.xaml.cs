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

namespace WpfMSSQLtoSQLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ColumnInfo> selectedRows;

        private List<ConstraintInfo> constraintInfo;

        private List<string> sqlTableName;

        private string connectionStringMSSql = @"Data Source=.\SQLEXPRESS;Initial Catalog={0};Integrated Security=True";

        private string connectionStringMSQLite = @"Data Source=d:\MyProgram\Web\Html\Birdwatching\assets\Data\{0}.sqlite";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_ClickDatabaseInfo(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbxConnectionStringMSSQL.Text))
            {
                MessageBox.Show("Введите имя базы данных MS SQL");
                return;
            }

            if (string.IsNullOrWhiteSpace(TbxConnectionStringSQLite.Text))
            {
                MessageBox.Show("Введите имя базы данных SQLite");
                return;
            }

            ImportDB(TbxConnectionStringMSSQL.Text, TbxConnectionStringSQLite.Text);
        }


        private void ImportDB(string MSSqlDbName, string MSQLiteDbName)
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

            selectedRows = (from info in columnsTable.AsEnumerable()
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

            TbxScript.Text = "Data loading completed!";

            BtnCreateScript.IsEnabled = true;

            BtnCopyTable.IsEnabled = true;
        }

        private int beginIndex = 0;

        private void Btn_ClickCreateTableScript(object sender, RoutedEventArgs e)
        {

            string createSQliteTable = "CREATE TABLE '{0}' (";

            string currentTable = "";

            string currentColumn = "";

            string nextTable = "";

            for (int i = beginIndex; i != selectedRows.Count; i++)
            {
                nextTable = selectedRows[i].TableName;

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

                currentColumn = string.Format("\n{0} {1}", SetColumnName(selectedRows[i]),
                                                            (selectedRows[i].IsNullable == "NO")
                                                                    ? "NOT NULL"
                                                                    : "NULL");
            }

            TbxScript.Text += $"{currentColumn}\n);\n";

            BtnCreateTable.IsEnabled = true;

            BtnCreateTable.Content = $"Создать таблицу {currentTable}";

            BtnCreateScript.Content = $"Создать скрипт для таблицы {nextTable}";

            return;

            string SetColumnName(ColumnInfo info)
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

            string GetConstraintName(ColumnInfo column)
            {
                foreach (ConstraintInfo c in constraintInfo)
                {
                    if (c.IsTableColumn(column.TableName, column.ColumnName) == true)
                    {
                        return c.ConstraintName;
                    }
                }

                return null;
            }

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

        private void Btn_ClickCreateTable(object sender, RoutedEventArgs e)
        {
            using SqliteConnection connection = new(connectionString: string.Format(connectionStringMSQLite, TbxConnectionStringSQLite.Text));
            connection.Open();

            SqliteCommand command = new()
            {
                Connection = connection,
                CommandText = TbxScript.Text
            };

            try
            {
                int res = command.ExecuteNonQuery();
            }
            catch (SqliteException se)
            {
                MessageBox.Show(se.Message);

                return;
            }

            TbxScript.Text += "\nThe table was created successfully!\n";
        }

        private void TbxDbName_LostFocus(object sender, RoutedEventArgs e)
        {
            EnableBtnLoadAndCreate();
        }

        private void EnableBtnLoadAndCreate()
        {
            if (string.IsNullOrWhiteSpace(TbxConnectionStringMSSQL.Text) == false && string.IsNullOrWhiteSpace(TbxConnectionStringSQLite.Text) == false)
            {
                BtnLoadDb.IsEnabled = true;
            }
            else
            {
                BtnLoadDb.IsEnabled = false;
            }
        }

        private void BtnExitProgram(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TbxConnectionStringMSSQL.LostFocus += new(TbxDbName_LostFocus);
            TbxConnectionStringSQLite.LostFocus += new(TbxDbName_LostFocus);

            EnableBtnLoadAndCreate();
        }

        private void Btn_ClickCopyTable(object sender, RoutedEventArgs e)
        {
            CopyData();
        }

        private void CopyData()
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

                        for (int i = 0; i != dt.Columns.Count; i++)
                        {
                            sa[i] = dt.Columns[i].ColumnName;
                        }

                        columns.AppendJoin(',', sa);

                        foreach (DataRow row in dt.Rows)
                        {
                            for (int i = 0; i != dt.Columns.Count; i++)
                            {
                                sa[i] = $"'{GetDataSqliteType(row[i])}'";
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
                    return ((decimal)data).ToString("0.00#"); //##########################
                }

                return data.ToString();
            }
        }

    }
}
