using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 数据库优先提供者基类
    /// </summary>
    public abstract partial class DbFirstProvider : IDbFirst
    {
        /// <summary>
        /// SqlSugar客户端实例
        /// </summary>
        public virtual ISqlSugarClient Context { get; set; }
        /// <summary>
        /// 类模板
        /// </summary>
        private string ClassTemplate { get; set; }
        /// <summary>
        /// 类描述模板
        /// </summary>
        private string ClassDescriptionTemplate { get; set; }
        /// <summary>
        /// 属性模板
        /// </summary>
        private string PropertyTemplate { get; set; }
        /// <summary>
        /// 属性描述模板
        /// </summary>
        private string PropertyDescriptionTemplate { get; set; }
        /// <summary>
        /// 构造函数模板
        /// </summary>
        private string ConstructorTemplate { get; set; }
        /// <summary>
        /// 命名空间模板
        /// </summary>
        private string UsingTemplate { get; set; }
        /// <summary>
        /// 命名空间名称
        /// </summary>
        private string Namespace { get; set; }
        /// <summary>
        /// 是否创建属性
        /// </summary>
        private bool IsAttribute { get; set; }
        /// <summary>
        /// 是否创建默认值
        /// </summary>
        private bool IsDefaultValue { get; set; }
        /// <summary>
        /// 列过滤条件函数
        /// </summary>
        private Func<string, bool> WhereColumnsfunc;
        /// <summary>
        /// 文件名格式化函数
        /// </summary>
        private Func<string, string> FormatFileNameFunc { get; set; }
        /// <summary>
        /// 类名格式化函数
        /// </summary>
        private Func<string, string> FormatClassNameFunc { get; set; }
        /// <summary>
        /// 属性名格式化函数
        /// </summary>
        private Func<string, string> FormatPropertyNameFunc { get; set; }
        /// <summary>
        /// 字符串是否可为空
        /// </summary>
        private bool IsStringNullable { get; set; }
        /// <summary>
        /// 属性文本模板函数
        /// </summary>
        private Func<DbColumnInfo, string, string, string> PropertyTextTemplateFunc { get; set; }
        /// <summary>
        /// 替换类字符串函数
        /// </summary>
        private Func<string, string> ReplaceClassStringFunc { get; set; }
        /// <summary>
        /// SQL构建器
        /// </summary>
        private ISqlBuilder SqlBuilder
        {
            get
            {
                return InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
            }
        }
        /// <summary>
        /// 表信息列表
        /// </summary>
        private List<DbTableInfo> TableInfoList { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DbFirstProvider()
        {
            this.ClassTemplate = DbFirstTemplate.ClassTemplate;
            this.ClassDescriptionTemplate = DbFirstTemplate.ClassDescriptionTemplate;
            this.PropertyTemplate = DbFirstTemplate.PropertyTemplate;
            this.PropertyDescriptionTemplate = DbFirstTemplate.PropertyDescriptionTemplate;
            this.ConstructorTemplate = DbFirstTemplate.ConstructorTemplate;
            this.UsingTemplate = DbFirstTemplate.UsingTemplate;
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Init()
        {
            this.Context.Utilities.RemoveCacheAll();
            if (!this.Context.DbMaintenance.IsAnySystemTablePermissions())
            {
                { throw new SqlSugarException("Dbfirst and  Codefirst requires system table permissions"); }
            }
            this.TableInfoList = this.Context.Utilities.TranslateCopy(this.Context.DbMaintenance.GetTableInfoList());
            var viewList = this.Context.Utilities.TranslateCopy(this.Context.DbMaintenance.GetViewInfoList());
            if (viewList.HasValue())
            {
                this.TableInfoList.AddRange(viewList);
            }
        }

        #region Setting Template
        /// <summary>
        /// 设置字符串可为空
        /// </summary>
        public IDbFirst StringNullable()
        {
            IsStringNullable = true;
            return this;
        }
        /// <summary>
        /// 设置类描述模板
        /// </summary>
        public IDbFirst SettingClassDescriptionTemplate(Func<string, string> func)
        {
            this.ClassDescriptionTemplate = func(this.ClassDescriptionTemplate);
            return this;
        }

        /// <summary>
        /// 设置类模板
        /// </summary>
        public IDbFirst SettingClassTemplate(Func<string, string> func)
        {
            this.ClassTemplate = func(this.ClassTemplate);
            return this;
        }

        /// <summary>
        /// 设置构造函数模板
        /// </summary>
        public IDbFirst SettingConstructorTemplate(Func<string, string> func)
        {
            this.ConstructorTemplate = func(this.ConstructorTemplate);
            return this;
        }

        /// <summary>
        /// 设置属性描述模板
        /// </summary>
        public IDbFirst SettingPropertyDescriptionTemplate(Func<string, string> func)
        {
            this.PropertyDescriptionTemplate = func(this.PropertyDescriptionTemplate);
            return this;
        }

        /// <summary>
        /// 设置命名空间模板
        /// </summary>
        public IDbFirst SettingNamespaceTemplate(Func<string, string> func)
        {
            this.UsingTemplate = func(this.UsingTemplate);
            return this;
        }

        /// <summary>
        /// 设置属性模板
        /// </summary>
        public IDbFirst SettingPropertyTemplate(Func<string, string> func)
        {
            this.PropertyTemplate = func(this.PropertyTemplate);
            return this;
        }
        /// <summary>
        /// 设置属性模板
        /// </summary>
        public IDbFirst SettingPropertyTemplate(Func<DbColumnInfo, string, string, string> func)
        {
            this.PropertyTextTemplateFunc = func;
            return this;
        }
        /// <summary>
        /// 使用Razor分析
        /// </summary>
        public RazorFirst UseRazorAnalysis(string razorClassTemplate, string classNamespace = "Models")
        {
            if (razorClassTemplate == null)
            {
                razorClassTemplate = "";
            }
            razorClassTemplate = razorClassTemplate.Replace("@Model.Namespace", classNamespace);
            var result = new RazorFirst();
            if (this.Context.CurrentConnectionConfig.ConfigureExternalServices?.RazorService != null)
            {
                List<RazorTableInfo> razorList = new List<RazorTableInfo>();
                var tables = this.TableInfoList;
                if (tables.HasValue())
                {
                    foreach (var item in tables)
                    {
                        var columns = this.Context.DbMaintenance.GetColumnInfosByTableName(item.Name, false);
                        RazorTableInfo table = new RazorTableInfo()
                        {
                            Columns = columns.Where(it => WhereColumnsfunc == null || WhereColumnsfunc(it.DbColumnName)).Select(it => new RazorColumnInfo()
                            {
                                ColumnDescription = it.ColumnDescription,
                                DataType = it.DataType,
                                DbColumnName = it.DbColumnName,
                                DefaultValue = it.DefaultValue,
                                IsIdentity = it.IsIdentity,
                                IsNullable = it.IsNullable,
                                IsPrimarykey = it.IsPrimarykey,
                                Length = it.Length
                            }).ToList(),
                            Description = item.Description,
                            DbTableName = item.Name
                        };
                        foreach (var col in table.Columns)
                        {
                            col.DataType = GetPropertyTypeName(columns.First(it => it.DbColumnName == col.DbColumnName));
                        }
                        razorList.Add(table);
                    }
                }
                result.ClassStringList = this.Context.CurrentConnectionConfig.ConfigureExternalServices.RazorService.GetClassStringList(razorClassTemplate, razorList);
            }
            else
            {
                { throw new SqlSugarException(ErrorMessage.GetThrowMessage("Need to achieve ConnectionConfig.ConfigureExternal Services.RazorService", "需要实现 ConnectionConfig.ConfigureExternal Services.RazorService接口")); }
            }
            this.Context.Utilities.RemoveCacheAll();
            result.FormatFileNameFunc = this.FormatFileNameFunc;
            return result;
        }
        #endregion

        #region Setting Content
        /// <summary>
        /// 设置是否创建属性
        /// </summary>
        public IDbFirst IsCreateAttribute(bool isCreateAttribute = true)
        {
            this.IsAttribute = isCreateAttribute;
            return this;
        }
        /// <summary>
        /// 设置文件名格式化函数
        /// </summary>
        public IDbFirst FormatFileName(Func<string, string> formatFileNameFunc)
        {
            this.FormatFileNameFunc = formatFileNameFunc;
            return this;
        }
        /// <summary>
        /// 设置类名格式化函数
        /// </summary>
        public IDbFirst FormatClassName(Func<string, string> formatClassNameFunc)
        {
            this.FormatClassNameFunc = formatClassNameFunc;
            return this;
        }
        /// <summary>
        /// 设置属性名格式化函数
        /// </summary>
        public IDbFirst FormatPropertyName(Func<string, string> formatPropertyNameFunc)
        {
            this.FormatPropertyNameFunc = formatPropertyNameFunc;
            return this;
        }
        /// <summary>
        /// 设置创建替换类字符串函数
        /// </summary>
        public IDbFirst CreatedReplaceClassString(Func<string, string> replaceClassStringFunc)
        {
            this.ReplaceClassStringFunc = replaceClassStringFunc;
            return this;
        }
        /// <summary>
        /// 设置是否创建默认值
        /// </summary>
        public IDbFirst IsCreateDefaultValue(bool isCreateDefaultValue = true)
        {
            this.IsDefaultValue = isCreateDefaultValue;
            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 设置数据库对象类型过滤条件
        /// </summary>
        public IDbFirst Where(DbObjectType dbObjectType)
        {
            if (dbObjectType != DbObjectType.All)
                this.TableInfoList = this.TableInfoList.Where(it => it.DbObjectType == dbObjectType).ToList();
            return this;
        }

        /// <summary>
        /// 设置表名过滤条件
        /// </summary>
        public IDbFirst Where(Func<string, bool> func)
        {
            this.TableInfoList = this.TableInfoList.Where(it => func(it.Name)).ToList();
            return this;
        }

        /// <summary>
        /// 设置列名过滤条件
        /// </summary>
        public IDbFirst WhereColumns(Func<string, bool> func)
        {
            WhereColumnsfunc = func;
            return this;
        }

        /// <summary>
        /// 设置对象名过滤条件
        /// </summary>
        public IDbFirst Where(params string[] objectNames)
        {
            if (objectNames.HasValue())
            {
                this.TableInfoList = this.TableInfoList.Where(it => objectNames.Contains(it.Name, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            return this;
        }
        #endregion

        #region Core
        /// <summary>
        /// 生成类字符串列表
        /// </summary>
        public Dictionary<string, string> ToClassStringList(string nameSpace = "Models")
        {
            this.Namespace = nameSpace;
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (this.TableInfoList.HasValue())
            {
                foreach (var tableInfo in this.TableInfoList)
                {
                    try
                    {
                        string classText = null;
                        string className = tableInfo.Name;
                        var oldClasName = className;
                        classText = GetClassString(tableInfo, ref className);
                        result.Remove(className);
                        if (this.ReplaceClassStringFunc != null)
                        {
                            classText = this.ReplaceClassStringFunc(classText);
                        }
                        if (FormatClassNameFunc != null && FormatFileNameFunc != null)
                        {
                            className = oldClasName;
                        }
                        result.Add(className, classText);
                    }
                    catch (Exception ex)
                    {
                        { throw new SqlSugarException($"Table '{0}' error,You can filter it with Db.DbFirst.Where(name=>name!=\"{tableInfo.Name}\" ) {Environment.NewLine} Error message:{ex.Message}"); }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取类字符串
        /// </summary>
        internal string GetClassString(DbTableInfo tableInfo, ref string className)
        {
            var columns = this.Context.DbMaintenance.GetColumnInfosByTableName(tableInfo.Name, false);
            if (this.Context.IgnoreColumns.HasValue())
            {
                var entityName = this.Context.EntityMaintenance.GetEntityName(tableInfo.Name);
                columns = columns.Where(c =>
                                         !this.Context.IgnoreColumns.Any(ig => ig.EntityName.Equals(entityName, StringComparison.CurrentCultureIgnoreCase) && c.DbColumnName == ig.PropertyName)
                                        ).ToList();
            }
            var classText = new StringBuilder(this.ClassTemplate);
            string ConstructorText = IsDefaultValue ? this.ConstructorTemplate : null;
            if (this.Context.MappingTables.HasValue())
            {
                var mappingInfo = this.Context.MappingTables.FirstOrDefault(it => it.DbTableName.Equals(tableInfo.Name, StringComparison.CurrentCultureIgnoreCase));
                if (mappingInfo.HasValue())
                {
                    className = mappingInfo.EntityName;
                }
                if (mappingInfo != null)
                {
                    classText = classText.Replace(DbFirstTemplate.KeyClassName, mappingInfo.EntityName);
                }
            }
            if (FormatClassNameFunc != null)
            {
                className = FormatClassNameFunc(className);
            }
            classText = classText.Replace(DbFirstTemplate.KeyClassName, className);
            classText = classText.Replace(DbFirstTemplate.KeyNamespace, this.Namespace);
            classText = classText.Replace(DbFirstTemplate.KeyUsing, IsAttribute ? (this.UsingTemplate + "using " + UtilConstants.AssemblyName + ";\r\n") : this.UsingTemplate);
            classText = classText.Replace(DbFirstTemplate.KeyClassDescription, this.ClassDescriptionTemplate.Replace(DbFirstTemplate.KeyClassDescription, tableInfo.Description?.Replace(Environment.NewLine, "\t") + "\r\n"));
            classText = classText.Replace(DbFirstTemplate.KeySugarTable, IsAttribute ? string.Format(null, DbFirstTemplate.ValueSugarTable, tableInfo.Name) : null);
            if (columns.HasValue())
            {
                foreach (var item in columns.Where(it => WhereColumnsfunc == null || WhereColumnsfunc(it.DbColumnName)))
                {
                    var isLast = columns.Last() == item;
                    var index = columns.IndexOf(item);
                    string PropertyText = this.PropertyTemplate;
                    string PropertyDescriptionText = this.PropertyDescriptionTemplate;
                    string propertyName = GetPropertyName(item);
                    var oldPropertyName = propertyName;
                    if (FormatPropertyNameFunc != null)
                    {
                        item.DbColumnName = propertyName = FormatPropertyNameFunc(propertyName);
                    }
                    string propertyTypeName = GetPropertyTypeName(item);
                    PropertyText = this.PropertyTextTemplateFunc == null ? GetPropertyText(item, PropertyText) : this.PropertyTextTemplateFunc(item, this.PropertyTemplate, propertyTypeName);
                    PropertyDescriptionText = GetPropertyDescriptionText(item, PropertyDescriptionText);
                    if (this.IsAttribute && item.DataType?.StartsWith('_') == true && PropertyText.Contains("[]"))
                    {
                        PropertyDescriptionText += "\r\n           [SugarColumn(IsArray=true)]";
                    }
                    else if (item?.DataType?.StartsWith("json") == true)
                    {
                        PropertyDescriptionText += "\r\n           [SugarColumn(IsJson=true)]";
                    }
                    else if (FormatPropertyNameFunc != null)
                    {
                        if (PropertyText.Contains("SugarColumn"))
                        {
                            PropertyText = PropertyText.Replace(")]", ",ColumnName=\"" + oldPropertyName + "\")]");
                        }
                        else
                        {
                            PropertyDescriptionText += "\r\n           [SugarColumn(ColumnName=\"" + oldPropertyName + "\")]";
                        }
                    }
                    PropertyText = PropertyDescriptionText + PropertyText;
                    classText = classText.Replace(DbFirstTemplate.KeyPropertyName, PropertyText + (isLast ? "" : ("\r\n" + DbFirstTemplate.KeyPropertyName)));
                    if (ConstructorText.HasValue() && item.DefaultValue != null && item.IsIdentity != true)
                    {
                        var hasDefaultValue = columns.Skip(index + 1).Any(it => it.DefaultValue.HasValue());
                        if (item.DefaultValue.EqualCase("CURRENT_TIMESTAMP"))
                        {
                            item.DefaultValue = "DateTime.Now";
                        }
                        else if (item.DefaultValue == "b'1'")
                        {
                            item.DefaultValue = "1";
                        }
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyDefaultValue, GetPropertyTypeConvert(item)) + (!hasDefaultValue ? "" : this.ConstructorTemplate);
                    }
                }
            }
            if (!columns.Any(it => it.DefaultValue != null && it.IsIdentity == false))
            {
                ConstructorText = null;
            }
            classText = classText.Replace(DbFirstTemplate.KeyConstructor, ConstructorText);
            classText = classText.Replace(DbFirstTemplate.KeyPropertyName, null);
            return classText.ToString();
        }

        /// <summary>
        /// 获取类字符串
        /// </summary>
        internal string GetClassString(List<DbColumnInfo> columns, ref string className)
        {
            var classText = new StringBuilder(this.ClassTemplate);
            string ConstructorText = IsDefaultValue ? this.ConstructorTemplate : null;
            classText = classText.Replace(DbFirstTemplate.KeyClassName, className);
            classText = classText.Replace(DbFirstTemplate.KeyNamespace, this.Namespace);
            classText = classText.Replace(DbFirstTemplate.KeyUsing, IsAttribute ? (this.UsingTemplate + "using " + UtilConstants.AssemblyName + ";\r\n") : this.UsingTemplate);
            classText = classText.Replace(DbFirstTemplate.KeyClassDescription, this.ClassDescriptionTemplate.Replace(DbFirstTemplate.KeyClassDescription, "\r\n"));
            classText = classText.Replace(DbFirstTemplate.KeySugarTable, IsAttribute ? string.Format(null, DbFirstTemplate.ValueSugarTable, className) : null);
            if (columns.HasValue())
            {
                foreach (var item in columns)
                {
                    var isLast = columns.Last() == item;
                    var index = columns.IndexOf(item);
                    string PropertyText = this.PropertyTemplate;
                    string PropertyDescriptionText = this.PropertyDescriptionTemplate;
                    string propertyName = GetPropertyName(item);
                    string propertyTypeName = item.PropertyName;
                    PropertyText = GetPropertyText(item, PropertyText);
                    PropertyDescriptionText = GetPropertyDescriptionText(item, PropertyDescriptionText);
                    PropertyText = PropertyDescriptionText + PropertyText;
                    classText = classText.Replace(DbFirstTemplate.KeyPropertyName, PropertyText + (isLast ? "" : ("\r\n" + DbFirstTemplate.KeyPropertyName)));
                    if (ConstructorText.HasValue() && item.DefaultValue != null)
                    {
                        var hasDefaultValue = columns.Skip(index + 1).Any(it => it.DefaultValue.HasValue());
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
                        ConstructorText = ConstructorText.Replace(DbFirstTemplate.KeyDefaultValue, GetPropertyTypeConvert(item)) + (!hasDefaultValue ? "" : this.ConstructorTemplate);
                    }
                }
            }
            if (!columns.Any(it => it.DefaultValue != null))
            {
                ConstructorText = null;
            }
            classText = classText.Replace(DbFirstTemplate.KeyConstructor, ConstructorText);
            classText = classText.Replace(DbFirstTemplate.KeyPropertyName, null);
            return classText.ToString();
        }
        /// <summary>
        /// 创建类文件
        /// </summary>
        public void CreateClassFile(string directoryPath, string nameSpace = "Models")
        {
            var seChar = Path.DirectorySeparatorChar.ToString();
            if (directoryPath == null) { throw new SqlSugarException("directoryPath can't null"); }
            var classStringList = ToClassStringList(nameSpace);
            if (classStringList.IsValuable())
            {
                foreach (var item in classStringList)
                {
                    var fileName = item.Key;
                    if (FormatFileNameFunc != null)
                    {
                        fileName = FormatFileNameFunc(fileName);
                    }
                    var filePath = directoryPath.TrimEnd('\\').TrimEnd('/') + string.Format(seChar + "{0}.cs", fileName);
                    FileHelper.CreateFile(filePath, item.Value, Encoding.UTF8);
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// 获取属性类型默认值
        /// </summary>
        private string GetProertypeDefaultValue(DbColumnInfo item)
        {
            var result = item.DefaultValue;
            if (result == null) return null;
            if (Regex.IsMatch(result, @"^\(\'(.+)\'\)$"))
            {
                result = Regex.Match(result, @"^\(\'(.+)\'\)$").Groups[1].Value;
            }
            if (Regex.IsMatch(result, @"^\(\((.+)\)\)$"))
            {
                result = Regex.Match(result, @"^\(\((.+)\)\)$").Groups[1].Value;
            }
            if (Regex.IsMatch(result, @"^\((.+)\)$"))
            {
                result = Regex.Match(result, @"^\((.+)\)$").Groups[1].Value;
            }
            var lowerResult = result.ToLower();
            if (lowerResult.Equals(this.SqlBuilder.SqlDateNow, StringComparison.OrdinalIgnoreCase) ||
                lowerResult == "getdate()" ||
                lowerResult == "getutcdate()" ||
                lowerResult == "now()")
            {
                result = "DateTime.Now";
            }
            result = result.Replace("\r", "\t").Replace("\n", "\t");
            result = result.IsIn("''", "\"\"") ? string.Empty : result;
            return result;
        }
        /// <summary>
        /// 获取属性文本
        /// </summary>
        private string GetPropertyText(DbColumnInfo item, string PropertyText)
        {
            string SugarColumnText = "\r\n           [SugarColumn({0})]";
            var propertyName = GetPropertyName(item);
            var isMappingColumn = propertyName != item.DbColumnName;
            var hasSugarColumn = item.IsPrimarykey == true || item.IsIdentity == true || isMappingColumn;
            if (hasSugarColumn && this.IsAttribute)
            {
                List<string> joinList = new List<string>();
                if (item.IsPrimarykey)
                {
                    joinList.Add("IsPrimaryKey=true");
                }
                if (item.IsIdentity)
                {
                    joinList.Add("IsIdentity=true");
                }
                if (isMappingColumn)
                {
                    joinList.Add("ColumnName=\"" + item.DbColumnName + '\"');
                }
                SugarColumnText = string.Format(SugarColumnText, string.Join(",", joinList));
            }
            else
            {
                SugarColumnText = null;
            }
            string typeString = GetPropertyTypeName(item);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeySugarColumn, SugarColumnText);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyType, typeString);
            PropertyText = PropertyText.Replace(DbFirstTemplate.KeyPropertyName, propertyName);
            if (typeString == "string" && this.IsStringNullable && item.IsNullable == false && PropertyText.EndsWith("{get;set;}\r\n"))
            {
                PropertyText = PropertyText.Replace("{get;set;}\r\n", "{get;set;} = null!;\r\n");
            }
            return PropertyText;
        }
        /// <summary>
        /// 获取实体名称
        /// </summary>
        private string GetEnityName(DbColumnInfo item)
        {
            var mappingInfo = this.Context.MappingTables.FirstOrDefault(it => it.DbTableName.Equals(item.TableName, StringComparison.CurrentCultureIgnoreCase));
            return mappingInfo == null ? item.TableName : mappingInfo.EntityName;
        }
        /// <summary>
        /// 获取属性名称
        /// </summary>
        private string GetPropertyName(DbColumnInfo item)
        {
            if (this.Context.MappingColumns.HasValue())
            {
                var mappingInfo = this.Context.MappingColumns.SingleOrDefault(it => it.DbColumnName == item.DbColumnName && it.EntityName == GetEnityName(item));
                return mappingInfo == null ? item.DbColumnName : mappingInfo.PropertyName;
            }
            else
            {
                return item.DbColumnName;
            }
        }
        /// <summary>
        /// 获取属性类型名称
        /// </summary>
        protected virtual string GetPropertyTypeName(DbColumnInfo item)
        {
            string result = item.PropertyType != null ? item.PropertyType.Name : this.Context.Ado.DbBind.GetPropertyTypeName(item.DataType);
            if (result != "string" && result != "byte[]" && result != "object" && item.IsNullable)
            {
                result += "?";
            }
            if (result == "Int32")
            {
                result = item.IsNullable ? "int?" : "int";
            }
            if (result == "String")
            {
                result = "string";
            }
            if (result == "string" && item.IsNullable && IsStringNullable)
            {
                result = result + "?";
            }
            if (item.OracleDataType.EqualCase("raw") && item.Length == 16)
            {
                return "Guid";
            }
            if (item.OracleDataType.EqualCase("number") && item.Length == 1 && item.Scale == 0)
            {
                return "bool";
            }
            if (result.EqualCase("char") || result.EqualCase("char?"))
            {
                return "string";
            }
            if (item.DataType == "tinyint unsigned")
            {
                return "short";
            }
            if (item.DataType == "smallint unsigned")
            {
                return "ushort";
            }
            if (item.DataType == "bigint unsigned")
            {
                return "ulong";
            }
            if (item.DataType == "int unsigned")
            {
                return "uint";
            }
            if (item.DataType == "MediumInt")
            {
                return "int";
            }
            if (item.DataType == "MediumInt unsigned")
            {
                return "uint";
            }
            return result;
        }
        /// <summary>
        /// 获取属性类型转换
        /// </summary>
        private string GetPropertyTypeConvert(DbColumnInfo item)
        {
            var convertString = GetProertypeDefaultValue(item);
            if (convertString == "DateTime.Now" || convertString == null)
                return convertString;
            if (convertString.ObjToString() == "newid()")
            {
                return "Guid.NewGuid()";
            }
            if (item.DataType?.ToString()?.EndsWith("unsigned") == true)
            {
                return convertString;
            }
            if (item.DataType == "bit")
                return (convertString == "1" || convertString.Equals("true", StringComparison.CurrentCultureIgnoreCase)).ToString().ToLower();
            if (convertString.EqualCase("NULL"))
            {
                return "null";
            }
            string result = this.Context.Ado.DbBind.GetConvertString(item.DataType) + "(\"" + convertString + "\")";
            if (this.SqlBuilder.SqlParameterKeyWord == ":" && !string.IsNullOrEmpty(item.OracleDataType))
            {
                result = this.Context.Ado.DbBind.GetConvertString(item.OracleDataType) + "(\"" + convertString + "\")";
            }
            return result;
        }
        /// <summary>
        /// 获取属性描述文本
        /// </summary>
        private string GetPropertyDescriptionText(DbColumnInfo item, string propertyDescriptionText)
        {
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyPropertyDescription, GetColumnDescription(item.ColumnDescription));
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyDefaultValue, GetProertypeDefaultValue(item));
            propertyDescriptionText = propertyDescriptionText.Replace(DbFirstTemplate.KeyIsNullable, item.IsNullable.ObjToString());
            return propertyDescriptionText;
        }
        /// <summary>
        /// 获取列描述
        /// </summary>
        private string GetColumnDescription(string columnDescription)
        {
            if (columnDescription == null) return columnDescription;
            columnDescription = columnDescription.Replace("\r", "\t");
            columnDescription = columnDescription.Replace("\n", "\t");
            columnDescription = columnDescription.Replace(Environment.NewLine, "\t");
            columnDescription = Regex.Replace(columnDescription, "\t{2,}", "\t");
            return columnDescription;
        }
        #endregion
    }
}