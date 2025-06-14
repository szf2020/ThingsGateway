using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;

using System.Net.Http.Headers;
using System.Text.Json;

using ThingsGateway.NewLife.Log;

namespace ThingsGateway.Admin.Application;

public class GiteeOAuthOptions : AdminOAuthOptions
{
    INoticeService _noticeService;
    IVerificatInfoService _verificatInfoService;
    public GiteeOAuthOptions() : base()
    {
        _noticeService = App.GetService<INoticeService>();
        _verificatInfoService = App.GetService<IVerificatInfoService>();
        this.SignInScheme = ClaimConst.Scheme;
        this.AuthorizationEndpoint = "https://gitee.com/oauth/authorize";
        this.TokenEndpoint = "https://gitee.com/oauth/token";
        this.UserInformationEndpoint = "https://gitee.com/api/v5/user";
        this.HomePath = "/";
        this.CallbackPath = "/signin-gitee";
        Scope.Add("user_info");
        Scope.Add("projects");

        Events.OnCreatingTicket = async context =>
        {
            await HandlerGiteeStarredUrl(context).ConfigureAwait(false);
        };

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

    /// <summary>刷新 Token 方法</summary>
    protected virtual async Task<OAuthTokenResponse> RefreshTokenAsync(TicketReceivedContext ticketReceivedContext, string refreshToken)
    {
        var query = new Dictionary<string, string>
        {
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, QueryHelpers.AddQueryString(TokenEndpoint, query));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await Backchannel.SendAsync(request, ticketReceivedContext.HttpContext.RequestAborted).ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return OAuthTokenResponse.Success(JsonDocument.Parse(content));
        }

        return OAuthTokenResponse.Failed(new OAuthTokenException($"OAuth token endpoint failure: {await Display(response).ConfigureAwait(false)}"));
    }



    public override string GetName(JsonElement element)
    {
        JsonElement.ObjectEnumerator target = element.EnumerateObject();
        return target.TryGetValue("name");
    }

    private async Task HandlerGiteeStarredUrl(OAuthCreatingTicketContext context, string repoFullName = "ThingsGateway/ThingsGateway")
    {
        if (string.IsNullOrWhiteSpace(context.AccessToken))
            throw new InvalidOperationException("Access token is missing.");

        var uri = $"https://gitee.com/api/v5/user/starred/{repoFullName}";

        var queryString = new Dictionary<string, string>
        {
            { "access_token", context.AccessToken }
        };

        var request = new HttpRequestMessage(HttpMethod.Get, QueryHelpers.AddQueryString(uri, queryString))
        {
            Headers = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } }
        };

        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var id = context.Identity.Claims.FirstOrDefault(a => a.Type == ClaimConst.VerificatId).Value;

            var verificatInfoIds = _verificatInfoService.GetOne(id.ToLong());

            _ = Task.Run(async () =>
             {
                 await Task.Delay(5000).ConfigureAwait(false);
                 await _noticeService.NavigationMesage(verificatInfoIds.ClientIds, "https://gitee.com/ThingsGateway/ThingsGateway", "创作不易，如有帮助请star仓库").ConfigureAwait(false);
             });
            //throw new Exception($"Failed to star repository: {response.StatusCode}, {content}");
        }


    }
    protected override void ConfigureClaims()
    {
        ClaimActions.MapJsonKey(ClaimConst.AvatarUrl, "avatar_url");
        ClaimActions.MapJsonKey(ClaimConst.Account, "name");

        base.ConfigureClaims();
    }
}
