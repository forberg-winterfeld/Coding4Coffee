using Coding4Coffee.WinForms.Commands;
using Xunit;

namespace Coding4Coffee.WinForms.Tests.Commands
{
    public class OpenUrlCommandTests
    {
        [Fact]
        public void Constructor_NullString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenUrlCommand((string)null!));
        }

        [Fact]
        public void Constructor_NullUri_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenUrlCommand((Uri)null!));
        }

        [Fact]
        public void Constructor_NullStringProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenUrlCommand((Func<string?>)null!));
        }

        [Fact]
        public void Constructor_NullUriProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenUrlCommand((Func<Uri?>)null!));
        }

        [Theory]
        [InlineData("https://coding4coffee.dev", true)]
        [InlineData("http://example.com", true)]
        [InlineData("mailto:test@example.com", true)]
        [InlineData("ftp://ftp.example.com", false)]
        [InlineData("invalid-url", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void CanExecute_ParameterDriven_ValidatesUrlAndScheme(string? url, bool expectedCanExecute)
        {
            var command = new OpenUrlCommand();

            Assert.Equal(expectedCanExecute, command.CanExecute(url));
        }

        [Fact]
        public void CanExecute_UriParameter_WorksAsExpected()
        {
            var command = new OpenUrlCommand();

            Assert.True(command.CanExecute(new Uri("https://github.com")));
            Assert.False(command.CanExecute(new Uri("ftp://ftp.example.com")));
        }

        [Fact]
        public void CanExecute_FixedUri_ReturnsTrue()
        {
            var command = new OpenUrlCommand("https://coding4coffee.dev");

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_DynamicProvider_EvaluatesProvider()
        {
            string? currentUrl = null;
            var command = new OpenUrlCommand(() => currentUrl);

            Assert.False(command.CanExecute(null));

            currentUrl = "https://learn.microsoft.com";
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_CustomPredicate_EvaluatesPredicate()
        {
            var command = new OpenUrlCommand(
                "https://example.com",
                canExecute: _ => false);

            Assert.False(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_CustomAllowedSchemes_RespectsAllowedSchemes()
        {
            var command = new OpenUrlCommand
            {
                AllowedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ftp" }
            };

            Assert.False(command.CanExecute("https://example.com"));
            Assert.True(command.CanExecute("ftp://ftp.example.com"));
        }

        [Fact]
        public void Execute_LaunchesUri_SetsLastOpenedUri_AndRaisesExecutedEvent()
        {
            Uri? launchedUri = null;
            var command = new OpenUrlCommand(
                uriProvider: () => new Uri("https://coding4coffee.dev"),
                canExecute: null,
                launcher: uri => launchedUri = uri);

            Uri? eventUri = null;
            command.Executed += (_, uri) => eventUri = uri;

            command.Execute(null);

            Assert.Equal(new Uri("https://coding4coffee.dev"), launchedUri);
            Assert.Equal(new Uri("https://coding4coffee.dev"), command.LastOpenedUri);
            Assert.Equal(new Uri("https://coding4coffee.dev"), eventUri);
        }

        [Fact]
        public void Execute_ParameterOverridesProvider()
        {
            Uri? launchedUri = null;
            var command = new OpenUrlCommand(
                uriProvider: () => new Uri("https://coding4coffee.dev"),
                canExecute: null,
                launcher: uri => launchedUri = uri);

            command.Execute("https://github.com");

            Assert.Equal(new Uri("https://github.com"), launchedUri);
            Assert.Equal(new Uri("https://github.com"), command.LastOpenedUri);
        }

        [Fact]
        public void Execute_WhenCannotExecute_DoesNothing()
        {
            bool launcherCalled = false;
            var command = new OpenUrlCommand(
                uriProvider: () => null,
                canExecute: null,
                launcher: _ => launcherCalled = true);

            command.Execute(null);

            Assert.False(launcherCalled);
            Assert.Null(command.LastOpenedUri);
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new OpenUrlCommand("https://example.com");
            bool eventFired = false;
            command.CanExecuteChanged += (_, _) => eventFired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(eventFired);
        }
    }
}
