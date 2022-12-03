using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public String UserName
        {
            get
            {
                return JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value).Surname;
            }
        }

        public String PreviousConnection { get; set; }

        public List<Conge> CongesInProgress { get; set; } = new List<Conge>();
        public List<Conge> CongesValidated { get; set; } = new List<Conge>();
        public List<Conge> CongesRefused { get; set; } = new List<Conge>();

        [TempData]
        public String ErrorMessage { get; set; }

        public void OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            User user = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);

            PreviousConnection = user.LastConnection.ToString("D", CultureInfo.GetCultureInfo("fr-FR")) + " à " + user.LastConnection.ToString("T", CultureInfo.GetCultureInfo("fr-FR"));

            CongesInProgress = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.InProgress).OrderBy(c => (String)(c.BeginDate.ToString("yyyyMMdd"))).ToList();
            CongesValidated = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted).OrderBy(c => (String)(c.BeginDate.ToString("yyyyMMdd"))).ToList();
            CongesRefused = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Refused).OrderBy(c => (String)(c.BeginDate.ToString("yyyyMMdd"))).ToList();
        }

        public IActionResult OnPostDeleteAsync(int id)
        {
            try
            {
                Db.Instance.DataBase.CongeRepository.Delete(new Conge() { Id = id });
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                _logger.LogError(except.ToString());
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCancelAsync(int id)
        {
            try
            {
                Conge congeToCancel = Db.Instance.DataBase.CongeRepository.Get(id);
                if (congeToCancel != null)
                {
                    congeToCancel.CanDeleted = true;
                    Db.Instance.DataBase.CongeRepository.Update(congeToCancel);

                    User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                    Toolkit.Notify(Toolkit.NotifyTypeEnum.LeaveCancelPending, congeToCancel.UserId, current.Manager.Id, Toolkit.Configuration[Toolkit.ConfigEnum.SmtpManagerCancelSubject.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.SmtpManagerCancelBody.ToString()]);
                }
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                _logger.LogError(except.ToString());
            }

            return RedirectToPage();
        }
    }
}
