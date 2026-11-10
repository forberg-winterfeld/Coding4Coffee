# Coding4Coffee.WinForms

A modern Windows Forms library for .NET 10 providing localizable UI controls, MVVM-style command binding for controls, and form management commands.

## Features

- 🎨 **Localizable WinForms Controls**: Built-in dynamic localization support for 45 controls including `LocalizableForm`, `LocalizableButton`, `LocalizableLabel`, `LocalizableTextBox`, `LocalizableCheckBox`, `LocalizableRadioButton`, `LocalizableComboBox`, `LocalizableListBox`, `LocalizableCheckedListBox`, `LocalizableDataGridView`, `LocalizableGroupBox`, `LocalizableMenuStrip`, `LocalizableContextMenuStrip`, `LocalizableStatusStrip`, `LocalizableToolStrip`, and more.
- ⚡ **Command-Enabled Controls**: Binds `ICommand` implementations to standard WinForms controls (`ButtonBase.BindCommand()`, `ToolStripMenuItem.BindCommand()`, and `NumericUpDownWithCommandSupport`).
- 🚪 **Form Commands**: Reusable commands to open modeless forms with lifetime & activate management (`OpenFormCommand<TForm>`) and close forms (`CloseFormCommand`).
- 🎯 **WinForms Configuration**: Specialized configs for UI types with custom serialization (`ColorConfig`, `FontConfig`, `IconConfig`, `ImageConfig` via `WinFormsConfigFactory`), while standard types (`Point`, `Size`, `FormWindowState`) are supported directly through `ConfigFactory.GetConfig<T>()`.

## Installation

Install via NuGet Package Manager CLI:

```bash
dotnet add package Coding4Coffee.WinForms
```

## Quick Start

### 1. Localizable Controls & Forms

```csharp
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Localization.Controls;

// 1. Assign application-wide ResourceManager
BasicsLocalizationExtensions.ResourceManager = MyResources.ResourceManager;

// 2. Derive forms from LocalizableForm:
// Automatically localizes the form and all contained ILocalizable child controls on Load
// and whenever LocalizationManager.CurrentCulture changes.
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

### 2. Command Binding & Form Commands

```csharp
using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.Controls;

// Bind an ICommand to any ButtonBase or ToolStripMenuItem
var saveButton = new Button();
saveButton.BindCommand(saveCommand, commandParameter);

var exitMenuItem = new ToolStripMenuItem();
exitMenuItem.BindCommand(new CloseFormCommand(this));

// Open a modeless form (creates form on first execution, restores & brings to front if open)
var openSettingsCommand = new OpenFormCommand<SettingsForm>();
openSettingsCommand.Execute(null);

// NumericUpDown with automatic command execution on ValueChanged
var numericUpDown = new NumericUpDownWithCommandSupport
{
	Command = updateValueCommand
};
```

### 3. UI Configuration (State & Drawing Types)

```csharp
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration;
using System.Drawing;

// Standard types work directly with generic ConfigFactory.GetConfig<T>
var windowState = ConfigFactory.GetConfig<FormWindowState>("MainWindow_State");
var windowSize  = ConfigFactory.GetConfig<Size>("MainWindow_Size");

// Specialized WinForms types (Color, Font, Icon, Image) via WinFormsConfigFactory
var accentColor = WinFormsConfigFactory.GetColorConfig("App_AccentColor");
accentColor.Value = Color.DarkSlateBlue;
Color? color = accentColor; // Implicit conversion

var appFont = WinFormsConfigFactory.GetFontConfig("App_DefaultFont");
appFont.Value = new Font("Segoe UI", 10f);
Font? font = appFont; // Implicit conversion

var appIcon = WinFormsConfigFactory.GetIconConfig("App_WindowIcon");
appIcon.Value = SystemIcons.Application;
Icon? icon = appIcon; // Implicit conversion

