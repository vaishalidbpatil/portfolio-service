using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Portfolio.TradeOrderService.Auth.Pages.Account;

public class LoginModel : PageModel
{
    private readonly TestUserStore _users;
    private readonly IIdentityServerInteractionService _interaction;

    public LoginModel(TestUserStore users, IIdentityServerInteractionService interaction)
    {
        _users = users;
        _interaction = interaction;
    }

    [BindProperty] public string? Username { get; set; }
    [BindProperty] public string? Password { get; set; }
    [BindProperty] public string? ReturnUrl { get; set; }
    public string? Error { get; private set; }

    public void OnGet(string? returnUrl) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl, ct);

        if (!_users.ValidateCredentials(Username!, Password!))
        {
            Error = "Invalid username or password.";
            return Page();
        }

        var user = _users.FindByUsername(Username!);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user!.SubjectId),
            new(ClaimTypes.Name, user.Username)
        };
        foreach (var c in user.Claims) claims.Add(c);

        var identity = new ClaimsIdentity(claims, IdentityServerConstants.DefaultCookieAuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme, principal, props);

        if (context is not null || Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl ?? "/");

        return Redirect("/");
    }
}
