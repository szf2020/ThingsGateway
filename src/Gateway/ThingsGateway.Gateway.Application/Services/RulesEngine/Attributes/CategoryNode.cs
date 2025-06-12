using Microsoft.Extensions.Localization;

namespace ThingsGateway.Gateway.Application;

public sealed class CategoryNode : Attribute
{
    public string WidgetType { get; set; }
    public string ImgUrl { get; set; } = "ImgUrl";
    public string Desc { get; set; } = "Desc";
    public string Category { get; set; } = "Other";
    public Type LocalizerType { get; set; }
    public IStringLocalizer StringLocalizer;
}
