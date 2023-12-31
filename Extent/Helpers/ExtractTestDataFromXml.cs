using Reports.Models;
using System.Xml;

namespace Reports.Extent.Helpers
{
    public class ExtractTestDataFromXml
    {
        readonly string[] xmlFiles;

        public ExtractTestDataFromXml(string[] _xmlFiles, out List<TestFeature> features)
        {
            xmlFiles = _xmlFiles;
            features = GetTestsDataFromXml();
        }

        public List<TestFeature> GetTestsDataFromXml()
        {
            List<TestFeature> features = new List<TestFeature>();
            foreach (string file in xmlFiles)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file);
                TestFeature feature = new TestFeature();
                feature.Name = xmlDoc.SelectSingleNode("//name").InnerText;
                feature.Scenarios = GetTestCases(xmlDoc);
                features.Add(feature);
            }
            return features;
        }

        /// <summary>
        /// Gets the Scenario/testcase info
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        List<TestScenario> GetTestCases(XmlNode node)
        {
            XmlNodeList testcaseNodes = node.SelectNodes("//test-case");
            List<TestScenario> testcases = new List<TestScenario>();

            foreach (XmlNode tc in testcaseNodes)
            {
                TestScenario test = new TestScenario();
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
                test.Name = scenario;
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
        List<TestStep> GetSteps(XmlNode node)
        {
            XmlNodeList stepNodes = node.Cast<XmlNode>()
                .Where(x => x.Name == "steps")
                .Select(x => x.ChildNodes)
                .FirstOrDefault();
            List<TestStep> steps = new List<TestStep>();

            foreach (XmlNode st in stepNodes)
            {
                TestStep step = new TestStep();
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
                step.Name = info;
                step.ImageName = imageName;
                steps.Add(step);
            }
            return steps;
        }
    }
}
