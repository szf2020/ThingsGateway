using System.Linq.Expressions;

namespace SqlSugar
{
    public class UpdateNavMethodInfo
    {
        internal object MethodInfos { get; set; }
        internal SqlSugarProvider Context { get; set; }

        public UpdateNavMethodInfo IncludeByNameString(string navMemberName, UpdateNavOptions updateNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[0];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type properyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out properyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(properyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, updateNavOptions });
            this.MethodInfos = obj;
            return this;
        }
        public UpdateNavMethodInfo ThenIncludeByNameString(string navMemberName, UpdateNavOptions updateNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[1];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type properyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out properyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("ThenInclude", 2, isList)
                            .MakeGenericMethod(properyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, updateNavOptions });
            this.MethodInfos = obj;
            return this;
        }
        public async Task<bool> ExecuteCommandAsync()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommandAsync").Invoke(MethodInfos, Array.Empty<object>());
            return await ((Task<bool>)result).ConfigureAwait(false);
        }
        public bool ExecuteCommand()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommand").Invoke(MethodInfos, Array.Empty<object>());
            return (bool)result;
        }
    }
}
