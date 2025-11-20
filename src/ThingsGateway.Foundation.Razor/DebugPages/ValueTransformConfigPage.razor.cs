//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using System.Text.RegularExpressions;

namespace ThingsGateway.Debug;

public partial class ValueTransformConfigPage
{
    private ValueTransformConfig ValueTransformConfig = new();

    [Inject]
    [NotNull]
    private IStringLocalizer<ValueTransformConfig>? ValueTransformConfigLocalizer { get; set; }

    [Parameter]
    public string Expressions { get; set; }

    [Parameter]
    public EventCallback<string> ExpressionsChanged { get; set; }

    public static bool TryParseLinearFormula(string formula, out ValueTransformConfig config)
    {
        config = new ValueTransformConfig();
        var dec = @"[\d]+(?:\.[\d]+)?";

        try
        {
            // None + clamp actual
            var m = Regex.Match(formula, $@"^Math\.Round\(\s*
            Math\.Min\(\s*
                Math\.Max\(\s*
                    raw\.ToDecimal\(\)\s*,\s*({dec})\s*\)\s*,\s*({dec})\s*\)\s*,\s*(\d+)\s*\)$", RegexOptions.IgnorePatternWhitespace);
            if (m.Success)
            {
                config.TransformType = ValueTransformType.None;
                config.ClampToRawRange = true;
                config.ActualMin = decimal.Parse(m.Groups[1].Value);
                config.ActualMax = decimal.Parse(m.Groups[2].Value);
                config.DecimalPlaces = int.Parse(m.Groups[3].Value);
                return true;
            }

            // None pure
            m = Regex.Match(formula, $@"^Math\.Round\(\s*raw\.ToDecimal\(\)\s*,\s*(\d+)\s*\)$");
            if (m.Success)
            {
                config.TransformType = ValueTransformType.None;
                config.ClampToRawRange = false;
                config.DecimalPlaces = int.Parse(m.Groups[1].Value);
                return true;
            }

            // Linear + clamp actual

            m = Regex.Match(formula, $@"^Math\.Round\(\s*
    Math\.Min\(\s*
        Math\.Max\(\s*
            \(\(raw\.ToDecimal\(\)\s*-\s*({dec})\)\s*/\s*
            \(({dec})\s*-\s*({dec})\)\s*\*\s*
            \(({dec})\s*-\s*({dec})\)\s*\+\s*
            ({dec})\)\s*,\s*({dec})\)\s*,\s*({dec})\)\s*,\s*(\d+)\s*\)$", RegexOptions.IgnorePatternWhitespace);
            if (m.Success)
            {
                config.TransformType = ValueTransformType.Linear;
                config.ClampToRawRange = true;
                config.RawMin = decimal.Parse(m.Groups[1].Value);
                config.RawMax = decimal.Parse(m.Groups[2].Value);
                config.ActualMax = decimal.Parse(m.Groups[4].Value);
                config.ActualMin = decimal.Parse(m.Groups[5].Value);
                config.DecimalPlaces = int.Parse(m.Groups[9].Value);
                return true;
            }

            // Linear pure
            m = Regex.Match(formula, $@"^Math\.Round\(\s*
    \(\(raw\.ToDecimal\(\)\s*-\s*({dec})\)\s*/\s*
    \(({dec})\s*-\s*({dec})\)\s*\*\s*
    \(({dec})\s*-\s*({dec})\)\s*\+\s*({dec})\)\s*,\s*(\d+)\s*\)$", RegexOptions.IgnorePatternWhitespace);

            if (m.Success)
            {
                config.TransformType = ValueTransformType.Linear;
                config.ClampToRawRange = false;
                config.RawMin = decimal.Parse(m.Groups[1].Value);    // raw减数（0）
                config.RawMax = decimal.Parse(m.Groups[2].Value);    // 分母第一个数（10）
                config.ActualMax = decimal.Parse(m.Groups[4].Value); // 乘数第一个数（1）
                config.ActualMin = decimal.Parse(m.Groups[6].Value); // 加数（0）
                config.DecimalPlaces = int.Parse(m.Groups[7].Value); // 小数位（2）
                return true;
            }

            // Sqrt + clamp actual
            m = Regex.Match(formula, $@"^Math\.Round\(\s*
            Math\.Min\(\s*
                Math\.Max\(\s*
                    Math\.Sqrt\(Math\.Max\(raw\.ToDecimal\(\),\s*0\)\)\s*\*\s*({dec})\s*,\s*({dec})\)\s*,\s*({dec})\)\s*,\s*(\d+)\s*\)$", RegexOptions.IgnorePatternWhitespace);
            if (m.Success)
            {
                config.TransformType = ValueTransformType.Sqrt;
                config.ClampToRawRange = true;
                config.ActualMax = decimal.Parse(m.Groups[1].Value);
                config.ActualMin = decimal.Parse(m.Groups[2].Value);
                config.DecimalPlaces = int.Parse(m.Groups[4].Value);
                return true;
            }

            // Sqrt pure
            m = Regex.Match(formula, $@"^Math\.Round\(\s*
            Math\.Sqrt\(Math\.Max\(raw\.ToDecimal\(\),\s*0\)\)\s*\*\s*({dec})\s*,\s*(\d+)\s*\)$", RegexOptions.IgnorePatternWhitespace);
            if (m.Success)
            {
                config.TransformType = ValueTransformType.Sqrt;
                config.ClampToRawRange = false;
                config.DecimalPlaces = int.Parse(m.Groups[2].Value);
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    public static string GenerateFormula(ValueTransformConfig config)
    {
        // 只有 ClampToRawRange 为 true 时才包裹实际值范围限制
        string clampActual(string expr)
        {
            if (config.ClampToRawRange)
                return $"Math.Min(Math.Max({expr}, {config.ActualMin}), {config.ActualMax})";
            else
                return expr;
        }

        string rawExpr = "raw.ToDecimal()"; // 这里不做 raw clamp

        switch (config.TransformType)
        {
            case ValueTransformType.None:
                return $"Math.Round({clampActual(rawExpr)}, {config.DecimalPlaces})";

            case ValueTransformType.Linear:
                var linearExpr = $"(({rawExpr} - {config.RawMin}) / ({config.RawMax} - {config.RawMin}) * ({config.ActualMax} - {config.ActualMin}) + {config.ActualMin})";
                return $"Math.Round({clampActual(linearExpr)}, {config.DecimalPlaces})";

            case ValueTransformType.Sqrt:
                var sqrtExpr = $"Math.Sqrt(Math.Max({rawExpr}, 0)) * {config.ActualMax}";
                return $"Math.Round({clampActual(sqrtExpr)}, {config.DecimalPlaces})";

            default:
                throw new NotSupportedException($"Unsupported transform type: {config.TransformType}");
        }
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(Expressions))
        {
            if (TryParseLinearFormula(Expressions, out var config))
            {
                ValueTransformConfig = config;
            }
        }
        base.OnParametersSet();
    }

    #region 修改
    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    private async Task OnSave(EditContext editContext)
    {
        try
        {
            var result = GenerateFormula(ValueTransformConfig);
            Expressions = result;
            if (ExpressionsChanged.HasDelegate)
            {
                await ExpressionsChanged.InvokeAsync(result);
            }
            else
            {
                Expressions = result;
            }
            if (OnCloseAsync != null)
                await OnCloseAsync();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    [Inject]
    ToastService ToastService { get; set; }

    [Inject]
    IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }
    #endregion 修改
}
