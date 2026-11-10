using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.Enums;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class SwitchTabCommandTests
    {
        [Fact]
        public void Constructor_NullTabControl_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SwitchTabCommand(null!));
        }

        [Fact]
        public void CanExecute_NullOrInvalidParameter_ReturnsFalse()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            using var command = new SwitchTabCommand(tabControl);

            Assert.False(command.CanExecute(null));
            Assert.False(command.CanExecute("invalid"));
            Assert.False(command.CanExecute(123));
        }

        [Fact]
        public void CanExecute_EmptyTabControl_ReturnsFalse()
        {
            using var tabControl = new TabControl();
            using var command = new SwitchTabCommand(tabControl);

            Assert.False(command.CanExecute(SwitchTabDirection.Next));
            Assert.False(command.CanExecute(SwitchTabDirection.Previous));
        }

        [Fact]
        public void CanExecute_Next_WhenNotLastTab_ReturnsTrue()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 0;

            using var command = new SwitchTabCommand(tabControl);

            Assert.True(command.CanExecute(SwitchTabDirection.Next));
        }

        [Fact]
        public void CanExecute_Next_WhenLastTab_ReturnsFalse()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 1;

            using var command = new SwitchTabCommand(tabControl);

            Assert.False(command.CanExecute(SwitchTabDirection.Next));
        }

        [Fact]
        public void CanExecute_Previous_WhenFirstTab_ReturnsFalse()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 0;

            using var command = new SwitchTabCommand(tabControl);

            Assert.False(command.CanExecute(SwitchTabDirection.Previous));
        }

        [Fact]
        public void CanExecute_Previous_WhenNotFirstTab_ReturnsTrue()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 1;

            using var command = new SwitchTabCommand(tabControl);

            Assert.True(command.CanExecute(SwitchTabDirection.Previous));
        }

        [Fact]
        public void Execute_Next_AdvancesSelectedIndex()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 0;

            using var command = new SwitchTabCommand(tabControl);

            command.Execute(SwitchTabDirection.Next);

            Assert.Equal(1, tabControl.SelectedIndex);
        }

        [Fact]
        public void Execute_Previous_DecrementsSelectedIndex()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 1;

            using var command = new SwitchTabCommand(tabControl);

            command.Execute(SwitchTabDirection.Previous);

            Assert.Equal(0, tabControl.SelectedIndex);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNotChangeIndex()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.SelectedIndex = 0;

            using var command = new SwitchTabCommand(tabControl);

            command.Execute(SwitchTabDirection.Next);
            Assert.Equal(0, tabControl.SelectedIndex);

            command.Execute(SwitchTabDirection.Previous);
            Assert.Equal(0, tabControl.SelectedIndex);
        }

        [Fact]
        public void CanExecuteChanged_FiresOnTabControlEvents()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            using var command = new SwitchTabCommand(tabControl);

            int eventCount = 0;
            command.CanExecuteChanged += (_, _) => eventCount++;

            tabControl.SelectedIndex = 1;
            Assert.True(eventCount > 0);

            int countBeforeAdd = eventCount;
            var newPage = new TabPage("Tab 3");
            tabControl.TabPages.Add(newPage);
            Assert.True(eventCount > countBeforeAdd);

            int countBeforeRemove = eventCount;
            tabControl.TabPages.Remove(newPage);
            Assert.True(eventCount > countBeforeRemove);
        }

        [Fact]
        public void Dispose_PreventsExecutionAndDetaches()
        {
            using var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");
            tabControl.SelectedIndex = 0;

            var command = new SwitchTabCommand(tabControl);
            Assert.True(command.CanExecute(SwitchTabDirection.Next));

            command.Dispose();

            Assert.False(command.CanExecute(SwitchTabDirection.Next));
            command.Execute(SwitchTabDirection.Next);
            Assert.Equal(0, tabControl.SelectedIndex);
        }

        [Fact]
        public void TabControlDisposed_DisposesCommand()
        {
            var tabControl = new TabControl();
            tabControl.TabPages.Add("Tab 1");
            tabControl.TabPages.Add("Tab 2");

            var command = new SwitchTabCommand(tabControl);
            tabControl.Dispose();

            Assert.False(command.CanExecute(SwitchTabDirection.Next));
        }
    }
}
