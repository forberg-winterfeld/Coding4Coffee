using System.ComponentModel;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A masked text box control that supports dynamic localization of its mask and text at runtime.
    /// </summary>
    public class LocalizableMaskedTextBox : MaskedTextBox, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the mask of the masked text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized mask from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? MaskResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the text of the masked text box.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Localizes the mask and text of the masked text box using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager == null) return;

            if (!string.IsNullOrWhiteSpace(MaskResourceKey))
                Mask = resourceManager.GetString(MaskResourceKey) ?? Mask;

            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = resourceManager.GetString(TextResourceKey) ?? Text;
        }
    }
}
