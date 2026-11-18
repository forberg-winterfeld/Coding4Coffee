# Contributing to Coding4Coffee

Thank you for your interest in contributing to Coding4Coffee! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

Please review our [Code of Conduct](CODE_OF_CONDUCT.md) before participating in this project.

## Getting Started

### Prerequisites

- .NET 11 SDK and .NET 10 runtime (to build and test both target frameworks)
- Visual Studio 2022 (Community/Professional/Enterprise) or Visual Studio Code
- Git

### Setting Up Your Development Environment

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/Coding4Coffee.git
   cd Coding4Coffee
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/nolanrasinjaria/Coding4Coffee.git
   ```
4. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

### Building the Project

```bash
# Build all projects
dotnet build

# Run tests
dotnet test

# Build specific project
dotnet build Coding4Coffee.WinForms/Coding4Coffee.WinForms.csproj
```

## Development Guidelines

### Code Style

- Follow the [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use primary constructors (C# 12+) when appropriate
- Prefer null propagation (`?.`) over null checks
- Use explicit `new` keyword for property shadowing in derived classes
- Omit curly braces for single-statement if blocks

### Naming Conventions

- **Localizable Base Controls**: `LocalizableXxxControl`
- **Command-Support Controls**: `LocalizableXxxControlWithCommandSupport`
- **Commands**: `XxxCommand`
- **ToolStrip Variants**: `LocalizableToolStripXxxWithCommandSupport`
- **Private Fields**: `_camelCase`
- **Properties**: `PascalCase`

### Command-Support Control Implementation

When implementing command-support controls, follow this structure:

```csharp
public class LocalizableXxxWithCommandSupport(ILocalizable? parent = null) 
	: LocalizableXxx(parent)
{
	private ICommand? _command;
	private object? _commandParameter;

	/// <summary>
	/// Gets or sets the command to execute when the control's action occurs.
	/// </summary>
	public ICommand? Command
	{
		get => _command;
		set
		{
			if (_command == value)
				return;

			_command?.CanExecuteChanged -= Command_CanExecuteChanged;
			_command = value;
			_command?.CanExecuteChanged += Command_CanExecuteChanged;
			UpdateEnabledState();
		}
	}

	/// <summary>
	/// Gets or sets the parameter passed to the command.
	/// Defaults to a control-specific value (e.g., SelectedItem, Text) if null.
	/// </summary>
	public object? CommandParameter
	{
		get => _commandParameter;
		set => _commandParameter = value;
	}

	/// <summary>
	/// Binds a command to this control with optional command parameter.
	/// </summary>
	public LocalizableXxxWithCommandSupport BindCommand(ICommand command, object? parameter = null)
	{
		Command = command;
		CommandParameter = parameter;
		return this;
	}

	private void Command_CanExecuteChanged(object? sender, EventArgs e)
		=> UpdateEnabledState();

	private void UpdateEnabledState()
		=> Enabled = _command?.CanExecute(_commandParameter ?? GetDefaultParameter()) ?? true;

	private object GetDefaultParameter() => /* control-specific default */;
}
```

### XML Documentation

- Provide comprehensive XML documentation for all public members
- Use `<summary>`, `<remarks>`, `<param>`, `<returns>` tags
- Include example usage in remarks when appropriate
- Link to related types using `<see cref="..."/>`

### Testing

- Write unit tests for new features
- Maintain test coverage above 80%
- Use xUnit for testing
- Follow the AAA pattern (Arrange, Act, Assert)
- Mock external dependencies

### Commit Messages

Follow the conventional commit format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`

Example:
```
feat(commands): add debounce support for text input controls

- Implement configurable debounce timer (default 300ms)
- Add TextChanged event debouncing
- Update LocalizableTextBoxWithCommandSupport

Closes #42
```

## Submitting Changes

1. Ensure your code follows the style guidelines
2. Write or update tests for your changes
3. Document any new public APIs
4. Update relevant README files if needed
5. Commit with clear, descriptive messages
6. Push to your fork and submit a pull request

### Pull Request Process

1. Create a descriptive title for your PR
2. Fill out the PR template completely
3. Link any related issues (use `Closes #123`)
4. Ensure all CI checks pass
5. Request review from maintainers
6. Address review feedback in follow-up commits

## Reporting Issues

### Security Issues

**Do not** open a public issue for security vulnerabilities. Please see [SECURITY.md](SECURITY.md) for reporting instructions.

### Bug Reports

Use the [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md) and include:
- Clear reproduction steps
- Expected vs. actual behavior
- Environment details (.NET version, OS, VS version)
- Error messages or stack traces

### Feature Requests

Use the [Feature Request Template](.github/ISSUE_TEMPLATE/feature_request.md) and include:
- Clear description of the feature
- Use cases and benefits
- Proposed implementation (if applicable)

## Project Structure

```
Coding4Coffee/
├── Coding4Coffee.Basics/              # Core utilities library
│   ├── Configuration/                 # Configuration management
│   ├── Localization/                  # Localization support
│   └── Commands/                      # MVVM command implementations
├── Coding4Coffee.Basics.Tests/        # Unit tests for Basics
├── Coding4Coffee.WinForms/            # Windows Forms extensions
│   ├── Localization/Controls/         # Localizable controls
│   ├── Commands/Controls/             # Command-enabled controls
│   ├── Commands/                      # Form management commands
│   └── Configuration/                 # UI configuration serializers
├── Coding4Coffee.WinForms.Tests/      # Unit tests for WinForms
```

## Questions or Need Help?

- Open a [Discussion](https://github.com/nolanrasinjaria/Coding4Coffee/discussions) for questions
- Check existing [Issues](https://github.com/nolanrasinjaria/Coding4Coffee/issues) before reporting
- Review the [Wikis](https://github.com/nolanrasinjaria/Coding4Coffee/wiki) for documentation

## License

By contributing to Coding4Coffee, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🎉