var logoImage = WinFormsConfigFactory.GetImageConfig("App_LogoImage");
Image? logo = logoImage; // Implicit conversion
```

## Localizable Controls

The library provides localizable versions of 45 major WinForms controls:

### Standard Controls (10)
- `LocalizableButton`
- `LocalizableCheckBox`
- `LocalizableLabel`
- `LocalizableTextBox`
- `LocalizableRadioButton`
- `LocalizableForm`
- `LocalizableGroupBox`
- `LocalizableLinkLabel`
- `LocalizablePanel`
- `LocalizableUserControl`

### Selection Controls (9)
- `LocalizableListBox`
- `LocalizableCheckedListBox`
- `LocalizableComboBox`
- `LocalizableTreeView`
- `LocalizableTreeNode`
- `LocalizableListView`
- `LocalizableDomainUpDown`
- `LocalizableTabControl`
- `LocalizableTabPage`

### Data Display (2)
- `LocalizableDataGridView`
- `LocalizableColumnHeader`

### Text Input (2)
- `LocalizableRichTextBox`
- `LocalizableMaskedTextBox`

### Menus & Toolbars (13)
- `LocalizableMenuStrip`
- `LocalizableToolStripMenuItem`
- `LocalizableContextMenuStrip`
- `LocalizableToolStrip`
- `LocalizableToolStripButton`
- `LocalizableToolStripLabel`
- `LocalizableToolStripDropDownButton`
- `LocalizableToolStripSplitButton`
- `LocalizableToolStripTextBox`
- `LocalizableToolStripComboBox`
- `LocalizableStatusStrip`
- `LocalizableToolStripStatusLabel`
- `LocalizableBindingNavigator`

### Dialogs (3)
- `LocalizableFolderBrowserDialog`
- `LocalizableOpenFileDialog`
- `LocalizableSaveFileDialog`

### Other Controls (6)
- `LocalizableToolTip`
- `LocalizableHelpProvider`
- `LocalizableFlowLayoutPanel`
- `LocalizableSplitContainer`
- `LocalizableTableLayoutPanel`
- `LocalizableNotifyIcon`

## Command-Enabled Controls

### Built-in Extensions
- `ButtonBase.BindCommand()` - Extension for Button, CheckBox, RadioButton, etc.
- `ToolStripMenuItem.BindCommand()` - Extension for menu items

### Custom Command-Support Controls (21)

#### Standard Desktop Controls (3)
- `DateTimePickerWithCommandSupport` - Executes command on value change
- `NumericUpDownWithCommandSupport` - Executes command on value change
- `TrackBarWithCommandSupport` - Executes command on value change

#### Localizable Desktop Controls (12)
- `LocalizableCheckBoxWithCommandSupport`
- `LocalizableComboBoxWithCommandSupport` - Uses SelectionChangeCommitted for user-only changes + SelectedItem default
- `LocalizableCheckedListBoxWithCommandSupport`
- `LocalizableDomainUpDownWithCommandSupport`
- `LocalizableLinkLabelWithCommandSupport`
- `LocalizableListBoxWithCommandSupport`
- `LocalizableListViewWithCommandSupport`
- `LocalizableMaskedTextBoxWithCommandSupport`
- `LocalizableRadioButtonWithCommandSupport`
- `LocalizableRichTextBoxWithCommandSupport`
- `LocalizableTextBoxWithCommandSupport` - Debounced (300ms default) + Text default parameter
- `LocalizableTreeViewWithCommandSupport`

#### Specialized Controls (3)
- `LocalizableDataGridViewWithCommandSupport` - 7 independent commands: CellClick, CellDoubleClick, CellValueChanged (debounced), CellBeginEdit, CellEndEdit, SelectionChanged, RowHeaderClick
- `LocalizableTabControlWithCommandSupport` - Executes on tab selection change with SelectedTab.Name default
- `LocalizableBindingNavigatorWithCommandSupport` - 2 commands: ItemClicked (for navigation buttons), PositionChanged (for data position changes)

#### ToolStrip Controls (3)
- `LocalizableToolStripComboBoxWithCommandSupport` - Uses SelectionChangeCommitted + SelectedItem default
- `LocalizableToolStripSplitButtonWithCommandSupport` - Uses ButtonClick for button portion only
- `LocalizableToolStripTextBoxWithCommandSupport` - Debounced (300ms default) + Text default parameter

### Form Management Commands
- `OpenFormCommand<TForm>` - Opens or activates a modeless form
- `CloseFormCommand` - Closes a form
- `ShowContextHelpCommand` - Shows context help for a control

## Commands Reference

Comprehensive documentation for all available commands in Coding4Coffee.WinForms:

### Form Management

#### CloseFormCommand
**Purpose:** Closes a specific form.

**Usage:**
```csharp
var closeCommand = new CloseFormCommand(myForm);
exitButton.BindCommand(closeCommand);
```

**Characteristics:** Monitors the form's lifecycle and prevents errors when closing an already-disposed form.

---

#### OpenFormCommand<TForm>
**Purpose:** Opens a form as a modeless window. Manages a single instance and restores it if already open.

**Usage:**
```csharp
var openSettingsCommand = new OpenFormCommand<SettingsForm>();
settingsButton.BindCommand(openSettingsCommand);

