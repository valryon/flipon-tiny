# UT Mobile Game Design C# Unity Style Guide

This guide  is based on the existing codebase as well as the `flipon-tiny.sln.DotSettings` file provided in the original repository. 

**Note:** Even within the existing code, there are inconsistencies so if you notice them, please change them.

## Capitalization and Underscores

### File Naming

- File names should use `PascalCase_FileDesc`. For example, `MainChar_WalkSprite.png` or `PlayerController.cs`.

### Classes

- Class names should be concise and descriptive, using `PascalCase`. For example, `PlayerController`.

### Variables

- Variable names should be clear and meaningful. Use `camelCase`. For example, `playerSpeed` or `initialPosition`.
  
- Constants should be self-explanatory and use the `AA_BB` style. For example, `MAX_SPEED` or `INITIAL_LIVES`.
  
- Unity serialized fields, which are typically private but need to be exposed in the Unity editor, should use `camelCase`.

## Code Formatting

### Indentation

- Stick to 2 spaces for indentation, so that code doesn't run too far to the right.
- Do not use tabs. Make sure your editor replaces tabs with spaces.

### Braces

- Use the Allman style (BSD style) for braces. It helps visually separate blocks of code, making them easier to read:

```csharp
if (condition)
{
    // Code here
}
```


### Comments

- Comments should explain the why, not the what.
- Keep comments concise and to the point.
- Always update comments if you update the code they describe.

## Final Notes

I believe that the `.DotSettings` is only for Rider/Omnisharp. If you are not using the `.DotSettings` in your IDE, make sure you manually configure your IDE to match this guide. 
