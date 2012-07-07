using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SPC = SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using System.Web;
using System.Text;
using SenseNet.Portal.UI.Controls;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
    [ContentHandler]
    public class EventRegistrationFormItem : FormItem
    {
        private bool _admin = false;
        private string _eventTitle;

        private const string MailHostAppsettingKey = "SMTP";

        //================================================================================= Properties

        public string EventTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_eventTitle))
                {
                    var query = LucQuery.Parse("+Type:calendarevent +RegistrationForm:" + this.ParentId);
                    var results = query.Execute();
                    if (results.FirstOrDefault() != null)
                    {
                        var ec = Content.Load(results.FirstOrDefault().NodeId);
                        _eventTitle = ec.DisplayName;
                    }
                    
                }
                
                return _eventTitle;
            }

        }


        //================================================================================= Constructors

        public EventRegistrationFormItem(Node parent) : this(parent, null) { }
        public EventRegistrationFormItem(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected EventRegistrationFormItem(NodeToken nt) : base(nt) { }

        //================================================================================= Methods

        public override bool IsTrashable
        {
            get
            {
                return false;
            }
        }

        protected override string CreateEmailBody(bool isHtml)
        {
            string emailText = "";
            ContentRepository.Content c = ContentRepository.Content.Create(this);
            bool first = true;
            foreach (var kvp in c.Fields)
            {
                Field f = kvp.Value;

                if (!f.Name.StartsWith("#") || f.Name == "Email")
                    continue;

                if (first)
                    first = false;
                else
                    emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");

                emailText = string.Concat(emailText, f.DisplayName, ": ");
                foreach (string b in f.FieldSetting.Bindings)
                    emailText = string.Concat(emailText, Convert.ToString(this[b]));
            }
            emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
            emailText = string.Concat(emailText, SenseNetResourceManager.Current.GetString("EventRegistration", "ToCancel"));
            emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
            emailText = isHtml
                             ? string.Concat(emailText, "<a href=", GenerateCancelLink(), @""">", GenerateCancelLink(), "</a>")
                             : string.Concat(emailText, GenerateCancelLink());

            if (_admin)
            {
                emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
                emailText = string.Concat(emailText, SenseNetResourceManager.Current.GetString("EventRegistration", "ToApprove"));
                emailText = isHtml ? string.Concat(emailText, "<br/>") : string.Concat(emailText, "\n\r\n");
                emailText = isHtml
                                 ? string.Concat(emailText, "<a href=", GenerateApproveLink(), @""">", GenerateApproveLink(), "</a>")
                                 : string.Concat(emailText, GenerateApproveLink());

            }
            return emailText;
        }

        protected override string CreateEmailText()
        {
            string emailText = string.Empty;
            emailText = string.Concat(emailText, "\nName: ", Name, "\n");
            emailText = string.Concat(emailText, "\nUser: ", CreatedBy.Name, "\n");
            emailText = string.Concat(emailText, "\n\r\n", CreateEmailBody(false));
            //if (_admin)
            //{
            //    emailText = string.Concat(emailText, "\n");
            //    emailText = string.Concat(emailText, "\n\r\n", SenseNetResourceManager.Current.GetString("EventRegistration", "ToApprove"));
            //    emailText = string.Concat(emailText, "\n\r\n", GenerateApproveLink());
            //}

            return emailText;
        }

        private string CreateAdminemailText()
        {
            string emailText = this.CreateEmailText();
            //emailText = string.Concat(emailText, "\n");
            //emailText = string.Concat(emailText, "\n\r\n", SenseNetResourceManager.Current.GetString("EventRegistration", "ToApprove"));
            //emailText = string.Concat(emailText, "\n\r\n", GenerateApproveLink());
            return emailText;
        }

        private string GenerateCancelLink()
        {
            string page = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return page + this.Path + "?action=Cancel&back=" + page;
        }

        private string GenerateApproveLink()
        {
            string page = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return page + this.Path + "?action=Approve&back=" + page;
        }

        public void SendCancellationMail()
        {
            var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
            if (!string.IsNullOrEmpty(emailHost))
            {
                var parentForm = this.LoadContentList() as Form;
                if (parentForm != null)
                {
                    var itemContent = Content.Create(this);
                    var sc = new SmtpClient(emailHost);
                    var from = GetSender(parentForm);
                    var emailList = String.Empty;
                    if (!String.IsNullOrEmpty(parentForm.EmailList))
                    {
                        emailList = parentForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                    }

                    // send mail to administrator
                    _admin = true;
                    if (!string.IsNullOrEmpty(emailList))
                    {
                        var ms = new MailMessage(from, emailList)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminCancellingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = "Event registration cancelled."
                        };

                        sc.Send(ms);
                    }


                    //============= Send notification email
                    // send mail to submitter
                    _admin = false;
                    string submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                    if (!string.IsNullOrEmpty(submitterAddress))
                    {
                        string fromField = GetSenderOfSubmiter(parentForm);
                        // send mail to submitter
                        var ms = new MailMessage(fromField, submitterAddress)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserCancellingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = "Event registration cancelled",
                            IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                        };

                        //sc = new SmtpClient(emailHost);
                        sc.Send(ms);
                    }
                }
            }
        }

        protected override void SendMail()
        {
            var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
            if (!string.IsNullOrEmpty(emailHost))
            {
                var parentForm = this.LoadContentList() as Form;
                if (parentForm != null)
                {
                    var itemContent = Content.Create(this);
                    var sc = new SmtpClient(emailHost);
                    var from = GetSender(parentForm);
                    var emailList = String.Empty;
                    if (!String.IsNullOrEmpty(parentForm.EmailList))
                    {
                        emailList = parentForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                                        .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                    }

                    // send mail to administrator
                    _admin = true;
                    if (!string.IsNullOrEmpty(emailList))
                    {
                        var ms = new MailMessage(from, emailList)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminSubscriptionSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                                       ? CreateAdminemailText()
                                       : ReplaceField(parentForm.EmailTemplate, false, itemContent)
                        };

                        sc.Send(ms);
                    }


                    //============= Send notification email
                    // send mail to submitter
                    _admin = false;
                    string submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                    if (!string.IsNullOrEmpty(submitterAddress))
                    {
                        string fromField = GetSenderOfSubmiter(parentForm);

                        // send mail to submitter
                        var ms = new MailMessage(fromField, submitterAddress)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserSubscriptionSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                                       ? CreateEmailText()
                                       : ReplaceField(parentForm.EmailTemplateSubmitter, true, itemContent),
                            IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                        };

                        //sc = new SmtpClient(emailHost);
                        sc.Send(ms);
                    }
                }
            }
        }

        private void CheckParticipantNumber(bool isApproving)
        {
            var form = Content.Load(Parent.Id);

            var subs = form.Children;

            var overallWantedGuests = 1;

            // Collecting approved subscriptions and getting participants.
            foreach (var node in subs)
            {
                if (node["Version"].ToString() == "V1.0.A")
                {
                    var guests = 0;
                    int.TryParse(node["GuestNumber"].ToString(), out guests);
                    overallWantedGuests += (1 + guests);
                }
            }

            var thisGuests = 0;
            int.TryParse(this["GuestNumber"].ToString(), out thisGuests);

            overallWantedGuests += thisGuests;

            var maxPart = 0;
            var result = ContentQuery.Query("+TypeIs:calendarevent +RegistrationForm:" + ParentId, new QuerySettings { EnableAutofilters = false });
            if (result.Count == 0)
                throw new NotSupportedException("This registration form must be connected to an event before you can register");
            if (result.Count > 1)
                throw new NotSupportedException("This registration form is connected to more than one event");

            var eventContent = Content.Create(result.Nodes.First());

            int.TryParse(eventContent["MaxParticipants"].ToString(), out maxPart);

            if (overallWantedGuests > maxPart)
            {
                if (!isApproving)
                {
                    throw new Exception(String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "TooManyGuests"), maxPart - (overallWantedGuests - thisGuests)));
                }
                else
                {
                    throw new Exception(String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "TooManyParticipants"), maxPart - (overallWantedGuests - thisGuests - 1)));
                }
            }


        }

        public override void Save(SavingMode mode)
        {
            CheckParticipantNumber(false);
            
            base.Save(mode);
        }

        public override void Approve()
        {
            CheckParticipantNumber(true);
            base.Approve();
            var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
            if (!string.IsNullOrEmpty(emailHost))
            {
                var parentForm = this.LoadContentList() as Form;
                if (parentForm != null)
                {
                    var itemContent = Content.Create(this);
                    var sc = new SmtpClient(emailHost);
                    var from = GetSender(parentForm);
                    var emailList = String.Empty;
                    if (!String.IsNullOrEmpty(parentForm.EmailList))
                    {
                        emailList = parentForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                            .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                    }

                    // send mail to administrator
                    _admin = true;
                    if (!string.IsNullOrEmpty(emailList))
                    {
                        var ms = new MailMessage(from, emailList)
                                     {
                                         Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminApprovingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                                         Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                                                    ? CreateAdminemailText()
                                                    : SenseNetResourceManager.Current.GetString("EventRegistration", "AdminApprovingBody")
                                     };

                        sc.Send(ms);
                    }

                    // send mail to submitter
                    _admin = false;
                    string submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                    if (!string.IsNullOrEmpty(submitterAddress))
                    {
                        string fromField = GetSenderOfSubmiter(parentForm);
                        // send mail to submitter
                        var ms = new MailMessage(fromField, submitterAddress)
                                     {
                                         Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserApprovingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                                         Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                                                    ? CreateEmailText()
                                                    : SenseNetResourceManager.Current.GetString("EventRegistration", "UserApprovingBody"),
                                         IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                                     };

                        //sc = new SmtpClient(emailHost);
                        sc.Send(ms);

                    }
                }
            }

        }

        public override void Reject()
        {
            base.Reject();

            var emailHost = System.Web.Configuration.WebConfigurationManager.AppSettings[MailHostAppsettingKey];
            if (!string.IsNullOrEmpty(emailHost))
            {
                var parentForm = this.LoadContentList() as Form;
                if (parentForm != null)
                {
                    var itemContent = Content.Create(this);
                    var sc = new SmtpClient(emailHost);
                    var from = GetSender(parentForm);
                    var emailList = String.Empty;
                    if (!String.IsNullOrEmpty(parentForm.EmailList))
                    {
                        emailList = parentForm.EmailList.Trim(new char[] { ' ', ';', ',' })
                            .Replace(";", ",").Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
                    }

                    // send mail to administrator
                    _admin = true;
                    if (!string.IsNullOrEmpty(emailList))
                    {
                        var ms = new MailMessage(from, emailList)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "AdminRejectingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = string.IsNullOrEmpty(parentForm.EmailTemplate)
                                       ? CreateAdminemailText()
                                       : SenseNetResourceManager.Current.GetString("EventRegistration", "AdminRejectingBody")
                        };

                        sc.Send(ms);
                    }

                    // send mail to submitter
                    _admin = false;
                    string submitterAddress = GetSubmitterAddress(parentForm.EmailField, itemContent);
                    if (!string.IsNullOrEmpty(submitterAddress))
                    {
                        string fromField = GetSenderOfSubmiter(parentForm);
                        // send mail to submitter
                        var ms = new MailMessage(fromField, submitterAddress)
                        {
                            Subject = String.Format(SenseNetResourceManager.Current.GetString("EventRegistration", "UserRejectingSubject"),
                                      string.IsNullOrEmpty(parentForm.TitleSubmitter)
                                          ? EventTitle
                                          : ReplaceField(parentForm.TitleSubmitter, true, itemContent)),
                            Body = string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                                       ? CreateEmailText()
                                       : SenseNetResourceManager.Current.GetString("EventRegistration", "UserRejectingBody"),
                            IsBodyHtml = !string.IsNullOrEmpty(parentForm.EmailTemplateSubmitter)
                        };

                        //sc = new SmtpClient(emailHost);
                        sc.Send(ms);

                    }
                }
            }

        }

        //================================================================================= Generic Property handling
    }
}