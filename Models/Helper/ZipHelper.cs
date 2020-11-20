using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NewSprt.Data.Zarnica.Models;
using NewSprt.Models.Helper.Documents;

namespace NewSprt.Models.Helper
{
    public class ZipHelper
    {
        protected const string TempPath = @"wwwroot/Temp/";
        
        public const string OutputFormatType = "application/zip";
        public static FileStream GeneratePersonalGuidanceReport(IEnumerable<MilitaryComissariat> militaryComissariats,
            List<SpecialPerson> persons)
        {
            const string tempDirectory = TempPath + "PersonalGuidance";
            const string zipFile = TempPath + "personalGuidanceReport.zip";
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);
            File.Delete(zipFile);
            foreach (var militaryComissariat in militaryComissariats)
            {
                var mcRecruits = persons.Where(m => m.MilitaryComissariatCode == militaryComissariat.Id).ToList();
                if (mcRecruits.Count == 0) continue;
        
                var file = WordDocumentHelper.GeneratePersonalGuidanceReport(mcRecruits, militaryComissariat);
                file.Close();
            }
            
            ZipFile.CreateFromDirectory(tempDirectory, zipFile);
            return new FileStream(zipFile, FileMode.Open);
        }
    }
}