openSettingsCommand.Execute(null); // Opens or activates SettingsForm
```

**Characteristics:** 
- Creates the form on first execution
- Restores minimized windows and brings them to foreground
- `IsOpen` property indicates whether the form is open
- Maintains separate instances per form type

---

#### OpenFormAsDialogCommand<TForm>
**Purpose:** Opens a form as a modal dialog window (blocks other windows).

**Usage:**
```csharp
var openDialogCommand = new OpenFormAsDialogCommand<ConfirmDeleteForm>();
deleteButton.BindCommand(openDialogCommand);
```

**Characteristics:** Waits for dialog closure before continuing application execution.

---

#### ExitApplicationCommand
**Purpose:** Cleanly terminates the entire application.

**Usage:**
```csharp
var exitCommand = new ExitApplicationCommand();
exitMenuItem.BindCommand(exitCommand);

// With confirmation dialog
var exitWithConfirm = new ExitApplicationCommand(confirmExit: () => 
    MessageBox.Show("Are you sure you want to exit?", "Exit", 
    MessageBoxButtons.YesNo) == DialogResult.Yes);
```

**Characteristics:** 
- Differs from `CloseFormCommand` - exits entire application
- Allows each form to run its `FormClosing` events
- Optional confirmation callback

---

### Window State Management

#### MinimizeFormCommand
**Purpose:** Minimizes a specific form.

**Usage:**
```csharp
var minimizeCommand = new MinimizeFormCommand(myForm);
minimizeButton.BindCommand(minimizeCommand);
```

---

#### MaximizeFormCommand
**Purpose:** Maximizes a specific form.

**Usage:**
```csharp
var maximizeCommand = new MaximizeFormCommand(myForm);
maximizeButton.BindCommand(maximizeCommand);
```

---

#### RestoreFormCommand
**Purpose:** Restores a minimized or maximized form to normal size.

**Usage:**
```csharp
var restoreCommand = new RestoreFormCommand(myForm);
restoreButton.BindCommand(restoreCommand);
```

---

#### ToggleWindowStateCommand
**Purpose:** Toggles between maximized and normal window size.

**Usage:**
```csharp
var toggleCommand = new ToggleWindowStateCommand(myForm);
toggleButton.BindCommand(toggleCommand);
```

---

### User Input & Interaction

#### FocusControlCommand
**Purpose:** Sets keyboard focus to a specific control.

**Usage:**
```csharp
var focusCommand = new FocusControlCommand(searchTextBox) { SelectAllOnFocus = true };
searchMenuItem.BindCommand(focusCommand);
```

**Characteristics:**
- Optional: Automatically select text in TextBox when focused
- Supports multiple sources: fixed control, provider function, or command parameter
- Monitors control state changes and updates `CanExecute`

---

#### SwitchTabCommand
**Purpose:** Switches between tabs in a TabControl forward or backward.

**Usage:**
```csharp
var switchTabCommand = new SwitchTabCommand(tabControl);
nextTabButton.BindCommand(switchTabCommand, SwitchTabDirection.Next);
previousTabButton.BindCommand(switchTabCommand, SwitchTabDirection.Previous);
```

**Characteristics:**
- Requires `SwitchTabDirection.Next` or `Previous` as parameter
- Intelligent: Disables itself at boundary conditions
- Monitors tab count and active index

---

### Clipboard & System Integration

#### CopyToClipboardCommand
**Purpose:** Safely copies text or data to the Windows Clipboard.

**Usage:**
```csharp
// Parameter-driven
var copyCommand = new CopyToClipboardCommand();
copyButton.BindCommand(copyCommand, "Text to copy");

