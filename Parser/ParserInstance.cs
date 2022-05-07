using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Parser.Entities;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Globalization;

namespace Parser
{
    public class ParserInstance
    {
        private IWebDriver _driver;

        public ParserInstance()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.PageLoadStrategy = PageLoadStrategy.None;
            _driver = new ChromeDriver();
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromMilliseconds(100000);
        }

        public List<TeamStatistics> GetTeamStatistics(Dictionary<string, string> urlsNbBet, string statLeague, string matchesAmount)
        {
            var statictics = new List<TeamStatistics>();

            foreach(var url in urlsNbBet)
            {
                try
                {
                    _driver.Navigate().GoToUrl(url.Value);
                    var selectLeague = new SelectElement(_driver.FindElement(By.Id("MainContent_ddlLeagues")));
                    selectLeague.SelectByText(statLeague);
                    var selectType = new SelectElement(_driver.FindElement(By.Id("MainContent_ddlState")));
                    selectType.SelectByValue("1"); // Home
                    _driver.FindElement(By.Name("ctl00$MainContent$txtNum")).Clear();
                    _driver.FindElement(By.Name("ctl00$MainContent$txtNum")).SendKeys(matchesAmount);
                    _driver.FindElement(By.XPath("//*[@id='MainContent_pnlRefresh']/input")).Click();
                    //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
                    Thread.Sleep(1000);
                    var stylePageLoading = _driver.FindElement(By.Id("PageUpdateProgress")).GetAttribute("style");
                    while(stylePageLoading == "display: block;")
                    {
                        Thread.Sleep(1000);
                        stylePageLoading = _driver.FindElement(By.Id("PageUpdateProgress")).GetAttribute("style");
                    };
                    var teamStatistics = new TeamStatistics();
                    teamStatistics.Name = url.Key;
                    int? winAmountTableRow = null;
                    int? drawAmountTableRow = null;
                    int? loseAmountTableRow = null;
                    var tableNamesRight = _driver.FindElements(By.XPath("//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr/td[1]")).Select(e => e.Text).ToList();
                    for (int i = 0; i < tableNamesRight.Count; i++)
                    {
                        if(tableNamesRight[i] == "Победы")
                        {
                            winAmountTableRow = i + 1;
                        }
                        if(tableNamesRight[i] == "Ничьи")
                        {
                            drawAmountTableRow = i + 1;
                        }
                        if(tableNamesRight[i] == "Поражения")
                        {
                            loseAmountTableRow = i + 1;
                        }
                    }
                    var winAmount = winAmountTableRow.HasValue? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{winAmountTableRow}]/td[3]")).Text) : 0;
                    var drawAmount = drawAmountTableRow.HasValue? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{drawAmountTableRow}]/td[3]")).Text) : 0;
                    var loseAmount = loseAmountTableRow.HasValue? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{loseAmountTableRow}]/td[3]")).Text) : 0;
                    teamStatistics.MatchesAtHome = winAmount + drawAmount + loseAmount;

                    int? missedGoalsHomeRow = null;
                    int? goalsScoredHomeRow = null;
                    int? shotsOnTargetHomeRow = null;
                    int? missedOnTargetRow = null;

                    var tableNamesLeft = _driver.FindElements(By.XPath("//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr/td[1]")).Select(e => e.Text).ToList();

                    for (int i = 0; i < tableNamesLeft.Count; i++)
                    {
                        if (tableNamesLeft[i] == "Пропущенные мячи")
                        {
                            missedGoalsHomeRow = i + 1;
                        }
                        if (tableNamesLeft[i] == "Забитые мячи")
                        {
                            goalsScoredHomeRow = i + 1;
                        }
                        if (tableNamesLeft[i] == "Удары в створ")
                        {
                            shotsOnTargetHomeRow = i + 1;
                        }
                        if (tableNamesLeft[i] == "Удары в створ (соперники)")
                        {
                            missedOnTargetRow = i + 1;
                        }
                    }

