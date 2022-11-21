using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Globalization;
using WebApplicationConges;
using WebApplicationConges.Data;
using WebApplicationConges.Model;

namespace TestProject
{
    [TestClass]
    public class UnitTestMisc
    {
        [TestMethod]
        public void ExportExcel()
        {
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

                excel.Save();
            }
        }        
    }
}