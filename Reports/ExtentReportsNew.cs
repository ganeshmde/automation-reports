//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using AventStack.ExtentReports;
//using AventStack.ExtentReports.Gherkin.Model;
//using AventStack.ExtentReports.Reporter;
//using AventStack.ExtentReports.Reporter.Config;
//using AventStack.ExtentReports.Model;
//using Microsoft.VisualBasic;
//using Reports.Models;

//namespace HTMLReports
//{
//    /// <summary>
//    /// Extent Reports Version - 5.0.1
//    /// </summary>
//    class ExtentReportsNew
//    {
//        public static ExtentReports extent;
//        public static ExtentSparkReporter sparkReporter;
//        //public static ExtentTest extentTest;
//        public static List<TestFeature> features = new List<TestFeature>();
//        public static string[] testSuiteFiles;

//        public static void ImplementReports()
//        {
//            if (extent == null)
//            {
//                string reportsPath = GetReportsPath();
//                sparkReporter = new ExtentSparkReporter(reportsPath);
//                extent = new ExtentReports();
//                extent.AttachReporter(sparkReporter);

//                sparkReporter.Config.OfflineMode = true;
//                sparkReporter.Config.DocumentTitle = "Strategic Space Automation Report";
//                sparkReporter.Config.ReportName = "Automation Test Report";
//                sparkReporter.Config.Theme = Theme.Standard;
//                sparkReporter.Config.TimelineEnabled = true;
//                sparkReporter.Config.Encoding = "UTF-8";
//            }

//            //return extent;
//        }
//        static string GetReportsPath()
//        {
//            string codeBasePath = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
//            string directory = Path.GetDirectoryName(codeBasePath);
//            string projectPath = new Uri(directory).LocalPath;
//            string reportsDir = projectPath + "\\reports";

//            if (Directory.Exists(reportsDir))
//            {
//                Directory.Delete(reportsDir, true);
//            }

//            Directory.CreateDirectory(reportsDir);

//            return reportsDir + "\\index.html";
//        }

//        public static void GetTestSuiteFiles()
//        {
//            string directory = "C:\\Users\\1034557\\source\\sts\\retail-catman-test-automation\\STSWebAutomation\\allure-results";
//            string[] files = Directory.GetFiles(directory);
//            testSuiteFiles = files.Where(x => x.Contains("-testsuite.xml")).ToArray();
//        }

//        public static void GetXmlData()
//        {
//            foreach (string file in testSuiteFiles)
//            {
//                XmlDocument xmlDoc = new XmlDocument();
//                xmlDoc.Load(file);

//                TestFeature feature = new TestFeature();
//                feature.name = xmlDoc.SelectSingleNode("//name").InnerText;
//                feature.tests = GetTestCases(xmlDoc);
//                features.Add(feature);
//            }
//        }

//        static List<Testcase> GetTestCases(XmlNode node)
//        {
//            XmlNodeList testcaseNodes = node.SelectNodes("//test-case");
//            List<Testcase> testcases = new List<Testcase>();

//            foreach (XmlNode tc in testcaseNodes)
//            {
//                Testcase test = new Testcase();
//                long startTime = Int64.Parse(tc.Attributes["start"].InnerText);
//                long stopTime = Int64.Parse(tc.Attributes["stop"].InnerText);
//                string status = tc.Attributes["status"].InnerText;
//                string scenario = tc.FirstChild.InnerText;
//                if (status == "passed" || status == "failed")
//                {
//                    string errorMsg = tc.SelectSingleNode("//failure//message").InnerText;
//                    string errorInfo = tc.SelectSingleNode("//failure//stack-trace").InnerText;
//                    test.error = errorMsg + "\r\n" + errorInfo;
//                }


//                test.status = status;
//                test.duration = stopTime - startTime;
//                test.scenario = scenario;
//                test.steps = GetSteps(tc);

//                testcases.Add(test);
//            }

//            return testcases;
//        }

//        static List<Step> GetSteps(XmlNode node)
//        {
//            XmlNodeList stepNodes = node.SelectNodes("//step");
//            List<Step> steps = new List<Step>();

