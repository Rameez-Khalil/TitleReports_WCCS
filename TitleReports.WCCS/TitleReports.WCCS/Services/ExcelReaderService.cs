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
                        SuperlienState = GetValue(row, columnMap,"Superlien State"),
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

        // READS MORTGAGE AND AOM INFORMATION
        public List<MTGPageModel> ReadMtgData(XLWorkbook workbook)
        {
            var mtgData = new List<MTGPageModel>();

            var worksheet = workbook.Worksheet("Sheet1");            // adjust sheet name if needed
            if (worksheet == null) return mtgData;

            var rows = worksheet.RangeUsed().RowsUsed().ToList();
            if (rows.Count < 2) return mtgData;

            var headerRow = rows[0];
            var columnMap = PopulateHeader(headerRow);

            // regex to find columns like "1st MTG Amount"
            var mtgCoreRegex = new System.Text.RegularExpressions.Regex(
                @"^(?<idx>\d+)(?:st|nd|rd|th)\s+MTG\s+Amount$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var row in rows.Skip(1))    // each asset row
            {
                // ── Phase 1: discover which MTG indices exist in this file ──
                var mtgIndices = columnMap.Keys
                    .Select(h => mtgCoreRegex.Match(h))
                    .Where(m => m.Success)
                    .Select(m => int.Parse(m.Groups["idx"].Value))
                    .Distinct()
                    .OrderBy(i => i);

                foreach (var mtgIdx in mtgIndices)
                {
                    var ordinal = ToOrdinal(mtgIdx);                  // "1st", "2nd", …
                    string mtgPrefix = $"{ordinal} MTG ";

                    // create model & fill shared base fields
                    var pageModel = new MTGPageModel();
                    FillBaseFields(pageModel, row, columnMap);

                    // ── Mortgage core fields ──
                    pageModel.MTG = new MortgageDetails
                    {
                        Title = $"{ordinal} Mortgage",
                        Amount = GetValue(row, columnMap, mtgPrefix + "Amount"),
                        InstrumentType = GetValue(row, columnMap, mtgPrefix + "Type"),
                        Borrower = GetValue(row, columnMap, mtgPrefix + "Borrower"),
                        Lender = GetValue(row, columnMap, mtgPrefix + "Lender"),
                        OpenEnded = GetValue(row, columnMap, mtgPrefix + "is Open Ended?"),
                        Dated = GetDateOnly(row, columnMap, mtgPrefix + "Dated"),
                        Recorded = GetDateOnly(row, columnMap, mtgPrefix + "Recorded"),
                        Maturity = GetDateOnly(row, columnMap, mtgPrefix + "Maturity Date"),
                        Book = GetValue(row, columnMap, mtgPrefix + "Rec. Book"),
                        Page = GetValue(row, columnMap, mtgPrefix + "Rec. Page"),
                        Instrument = GetValue(row, columnMap, mtgPrefix + "Rec. Instrument #"),
                        Notes = GetValue(row, columnMap, mtgPrefix + "Notes")
                    };

                    // ── Phase 2: discover AOMs for this MTG ──
                    var aomRegex = new System.Text.RegularExpressions.Regex(
                        $"^{mtgIdx}(?:st|nd|rd|th)\\s+MTG\\s+(?<aom>\\d+)(?:st|nd|rd|th)\\s+AOM\\s+From$",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var aomIndices = columnMap.Keys
                        .Select(h => aomRegex.Match(h))
                        .Where(m => m.Success)
                        .Select(m => int.Parse(m.Groups["aom"].Value))
                        .Distinct()
                        .OrderBy(a => a);

                    foreach (var aIdx in aomIndices)
                    {
                        var aOrdinal = ToOrdinal(aIdx);
                        string aPrefix = $"{ordinal} MTG {aOrdinal} AOM ";

                        var from = GetValue(row, columnMap, aPrefix + "From");
                        var to = GetValue(row, columnMap, aPrefix + "To");

                        // Skip empty AOM rows (all key fields blank)
                        if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
                            continue;

                        pageModel.AOMs.Add(new AOMDetails
                        {
                            From = from,
                            To = to,
                            Dated = GetDateOnly(row, columnMap, aPrefix + "Dated"),
                            Recorded = GetDateOnly(row, columnMap, aPrefix + "Recorded"),
                            Book = GetValue(row, columnMap, aPrefix + "Rec. Book"),
                            Page = GetValue(row, columnMap, aPrefix + "Rec. Page"),
                            Instrument = GetValue(row, columnMap, aPrefix + "Rec. Instrument #"),
                            // Optional fields: Type / Comment if present
                            // Type     = GetValue(row, columnMap, aPrefix + "Type"),
                            // Comment  = GetValue(row, columnMap, aPrefix + "Comment")
                        });
                    }

                    mtgData.Add(pageModel);
                }
            }

            return mtgData;
        }


        //READS JUDGMENT DATA. 
        public List<JudgmentPageModel> ReadJudgmentData(XLWorkbook workbook)
        {
            var judgments = new List<JudgmentPageModel>();
            var sheet = workbook.Worksheet("Sheet1");
            if (sheet == null) return judgments;

            var rows = sheet.RangeUsed().RowsUsed().ToList();
            if (rows.Count < 2) return judgments;

            var columnMap = PopulateHeader(rows[0]);

            // Extract all judgment prefixes like "1st", "2nd", "3rd"
            var judgmentPrefixes = columnMap.Keys
                .Where(h => h.Contains("Judgment Type"))
                .Select(h => h.Split(' ')[0])  // "1st", "2nd", etc.
                .Distinct()
                .ToList();

            foreach (var row in rows.Skip(1))
            {
                var model = new JudgmentPageModel
                {
                    Client = GetValue(row, columnMap, "Client"),
                    SearchDate = GetDateOnly(row, columnMap, "Search Date"),
                    Project = GetValue(row, columnMap, "Project"),
                    LoanNumber = GetValue(row, columnMap, "Servicer Loan ID"),
                    FileNumber = GetValue(row, columnMap, "Collateral ID"),
                };

                foreach (var prefix in judgmentPrefixes)
                {
                    var type = GetValue(row, columnMap, $"{prefix} Judgment Type");
                    if (string.IsNullOrWhiteSpace(type)) continue;

                    var entry = new JudgmentEntry
                    {
                        Type = type,
                        Against = GetValue(row, columnMap, $"{prefix} Judgment Against"),
                        InFavorOf = GetValue(row, columnMap, $"{prefix} Judgment In Favor Of"),
                        Amount = GetValue(row, columnMap, $"{prefix} Judgment Amount"),
                        Dated = GetDateOnly(row, columnMap, $"{prefix} Judgment Dated"),
                        Recorded = GetDateOnly(row, columnMap, $"{prefix} Judgment Recorded"),
                        Book = GetValue(row, columnMap, $"{prefix} Judgment Rec. Book"),
                        Page = GetValue(row, columnMap, $"{prefix} Judgment Rec. Page"),
                        Instrument = GetValue(row, columnMap, $"{prefix} Judgment Rec. Instrument #")
                    };

                    model.Judgments.Add(entry);
                }

                // Add to list only if at least one judgment exists
                if (model.Judgments.Any())
                    judgments.Add(model);
            }

            return judgments;
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

        // Converts 1 → "1st", 2 → "2nd", 3 → "3rd", etc.
        private string ToOrdinal(int num)
        {
            int rem100 = num % 100;
            if (rem100 is 11 or 12 or 13) return $"{num}th";
            return (num % 10) switch
            {
                1 => $"{num}st",
                2 => $"{num}nd",
                3 => $"{num}rd",
                _ => $"{num}th"
            };
        }

        // Populates the common header fields on any page model
        private void FillBaseFields(BaseReportModel target,
                                    IXLRangeRow row,
                                    Dictionary<string, int> map)
        {
            target.Client = GetValue(row, map, "Client");
            target.SearchDate = GetDateOnly(row, map, "Search Date");
            target.Project = GetValue(row, map, "Project");
            target.LoanNumber = GetValue(row, map, "Servicer Loan ID");
            target.FileNumber = GetValue(row, map, "Collateral ID");
        }
    }
}
