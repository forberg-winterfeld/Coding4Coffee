using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A text box control that supports dynamic localization of its placeholder text at runtime.
    /// </summary>
    public class LocalizableTextBox : TextBox, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the placeholder text of the text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? PlaceholderResourceKey { get; set; }

        /// <summary>
        /// Localizes the placeholder text of the text box using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            if (!string.IsNullOrWhiteSpace(PlaceholderResourceKey))
                PlaceholderText = this.GetResourceManager()?.GetString(PlaceholderResourceKey) ?? PlaceholderText;
        }
    }
}

