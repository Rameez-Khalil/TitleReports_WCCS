using System;
using System.IO;
using Scriban;

class HtmlRenderer
{
    public static void RenderLoanHtml(LoanPageModel loan)
    {
        string templatePath = Path.Combine("ScribanTemplates", "1_LoanPage.html");

        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"❌ Template not found at: {templatePath}");
            return;
        }

        string templateContent = File.ReadAllText(templatePath);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            Console.WriteLine("❌ Template parsing failed:");
            foreach (var error in template.Messages)
                Console.WriteLine($" - {error}");
            return;
        }

        // Render the HTML using Scriban
        string result = template.Render(loan, member => member.Name);

        // Prompt user for output directory
        Console.WriteLine("📁 Please enter the full output directory path (leave empty for default 'Output'):");
        string userDir = Console.ReadLine();
        string outputDir = string.IsNullOrWhiteSpace(userDir) ? "Output" : userDir;

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        string outputFile = $"{loan.LoanNumber}_{loan.FileNumber}_Loan.html";
        string outputPath = Path.Combine(outputDir, outputFile);

        File.WriteAllText(outputPath, result);

        Console.WriteLine($"✅ Loan HTML generated at: {outputPath}");
    }
}
