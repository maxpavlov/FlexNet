using System;
using System.Web;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class CalendarEvent : GenericContent
    {
        private bool _successfulFormCreation = false;

        [RepositoryProperty("RegistrationForm", RepositoryDataType.Reference)]
        public virtual Node RegistrationForm
        {
            get { return GetReference<Node>("RegistrationForm"); }
            set { SetReference("RegistrationForm", value); }
        }

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case "RegistrationForm":
                    return RegistrationForm;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case "RegistrationForm":
                    RegistrationForm = (Node)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }


        //================================================================================= Construction

        public CalendarEvent(Node parent) : this(parent, null) { }
        public CalendarEvent(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected CalendarEvent(NodeToken nt) : base(nt) { }


        public override void Save(SavingMode mode)
        {
            _successfulFormCreation = false;
            // Creating registration form if necessary.
            if (GetReferenceCount("RegistrationForm") == 0 && Convert.ToBoolean(this["RequiresRegistration"]))
            {
                try
                {
                    var regFormFolder = Parent.GetPropertySafely("RegistrationFolder") as NodeList<Node>;


                    if (regFormFolder != null)
                    {
                        var formFolder = regFormFolder[0];

                        var formName = String.Format("{0}_{1}", ParentName, this["Name"]);

                        if (Content.Load(formFolder.Path + "/" + formName) == null)
                        {
                            var regForm = Content.CreateNew("EventRegistrationForm", formFolder, formName);

                            regForm["Name"] = formName;

                            //regForm["ContentTypes"] = LoadNode("/Root/System/Schema/ContentTypes/GenericContent/ListItem/FormItem/EventRegistrationFormItem");
                            regForm["AllowedChildTypes"] = ContentType.GetByName("EventRegistrationFormItem");

                            regForm["EmailList"] = !String.IsNullOrEmpty(this["OwnerEmail"].ToString()) ? this["OwnerEmail"].ToString() : String.Empty;

                            regForm["EmailTemplate"] = !String.IsNullOrEmpty(this["EmailTemplate"].ToString()) ? this["EmailTemplate"] : "{0}";

                            regForm["EmailTemplateSubmitter"] = !String.IsNullOrEmpty(this["EmailTemplateSubmitter"].ToString()) ? this["EmailTemplateSubmitter"] : "{0}";

                            regForm["EmailFrom"] = !String.IsNullOrEmpty(this["EmailFrom"].ToString()) ? this["EmailFrom"] : "mailerservice@portalengine.org";

                            regForm["EmailFromSubmitter"] = !String.IsNullOrEmpty(this["EmailFromSubmitter"].ToString()) ? this["EmailFromSubmitter"] : "mailerservice@portalengine.org";

                            regForm["EmailField"] = !String.IsNullOrEmpty(this["EmailField"].ToString()) ? this["EmailField"] : "mailerservice@portalengine.org";

                            regForm.Save();

                            AddReference("RegistrationForm", LoadNode(regForm.Id));
                            
                            _successfulFormCreation = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            base.Save(mode);
        }

        protected override void OnCreated(object sender, Storage.Events.NodeEventArgs e)
        {
            base.OnCreated(sender, e);
            if (_successfulFormCreation)
            {
                string page = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                var backUrl = HttpContext.Current.Request.Params["back"];
                var link = string.Concat(page, RegistrationForm.Path, "?action=ManageFields", "&back=", backUrl);
                HttpContext.Current.Response.Redirect(link);
            }
        }



    }
}
