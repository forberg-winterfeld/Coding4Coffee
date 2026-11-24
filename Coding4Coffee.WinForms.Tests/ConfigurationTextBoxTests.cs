using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public class ConfigurationTextBoxTests
    {
        [Fact]
        public void ConfigurationChanges_AreAppliedOnOwnerThreadAcrossHandleLifecycle()
        {
            var key = $"TextBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "initial";
            using var textBox = new TestConfigurationTextBox { ConfigKey = key };
            var ownerThread = Environment.CurrentManagedThreadId;
            var changeThreads = new List<int>();
            textBox.TextChanged += (_, _) => changeThreads.Add(Environment.CurrentManagedThreadId);

            RunOnWorker(() => config.Value = "before handle");
            Assert.False(textBox.IsHandleCreated);
            Assert.Empty(changeThreads);

            _ = textBox.Handle;
            Assert.Equal("before handle", textBox.Text);

            RunOnWorker(() => config.Value = "with handle");
            Assert.Equal("before handle", textBox.Text);
            Application.DoEvents();
            Assert.Equal("with handle", textBox.Text);

            RunOnWorker(() => config.Value = "queued before handle destroyed");
            textBox.RemoveHandle();
            _ = textBox.Handle;
            Assert.Equal("queued before handle destroyed", textBox.Text);

            textBox.RemoveHandle();
            RunOnWorker(() => config.Value = "after handle destroyed");
            _ = textBox.Handle;
            Assert.Equal("after handle destroyed", textBox.Text);
            Assert.All(changeThreads, thread => Assert.Equal(ownerThread, thread));

            RunOnWorker(() => config.Value = "queued before unbinding");
            textBox.ConfigKey = null;
            Application.DoEvents();
            Assert.Equal("after handle destroyed", textBox.Text);
            textBox.ConfigKey = key;
            Assert.Equal("queued before unbinding", textBox.Text);

            RunOnWorker(() => config.Value = "queued before disposal");
            textBox.Dispose();
            Application.DoEvents();
            config.Value = "after disposal";
            Assert.Equal("after disposal", config.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t")]
        public void ConfigKey_NullOrWhitespace_UnbindsAndPreservesText(string? emptyKey)
        {
            var key = $"TextBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "initial";
            using var textBox = new ConfigurationTextBox { ConfigKey = key };
            Assert.Equal("initial", textBox.Text);

            textBox.ConfigKey = emptyKey;

            Assert.Equal(emptyKey, textBox.ConfigKey);
            Assert.Equal("initial", textBox.Text);
            Assert.Equal("initial", config.Value);

            config.Value = "changed after unbinding";
            Assert.Equal("initial", textBox.Text);
            textBox.Text = "unbound text";
            Assert.Equal("changed after unbinding", config.Value);

            textBox.ConfigKey = key;
            Assert.Equal("changed after unbinding", textBox.Text);
            textBox.Text = "rebound text";
            Assert.Equal("changed after unbinding", config.Value);
            textBox.ConfigKey = null;
            Assert.Equal("rebound text", config.Value);
        }

        [Fact]
        public void DebounceInterval_UsesDesignerDefaultsAndMinimum()
        {
            using var textBox = new ConfigurationTextBox();
            Assert.Equal(300, textBox.DebounceInterval);
            var property = System.ComponentModel.TypeDescriptor.GetProperties(textBox)[nameof(ConfigurationTextBox.DebounceInterval)];
            Assert.NotNull(property);
            Assert.True(property.IsBrowsable);
            Assert.Equal(300, ((System.ComponentModel.DefaultValueAttribute)property.Attributes[typeof(System.ComponentModel.DefaultValueAttribute)]!).Value);
            textBox.DebounceInterval = -1;
            Assert.Equal(50, textBox.DebounceInterval);
            textBox.DebounceInterval = 125;
            Assert.Equal(125, textBox.DebounceInterval);
        }

        [Fact]
        public void TextChanges_AreDebouncedAndRestartTheTimer()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            config.Value = "initial";
            using var textBox = new ConfigurationTextBox { ConfigKey = config.Key, DebounceInterval = 400 };
            _ = textBox.Handle;
            var values = new List<string?>();
            config.ValueChanged += (_, _) => values.Add(config.Value);
            textBox.Text = "first";
            PumpFor(220);
            Assert.Equal("initial", config.Value);
            textBox.Text = "final";
            PumpFor(220);
            Assert.Equal("initial", config.Value);
            PumpUntil(() => config.Value == "final");
            Assert.Equal("final", Assert.Single(values));
        }

        [Fact]
        public void ExternalChanges_CancelPendingTextWithoutWritingBackNull()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            config.Value = "initial";
            using var textBox = new ConfigurationTextBox { ConfigKey = config.Key, DebounceInterval = 50 };
            _ = textBox.Handle;
            textBox.Text = "pending";
            config.Value = null;
            Assert.Equal(string.Empty, textBox.Text);
            PumpFor(150);
            Assert.Null(config.Value);
            textBox.Text = "other pending";
            RunOnWorker(() => config.Value = "external");
            Application.DoEvents();
            PumpFor(150);
            Assert.Equal("external", textBox.Text);
            Assert.Equal("external", config.Value);
        }

        [Fact]
        public void BindingNullValue_DoesNotWriteEmptyTextToConfiguration()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            using var textBox = new ConfigurationTextBox { Text = "before binding", DebounceInterval = 50, ConfigKey = config.Key };
            _ = textBox.Handle;
            Assert.Equal(string.Empty, textBox.Text);
            PumpFor(150);
            Assert.Null(config.Value);
        }

        [Fact]
        public void InvalidRebind_PreservesExistingBindingAndPendingText()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            var invalidKey = $"TextBox_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<int>(invalidKey, isTransient: true);
            config.Value = "initial";
            using var textBox = new ConfigurationTextBox { ConfigKey = config.Key, DebounceInterval = 50 };
            textBox.Text = "pending";
            Assert.Throws<InvalidOperationException>(() => textBox.ConfigKey = invalidKey);
            Assert.Equal(config.Key, textBox.ConfigKey);
            PumpUntil(() => config.Value == "pending");
            config.Value = "external";
            Assert.Equal("external", textBox.Text);
        }

        [Fact]
        public void PendingText_IsSavedToOldKeyOnRebindAndOnDispose()
        {
            var first = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            var second = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            first.Value = "first";
            second.Value = "second";
            using var textBox = new ConfigurationTextBox { ConfigKey = first.Key };
            textBox.Text = "edited first";
            textBox.ConfigKey = second.Key;
            Assert.Equal("edited first", first.Value);
            Assert.Equal("second", textBox.Text);
            textBox.Text = "edited second";
            textBox.Dispose();
            Assert.Equal("edited second", second.Value);
        }

        [Fact]
        public void PendingText_SurvivesHandleRecreationAndQueuedConfigWinsOnUnbind()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            config.Value = "initial";
            using var textBox = new TestConfigurationTextBox { ConfigKey = config.Key, DebounceInterval = 50 };
            _ = textBox.Handle;
            textBox.Text = "pending";
            textBox.RemoveHandle();
            _ = textBox.Handle;
            Assert.Equal("pending", textBox.Text);
            PumpUntil(() => config.Value == "pending");
            textBox.Text = "local edit";
            RunOnWorker(() => config.Value = "external");
            textBox.ConfigKey = null;
            Assert.Equal("local edit", textBox.Text);
            Assert.Equal("external", config.Value);
        }

        [Fact]
        public void PendingExternalUpdate_TakesPrecedenceOnDispose()
        {
            var config = ConfigFactory.GetConfig<string>($"TextBox_{Guid.NewGuid()}", isTransient: true);
            config.Value = "initial";
            using var textBox = new ConfigurationTextBox { ConfigKey = config.Key };
            _ = textBox.Handle;
            textBox.Text = "pending edit";
            RunOnWorker(() => config.Value = "external");
            textBox.Dispose();
            Application.DoEvents();
            Assert.Equal("external", config.Value);
        }

        [Fact]
        public void ConfigKey_IsDesignerVisible()
        {
            var property = System.ComponentModel.TypeDescriptor.GetProperties(typeof(ConfigurationTextBox))[nameof(ConfigurationTextBox.ConfigKey)];
            Assert.NotNull(property);
            Assert.True(property.IsBrowsable);
            Assert.Equal(System.ComponentModel.DesignerSerializationVisibility.Visible, property.SerializationVisibility);
        }

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
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    error = exception;
                }
            });
            thread.Start();
            thread.Join();
            if (error != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }

        private sealed class TestConfigurationTextBox : ConfigurationTextBox
        {
            public void RemoveHandle() => DestroyHandle();
        }
    }
}
