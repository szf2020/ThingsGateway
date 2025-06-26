using System.Reflection;
using System.Reflection.Emit;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 动态构建器类
    /// </summary>
    public partial class DynamicBuilder
    {
        /// <summary>
        /// 属性元数据列表
        /// </summary>
        internal List<PropertyMetadata> propertyAttr = new List<PropertyMetadata>();

        /// <summary>
        /// 实体属性构建器列表
        /// </summary>
        internal List<CustomAttributeBuilder> entityAttr = new List<CustomAttributeBuilder>();

        /// <summary>
        /// 实体名称
        /// </summary>
        internal string entityName { get; set; }

        /// <summary>
        /// 基类类型
        /// </summary>
        internal Type baseType = null;

        /// <summary>
        /// 接口类型数组
        /// </summary>
        internal Type[] interfaces = null;

        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        internal SqlSugarProvider context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">SqlSugar提供者实例</param>
        public DynamicBuilder(SqlSugarProvider context)
        {
            this.context = context;
        }

        /// <summary>
        /// 创建类
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="table">SugarTable实例</param>
        /// <param name="baseType">基类类型</param>
        /// <param name="interfaces">接口类型数组</param>
        /// <param name="splitTableAttribute">分表属性</param>
        /// <returns>动态属性构建器</returns>
        public DynamicProperyBuilder CreateClass(string entityName, SugarTable table = null, Type baseType = null, Type[] interfaces = null, SplitTableAttribute splitTableAttribute = null)
        {
            this.baseType = baseType;
            this.interfaces = interfaces;
            this.entityName = entityName;
            if (table == null)
            {
                table = new SugarTable() { TableName = entityName };
            }
            this.entityAttr = new List<CustomAttributeBuilder>() { GetEntity(table) };
            if (splitTableAttribute != null)
            {
                this.entityAttr.Add(GetSplitEntityAttr(splitTableAttribute));
            }
            return new DynamicProperyBuilder() { baseBuilder = this };
        }

        /// <summary>
        /// 根据类型创建对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="dict">属性字典</param>
        /// <returns>创建的对象实例</returns>
        public object CreateObjectByType(Type type, Dictionary<string, object> dict)
        {
            // 创建一个默认的空对象
            object obj = Activator.CreateInstance(type);

            // 遍历字典中的每个 key-value 对
            foreach (KeyValuePair<string, object> pair in dict)
            {
                // 获取对象中的属性
                PropertyInfo propertyInfo = type.GetProperty(pair.Key);

                if (propertyInfo == null)
                {
                    propertyInfo = type.GetProperties().FirstOrDefault(it => it.Name.EqualCase(pair.Key));
                }

                propertyInfo?.SetValue(obj, UtilMethods.ChangeType2(pair.Value, propertyInfo.PropertyType));
            }

            // 返回创建的对象
            return obj;
        }

        /// <summary>
        /// 根据类型批量创建对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="dictList">属性字典列表</param>
        /// <returns>创建的对象列表</returns>
        public List<object> CreateObjectByType(Type type, List<Dictionary<string, object>> dictList)
        {
            List<object> result = new List<object>();
            foreach (var item in dictList)
            {
                result.Add(CreateObjectByType(type, item));
            }
            return result;
        }
    }
}
