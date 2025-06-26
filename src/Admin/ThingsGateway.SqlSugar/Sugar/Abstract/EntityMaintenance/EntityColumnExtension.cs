using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 实体列扩展方法
    /// </summary>
    public static class EntityColumnExtension
    {
        /// <summary>
        /// 判断是否为指定表实体列
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entityColumnInfo">实体列信息</param>
        /// <returns>实体列操作对象</returns>
        public static EntityColumnable<T> IfTable<T>(this EntityColumnInfo entityColumnInfo)
        {
            EntityColumnable<T> result = new EntityColumnable<T>();
            result.entityColumnInfo = entityColumnInfo;
            result.IsTable = entityColumnInfo.EntityName == typeof(T).Name;
            return result;
        }
    }

    /// <summary>
    /// 实体列操作类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class EntityColumnable<T>
    {
        /// <summary>
        /// 实体列信息
        /// </summary>
        public EntityColumnInfo entityColumnInfo { get; set; }

        /// <summary>
        /// 是否属于当前表
        /// </summary>
        public bool IsTable { get; set; }

        /// <summary>
        /// 更新属性配置
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="updateAction">更新操作</param>
        /// <returns>当前实例</returns>
        public EntityColumnable<T> UpdateProperty(Expression<Func<T, object>> propertyExpression, Action<EntityColumnInfo> updateAction)
        {
            var name = ExpressionTool.GetMemberName(propertyExpression);
            if (entityColumnInfo.PropertyName == name && IsTable)
            {
                updateAction(entityColumnInfo);
            }
            return this;
        }

        /// <summary>
        /// 配置一对一关系
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="firstName">关联属性名</param>
        /// <param name="lastName">关联属性名(可选)</param>
        /// <returns>当前实例</returns>
        public EntityColumnable<T> OneToOne(Expression<Func<T, object>> propertyExpression, string firstName, string lastName = null)
        {
            var name = ExpressionTool.GetMemberName(propertyExpression);
            if (entityColumnInfo.PropertyName == name && IsTable)
            {
                entityColumnInfo.Navigat = new Navigate(NavigateType.OneToOne, firstName, lastName);
                entityColumnInfo.IsIgnore = true;
            }
            return this;
        }

        /// <summary>
        /// 配置一对多关系
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="firstName">关联属性名</param>
        /// <param name="lastName">关联属性名</param>
        /// <returns>当前实例</returns>
        public EntityColumnable<T> OneToMany(Expression<Func<T, object>> propertyExpression, string firstName, string lastName)
        {
            var name = ExpressionTool.GetMemberName(propertyExpression);
            if (entityColumnInfo.PropertyName == name && IsTable)
            {
                entityColumnInfo.Navigat = new Navigate(NavigateType.OneToMany, firstName, lastName);
                entityColumnInfo.IsIgnore = true;
            }
            return this;
        }

        /// <summary>
        /// 配置多对多关系
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <param name="mapppingType">中间表类型</param>
        /// <param name="mapppingTypeAid">中间表AID</param>
        /// <param name="mapppingTypeBid">中间表BID</param>
        /// <returns>当前实例</returns>
        public EntityColumnable<T> ManyToMany(Expression<Func<T, object>> propertyExpression, Type mapppingType, string mapppingTypeAid, string mapppingTypeBid)
        {
            var name = ExpressionTool.GetMemberName(propertyExpression);
            if (entityColumnInfo.PropertyName == name && IsTable)
            {
                entityColumnInfo.Navigat = new Navigate(mapppingType, mapppingTypeAid, mapppingTypeBid);
                entityColumnInfo.IsIgnore = true;
            }
            return this;
        }
    }
}
