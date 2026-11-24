using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A rich text box control that supports dynamic localization of its text at runtime.
    /// </summary>
    public class LocalizableRichTextBox : RichTextBox, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the text of the rich text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Localizes the text of the rich text box using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = this.GetResourceManager()?.GetString(TextResourceKey) ?? Text;
        }
    }
}
