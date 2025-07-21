using System.Reflection;
namespace ThingsGateway.SqlSugar
{
    public class UpdateExpressionMethodInfo
    {
        internal SqlSugarProvider Context { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal object objectValue { get; set; }
        internal Type Type { get; set; }

        public int ExecuteCommand()
        {
            if (Context == null) return 0;
            var result = objectValue.GetType().GetMethod("ExecuteCommand").Invoke(objectValue, Array.Empty<object>());
            return (int)result;
        }

        public async Task<int> ExecuteCommandAsync()
        {
            if (Context == null) return 0;
            var result = objectValue.GetType().GetMyMethod("ExecuteCommandAsync", 0).Invoke(objectValue, Array.Empty<object>());
            return await ((Task<int>)result).ConfigureAwait(false);
        }

        public UpdateExpressionMethodInfo Where(string expShortName, FormattableString whereExpressionString)
        {
            var newMethod = objectValue.GetType().GetMyMethod("Where", 1);
            var exp = DynamicCoreHelper.GetWhere(Type, expShortName, whereExpressionString);
            var result = newMethod.Invoke(objectValue, new object[] { exp });
            return new UpdateExpressionMethodInfo()
            {
                objectValue = result,
                Type = this.Type,
                Context = this.Context
            };
        }
        public UpdateExpressionMethodInfo SetColumns(string expShortName, FormattableString fieldExpressionString)
        {
            var newMethod = objectValue.GetType().GetMethods()
                .Where(it =>
                {
                    var isTrue = it.Name == "SetColumns" && it.GetParameters().Length == 1;
                    if (isTrue)
                    {
                        return it.GetParameters().First().ToString().Contains(",System.Boolean");
                    }
                    return false;
                })
                .Single();
            var exp1 = DynamicCoreHelper.GetWhere(Type, expShortName, fieldExpressionString);
            var result = newMethod.Invoke(objectValue, new object[] { exp1 });
            return new UpdateExpressionMethodInfo()
            {
                objectValue = result,
                Type = this.Type,
                Context = this.Context
            };
        }
    }
}