//            foreach (XmlNode stepNode in stepNodes)
//            {
//                Step step = new Step();
//                long startTime = Int64.Parse(stepNode.Attributes["start"].InnerText);
//                long stopTime = Int64.Parse(stepNode.Attributes["stop"].InnerText);
//                string status = stepNode.Attributes["status"].InnerText;
//                string stepInfo = stepNode.FirstChild.InnerText;
//                string[] arr = stepInfo.Split(" ");
//                string info = String.Join(" ", arr.Skip(1));
//                XmlNode attatchments = stepNode.ChildNodes.Item(2);
//                XmlNode attachment = attatchments.FirstChild;
//                string imageName = null;

//                if (attachment != null)
//                {
//                    imageName = attachment.Attributes["source"].InnerText;
//                }


//                step.duration = stopTime - startTime;
//                step.type = arr[0];
//                step.status = status;
//                step.info = info;
//                step.imageName = imageName;

//                steps.Add(step);
//            }
//            return steps;
//        }

//        public static void GenerateReport()
//        {
//            foreach (TestFeature feature in features)
//            {
//                ExtentTest feature_extent = extent.CreateTest<Feature>("Feature: " + feature.name);
//                CreateScenario(feature_extent, feature);
//            }
//        }

//        static void CreateScenario(ExtentTest extentTest, TestFeature feature)
//        {
//            List<Testcase> testcases = feature.tests;
//            foreach (var test in testcases)
//            {
//                ExtentTest scenario_extent = extentTest.CreateNode<Scenario>(test.scenario);
//                CreateStep(scenario_extent, test);
//            }

//        }

//        static void CreateStep(ExtentTest extentTest, Testcase test)
//        {
//            List<Step> steps = test.steps;

//            foreach (Step step in steps)
//            {
//                if (step.type == "Given")
//                {
//                    if (step.status == "passed")
//                    {
//                        extentTest.CreateNode<Given>(step.info).Pass();
//                    }
//                    else if (step.status == "failed" || step.status == "broken")
//                    {
//                        extentTest.CreateNode<Given>(step.info).Fail(test.error, GetScreenshot(step.imageName));
//                    }
//                    else
//                    {
//                        extentTest.CreateNode<Given>(step.info).Skip();
//                    }
//                }
//                else if (step.type == "When")
//                {
//                    if (step.status == "passed")
//                    {
//                        extentTest.CreateNode<When>(step.info).Pass();
//                    }
//                    else if (step.status == "failed" || step.status == "broken")
//                    {
//                        extentTest.CreateNode<When>(step.info).Fail(test.error, GetScreenshot(step.imageName));
//                    }
//                    else
//                    {
//                        extentTest.CreateNode<When>(step.info).Skip();
//                    }
//                }
//                else if (step.type == "Then")
//                {
//                    if (step.status == "passed")
//                    {
//                        extentTest.CreateNode<Then>(step.info).Pass();
//                    }
//                    else if (step.status == "failed" || step.status == "broken")
//                    {
//                        extentTest.CreateNode<Then>(step.info).Fail(test.error, GetScreenshot(step.imageName));
//                    }
//                    else
//                    {
//                        extentTest.CreateNode<Then>(step.info).Skip();
//                    }
//                }
//                else
//                {
//                    if (step.status == "passed")
//                    {
//                        extentTest.CreateNode<And>(step.info).Pass();
//                    }
//                    else if (step.status == "failed" || step.status == "broken")
//                    {
//                        extentTest.CreateNode<And>(step.info).Fail(test.error, GetScreenshot(step.imageName));
//                    }
//                    else
//                    {
//                        extentTest.CreateNode<And>(step.info).Skip();
//                    }
//                }

//            }
//        }

//        static Media GetScreenshot(string imageName)
//        {
//            string directory = "C:\\Users\\1034557\\source\\sts\\retail-catman-test-automation\\STSWebAutomation\\allure-results";
//            return MediaEntityBuilder.CreateScreenCaptureFromPath(directory + "\\" + imageName).Build();
//        }

//        public static void FlushReport()
//        {
//            extent.Flush();
//        }
//    }
//}
