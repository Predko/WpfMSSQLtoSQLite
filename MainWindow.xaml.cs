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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_ClickDoImport(object sender, RoutedEventArgs e)
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

        private List<ColumnInfo> selectedRows;

        private void ImportDB(string MSSqlDbName, string MSQLiteDbName)
        {
            string connectionStringMSSql = $"Data Source=.\\SQLEXPRESS;Initial Catalog={MSSqlDbName};Integrated Security=True";
            string connectionStringMSQLite = $"Data Source=d:\\MyProgram\\Web\\Html\\Birdwatching\\assets\\Data\\{MSQLiteDbName}.sqlite";

            DataSet dataSet = new();

            using (SqlConnection MSSQLconnection = new(connectionStringMSSql))
            {
                string[] tableNames = { "Customers", "Contracts", "Income", "Expenses" };


                MSSQLconnection.Open();

                DataTable columnsTable = MSSQLconnection.GetSchema("Columns");

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


                // TABLE_CATALOG
                // TABLE_SCHEMA
                // TABLE_NAME
                // COLUMN_NAME
                // ORDINAL_POSITION -
                // COLUMN_DEFAULT
                // IS_NULLABLE
                // DATA_TYPE
                // CHARACTER_MAXIMUM_LENGTH
                // CHARACTER_OCTET_LENGTH
                // NUMERIC_PRECISION
                // NUMERIC_PRECISION_RADIX
                // NUMERIC_SCALE
                // DATETIME_PRECISION
                // CHARACTER_SET_CATALOG
                // CHARACTER_SET_SCHEMA
                // CHARACTER_SET_NAME
                // COLLATION_CATALOG
                // IS_SPARSE
                // IS_COLUMN_SET
                // IS_FILESTREAM

                //// Get the Meta Data for Supported Schema Collections
                //DataTable metaDataTable = MSSQLconnection.GetSchema("MetaDataCollections");

                //TbxWriteLine("Meta Data for Supported Schema Collections:");
                //ShowDataTable(metaDataTable, 25);

                //// Get the schema information of Databases in your instance
                //DataTable databasesSchemaTable = MSSQLconnection.GetSchema("Databases");

                //TbxWriteLine("Schema Information of Databases:");
                //ShowDataTable(databasesSchemaTable, 25);

                //DataTable schema = MSSQLconnection.GetSchema("tables"); TbxWriteLine("-----------------------------------------");

                //TbxWriteLine("Schema Information of All Tables:");

                //ShowDataTable(schema, 20); TbxWriteLine("-----------------------------------------");

                // First, get schema information of all the columns in current database.
                //DataTable allColumnsSchemaTable = MSSQLconnection.GetSchema("Columns");

                //ShowDataTable(allColumnsSchemaTable, 30); TbxWriteLine("-----------------------------------------");

                //TbxWriteLine("Schema Information of All Columns:");
                //ShowColumns(allColumnsSchemaTable); TbxWriteLine("-----------------------------------------");

                //// First, get schema information of all the IndexColumns in current database
                //DataTable allIndexColumnsSchemaTable = MSSQLconnection.GetSchema("IndexColumns");

                //ShowIndexColumns(allIndexColumnsSchemaTable); TbxWriteLine("-----------------------------------------");

                TbxNamesTables.Text = "Data loading completed!";
                
                return;

                SqlDataAdapter sqlDataAdapter = new("SELECT * FROM Customers;" +
                                                  "\nSELECT * FROM Contracts;" +
                                                  "\nSELECT * FROM Income;" +
                                                  "\nSELECT * FROM Expenses;", MSSQLconnection);

                sqlDataAdapter.Fill(dataSet);

                int j = 0;
                foreach (DataTable table in dataSet.Tables)
                {
                    table.TableName = tableNames[j++];

                    TbxNamesTables.Text += $"Table name: {table.TableName}" + $"\n\t\tRows count: {table.Rows.Count}\n";
                }
            }

            using (SqliteConnection connection = new(connectionStringMSQLite))
            {
                connection.Open();

                SqliteCommand command = new()
                {
                    Connection = connection,
                    CommandText = "CREATE TABLE Customers(" +
                    "Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
                    "NameCompany TEXT NOT NULL," +
                    "account TEXT NULL," +
                    "city TEXT NULL," +
                    "account1 TEXT NULL," +
                    "region TEXT NULL," +
                    "phoneNumber TEXT NULL," +
                    "fax TEXT NULL," +
                    "mail TEXT NULL," +
                    "file TEXT NULL," +
                    "UNP TEXT NULL)"
                };

                command.ExecuteNonQuery();
            }
        }


        private void TbxWrite(string s)
        {
            TbxNamesTables.Text += s;
        }

        private void TbxWrite(params string[] ps)
        {
            TbxNamesTables.Text += string.Format(ps[0], ps[1..]);
        }

        private void TbxWriteLine(params string[] ps)
        {
            string s = "";

            if (ps.Length != 0)
            {
                s = string.Format(ps[0], ps[1..]);
            }

            TbxNamesTables.Text += s + "\n";
        }

        private void ShowDataTable(DataTable table, Int32 length)
        {
            foreach (DataColumn col in table.Columns)
            {
                TbxWrite(string.Format("{0,-" + length + "}", col.ColumnName));
            }
            TbxWriteLine();

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (col.DataType.Equals(typeof(DateTime)))
                        TbxWrite(string.Format("{0,-" + length + ":d}", row[col]));
                    else if (col.DataType.Equals(typeof(Decimal)))
                        TbxWrite(string.Format("{0,-" + length + ":C}", row[col]));
                    else
                        TbxWrite(string.Format("{0,-" + length + "}", row[col]));
                }
                TbxWriteLine();
            }
        }

        private void ShowDataTable(DataTable table)
        {
            ShowDataTable(table, 14);
        }

        private void ShowColumns(DataTable columnsTable)
        {
            var selectedRows = from info in columnsTable.AsEnumerable()
                               select new
                               {
                                   TableCatalog = info["TABLE_CATALOG"],
                                   TableSchema = info["TABLE_SCHEMA"],
                                   TableName = info["TABLE_NAME"],
                                   ColumnName = info["COLUMN_NAME"],
                                   MaxLength = info["CHARACTER_MAXIMUM_LENGTH"],
                                   NumericPrecision = info["NUMERIC_PRECISION"],
                                   NumericScale = info["NUMERIC_SCALE"],
                                   DataType = info["DATA_TYPE"]
                               };

            TbxWriteLine($"{"TableCatalog",-15}{"TABLE_SCHEMA",-15}{"TABLE_NAME",-15}{"COLUMN_NAME",-20}{"CHARACTER_MAXIMUM_LENGTH",-15}{"NUMERIC_PRECISION",-15}{"NUMERIC_SCALE",-15}{"DATA_TYPE",-15}");
            foreach (var row in selectedRows)
            {
                TbxWriteLine($"{row.TableCatalog,-15}{row.TableSchema,-15}{row.TableName,-15}{row.ColumnName,-20}{row.MaxLength,-15}{row.NumericPrecision,-15}{row.NumericScale,-15}{row.DataType,-15}");
            }
        }

        private void ShowIndexColumns(DataTable indexColumnsTable)
        {
            var selectedRows = from info in indexColumnsTable.AsEnumerable()
                               select new
                               {
                                   TableSchema = info["table_schema"],
                                   TableName = info["table_name"],
                                   ColumnName = info["column_name"],
                                   ConstraintSchema = info["constraint_schema"],
                                   ConstraintName = info["constraint_name"],
                                   KeyType = info["KeyType"]
                               };

            TbxWriteLine($"{"table_schema",-14}{"table_name",-11}{"column_name",-14}{"constraint_schema",-18}{"constraint_name",-20}{"KeyType",-8}");
            foreach (var row in selectedRows)
            {
                TbxWriteLine($"{row.TableSchema,-14}{row.TableName,-11}{row.ColumnName,-14}{row.ConstraintSchema,-18}{row.ConstraintName,-20}{row.KeyType,-8}");
            }
        }

        private void Btn_ClickCreateTableScript(object sender, RoutedEventArgs e)
        {

            string createSQliteTable = "CREATE TABLE {0}(";
            string columnDefinition = "\n{0} {1} {2} {3}";

            string currentTable = "";

            for (int i = 0; i != selectedRows.Count; i++)
            {
                string s = (string)selectedRows[i].TableName;

                if (currentTable == "")
                {
                    currentTable = s;

                    TbxNamesTables.Text = string.Format(createSQliteTable, s);
                }

                if (s != currentTable)
                {
                    TbxNamesTables.Text += "\n);\n";

                    return;
                }

                TbxNamesTables.Text += string.Format(columnDefinition,
                                                        SetColumnName(selectedRows[i]),
                                                        ConvertType((string)selectedRows[i].DataType),
                                                        LengthDate(selectedRows[i]),
                                                        ((string)selectedRows[i].IsNullable == "NO") 
                                                                    ? "NOT NULL," 
                                                                    : "NULL,");
            }


            string SetColumnName(ColumnInfo info)
            {
                string name = (string)info.ColumnName;

                if (name.ToUpper().StartsWith("ID"))
                {
                    return $"{(string)info.ColumnName} PRIMARY KEY AUTOINCREMENT UNIQUE";
                }

                return (string)info.ColumnName;
            }

            string LengthDate(ColumnInfo info)
            {
                string type = ((string)(info.DataType)).ToUpper();
                
                if (type == "NVARCHAR" || type == "VARCHAR")
                {
                    return info.MaxLength.ToString();
                }

                return "";
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

        }
    }

    internal class ColumnInfo
    {
        public object TableCatalog { get; }
        public object TableName { get; }
        public object ColumnName { get; }
        public object DataType { get; }
        public object MaxLength { get; }
        public object ColumnDefault { get; }
        public object IsNullable { get; }
        public object NumericPrecision { get; }
        public object NumericScale { get; }

        public ColumnInfo(object tableCatalog, object tableName, object columnName, object dataType, object maxLength, object columnDefault, object isNullable, object numericPrecision, object numericScale)
        {
            TableCatalog = tableCatalog;
            TableName = tableName;
            ColumnName = columnName;
            DataType = dataType;
            MaxLength = maxLength;
            ColumnDefault = columnDefault;
            IsNullable = isNullable;
            NumericPrecision = numericPrecision;
            NumericScale = numericScale;
        }

        public override bool Equals(object obj)
        {
            return obj is ColumnInfo other &&
                   EqualityComparer<object>.Default.Equals(TableCatalog, other.TableCatalog) &&
                   EqualityComparer<object>.Default.Equals(TableName, other.TableName) &&
                   EqualityComparer<object>.Default.Equals(ColumnName, other.ColumnName) &&
                   EqualityComparer<object>.Default.Equals(DataType, other.DataType) &&
                   EqualityComparer<object>.Default.Equals(MaxLength, other.MaxLength) &&
                   EqualityComparer<object>.Default.Equals(ColumnDefault, other.ColumnDefault) &&
                   EqualityComparer<object>.Default.Equals(IsNullable, other.IsNullable) &&
                   EqualityComparer<object>.Default.Equals(NumericPrecision, other.NumericPrecision) &&
                   EqualityComparer<object>.Default.Equals(NumericScale, other.NumericScale);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(TableCatalog);
            hash.Add(TableName);
            hash.Add(ColumnName);
            hash.Add(DataType);
            hash.Add(MaxLength);
            hash.Add(ColumnDefault);
            hash.Add(IsNullable);
            hash.Add(NumericPrecision);
            hash.Add(NumericScale);
            return hash.ToHashCode();
        }
    }
}
