using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A TabControl that supports dynamic localization of its tab pages and child controls at runtime.
    /// </summary>
    public class LocalizableTabControl : TabControl, ILocalizable
    {
        /// <summary>
        /// Localizes all tab pages and child controls contained within the TabControl.
        /// </summary>
        public void Localize()
        {
            TabPages.LocalizeCollection();
            Controls.LocalizeCollection();
        }
    }
}
