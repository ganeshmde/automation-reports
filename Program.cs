// See https://aka.ms/new-console-template for more information
using Reports.Old;
class Program
{
    private static void Main(string[] args)
    {
        ExtentReportsOld.ImplementReports();
        ExtentReportsOld.GetTestSuiteFiles();
        ExtentReportsOld.GetXmlData();
        ExtentReportsOld.GenerateReport();
        ExtentReportsOld.FlushReport();
    }
}