// With dynamic provider
var copySelectedCommand = new CopyToClipboardCommand(() => listBox.SelectedItem?.ToString());
copyCommand.Copied += (_, e) => ShowNotification($"Copied: {e.Text}");
```

**Characteristics:**
- Graceful handling of clipboard locks
- `LastCopiedData` property shows most recently copied data
- `Copied` event for notifications

---

#### OpenUrlCommand
**Purpose:** Opens a URL in the default web browser or associated system application.

**Usage:**
```csharp
// Static URL
var helpCommand = new OpenUrlCommand("https://coding4coffee.dev/docs");
helpButton.BindCommand(helpCommand);

// Parameter-driven
var openUrlCommand = new OpenUrlCommand();
openUrlCommand.Execute("https://github.com");
```

**Characteristics:**
- Security: Only predefined URI schemes (http, https, mailto) allowed
- Customizable: `AllowedSchemes` property
- `LastOpenedUri` property for auditing/logging

---

#### OpenFileCommand
**Purpose:** Opens a file with the default system application.

**Usage:**
```csharp
var openFileCommand = new OpenFileCommand("C:\\Documents\\report.pdf");
openFileButton.BindCommand(openFileCommand);
```

---

#### ShowInExplorerCommand
**Purpose:** Opens a file or folder in Windows Explorer and highlights the item.

**Usage:**
```csharp
var exploreCommand = new ShowInExplorerCommand("C:\\Documents\\report.pdf");
exploreButton.BindCommand(exploreCommand);
```

---

### Dialog & Message Boxes

#### ShowMessageBoxCommand
**Purpose:** Displays a configured message box and captures the result.

**Usage:**
```csharp
// Simple alert
var alertCommand = new ShowMessageBoxCommand("Data saved successfully.", "Success", 
    MessageBoxButtons.OK, MessageBoxIcon.Information);
alertCommand.Execute(this);

