using Coding4Coffee.WinForms.Localization.Controls;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A custom TreeView control that supports multiple command bindings for different user interactions.
    /// </summary>
    /// <remarks>
    /// Provides dedicated command binding for three distinct tree view interactions: node selection (<see cref="SelectCommand"/>),
    /// double-clicking (<see cref="DoubleClickCommand"/>), and expanding (<see cref="ExpandCommand"/>).
    /// For each command, if no explicit parameter is configured, the parameter defaults to the affected <see cref="TreeNode.Tag"/>
    /// (if non-null) or the <see cref="TreeNode"/> itself.
    /// </remarks>
    public class LocalizableTreeViewWithCommandSupport : LocalizableTreeView
    {
        private ICommand? _selectCommand;
        private object? _selectCommandParameter;

        private ICommand? _doubleClickCommand;
        private object? _doubleClickCommandParameter;

        private ICommand? _expandCommand;
        private object? _expandCommandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableTreeViewWithCommandSupport"/> class.
        /// </summary>
        public LocalizableTreeViewWithCommandSupport()
        {
            // Events einmal zentral registrieren
            AfterSelect += TreeView_AfterSelect;
            NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;
            AfterExpand += TreeView_AfterExpand;
        }

        #region Select Command
        /// <summary>
        /// Gets or sets the command executed when a tree node is selected.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="TreeView.AfterSelect"/>. Passes <see cref="SelectCommandParameter"/>,
        /// or falls back to <see cref="TreeNode.Tag"/> (if non-null), or the <see cref="TreeNode"/> itself.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Command executed when a tree node is selected.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? SelectCommand
        {
            get => _selectCommand;
            set => _selectCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for the <see cref="SelectCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the parameter passed to the command defaults to the selected node's
        /// <see cref="TreeNode.Tag"/> (if non-null) or the <see cref="TreeNode"/> instance.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional parameter for the SelectCommand on AfterSelect event. Defaults to SelectedNode.Tag or SelectedNode.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? SelectCommandParameter
        {
            get => _selectCommandParameter;
            set => _selectCommandParameter = value;
        }

        private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (_selectCommand != null && e.Node != null)
            {
                var parameter = _selectCommandParameter ?? e.Node.Tag ?? e.Node;
                if (_selectCommand.CanExecute(parameter))
                {
                    _selectCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a select command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableTreeViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableTreeViewWithCommandSupport BindSelectCommand(ICommand command, object? parameter = null)
        {
            SelectCommandParameter = parameter;
            SelectCommand = command;

            return this;
        }
        #endregion

        #region DoubleClick Command
        /// <summary>
        /// Gets or sets the command executed when a tree node is double-clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="TreeView.NodeMouseDoubleClick"/>. Passes <see cref="DoubleClickCommandParameter"/>,
        /// or falls back to <see cref="TreeNode.Tag"/> (if non-null), or the <see cref="TreeNode"/> itself.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Command executed when a tree node is double-clicked.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? DoubleClickCommand
        {
            get => _doubleClickCommand;
            set => _doubleClickCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for the <see cref="DoubleClickCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the parameter passed to the command defaults to the double-clicked node's
        /// <see cref="TreeNode.Tag"/> (if non-null) or the <see cref="TreeNode"/> instance.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional parameter for the DoubleClickCommand on NodeMouseDoubleClick event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? DoubleClickCommandParameter
        {
            get => _doubleClickCommandParameter;
            set => _doubleClickCommandParameter = value;
        }

        private void TreeView_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (_doubleClickCommand != null && e.Node != null)
            {
                var parameter = _doubleClickCommandParameter ?? e.Node.Tag ?? e.Node;
                if (_doubleClickCommand.CanExecute(parameter))
                {
                    _doubleClickCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a double-click command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableTreeViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableTreeViewWithCommandSupport BindDoubleClickCommand(ICommand command, object? parameter = null)
        {
            DoubleClickCommandParameter = parameter;
            DoubleClickCommand = command;

            return this;
        }
        #endregion

        #region Expand Command
        /// <summary>
        /// Gets or sets the command executed when a tree node is expanded.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="TreeView.AfterExpand"/>. Passes <see cref="ExpandCommandParameter"/>,
        /// or falls back to <see cref="TreeNode.Tag"/> (if non-null), or the <see cref="TreeNode"/> itself.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Command executed when a tree node is expanded.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? ExpandCommand
        {
            get => _expandCommand;
            set => _expandCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for the <see cref="ExpandCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the parameter passed to the command defaults to the expanded node's
        /// <see cref="TreeNode.Tag"/> (if non-null) or the <see cref="TreeNode"/> instance.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional parameter for the ExpandCommand on AfterExpand event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? ExpandCommandParameter
        {
            get => _expandCommandParameter;
            set => _expandCommandParameter = value;
        }

        private void TreeView_AfterExpand(object? sender, TreeViewEventArgs e)
        {
            if (_expandCommand != null && e.Node != null)
            {
                var parameter = _expandCommandParameter ?? e.Node.Tag ?? e.Node;
                if (_expandCommand.CanExecute(parameter))
                {
                    _expandCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds an expand command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableTreeViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableTreeViewWithCommandSupport BindExpandCommand(ICommand command, object? parameter = null)
        {
            ExpandCommandParameter = parameter;
            ExpandCommand = command;

            return this;
        }
        #endregion
    }
}