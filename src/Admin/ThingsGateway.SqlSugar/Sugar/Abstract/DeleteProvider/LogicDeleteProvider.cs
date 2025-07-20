namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 逻辑删除提供者
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class LogicDeleteProvider<T> where T : class, new()
    {
        /// <summary>
        /// 可删除提供者
        /// </summary>
        public DeleteableProvider<T> Deleteable { get; set; }
        /// <summary>
        /// 删除构建器
        /// </summary>
        public DeleteBuilder DeleteBuilder { get; set; }

        /// <summary>
        /// 执行逻辑删除命令
        /// </summary>
        /// <param name="LogicFieldName">逻辑字段名</param>
        /// <param name="deleteValue">删除值</param>
        /// <param name="deleteTimeFieldName">删除时间字段名</param>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand(string LogicFieldName = null, object deleteValue = null, string deleteTimeFieldName = null)
        {
            ISqlSugarClient db;
            List<SugarParameter> pars;
            string where;
            var isAutoDelFilter =
                DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true &&
                DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoUpdateQueryFilter == true;
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = false;
            }
            LogicFieldName = _ExecuteCommand(LogicFieldName, out db, out where, out pars);
            if (deleteValue == null)
            {
                deleteValue = true;
            }
            var updateable = db.Updateable<T>().SetColumns(LogicFieldName, deleteValue);
            if (deleteTimeFieldName != null)
            {
                updateable.SetColumns(deleteTimeFieldName, DateTime.Now);
            }
            if (pars != null)
                updateable.UpdateBuilder.Parameters.AddRange(pars);
            Convert(updateable as UpdateableProvider<T>);
            var result = updateable.Where(where).ExecuteCommand();
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = true;
            }
            return result;
        }

        /// <summary>
        /// 执行逻辑删除命令(带用户名)
        /// </summary>
        /// <param name="LogicFieldName">逻辑字段名</param>
        /// <param name="deleteValue">删除值</param>
        /// <param name="deleteTimeFieldName">删除时间字段名</param>
        /// <param name="userNameFieldName">用户名字段名</param>
        /// <param name="userNameValue">用户名值</param>
        /// <returns>影响的行数</returns>
        public int ExecuteCommand(string LogicFieldName, object deleteValue, string deleteTimeFieldName, string userNameFieldName, object userNameValue)
        {
            ISqlSugarClient db;
            List<SugarParameter> pars;
            string where;
            var isAutoDelFilter =
             DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true &&
             DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoUpdateQueryFilter == true;
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = false;
            }
            LogicFieldName = _ExecuteCommand(LogicFieldName, out db, out where, out pars);
            var updateable = db.Updateable<T>();
            updateable.UpdateBuilder.LambdaExpressions.ParameterIndex = 1000;
            updateable.SetColumns(LogicFieldName, deleteValue);
            updateable.SetColumns(deleteTimeFieldName, DateTime.Now);
            updateable.SetColumns(userNameFieldName, userNameValue);
            if (pars != null)
                updateable.UpdateBuilder.Parameters.AddRange(pars);
            Convert(updateable as UpdateableProvider<T>);
            var result = updateable.Where(where).ExecuteCommand();
            return result;
        }

        /// <summary>
        /// 异步执行逻辑删除命令(带用户名)
        /// </summary>
        /// <param name="LogicFieldName">逻辑字段名</param>
        /// <param name="deleteValue">删除值</param>
        /// <param name="deleteTimeFieldName">删除时间字段名</param>
        /// <param name="userNameFieldName">用户名字段名</param>
        /// <param name="userNameValue">用户名值</param>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync(string LogicFieldName, object deleteValue, string deleteTimeFieldName, string userNameFieldName, object userNameValue)
        {
            ISqlSugarClient db;
            List<SugarParameter> pars;
            string where;
            var isAutoDelFilter =
             DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true &&
             DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoUpdateQueryFilter == true;
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = false;
            }
            LogicFieldName = _ExecuteCommand(LogicFieldName, out db, out where, out pars);
            var updateable = db.Updateable<T>();
            updateable.UpdateBuilder.LambdaExpressions.ParameterIndex = 1000;
            updateable.SetColumns(LogicFieldName, deleteValue);
            updateable.SetColumns(deleteTimeFieldName, DateTime.Now);
            updateable.SetColumns(userNameFieldName, userNameValue);
            if (pars != null)
                updateable.UpdateBuilder.Parameters.AddRange(pars);
            Convert(updateable as UpdateableProvider<T>);
            var result = await updateable.Where(where).ExecuteCommandAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// 异步执行逻辑删除命令
        /// </summary>
        /// <param name="LogicFieldName">逻辑字段名</param>
        /// <param name="deleteValue">删除值</param>
        /// <param name="deleteTimeFieldName">删除时间字段名</param>
        /// <returns>影响的行数任务</returns>
        public async Task<int> ExecuteCommandAsync(string LogicFieldName = null, object deleteValue = null, string deleteTimeFieldName = null)
        {
            ISqlSugarClient db;
            List<SugarParameter> pars;
            string where;
            var isAutoDelFilter =
                DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoDeleteQueryFilter == true &&
                DeleteBuilder.Context?.CurrentConnectionConfig?.MoreSettings?.IsAutoUpdateQueryFilter == true;
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = false;
            }
            LogicFieldName = _ExecuteCommand(LogicFieldName, out db, out where, out pars);
            if (deleteValue == null)
            {
                deleteValue = true;
            }
            var updateable = db.Updateable<T>().SetColumns(LogicFieldName, deleteValue);
            if (deleteTimeFieldName != null)
            {
                updateable.SetColumns(deleteTimeFieldName, DateTime.Now);
            }
            if (pars != null)
                updateable.UpdateBuilder.Parameters.AddRange(pars);
            Convert(updateable as UpdateableProvider<T>);
            var result = await updateable.Where(where).ExecuteCommandAsync().ConfigureAwait(false);
            if (isAutoDelFilter)
            {
                DeleteBuilder.Context.CurrentConnectionConfig.MoreSettings.IsAutoUpdateQueryFilter = true;
            }
            return result;
        }

        /// <summary>
        /// 转换更新属性
        /// </summary>
        /// <param name="updateable">可更新提供者</param>
        private void Convert(UpdateableProvider<T> updateable)
        {
            updateable.IsEnableDiffLogEvent = Deleteable.IsEnableDiffLogEvent;
            updateable.DiffModel = Deleteable.DiffModel;
            updateable.UpdateBuilder.TableWithString = DeleteBuilder.TableWithString;
            updateable.RemoveCacheFunc = Deleteable.RemoveCacheFunc;
        }

        /// <summary>
        /// 执行逻辑删除命令的内部方法
        /// </summary>
        /// <param name="LogicFieldName">逻辑字段名</param>
        /// <param name="db">数据库客户端</param>
        /// <param name="where">条件语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>逻辑字段名</returns>
        private string _ExecuteCommand(string LogicFieldName, out ISqlSugarClient db, out string where, out List<SugarParameter> pars)
        {
            var entityInfo = Deleteable.EntityInfo;
            db = Deleteable.Context;
            if (DeleteBuilder.BigDataInValues?.Count > 0)
            {
                var sql = db.Queryable<T>().Select("1").AS(nameof(T)).In(DeleteBuilder.BigDataInValues).ToSqlString();
                var whereIndex = sql.IndexOf("  WHERE ");
                var whereItem = sql.Substring(whereIndex + 7);
                this.DeleteBuilder.WhereInfos.Add(whereItem);
            }

            Check.ExceptionEasy(DeleteBuilder.GetWhereString == null, "Logical Delete requires a Where condition", "逻辑删除需要加Where条件");

            where = DeleteBuilder.GetWhereString.Substring(5);
            pars = DeleteBuilder.Parameters;
            if (LogicFieldName.IsNullOrEmpty())
            {
                var column = entityInfo.Columns.FirstOrDefault(it =>
                it.PropertyName.EqualCase("isdelete") ||
                it.PropertyName.EqualCase("isdeleted") ||
                it.DbColumnName.EqualCase("isdelete") ||
                it.DbColumnName.EqualCase("isdeleted"));
                if (column != null)
                {
                    LogicFieldName = column.DbColumnName;
                }
            }
            Check.Exception(LogicFieldName == null, ErrorMessage.GetThrowMessage(
                 $"{entityInfo.EntityName} is not isdelete or isdeleted"
                , $"{entityInfo.EntityName} 没有IsDelete或者IsDeleted 的属性, 你也可以用 IsLogic().ExecuteCommand(\"列名\")"));
            return LogicFieldName;
        }
    }
}