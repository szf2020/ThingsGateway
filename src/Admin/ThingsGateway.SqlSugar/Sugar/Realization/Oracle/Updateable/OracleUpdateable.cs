namespace ThingsGateway.SqlSugar
{
    public class OracleUpdateable<T> : UpdateableProvider<T> where T : class, new()
    {
        protected override List<string> GetIdentityKeys()
        {
            return this.EntityInfo.Columns.Where(it => it.OracleSequenceName.HasValue()).Select(it => it.DbColumnName).ToList();
        }
        public override int ExecuteCommand()
        {
            if (base.UpdateObjs.Length == 1)
            {
                var resultl = base.ExecuteCommand();
                if (resultl == -1)
                {
                    return 1;
                }
                else
                {
                    return resultl;
                }
            }
            else if (base.UpdateObjs.Length == 0)
            {
                return 0;
            }
            else
            {
                base.ExecuteCommand();
                return base.UpdateObjs.Length;
            }
        }
        public async override Task<int> ExecuteCommandAsync()
        {
            if (base.UpdateObjs.Length == 1)
            {
                var result = await base.ExecuteCommandAsync().ConfigureAwait(false);
                if (result == -1)
                {
                    return 1;
                }
                else
                {
                    return result;
                }
            }
            else if (base.UpdateObjs.Length == 0)
            {
                return 0;
            }
            else
            {
                await base.ExecuteCommandAsync().ConfigureAwait(false);
                return base.UpdateObjs.Length;
            }
        }
    }
}
