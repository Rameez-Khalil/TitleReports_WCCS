using TitleReports.WCCS.Services;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Please provide the data file path");
        string filePath = Console.ReadLine();

        var reader = new ExcelReaderService();
        var loans = reader.ReadLoanData(filePath);

        Console.WriteLine($"Total loans found: {loans.Count}");

        foreach (var loan in loans)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine($"Loan Number: {loan.LoanNumber}");
            Console.WriteLine($"File Number: {loan.FileNumber}");
            Console.WriteLine($"Client: {loan.Client}");
            Console.WriteLine($"Search Date: {loan.SearchDate}");
            Console.WriteLine($"Project: {loan.Project}");
            Console.WriteLine($"Borrower Name: {loan.BorrowerName}");
            Console.WriteLine($"Property Address: {loan.PropertyAddress}");
            Console.WriteLine($"Property City: {loan.PropertyCity}");
            Console.WriteLine($"Property State: {loan.PropertyState}");
            Console.WriteLine($"Property Zip: {loan.PropertyZip}");
            Console.WriteLine($"Property County: {loan.PropertyCounty}");
            Console.WriteLine($"Loan Amount: {loan.LoanAmount}");
            Console.WriteLine($"Origination Date: {loan.OriginationDate}");
            Console.WriteLine($"Lien Position: {loan.LienPosition}");
            Console.WriteLine($"Vesting Issue: {loan.VestingIssue}");
            Console.WriteLine($"AOM Chain Issue: {loan.AOMChainIssue}");
            Console.WriteLine($"Judgment Before Lien: {loan.JudgmentBeforeLien}");
            Console.WriteLine($"Judgment Amount: {loan.JudgmentAmount}");
            Console.WriteLine($"Muni Lien: {loan.MuniLien}");
            Console.WriteLine($"Muni Amount: {loan.MuniAmount}");
            Console.WriteLine($"HOA Super Lien: {loan.HOASuperLien}");
            Console.WriteLine($"HOA Amount: {loan.HOAAmount}");
            Console.WriteLine($"Notes: {loan.Notes}");
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
