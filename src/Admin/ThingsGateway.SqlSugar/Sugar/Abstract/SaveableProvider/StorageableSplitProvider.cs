using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class StorageableSplitProvider<T> where T : class, new()
    {
        internal Storageable<T> SaveInfo { get; set; }
        internal SqlSugarProvider Context { get; set; }
        internal List<T> List { get; set; }
        internal EntityInfo EntityInfo { get; set; }
        internal Expression<Func<T, object>> whereExpression { get; set; }

        internal int pageSize = 1000;
        internal Action<int> ActionCallBack = null;
        public StorageableSplitProvider<T> PageSize(int size, Action<int> ActionCallBack = null)
        {
            this.pageSize = size;
            return this;
        }
        public int ExecuteCommand()
        {
            if (List.Count > pageSize)
            {
                var result = 0;
                this.Context.Utilities.PageEach(List, pageSize, pageItem => result += _ExecuteCommand(pageItem));
                return result;
            }
            else
            {
                var list = List;
                return _ExecuteCommand(list);
            }
        }
        public int ExecuteSqlBulkCopy()
        {
            if (List.Count > pageSize)
            {
                var result = 0;
                this.Context.Utilities.PageEach(List, pageSize, pageItem => result += _ExecuteSqlBulkCopy(pageItem));
                return result;
            }
            else
            {
                var list = List;
                return _ExecuteSqlBulkCopy(list);
            }
        }

        public async Task<int> ExecuteCommandAsync()
        {
            if (List.Count > pageSize)
            {
                var result = 0;
                await Context.Utilities.PageEachAsync(List, pageSize, async pageItem =>
                {
                    result += await _ExecuteCommandAsync(pageItem).ConfigureAwait(false);
                    if (ActionCallBack != null)
                    {
                        ActionCallBack(result);
                    }
                }).ConfigureAwait(false);
                return result;
            }
            else
            {
                var list = List;
                return await _ExecuteCommandAsync(list).ConfigureAwait(false);
            }
        }
        public async Task<int> ExecuteSqlBulkCopyAsync()
        {
            if (List.Count > pageSize)
            {
                var result = 0;
                await Context.Utilities.PageEachAsync(List, pageSize, async pageItem =>
                {
                    result += await _ExecuteSqlBulkCopyAsync(pageItem).ConfigureAwait(false);
                    if (ActionCallBack != null)
                    {
                        ActionCallBack(result);
                    }
                }).ConfigureAwait(false);
                return result;
            }
            else
            {
                var list = List;
                return await _ExecuteSqlBulkCopyAsync(list).ConfigureAwait(false);
            }
        }

        private async Task<int> _ExecuteCommandAsync(IEnumerable<T> list)
        {
            int resultValue = 0;
            var groupModels = GroupDataList(list);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item);
                resultValue += await Context.Storageable(addList).AS(item.Key).WhereColumns(whereExpression).ExecuteCommandAsync().ConfigureAwait(false);
                if (ActionCallBack != null)
                {
                    ActionCallBack(resultValue);
                }
            }
            return resultValue;
        }
        private int _ExecuteCommand(IEnumerable<T> list)
        {
            int resultValue = 0;

            var groupModels = GroupDataList(list);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item);
                resultValue += this.Context.Storageable(addList).AS(item.Key).WhereColumns(whereExpression).ExecuteCommand();
            }
            return resultValue;
        }

        private async Task<int> _ExecuteSqlBulkCopyAsync(IEnumerable<T> list)
        {
            int resultValue = 0;
            var groupModels = GroupDataList(list);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item);
                resultValue += await Context.Storageable(addList).AS(item.Key).WhereColumns(whereExpression).ExecuteSqlBulkCopyAsync().ConfigureAwait(false);
                if (ActionCallBack != null)
                {
                    ActionCallBack(resultValue);
                }
            }
            return resultValue;
        }
        private int _ExecuteSqlBulkCopy(IEnumerable<T> list)
        {
            int resultValue = 0;

            var groupModels = GroupDataList(list);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                CreateTable(item.Key);
                var addList = item.Select(it => it.Item);
                resultValue += this.Context.Storageable(addList).AS(item.Key).WhereColumns(whereExpression).ExecuteSqlBulkCopy();
            }
            return resultValue;
        }

        private IEnumerable<GroupModel> GroupDataList(IEnumerable<T> datas)
        {
            var attribute = typeof(T).GetCustomAttribute<SplitTableAttribute>() as SplitTableAttribute;
            if (attribute == null) { throw new SqlSugarException($"{typeof(T).Name} need SplitTableAttribute"); }

            var db = this.Context;
            var context = db.SplitHelper<T>();

            foreach (var item in datas)
            {
                var value = context.GetValue(attribute.SplitType, item);
                var tableName = context.GetTableName(attribute.SplitType, value);
                yield return new GroupModel { GroupName = tableName, Item = item };
            }
        }

        /// <summary>创建分表</summary>
        private void CreateTable(string tableName)
        {
            if (tableName != null)
            {
                var isLog = this.Context.Ado.IsEnableLogEvent;
                this.Context.Ado.IsEnableLogEvent = false;
                if (!this.Context.DbMaintenance.IsAnyTable(tableName, false))
                {
                    this.Context.MappingTables.Add(EntityInfo.EntityName, tableName);
                    this.Context.CodeFirst.InitTables<T>();
                }
                this.Context.Ado.IsEnableLogEvent = isLog;
                this.Context.MappingTables.Add(EntityInfo.EntityName, EntityInfo.DbTableName);
            }
        }

        internal class GroupModel
        {
            public string GroupName { get; set; }
            public T Item { get; set; }
        }
    }
}
