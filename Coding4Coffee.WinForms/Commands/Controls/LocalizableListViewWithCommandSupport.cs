using Coding4Coffee.WinForms.Localization.Controls;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable ListView control that supports multiple command bindings for different user interactions.
    /// </summary>
    /// <remarks>
    /// Provides dedicated command binding for two distinct list view interactions: item selection (<see cref="SelectCommand"/>)
    /// and double-clicking (<see cref="DoubleClickCommand"/>).
    /// For each command, if no explicit parameter is configured, the parameter defaults to the affected <see cref="ListViewItem.Tag"/>
    /// (if non-null) or the <see cref="ListViewItem"/> itself.
    /// </remarks>
    public class LocalizableListViewWithCommandSupport : LocalizableListView
    {
        private ICommand? _selectCommand;
        private object? _selectCommandParameter;

        private ICommand? _doubleClickCommand;
        private object? _doubleClickCommandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableListViewWithCommandSupport"/> class.
        /// </summary>
        public LocalizableListViewWithCommandSupport()
        {
            // Register events once centrally
            ItemSelectionChanged += ListView_ItemSelectionChanged;
            MouseDoubleClick += ListView_MouseDoubleClick;
        }

        #region Select Command
        /// <summary>
        /// Gets or sets the command executed when a list view item is selected.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="ListView.ItemSelectionChanged"/>. Passes <see cref="SelectCommandParameter"/>,
        /// or falls back to <see cref="ListViewItem.Tag"/> (if non-null), or the <see cref="ListViewItem"/> itself.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Command executed when a list view item is selected.")]
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
        /// If <see langword="null"/>, the parameter passed to the command defaults to the selected item's
        /// <see cref="ListViewItem.Tag"/> (if non-null) or the <see cref="ListViewItem"/> instance.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional parameter for the SelectCommand on ItemSelectionChanged event. Defaults to SelectedItem.Tag or SelectedItem.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? SelectCommandParameter
        {
            get => _selectCommandParameter;
            set => _selectCommandParameter = value;
        }

        private void ListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (_selectCommand != null && e.IsSelected && e.Item != null)
            {
                var parameter = _selectCommandParameter ?? e.Item.Tag ?? e.Item;
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
        /// <returns>The current <see cref="LocalizableListViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableListViewWithCommandSupport BindSelectCommand(ICommand command, object? parameter = null)
        {
            SelectCommandParameter = parameter;
            SelectCommand = command;

            return this;
        }
        #endregion

        #region DoubleClick Command
        /// <summary>
        /// Gets or sets the command executed when a list view item is double-clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="Control.MouseDoubleClick"/> when a list item is targeted.
        /// Passes <see cref="DoubleClickCommandParameter"/>, or falls back to <see cref="ListViewItem.Tag"/> (if non-null),
        /// or the <see cref="ListViewItem"/> itself.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Command executed when a list view item is double-clicked.")]
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
        /// If <see langword="null"/>, the parameter passed to the command defaults to the double-clicked item's
        /// <see cref="ListViewItem.Tag"/> (if non-null) or the <see cref="ListViewItem"/> instance.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional parameter for the DoubleClickCommand on MouseDoubleClick event.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? DoubleClickCommandParameter
        {
            get => _doubleClickCommandParameter;
            set => _doubleClickCommandParameter = value;
        }

        private void ListView_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (_doubleClickCommand != null)
            {
                var item = GetItemAt(e.X, e.Y);
                if (item != null)
                {
                    var parameter = _doubleClickCommandParameter ?? item.Tag ?? item;
                    if (_doubleClickCommand.CanExecute(parameter))
                    {
                        _doubleClickCommand.Execute(parameter);
                    }
                }
            }
        }

        /// <summary>
        /// Binds a double-click command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableListViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableListViewWithCommandSupport BindDoubleClickCommand(ICommand command, object? parameter = null)
        {
            DoubleClickCommandParameter = parameter;
            DoubleClickCommand = command;

            return this;
        }
        #endregion
    }
}
