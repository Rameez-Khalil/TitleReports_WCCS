using ClosedXML.Excel;
using TitleReports.WCCS.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Please provide the data file path");
        string filePath = Console.ReadLine();

        Console.WriteLine("Please provide the output directory path:");
        string outputDirectory = Console.ReadLine();

        if (!Directory.Exists(outputDirectory))
        {
            Console.WriteLine("❌ Output directory not found. Creating...");
            Directory.CreateDirectory(outputDirectory);
        }

        if (!File.Exists(filePath))
        {
            Console.WriteLine("❌ File not found.");
            return;
        }

        try
        {
            var workbook = new XLWorkbook(filePath);
            var reader = new ExcelReaderService();

            // Read all datasets once
            var loanDataSet = reader.ReadLoanData(workbook);             // List<LoanPageModel>
            var vestingDataSet = reader.ReadVestingData(workbook);       // List<VestingPageModel>
            var mtgDataSet = reader.ReadMtgData(workbook);               // List<List<MtgPageModel>>
            var judgmentDataSet = reader.ReadJudgmentData(workbook);     // List<JudgmentPageModel>

            Console.WriteLine($"Total loans found: {loanDataSet.Count}");

            // Initialize all renderers once
            var loanRenderer = new RazorTemplateRenderer(outputDirectory);
            var vestingRenderer = new RazorTemplateRenderer(outputDirectory);
             var mtgRenderer = new RazorTemplateRenderer(outputDirectory);
            // var judgmentRenderer = new RazorJudgmentRenderer(outputDirectory);

            for (int i = 0; i < loanDataSet.Count; i++)
            {
                var loan = loanDataSet[i];
                var vesting = vestingDataSet.ElementAtOrDefault(i);

                // All MTGs matching this loan
                var mtgs = mtgDataSet
                    .Where(x => x.LoanNumber == loan.LoanNumber && x.FileNumber == loan.FileNumber)
                    .ToList();

                // Render Loan Page
                await loanRenderer.RenderLoanHtmlAsync(loan);

                // Render Vesting Page if available
                if (vesting != null)
                    await vestingRenderer.RenderVestingHtmlAsync(vesting);

                // Render each MTG Page for the loan
                var mtgDocument = new MTGDocumentModel
                {
                    Client = loan.Client,
                    SearchDate = loan.SearchDate,
                    Project = loan.Project,
                    LoanNumber = loan.LoanNumber,
                    FileNumber = loan.FileNumber,
                    MTGs = mtgs
                };

                if (mtgDocument.MTGs.Any(x => !string.IsNullOrWhiteSpace(x.MTG?.Amount)))
                {
                    await mtgRenderer.RenderMtgHtmlAsync(mtgDocument);
                }

                //Render Judgments.
                var judgment = judgmentDataSet.ElementAtOrDefault(i);

                // Create fallback empty model if null
                var judgmentDocument = new JudgmentPageModel
                {
                    Client = loan.Client,
                    SearchDate = loan.SearchDate,
                    Project = loan.Project,
                    LoanNumber = loan.LoanNumber,
                    FileNumber = loan.FileNumber,
                    Judgments = judgment?.Judgments ?? new List<JudgmentEntry>()
                };

                Console.WriteLine("Rendering Judgment Page");
                await mtgRenderer.RenderJudgmentHtmlAsync(judgmentDocument);

            }


            Console.WriteLine("All assets pages rendered.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
