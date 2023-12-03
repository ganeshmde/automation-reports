using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using Reports.Extent.Helpers;
using Reports.Models;

namespace Reports.Extent
{
    public class NewExtent : BaseExtent
    {
        ExtentSparkReporter sparkReporter;

        public NewExtent()
        {
            if (xmlFiles.Length > 0)
            {
                new ExtractXmlTestData(xmlFiles, out features);
            }
            else if (jsonFiles.Length > 0)
            {
                new ExtractJsonTestData(jsonFiles, allureResultsDir, out features);
            }
            else
            {
                throw new Exception("No data (*.xml | *.json) in the allure results directory");
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
            sparkReporter = new ExtentSparkReporter(reportsDir + "index.html");
            extent = new ExtentReports();
            extent.AttachReporter(sparkReporter);
            //Reports Configuration
            sparkReporter.Config.DocumentTitle = "Strategic Space Automation Report";
            sparkReporter.Config.ReportName = "Automation Reports";
            sparkReporter.Config.Theme = Theme.Standard;
            sparkReporter.Config.Encoding = "UTF-8";
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
        Media GetScreenshot(string imageName)
        {
            return MediaEntityBuilder.CreateScreenCaptureFromPath(allureResultsDir + "\\" + imageName).Build();
        }
    }
}
