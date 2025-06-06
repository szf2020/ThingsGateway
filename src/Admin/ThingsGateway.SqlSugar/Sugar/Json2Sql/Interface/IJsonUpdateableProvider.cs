namespace SqlSugar
{
    public interface IJsonUpdateableProvider<T> : IJsonProvider<T>
    {
        // IJsonQueryableProvider<T> UpdateColumns(string tableName, string[] columns);
    }
}
