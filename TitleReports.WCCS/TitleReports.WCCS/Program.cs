//using ClosedXML.Excel;
//using TitleReports.WCCS.Services;

//class Program
//{
//    static async Task Main(string[] args)
//    {


//        Console.WriteLine("Please provide the data file path");
//        string filePath = Console.ReadLine();

//        Console.WriteLine("Please provide the output directory path:");
//        string outputDirectory = Console.ReadLine();

//        var pdfService = new DinkPdfService();
//        void ConvertHtmlToPdfIfExists(string fileName)
//        {
//            string htmlPath = Path.Combine(outputDirectory, fileName);
//            if (File.Exists(htmlPath))
//                pdfService.ConvertHtmlToPdf(htmlPath);
//        }

//        if (!Directory.Exists(outputDirectory))
//        {
//            Console.WriteLine("❌ Output directory not found. Creating...");
//            Directory.CreateDirectory(outputDirectory);
//        }

//        if (!File.Exists(filePath))
//        {
//            Console.WriteLine("❌ File not found.");
//            return;
//        }

//        try
//        {
//            var workbook = new XLWorkbook(filePath);
//            var reader = new ExcelReaderService();

//            // Read all datasets once
//            var loanDataSet = reader.ReadLoanData(workbook);            
//            var vestingDataSet = reader.ReadVestingData(workbook);       
//            var mtgDataSet = reader.ReadMtgData(workbook);              
//            var judgmentDataSet = reader.ReadJudgmentData(workbook);    
//            var disclosureDataSet = reader.ReadDisclosureData(workbook); 


//            Console.WriteLine($"Total loans found: {loanDataSet.Count}");

//            // Initialize all renderers once
//            var loanRenderer = new RazorTemplateRenderer(outputDirectory);
//            var vestingRenderer = new RazorTemplateRenderer(outputDirectory);
//             var mtgRenderer = new RazorTemplateRenderer(outputDirectory);
//             var judgmentRenderer = new RazorTemplateRenderer(outputDirectory);

//            for (int i = 0; i < loanDataSet.Count; i++)
//            {
//                var loan = loanDataSet[i];
//                var vesting = vestingDataSet.ElementAtOrDefault(i);

//                // All MTGs matching this loan
//                var mtgs = mtgDataSet
//                    .Where(x => x.LoanNumber == loan.LoanNumber && x.FileNumber == loan.FileNumber)
//                    .ToList();

//                // Render Loan Page
//                await loanRenderer.RenderLoanHtmlAsync(loan);
//                ConvertHtmlToPdfIfExists($"{loan.LoanNumber}_{loan.FileNumber}_Loan.html");

//                // Render Vesting Page if available
//                if (vesting != null)
//                    await vestingRenderer.RenderVestingHtmlAsync(vesting);
//                ConvertHtmlToPdfIfExists($"{loan.LoanNumber}_{loan.FileNumber}_Vesting.html");


//                // Render each MTG Page for the loan
//                var mtgDocument = new MTGDocumentModel
//                {
//                    Client = loan.Client,
//                    SearchDate = loan.SearchDate,
//                    Project = loan.Project,
//                    LoanNumber = loan.LoanNumber,
//                    FileNumber = loan.FileNumber,
//                    MTGs = mtgs
//                };

//                if (mtgDocument.MTGs.Any(x => !string.IsNullOrWhiteSpace(x.MTG?.Amount)))
//                {
//                    await mtgRenderer.RenderMtgHtmlAsync(mtgDocument);
//                    ConvertHtmlToPdfIfExists($"{loan.LoanNumber}_{loan.FileNumber}_Mtg.html");

//                }

//                //Render Judgments.
//                var judgment = judgmentDataSet
//                    .FirstOrDefault(j =>
//                        j.LoanNumber == loan.LoanNumber &&
//                        j.FileNumber == loan.FileNumber);
//                // Create fallback empty model if null
//                var judgmentDocument = new JudgmentPageModel
//                {
//                    Client = loan.Client,
//                    SearchDate = loan.SearchDate,
//                    Project = loan.Project,
//                    LoanNumber = loan.LoanNumber,
//                    FileNumber = loan.FileNumber,
//                    Judgments = judgment?.Judgments ?? new List<JudgmentEntry>()
//                };

//                Console.WriteLine("Rendering Judgment Page");
//                await judgmentRenderer.RenderJudgmentHtmlAsync(judgmentDocument);
//                ConvertHtmlToPdfIfExists($"{loan.LoanNumber}_{loan.FileNumber}_Judgment.html");


//                //Render disclosure page.
//                var disclosure = disclosureDataSet
//                    .FirstOrDefault(d => d.LoanNumber == loan.LoanNumber && d.FileNumber == loan.FileNumber);

//                if (disclosure != null)
//                {
//                    await mtgRenderer.RenderDisclosureHtmlAsync(disclosure);
//                    ConvertHtmlToPdfIfExists($"{loan.LoanNumber}_{loan.FileNumber}_Disclosure.html");

//                }

//            }


//            Console.WriteLine("All assets pages rendered.");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error: {ex.Message}");
//        }

//        Console.WriteLine("Press any key to exit...");
//        Console.ReadKey();
//    }
//}


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
        Console.WriteLine("Please provide the Excel data file path:");
        string excelFilePath = Console.ReadLine();

        if (!File.Exists(excelFilePath))
        {
            Console.WriteLine("Excel file not found.");
            return;
        }

        Console.WriteLine("Please provide the folder path to save all HTML files:");
        string htmlOutputDir = Console.ReadLine();

        Console.WriteLine("Please provide the folder path to save combined PDF files:");
        string pdfOutputDir = Console.ReadLine();

        Directory.CreateDirectory(htmlOutputDir);
        Directory.CreateDirectory(pdfOutputDir);

        try
        {
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

                // Loan Page
                await loanRenderer.RenderLoanHtmlAsync(loan);
                htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Loan.html"));

                // Vesting Page
                if (vesting != null)
                {
                    await vestingRenderer.RenderVestingHtmlAsync(vesting);
                    htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Vesting.html"));
                }

                // MTG Page
                var mtgs = mtgDataSet
                    .Where(x => x.LoanNumber == loan.LoanNumber && x.FileNumber == loan.FileNumber)
                    .ToList();

                if (mtgs.Any(x => !string.IsNullOrWhiteSpace(x.MTG?.Amount)))
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

                // Judgment Page
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

                // Disclosure Page
                var disclosure = disclosureDataSet
                    .FirstOrDefault(d => d.LoanNumber == loan.LoanNumber && d.FileNumber == loan.FileNumber);

                if (disclosure != null)
                {
                    await mtgRenderer.RenderDisclosureHtmlAsync(disclosure);
                    htmlFiles.Add(Path.Combine(htmlOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Disclosure.html"));
                }

                // ✅ Combine all pages into one PDF per asset
                string combinedPdfPath = Path.Combine(pdfOutputDir, $"{loan.LoanNumber}_{loan.FileNumber}_Combined.pdf");
                pdfService.ConvertMultipleHtmlToSinglePdf(htmlFiles, combinedPdfPath);
            }

            Console.WriteLine("All HTMLs and combined PDFs generated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
