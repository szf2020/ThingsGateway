using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 插入导航方法信息类
    /// </summary>
    public class InsertNavMethodInfo
    {
        /// <summary>
        /// 方法信息集合
        /// </summary>
        internal object MethodInfos { get; set; }
        /// <summary>
        /// SqlSugar提供者上下文
        /// </summary>
        internal SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 通过名称字符串包含导航属性
        /// </summary>
        public InsertNavMethodInfo IncludeByNameString(string navMemberName, InsertNavOptions insertNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[0];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, insertNavOptions });
            this.MethodInfos = obj;
            return this;
        }

        /// <summary>
        /// 通过名称字符串包含后续导航属性
        /// </summary>
        public InsertNavMethodInfo ThenIncludeByNameString(string navMemberName, InsertNavOptions insertNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[1];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("ThenInclude", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, insertNavOptions });
            this.MethodInfos = obj;
            return this;
        }

        /// <summary>
        /// 异步执行插入命令
        /// </summary>
        public async Task<bool> ExecuteCommandAsync()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommandAsync").Invoke(MethodInfos, Array.Empty<object>());
            return await ((Task<bool>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行插入命令
        /// </summary>
        public bool ExecuteCommand()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommand").Invoke(MethodInfos, Array.Empty<object>());
            return (bool)result;
        }
    }
}