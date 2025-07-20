namespace ThingsGateway.SqlSugar
{
    public class OscarInserttable<T> : InsertableProvider<T> where T : class, new()
    {
        public override int ExecuteReturnIdentity()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var result = Ado.GetScalar(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters).ObjToInt();
            return result;
        }
        public override async Task<int> ExecuteReturnIdentityAsync()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var obj = await Ado.GetScalarAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters).ConfigureAwait(false);
            var result = obj.ObjToInt();
            return result;
        }
        public override KeyValuePair<string, IReadOnlyList<SugarParameter>> ToSql()
        {
            var result = base.ToSql();
            return new KeyValuePair<string, IReadOnlyList<SugarParameter>>(result.Key.Replace("$PrimaryKey", GetPrimaryKeys().FirstOrDefault()), result.Value);
        }

        public override long ExecuteReturnBigIdentity()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var result = Convert.ToInt64(Ado.GetScalar(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters.ToArray()) ?? "0");
            return result;
        }
        public override async Task<long> ExecuteReturnBigIdentityAsync()
        {
            InsertBuilder.IsReturnIdentity = true;
            PreToSql();
            string sql = InsertBuilder.ToSqlString().Replace("$PrimaryKey", this.SqlBuilder.GetTranslationColumnName(GetIdentityKeys().FirstOrDefault()));
            RestoreMapping();
            var result = Convert.ToInt64(await Ado.GetScalarAsync(sql, InsertBuilder.Parameters == null ? null : InsertBuilder.Parameters).ConfigureAwait(false) ?? "0");
            return result;
        }

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
            typeof(T).GetProperties().First(t => t.Name.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)).SetValue(result, setValue, null);
            return idValue > 0;
        }
    }
}
