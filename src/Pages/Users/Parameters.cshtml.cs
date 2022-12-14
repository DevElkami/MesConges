﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.Users
{
    public class ParametersModel : PageModel
    {
        [BindProperty]
        public new User User { get; set; }

        [BindProperty]
        public String Password { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                User user = JsonSerializer.Deserialize<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
                if (user != null)
                    User = Db.Instance.DataBase.UserRepository.Get(user.Email);
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
                User modifiedUser = Db.Instance.DataBase.UserRepository.Get(User.Email);
                if ((Connect.UserAccess.Instance is Connect.DbConnection) && !String.IsNullOrEmpty(Password))
                    modifiedUser.HashPwd = Toolkit.CreateSHAHash(Password);

                modifiedUser.Login = User.Login;
                modifiedUser.Name = User.Name;
                modifiedUser.Surname = User.Surname;
                modifiedUser.FamilyName = User.FamilyName;
                modifiedUser.Description = User.Description;
                modifiedUser.PhoneNumber = User.PhoneNumber;

                Db.Instance.DataBase.UserRepository.Update(modifiedUser);
                Toolkit.Log(HttpContext, $"Paramètres: Mise à jour de l'utilisateur {User.Email}.");
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