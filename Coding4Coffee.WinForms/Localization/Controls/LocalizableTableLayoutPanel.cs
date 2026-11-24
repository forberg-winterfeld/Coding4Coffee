using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A TableLayoutPanel control that supports dynamic localization of its child controls at runtime.
    /// </summary>
    public class LocalizableTableLayoutPanel : TableLayoutPanel, ILocalizable
    {
        /// <summary>
        /// Localizes all child controls contained within the table layout panel.
        /// </summary>
        public void Localize() => Controls.LocalizeCollection();
    }
}
