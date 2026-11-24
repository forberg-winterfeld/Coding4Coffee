using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;
using Coding4Coffee.WinForms.Localization.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public class ConfigurationCheckBoxTests
    {
        [Fact]
        public void CheckState_SynchronizesAllThreeStatesInBothDirections()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<CheckState>(key, isTransient: true);
            config.Value = CheckState.Indeterminate;
            using var checkBox = new ConfigurationCheckBox { ThreeState = true, ConfigKey = key };
            Assert.IsType<LocalizableCheckBox>(checkBox, exactMatch: false);
            Assert.Equal(CheckState.Indeterminate, checkBox.CheckState);

            checkBox.CheckState = CheckState.Checked;
            Assert.Equal(CheckState.Checked, config.Value);
            config.Value = CheckState.Unchecked;
            Assert.Equal(CheckState.Unchecked, checkBox.CheckState);
            checkBox.CheckState = CheckState.Indeterminate;
            Assert.Equal(CheckState.Indeterminate, config.Value);
            config.Value = CheckState.Checked;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
        }

        [Fact]
        public void ConfigKey_NewConfigDefaultsToUnchecked()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<CheckState>(key, isTransient: true);
            using var checkBox = new ConfigurationCheckBox { CheckState = CheckState.Checked, ConfigKey = key };
            Assert.Equal(CheckState.Unchecked, checkBox.CheckState);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" \t")]
        public void EmptyKey_UnbindsWithoutChangingState(string? keyWithoutBinding)
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<CheckState>(key, isTransient: true);
            config.Value = CheckState.Checked;
            using var checkBox = new ConfigurationCheckBox { ConfigKey = key };

            checkBox.ConfigKey = keyWithoutBinding;
            Assert.Equal(keyWithoutBinding, checkBox.ConfigKey);
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            config.Value = CheckState.Unchecked;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            checkBox.CheckState = CheckState.Indeterminate;
            Assert.Equal(CheckState.Unchecked, config.Value);
            checkBox.ConfigKey = key;
            Assert.Equal(CheckState.Unchecked, checkBox.CheckState);
        }

        [Fact]
        public void InvalidKeyType_LeavesPreviousBindingIntact()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var otherKey = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<CheckState>(key, isTransient: true);
            ConfigFactory.GetConfig<bool>(otherKey, isTransient: true);
            using var checkBox = new ConfigurationCheckBox { ConfigKey = key };

            Assert.Throws<InvalidOperationException>(() => checkBox.ConfigKey = otherKey);
            Assert.Equal(key, checkBox.ConfigKey);
            config.Value = CheckState.Checked;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            checkBox.CheckState = CheckState.Unchecked;
            Assert.Equal(CheckState.Unchecked, config.Value);
        }

        [Fact]
        public void ConfigKey_IsVisibleInDesigner()
        {
            var property = TypeDescriptor.GetProperties(typeof(ConfigurationCheckBox))[nameof(ConfigurationCheckBox.ConfigKey)];
            Assert.NotNull(property);
            Assert.True(property.IsBrowsable);
            Assert.Equal(DesignerSerializationVisibility.Visible, property.SerializationVisibility);
        }

        [Fact]
        public void WorkerUpdates_AreAppliedOnOwnerThreadAcrossHandleLifecycle()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<CheckState>(key, isTransient: true);
            using var checkBox = new TestCheckBox { ThreeState = true, ConfigKey = key };
            var ownerThread = Environment.CurrentManagedThreadId;
            var threads = new List<int>();
            checkBox.CheckStateChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);

            RunOnWorker(() => config.Value = CheckState.Checked);
            Assert.False(checkBox.IsHandleCreated);
            Assert.Empty(threads);
            _ = checkBox.Handle;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);

            RunOnWorker(() => config.Value = CheckState.Indeterminate);
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            Application.DoEvents();
            Assert.Equal(CheckState.Indeterminate, checkBox.CheckState);

            RunOnWorker(() => config.Value = CheckState.Unchecked);
            checkBox.RemoveHandle();
            _ = checkBox.Handle;
            Assert.Equal(CheckState.Unchecked, checkBox.CheckState);
            Assert.NotEmpty(threads);
            Assert.All(threads, thread => Assert.Equal(ownerThread, thread));

            RunOnWorker(() => config.Value = CheckState.Checked);
            checkBox.ConfigKey = null;
            Application.DoEvents();
            Assert.Equal(CheckState.Unchecked, checkBox.CheckState);
            checkBox.ConfigKey = key;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);

            RunOnWorker(() => config.Value = CheckState.Indeterminate);
            checkBox.Dispose();
            Application.DoEvents();
            config.Value = CheckState.Unchecked;
            Assert.Equal(CheckState.Unchecked, config.Value);
        }

        [Fact]
        public void BooleanMode_SynchronizesAndNormalizesIndeterminate()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<bool>(key, isTransient: true);
            config.Value = true;
            using var checkBox = new ConfigurationCheckBox { ThreeState = true, ValueMode = ConfigurationCheckBoxValueMode.Boolean, ConfigKey = key };
            Assert.Equal(CheckState.Checked, checkBox.CheckState);

            checkBox.Checked = false;
            Assert.False(config.Value);
            config.Value = true;
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            checkBox.CheckState = CheckState.Indeterminate;
            Assert.True(config.Value);
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
            checkBox.Checked = false;
            checkBox.CheckState = CheckState.Indeterminate;
            Assert.True(config.Value);
            Assert.Equal(CheckState.Checked, checkBox.CheckState);
        }

        [Fact]
        public void BooleanMode_DesignerInitializationAndUnbindingWork()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<bool>(key, isTransient: true);
            config.Value = true;
            using var checkBox = new ConfigurationCheckBox();
            checkBox.BeginInit();
            checkBox.ConfigKey = key;
            checkBox.ValueMode = ConfigurationCheckBoxValueMode.Boolean;
            checkBox.EndInit();
            Assert.True(checkBox.Checked);
            Assert.Throws<InvalidOperationException>(() => checkBox.ValueMode = ConfigurationCheckBoxValueMode.CheckState);
            checkBox.ConfigKey = null;
            config.Value = false;
            Assert.True(checkBox.Checked);
            checkBox.Checked = false;
            checkBox.Checked = true;
            Assert.False(config.Value);
            checkBox.ConfigKey = key;
            Assert.False(checkBox.Checked);
        }

        [Fact]
        public void BooleanMode_BackgroundUpdatesAndDisposal()
        {
            var key = $"CheckBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<bool>(key, isTransient: true);
            using var checkBox = new TestCheckBox { ValueMode = ConfigurationCheckBoxValueMode.Boolean, ConfigKey = key };
            var owner = Environment.CurrentManagedThreadId;
            var threads = new List<int>();
            checkBox.CheckStateChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);
            RunOnWorker(() => config.Value = true);
            Assert.Empty(threads);
            _ = checkBox.Handle;
            Assert.True(checkBox.Checked);
            RunOnWorker(() => config.Value = false);
            Assert.True(checkBox.Checked);
            Application.DoEvents();
            Assert.False(checkBox.Checked);
            Assert.NotEmpty(threads);
            Assert.All(threads, id => Assert.Equal(owner, id));
            RunOnWorker(() => config.Value = true);
            checkBox.RemoveHandle();
            _ = checkBox.Handle;
            Assert.True(checkBox.Checked);
            RunOnWorker(() => config.Value = false);
            checkBox.Dispose();
            Application.DoEvents();
            config.Value = true;
            Assert.True(config.Value);
        }

        [Fact]
        public void ValueMode_IsDesignerVisibleAndDefaultsToCheckState()
        {
            var property = TypeDescriptor.GetProperties(typeof(ConfigurationCheckBox))[nameof(ConfigurationCheckBox.ValueMode)];
            Assert.NotNull(property);
            Assert.True(property.IsBrowsable);
            Assert.Equal(DesignerSerializationVisibility.Visible, property.SerializationVisibility);
            using var checkBox = new ConfigurationCheckBox();
            Assert.Equal(ConfigurationCheckBoxValueMode.CheckState, checkBox.ValueMode);
            Assert.Throws<InvalidEnumArgumentException>(() => checkBox.ValueMode = (ConfigurationCheckBoxValueMode)42);
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

        private sealed class TestCheckBox : ConfigurationCheckBox
        {
            public void RemoveHandle() => DestroyHandle();
        }
    }
}
