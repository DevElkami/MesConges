using System;
using System.ComponentModel;

namespace WebApplicationConges.Model
{
    public class Conge : ICloneable
    {
        public long Id { get; set; }
        public String UserId { get; set; }
        public String Motif { get; set; }

        public enum StateEnum
        {
            InProgress = 0,
            Accepted,
            Refused
        }
        public StateEnum State { get; set; }

        public enum CGTypeEnum
        {
            [Description("Congé payé")]
            Conge = 0,

            [Description("Congé sans solde *")]
            SansSolde,

            [Description("Récupération")]
            Recup,

            [Description("Absence temporaire")]
            AbsenceTemporaire,

            [Description("Evénements familiaux *")]
            FamilyEvent,

            [Description("Enfants malades *")]
            ChildSick
        }
        public CGTypeEnum CGType { get; set; } = CGTypeEnum.Conge;
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public User User { get; set; }
        public bool IsExported { get; set; } = false;
        public bool CanDeleted { get; set; } = false;

        public override String ToString()
        {
            String info = String.Empty;

            if (CGType == CGTypeEnum.AbsenceTemporaire)
            {
                info = "Abscence le " + BeginDate.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                info = "Congé de " + UserId + " du " + BeginDate.ToString("dd/MM/yyyy HH:mm");
            }
            return info;
        }

        public object Clone()
        {
            Conge conge = new Conge();
            conge.BeginDate = this.BeginDate;
            conge.EndDate = this.EndDate;
            conge.CGType = this.CGType;
            conge.CreateDate = this.CreateDate;
            conge.Duration = this.Duration;
            conge.Id = -1;
            conge.ModifyDate = this.ModifyDate;
            conge.State = this.State;
            conge.IsExported = this.IsExported;
            conge.CanDeleted = this.CanDeleted;

            if (!String.IsNullOrEmpty(this.Motif))
                conge.Motif = new String(this.Motif);

            if (!String.IsNullOrEmpty(this.UserId))
                conge.UserId = new String(this.UserId);

            return conge;
        }
    }
}
