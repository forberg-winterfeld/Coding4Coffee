using System.Collections;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ComboBox control that supports dynamic localization of its items at runtime.
    /// </summary>
    public class LocalizableComboBox : ComboBox, ILocalizable
    {
        /// <summary>
        /// Localizes the items of the ComboBox.
        /// </summary>
        public virtual void Localize()
        {
            if (DataSource is IEnumerable dataSourceCollection)
                dataSourceCollection.LocalizeCollection();
            else
                Items.LocalizeCollection();

            RefreshItems();
        }
    }
}