// Confirmation dialog with reaction
var confirmCommand = new ShowMessageBoxCommand(
    "Really delete this item?", "Confirm", 
    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
confirmCommand.MessageBoxClosed += (_, e) => 
{
    if (e.DialogResult == DialogResult.Yes)
        DeleteItem();
};
confirmCommand.Execute(this);
```

**Characteristics:**
- `LastDialogResult` property for result access
- `MessageBoxClosed` event for async handling
- Supports all standard MessageBox configurations

---

#### ShowTaskDialogCommand
**Purpose:** Displays a modern Task Dialog (Windows Vista+).

**Usage:**
```csharp
var taskDialogCommand = new ShowTaskDialogCommand(mainForm)
{
    Title = "Save changes?",
    MainInstruction = "Have you made any changes?",
    Content = "Your changes will be lost if you don't save."
};
taskDialogCommand.Execute(null);
```

**Characteristics:**
- More modern UI than `MessageBoxCommand`
- Highly configurable
- `TaskDialogClosed` event for results

---

#### OpenDialogCommand<TDialog>
**Purpose:** Opens a standard dialog (File Open, Save, etc.) and manages the result.

**Usage:**
```csharp
var openFileDialogCommand = new OpenDialogCommand<OpenFileDialog>();
openFileButton.BindCommand(openFileDialogCommand);

// With specific configuration
var openDialogCommand = new OpenDialogCommand<OpenFileDialog>(
    () => new OpenFileDialog 
    { 
        Filter = "Text Files|*.txt|All Files|*.*" 
    });
openDialogCommand.Execute(this);
```

**Characteristics:**
- Supports any dialog type
- `LastDialogResult` contains selected file/folder path
- Flexible configuration via factory function

---

### Workflow & Execution Control

#### ConfirmAndExecuteCommand
**Purpose:** Shows a confirmation prompt and executes an inner command only if user approves.

**Usage:**
```csharp
var innerCommand = new ExitApplicationCommand();
var confirmCommand = new ConfirmAndExecuteCommand(
    innerCommand, 
    "Are you really sure you want to exit?", 
    "Confirmation Required");
exitButton.BindCommand(confirmCommand);
```

**Characteristics:**
- Wraps commands for safety
- Delegates `CanExecute` to inner command
- Simple user confirmation before critical actions

---

#### RestartApplicationCommand
**Purpose:** Restarts the application.

**Usage:**
```csharp
var restartCommand = new RestartApplicationCommand();
restartButton.BindCommand(restartCommand);
```

**Characteristics:**
- Terminates current instance and launches new one
- Abstracts process management details

---

### Help & Documentation

#### ShowContextHelpCommand
**Purpose:** Shows context-sensitive help for a control.

**Usage:**
```csharp
var helpCommand = new ShowContextHelpCommand(myControl);
helpButton.BindCommand(helpCommand);
```

---

## Configuration Types

### Standard Types (via ConfigFactory)
- `Size`
- `Point`
- `FormWindowState`
- All types from `Coding4Coffee.Basics.Configuration`

### Specialized WinForms Types (via WinFormsConfigFactory)
- `Color` - with implicit conversion
- `Font` - with implicit conversion
- `Icon` - with implicit conversion
- `Image` - with implicit conversion

## API Reference

### WinFormsConfigFactory
- `GetColorConfig(string key)` - Get or create a Color configuration
- `GetFontConfig(string key)` - Get or create a Font configuration
- `GetIconConfig(string key)` - Get or create an Icon configuration
- `GetImageConfig(string key)` - Get or create an Image configuration

### LocalizableForm
- `TextResourceKey` - Gets or sets the resource key for the form title
- Automatically localizes all child `ILocalizable` controls
- Subscribes to culture change events

### Command Extensions & Fluent API
- `ButtonBase.BindCommand(ICommand command, object? parameter = null)` - Binds command to button
- `ToolStripMenuItem.BindCommand(ICommand command, object? parameter = null)` - Binds command to menu item
- Custom controls provide type-specific `Bind*Command()` methods for fluent chaining:
  - Single commands: `BindCommand(ICommand, object?)`
  - Multiple commands (e.g., DataGridView): `BindCellClickCommand()`, `BindCellDoubleClickCommand()`, `BindCellValueChangedCommand()`, etc.
  - Navigation: `BindItemClickedCommand()`, `BindPositionChangedCommand()` (BindingNavigator)

## Best Practices

1. **Always set fallback text** - Provide default text for controls in case the resource key is not found
2. **Use LocalizableForm** - Derive your main forms from `LocalizableForm` for automatic localization
3. **Resource naming** - Use a consistent naming convention for resource keys (e.g., `FormName_ControlName`)
4. **Persist configuration** - Assign `ConfigFactory.Settings` to enable automatic persistence of UI state
5. **Command parameters** - Pass relevant data as command parameters instead of storing state in the form

## License

Distributed under the MIT License.
