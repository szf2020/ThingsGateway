using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// IDataReader实体构建器
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public partial class IDataReaderEntityBuilder<T>
    {
        #region Properies
        /// <summary>
        /// 读取器字段名集合
        /// </summary>
        private HashSet<string> ReaderKeys { get; set; }
        #endregion

        #region Fields
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        private SqlSugarProvider Context = null;
        /// <summary>
        /// 动态构建器实例
        /// </summary>
        private IDataReaderEntityBuilder<T> DynamicBuilder;
        /// <summary>
        /// 数据记录器
        /// </summary>
        private IDataRecord DataRecord;
        /// <summary>
        /// IsDBNull方法
        /// </summary>
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        /// <summary>
        /// GetBoolean方法
        /// </summary>
        private static readonly MethodInfo getBoolean = typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) });
        /// <summary>
        /// GetByte方法
        /// </summary>
        private static readonly MethodInfo getByte = typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) });
        /// <summary>
        /// GetDateTime方法
        /// </summary>
        private static readonly MethodInfo getDateTime = typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) });
        /// <summary>
        /// GetDecimal方法
        /// </summary>
        private static readonly MethodInfo getDecimal = typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) });
        /// <summary>
        /// GetDouble方法
        /// </summary>
        private static readonly MethodInfo getDouble = typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) });
        /// <summary>
        /// GetFloat方法
        /// </summary>
        private static readonly MethodInfo getFloat = typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) });
        /// <summary>
        /// GetGuid方法
        /// </summary>
        private static readonly MethodInfo getGuid = typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) });
        /// <summary>
        /// GetInt16方法
        /// </summary>
        private static readonly MethodInfo getInt16 = typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) });
        /// <summary>
        /// GetInt32方法
        /// </summary>
        private static readonly MethodInfo getInt32 = typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) });
        /// <summary>
        /// GetInt64方法
        /// </summary>
        private static readonly MethodInfo getInt64 = typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) });
        /// <summary>
        /// GetString方法
        /// </summary>
        private static readonly MethodInfo getString = typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) });
        /// <summary>
        /// Getdatetimeoffset方法
        /// </summary>
        private static readonly MethodInfo getdatetimeoffset = typeof(IDataRecordExtensions).GetMethod("Getdatetimeoffset");
        /// <summary>
        /// GetdatetimeoffsetDate方法
        /// </summary>
        private static readonly MethodInfo getdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod("GetdatetimeoffsetDate");
        /// <summary>
        /// GetStringGuid方法
        /// </summary>
        private static readonly MethodInfo getStringGuid = typeof(IDataRecordExtensions).GetMethod("GetStringGuid");
        /// <summary>
        /// GetXelement方法
        /// </summary>
        private static readonly MethodInfo getXelement = typeof(IDataRecordExtensions).GetMethod("GetXelement");
        /// <summary>
        /// GetConvertStringGuid方法
        /// </summary>
        private static readonly MethodInfo getConvertStringGuid = typeof(IDataRecordExtensions).GetMethod("GetConvertStringGuid");
        /// <summary>
        /// GetEnum方法
        /// </summary>
        private static readonly MethodInfo getEnum = typeof(IDataRecordExtensions).GetMethod("GetEnum");
        /// <summary>
        /// GetConvertString方法
        /// </summary>
        private static readonly MethodInfo getConvertString = typeof(IDataRecordExtensions).GetMethod("GetConvertString");
        /// <summary>
        /// GetConvertFloat方法
        /// </summary>
        private static readonly MethodInfo getConvertFloat = typeof(IDataRecordExtensions).GetMethod("GetConvertFloat");
        /// <summary>
        /// GetConvertBoolean方法
        /// </summary>
        private static readonly MethodInfo getConvertBoolean = typeof(IDataRecordExtensions).GetMethod("GetConvertBoolean");
        /// <summary>
        /// GetConvertByte方法
        /// </summary>
        private static readonly MethodInfo getConvertByte = typeof(IDataRecordExtensions).GetMethod("GetConvertByte");
        /// <summary>
        /// GetConvertChar方法
        /// </summary>
        private static readonly MethodInfo getConvertChar = typeof(IDataRecordExtensions).GetMethod("GetConvertChar");
        /// <summary>
        /// GetConvertDateTime方法
        /// </summary>
        private static readonly MethodInfo getConvertDateTime = typeof(IDataRecordExtensions).GetMethod("GetConvertDateTime");
        /// <summary>
        /// GetConvertTime方法
        /// </summary>
        private static readonly MethodInfo getConvertTime = typeof(IDataRecordExtensions).GetMethod("GetConvertTime");
        /// <summary>
        /// GetTime方法
        /// </summary>
        private static readonly MethodInfo getTime = typeof(IDataRecordExtensions).GetMethod("GetTime");
        /// <summary>
        /// GetConvertDecimal方法
        /// </summary>
        private static readonly MethodInfo getConvertDecimal = typeof(IDataRecordExtensions).GetMethod("GetConvertDecimal");
        /// <summary>
        /// GetConvertDouble方法
        /// </summary>
        private static readonly MethodInfo getConvertDouble = typeof(IDataRecordExtensions).GetMethod("GetConvertDouble");
        /// <summary>
        /// GetConvertDoubleToFloat方法
        /// </summary>
        private static readonly MethodInfo getConvertDoubleToFloat = typeof(IDataRecordExtensions).GetMethod("GetConvertDoubleToFloat");
        /// <summary>
        /// GetConvertGuid方法
        /// </summary>
        private static readonly MethodInfo getConvertGuid = typeof(IDataRecordExtensions).GetMethod("GetConvertGuid");
        /// <summary>
        /// GetConvertInt16方法
        /// </summary>
        private static readonly MethodInfo getConvertInt16 = typeof(IDataRecordExtensions).GetMethod("GetConvertInt16");
        /// <summary>
        /// GetConvertInt32方法
        /// </summary>
        private static readonly MethodInfo getConvertInt32 = typeof(IDataRecordExtensions).GetMethod("GetConvertInt32");
        /// <summary>
        /// GetConvetInt64方法
        /// </summary>
        private static readonly MethodInfo getConvertInt64 = typeof(IDataRecordExtensions).GetMethod("GetConvetInt64");
        /// <summary>
        /// GetConvertEnum_Null方法
        /// </summary>
        private static readonly MethodInfo getConvertEnum_Null = typeof(IDataRecordExtensions).GetMethod("GetConvertEnum_Null");
        /// <summary>
        /// GetConvertdatetimeoffset方法
        /// </summary>
        private static readonly MethodInfo getConvertdatetimeoffset = typeof(IDataRecordExtensions).GetMethod("GetConvertdatetimeoffset");
        /// <summary>
        /// GetConvertdatetimeoffsetDate方法
        /// </summary>
        private static readonly MethodInfo getConvertdatetimeoffsetDate = typeof(IDataRecordExtensions).GetMethod("GetConvertdatetimeoffsetDate");
        /// <summary>
        /// GetOtherNull方法
        /// </summary>
        private static readonly MethodInfo getOtherNull = typeof(IDataRecordExtensions).GetMethod("GetOtherNull");
        /// <summary>
        /// GetOther方法
        /// </summary>
        private static readonly MethodInfo getOther = typeof(IDataRecordExtensions).GetMethod("GetOther");
        /// <summary>
        /// GetJson方法
        /// </summary>
        private static readonly MethodInfo getJson = typeof(IDataRecordExtensions).GetMethod("GetJson");
        /// <summary>
        /// GetArray方法
        /// </summary>
        private static readonly MethodInfo getArray = typeof(IDataRecordExtensions).GetMethod("GetArray");
        /// <summary>
        /// GetEntity方法
        /// </summary>
        private static readonly MethodInfo getEntity = typeof(IDataRecordExtensions).GetMethod("GetEntity", new Type[] { typeof(SqlSugarProvider) });
        /// <summary>
        /// GetMyIntNull方法
        /// </summary>
        private static readonly MethodInfo getMyIntNull = typeof(IDataRecordExtensions).GetMethod("GetMyIntNull");
        /// <summary>
        /// GetMyInt方法
        /// </summary>
        private static readonly MethodInfo getMyInt = typeof(IDataRecordExtensions).GetMethod("GetMyInt");

        /// <summary>
        /// 加载委托
        /// </summary>
        private delegate T Load(IDataRecord dataRecord);
        /// <summary>
        /// 处理程序
        /// </summary>
        private Load handler;
        #endregion

        #region Constructor
        /// <summary>
        /// 私有构造函数
        /// </summary>
        private IDataReaderEntityBuilder()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">SqlSugar提供者</param>
        /// <param name="dataRecord">数据记录器</param>
        /// <param name="fieldNames">字段名列表</param>
        public IDataReaderEntityBuilder(SqlSugarProvider context, IDataRecord dataRecord, List<string> fieldNames)
        {
            this.Context = context;
            this.DataRecord = dataRecord;
            this.DynamicBuilder = new IDataReaderEntityBuilder<T>();
            this.ReaderKeys = fieldNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// 构建实体
        /// </summary>
        /// <param name="dataRecord">数据记录器</param>
        /// <returns>实体对象</returns>
        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        /// <summary>
        /// 创建构建器
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>构建器实例</returns>
        public IDataReaderEntityBuilder<T> CreateBuilder(Type type)
        {
            DynamicMethod method = new DynamicMethod("SqlSugarEntity", type,
            new Type[] { typeof(IDataRecord) }, type, true);
            ILGenerator generator = method.GetILGenerator();
            LocalBuilder result = generator.DeclareLocal(type);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                 null, Type.EmptyTypes, null));
            generator.Emit(OpCodes.Stloc, result);
            this.Context.InitMappingInfo(type);
            var columnInfos = this.Context.EntityMaintenance.GetEntityInfoWithAttr(type).Columns;
            foreach (var columnInfo in columnInfos)
            {
                string fileName = columnInfo.DbColumnName ?? columnInfo.PropertyName;
                if (columnInfo.IsIgnore && !this.ReaderKeys.Contains(fileName))
                {
                    continue;
                }
                else if (columnInfo.ForOwnsOnePropertyInfo != null)
                {
                    continue;
                }
                if (columnInfo?.PropertyInfo.GetSetMethod(true) != null)
                {
                    var isGemo = columnInfo.PropertyInfo?.PropertyType?.FullName == "NetTopologySuite.Geometries.Geometry";
                    if (isGemo == false && columnInfo.PropertyInfo?.PropertyType?.FullName == "Kdbndp.LegacyPostgis.PostgisGeometry")
                    {
                        isGemo = true;
                    }
                    if (!isGemo && columnInfo.PropertyInfo.PropertyType.IsClass() && columnInfo.PropertyInfo.PropertyType != UtilConstants.ByteArrayType && columnInfo.PropertyInfo.PropertyType != UtilConstants.ObjType)
                    {
                        if (this.ReaderKeys.Contains(fileName))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                        else if (this.ReaderKeys.Any(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                    else if (!isGemo && columnInfo.IsJson && columnInfo.PropertyInfo.PropertyType != UtilConstants.StringType)
                    {   //json is struct
                        if (this.ReaderKeys.Contains(fileName))
                        {
                            BindClass(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                    else
                    {
                        if (this.ReaderKeys.Contains(fileName))
                        {
                            BindField(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                        else if (this.ReaderKeys.Any(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            BindField(generator, result, columnInfo, ReaderKeys.First(it => it.Equals(columnInfo.PropertyName, StringComparison.CurrentCultureIgnoreCase)));
                        }
                    }
                }
            }
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);
            DynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return DynamicBuilder;
        }

        #endregion

        #region Private methods
        /// <summary>
        /// 绑定自定义函数
        /// </summary>
        /// <param name="generator">IL生成器</param>
        /// <param name="result">本地变量</param>
        /// <param name="columnInfo">列信息</param>
        /// <param name="fieldName">字段名</param>
        private void BindCustomFunc(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName)
        {
            int i = DataRecord.GetOrdinal(fieldName);
            Label endIfLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, isDBNullMethod);
            generator.Emit(OpCodes.Brtrue, endIfLabel);
            generator.Emit(OpCodes.Ldloc, result);
            var method = (columnInfo.SqlParameterDbType as Type).GetMethod("QueryConverter");
            method = method.MakeGenericMethod(new Type[] { columnInfo.PropertyInfo.PropertyType });
            Type type = (columnInfo.SqlParameterDbType as Type);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            if (method.IsVirtual)
                generator.Emit(OpCodes.Callvirt, method);
            else
                generator.Emit(OpCodes.Call, method);
            generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
            generator.MarkLabel(endIfLabel);
        }

        /// <summary>
        /// 绑定类类型
        /// </summary>
        /// <param name="generator">IL生成器</param>
        /// <param name="result">本地变量</param>
        /// <param name="columnInfo">列信息</param>
        /// <param name="fieldName">字段名</param>
        private void BindClass(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName)
        {
            if (columnInfo.SqlParameterDbType is Type)
            {
                BindCustomFunc(generator, result, columnInfo, fieldName);
                return;
            }

            if (columnInfo.IsJson)
            {
                MethodInfo jsonMethod = getJson.MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                var insertBuilder = InstanceFactory.GetInsertBuilder(this.Context?.CurrentConnectionConfig);
                if (insertBuilder?.DeserializeObjectFunc != null)
                {
                    if (IDataRecordExtensions.DeserializeObjectFunc == null)
                    {
                        IDataRecordExtensions.DeserializeObjectFunc = insertBuilder.DeserializeObjectFunc;
                    }
                    jsonMethod = typeof(IDataRecordExtensions).GetMethod("GetDeserializeObject").MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                }
                generator.Emit(OpCodes.Call, jsonMethod);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            }
            if (columnInfo.IsArray)
            {
                MethodInfo arrayMehtod = getArray.MakeGenericMethod(columnInfo.PropertyInfo.PropertyType);
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Call, arrayMehtod);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            }
            else if (columnInfo.UnderType == typeof(XElement))
            {
                int i = DataRecord.GetOrdinal(fieldName);
                Label endIfLabel = generator.DefineLabel();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                generator.Emit(OpCodes.Brtrue, endIfLabel);
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldc_I4, i);
                BindMethod(generator, columnInfo, i);
                generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
                generator.MarkLabel(endIfLabel);
            }
        }

        /// <summary>
        /// 绑定字段
        /// </summary>
        /// <param name="generator">IL生成器</param>
        /// <param name="result">本地变量</param>
        /// <param name="columnInfo">列信息</param>
        /// <param name="fieldName">字段名</param>
        private void BindField(ILGenerator generator, LocalBuilder result, EntityColumnInfo columnInfo, string fieldName)
        {
            if (columnInfo.SqlParameterDbType is Type)
            {
                BindCustomFunc(generator, result, columnInfo, fieldName);
                return;
            }
            int i = DataRecord.GetOrdinal(fieldName);
            Label endIfLabel = generator.DefineLabel();

            //2023-3-8
            Label tryStart = generator.BeginExceptionBlock();//begin try
            //2023-3-8 

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Callvirt, isDBNullMethod);
            generator.Emit(OpCodes.Brtrue, endIfLabel);
            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, i);
            BindMethod(generator, columnInfo, i);
            generator.Emit(OpCodes.Callvirt, columnInfo.PropertyInfo.GetSetMethod(true));
            generator.MarkLabel(endIfLabel);

            //2023-3-8
            generator.Emit(OpCodes.Leave, tryStart);//eng try
            generator.BeginCatchBlock(typeof(Exception));//begin catch
            generator.Emit(OpCodes.Ldstr, ErrorMessage.GetThrowMessage($"{columnInfo.EntityName} {columnInfo.PropertyName} bind error", $"{columnInfo.PropertyName}绑定到{columnInfo.EntityName}失败,可以试着换一个类型，或者使用ORM自定义类型实现"));//thow message
            generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[] { typeof(string) }));
            generator.Emit(OpCodes.Throw);
            generator.EndExceptionBlock();
            //2023-3-8
        }

        /// <summary>
        /// 绑定方法
        /// </summary>
        /// <param name="generator">IL生成器</param>
        /// <param name="columnInfo">列信息</param>
        /// <param name="ordinal">序号</param>
        private void BindMethod(ILGenerator generator, EntityColumnInfo columnInfo, int ordinal)
        {
            IDbBind bind = Context.Ado.DbBind;
            bool isNullableType = false;
            MethodInfo method = null;
            Type bindPropertyType = UtilMethods.GetUnderType(columnInfo.PropertyInfo, ref isNullableType);
            string dbTypeName = UtilMethods.GetParenthesesValue(DataRecord.GetDataTypeName(ordinal));
            if (dbTypeName.IsNullOrEmpty())
            {
                dbTypeName = bindPropertyType.Name;
            }
            string propertyName = columnInfo.PropertyName;
            string validPropertyName = bind.GetPropertyTypeName(dbTypeName);
            validPropertyName = validPropertyName == "byte[]" ? "byteArray" : validPropertyName;
            CSharpDataType validPropertyType = (CSharpDataType)Enum.Parse(typeof(CSharpDataType), validPropertyName);

            #region NoSql
            if (this.Context.Ado is AdoProvider provider)
            {
                if (provider.IsNoSql)
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    if (method.IsVirtual)
                        generator.Emit(OpCodes.Callvirt, method);
                    else
                        generator.Emit(OpCodes.Call, method);
                    return;
                }
            }
            #endregion

            #region Sqlite Logic
            if (this.Context.CurrentConnectionConfig.DbType == DbType.Sqlite)
            {
                if (bindPropertyType.IsEnum())
                {
                    method = isNullableType ? getConvertEnum_Null.MakeGenericMethod(bindPropertyType) : getEnum.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.IntType)
                {
                    method = isNullableType ? getConvertInt32 : getInt32;
                }
                else if (bindPropertyType == UtilConstants.DateTimeOffsetType && SugarCompatible.IsFramework)
                {
                    method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                }
                else if (bindPropertyType == UtilConstants.ByteType)
                {
                    method = isNullableType ? getConvertByte : getByte;
                }
                else if (bindPropertyType == UtilConstants.StringType && dbTypeName.EqualCase("timestamp"))
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (dbTypeName.EqualCase("STRING"))
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.StringType && validPropertyName == "int")
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                else if (bindPropertyType == UtilConstants.StringType)
                {
                    method = getString;
                }
                else
                {
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                }
                if (method.IsVirtual)
                    generator.Emit(OpCodes.Callvirt, method);
                else
                    generator.Emit(OpCodes.Call, method);
                return;
            }

            #endregion

            #region Common Database Logic
            string bindPropertyTypeName = bindPropertyType.Name.ToLower();
            bool isEnum = bindPropertyType.IsEnum();
            if (isEnum) { validPropertyType = CSharpDataType.@enum; }
            switch (validPropertyType)
            {
                case CSharpDataType.@int:
                    CheckType(bind.IntThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName.IsContainsIn("int", "int32"))
                        method = isNullableType ? getConvertInt32 : getInt32;
                    if (bindPropertyTypeName.IsContainsIn("int64"))
                        method = null;
                    if (bindPropertyTypeName.IsContainsIn("byte"))
                        method = isNullableType ? getConvertByte : getByte;
                    if (bindPropertyTypeName.IsContainsIn("int16"))
                        method = isNullableType ? getConvertInt16 : getInt16;
                    if (bindPropertyTypeName == "uint32" && this.Context.CurrentConnectionConfig.DbType.IsIn(DbType.MySql, DbType.MySqlConnector))
                        method = null;
                    if (bindPropertyTypeName == "int16")
                        method = null;
                    break;
                case CSharpDataType.@bool:
                    if (bindPropertyTypeName == "bool" || bindPropertyTypeName == "boolean")
                        method = isNullableType ? getConvertBoolean : getBoolean;
                    break;
                case CSharpDataType.@string:
                    if (this.Context.CurrentConnectionConfig.DbType != DbType.Oracle)
                    {
                        CheckType(bind.StringThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    }
                    method = getString;
                    if (bindPropertyTypeName == "guid")
                    {
                        method = isNullableType ? getConvertStringGuid : getStringGuid;
                    }
                    else if (bindPropertyTypeName == "xelement")
                    {
                        method = getXelement;
                    }
                    else if (dbTypeName == "CHAR" && DataRecord.GetDataTypeName(ordinal) == "CHAR(36)")
                    {
                        method = null;
                    }
                    else if (bindPropertyType.Name == "Char")
                    {
                        method = null;
                    }
                    break;
                case CSharpDataType.DateTime:
                    CheckType(bind.DateThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName == "datetime")
                        method = isNullableType ? getConvertDateTime : getDateTime;
                    if (bindPropertyTypeName == "datetime" && dbTypeName.Equals("time", StringComparison.CurrentCultureIgnoreCase))
                        method = isNullableType ? getConvertTime : getTime;
                    if (bindPropertyTypeName == "datetimeoffset")
                        method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                    break;
                case CSharpDataType.@decimal:
                    CheckType(bind.DecimalThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName == "decimal")
                        method = isNullableType ? getConvertDecimal : getDecimal;
                    break;
                case CSharpDataType.@float:
                case CSharpDataType.@double:
                    CheckType(bind.DoubleThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName.IsIn("double", "single") && dbTypeName != "real")
                        method = isNullableType ? getConvertDouble : getDouble;
                    else
                        method = isNullableType ? getConvertFloat : getFloat;
                    if (dbTypeName.Equals("float", StringComparison.CurrentCultureIgnoreCase) && isNullableType && bindPropertyTypeName.Equals("single", StringComparison.CurrentCultureIgnoreCase))
                    {
                        method = getConvertDoubleToFloat;
                    }
                    if (bindPropertyType == UtilConstants.DecType)
                    {
                        method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    }
                    if (bindPropertyType == UtilConstants.IntType)
                    {
                        method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    }
                    if (bindPropertyTypeName == "string")
                    {
                        method = null;
                    }
                    break;
                case CSharpDataType.Guid:
                    CheckType(bind.GuidThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName == "guid")
                        method = isNullableType ? getConvertGuid : getGuid;
                    break;
                case CSharpDataType.@byte:
                    if (bindPropertyTypeName == "byte")
                        method = isNullableType ? getConvertByte : getByte;
                    break;
                case CSharpDataType.@enum:
                    method = isNullableType ? getConvertEnum_Null.MakeGenericMethod(bindPropertyType) : getEnum.MakeGenericMethod(bindPropertyType);
                    break;
                case CSharpDataType.@short:
                    CheckType(bind.ShortThrow, bindPropertyTypeName, validPropertyName, propertyName);
                    if (bindPropertyTypeName == "int16" || bindPropertyTypeName == "short")
                        method = isNullableType ? getConvertInt16 : getInt16;
                    break;
                case CSharpDataType.@long:
                    if (bindPropertyTypeName == "int64" || bindPropertyTypeName == "long")
                        method = isNullableType ? getConvertInt64 : getInt64;
                    break;
                case CSharpDataType.DateTimeOffset:
                    method = isNullableType ? getConvertdatetimeoffset : getdatetimeoffset;
                    if (bindPropertyTypeName == "datetime")
                        method = isNullableType ? getConvertdatetimeoffsetDate : getdatetimeoffsetDate;
                    break;
                case CSharpDataType.Single:
                    break;
                default:
                    method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
                    break;
            }
            if (method == null && bindPropertyType == UtilConstants.StringType)
            {
                method = getConvertString;
            }
            if (bindPropertyType == UtilConstants.ObjType)
            {
                method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);
            }
            if (method == null)
                method = isNullableType ? getOtherNull.MakeGenericMethod(bindPropertyType) : getOther.MakeGenericMethod(bindPropertyType);

            if (method.IsVirtual)
                generator.Emit(OpCodes.Callvirt, method);
            else
                generator.Emit(OpCodes.Call, method);
            #endregion
        }

        /// <summary>
        /// 检查类型
        /// </summary>
        /// <param name="invalidTypes">无效类型列表</param>
        /// <param name="bindPropertyTypeName">绑定属性类型名称</param>
        /// <param name="validPropertyType">有效属性类型</param>
        /// <param name="propertyName">属性名</param>
        private void CheckType(List<string> invalidTypes, string bindPropertyTypeName, string validPropertyType, string propertyName)
        {
            var isAny = invalidTypes.Contains(bindPropertyTypeName);
            if (isAny)
            {
                throw new SqlSugarException(string.Format("{0} can't  convert {1} to {2}", propertyName, validPropertyType, bindPropertyTypeName));
            }
        }
        #endregion
    }
}