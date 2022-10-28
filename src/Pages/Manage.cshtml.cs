using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages
{
    public class ManageModel : PageModel
    {
        private readonly ILogger<ManageModel> _logger;

        public ManageModel(ILogger<ManageModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public List<Conge> CongesInProgress { get; set; }

        [BindProperty]
        public List<Conge> CongesToDelete { get; set; }

        public List<User> Members { get; set; } = new List<User>();

        public String Calendar { get; set; } = String.Empty;

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            User manager = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
            if (manager.IsManager)
            {
                CongesInProgress = new List<Conge>();
                CongesToDelete = new List<Conge>();

                // Quel service est gérer par ce manager ?
                long serviceId = Db.Instance.DataBase.ManagerRepository.Get(manager.Email).ServiceId;

                Members = Db.Instance.DataBase.UserRepository.GetAll(serviceId).OrderBy(u => u.Name).ToList();
                List<User> users = Db.Instance.DataBase.UserRepository.GetAll(serviceId).OrderBy(u => u.Name).ToList();
                foreach (User user in users)
                {
                    List<Conge> congeInProgress = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.InProgress).OrderBy(c => c.BeginDate.ToString("yyyyMMdd")).ToList();
                    if (congeInProgress.Count > 0)
                    {
                        foreach (Conge conge in congeInProgress)
                            conge.User = user;

                        CongesInProgress.AddRange(congeInProgress);
                    }

                    Calendar += Toolkit.CalendarFormater(user.Name, Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted));

                    List<Conge> congeToDelete = Db.Instance.DataBase.CongeRepository.Get(user.Email, true, Conge.StateEnum.Accepted).OrderBy(c => c.BeginDate.ToString("yyyyMMdd")).ToList();
                    if (congeToDelete.Count > 0)
                    {
                        foreach (Conge conge in congeToDelete)
                        {
                            conge.User = user;
                            if (!conge.IsExported)
                                CongesToDelete.Add(conge);
                        }
                    }
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
                CongeAccept(current.Email, id);
            }
            catch (Exception except)
            {
                _logger.LogError(except.ToString());
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public static void CongeAccept(String mailFrom, long id)
        {
            Conge conge = Db.Instance.DataBase.CongeRepository.Get(id);
            if (conge != null)
            {
                conge.ModifyDate = DateTime.Now;
                conge.State = Conge.StateEnum.Accepted;

                // Split des congés à partir d'ici pour intégration en compta
                if (conge.BeginDate.Month != conge.EndDate.Month)
                {
                    DateTime current = conge.BeginDate;
                    while (current < conge.EndDate)
                    {
                        DateTime endOfCurrentMounth = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month), 23, 59, 59);

                        Conge part = conge.Clone() as Conge;
                        part.BeginDate = current;
                        part.EndDate = (endOfCurrentMounth < conge.EndDate) ? endOfCurrentMounth : conge.EndDate;
                        Db.Instance.DataBase.CongeRepository.Insert(part);

                        current = part.EndDate + new TimeSpan(0, 0, 0, 1);
                    }

                    Db.Instance.DataBase.CongeRepository.Delete(conge);
                }
                else
                    Db.Instance.DataBase.CongeRepository.Update(conge);

                // Envoie d'un mail au membre de l'équipe pour l'avertir que son congé est accepté                
                Toolkit.SendEmail(mailFrom, conge.UserId, Toolkit.Configuration[Toolkit.ConfigEnum.SmtpAcceptSubject.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.SmtpAcceptBody.ToString()]);
                Cache.Clear(CalendarModel.MAIN_CALENDAR_KEY);
            }
            else
            {
                throw new Exception("Le congé a été supprimé par le collaborateur.");
            }
        }

        public IActionResult OnPostAcceptAllAsync()
        {
            try
            {
                User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                foreach (Conge conge in CongesInProgress)
                    CongeAccept(current.Email, conge.Id);
            }
            catch (Exception except)
            {
                _logger.LogError(except.ToString());
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRefuseAsync(int id)
        {
            try
            {
                Conge congeRefuse = null;
                foreach (Conge conge in CongesInProgress)
                {
                    if (conge.Id == id)
                    {
                        congeRefuse = conge;
                        break;
                    }
                }

                if (String.IsNullOrEmpty(congeRefuse.Motif) || (congeRefuse.Motif.Length < 5))
                {
                    ErrorMessage = "Vous devez indiquer un motif (avec un minimum de mots).";
                    return RedirectToPage();
                }

                String motif = congeRefuse.Motif;

                // On vérifie qu'il est toujours là
                congeRefuse = Db.Instance.DataBase.CongeRepository.Get(id);
                if (congeRefuse != null)
                {
                    congeRefuse.ModifyDate = DateTime.Now;
                    congeRefuse.State = Conge.StateEnum.Refused;
                    congeRefuse.Motif = motif;
                    Db.Instance.DataBase.CongeRepository.Update(congeRefuse);

                    // Envoie d'un mail au membre de l'équipe pour l'avertir que son congé est refusé
                    User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                    Toolkit.SendEmail(current.Email, congeRefuse.UserId, Toolkit.Configuration[Toolkit.ConfigEnum.SmtpRefuseSubject.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.SmtpRefuseBody.ToString()] + motif);
                }
                else
                {
                    ErrorMessage = "Le congé a été supprimé par le collaborateur.";
                }
            }
            catch (Exception except)
            {
                _logger.LogError(except.ToString());
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteAsync(int id)
        {
            try
            {
                // On vérifie qu'il est toujours là
                Conge congeDeleted = Db.Instance.DataBase.CongeRepository.Get(id);
                if (congeDeleted != null)
                {
                    Db.Instance.DataBase.CongeRepository.Delete(congeDeleted);

                    // Envoie d'un mail au membre de l'équipe pour l'avertir que son congé est annulé
                    User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                    Toolkit.SendEmail(current.Email, congeDeleted.UserId, Toolkit.Configuration[Toolkit.ConfigEnum.SmtpDeletedSubject.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.SmtpDeletedBody.ToString()]);

                    Cache.Clear(CalendarModel.MAIN_CALENDAR_KEY);

                    // Dans le cas d'un congé déjà exporté en compta, il faut avertir la DRH pour qu'elle fasse une régulation
                    if (congeDeleted.IsExported)
                    {
                        foreach (User drh in Db.Instance.GetDrh())
                        {
                            Toolkit.SendEmail(
                                Toolkit.Configuration[Toolkit.ConfigEnum.AppAdminEmail.ToString()], // L'email de l'admin qui écrit à la DRH
                                drh.Email,
                                "App des congés - Problème d'annulation",
                                "Le congé de " + congeDeleted.UserId + " vient d'être annulé, mais il était déjà exporté en compta: il va falloir faire une régulation.");
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Le congé a été supprimé par le collaborateur.";
                }
            }
            catch (Exception except)
            {
                _logger.LogError(except.ToString());
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCancelDeleteAsync(int id)
        {
            try
            {
                // On vérifie qu'il est toujours là
                Conge congeDeleted = Db.Instance.DataBase.CongeRepository.Get(id);
                if (congeDeleted != null)
                {
                    congeDeleted.CanDeleted = false;
                    Db.Instance.DataBase.CongeRepository.Update(congeDeleted);

                    // Envoie d'un mail au membre de l'équipe pour l'avertir que sa demande d'annulation de congé est refusé
                    User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                    Toolkit.SendEmail(current.Email, congeDeleted.UserId, Toolkit.Configuration[Toolkit.ConfigEnum.SmtpCancelSubject.ToString()], Toolkit.Configuration[Toolkit.ConfigEnum.SmtpCancelRefusedBody.ToString()]);
                }
                else
                {
                    ErrorMessage = "Le congé a été supprimé par le collaborateur.";
                }
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