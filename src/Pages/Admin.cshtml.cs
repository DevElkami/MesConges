using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
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
        [DataType(DataType.Password)]
        public String CurrentPassword { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public String NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Le nouveau mot de passe et sa conformation doivent être les mêmes.")]
        public String ConfirmPassword { get; set; }

        [BindProperty]
        public Config MyConfig { get; set; }

        [BindProperty]
        public List<Service> Services { get; set; }

        [BindProperty]
        public List<User> Users { get; set; }

        [BindProperty]
        public List<Log> Logs { get; set; }

        [BindProperty]
        public List<Manager> Managers { get; set; }

        public List<Conge> OldAbsenceTemporaires { get; set; } = new List<Conge>();

        public List<Conge> OldConges { get; set; } = new List<Conge>();

        public int FilesCount { get; set; } = 0;

        public List<KeyValuePair<String, DateTime>> BackupColl { get; set; } = new List<KeyValuePair<string, DateTime>>();

        [TempData]
        public String ErrorMessage { get; set; }

        private User GetCurrentUser()
        {
            return JsonSerializer.Deserialize<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
        }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            MyConfig = Db.Instance.DataBase.ConfigRepository.Get();
            Logs = Db.Instance.DataBase.LogRepository.GetAll();

            User currentUser = GetCurrentUser();
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

                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirExport);
                if (Directory.Exists(exportPath))
                    FilesCount = Directory.GetFiles(exportPath).Length;

                BackupColl = new List<KeyValuePair<String, DateTime>>();
                String backupPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirBackupBdd);
                if (Directory.Exists(backupPath))
                {
                    foreach (String filePath in Directory.GetFiles(backupPath).OrderBy(f => f))
                    {
                        String fileName = Path.GetFileName(filePath);
                        String backupDir = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value + HttpContext.Request.PathBase;
                        backupDir += "/" + Db.Instance.DataBase.ConfigRepository.Get().DirBackupBdd + "/" + fileName;
                        BackupColl.Add(new KeyValuePair<String, DateTime>(backupDir, System.IO.File.GetCreationTime(filePath)));
                    }
                }

                BackupColl = BackupColl.OrderByDescending(o => o.Value).ToList();

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
                Toolkit.Log(HttpContext, $"Suppression du service {Db.Instance.DataBase.ServiceRepository.Get(id).Name}");

                // On supprime le manager du service si il existait (sur les nouvelles versions: ON DELETE CASCADE)
                Manager manager = Db.Instance.DataBase.ManagerRepository.GetByServiceId(id);
                if (manager != null)
                    Db.Instance.DataBase.ManagerRepository.Delete(manager);

                // Supression du service
                Db.Instance.DataBase.ServiceRepository.Delete(new Service() { Id = id });
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
                Toolkit.Log(HttpContext, $"Suppression de l'utilisateur {id}");

                User current = JsonSerializer.Deserialize<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
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

        public IActionResult OnPostUpdateConfigAsync()
        {
            try
            {
                String description = "Mise à jour de la configuration générale";

                if (!String.IsNullOrEmpty(NewPassword))
                {
                    if (Toolkit.CreateSHAHash(CurrentPassword) != Db.Instance.DataBase.ConfigRepository.Get().AppAdminPwd)
                    {
                        ErrorMessage = "Le mot de passe courant n'est pas le bon";
                        return RedirectToPage();
                    }
                    description += " avec changement du mot de passe administrateur.";
                    MyConfig.AppAdminPwd = Toolkit.CreateSHAHash(NewPassword);
                }

                Toolkit.Log(HttpContext, description);
                Db.Instance.DataBase.ConfigRepository.Update(MyConfig);
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostBackupAsync()
        {
            try
            {
                Toolkit.Log(HttpContext, $"Création d'une sauvegarde de la BDD");

                String backupPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirBackupBdd);
                Directory.CreateDirectory(backupPath);
                Db.Instance.DataBase.Backup(Path.Combine(backupPath, DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-data.tmp"));
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostReplace(List<IFormFile> bddBackups)
        {
            try
            {
                string wwwPath = _hostingEnvironment.WebRootPath;
                string contentPath = _hostingEnvironment.ContentRootPath;

                string path = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                foreach (IFormFile postedFile in bddBackups)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }

                    Db.Instance.DataBase.Load(Path.Combine(path, fileName));
                }

                Toolkit.Log(HttpContext, $"Restauration complète de la BDD");

            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostCleanBackupAsync()
        {
            try
            {
                Toolkit.Log(HttpContext, $"Suppression de toutes les sauvegardes de la BDD");

                String backupPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirBackupBdd);
                if (Directory.Exists(backupPath))
                {
                    foreach (String filePath in Directory.GetFiles(backupPath).OrderBy(f => f))
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

        public IActionResult OnPostCleanExportAsync()
        {
            try
            {
                Toolkit.Log(HttpContext, $"Suppression de toutes les exports comptables");

                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirExport);
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
                Toolkit.Log(HttpContext, $"Suppression de toutes les abscences temporaires");

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
                Toolkit.Log(HttpContext, $"Suppression de toutes les congés plus vieux de {Toolkit.IntervalCongeOld()}");

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
                User currentUser = JsonSerializer.Deserialize<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                Toolkit.Notify(Toolkit.NotifyTypeEnum.Test, currentUser.Email, currentUser.Email, "Ceci est un test", "Pour vérifier si les emails sont bien envoyés.");
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
                Toolkit.Log(HttpContext, $"Remise à zéro du cache");

                Cache.ClearAll();
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteOldLogsAsync()
        {
            try
            {
                Toolkit.Log(HttpContext, $"Suppression des vieux logs.");
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }

            return RedirectToPage();
        }
    }
}
