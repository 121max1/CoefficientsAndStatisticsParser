using Newtonsoft.Json;
using Parser.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parser
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Dictionary<string, string>> _statisticsUrls;
        private Dictionary<string, string> _coeffLeagueUrls;
        private Dictionary<string, string> _betBrainUrls;
        private ParserInstance _parser;
        private string _fileName;

        public Form1()
        {
            InitializeComponent();
            _parser = new ParserInstance();
            _statisticsUrls = new Dictionary<string, Dictionary<string, string>>();
            _coeffLeagueUrls = new Dictionary<string, string>();
            _betBrainUrls = new Dictionary<string, string>();
            string score24JsonPath = "LeaguesUrl.json";
            using (StreamReader reader = new StreamReader(score24JsonPath))
            {
                string json = reader.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var pair in values)
                {
                    _coeffLeagueUrls.Add(pair.Key, pair.Value);
                }
            }

            string betBrainUrlsJson = "LeaguesBetBrain.json";
            using (StreamReader reader = new StreamReader(betBrainUrlsJson))
            {
                string json = reader.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var pair in values)
                {
                    _betBrainUrls.Add(pair.Key, pair.Value);
                }
            }

            string nbBetJsonPath = "LeaguesStatistics.json";
            using (StreamReader reader = new StreamReader(nbBetJsonPath))
            {
                string json = reader.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                foreach (var pair in values)
                {
                    _statisticsUrls.Add(pair.Key, pair.Value);
                    comboBox1.Items.Add(pair.Key);
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var league = comboBox1.Text;
            var statisticsUrls = _statisticsUrls[league];
            var coeffLeagueUrl = _coeffLeagueUrls[league];
            var betBrainUrl = "";
            if (_betBrainUrls.ContainsKey(league))
            {
                betBrainUrl = _betBrainUrls[league];
            }

            try
            {
                var statistics = _parser.GetTeamStatistics(statisticsUrls, league, textBox1.Text);
                //var statistics = new List<TeamStatistics>();
                var coeffs = _parser.GetAllCoefficients(coeffLeagueUrl, betBrainUrl);

                var excelGenerator = new ExcelGenerator();

                byte[] excelDocument = excelGenerator.GenerateFile(
                    new WinDrawLoseAndStatistics()
                    {
                        CoefficientsWinDrawLose = coeffs.CoefficientsWinDrawLose,
                        TeamStatistics = statistics
                    },
                    new CoefficentsHandicapAndStatistics()
                    {
                        CoefficientsHandicaps = coeffs.CoefficientsHandicap,
                        TeamStatistics = statistics
                    },
                    new CoefficeintsTotalAndStatistics()
                    {
                        BookmakerTotals = coeffs.CoefficientsTotal,
                        TeamStatistics = statistics
                    },
                    coeffs.CoefficientsWinDrawLoseBetBrain,
                    coeffs.CoefficientsHandicapBetBrain,
                    coeffs.CoefficientsTotalBetBrain
                    );
                File.WriteAllBytes($"{league} {DateTime.Now.Date.Day}.{DateTime.Now.Date.Month}.{DateTime.Now.Date.Year}.xlsx", excelDocument);
                MessageBox.Show(
                    "Файл успешно создан!",
                    "Успешно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                    );
            }
            catch(Exception ex)
            {
                MessageBox.Show(
                    $"Произошла ошибка! Текст ошибки:{ex.Message} стек: {ex.StackTrace}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                    );
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                _fileName = openFileDialog1.FileName;
            }
            label3.Text = _fileName.Split("\\")[^1];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var excelGenerator = new ExcelGenerator();
            try
            {
                var hrefs = excelGenerator.GetMatchesHrefs(_fileName);
                var result = _parser.GetMatchResults(hrefs);
                var fileExcel = excelGenerator.FillScore(result, _fileName);
                File.Delete(_fileName);
                File.WriteAllBytes(_fileName, fileExcel);

                MessageBox.Show(
                    "Результаты для прошедших матчей успешно заполнены!",
                    "Успешно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly
                    );

            }
            catch
            {
                MessageBox.Show(
                   "Произошла ошибка!",
                   "Ошибка",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error,
                   MessageBoxDefaultButton.Button1,
                   MessageBoxOptions.DefaultDesktopOnly
                   );
            }
        }
    }
}
