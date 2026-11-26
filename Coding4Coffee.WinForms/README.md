# Coding4Coffee.WinForms

A modern Windows Forms library for .NET 10 and .NET 11 providing localizable UI controls, MVVM-style command binding for controls, and form management commands.

## Features

- 🎨 **Localizable WinForms Controls**: Built-in dynamic localization support for 45 controls including `LocalizableForm`, `LocalizableButton`, `LocalizableLabel`, `LocalizableTextBox`, `LocalizableCheckBox`, `LocalizableRadioButton`, `LocalizableComboBox`, `LocalizableListBox`, `LocalizableCheckedListBox`, `LocalizableDataGridView`, `LocalizableGroupBox`, `LocalizableMenuStrip`, `LocalizableContextMenuStrip`, `LocalizableStatusStrip`, `LocalizableToolStrip`, and more.
- ⚡ **Command-Enabled Controls**: Binds `ICommand` implementations to WinForms controls, including `LocalizableButtonWithCommandSupport` and `NumericUpDownWithCommandSupport`.
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

Changes to the `CultureInfoConfig` bound through `LocalizationManager.CultureConfigKey` also trigger localization. Change the configuration on the UI thread because `CultureChanged` and control localization run synchronously on the calling thread.

### 2. Command Binding & Form Commands

```csharp
using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.Controls;
using Coding4Coffee.WinForms.Commands.Extensions;

// Bind an ICommand to a Button or ToolStripMenuItem
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

### Temporarily reveal a password

```csharp
using Coding4Coffee.WinForms.Commands;
using Coding4Coffee.WinForms.Commands.Extensions;

// passwordTextBox is initially masked, for example with PasswordChar = '*'.
var showPassword = new ShowPasswordCommand(passwordTextBox, delayMilliseconds: 3000, passwordChar: '*');
showPasswordButton.BindCommand(showPassword);
```

The delay is specified in milliseconds and defaults to 5000; it must be positive. The masking character defaults to `'*'` and must not be `\0`. Mask the text box before the command is first executed. Execute on the text box's owning UI thread after its handle has been created, with a running WinForms message loop. While the command is executing, further invocations are ignored. When the delay ends or the command is cancelled, the text is masked with the configured character. The command disables `UseSystemPasswordChar` and does not restore the previous masking settings. Use one command per text box and do not change its masking settings while it is running. Cancellation and execution errors, including an invalid execution context or an already disposed text box, are reported through `ExecutionFailed`. If the text box is disposed during the delay, it is not accessed again.

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
using Icon? icon = appIcon; // Implicit conversion creates a caller-owned icon

var logoImage = WinFormsConfigFactory.GetImageConfig("App_LogoImage");
using Image? logo = logoImage.Value; // Each read creates a caller-owned image
```

