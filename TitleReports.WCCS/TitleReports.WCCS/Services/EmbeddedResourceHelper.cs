using System.IO;
using System.Reflection;

public static class EmbeddedResourceHelper
{
    public static string ReadTemplate(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"TitleReports.WCCS.ScribanTemplates.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
