using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public partial class ConfigurationDateAndRadioTests
    {
        [Fact]
        public void DatePicker_NullableValueAndCheckboxSynchronizeBothDirections()
        {
            var config = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            using var picker = new ConfigurationDateTimePicker { ShowCheckBox = true, ConfigKey = config.Key };
            _ = picker.Handle;
            Assert.False(picker.Checked);
            Assert.Null(config.Value);
            var date = new DateTime(2025, 6, 12, 14, 30, 0);
            config.Value = date;
            Assert.True(picker.Checked);
            Assert.Equal(date, picker.Value);
            picker.Checked = false;
            Assert.Null(config.Value);
            picker.Checked = true;
            Assert.Equal(date, config.Value);
            var next = date.AddDays(1);
            picker.Value = next;
            Assert.Equal(next, config.Value);
        }

        [Fact]
        public void DatePicker_NativeCheckboxInputUpdatesConfiguration()
        {
            var config = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            config.Value = new DateTime(2025, 6, 12);
            using var form = new Form();
            using var picker = new ConfigurationDateTimePicker { ShowCheckBox = true, ConfigKey = config.Key };
            form.Controls.Add(picker);
            form.Show();
            picker.Focus();
            Application.DoEvents();
            nint point = (nint)(8 | ((picker.ClientSize.Height / 2) << 16));
            SendMessage(picker.Handle, 0x0201, (nint)1, point);
            SendMessage(picker.Handle, 0x0202, nint.Zero, point);
            Application.DoEvents();
            Assert.False(picker.Checked);
            Assert.Null(config.Value);
        }

        [Fact]
        public void DatePicker_ClampsWithoutOverwritingAndRestoresWhenRangeExpands()
        {
            var config = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            var date = new DateTime(2030, 1, 1);
            config.Value = date;
            using var picker = new ConfigurationDateTimePicker { MinDate = new DateTime(2020, 1, 1), MaxDate = new DateTime(2029, 1, 1), ConfigKey = config.Key };
            Assert.Equal(picker.MaxDate, picker.Value);
            Assert.Equal(date, config.Value);
            picker.MaxDate = date.AddYears(1);
            Assert.Equal(date, picker.Value);
            Assert.Equal(date, config.Value);
            picker.ShowCheckBox = true;
            config.Value = null;
            Assert.False(picker.Checked);
            picker.ShowCheckBox = false;
            Assert.Null(config.Value);
        }

        [Fact]
        public void RadioGroup_SynchronizesStableTagsAndHandlesDynamicChildren()
        {
            var config = ConfigFactory.GetConfig<string>($"Radio_{Guid.NewGuid()}", isTransient: true);
            config.Value = "dark";
            using var group = new ConfigurationRadioButtonGroup { ConfigKey = config.Key };
            using var light = new LocalizableRadioButton { Tag = "light", Text = "Light" };
            using var dark = new LocalizableRadioButton { Tag = "dark", Text = "Dark" };
            group.Controls.Add(light);
            Assert.False(light.Checked);
            group.Controls.Add(dark);
            Assert.True(dark.Checked);
            group.Localize();
            Assert.True(dark.Checked);
            Assert.Equal("dark", config.Value);
            light.Checked = true;
            Assert.Equal("light", config.Value);
            Assert.False(dark.Checked);
            config.Value = "unknown";
            Assert.False(light.Checked);
            Assert.Equal("unknown", config.Value);
            dark.Tag = "unknown";
            group.RefreshSelection();
            Assert.True(dark.Checked);
            group.Controls.Remove(dark);
            Assert.Equal("unknown", config.Value);
            Assert.False(light.Checked);
            dark.Checked = false;
            dark.Checked = true;
            Assert.Equal("unknown", config.Value);
            config.Value = null;
            Assert.False(light.Checked);
        }

        [Fact]
        public void DesignerAndInitialization_DeferBindingUntilPropertiesAreSet()
        {
            var date = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            var radio = ConfigFactory.GetConfig<string>($"Radio_{Guid.NewGuid()}", isTransient: true);
            date.Value = new DateTime(2025, 1, 1);
            radio.Value = "a";
            using var picker = new ConfigurationDateTimePicker();
            using var group = new ConfigurationRadioButtonGroup();
            using var button = new RadioButton { Tag = "a" };
            ((ISupportInitialize)picker).BeginInit();
            picker.ConfigKey = date.Key;
            picker.ShowCheckBox = true;
            ((ISupportInitialize)picker).EndInit();
            ((ISupportInitialize)group).BeginInit();
            group.ConfigKey = radio.Key;
            group.Controls.Add(button);
            ((ISupportInitialize)group).EndInit();
            Assert.Equal(date.Value, picker.Value);
            Assert.True(picker.Checked);
            Assert.True(button.Checked);
            var invalid = $"Design_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<int>(invalid, isTransient: true);
            using var designPicker = new ConfigurationDateTimePicker();
            using var designGroup = new ConfigurationRadioButtonGroup();
            designPicker.Site = new DesignSite(designPicker);
            designGroup.Site = new DesignSite(designGroup);
            designPicker.ConfigKey = invalid;
            designGroup.ConfigKey = invalid;
            Assert.Equal(invalid, designPicker.ConfigKey);
            Assert.Equal(invalid, designGroup.ConfigKey);
            foreach (var type in new[] { typeof(ConfigurationDateTimePicker), typeof(ConfigurationRadioButtonGroup) })
            {
                var property = TypeDescriptor.GetProperties(type)["ConfigKey"]!;
                Assert.True(property.IsBrowsable);
                Assert.Equal(DesignerSerializationVisibility.Visible, property.SerializationVisibility);
            }
        }

        [Fact]
        public void InvalidKeysAndTags_ReportErrorsWithoutDiscardingBindings()
        {
            var date = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            var radio = ConfigFactory.GetConfig<string>($"Radio_{Guid.NewGuid()}", isTransient: true);
            var invalid = $"Invalid_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<int>(invalid, isTransient: true);
            using var picker = new ConfigurationDateTimePicker { ConfigKey = date.Key };
            using var group = new ConfigurationRadioButtonGroup { ConfigKey = radio.Key };
            using var button = new RadioButton();
            group.Controls.Add(button);
            Assert.Throws<InvalidOperationException>(() => picker.ConfigKey = invalid);
            Assert.Throws<InvalidOperationException>(() => group.ConfigKey = invalid);
            Assert.Equal(date.Key, picker.ConfigKey);
            Assert.Equal(radio.Key, group.ConfigKey);
            Assert.Throws<InvalidOperationException>(() => button.Checked = true);
            button.Tag = "valid";
            button.Checked = false;
            button.Checked = true;
            Assert.Equal("valid", radio.Value);
        }

        [Fact]
        public void WorkerUpdates_AreMarshaledAndBindingsCanBeRemoved()
        {
            var date = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            var radio = ConfigFactory.GetConfig<string>($"Radio_{Guid.NewGuid()}", isTransient: true);
            using var picker = new TestPicker { ShowCheckBox = true, ConfigKey = date.Key };
            using var group = new TestGroup();
            using var button = new RadioButton { Tag = "a" };
            group.Controls.Add(button);
            group.ConfigKey = radio.Key;
            var expected = new DateTime(2025, 2, 1);
            RunOnWorker(() => { date.Value = expected; radio.Value = "a"; });
            Assert.False(picker.Checked);
            Assert.False(button.Checked);
            _ = picker.Handle;
            _ = group.Handle;
            Assert.Equal(expected, picker.Value);
            Assert.True(button.Checked);
            RunOnWorker(() => { date.Value = expected.AddDays(1); radio.Value = null; });
            picker.RemoveHandle();
            group.RemoveHandle();
            _ = picker.Handle;
            _ = group.Handle;
            Assert.Equal(expected.AddDays(1), picker.Value);
            Assert.False(button.Checked);
            RunOnWorker(() => { date.Value = expected; radio.Value = "a"; });
            Assert.False(button.Checked);
            Application.DoEvents();
            Assert.Equal(expected, picker.Value);
            Assert.True(button.Checked);
            picker.ConfigKey = null;
            group.ConfigKey = null;
            date.Value = expected.AddDays(1);
            radio.Value = null;
            Assert.Equal(expected, picker.Value);
            Assert.True(button.Checked);
        }

        [Fact]
        public void DisposingControls_UnsubscribesFromQueuedAndFutureChanges()
        {
            var date = ConfigFactory.GetConfig<DateTime?>($"Date_{Guid.NewGuid()}", isTransient: true);
            var radio = ConfigFactory.GetConfig<string>($"Radio_{Guid.NewGuid()}", isTransient: true);
            var picker = new ConfigurationDateTimePicker { ConfigKey = date.Key };
            var group = new ConfigurationRadioButtonGroup { ConfigKey = radio.Key };
            _ = picker.Handle;
            _ = group.Handle;
            RunOnWorker(() => { date.Value = new DateTime(2025, 1, 1); radio.Value = "a"; });
            picker.Dispose();
            group.Dispose();
            Application.DoEvents();
            date.Value = new DateTime(2025, 1, 2);
            radio.Value = "b";
            Assert.Equal(new DateTime(2025, 1, 2), date.Value);
            Assert.Equal("b", radio.Value);
        }

        [System.Runtime.InteropServices.LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessage(nint handle, uint message, nint wParam, nint lParam);

        private static void RunOnWorker(Action action)
        {
            Exception? error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception exception) { error = exception; }
            });
            thread.Start();
            thread.Join();
            if (error != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }

        private sealed class DesignSite(IComponent component) : ISite
        {
            public IComponent Component => component;
            public IContainer? Container => null;
            public bool DesignMode => true;
            public string? Name { get; set; }
            public object? GetService(Type serviceType) => null;
        }

        private sealed class TestPicker : ConfigurationDateTimePicker
        {
            public void RemoveHandle() => DestroyHandle();
        }

        private sealed class TestGroup : ConfigurationRadioButtonGroup
        {
            public void RemoveHandle() => DestroyHandle();
        }
    }
}