Keep the returned icon or image alive for as long as it is in use, including by a control. Dispose it only after the control no longer needs it. See [Configuration Types](#configuration-types) for ownership details.

### Configuration-bound TextBox

```csharp
using Coding4Coffee.WinForms.Configuration.Controls;

var textBox = new ConfigurationTextBox { ConfigKey = "App_UserName", DebounceInterval = 300 };
```

User and programmatic text changes are stored after the configured delay without further edits (300 ms by default; minimum 50 ms). This requires a running WinForms message loop. Pending text is saved when changing or clearing the key or disposing the control, unless an external configuration change is already pending. An external configuration change takes precedence over pending text. Clearing the key preserves the displayed text even if an external update is queued. Configuration notifications from other threads are applied on the UI thread; the configuration itself is not thread-safe.

The same binding and debounce behavior is available for masked and rich text input:

```csharp
var phone = new ConfigurationMaskedTextBox
{
    Mask = "000-0000",
    TextMaskFormat = MaskFormat.ExcludePromptAndLiterals,
    ConfigKey = "App_Phone"
};
var notes = new ConfigurationRichTextBox { ConfigKey = "App_Notes", DebounceInterval = 300 };
```

Configure the mask and `TextMaskFormat` before assigning `ConfigKey` in code. The mask controls which characters can be displayed, while `TextMaskFormat` controls the text saved to `Config<string>`; a stored value that does not fit the mask is not overwritten merely by loading it. The rich text box persists plain `Text`, not RTF or formatting. Both controls inherit the standard WinForms text boxes rather than the localizable variants, so localization cannot overwrite configuration-bound user input. The designer defers configuration binding until initialization completes and does not access application settings in design mode.

### 4. Configuration-bound ComboBox

```csharp
using Coding4Coffee.WinForms.Configuration.Controls;

var combo = new ConfigurationComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
combo.Items.AddRange(["Light", "Dark", "System"]);
combo.ConfigKey = "App_ThemeSelection";
```

`ConfigurationComboBox` inherits `LocalizableComboBox`. Set the designer-visible `SelectionStorageMode` before assigning `ConfigKey` in code. The designer stores the key without accessing application settings. In `Value` mode, the data source and value member may be assigned later; the saved selection is applied when data binding is ready. In `Index` and `Text` modes, populate the items before assigning the key. The modes are:

- `Index` (default): stores `SelectedIndex` in `Config<int?>`; item order must remain stable.
- `Value`: stores the selected item's non-null string `ValueMember` in `Config<string>`. Set `DataSource`, `DisplayMember`, and `ValueMember`; use unique, language-independent values to survive localized sorting.
- `Text`: stores the displayed item text in `Config<string>`. Localized or duplicate text can prevent reliable restoration across languages.

For example, with a data source of items exposing string `Id` and `Label` properties:

```csharp
var combo = new ConfigurationComboBox
{
    SelectionStorageMode = ConfigurationComboBoxValueMode.Value,
    ConfigKey = "App_ThemeId",
    DisplayMember = "Label",
    ValueMember = "Id",
    DataSource = choices
};
```

When `ConfigKey` is set in the designer, `DataSource`, `DisplayMember`, and `ValueMember` can likewise be assigned after `InitializeComponent()`. The initial data binding and localization refresh do not overwrite the saved selection. User and programmatic selection changes are saved; no selection is stored as `null`. A value without a matching item clears the selection without overwriting the setting. Duplicate string matches select the first item. Clearing the key unbinds without changing the selection. Because `ConfigFactory` associates each key with one configuration type, changing between `Index` and the string modes requires unbinding and using a key of the appropriate type. Free-form text is not stored. Configuration notifications from other threads are applied on the UI thread, but the configuration itself is not thread-safe.

### 5. Configuration-bound CheckBox

```csharp
using Coding4Coffee.WinForms.Configuration.Controls;

var checkBox = new ConfigurationCheckBox
{
    TextResourceKey = "UseSystemTheme",
    ConfigKey = "App_UseSystemTheme"
};
```

`ConfigurationCheckBox` inherits `LocalizableCheckBox`. By default it synchronizes `CheckState` with a `Config<CheckState>` in both directions. An unset setting defaults to `Unchecked`. Set `ThreeState` to `true` to let users select `Indeterminate` as well.

To share a Boolean setting with other application code, select Boolean storage before assigning the key:

```csharp
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

var checkBox = new ConfigurationCheckBox
{
    ValueMode = ConfigurationCheckBoxValueMode.Boolean,
    ConfigKey = "App_UseSystemTheme"
};

bool useSystemTheme = ConfigFactory.GetConfig<bool>("App_UseSystemTheme").Value;
```

Boolean mode uses `Config<bool>` and defaults to `false`. `Indeterminate` maps to `true` and is normalized to `Checked`; use the default `CheckState` mode if all three states must be retained. The designer-visible `ValueMode` must be set before `ConfigKey` in code; designer initialization also supports the opposite property order. Unbind before changing modes, and use a key associated with the matching configuration type. Both user and programmatic changes are stored. A null, empty, or whitespace `ConfigKey` unbinds without changing the current state. Configuration notifications from other threads are applied on the UI thread, but the configuration itself is not thread-safe.

### Configuration-bound numeric controls

```csharp
using Coding4Coffee.WinForms.Configuration.Controls;

var timeout = new ConfigurationNumericUpDown
{
    Minimum = 1, Maximum = 120, DecimalPlaces = 1, ConfigKey = "App_Timeout"
};
var volume = new ConfigurationTrackBar
{
    Minimum = 0, Maximum = 100, DebounceInterval = 300, ConfigKey = "App_Volume"
};
```

`ConfigurationNumericUpDown` uses `Config<decimal>` and saves user and programmatic value changes immediately. `ConfigurationTrackBar` uses `Config<int>` and saves them after a 300 ms pause by default; `DebounceInterval` is designer-visible and has a 10 ms minimum. A running WinForms message loop is required for the timer. Pending track bar edits are saved on unbinding, rebinding or disposal, unless a configuration update is already pending. External configuration changes take precedence over pending edits.

An unset setting defaults to zero. Values outside `Minimum` and `Maximum` are displayed at the nearest bound without changing the saved setting. Set these bounds on the concrete configuration control (or call `ConfigurationTrackBar.SetRange`) so the stored value is reapplied when the range changes; setting them through a base-class reference bypasses this behavior. Set bounds before `ConfigKey` in code, or use designer initialization. A blank key unbinds without changing the displayed value. The designer does not access application settings. Background configuration notifications are applied on the owning UI thread; the configuration itself is not thread-safe.

### Configuration-bound date/time picker

```csharp
using Coding4Coffee.Basics.Configuration;
using Coding4Coffee.WinForms.Configuration.Controls;

var picker = new ConfigurationDateTimePicker
{
    ShowCheckBox = true,
    ConfigKey = "App_ReminderDate"
};
DateTime? reminder = ConfigFactory.GetConfig<DateTime?>("App_ReminderDate").Value;
```

The picker stores `DateTime?` immediately. An unchecked checkbox represents `null`. Without the checkbox, a null setting leaves the current date displayed and remains null until the value changes. Out-of-range dates are displayed at the nearest bound without overwriting the setting. Set `MinDate`, `MaxDate`, `ShowCheckBox` and programmatic `Checked` through `ConfigurationDateTimePicker`, not a `DateTimePicker` reference, to preserve synchronization. Set bounds and checkbox visibility before `ConfigKey` in code; designer initialization handles property ordering.

### Configuration-bound radio button group

```csharp
using Coding4Coffee.WinForms.Configuration.Controls;
using Coding4Coffee.WinForms.Localization.Controls;

var group = new ConfigurationRadioButtonGroup();
group.Controls.Add(new LocalizableRadioButton { Tag = "light", Text = "Light", AutoSize = true, Location = new System.Drawing.Point(8, 24) });
group.Controls.Add(new LocalizableRadioButton { Tag = "dark", Text = "Dark", AutoSize = true, Location = new System.Drawing.Point(8, 48) });
group.ConfigKey = "App_Theme";
```

The group derives from `LocalizableGroupBox`; its direct-child radio buttons share a `Config<string>`. Assign each button a unique, nonempty string `Tag` in code or in the designer. Selecting a button stores its tag, not its localized text. Null or unmatched settings clear the selection without changing the setting; adding a matching button restores it. Duplicate tags select the first matching button. Unchecking a button alone does not clear the setting: set the config value to `null` to clear the group. After changing a tag, call `RefreshSelection()`. Selecting a button without a valid tag when bound throws `InvalidOperationException`.

For both controls, a blank key removes the binding without changing the displayed value; the designer does not access application configuration. Configuration notifications from other threads are applied on the owning UI thread, but the configuration itself is not thread-safe.

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
- `Button.BindCommand()` - Extension for buttons
- `ToolStripMenuItem.BindCommand()` - Extension for menu items

### Custom Command-Support Controls (22)

#### Standard Desktop Controls (3)
- `DateTimePickerWithCommandSupport` - Executes command on value change
- `NumericUpDownWithCommandSupport` - Executes command on value change
- `TrackBarWithCommandSupport` - Executes the command after scrolling pauses (300 ms default; 10 ms minimum)

#### Localizable Desktop Controls (13)
- `LocalizableButtonWithCommandSupport` - Rechecks `CanExecute` and updates `Enabled` when `CommandParameter` changes, unlike the native button. Changes inside the same parameter object still require `CanExecuteChanged`.
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
- Each command instance manages its own form instance

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
copySelectedCommand.Copied += (_, e) => ShowNotification($"Copied: {e.Text}");
```

**Characteristics:**
- Retries clipboard operations when the clipboard is locked; failures can still propagate
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
- `MessageBoxClosed` event for notification when the modal dialog closes
- Supports standard MessageBox configurations

---

#### ShowTaskDialogCommand
**Purpose:** Displays a modern Task Dialog (Windows Vista+).

**Usage:**
```csharp
var taskDialogCommand = new ShowTaskDialogCommand(
    heading: "Have you made any changes?",
    text: "Your changes will be lost if you don't save.",
    caption: "Save changes?",
    buttons: MessageBoxButtons.YesNo);
taskDialogCommand.Execute(mainForm);
```

**Characteristics:**
- Uses the native WinForms task dialog
- Configurable through `TaskDialogSetupData`
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
    setupDialog: dialog => dialog.Filter = "Text Files|*.txt|All Files|*.*");
openDialogCommand.DialogClosed += (_, e) =>
{
    if (e.DialogResult == DialogResult.OK)
        UseFile(e.Dialog.FileName);
};
openDialogCommand.Execute(this);
```

**Characteristics:**
- Supports `CommonDialog` types such as file and folder dialogs
- `LastDialogResult` contains the `DialogResult`; selected paths are accessible through the dialog in `DialogClosed`
- Flexible configuration via a setup action or factory function

---

### Workflow & Execution Control

#### ConfirmAndExecuteCommand
**Purpose:** Shows a confirmation prompt and executes an inner command only if user approves.

**Usage:**
```csharp
var innerCommand = new ExitApplicationCommand();
var confirmCommand = new ConfirmAndExecuteCommand(
    innerCommand,
    heading: "Are you really sure you want to exit?",
    caption: "Confirmation Required");
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

### IconConfig and ImageConfig ownership

`IconConfig` stores Base64-encoded icon data; `ImageConfig` stores images as Base64-encoded PNG data. Each successful `Value` read or implicit conversion creates a separate `Icon` or `Image`. The caller owns the returned object and must dispose it when it is no longer needed. Empty, whitespace, or invalid stored data returns `null`.

Assigning an icon or image to `Value` does not transfer ownership: the caller remains responsible for disposing any supplied disposable object it owns. The configuration serializes the supplied value and does not retain it. `SystemIcons.Application` in the quick start is a shared system icon, not an object created by the caller.

For `ImageConfig`, the source stream remains open while the returned image is reachable; its memory can be reclaimed after the image is no longer reachable. Disposing the image does not immediately release that stream's memory. When displaying a returned image or icon in a control, retain it until the control has finished using it before disposing it.

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
- `Button.BindCommand(ICommand command, object? parameter = null)` - Binds command to button
- `ToolStripMenuItem.BindCommand(ICommand command, object? parameter = null)` - Binds command to menu item
- Custom controls provide type-specific `Bind*Command()` methods for fluent chaining:
  - Single commands: `BindCommand(ICommand, object?)`
  - Multiple commands (e.g., DataGridView): `BindCellClickCommand()`, `BindCellDoubleClickCommand()`, `BindCellValueChangedCommand()`, etc.
  - Navigation: `BindItemClickedCommand()`, `BindPositionChangedCommand()` (BindingNavigator)

`LocalizableDataGridViewWithCommandSupport` debounces `CellValueChangedCommand` globally: changes within the interval are coalesced, and the last changed cell supplies the default parameter even if selection changes. The default interval is 300 ms, the minimum is 50 ms, and a running WinForms message loop is required. Programmatic changes are included. When the timer fires, the command receives the explicit parameter, or the cell's current value, or its row's `DataBoundItem` if that value is null; it runs only if `CanExecute` permits it. Removing the cell, replacing or clearing the command, or disposing the grid discards the pending execution.

## Best Practices

1. **Always set fallback text** - Provide default text for controls in case the resource key is not found
2. **Use LocalizableForm** - Derive your main forms from `LocalizableForm` for automatic localization
3. **Resource naming** - Use a consistent naming convention for resource keys (e.g., `FormName_ControlName`)
4. **Persist configuration** - Assign `ConfigFactory.Settings` to enable automatic persistence of UI state
5. **Command parameters** - Pass relevant data as command parameters instead of storing state in the form

## License

Distributed under the MIT License.
