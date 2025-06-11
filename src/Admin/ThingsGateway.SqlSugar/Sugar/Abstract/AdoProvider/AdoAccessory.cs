using System.Data;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    public partial class AdoAccessory
    {
        // 数据库类型绑定接口
        protected IDbBind _DbBind;
        // 数据库反向生成实体接口
        protected IDbFirst _DbFirst;
        // 数据库建库接口
        protected ICodeFirst _CodeFirst;
        // 数据库维护接口
        protected IDbMaintenance _DbMaintenance;
        // 数据库连接接口
        protected IDbConnection _DbConnection;

        /// <summary>
        /// 将对象类型的参数转换为 SugarParameter 数组
        /// </summary>
        /// <param name="parameters">参数对象，可以是匿名对象、字典、SugarParameter 数组或列表</param>
        /// <param name="propertyInfo">可选的属性信息数组，若为空则通过反射获取</param>
        /// <param name="sqlParameterKeyWord">SQL参数前缀，例如 "@"</param>
        /// <returns>SugarParameter 数组</returns>
        protected virtual SugarParameter[] GetParameters(object parameters, PropertyInfo[] propertyInfo, string sqlParameterKeyWord)
        {
            // 最终要返回的参数集合
            List<SugarParameter> result = new List<SugarParameter>();

            if (parameters != null)
            {
                var entityType = parameters.GetType();

                // 判断是否是字典类型，利用 UtilConstants.DicArraySO / DicArraySS 判断
                var isDictionary = entityType.IsIn(UtilConstants.DicArraySO, UtilConstants.DicArraySS);

                if (isDictionary)
                    // 字典类型转参数
                    DictionaryToParameters(parameters, sqlParameterKeyWord, result, entityType);
                else if (parameters is List<SugarParameter> sugarParamList)
                {
                    // 如果已经是 SugarParameter 列表，直接赋值
                    result = sugarParamList;
                }
                else if (parameters is SugarParameter[] sugarParamArray)
                {
                    // 如果是 SugarParameter 数组，转 List
                    result = sugarParamArray.ToList();
                }
                else
                {
                    // 如果不是匿名对象，抛异常
                    Check.Exception(!entityType.IsAnonymousType(),
                        "参数格式错误。\n请使用 new{xx=xx, xx2=xx2} 或 \nDictionary<string, object> 或 \nSugarParameter [] ");

                    // 反射对象属性转参数
                    ProperyToParameter(parameters, propertyInfo, sqlParameterKeyWord, result, entityType);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将对象的属性转换为参数集合
        /// </summary>
        protected void ProperyToParameter(object parameters, PropertyInfo[] propertyInfo, string sqlParameterKeyWord, List<SugarParameter> listParams, Type entityType)
        {
            PropertyInfo[] properties = propertyInfo ?? entityType.GetProperties();

            foreach (PropertyInfo properyty in properties)
            {
                var value = properyty.GetValue(parameters, null);

                // 如果是枚举类型，转成 long
                if (properyty.PropertyType.IsEnum())
                    value = Convert.ToInt64(value);

                // DateTime.MinValue 处理为 DBNull
                if (value?.Equals(DateTime.MinValue) != false)
                    value = DBNull.Value;

                // 特殊处理 HIERARCHYID 类型
                if (properyty.Name.Contains("hierarchyid", StringComparison.CurrentCultureIgnoreCase))
                {
                    var parameter = new SugarParameter(sqlParameterKeyWord + properyty.Name, SqlDbType.Udt)
                    {
                        UdtTypeName = "HIERARCHYID",
                        Value = value
                    };
                    listParams.Add(parameter);
                }
                else
                {
                    // 常规参数
                    var parameter = new SugarParameter(sqlParameterKeyWord + properyty.Name, value);
                    listParams.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 将字典类型的参数转换为参数集合
        /// </summary>
        protected void DictionaryToParameters(object parameters, string sqlParameterKeyWord, List<SugarParameter> listParams, Type entityType)
        {
            if (entityType == UtilConstants.DicArraySO)
            {
                // Dictionary<string, object> 转参数
                var dictionaryParameters = (Dictionary<string, object>)parameters;
                var sugarParameters = dictionaryParameters.Select(it => new SugarParameter(sqlParameterKeyWord + it.Key, it.Value));
                listParams.AddRange(sugarParameters);
            }
            else
            {
                // Dictionary<string, string> 转参数
                var dictionaryParameters = (Dictionary<string, string>)parameters;
                var sugarParameters = dictionaryParameters.Select(it => new SugarParameter(sqlParameterKeyWord + it.Key, it.Value));
                listParams.AddRange(sugarParameters);
            }
        }
    }
}
