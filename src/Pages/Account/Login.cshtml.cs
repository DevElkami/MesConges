using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [RegularExpression("[^<>:='\"]*")]
            [MaxLength(50)]
            public String Login { get; set; }

            [Required]
            [MaxLength(50)]
            [DataType(DataType.Password)]
            public String Password { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                User user = AuthenticateUser(Input.Login, Input.Password);
                if (user == null)
                {
                    ModelState.AddModelError(String.Empty, "Mauvais login ou mauvais mot de passe.");
                    return Page();
                }

                user.Login = Input.Login;

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("CurrentUser", JsonConvert.SerializeObject(user))
                };

                if (user.IsAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "admin"));
                    claims.Add(new Claim("adminextra", Input.Password));
                }

                if (user.IsManager)
                    claims.Add(new Claim(ClaimTypes.Role, "manager"));

                if (user.IsDrh)
                    claims.Add(new Claim(ClaimTypes.Role, "drh"));

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    //IsPersistent = true,
                    // Whether the authentication session is persisted across 
                    // multiple requests. Required when setting the 
                    // ExpireTimeSpan option of CookieAuthenticationOptions 
                    // set with AddCookie. Also required when setting 
                    // ExpiresUtc.

                    //IssuedUtc = <DateTimeOffset>,
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };

                // Mise à jour de la date de dernière connexion
                user.LastConnection = DateTime.Now;
                Db.Instance.DataBase.UserRepository.Update(user);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation($"User {user.Email} logged in at {DateTime.UtcNow}.");

                return LocalRedirect(Url.GetLocalUrl(returnUrl));
            }

            // Something failed. Redisplay the form.
            return Page();
        }

        private User AuthenticateUser(String login, String password)
        {
            // Cas de l'admin local => Tous les droits
            var CheckLocalAdminCredential = () =>
            {
                try
                {
                    if ((login == Db.Instance.DataBase.ConfigRepository.Get().AppAdminLogin) &&
                        (Toolkit.CreateSHAHash(password) == Db.Instance.DataBase.ConfigRepository.Get().AppAdminPwd))
                    {
                        User user = new User();
                        user.FamilyName = Db.Instance.DataBase.ConfigRepository.Get().AppAdminLogin;
                        user.Email = Db.Instance.DataBase.ConfigRepository.Get().AppAdminEmail;
                        user.IsAdmin = true;
                        user.IsDrh = true;
                        return user;
                    }
                }
                catch (Exception except)
                {
                    _logger.LogError(except.Message);
                }

                return null;
            };

            try
            {
                // App administrator login
                User currentUser = CheckLocalAdminCredential();
                if (currentUser != null)
                    return currentUser;

                // Other login (ldap, database)
                currentUser = Connect.UserAccess.Instance.Connect(login, password);
                if (currentUser != null)
                {
                    User user = Db.Instance.DataBase.UserRepository.Get(currentUser.Email);
                    if (user != null)
                        return Db.Instance.BeautifyUser(user);
                    else
                        return null;
                }
            }
            catch (Exception except)
            {
                _logger.LogError(except.Message);
            }

            return CheckLocalAdminCredential();
        }
    }
}
