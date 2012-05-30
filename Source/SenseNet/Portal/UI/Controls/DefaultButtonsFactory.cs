using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Versioning;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    internal class DefaultButtonsFactory : DefaultButtonsFactoryBase
    {
        
        public override Control CreateActionButtons(DefaultButtonType button)
        {
            var _isContentType = false;    
            var currentButtonsControl = CurrentControl as DefaultButtons;
            if (currentButtonsControl == null)
                return null;
            if (currentButtonsControl.CurrentGC == null)
            {
                //
                //  ContentType is always being handled specially.
                //
                var nodeTypeName = currentButtonsControl.CurrentNode.GetType().Name;

                //
                // treat contenttype behaviour 'til the final solution
                //
                if (nodeTypeName.Equals("RuntimeContentHandler")) 
                {
                    _isContentType = true;
                } else
                {
                    if (!nodeTypeName.Equals(typeof(ContentType).Name))
                        return null;
                    _isContentType = true;
                }

            }
                
            var currentGenericContent = currentButtonsControl.CurrentGC;
            SecurityHandler security = null;
            if (!_isContentType)
                security = currentButtonsControl.NewContent ? currentGenericContent.Parent.Security : currentGenericContent.Security;

            switch (button)
            {
                case DefaultButtonType.CheckIn:
                    if (currentButtonsControl.VisibleCheckIn && !_isContentType && SavingAction.HasCheckIn(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnCheckIn",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "CheckIn") as
                                 string),
                            CommandName = "checkin",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.CheckOut:
                    var isInWorkspaceAction = PortalContext.Current.ContextWorkspace != null;

                    if (currentButtonsControl.VisibleCheckOut && !_isContentType && SavingAction.HasCheckOut(currentGenericContent))
                    {
                        //
                        //  TODO: checkout button must be displayed in the next release
                        //  In Beta 5 this function is not allowed. Check the Task 3978.
                        //
                        if (isInWorkspaceAction)
                            return null;

                        return new Button
                        {
                            ID = "btnCheckOut",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "CheckOut") as
                                 string),
                            CommandName = "checkout",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.UndoCheckOut:
                    if (currentButtonsControl.VisibleUndoCheckOut && !_isContentType && SavingAction.HasUndoCheckOut(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnUndoCheckOut",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "UndoCheckOut")
                                 as string),
                            CommandName = "undocheckout",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.Save:
                    var saveButton = new Button
                    {
                        ID = "btnSave",
                        Text =
                            (HttpContext.GetGlobalResourceObject("Portal", "Save") as string),
                        CommandName = "save",
                        Enabled = true,
                        EnableViewState = false
                    };

                    if (currentButtonsControl.VisibleSave && !_isContentType && SavingAction.HasSave(currentGenericContent))
                    {
                        if (currentButtonsControl.NewContent
                                ? security.HasPermission(PermissionType.AddNew)
                                : security.HasPermission(PermissionType.Save))
                            return saveButton;
                    }

                    if (_isContentType)
                        return saveButton;
                    
                    break;
                case DefaultButtonType.Publish:
                    if (currentButtonsControl.VisiblePublish && !_isContentType && SavingAction.HasPublish(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnPublish",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "Publish") as
                                 string),
                            CommandName = "publish",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.Approve:
                    if (currentButtonsControl.VisibleApprove && !_isContentType && SavingAction.HasApprove(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnApprove",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "Approve") as
                                 string),
                            CommandName = "approve",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.Reject:
                    if (currentButtonsControl.VisibleReject && !_isContentType && SavingAction.HasReject(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnReject",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "Reject") as string),
                            CommandName = "reject",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.Cancel:
                    if (currentButtonsControl.VisibleCancel)
                    {
                        return new Button
                        {
                            ID = "btnCancel",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal", "Cancel") as string),
                            CommandName = "cancel",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.ForceUndoCheckOut:
                    if (currentButtonsControl.VisibleForceUndoCheckOut && !_isContentType && SavingAction.HasForceUndoCheckOutRight(currentGenericContent))
                    {
                        return new Button
                        {
                            ID = "btnForceUndoCheckOut",
                            Text =
                                (HttpContext.GetGlobalResourceObject("Portal",
                                                                     "ForceUndoCheckOut")
                                 as string),
                            CommandName = "forceundocheckout",
                            Enabled = true,
                            EnableViewState = false
                        };
                    }
                    break;
                case DefaultButtonType.None:
                case DefaultButtonType.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
            return null;
        }
    }
}
