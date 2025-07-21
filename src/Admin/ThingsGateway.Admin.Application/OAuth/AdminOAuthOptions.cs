using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ThingsGateway.Admin.Application;

/// <summary>OAuthOptions 配置类</summary>
public abstract class AdminOAuthOptions : OAuthOptions
{
    /// <summary>默认构造函数</summary>
    protected AdminOAuthOptions()
    {
        ConfigureClaims();
        this.Events.OnRemoteFailure = context =>
        {
            var redirectUri = string.IsNullOrEmpty(HomePath) ? "/" : HomePath;
            context.Response.Redirect(redirectUri);
            context.HandleResponse();
            return Task.CompletedTask;
        };

        Backchannel = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        Backchannel.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ThingsGateway", "1.0"));
    }

    /// <summary>配置 Claims 映射</summary>
    protected virtual void ConfigureClaims()
    {
    }

    public virtual string GetName(JsonElement element)
    {
        JsonElement.ObjectEnumerator target = element.EnumerateObject();
        return target.TryGetValue("name");
    }

    /// <summary>获得/设置 登陆后首页</summary>
    public string HomePath { get; set; } = "/";

    /// <summary>处理用户信息方法</summary>
    public virtual async Task<JsonElement> HandleUserInfoAsync(HttpContext context, OAuthTokenResponse tokens)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUserInfoUrl(tokens));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await Backchannel.SendAsync(request, context.RequestAborted).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return JsonDocument.Parse(content).RootElement;
        }

        throw new OAuthTokenException($"OAuth user info endpoint failure: {await Display(response).ConfigureAwait(false)}");
    }

    /// <summary>生成用户信息请求地址方法</summary>
    protected virtual string BuildUserInfoUrl(OAuthTokenResponse tokens)
    {
        return QueryHelpers.AddQueryString(UserInformationEndpoint, new Dictionary<string, string>
        {
            { "access_token", tokens.AccessToken }
        });
    }
    /// <summary>生成错误信息方法</summary>
    protected async Task<string> Display(HttpResponseMessage response)
    {
        var output = new StringBuilder();
        output.Append($"Status: {response.StatusCode}; ");
        output.Append($"Headers: {response.Headers}; ");
        output.Append($"Body: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)};");

        return output.ToString();
    }
}
