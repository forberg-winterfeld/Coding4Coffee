using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Coding4Coffee.WinForms.Commands.Controls;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class LocalizableDataGridViewWithCommandSupportTests
    {
        [Fact]
        public void CellValueChanged_UsesChangedCellAfterSelectionAndRowOrderChange()
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            var changedCell = grid.Rows[0].Cells[0];
            changedCell.Value = "changed";
            grid.CurrentCell = grid.Rows[1].Cells[0];
            grid.Rows.Insert(0, "inserted");

            PumpUntil(() => command.Parameters.Count == 1);

            Assert.Equal("changed", Assert.Single(command.Parameters));
            Assert.Same(grid.Rows[1].Cells[0], changedCell);
        }

        [Fact]
        public void CellValueChanged_CoalescesChangesAndUsesTheLastChangedCell()
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = "first";
            grid.Rows[1].Cells[0].Value = "last";
            grid.CurrentCell = grid.Rows[0].Cells[0];

            PumpUntil(() => command.Parameters.Count == 1);
            PumpFor(150);

            Assert.Equal("last", Assert.Single(command.Parameters));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CellValueChanged_RemovedRowOrColumnDiscardsPendingExecution(bool removeRow)
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = "pending";
            if (removeRow)
                grid.Rows.RemoveAt(0);
            else
                grid.Columns.RemoveAt(0);

            PumpFor(250);
            Assert.Empty(command.Parameters);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CellValueChanged_ReplacingOrRemovingCommandDiscardsPendingExecution(bool replace)
        {
            using var grid = CreateGrid();
            var original = new RecordingCommand();
            var replacement = new RecordingCommand();
            grid.CellValueChangedCommand = original;
            grid.Rows[0].Cells[0].Value = "pending";
            grid.CellValueChangedCommand = replace ? replacement : null;

            PumpFor(250);
            Assert.Empty(original.Parameters);
            Assert.Empty(replacement.Parameters);

            grid.CellValueChangedCommand = replacement;
            grid.Rows[1].Cells[0].Value = "new";
            PumpUntil(() => replacement.Parameters.Count == 1);
            Assert.Equal("new", Assert.Single(replacement.Parameters));
        }

        [Fact]
        public void CellValueChanged_AssigningSameCommandPreservesPendingExecution()
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = "pending";
            grid.CellValueChangedCommand = command;

            PumpUntil(() => command.Parameters.Count == 1);
            Assert.Equal("pending", Assert.Single(command.Parameters));
        }

        [Fact]
        public void CellValueChanged_WithoutCommandDoesNotScheduleExecutionForNewCommand()
        {
            using var grid = CreateGrid();
            grid.Rows[0].Cells[0].Value = "unbound";
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;

            PumpFor(250);
            Assert.Empty(command.Parameters);
        }

        [Fact]
        public void CellValueChanged_UsesExplicitParameterAtExecutionTime()
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = "changed";
            var parameter = new object();
            grid.CellValueChangedCommandParameter = parameter;

            PumpUntil(() => command.Parameters.Count == 1);
            Assert.Same(parameter, Assert.Single(command.Parameters));
        }

        [Fact]
        public void CellValueChanged_NullValueFallsBackToBoundItem()
        {
            using var grid = new LocalizableDataGridViewWithCommandSupport { CellValueChangedDebounceInterval = 50 };
            var item = new RowData { Value = "initial" };
            grid.BindingContext = new BindingContext();
            grid.DataSource = new BindingList<RowData> { item };
            _ = grid.Handle;
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = null;

            PumpUntil(() => command.Parameters.Count == 1);
            Assert.Same(item, Assert.Single(command.Parameters));
        }

        [Fact]
        public void CellValueChanged_DisposeDiscardsPendingExecution()
        {
            using var grid = CreateGrid();
            var command = new RecordingCommand();
            grid.CellValueChangedCommand = command;
            grid.Rows[0].Cells[0].Value = "pending";
            grid.Dispose();
            grid.Dispose();

            PumpFor(250);
            Assert.Empty(command.Parameters);
        }

        private static LocalizableDataGridViewWithCommandSupport CreateGrid()
        {
            var grid = new LocalizableDataGridViewWithCommandSupport
            {
                AllowUserToAddRows = false,
                CellValueChangedDebounceInterval = 50
            };
            grid.Columns.Add("value", "Value");
            grid.Rows.Add("first");
            grid.Rows.Add("second");
            _ = grid.Handle;
            return grid;
        }

        private static void PumpFor(int milliseconds)
        {
            var elapsed = Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }

        private static void PumpUntil(Func<bool> condition)
        {
            var elapsed = Stopwatch.StartNew();
            while (!condition() && elapsed.ElapsedMilliseconds < 5000)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
            Assert.True(condition(), "The debounced command did not execute before the timeout.");
        }

        private sealed class RecordingCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged { add { } remove { } }
            public List<object?> Parameters { get; } = [];
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => Parameters.Add(parameter);
        }

        public sealed class RowData
        {
            public string? Value { get; set; }
        }
    }
}
