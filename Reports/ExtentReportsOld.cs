using System.Configuration;
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
        public static ExtentReports extent;
        public static ExtentHtmlReporter htmlReporter;
        public static List<TestFeature> features = new List<TestFeature>();
        public static string[] testSuiteFiles;
        static string allureResultsDir = ConfigurationManager.AppSettings.Get("allure-results");

        public static void ImplementReports()
        {
            if (extent == null)
            {
                string reportsPath = GetReportsPath();
                htmlReporter = new ExtentHtmlReporter(reportsPath);
                extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);
                //Reports Configuration
                htmlReporter.Config.DocumentTitle = "Strategic Space Automation Report";
                htmlReporter.Config.ReportName = "Automation Test Report";
                htmlReporter.Config.Theme = Theme.Standard;
                htmlReporter.Config.Encoding = "UTF-8";
            }
        }
        static string GetReportsPath()
        {
            string codeBasePath = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            string directory = Path.GetDirectoryName(codeBasePath);
            string projectPath = new Uri(directory).LocalPath;
            string reportsDir = projectPath + "\\reports";

            if (Directory.Exists(reportsDir))
            {
                Directory.Delete(reportsDir, true);
            }

            Directory.CreateDirectory(reportsDir);

            return reportsDir + "\\index.html";
        }

        public static void GetTestSuiteFiles()
        {
            string[] files = Directory.GetFiles(allureResultsDir);
            testSuiteFiles = files.Where(x => x.Contains("-testsuite.xml")).ToArray();
        }

        public static void GetXmlData()
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

        static List<Testcase> GetTestCases(XmlNode node)
        {
            XmlNodeList testcaseNodes = node.SelectNodes("//test-case");
            List<Testcase> testcases = new List<Testcase>();

            foreach (XmlNode tc in testcaseNodes)
            {
                Testcase test = new Testcase();
                long startTime = long.Parse(tc.Attributes["start"].InnerText);
                long stopTime = long.Parse(tc.Attributes["stop"].InnerText);
                string status = tc.Attributes["status"].InnerText;
                string scenario = tc.FirstChild.InnerText;

                if (status == "broken" || status == "failed")
                {
                    string errorInfo = tc.LastChild.LastChild.FirstChild.InnerText;
                    test.Error = errorInfo;
                }


                test.Status = status;
                test.StartTime = startTime;
                test.EndTime = stopTime;
                test.Scenario = scenario;
                test.Steps = GetSteps(tc);

                testcases.Add(test);
            }

            return testcases;
        }

        static List<Step> GetSteps(XmlNode node)
        {
            XmlNodeList stepNodes = node.Cast<XmlNode>().Where(x => x.Name == "steps").Select(x => x.ChildNodes).FirstOrDefault();
            List<Step> steps = new List<Step>();

            foreach (XmlNode stepNode in stepNodes)
            {
                Step step = new Step();
                long startTime = long.Parse(stepNode.Attributes["start"].InnerText);
                long stopTime = long.Parse(stepNode.Attributes["stop"].InnerText);
                string status = stepNode.Attributes["status"].InnerText;
                string stepInfo = stepNode.FirstChild.InnerText;
                string[] arr = stepInfo.Split(" ");
                string info = string.Join(" ", arr.Skip(1));
                XmlNode attatchments = stepNode.ChildNodes.Item(2);
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

        public static void GenerateReport()
        {
            foreach (TestFeature feature in features)
            {
                ExtentTest feature_extent = extent.CreateTest<Feature>("Feature: " + feature.name);
                CreateScenario(feature_extent, feature);
            }
        }

        static void CreateScenario(ExtentTest extentTest, TestFeature feature)
        {
            List<Testcase> testcases = feature.tests;
            foreach (var test in testcases)
            {
                ExtentTest scenario_extent = extentTest.CreateNode<Scenario>(test.Scenario);
                CreateStep(scenario_extent, test);
            }

        }

        static void CreateStep(ExtentTest extentTest, Testcase test)
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
        static MediaEntityModelProvider GetScreenshot(string imageName)
        {
            return MediaEntityBuilder.CreateScreenCaptureFromPath(allureResultsDir + "\\" + imageName).Build();
        }

        public static void FlushReport()
        {
            extent.Flush();
        }
    }
}
