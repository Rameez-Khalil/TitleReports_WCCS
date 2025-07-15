using RazorLight;
using System;
using System.IO;
using System.Threading.Tasks;

public class RazorTemplateRenderer
{
    private readonly string _outputDirectory;
    private readonly RazorLightEngine _engine;

    public RazorTemplateRenderer(string outputDirectory)
    {
        _outputDirectory = outputDirectory;

        // Locate RazorTemplates folder relative to project root (not bin folder)
        string projectRoot = AppContext.BaseDirectory;
        string razorTemplatesPath = Path.Combine(projectRoot, "..", "..", "..", "RazorTemplates");
        razorTemplatesPath = Path.GetFullPath(razorTemplatesPath);

        if (!Directory.Exists(razorTemplatesPath))
        {
            Console.WriteLine($"❌ RazorTemplates folder not found at: {razorTemplatesPath}");
            return;
        }

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(razorTemplatesPath)
            .UseMemoryCachingProvider()
            .Build();
    }

    private async Task RenderHtmlAsync<TModel>(TModel model, string templateName, string fileName)
    {
        if (_engine == null)
        {
            Console.WriteLine("❌ Razor engine not initialized.");
            return;
        }

        string result = await _engine.CompileRenderAsync(templateName, model);
        string outputPath = Path.Combine(_outputDirectory, fileName);
        await File.WriteAllTextAsync(outputPath, result);

        Console.WriteLine($"Page Saved: {outputPath}");
    }

    public Task RenderLoanHtmlAsync(LoanPageModel loan)
    {
        string fileName = $"{loan.LoanNumber}_{loan.FileNumber}_Loan.html";
        return RenderHtmlAsync(loan, "1_LoanPageTemplate.cshtml", fileName);
    }

    public async Task RenderVestingHtmlAsync(VestingPageModel vesting)
    {
        if (_engine == null)
        {
            Console.WriteLine("Razor engine not initialized.");
            return;
        }

        Console.WriteLine($"Rendering vesting for: {vesting.LoanNumber}");

        string templateName = "2_VestingPageTemplate.cshtml";
        string result = await _engine.CompileRenderAsync(templateName, vesting);

        string fileName = $"{vesting.LoanNumber}_{vesting.FileNumber}_Vesting.html";
        string outputPath = Path.Combine(_outputDirectory, fileName);

        await File.WriteAllTextAsync(outputPath, result);

        Console.WriteLine($"Vesting HTML saved to: {outputPath}");
    }

    public Task RenderMtgHtmlAsync(MTGDocumentModel mtgDocument)
    {
        Console.WriteLine("Rendering MTG Page");
        string fileName = $"{mtgDocument.LoanNumber}_{mtgDocument.FileNumber}_Mtg.html";
        return RenderHtmlAsync(mtgDocument, "3_MtgPageTemplate.cshtml", fileName);
    }

    public Task RenderJudgmentHtmlAsync(JudgmentPageModel judgment)
    {
        Console.WriteLine("Rendering Judgment Page");
        string fileName = $"{judgment.LoanNumber}_{judgment.FileNumber}_Judgment.html";
        return RenderHtmlAsync(judgment, "4_JudgmentPageTemplate.cshtml", fileName);
    }

    public Task RenderDisclosureHtmlAsync(DisclosurePageModel disclosure)
    {
        Console.WriteLine("Rendering Disclosure Page");
        string fileName = $"{disclosure.LoanNumber}_{disclosure.FileNumber}_Disclosure.html";
        return RenderHtmlAsync(disclosure, "5_DisclosurePageTemplate.cshtml", fileName);
    }




}
