namespace Coding4Coffee.WinForms.Configuration.Controls
{
    /// <summary>Specifies which part of a combo box selection is stored in configuration.</summary>
    public enum ConfigurationComboBoxValueMode
    {
        /// <summary>Stores the selected index as a nullable integer.</summary>
        Index,
        /// <summary>Stores the string value identified by <see cref="ListControl.ValueMember"/>.</summary>
        Value,
        /// <summary>Stores the displayed text of the selected item.</summary>
        Text
    }
}
