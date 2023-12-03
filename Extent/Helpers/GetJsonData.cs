using Newtonsoft.Json;
using Reports.Models;

namespace Reports.Extent.Helpers
{
    public class GetJsonData
    {
        readonly string[] jsonFiles;
        readonly string allureResultsDir;

        public GetJsonData(string[] _jsonFiles, string _allureResultsDir)
        {
            jsonFiles = _jsonFiles;
            allureResultsDir = _allureResultsDir;
        }

        public List<TestFeature> GetTestsDataFromJson()
        {
            List<TestFeature> features = new List<TestFeature>();
            foreach (var file in jsonFiles)
            {
                TestFeature feature = new TestFeature();
                var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(file));
                string[] scenarios = data["children"].ToObject<string[]>();
                string featureName = data["name"];
                if (features.Where(f => f.name == featureName).Count() > 0)
                {
                    featureName = "(Rerun Results: )" + featureName;
                }
                feature.name = featureName;
                feature.tests = GetScenariosData(scenarios);
                features.Add(feature);
            }
            return features;
        }

        List<Testcase> GetScenariosData(string[] scenarios)
        {
            List<Testcase> tests = new List<Testcase>();
            foreach (string scenario in scenarios)
            {
                Testcase test = new Testcase();
                string resultsPath = allureResultsDir + "\\" + scenario + "-result.json";
                var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(resultsPath));
                var stepsData = GetStepsData(data["steps"].ToObject<Object[]>());
                test.Steps = stepsData.Item1;
                test.Error = stepsData.Item2;
                test.Scenario = data["name"];
                test.StartTime = data["start"];
                test.EndTime = data["stop"];
                test.Status = data["status"];
                tests.Add(test);
            }
            return tests;
        }

        Tuple<List<Step>, string> GetStepsData(Object[] stepsData)
        {
            List<Step> steps = new List<Step>();
            string testError = "";
            foreach (var st in stepsData)
            {
                Step step = new Step();
                var json = JsonConvert.SerializeObject(st);
                var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                step.Status = data["status"];
                step.Info = data["name"];
                step.StartTime = data["start"];
                step.EndTime = data["stop"];

                var attachments = data["attachments"].ToObject<Object[]>();
                if (attachments.Length > 0)
                {
                    var attachmentJson = JsonConvert.SerializeObject(attachments[0]);
                    var attachmentData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(attachmentJson);
                    step.ImageName = attachmentData["source"];
                }

                var statusDetails = data["statusDetails"];
                var statusDetailsJson = JsonConvert.SerializeObject(statusDetails);
                var statusDetailsData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(statusDetailsJson);
                if (statusDetailsData.Count > 0)
                {
                    testError = statusDetailsData["trace"];
                }

                steps.Add(step);
            }
            return new Tuple<List<Step>, string>(steps, testError);
        }

    }
}
