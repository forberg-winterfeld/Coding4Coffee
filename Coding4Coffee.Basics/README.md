# Coding4Coffee.Basics

A lightweight, modern .NET 10 and .NET 11 utility library providing strongly-typed configuration management, dynamic runtime localization, and reusable `ICommand` implementations.

## Features

- ⚙️ **Configuration Management**: Strongly-typed configuration wrapper (`Config<TValue>`) with JSON serialization and optional persistence via `SettingsBase`. Includes specialized handlers for 7 types with custom serialization needs (`CultureInfoConfig`, `DirectoryInfoConfig`, `FileInfoConfig`, `UriConfig`, `IPAddressConfig`, `VersionConfig`, `EncodingConfig`) and implicit cast operators.
- 🌐 **Dynamic Localization**: Runtime culture switching via `LocalizationManager`, localizable strings (`LocalizableString`) with formatting and fallback values, and automatic culture change notifications.
- ⚡ **MVVM Commands**: Ready-to-use `ICommand` implementations: `AlwaysExecutableCommand`, `AsyncCommandBase`, `AsyncCommandBase<T>`, `SetCultureCommand`, and framework-agnostic interfaces for application lifecycle management (`IExitApplicationCommand`, `IRestartApplicationCommand`).
- 🔁 **Enumerable Extensions**: `ForEach` helpers for synchronous iteration with cancellation support, including an overload that passes the token to each action.

## Installation

Install via NuGet Package Manager CLI:

```bash
dotnet add package Coding4Coffee.Basics
```

## Quick Start

### 1. Configuration

```csharp
using Coding4Coffee.Basics.Configuration;
using System.Globalization;
using System.IO;

// Optional: Assign SettingsBase (e.g. Properties.Settings.Default) to persist values
// ConfigFactory.Settings = Properties.Settings.Default;

// Retrieve or create strongly-typed config settings
var themeConfig = ConfigFactory.GetConfig<string>("AppTheme");
themeConfig.Value = "Dark";
string? theme = themeConfig; // Implicit conversion

var timeoutConfig = ConfigFactory.GetConfig<int>("TimeoutSeconds");
timeoutConfig.Value = 30;
int timeout = timeoutConfig; // Implicit conversion

// Use specialized configs for types with custom serialization
var cultureConfig = ConfigFactory.GetCultureInfoConfig("AppCulture");
cultureConfig.Value = new CultureInfo("de-DE");
CultureInfo? culture = cultureConfig;

var dataDirConfig = ConfigFactory.GetDirectoryInfoConfig("DataDirectory");
dataDirConfig.Value = new DirectoryInfo(@"C:\AppData");
DirectoryInfo? dataDir = dataDirConfig;
```

### 2. Localization

```csharp
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.Basics.Localization.Commands;
using System.Globalization;

// 1. Assign your application's ResourceManager
BasicsLocalizationExtensions.ResourceManager = MyResources.ResourceManager;

// 2. Optionally configure culture persistence key (auto-saves/loads via ConfigFactory)
LocalizationManager.CultureConfigKey = "AppCulture";

// 3. Change application culture at runtime (notifies subscribers via CultureChanged event)
LocalizationManager.CurrentCulture = new CultureInfo("de-DE");

// 4. Use LocalizableString with resource key and fallback value
var greeting = new LocalizableString("Greeting_Hello", "Hello World!");
Console.WriteLine(greeting.ToString());
string text = greeting; // Implicit conversion to string

// 5. Or switch cultures using SetCultureCommand (e.g. in UI bindings)
SetCultureCommand.Instance.Execute("en-US");

// 6. Changes through the same configuration are observed as well
ConfigFactory.GetCultureInfoConfig("AppCulture").Value = CultureInfo.GetCultureInfo("de-DE");
```

Changes to the bound `CultureInfoConfig` also update the current culture and raise `CultureChanged`. Clearing its value keeps the current culture; use `ResetToSystemCulture` to apply a fallback and clear the setting. Changing or clearing `CultureConfigKey` detaches the old configuration. Configuration and culture change notifications run synchronously on the calling thread; make these changes on the UI thread when UI controls subscribe. Configuration and persistence errors are propagated to the caller.

### 3. Enumerable Extensions

Use `ForEach` from `Coding4Coffee.Basics.Extensions` to run an action for each item in an enumerable. Both overloads check the cancellation token before each iteration. The first overload also passes the token to the action:

