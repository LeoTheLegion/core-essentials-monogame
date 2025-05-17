# XML Documentation Guidelines

This document provides guidelines on how to properly document your code in this CoreEssentials project so that IntelliSense information appears for API users.

## Overview

When you document your code correctly with XML documentation comments, your library's users will see helpful tooltips and descriptions in their IDE when they use your code.

## How to Add XML Documentation Comments

### Basic Structure

XML documentation comments start with three forward slashes (`///`) and should be placed directly above the member they document.

Example:

```csharp
/// <summary>
/// Brief description of the class or member.
/// </summary>
public class MyClass
{
    /// <summary>
    /// Description of what this property does.
    /// </summary>
    public int MyProperty { get; set; }

    /// <summary>
    /// Description of what this method does.
    /// </summary>
    /// <param name="paramName">Description of the parameter.</param>
    /// <returns>Description of what is returned.</returns>
    /// <exception cref="ExceptionType">When this exception is thrown.</exception>
    public string MyMethod(int paramName)
    {
        // Method implementation
    }
}
```

### Common Documentation Tags

- `<summary>`: Brief description of the member
- `<param name="paramName">`: Description of a parameter
- `<returns>`: Description of the return value
- `<exception cref="ExceptionType">`: When a specific exception can be thrown
- `<remarks>`: Additional information about the member
- `<example>`: Example code
- `<see cref="Member">`: Creates a link to another member
- `<seealso cref="Member">`: Creates a "see also" link
- `<value>`: Description of a property's value

## Implementation in Our Project

Our project is set up to automatically generate XML documentation files when building. These files are included in the NuGet package, so users of our library will see IntelliSense documentation.

The following settings in `CoreEssentials.csproj` enable this functionality:

```xml
<PropertyGroup>
  <!-- XML Documentation Generation -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\CoreEssentials.xml</DocumentationFile>
</PropertyGroup>
```

## Best Practices

1. Document all public members.
2. Keep summaries concise and focused.
3. Provide examples for complex operations.
4. Document exceptions that might be thrown.
5. Use `<see cref=""/>` to link to related members.
6. Keep documentation up-to-date when code changes.

## Testing Documentation

You can verify your documentation is working correctly by:

1. Building the project
2. Checking that an XML file is generated in the output directory
3. Reference the project in another solution and verify IntelliSense shows your documentation

## Additional Resources

- [Microsoft's XML Documentation Comments Guide](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [C# XML Documentation Comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments)
