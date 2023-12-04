using System.Configuration;
using System.Net.Mail;

namespace Reports.Mail
{
    public class Mail
    {
        SmtpClient smtp;
        MailMessage msg;
        string system_name = ConfigurationManager.AppSettings.Get("system-name");
        string mailId = ConfigurationManager.AppSettings.Get("mailId");
        string pwd = ConfigurationManager.AppSettings.Get("pwd");
        string host = ConfigurationManager.AppSettings.Get("host");
        string[] recipients = ConfigurationManager.AppSettings.Get("recipients").Split(',');
        string reportsPath;
        Boolean sendMail = bool.Parse(ConfigurationManager.AppSettings.Get("send-mail"));

        public Mail(string _reportsPath)
        {
            if (sendMail)
            {
                //Set reports path
                reportsPath = _reportsPath;
                //SmtpClient setup
                SetSmtpClient();
                //MailMessage setup
                SetMailMessage();
            }
        }

        void SetSmtpClient()
        {
            smtp = new SmtpClient();
            smtp.Timeout = int.MaxValue;
            smtp.Host = host;
            smtp.Port = 587;
            smtp.Credentials = new System.Net.NetworkCredential(mailId, pwd);
        }

        void SetMailMessage()
        {
            msg = new MailMessage();
            msg.Subject = "Automation testcases result";
            msg.Body = "Hi team, \r\nAutomation test suite run completed. Please view the attached report";
            msg.From = new MailAddress(mailId);
            Attachment attachment = new Attachment(reportsPath);
            msg.Attachments.Add(attachment);
            foreach (var r in recipients)
            {
                msg.To.Add(r.Trim());
            }
        }

        public void SendMail()
        {
            if (sendMail && system_name == Environment.MachineName)
            {
                smtp.Send(msg);
                smtp.Dispose();
            }

        }
    }
}