using ClosedXML.Excel;
using TitleReports.WCCS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Please provide the Excel data file path:");
                string excelFilePath = Console.ReadLine();

                Console.WriteLine("Please provide the output directory path for HTML files:");
                string htmlOutputDir = Console.ReadLine();

                Console.WriteLine("Please provide the output directory path for PDF files:");
                string pdfOutputDir = Console.ReadLine();

                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1 - Generate ALL pages (Loan, Vesting, MTG, Judgment, Disclosure)");
                Console.WriteLine("2 - Generate ONLY Loan and Disclosure pages");
                string option = Console.ReadLine();

                Directory.CreateDirectory(htmlOutputDir);
                Directory.CreateDirectory(pdfOutputDir);

                if (!File.Exists(excelFilePath))
                {
                    Console.WriteLine(" Excel file not found.");
                    continue;
                }

                var workbook = new XLWorkbook(excelFilePath);
                var reader = new ExcelReaderService();

                var loanDataSet = reader.ReadLoanData(workbook);
               

                var vestingDataSet = reader.ReadVestingData(workbook);
                var mtgDataSet = reader.ReadMtgData(workbook);
                var judgmentDataSet = reader.ReadJudgmentData(workbook);
                var disclosureDataSet = reader.ReadDisclosureData(workbook);

                var pdfService = new DinkPdfService();

                var loanRenderer = new RazorTemplateRenderer(htmlOutputDir);
                var vestingRenderer = new RazorTemplateRenderer(htmlOutputDir);
                var mtgRenderer = new RazorTemplateRenderer(htmlOutputDir);
                var judgmentRenderer = new RazorTemplateRenderer(htmlOutputDir);

                Console.WriteLine($"Total loans found: {loanDataSet.Count}");

                for (int i = 0; i < loanDataSet.Count; i++)
                {
                    var loan = loanDataSet[i];
                    var vesting = vestingDataSet.ElementAtOrDefault(i);
                    var htmlFiles = new List<string>();

                    await loanRenderer.RenderLoanHtmlAsync(loan);
                    htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Loan.html"));

                    if (option == "1" && vesting != null)
                    {
                        await vestingRenderer.RenderVestingHtmlAsync(vesting);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Vesting.html"));
                    }

                    var mtgs = mtgDataSet
                        .Where(x => x.LoanNumber == loan.LoanNumber && x.FileNumber == loan.FileNumber)
                        .ToList();

                    if (option == "1" && mtgs.Any(x => !string.IsNullOrWhiteSpace(x.MTG?.Amount)))
                    {
                        var mtgDoc = new MTGDocumentModel
                        {
                            Client = loan.Client,
                            SearchDate = loan.SearchDate,
                            Project = loan.Project,
                            LoanNumber = loan.LoanNumber,
                            FileNumber = loan.FileNumber,
                            MTGs = mtgs
                        };

                        await mtgRenderer.RenderMtgHtmlAsync(mtgDoc);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Mtg.html"));
                    }

                    if (option == "1")
                    {
                        var judgment = judgmentDataSet.FirstOrDefault(j =>
                            j.LoanNumber == loan.LoanNumber &&
                            j.FileNumber == loan.FileNumber);

                        var judgmentDoc = new JudgmentPageModel
                        {
                            Client = loan.Client,
                            SearchDate = loan.SearchDate,
                            Project = loan.Project,
                            LoanNumber = loan.LoanNumber,
                            FileNumber = loan.FileNumber,
                            Judgments = judgment?.Judgments ?? new List<JudgmentEntry>()
                        };

                        await judgmentRenderer.RenderJudgmentHtmlAsync(judgmentDoc);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Judgment.html"));
                    }

                    var disclosure = disclosureDataSet
                        .FirstOrDefault(d => d.LoanNumber == loan.LoanNumber && d.FileNumber == loan.FileNumber);

                    if (disclosure != null)
                    {
                        await mtgRenderer.RenderDisclosureHtmlAsync(disclosure);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Disclosure.html"));
                    }

                    string combinedPdfPath = Path.Combine(pdfOutputDir, $"{loan.LoanNumber}_coverPage.pdf");
                    pdfService.ConvertMultipleHtmlToSinglePdf(htmlFiles, combinedPdfPath);
                }

                Console.WriteLine("\n HTMLs and PDFs generated successfully.");
                Console.WriteLine(" Press Esc to exit or any other key to run again...\n");

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Error: {ex.Message}");
                Console.WriteLine("Press Esc to exit or any other key to try again...\n");

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
        }
    }

}
