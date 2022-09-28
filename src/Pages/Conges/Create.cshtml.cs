using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Conges
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<PageModel> _logger;

        public CreateModel(ILogger<PageModel> logger)
        {
            _logger = logger;
        }

        [Required(AllowEmptyStrings = true)]
        [BindProperty]
        public String DateBegin { get; set; }

        [Required(AllowEmptyStrings = true)]
        [BindProperty]
        public String DateEnd { get; set; }

        [Required(AllowEmptyStrings = false)]
        [BindProperty]
        public String Motif { get; set; }

        #region Type de congé (payé, récupération, sans solde, etc.)
        public List<KeyValuePair<String, int>> CgTypes { get; set; } = new List<KeyValuePair<String, int>>();

        [BindProperty]
        public Conge.CGTypeEnum CgType { get; set; }
        #endregion

        #region Début de la journée (matin ou après midi)
        public List<KeyValuePair<String, int>> IntervalTypes { get; set; } = new List<KeyValuePair<String, int>>();
        [BindProperty]
        public int IntervalTypeBegin { get; set; } = 0;
        [BindProperty]
        public int IntervalTypeEnd { get; set; } = 1;
        #endregion

        [Required(AllowEmptyStrings = true)]
        [BindProperty]
        public String HourBegin { get; set; }

        [Required(AllowEmptyStrings = true)]
        [BindProperty]
        public String HourEnd { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public void OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            foreach (var cgType in Enum.GetValues(typeof(Conge.CGTypeEnum)))
                CgTypes.Add(new KeyValuePair<String, int>(((Conge.CGTypeEnum)cgType).GetDescription(), (int)cgType));

            IntervalTypes.Add(new KeyValuePair<String, int>("Matin", 0));
            IntervalTypes.Add(new KeyValuePair<String, int>("Après-midi", 1));
        }

        public IActionResult OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();

                String parseDateFormat = "dd/MM/yyyy";
                if (CgType == Model.Conge.CGTypeEnum.AbsenceTemporaire)
                {
                    parseDateFormat = "dd/MM/yyyyHH:mm";
                    DateEnd = DateBegin;

                    DateBegin += HourBegin;
                    DateEnd += HourEnd;
                }

                DateTime dateBegin = DateTime.Now;
                try
                {
                    dateBegin = DateTime.ParseExact(DateBegin, parseDateFormat, CultureInfo.InvariantCulture);
                }
                catch (Exception except)
                {
                    _logger.LogError(except.ToString());

                    ErrorMessage = "Vous devez renseigner une date de début sous la forme jj/mm/aaaa";
                    return RedirectToPage();
                }

                DateTime dateEnd = DateTime.Now;
                try
                {
                    dateEnd = DateTime.ParseExact(DateEnd, parseDateFormat, CultureInfo.InvariantCulture);
                }
                catch (Exception except)
                {
                    _logger.LogError(except.ToString());

                    ErrorMessage = "Vous devez renseigner une date de fin sous la forme jj/mm/aaaa";
                    return RedirectToPage();
                }

                if (dateBegin > dateEnd)
                {
                    ErrorMessage = "La date de fin ne peut pas être avant celle du début.";
                    return RedirectToPage();
                }

                if (Toolkit.IsWorkingDay(dateBegin) == false)
                {
                    ErrorMessage = "Vous ne pouvez pas débuter vos congés par un jour férié.";
                    return RedirectToPage();
                }

                Conge Conge = new Conge();
                if (CgType != Model.Conge.CGTypeEnum.AbsenceTemporaire)
                {
                    TimeSpan timeSpanBegin = new TimeSpan(0, 0, 0);
                    if (IntervalTypeBegin == 1)
                        timeSpanBegin = new TimeSpan(12, 0, 0);
                    Conge.BeginDate = new DateTime(dateBegin.Year, dateBegin.Month, dateBegin.Day, timeSpanBegin.Hours, timeSpanBegin.Minutes, timeSpanBegin.Seconds);

                    TimeSpan timeSpanEnd = new TimeSpan(23, 59, 59);
                    if (IntervalTypeEnd == 0)
                        timeSpanEnd = new TimeSpan(12, 0, 0);
                    Conge.EndDate = new DateTime(dateEnd.Year, dateEnd.Month, dateEnd.Day, timeSpanEnd.Hours, timeSpanEnd.Minutes, timeSpanEnd.Seconds);
                }
                else
                {
                    Conge.BeginDate = dateBegin;
                    Conge.EndDate = dateEnd;
                }

                User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);

                // Vérification des chevauchements
                List<Conge> Conges = Db.Instance.DataBase.CongeRepository.Get(current.Email, Conge.StateEnum.Accepted);
                Conges.AddRange(Db.Instance.DataBase.CongeRepository.Get(current.Email, Conge.StateEnum.InProgress));

                foreach (Conge conge in Conges)
                {
                    if (Conge.BeginDate < conge.EndDate && conge.BeginDate < Conge.EndDate)
                    {
                        ErrorMessage = "Congés déjà existants sur cette période.";
                        return RedirectToPage();
                    }
                }

                Conge.CreateDate = Conge.BeginDate;
                Conge.ModifyDate = DateTime.Now;
                Conge.UserId = current.Email;
                Conge.CGType = CgType;

                // Pour ne pas exporter ou rendre visible en compa les demandes d'absence temporaires (afin de les différencier des congés)
                if (Conge.CGType == Model.Conge.CGTypeEnum.AbsenceTemporaire)
                    Conge.IsExported = true;

                Db.Instance.DataBase.CongeRepository.Insert(Conge);

                // Envoie d'un mail au responsable pour l'avertir
                String body = Toolkit.Configuration[Toolkit.ConfigEnum.SmtpReplyBody.ToString()];
                if (!String.IsNullOrEmpty(Motif))
                    body += " - Motif: " + Sanitizer.GetSafeHtmlFragment(Motif);

                Toolkit.SendEmail(
                    current.Email,
                    current.Manager.Id,
                    Toolkit.Configuration[Toolkit.ConfigEnum.SmtpReplySubject.ToString()],
                    body);
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
                return RedirectToPage();
            }
            return RedirectToPage("/Index");
        }
    }
}