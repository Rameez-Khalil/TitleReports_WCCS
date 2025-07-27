using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.Reflection.PortableExecutable;

public static class PdfMerger
{
    public static void MergeCoverWithSearch(string coverPath, string searchPath, string outputPath)
    {
        using var outputDocument = new PdfDocument();

        if (File.Exists(coverPath))
        {
            using var coverDoc = PdfReader.Open(coverPath, PdfDocumentOpenMode.Import);
            foreach (var page in coverDoc.Pages)
                outputDocument.AddPage(page);
        }
        else
        {
            Console.WriteLine($"Cover PDF not found at {coverPath}");
        }

        if (File.Exists(searchPath))
        {
            using var searchDoc = PdfReader.Open(searchPath, PdfDocumentOpenMode.Import);
            foreach (var page in searchDoc.Pages)
                outputDocument.AddPage(page);
        }
        else
        {
            Console.WriteLine($"⚠️ Search PDF not found at {searchPath}");
        }

        outputDocument.Save(outputPath);
        Console.WriteLine($" Final merged PDF saved at: {outputPath}");
    }
}
