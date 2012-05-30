using System;
using System.Linq;
using System.Net.Mail;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using System.Configuration;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class FormItem : GenericContent
    {
        private const string DefaultEmailSenderAppsettingKey = "DefaultEmailSender";
        private const string MailHostAppsettingKey = "SMTP";

        protected List<Attachment> Attachments { get; set; }

        //================================================================================= Constructors

        public FormItem(Node parent) : this(parent, null) { }
        public FormItem(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected FormItem(NodeToken nt) : base(nt) { }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                default:
                    return base.GetProperty(name);
            }
        }

        //================================================================================= Methods

        public override void Save(NodeSaveSettings settings)
        {
            var isNew = this.IsNew;
            bool import = RepositoryConfiguration.SpecialWorkingMode == "Import";
            if (isNew && !import)
            {
                Name = GenerateName();
            }

            base.Save(settings);

            if (isNew && !this.CopyInProgress && !import)
                SendMail();
        }

        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

        protected virtual string CreateEmailBody(bool isHtml)
        {
            var emailText = "";
            var c = Content.Create(this);
            var first = true;
            Attachments = new List<Attachment>();

            foreach (var f in c.Fields.Select(kvp => kvp.Value).Where(f => f.Name.StartsWith("#")))
            {
                var bf = f as BinaryField;
                if (bf != null)
                {
                    var bd = bf.GetData() as BinaryData;
                    if (bd != null)
                    {
                        try
                        {
                            var fs = bd.GetStream();
                            if (fs != null)
                            {
                                var fn = System.IO.Path.GetFileName(bd.FileName);
                                if (string.IsNullOrEmpty(fn))
                                    fn = string.Format("{0}-{1}", this.Name, f.Name.Replace("#", "-"));

                                Attachments.Add(new Attachment(fs, fn, bd.ContentType));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteException(ex);
                        }
                    }

                    continue;
                }

                if (first) 
                    first = false;
                else
                    emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");

                emailText = string.Concat(emailText, f.DisplayName, ": ");
                emailText = f.FieldSetting.Bindings.Aggregate(emailText, (current, b) => string.Concat(current, Convert.ToString(this[b])));
            }
            return emailText;
        }

        protected virtual string CreateEmailText()
        {
            string emailText = string.Empty;
            emailText = string.Concat(emailText, "\nName: ", Name, "\n");
            emailText = string.Concat(emailText, "\nUser: ", CreatedBy.Name, "\n");
            emailText = string.Concat(emailText, "\n\r\n", CreateEmailBody(false));

            return emailText;
        }
        
        protected static string GetSubmitterAddress(string emailField, Content c)
        {
            if (emailField == null) return string.Empty;
            //ContentRepository.Content c = ContentRepository.Content.Create(this);
            if (!c.Fields.ContainsKey(emailField)) return string.Empty;
            return c.Fields[emailField].GetData() as string;
        }

        protected virtual void SendMail()
        {
            var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
            if (string.IsNullOrEmpty(emailHost)) 
                return;

            var parentForm = this.LoadContentList() as Form;
            if (parentForm == null) 
                return;

            var itemContent = Content.Create(this);
            var sc = new SmtpClient(emailHost);
            var from = GetSender(parentForm);
            var emailList = String.Empty;
            if (!String.IsNullOrEmpty(parentForm.EmailList) )
            {
                emailList = parentForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                    .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " "); 
            }

            // send mail to administrator
            if (!string.IsNullOrEmpty(emailList))
            {
                var ms = new MailMessage(from, emailList)
                             {
                                 Subject = string.IsNullOrEmpty(parentForm.DisplayName)
                                               ? parentForm.Name
                                               : ReplaceField(parentForm.DisplayName, false, itemContent),
                                 Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                                            ? CreateEmailText()
                                            : ReplaceField(parentForm.EmailTemplate, false, itemContent)
                             };
                    
                if (Attachments.Count > 0)
                    foreach (var a in Attachments)
                        ms.Attachments.Add(a);
                        
                sc.Send(ms);
            }

            //============= Send notification email
            // send mail to submitter
            var submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
            if (!string.IsNullOrEmpty(submitterAddress))
            {
                string fromField = GetSenderOfSubmiter(parentForm);
                // send mail to submitter
                var ms = new MailMessage(fromField, submitterAddress)
                             {
                                 Subject = string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                               ? parentForm.Name
                                               : ReplaceField(parentForm.TitleSubmitter, true, itemContent),
                                 Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                                            ? CreateEmailText()
                                            : ReplaceField(parentForm.EmailTemplateSubmitter, true, itemContent),
                                 IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                             };

                if (Attachments.Count > 0)
                    foreach (var a in Attachments)
                        ms.Attachments.Add(a);

                sc.Send(ms);
            }
        }

        protected string ReplaceField(string body, bool isHtml, Content itemContent)
        {
            try
            {
                body = body.Replace("{0}", CreateEmailBody(isHtml));

                int startIdx = body.IndexOf('{');
                while (startIdx >= 0)
                {
                    int cIdx = startIdx;
                    int endIdx = body.IndexOf('}', startIdx);
                    if (startIdx < endIdx)
                    {
                        cIdx = endIdx;

                        string fieldName = body.Substring(startIdx, endIdx - startIdx + 1);
                        fieldName = fieldName.Trim('{', '}');
                        if (itemContent.Fields.ContainsKey(fieldName))
                        {
                            body = body.Remove(startIdx, endIdx - startIdx + 1);
                            var objValue = itemContent.Fields[fieldName].GetData();
                            string fieldValue = string.Empty;
                            if (objValue is List<String>)
                            {
                                fieldValue =((List<string>)objValue)[0];
                            }
                            else
                            {
                                fieldValue = itemContent.Fields[fieldName].GetData().ToString();
                            }
                            body = body.Insert(startIdx, fieldValue);
                            cIdx = startIdx + fieldValue.Length;
                        }
                    }
                    startIdx = body.IndexOf('{', cIdx);
                }
                return body;
            }
            catch (Exception ex) //logged
            {
                Logger.WriteException(ex);
                return body + " " + ex.Message + ex.StackTrace;
            }
        }

        private string GenerateName()
        {
            #region from #23609

            // return string.Concat(Parent.Name, "_", DateTime.Now.ToString("yyyy_MM_dd___hh_mm_ss_fffffff"));
            return string.Concat(Parent.Name, "_", DateTime.Now.ToString("yyyy_MM_dd___HH_mm_ss_fffffff"));

            #endregion
        }

        protected string GetSender(Form form)
        {
            var sender = form.EmailFrom;
            if (!string.IsNullOrEmpty(sender))
                return sender;
            return System.Web.Configuration.WebConfigurationManager.AppSettings[DefaultEmailSenderAppsettingKey];
        }

        protected string GetSenderOfSubmiter(Form form)
        {
            var sender = form.EmailFromSubmitter;
            if (!string.IsNullOrEmpty(sender))
                return sender;
            return System.Web.Configuration.WebConfigurationManager.AppSettings[DefaultEmailSenderAppsettingKey];
        }
        //================================================================================= Generic Property handling
    }
}