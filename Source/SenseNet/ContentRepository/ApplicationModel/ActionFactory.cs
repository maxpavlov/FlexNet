using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using System.Web.Configuration;

namespace SenseNet.ApplicationModel
{
    internal class ActionFactory
    {
        internal static ActionBase CreateAction(Type actionType, Application application, Content context, string backUri, object parameters)
        {
            var act = TypeHandler.CreateInstance(actionType.FullName) as ActionBase;
            if (act != null)
                act.Initialize(context, backUri, application, parameters);

            return act == null || !act.Visible ? null : act;
        }

        internal static ActionBase CreateAction(string actionType, Content context, string backUri, object parameters)
        {
            return CreateAction(actionType, null, context, backUri, parameters);
        }

        internal static ActionBase CreateAction(string actionType, Application application, Content context, string backUri, object parameters)
        {
            var actionName = application != null ? application.Name : actionType;

            //check versioning action validity
            if (IsInvalidVersioningAction(context, actionName))
                return null;

            if (string.IsNullOrEmpty(actionType))
                actionType = WebConfigurationManager.AppSettings["DefaultActionType"] ?? "UrlAction";

            var act = TypeHandler.ResolveNamedType<ActionBase>(actionType);

            if (act == null)
                throw new ApplicationException("Unknown action: " + actionType);

            act.Initialize(context, backUri, application, parameters);          

            return act.Visible ? act : null;
        }

        private static bool IsInvalidVersioningAction(Content context, string actionName)
        {
            if (string.IsNullOrEmpty(actionName) || context == null)
                return false;

            actionName = actionName.ToLower();

            var generic = context.ContentHandler as GenericContent;
            if (generic == null)
                return false;

            switch (actionName)
            {
                case "checkin":
                    return !SavingAction.HasCheckIn(generic);
                case "checkout":
                    return (generic.VersioningMode <= VersioningType.None && !(generic is IFile || generic.NodeType.IsInstaceOfOrDerivedFrom("Page"))) || !SavingAction.HasCheckOut(generic);
                case "undocheckout":
                    return !SavingAction.HasUndoCheckOut(generic);
                case "forceundocheckout":
                    return !SavingAction.HasForceUndoCheckOutRight(generic);
                case "publish":
                    return (generic.VersioningMode <= VersioningType.None || !SavingAction.HasPublish(generic));
                case "approve":
                case "reject":
                    return !generic.Approvable;
                default:
                    return false;
            }
        }
    }
}
