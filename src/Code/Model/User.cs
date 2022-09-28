using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class User
    {
        public String Email { get; set; }
        public int ServiceId { get; set; }
        public String Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public String Surname { get; set; }
        public String FamilyName { get; set; }
        public String PhoneNumber { get; set; }
        public String Description { get; set; }
        public String Matricule { get; set; }
        public DateTime LastConnection { get; set; } = DateTime.Now;

        public List<Conge> CongesValidated { get; set; } = new List<Conge>();
        public List<Conge> CongesInProgress { get; set; } = new List<Conge>();
        public List<Conge> CongesRefused { get; set; } = new List<Conge>();
        public Manager Manager { get; set; }
        public String Login { get; set; }
        public bool IsManager { get; set; }
        public bool IsDrh { get; set; }
        public bool IsAdmin { get; set; }
        public Service Service { get; set; }
        public bool Imported { get; set; } = false;

        static public List<String> Admins = null;

        static public List<String> Drh = null;

        public override string ToString()
        {
            return Name;
        }
    }
}