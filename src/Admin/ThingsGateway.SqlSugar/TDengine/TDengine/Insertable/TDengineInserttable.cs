namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 可插入数据提供类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class TDengineInsertable<T> : InsertableProvider<T> where T : class, new()
    {
        /// <summary>
        /// 执行插入并返回自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public override int ExecuteReturnIdentity()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string identityColumn = GetIdentityColumn();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(identityColumn));
            RestoreMapping();
            var result = Ado.GetScalar(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ObjToInt();
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 异步执行插入并返回自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public override async Task<int> ExecuteReturnIdentityAsync()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string identityColumn = GetIdentityColumn();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(identityColumn));
            RestoreMapping();
            var obj = await Ado.GetScalarAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ConfigureAwait(false);
            var result = obj.ObjToInt();
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 生成SQL语句和参数
        /// </summary>
        /// <returns>SQL语句和参数</returns>
        public override KeyValuePair<string, List<SugarParameter>> ToSql()
        {
            var result = base.ToSql();
            var primaryKey = GetPrimaryKeys().FirstOrDefault();
            if (primaryKey != null)
            {
                primaryKey = this.SqlBuilder.GetTranslationColumnName(primaryKey);
            }
            return new KeyValuePair<string, List<SugarParameter>>(result.Key.Replace("$PrimaryKey", primaryKey), result.Value);
        }

        /// <summary>
        /// 执行插入并返回大整数自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public override long ExecuteReturnBigIdentity()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var result = Convert.ToInt64(Ado.GetScalar(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()) ?? "0");
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 异步执行插入并返回大整数自增ID
        /// </summary>
        /// <returns>自增ID</returns>
        public override async Task<long> ExecuteReturnBigIdentityAsync()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var result = Convert.ToInt64(await Ado.GetScalarAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()).ConfigureAwait(false) ?? "0");
            After(sql, result);
            return result;
        }

        /// <summary>
        /// 执行插入并将自增ID设置到实体中
        /// </summary>
        /// <returns>是否成功</returns>
        public override bool ExecuteCommandIdentityIntoEntity()
        {
            var result = InsertObjs[0];
            var identityKeys = GetIdentityKeys();
            if (identityKeys.Count == 0) { return this.ExecuteCommand() > 0; }
            var idValue = ExecuteReturnBigIdentity();
            Check.Exception(identityKeys.Count > 1, "ExecuteCommandIdentityIntoEntity does not support multiple identity keys");
            var identityKey = identityKeys.First();
            object setValue = 0;
            if (idValue > int.MaxValue)
                setValue = idValue;
            else
                setValue = Convert.ToInt32(idValue);
            var propertyName = this.Context.EntityMaintenance.GetPropertyName<T>(identityKey);
            typeof(T).GetProperties().First(t => string.Equals(t.Name, propertyName, StringComparison.OrdinalIgnoreCase)).SetValue(result, setValue, null);
            return idValue > 0;
        }

        /// <summary>
        /// 获取标识列名称
        /// </summary>
        /// <returns>标识列名称</returns>
        private string GetIdentityColumn()
        {
            var identityColumn = GetIdentityKeys().FirstOrDefault();
            if (identityColumn == null)
            {
                var columns = this.Context.DbMaintenance.GetColumnInfosByTableName(InsertBuilder.GetTableNameString);
                identityColumn = columns.First(it => it.IsIdentity || it.IsPrimarykey).DbColumnName;
            }
            return identityColumn;
        }
    }
}