using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Web;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using System.Net;
using SenseNet.Search;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class SurveyItem : GenericContent
    {
        private bool _isNew = false;

        public SurveyItem(Node parent)
            : this(parent, null)
        {
        }
        public SurveyItem(Node parent, string nodeTypeName)
            : base(parent, nodeTypeName)
        {
        }
        protected SurveyItem(NodeToken nt)
            : base(nt)
        {    
        }

        protected override void OnCreating(object sender, Storage.Events.CancellableNodeEventArgs e)
        {
            base.OnCreating(sender, e);

            var searchPath = e.SourceNode.Parent.GetType().Name == "Survey" ? e.SourceNode.ParentPath : e.SourceNode.Parent.ParentPath;

            // Count Survey Items
            var surveyItemCount = ContentQuery.Query(string.Format("+Type:surveyitem +InTree:\"{0}\" .AUTOFILTERS:OFF .COUNTONLY", searchPath)).Count;

            // Get children (SurveyItems) count
            String tempName;
            if (surveyItemCount < 10 && surveyItemCount != 9)
                tempName = "SurveyItem_0" + (surveyItemCount + 1);
            else
                tempName = "SurveyItem_" + (surveyItemCount + 1);

            // If node already exits
            while (Node.Exists(RepositoryPath.Combine(e.SourceNode.Parent.Path, tempName)))
            {
                surveyItemCount++;
                if (surveyItemCount < 10)
                    tempName = "SurveyItem_0" + (surveyItemCount + 1);
                else
                    tempName = "SurveyItem_" + (surveyItemCount + 1);
            }

            e.SourceNode["DisplayName"] = tempName;
            e.SourceNode["Name"] = tempName.ToLower();
        }

        protected override void OnCreated(object sender, Storage.Events.NodeEventArgs e)
        {
            base.OnCreated(sender, e);

            SendNotification();
        }

        private void SendNotification()
        {
            var parent = Content.Create(this.Parent);
            bool isNotificationEnabled;


            if (bool.TryParse(parent.Fields["EnableNotificationMail"].GetData().ToString(), out isNotificationEnabled) && isNotificationEnabled)
            {
                var mailTemplate = this.Parent.GetReference<Node>("MailTemplatePage");
                var senderAddress = parent.Fields["SenderAddress"].GetData().ToString();
                
                if (mailTemplate != null && !string.IsNullOrEmpty(senderAddress))
                {
                    var evaluators = parent.Fields["Evaluators"].GetData() as List<Node>;
                    var emailList = new Dictionary<string, string>();

                    if (evaluators != null)
                    {
                        foreach (var evaluator in evaluators)
                        {
                            var user = evaluator as IUser;

                            if (user != null && !string.IsNullOrEmpty(user.Email) && !emailList.ContainsKey(user.Email))
                            {
                                emailList.Add(user.Email, user.FullName);
                            }
                            else
                            {
                                var group = evaluator as Group;

                                if (group != null)
                                {
                                    foreach (var usr in group.GetAllMemberUsers())
                                    {
                                        if (!string.IsNullOrEmpty(usr.Email) && !emailList.ContainsKey(usr.Email))
                                        {
                                            emailList.Add(usr.Email, usr.FullName);
                                        }
                                    }
                                }
                            }
                        }

                        var mailTemplateCnt = Content.Create(mailTemplate);

                        var mailSubject = new StringBuilder(mailTemplateCnt.Fields["Subtitle"].GetData().ToString());

                        var mailBody = new StringBuilder(mailTemplateCnt.Fields["Body"].GetData().ToString());
                        var linkText = "<a href='{0}?action={1}'>{1}</a>";
                        var url = HttpContext.Current.Request.UrlReferrer.AbsoluteUri.Substring(0,
                                                                                                HttpContext.Current.
                                                                                                    Request.
                                                                                                    UrlReferrer.
                                                                                                    AbsoluteUri.
                                                                                                    IndexOf("?"))
                                    + "/" + this.Name;

                        mailBody = mailBody.Replace("{User}", (this.CreatedBy as IUser).FullName);
                        mailBody = mailBody.Replace("{SurveyName}", parent.DisplayName);
                        mailBody = mailBody.Replace("{Browse}", string.Format(linkText, url, "Browse"));
                        mailBody = mailBody.Replace("{Evaluate}", string.Format(linkText, url, "Evaluate"));
                        mailBody = mailBody.Replace("{Creator}", (this.Parent.CreatedBy as IUser).FullName);

                        var smtpClient = new SmtpClient(System.Web.Configuration.WebConfigurationManager.AppSettings["SMTP"]);
                        var smtpUser = System.Web.Configuration.WebConfigurationManager.AppSettings["SMTPUser"];
                        var smtpPassword = System.Web.Configuration.WebConfigurationManager.AppSettings["SMTPPassword"];

                        if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPassword))
                        {
                            smtpClient.UseDefaultCredentials = false;

                            var smtpDomain = System.Web.Configuration.WebConfigurationManager.AppSettings["SMTPDomain"];
                            smtpClient.Credentials = string.IsNullOrEmpty(smtpDomain) ? new NetworkCredential(smtpUser, smtpPassword) : new NetworkCredential(smtpUser, smtpPassword, smtpDomain);
                        }

                        foreach (var email in emailList)
                        {
                            mailBody = mailBody.Replace("{Addressee}", email.Value);
                            var mailMessage = new MailMessage(senderAddress, email.Key)
                                                  {
                                                      Subject = mailSubject.ToString(),
                                                      IsBodyHtml = true,
                                                      Body = mailBody.ToString()
                                                  };

                            try
                            {
                                smtpClient.Send(mailMessage);
                            }
                            catch (Exception ex) //logged
                            {
                                Logger.WriteException(ex);
                            }
                        }
                    }
                }
                else
                {
                    Logger.WriteError("Notification e-mail cannot be sent because the template content or the sender address is missing");
                }
            }
        }
    }
}
