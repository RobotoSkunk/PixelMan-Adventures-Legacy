# Code Style
To keep the code organized and easy to read, we follow a certain syntax.
This document explains what we follow.

## Naming
Try using a non-confusing name for your variables, functions, classes, etc.

Also, consider using `camelCase` for variables and properties, and
`PascalCase` for functions, classes and structs.

## Indenting
We use only tabs for indentation to work faster and keep the code organized.
Spaces are only used for alignment.

```csharp
if (cow_exists) {
	float time = 0;

	while (cow_is_alive) {
		time += 0.1f;

		if (time > 10) {
			Moo();
			time = Mathf.random(0, 10);
		}
	}
}
```

## Comments
Using comments is a good practice, but don't overuse them like if you were writing a book.
Try to keep your code as simple as possible.

## Long lines
Try to keep your lines short. If you have a long line, try to break it into multiple lines.

## Braces
We write the open brace on the same line as the statement and we then set the closing
brace on the same identation level as the statement.

```csharp
if (player_is_alive) {
	// Do something...
}
```

You may omit the braces if the statement is only a one-line statement.

```csharp
if (cow_is_alive) Moo();
```

### 'else' statement
If you have an `else` statement, you should put it on the same line as the closing

```csharp
if (cow_is_alive) {
	Moo();
} else {
	Moo_nt();
}
```

## Boolean conditions
Instead of checking if a variable is `true` or `false` implicitly, you should
just write the variable name.

```csharp
if (cow_is_alive) {
	// Do something...
}

if (!cow_is_alive) {
	// Do something...
}
```



