using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    public interface ISubInsertable<T>
    {
        ISubInsertable<T> AddSubList(Expression<Func<T, object>> items);
        ISubInsertable<T> AddSubList(Expression<Func<T, SubInsertTree>> tree);
        object ExecuteCommand();
        Task<object> ExecuteCommandAsync();
    }
}