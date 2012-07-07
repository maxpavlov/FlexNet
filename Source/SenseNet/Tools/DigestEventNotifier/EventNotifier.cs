using System;
using System.Net.Mail;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Portlets.ContentHandlers;
using SenseNet.ContentRepository;
using MailMessage = System.Net.Mail.MailMessage;
namespace SenseNet.Portal.ContentExplorer
{

    /// <summary>
    /// Notifier class for sending event notifications
    /// </summary>
    public static class EventNotifier
    {
        private const string MailHostAppsettingKey = "SMTP";

        /// <summary>
        /// Sends the digest.
        /// </summary>
        /// <param name="calPath">The calendar path.</param>
        public static void SendDigest(string calPath)
        {
            const string line = "\n\r\n____________________________________________________\n\r\n";
            var query = new NodeQuery(ChainOperator.And);
            //var calPath = TextBox1.Text;

            var cal = Node.LoadNode(calPath);

            foreach (var item in cal.PhysicalChildArray)
            {
                var nMode = item.GetPropertySafely("NotificationMode") as string;

                if (!string.IsNullOrEmpty(nMode) && nMode == "E-mail digest")
                {

                    var regForm = item.GetReference<Form>("RegistrationForm");

                    if (regForm != null)
                    {

                        string emailText = "";
                        emailText = string.Concat(emailText, "Event registrations digest for ", item.Name);
                        int regCount = 0;

                        foreach (var reg in regForm.Children)
                        {
                            //ha tegnap volt
                            var span = DateTime.Now - reg.CreationDate;
                            if (span < TimeSpan.FromHours(48) && DateTime.Now.Day == reg.CreationDate.Day)
                            {
                                var c = ContentRepository.Content.Create(reg);
                                emailText = string.Concat(emailText, line);
                                if (reg.HasProperty("Email"))
                                {
                                    emailText = string.Concat(emailText, "Registered e-mail: ", reg["Email"].ToString());
                                }
                                bool first = true;
                                foreach (var kvp in c.Fields)
                                {
                                    Field f = kvp.Value;

                                    if (!f.Name.StartsWith("#") || f.Name == "Email")
                                        continue;

                                    if (f.HasValue())
                                    {
                                        emailText = string.Concat(emailText, "\n\r\n");
                                        emailText = string.Concat(emailText, f.DisplayName, ": ");
                                        foreach (string b in f.FieldSetting.Bindings)
                                            emailText = string.Concat(emailText, Convert.ToString(reg[b]));
                                    }

                                }
                                regCount += 1;
                            }
                        }
                        emailText = string.Concat(emailText, line);
                        emailText = string.Concat(emailText, "New registrations: ", regCount.ToString());

                        var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
                        var sc = new SmtpClient(emailHost);
                        var from = regForm.EmailFrom;
                        if (string.IsNullOrEmpty(from))
                            from = Repository.EmailSenderAddress;

                        var emailList = String.Empty;
                        if (!String.IsNullOrEmpty(regForm.EmailList))
                        {
                            emailList = regForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                                            .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                        }
                        if (!string.IsNullOrEmpty(emailList))
                        {
                            var ms = new MailMessage(from, emailList)
                            {
                                Subject = "DIGEST: " + regForm.Name,
                                Body = emailText
                            };

                            sc.Send(ms);
                        }
                    }
                }
            }
   
        }
    }
}
