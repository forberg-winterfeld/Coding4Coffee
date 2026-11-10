using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A UserControl that supports dynamic localization of its child controls at runtime.
    /// Use this as the base class for custom localizable composite controls.
    /// </summary>
    public class LocalizableUserControl : UserControl, ILocalizable
    {
        /// <summary>
        /// Localizes all child controls contained within the user control.
        /// </summary>
        public virtual void Localize() => Controls.LocalizeCollection();
    }
}
