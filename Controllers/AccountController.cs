using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TruckTaxPro.Data;

namespace Trucktaxpro.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    [HttpGet("login")]
    public IActionResult LoginWithGoogle([FromServices] SignInManager<ApplicationUser> signInManager)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback));
        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            GoogleDefaults.AuthenticationScheme, redirectUrl);
        return new ChallengeResult(GoogleDefaults.AuthenticationScheme, properties);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromServices] SignInManager<ApplicationUser> signInManager,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (signInResult.Succeeded)
        {
            return Redirect("/");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = name };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        await userManager.AddLoginAsync(user, info);
        await signInManager.SignInAsync(user, isPersistent: false);

        return Redirect("/");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}