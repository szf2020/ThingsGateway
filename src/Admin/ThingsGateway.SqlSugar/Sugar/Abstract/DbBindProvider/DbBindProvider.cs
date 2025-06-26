using System.Collections;
using System.Data;
using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 数据库绑定提供者抽象类
    /// </summary>
    public abstract partial class DbBindProvider : DbBindAccessory, IDbBind
    {
        #region Properties
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public virtual SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 数据库类型与C#类型映射列表
        /// </summary>
        public abstract List<KeyValuePair<string, CSharpDataType>> MappingTypes { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// 获取数据库类型名称
        /// </summary>
        /// <param name="csharpTypeName">C#类型名称</param>
        /// <returns>数据库类型名称</returns>
        public virtual string GetDbTypeName(string csharpTypeName)
        {
            if (csharpTypeName == UtilConstants.ByteArrayType.Name)
                return "varbinary";
            if (csharpTypeName.Equals("int32", StringComparison.CurrentCultureIgnoreCase))
                csharpTypeName = "int";
            if (csharpTypeName.Equals("int16", StringComparison.CurrentCultureIgnoreCase))
                csharpTypeName = "short";
            if (csharpTypeName.Equals("int64", StringComparison.CurrentCultureIgnoreCase))
                csharpTypeName = "long";
            if (csharpTypeName.IsInCase("boolean", "bool"))
                csharpTypeName = "bool";
            if (csharpTypeName == "DateTimeOffset")
                csharpTypeName = "DateTime";
            var mappings = this.MappingTypes.Where(it => it.Value.ToString().Equals(csharpTypeName, StringComparison.CurrentCultureIgnoreCase)).ToList();
            if (mappings?.Count > 0)
                return mappings.First().Key;
            else
                return "varchar";
        }

        /// <summary>
        /// 获取C#类型名称
        /// </summary>
        /// <param name="dbTypeName">数据库类型名称</param>
        /// <returns>C#类型名称</returns>
        public string GetCsharpTypeName(string dbTypeName)
        {
            var mappings = this.MappingTypes.Where(it => it.Key == dbTypeName);
            return mappings.HasValue() ? mappings.First().Key : "string";
        }

        /// <summary>
        /// 根据数据库类型名称获取C#类型名称
        /// </summary>
        /// <param name="dbTypeName">数据库类型名称</param>
        /// <returns>C#类型名称</returns>
        public string GetCsharpTypeNameByDbTypeName(string dbTypeName)
        {
            var mappings = this.MappingTypes.Where(it => it.Key == dbTypeName);
            if (mappings?.Any() != true)
            {
                return "string";
            }
            var result = mappings.First().Value.ObjToString();
            return result;
        }

        /// <summary>
        /// 获取类型转换字符串
        /// </summary>
        /// <param name="dbTypeName">数据库类型名称</param>
        /// <returns>转换字符串</returns>
        public virtual string GetConvertString(string dbTypeName)
        {
            string result = string.Empty;
            dbTypeName = dbTypeName.ToLower();
            switch (dbTypeName)
            {
                #region Int
                case "int":
                    result = "Convert.ToInt32";
                    break;
                #endregion

                #region String
                case "nchar":
                case "char":
                case "ntext":
                case "nvarchar":
                case "varchar":
                case "text":
                    result = "Convert.ToString";
                    break;
                #endregion

                #region Long
                case "bigint":
                    result = "Convert.ToInt64";
                    break;
                #endregion

                #region Bool
                case "bit":
                    result = "Convert.ToBoolean";
                    break;

                #endregion

                #region Datetime
                case "timestamp":
                case "smalldatetime":
                case "datetime":
                case "date":
                case "datetime2":
                    result = "Convert.ToDateTime";
                    break;
                #endregion

                #region Decimal
                case "smallmoney":
                case "single":
                case "numeric":
                case "money":
                case "decimal":
                    result = "Convert.ToDecimal";
                    break;
                #endregion

                #region Double
                case "float":
                    result = "Convert.ToSingle";
                    break;
                case "double":
                    result = "Convert.ToDouble";
                    break;
                #endregion

                #region Byte[]
                case "varbinary":
                case "binary":
                case "image":
                    result = "byte[]";
                    break;
                #endregion

                #region Float
                case "real":
                    result = "Convert.ToSingle";
                    break;
                #endregion

                #region Short
                case "smallint":
                    result = "Convert.ToInt16";
                    break;
                #endregion

                #region Byte
                case "tinyint":
                    result = "Convert.ToByte";
                    break;

                #endregion

                #region Guid
                case "uniqueidentifier":
                    result = "Guid.Parse";
                    break;
                #endregion

                #region Null
                default:
                    result = null;
                    break;
                    #endregion
            }
            return result;
        }

        /// <summary>
        /// 获取属性类型名称
        /// </summary>
        /// <param name="dbTypeName">数据库类型名称</param>
        /// <returns>属性类型名称</returns>
        public virtual string GetPropertyTypeName(string dbTypeName)
        {
            dbTypeName = dbTypeName.ToLower();
            var propertyTypes = MappingTypes.Where(it => it.Key.Equals(dbTypeName, StringComparison.CurrentCultureIgnoreCase));
            if (dbTypeName == "int32")
            {
                return "int";
            }
            else if (dbTypeName == "int64")
            {
                return "long";
            }
            else if (dbTypeName == "int16")
            {
                return "short";
            }
            else if (propertyTypes == null)
            {
                return "other";
            }
            else if (dbTypeName.IsContainsIn("xml", "string", "String"))
            {
                return "string";
            }
            else if (dbTypeName.IsContainsIn("boolean", "bool"))
            {
                return "bool";
            }
            else if (propertyTypes?.Any() != true)
            {
                return "object";
            }
            else if (propertyTypes.First().Value == CSharpDataType.byteArray)
            {
                return "byte[]";
            }
            else
            {
                return propertyTypes.First().Value.ToString();
            }
        }

        /// <summary>
        /// 将数据读取器转换为列表
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="type">实际类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual List<T> DataReaderToList<T>(Type type, IDataReader dataReader)
        {
            using (dataReader)
            {
                if (UtilMethods.IsKeyValuePairType(type))
                {
                    return GetKeyValueList<T>(type, dataReader);
                }
                else if (type.IsValueType || type == UtilConstants.StringType || type == UtilConstants.ByteArrayType)
                {
                    return GetValueTypeList<T>(type, dataReader);
                }
                else if (type.IsArray)
                {
                    return GetArrayList<T>(type, dataReader);
                }
                else if (typeof(T) != type && typeof(T).IsInterface)
                {
                    //这里是为了解决返回类型是接口的问题
                    return GetEntityListByType<T>(type, Context, dataReader);
                }
                else
                {
                    return GetEntityList<T>(Context, dataReader);
                }
            }
        }

        /// <summary>
        /// 异步将数据读取器转换为列表
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="type">实际类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual async Task<List<T>> DataReaderToListAsync<T>(Type type, IDataReader dataReader)
        {
            using (dataReader)
            {
                if (UtilMethods.IsKeyValuePairType(type))
                {
                    return await GetKeyValueListAsync<T>(type, dataReader).ConfigureAwait(false);
                }
                else if (type.IsValueType || type == UtilConstants.StringType || type == UtilConstants.ByteArrayType)
                {
                    return await GetValueTypeListAsync<T>(type, dataReader).ConfigureAwait(false);
                }
                else if (type.IsArray)
                {
                    return await GetArrayListAsync<T>(type, dataReader).ConfigureAwait(false);
                }
                else if (typeof(T) != type && typeof(T).IsInterface)
                {
                    //这里是为了解决返回类型是接口的问题
                    return await GetEntityListByTypeAsync<T>(type, Context, dataReader).ConfigureAwait(false);
                }
                else
                {
                    return await GetEntityListAsync<T>(Context, dataReader).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// 将数据读取器转换为列表(不使用using)
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="type">实际类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual List<T> DataReaderToListNoUsing<T>(Type type, IDataReader dataReader)
        {
            if (UtilMethods.IsKeyValuePairType(type))
            {
                return GetKeyValueList<T>(type, dataReader);
            }
            else if (type.IsValueType || type == UtilConstants.StringType || type == UtilConstants.ByteArrayType)
            {
                return GetValueTypeList<T>(type, dataReader);
            }
            else if (type.IsArray)
            {
                return GetArrayList<T>(type, dataReader);
            }
            else
            {
                return GetEntityList<T>(Context, dataReader);
            }
        }

        /// <summary>
        /// 异步将数据读取器转换为列表(不使用using)
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="type">实际类型</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual Task<List<T>> DataReaderToListNoUsingAsync<T>(Type type, IDataReader dataReader)
        {
            if (UtilMethods.IsKeyValuePairType(type))
            {
                return GetKeyValueListAsync<T>(type, dataReader);
            }
            else if (type.IsValueType || type == UtilConstants.StringType || type == UtilConstants.ByteArrayType)
            {
                return GetValueTypeListAsync<T>(type, dataReader);
            }
            else if (type.IsArray)
            {
                return GetArrayListAsync<T>(type, dataReader);
            }
            else
            {
                return GetEntityListAsync<T>(Context, dataReader);
            }
        }

        /// <summary>
        /// 根据类型获取实体列表
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="entityType">实体类型</param>
        /// <param name="context">SqlSugar提供者</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual List<T> GetEntityListByType<T>(Type entityType, SqlSugarProvider context, IDataReader dataReader)
        {
            var method = typeof(DbBindProvider).GetMethod("GetEntityList", BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(entityType);
            var objectValue = genericMethod.Invoke(this, new object[] { context, dataReader });
            List<T> result = new List<T>();
            foreach (var item in objectValue as IEnumerable)
            {
                result.Add((T)item);
            }
            return result;
        }

        /// <summary>
        /// 异步根据类型获取实体列表
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="entityType">实体类型</param>
        /// <param name="context">SqlSugar提供者</param>
        /// <param name="dataReader">数据读取器</param>
        /// <returns>实体列表</returns>
        public virtual async Task<List<T>> GetEntityListByTypeAsync<T>(Type entityType, SqlSugarProvider context, IDataReader dataReader)
        {
            var method = typeof(DbBindProvider).GetMethod("GetEntityListAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(entityType);
            Task task = (Task)genericMethod.Invoke(this, new object[] { context, dataReader });
            return await GetTask<T>(task).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取任务结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="task">任务</param>
        /// <returns>结果列表</returns>
        private static async Task<List<T>> GetTask<T>(Task task)
        {
            await task.ConfigureAwait(false); // 等待任务完成
            var resultProperty = task.GetType().GetProperty("Result");
            var value = resultProperty.GetValue(task);
            List<T> result = new List<T>();
            foreach (var item in value as IEnumerable)
            {
                result.Add((T)item);
            }
            return (List<T>)result;
        }
        #endregion

        #region Throw rule
        /// <summary>
        /// Int类型转换异常规则
        /// </summary>
        public virtual List<string> IntThrow
        {
            get
            {
                return new List<string>() { "datetime", "byte" };
            }
        }

        /// <summary>
        /// Short类型转换异常规则
        /// </summary>
        public virtual List<string> ShortThrow
        {
            get
            {
                return new List<string>() { "datetime", "guid" };
            }
        }

        /// <summary>
        /// Decimal类型转换异常规则
        /// </summary>
        public virtual List<string> DecimalThrow
        {
            get
            {
                return new List<string>() { "datetime", "byte", "guid" };
            }
        }

        /// <summary>
        /// Double类型转换异常规则
        /// </summary>
        public virtual List<string> DoubleThrow
        {
            get
            {
                return new List<string>() { "datetime", "byte", "guid" };
            }
        }

        /// <summary>
        /// Date类型转换异常规则
        /// </summary>
        public virtual List<string> DateThrow
        {
            get
            {
                return new List<string>() { "int32", "decimal", "double", "byte", "guid" };
            }
        }

        /// <summary>
        /// Guid类型转换异常规则
        /// </summary>
        public virtual List<string> GuidThrow
        {
            get
            {
                return new List<string>() { "int32", "datetime", "decimal", "double", "byte" };
            }
        }

        /// <summary>
        /// String类型转换异常规则
        /// </summary>
        public virtual List<string> StringThrow
        {
            get
            {
                return new List<string>() { "int32", "datetime", "decimal", "double", "byte", "int64", "uint32", "uint64" };
            }
        }
        #endregion
    }
}