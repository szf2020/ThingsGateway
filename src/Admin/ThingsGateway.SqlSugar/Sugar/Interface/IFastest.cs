using System.Data;

namespace ThingsGateway.SqlSugar
{
    public interface IFastest<T> where T : class, new()
    {
        IFastest<T> IgnoreInsertError();
        IFastest<T> RemoveDataCache();
        IFastest<T> RemoveDataCache(string cacheKey);
        IFastest<T> AS(string tableName);
        IFastest<T> PageSize(int Size);
        IFastest<T> OffIdentity();
        IFastest<T> SetCharacterSet(string CharacterSet);
        IFastest<T> EnableDataAop();
        int BulkCopy(IEnumerable<T> datas);
        Task<int> BulkCopyAsync(IEnumerable<T> datas);
        int BulkCopy(string tableName, DataTable dataTable);
        int BulkCopy(DataTable dataTable);
        Task<int> BulkCopyAsync(string tableName, DataTable dataTable);
        Task<int> BulkCopyAsync(DataTable dataTable);

        int BulkUpdate(IEnumerable<T> datas);
        Task<int> BulkUpdateAsync(IEnumerable<T> datas);
        int BulkUpdate(IEnumerable<T> datas, string[] whereColumns, string[] updateColumns);
        int BulkUpdate(IEnumerable<T> datas, string[] whereColumns);
        Task<int> BulkUpdateAsync(IEnumerable<T> datas, string[] whereColumns);
        Task<int> BulkUpdateAsync(IEnumerable<T> datas, string[] whereColumns, string[] updateColumns);
        int BulkUpdate(string tableName, DataTable dataTable, string[] whereColumns, string[] updateColumns);
        int BulkUpdate(DataTable dataTable, string[] whereColumns, string[] updateColumns);
        int BulkUpdate(DataTable dataTable, string[] whereColumns);
        Task<int> BulkUpdateAsync(string tableName, DataTable dataTable, string[] whereColumns, string[] updateColumns);
        Task<int> BulkUpdateAsync(DataTable dataTable, string[] whereColumns);
        SplitFastest<T> SplitTable();
        Task<int> BulkMergeAsync(IEnumerable<T> datas);
        int BulkMerge(IEnumerable<T> datas);
        int BulkMerge(DataTable dataTable, string[] whereColumns, bool isIdentity);
        Task<int> BulkMergeAsync(DataTable dataTable, string[] whereColumns, bool isIdentity);
        int BulkMerge(DataTable dataTable, string[] whereColumns, string[] updateColumns, bool isIdentity);
        Task<int> BulkMergeAsync(DataTable dataTable, string[] whereColumns, string[] updateColumns, bool isIdentity);
        Task<int> BulkMergeAsync(IEnumerable<T> datas, string[] whereColumns);
        int BulkMerge(IEnumerable<T> datas, string[] whereColumns);
        Task<int> BulkMergeAsync(IEnumerable<T> datas, string[] whereColumns, string[] updateColumns);
        int BulkMerge(IEnumerable<T> datas, string[] whereColumns, string[] updateColumns);
    }
}
