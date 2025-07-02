public class VestingPageModel : BaseReportModel
{
    public VestingDetails Vesting { get; set; }
}

public class VestingDetails
{
    public string TypeOfDeed { get; set; }
    public string Grantee { get; set; }
    public string Grantor { get; set; }
    public string Dated { get; set; }
    public string Recorded { get; set; }
    public string Book { get; set; }
    public string Page { get; set; }
    public string Instrument { get; set; }
}
