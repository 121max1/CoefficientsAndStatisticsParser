using DocumentFormat.OpenXml.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class ExcelGenerator
    {
        private List<Color> _colors = new List<Color>() { Color.LightBlue, Color.LightCoral, Color.OrangeRed, Color.LightSeaGreen, Color.LightGray, Color.LightCyan, Color.Orange, Color.MediumVioletRed, Color.LightGoldenrodYellow };

        public byte[] GenerateFile(WinDrawLoseAndStatistics windrawLoseAndStatistics, 
                                   CoefficentsHandicapAndStatistics coefficentsHandicapAndStatistics, 
                                   CoefficeintsTotalAndStatistics coefficeintsTotalAndStatistics,
                                   List<CoefficientsWinDrawLose> winDrawLoseBetBrain,
                                   List<CoefficientsHandicap> handicapBetBrain,
                                   List<CoefficientsTotal> totalBetBrain)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            FillWinDrawLoseSheet(package, windrawLoseAndStatistics, winDrawLoseBetBrain);
            FillHandicapSheet(package, coefficentsHandicapAndStatistics, handicapBetBrain);
            FillTotalsSheet(package, coefficeintsTotalAndStatistics, totalBetBrain);
            return package.GetAsByteArray();
        }

        public byte[] FillScore(List<TeamResult> result, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(fileName);
            foreach(var sheet in package.Workbook.Worksheets)
            {
                int resultCol = 1;
                while ((sheet.Cells[2, resultCol].Value == null ? " " : sheet.Cells[2, resultCol].Value.ToString()) != "Result")
                {
                    resultCol++;
                }
                int row = 3;

                var rows = new List<int>();

                while (sheet.Cells[row, resultCol].Value is not null)
                {
                    if (sheet.Cells[row, resultCol].Value.ToString().Contains("http"))
                    {
                        rows.Add(row);
                    }
                    row++;
                }

                for (int i = 0; i < rows.Count; i++)
                {
                    if (result[i].FirstTeam != null && result[i].SecondTeam != null)
                    {
                        sheet.Cells[rows[i], resultCol].Value = result[i].FirstTeam;
                        sheet.Cells[rows[i], resultCol + 1].Value = result[i].SecondTeam;
                    }
                }
            }
            return package.GetAsByteArray();
        }

        public List<string> GetMatchesHrefs(string filename)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(filename);
            var matchHrefs = new List<string>();
            var sheet = package.Workbook.Worksheets.First();
            int resultCol = 1;
            while((sheet.Cells[2, resultCol].Value == null ? " " : sheet.Cells[2, resultCol].Value.ToString()) != "Result")
            {
                resultCol++;
            }
            int row = 3;
            while (sheet.Cells[row, resultCol].Value is not null)
            {
                if(sheet.Cells[row, resultCol].Value.ToString().Contains("http"))
                {
                    matchHrefs.Add(sheet.Cells[row, resultCol].Value.ToString());
                }
                row++;
            }
            return matchHrefs;
        }

        public void FillTotalsSheet(ExcelPackage excelPackage, CoefficeintsTotalAndStatistics coefficeintsTotalAndStatistics, List<CoefficientsTotal> totalBetBrain)
        {
            var sheet = excelPackage.Workbook.Worksheets.Add("тоталы");
            sheet.Cells["A2"].Value = "date";
            sheet.Cells["B2"].Value = "home";
            sheet.Cells["C2"].Value = "guest";
            int indexMatchCoef = 3;
        
            foreach (var match in coefficeintsTotalAndStatistics.BookmakerTotals)
            {
                sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                int startCoefIndex = 4;
                int colorIndex = 0;
                foreach (var totalBookmaker in match.BookmakerTotals.OrderBy(tb => tb.Bookmaker))
                {
                    int cellAmount = totalBookmaker.TotalCoeffs.Count * 2;
                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Value = totalBookmaker.Bookmaker.ToString();
                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);
                    int currentPairCoeff = startCoefIndex;
                    foreach (var totalPair in totalBookmaker.TotalCoeffs.OrderBy(tp => tp.Handicap))
                    {
                        double totalAmount = 0;

                        switch (totalPair.Handicap)
                        {
                            case Handicap.OneHalf:
                                totalAmount = 1.5;
                                break;
                            case Handicap.Two:
                                totalAmount = 2;
                                break;
                            case Handicap.TwoHalf:
                                totalAmount = 2.5;
                                break;
                            case Handicap.Three:
                                totalAmount = 3;
                                break;
                            case Handicap.ThreeHalf:
                                totalAmount = 3.5;
                                break;
                            case Handicap.Four:
                                totalAmount = 4;
                                break;
                            case Handicap.FourHalf:
                                totalAmount = 4.5;
                                break;

                        }
                        if (indexMatchCoef == 3)
                        {
                            string totalMore = $"тб ({totalAmount})";
                            string totalLess = $"тм ({totalAmount})";
                            sheet.Cells[2, currentPairCoeff].Value = totalMore;
                            sheet.Cells[2, currentPairCoeff + 1].Value = totalLess;
                        }
                        sheet.Cells[indexMatchCoef, currentPairCoeff].Value = totalPair.TotalMore is null? "-": totalPair.TotalMore;
                        sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = totalPair.TotalLess is null ? "-" : totalPair.TotalLess;
                        currentPairCoeff += 2;
                    }
                    startCoefIndex += totalBookmaker.TotalCoeffs.Count * 2 + 1;
                    colorIndex++;
                }

                startCoefIndex += 1;
                sheet.Cells[2, startCoefIndex].Value = "Result";
                sheet.Cells[indexMatchCoef, startCoefIndex].Value = match.MatchUrl;
                
                indexMatchCoef++;

            }

            if (totalBetBrain is not null)
            {
                indexMatchCoef += 3;
                var rowNumForBook = indexMatchCoef - 2;

                foreach (var match in totalBetBrain)
                {
                    sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                    sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                    sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                    int startCoefIndex = 4;
                    int colorIndex = 0;
                    foreach (var totalBookmaker in match.BookmakerTotals.OrderBy(tb => tb.Bookmaker))
                    {
                        int cellAmount = totalBookmaker.TotalCoeffs.Count * 2;
                        sheet.Cells[rowNumForBook, startCoefIndex, rowNumForBook, startCoefIndex + cellAmount - 1].Value = totalBookmaker.Bookmaker.ToString();
                        sheet.Cells[rowNumForBook, startCoefIndex, rowNumForBook, startCoefIndex + cellAmount - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowNumForBook, startCoefIndex, rowNumForBook, startCoefIndex + cellAmount - 1].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);
                        int currentPairCoeff = startCoefIndex;
                        foreach (var totalPair in totalBookmaker.TotalCoeffs.OrderBy(tp => tp.Handicap))
                        {
                            double totalAmount = 0;

                            switch (totalPair.Handicap)
                            {
                                case Handicap.OneHalf:
                                    totalAmount = 1.5;
                                    break;
                                case Handicap.Two:
                                    totalAmount = 2;
                                    break;
                                case Handicap.TwoHalf:
                                    totalAmount = 2.5;
                                    break;
                                case Handicap.Three:
                                    totalAmount = 3;
                                    break;
                                case Handicap.ThreeHalf:
                                    totalAmount = 3.5;
                                    break;
                                case Handicap.Four:
                                    totalAmount = 4;
                                    break;
                                case Handicap.FourHalf:
                                    totalAmount = 4.5;
                                    break;

                            }
                            if (indexMatchCoef - 2 == rowNumForBook)
                            {
                                string totalMore = $"тб ({totalAmount})";
                                string totalLess = $"тм ({totalAmount})";
                                sheet.Cells[rowNumForBook + 1, currentPairCoeff].Value = totalMore;
                                sheet.Cells[rowNumForBook + 1, currentPairCoeff + 1].Value = totalLess;
                            }
                            sheet.Cells[indexMatchCoef, currentPairCoeff].Value = totalPair.TotalMore is null ? "-" : totalPair.TotalMore;
                            sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = totalPair.TotalLess is null ? "-" : totalPair.TotalLess;
                            currentPairCoeff += 2;
                        }
                        startCoefIndex += totalBookmaker.TotalCoeffs.Count * 2 + 1;
                        colorIndex++;
                    }
                    indexMatchCoef++;
                }
            }

            indexMatchCoef++;

            FormatStatisticsPart(sheet, indexMatchCoef, coefficeintsTotalAndStatistics.TeamStatistics);
        }

        public void FillHandicapSheet(ExcelPackage excelPackage, CoefficentsHandicapAndStatistics coefficentsHandicapAndStatistics, List<CoefficientsHandicap> handicapBetBrain)
        {
            var sheet = excelPackage.Workbook.Worksheets.Add("форы");
            sheet.Cells["A2"].Value = "date";
            sheet.Cells["B2"].Value = "home";
            sheet.Cells["C2"].Value = "guest";
            int indexMatchCoef = 3;
            foreach (var match in coefficentsHandicapAndStatistics.CoefficientsHandicaps)
            {
                sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                int startCoefIndex = 4;
                int colorIndex = 0;
                foreach (var coef in match.BookmakerHandicapCoeffs.OrderBy(c => c.Bookmaker))
                {
                    int cellAmount = coef.HandicapCoeffs.Count * 2;
                    int invertCellAmount = coef.HandicapCoeffsInvert.Count * 2;

                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Value = coef.Bookmaker.ToString();
                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, startCoefIndex, 1, startCoefIndex + cellAmount - 1].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);

                    sheet.Cells[1, startCoefIndex + cellAmount + 1, 1, startCoefIndex + cellAmount + invertCellAmount].Value = coef.Bookmaker.ToString();
                    sheet.Cells[1, startCoefIndex + cellAmount + 1, 1, startCoefIndex + cellAmount + invertCellAmount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, startCoefIndex + cellAmount + 1, 1, startCoefIndex + cellAmount + invertCellAmount].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);

                    int currentPairCoeff = startCoefIndex;
                    foreach (var coefPair in coef.HandicapCoeffs.OrderBy(h => h.Handicap))
                    {
                        double firstHandicap = 0, secondHandicap = 0;
           
                        switch (coefPair.Handicap)
                        {
                            case Handicap.Zero:
                                firstHandicap = 0;
                                secondHandicap = 0;
                                break;
                            case Handicap.ZeroHalf:
                                firstHandicap = 0.5;
                                secondHandicap = -0.5;
                                break;
                            case Handicap.One:
                                firstHandicap = 1;
                                secondHandicap = -1;
                                break;
                            case Handicap.OneHalf:
                                firstHandicap = 1.5;
                                secondHandicap = -1.5;
                                break;
                            case Handicap.Two:
                                firstHandicap = 2;
                                secondHandicap = -2;
                                break;
                            case Handicap.TwoHalf:
                                firstHandicap = 2.5;
                                secondHandicap = -2.5;
                                break;
                            case Handicap.Three:
                                firstHandicap = 3;
                                secondHandicap = -3;
                                break;
                            case Handicap.ThreeHalf:
                                firstHandicap = 3.5;
                                secondHandicap = -3.5;
                                break;
                            case Handicap.Four:
                                firstHandicap = 4;
                                secondHandicap = -4;
                                break;

                        }
                        if (indexMatchCoef == 3)
                        {
                            string homeTeam = $"ф1 ({firstHandicap})";
                            string awayTeam = $"ф2 ({secondHandicap})";
                            sheet.Cells[2, currentPairCoeff].Value = homeTeam;
                            sheet.Cells[2, currentPairCoeff + 1].Value = awayTeam;
                        }
                        sheet.Cells[indexMatchCoef, currentPairCoeff].Value = coefPair.FirstTeam is null? "-" : coefPair.FirstTeam;
                        sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = coefPair.SecondTeam is null ? "-" : coefPair.SecondTeam;
                        currentPairCoeff += 2;
                    }
                    currentPairCoeff++;

                    foreach (var coefPair in coef.HandicapCoeffsInvert.OrderBy(h => h.Handicap))
                    {
                        double firstHandicap = 0, secondHandicap = 0;

                        switch (coefPair.Handicap)
                        {
                            case Handicap.ZeroHalf:
                                firstHandicap = -0.5;
                                secondHandicap = 0.5;
                                break;
                            case Handicap.One:
                                firstHandicap = -1;
                                secondHandicap = 1;
                                break;
                            case Handicap.OneHalf:
                                firstHandicap = -1.5;
                                secondHandicap = 1.5;
                                break;
                            case Handicap.Two:
                                firstHandicap = -2;
                                secondHandicap = 2;
                                break;
                            case Handicap.TwoHalf:
                                firstHandicap = -2.5;
                                secondHandicap = 2.5;
                                break;
                            case Handicap.Three:
                                firstHandicap = -3;
                                secondHandicap = 3;
                                break;
                            case Handicap.ThreeHalf:
                                firstHandicap = -3.5;
                                secondHandicap = 3.5;
                                break;
                            case Handicap.Four:
                                firstHandicap = -4;
                                secondHandicap = 4;
                                break;

                        }
                        if (indexMatchCoef == 3)
                        {
                            string homeTeam = $"ф1 ({firstHandicap})";
                            string awayTeam = $"ф2 ({secondHandicap})";
                            sheet.Cells[2, currentPairCoeff].Value = homeTeam;
                            sheet.Cells[2, currentPairCoeff + 1].Value = awayTeam;
                        }
                        sheet.Cells[indexMatchCoef, currentPairCoeff].Value = coefPair.FirstTeam is null ? "-" : coefPair.FirstTeam;
                        sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = coefPair.SecondTeam is null ? "-" : coefPair.SecondTeam;
                        currentPairCoeff += 2;
                    }
                    startCoefIndex += coef.HandicapCoeffs.Count * 2 + coef.HandicapCoeffsInvert.Count * 2 + 2;
                    colorIndex++;
                }

                startCoefIndex += 1;
                sheet.Cells[2, startCoefIndex].Value = "Result";
                sheet.Cells[indexMatchCoef, startCoefIndex].Value = match.MatchUrl;

                indexMatchCoef++;
            }
            if (handicapBetBrain is not null)
            {
                indexMatchCoef += 3;
                var rowForBook = indexMatchCoef - 2;

                foreach (var match in handicapBetBrain)
                {
                    sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                    sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                    sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                    int startCoefIndex = 4;
                    int colorIndex = 0;
                    foreach (var coef in match.BookmakerHandicapCoeffs.OrderBy(c => c.Bookmaker))
                    {
                        int cellAmount = coef.HandicapCoeffs.Count * 2;
                        int invertCellAmount = coef.HandicapCoeffsInvert.Count * 2;

                        sheet.Cells[rowForBook, startCoefIndex, rowForBook, startCoefIndex + cellAmount - 1].Value = coef.Bookmaker.ToString();
                        sheet.Cells[rowForBook, startCoefIndex, rowForBook, startCoefIndex + cellAmount - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowForBook, startCoefIndex, rowForBook, startCoefIndex + cellAmount - 1].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);

                        sheet.Cells[rowForBook, startCoefIndex + cellAmount + 1, rowForBook, startCoefIndex + cellAmount + invertCellAmount].Value = coef.Bookmaker.ToString();
                        sheet.Cells[rowForBook, startCoefIndex + cellAmount + 1, rowForBook, startCoefIndex + cellAmount + invertCellAmount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowForBook, startCoefIndex + cellAmount + 1, rowForBook, startCoefIndex + cellAmount + invertCellAmount].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);

                        int currentPairCoeff = startCoefIndex;
                        foreach (var coefPair in coef.HandicapCoeffs.OrderBy(h => h.Handicap))
                        {
                            double firstHandicap = 0, secondHandicap = 0;

                            switch (coefPair.Handicap)
                            {
                                case Handicap.Zero:
                                    firstHandicap = 0;
                                    secondHandicap = 0;
                                    break;
                                case Handicap.ZeroHalf:
                                    firstHandicap = 0.5;
                                    secondHandicap = -0.5;
                                    break;
                                case Handicap.One:
                                    firstHandicap = 1;
                                    secondHandicap = -1;
                                    break;
                                case Handicap.OneHalf:
                                    firstHandicap = 1.5;
                                    secondHandicap = -1.5;
                                    break;
                                case Handicap.Two:
                                    firstHandicap = 2;
                                    secondHandicap = -2;
                                    break;
                                case Handicap.TwoHalf:
                                    firstHandicap = 2.5;
                                    secondHandicap = -2.5;
                                    break;
                                case Handicap.Three:
                                    firstHandicap = 3;
                                    secondHandicap = -3;
                                    break;
                                case Handicap.ThreeHalf:
                                    firstHandicap = 3.5;
                                    secondHandicap = -3.5;
                                    break;
                                case Handicap.Four:
                                    firstHandicap = 4;
                                    secondHandicap = -4;
                                    break;

                            }
                            if (indexMatchCoef - 2 == rowForBook)
                            {
                                string homeTeam = $"ф1 ({firstHandicap})";
                                string awayTeam = $"ф2 ({secondHandicap})";
                                sheet.Cells[rowForBook + 1, currentPairCoeff].Value = homeTeam;
                                sheet.Cells[rowForBook + 1, currentPairCoeff + 1].Value = awayTeam;
                            }
                            sheet.Cells[indexMatchCoef, currentPairCoeff].Value = coefPair.FirstTeam is null ? "-" : coefPair.FirstTeam;
                            sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = coefPair.SecondTeam is null ? "-" : coefPair.SecondTeam;
                            currentPairCoeff += 2;
                        }
                        currentPairCoeff++;

                        foreach (var coefPair in coef.HandicapCoeffsInvert.OrderBy(h => h.Handicap))
                        {
                            double firstHandicap = 0, secondHandicap = 0;

                            switch (coefPair.Handicap)
                            {
                                case Handicap.ZeroHalf:
                                    firstHandicap = -0.5;
                                    secondHandicap = 0.5;
                                    break;
                                case Handicap.One:
                                    firstHandicap = -1;
                                    secondHandicap = 1;
                                    break;
                                case Handicap.OneHalf:
                                    firstHandicap = -1.5;
                                    secondHandicap = 1.5;
                                    break;
                                case Handicap.Two:
                                    firstHandicap = -2;
                                    secondHandicap = 2;
                                    break;
                                case Handicap.TwoHalf:
                                    firstHandicap = -2.5;
                                    secondHandicap = 2.5;
                                    break;
                                case Handicap.Three:
                                    firstHandicap = -3;
                                    secondHandicap = 3;
                                    break;
                                case Handicap.ThreeHalf:
                                    firstHandicap = -3.5;
                                    secondHandicap = 3.5;
                                    break;
                                case Handicap.Four:
                                    firstHandicap = -4;
                                    secondHandicap = 4;
                                    break;

                            }
                            if (indexMatchCoef - 2 == rowForBook)
                            {
                                string homeTeam = $"ф1 ({firstHandicap})";
                                string awayTeam = $"ф2 ({secondHandicap})";
                                sheet.Cells[rowForBook + 1, currentPairCoeff].Value = homeTeam;
                                sheet.Cells[rowForBook + 1, currentPairCoeff + 1].Value = awayTeam;
                            }
                            sheet.Cells[indexMatchCoef, currentPairCoeff].Value = coefPair.FirstTeam is null ? "-" : coefPair.FirstTeam;
                            sheet.Cells[indexMatchCoef, currentPairCoeff + 1].Value = coefPair.SecondTeam is null ? "-" : coefPair.SecondTeam;
                            currentPairCoeff += 2;
                        }
                        startCoefIndex += coef.HandicapCoeffs.Count * 2 + coef.HandicapCoeffsInvert.Count * 2 + 2;
                        colorIndex++;
                    }

                    startCoefIndex += 1;
                    sheet.Cells[2, startCoefIndex].Value = "Result";
                    sheet.Cells[indexMatchCoef, startCoefIndex].Value = match.MatchUrl;

                    indexMatchCoef++;
                }
            }

            indexMatchCoef++;

            FormatStatisticsPart(sheet, indexMatchCoef, coefficentsHandicapAndStatistics.TeamStatistics);
        }

        public void FormatStatisticsPart(ExcelWorksheet sheet, int startIndex, IList<TeamStatistics> statistics)
        {
            sheet.Cells[startIndex, 3].Value = "игры дома";
            sheet.Cells[startIndex, 12 ].Value = "игры на выезде";
            startIndex++;

            sheet.Cells[startIndex, 2].Value = "список";
            sheet.Cells[startIndex, 3].Value = "количество игр дома";
            sheet.Cells[startIndex, 4].Value = "голы забитые";
            sheet.Cells[startIndex, 5].Value = "ударов в створ";
            sheet.Cells[startIndex, 7].Value = "голы пропущенные";
            sheet.Cells[startIndex, 8].Value = "пропущенные удары в створ";
            sheet.Cells[startIndex, 11].Value = "список";
            sheet.Cells[startIndex, 12].Value = "количество игр на выезде";
            sheet.Cells[startIndex, 13].Value = "голы забитые";
            sheet.Cells[startIndex, 14].Value = "ударов в створ";
            sheet.Cells[startIndex, 16].Value = "голы пропущенные";
            sheet.Cells[startIndex, 17].Value = "пропущенные удары в створ";
            sheet.Cells[startIndex, 2, startIndex, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startIndex, 2, startIndex, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            sheet.Cells[startIndex, 7, startIndex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startIndex, 7, startIndex, 8].Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            sheet.Cells[startIndex, 11, startIndex, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startIndex, 11, startIndex, 14].Style.Fill.BackgroundColor.SetColor(Color.Red);
            sheet.Cells[startIndex, 16, startIndex, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startIndex, 16, startIndex, 17].Style.Fill.BackgroundColor.SetColor(Color.Red);

            int statMatchStartIndex = startIndex + 1;
            int matchNum = 1;
            foreach (var teamStat in statistics)
            {
                sheet.Cells[statMatchStartIndex, 1].Value = matchNum;
                sheet.Cells[statMatchStartIndex, 2].Value = teamStat.Name;
                sheet.Cells[statMatchStartIndex, 3].Value = teamStat.MatchesAtHome;
                sheet.Cells[statMatchStartIndex, 4].Value = teamStat.GoalsScoredHome;
                sheet.Cells[statMatchStartIndex, 5].Value = teamStat.ShotsOnTargetHome;
                sheet.Cells[statMatchStartIndex, 7].Value = teamStat.MissedGoalsHome;
                sheet.Cells[statMatchStartIndex, 8].Value = teamStat.MissedOnTargetHome;
                sheet.Cells[statMatchStartIndex, 11].Value = teamStat.Name;
                sheet.Cells[statMatchStartIndex, 12].Value = teamStat.MatchesAway;
                sheet.Cells[statMatchStartIndex, 13].Value = teamStat.GoalsScoredAway;
                sheet.Cells[statMatchStartIndex, 14].Value = teamStat.ShotsOnTargetAway;
                sheet.Cells[statMatchStartIndex, 16].Value = teamStat.MissedGoalsAway;
                sheet.Cells[statMatchStartIndex, 17].Value = teamStat.MissedOnTargetAway;
                matchNum++;
                statMatchStartIndex++;
            }

            sheet.Cells[1, 1, 100, 200].AutoFitColumns();
            sheet.Cells[1, 1, 100, 200].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        public void FillWinDrawLoseSheet(ExcelPackage excelPackage, WinDrawLoseAndStatistics winDrawLoseAndStatistics, List<CoefficientsWinDrawLose> winDrawLoseBetBrain)
        {
            var sheet = excelPackage.Workbook.Worksheets.Add("1x2");
            sheet.Cells["A2"].Value = "date";
            sheet.Cells["B2"].Value = "home";
            sheet.Cells["C2"].Value = "guest";

            int indexMatchCoef = 3;
            foreach (var match in winDrawLoseAndStatistics.CoefficientsWinDrawLose)
            {
                sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                int startCoefIndex = 4;
                int coefPerBookIndex = 0;
                int colorIndex = 0;
                foreach (var coef in match.CoefficientsPerBookmakercs.OrderBy(c => c.Bookmaker))
                {
                    if (indexMatchCoef == 3)
                    {
                        sheet.Cells[2, startCoefIndex + coefPerBookIndex].Value = 1;
                        sheet.Cells[2, startCoefIndex + coefPerBookIndex + 1].Value = "X";
                        sheet.Cells[2, startCoefIndex + coefPerBookIndex + 2].Value = 2;
                    }
                    sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex].Value = coef.Win is null ? "-" : coef.Win;
                    sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex + 1].Value = coef.Draw is null ? "-" : coef.Draw;
                    sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex + 2].Value = coef.Lose is null ? "-" : coef.Lose;
                    sheet.Cells[1, startCoefIndex + coefPerBookIndex, 1, startCoefIndex + coefPerBookIndex + 2].Value = coef.Bookmaker.ToString();
                    sheet.Cells[1, startCoefIndex + coefPerBookIndex, 1, startCoefIndex + coefPerBookIndex + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, startCoefIndex + coefPerBookIndex, 1, startCoefIndex + coefPerBookIndex + 2].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);
                    coefPerBookIndex += 4;
                    colorIndex++;
                }

                startCoefIndex += 1;
                sheet.Cells[2, startCoefIndex + coefPerBookIndex -2 ].Value = "Result";
                sheet.Cells[indexMatchCoef, startCoefIndex+coefPerBookIndex - 2 ].Value = match.MatchUrl;

                indexMatchCoef += 1;
            }

            if (winDrawLoseBetBrain is not null)
            {

                indexMatchCoef += 3;
                int rowNumForBooks = indexMatchCoef - 2;

                foreach (var match in winDrawLoseBetBrain)
                {
                    sheet.Cells[indexMatchCoef, 1].Value = $"{match.Date.Day}.{match.Date.Month}.{match.Date.Year}";
                    sheet.Cells[indexMatchCoef, 2].Value = match.TeamNameHome;
                    sheet.Cells[indexMatchCoef, 3].Value = match.TeamNameGuest;
                    int startCoefIndex = 4;
                    int coefPerBookIndex = 0;
                    int colorIndex = 0;
                    foreach (var coef in match.CoefficientsPerBookmakercs.OrderBy(c => c.Bookmaker))
                    {
                        if (indexMatchCoef - 2 == rowNumForBooks)
                        {
                            sheet.Cells[rowNumForBooks + 1, startCoefIndex + coefPerBookIndex].Value = 1;
                            sheet.Cells[rowNumForBooks + 1, startCoefIndex + coefPerBookIndex + 1].Value = "X";
                            sheet.Cells[rowNumForBooks + 1, startCoefIndex + coefPerBookIndex + 2].Value = 2;
                        }
                        sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex].Value = coef.Win is null ? "-" : coef.Win;
                        sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex + 1].Value = coef.Draw is null ? "-" : coef.Draw;
                        sheet.Cells[indexMatchCoef, startCoefIndex + coefPerBookIndex + 2].Value = coef.Lose is null ? "-" : coef.Lose;
                        sheet.Cells[rowNumForBooks, startCoefIndex + coefPerBookIndex, rowNumForBooks, startCoefIndex + coefPerBookIndex + 2].Value = coef.Bookmaker.ToString();
                        sheet.Cells[rowNumForBooks, startCoefIndex + coefPerBookIndex, rowNumForBooks, startCoefIndex + coefPerBookIndex + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowNumForBooks, startCoefIndex + coefPerBookIndex, rowNumForBooks, startCoefIndex + coefPerBookIndex + 2].Style.Fill.BackgroundColor.SetColor(_colors[colorIndex]);
                        coefPerBookIndex += 4;
                        colorIndex++;
                    }
                    indexMatchCoef += 1;
                }
            }
            indexMatchCoef++;

            FormatStatisticsPart(sheet, indexMatchCoef, winDrawLoseAndStatistics.TeamStatistics);
        }

    }
}
