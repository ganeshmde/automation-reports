using AventStack.ExtentReports;
using Reports.Models;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace Reports.Extent
{
    public abstract class BaseExtent
    {
        protected ExtentReports extent;

        protected List<TestFeature> features = new List<TestFeature>();

        public readonly string[] xmlFiles, jsonFiles;

        protected readonly string allureResultsDir;

        protected string reportsDir, reportsPath;

        protected abstract void ImplementReports();

        protected abstract void CreateFeature();

        protected abstract void CreateScenario(ExtentTest extentTest, TestFeature feature);

        protected abstract void CreateStep(ExtentTest extentTest, Testcase test);

        public abstract void GenerateReport();

        public BaseExtent()
        {
            extent = new ExtentReports();
            allureResultsDir = ConfigurationManager.AppSettings.Get("allure-results");
            CreateReportsDirectory();
            string[] dirFiles = Directory.GetFiles(allureResultsDir);
            xmlFiles = dirFiles.Where(x => x.Contains("-testsuite.xml")).ToArray();
            jsonFiles = new DirectoryInfo(allureResultsDir).GetFiles()
                        .OrderBy(f => f.LastWriteTime)
                        .Where(f => f.Name.Contains("-container.json"))
                        .Select(f => f.FullName)
                        .ToArray();
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
                Console.WriteLine("Opening report\r\n");
                var proc = new Process();
                proc.StartInfo = new ProcessStartInfo(reportsPath)
                {
                    UseShellExecute = true
                };
                proc.Start();
                Console.WriteLine("Report opened in the default browser\r\n");
            }
        }
    }
}
