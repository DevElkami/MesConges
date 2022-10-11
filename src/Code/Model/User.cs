using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class User
    {
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public String Email { get; set; }
        public int ServiceId { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(70)]
        public String Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(70)]
        public String Surname { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(70)]
        public String FamilyName { get; set; }

        [Phone]
        [MaxLength(20)]
        public String PhoneNumber { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(200)]
        public String Description { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(30)]
        public String Matricule { get; set; }
        public DateTime LastConnection { get; set; } = DateTime.Now;

        public List<Conge> CongesValidated { get; set; } = new List<Conge>();
        public List<Conge> CongesInProgress { get; set; } = new List<Conge>();
        public List<Conge> CongesRefused { get; set; } = new List<Conge>();
        public Manager Manager { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9_.-]*")]
        [MaxLength(50)]
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