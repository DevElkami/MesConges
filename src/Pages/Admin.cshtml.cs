using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages
{
    public class AdminModel : PageModel
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AdminModel(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [BindProperty]
        public List<Service> Services { get; set; }

        [BindProperty]
        public List<User> Users { get; set; }

        [BindProperty]
        public List<Manager> Managers { get; set; }

        public List<Conge> OldAbsenceTemporaires { get; set; } = new List<Conge>();

        public List<Conge> OldConges { get; set; } = new List<Conge>();

        public int FilesCount { get; set; } = 0;

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            User currentUser = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
            if (currentUser.IsAdmin)
            {
                Services = Db.Instance.DataBase.ServiceRepository.GetAll().OrderBy(s => s.Name).ToList();
                Users = Db.Instance.DataBase.UserRepository.GetAll().OrderBy(u => u.Name).ToList();
                Managers = Db.Instance.DataBase.ManagerRepository.GetAll();

                foreach (User user in Users)
                {
                    user.Service = Db.Instance.DataBase.ServiceRepository.Get(user.ServiceId);

                    foreach (Conge.CGTypeEnum cgType in Enum.GetValues(typeof(Conge.CGTypeEnum)))
                    {
                        if (cgType == Conge.CGTypeEnum.AbsenceTemporaire)
                            continue;

                        OldConges.AddRange(GetOldConge(user.Email, Conge.StateEnum.Accepted, cgType, Toolkit.IntervalCongeOld()));
                        OldConges.AddRange(GetOldConge(user.Email, Conge.StateEnum.Refused, cgType, Toolkit.IntervalCongeOld()));
                    }

                    OldAbsenceTemporaires.AddRange(GetOldConge(user.Email, Conge.StateEnum.Accepted, Conge.CGTypeEnum.AbsenceTemporaire, Toolkit.IntervalAbsenceTemporaire()));
                    OldAbsenceTemporaires.AddRange(GetOldConge(user.Email, Conge.StateEnum.Refused, Conge.CGTypeEnum.AbsenceTemporaire, Toolkit.IntervalAbsenceTemporaire()));
                }

                foreach (Manager manager in Managers)
                {
                    foreach (Service service in Services)
                    {
                        if (manager.ServiceId == service.Id)
                        {
                            service.Manager = manager;
                            manager.Service = service;
                            manager.User = Db.Instance.DataBase.UserRepository.Get(manager.Id);
                            break;
                        }
                    }
                }

                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Toolkit.Configuration[Toolkit.ConfigEnum.ExportDir.ToString()]);
                if (Directory.Exists(exportPath))
                    FilesCount = Directory.GetFiles(exportPath).Length;

                return Page();
            }
            else
                return RedirectToPage("/Index");
        }

        private static List<Conge> GetOldConge(String userId, Conge.StateEnum state, Conge.CGTypeEnum cgType, TimeSpan interval)
        {
            return Db.Instance.DataBase.CongeRepository.Get(userId, state, cgType, DateTime.MinValue, DateTime.Now - interval).OrderBy(c => (String)(c.BeginDate.ToString("yyyyMMdd"))).ToList();
        }

        public IActionResult OnPostDeleteServiceAsync(int id)
        {
            try
            {
                Db.Instance.DataBase.ServiceRepository.Delete(new Service() { Id = id });

                // On supprime aussi le manager du service si il existait
                Manager manager = Db.Instance.DataBase.ManagerRepository.GetByServiceId(id);
                if (manager != null)
                    Db.Instance.DataBase.ManagerRepository.Delete(manager);
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteUserAsync(String id)
        {
            try
            {
                User current = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (current.Email == id)
                {
                    ErrorMessage = "Vous ne pouvez pas vous supprimer.";
                    return RedirectToPage();
                }
                else
                    Db.Instance.DataBase.UserRepository.Delete(new User() { Email = id });
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCleanExportAsync()
        {
            try
            {
                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Toolkit.Configuration[Toolkit.ConfigEnum.ExportDir.ToString()]);
                if (Directory.Exists(exportPath))
                {
                    foreach (String filePath in Directory.GetFiles(exportPath).OrderBy(f => f))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCleanAbsTempAsync()
        {
            try
            {
                foreach (User user in Db.Instance.DataBase.UserRepository.GetAll())
                {
                    OldAbsenceTemporaires.AddRange(GetOldConge(user.Email, Conge.StateEnum.Accepted, Conge.CGTypeEnum.AbsenceTemporaire, Toolkit.IntervalAbsenceTemporaire()));
                    OldAbsenceTemporaires.AddRange(GetOldConge(user.Email, Conge.StateEnum.Refused, Conge.CGTypeEnum.AbsenceTemporaire, Toolkit.IntervalAbsenceTemporaire()));
                }

                foreach (Conge conge in OldAbsenceTemporaires)
                    Db.Instance.DataBase.CongeRepository.Delete(conge);
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCleanCongeAsync()
        {
            try
            {
                foreach (User user in Db.Instance.DataBase.UserRepository.GetAll())
                {
                    foreach (Conge.CGTypeEnum cgType in Enum.GetValues(typeof(Conge.CGTypeEnum)))
                    {
                        if (cgType == Conge.CGTypeEnum.AbsenceTemporaire)
                            continue;

                        OldConges.AddRange(GetOldConge(user.Email, Conge.StateEnum.Accepted, cgType, Toolkit.IntervalCongeOld()));
                        OldConges.AddRange(GetOldConge(user.Email, Conge.StateEnum.Refused, cgType, Toolkit.IntervalCongeOld()));
                    }
                }

                foreach (Conge conge in OldConges)
                    Db.Instance.DataBase.CongeRepository.Delete(conge);
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostTestEmailAsync()
        {
            try
            {
                User currentUser = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                Toolkit.SendEmail(currentUser.Email, currentUser.Email, "Ceci est un test", "Pour vérifier si les emails sont bien envoyés.");
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostResetCacheAsync()
        {
            try
            {
                Cache.ClearAll();
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }
    }
}
