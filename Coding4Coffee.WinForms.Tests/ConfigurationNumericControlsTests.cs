using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public class ConfigurationNumericControlsTests
    {
        [Fact]
        public void NumericUpDown_SynchronizesAndPreservesOutOfRangeValues()
        {
            var config = ConfigFactory.GetConfig<decimal>($"Numeric_{Guid.NewGuid()}", isTransient: true);
            config.Value = 125.5m;
            using var control = new ConfigurationNumericUpDown { Minimum = 10, Maximum = 100, ConfigKey = config.Key };
            Assert.Equal(100, control.Value);
            Assert.Equal(125.5m, config.Value);
            control.Maximum = 150;
            Assert.Equal(125.5m, control.Value);
            Assert.Equal(125.5m, config.Value);
            control.Minimum = 130;
            Assert.Equal(130, control.Value);
            Assert.Equal(125.5m, config.Value);
            control.Minimum = 0;
            Assert.Equal(125.5m, control.Value);
            control.Value = 20.25m;
            Assert.Equal(20.25m, config.Value);
            config.Value = -5;
            Assert.Equal(0, control.Value);
            Assert.Equal(-5, config.Value);
            control.ConfigKey = null;
            config.Value = 25;
            Assert.Equal(0, control.Value);
            control.Value = 30;
            Assert.Equal(25, config.Value);
        }

        [Fact]
        public void TrackBar_DebouncesAndPreservesOutOfRangeValues()
        {
            var config = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            config.Value = 50;
            using var control = new ConfigurationTrackBar { Minimum = 0, Maximum = 10, ConfigKey = config.Key, DebounceInterval = 400 };
            _ = control.Handle;
            Assert.Equal(10, control.Value);
            Assert.Equal(50, config.Value);
            control.SetRange(0, 100);
            Assert.Equal(50, control.Value);
            Assert.Equal(50, config.Value);
            control.Value = 20;
            PumpFor(220);
            Assert.Equal(50, config.Value);
            control.Value = 30;
            PumpFor(220);
            Assert.Equal(50, config.Value);
            PumpUntil(() => config.Value == 30);
            control.Value = 40;
            config.Value = 75;
            Assert.Equal(75, control.Value);
            PumpFor(500);
            Assert.Equal(75, config.Value);
            config.Value = -10;
            Assert.Equal(0, control.Value);
            Assert.Equal(-10, config.Value);
        }

        [Fact]
        public void TrackBar_FlushesPendingValueWhenRebindingOrDisposing()
        {
            var first = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            var second = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            second.Value = 4;
            using var control = new ConfigurationTrackBar { ConfigKey = first.Key };
            control.Value = 3;
            Assert.Equal(0, first.Value);
            control.ConfigKey = second.Key;
            Assert.Equal(3, first.Value);
            Assert.Equal(4, control.Value);
            control.Value = 7;
            control.Dispose();
            Assert.Equal(7, second.Value);
        }

        [Fact]
        public void TrackBar_RangeChangeClampsPendingEditWithoutRestoringOldConfig()
        {
            var config = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            config.Value = 3;
            using var control = new ConfigurationTrackBar { ConfigKey = config.Key, DebounceInterval = 10 };
            control.Value = 9;
            control.Maximum = 5;
            Assert.Equal(5, control.Value);
            Assert.Equal(3, config.Value);
            PumpUntil(() => config.Value == 5);
        }

        [Fact]
        public void TrackBar_QueuedExternalChangeWinsOnDispose()
        {
            var config = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            using var control = new ConfigurationTrackBar { ConfigKey = config.Key };
            _ = control.Handle;
            control.Value = 2;
            RunOnWorker(() => config.Value = 8);
            control.Dispose();
            Application.DoEvents();
            Assert.Equal(8, config.Value);
        }

        [Fact]
        public void Initialization_UsesFinalBoundsAndDesignerDoesNotBind()
        {
            var number = ConfigFactory.GetConfig<decimal>($"Numeric_{Guid.NewGuid()}", isTransient: true);
            var slider = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            number.Value = 150;
            slider.Value = 80;
            using var numeric = new ConfigurationNumericUpDown();
            using var track = new ConfigurationTrackBar();
            ((ISupportInitialize)numeric).BeginInit();
            numeric.ConfigKey = number.Key;
            numeric.Maximum = 200;
            ((ISupportInitialize)numeric).EndInit();
            ((ISupportInitialize)track).BeginInit();
            track.ConfigKey = slider.Key;
            track.Maximum = 100;
            ((ISupportInitialize)track).EndInit();
            Assert.Equal(150, numeric.Value);
            Assert.Equal(80, track.Value);
            var incompatible = $"Design_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<string>(incompatible, isTransient: true);
            using var designNumeric = new ConfigurationNumericUpDown();
            using var designTrack = new ConfigurationTrackBar();
            designNumeric.Site = new DesignSite(designNumeric);
            designTrack.Site = new DesignSite(designTrack);
            designNumeric.ConfigKey = incompatible;
            designTrack.ConfigKey = incompatible;
            Assert.Equal(incompatible, designNumeric.ConfigKey);
            Assert.Equal(incompatible, designTrack.ConfigKey);
        }

        [Fact]
        public void InvalidKey_DoesNotRemoveExistingBinding()
        {
            var number = ConfigFactory.GetConfig<decimal>($"Numeric_{Guid.NewGuid()}", isTransient: true);
            var slider = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            var invalid = $"Other_{Guid.NewGuid()}";
            ConfigFactory.GetConfig<string>(invalid, isTransient: true);
            using var numeric = new ConfigurationNumericUpDown { ConfigKey = number.Key };
            using var track = new ConfigurationTrackBar { ConfigKey = slider.Key };
            Assert.Throws<InvalidOperationException>(() => numeric.ConfigKey = invalid);
            Assert.Throws<InvalidOperationException>(() => track.ConfigKey = invalid);
            number.Value = 4;
            slider.Value = 5;
            Assert.Equal(4, numeric.Value);
            Assert.Equal(5, track.Value);
        }

        [Fact]
        public void WorkerUpdates_AreMarshaledAndSurviveHandleRecreation()
        {
            var number = ConfigFactory.GetConfig<decimal>($"Numeric_{Guid.NewGuid()}", isTransient: true);
            var slider = ConfigFactory.GetConfig<int>($"Track_{Guid.NewGuid()}", isTransient: true);
            using var numeric = new TestNumeric { ConfigKey = number.Key };
            using var track = new TestTrack { ConfigKey = slider.Key };
            var owner = Environment.CurrentManagedThreadId;
            var threads = new List<int>();
            numeric.ValueChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);
            track.ValueChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);
            RunOnWorker(() => { number.Value = 3; slider.Value = 4; });
            Assert.Empty(threads);
            _ = numeric.Handle;
            _ = track.Handle;
            Assert.Equal(3, numeric.Value);
            Assert.Equal(4, track.Value);
            RunOnWorker(() => { number.Value = 5; slider.Value = 6; });
            numeric.RemoveHandle();
            track.RemoveHandle();
            _ = numeric.Handle;
            _ = track.Handle;
            Assert.Equal(5, numeric.Value);
            Assert.Equal(6, track.Value);
            RunOnWorker(() => { number.Value = 7; slider.Value = 9; });
            Assert.Equal(5, numeric.Value);
            Assert.Equal(6, track.Value);
            Application.DoEvents();
            Assert.Equal(7, numeric.Value);
            Assert.Equal(9, track.Value);
            Assert.NotEmpty(threads);
            Assert.All(threads, id => Assert.Equal(owner, id));
            track.Value = 7;
            RunOnWorker(() => slider.Value = 8);
            track.ConfigKey = null;
            Application.DoEvents();
            Assert.Equal(8, slider.Value);
            Assert.Equal(7, track.Value);
        }

        [Fact]
        public void DesignerProperties_AreVisibleAndDebounceIsClamped()
        {
            foreach (var type in new[] { typeof(ConfigurationTrackBar), typeof(ConfigurationNumericUpDown) })
            {
                var properties = TypeDescriptor.GetProperties(type);
                Assert.True(properties[nameof(ConfigurationTrackBar.ConfigKey)]!.IsBrowsable);
                Assert.Equal(DesignerSerializationVisibility.Visible, properties[nameof(ConfigurationTrackBar.ConfigKey)]!.SerializationVisibility);
                Assert.Equal(DesignerSerializationVisibility.Visible, properties["Minimum"]!.SerializationVisibility);
                Assert.Equal(DesignerSerializationVisibility.Visible, properties["Maximum"]!.SerializationVisibility);
            }
            using var track = new ConfigurationTrackBar();
            Assert.Equal(300, track.DebounceInterval);
            track.DebounceInterval = 0;
            Assert.Equal(10, track.DebounceInterval);
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
            Assert.True(condition(), "The debounced value was not stored before the timeout.");
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

        private sealed class TestNumeric : ConfigurationNumericUpDown
        {
            public void RemoveHandle() => DestroyHandle();
        }

        private sealed class TestTrack : ConfigurationTrackBar
        {
            public void RemoveHandle() => DestroyHandle();
        }
    }
}
