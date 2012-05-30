using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using System.Xml.Serialization;

namespace SenseNet.DirectoryServices
{
    public enum ActionType
    {
        CreateNewADUser,
        UpdateADUser,
        CreateNewADContainer,
        UpdateADContainer,
        DeleteADObject
    }

    [XmlRootAttribute("ADAction")]
    public class ADAction
    {
        private ActionType _actionType;
        public ActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }

        private Node _node;
        [XmlIgnore()]
        public Node Node
        {
            get 
            {
                if (_node == null)
                    _node = Node.LoadNode(_nodeId);
                return _node;
            }
            set { _node = value; }
        }

        private int _nodeId;
        public int NodeId
        {
            get 
            {
                if (_node != null)
                    return _node.Id;
                return _nodeId; 
            }
            set { _nodeId = value; }
        }

        private string _nodePath;
        public string NodePath
        {
            get { return _nodePath; }
            set { _nodePath = value; }
        }

        private Guid? _guid;
        public Guid? Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        private string _passWd;
        public string PassWd
        {
            get { return _passWd; }
            set { _passWd = value; }
        }

        private string _newPath;
        public string NewPath
        {
            get { return _newPath; }
            set { _newPath = value; }
        }

        private string _lastException;
        public string LastException
        {
            get { return _lastException; }
            set { _lastException = value; }
        }

        public void Execute()
        {
            var syncPortal2AD = new SyncPortal2AD();
            switch (_actionType)
            {
                case ActionType.CreateNewADUser:
                    syncPortal2AD.CreateNewADUser((User)Node, NewPath, PassWd);
                    break;
                case ActionType.UpdateADUser:
                    syncPortal2AD.UpdateADUser((User)Node, NewPath, PassWd);
                    break;
                case ActionType.CreateNewADContainer:
                    syncPortal2AD.CreateNewADContainer(Node, NewPath);
                    break;
                case ActionType.UpdateADContainer:
                    syncPortal2AD.UpdateADContainer(Node, NewPath);
                    break;
                case ActionType.DeleteADObject:
                    syncPortal2AD.DeleteADObject(NodePath, Guid);
                    break;
            }
        }

        public ADAction(ActionType actionType, Node node, string newPath, string passWd)
        {
            _actionType = actionType;
            _node = node;
            _newPath = newPath;
            _passWd = passWd;
            _nodePath = node.Path;
            _guid = Common.GetPortalObjectGuid(node);
        }

        public ADAction()
        {
        }

    }
}
