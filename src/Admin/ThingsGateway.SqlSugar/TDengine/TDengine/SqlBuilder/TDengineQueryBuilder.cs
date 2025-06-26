using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// TDengine 查询构建器
    /// </summary>
    public partial class TDengineQueryBuilder : QueryBuilder
    {
        #region Sql Template
        /// <summary>
        /// 获取分页模板
        /// </summary>
        public override string PageTempalte
        {
            get
            {
                /*
                 SELECT * FROM TABLE WHERE CONDITION ORDER BY ID DESC LIMIT 10 offset 0
                 */
                var template = "SELECT {0} FROM {1} {2} {3} {4} LIMIT {6} offset {5}";
                return template;
            }
        }

        /// <summary>
        /// 获取默认排序模板
        /// </summary>
        public override string DefaultOrderByTemplate
        {
            get
            {
                return "ORDER BY NOW() ";
            }
        }
        #endregion

        #region Common Methods
        /// <summary>
        /// 获取表名字符串
        /// </summary>
        public override string GetTableNameString
        {
            get
            {
                if (this.TableShortName != null && this.Context.CurrentConnectionConfig?.MoreSettings?.PgSqlIsAutoToLower == false)
                {
                    this.TableShortName = Builder.GetTranslationColumnName(this.TableShortName);
                }
                return base.GetTableNameString;
            }
        }

        /// <summary>
        /// 判断是否为复杂模型
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为复杂模型</returns>
        public override bool IsComplexModel(string sql)
        {
            return Regex.IsMatch(sql, @"AS ""\w+\.\w+""") || Regex.IsMatch(sql, @"AS ""\w+\.\w+\.\w+""");
        }

        /// <summary>
        /// 转换为SQL字符串
        /// </summary>
        /// <returns>SQL字符串</returns>
        public override string ToSqlString()
        {
            base.AppendFilter();
            string oldOrderValue = this.OrderByValue;
            string result = null;
            sql = new StringBuilder();
            sql.AppendFormat(SqlTemplate, GetSelectValue, GetTableNameString, GetWhereValueString, GetGroupByString + HavingInfos, (Skip != null || Take != null) ? null : GetOrderByString);
            if (IsCount) { return sql.ToString(); }
            if (Skip != null && Take == null)
            {
                if (this.OrderByValue == "ORDER BY ") this.OrderByValue += GetSelectValue.Split(',')[0];
                result = string.Format(PageTempalte, GetSelectValue, GetTableNameString, GetWhereValueString, GetGroupByString + HavingInfos, (Skip != null || Take != null) ? null : GetOrderByString, Skip.ObjToInt(), long.MaxValue);
            }
            else if (Skip == null && Take != null)
            {
                if (this.OrderByValue == "ORDER BY ") this.OrderByValue += GetSelectValue.Split(',')[0];
                result = string.Format(PageTempalte, GetSelectValue, GetTableNameString, GetWhereValueString, GetGroupByString + HavingInfos, GetOrderByString, 0, Take.ObjToInt());
            }
            else if (Skip != null && Take != null)
            {
                if (this.OrderByValue == "ORDER BY ") this.OrderByValue += GetSelectValue.Split(',')[0];
                result = string.Format(PageTempalte, GetSelectValue, GetTableNameString, GetWhereValueString, GetGroupByString + HavingInfos, GetOrderByString, Skip.ObjToInt() > 0 ? Skip.ObjToInt() : 0, Take);
            }
            else
            {
                result = sql.ToString();
            }
            this.OrderByValue = oldOrderValue;
            result = GetSqlQuerySql(result);
            if (result.IndexOf("-- No table") > 0)
            {
                return "-- No table";
            }
            if (TranLock != null)
            {
                result = result + TranLock;
            }
            return result;
        }
        #endregion

        #region Get SQL Partial
        /// <summary>
        /// 获取选择值
        /// </summary>
        public override string GetSelectValue
        {
            get
            {
                string result = string.Empty;
                if (this.SelectValue == null || this.SelectValue is string)
                {
                    result = GetSelectValueByString();
                }
                else
                {
                    result = GetSelectValueByExpression();
                }
                if (this.SelectType == ResolveExpressType.SelectMultiple)
                {
                    this.SelectCacheKey = this.SelectCacheKey + string.Join("-", this.JoinQueryInfos.Select(it => it.TableName));
                }
                if (IsDistinct)
                {
                    result = "distinct " + result;
                }
                if (this.SubToListParameters?.Count > 0)
                {
                    result = SubToListMethod(result);
                }
                return result;
            }
        }
        #endregion
    }
}