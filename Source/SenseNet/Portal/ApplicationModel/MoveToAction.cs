namespace SenseNet.ApplicationModel
{
    public class MoveToAction : OpenPickerAction
    {
        protected override string TargetActionName
        {
            get { return "MoveToTarget"; }
        }
    }
}
