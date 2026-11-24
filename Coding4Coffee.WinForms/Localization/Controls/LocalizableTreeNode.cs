using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A TreeNode that supports dynamic localization at runtime.
    /// </summary>
    public class LocalizableTreeNode : TreeNode, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the text of the node.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Localizes the text of the node and its child nodes.
        /// </summary>
        public void Localize()
        {
            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = this.GetResourceManager()?.GetString(TextResourceKey) ?? Text;

            if (Nodes.Count > 0)
                Nodes.LocalizeCollection();
        }
    }
}

