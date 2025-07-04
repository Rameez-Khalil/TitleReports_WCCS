using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TitleReports.WCCS.Services
{
    public class ExcelReaderService
    {
 

        //READS LOAN RELATED INFORMATON, 1st PAGE OF THE TITLE REPORT. 
        public List<LoanPageModel> ReadLoanData(XLWorkbook workbook)
        {
            var loanData = new List<LoanPageModel>();

            {
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RangeUsed().RowsUsed().ToList();

                if (rows.Count < 2)
                    return loanData;

                var headerRow = rows[0];
                var columnMap = PopulateHeader(headerRow);


                foreach (var row in rows.Skip(1)) // skip header
                {
                    var model = new LoanPageModel
                    {
                        Client = GetValue(row, columnMap, "Client"),
                        SearchDate = GetDateOnly(row, columnMap, "Search Date"),
                        Project = GetValue(row, columnMap, "Project"),
                        LoanNumber = GetValue(row, columnMap, "Servicer Loan ID"),
                        FileNumber = GetValue(row, columnMap, "Collateral ID"),

                        BorrowerName = GetValue(row, columnMap, "Borrower Name"),
                        PropertyAddress = GetValue(row, columnMap, "Property Address"),
                        PropertyCity = GetValue(row, columnMap, "City"),
                        PropertyState = GetValue(row, columnMap, "State"),
                        PropertyZip = GetValue(row, columnMap, "Zipcode"),
                        PropertyCounty = GetValue(row, columnMap, "County"),

                        LoanAmount = GetValue(row, columnMap, "Loan Amount"),
                        OriginationDate = GetDateOnly(row, columnMap, "Origination Date"),
                        LienPosition = GetValue(row, columnMap, "Lien Position"),
                        VestingIssue = GetValue(row, columnMap, "Vesting Issue"),
                        AOMChainIssue = GetValue(row, columnMap, "AOM Chain Issues Summary"),

                        JudgmentBeforeLien = GetValue(row, columnMap, "Judgments Before Target"),
                        JudgmentAmount = GetValue(row, columnMap, "Total Judgments Before Lien"),
                        
                        MuniLien = GetValue(row, columnMap, "Muni Lien"),
                        MuniAmount = GetValue(row, columnMap, "Muni Amount"),

                        HOASuperLien = GetValue(row, columnMap, "HOA Superlien"),
                        HOAAmount = GetValue(row, columnMap, "HOA Amount"),

                        Notes = GetValue(row, columnMap, "Notes")
                    };

                    loanData.Add(model);
                }
            }

            return loanData;
        }


        //READS VESTING RELATED INFORMATION, 2nd PAGE OF THE TITLE REPORT. 
        public List<VestingPageModel> ReadVestingData(XLWorkbook workbook)
        {
            var vestingData = new List<VestingPageModel>();

            var worksheet = workbook.Worksheet("Sheet1");
            if (worksheet == null) return vestingData;
            
            //grab the rows used in sheet1. 
            var rowsUsed = worksheet.RangeUsed().RowsUsed().ToList();

            if (rowsUsed.Count < 2) return vestingData;

            //populate the header. 
            var headerRow = rowsUsed[0];
            var columnMap = PopulateHeader(headerRow);


            //parse each data row. 
            foreach (var row in rowsUsed.Skip(1))
            {
                var model = new VestingPageModel
                {
                    Client = GetValue(row, columnMap, "Client"),
                    SearchDate = GetDateOnly(row, columnMap, "Search Date"),
                    Project = GetValue(row, columnMap, "Project"),
                    LoanNumber = GetValue(row, columnMap, "Servicer Loan ID"),
                    FileNumber = GetValue(row, columnMap, "Collateral ID"),

                    Vesting = new VestingDetails
                    {
                        TypeOfDeed = GetValue(row, columnMap, "Type of Deed"),
                        Grantee = GetValue(row, columnMap, "Grantee"),
                        Grantor = GetValue(row, columnMap, "Grantor"),
                        Dated = GetDateOnly(row, columnMap, "Dated"),
                        Recorded = GetDateOnly(row, columnMap, "Recorded"),
                        Book = GetValue(row, columnMap, "Rec. Book"),
                        Page = GetValue(row, columnMap, "Rec. Page"),
                        Instrument = GetValue(row, columnMap, "Rec. Instrument #")
                    }
                };

                vestingData.Add(model);
            }

            return vestingData;
        }




        //HELPER METHODS. 
        private string GetValue(IXLRangeRow row, Dictionary<string, int> map, string columnName)
        {
            return map.ContainsKey(columnName)
                ? row.Cell(map[columnName]).GetString().Trim()
                : string.Empty;
        }

        private string GetDateOnly(IXLRangeRow row, Dictionary<string, int> map, string columnName)
        {
            if (!map.ContainsKey(columnName))
                return string.Empty;

            var raw = row.Cell(map[columnName]).GetString().Trim();
            return raw.Split(' ')[0]; // removes time part
        }

        private Dictionary<string, int> PopulateHeader(IXLRangeRow headerRow)
        {
            var columnMap = new Dictionary<string, int>();

            for (int i = 1; i <= headerRow.CellCount(); i++)
            {
                var header = headerRow.Cell(i).GetString().Trim();
                if (!string.IsNullOrEmpty(header) && !columnMap.ContainsKey(header))
                {
                    columnMap[header] = i;
                }
            }

            return columnMap;
        }
    }
}
