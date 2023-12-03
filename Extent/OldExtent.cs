
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using Reports.Extent.Helpers;
using Reports.Models;

namespace Reports.Extent
{
    public class OldExtent : BaseExtent
    {
        ExtentHtmlReporter htmlReporter;

        public OldExtent()
        {
            if (xmlFiles.Length > 0)
            {
                features = new GetXmlData(xmlFiles).GetTestsDataFromXml();
            }
            else if (jsonFiles.Length > 0)
            {
                features = new GetJsonData(jsonFiles, allureResultsDir).GetTestsDataFromJson();
            }
        }

        public override void GenerateReport()
        {
            Console.WriteLine("Generating extent reports\r\n");
            ImplementReports();
            CreateFeature();
            extent.Flush();
            ChangeReportName();
            Console.WriteLine($"Reports generated in '{reportsPath}'\r\n");
            OpenReport();
        }

        protected override void ImplementReports()
        {
            htmlReporter = new ExtentHtmlReporter(reportsDir + "index.html");
            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
            //Reports Configuration
            htmlReporter.Config.DocumentTitle = "Strategic Space Automation Report";
            htmlReporter.Config.ReportName = "Automation Reports";
            htmlReporter.Config.Theme = Theme.Standard;
            htmlReporter.Config.Encoding = "UTF-8";
        }


        protected override void CreateFeature()
        {
            foreach (TestFeature feature in features)
            {
                ExtentTest feature_extent = extent.CreateTest<Feature>("Feature: " + feature.name);
                CreateScenario(feature_extent, feature);
            }
        }

        protected override void CreateScenario(ExtentTest extentTest, TestFeature feature)
        {
            List<Testcase> testcases = feature.tests;
            foreach (var test in testcases)
            {
                ExtentTest scenario_extent = extentTest.CreateNode<Scenario>(test.Scenario);
                CreateStep(scenario_extent, test);
            }

        }

        protected override void CreateStep(ExtentTest extentTest, Testcase test)
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
