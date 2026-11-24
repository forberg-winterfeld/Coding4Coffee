using System.Collections;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A checked list box control that supports dynamic localization of its items or datasource elements.
    /// </summary>
    public class LocalizableCheckedListBox : CheckedListBox, ILocalizable
    {
        /// <summary>
        /// Localizes the items inside the data source of the checked list box and refreshes the displayed items.
        /// </summary>
        public void Localize()
        {
            if (DataSource is IEnumerable collection)
                collection.LocalizeCollection();
            else
                Items.LocalizeCollection();

            RefreshItems();
        }

        /// <summary>
        /// Refreshes the display of items in the checked list box.
        /// </summary>
        public void RefreshData() => RefreshItems();
    }
}
