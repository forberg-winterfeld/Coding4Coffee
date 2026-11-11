# Coding4Coffee

A comprehensive .NET 10 suite of libraries for building modern, localizable Windows Forms applications with strong MVVM support, configuration management, and command-based UI controls.

## 📦 Libraries

### [Coding4Coffee.Basics](./Coding4Coffee.Basics)
A lightweight, modern .NET 10 utility library providing:
- **Configuration Management**: Strongly-typed configuration wrapper with JSON serialization and optional persistence
- **Dynamic Localization**: Runtime culture switching with automatic UI updates
- **MVVM Commands**: Ready-to-use `ICommand` implementations

**Install via NuGet:**
```bash
dotnet add package Coding4Coffee.Basics
```

### [Coding4Coffee.WinForms](./Coding4Coffee.WinForms)
A modern Windows Forms library extending standard controls with:
- **Localizable Controls**: 45 localizable WinForms controls (Forms, Buttons, TextBoxes, DataGridViews, MenuStrips, ToolStrips, Dialogs, etc.)
- **Command-Enabled Controls**: Bind `ICommand` implementations to standard WinForms controls
- **Form Commands**: Reusable commands for managing modeless forms
- **UI Configuration**: Specialized serialization for WinForms types (Colors, Fonts, Icons, Images)

**Install via NuGet:**
```bash
dotnet add package Coding4Coffee.WinForms
```

## 🚀 Quick Start

### Basic Configuration
```csharp
using Coding4Coffee.Basics.Configuration;

// Retrieve or create strongly-typed config settings
var themeConfig = ConfigFactory.GetConfig<string>("AppTheme");
themeConfig.Value = "Dark";
string? theme = themeConfig; // Implicit conversion
```

### Localization
```csharp
using Coding4Coffee.Basics.Localization;

// Assign your application's ResourceManager
BasicsLocalizationExtensions.ResourceManager = MyResources.ResourceManager;

// Change application culture at runtime
LocalizationManager.CurrentCulture = new CultureInfo("de-DE");

// Use localizable strings
var greeting = new LocalizableString("Greeting_Hello", "Hello World!");
Console.WriteLine(greeting.ToString());
```

### Localizable WinForms Controls
```csharp
using Coding4Coffee.WinForms.Localization.Controls;

// Derive forms from LocalizableForm
public class MainForm : LocalizableForm
{
	public MainForm()
	{
		TextResourceKey = "MainForm_Title";

		var saveButton = new LocalizableButton
		{
			TextResourceKey = "Btn_Save_Text",
			Text = "Save" // Fallback text
		};

		Controls.Add(saveButton);
	}
}
```

### Command Binding
```csharp
using Coding4Coffee.WinForms.Commands;

// Bind an ICommand to Button or ToolStripMenuItem
var saveButton = new Button();
saveButton.BindCommand(saveCommand, commandParameter);

var exitMenuItem = new ToolStripMenuItem();
exitMenuItem.BindCommand(new CloseFormCommand(this));

// Open a modeless form
var openSettingsCommand = new OpenFormCommand<SettingsForm>();
openSettingsCommand.Execute(null);
```

## 🏗️ Project Structure

```
Coding4Coffee/
├── Coding4Coffee.Basics/              # Core utilities library
│   ├── Configuration/                 # Configuration management
│   ├── Localization/                  # Localization support
│   └── Commands/                      # MVVM command implementations
├── Coding4Coffee.Basics.Tests/        # Unit tests
├── Coding4Coffee.WinForms/            # Windows Forms extensions
│   ├── Localization/Controls/         # Localizable controls
│   ├── Commands/Controls/             # Command-enabled controls
│   ├── Commands/                      # Form management commands
│   ├── Configuration/                 # UI configuration serializers
│   └── README.md
├── Coding4Coffee.WinForms.Tests/      # Unit tests
└── README.md                          # This file
```

## ✨ Features

### Coding4Coffee.Basics
- ✅ 7 specialized config types with custom serialization
- ✅ Runtime culture switching with event notifications
- ✅ Strongly-typed configuration with implicit casting
- ✅ Optional JSON persistence via `SettingsBase`
- ✅ 6 pre-built `ICommand` implementations (including framework-agnostic lifecycle commands)

### Coding4Coffee.WinForms
- ✅ 45 localizable WinForms controls
- ✅ Automatic localization on culture change
- ✅ Command binding for ButtonBase and ToolStripMenuItem
- ✅ 21+ commands for form management, dialogs, clipboard, and system integration
- ✅ Custom serialization for Colors, Fonts, Icons, and Images
- ✅ 21 command-enabled control types with intelligent parameter fallback and debounce support

## 📝 Usage Examples

### Example 1: Create a Localizable Form
```csharp
using Coding4Coffee.WinForms.Localization.Controls;

public class SettingsForm : LocalizableForm
{
	private LocalizableButton _okButton;
	private LocalizableButton _cancelButton;

	public SettingsForm()
	{
		TextResourceKey = "SettingsForm_Title";

		_okButton = new LocalizableButton
		{
			TextResourceKey = "Btn_OK",
			Dock = DockStyle.Right
		};

		_cancelButton = new LocalizableButton
		{
			TextResourceKey = "Btn_Cancel",
			Dock = DockStyle.Right
		};

		Controls.Add(_okButton);
		Controls.Add(_cancelButton);
	}
}
```

### Example 2: Configuration with Persistence
```csharp
using Coding4Coffee.Basics.Configuration;

// Optional: Enable persistence
ConfigFactory.Settings = Properties.Settings.Default;

// Get or create config
var lastOpenedPath = ConfigFactory.GetDirectoryInfoConfig("LastOpenedPath");
lastOpenedPath.Value = new DirectoryInfo(@"C:\Documents");

// Values are automatically persisted if Settings is assigned
```

### Example 3: Dynamic Localization
```csharp
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.Basics.Localization.Commands;

// Subscribe to culture changes
LocalizationManager.CultureChanged += (oldCulture, newCulture) =>
{
	Console.WriteLine($"Culture changed from {oldCulture} to {newCulture}");
};

// Change culture at runtime  
LocalizationManager.CurrentCulture = new CultureInfo("fr-FR");

// Or use the command
SetCultureCommand.Instance.Execute("es-ES");
```

## 🧪 Testing

Both libraries include comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Coding4Coffee.Basics.Tests
dotnet test Coding4Coffee.WinForms.Tests
```

## 📄 License

This project is distributed under the **MIT License**.

## 🔗 Related Documentation

- [Coding4Coffee.Basics README](./Coding4Coffee.Basics/README.md)
- [Coding4Coffee.WinForms README](./Coding4Coffee.WinForms/README.md)

---

**Made with ☕ by the Coding4Coffee team**

## ☕ Support This Project

If you find this project helpful and would like to support its development, consider buying me a coffee!

[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support%20me-FF5E5B?style=flat-square&logo=kofi&logoColor=white)](https://ko-fi.com/forbergwinterfeld)

*Your support helps fuel the development and maintenance of Coding4Coffee. Thank you!* ❤️

