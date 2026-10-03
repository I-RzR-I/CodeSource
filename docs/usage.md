# USING

## Installation

Install the current NuGet package ID `RzR.Core.CodeSource`:

```powershell
Install-Package RzR.Core.CodeSource -Version x.x.x.x
```

```bash
dotnet add package RzR.Core.CodeSource --version x.x.x.x
```

Using this attribute is quite simple. You must add in code `[CodeSource(...)]` with specifying details and that is all.

For `CodeSource` are available some properties/input parameters.
* `SourceUrl` -> Source of code/inspiration;
* `AuthorName` -> Author;
* `Copyright` -> Copyright data for user or company;
* `AppliedOn` -> Date when was applied this to your code, in the `yyyy-MM-dd` format;
* `Comment` -> Addition comment for this code source;
* `Version` -> Code changes version;
* `Tags` -> Specific tags e.g., security, design-doc, todo;
* `RelatedTaskId` -> Working item id: e.g., bug tracker, feature or task; As in many tracking systems, the ids are notated with `#`, from the start. A good idea to set it as the `#123` format. The constructor argument for this property is named `workItemId`.

### Recommended style

Pass the source URL as the only constructor argument and set everything else with named properties:

```csharp
using RzR.Core.CodeSource;

[CodeSource("https://example.com/blog/retry-with-backoff",
    AuthorName = "Jane Doe",
    Copyright = "Example Ltd",
    AppliedOn = "2026-09-29",
    Comment = "Retry policy adapted from the blog post",
    Version = "1.1",
    Tags = "resilience;http",
    RelatedTaskId = "#123")]
public class RetryPolicy
{
}
```

Keep these rules in mind:

