using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges
{
    public static class Toolkit
    {
        public static String Copyright { get { return "yourCompagny © " + DateTime.Now.Year.ToString(); } }
        public static HubConnection Connection { get; set; }

        public enum ConfigEnum
        {
            DbType,
            DbName,
            DbConnectionString,
            SmtpReplyText,
            SmtpReplySubject,
            SmtpReplyBody,
            SmtpAcceptSubject,
            SmtpAcceptBody,
            SmtpRefuseSubject,
            SmtpRefuseBody,
            SmtpDeletedSubject,
            SmtpDeletedBody,
            SmtpCancelSubject,
            SmtpCancelRefusedBody,
            SmtpCancelAcceptBody,
            SmtpManagerCancelSubject,
            SmtpManagerCancelBody
        }

        public static void InitConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();            
        }        

        public static IConfigurationRoot Configuration { get; set; }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            String description = e.ToString();

            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (descriptionAttributes.Length > 0)
                        {
                            // we're only getting the first description we find
                            // others will be ignored
                            description = ((DescriptionAttribute)descriptionAttributes[0]).Description;
                        }

                        break;
                    }
                }
            }

            return description;
        }

        public static void Log(HttpContext context, String description)
        {
            User user = JsonConvert.DeserializeObject<User>(context.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
            user ??= new User() { Email = "unknown@user" };
            Db.Instance.DataBase.LogRepository.Insert(new Log { UserId = user.Email, ActionDate = DateTime.Now, Description = description });
        }

        public enum NotifyTypeEnum
        {
            Test = 0,               // Test
            LeavePending = 1,       // Congés à valider
            LeaveValidated = 2,     // Congés validés
            LeaveRefused = 3,       // Congés refusés
            LeaveCanceled = 4,      // Congés annulés
            LeaveCancelPending = 5, // Congés à annuler
            DrhActionNeeded = 6     // Action nécessaire de la DRH
        }

        public static void Notify(NotifyTypeEnum notifyType, String mailFrom, String mailTo, String subject, String body)
        {
            if (Db.Instance.DataBase.ConfigRepository.Get().Smtp)
            {
                SmtpClient client = new SmtpClient(Db.Instance.DataBase.ConfigRepository.Get().SmtpServer);
                client.Port = Db.Instance.DataBase.ConfigRepository.Get().SmtpPort;

                MailAddress from = new MailAddress(mailFrom);
                MailAddress to = new MailAddress(mailTo);

                MailMessage message = new MailMessage(from, to);

                message.Body = body;
                message.Body += Environment.NewLine;
                message.BodyEncoding = System.Text.Encoding.UTF8;

                message.Subject = subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;

                client.SendMailAsync(message);
            }

            try
            {
                // SignalR
                Toolkit.Connection.InvokeAsync("Notify", (int)notifyType, mailFrom, mailTo, subject, body);
            }
            catch (Exception)
            {
                // Nothing: SignalR is not mandatory
            }

            // Log
            Db.Instance.DataBase.LogRepository.Insert(new Log { UserId = mailFrom, ActionDate = DateTime.Now, Description = $"Pour {mailTo} : {notifyType}. {subject} - {body}" });
        }

        public static String LayoutColumnDateBeginTitle() { return "Date de début"; }
        public static String LayoutColumnDateEndTitle() { return "Date de fin"; }
        public static String LayoutColumnCongeTypeTitle() { return "Type de congé"; }
        public static String LayoutColumnElapsedTitle() { return "Durée"; }

        public static String LayoutColumnElapsedContent(Conge conge)
        {
            if (conge.CGType == Conge.CGTypeEnum.AbsenceTemporaire)
                return (conge.EndDate - conge.BeginDate).ToString();
            else
                return DaysLeft(conge.BeginDate, conge.EndDate).ToString("0.#");
        }

        public static double DaysLeft(DateTime startDate, DateTime endDate)
        {
            double count = (endDate - startDate).TotalDays;

            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            for (DateTime index = startDate; index <= endDate; index = index.AddDays(1))
            {
                if (!IsWorkingDay(index))
                    count--;
            }

            return count;
        }

        private static CultureInfo FrenchCulture = new CultureInfo("fr-FR", true);

        public static bool IsWorkingDay(DateTime dtDate)
        {
            bool workingDay = true;

            // Préparation des données
            List<DateTime> dataOutofOffice = new List<DateTime>();
            dataOutofOffice.Add(new DateTime(dtDate.Year, 1, 1));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 5, 1));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 5, 8));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 7, 14));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 8, 15));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 11, 1));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 11, 11));
            dataOutofOffice.Add(new DateTime(dtDate.Year, 12, 25));

            // Plus élégants de modifier le fichier conf de l'application
            // que de mettre les dates en dur dans le code
            foreach (String date in Db.Instance.DataBase.ConfigRepository.Get().ExtraDaysOff)
            {
                if (DateTime.TryParseExact(date, "yyyyMMdd", FrenchCulture, DateTimeStyles.None, out _))
                    dataOutofOffice.Add(DateTime.ParseExact(date, "yyyyMMdd", FrenchCulture, DateTimeStyles.None));
            }
            dataOutofOffice.Sort();

            Array arrDateFerie = Array.CreateInstance(typeof(DateTime), dataOutofOffice.Count);
            for (int i = 0; i < dataOutofOffice.Count; i++)
                arrDateFerie.SetValue(dataOutofOffice[i], i);

            // Dimanche ou jour férié
            workingDay = !((dtDate.DayOfWeek == DayOfWeek.Saturday) || (dtDate.DayOfWeek == DayOfWeek.Sunday) || (Array.BinarySearch(arrDateFerie, dtDate) >= 0));
            if (workingDay)
            {
                // Calcul du jour de pâques (algorithme de Oudin (1940))
                //Calcul du nombre d'or - 1
                int intGoldNumber = dtDate.Year % 19;
                // Année divisé par cent
                int intAnneeDiv100 = dtDate.Year / 100;
                // intEpacte est = 23 - Epacte (modulo 30)
                int intEpacte = (intAnneeDiv100 - intAnneeDiv100 / 4 - (8 * intAnneeDiv100 + 13) / 25 + (19 * intGoldNumber) + 15) % 30;
                //Le nombre de jours à partir du 21 mars pour atteindre la pleine lune Pascale
                int intDaysEquinoxeToMoonFull = intEpacte - (intEpacte / 28) * (1 - (intEpacte / 28) * (29 / (intEpacte + 1)) * ((21 - intGoldNumber) / 11));
                //Jour de la semaine pour la pleine lune Pascale (0=dimanche)
                int intWeekDayMoonFull = (dtDate.Year + dtDate.Year / 4 + intDaysEquinoxeToMoonFull + 2 - intAnneeDiv100 + intAnneeDiv100 / 4) % 7;
                // Nombre de jours du 21 mars jusqu'au dimanche de ou avant la pleine lune Pascale (un nombre entre -6 et 28)
                int intDaysEquinoxeBeforeFullMoon = intDaysEquinoxeToMoonFull - intWeekDayMoonFull;
                // mois de pâques
                int intMonthPaques = 3 + (intDaysEquinoxeBeforeFullMoon + 40) / 44;
                // jour de pâques
                int intDayPaques = intDaysEquinoxeBeforeFullMoon + 28 - 31 * (intMonthPaques / 4);
                // lundi de pâques
                DateTime dtMondayPaques = new DateTime(dtDate.Year, intMonthPaques, intDayPaques).AddDays(1);
                // Ascension
                DateTime dtAscension = dtMondayPaques.AddDays(38);
                /*//Pentecote
                DateTime dtMondayPentecote = dtMondayPaques.AddDays(49);*/
                workingDay = !((DateTime.Compare(dtMondayPaques, dtDate) == 0) || (DateTime.Compare(dtAscension, dtDate) == 0)
                /*|| (DateTime.Compare(dtMondayPentecote, dtDate) == 0)*/);
            }
            return workingDay;
        }

        public static String LayoutColumnCongeTypeContent(Conge conge)
        {
            return conge.CGType.GetDescription();
        }

        public static String LayoutColumnDateFormat(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm");
        }

        public static TimeSpan IntervalAbsenceTemporaire() { return new TimeSpan(24 * 7, 0, 0); } // 7 jours

        public static TimeSpan IntervalCongeOld() { return new TimeSpan(24 * 365, 0, 0); } // 1 an

        public static String CalendarFormater(String userName, List<Conge> congeAccepted)
        {
            String calendar = String.Empty;
            foreach (Conge conge in congeAccepted)
            {
                String color = String.Empty;
                String begin = conge.BeginDate.ToString("yyyy-MM-dd");
                if (conge.BeginDate.Hour != 0)
                    begin = conge.BeginDate.ToString("yyyy-MM-ddTHH:mm:ss");

                String end = conge.EndDate.ToString("yyyy-MM-dd");
                if (conge.EndDate.Hour != 23)
                    end = conge.EndDate.ToString("yyyy-MM-ddTHH:mm:ss");
                else
                    end = (conge.EndDate + new TimeSpan(1, 0, 0, 0)).ToString("yyyy-MM-dd");

                if (conge.CGType == Conge.CGTypeEnum.AbsenceTemporaire)
                {
                    begin = conge.BeginDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    end = conge.EndDate.ToString("yyyy-MM-ddTHH:mm:ss");
                    color = ",color: '#257e4a'";
                }

                calendar += "{title:'" + userName + "',start:'" + begin + "',end:'" + end + "'" + color + "},";
            }

            return calendar;
        }

        public static string CreateSHAHash(string phrase)
        {
            SHA512 HashTool = SHA512.Create();
            Byte[] PhraseAsByte = System.Text.Encoding.UTF8.GetBytes(string.Concat(phrase));
            Byte[] EncryptedBytes = HashTool.ComputeHash(PhraseAsByte);
            HashTool.Clear();
            return Convert.ToBase64String(EncryptedBytes);
        }
    }
}
