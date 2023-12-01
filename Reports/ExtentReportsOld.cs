using System.Configuration;
using System.Globalization;
using System.Xml;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using Reports.Models;

namespace Reports.Old
{
    class ExtentReportsOld
    {
        ExtentReports extent;
        ExtentHtmlReporter htmlReporter;
        List<TestFeature> features = new List<TestFeature>();
        string[] testSuiteFiles;
        string allureResultsDir = ConfigurationManager.AppSettings.Get("allure-results");

        public ExtentReportsOld()
        {
            GenearateReports();
        }

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
            string reportsPath = GetReportsPath();
            htmlReporter = new ExtentHtmlReporter(reportsPath);
            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
            //Reports Configuration
            htmlReporter.Config.DocumentTitle = "Strategic Space Automation Report";
            htmlReporter.Config.ReportName = "Automation Reports";
            htmlReporter.Config.Theme = Theme.Standard;
            htmlReporter.Config.Encoding = "UTF-8";
        }

        /// <summary>
        /// Creates html report file with current date as name
        /// </summary>
        /// <returns></returns>
        string GetReportsPath()
        {
            string codeBasePath = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            string directory = Path.GetDirectoryName(codeBasePath);
            string projectPath = new Uri(directory).LocalPath;
            string reportsDir = projectPath + "\\reports";

            if (!Directory.Exists(reportsDir))
            {
                Directory.CreateDirectory(reportsDir);
            }

            DateTime dt = DateTime.Now;
            string date = dt.Day.ToString() + "_" + GetMonthName(dt) + "_" + dt.Year.ToString();
            string reportName = $"index_{date}.html";

            return reportsDir + reportName;
        }

        /// <summary>
        /// Returns Abbreviated Month name for the given date
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        string GetMonthName(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        /// <summary>
        /// Stores the paths of test suite xml files
        /// </summary>
        void GetTestSuiteFiles()
        {
            string[] files = Directory.GetFiles(allureResultsDir);
            testSuiteFiles = files.Where(x => x.Contains("-testsuite.xml")).ToArray();
        }

        /// <summary>
        /// Reads the xml and gets the features, scenarios and steps related info
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
                XmlNode attatchments = st.ChildNodes.Item(2);
                XmlNode attachment = attatchments.FirstChild;
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
    }
}
