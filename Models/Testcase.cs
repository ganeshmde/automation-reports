namespace Reports.Models
{
    public class Testcase
    {
        public string Scenario { get; set; }

        public List<Step> Steps { get; set; }

        public string Status { get; set; }

        public double StartTime { get; set; }

        public double EndTime { get; set; }

        public string Error { get; set; }
    }
}
