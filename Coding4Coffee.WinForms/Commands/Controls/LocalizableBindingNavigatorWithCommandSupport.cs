using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable BindingNavigator control that integrates command binding support for navigation events.
    /// </summary>
    /// <remarks>
    /// Provides two independent command bindings for BindingNavigator scenarios:
    /// 1. <see cref="ItemClickedCommand"/> - When any navigator button/item is clicked
    /// 2. <see cref="PositionChangedCommand"/> - When the BindingSource position changes
    /// 
    /// Inherits from <see cref="LocalizableBindingNavigator"/>, meaning dynamic localization applies to navigator items at runtime.
    /// </remarks>
    public class LocalizableBindingNavigatorWithCommandSupport : LocalizableBindingNavigator
    {
        #region Command Fields
        private ICommand? _itemClickedCommand;
        private object? _itemClickedCommandParameter;

        private ICommand? _positionChangedCommand;
        private object? _positionChangedCommandParameter;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableBindingNavigatorWithCommandSupport"/> class.
        /// </summary>
        public LocalizableBindingNavigatorWithCommandSupport()
        {
            // Register events
            ItemClicked += BindingNavigator_ItemClicked;
            BindingSource?.PositionChanged += BindingSource_PositionChanged;
        }

        #region ItemClicked Command
        /// <summary>
        /// Gets or sets the command executed when a navigator item or button is clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="ToolStrip.ItemClicked"/> event.
        /// Useful for reacting to specific navigation button clicks or other navigator toolbar actions.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? ItemClickedCommand
        {
            get => _itemClickedCommand;
            set => _itemClickedCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="ItemClickedCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the clicked ToolStripItem is used as the parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the ItemClickedCommand on ItemClicked event. Defaults to the ToolStripItem.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? ItemClickedCommandParameter
        {
            get => _itemClickedCommandParameter;
            set => _itemClickedCommandParameter = value;
        }

        private void BindingNavigator_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (_itemClickedCommand != null)
            {
                var parameter = _itemClickedCommandParameter ?? e.ClickedItem;

                if (_itemClickedCommand.CanExecute(parameter))
                    _itemClickedCommand.Execute(parameter);
            }
        }

        /// <summary>
        /// Binds an item clicked command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableBindingNavigatorWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableBindingNavigatorWithCommandSupport BindItemClickedCommand(ICommand command, object? parameter = null)
        {
            ItemClickedCommandParameter = parameter;
            ItemClickedCommand = command;
            return this;
        }
        #endregion

        #region PositionChanged Command
        /// <summary>
        /// Gets or sets the command executed when the BindingSource position changes.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="BindingSource.PositionChanged"/> event.
        /// Useful for data-aware actions tied to navigation, such as loading related data or updating dependent controls.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? PositionChangedCommand
        {
            get => _positionChangedCommand;
            set => _positionChangedCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="PositionChangedCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the current position index is used as the parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the PositionChangedCommand on PositionChanged event. Defaults to the current Position.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? PositionChangedCommandParameter
        {
            get => _positionChangedCommandParameter;
            set => _positionChangedCommandParameter = value;
        }

        private void BindingSource_PositionChanged(object? sender, EventArgs e)
        {
            if (_positionChangedCommand != null && BindingSource != null)
            {
                var parameter = _positionChangedCommandParameter ?? BindingSource.Position;

                if (_positionChangedCommand.CanExecute(parameter))
                    _positionChangedCommand.Execute(parameter);
            }
        }

        /// <summary>
        /// Binds a position changed command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableBindingNavigatorWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableBindingNavigatorWithCommandSupport BindPositionChangedCommand(ICommand command, object? parameter = null)
        {
            PositionChangedCommandParameter = parameter;
            PositionChangedCommand = command;
            return this;
        }
        #endregion

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                BindingSource?.PositionChanged -= BindingSource_PositionChanged;

            base.Dispose(disposing);
        }
    }
}
