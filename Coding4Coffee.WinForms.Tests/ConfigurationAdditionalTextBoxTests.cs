using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public class ConfigurationAdditionalTextBoxTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextChanges_AreDebouncedAndExternalChangesTakePrecedence(bool masked)
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            config.Value = "first";
            using var control = Create(masked);
            SetKey(control, config.Key);
            SetInterval(control, 400);
            _ = control.Handle;
            Assert.Equal("first", control.Text);
            control.Text = "one";
            PumpFor(220);
            Assert.Equal("first", config.Value);
            control.Text = "two";
            PumpFor(220);
            Assert.Equal("first", config.Value);
            PumpUntil(() => config.Value == "two");
            control.Text = "pending";
            config.Value = "external";
            Assert.Equal("external", control.Text);
            PumpFor(450);
            Assert.Equal("external", config.Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Binding_RebindDisposeAndInvalidKeyPreserveValues(bool masked)
        {
            var first = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            var second = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            var invalid = $"Text_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<int>(invalid, isTransient: true);
            first.Value = "first";
            second.Value = "second";
            using var control = Create(masked);
            SetKey(control, first.Key);
            control.Text = "edit";
            Assert.Throws<InvalidOperationException>(() => SetKey(control, invalid));
            SetKey(control, second.Key);
            Assert.Equal("edit", first.Value);
            Assert.Equal("second", control.Text);
            control.Text = "last";
            control.Dispose();
            Assert.Equal("last", second.Value);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void BackgroundUpdates_AreAppliedOnOwnerThreadAndAfterHandleRecreation(bool masked)
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            config.Value = "initial";
            using var control = Create(masked);
            SetKey(control, config.Key);
            int owner = Environment.CurrentManagedThreadId;
            var threads = new List<int>();
            control.TextChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);
            RunOnWorker(() => config.Value = "before handle");
            Assert.Empty(threads);
            _ = control.Handle;
            Assert.Equal("before handle", control.Text);
            RunOnWorker(() => config.Value = "after handle");
            Assert.Equal("before handle", control.Text);
            Application.DoEvents();
            Assert.Equal("after handle", control.Text);
            Assert.NotEmpty(threads);
            Assert.All(threads, id => Assert.Equal(owner, id));
            control.Text = "pending";
            RunOnWorker(() => config.Value = "queued");
            SetKey(control, null);
            Application.DoEvents();
            Assert.Equal("queued", config.Value);
            Assert.Equal("pending", control.Text);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void DesignerInitializationAndIntervalAreSupported(bool masked)
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            config.Value = "saved";
            using var control = Create(masked);
            Assert.Equal(300, GetInterval(control));
            SetInterval(control, 0);
            Assert.Equal(50, GetInterval(control));
            var property = TypeDescriptor.GetProperties(control)["ConfigKey"]!;
            Assert.True(property.IsBrowsable);
            Assert.Equal(DesignerSerializationVisibility.Visible, property.SerializationVisibility);
            var init = (ISupportInitialize)control;
            init.BeginInit();
            SetKey(control, config.Key);
            Assert.NotEqual("saved", control.Text);
            init.EndInit();
            Assert.Equal("saved", control.Text);
            using var designControl = Create(masked);
            designControl.Site = new DesignSite(designControl);
            var invalid = $"Text_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<int>(invalid, isTransient: true);
            SetKey(designControl, invalid);
        }

        [Fact]
        public void MaskedTextBox_StoresTextUsingTextMaskFormat()
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            config.Value = "1234";
            using var control = new ConfigurationMaskedTextBox { Mask = "00-00", TextMaskFormat = MaskFormat.ExcludePromptAndLiterals, ConfigKey = config.Key, DebounceInterval = 50 };
            _ = control.Handle;
            Assert.Equal("1234", control.Text);
            control.Text = "5678";
            PumpUntil(() => config.Value == "5678");
            control.TextMaskFormat = MaskFormat.IncludeLiterals;
            control.Text = "9012";
            PumpUntil(() => config.Value == "90-12");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void NullConfiguration_IsNotWrittenBackAsEmptyText(bool masked)
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            using var control = Create(masked);
            control.Text = "previous";
            SetInterval(control, 50);
            SetKey(control, config.Key);
            _ = control.Handle;
            Assert.Equal(string.Empty, control.Text);
            PumpFor(150);
            Assert.Null(config.Value);
        }

        [Fact]
        public void MaskedTextBox_InvalidStoredTextIsNotOverwritten()
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            config.Value = "invalid";
            using var control = new ConfigurationMaskedTextBox { Mask = "0000", ConfigKey = config.Key, DebounceInterval = 50 };
            _ = control.Handle;
            PumpFor(150);
            Assert.Equal("invalid", config.Value);
        }

        [Fact]
        public void RichTextBox_PersistsPlainTextNotRtf()
        {
            var config = ConfigFactory.GetConfig<string>($"Text_{Guid.NewGuid()}", isTransient: true);
            using var control = new ConfigurationRichTextBox { ConfigKey = config.Key, DebounceInterval = 50 };
            _ = control.Handle;
            control.Rtf = @"{\rtf1\ansi {\b Hello} world}";
            PumpUntil(() => config.Value == "Hello world");
            Assert.Equal("Hello world", config.Value);
        }

        private static TextBoxBase Create(bool masked) => masked ? new ConfigurationMaskedTextBox() : new ConfigurationRichTextBox();
        private static void SetKey(TextBoxBase control, string? key)
        {
            if (control is ConfigurationMaskedTextBox masked)
                masked.ConfigKey = key;
            else
                ((ConfigurationRichTextBox)control).ConfigKey = key;
        }
        private static void SetInterval(TextBoxBase control, int value)
        {
            if (control is ConfigurationMaskedTextBox masked)
                masked.DebounceInterval = value;
            else
                ((ConfigurationRichTextBox)control).DebounceInterval = value;
        }
        private static int GetInterval(TextBoxBase control) => control is ConfigurationMaskedTextBox masked ? masked.DebounceInterval : ((ConfigurationRichTextBox)control).DebounceInterval;
        private static void PumpFor(int milliseconds)
        {
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            while (elapsed.ElapsedMilliseconds < milliseconds)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
        }
        private static void PumpUntil(Func<bool> condition)
        {
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            while (!condition() && elapsed.ElapsedMilliseconds < 5000)
            {
                Application.DoEvents();
                Thread.Sleep(5);
            }
            Assert.True(condition(), "The debounced text was not stored before the timeout.");
        }
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
    }
}
