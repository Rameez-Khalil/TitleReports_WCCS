public class JudgmentPageModel : BaseReportModel
{
    public List<JudgmentEntry> Judgments { get; set; } = new();
}

public class JudgmentEntry
{
    public string Type { get; set; }
    public string Amount { get; set; }
    public string Dated { get; set; }
    public string Recorded { get; set; }
    public string Against { get; set; }
    public string InFavorOf { get; set; }
    public string Book { get; set; }
    public string Page { get; set; }
    public string Instrument { get; set; }
}
