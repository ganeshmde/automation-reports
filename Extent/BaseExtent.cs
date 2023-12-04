using AventStack.ExtentReports;
using Reports.Extent.Helpers;
using Reports.Models;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace Reports.Extent
{
    public abstract class BaseExtent
    {
        protected ExtentReports extent;

        protected readonly List<TestFeature> features = new List<TestFeature>();

        string[] xmlFiles, jsonFiles;

        protected readonly string allureResultsDir;

        public string reportsDir, reportsPath;

        protected abstract void ImplementReports();

        protected abstract void CreateFeature();

        protected abstract void CreateScenario(ExtentTest extentTest, TestFeature feature);

        protected abstract void CreateStep(ExtentTest extentTest, TestScenario test);

        public abstract void GenerateReport();

        public BaseExtent()
        {
            extent = new ExtentReports();
            allureResultsDir = ConfigurationManager.AppSettings.Get("allure-results");
            CreateReportsDirectory();
            features = GetTestData();
        }

        void CreateReportsDirectory()
        {
            string codeBasePath = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            string directory = Path.GetDirectoryName(codeBasePath);
            string projectPath = new Uri(directory).LocalPath;
            reportsDir = projectPath + "\\reports\\";

            if (!Directory.Exists(reportsDir))
            {
                Directory.CreateDirectory(reportsDir);
            }
        }

        List<TestFeature> GetTestData()
        {
            string[] dirFiles = Directory.GetFiles(allureResultsDir);
            xmlFiles = dirFiles.Where(x => x.Contains("-testsuite.xml")).ToArray();
            jsonFiles = new DirectoryInfo(allureResultsDir).GetFiles()
                        .OrderBy(f => f.LastWriteTime)
                        .Where(f => f.Name.Contains("-container.json"))
                        .Select(f => f.FullName)
                        .ToArray();

            List<TestFeature> testData = new List<TestFeature>();
            if (xmlFiles.Length > 0)
            {
                new ExtractTestDataFromXml(xmlFiles, out testData);
            }
            else if (jsonFiles.Length > 0)
            {
                new ExtractTestDataFromJson(jsonFiles, allureResultsDir, out testData);
            }
            else
            {
                throw new Exception("No data (*.xml | *.json) in the allure results directory");
            }

            return testData;
        }

        protected void ChangeReportName()
        {
            DateTime dt = DateTime.Now;
            string time = $" ({dt.Hour}_{dt.Minute}_{dt.Second})";
            string date = $"{dt.Day}_{GetMonthName(dt)}_{dt.Year}";
            string reportName = $"index_{date + time}.html";
            reportsPath = reportsDir + reportName;
            File.Move($"{reportsDir + "index.html"}", reportsPath);
        }

        string GetMonthName(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        protected void ClearLine()
        {
            int cursorTop = Console.CursorTop == 0 ? 0 : Console.CursorTop - 1;
            Console.SetCursorPosition(0, cursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, cursorTop);
        }

        protected void OpenReport()
        {
            Boolean openReport = false;
            try
            {
                openReport = bool.Parse(ConfigurationManager.AppSettings.Get("open-report"));
            }
            catch (Exception) { }

            if (openReport)
            {
                var proc = new Process();
                proc.StartInfo = new ProcessStartInfo(reportsPath)
                {
                    UseShellExecute = true
                };
                proc.Start();
                Console.WriteLine("Report opened in the default browser\r\n");
            }
        }

        protected void StartProgress()
        {
            Console.Write("Generating extent report... ");
            using (var progress = new ProgressBar())
            {
                try
                {
                    int sum = features.Sum(x => x.Scenarios.Count);
                    for (int i = 0; i <= sum; i++)
                    {
                        progress.Report((double)i / sum);
                        Thread.Sleep(30);
                    }
                }
                catch (Exception)
                {
                    progress.Dispose();
                }
            }
        }
    }
}
