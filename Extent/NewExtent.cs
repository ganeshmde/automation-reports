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
        }

        public override void GenerateReport()
        {
            Thread th = new Thread(new ThreadStart(StartProgress));
            th.Start();
            ImplementReports();
            CreateFeature();
            extent.Flush();
            ChangeReportName();
            th.Interrupt();
            th.Join();
            Console.WriteLine($"\r\nReports generated in '{reportsPath}'\r\n");
            OpenReport();
        }

        void StartProgress()
        {
            Console.Write("Generating extent reports... ");
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
                    Console.WriteLine("Done.");
                }

            }
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
            extent.Report.StartTime = new DateTime(1970, 1, 1).AddMilliseconds(features[0].Scenarios[0].StartTime + 19800000);
            foreach (TestFeature feature in features)
            {
                ExtentTest feature_extent = extent.CreateTest<Feature>("Feature: " + feature.Name);
                CreateScenario(feature_extent, feature);
            }
        }

        protected override void CreateScenario(ExtentTest extentTest, TestFeature feature)
        {
            List<TestScenario> testcases = feature.Scenarios;
            foreach (var test in testcases)
            {
                ExtentTest scenario_extent = extentTest.CreateNode<Scenario>(test.Name);
                CreateStep(scenario_extent, test);
            }

        }

        protected override void CreateStep(ExtentTest extentTest, TestScenario test)
        {
            List<TestStep> steps = test.Steps;

            foreach (TestStep step in steps)
            {
                if (step.Type == "Given")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<Given>(step.Name);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<Given>(step.Name).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<Given>(step.Name).Skip("");
                    }
                }
                else if (step.Type == "When")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<When>(step.Name);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<When>(step.Name).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<When>(step.Name).Skip("");
                    }
                }
                else if (step.Type == "Then")
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<Then>(step.Name);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<Then>(step.Name).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<Then>(step.Name).Skip("");
                    }
                }
                else
                {
                    if (step.Status == "passed")
                    {
                        extentTest.CreateNode<And>(step.Name);
                    }
                    else if (step.Status == "failed" || step.Status == "broken")
                    {
                        extentTest.CreateNode<And>(step.Name).Fail(test.Error, GetScreenshot(step.ImageName));
                    }
                    else
                    {
                        extentTest.CreateNode<And>(step.Name).Skip("");
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
