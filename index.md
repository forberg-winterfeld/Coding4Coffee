# Coding4Coffee Documentation

Welcome to the **Coding4Coffee** library documentation!

Coding4Coffee provides lightweight, modern C# libraries for **Configuration**, **Localization**, and **MVVM-style Commands** in .NET 10 and .NET 11 applications, including Windows Forms.

## Included Packages

### ☕ Coding4Coffee.Basics
Core utilities for application configuration and dynamic runtime localization:
- **Configuration**: Strongly-typed settings wrapper (`Config<TValue>`, `ConfigFactory`, `CultureInfoConfig`, `DirectoryInfoConfig`, `FileInfoConfig`).
- **Localization**: Dynamic culture management (`LocalizationManager`, `LocalizableString`, `SetCultureCommand`).
- **Commands**: Reusable `ICommand` implementations such as `AlwaysExecutableCommand`.

### ☕ Coding4Coffee.WinForms
Windows Forms controls and extensions with built-in localization and command binding support:
- **Localizable Controls**: `LocalizableForm`, `LocalizableButton`, `LocalizableLabel`, `LocalizableTextBox`, `LocalizableDataGridView`, `LocalizableToolStripMenuItem`, `LocalizableTreeNode`, etc.
- **Command Controls & Forms**: `NumericUpDownWithCommandSupport`, `OpenFormCommand`, `CloseFormCommand`, and command binding extensions.

---

For detailed API references, navigate to the **API Documentation** section in the header menu.
