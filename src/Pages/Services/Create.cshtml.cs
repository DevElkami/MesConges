using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Services
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Service Service { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            try
            {
                User admin = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (!admin.IsAdmin)
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
                Db.Instance.DataBase.ServiceRepository.Insert(Service);
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