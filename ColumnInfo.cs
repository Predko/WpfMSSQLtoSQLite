using System;
using System.Collections.Generic;

namespace WpfMSSQLtoSQLite
{
    internal class ColumnInfo
    {
        public string TableCatalog { get; }
        public string TableName { get; }
        public string ColumnName { get; }
        public string DataType { get; }
        public int? MaxLength { get; }
        public string ColumnDefault { get; }
        public string IsNullable { get; }
        public string NumericPrecision { get; }
        public string NumericScale { get; }

        public ColumnInfo(object tableCatalog,
                          object tableName,
                          object columnName,
                          object dataType,
                          object maxLength,
                          object columnDefault,
                          object isNullable,
                          object numericPrecision,
                          object numericScale)
        {
            TableCatalog = ConvertFromDBValToObject(tableCatalog)?.ToString();
            TableName = ConvertFromDBValToObject(tableName)?.ToString();
            ColumnName = ConvertFromDBValToObject(columnName)?.ToString();
            DataType = ConvertFromDBValToObject(dataType)?.ToString();
            
            //if (int.TryParse(maxLength ?? "0", out int ml) == false)
            //{
            //    ml = 0;
            //}

            MaxLength = (int)(ConvertFromDBValToObject(maxLength) ?? 0);

            ColumnDefault = ConvertFromDBValToObject(columnDefault)?.ToString();
            IsNullable = ConvertFromDBValToObject(isNullable)?.ToString();
            NumericPrecision = ConvertFromDBValToObject(numericPrecision)?.ToString();
            NumericScale = ConvertFromDBValToObject(numericScale)?.ToString();
        }

        public static object ConvertFromDBValToObject(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default; // returns the default value for the type
            }
            else
            {
                return obj;
            }
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
