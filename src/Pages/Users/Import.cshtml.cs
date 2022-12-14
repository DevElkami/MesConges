using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Users
{
    public class ImportModel : PageModel
    {
        [BindProperty]
        public List<User> Users { get; set; }

        public List<Service> Services { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                User admin = JsonSerializer.Deserialize<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (admin.IsAdmin)
                {
                    Users = Connect.UserAccess.Instance.GetUsers().OrderBy(u => u.Name).ToList();
                    foreach (User user in Users)
                    {
                        User dbUser = Db.Instance.DataBase.UserRepository.Get(user.Email);
                        user.Imported = false;

                        if (dbUser != null)
                        {
                            user.Imported = true;
                            user.ServiceId = dbUser.ServiceId;
                        }
                    }

                    Services = Db.Instance.DataBase.ServiceRepository.GetAll().OrderBy(s => s.Name).ToList();
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

        public ActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                foreach (User user in Users)
                {
                    if (user.Imported && (Db.Instance.DataBase.UserRepository.Get(user.Email) == null))
                    {
                        Toolkit.Log(HttpContext, $"Import en masse: Ajout de l'utilisateur {user.Email}.");
                        Db.Instance.DataBase.UserRepository.Insert(user);
                    }
                    else if ((!user.Imported) && (Db.Instance.DataBase.UserRepository.Get(user.Email) != null))
                    {
                        Toolkit.Log(HttpContext, $"Import en masse: Suppression de l'utilisateur {user.Email}.");
                        Db.Instance.DataBase.UserRepository.Delete(user);
                    }

                    User dataBaseUser = Db.Instance.DataBase.UserRepository.Get(user.Email);
                    if (dataBaseUser != null)
                    {
                        if (dataBaseUser.ServiceId != user.ServiceId)
                        {
                            dataBaseUser.ServiceId = user.ServiceId;
                            Db.Instance.DataBase.UserRepository.Update(dataBaseUser);
                            Toolkit.Log(HttpContext, $"Import en masse: Changement de service de l'utilisateur {user.Email}.");
                        }
                    }
                }
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                ModelState.AddModelError(String.Empty, ErrorMessage);
                return RedirectToPage();
            }
            return RedirectToPage("/Admin");
        }
    }
}