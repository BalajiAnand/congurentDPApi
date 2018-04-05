using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace drportal.Helpers {
    public class MailHelper {
        private MailSettings MailSettings { get; set; }
        private string WebAppBaseUrl { get; set; }

        public MailHelper(MailSettings mailSettings, string webAppBaseUrl) {
            MailSettings = mailSettings;
            this.WebAppBaseUrl = webAppBaseUrl;
        }

        public void SendMail(string[] recipients, string title, string message) {
            // DEV: Mechansim that prevents mails from getting sent to unknown email ids
            using (var mailMessage = new MailMessage()) {
                foreach (var recipient in recipients) {
                    foreach (var whitelistedRecipient in MailSettings.WhitelistedRecipients) {
                        if (recipient.EndsWith(whitelistedRecipient)) {
                            mailMessage.To.Add(recipient);
                            break;
                        }
                    }
                }

                if (mailMessage.To.Count > 0) {
                    using(var mailClient = new SmtpClient(MailSettings.Host, MailSettings.Port)) {
                        mailMessage.From = new MailAddress(MailSettings.UserName);
                        mailMessage.Subject = title;
                        mailMessage.Body = "<span style='font-family:calibri,arial;'>" + message + 
                        "<hr /><span style='font-size: 0.75em;'>Sent by Dr.Portal automated mailing system. Please do not reply.</span></span>";
                        mailMessage.IsBodyHtml = true;
                        mailClient.Credentials = new NetworkCredential(MailSettings.UserName, MailSettings.Password);
                        mailClient.EnableSsl = true;
                        mailClient.Send(mailMessage);
                    }
                }
            }
        }

        public void SendForgotPasswordMail(string emailId, string name, string forgotHash) {
            SendMail(new string[] { emailId },
                "Dr.Portal: Change Password",
                $@"Hi {name},<br />
<br />
Please click <a href='{this.WebAppBaseUrl}/changepassword/{HttpUtility.UrlEncode(forgotHash)}'>this link</a> to change your password in Dr.Portal.<br />
<span style='font-size: 0.75em;'>NOTE: If you haven't requested for changing password, please ignore this mail.</span><br />");
        }
    }

    public class MailSettings {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string[] WhitelistedRecipients { get; set; }
    }
}