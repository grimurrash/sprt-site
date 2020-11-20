using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NewSprt.Data.Zarnica.Models;
using NewSprt.Models.Extensions;

namespace NewSprt.Models.Helper.Documents
{
    public class WordDocumentHelper
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

        private static Dictionary<BookmarkStart, BookmarkEnd> FindBookmarks(OpenXmlElement documentPart,
            Dictionary<BookmarkStart, BookmarkEnd> outs = null,
            Dictionary<string, BookmarkStart> bStartWithNoEnds = null)
        {
            if (outs == null)
            {
                outs = new Dictionary<BookmarkStart, BookmarkEnd>();
            }

            if (bStartWithNoEnds == null)
            {
                bStartWithNoEnds = new Dictionary<string, BookmarkStart>();
            }

            // Проходимся по всем элементам на странице Word-документа
            foreach (var docElement in documentPart.Elements())
            {
                switch (docElement)
                {
                    // BookmarkStart определяет начало закладки в рамках документа
                    // маркер начала связан с маркером конца закладки
                    case BookmarkStart bookmarkStart:
                        // Записываем id и имя закладки
                        bStartWithNoEnds.Add(bookmarkStart.Id, bookmarkStart);
                        break;
                    // BookmarkEnd определяет конец закладки в рамках документа
                    case BookmarkEnd bookmarkEnd:
                    {
                        foreach (var startName in bStartWithNoEnds
                            .Where(startName => bookmarkEnd.Id == startName.Key))
                        {
                            outs.Add(startName.Value, bookmarkEnd);
                        }

                        break;
                    }
                }

                // Рекурсивно вызываем данный метод, чтобы пройтись по всем элементам
                // word-документа
                FindBookmarks(docElement, outs, bStartWithNoEnds);
            }

            return outs;
        }

        private static void RemoveBookMarkContent(Dictionary<BookmarkStart, BookmarkEnd> bookMarks)
        {
            foreach (var (bmStart, bmEnd) in bookMarks)
            {
                while (true)
                {
                    var run = bmStart.NextSibling();
                    if (run is Run)
                    {
                        run.Remove();
                    }

                    if (run is BookmarkEnd end && end == bmEnd)
                    {
                        break;
                    }
                }
            }
        }

        public static FileStream GenerateDactyloscopyRecruitCard(Recruit recruit)
        {
            const string templateFile = TemplatePath + "Dactyloscopy/card.docx";
            const string tempFile = TempPath + "card.docx";
            CopyTemplateFileToTempDirectory(templateFile, tempFile);
            var replaceTextDictionary = new Dictionary<string, string>
            {
                {"SEX", "МУЖ"},
                {"LASTNAME", recruit.LastName},
                {"FIRSTNAME", recruit.FirstName},
                {"PATRONYMIC", recruit.Patronymic},
                {"BD", recruit.BirthDate.ToString("dd")},
                {"BM", recruit.BirthDate.GetMonthName()},
                {"BY", recruit.BirthDate.ToString("yyyy")},
                {"BIRTHPLACE", recruit.AdditionalData.BirthPlace},
                {"FULLADDRESS", recruit.FullAddress},
                {"RD", DateTime.Now.ToString("dd")},
                {"RM", DateTime.Now.GetMonthName()},
                {"RY", DateTime.Now.ToString("yyyy")}
            };

            using (var document = WordprocessingDocument.Open(tempFile, true))
            {
                try
                {
                    var bookMarks = FindBookmarks(document.MainDocumentPart.Document);
                    RemoveBookMarkContent(bookMarks);
                    foreach (var (bookmarkStart, bookmarkEnd) in bookMarks)
                    {
                        if (!replaceTextDictionary.Select(m => m.Key).Contains<string>(bookmarkStart.Name))
                            continue;
                        var replaceText = replaceTextDictionary.First(m => m.Key == bookmarkStart.Name).Value;


                        var run = new Run();
                        var runProperties = new RunProperties();
                        var runFonts = new RunFonts() {Ascii = "Arial", HighAnsi = "Arial", ComplexScript = "Arial"};
                        var bold = new Bold();
                        var boldComplexScript = new BoldComplexScript();
                        runProperties.Append(runFonts, bold, boldComplexScript);
                        var text = new Text(replaceText);
                        run.Append(runProperties, text);
                        bookmarkEnd.InsertAfterSelf(run);
                    }
                }
                catch
                {
                    document.Close();
                    throw new Exception("Неизвестная ошибка");
                }
                document.MainDocumentPart.Document.Save();
            }

            return new FileStream(tempFile, FileMode.Open);
        }

