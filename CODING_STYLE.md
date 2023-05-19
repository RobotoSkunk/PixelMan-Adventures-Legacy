# Code Style
> Note: Not all the code follows this style... yet.
> 
> This is planned to be the new coding style for this project.

This document describes the recommended coding style for this project.
If you don't follow this style, we'll ask you to change your pull request to follow this style.


## Contents

1. [Indentation](#indentation)
2. [Variable declaration](#variable-declaration)
3. [Whitespace](#whitespace)
4. [Ennumeration](#ennumeration)
5. [Braces](#braces)
6. [Switch statements](#switch-statements)
7. [Line breaks](#line-breaks)
8. [Comments](#comments)
9. [Shaders](#shaders)
10. [Exceptions](#exceptions)


## Indentation
- Use tabs for indentation.
- Use 4 spaces for indentation in Markdown files.

## Variable declaration
- Use `readonly` for variables that are not changed after initialization, except:
    - Variables that are used in the inspector.
	- Variables that are going to be serialized.
- Use camelCase for variable names.
    - Ignore C# naming conventions for getters and setters.
- Declare each variable on a separate line, even if they are of the same type.
- Try to group variables by their type.
- Use descriptive variable names, avoid abbreviations and single letter names.
    - Exception: `i`, `j`, `k`, etc... for loop variables.
- Try to avoid using `var` for variables, except:
	- When the type is obvious.
	- When the type is long.
	- When the type is not important.

For example:
```csharp
// Bad
public float dwlbar = 0f;
public string prtxt, errstr;

// Good
public float downloadProgress = 0f;
public string progressText;
public string errorString;
```

## Whitespace
- Use blank lines to group statements.
- Use only one blank line between methods, variables, etc... if they are of the same type or group.
- Use two blank lines between groups of methods, variables, etc...
- Use only space after each keyword.
    - Exception: No space between return and `;`.
- For pointers or references, use a single space after `*`.
- No space after a cast.
- Single spaces around binary arithmetic operators `+`, `-`, `*`, `/`, `%`.
- Insert a single space before and after the colon in a range-based for loop.

For example:
```csharp
// Bad
public int *pointer;
if(1==1){
}

// Good
public int* pointer;
if (1 == 1) {
}
```

## Ennumeration
Ideally it should be one member per line.

Allwas add a trailing comma after the last member. This helps produce cleaner diffs.

The same rule in the braces section below applies here.

For example:
```csharp
// Bad
public enum MyEnum {
	First, Second, Third
}

// Good
public enum MyEnum {
	First,
	Second,
	Third,
}
```

Sometimes enum members are written in all caps. It's completely fine to do so.


## Braces
As a base rule, the left curly brace goes on the same line as the start of the statement.

For example:
```csharp
// Bad
if (1 == 1)
{
}

enum OperatingSystem
{
	Windows,
	MacOS,
	Linux
}


// Good
if (1 == 1) {
}

enum OperatingSystem {
	Windows,
	MacOS,
	Linux,
}
```

Exception: Function implementations, classes, structs and namespaces declarations always have the opnening brace
on the start of a line.

For example:
```csharp
void LogError(string message)
{
	Debug.LogError(message);
}

class Ball : MonoBehaviour
{
	// ...
}
```

Use curly braces even when the body of a conditional statement contains only one line.

For example:
```csharp
// Bad
if (1 == 1)
	Debug.Log("1 is equal to 1");

for (int i = 0; i < 10; i++)
	Debug.Log(i);


// Good
if (1 == 1) {
	Debug.Log("1 is equal to 1");
}

for (int i = 0; i < 10; i++) {
	Debug.Log(i);
}
```

Put `else` on the same line as the closing brace of the previous `if` statement.

For example:
```csharp
// Bad
if (1 == 1) {
	Debug.Log("1 is equal to 1");
}
else {
	Debug.Log("1 is not equal to 1");
}


// Good
if (1 == 1) {
	Debug.Log("1 is equal to 1");
} else {
	Debug.Log("1 is not equal to 1");
}
```

## Switch statements
Case labels are indented from the switch statement.

For example:
```csharp
switch (age) {
	case > 18:
		Debug.Log("You are legally an adult");
		break;
	case > 50:
		Debug.Log("You are old");
		break;
	case < 13:
		Debug.Log("You are a child");
		break;
	default:
		Debug.Log("You are a teenager");
		break;
}
```

If `break` is not used, add a comment `// Fallthorugh` to indicate that the fallthrough is intentional.
If `default` is not used, add a comment `// No default` instead.


## Line breaks
Try to keep lines shorter than 120 characters. If not possible, use line breaks to keep the code readable.


## Comments
- Use `//` for single line comments.
- Use `/* */` for multi line comments.
- Use `///` for XML comments.
- Use `// TODO:` for comments that describe something that needs to be done.
- Don't use too many comments, try to write code that is self-explanatory.


## Shaders
Shaders follows [KDE's frameworks coding style](https://community.kde.org/Policies/Frameworks_Coding_Style),
but overriding with this same document.


## Exceptions
Sometimes it's better to break the rules than to follow them blindly. If you think that breaking the rules
makes the code more readable, then do it.

Encapsulate the code with `// Styling excuse: ...` to indicate that the code is an exception to the
coding style rules, then, close the exception with `// End of styling excuse`.

For example:
```csharp
// Styling excuse: This code is hard to read with the normal style

if (something > 0 && somethingElse < 50) variable = 1;
else if (something > 10 && somethingElse > 40) variable = 2;
else if (something < 20 && somethingElse < 30) variable = 3;
else if (something < 30 && somethingElse > 20) variable = 4;
else variable = 5;

// End of styling excuse
```
