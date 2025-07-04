using ClosedXML.Excel;
using TitleReports.WCCS.Services;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Please provide the data file path");
        string filePath = Console.ReadLine();


        //workbook object. 
        var workbook = new XLWorkbook(filePath); 
        var reader = new ExcelReaderService();
        var loanDataSet = reader.ReadLoanData(workbook);
        var vestingDataSet = reader.ReadVestingData(workbook);

        Console.WriteLine($"Total loans found: {loanDataSet.Count}");

        foreach (var loan in loanDataSet)
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

            var vesting = vestingDataSet.FirstOrDefault(v =>
               v.LoanNumber == loan.LoanNumber &&
               v.FileNumber == loan.FileNumber);

            if (vesting != null && vesting.Vesting != null)
            {
                Console.WriteLine("---- Vesting Details ----");
                Console.WriteLine($"Type of Deed:     {vesting.Vesting.TypeOfDeed}");
                Console.WriteLine($"Grantee:          {vesting.Vesting.Grantee}");
                Console.WriteLine($"Grantor:          {vesting.Vesting.Grantor}");
                Console.WriteLine($"Dated:            {vesting.Vesting.Dated}");
                Console.WriteLine($"Recorded:         {vesting.Vesting.Recorded}");
                Console.WriteLine($"Book:             {vesting.Vesting.Book}");
                Console.WriteLine($"Page:             {vesting.Vesting.Page}");
                Console.WriteLine($"Instrument #:     {vesting.Vesting.Instrument}");
            }
            else
            {
                Console.WriteLine("No vesting details found for this loan.");
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
