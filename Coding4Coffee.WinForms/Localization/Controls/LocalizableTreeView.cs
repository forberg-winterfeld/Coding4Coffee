using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A TreeView control that supports dynamic localization of its nodes and child controls at runtime.
    /// </summary>
    public class LocalizableTreeView : TreeView, ILocalizable
    {
        /// <summary>
        /// Localizes all tree nodes and child controls contained within the TreeView.
        /// </summary>
        public void Localize()
        {
            Nodes.LocalizeCollection();
            Controls.LocalizeCollection();
        }
    }
}
