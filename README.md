> **Note** This repository is developed for .netstandard1.0+ and .net framework 4.0+

[![NuGet Version](https://img.shields.io/nuget/v/RzR.Core.CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Core.CodeSource/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.Core.CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Core.CodeSource/)

<details>

  <summary>Legacy package (`CodeSource`)</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)

</details>

<br />
The primary purpose of this repository/library is to make an easy, accurate, and organized solution for storing data in your source code about some ideas, comments, or code references, which was an inspiration for realizing your current functionality.

From the box is provided an attribute with multiple input parameters such as: `SourceUrl`, `AuthorName`, `Copyright`, `AppliedOn`, `Comment`, `Version`, `Tags`, `RelatedTaskId`.

Also, it was implemented a method that can return a list with every place where was applied attribute grouped by class with user-specified details.

In addition to generating code source history, the current implementation can export possibilities.
From the box, built-in export formats are: `CSV`, `HTML`, `JSON`, `YAML`, `XML`, and `MD`(Markdown).
Format constants are available via the `ExportFormats` class (e.g. `ExportFormats.Json`).
Custom exporters can be registered at runtime through `ExporterRegistry.Register()`.

No additional components or packs are required for use. So, it only needs to be added/installed in the project and can be used instantly.

The current NuGet package ID is `RzR.Core.CodeSource`.
If you are upgrading from older releases published as `CodeSource`, update the package reference and namespaces together.

**In case you wish to use it in your project, you can install the package from <a href="https://www.nuget.org/packages/RzR.Core.CodeSource" target="_blank">nuget.org</a>** or specify what version you want:


> `Install-Package RzR.Core.CodeSource -Version x.x.x.x`
>
> `dotnet add package RzR.Core.CodeSource --version x.x.x.x`

## Quick start

Pass the source URL as the only constructor argument and set everything else with named properties:

```csharp
using RzR.Core.CodeSource;

[CodeSource("https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose",
    AuthorName = "Jane Doe",
    Version = "1.0")]
public class DisposableResource
{
}
```

Avoid extra positional arguments: `[CodeSource("https://example.com", "Jane Doe")]` sets `Version` to `"Jane Doe"`. Since 6.1 that form raises warning `CS0618`.

Scan an assembly and export what it finds:

```csharp
using System.Linq;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Services;

var history = CodeSourceScanner.Instance.FindAnnotations(typeof(DisposableResource).Assembly).ToList();
ExporterRegistry.Export(ExportFormats.Markdown, history, "code-sources.md");
```

## Content
1. [USING](docs/usage.md)
2. [MIGRATION GUIDE](docs/migration-guide.md)
3. [CHANGELOG](docs/CHANGELOG.md)
4. [BRANCH-GUIDE](docs/branch-guide.md)