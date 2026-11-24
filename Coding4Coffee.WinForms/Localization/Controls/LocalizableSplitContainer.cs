using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A SplitContainer control that supports dynamic localization of its child controls at runtime.
    /// </summary>
    public class LocalizableSplitContainer : SplitContainer, ILocalizable
    {
        /// <summary>
        /// Localizes all child controls contained within both panels of the split container.
        /// </summary>
        public void Localize()
        {
            Panel1.Controls.LocalizeCollection();
            Panel2.Controls.LocalizeCollection();
        }
    }
}