        public static FileStream GenerateMedicalTestProtocol(List<Recruit> recruits, string teamNumber,
            DateTime sendDate)
        {
            const string templateFile = TemplatePath + "MedicalTest/Protocol.docx";
            const string tempFile = TempPath + "Protocol.docx";
            CopyTemplateFileToTempDirectory(templateFile, tempFile);
            var replaceTextDictionary = new Dictionary<string, string>
            {
                {"TEAMNUM", teamNumber},
                {"CURRENTDATE", sendDate.Day.ToString()},
                {"CURRENTMONTH", sendDate.GetMonthName()},
                {"CURRENTYEAR", sendDate.Year.ToString()},
                {
                    "RESULT",
                    $"в {recruits.Count} исследуемых пробах из {recruits.Count} не обнаружены антитела SARS-CoV-2."
                }
            };
            using (var document = WordprocessingDocument.Open(tempFile, true))
            {
                try
                {
                    var bookMarks = FindBookmarks(document.MainDocumentPart.Document);
                    RemoveBookMarkContent(bookMarks);
                    var runFonts = new RunFonts() {Ascii = "Times New Roman", HighAnsi = "Times New Roman"};
                    var fontSize = new FontSize() {Val = "24"};
                    var fontSizeComplexScript = new FontSizeComplexScript() {Val = "24"};
                    var runProperties = new RunProperties(new Bold(), runFonts, fontSize, fontSizeComplexScript);
                    foreach (var (bookmarkStart, bookmarkEnd) in bookMarks)
                    {
                        if (!replaceTextDictionary.Select(m => m.Key).Contains<string>(bookmarkStart.Name))
                            continue;
                        var replaceText = replaceTextDictionary.First(m => m.Key == bookmarkStart.Name).Value;


                        var run = new Run();

                        var text = new Text(replaceText);
                        run.Append(runProperties.CloneNode(true), text);
                        bookmarkEnd.InsertAfterSelf(run);
                    }

                    var body = document.MainDocumentPart.Document.Body;
                    var index = 1;
                    var runFontsTable = new RunFonts() {Ascii = "Times New Roman", HighAnsi = "Times New Roman"};
                    var fontSizeTable = new FontSize() {Val = "22"};
                    var fontSizeComplexScriptTable = new FontSizeComplexScript() {Val = "22"};
                    var runPropertiesTable =
                        new RunProperties(runFontsTable, fontSizeTable, fontSizeComplexScriptTable);
                    var paragraphProperties = new ParagraphProperties(new SpacingBetweenLines() {After = "0"});

                    foreach (var table in body.Descendants<Table>())
                    {
                        foreach (var recruit in recruits)
                        {
                            var tableCellIndex = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(index.ToString()))));
                            var tableCellFullName = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(recruit.FullName))));
                            var tableCellDisc = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text("антитела SARS-CoV-2"))));
                            var tableCellTestNum = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(
                                    $"{recruit.AdditionalData.TestNum} от {recruit.AdditionalData.TestDate?.ToShortDateString()}"))));
                            var tableCellResult = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text("не обнаружены"))));
                            var tableRow = new TableRow();
                            tableRow.Append(tableCellIndex, tableCellFullName, tableCellDisc, tableCellTestNum,
                                tableCellResult);
                            table.AppendChild(tableRow);
                            index++;
                        }

                        var nullTCell = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                            new Run(new Text(""))));
                        var nullTRow = new TableRow(nullTCell.CloneNode(true),
                            nullTCell.CloneNode(true), nullTCell.CloneNode(true),
                            nullTCell.CloneNode(true), nullTCell.CloneNode(true));
                        table.AppendChild(nullTRow.CloneNode(true));
                        table.AppendChild(nullTRow.CloneNode(true));
                        table.AppendChild(nullTRow.CloneNode(true));
                    }
                    document.MainDocumentPart.Document.Save();
                }
                catch
                {
                    document.MainDocumentPart.Document.Save();
                    throw new Exception("Неизвестная ошибка");
                }
            }

            return new FileStream(tempFile, FileMode.Open);
        }

        public static FileStream GeneratePersonalGuidanceReport(List<SpecialPerson> persons,
            MilitaryComissariat militaryComissariat)
        {
            const string templateFile = TemplatePath + "PersonalGuidance/person_report.docx";
            var tempFile = TempPath + $"PersonalGuidance/{militaryComissariat.Name}.docx";
            if (!Directory.Exists(TempPath + "PersonalGuidance"))
            {
                Directory.CreateDirectory(TempPath + "PersonalGuidance");
            }
            CopyTemplateFileToTempDirectory(templateFile, tempFile);
            var replaceTextDictionary = new Dictionary<string, string>
            {
                {"MilitaryComissariat", militaryComissariat.Name},
            };
            using (var document = WordprocessingDocument.Open(tempFile, true))
            {
                try
                {
                    var bookMarks = FindBookmarks(document.MainDocumentPart.Document);
                    RemoveBookMarkContent(bookMarks);
                    var runFonts = new RunFonts() {Ascii = "Times New Roman", HighAnsi = "Times New Roman"};
                    var fontSize = new FontSize() {Val = "28"};
                    var fontSizeComplexScript = new FontSizeComplexScript() {Val = "28"};
                    var runProperties = new RunProperties(new Bold(), runFonts, fontSize, fontSizeComplexScript);
                    foreach (var (bookmarkStart, bookmarkEnd) in bookMarks)
                    {
                        if (!replaceTextDictionary.Select(m => m.Key).Contains<string>(bookmarkStart.Name))
                            continue;
                        var replaceText = replaceTextDictionary.First(m => m.Key == bookmarkStart.Name).Value;


                        var run = new Run();

                        var text = new Text(replaceText);
                        run.Append(runProperties.CloneNode(true), text);
                        bookmarkEnd.InsertAfterSelf(run);
                    }

                    var body = document.MainDocumentPart.Document.Body;
                    var index = 1;
                    var runFontsTable = new RunFonts() {Ascii = "Times New Roman", HighAnsi = "Times New Roman"};
                    var fontSizeTable = new FontSize() {Val = "24"};
                    var fontSizeComplexScriptTable = new FontSizeComplexScript() {Val = "24"};
                    var runPropertiesTable =
                        new RunProperties(runFontsTable, fontSizeTable, fontSizeComplexScriptTable);
                    var paragraphProperties = new ParagraphProperties(new SpacingBetweenLines() {After = "0"});

                    foreach (var table in body.Descendants<Table>())
                    {
                        foreach (var person in persons)
                        {
                            var tableCellIndex = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(index.ToString()))));
                            var tableCellFullName = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(person.FullName))));
                            var tableCellBirthYear = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(person.BirthYear.ToString()))));
                            var tableCellMilitaryUnitCode = new TableCell(new Paragraph(
                                paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true),
                                    new Text(person.Requirement.MilitaryUnitCode))));
                            var tableCellMilitaryUnitName = new TableCell(new Paragraph(
                                paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true),
                                    new Text(person.Requirement.MilitaryUnit.Name))));
                            var tableCellSendDate = new TableCell(new Paragraph(paragraphProperties.CloneNode(true),
                                new Run(runPropertiesTable.CloneNode(true), new Text(person.Notice))));
                            var tableRow = new TableRow();
                            tableRow.Append(tableCellIndex, tableCellFullName, tableCellBirthYear,
                                tableCellMilitaryUnitCode, tableCellMilitaryUnitName, tableCellSendDate);
                            table.AppendChild(tableRow);
                            index++;
                        }
                    }
                    document.MainDocumentPart.Document.Save();
                }
                catch
                {
                    document.MainDocumentPart.Document.Save();
                    throw new Exception("Неизвестная ошибка");
                }
            }

            return new FileStream(tempFile, FileMode.Open);
        }
    }
}