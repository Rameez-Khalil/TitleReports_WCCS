public class LoanPageModel : BaseReportModel
{
    public string Client { get; set; }
    public string SearchDate { get; set; }
    public string Project { get; set; }

    public string BorrowerName { get; set; }
    public string PropertyAddress { get; set; } 
    public string PropertyCity { get; set; }
    public string PropertyState { get; set; }
    public string PropertyZip { get; set; }
    public string PropertyCounty { get; set; }
    public string LoanAmount { get; set; }
    public string OriginationDate { get; set; }
    public string LienPosition { get; set; }
    public string VestingIssue { get; set; }
    public string AOMChainIssue { get; set; }
    public string JudgmentBeforeLien { get; set; }
    public string JudgmentAmount { get; set; }
    public string MuniLien { get; set; }
    public string MuniAmount { get; set; }
    public string SuperlienState { get; set; }

    public string HOASuperLien { get; set; }
    public string HOAAmount { get; set; }
    public string Notes { get; set; }
}