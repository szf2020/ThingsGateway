using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 删除导航方法信息类
    /// </summary>
    public class DeleteNavMethodInfo
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
        /// <param name="navMemberName">导航成员名称</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航方法信息</returns>
        public DeleteNavMethodInfo IncludeByNameString(string navMemberName, DeleteNavOptions deleteNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[0];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("Include", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, deleteNavOptions });
            this.MethodInfos = obj;
            return this;
        }

        /// <summary>
        /// 然后通过名称字符串包含导航属性
        /// </summary>
        /// <param name="navMemberName">导航成员名称</param>
        /// <param name="deleteNavOptions">删除导航选项</param>
        /// <returns>删除导航方法信息</returns>
        public DeleteNavMethodInfo ThenIncludeByNameString(string navMemberName, DeleteNavOptions deleteNavOptions = null)
        {
            var type = MethodInfos.GetType().GetGenericArguments()[1];
            var entityInfo = this.Context.EntityMaintenance.GetEntityInfo(type);
            Type propertyItemType;
            bool isList;
            Expression exp = UtilMethods.GetIncludeExpression(navMemberName, entityInfo, out propertyItemType, out isList);
            var method = this.MethodInfos.GetType().GetMyMethod("ThenInclude", 2, isList)
                            .MakeGenericMethod(propertyItemType);
            var obj = method.Invoke(this.MethodInfos, new object[] { exp, deleteNavOptions });
            this.MethodInfos = obj;
            return this;
        }

        /// <summary>
        /// 异步执行删除命令
        /// </summary>
        /// <returns>是否成功</returns>
        public async Task<bool> ExecuteCommandAsync()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommandAsync").Invoke(MethodInfos, Array.Empty<object>());
            return await ((Task<bool>)result).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行删除命令
        /// </summary>
        /// <returns>是否成功</returns>
        public bool ExecuteCommand()
        {
            if (Context == null) return false;
            var result = MethodInfos.GetType().GetMethod("ExecuteCommand").Invoke(MethodInfos, Array.Empty<object>());
            return (bool)result;
        }
    }
}