using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.IO;

public static class PdfMerger
{
    public static void MergeCoverWithSearch(string coverPath, string searchPath, string outputPath)
    {
        if (!File.Exists(coverPath))
        {
            Console.WriteLine($"Cover file not found: {coverPath}");
            return;
        }

        if (!File.Exists(searchPath))
        {
            Console.WriteLine($"Search file not found: {searchPath}");
            // Optional: Save only cover if search is missing
            File.Copy(coverPath, outputPath, overwrite: true);
            return;
        }

        using var coverDoc = PdfReader.Open(coverPath, PdfDocumentOpenMode.Import);
        using var searchDoc = PdfReader.Open(searchPath, PdfDocumentOpenMode.Import);
        using var outputDoc = new PdfDocument();

        foreach (var page in coverDoc.Pages)
            outputDoc.AddPage(page);

        foreach (var page in searchDoc.Pages)
            outputDoc.AddPage(page);

        outputDoc.Save(outputPath);
        Console.WriteLine($"Final PDF saved: {outputPath}");
    }
}
