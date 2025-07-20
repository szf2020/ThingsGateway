using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public class SplitTableUpdateByObjectProvider<T> where T : class, new()
    {
        public SqlSugarProvider Context;
        public UpdateableProvider<T> updateobj;
        public IReadOnlyList<T> UpdateObjects { get; set; }

        public IEnumerable<SplitTableInfo> Tables { get; set; }
        internal List<string> WhereColumns { get; set; }
        internal bool IsEnableDiffLogEvent { get; set; }
        internal object BusinessData { get; set; }

        public int ExecuteCommandWithOptLock(bool isThrowError = false)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(UpdateObjects, out groupModels, out result);
            var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
            this.Context.Aop.DataExecuting = null;
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                if (IsVersion())
                {
                    Check.ExceptionEasy(addList.Count > 1, "The version number can only be used for single record updates", "版本号只能用于单条记录更新");
                    result += this.Context.UpdateableT(addList.First())
                    .WhereColumns(this.WhereColumns)
                    .EnableDiffLogEventIF(this.IsEnableDiffLogEvent, this.BusinessData)
                    .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                    .IgnoreColumns(this.updateobj.UpdateBuilder.IsNoUpdateNull, this.updateobj.UpdateBuilder.IsOffIdentity, this.updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                    .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommandWithOptLock(isThrowError);
                }
                else
                {
                    result += this.Context.Updateable(addList)
                        .WhereColumns(this.WhereColumns)
                        .EnableDiffLogEventIF(this.IsEnableDiffLogEvent, this.BusinessData)
                        .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                        .IgnoreColumns(this.updateobj.UpdateBuilder.IsNoUpdateNull, this.updateobj.UpdateBuilder.IsOffIdentity, this.updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                        .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommandWithOptLock(isThrowError);
                }
            }
            this.Context.Aop.DataExecuting = dataEvent;
            return result;
        }
        public int ExecuteCommand()
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(UpdateObjects, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
                this.Context.Aop.DataExecuting = null;
                result += this.Context.Updateable(addList)
                    .EnableDiffLogEventIF(this.IsEnableDiffLogEvent, this.BusinessData)
                    .WhereColumns(this.WhereColumns)
                    .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                    .IgnoreColumns(this.updateobj.UpdateBuilder.IsNoUpdateNull, this.updateobj.UpdateBuilder.IsOffIdentity, this.updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                    .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommand();
                this.Context.Aop.DataExecuting = dataEvent;
            }
            return result;
        }


        public async Task<int> ExecuteCommandAsync()
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(UpdateObjects, out groupModels, out result);
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
                this.Context.Aop.DataExecuting = null;
                result += await Context.Updateable(addList)
                    .WhereColumns(WhereColumns)
                    .EnableDiffLogEventIF(IsEnableDiffLogEvent, BusinessData)
                    .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                    .IgnoreColumns(updateobj.UpdateBuilder.IsNoUpdateNull, updateobj.UpdateBuilder.IsOffIdentity, updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                    .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommandAsync().ConfigureAwait(false);
                this.Context.Aop.DataExecuting = dataEvent;
            }
            return result;
        }
        public async Task<int> ExecuteCommandWithOptLockAsync(bool isThrowError = false)
        {
            List<GroupModel> groupModels;
            int result;
            GroupDataList(UpdateObjects, out groupModels, out result);
            var dataEvent = this.Context.CurrentConnectionConfig.AopEvents?.DataExecuting;
            this.Context.Aop.DataExecuting = null;
            foreach (var item in groupModels.GroupBy(it => it.GroupName))
            {
                var addList = item.Select(it => it.Item).ToList();
                if (IsVersion())
                {
                    Check.ExceptionEasy(addList.Count > 1, "The version number can only be used for single record updates", "版本号只能用于单条记录更新");
                    result += await Context.UpdateableT(addList.First())
                      .WhereColumns(WhereColumns)
                      .EnableDiffLogEventIF(IsEnableDiffLogEvent, BusinessData)
                      .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                      .IgnoreColumns(updateobj.UpdateBuilder.IsNoUpdateNull, updateobj.UpdateBuilder.IsOffIdentity, updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                      .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommandWithOptLockAsync(isThrowError).ConfigureAwait(false);

                }
                else
                {
                    result += await Context.Updateable(addList)
                        .WhereColumns(WhereColumns)
                        .EnableDiffLogEventIF(IsEnableDiffLogEvent, BusinessData)
                        .UpdateColumns(updateobj.UpdateBuilder.UpdateColumns)
                        .IgnoreColumns(updateobj.UpdateBuilder.IsNoUpdateNull, updateobj.UpdateBuilder.IsOffIdentity, updateobj.UpdateBuilder.IsNoUpdateDefaultValue)
                        .IgnoreColumns(GetIgnoreColumns()).AS(item.Key).ExecuteCommandWithOptLockAsync(isThrowError).ConfigureAwait(false);
                }
            }
            this.Context.Aop.DataExecuting = dataEvent;
            return result;
        }
        private string[] GetIgnoreColumns()
        {
            if (this.updateobj.UpdateBuilder.DbColumnInfoList.Count != 0)
            {
                var columns = this.updateobj.UpdateBuilder.DbColumnInfoList.Select(it => it.DbColumnName).ToHashSet();
                var result = this.Context.EntityMaintenance.GetEntityInfo<T>().Columns.Where(x => !columns.Any(y => y.EqualCase(x.DbColumnName))).Select(it => it.DbColumnName).Where(it => !string.IsNullOrEmpty(it)).ToArray();
                return result;
            }
            else
            {
                return null;
            }
        }
        private void GroupDataList(IReadOnlyList<T> datas, out List<GroupModel> groupModels, out int result)
        {
            var attribute = typeof(T).GetCustomAttribute<SplitTableAttribute>() as SplitTableAttribute;
            Check.Exception(attribute == null, $"{typeof(T).Name} need SplitTableAttribute");
            groupModels = new List<GroupModel>();
            var db = this.Context;
            foreach (var item in datas)
            {
                var value = db.SplitHelper<T>().GetValue(attribute.SplitType, item);
                var tableName = db.SplitHelper<T>().GetTableName(attribute.SplitType, value);
                groupModels.Add(new GroupModel() { GroupName = tableName, Item = item });
            }
            result = 0;
        }
        private bool IsVersion()
        {
            return this.Context.EntityMaintenance.GetEntityInfo<T>().Columns.Any(it => it.IsEnableUpdateVersionValidation);
        }

        internal class GroupModel
        {
            public string GroupName { get; set; }
            public T Item { get; set; }
        }
    }
}