- **Positional arguments bind by position, not by meaning.** `[CodeSource("https://example.com", "John")]` sets `Version` to `"John"` and leaves `AuthorName` empty.
- **`Copyright` depends on how you set it.** The named property `Copyright = "Example Ltd"` stores the value as given. The constructor argument `copyright: "Example Ltd"` stores `"© Example Ltd"`; it does not add a second `©` if the value already starts with one, and it stores `null` when the value is missing or blank.
- **`AppliedOn` must be a `yyyy-MM-dd` date.** Leading and trailing spaces are ignored. An invalid date does not throw: the annotation is still returned, but no date is exported. To find invalid dates, scan with an error callback (see [Reporting scan errors](#reporting-scan-errors)).
- **`Version`** is `"1.0"` when you use `[CodeSource("url")]`. When the attribute has no version at all (`[CodeSource(AuthorName = "...")]`), the exporters write `"1.0"`.

Some examples are shown below:
```csharp
using RzR.Core.CodeSource;

[CodeSource(AuthorName = "Company User", Copyright = "Company INC")]
public class Foo
{
}
```

```csharp
using System.Threading.Tasks;
using RzR.Core.CodeSource;

[CodeSource("https://example.com", AuthorName = "User", Copyright = "Company INC", Version = "2.0")]
public class Foo
{
    [CodeSource("https://example.com/articles/use-async",
        AuthorName = "User",
        Copyright = "Company INC",
        AppliedOn = "2022-12-12",
        Comment = "This is how to use async",
        Version = "1.3")]
    public async Task RunAsync()
    {
        await Task.CompletedTask;
    }
}
```

#### Named constructor arguments

Named constructor arguments also work. Two differences from the named-property style: `copyright:` adds the `©` prefix, and `workItemId:` sets `RelatedTaskId`.

```csharp
using RzR.Core.CodeSource;

[CodeSource("https://example.com/blog/retry-with-backoff",
    authorName: "Jane Doe",
    copyright: "Example Ltd",
    appliedOn: "2026-09-29",
    comment: "Retry policy adapted from the blog post",
    version: "1.1",
    workItemId: "#123",
    tags: "resilience;http")]
public class RetryPolicy
{
}
```

Here `Copyright` is `"© Example Ltd"` and `RelatedTaskId` is `"#123"`.

One case still warns. When the only values after the URL are a version, or an author name and a version (`version: "1.1"`, or `authorName: "Jane Doe", version: "1.1"`), the call resolves to an obsolete constructor and raises `CS0618`, even though the values bind correctly. Use `Version =` and `AuthorName =` instead. See the [migration guide](migration-guide.md#23-correct-calls-that-now-warn).

#### Recording several changes

The attribute can be applied more than once, so you can keep the history of a type in one place:

```csharp
using RzR.Core.CodeSource;

[CodeSource("https://example.com/blog/retry-with-backoff", AuthorName = "Jane Doe", AppliedOn = "2026-01-15", Version = "1.0")]
[CodeSource("https://example.com/blog/retry-with-jitter", AuthorName = "John Roe", AppliedOn = "2026-09-29", Comment = "Added jitter", Version = "1.1")]
public class RetryPolicy
{
}
```

Reflection does not guarantee the order of the attributes. Sort the history by `Version` or `AppliedOn` if order matters to you.


### Generate code source history

Available code source search methods are:
```csharp
using RzR.Core.CodeSource.Abstractions;

public interface ICodeSourceScanner
{
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly);
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName);
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies);
}
```

The `CodeSourceScanner` class adds one overload per method that takes a `CodeSourceScanOptions` argument (see [Reporting scan errors](#reporting-scan-errors)). These overloads are on the class only, so call them through `CodeSourceScanner.Instance` or a `CodeSourceScanner` variable.

So to see all code source references added to your code, just make a method call for every assembly you have.
```csharp
using RzR.Core.CodeSource.Services;

var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly);
```

What each overload scans:

| Overload | Scans |
|---|---|
| `FindAnnotations(Assembly)` | The given `Assembly` object and its direct references. Each assembly is scanned once. The given object is scanned directly, so assemblies loaded from bytes, from a path or from a plugin load context work. Reflection-only assemblies (`ReflectionOnlyLoad`, `MetadataLoadContext`) are loaded again by their full name, with no file-path fallback. Dynamic assemblies are skipped and reported as `TypeEnumeration`. Custom `Assembly` subclasses that do not implement the reflection APIs are not supported. |
| `FindAnnotations(string)` | The assembly loaded by display name (for example `"MyApp.Core"`, not a file path) and its direct references. If the named assembly itself cannot be loaded, the exception propagates. |
| `FindAnnotations(IEnumerable<Assembly>)` | Exactly the assemblies you pass. References are not scanned, `null` elements are skipped, and an assembly passed twice is scanned twice. |

On netstandard1.0 only the given assembly is scanned; references are not.

A `null` assembly or assembly list throws `ArgumentNullException`. A `null` assembly name throws `ArgumentNullException`, and an empty or blank one throws `ArgumentException`; in both cases `ParamName` is `assemblyName`. `ArgumentNullException` derives from `ArgumentException`, so `catch (ArgumentException)` handles all of them.

`CodeSourceScanner.Instance` is a `static readonly` field and holds no state, so it is safe to share across threads.

#### Reporting scan errors

**Without an error callback, a referenced assembly that cannot be loaded is skipped silently: the scan returns fewer results and gives no signal.** The same applies to the other recoverable failures listed below. To see them, pass a `CodeSourceScanOptions` with an `OnError` callback:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;

public static class CodeSourceReport
{
    public static IReadOnlyList<CodeSourceObjectsResult> Scan(Assembly assembly)
    {
        // Every recoverable failure is reported, and the scan continues.
        var errors = new List<CodeSourceScanError>();
        var options = new CodeSourceScanOptions
        {
            OnError = error => errors.Add(error)
        };

        var results = CodeSourceScanner.Instance.FindAnnotations(assembly, options).ToList();

        // Message already contains the stage and the assembly, type and member names.
        foreach (var error in errors)
        {
            Console.WriteLine(error.Message);
        }

        return results;
    }
}
```

Call `.ToList()` before you read `errors`, so that the error list is complete when you print it.

Each `CodeSourceScanError` has `Stage`, `AssemblyName`, `TypeName`, `MemberName`, `Message` and `Exception`. `TypeName` and `MemberName` are `null` when the failure is at assembly or type level. The scan reports these stages:

| `Stage` | What failed | What the scan does |
|---|---|---|
| `AssemblyLoad` | A referenced assembly could not be loaded by name | Skips that reference |
| `TypeEnumeration` | The exported types of an assembly could not be listed (for example a dynamic assembly) | Skips that assembly |
| `MemberEnumeration` | The constructors or methods of a type could not be listed | Skips that member kind |
| `AttributeRead` | The `[CodeSource]` attributes of a type or member could not be read | Skips that type or member |
| `AttributeValue` | A value is present but invalid, for example `AppliedOn = "12/12/2022"` | Returns the annotation without that value; `Exception` is `null` |

`Message` has the form `<Stage>: <exception type>: <exception message> [assembly: ...] [type: ...] [member: ...]`. It never contains a stack trace. The `<exception message>` part is the runtime's own text, which on .NET Core can include a file path the loader probed, so log it but do not show it verbatim to end users. Use `Exception` if you need the full details.

Two cases are easy to miss without `OnError`, which is why tooling and CI should always pass it:

- If the exported types of the assembly cannot be listed, for example because one exported type derives from a type in a missing dependency, the whole assembly is skipped.
- If the attributes of a class cannot be read but one of its members is annotated, the class appears as a `Parent` with an empty history, which looks the same as a class with no `[CodeSource]` of its own.

Only loader and reflection failures are treated as recoverable (for example `FileNotFoundException`, `FileLoadException`, `BadImageFormatException`, `TypeLoadException`, `ReflectionTypeLoadException`). Any other exception, such as a `NullReferenceException`, propagates to the caller. An exception thrown by your `OnError` callback also propagates and ends the scan.


### Export generated code source history
Available export types (formats) are:
- [`CSV` -> Export history in `Comma-Separated Values` aka `Excel` file format](result/result_in_csv.csv);
- [`HTML` -> Export history in `HTML` file format](result/result_in_html.html);
- [`JSON` -> Export history in `JSON` file format](result/result_in_json.json);
- [`YAML` -> Export history in `YAML` file format](result/result_in_yaml.yaml);
- [`XML` -> Export history in `XML` file format](result/result_in_xml.xml);
- [`MD`(Markdown) -> Export history in `Markdown` file format](result/result_in_md.md);


The export functionalities are available from `ExporterRegistry` with the following defined methods.
More examples of how they can be used, you can find in `src\tests\Tests\ExportTests.cs`.

```csharp
using RzR.Core.CodeSource.Services;

public static class ExporterRegistry
{
    static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, string savePath);
    static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, Stream stream);

    static void Register(ICodeSourceExporter exporter);
    static bool Unregister(string format);
    static IEnumerable<string> GetRegisteredFormats();
}
```

How the two `Export` overloads handle output:

- **Stream overload:** the exporter writes and flushes, then **leaves your stream open**. You own the stream and must dispose it.
- **`savePath` overload:** creates the file (or overwrites an existing one), writes it and closes it. It checks every argument before it opens the file, so an invalid call never creates or truncates the target file. This overload is not available on netstandard1.0.

Argument errors, all raised before anything is written:

| Argument | Exception |
|---|---|
| `format` is `null` | `ArgumentNullException` (`ParamName` `format`), both overloads |
| no exporter registered for `format` | `CodeSourceUndefinedExportFormat` (namespace `RzR.Core.CodeSource.Exceptions`) |
| `items` is `null` | `ArgumentNullException` (`ParamName` `items`) |
| stream is `null` | `ArgumentNullException` (`ParamName` `outputStream`) |
| `savePath` is `null` | `ArgumentNullException` (`ParamName` `savePath`) |
| `savePath` is empty or blank | `ArgumentException` (`ParamName` `savePath`) |

```csharp
using System.IO;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Services;

using (var stream = new MemoryStream())
{
    ExporterRegistry.Export(ExportFormats.Json, items, stream);

    // The stream is still open, so you can read the result.
    stream.Position = 0;
    var json = new StreamReader(stream).ReadToEnd();
}
```

#### Export format constants
Instead of passing raw strings, use the `ExportFormats` constants class:

```csharp
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Services;

ExporterRegistry.Export(ExportFormats.Json, items, stream);
ExporterRegistry.Export(ExportFormats.Csv, items, "output.csv");
```

Available constants: `ExportFormats.Csv`, `ExportFormats.Html`, `ExportFormats.Json`, `ExportFormats.Markdown`, `ExportFormats.Xml`, `ExportFormats.Yaml`.

Format names are case-insensitive: `"json"`, `"JSON"` and `ExportFormats.Json` select the same exporter.

#### Custom exporter registration
You can register, replace, or remove exporters at runtime:

```csharp
using System.Collections.Generic;
using RzR.Core.CodeSource.Services;

// Register a custom exporter (implements ICodeSourceExporter)
ExporterRegistry.Register(new MyCustomExporter());

// Remove a registered exporter by format name
ExporterRegistry.Unregister("CUSTOM");

// List all registered format names
IEnumerable<string> formats = ExporterRegistry.GetRegisteredFormats();
```

`Register` replaces any exporter already registered for the same format, including a built-in one; the last registration wins.

The registry keeps one exporter instance per format and shares it across all callers and threads. A custom exporter must therefore be stateless: do not keep per-export state in fields. The built-in exporters leave the caller's stream open; do the same in a custom exporter so that callers see consistent behaviour.

---

## Migration Guide from (6.0)

See [migration-guide.md](migration-guide.md). It opens with the list of breaking and behaviour changes, starting with the silent skip of unloadable referenced assemblies, and then covers the obsolete constructors and warning `CS0618`, the scanner and exporter changes and the output format changes.

## Migration Guide (v3.x / v4.x -> v5.0)

### Package ID rename
The NuGet package ID changed from `CodeSource` to `RzR.Core.CodeSource`.

| Old | New |
|---|---|
| `Install-Package CodeSource` | `Install-Package RzR.Core.CodeSource` |
| `<PackageReference Include="CodeSource" Version="x.x.x" />` | `<PackageReference Include="RzR.Core.CodeSource" Version="x.x.x.x" />` |

### Namespace rename
All namespaces changed from `CodeSource.*` to `RzR.Core.CodeSource.*`.

| Old | New |
|---|---|
| `using CodeSource;` | `using RzR.Core.CodeSource;` |
| `using CodeSource.Models;` | `using RzR.Core.CodeSource.Models;` |
| `using CodeSource.Services;` | `using RzR.Core.CodeSource.Services;` |
| `using CodeSource.Abstractions;` | `using RzR.Core.CodeSource.Abstractions;` |
| `using CodeSource.Exceptions;` | `using RzR.Core.CodeSource.Exceptions;` |

Assembly name changed from `CodeSource.dll` to `RzR.Core.CodeSource.dll`.

### Version type: `double` -> `string`
The `Version` property on `CodeSourceAttribute`, `ICodeSourceAttribute`, and `CodeSourceObjectHistory` changed from `double` to `string`.

```csharp
using RzR.Core.CodeSource;

// Before (v4.x): Version was a double. This no longer compiles.
// [CodeSource("https://example.com", version: 1.5)]

// After (5.0+): Version is a string. Set it as a named property.
[CodeSource("https://example.com", Version = "1.5")]
public class Foo
{
}
```

### Collection property types
`CodeSourceObject.History` and `CodeSourceObjectsResult.Children` changed from `IEnumerable<T>` to `IReadOnlyList<T>` (net45+ / netstandard / net). On net40 they are `IList<T>`.

Code that only reads/enumerates these properties is unaffected. Code that assigns them must provide a `List<T>` or another `IReadOnlyList<T>` implementation.

### Export magic strings → constants
Replace raw format strings with `ExportFormats` constants:

```csharp
// Before
ExporterRegistry.Export("json", items, stream);

// After
ExporterRegistry.Export(ExportFormats.Json, items, stream);
```
