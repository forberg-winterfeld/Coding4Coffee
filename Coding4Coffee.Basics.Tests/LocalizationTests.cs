using System.Globalization;
using System.Resources;
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.Basics.Localization.Commands;
using Xunit;

namespace Coding4Coffee.Basics.Tests
{
    public class LocalizationTests
    {
        private class InMemoryResourceManager : ResourceManager
        {
            private readonly Dictionary<string, string> _resources;

            public InMemoryResourceManager(Dictionary<string, string> resources)
            {
                _resources = resources;
            }

            public override string? GetString(string name, CultureInfo? culture)
            {
                return _resources.TryGetValue(name, out var val) ? val : null;
            }
        }

        [Fact]
        public void LocalizableString_ReturnsFallbackValue_WhenNoResourceManager()
        {
            var originalMgr = BasicsLocalizationExtensions.ResourceManager;
            BasicsLocalizationExtensions.ResourceManager = null;

            try
            {
                var str = new LocalizableString("NonExistent_Key", "Fallback Text");

                Assert.Equal("Fallback Text", str.ToString());

                string? implicitStr = str;
                Assert.Equal("Fallback Text", implicitStr);
            }
            finally
            {
                BasicsLocalizationExtensions.ResourceManager = originalMgr;
            }
        }

        [Fact]
        public void LocalizableString_ResolvesStringFromResourceManager()
        {
            var originalMgr = BasicsLocalizationExtensions.ResourceManager;
            BasicsLocalizationExtensions.ResourceManager = new InMemoryResourceManager(new()
            {
                { "Greeting_Key", "Willkommen!" }
            });

            try
            {
                var str = new LocalizableString("Greeting_Key", "Fallback");
                Assert.Equal("Willkommen!", str.ToString());
            }
            finally
            {
                BasicsLocalizationExtensions.ResourceManager = originalMgr;
            }
        }

        [Fact]
        public void LocalizableString_Format_FormatsFallbackAndResolvedValues()
        {
            var originalMgr = BasicsLocalizationExtensions.ResourceManager;
            BasicsLocalizationExtensions.ResourceManager = new InMemoryResourceManager(new()
            {
                { "Message_Pattern", "Hallo {0}!" }
            });

            try
            {
                var resolved = new LocalizableString("Message_Pattern", "Fallback {0}");
                Assert.Equal("Hallo Bob!", resolved.Format("Bob"));

                var fallback = new LocalizableString("NonExistent_Pattern", "Hi {0} ({1})");
                Assert.Equal("Hi Bob (admin)", fallback.Format("Bob", "admin"));
            }
            finally
            {
                BasicsLocalizationExtensions.ResourceManager = originalMgr;
            }
        }

        [Fact]
        public void LocalizationManager_ChangingCulture_RaisesCultureChanged()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            bool eventFired = false;
            EventHandler handler = (s, e) => eventFired = true;
            LocalizationManager.CultureChanged += handler;

            try
            {
                var targetCulture = initialCulture.Name == "fr-FR"
                    ? new CultureInfo("de-DE")
                    : new CultureInfo("fr-FR");

                LocalizationManager.CurrentCulture = targetCulture;

                Assert.True(eventFired);
                Assert.Equal(targetCulture.Name, LocalizationManager.CurrentCulture.Name);
            }
            finally
            {
                LocalizationManager.CultureChanged -= handler;
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Fact]
        public void LocalizationManager_SynchronizesWith_CultureConfigKey()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            string key = $"AppCulture_{Guid.NewGuid()}";

            try
            {
                LocalizationManager.CultureConfigKey = key;

                var targetCulture = new CultureInfo("es-ES");
                LocalizationManager.CurrentCulture = targetCulture;

                var config = ConfigFactory.GetCultureInfoConfig(key, isTransient: true);
                Assert.NotNull(config.Value);
                Assert.Equal("es-ES", config.Value.Name);
            }
            finally
            {
                LocalizationManager.CultureConfigKey = string.Empty;
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Fact]
        public void LocalizationManager_HasConfiguredCulture_TracksConfigurationState()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            string key = $"AppCulture_{Guid.NewGuid()}";

            try
            {
                LocalizationManager.CultureConfigKey = string.Empty;
                Assert.False(LocalizationManager.HasConfiguredCulture);

                LocalizationManager.CultureConfigKey = key;
                Assert.False(LocalizationManager.HasConfiguredCulture);

                LocalizationManager.CurrentCulture = new CultureInfo("it-IT");
                Assert.True(LocalizationManager.HasConfiguredCulture);
            }
            finally
            {
                LocalizationManager.CultureConfigKey = string.Empty;
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Fact]
        public void LocalizationManager_ResetToSystemCulture_ClearsConfigurationAndAppliesCulture()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            string key = $"AppCulture_{Guid.NewGuid()}";

            try
            {
                LocalizationManager.CultureConfigKey = key;
                LocalizationManager.CurrentCulture = new CultureInfo("ja-JP");
                Assert.True(LocalizationManager.HasConfiguredCulture);
                Assert.Equal("ja-JP", LocalizationManager.CurrentCulture.Name);

                bool eventFired = false;
                EventHandler handler = (s, e) => eventFired = true;
                LocalizationManager.CultureChanged += handler;

                try
                {
                    var fallback = new CultureInfo("en-US");
                    LocalizationManager.ResetToSystemCulture(fallback);

                    Assert.False(LocalizationManager.HasConfiguredCulture);
                    Assert.Equal("en-US", LocalizationManager.CurrentCulture.Name);
                    Assert.True(eventFired);
                }
                finally
                {
                    LocalizationManager.CultureChanged -= handler;
                }
            }
            finally
            {
                LocalizationManager.CultureConfigKey = string.Empty;
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Fact]
        public void SetCultureCommand_CanExecute_And_Execute()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            var command = SetCultureCommand.Instance;

            try
            {
                string currentCultureName = LocalizationManager.CurrentCulture.Name;
                string differentCultureName = currentCultureName == "en-US" ? "de-DE" : "en-US";

                Assert.False(command.CanExecute(currentCultureName));
                Assert.True(command.CanExecute(differentCultureName));

                command.Execute(differentCultureName);

                Assert.Equal(differentCultureName, LocalizationManager.CurrentCulture.Name);
            }
            finally
            {
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }

        [Fact]
        public void ResetCultureToSystemDefaultCommand_CanExecute_And_Execute()
        {
            var initialCulture = LocalizationManager.CurrentCulture;
            string key = $"AppCulture_{Guid.NewGuid()}";
            var fallback = new CultureInfo("en-US");
            var command = new ResetCultureToSystemDefaultCommand(fallback);

            try
            {
                LocalizationManager.CultureConfigKey = key;
                LocalizationManager.CurrentCulture = new CultureInfo("de-DE");
                Assert.True(LocalizationManager.HasConfiguredCulture);

                // CanExecute is true because a culture is configured
                Assert.True(command.CanExecute(null));

                bool canExecuteChangedFired = false;
                command.CanExecuteChanged += (s, e) => canExecuteChangedFired = true;

                command.Execute(null);

                // Culture was reset to fallback and config was cleared
                Assert.False(LocalizationManager.HasConfiguredCulture);
                Assert.Equal("en-US", LocalizationManager.CurrentCulture.Name);
                Assert.True(canExecuteChangedFired);

                // Now that culture is already fallback and no config is saved, CanExecute should be false
                Assert.False(command.CanExecute(null));
            }
            finally
            {
                LocalizationManager.CultureConfigKey = string.Empty;
                LocalizationManager.CurrentCulture = initialCulture;
            }
        }
    }
}
