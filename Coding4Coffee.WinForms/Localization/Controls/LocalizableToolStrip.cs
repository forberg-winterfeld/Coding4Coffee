using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ToolStrip control that supports dynamic localization of its items at runtime.
    /// </summary>
    public class LocalizableToolStrip : ToolStrip, ILocalizable
    {
        /// <summary>
        /// Localizes all items contained within the ToolStrip.
        /// </summary>
        public void Localize() => Items.LocalizeCollection();
    }
}
