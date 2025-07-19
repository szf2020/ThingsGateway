using System.Collections;
using System.Linq.Expressions;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 映射字段帮助类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    internal class MappingFieldsHelper<T>
    {
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }
        /// <summary>
        /// 导航实体信息
        /// </summary>
        public EntityInfo NavEntity { get; set; }
        /// <summary>
        /// 根实体信息
        /// </summary>
        public EntityInfo RootEntity { get; set; }

        /// <summary>
        /// 获取映射SQL条件
        /// </summary>
        /// <param name="list">对象列表</param>
        /// <param name="mappingFieldsExpressions">映射字段表达式列表</param>
        /// <returns>条件模型列表</returns>
        public List<IConditionalModel> GetMappingSql(List<object> list, List<MappingFieldsExpression> mappingFieldsExpressions)
        {
            List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
            foreach (var model in list)
            {
                var clist = new List<KeyValuePair<WhereType, ConditionalModel>>();
                var i = 0;
                foreach (var item in mappingFieldsExpressions)
                {
                    InitMappingFieldsExpression(item);
                    clist.Add(new KeyValuePair<WhereType, ConditionalModel>(i == 0 ? WhereType.Or : WhereType.And, new ConditionalModel()
                    {
                        FieldName = item.LeftEntityColumn.DbColumnName,
                        ConditionalType = ConditionalType.Equal,
                        FieldValue = item.RightEntityColumn.PropertyInfo.GetValue(model).ObjToString(),
                        CSharpTypeName = UtilMethods.GetUnderType(item.RightEntityColumn.PropertyInfo.PropertyType).Name
                    }));
                    i++;
                }
                conditionalModels.Add(new ConditionalCollections()
                {
                    ConditionalList = clist
                });
            }
            return conditionalModels;
        }

        /// <summary>
        /// 设置子列表
        /// </summary>
        /// <param name="navColumnInfo">导航列信息</param>
        /// <param name="item">当前对象</param>
        /// <param name="list">对象列表</param>
        /// <param name="mappingFieldsExpressions">映射字段表达式列表</param>
        public void SetChildList(EntityColumnInfo navColumnInfo, object item, List<object> list, List<MappingFieldsExpression> mappingFieldsExpressions)
        {
            if (item != null)
            {
                //var expable =Expressionable.Create<object>();
                List<object> setList = GetSetList(item, list, mappingFieldsExpressions);
                //navColumnInfo.PropertyInfo.SetValue();
                var instance = Activator.CreateInstance(navColumnInfo.PropertyInfo.PropertyType, true);
                var ilist = instance as IList;
                foreach (var value in setList)
                {
                    ilist.Add(value);
                }
                navColumnInfo.PropertyInfo.SetValue(item, ilist);
            }
        }

        /// <summary>
        /// 设置子项
        /// </summary>
        /// <param name="navColumnInfo">导航列信息</param>
        /// <param name="item">当前对象</param>
        /// <param name="list">对象列表</param>
        /// <param name="mappingFieldsExpressions">映射字段表达式列表</param>
        public void SetChildItem(EntityColumnInfo navColumnInfo, object item, List<object> list, List<MappingFieldsExpression> mappingFieldsExpressions)
        {
            if (item != null)
            {
                //var expable =Expressionable.Create<object>();
                List<object> setList = GetSetList(item, list, mappingFieldsExpressions);
                navColumnInfo.PropertyInfo.SetValue(item, setList.LastOrDefault());
            }
        }
        /// <summary>
        /// 获取设置列表
        /// </summary>
        /// <param name="item">当前对象</param>
        /// <param name="list">对象列表</param>
        /// <param name="mappingFieldsExpressions">映射字段表达式列表</param>
        /// <returns>对象列表</returns>
        public List<object> GetSetList(object item, List<object> list, List<MappingFieldsExpression> mappingFieldsExpressions)
        {
            foreach (var field in mappingFieldsExpressions)
            {
                InitMappingFieldsExpression(field);
            }
            var setList = new List<object>();
            var count = mappingFieldsExpressions.Count;
            if (count == 1)
            {
                setList = list.Where(it => GetWhereByIndex(item, mappingFieldsExpressions, it, 0)).ToList();
            }
            else if (count == 2)
            {
                setList = list.Where(it =>
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 0) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 1)
                ).ToList();
            }
            else if (count == 3)
            {
                setList = list.Where(it =>
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 0) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 1) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 2)
                ).ToList();
            }
            else if (count == 4)
            {
                setList = list.Where(it =>
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 0) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 1) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 2) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 3)
                ).ToList();
            }
            else if (count == 5)
            {
                setList = list.Where(it =>
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 0) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 1) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 2) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 3) &&
                 GetWhereByIndex(item, mappingFieldsExpressions, it, 4)
                ).ToList();
            }
            else
            {
                Check.ExceptionEasy("MappingField max value  is  5", "MappingField最大数量不能超过5");
            }

            return setList;
        }

        /// <summary>
        /// 根据索引获取条件
        /// </summary>
        /// <param name="item">当前对象</param>
        /// <param name="mappingFieldsExpressions">映射字段表达式列表</param>
        /// <param name="it">比较对象</param>
        /// <param name="index">索引</param>
        /// <returns>比较结果</returns>
        private static bool GetWhereByIndex(object item, List<MappingFieldsExpression> mappingFieldsExpressions, object it, int index)
        {
            var left = mappingFieldsExpressions[index].LeftEntityColumn.PropertyInfo.GetValue(it).ObjToString();
            var right = mappingFieldsExpressions[index].RightEntityColumn.PropertyInfo.GetValue(item).ObjToString(); ;
            return left == right;
        }

        /// <summary>
        /// 初始化映射字段表达式
        /// </summary>
        /// <param name="item">映射字段表达式</param>
        private void InitMappingFieldsExpression(MappingFieldsExpression item)
        {
            var leftName = item.LeftName;
            var rightName = item.RightName;
            if (item.LeftEntityColumn == null)
            {
                item.LeftEntityColumn = this.NavEntity.Columns.FirstOrDefault(it => it.PropertyName == leftName);
            }
            if (item.RightEntityColumn == null && this.Context != null)
            {
                if (item.RightColumnExpression is LambdaExpression)
                {
                    var body = (item.RightColumnExpression as LambdaExpression).Body;
                    if (body is UnaryExpression)
                    {
                        body = ((UnaryExpression)body).Operand;
                    }
                    if (body is MemberExpression)
                    {
                        var exp = (body as MemberExpression).Expression;
                        if (exp.NodeType == ExpressionType.Parameter)
                        {
                            item.RightEntityColumn = this.Context.EntityMaintenance.GetEntityInfo(exp.Type).Columns.FirstOrDefault(it => it.PropertyName == rightName);
                        }
                    }
                }
                if (item.RightEntityColumn == null)
                    item.RightEntityColumn = this.RootEntity.Columns.FirstOrDefault(it => it.PropertyName == rightName);
            }
        }

    }
    /// <summary>
    /// 映射字段信息
    /// </summary>
    public class MappingFieldsInfo
    {
        /// <summary>
        /// 左侧列信息
        /// </summary>
        public DbColumnInfo LeftColumn { get; set; }
        /// <summary>
        /// 右侧列信息
        /// </summary>
        public DbColumnInfo RightColumn { get; set; }
    }
    /// <summary>
    /// 映射字段表达式
    /// </summary>
    public class MappingFieldsExpression
    {
        /// <summary>
        /// 左侧列表达式
        /// </summary>
        public Expression LeftColumnExpression { get; set; }
        /// <summary>
        /// 右侧列表达式
        /// </summary>
        public Expression RightColumnExpression { get; set; }
        /// <summary>
        /// 左侧实体列信息
        /// </summary>
        public EntityColumnInfo LeftEntityColumn { get; set; }
        /// <summary>
        /// 右侧实体列信息
        /// </summary>
        public EntityColumnInfo RightEntityColumn { get; set; }
        private string _LeftName;
        /// <summary>
        /// 左侧名称
        /// </summary>
        public string LeftName
        {
            get
            {
                if (_LeftName == null)
                {
                    _LeftName = ExpressionTool.GetMemberName(this.LeftColumnExpression);
                }
                return _LeftName;
            }
        }
        private string _RightName;
        /// <summary>
        /// 右侧名称
        /// </summary>
        public string RightName
        {
            get
            {
                if (_RightName == null)
                {
                    _RightName = ExpressionTool.GetMemberName(this.RightColumnExpression);
                }
                return _RightName;
            }
        }
    }
}