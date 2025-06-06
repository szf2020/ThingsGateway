namespace SqlSugar
{
    public interface IParameterInsertable<T>
    {
        int ExecuteCommand();
        Task<int> ExecuteCommandAsync();
    }
}