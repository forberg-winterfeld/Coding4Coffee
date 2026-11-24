using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A BindingNavigator control that supports dynamic localization of its navigation items at runtime.
    /// </summary>
    public class LocalizableBindingNavigator : BindingNavigator, ILocalizable
    {
        /// <summary>
        /// Localizes all items contained within the binding navigator.
        /// </summary>
        public void Localize() => Items.LocalizeCollection();
    }
}
