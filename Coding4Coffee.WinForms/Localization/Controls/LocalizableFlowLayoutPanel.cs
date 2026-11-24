using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A FlowLayoutPanel control that supports dynamic localization of its child controls at runtime.
    /// </summary>
    public class LocalizableFlowLayoutPanel : FlowLayoutPanel, ILocalizable
    {
        /// <summary>
        /// Localizes all child controls contained within the flow layout panel.
        /// </summary>
        public void Localize() => Controls.LocalizeCollection();
    }
}
