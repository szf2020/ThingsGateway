namespace ThingsGateway.SqlSugar
{
    /// <summary>
    /// 数据库维护提供者基类
    /// </summary>
    public abstract partial class DbMaintenanceProvider : IDbMaintenance
    {
        #region Context
        /// <summary>
        /// SQL构建器实例
        /// </summary>
        private ISqlBuilder _SqlBuilder;
        /// <summary>
        /// SqlSugar上下文
        /// </summary>
        public SqlSugarProvider Context { get; set; }
        /// <summary>
        /// SQL构建器
        /// </summary>
        public ISqlBuilder SqlBuilder
        {
            get
            {
                if (_SqlBuilder == null)
                {
                    _SqlBuilder = InstanceFactory.GetSqlbuilder(this.Context.CurrentConnectionConfig);
                    _SqlBuilder.Context = this.Context;
                }
                return _SqlBuilder;
            }
        }
        #endregion

        #region DML
        /// <summary>
        /// 获取视图信息列表SQL
        /// </summary>
        protected abstract string GetViewInfoListSql { get; }
        /// <summary>
        /// 获取数据库列表SQL
        /// </summary>
        protected abstract string GetDataBaseSql { get; }
        /// <summary>
        /// 获取表信息列表SQL
        /// </summary>
        protected abstract string GetTableInfoListSql { get; }
        /// <summary>
        /// 根据表名获取列信息SQL
        /// </summary>
        protected abstract string GetColumnInfosByTableNameSql { get; }
        #endregion

        #region DDL
        /// <summary>
        /// 创建索引SQL
        /// </summary>
        protected abstract string CreateIndexSql { get; }
        /// <summary>
        /// 检查索引是否存在SQL
        /// </summary>
        protected abstract string IsAnyIndexSql { get; }
        /// <summary>
        /// 添加默认值SQL
        /// </summary>
        protected abstract string AddDefaultValueSql { get; }
        /// <summary>
        /// 创建数据库SQL
        /// </summary>
        protected abstract string CreateDataBaseSql { get; }
        /// <summary>
        /// 添加列SQL
        /// </summary>
        protected abstract string AddColumnToTableSql { get; }
        /// <summary>
        /// 修改列SQL
        /// </summary>
        protected abstract string AlterColumnToTableSql { get; }
        /// <summary>
        /// 备份数据库SQL
        /// </summary>
        protected abstract string BackupDataBaseSql { get; }
        /// <summary>
        /// 创建表SQL
        /// </summary>
        protected abstract string CreateTableSql { get; }
        /// <summary>
        /// 创建表列定义SQL
        /// </summary>
        protected abstract string CreateTableColumn { get; }
        /// <summary>
        /// 备份表SQL
        /// </summary>
        protected abstract string BackupTableSql { get; }
        /// <summary>
        /// 清空表SQL
        /// </summary>
        protected abstract string TruncateTableSql { get; }
        /// <summary>
        /// 删除表SQL
        /// </summary>
        protected abstract string DropTableSql { get; }
        /// <summary>
        /// 删除列SQL
        /// </summary>
        protected abstract string DropColumnToTableSql { get; }
        /// <summary>
        /// 删除约束SQL
        /// </summary>
        protected abstract string DropConstraintSql { get; }
        /// <summary>
        /// 添加主键SQL
        /// </summary>
        protected abstract string AddPrimaryKeySql { get; }
        /// <summary>
        /// 重命名列SQL
        /// </summary>
        protected abstract string RenameColumnSql { get; }
        /// <summary>
        /// 添加列注释SQL
        /// </summary>
        protected abstract string AddColumnRemarkSql { get; }
        /// <summary>
        /// 删除列注释SQL
        /// </summary>
        protected abstract string DeleteColumnRemarkSql { get; }
        /// <summary>
        /// 检查列注释是否存在SQL
        /// </summary>
        protected abstract string IsAnyColumnRemarkSql { get; }
        /// <summary>
        /// 添加表注释SQL
        /// </summary>
        protected abstract string AddTableRemarkSql { get; }
        /// <summary>
        /// 删除表注释SQL
        /// </summary>
        protected abstract string DeleteTableRemarkSql { get; }
        /// <summary>
        /// 检查表注释是否存在SQL
        /// </summary>
        protected abstract string IsAnyTableRemarkSql { get; }
        /// <summary>
        /// 重命名表SQL
        /// </summary>
        protected abstract string RenameTableSql { get; }
        /// <summary>
        /// 检查存储过程是否存在SQL
        /// </summary>
        protected virtual string IsAnyProcedureSql { get; }
        #endregion

        #region Check
        /// <summary>
        /// 检查系统表权限SQL
        /// </summary>
        protected abstract string CheckSystemTablePermissionsSql { get; }
        #endregion

        #region Scattered
        /// <summary>
        /// 创建表可空定义
        /// </summary>
        protected abstract string CreateTableNull { get; }
        /// <summary>
        /// 创建表非空定义
        /// </summary>
        protected abstract string CreateTableNotNull { get; }
        /// <summary>
        /// 创建表主键定义
        /// </summary>
        protected abstract string CreateTablePirmaryKey { get; }
        /// <summary>
        /// 创建表自增定义
        /// </summary>
        protected abstract string CreateTableIdentity { get; }
        #endregion
    }
}