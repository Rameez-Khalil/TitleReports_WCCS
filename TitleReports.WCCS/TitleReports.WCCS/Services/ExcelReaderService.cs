using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitleReports.WCCS.Services;

public class ExcelReaderService
{
    public List<LoanPageModel> ReadLoanData(string filepath)
    {
        var loanData = new List<LoanPageModel>();

        using (var workbook = new XLWorkbook(filepath))
        {
            var worksheet = workbook.Worksheet("Loans");
            var rows = worksheet.RangeUsed().RowsUsed(); 

            foreach(var row in rows.Skip(1)) //skipping the header row. 
            {
                //gernate the model.
                var model = new LoanPageModel
                {
                    Client = row.Cell(1).GetString(),
                    SearchDate = row.Cell(2).GetString(),
                    Project = row.Cell(3).GetString(),
                    LoanNumber = row.Cell(4).GetString(),
                    FileNumber = row.Cell(5).GetString(),

                    // Add other LoanPageModel-specific properties below as needed
                    BorrowerName = row.Cell(6).GetString(),
                    PropertyAddress = row.Cell(7).GetString(),

                };
                loanData.Add(model);

            }
        }


        return loanData; 
    }

}
