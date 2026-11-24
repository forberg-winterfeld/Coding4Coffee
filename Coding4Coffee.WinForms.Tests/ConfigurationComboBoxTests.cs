using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

namespace Coding4Coffee.WinForms.Tests
{
    public class ConfigurationComboBoxTests
    {
        [Fact]
        public void Selection_SynchronizesInBothDirectionsAndPreservesInvalidConfiguration()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            config.Value = 1;
            using var combo = new ConfigurationComboBox();
            combo.Items.AddRange(["First", "Second"]);
            combo.ConfigKey = key;
            Assert.Equal(1, combo.SelectedIndex);

            combo.SelectedIndex = 0;
            Assert.Equal(0, config.Value);
            config.Value = 1;
            Assert.Equal(1, combo.SelectedIndex);
            combo.SelectedIndex = -1;
            Assert.Null(config.Value);

            config.Value = 5;
            Assert.Equal(-1, combo.SelectedIndex);
            Assert.Equal(5, config.Value);
            config.Value = 0;
            Assert.Equal(0, combo.SelectedIndex);
            combo.Localize();
            Assert.Equal(0, combo.SelectedIndex);
            Assert.Equal(0, config.Value);
        }

        [Fact]
        public void DataSource_SelectionUsesConfiguredIndex()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            config.Value = 1;
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext() };
            combo.DataSource = new[] { "First", "Second" };
            combo.ConfigKey = key;
            Assert.Equal(1, combo.SelectedIndex);
            Assert.Equal("Second", combo.SelectedItem);

            combo.SelectedIndex = 0;
            Assert.Equal(0, config.Value);
            config.Value = 1;
            Assert.Equal("Second", combo.SelectedItem);
            config.Value = 4;
            Assert.Equal(-1, combo.SelectedIndex);
            Assert.Equal(4, config.Value);
            config.Value = 0;
            Assert.Equal("First", combo.SelectedItem);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void EmptyKey_UnbindsWithoutChangingSelection(string? emptyKey)
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            config.Value = 1;
            using var combo = new ConfigurationComboBox();
            combo.Items.AddRange(["First", "Second"]);
            combo.ConfigKey = key;

            combo.ConfigKey = emptyKey;
            Assert.Equal(emptyKey, combo.ConfigKey);
            Assert.Equal(1, combo.SelectedIndex);
            config.Value = 0;
            Assert.Equal(1, combo.SelectedIndex);
            combo.SelectedIndex = -1;
            Assert.Equal(0, config.Value);
            config.Value = 1;
            combo.ConfigKey = key;
            Assert.Equal(1, combo.SelectedIndex);
        }

        [Fact]
        public void WorkerUpdates_AreAppliedOnOwnerThreadAndAfterHandleRecreation()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            config.Value = 0;
            using var combo = new TestComboBox();
            combo.Items.AddRange(["First", "Second"]);
            combo.ConfigKey = key;
            var ownerThread = Environment.CurrentManagedThreadId;
            var threads = new List<int>();
            combo.SelectedIndexChanged += (_, _) => threads.Add(Environment.CurrentManagedThreadId);

            RunOnWorker(() => config.Value = 1);
            Assert.False(combo.IsHandleCreated);
            Assert.Empty(threads);
            _ = combo.Handle;
            Assert.Equal(1, combo.SelectedIndex);

            RunOnWorker(() => config.Value = 0);
            Assert.Equal(1, combo.SelectedIndex);
            Application.DoEvents();
            Assert.Equal(0, combo.SelectedIndex);

            RunOnWorker(() => config.Value = 1);
            combo.RemoveHandle();
            _ = combo.Handle;
            Assert.Equal(1, combo.SelectedIndex);
            Assert.All(threads, thread => Assert.Equal(ownerThread, thread));

            RunOnWorker(() => config.Value = 0);
            combo.Dispose();
            Application.DoEvents();
            config.Value = 1;
            Assert.Equal(1, config.Value);
        }

        [Fact]
        public void ValueMode_UsesStableStringKeyAcrossDifferentItemOrder()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "dark";
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext() };
            combo.BeginInit();
            combo.ConfigKey = key;
            combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value;
            combo.DisplayMember = nameof(Choice.Label);
            combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("dark", "Dunkel"), new Choice("light", "Hell") };
            combo.EndInit();
            Assert.Equal(0, combo.SelectedIndex);
            Assert.Equal("dark", config.Value);

            combo.SelectedIndex = 1;
            Assert.Equal("light", config.Value);
            config.Value = "missing";
            Assert.Equal(-1, combo.SelectedIndex);
            Assert.Equal("missing", config.Value);
            config.Value = "dark";
            Assert.Equal("Dunkel", combo.Text);
            combo.Localize();
            Assert.Equal("dark", config.Value);
            Assert.Equal(0, combo.SelectedIndex);

            using var reordered = new ConfigurationComboBox
            {
                BindingContext = new BindingContext(),
                SelectionStorageMode = ConfigurationComboBoxValueMode.Value,
                DisplayMember = nameof(Choice.Label),
                ValueMember = nameof(Choice.Id),
                DataSource = new[] { new Choice("light", "Light"), new Choice("dark", "Dark") }
            };
            reordered.ConfigKey = key;
            Assert.Equal(1, reordered.SelectedIndex);
            Assert.Equal("Dark", reordered.Text);
            Assert.Equal("dark", config.Value);
        }

        [Fact]
        public void ValueMode_RequiresStringValueMember()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "1";
            using var combo = new ConfigurationComboBox
            {
                BindingContext = new BindingContext(),
                SelectionStorageMode = ConfigurationComboBoxValueMode.Value,
                ValueMember = nameof(NumberedChoice.Id),
                DataSource = new[] { new NumberedChoice(1) }
            };
            Assert.Throws<InvalidOperationException>(() => combo.ConfigKey = key);
            Assert.Equal("1", config.Value);
        }

        [Fact]
        public void SelectionStorageMode_IsVisibleInDesignerAndDefaultsToIndex()
        {
            var property = System.ComponentModel.TypeDescriptor.GetProperties(typeof(ConfigurationComboBox))[nameof(ConfigurationComboBox.SelectionStorageMode)];
            Assert.NotNull(property);
            Assert.True(property.IsBrowsable);
            Assert.Equal(System.ComponentModel.DesignerSerializationVisibility.Visible, property.SerializationVisibility);
            var dataSource = System.ComponentModel.TypeDescriptor.GetProperties(typeof(ConfigurationComboBox))[nameof(ConfigurationComboBox.DataSource)];
            var baseDataSource = System.ComponentModel.TypeDescriptor.GetProperties(typeof(ComboBox))[nameof(ComboBox.DataSource)];
            Assert.NotNull(dataSource);
            Assert.NotNull(baseDataSource);
            Assert.Equal(baseDataSource.IsBrowsable, dataSource.IsBrowsable);
            Assert.Equal(baseDataSource.SerializationVisibility, dataSource.SerializationVisibility);
            Assert.Equal(baseDataSource.Converter.GetType(), dataSource.Converter.GetType());
            using var combo = new ConfigurationComboBox();
            Assert.Equal(ConfigurationComboBoxValueMode.Index, combo.SelectionStorageMode);
        }

        [Fact]
        public void TextMode_StoresDisplayedTextAndPreservesUnmatchedValue()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "Second";
            using var combo = new ConfigurationComboBox { SelectionStorageMode = ConfigurationComboBoxValueMode.Text };
            combo.Items.AddRange(["First", "Second"]);
            combo.ConfigKey = key;
            Assert.Equal(1, combo.SelectedIndex);
            combo.SelectedIndex = 0;
            Assert.Equal("First", config.Value);
            config.Value = "missing";
            Assert.Equal(-1, combo.SelectedIndex);
            Assert.Equal("missing", config.Value);
            combo.SelectedIndex = 1;
            Assert.Equal("Second", config.Value);
            combo.SelectedIndex = -1;
            Assert.Null(config.Value);
        }

        [Fact]
        public void ConfigKey_WhenNewKeyHasWrongType_PreservesExistingBinding()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var otherKey = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            ConfigFactory.GetConfig<string>(otherKey, isTransient: true);
            config.Value = 0;
            using var combo = new ConfigurationComboBox();
            combo.Items.AddRange(["First", "Second"]);
            combo.ConfigKey = key;
            Assert.Throws<InvalidOperationException>(() => combo.ConfigKey = otherKey);
            Assert.Equal(key, combo.ConfigKey);
            config.Value = 1;
            Assert.Equal(1, combo.SelectedIndex);
            Assert.Throws<InvalidOperationException>(() => combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value);
            Assert.Equal(ConfigurationComboBoxValueMode.Index, combo.SelectionStorageMode);
        }

        [Theory]
        [InlineData(true, "dark")]
        [InlineData(false, "dark")]
        [InlineData(true, null)]
        [InlineData(false, null)]
        public void ValueMode_DesignerInitializedKeyWaitsForRuntimeData(bool valueMemberFirst, string? savedValue)
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = savedValue;
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext() };
            combo.BeginInit();
            combo.ConfigKey = key;
            combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value;
            combo.EndInit();
            Assert.Equal(savedValue, config.Value);
            combo.DisplayMember = nameof(Choice.Label);
            if (valueMemberFirst)
                combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("light", "Light"), new Choice("dark", "Dark") };
            Assert.Equal(savedValue, config.Value);
            if (!valueMemberFirst)
                combo.ValueMember = nameof(Choice.Id);
            Assert.Equal(savedValue, config.Value);
            Assert.Equal(savedValue == null ? -1 : 1, combo.SelectedIndex);
            combo.SelectedIndex = 0;
            Assert.Equal("light", config.Value);
        }

        [Fact]
        public void ValueMode_WaitsForBindingContextBeforeRestoringSelection()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "dark";
            using var combo = new ConfigurationComboBox { SelectionStorageMode = ConfigurationComboBoxValueMode.Value, ConfigKey = key };
            combo.DisplayMember = nameof(Choice.Label);
            combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("light", "Light"), new Choice("dark", "Dark") };
            Assert.Equal("dark", config.Value);
            combo.BindingContext = new BindingContext();
            Assert.Equal(1, combo.SelectedIndex);
            Assert.Equal("dark", config.Value);
        }

        [Fact]
        public void ValueMode_RestoresSelectionWhenPlacedOnFormBeforeDataSource()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "dark";
            using var form = new Form();
            using var combo = new ConfigurationComboBox();
            combo.BeginInit();
            combo.ConfigKey = key;
            combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value;
            form.Controls.Add(combo);
            combo.EndInit();
            combo.DisplayMember = nameof(Choice.Label);
            combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("light", "Light"), new Choice("dark", "Dark") };
            form.CreateControl();
            _ = combo.Handle;
            Assert.Equal(1, combo.SelectedIndex);
            Assert.Equal("dark", config.Value);
        }

        [Theory]
        [InlineData("en-US", 0, false)]
        [InlineData("de-DE", 1, false)]
        [InlineData("en-US", 0, true)]
        [InlineData("de-DE", 1, true)]
        public void ValueMode_CultureConfigIsRestoredWhenLocalizableFormLoads(string culture, int expectedIndex, bool localizeBeforeLoad)
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetCultureInfoConfig(key, isTransient: true);
            config.Value = System.Globalization.CultureInfo.GetCultureInfo(culture);
            using var form = new Coding4Coffee.WinForms.Localization.Controls.LocalizableForm();
            using var combo = new ConfigurationComboBox();
            combo.BeginInit();
            combo.ConfigKey = key;
            combo.DisplayMember = nameof(Choice.Label);
            combo.FormattingEnabled = true;
            combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value;
            combo.ValueMember = nameof(Choice.Id);
            form.Controls.Add(combo);
            combo.EndInit();
            combo.DataSource = new List<Choice> { new("en-US", "English"), new("de-DE", "German") };
            Assert.Equal(culture, config.Value?.Name);
            form.CreateControl();
            _ = combo.Handle;
            Assert.Equal(culture, config.Value?.Name);
            if (localizeBeforeLoad)
                combo.Localize();
            Assert.Equal(culture, config.Value?.Name);
            form.Show();
            Application.DoEvents();
            Assert.Equal(culture, config.Value?.Name);
            Assert.Equal(expectedIndex, combo.SelectedIndex);
            Assert.Equal(culture, combo.SelectedValue);
            Assert.Equal(culture, config.Value?.Name);
            combo.SelectedIndex = 1 - expectedIndex;
            Assert.Equal(expectedIndex == 0 ? "de-DE" : "en-US", config.Value?.Name);
        }

        [Fact]
        public void ValueMode_SelectionChangeUpdatesLocalizationManagerAndForm()
        {
            var initialCulture = Coding4Coffee.Basics.Localization.LocalizationManager.CurrentCulture;
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetCultureInfoConfig(key, isTransient: true);
            config.Value = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            try
            {
                Coding4Coffee.Basics.Localization.LocalizationManager.CultureConfigKey = key;
                using var form = new Coding4Coffee.WinForms.Localization.Controls.LocalizableForm();
                using var combo = new ConfigurationComboBox { SelectionStorageMode = ConfigurationComboBoxValueMode.Value };
                var localizations = new List<string>();
                var items = new[]
                {
                    new CultureChoice("en-US", () => localizations.Add(Coding4Coffee.Basics.Localization.LocalizationManager.CurrentCulture.Name)),
                    new CultureChoice("de-DE", () => { })
                };
                combo.ValueMember = nameof(CultureChoice.Id);
                form.Controls.Add(combo);
                combo.ConfigKey = key;
                combo.DataSource = items;
                form.Show();
                Application.DoEvents();
                localizations.Clear();
                combo.SelectedIndex = 1;
                Assert.Equal("de-DE", config.Value?.Name);
                Assert.Equal("de-DE", Coding4Coffee.Basics.Localization.LocalizationManager.CurrentCulture.Name);
                Assert.Equal("de-DE", combo.SelectedValue);
                Assert.Equal("de-DE", Assert.Single(localizations));
            }
            finally
            {
                Coding4Coffee.Basics.Localization.LocalizationManager.CultureConfigKey = string.Empty;
                Coding4Coffee.Basics.Localization.LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Theory]
        [InlineData(ConfigurationComboBoxValueMode.Index)]
        [InlineData(ConfigurationComboBoxValueMode.Text)]
        public void Localize_PreservesConfiguredSelection(ConfigurationComboBoxValueMode mode)
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var indexConfig = mode == ConfigurationComboBoxValueMode.Index ? ConfigFactory.GetConfig<int?>(key, isTransient: true) : null;
            var textConfig = mode == ConfigurationComboBoxValueMode.Text ? ConfigFactory.GetConfig<string>(key, isTransient: true) : null;
            indexConfig?.Value = 1;
            textConfig?.Value = "Second";
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext(), SelectionStorageMode = mode };
            combo.DataSource = new[] { "First", "Second" };
            combo.ConfigKey = key;
            _ = combo.Handle;
            ((Coding4Coffee.Basics.Localization.ILocalizable)combo).Localize();
            Assert.Equal(1, combo.SelectedIndex);
            if (indexConfig != null)
                Assert.Equal(1, indexConfig.Value);
            if (textConfig != null)
                Assert.Equal("Second", textConfig.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("missing")]
        public void ValueMode_LocalizePreservesEmptyOrUnmatchedConfiguration(string? savedValue)
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = savedValue;
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext(), SelectionStorageMode = ConfigurationComboBoxValueMode.Value };
            combo.DisplayMember = nameof(Choice.Label);
            combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("first", "First"), new Choice("second", "Second") };
            combo.ConfigKey = key;
            _ = combo.Handle;
            combo.Localize();
            Assert.Equal(savedValue, config.Value);
            Assert.Equal(-1, combo.SelectedIndex);
        }

        [Fact]
        public void Localize_ConfigurationChangeDuringItemLocalizationDoesNotLoseGuard()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int?>(key, isTransient: true);
            config.Value = 0;
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext() };
            combo.DataSource = new[] { new LocalizableChoice(() => config.Value = 1), new LocalizableChoice(() => { }) };
            combo.ConfigKey = key;
            _ = combo.Handle;
            combo.Localize();
            Assert.Equal(1, config.Value);
            Assert.Equal(1, combo.SelectedIndex);
        }

        [Fact]
        public void ValueMode_ReplacingDataSourcePreservesStoredSelection()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "dark";
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext(), SelectionStorageMode = ConfigurationComboBoxValueMode.Value };
            combo.DisplayMember = nameof(Choice.Label);
            combo.ValueMember = nameof(Choice.Id);
            combo.DataSource = new[] { new Choice("light", "Light"), new Choice("dark", "Dark") };
            combo.ConfigKey = key;
            combo.DataSource = new[] { new Choice("dark", "Dunkel"), new Choice("light", "Hell") };
            Assert.Equal("dark", config.Value);
            Assert.Equal(0, combo.SelectedIndex);
            Assert.Equal("Dunkel", combo.Text);
        }

        [Fact]
        public void ValueMode_LateInvalidValueMemberReportsError()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "1";
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext(), SelectionStorageMode = ConfigurationComboBoxValueMode.Value, ConfigKey = key };
            combo.DataSource = new[] { new NumberedChoice(1) };
            Assert.Throws<InvalidOperationException>(() => combo.ValueMember = nameof(NumberedChoice.Id));
            Assert.Equal("1", config.Value);
        }

        [Fact]
        public void ValueMode_InvalidLateDataSourceIsReported()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<string>(key, isTransient: true);
            config.Value = "1";
            using var combo = new ConfigurationComboBox { BindingContext = new BindingContext(), SelectionStorageMode = ConfigurationComboBoxValueMode.Value, ConfigKey = key };
            combo.ValueMember = nameof(NumberedChoice.Id);
            Assert.Throws<InvalidOperationException>(() => combo.DataSource = new[] { new NumberedChoice(1) });
            Assert.Equal("1", config.Value);
        }

        [Fact]
        public void DesignerKeyAndMode_DoNotAccessConfiguration()
        {
            var key = $"ComboBox_{Guid.NewGuid()}";
            var config = ConfigFactory.GetConfig<int>(key, isTransient: true);
            config.Value = 42;
            using var combo = new ConfigurationComboBox();
            combo.Site = new DesignSite(combo);
            combo.BeginInit();
            combo.ConfigKey = key;
            combo.SelectionStorageMode = ConfigurationComboBoxValueMode.Value;
            combo.EndInit();
            Assert.Equal(key, combo.ConfigKey);
            Assert.Equal(42, config.Value);
            combo.ConfigKey = key + "_other";
            Assert.Equal(key + "_other", combo.ConfigKey);
        }

        private sealed class DesignSite(System.ComponentModel.IComponent component) : System.ComponentModel.ISite
        {
            public System.ComponentModel.IComponent Component => component;
            public System.ComponentModel.IContainer? Container => null;
            public bool DesignMode => true;
            public string? Name { get; set; }
            public object? GetService(Type serviceType) => null;
        }

        private sealed record Choice(string Id, string Label);
        private sealed record NumberedChoice(int Id);
        private sealed class CultureChoice(string id, Action localize) : Coding4Coffee.Basics.Localization.ILocalizable
        {
            public string Id { get; } = id;
            public void Localize() => localize();
        }

        private sealed class LocalizableChoice(Action localize) : Coding4Coffee.Basics.Localization.ILocalizable
        {
            public void Localize() => localize();
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

        private sealed class TestComboBox : ConfigurationComboBox
        {
            public void RemoveHandle() => DestroyHandle();
        }
    }
}
