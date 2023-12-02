using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using Reports.Models;

namespace Reports.Old
{
    /// <summary>
    /// Extent report version - 4.1.0
    /// </summary>
    class ExtentReportsOld
    {
        ExtentReports extent;
        ExtentHtmlReporter htmlReporter;
        List<TestFeature> features = new List<TestFeature>();
        string[] testSuiteFiles;
        string allureResultsDir = ConfigurationManager.AppSettings.Get("allure-results");
        Boolean openReport = bool.Parse(ConfigurationManager.AppSettings.Get("open-report"));
        string reportsDir, reportsPath;

        public ExtentReportsOld()
        {
            Console.WriteLine("Generating extent reports\r\n");
            GenearateReports();
            ChangeReportName();
            Console.WriteLine($"Reports generated in '{reportsPath}'\r\n");
            if (openReport)
            {
                OpenReport();
            }
        }

        /// <summary>
        /// Generate reports
        /// </summary>
        void GenearateReports()
        {
            ImplementReports();
            GetTestSuiteFiles();
            GetXmlData();
            CreateTestNodes();
            extent.Flush();
        }

        /// <summary>
        /// Attach htmlreporter and set the configuration
        /// </summary>
        void ImplementReports()
        {
            GetReportsPath();
            htmlReporter = new ExtentHtmlReporter(reportsDir + "index.html");
            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
            //Reports Configuration
            htmlReporter.Config.DocumentTitle = "Strategic Space Automation Report";
            htmlReporter.Config.ReportName = "Automation Reports";
            htmlReporter.Config.Theme = Theme.Standard;
            htmlReporter.Config.Encoding = "UTF-8";
        }

        /// <summary>
        /// Creates an html report file with the current date name
        /// </summary>
        /// <returns></returns>
        void GetReportsPath()
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

        void ChangeReportName()
        {
            DateTime dt = DateTime.Now;
            string time = $" ({dt.Hour}_{dt.Minute}_{dt.Second})";
            string date = $"{dt.Day}_{GetMonthName(dt)}_{dt.Year}";
            string reportName = $"index_{date + time}.html";
            reportsPath = reportsDir + reportName;
            File.Move($"{reportsDir + "index.html"}", reportsPath);
        }

        /// <summary>
        /// Returns the abbreviated month name for the given date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        string GetMonthName(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        /// <summary>
        /// Gets the file path of xml files
        /// </summary>
        void GetTestSuiteFiles()
        {
            try
            {
                string[] files = Directory.GetFiles(allureResultsDir);
                testSuiteFiles = files.Where(x => x.Contains("-testsuite.xml")).ToArray();
            }
            catch (Exception)
            {
                throw new Exception("set allure-results folder path in config file");
            }
        }

        /// <summary>
        /// Reads the xml and gets information about features, scenarios and steps
        /// </summary>
        void GetXmlData()
        {
            foreach (string file in testSuiteFiles)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);
                TestFeature feature = new TestFeature();
                feature.name = xmlDoc.SelectSingleNode("//name").InnerText;
                feature.tests = GetTestCases(xmlDoc);
                features.Add(feature);
            }
        }

        /// <summary>
        /// Gets the Scenario/testcase info
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        List<Testcase> GetTestCases(XmlNode node)
        {
            XmlNodeList testcaseNodes = node.SelectNodes("//test-case");
            List<Testcase> testcases = new List<Testcase>();

            foreach (XmlNode tc in testcaseNodes)
            {
                Testcase test = new Testcase();
                double startTime = double.Parse(tc.Attributes["start"].InnerText);
                double stopTime = double.Parse(tc.Attributes["stop"].InnerText);
                string status = tc.Attributes["status"].InnerText;
                string scenario = tc.FirstChild.InnerText;
                string error = null;

                if (status == "broken" || status == "failed")
                {
                    error = tc.LastChild.LastChild.FirstChild.InnerText;
                }
                test.Status = status;
                test.StartTime = startTime;
                test.EndTime = stopTime;
                test.Scenario = scenario;
                test.Steps = GetSteps(tc);
                test.Error = error;
                testcases.Add(test);
            }

            return testcases;
        }

        /// <summary>
        /// Gets the steps info corresponding to the scenario
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        List<Step> GetSteps(XmlNode node)
        {
            XmlNodeList stepNodes = node.Cast<XmlNode>()
                .Where(x => x.Name == "steps")
                .Select(x => x.ChildNodes)
                .FirstOrDefault();
            List<Step> steps = new List<Step>();

            foreach (XmlNode st in stepNodes)
            {
                Step step = new Step();
                long startTime = long.Parse(st.Attributes["start"].InnerText);
                long stopTime = long.Parse(st.Attributes["stop"].InnerText);
                string status = st.Attributes["status"].InnerText;
                string stepInfo = st.FirstChild.InnerText;
                string[] arr = stepInfo.Split(" ");
                string info = string.Join(" ", arr.Skip(1));

                XmlNode attachment = st.ChildNodes.Cast<XmlNode>().Where(x => x.Name == "attachments").FirstOrDefault().FirstChild;
                string imageName = null;

                if (attachment != null)
                {
                    imageName = attachment.Attributes["source"].InnerText;
                }

                step.StartTime = startTime;
                step.EndTime = stopTime;
                step.Type = arr[0];
                step.Status = status;
                step.Info = info;
                step.ImageName = imageName;
                steps.Add(step);
            }
            return steps;
        }

        /// <summary>
        /// Creates test nodes using the tests info that read from xml files
        /// </summary>
        void CreateTestNodes()
        {
            foreach (TestFeature feature in features)
            {
                ExtentTest feature_extent = extent.CreateTest<Feature>("Feature: " + feature.name);
                CreateScenario(feature_extent, feature);
            }
        }

        void CreateScenario(ExtentTest extentTest, TestFeature feature)
        {
            List<Testcase> testcases = feature.tests;
            foreach (var test in testcases)
            {
                ExtentTest scenario_extent = extentTest.CreateNode<Scenario>(test.Scenario);
                CreateStep(scenario_extent, test);
            }

        }

        void CreateStep(ExtentTest extentTest, Testcase test)
        {
            List<Step> steps = test.Steps;

            foreach (Step step in steps)
            {
                if (step.Type == "Given")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<Given>(step.Info);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<Given>(step.Info).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<Given>(step.Info).Skip("");
                    }
                }
                else if (step.Type == "When")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<When>(step.Info);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<When>(step.Info).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<When>(step.Info).Skip("");
                    }
                }
                else if (step.Type == "Then")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<Then>(step.Info);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<Then>(step.Info).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<Then>(step.Info).Skip("");
                    }
                }
                else
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<And>(step.Info);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<And>(step.Info).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<And>(step.Info).Skip("");
                    }
                }

            }
        }

        /// <summary>
        /// Returns failed scenario screenshot by its image name
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        MediaEntityModelProvider GetScreenshot(string imageName)
        {
            return MediaEntityBuilder.CreateScreenCaptureFromPath(allureResultsDir + "\\" + imageName).Build();
        }

        /// <summary>
        /// Opens the report in browser
        /// </summary>
        void OpenReport()
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
