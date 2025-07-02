public class MTGPageModel : BaseReportModel
{
    public MortgageDetails MTG { get; set; }
    public List<AOMDetails> AOMs { get; set; } = new();
}

public class MortgageDetails
{
    public string Title { get; set; }
    public string InstrumentType { get; set; }
    public string Amount { get; set; }
    public string Borrower { get; set; }
    public string Lender { get; set; }
    public string Dated { get; set; }
    public string Recorded { get; set; }
    public string Maturity { get; set; }
    public string OpenEnded { get; set; }
    public string Book { get; set; }
    public string Page { get; set; }
    public string Instrument { get; set; }
    public string Notes { get; set; }
}

public class AOMDetails
{
    public string From { get; set; }
    public string To { get; set; }
    public string Dated { get; set; }
    public string Recorded { get; set; }
    public string Book { get; set; }
    public string Page { get; set; }
    public string Instrument { get; set; }
}
