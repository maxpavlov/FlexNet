namespace SenseNet.Portal.UI.Controls
{
    public interface IActionUiAdapter
    {
        string NodePath { get; set; }
        string ContextInfoID { get; set; }
        string WrapperCssClass { get; set; }
        string Scenario { get; set; }
        string ScenarioParameters { get; set; }
        string Text { get; set; }
        string ActionName { get; set; }
        string IconName { get; set; }
        string IconUrl { get; set; }
        bool OverlayVisible { get; set; }
    }
}