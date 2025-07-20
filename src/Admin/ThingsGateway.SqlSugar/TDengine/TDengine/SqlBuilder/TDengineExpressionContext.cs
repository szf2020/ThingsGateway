using Dm.util;
namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 表达式上下文
    /// </summary>
    public class TDengineExpressionContext : ExpressionContext, ILambdaExpressions
    {
        /// <summary>
        /// SqlSugar 上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TDengineExpressionContext()
        {
            base.DbMehtods = new TDengineExpressionContextMethod();
        }

        /// <summary>
        /// 获取 SQL 左引号
        /// </summary>
        public override string SqlTranslationLeft
        {
            get
            {
                return "`";
            }
        }

        /// <summary>
        /// 获取 SQL 右引号
        /// </summary>
        public override string SqlTranslationRight
        {
            get
            {
                return "`";
            }
        }

        /// <summary>
        /// 获取转换后的文本
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>转换后的文本</returns>
        public override string GetTranslationText(string name)
        {
            return SqlTranslationLeft + name.ToLower(isAutoToLower) + SqlTranslationRight;
        }

        /// <summary>
        /// 是否自动转换为小写
        /// </summary>
        public bool isAutoToLower
        {
            get
            {
                return base.PgSqlIsAutoToLower;
            }
        }

        /// <summary>
        /// 获取转换后的表名
        /// </summary>
        /// <param name="entityName">实体名</param>
        /// <param name="isMapping">是否使用映射</param>
        /// <returns>转换后的表名</returns>
        public override string GetTranslationTableName(string entityName, bool isMapping = true)
        {
            Check.ArgumentNullException(entityName, string.Format(null, ErrorMessage.ObjNotExistCompositeFormat, "Table Name"));
            if (IsTranslationText(entityName)) return entityName;
            isMapping = isMapping && this.MappingTables.HasValue();
            var isComplex = entityName.Contains(UtilConstants.Dot);
            if (isMapping && isComplex)
            {
                var columnInfo = entityName.Split(UtilConstants.DotChar);
                var mappingInfo = this.MappingTables.FirstOrDefault(it => it.EntityName.Equals(columnInfo.Last(), StringComparison.CurrentCultureIgnoreCase));
                if (mappingInfo != null)
                {
                    columnInfo[columnInfo.Length - 1] = mappingInfo.EntityName;
                }
                return string.Join(UtilConstants.Dot, columnInfo.Select(it => GetTranslationText(it)));
            }
            else if (isMapping)
            {
                var mappingInfo = this.MappingTables.FirstOrDefault(it => it.EntityName.Equals(entityName, StringComparison.CurrentCultureIgnoreCase));

                var tableName = mappingInfo?.DbTableName + "";
                if (tableName.Contains('.'))
                {
                    tableName = string.Join(UtilConstants.Dot, tableName.Split(UtilConstants.DotChar).Select(it => GetTranslationText(it)));
                    return tableName;
                }

                return SqlTranslationLeft + (mappingInfo == null ? entityName : mappingInfo.DbTableName).ToLower(isAutoToLower) + SqlTranslationRight;
            }
            else if (isComplex)
            {
                return string.Join(UtilConstants.Dot, entityName.Split(UtilConstants.DotChar).Select(it => GetTranslationText(it)));
            }
            else
            {
                return GetTranslationText(entityName);
            }
        }

        /// <summary>
        /// 获取转换后的列名
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <returns>转换后的列名</returns>
        public override string GetTranslationColumnName(string columnName)
        {
            Check.ArgumentNullException(columnName, string.Format(null, ErrorMessage.ObjNotExistCompositeFormat, "Column Name"));
            if (columnName.Substring(0, 1) == this.SqlParameterKeyWord)
            {
                return columnName;
            }
            if (IsTranslationText(columnName)) return columnName;
            if (columnName.Contains(UtilConstants.Dot))
            {
                return string.Join(UtilConstants.Dot, columnName.Split(UtilConstants.DotChar).Select(it => GetTranslationText(it)));
            }
            else
            {
                return GetTranslationText(columnName);
            }
        }

        /// <summary>
        /// 获取数据库列名
        /// </summary>
        /// <param name="entityName">实体名</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>数据库列名</returns>
        public override string GetDbColumnName(string entityName, string propertyName)
        {
            if (this.MappingColumns.HasValue())
            {
                var mappingInfo = this.MappingColumns.SingleOrDefault(it => it.EntityName == entityName && it.PropertyName == propertyName);
                return (mappingInfo == null ? propertyName : mappingInfo.DbColumnName).ToLower(isAutoToLower);
            }
            else
            {
                return propertyName.ToLower(isAutoToLower);
            }
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="entityValue">实体值</param>
        /// <returns>转换后的值</returns>
        public string GetValue(object entityValue)
        {
            if (entityValue == null)
                return null;
            var type = TaosUtilMethods.GetUnderType(entityValue.GetType());
            if (UtilConstants.NumericalTypes.Contains(type))
            {
                return entityValue.ToString();
            }
            else if (type == UtilConstants.DateType)
            {
                return this.DbMehtods.ToDate(new MethodCallExpressionModel()
                {
                    Args = new System.Collections.Generic.List<MethodCallExpressionArgs>() {
                 new MethodCallExpressionArgs(){ MemberName=$"'{entityValue}'" }
                }
                });
            }
            else
            {
                return this.DbMehtods.ToString(new MethodCallExpressionModel()
                {
                    Args = new System.Collections.Generic.List<MethodCallExpressionArgs>() {
                 new MethodCallExpressionArgs(){ MemberName=$"'{entityValue}'" }
                }
                });
            }
        }
    }

    /// <summary>
    /// TDengine 表达式上下文方法
    /// </summary>
    public class TDengineExpressionContextMethod : DefaultDbMethod, IDbMethods
    {
        /// <summary>
        /// 字符索引
        /// </summary>
        public override string CharIndex(MethodCallExpressionModel model)
        {
            return string.Format(" (strpos ({1},{0})-1)", model.Args[0].MemberName, model.Args[1].MemberName);
        }

        /// <summary>
        /// 真值
        /// </summary>
        public override string TrueValue()
        {
            return "true";
        }

        /// <summary>
        /// 假值
        /// </summary>
        public override string FalseValue()
        {
            return "false";
        }

        /// <summary>
        /// 子字符串
        /// </summary>
        public override string Substring(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            if (parameter2.MemberValue is int && parameter3.MemberValue is int)
            {
                model.Parameters.RemoveAll(it => it.ParameterName.Equals(parameter2.MemberName) || it.ParameterName.Equals(parameter3.MemberName));
                return string.Format("SUBSTR({0},{1},{2})", parameter.MemberName, Convert.ToInt32(parameter2.MemberValue) + 1, parameter3.MemberValue);
            }
            else
            {
                return string.Format("SUBSTR({0},{1},{2})", parameter.MemberName, parameter2.MemberName, parameter3.MemberName);
            }
        }

        /// <summary>
        /// 日期差异
        /// </summary>
        public override string DateDiff(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            return string.Format(" TIMEDIFF({1},{2},1{0}) ", parameter.MemberValue.ObjToString().ToLower().First(), parameter2.MemberName, parameter3.MemberName);
        }

        /// <summary>
        /// IIF条件判断
        /// </summary>
        public override string IIF(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            if (parameter.Type == UtilConstants.BoolType)
            {
                parameter.MemberName = parameter.MemberName.ToString().Replace("=1", "=true");
                parameter2.MemberName = false;
                parameter3.MemberName = true;
            }
            return string.Format("( CASE  WHEN {0} THEN {1}  ELSE {2} END )", parameter.MemberName, parameter2.MemberName, parameter3.MemberName);
        }

        /// <summary>
        /// 日期值
        /// </summary>
        public override string DateValue(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var format = parameter2.MemberValue.ObjToString();
            return string.Format("  {0}({1})   ", format, parameter.MemberName);
        }

        /// <summary>
        /// 包含
        /// </summary>
        public override string Contains(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return string.Format(" ({0} like  {1}  ) ", parameter.MemberName, ("%" + parameter2.MemberValue + "%").ToSqlValue());
        }

        /// <summary>
        /// 开头是
        /// </summary>
        public override string StartsWith(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return string.Format(" ({0} like  {1}  ) ", parameter.MemberName, ("%" + parameter2.MemberValue).ToSqlValue());
        }

        /// <summary>
        /// 结尾是
        /// </summary>
        public override string EndsWith(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return string.Format("({0} like  {1}  ) ", parameter.MemberName, (parameter2.MemberValue + "%").ToSqlValue());
        }

        /// <summary>
        /// 是否是同一天
        /// </summary>
        public override string DateIsSameDay(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return string.Format(" ( to_char({0},'yyyy-MM-dd')=to_char({1},'yyyy-MM-dd') ) ", parameter.MemberName, parameter2.MemberName);
        }

        /// <summary>
        /// 是否有值
        /// </summary>
        public override string HasValue(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format("( {0} IS NOT NULL )", parameter.MemberName);
        }

        /// <summary>
        /// 按类型判断日期是否相同
        /// </summary>
        public override string DateIsSameByType(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            DateType dateType = (DateType)parameter3.MemberValue;
            var format = "yyyy-MM-dd";
            if (dateType == DateType.Quarter)
            {
                return string.Format(" (date_trunc('quarter',{0})=date_trunc('quarter',{1}) ) ", parameter.MemberName, parameter2.MemberName, format);
            }
            switch (dateType)
            {
                case DateType.Year:
                    format = "yyyy";
                    break;
                case DateType.Month:
                    format = "yyyy-MM";
                    break;
                case DateType.Day:
                    break;
                case DateType.Hour:
                    format = "yyyy-MM-dd HH";
                    break;
                case DateType.Second:
                    format = "yyyy-MM-dd HH:mm:ss";
                    break;
                case DateType.Minute:
                    format = "yyyy-MM-dd HH:mm";
                    break;
                case DateType.Millisecond:
                    format = "yyyy-MM-dd HH:mm.ms";
                    break;
                default:
                    break;
            }
            return string.Format(" ( to_char({0},'{2}')=to_char({1},'{2}') ) ", parameter.MemberName, parameter2.MemberName, format);
        }

        /// <summary>
        /// 转换为日期
        /// </summary>
        public override string ToDate(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS timestamp)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为短日期
        /// </summary>
        public override string ToDateShort(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format("  CAST( SUBSTR(TO_ISO8601({0}),1,10) AS timestamp)", parameter.MemberName);
        }

        /// <summary>
        /// 按类型添加日期
        /// </summary>
        public override string DateAddByType(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            var parameter3 = model.Args[2];
            var result = string.Format(" {1}+{2}{0} ", parameter3.MemberValue.ObjToString().ToLower().First(), parameter.MemberName, parameter2.MemberValue);
            return result.replace("+-", "-");
        }

        /// <summary>
        /// 添加天数
        /// </summary>
        public override string DateAddDay(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter2 = model.Args[1];
            return string.Format(" ({0} + ({1}||'day')::INTERVAL) ", parameter.MemberName, parameter2.MemberName);
        }

        /// <summary>
        /// 转换为Int32
        /// </summary>
        public override string ToInt32(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS INT4)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为Int64
        /// </summary>
        public override string ToInt64(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS INT8)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS VARCHAR)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为Guid
        /// </summary>
        public override string ToGuid(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS UUID)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为Double
        /// </summary>
        public override string ToDouble(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS DOUBLE)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为布尔值
        /// </summary>
        public override string ToBool(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS boolean)", parameter.MemberName);
        }

        /// <summary>
        /// 转换为Decimal
        /// </summary>
        public override string ToDecimal(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" CAST({0} AS DOUBLE)", parameter.MemberName);
        }

        /// <summary>
        /// 获取长度
        /// </summary>
        public override string Length(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return string.Format(" LENGTH({0})", parameter.MemberName);
        }

        /// <summary>
        /// 合并字符串
        /// </summary>
        public override string MergeString(params string[] strings)
        {
            return " concat(" + string.Join(",", strings).Replace("+", "") + ") ";
        }

        /// <summary>
        /// 是否为null
        /// </summary>
        public override string IsNull(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            return string.Format("(CASE WHEN  {0} IS NULL THEN  {1} ELSE {0} END)", parameter.MemberName, parameter1.MemberName);
        }

        /// <summary>
        /// 获取当前日期
        /// </summary>
        public override string GetDate()
        {
            return "NOW()";
        }

        /// <summary>
        /// 获取随机数
        /// </summary>
        public override string GetRandom()
        {
            return "RANDOM()";
        }

        /// <summary>
        /// 等于真
        /// </summary>
        public override string EqualTrue(string fieldName)
        {
            return "( " + fieldName + "=true )";
        }

        /// <summary>
        /// 获取 JSON 字段值
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 字段访问表达式</returns>
        public override string JsonField(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            var result = GetJson(parameter.MemberName, parameter1.MemberName, model.Args.Count == 2);
            if (model.Args.Count > 2)
            {
                result = GetJson(result, model.Args[2].MemberName, model.Args.Count == 3);
            }
            if (model.Args.Count > 3)
            {
                result = GetJson(result, model.Args[3].MemberName, model.Args.Count == 4);
            }
            if (model.Args.Count > 4)
            {
                result = GetJson(result, model.Args[4].MemberName, model.Args.Count == 5);
            }
            if (model.Args.Count > 5)
            {
                result = GetJson(result, model.Args[5].MemberName, model.Args.Count == 6);
            }
            return result;
        }

        /// <summary>
        /// 检查 JSON 是否包含指定字段名
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 字段存在性检查表达式</returns>
        public override string JsonContainsFieldName(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            var parameter1 = model.Args[1];
            return $"({parameter.MemberName}::jsonb ?{parameter1.MemberName})";
        }

        /// <summary>
        /// 获取 JSON 字段访问表达式
        /// </summary>
        /// <param name="memberName1">JSON 对象或字段名</param>
        /// <param name="memberName2">字段名或索引</param>
        /// <param name="isLast">是否为最后一级访问</param>
        /// <returns>JSON 访问表达式</returns>
        private string GetJson(object memberName1, object memberName2, bool isLast)
        {
            if (isLast)
            {
                return $"({memberName1}::json->>{memberName2})";
            }
            else
            {
                return $"({memberName1}->{memberName2})";
            }
        }

        /// <summary>
        /// 获取 JSON 数组长度
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 数组长度表达式</returns>
        public override string JsonArrayLength(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" json_array_length({parameter.MemberName}::json) ";
        }

        /// <summary>
        /// JSON 解析表达式
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 解析表达式</returns>
        public override string JsonParse(MethodCallExpressionModel model)
        {
            var parameter = model.Args[0];
            return $" ({parameter.MemberName}::json) ";
        }

        /// <summary>
        /// 检查 JSON 数组是否包含指定元素
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 数组包含检查表达式</returns>
        public override string JsonArrayAny(MethodCallExpressionModel model)
        {
            if (SqlSugar.UtilMethods.IsNumber(model.Args[1].MemberValue.GetType().Name))
            {
                return $" {model.Args[0].MemberName}::jsonb @> '[{model.Args[1].MemberValue.ObjToStringNoTrim().ToSqlFilter()}]'::jsonb";
            }
            else
            {
                return $" {model.Args[0].MemberName}::jsonb @> '[\"{model.Args[1].MemberValue}\"]'::jsonb";
            }
        }

        /// <summary>
        /// 检查 JSON 对象数组是否包含指定键值对
        /// </summary>
        /// <param name="model">方法调用表达式模型</param>
        /// <returns>JSON 对象数组包含检查表达式</returns>
        public override string JsonListObjectAny(MethodCallExpressionModel model)
        {
            if (SqlSugar.UtilMethods.IsNumber(model.Args[2].MemberValue.GetType().Name))
            {
                return $" {model.Args[0].MemberName}::jsonb @> '[{{\"{model.Args[1].MemberValue}\":{model.Args[2].MemberValue}}}]'::jsonb";
            }
            else
            {
                return $" {model.Args[0].MemberName}::jsonb @> '[{{\"{model.Args[1].MemberValue}\":\"{model.Args[2].MemberValue.ObjToStringNoTrim().ToSqlFilter()}\"}}]'::jsonb";
            }
        }
    }
}
