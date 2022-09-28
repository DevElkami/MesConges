using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.RH
{
    public class ManageModel : PageModel
    {
        private readonly ILogger<ManageModel> _logger;

        public ManageModel(ILogger<ManageModel> logger)
        {
            _logger = logger;
        }

        public List<KeyValuePair<Service, List<Conge>>> Services { get; set; } = new List<KeyValuePair<Service, List<Conge>>>();

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            User currentUser = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
            if (currentUser.IsDrh)
            {
                foreach (Service service in Db.Instance.DataBase.ServiceRepository.GetAll().OrderBy(u => u.Name).ToList())
                {
                    List<Conge> congesInProgress = new List<Conge>();
                    List<User> users = Db.Instance.DataBase.UserRepository.GetAll(service.Id).OrderBy(u => u.Name).ToList();
                    foreach (User user in users)
                    {
                        List<Conge> congeInProgress = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.InProgress).OrderBy(c => (String)(c.BeginDate.ToString("yyyyMMdd"))).ToList();
                        if (congeInProgress.Count > 0)
                        {
                            foreach (Conge conge in congeInProgress)
                                conge.User = user;

                            congesInProgress.AddRange(congeInProgress);
                        }
                    }

                    Services.Add(new KeyValuePair<Service, List<Conge>>(service, congesInProgress));
                }
                return Page();
            }
            else
                return RedirectToPage("/Index");
        }

        public IActionResult OnPostAcceptAsync(int id)
        {
            try
            {
                User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                Pages.ManageModel.CongeAccept(current.Email, id);
            }
            catch (Exception except)
            {
                _logger.LogError(except.ToString());
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

    }
}