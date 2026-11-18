using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable CheckedListBox control that integrates command binding support.
    /// </summary>
    /// <remarks>
    /// Executes the bound command when the checked state of an item changes via the <see cref="CheckedListBox.ItemCheck"/> event.
    /// Inherits from <see cref="LocalizableCheckedListBox"/>, meaning dynamic localization applies to items at runtime.
    /// For each checked item change, if no explicit parameter is configured, the parameter defaults to the affected 
    /// <see cref="object.ToString()"/> representation of the item (typically the item's Text or display value).
    /// Use this control for multi-select scenarios such as permission management, filtering, or bulk operations.
    /// </remarks>
    public class LocalizableCheckedListBoxWithCommandSupport : LocalizableCheckedListBox
    {
        private ICommand? _command;
        private object? _commandParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableCheckedListBoxWithCommandSupport"/> class.
        /// </summary>
        public LocalizableCheckedListBoxWithCommandSupport()
        {
            ItemCheck += CheckedListBox_ItemCheck;
        }

        /// <summary>
        /// Gets or sets the command to execute when an item's checked state changes.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="CheckedListBox.ItemCheck"/>.
        /// The command receives the item at the changed index.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? Command
        {
            get => _command;
            set => _command = value;
        }

        /// <summary>
        /// Gets or sets an optional command parameter.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, the item at the changed index is passed as the command parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the command on ItemCheck event. If not set, the changed item is used.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CommandParameter
        {
            get => _commandParameter;
            set => _commandParameter = value;
        }

        private void CheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_command != null && e.Index >= 0 && e.Index < Items.Count)
            {
                var item = Items[e.Index];
                var parameter = _commandParameter ?? item;

                if (_command.CanExecute(parameter))
                {
                    _command.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableCheckedListBoxWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableCheckedListBoxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;

            return this;
        }
    }
}
