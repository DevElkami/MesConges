using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages
{
    public class CalendarModel : PageModel
    {
        public static String MAIN_CALENDAR_KEY = "MAIN_CALENDAR";

        public String Calendar { get; set; } = String.Empty;

        public IActionResult OnGet()
        {
            if (Db.Instance.DataBase.ConfigRepository.Get().CustomizeDisplay.DisplayPublicCalendar || HttpContext.User.Identity.IsAuthenticated)
            {
                Calendar = Cache.Get(MAIN_CALENDAR_KEY) as String;
                if (String.IsNullOrEmpty(Calendar))
                {
                    List<User> users = Db.Instance.DataBase.UserRepository.GetAll();
                    foreach (User user in users)
                        Calendar += Toolkit.CalendarFormater(user.Name, Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted));

                    Cache.Set(MAIN_CALENDAR_KEY, Calendar);
                }

                return Page();
            }
            else
                return RedirectToPage("/Index");
        }
    }
}