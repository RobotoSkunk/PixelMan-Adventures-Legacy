# PixelMan Adventures
It's a simple platformer game with a pixel art style. The game is about just going from point A to point B without dying.

## Dependencies
- [Unity 2021.3.0f1](https://unity3d.com/download)
	- Android SDK, NDK and JDK.
	- Windows IL2CPP compiler.
	- Linux IL2CPP compiler.
- Visual Studio 2019 or 2022, Visual Studio Code or any other IDE compatible with Unity.
- The rest of dependencies will be downloaded automatically by Unity.

## Installation
Just clone the repository and open it with Unity Hub. Unity Hub will install everything automatically.
```git
> git clone https://github.com/RobotoSkunk/PixelMan-Development.git
```

## Compile targets
The game will be compiled for Windows, Linux and Android. IOS and MacOS are not supported at the moment.

# Syntax rules

## Indentation
Use tab characters for indentation. Use spaces for alignment.

```cs
class Example : MonoBehaviour {
	void Start() {
		int[] array = { 1, 2, 3 };

		foreach (int i in array
			Debug.Log(i);
	}
}
```

## Naming conventions
- Start every class, function, struct or enum with a capital letter.
- Use camelCase for naming.
- Start every variable, parameter, constant or getter/setter with a lower case letter.
- For enum values use all capital letters and separate them with underscores (this rule is not obligatory).
- If you want to add a newline for brackets it's okay, but don't start a fight with the indentation.
- Start variables with __ (two underscores) if you want to hide them using a getter/setter.

```cs
class Example {
	public int __example;

	public int exampleVariable {
		get {
			return __example;
		}
		set {
			__example = value;
		}
	}

	void ExampleFunction() => exampleVariable = 1;
}
```

## Some extra tips
- Use `#region` and `#endregion` to mark sections of code.
- Use `#if` and `#endif` to mark sections of code that should be compiled only in certain conditions.
- Use the phrase "don't repeat yourself" to avoid code duplication.
- DON'T MIX CLASSES AND STRUCTS!
- Use bitmask enums for flags.
- Remember to use non-alloc functions every time you can.
- Code in C# like if you're coding in C++ (remember, the game will be compiled with IL2CPP).
- Write your code in a way that it's easy to use dinamically.

```cs
using UnityEngine;

[System.Flags]
public enum Allowed {
	None  = 0,
	Left  = 1 << 0,
	Right = 1 << 1,
	Up    = 1 << 2,
	Down  = 1 << 3,
	Sides = Left | Right,
	All   = ~0
}

class Example : MonoBehaviour {
	public Allowed allowed;

	void Start() {
		if ((allowed & Allowed.Left) != 0) {
			Debug.Log("Left");
		} else if ((allowed & Allowed.Right) != 0) {
			Debug.Log("Right");
		} else {
			Debug.Log("None");
		}
	}
}
```