                    teamStatistics.MissedGoalsHome = missedGoalsHomeRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{missedGoalsHomeRow}]/td[3]")).Text) : null;
                    teamStatistics.GoalsScoredHome = goalsScoredHomeRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{goalsScoredHomeRow}]/td[3]")).Text) : null;
                    teamStatistics.ShotsOnTargetHome = shotsOnTargetHomeRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{shotsOnTargetHomeRow}]/td[3]")).Text) : null;
                    teamStatistics.MissedOnTargetHome = missedOnTargetRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{missedOnTargetRow}]/td[3]")).Text) : null;

                    var selectTypeAway = new SelectElement(_driver.FindElement(By.Id("MainContent_ddlState")));
                    selectTypeAway.SelectByValue("2"); //Away
                    _driver.FindElement(By.XPath("//*[@id='MainContent_pnlRefresh']/input")).Click();
                    Thread.Sleep(1000);
                    //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

                    stylePageLoading = _driver.FindElement(By.Id("PageUpdateProgress")).GetAttribute("style");
                    while (stylePageLoading == "display: block;")
                    {
                        Thread.Sleep(1000);
                        stylePageLoading = _driver.FindElement(By.Id("PageUpdateProgress")).GetAttribute("style");
                    };
                    new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(e => e.FindElement(By.XPath("//*[@id='MainContent_pnlRefresh']/input")));

                    int? winAmountAwayTableRow = null;
                    int? drawAmountAwayTableRow = null;
                    int? loseAmountAwayTableRow = null;
                    var tableNamesRightAway = _driver.FindElements(By.XPath("//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr/td[1]")).Select(e => e.Text).ToList();
                    for (int i = 0; i < tableNamesRightAway.Count; i++)
                    {
                        if (tableNamesRightAway[i] == "Победы")
                        {
                            winAmountAwayTableRow = i + 1;
                        }
                        if (tableNamesRightAway[i] == "Ничьи")
                        {
                            drawAmountAwayTableRow = i + 1;
                        }
                        if (tableNamesRightAway[i] == "Поражения")
                        {
                            loseAmountAwayTableRow = i + 1;
                        }
                    }

                    var winAmountAway = winAmountAwayTableRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{winAmountAwayTableRow}]/td[3]")).Text) : 0;
                    var drawAmountAway = drawAmountAwayTableRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{drawAmountAwayTableRow}]/td[3]")).Text) : 0;
                    var loseAmountAway = loseAmountAwayTableRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[3]/table/tbody/tr[{loseAmountAwayTableRow}]/td[3]")).Text) : 0;
                    teamStatistics.MatchesAway = winAmountAway + drawAmountAway + loseAmountAway;

                    var tableNamesLeftAway = _driver.FindElements(By.XPath("//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr/td[1]")).Select(e => e.Text).ToList();

                    int? missedGoalsAwayRow = null;
                    int? goalsScoredAwayRow = null;
                    int? shotsOnTargetAwayRow = null;
                    int? missedOnTargetAwayRow = null;

                    for (int i = 0; i < tableNamesLeftAway.Count; i++)
                    {
                        if (tableNamesLeftAway[i] == "Пропущенные мячи")
                        {
                            missedGoalsAwayRow = i + 1;
                        }
                        if (tableNamesLeftAway[i] == "Забитые мячи")
                        {
                            goalsScoredAwayRow = i + 1;
                        }
                        if (tableNamesLeftAway[i] == "Удары в створ")
                        {
                            shotsOnTargetAwayRow = i + 1;
                        }
                        if (tableNamesLeftAway[i] == "Удары в створ (соперники)")
                        {
                            missedOnTargetAwayRow = i + 1;
                        }
                    }

                    teamStatistics.MissedGoalsAway = missedGoalsAwayRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{missedGoalsAwayRow}]/td[3]")).Text) : null;
                    teamStatistics.GoalsScoredAway = goalsScoredAwayRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{goalsScoredAwayRow}]/td[3]")).Text) : null;
                    teamStatistics.ShotsOnTargetAway = shotsOnTargetAwayRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{shotsOnTargetAwayRow}]/td[3]")).Text) : null;
                    teamStatistics.MissedOnTargetAway = missedOnTargetAwayRow.HasValue ? int.Parse(_driver.FindElement(By.XPath($"//*[@id='MainContent_TabContainer_ctl00']/div[2]/div[2]/table/tbody/tr[{missedOnTargetAwayRow}]/td[3]")).Text) : null;

                    statictics.Add(teamStatistics);
                }
                catch
                {

                }

            }
            return statictics;
        }

        public AllCoefficients GetAllCoefficients(string urlBetonLeague, string urlBetBrain)
        {
            var winDrawLoseCoeffs = new List<CoefficientsWinDrawLose>();
            var handicapCoeffs = new List<CoefficientsHandicap>();
            var totalCoeffs = new List<CoefficientsTotal>();
            var winDrawLoseCoeffsBetBrain = new List<CoefficientsWinDrawLose>();
            var handicapCoeffsBetBrain = new List<CoefficientsHandicap>();
            var totalCoeffsBetBrain = new List<CoefficientsTotal>();

            _driver.Navigate().GoToUrl(urlBetonLeague);
            //List<string> text = _driver.FindElements(By.XPath("//*[@id='sizemetr']/div/div[1]/script")).Select(e => e.GetAttribute("textContent").Replace("\\\"","\"")).ToList();
            //List<BetonMatchJsonEntity> betonMatches = _driver.FindElements(By.XPath("//*[@id='sizemetr']/div/div[1]/script"))
            //                                                    .Select(e => JsonSerializer.Deserialize<BetonMatchJsonEntity>(e.Text)).ToList();
            var betonMatches = new List<Match>();
            Thread.Sleep(10000);

            var matchFirstNames = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'chm_event upcoming')]/div[2]/a[1]/div[1]/div[1]")).Select(e => e.Text).ToList();
            var matchSecondNames = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'chm_event upcoming')]/div[2]/a[1]/div[3]/div[2]")).Select(e => e.Text).ToList();
            var matchUrls = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'chm_event upcoming')]/div[2]/a[1]")).Select(e => e.GetAttribute("href")).ToList();

            for (int i = 0; i < matchFirstNames.Count; i++)
            {
                var match = new Match();
                match.FirstTeamName = matchFirstNames[i];
                match.SecondTeamName = matchSecondNames[i];
                match.Url = matchUrls[i];
                var urlParts = match.Url.Split("-");
                int year = int.Parse(urlParts[^3]);
                int month = int.Parse(urlParts[^2]);
                int day = int.Parse(urlParts[^1]);
                match.Date = new DateTime(year, month, day);
                betonMatches.Add(match);
            }

            foreach (var betonMatch in betonMatches)
            {
                try
                {
                    _driver.Navigate().GoToUrl($"{betonMatch.Url}#coefficients");
                }
                catch
                {
                    continue;
                }
                var tabs = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-menu-item')]/span"));
                var winDrawLoseTabNum = 0;
                var HandicapTabNum = 0;
                var TotalTabNum = 0;
                try
                {
                    while (tabs[winDrawLoseTabNum].Text != "1X2")
                    {
                        winDrawLoseTabNum++;
                    }
                }
                catch
                {
                }
                try
                {
                    while (tabs[HandicapTabNum].Text != "Фора")
                    {
                        HandicapTabNum++;
                    }
                }
                catch
                {
                }
                try
                {
                    while (tabs[TotalTabNum].Text != "Тотал")
                    {
                        TotalTabNum++;
                    }
                }
                catch
                {
                }
                winDrawLoseTabNum++;
                HandicapTabNum++;
                TotalTabNum++;
                if (winDrawLoseTabNum - 1 != 0)
                {
                    var cefficientsWinDrawLose = new CoefficientsWinDrawLose();
                    cefficientsWinDrawLose.TeamNameHome = betonMatch.FirstTeamName;
                    cefficientsWinDrawLose.TeamNameGuest = betonMatch.SecondTeamName;
                    cefficientsWinDrawLose.Date = betonMatch.Date.Date;
                    cefficientsWinDrawLose.MatchUrl = betonMatch.Url;
                    cefficientsWinDrawLose.CoefficientsPerBookmakercs = new List<CoefficientsPerBookmakercs>();

                    string getCoeffXpath = "//div[contains(concat(' ', normalize-space(@id), ' '), 'coefficients')]/div[3]/div[1]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')]/div";
                    var coeffsWinDrawLose = _driver.FindElements(By.XPath(getCoeffXpath)).Select(e => e.Text).ToList();
                    coeffsWinDrawLose.RemoveAll(s => s == "П1");
                    coeffsWinDrawLose.RemoveAll(s => s == "Х");
                    coeffsWinDrawLose.RemoveAll(s => s == "П2");
                    coeffsWinDrawLose.RemoveAt(0);
                    coeffsWinDrawLose.RemoveAt(6);
                    coeffsWinDrawLose.RemoveAt(12);
                    for (int i = 0; i < coeffsWinDrawLose.Count / 3; i++)
                    {
                        cefficientsWinDrawLose.CoefficientsPerBookmakercs.Add(new CoefficientsPerBookmakercs()
                        {
                            Bookmaker = (Bookmaker)(i + 1),
                            Win = coeffsWinDrawLose[i] == "-" ? null : coeffsWinDrawLose[i],
                            Draw = coeffsWinDrawLose[i + 6] == "-" ? null : coeffsWinDrawLose[i + 6],
                            Lose = coeffsWinDrawLose[i + 12] == "-" ? null : coeffsWinDrawLose[i + 12]
                        });

                    }
                    winDrawLoseCoeffs.Add(cefficientsWinDrawLose);
                }

                if (HandicapTabNum - 1 != 0)
                {
                    _driver.FindElement(By.XPath($"//*[@id='coefficients']/div[1]/div/div[{HandicapTabNum}]")).Click();
                    var coefficientsHandicap = new CoefficientsHandicap();
                    coefficientsHandicap.Date = betonMatch.Date;
                    coefficientsHandicap.TeamNameHome = betonMatch.FirstTeamName;
                    coefficientsHandicap.TeamNameGuest = betonMatch.SecondTeamName;
                    coefficientsHandicap.MatchUrl = betonMatch.Url;
                    coefficientsHandicap.BookmakerHandicapCoeffs = new List<BookmakerHandicap>();

                    var handicapValues = new Dictionary<string, List<string>>();
                    var rowsHandicapXpath = $"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{HandicapTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')]";
                    var rowsHandicap = _driver.FindElements(By.XPath(rowsHandicapXpath));
                    for (int i = 1; i < rowsHandicap.Count + 1; i++)
                    {
                        var handicap = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{HandicapTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')][{i}]/div[1]")).Text;
                        var handicapValuesPerRow =
                            _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{HandicapTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')][{i}]/div"))
                            .Select(e => e.Text).ToList();
                        handicapValuesPerRow.RemoveRange(0, 2);
                        handicapValues.Add(handicap, handicapValuesPerRow);
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        var bookmakerHandicap = new BookmakerHandicap();
                        bookmakerHandicap.Bookmaker = (Bookmaker)(i + 1);
                        bookmakerHandicap.HandicapCoeffs = new List<HandicapCoeff>();
                        bookmakerHandicap.HandicapCoeffsInvert = new List<HandicapCoeff>();
                        coefficientsHandicap.BookmakerHandicapCoeffs.Add(bookmakerHandicap);
                    }
                    for (double handicapAmount = 0; handicapAmount < 4.5; handicapAmount += 0.5)
                    {
                        string handicapAmountForNegative = handicapAmount == 0 ? "0" : $"{handicapAmount * (-1)}";
                        string handicapValueNegativeFirstTeam = $"Ф1 ({handicapAmountForNegative})";
                        string handicapValuePositiveFirstTeam = $"Ф1 ({handicapAmount})";
                        string handicapValueNegativeSecondTeam = $"Ф2 ({handicapAmountForNegative})";
                        string handicapValuePositiveSecondTeam = $"Ф2 ({handicapAmount})";
                        var handicapNegativeCoeffsFirstTeam = handicapValues.FirstOrDefault(p => p.Key == handicapValueNegativeFirstTeam).Value;
                        var handicapPositiveCoeffsSecondTeam = handicapValues.FirstOrDefault(p => p.Key == handicapValuePositiveSecondTeam).Value;
                        var handicapPositiveCoeffsFirstTeamInvert = handicapValues.FirstOrDefault(p => p.Key == handicapValuePositiveFirstTeam).Value;
                        var handicapNegativeCoeffsSecondTeamInvert = handicapValues.FirstOrDefault(p => p.Key == handicapValueNegativeSecondTeam).Value;

                        for (int i = 0; i < 6; i++)
                        {
                            var handicapCoeff = new HandicapCoeff();
                            handicapCoeff.Handicap = (Handicap)(int)(handicapAmount * 2 + 1);
                            if (handicapNegativeCoeffsFirstTeam is not null)
                                handicapCoeff.FirstTeam = handicapNegativeCoeffsFirstTeam[i] == "-" ? null : handicapNegativeCoeffsFirstTeam[i];
                            else
                                handicapCoeff.FirstTeam = null;
                            if (handicapPositiveCoeffsSecondTeam is not null)
                                handicapCoeff.SecondTeam = handicapPositiveCoeffsSecondTeam[i] == "-" ? null : handicapPositiveCoeffsSecondTeam[i];
                            else
                                handicapCoeff.SecondTeam = null;
                            coefficientsHandicap.BookmakerHandicapCoeffs.FirstOrDefault(bhc => bhc.Bookmaker == (Bookmaker)(i + 1)).HandicapCoeffsInvert.Add(handicapCoeff);

                            var handicapCoeffInvert = new HandicapCoeff();
                            handicapCoeffInvert.Handicap = (Handicap)(int)(handicapAmount * 2 + 1);
                            if (handicapPositiveCoeffsFirstTeamInvert is not null)
                                handicapCoeffInvert.FirstTeam = handicapPositiveCoeffsFirstTeamInvert[i] == "-" ? null : handicapPositiveCoeffsFirstTeamInvert[i];
                            else
                                handicapCoeffInvert.FirstTeam = null;
                            if (handicapNegativeCoeffsSecondTeamInvert is not null)
                                handicapCoeffInvert.SecondTeam = handicapNegativeCoeffsSecondTeamInvert[i] == "-" ? null : handicapNegativeCoeffsSecondTeamInvert[i];
                            else
                                handicapCoeffInvert.SecondTeam = null;
                            coefficientsHandicap.BookmakerHandicapCoeffs.FirstOrDefault(bhc => bhc.Bookmaker == (Bookmaker)(i + 1)).HandicapCoeffs.Add(handicapCoeffInvert);
                        }
                    }
                    handicapCoeffs.Add(coefficientsHandicap);
                }

                if (TotalTabNum - 1 != 0)
                {

                    _driver.FindElement(By.XPath($"//*[@id='coefficients']/div[1]/div/div[{TotalTabNum}]")).Click();
                    var coefficientsTotal = new CoefficientsTotal();
                    coefficientsTotal.Date = betonMatch.Date;
                    coefficientsTotal.TeamNameHome = betonMatch.FirstTeamName;
                    coefficientsTotal.TeamNameGuest = betonMatch.SecondTeamName;
                    coefficientsTotal.MatchUrl = betonMatch.Url;
                    coefficientsTotal.BookmakerTotals = new List<BookmakerTotal>();

                    var totalValues = new Dictionary<string, List<string>>();
                    string getTotalCoeffXpathRows = $"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{TotalTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')]";
                    var rowsTotal = _driver.FindElements(By.XPath(getTotalCoeffXpathRows));
                    for (int i = 1; i < rowsTotal.Count; i++)
                    {
                        var total = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{TotalTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')][{i}]/div[1]")).Text;
                        var totalValuesPerRow = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@id), ' '), 'tab_c{TotalTabNum}')]/div[1]/div[1]/div[contains(concat(' ', normalize-space(@class), ' '), 'odds-table-item')][{i}]/div"))
                            .Select(e => e.Text).ToList();
                        totalValuesPerRow.RemoveRange(0, 2);
                        totalValues.Add(total, totalValuesPerRow);
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        var bookmakerTotal = new BookmakerTotal();
                        bookmakerTotal.Bookmaker = (Bookmaker)(i + 1);
                        bookmakerTotal.TotalCoeffs = new List<TotalCoeff>();
                        coefficientsTotal.BookmakerTotals.Add(bookmakerTotal);
                    }
                    for (double totalAmount = 1.5; totalAmount < 5; totalAmount += 0.5)
                    {
                        string totalMoreValue = $"ТБ ({totalAmount})";
                        string totalLessValue = $"ТМ ({totalAmount})";
                        var totalMoreValueCoeffs = totalValues.FirstOrDefault(p => p.Key == totalMoreValue).Value;
                        var totalLessValueCoeffs = totalValues.FirstOrDefault(p => p.Key == totalLessValue).Value;

                        for (int i = 0; i < 6; i++)
                        {
                            var totalCoeff = new TotalCoeff();
                            totalCoeff.Handicap = (Handicap)(int)(totalAmount * 2 + 1);
                            if (totalMoreValueCoeffs is not null)
                                totalCoeff.TotalMore = totalMoreValueCoeffs[i] == "-" ? null : totalMoreValueCoeffs[i];
                            else
                                totalCoeff.TotalMore = null;
                            if (totalLessValueCoeffs is not null)
                                totalCoeff.TotalLess = totalLessValueCoeffs[i] == "-" ? null : totalLessValueCoeffs[i];
                            else
                                totalCoeff.TotalLess = null;
                            coefficientsTotal.BookmakerTotals.FirstOrDefault(bhc => bhc.Bookmaker == (Bookmaker)(i + 1)).TotalCoeffs.Add(totalCoeff);
                        }
                    }
                    totalCoeffs.Add(coefficientsTotal);
                }
            }

            try
            {
                try
                {
                    _driver.Navigate().GoToUrl(urlBetBrain);
                }
                catch
                {
                }
                int takeAmount = 10;
                try
                {
                    _driver.FindElement(By.XPath("//*[@id='onesignal-slidedown-cancel-button']")).Click();
                }
                catch
                {
                }
                try
                {
                    _driver.FindElement(By.XPath("/html/body/div[53]/div/div/button")).Click();
                }
                catch
                {

                }
                var betBrainMatchesHrefs = _driver.FindElements(By.XPath("//li[contains(concat(' ', normalize-space(@class), ' '), 'Match')]/div[1]/a[1]")).Take(takeAmount).Select(e => e.GetAttribute("href").Split("#")[0]).ToList();
                var betBrainMatchesTimeString = _driver.FindElements(By.XPath("//li[contains(concat(' ', normalize-space(@class), ' '), 'Match')]/span[1]/time[1]")).Take(takeAmount).Select(e => e.Text).ToList();
                var betBrainMatchesTime = new List<DateTime>();
                foreach (var strDate in betBrainMatchesTimeString)
                {
                    betBrainMatchesTime.Add(DateTime.ParseExact(strDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
                }
                var betBrainMatchesFirstTeamName = _driver.FindElements(By.XPath("//li[contains(concat(' ', normalize-space(@class), ' '), 'Match')]/div[1]/a[1]/span[1]/span[1]")).Take(takeAmount).Select(e => e.Text).ToList();
                var betBrainMatchesSecondTeamName = _driver.FindElements(By.XPath("//li[contains(concat(' ', normalize-space(@class), ' '), 'Match')]/div[1]/a[1]/span[1]/span[3]")).Take(takeAmount).Select(e => e.Text).ToList();

                var betBrainMatches = new List<Match>();
                for (int i = 0; i < takeAmount; i++)
                {
                    var match = new Match();
                    match.Date = betBrainMatchesTime[i];
                    match.FirstTeamName = betBrainMatchesFirstTeamName[i];
                    match.SecondTeamName = betBrainMatchesSecondTeamName[i];
                    match.Url = betBrainMatchesHrefs[i];
                    betBrainMatches.Add(match);
                }

                foreach (var match in betBrainMatches)
                {
                    try
                    {
                        _driver.Navigate().GoToUrl($"{match.Url}/#/home-draw-away/ordinary-time/");
                    }
                    catch
                    {

                    }
                    var winDrawLoseCoeffBetBrain = new CoefficientsWinDrawLose();
                    winDrawLoseCoeffBetBrain.Date = match.Date;
                    winDrawLoseCoeffBetBrain.TeamNameHome = match.FirstTeamName;
                    winDrawLoseCoeffBetBrain.TeamNameGuest = match.SecondTeamName;
                    winDrawLoseCoeffBetBrain.CoefficientsPerBookmakercs = new List<CoefficientsPerBookmakercs>();
                    var bookNames = _driver.FindElements(By.XPath("//ol[contains(concat(' ', normalize-space(@class), ' '), 'OTBookmakersContainer')][1]/li/div[1]/a[1]/span[1]")).Select(e => e.Text).ToList();
                    int pinnancleRow = bookNames.IndexOf("Pinnacle Sports") + 1;
                    int sboBetRow = bookNames.IndexOf("SBOBet") + 1;

                    if (pinnancleRow != 0)
                    {
                        var coeffsPin = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'OTOddsData')]/div[{pinnancleRow}]/ul[1]/li/a/span[1]/span[1]")).Select(e => e.Text).ToList();
                        winDrawLoseCoeffBetBrain.CoefficientsPerBookmakercs.Add(new CoefficientsPerBookmakercs()
                        {
                            Bookmaker = Bookmaker.Pinnacle,
                            Win = coeffsPin[0],
                            Draw = coeffsPin[1],
                            Lose = coeffsPin[2]
                        });
                    }
                    else
                    {
                        winDrawLoseCoeffBetBrain.CoefficientsPerBookmakercs.Add(new CoefficientsPerBookmakercs()
                        {
                            Bookmaker = Bookmaker.Pinnacle,
                            Win = null,
                            Draw = null,
                            Lose = null
                        });
                    }

                    if (sboBetRow != 0)
                    {
                        var coeffsSbo = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'OTOddsData')]/div[{sboBetRow}]/ul[1]/li/a/span[1]/span[1]")).Select(e => e.Text).ToList();
                        winDrawLoseCoeffBetBrain.CoefficientsPerBookmakercs.Add(new CoefficientsPerBookmakercs()
                        {
                            Bookmaker = Bookmaker.SboBet,
                            Win = coeffsSbo[0],
                            Draw = coeffsSbo[1],
                            Lose =coeffsSbo[2]
                        });
                    }
                    else
                    {
                        winDrawLoseCoeffBetBrain.CoefficientsPerBookmakercs.Add(new CoefficientsPerBookmakercs()
                        {
                            Bookmaker = Bookmaker.Pinnacle,
                            Win = null,
                            Draw = null,
                            Lose = null
                        });
                    }
                    winDrawLoseCoeffsBetBrain.Add(winDrawLoseCoeffBetBrain);


                    var coefficientsTotalBetBrain = new CoefficientsTotal()
                    {
                        Date = match.Date,
                        TeamNameGuest = match.SecondTeamName,
                        TeamNameHome = match.FirstTeamName,
                        BookmakerTotals = new List<BookmakerTotal>()
                    };

                    coefficientsTotalBetBrain.BookmakerTotals.Add(new BookmakerTotal()
                    {
                        Bookmaker = Bookmaker.Pinnacle,
                        TotalCoeffs = new List<TotalCoeff>()
                    });

                    coefficientsTotalBetBrain.BookmakerTotals.Add(new BookmakerTotal()
                    {
                        Bookmaker = Bookmaker.SboBet,
                        TotalCoeffs = new List<TotalCoeff>()
                    });

                    try
                    {
                        _driver.Navigate().GoToUrl("{match.Url}/#/over-under/ordinary-time/");
                    }
                    catch
                    {

                    }
                    var rowsUnderOverValue = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div/div[1]/span[1]")).Select(e => e.Text).ToList();

                    var handicapDict = new Dictionary<Handicap, List<string>>();

                    for (double i = 0.5; i <= 4.5; i += 0.5)
                    {
                        var handicapValue = (Handicap)(i * 2 + 1);
                        var rowNumber = rowsUnderOverValue.IndexOf($"{i}");
                        _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]")).Click();
                        var booksPerBet = new List<string>();
                        try
                        {
                            booksPerBet = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div/ul/li[1]/div[1]/a[1]/span[1]")).Select(e => e.Text).ToList();
                        }
                        catch
                        {
                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.Pinnacle)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = null,
                                    TotalLess = null
                                });

                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.SboBet)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = null,
                                    TotalLess = null
                                });
                            continue;
                        }
                        var PinSeq = booksPerBet.IndexOf("Pinnacle Sports") + 1;
                        var SboSeq = booksPerBet.IndexOf("SBOBet") + 1;

                        if (PinSeq != 0)
                        {
                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.Pinnacle)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    TotalLess = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[3]/a[1]/span[1]/span[1]")).Text
                                });
                        }
                        else
                        {
                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.Pinnacle)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = null,
                                    TotalLess = null
                                });
                        }

                        if (SboSeq != 0)
                        {
                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.SboBet)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    TotalLess = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumber + 2}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[3]/a[1]/span[1]/span[1]")).Text
                                });
                        }
                        else
                        {
                            coefficientsTotalBetBrain.BookmakerTotals
                                .FirstOrDefault(bt => bt.Bookmaker == Bookmaker.SboBet)
                                .TotalCoeffs.Add(new TotalCoeff()
                                {
                                    Handicap = handicapValue,
                                    TotalMore = null,
                                    TotalLess = null
                                });
                        }
                    }
                    totalCoeffsBetBrain.Add(coefficientsTotalBetBrain);


                    var coeffHandicapBetBrain = new CoefficientsHandicap();
                    coeffHandicapBetBrain.Date = match.Date;
                    coeffHandicapBetBrain.TeamNameGuest = match.SecondTeamName;
                    coeffHandicapBetBrain.TeamNameHome = match.FirstTeamName;
                    coeffHandicapBetBrain.BookmakerHandicapCoeffs = new List<BookmakerHandicap>();

                    coeffHandicapBetBrain.BookmakerHandicapCoeffs.Add(new BookmakerHandicap()
                    {
                        Bookmaker = Bookmaker.Pinnacle,
                        HandicapCoeffs = new List<HandicapCoeff>(),
                        HandicapCoeffsInvert = new List<HandicapCoeff>()
                    });

                    coeffHandicapBetBrain.BookmakerHandicapCoeffs.Add(new BookmakerHandicap()
                    {
                        Bookmaker = Bookmaker.SboBet,
                        HandicapCoeffs = new List<HandicapCoeff>(),
                        HandicapCoeffsInvert = new List<HandicapCoeff>()
                    });

                    try
                    {
                        _driver.Navigate().GoToUrl("{match.Url}#/asian-handicap/ordinary-time/");
                    }
                    catch
                    {

                    }
                    Thread.Sleep(200);
                    var rowsHandicapValues = _driver.FindElements(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div/div[1]/span[1]")).Select(e => e.Text).ToList();

                    for (double i = 0; i <= 4; i += 0.5)
                    {
                        var handicapValue = (Handicap)(i * 2 + 1);
                        var handicapAmoint = i == 0 ? "0" : $"{i}";
                        var handicapAmointNegative = i == 0 ? "0" : $"{i * (-1)}";
                        var rowNumberNegativeHandicap = rowsUnderOverValue.IndexOf(handicapAmointNegative) + 2;
                        var rowNumberPositiveHandicap = rowsUnderOverValue.IndexOf(handicapAmoint) + 2;
                        _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap + 2}]")).Click();
                        _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap + 2}]")).Click();

                        var booksPerBetPositiveHandicap = new List<string>();
                        try
                        {
                            booksPerBetPositiveHandicap = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div/ul/li[1]/div[1]/a[1]/span[1]")).Select(e => e.Text).ToList();
                        }
                        catch
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });

                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                            continue;
                        }

                        var PinSeq = booksPerBetPositiveHandicap.IndexOf("Pinnacle Sports") + 1;
                        var SboSeq = booksPerBetPositiveHandicap.IndexOf("SBOBet") + 1;

                        if (PinSeq != 0)
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    FirstTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    SecondTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    Handicap = handicapValue
                                });
                        }
                        else
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                        }

                        if (SboSeq != 0)
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    FirstTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    SecondTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberPositiveHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    Handicap = handicapValue
                                });
                        }
                        else
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffs.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                        }

                        var booksPerBetNegativeHandicap = new List<string>();
                        try
                        {
                            booksPerBetNegativeHandicap = _driver.FindElements(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div/ul/li[1]/div[1]/a[1]/span[1]")).Select(e => e.Text).ToList();
                        }
                        catch
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });

                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                            continue;
                        }

                        PinSeq = booksPerBetPositiveHandicap.IndexOf("Pinnacle Sports") + 1;
                        SboSeq = booksPerBetPositiveHandicap.IndexOf("SBOBet") + 1;

                        if (PinSeq != 0)
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    FirstTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    SecondTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{PinSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    Handicap = handicapValue
                                });
                        }
                        else
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.Pinnacle)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                        }

                        if (SboSeq != 0)
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    FirstTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    SecondTeam = _driver.FindElement(By.XPath($"//div[contains(concat(' ', normalize-space(@class), ' '), 'DynamicPage DynamicTwoCols')]/div[{rowNumberNegativeHandicap}]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[{SboSeq}]/ul/li[2]/a[1]/span[1]/span[1]")).Text,
                                    Handicap = handicapValue
                                });
                        }
                        else
                        {
                            coeffHandicapBetBrain.BookmakerHandicapCoeffs
                                .FirstOrDefault(bh => bh.Bookmaker == Bookmaker.SboBet)
                                .HandicapCoeffsInvert.Add(new HandicapCoeff()
                                {
                                    Handicap = handicapValue,
                                    FirstTeam = null,
                                    SecondTeam = null
                                });
                        }
                    }
                    handicapCoeffsBetBrain.Add(coeffHandicapBetBrain);
                }
            }
            catch
            {
                winDrawLoseCoeffsBetBrain = null;
                handicapCoeffsBetBrain = null;
                totalCoeffsBetBrain = null;
            }
            
            return new AllCoefficients()
            {
                CoefficientsHandicap = handicapCoeffs,
                CoefficientsTotal = totalCoeffs,
                CoefficientsWinDrawLose = winDrawLoseCoeffs,
                CoefficientsWinDrawLoseBetBrain = winDrawLoseCoeffsBetBrain,
                CoefficientsHandicapBetBrain = handicapCoeffsBetBrain,
                CoefficientsTotalBetBrain = totalCoeffsBetBrain
            };
        }

        public List<TeamResult> GetMatchResults(List<string> hrefs)
        {
            var result = new List<TeamResult>();
            int i = 1;
            foreach(var href in hrefs)
            {
                _driver.Navigate().GoToUrl(href);
                string resultScore = "";
                try
                {
                    resultScore = _driver.FindElement(By.XPath("//div[contains(concat(' ', normalize-space(@class), ' '), 'info-center')]/div[1]/div[2]/div[1]/span[1]")).Text;
                }
                catch
                {
                    result.Add(new TeamResult() { FirstTeam = null, SecondTeam = null });
                }
                if (resultScore == "—")
                {
                    result.Add(new TeamResult() { FirstTeam = null, SecondTeam = null });
                }
                else
                {
                    var resultArray = resultScore.Split(":").Select(t => t.Replace(" ", ""));
                    result.Add(new TeamResult()
                    {
                        FirstTeam = resultScore.FirstOrDefault().ToString(),
                        SecondTeam = resultScore.LastOrDefault().ToString()
                    });
                }
            }
            return result;
        }
    }
}
