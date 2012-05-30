using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Schema;

using SenseNet.ContentRepository.i18n;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
    public abstract class ViewControlBase : WebControl, INamingContainer
    {
        private const string FieldValidationErrorPrefix = "ValidationError";
        private const string FieldValidationErrorCategory = "FieldValidation";

        private ContentView _contentView;

        internal ContentView ContentView
        {
            get { return _contentView; }
        }

        protected override void OnInit(EventArgs e)
        {
            _contentView = ContentView.RegisterControl(this);
        }

        internal static string ResolveValidationResult(Field field)
        {
            // ValidationError_Car_Make_ShortText_Compulsory
            // ValidationError_Car_ShortText_Compulsory
            // ValidationError_Make_ShortText_Compulsory
            // ValidationError_ShortText_Compulsory
            // ValidationError_Compulsory

            string key;
            string msg;

            var contentTypeName = field.Content.ContentType.Name;
            var fieldName = field.Name;
            var fieldTypeName = field.FieldSetting.ShortName;
            var error = field.ValidationResult.Category;

            key = String.Concat(FieldValidationErrorPrefix, "_", contentTypeName, "_", fieldName, "_", fieldTypeName, "_", error);
            msg = SenseNetResourceManager.Current.GetStringOrNull(FieldValidationErrorCategory, key);
            if (msg == null)
            {
                key = String.Concat(FieldValidationErrorPrefix, "_", contentTypeName, "_", fieldTypeName, "_", error);
                msg = SenseNetResourceManager.Current.GetStringOrNull(FieldValidationErrorCategory, key);
                if (msg == null)
                {
                    key = String.Concat(FieldValidationErrorPrefix, "_", fieldName, "_", fieldTypeName, "_", error);
                    msg = SenseNetResourceManager.Current.GetStringOrNull(FieldValidationErrorCategory, key);
                    if (msg == null)
                    {
                        key = String.Concat(FieldValidationErrorPrefix, "_", fieldTypeName, "_", error);
                        msg = SenseNetResourceManager.Current.GetStringOrNull(FieldValidationErrorCategory, key);
                        if (msg == null)
                        {
                            key = String.Concat(FieldValidationErrorPrefix, "_", error);
                            msg = SenseNetResourceManager.Current.GetStringOrNull(FieldValidationErrorCategory, key);
                        }
                    }
                }
            }

            if (msg != null)
                return field.ValidationResult.FormatMessage(msg);

            return field.GetValidationMessage();
        }

    }
}