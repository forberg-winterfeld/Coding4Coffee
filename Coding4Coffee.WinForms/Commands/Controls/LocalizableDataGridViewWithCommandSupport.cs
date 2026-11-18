using Coding4Coffee.WinForms.Localization.Controls;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable DataGridView control that integrates comprehensive command binding support for all major user interactions.
    /// </summary>
    /// <remarks>
    /// Provides seven independent command bindings for different DataGridView scenarios:
    /// 1. <see cref="CellClickCommand"/> - Single-click on a cell
    /// 2. <see cref="CellDoubleClickCommand"/> - Double-click on a cell
    /// 3. <see cref="CellValueChangedCommand"/> - After cell value changes (with optional debounce)
    /// 4. <see cref="CellBeginEditCommand"/> - Before editing begins (cancellable)
    /// 5. <see cref="CellEndEditCommand"/> - After editing, before commit (cancellable)
    /// 6. <see cref="SelectionChangedCommand"/> - When selection changes
    /// 7. <see cref="RowHeaderClickCommand"/> - Row header click (typically selects entire row)
    /// 
    /// Inherits from <see cref="LocalizableDataGridView"/>, meaning dynamic localization applies to column headers at runtime.
    /// Grid-based data editing scenarios benefit from this comprehensive command structure.
    /// </remarks>
    public class LocalizableDataGridViewWithCommandSupport : LocalizableDataGridView
    {
        private readonly System.Windows.Forms.Timer _cellValueChangedDebounceTimer;

        #region Command Fields
        private ICommand? _cellClickCommand;
        private object? _cellClickCommandParameter;

        private ICommand? _cellDoubleClickCommand;
        private object? _cellDoubleClickCommandParameter;

        private ICommand? _cellValueChangedCommand;
        private object? _cellValueChangedCommandParameter;

        private ICommand? _cellBeginEditCommand;
        private object? _cellBeginEditCommandParameter;

        private ICommand? _cellEndEditCommand;
        private object? _cellEndEditCommandParameter;

        private ICommand? _selectionChangedCommand;
        private object? _selectionChangedCommandParameter;

        private ICommand? _rowHeaderClickCommand;
        private object? _rowHeaderClickCommandParameter;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableDataGridViewWithCommandSupport"/> class.
        /// </summary>
        public LocalizableDataGridViewWithCommandSupport()
        {
            _cellValueChangedDebounceTimer = new System.Windows.Forms.Timer
            {
                Interval = 300 // Standard: 300 ms delay
            };
            _cellValueChangedDebounceTimer.Tick += CellValueChangedDebounceTimer_Tick;

            // Register events
            CellClick += DataGridView_CellClick;
            CellDoubleClick += DataGridView_CellDoubleClick;
            CellValueChanged += DataGridView_CellValueChanged;
            CellBeginEdit += DataGridView_CellBeginEdit;
            CellEndEdit += DataGridView_CellEndEdit;
            SelectionChanged += DataGridView_SelectionChanged;
            RowHeaderMouseClick += DataGridView_RowHeaderMouseClick;
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds before the <see cref="CellValueChangedCommand"/> executes after a value change.
        /// </summary>
        /// <remarks>
        /// Any value smaller than 50 milliseconds is clamped to 50 milliseconds to maintain responsive UI performance.
        /// Set to exactly 50 to disable the debounce delay. Negative values are clamped to 50.
        /// </remarks>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The delay in milliseconds for cell value change debounce. Minimum is 50 ms.")]
        [DefaultValue(300)]
        public int CellValueChangedDebounceInterval
        {
            get => _cellValueChangedDebounceTimer.Interval;
            set => _cellValueChangedDebounceTimer.Interval = Math.Max(50, value);
        }

        #region CellClick Command
        /// <summary>
        /// Gets or sets the command executed when a cell is clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.CellClick"/> event.
        /// Useful for context menus, detail views, or selection-based actions.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? CellClickCommand
        {
            get => _cellClickCommand;
            set => _cellClickCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="CellClickCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the clicked cell's value is used; if that is null, the parent row's <see cref="DataGridViewRow.DataBoundItem"/> is used.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the CellClickCommand on CellClick event. Defaults to Cell.Value or Row.DataBoundItem.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CellClickCommandParameter
        {
            get => _cellClickCommandParameter;
            set => _cellClickCommandParameter = value;
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_cellClickCommand != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var parameter = _cellClickCommandParameter ?? cell.Value ?? Rows[e.RowIndex].DataBoundItem;

                if (_cellClickCommand.CanExecute(parameter))
                {
                    _cellClickCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a cell click command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindCellClickCommand(ICommand command, object? parameter = null)
        {
            CellClickCommandParameter = parameter;
            CellClickCommand = command;
            return this;
        }
        #endregion

        #region CellDoubleClick Command
        /// <summary>
        /// Gets or sets the command executed when a cell is double-clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.CellDoubleClick"/> event, which occurs after CellClick but before <see cref="DataGridView.CellBeginEdit"/>.
        /// Useful for opening detail views, inline editors, or confirming selections.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? CellDoubleClickCommand
        {
            get => _cellDoubleClickCommand;
            set => _cellDoubleClickCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="CellDoubleClickCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the double-clicked cell's value is used; if that is null, the parent row's <see cref="DataGridViewRow.DataBoundItem"/> is used.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the CellDoubleClickCommand on CellDoubleClick event. Defaults to Cell.Value or Row.DataBoundItem.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CellDoubleClickCommandParameter
        {
            get => _cellDoubleClickCommandParameter;
            set => _cellDoubleClickCommandParameter = value;
        }

        private void DataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_cellDoubleClickCommand != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var parameter = _cellDoubleClickCommandParameter ?? cell.Value ?? Rows[e.RowIndex].DataBoundItem;

                if (_cellDoubleClickCommand.CanExecute(parameter))
                {
                    _cellDoubleClickCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a cell double-click command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindCellDoubleClickCommand(ICommand command, object? parameter = null)
        {
            CellDoubleClickCommandParameter = parameter;
            CellDoubleClickCommand = command;
            return this;
        }
        #endregion

        #region CellValueChanged Command
        /// <summary>
        /// Gets or sets the command executed after a cell value changes.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.CellValueChanged"/> event after <see cref="CellValueChangedDebounceInterval"/> milliseconds of inactivity.
        /// This provides deferred value persistence, similar to TextBox debouncing. Fires after <see cref="CellEndEditCommand"/>.
        /// Set <see cref="CellValueChangedDebounceInterval"/> to 0 for immediate execution.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? CellValueChangedCommand
        {
            get => _cellValueChangedCommand;
            set => _cellValueChangedCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="CellValueChangedCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the changed cell's value is used; if that is null, the parent row's <see cref="DataGridViewRow.DataBoundItem"/> is used.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the CellValueChangedCommand on CellValueChanged event (debounced). Defaults to Cell.Value or Row.DataBoundItem.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CellValueChangedCommandParameter
        {
            get => _cellValueChangedCommandParameter;
            set => _cellValueChangedCommandParameter = value;
        }

        private void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            // Restart debounce timer on every value change
            _cellValueChangedDebounceTimer.Stop();
            if (CellValueChangedDebounceInterval > 0)
            {
                _cellValueChangedDebounceTimer.Start();
            }
            else
            {
                // Execute immediately if debounce is disabled
                ExecuteCellValueChangedCommand(e);
            }
        }

        private void CellValueChangedDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _cellValueChangedDebounceTimer.Stop();

            if (CurrentCell != null)
            {
                ExecuteCellValueChangedCommand(new DataGridViewCellEventArgs(CurrentCell.ColumnIndex, CurrentCell.RowIndex));
            }
        }

        private void ExecuteCellValueChangedCommand(DataGridViewCellEventArgs e)
        {
            if (_cellValueChangedCommand != null && e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.RowIndex < Rows.Count)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var parameter = _cellValueChangedCommandParameter ?? cell.Value ?? Rows[e.RowIndex].DataBoundItem;

                if (_cellValueChangedCommand.CanExecute(parameter))
                {
                    _cellValueChangedCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a cell value changed command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindCellValueChangedCommand(ICommand command, object? parameter = null)
        {
            CellValueChangedCommandParameter = parameter;
            CellValueChangedCommand = command;
            return this;
        }
        #endregion

        #region CellBeginEdit Command
        /// <summary>
        /// Gets or sets the command executed before a cell enters edit mode.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.CellBeginEdit"/> event BEFORE the cell editor appears.
        /// This is the pre-validation point. The command can inspect the cell and backing data.
        /// If the command's execution indicates failure (via <see cref="ICommand.CanExecute(object?)"/>), the edit can be prevented.
        /// Useful for pre-edit state preservation, readonly checks, or dependency validation.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? CellBeginEditCommand
        {
            get => _cellBeginEditCommand;
            set => _cellBeginEditCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="CellBeginEditCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the cell being edited is used as the parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the CellBeginEditCommand on CellBeginEdit event (before edit begins). Defaults to the DataGridViewCell.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CellBeginEditCommandParameter
        {
            get => _cellBeginEditCommandParameter;
            set => _cellBeginEditCommandParameter = value;
        }

        private void DataGridView_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (_cellBeginEditCommand != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var parameter = _cellBeginEditCommandParameter ?? cell;

                // If command cannot execute, cancel the edit
                if (!_cellBeginEditCommand.CanExecute(parameter))
                {
                    e.Cancel = true;
                    return;
                }

                _cellBeginEditCommand.Execute(parameter);
            }
        }

        /// <summary>
        /// Binds a cell begin edit command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindCellBeginEditCommand(ICommand command, object? parameter = null)
        {
            CellBeginEditCommandParameter = parameter;
            CellBeginEditCommand = command;
            return this;
        }
        #endregion

        #region CellEndEdit Command
        /// <summary>
        /// Gets or sets the command executed after a cell finishes editing but BEFORE the value is committed.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.CellEndEdit"/> event AFTER the user exits edit mode (presses Enter, Tab, or loses focus)
        /// but BEFORE the new value is committed to the cell. This is the post-edit validation point.
        /// If the command cannot execute (via <see cref="ICommand.CanExecute(object?)"/>), the edit can be rejected and the value reverted.
        /// Useful for input validation, data transformation, or consistency checks before committing.
        /// Note: Unlike <see cref="CellValueChangedCommand"/>, this fires on every edit exit, even if the value did not actually change.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? CellEndEditCommand
        {
            get => _cellEndEditCommand;
            set => _cellEndEditCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="CellEndEditCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the cell being edited is used as the parameter.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the CellEndEditCommand on CellEndEdit event (after edit, before commit). Defaults to the DataGridViewCell.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? CellEndEditCommandParameter
        {
            get => _cellEndEditCommandParameter;
            set => _cellEndEditCommandParameter = value;
        }

        private void DataGridView_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (_cellEndEditCommand != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                var parameter = _cellEndEditCommandParameter ?? cell;

                if (_cellEndEditCommand.CanExecute(parameter))
                {
                    _cellEndEditCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a cell end edit command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindCellEndEditCommand(ICommand command, object? parameter = null)
        {
            CellEndEditCommandParameter = parameter;
            CellEndEditCommand = command;
            return this;
        }
        #endregion

        #region SelectionChanged Command
        /// <summary>
        /// Gets or sets the command executed when the grid's selection changes.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.SelectionChanged"/> event.
        /// Fires whenever any cell, row, or column selection changes. Useful for enabling/disabling dependent controls,
        /// loading related data, or updating status displays based on the current selection.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? SelectionChangedCommand
        {
            get => _selectionChangedCommand;
            set => _selectionChangedCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="SelectionChangedCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the currently selected cell (or first selected row if row selection) is used.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the SelectionChangedCommand on SelectionChanged event. Defaults to CurrentCell or first SelectedRow.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? SelectionChangedCommandParameter
        {
            get => _selectionChangedCommandParameter;
            set => _selectionChangedCommandParameter = value;
        }

        private void DataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (_selectionChangedCommand != null)
            {
                var parameter = _selectionChangedCommandParameter;

                // Auto-detect selection if no explicit parameter set
                if (parameter == null)
                {
                    if (CurrentCell != null)
                        parameter = CurrentCell;
                    else if (SelectedRows.Count > 0)
                        parameter = SelectedRows[0];
                }

                if (_selectionChangedCommand.CanExecute(parameter))
                {
                    _selectionChangedCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a selection changed command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindSelectionChangedCommand(ICommand command, object? parameter = null)
        {
            SelectionChangedCommandParameter = parameter;
            SelectionChangedCommand = command;
            return this;
        }
        #endregion

        #region RowHeaderClick Command
        /// <summary>
        /// Gets or sets the command executed when a row header is clicked.
        /// </summary>
        /// <remarks>
        /// Executed in response to <see cref="DataGridView.RowHeaderMouseClick"/> event.
        /// Standard DataGridView behavior selects the entire row automatically. This command fires during that selection,
        /// allowing you to perform additional actions like context menu display or bulk operation triggers.
        /// Right-click row header clicks can be distinguished by button detection in subclassed handlers if needed.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ICommand? RowHeaderClickCommand
        {
            get => _rowHeaderClickCommand;
            set => _rowHeaderClickCommand = value;
        }

        /// <summary>
        /// Gets or sets the optional parameter for <see cref="RowHeaderClickCommand"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="null"/>, the clicked row's <see cref="DataGridViewRow.DataBoundItem"/> is used; if that is null, the row itself is used.
        /// </remarks>
        [Browsable(true)]
        [Category("Data")]
        [Description("The parameter to pass to the RowHeaderClickCommand on RowHeaderMouseClick event. Defaults to Row.DataBoundItem or Row.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public object? RowHeaderClickCommandParameter
        {
            get => _rowHeaderClickCommandParameter;
            set => _rowHeaderClickCommandParameter = value;
        }

        private void DataGridView_RowHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (_rowHeaderClickCommand != null && e.RowIndex >= 0)
            {
                var row = Rows[e.RowIndex];
                var parameter = _rowHeaderClickCommandParameter ?? row.DataBoundItem ?? row;

                if (_rowHeaderClickCommand.CanExecute(parameter))
                {
                    _rowHeaderClickCommand.Execute(parameter);
                }
            }
        }

        /// <summary>
        /// Binds a row header click command and optional parameter to the control.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter value for the command.</param>
        /// <returns>The current <see cref="LocalizableDataGridViewWithCommandSupport"/> instance to support fluent method chaining.</returns>
        public LocalizableDataGridViewWithCommandSupport BindRowHeaderClickCommand(ICommand command, object? parameter = null)
        {
            RowHeaderClickCommandParameter = parameter;
            RowHeaderClickCommand = command;
            return this;
        }
        #endregion

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cellValueChangedDebounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
