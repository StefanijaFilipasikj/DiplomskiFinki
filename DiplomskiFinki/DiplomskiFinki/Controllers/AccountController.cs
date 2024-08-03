    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;


    namespace DiplomskiFinki.Controllers
    {
        public class AccountController : Controller
        {
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly ILogger _logger;

            public AccountController(ILoggerFactory loggerFactory, SignInManager<IdentityUser> signInManager)
            {
                _logger = loggerFactory.CreateLogger<AccountController>();
                _signInManager = signInManager;
            }

            public IActionResult Index()
            {
                return View();
            }

            // POST: /Account/ExternalLogin
            [HttpPost]
            [AllowAnonymous]
            [ValidateAntiForgeryToken]
            public IActionResult ExternalLogin(string provider = "OpenIdConnect", string returnUrl = null)
            {
                // Request a redirect to the external login provider.
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            // GET: /Account/ExternalLoginCallback
            [HttpGet]
            [AllowAnonymous]
            public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
            {
                //if (remoteError != null)
                //{
                //    ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                //    return View(nameof(Login));
                //}
                var info = await _signInManager.GetExternalLoginInfoAsync();

                //if (info == null)
                //{
                //    return RedirectToAction(nameof(Login));
                //}

                // Sign in the user with this external login provider if the user already has a login.

                // TODO: fix this
                //var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
                //if (result.Succeeded)
                //{
                //    _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                //    return RedirectToLocal(returnUrl);
                //}

                return RedirectToAction("Index", "Home");
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
                //}
                //if (result.IsLockedOut)
                //{
                //    return View("Lockout");
                //}
                //else
                //{
                //    // If the user does not have an account, then ask the user to create an account.
                //    ViewData["ReturnUrl"] = returnUrl;
                //    ViewData["LoginProvider"] = info.LoginProvider;
                //    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                //    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
                //}
            }


            // POST: /Account/Logout
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Logout()
            {

                //var cookiesBefore = HttpContext.Request.Cookies;
                //_logger.LogInformation("Cookies before sign out: {0}", string.Join(", ", cookiesBefore.Select(c => $"{c.Key}: {c.Value}")));


                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            

                var httpClient = new HttpClient();
                var disco = await httpClient.GetDiscoveryDocumentAsync("https://is.iknow.ukim.mk");

                return Redirect(disco.EndSessionEndpoint);

                //var cookiesAfter = HttpContext.Request.Cookies;
                //_logger.LogInformation("Cookies after sign out: {0}", string.Join(", ", cookiesAfter.Select(c => $"{c.Key}: {c.Value}")));
                //_logger.LogInformation(4, "User logged out.");
                //return RedirectToAction("Index", "Home");
            }

            private IActionResult RedirectToLocal(string returnUrl)
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }
        }
    }
