using System.Collections.Generic;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class NewScenario : GenericScenario
    {
        public bool DisplaySystemFolders { get; set; }

        public override string Name
        {
            get
            {
                return "New";
            }
        }

        public override IComparer<ActionBase> GetActionComparer()
        {
            return new ActionComparerByText();
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actList = new List<ActionBase>();

            if (context == null || !context.Security.HasPermission(PermissionType.AddNew))
                return actList;

            var app = ApplicationStorage.Instance.GetApplication("Add", context, PortalContext.Current.DeviceName);
            var gc = context.ContentHandler as GenericContent;

            if (gc != null && app != null)
            {
                foreach (var node in GetNewItemNodes(gc))
                {                    
                    var ctype = node as ContentType;
                    if (ctype != null)
                    {
                        if (!DisplaySystemFolders && ctype.IsInstaceOfOrDerivedFrom("SystemFolder"))
                            continue;

                        //skip Add action if the user tries to add a list without having a manage container permission
                        if (!SavingAction.CheckManageListPermission(ctype.NodeType, context.ContentHandler))
                            continue;

                        var act = app.CreateAction(context, backUrl, new { ContentTypeName = ctype.Name });
                        if (act == null)
                            continue;
                        act.Text = ctype.DisplayName;
                        act.Icon = ctype.Icon;
                        actList.Add(act);
                    }
                    else
                    {
                        var ctd = node as GenericContent;
                        if (ctd == null)
                            continue;

                        if (!DisplaySystemFolders && ctd.NodeType.IsInstaceOfOrDerivedFrom("SystemFolder"))
                            continue;

                        //skip Add action if the user tries to add a list without having a manage container permission
                        if (!SavingAction.CheckManageListPermission(ctd.NodeType, context.ContentHandler))
                            continue;

                        var act = app.CreateAction(context, backUrl, new { ContentTypeName = ctd.Path });
                        act.Text = ctd.DisplayName;
                        act.Icon = ctd.Icon;
                        actList.Add(act);
                    }
                }
            }

            return actList;
        }

        public override void Initialize(Dictionary<string, object> parameters)
        {
            base.Initialize(parameters);

            if (parameters == null)
                return;

            if (!parameters.ContainsKey("DisplaySystemFolders")) 
                return;

            var dsfVal = parameters["DisplaySystemFolders"];
            if (dsfVal == null)
                return;

            bool dsf;
            if (bool.TryParse(dsfVal.ToString().ToLower(), out dsf))
                DisplaySystemFolders = dsf;
        }
    }
}
