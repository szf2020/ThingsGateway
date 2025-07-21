using System.Reflection;

namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 分表代码优先提供者
    /// </summary>
    public class SplitCodeFirstProvider
    {
        /// <summary>
        /// SqlSugar提供者实例
        /// </summary>
        public SqlSugarProvider Context;

        /// <summary>
        /// 默认字符串长度
        /// </summary>
        public int DefaultLength { get; set; }

        /// <summary>
        /// 初始化表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        public void InitTables<T>()
        {
            var type = typeof(T);
            InitTables(type);
        }

        /// <summary>
        /// 初始化表
        /// </summary>
        /// <param name="type">实体类型</param>
        public void InitTables(Type type)
        {
            var isSplitEntity = type.GetCustomAttribute<SplitTableAttribute>() != null;
            if (isSplitEntity)
            {
                UtilMethods.StartCustomSplitTable(this.Context, type);
                _InitTables(type);
                UtilMethods.EndCustomSplitTable(this.Context, type);
            }
            else
            {
                this.Context.CodeFirst.SetStringDefaultLength(this.DefaultLength).InitTables(type);
            }
        }
        /// <summary>
        /// 内部初始化表方法
        /// </summary>
        /// <param name="type">实体类型</param>
        private void _InitTables(Type type)
        {
            //var oldMapping = this.Context.Utilities.TranslateCopy(this.Context.MappingTables);
            SplitTableContext helper = new SplitTableContext(Context)
            {
                EntityInfo = this.Context.EntityMaintenance.GetEntityInfo(type)
            };
            helper.CheckPrimaryKey();
            var tables = helper.GetTables();
            //var oldMapingTables = this.Context.MappingTables;
            if (tables.Count > 0)
            {
                foreach (var item in tables)
                {
                    this.Context.MappingTables.Add(helper.EntityInfo.EntityName, item.TableName);
                    this.Context.CodeFirst.SetStringDefaultLength(this.DefaultLength).InitTables(type);
                }
            }
            else
            {
                this.Context.MappingTables.Add(helper.EntityInfo.EntityName, helper.GetDefaultTableName());
                this.Context.CodeFirst.SetStringDefaultLength(this.DefaultLength).InitTables(type);
            }
            this.Context.MappingTables.Add(helper.EntityInfo.EntityName, helper.EntityInfo.DbTableName);
        }

        /// <summary>
        /// 初始化多个表
        /// </summary>
        /// <param name="types">实体类型数组</param>
        public void InitTables(params Type[] types)
        {
            foreach (var type in types)
            {
                InitTables(type);
            }
        }
    }
}