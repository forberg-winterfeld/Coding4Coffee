using System.ComponentModel;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A DataGridView control that supports dynamic localization of its column headers at runtime.
    /// </summary>
    public class LocalizableDataGridView : DataGridView, ILocalizable
    {
        /// <summary>
        /// Gets the list of column localization configurations.
        /// </summary>
        [Category("Localization")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DataGridViewColumnLocalizationInfo> ColumnLocalizations { get; } = [];

        /// <summary>
        /// Localizes the column headers and child controls of the DataGridView.
        /// </summary>
        public void Localize()
        {
            foreach (var info in ColumnLocalizations)
            {
                if (!string.IsNullOrWhiteSpace(info.ColumnName) && !string.IsNullOrWhiteSpace(info.HeaderTextResourceKey))
                {
                    if (Columns.Contains(info.ColumnName))
                    {
                        var column = Columns[info.ColumnName];
                        column?.HeaderText = this.GetResourceManager()?.GetString(info.HeaderTextResourceKey) ?? column.HeaderText;
                    }
                }
            }

            Controls.LocalizeCollection();
        }
    }

    /// <summary>
    /// Configuration info for localizing a DataGridView column.
    /// </summary>
    public class DataGridViewColumnLocalizationInfo
    {
        /// <summary>
        /// Gets or sets the name of the column to localize.
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource key used to localize the column's header text.
        /// </summary>
        public string HeaderTextResourceKey { get; set; } = string.Empty;
    }
}
