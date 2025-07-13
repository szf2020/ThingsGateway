//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.DB;

/// <summary>
/// 主键id基类
/// </summary>
public abstract class PrimaryIdEntity : IPrimaryIdEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false, Sortable = true, DefaultSort = true, DefaultSortOrder = SortOrder.Asc)]
    public virtual long Id { get; set; }
}

/// <summary>
/// 主键实体基类
/// </summary>
public abstract class PrimaryKeyEntity : PrimaryIdEntity
{
    /// <summary>
    /// 拓展信息
    /// </summary>
    [SugarColumn(ColumnDescription = "扩展信息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string ExtJson { get; set; }
}

public interface IBaseEntity
{
    DateTime CreateTime { get; set; }
    string CreateUser { get; set; }
    long CreateUserId { get; set; }
    bool IsDelete { get; set; }
    int SortCode { get; set; }
    DateTime UpdateTime { get; set; }
    string UpdateUser { get; set; }
    long UpdateUserId { get; set; }
}

/// <summary>
/// 框架实体基类
/// </summary>
public abstract class BaseEntity : PrimaryKeyEntity, IBaseEntity
{
    private long createUserId;
    private long updateUserId;
    private DateTime createTime;
    private DateTime updateTime;
    private int sortCode;
    private bool isDelete = false;
    private string createUser;
    private string updateUser;

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime CreateTime { get => createTime; set => createTime = value; }

    /// <summary>
    /// 创建人
    /// </summary>
    [SugarColumn(ColumnDescription = "创建人", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [NotNull]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string CreateUser { get => createUser; set => createUser = value; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long CreateUserId { get => createUserId; set => createUserId = value; }

    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual bool IsDelete { get => isDelete; set => isDelete = value; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime UpdateTime { get => updateTime; set => updateTime = value; }

    /// <summary>
    /// 更新人
    /// </summary>
    [SugarColumn(ColumnDescription = "更新人", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string UpdateUser { get => updateUser; set => updateUser = value; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long UpdateUserId { get => updateUserId; set => updateUserId = value; }

    /// <summary>
    /// 排序码
    ///</summary>
    [SugarColumn(ColumnDescription = "排序码", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, DefaultSort = true, Sortable = true, DefaultSortOrder = SortOrder.Asc)]
    [IgnoreExcel]
    public virtual int SortCode { get => sortCode; set => sortCode = value; }
}

public interface IBaseDataEntity
{
    long CreateOrgId { get; set; }
}

/// <summary>
/// 业务数据实体基类(数据权限)
/// </summary>
public abstract class BaseDataEntity : BaseEntity, IBaseDataEntity
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual long CreateOrgId { get; set; }
}
