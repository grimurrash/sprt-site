using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Report;
using NewSprt.Data.App.Models;

namespace NewSprt.Models.Helper.Documents
{
    public class ExcelDocumentHelper
    {
        public const string OutputFormatType =
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        protected const string TemplatePath = @"wwwroot/Templates/";
        protected const string TempPath = @"wwwroot/Temp/";

        private static void CopyTemplateFileToTempDirectory(string templateFile, string tempFile)
        {
            if (!File.Exists(templateFile)) throw new FileNotFoundException("Не удалось найти шаблон");
            File.Copy(templateFile, tempFile, true);
        }

        public static FileStream GenerateMilitaryComissariatReport(List<Recruit> recruits,
            MilitaryComissariat militaryComissariat)
        {
            var index = 1;
            var recruitsList = recruits.Select(m => new
            {
                Index = index++,
                m.LastName,
                m.FirstName,
                m.Patronymic,
                m.BirthDate
            });
            
            const string templateFile = TemplatePath + "Dactyloscopy/comissariat_report.xlsx";
            const string tempFile = TempPath + "comissariat_report.xlsx";
            CopyTemplateFileToTempDirectory(templateFile, tempFile);
            var document = new XLTemplate(tempFile);
            document.AddVariable("Number", militaryComissariat.InnerCode.TrimStart('0'));
            var firstRecruitDeliveryDate = recruits.First().DeliveryDate;
            document.AddVariable("Season",
                (firstRecruitDeliveryDate.Month > 9 ? "осенью" : "весной") + " " + firstRecruitDeliveryDate.Year);
            document.AddVariable("Comissariat", militaryComissariat.GetDocumentName());
            document.AddVariable("Recruits", recruitsList);
            document.Generate();
            document.SaveAs(tempFile);
            
            return new FileStream(tempFile, FileMode.Open);
        }
        
        public static FileStream GenerateConscriptionPeriodReport(IEnumerable<Recruit> recruits, string dateAndOutgoingNumber)
        {
            var index = 1;
            var recruitsList = recruits.Select(m => new
            {
                Index = index++,
                m.FullName,
                DactyloscopyDate = m.DeliveryDate,
                OutText = dateAndOutgoingNumber,
                Notice = m.MilitaryComissariat.ShortName
            });
            
            const string templateFile = TemplatePath + "Dactyloscopy/conscription_report.xlsx";
            const string tempFile = TempPath + "conscription_report.xlsx";
            CopyTemplateFileToTempDirectory(templateFile, tempFile);
            var document = new XLTemplate(tempFile);
            document.AddVariable("Recruits", recruitsList);
            document.Generate();
            document.SaveAs(tempFile);
            
            return new FileStream(tempFile, FileMode.Open);
        }
    }
}