```csharp
using Coding4Coffee.Basics.Extensions;
using System.Linq;
using System.Threading;

var numbers = Enumerable.Range(1, 5);

numbers.ForEach((number, cancellationToken) =>
{
    Console.WriteLine(number);
    cancellationToken.ThrowIfCancellationRequested();
}, CancellationToken.None);
```

When the action does not need the token, use the `Action<T>` overload:

```csharp
numbers.ForEach(number => Console.WriteLine(number), CancellationToken.None);
```

Cancellation is observed between iterations and causes `OperationCanceledException` to be thrown. The token-aware overload also lets the action observe cancellation while it is running.

## API Reference

### Configuration Management

#### ConfigFactory
- `GetConfig<T>(string key)` - Get or create a generic configuration
- `GetCultureInfoConfig(string key)` - Get or create a CultureInfo configuration
- `GetDirectoryInfoConfig(string key)` - Get or create a DirectoryInfo configuration
- `GetFileInfoConfig(string key)` - Get or create a FileInfo configuration
- `GetUriConfig(string key)` - Get or create a Uri configuration
- `GetIPAddressConfig(string key)` - Get or create an IPAddress configuration
- `GetVersionConfig(string key)` - Get or create a Version configuration
- `GetEncodingConfig(string key)` - Get or create an Encoding configuration

#### Config<T>
- `Value` - Get or set the configuration value
- Implicit cast to `T?` for easy access

### Enumerable Extensions

#### IEnumerableExtensions
Namespace: `Coding4Coffee.Basics.Extensions`

- `ForEach<T>(IEnumerable<T>, Action<T, CancellationToken>, CancellationToken)` - Invokes the action for each item, checking cancellation before each iteration and passing the token to the action.
- `ForEach<T>(IEnumerable<T>, Action<T>, CancellationToken)` - Invokes the action for each item, checking cancellation before each iteration without passing the token to the action.

Both overloads are synchronous. If cancellation is requested before an iteration, they throw `OperationCanceledException`; they also throw `ArgumentNullException` if the collection or action is `null`.

### Localization Management

#### LocalizationManager
- `CurrentCulture` - Get or set the current application culture
- `CultureConfigKey` - Get or set the key for persisting culture preference
- `CultureChanged` - Event raised when culture changes

#### LocalizableString
- Constructor: `LocalizableString(string resourceKey, string fallbackValue)`
- `ToString()` - Get the localized string value
- Implicit cast to `string`

### Application Lifecycle Commands

Framework-agnostic command interfaces for managing application lifecycle. Framework-specific implementations (WinForms, WPF, MAUI, etc.) can be found in their respective packages.

#### IExitApplicationCommand
Abstraction for application exit command implementations across different UI frameworks.

**Contract:**
- Implements `ICommand` interface
- Event `Exiting` - Raised immediately before application exit is initiated
- Method `RaiseCanExecuteChanged()` - Signals that the command's ability to execute may have changed

**Framework Implementations:**
- **WinForms**: `Coding4Coffee.WinForms.Commands.ExitApplicationCommand` - Uses `Application.Exit()`

**Example Usage (Framework-Agnostic):**
```csharp
using Coding4Coffee.Basics.Commands;

public void SetupExitCommand(IExitApplicationCommand exitCommand)
{
    exitCommand.Exiting += (_, e) => Console.WriteLine("Application is exiting...");
    exitCommand.Execute(null);
}
```

---

#### IRestartApplicationCommand
Abstraction for application restart command implementations across different UI frameworks.

**Contract:**
- Implements `ICommand` interface
- Event `Restarting` - Raised immediately before application restart is initiated
- Method `RaiseCanExecuteChanged()` - Signals that the command's ability to execute may have changed

**Framework Implementations:**
- **WinForms**: `Coding4Coffee.WinForms.Commands.RestartApplicationCommand` - Uses `Application.Restart()`

**Example Usage (Framework-Agnostic):**
```csharp
using Coding4Coffee.Basics.Commands;

public void SetupRestartCommand(IRestartApplicationCommand restartCommand)
{
    restartCommand.Restarting += (_, e) => Console.WriteLine("Application is restarting...");
    restartCommand.Execute(null);
}
```

---

## License

Distributed under the MIT License.
