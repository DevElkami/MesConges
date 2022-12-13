using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace WebApplicationConges.Pages.RH
{
    public class ComptaModel : PageModel
    {
        private readonly ILogger<ComptaModel> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const String ExcelCongeFormat = "dd/MM/yyyy HH:mm";

        public ComptaModel(ILogger<ComptaModel> logger, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public List<Conge> CongesInProgress { get; set; }

        public List<Conge> AbsenceTemporaires { get; set; }

        [Required(AllowEmptyStrings = true)]
        [BindProperty]
        public String DateEnd { get; set; }

        public List<KeyValuePair<String, DateTime>> ExportColl { get; set; }

        [TempData]
        public String ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!String.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(String.Empty, ErrorMessage);

            User drh = JsonConvert.DeserializeObject<User>(HttpContext.User.Claims.FirstOrDefault(c => c.Type == "CurrentUser")?.Value);
            if (drh.IsDrh)
            {
                CongesInProgress = new List<Conge>();
                AbsenceTemporaires = new List<Conge>();

                List<User> users = Db.Instance.DataBase.UserRepository.GetAll();
                foreach (User user in users)
                {
                    List<Conge> congeInProgress = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted, false).OrderBy(c => c.BeginDate.ToString("yyyyMMdd")).ToList();
                    if (congeInProgress.Count > 0)
                    {
                        foreach (Conge conge in congeInProgress)
                            conge.User = user;

                        CongesInProgress.AddRange(congeInProgress);
                    }

                    List<Conge> absenceTemporaire = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted, Conge.CGTypeEnum.AbsenceTemporaire, DateTime.Now - Toolkit.IntervalAbsenceTemporaire(), DateTime.Now + Toolkit.IntervalAbsenceTemporaire()).OrderBy(c => c.BeginDate.ToString("yyyyMMdd")).ToList();
                    if (absenceTemporaire.Count > 0)
                    {
                        foreach (Conge conge in absenceTemporaire)
                        {
                            conge.User = user;
                            AbsenceTemporaires.Add(conge);
                        }
                    }
                }

                ExportColl = new List<KeyValuePair<String, DateTime>>();
                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirExport);
                if (Directory.Exists(exportPath))
                {
                    foreach (String filePath in Directory.GetFiles(exportPath).OrderBy(f => f))
                    {
                        String fileName = Path.GetFileName(filePath);
                        String exportDir = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value + HttpContext.Request.PathBase;
                        exportDir += "/" + Db.Instance.DataBase.ConfigRepository.Get().DirExport + "/" + fileName;
                        ExportColl.Add(new KeyValuePair<String, DateTime>(exportDir, System.IO.File.GetCreationTime(filePath)));
                    }
                }

                ExportColl = ExportColl.OrderByDescending(o => o.Value).ToList();

                return Page();
            }
            else
                return RedirectToPage("/Index");
        }

        public IActionResult OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                    return Page();


                DateTime dateEnd = DateTime.Now;
                try
                {
                    dateEnd = DateTime.ParseExact(DateEnd, "MM/yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception except)
                {
                    _logger.LogError(except.ToString());

                    ErrorMessage = "Vous devez renseigner une date de fin sous la forme jj/mm/aaaa";
                    return RedirectToPage();
                }
                dateEnd = new DateTime(dateEnd.Year, dateEnd.Month, DateTime.DaysInMonth(dateEnd.Year, dateEnd.Month), 23, 59, 59);

                // Les enregistrer sous format excel 
                List<Conge> congesToFlag = new List<Conge>();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                String exportPath = Path.Combine(_hostingEnvironment.WebRootPath, Db.Instance.DataBase.ConfigRepository.Get().DirExport);
                Directory.CreateDirectory(exportPath);

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excel = new ExcelPackage())
                {
                    const String WorkSheetName = "Conges";

                    excel.Workbook.Worksheets.Add(WorkSheetName);
                    ExcelWorksheet excelWorksheet = excel.Workbook.Worksheets[WorkSheetName];

                    List<String[]> headerRow = new List<String[]>()
                    {
                      new String[] { "Nom", "Prénom", "Type de congé", "Date de début", "Date de fin", "Total" }
                    };

                    String headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";
                    excelWorksheet.Cells[headerRange].LoadFromArrays(headerRow);

                    for (int i = 1; i < headerRow[0].Count() + 1; i++)
                        excelWorksheet.Column(i).Width = 20;

                    List<Object[]> cellData = new List<Object[]>();

                    List<User> users = Db.Instance.DataBase.UserRepository.GetAll();
                    List<KeyValuePair<int, Conge.CGTypeEnum>> colorlist = new List<KeyValuePair<int, Conge.CGTypeEnum>>();
                    foreach (User user in users)
                    {
                        List<Conge> congeInProgress = Db.Instance.DataBase.CongeRepository.Get(user.Email, Conge.StateEnum.Accepted, false).OrderBy(c => c.BeginDate.ToString("yyyyMMdd")).ToList();
                        if (congeInProgress.Count > 0)
                        {
                            foreach (Conge conge in congeInProgress)
                            {
                                if ((conge.EndDate <= dateEnd) && (conge.CGType != Conge.CGTypeEnum.AbsenceTemporaire))
                                {
                                    cellData.Add(new object[] { user.FamilyName, user.Surname, conge.CGType.ToString(), conge.BeginDate.ToString(ExcelCongeFormat), conge.EndDate.ToString(ExcelCongeFormat), Toolkit.DaysLeft(conge.BeginDate, conge.EndDate).ToString("0.#") });
                                    colorlist.Add(new KeyValuePair<int, Conge.CGTypeEnum>(cellData.Count + 1, conge.CGType));
                                    congesToFlag.Add(conge);
                                }
                            }
                        }
                    }

                    if (congesToFlag.Count > 0)
                    {
                        excelWorksheet.Cells[2, 1].LoadFromArrays(cellData);

                        for (int i = 1; i < headerRow[0].Count() + 1; i++)
                        {
                            excelWorksheet.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            excelWorksheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            excelWorksheet.Cells[1, i].Style.Font.Bold = true;
                        }

                        foreach (KeyValuePair<int, Conge.CGTypeEnum> kvp in colorlist)
                        {
                            switch (kvp.Value)
                            {
                                case Conge.CGTypeEnum.Conge:
                                    break;

                                case Conge.CGTypeEnum.Recup:
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                    break;

                                default:
                                case Conge.CGTypeEnum.SansSolde:
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                                    break;

                                case Conge.CGTypeEnum.ChildSick:
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);
                                    break;

                                case Conge.CGTypeEnum.FamilyEvent:
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[kvp.Key, 3].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                                    break;
                            }
                        }

                        FileInfo excelFile = new FileInfo(Path.Combine(exportPath, DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xlsx"));
                        excel.SaveAs(excelFile);
                    }
                }

                // Les marquer comme "exported"
                foreach (Conge conge in congesToFlag)
                {
                    conge.IsExported = true;
                    Db.Instance.DataBase.CongeRepository.Update(conge);
                }

                Toolkit.Log(HttpContext, $"Compta: export des congés à la date du {DateEnd}");
            }
            catch (Exception except)
            {
                ErrorMessage = except.Message;
            }
            return RedirectToPage();
        }

        private Tuple<String, String> GetDateType(DateTime begin, DateTime end)
        {
            Tuple<String, String> dayType = new Tuple<String, String>("jr", "jr");

            // Même jour ?
            if ((begin.Day == end.Day) && (begin.Month == end.Month) && (begin.Year == end.Year))
            {
                if ((begin.Hour == 00) && (end.Hour == 12))
                    dayType = new Tuple<String, String>("am", "am");
                else if ((begin.Hour == 12) && (end.Hour == 23))
                    dayType = new Tuple<String, String>("pm", "pm");
            }
            else
            {
                if ((begin.Hour == 00) && (end.Hour == 12))
                    dayType = new Tuple<String, String>("jr", "am");
                else if ((begin.Hour == 12) && (end.Hour == 23))
                    dayType = new Tuple<String, String>("pm", "jr");
                else if ((begin.Hour == 12) && (end.Hour == 12))
                    dayType = new Tuple<String, String>("pm", "am");
            }
            return dayType;
        }
    }
}
