using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ListView control that supports dynamic localization of its column headers at runtime.
    /// </summary>
    public class LocalizableListView : ListView, ILocalizable
    {
        /// <summary>
        /// Localizes the columns and child controls of the ListView.
        /// </summary>
        public void Localize()
        {
            foreach (ColumnHeader column in Columns)
            {
                if (column is ILocalizable localizableColumn)
                    localizableColumn.Localize();
            }

            Controls.LocalizeCollection();
        }
    }
}
