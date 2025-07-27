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
        var searchLogs = new List<string>();

        while (true)
        {
            try
            {
                Console.WriteLine("Please provide the Excel data file path:");
                string excelFilePath = Console.ReadLine();

                Console.WriteLine("Please provide the output directory path for HTML files:");
                string htmlOutputDir = Console.ReadLine();

                Console.WriteLine("Please provide the output directory path for cover PDFs:");
                string coverPdfDir = Console.ReadLine();

                Console.WriteLine("Please provide the directory path for existing Search PDFs:");
                string searchPdfDir = Console.ReadLine();

                Console.WriteLine("Please provide the output directory path for final merged PDFs:");
                string finalPdfDir = Console.ReadLine();

                Console.WriteLine("\nSelect an option:");
                Console.WriteLine("1 - Generate ALL pages (Loan, Vesting, MTG, Judgment, Disclosure)");
                Console.WriteLine("2 - Generate ONLY Loan and Disclosure pages");
                Console.WriteLine("3 - Generate ONLY Vesting, MTG, Judgment, and Disclosure pages (skip Loan)");

                string option = Console.ReadLine();

                Directory.CreateDirectory(htmlOutputDir);
                Directory.CreateDirectory(coverPdfDir);
                Directory.CreateDirectory(finalPdfDir);

                if (!File.Exists(excelFilePath))
                {
                    Console.WriteLine("❌ Excel file not found.");
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
                var razorRenderer = new RazorTemplateRenderer(htmlOutputDir);

                Console.WriteLine($"\nTotal loans found: {loanDataSet.Count}");

                for (int i = 0; i < loanDataSet.Count; i++)
                {
                    var loan = loanDataSet[i];
                    Console.WriteLine("\n*********************************************************************");
                    Console.WriteLine($"Working on loan {loan.LoanNumber}");

                    var vesting = vestingDataSet.ElementAtOrDefault(i);
                    var htmlFiles = new List<string>();

                    if (option == "1" || option == "2")
                    {
                        await razorRenderer.RenderLoanHtmlAsync(loan);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Loan.html"));
                    }

                    if ((option == "1" || option == "3") && vesting != null)
                    {
                        await razorRenderer.RenderVestingHtmlAsync(vesting);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Vesting.html"));
                    }

                    if (option == "1" || option == "3")
                    {
                        var mtgs = mtgDataSet
                            .Where(x => x.LoanNumber == loan.LoanNumber && x.FileNumber == loan.FileNumber)
                            .ToList();

                        var mtgDoc = new MTGDocumentModel
                        {
                            Client = loan.Client,
                            SearchDate = loan.SearchDate,
                            Project = loan.Project,
                            LoanNumber = loan.LoanNumber,
                            FileNumber = loan.FileNumber,
                            MTGs = mtgs
                        };

                        await razorRenderer.RenderMtgHtmlAsync(mtgDoc);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Mtg.html"));

                        var judgment = judgmentDataSet.FirstOrDefault(j =>
                            j.LoanNumber == loan.LoanNumber && j.FileNumber == loan.FileNumber);

                        var judgmentDoc = new JudgmentPageModel
                        {
                            Client = loan.Client,
                            SearchDate = loan.SearchDate,
                            Project = loan.Project,
                            LoanNumber = loan.LoanNumber,
                            FileNumber = loan.FileNumber,
                            Judgments = judgment?.Judgments ?? new List<JudgmentEntry>()
                        };

                        await razorRenderer.RenderJudgmentHtmlAsync(judgmentDoc);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Judgment.html"));
                    }

                    var disclosure = disclosureDataSet
                        .FirstOrDefault(d => d.LoanNumber == loan.LoanNumber && d.FileNumber == loan.FileNumber);

                    if (disclosure != null)
                    {
                        await razorRenderer.RenderDisclosureHtmlAsync(disclosure);
                        htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Disclosure.html"));
                    }

                    // Generate Cover PDF
                    string coverPdfPath = Path.Combine(coverPdfDir, $"{loan.LoanNumber}_coverPage.pdf");

                    try
                    {
                        pdfService.ConvertMultipleHtmlToSinglePdf(htmlFiles, coverPdfPath);
                        Console.WriteLine(File.Exists(coverPdfPath)
                            ? $" Cover PDF created: {coverPdfPath}"
                            : $"Cover PDF missing after generation: {coverPdfPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error generating cover PDF for loan {loan.LoanNumber}: {ex.Message}");
                        searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Cover PDF generation failed");
                        continue;
                    }

                    // Determine Search PDF Path
                    string searchPdfPath = Path.Combine(searchPdfDir, $"{loan.FileNumber}.pdf");
                    bool searchPdfFound = false;

                    if (loan.PropertyState?.Trim().ToUpper() == "NY")
                    {
                        string lmlPath = Path.Combine(searchPdfDir, $"{loan.FileNumber}_LML.pdf");
                        string copPath = Path.Combine(searchPdfDir, $"{loan.FileNumber}_COP.pdf");

                        if (File.Exists(lmlPath))
                        {
                            searchPdfPath = lmlPath;
                            searchPdfFound = true;
                        }
                        else if (File.Exists(copPath))
                        {
                            searchPdfPath = copPath;
                            searchPdfFound = true;
                        }
                        else
                        {
                            Console.WriteLine($"No _LML or _COP file found for NY loan {loan.LoanNumber}");
                        }
                    }
                    else
                    {
                        searchPdfFound = File.Exists(searchPdfPath);
                    }

                    // Merge PDFs
                    string finalPdfPath = Path.Combine(finalPdfDir, $"{loan.LoanNumber}.pdf");

                    if (!File.Exists(coverPdfPath))
                    {
                        Console.WriteLine($"Skipping merge — cover page missing for {loan.LoanNumber}");
                        searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Cover page missing");
                    }
                    else if (!searchPdfFound)
                    {
                        Console.WriteLine($"Skipping merge — search PDF not found for {loan.FileNumber}");
                        searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Search PDF not found");
                    }
                    else
                    {
                        try
                        {
                            PdfMerger.MergeCoverWithSearch(coverPdfPath, searchPdfPath, finalPdfPath);

                            if (File.Exists(finalPdfPath))
                            {
                                Console.WriteLine($"Final PDF saved: {finalPdfPath}");
                                searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Merged successfully");
                            }
                            else
                            {
                                Console.WriteLine($"Final file missing after merge: {finalPdfPath}");
                                searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Merge attempted but file missing");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Merge failed for {loan.LoanNumber}: {ex.Message}");
                            searchLogs.Add($"{loan.LoanNumber},{loan.FileNumber},Merge failed: {ex.Message}");
                        }
                    }

                    Console.WriteLine("*********************************************************************");
                }

                string logPath = Path.Combine(finalPdfDir, "MissingSearchLogs.csv");
                File.WriteAllLines(logPath, new[] { "LoanNumber,FileNumber,Status" }.Concat(searchLogs));
                Console.WriteLine($"\n📄 Log saved: {logPath}");
                Console.WriteLine("\n✅ Process complete. Press Esc to exit or any other key to run again...");

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Unexpected error: {ex.Message}");
                Console.WriteLine("Press Esc to exit or any other key to try again...");

                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
        }
    }
}
