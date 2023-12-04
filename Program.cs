// See https://aka.ms/new-console-template for more information
using Reports.Extent;
using Reports.Mail;

class Program
{
    private static void Main(string[] args)
    {
        //var extent = new OldExtent();
        var extent = new NewExtent();
        extent.GenerateReport();
        var mail = new Mail(extent.reportsPath);
        mail.SendMail();
    }
}
