namespace WpfMSSQLtoSQLite
{
    internal class ConstraintInfo
    {
        public string TableName { get; }
        public string ColumnName { get; }
        public string ConstraintName { get; }
        public int KeyType { get; }

        public ConstraintInfo(object tableName, object columnName, object constraintName, object keyType)
        {
            TableName = ColumnInfo.ConvertFromDBValToObject(tableName).ToString();
            ColumnName = ColumnInfo. ConvertFromDBValToObject(columnName).ToString();
            ConstraintName = ColumnInfo. ConvertFromDBValToObject(constraintName).ToString();
            KeyType = (byte)(ColumnInfo.ConvertFromDBValToObject(keyType) ?? 0);
        }

        public bool IsTableColumn(string tableName, string columnName)
        {
            if (TableName == tableName && ColumnName == columnName)
            {
                return true;
            }

            return false;
        }
    }
}