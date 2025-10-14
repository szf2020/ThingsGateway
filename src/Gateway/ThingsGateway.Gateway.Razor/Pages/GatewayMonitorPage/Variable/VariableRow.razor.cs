//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.NewLife.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableRow : IDisposable
{
    [Parameter]
    public TableRowContext<VariableRuntime>? RowContent { get; set; }
    private bool Disposed;
    public void Dispose()
    {
        Disposed = true;
        timer?.SafeDispose();
        GC.SuppressFinalize(this);
    }
    TimerX? timer;
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            timer = new TimerX(Refresh, null, 1000, 1000, "VariableRow");
        }
        base.OnAfterRender(firstRender);
    }

    private Task Refresh(object? state)
    {
        if (!Disposed)
            return InvokeAsync(StateHasChanged);
        else
            return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        FixedCellClassStringCache?.Clear();
        CellClassStringCache?.Clear();
        base.OnParametersSet();
    }
    /// <summary>
    /// 获得 指定单元格数据方法
    /// </summary>
    /// <param name="col"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    protected static RenderFragment GetValue(ITableColumn col, VariableRuntime item) => builder =>
    {
        if (col.Template != null)
        {
            builder.AddContent(0, col.Template(item));
        }
        else if (col.ComponentType == typeof(ColorPicker))
        {
            // 自动化处理 ColorPicker 组件
            builder.AddContent(10, col.RenderColor(item));
        }
        else
        {
            builder.AddContent(20, col.RenderValue(item));
        }
    };

    //    internal static string? GetDoubleClickCellClassString(bool trigger) => CssBuilder.Default()
    //.AddClass("is-dbcell", trigger)
    //.Build();

    /// <summary>
    /// 获得指定列头固定列样式
    /// </summary>
    /// <param name="col"></param>
    /// <param name="margin"></param>
    /// <returns></returns>
    protected string? GetFixedCellStyleString(ITableColumn col, int margin = 0)
    {
        string? ret = null;
        if (col.Fixed)
        {
            ret = IsTail(col) ? GetRightStyle(col, margin) : GetLeftStyle(col);
        }
        return ret;
    }

    private string? GetLeftStyle(ITableColumn col)
    {
        var columns = RowContent.Columns.ToList();
        var defaultWidth = 200;
        var width = 0;
        var start = 0;
        var index = columns.IndexOf(col);
        //if (GetFixedDetailRowHeaderColumn)
        //{
        //    width += DetailColumnWidth;
        //}
        //if (GetFixedMultipleSelectColumn)
        //{
        //    width += MultiColumnWidth;
        //}
        if (GetFixedLineNoColumn)
        {
            width += LineNoColumnWidth;
        }
        while (index > start)
        {
            var column = columns[start++];
            width += column.Width ?? defaultWidth;
        }
        return $"left: {width}px;";
    }
    private bool GetFixedLineNoColumn = false;

    private string? GetRightStyle(ITableColumn col, int margin)
    {
        var columns = RowContent.Columns.ToList();
        var defaultWidth = 200;
        var width = 0;
        var index = columns.IndexOf(col);

        // after
        while (index + 1 < columns.Count)
        {
            var column = columns[index++];
            width += column.Width ?? defaultWidth;
        }
        //if (ShowExtendButtons && FixedExtendButtonsColumn)
        {
            width += ExtendButtonColumnWidth;
        }

        // 如果是固定表头时增加滚动条位置
        if (IsFixedHeader && (index + 1) == columns.Count)
        {
            width += margin;
        }
        return $"right: {width}px;";
    }
    private bool IsFixedHeader = true;

    public int LineNoColumnWidth { get; set; } = 60;
    public int ExtendButtonColumnWidth { get; set; } = 220;


    private bool IsTail(ITableColumn col)
    {
        var middle = Math.Floor(RowContent.Columns.Count() * 1.0 / 2);
        var index = Columns.IndexOf(col);
        return middle < index;
    }
    private ConcurrentDictionary<ITableColumn, string> CellClassStringCache { get; } = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// 获得 Cell 文字样式
    /// </summary>
    /// <param name="col"></param>
    /// <param name="hasChildren"></param>
    /// <param name="inCell"></param>
    /// <returns></returns>
    protected string? GetCellClassString(ITableColumn col, bool hasChildren, bool inCell)
    {
        if (CellClassStringCache.TryGetValue(col, out var cached))
        {
            return cached;
        }
        else
        {
            bool trigger = false;
            return CellClassStringCache.GetOrAdd(col, col => CssBuilder.Default("table-cell")
 .AddClass(col.GetAlign().ToDescriptionString(), col.Align == Alignment.Center || col.Align == Alignment.Right)
 .AddClass("is-wrap", col.GetTextWrap())
 .AddClass("is-ellips", col.GetTextEllipsis())
 .AddClass("is-tips", col.GetShowTips())
 .AddClass("is-resizable", AllowResizing)
 .AddClass("is-tree", IsTree && hasChildren)
 .AddClass("is-incell", inCell)
.AddClass("is-dbcell", trigger)
 .AddClass(col.CssClass)
 .Build());
        }

    }

    private bool AllowResizing = true;
    private bool IsTree = false;

    private ConcurrentDictionary<ITableColumn, string> FixedCellClassStringCache { get; } = new(ReferenceEqualityComparer.Instance);
    /// <summary>
    /// 获得指定列头固定列样式
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    protected string? GetFixedCellClassString(ITableColumn col)
    {
        if (FixedCellClassStringCache.TryGetValue(col, out var cached))
        {
            return cached;
        }
        else
        {
            return FixedCellClassStringCache.GetOrAdd(col, col => CssBuilder.Default()
    .AddClass("fixed", col.Fixed)
    .AddClass("fixed-right", col.Fixed && IsTail(col))
    .AddClass("fr", IsLastColumn(col))
    .AddClass("fl", IsFirstColumn(col))
    .Build());

        }

    }


    [Parameter]
    public Func<List<ITableColumn>> ColumnsFunc { get; set; }
    public List<ITableColumn> Columns => ColumnsFunc();

    private ConcurrentDictionary<ITableColumn, bool> LastFixedColumnCache { get; } = new(ReferenceEqualityComparer.Instance);
    private bool IsLastColumn(ITableColumn col)
    {
        if (LastFixedColumnCache.TryGetValue(col, out var cached))
        {
            return cached;
        }
        else
        {
            return LastFixedColumnCache.GetOrAdd(col, col =>
            {
                var ret = false;
                if (col.Fixed && !IsTail(col))
                {
                    var index = Columns.IndexOf(col) + 1;
                    ret = index < Columns.Count && Columns[index].Fixed == false;
                }
                return ret;
            });

        }
    }
    private ConcurrentDictionary<ITableColumn, bool> FirstFixedColumnCache { get; } = new(ReferenceEqualityComparer.Instance);
    private bool IsFirstColumn(ITableColumn col)
    {
        if (FirstFixedColumnCache.TryGetValue(col, out var cached))
        {
            return cached;
        }
        else
        {
            return FirstFixedColumnCache.GetOrAdd(col, col =>
            {
                var ret = false;
                if (col.Fixed && IsTail(col))
                {
                    // 查找前一列是否固定
                    var index = Columns.IndexOf(col) - 1;
                    if (index > 0)
                    {
                        ret = !Columns[index].Fixed;
                    }
                }
                return ret;
            });

        }
    }



}
