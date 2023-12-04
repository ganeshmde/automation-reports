// See https://aka.ms/new-console-template for more information
using Reports.Extent;

class Program
{
    private static void Main(string[] args)
    {
        //var extent = new OldExtent();
        var extent = new NewExtent();
        extent.GenerateReport();
    }
}
