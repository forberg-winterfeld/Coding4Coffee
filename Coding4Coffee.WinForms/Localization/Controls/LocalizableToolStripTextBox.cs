using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ToolStripTextBox control that supports dynamic localization of its text and placeholder text at runtime.
    /// </summary>
    public class LocalizableToolStripTextBox : ToolStripTextBox, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the text of the text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the placeholder text of the text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized placeholder text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? PlaceholderTextResourceKey { get; set; }

        /// <summary>
        /// Localizes the text and placeholder text of the text box using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager == null) return;

            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = resourceManager.GetString(TextResourceKey) ?? Text;

            if (!string.IsNullOrWhiteSpace(PlaceholderTextResourceKey))
                TextBox.PlaceholderText = resourceManager.GetString(PlaceholderTextResourceKey) ?? TextBox.PlaceholderText;
        }
    }
}
