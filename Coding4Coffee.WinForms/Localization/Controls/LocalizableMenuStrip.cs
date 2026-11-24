using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A MenuStrip control that supports dynamic localization of its menu items at runtime.
    /// </summary>
    public class LocalizableMenuStrip : MenuStrip, ILocalizable
    {
        /// <summary>
        /// Localizes all menu items contained within the MenuStrip.
        /// </summary>
        public void Localize() => Items.LocalizeCollection();
    }
}
