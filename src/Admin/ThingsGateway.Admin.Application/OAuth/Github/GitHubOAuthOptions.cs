using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

using System.Net.Http.Headers;
using System.Text.Json;

using ThingsGateway.NewLife.Log;

namespace ThingsGateway.Admin.Application;

public class GitHubOAuthOptions : AdminOAuthOptions
{
    INoticeService _noticeService;
    IVerificatInfoService _verificatInfoService;
    public GitHubOAuthOptions() : base()
    {
        _noticeService = App.GetService<INoticeService>();
        _verificatInfoService = App.GetService<IVerificatInfoService>();
        SignInScheme = ClaimConst.Scheme;
        AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        TokenEndpoint = "https://github.com/login/oauth/access_token";
        UserInformationEndpoint = "https://api.github.com/user";
        HomePath = "/";
        CallbackPath = "/signin-github";

        Scope.Add("read:user");
        Scope.Add("public_repo"); // 需要用于 Star 仓库

        Events.OnCreatingTicket = async context => await HandleGitHubStarAsync(context).ConfigureAwait(false);

        Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        Events.OnRemoteFailure = context =>
        {
            XTrace.WriteException(context.Failure);
            return Task.CompletedTask;
        };
    }

    protected override void ConfigureClaims()
    {
        ClaimActions.MapJsonKey(ClaimConst.AvatarUrl, "avatar_url");
        ClaimActions.MapJsonKey(ClaimConst.Account, "login");

        base.ConfigureClaims();
    }

    public override string GetName(JsonElement element)
    {
        if (element.TryGetProperty("login", out var loginProp))
        {
            return loginProp.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    private async Task HandleGitHubStarAsync(OAuthCreatingTicketContext context, string repoFullName = "ThingsGateway/ThingsGateway")
    {
        if (string.IsNullOrWhiteSpace(context.AccessToken))
            throw new InvalidOperationException("Access token is missing.");

        var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.github.com/user/starred/{repoFullName}")
        {
            Headers =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/vnd.github+json") },
                Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken),
            },
            Content = new StringContent(string.Empty) // GitHub Star 接口需要空内容
        };
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("ThingsGateway", "1.0")); // GitHub API 要求 User-Agent

        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var id = context.Identity.Claims.FirstOrDefault(a => a.Type == ClaimConst.VerificatId).Value;

            var verificatInfoIds = _verificatInfoService.GetOne(id.ToLong());
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000).ConfigureAwait(false);
                await _noticeService.NavigationMesage(verificatInfoIds.ClientIds, "https://github.com/ThingsGateway/ThingsGateway", "创作不易，如有帮助请star仓库").ConfigureAwait(false);
            });
        }
    }

    /// <summary>处理用户信息方法</summary>
    public override async Task<JsonElement> HandleUserInfoAsync(HttpContext context, OAuthTokenResponse tokens)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("ThingsGateway", "1.0")); // GitHub API 要求 User-Agent
        var response = await Backchannel.SendAsync(request, context.RequestAborted).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return JsonDocument.Parse(content).RootElement;
        }

        throw new OAuthTokenException($"OAuth user info endpoint failure: {await Display(response).ConfigureAwait(false)}");
    }
}
