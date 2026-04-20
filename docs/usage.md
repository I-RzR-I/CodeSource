# USING

Using this attribute is quite simple. You must add in code `[CodeSource(...)]` with specifying details and that is all.

For `CodeSource` are available some properties/input parameters.
* `SourceUrl` -> Source of code/inspiation;
* `AuthorName` -> Author;
* `Copyright` -> Copyright data for user or company;
* `AppliedOn` -> Date when was applied this to your code;
* `Comment` -> Addition comment for this code source;
* `Version` -> Code changes version;
* `Tags` -> Specific tags e.g., security, design-doc, todo;
* `RelatedTaskId` -> Working item id: e.g., bug tracker, feature or task; As in many tracking systems, the ids are notated with `#`, from the start. A good idea to set it as the `#123` format.

Some examples are shown below:
```csharp
using RzR.Core.CodeSource;

[CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC")]
public class Foo
{}
```

```csharp
using RzR.Core.CodeSource;

[CodeSource(sourceUrl: "https://company.com", authorName: "User", copyright: "Company INC", version: "2.0")]
public class Foo
{
    [CodeSource("LocalHost/use-async", "User", "Company INC", "2022-12-12", "This is how to use async", version: "1.3")]
    public async Task RunAsync()
    {
        await Task.CompletedTask;
    }
}
```


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

So to see all code source references added to your code, just make a method call for every assembly you have.
```csharp
using RzR.Core.CodeSource.Services;

var codeSource = CodeSourceScanner.Instance.FindAnnotations(assembly);
```


### Export generated code source history
Available export types (formats) are:
- [`CSV` -> Export history in `Comma-Separated Values` aka `Excel` file format](/result/result_in_csv.csv);
- [`HTML` -> Export history in `HTML` file format](/result/result_in_html.html);
- [`JSON` -> Export history in `JSON` file format](/result/result_in_json.json);
- [`YAML` -> Export history in `YAML` file format](/result/result_in_yaml.yaml);
- [`XML` -> Export history in `XML` file format](/result/result_in_xml.xml);
- [`MD`(Markdown) -> Export history in `Markdown` file format](/result/result_in_md.md);


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

#### Export format constants
Instead of passing raw strings, use the `ExportFormats` constants class:

```csharp
using RzR.Core.CodeSource;

ExporterRegistry.Export(ExportFormats.Json, items, stream);
ExporterRegistry.Export(ExportFormats.Csv, items, "output.csv");
```

Available constants: `ExportFormats.Csv`, `ExportFormats.Html`, `ExportFormats.Json`, `ExportFormats.Markdown`, `ExportFormats.Xml`, `ExportFormats.Yaml`.

#### Custom exporter registration
You can register, replace, or remove exporters at runtime:

```csharp
// Register a custom exporter (implements ICodeSourceExporter)
ExporterRegistry.Register(new MyCustomExporter());

// Remove a registered exporter by format name
ExporterRegistry.Unregister("CUSTOM");

// List all registered format names
IEnumerable<string> formats = ExporterRegistry.GetRegisteredFormats();
```

---

## Migration Guide (v3.x / v4.x -> v5.0)

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
// Before
[CodeSource("https://example.com", version: 1.5)]

// After
[CodeSource("https://example.com", version: "1.5")]
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




