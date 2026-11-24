using System.Collections;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ToolStripComboBox control that supports dynamic localization of its items at runtime.
    /// </summary>
    public class LocalizableToolStripComboBox : ToolStripComboBox, ILocalizable
    {
        /// <summary>
        /// Localizes the items of the ToolStripComboBox.
        /// </summary>
        public void Localize()
        {
            if (ComboBox.DataSource is IEnumerable dataSourceCollection)
                dataSourceCollection.LocalizeCollection();
            else
                Items.LocalizeCollection();

            ComboBox.Refresh();
        }
    }
}
