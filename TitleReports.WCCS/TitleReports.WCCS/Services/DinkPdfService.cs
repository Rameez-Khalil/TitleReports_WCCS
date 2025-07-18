using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

public class DinkPdfService
{
    private readonly SynchronizedConverter _converter;

    public DinkPdfService()
    {
        var context = new CustomAssemblyLoadContext();

        var wkhtmlPath = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll");
        Console.WriteLine($"Loading wkhtmltox from: {wkhtmlPath}");

        context.LoadUnmanagedLibrary(wkhtmlPath);
        _converter = new SynchronizedConverter(new PdfTools());
    }

    public void ConvertHtmlToPdf(string htmlPath)
    {
        if (!File.Exists(htmlPath))
        {
            Console.WriteLine($"HTML file not found: {htmlPath}");
            return;
        }

        string htmlContent = File.ReadAllText(htmlPath);
        string pdfPath = Path.ChangeExtension(htmlPath, ".pdf");

        string footerPath = GetFooterPath();
        if (footerPath == null) return;

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings { Top = 20, Bottom = 20 },
                Out = pdfPath
            },
            Objects = {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
                    FooterSettings = {
                        HtmUrl = $"file:///{footerPath.Replace("\\", "/")}",
                        Spacing = 5,
                        Line = false
                    }
                }
            }
        };

        _converter.Convert(doc);
        Console.WriteLine($"PDF generated at: {pdfPath}");
    }

    public void ConvertMultipleHtmlToSinglePdf(List<string> htmlPaths, string outputPdfPath)
    {
        string footerPath = GetFooterPath();
        if (footerPath == null) return;

        var objectSettingsList = new List<ObjectSettings>();

        foreach (var htmlPath in htmlPaths)
        {
            if (!File.Exists(htmlPath))
            {
                Console.WriteLine($"⚠️ Skipping missing HTML file: {htmlPath}");
                continue;
            }

            string htmlContent = File.ReadAllText(htmlPath);

            objectSettingsList.Add(new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8", LoadImages = true },
                FooterSettings = new FooterSettings
                {
                    HtmUrl = $"file:///{footerPath.Replace("\\", "/")}",
                    Spacing = 5,
                    Line = false
                }
            });
        }

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
                Margins = new MarginSettings { Top = 20, Bottom = 20 },
                Out = outputPdfPath
            }
        };

        foreach (var obj in objectSettingsList)
        {
            doc.Objects.Add(obj); // ✅ Use Add() instead of assignment
        }

        _converter.Convert(doc);
        Console.WriteLine($"✅ Combined PDF generated at: {outputPdfPath}");
    }

    private string GetFooterPath()
    {
        var footerPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\RazorTemplates\Footer.htm"));
        if (!File.Exists(footerPath))
        {
            Console.WriteLine($"Footer file not found at: {footerPath}");
            return null;
        }
        return footerPath;
    }
}
