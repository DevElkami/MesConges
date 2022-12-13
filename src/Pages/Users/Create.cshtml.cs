using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Users
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public new User User { get; set; }

        [BindProperty]
        public String Password { get; set; }

        public List<Service> Services = new List<Service>();

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet(String id = null)
        {
            try
            {
                User admin = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (admin.IsAdmin)
                {
                    User = Db.Instance.DataBase.UserRepository.Get(id);
                    Services = Db.Instance.DataBase.ServiceRepository.GetAll();
                }
                else
                    return RedirectToPage("/Index");
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                ModelState.AddModelError(String.Empty, ErrorMessage);
            }

            return Page();
        }

        public IActionResult OnPostAsync()
        {
            try
            {
                if ((Connect.UserAccess.Instance is Connect.DbConnection) && !String.IsNullOrEmpty(Password))
                    User.HashPwd = Toolkit.CreateSHAHash(Password);

                Db.Instance.DataBase.UserRepository.Insert(User);

                Toolkit.Log(HttpContext, $"Création d'un nouvel utilisateur {User.Email}.");
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                return RedirectToPage();
            }
            return RedirectToPage("/Admin");
        }
    }
}
