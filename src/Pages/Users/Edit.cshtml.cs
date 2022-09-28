using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Users
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public new User User { get; set; }

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
                User.Name = Sanitizer.GetSafeHtmlFragment(User.Name);
                User.FamilyName = Sanitizer.GetSafeHtmlFragment(User.FamilyName);
                User.Surname = Sanitizer.GetSafeHtmlFragment(User.Surname);
                User.PhoneNumber = Sanitizer.GetSafeHtmlFragment(User.PhoneNumber);
                User.Description = Sanitizer.GetSafeHtmlFragment(User.Description);
                User.Login = Sanitizer.GetSafeHtmlFragment(User.Login);
                User.Matricule = Sanitizer.GetSafeHtmlFragment(User.Matricule);
                Db.Instance.DataBase.UserRepository.Update(User);
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