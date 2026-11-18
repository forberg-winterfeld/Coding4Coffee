using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A ToolStripDropDownButton control that supports dynamic localization of its text, tooltip text, and dropdown items at runtime.
    /// </summary>
    public class LocalizableToolStripDropDownButton : ToolStripDropDownButton, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the text of the drop down button.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the tooltip text of the drop down button.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized tooltip text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? ToolTipTextResourceKey { get; set; }

        /// <summary>
        /// Localizes the text, tooltip text, and dropdown items of the button using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager != null)
            {
                if (!string.IsNullOrWhiteSpace(TextResourceKey))
                    Text = resourceManager.GetString(TextResourceKey) ?? Text;

                if (!string.IsNullOrWhiteSpace(ToolTipTextResourceKey))
                    ToolTipText = resourceManager.GetString(ToolTipTextResourceKey) ?? ToolTipText;
            }

            DropDownItems.LocalizeCollection();
        }
    }
}
