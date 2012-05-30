namespace SenseNet.ApplicationModel
{
    public class CopyToAction : OpenPickerAction
    {
        protected override string TargetActionName
        {
            get { return "CopyToTarget"; }
        }
    }
}
