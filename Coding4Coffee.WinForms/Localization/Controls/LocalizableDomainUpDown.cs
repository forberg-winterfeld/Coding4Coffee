using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A DomainUpDown control that supports dynamic localization of its items at runtime.
    /// </summary>
    public class LocalizableDomainUpDown : DomainUpDown, ILocalizable
    {
        /// <summary>
        /// Localizes the items of the DomainUpDown control.
        /// </summary>
        public void Localize()
        {
            Items.LocalizeCollection();

            int selectedIndex = SelectedIndex;
            if (selectedIndex >= 0)
            {
                SelectedIndex = -1;
                SelectedIndex = selectedIndex;
            }
        }
    }
}

