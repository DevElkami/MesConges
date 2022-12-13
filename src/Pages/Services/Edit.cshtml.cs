using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Services
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public Service Service { get; set; }

        public List<User> Users { get; set; } = new List<User>();

        public List<User> Members { get; set; } = new List<User>();

        [BindProperty]
        public String ManagerId { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet(int id = 0)
        {
            try
            {
                User admin = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (admin.IsAdmin)
                {
                    Service = Db.Instance.DataBase.ServiceRepository.Get(id);
                    Users = Db.Instance.DataBase.UserRepository.GetAll().OrderBy(u => u.Name).ToList();
                    Members = Db.Instance.DataBase.UserRepository.GetAll(id).OrderBy(u => u.Name).ToList();
                    List<Manager> managers = Db.Instance.DataBase.ManagerRepository.GetAll();

                    foreach (Manager manager in managers)
                    {
                        if (manager.ServiceId == Service.Id)
                        {
                            Service.Manager = manager;
                            ManagerId = manager.Id;
                            break;
                        }
                    }

                    if (String.IsNullOrEmpty(ManagerId))
                        ManagerId = admin.Email;
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
                // Mise à jour manager
                Manager manager = Db.Instance.DataBase.ManagerRepository.Get(ManagerId);
                if (manager == null)
                {
                    // Nouveau manager, donc on supprime l'ancien avant
                    Manager oldManager = Db.Instance.DataBase.ManagerRepository.GetByServiceId(Service.Id);
                    if (oldManager != null)
                        Db.Instance.DataBase.ManagerRepository.Delete(oldManager);

                    manager = new Manager();
                    manager.Id = ManagerId;
                    manager.ServiceId = Service.Id;

                    Toolkit.Log(HttpContext, $"Mise à jour du service {Service.Name}. Nouveau manager: {ManagerId}");
                    Db.Instance.DataBase.ManagerRepository.Insert(manager);
                }
                else
                {
                    manager.Id = ManagerId;
                    manager.ServiceId = Service.Id;
                    Db.Instance.DataBase.ManagerRepository.Update(manager);
                }

                // Mise à jour descriptif ou nom
                Service oldService = Db.Instance.DataBase.ServiceRepository.Get((int)Service.Id);
                if ((oldService.Name != Service.Name) || (oldService.Description != Service.Description))
                {
                    Toolkit.Log(HttpContext, $"Mise à jour du service {Service.Name}: Nom ou descriptif changé.");
                    Db.Instance.DataBase.ServiceRepository.Update(Service);
                }
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