# Usage reference

This is the full reference for the attribute, the scanner and the exporters. To set up the `CODESOURCE` symbol for your project type, see [Setting up CODESOURCE](../README.md#setting-up-codesource) in the README. To publish a NuGet package that applies `[CodeSource]`, see [Publishing a library that uses CodeSource](emitcodesource.md).

## Installation

Install the current NuGet package ID `RzR.Core.CodeSource`:

```powershell
Install-Package RzR.Core.CodeSource -Version 7.0.0.x
```

```bash
dotnet add package RzR.Core.CodeSource --version 7.0.0.x
```

`7.0.0.x` stands for the exact 4-part version from nuget.org; see [Install](../README.md#install).

### Enabling CODESOURCE (required since 7.0)

Setup steps for every project type: [Setting up CODESOURCE](../README.md#setting-up-codesource). If you are upgrading from 6.x, read [Migrating from 6.x to 7.0](migration-guide.md#migrating-from-6x-to-70).

`[CodeSource]` is a conditional attribute. The compiler writes it into your assembly only when the project that applies it defines the `CODESOURCE` symbol. What to know beyond the setup:

- **It counts only where the attribute is applied.** Defining it in an app does not change what a referenced library contains.
- **Without it, the compiler still checks every usage.** `CS0618` and argument errors are still reported, so the project still needs its package reference to `RzR.Core.CodeSource`. The usages are dropped from the output, and so is the assembly reference to `RzR.Core.CodeSource` (the entry in your dll's metadata that makes the runtime load it), unless other code in the project uses CodeSource types (for example `typeof(CodeSourceAttribute)` or scanner calls).
- **In C#, you can enable a single file** by putting `#define CODESOURCE` at the top of that file, before any `using`. Only the usages in that file are emitted. In VB, use `#Const CODESOURCE = True`. F# has no per-file option. Use this only where the package reference to `RzR.Core.CodeSource` flows to consumers (not `PrivateAssets="all"`): apps, test projects and libraries you do not publish, or libraries built with `EmitCodeSource=true`. In a library whose reference is private it causes `FileNotFoundException` for consumers; see [Using CodeSource in a library](../README.md#using-codesource-in-a-library).

```csharp
#define CODESOURCE
using RzR.Core.CodeSource;

[CodeSource("https://example.com/blog/retry-with-backoff", Version = "1.0")]
public class RetryPolicy
{
}
```

#### Check which version you run

7.0 marks the attribute class with `[Conditional("CODESOURCE")]`, so you can detect it at run time:

```csharp
using System;
using System.Diagnostics;
using System.Linq;
using RzR.Core.CodeSource;

public static class Program
{
    public static void Main()
    {
        var conditional = typeof(CodeSourceAttribute)
            .GetCustomAttributes(typeof(ConditionalAttribute), false)
            .Cast<ConditionalAttribute>()
            .FirstOrDefault();

        Console.WriteLine("Conditional symbol: " + (conditional == null ? "(none, pre-7.0)" : conditional.ConditionString));
    }
}
```

On 7.0 this prints `Conditional symbol: CODESOURCE`. At compile time, the constant `CodeSourceAttribute.ConditionalSymbol` exists only from 7.0, so code that uses it does not compile against 6.x.

`typeof(CodeSourceAttribute)` adds a reference to `RzR.Core.CodeSource` to the assembly that contains it. Do not put this check in a packed library unless it is built with `EmitCodeSource=true` (see [rule 4](emitcodesource.md#rules)).

### Applying the attribute

In a project that defines `CODESOURCE`, add `[CodeSource(...)]` with the details you want to record. The attribute can be applied to a class, constructor or method, and more than once to the same element. All properties are `string`.

| Property | What to put in it | Notes |
|---|---|---|
| `SourceUrl` | URL of the original code or idea | Usually the only constructor argument. |
| `AuthorName` | Author of the original code | |
| `Copyright` | Copyright holder | The named property stores the value as given. The constructor argument `copyright:` adds a `© ` prefix (never a second one) and turns a missing or blank value into `null`. |
| `AppliedOn` | Date you applied it, in `yyyy-MM-dd` format | Surrounding spaces are ignored. An invalid date such as `"12/12/2022"` does not throw: the annotation is kept without a date, and an `AttributeValue` scan error is reported (see [Reporting scan errors](#reporting-scan-errors)). |
| `Comment` | Free text | |
| `Version` | Version of this change, for example `"1.1"` | `[CodeSource("url")]` sets it to `"1.0"`. When no version is set at all (`[CodeSource(AuthorName = "...")]`), the property is `null`, and the scanner and exporters report `"1.0"`. |
| `Tags` | Free text; by convention `;`-separated, for example `"security;design-doc"` | The library does not split it. |
| `RelatedTaskId` | Work-item id, for example `"#123"` | There is no `WorkItemId` property. `workItemId` is only the constructor parameter name, and `WorkItemId` is the column header in CSV, HTML and Markdown exports. JSON, XML and YAML use `relatedTaskId`. |

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

**Positional arguments bind by position, not by meaning.** `[CodeSource("https://example.com", "John")]` sets `Version` to `"John"` and leaves `AuthorName` empty. Since 6.1 that form raises warning `CS0618`.

The URL is optional:

```csharp
using RzR.Core.CodeSource;

[CodeSource(AuthorName = "Company User", Copyright = "Company INC")]
public class Foo
{
}
```

This example uses `Task.CompletedTask`, which is not available on net40 and net45 (it needs .NET Framework 4.6 or later):

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

A fuller example with its scan output: [A complete example](../README.md#a-complete-example).


### Generate code source history

Scanner methods (signature only):

```csharp
using System.Collections.Generic;
using System.Reflection;
using RzR.Core.CodeSource.Models;

namespace RzR.Core.CodeSource.Abstractions
{
    public interface ICodeSourceScanner
    {
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly);
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName);
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies);
    }
}
```

The `CodeSourceScanner` class adds one overload per method that takes a `CodeSourceScanOptions` argument (see [Reporting scan errors](#reporting-scan-errors)). These overloads are on the class only, so call them through `CodeSourceScanner.Instance` or a `CodeSourceScanner` variable. A complete scan-and-print program is in the README: [Scanning](../README.md#scanning).

**An assembly compiled without `CODESOURCE` gives an empty result.** Nothing is thrown and `OnError` is not called, because the assembly contains no `[CodeSource]` data to read. See [Enabling CODESOURCE](#enabling-codesource-required-since-70). To catch a project that loses the symbol, keep a check that the result is not empty; see [Add a guard test](../README.md#add-a-guard-test).

#### What the scanner reports

- **Types:** public types, including nested public types. A type is reported if it, or at least one of its constructors or methods, has an annotation. When only members are annotated, `Parent.History` is an empty list.
- **Constructors:** all constructors of the type, including the static constructor (reported as `.cctor`). Exception: projects that use the net40 build of the package (net40 to net403 targets) report public instance constructors only.
- **Methods:** the type's own methods, including private and static ones, and inherited non-private instance methods.
- **Not reported:** attributes on non-public types, or on properties, fields, events, parameters, return values, generic parameters, the assembly or the module. A `[CodeSource]` on a property compiles into the assembly, but `FindAnnotations` does not return it.
- **Inheritance:** the attribute is inherited. A public class that derives from an annotated class is reported with the base class's history and with the base class's annotated methods, so the same annotation appears under every public derived type. If the derived class has its own `[CodeSource]`, its history contains its own entries and the base class's entries, all under the derived class's `CodePath`.

#### What each overload scans

| Overload | Scans |
|---|---|
| `FindAnnotations(Assembly)` | The given `Assembly` object and its direct references. Each assembly is scanned once. The given object is scanned directly, so assemblies loaded from bytes, from a path or from a plugin load context work. Their references are loaded by name in the scanner's load context; for plugins, pass the plugin assemblies to `FindAnnotations(IEnumerable<Assembly>)` instead. Reflection-only assemblies (`ReflectionOnlyLoad`, `MetadataLoadContext`) are loaded again by their full name, with no file-path fallback. Dynamic assemblies are skipped and reported as `TypeEnumeration`. Custom `Assembly` subclasses that do not implement the reflection APIs are not supported. |
| `FindAnnotations(string)` | The assembly loaded by display name (for example `"MyApp.Core"`, not a file path) and its direct references. If the named assembly itself cannot be loaded, the exception propagates. |
| `FindAnnotations(IEnumerable<Assembly>)` | Exactly the assemblies you pass. References are not scanned, `null` elements are skipped, and an assembly passed twice is scanned twice. |

On netstandard1.0 only the given assembly is scanned; references are not.

The scanner loads the scanned assemblies and their direct references into your process; it is not a metadata-only inspection. Scan only assemblies you trust.

A `null` assembly or assembly list throws `ArgumentNullException`. A `null` assembly name throws `ArgumentNullException`, and an empty or blank one throws `ArgumentException`; in both cases `ParamName` is `assemblyName`. `ArgumentNullException` derives from `ArgumentException`, so `catch (ArgumentException)` handles all of them.

The scan runs immediately and returns a complete list, so the error list of an `OnError` callback is complete when `FindAnnotations` returns. Calling `.ToList()` on the result is harmless and keeps the code correct if that ever changes.

`CodeSourceScanner.Instance` is a `static readonly` field and holds no state, so it is safe to share across threads.

#### Reporting scan errors

**Without an error callback, a referenced assembly that cannot be loaded is skipped silently: the scan returns fewer results and gives no signal.** The same applies to the other recoverable failures listed below. Two cases are easy to miss, which is why tooling and CI should always pass `OnError`:

- One type whose base type cannot be loaded makes the scanner skip its **whole assembly**. Without `OnError`, that looks the same as an assembly with no annotations.
- If the attributes of a class cannot be read but one of its members is annotated, the class appears as a `Parent` with an empty history, which looks the same as a class with no `[CodeSource]` of its own.

To see these failures, pass a `CodeSourceScanOptions` with an `OnError` callback. The example below uses `Samples.Library` from the README ([A complete example](../README.md#a-complete-example)) with one more class that has an invalid date on purpose:

```csharp
using RzR.Core.CodeSource;

namespace Samples.Library
{
    public class LegacyClient
    {
        [CodeSource("https://example.com/legacy-client", AppliedOn = "12/12/2022", Comment = "Invalid AppliedOn on purpose")]
        public void Send()
        {
        }
    }
}
```

`Samples.Plugin` contains a public class that derives from a type in `Samples.Missing.dll`, and that dll is not deployed:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Samples.Library;

public static class Program
{
    public static void Main()
    {
        var errors = new List<CodeSourceScanError>();
        var options = new CodeSourceScanOptions { OnError = errors.Add };

        var assemblies = new[]
        {
            typeof(RetryPolicy).Assembly,
            Assembly.Load(new AssemblyName("Samples.Plugin"))
        };

        var results = CodeSourceScanner.Instance.FindAnnotations(assemblies, options).ToList();

        Console.WriteLine("Annotated types: " + results.Count);
        foreach (var error in errors)
            Console.WriteLine(error.Message);
    }
}
```

Output on .NET 8 (on .NET Framework the `FileNotFoundException` text adds `or one of its dependencies`):

```
Annotated types: 2
AttributeValue: AppliedOn is not a valid 'yyyy-MM-dd' date, so it was ignored. [assembly: Samples.Library, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null] [type: Samples.Library.LegacyClient] [member: Send]
TypeEnumeration: System.IO.FileNotFoundException: Could not load file or assembly 'Samples.Missing, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified. [assembly: Samples.Plugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]
```

The whole `Samples.Plugin` assembly is skipped because of one type with a missing base type.

Each `CodeSourceScanError` has `Stage`, `AssemblyName`, `TypeName`, `MemberName`, `Message` and `Exception`. `TypeName` is `null` for assembly-level failures (`AssemblyLoad`, `TypeEnumeration`). `MemberName` is `null` for assembly- and type-level failures. For `AssemblyLoad`, `AssemblyName` is usually the reference that could not be loaded; if the reference list itself could not be read, it is the scanned assembly. The scan reports these stages:

| `Stage` | What failed | What the scan does |
|---|---|---|
| `AssemblyLoad` | A referenced assembly could not be loaded by name | Skips that reference |
| `TypeEnumeration` | The exported types of an assembly could not be listed (for example a dynamic assembly) | Skips that assembly |
| `MemberEnumeration` | The constructors or methods of a type could not be listed | Skips that member kind |
| `AttributeRead` | The `[CodeSource]` attributes of a type or member could not be read | Skips that type or member |
| `AttributeValue` | A value is present but invalid, for example `AppliedOn = "12/12/2022"` | Returns the annotation without that value; `Exception` is `null` |

`Message` has the form `<Stage>: <exception type>: <exception message>[ Loader exceptions: ...] [assembly: ...] [type: ...] [member: ...]`, or `<Stage>: <description> [assembly: ...] ...` for `AttributeValue`. It never contains a stack trace. The `<exception message>` part is the runtime's own text, which on .NET Core can include a file path the loader probed, so log it but do not show it verbatim to end users. Use `Exception` if you need the full details.

Only loader and reflection failures are treated as recoverable (for example `FileNotFoundException`, `FileLoadException`, `BadImageFormatException`, `TypeLoadException`, `ReflectionTypeLoadException`). Any other exception, such as a `NullReferenceException`, propagates to the caller. An exception thrown by your `OnError` callback also propagates and ends the scan.

#### Fail a test on loader errors

The README guard test checks that an assembly is not empty. This NUnit test checks that nothing was skipped while scanning it:

```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Samples.Library;

[TestFixture]
public class CodeSourceLoadTests
{
    [Test]
    public void SamplesLibrary_ScansWithoutLoaderErrors()
    {
        var errors = new List<CodeSourceScanError>();
        var options = new CodeSourceScanOptions { OnError = errors.Add };

        CodeSourceScanner.Instance.FindAnnotations(new[] { typeof(RetryPolicy).Assembly }, options).ToList();

        // AttributeValue errors (such as an invalid AppliedOn) are data problems, not load failures.
        // Samples.Library has one on purpose (LegacyClient).
        Assert.That(errors.Where(e => e.Stage != CodeSourceScanStage.AttributeValue).Select(e => e.Message), Is.Empty);
    }
}
```

This test still passes when the library is built without `CODESOURCE`, because an assembly with nothing to read produces no errors. Keep the README guard test as well.


### Export generated code source history

Available formats:

| Format | Constant | Sample output | UTF-8 BOM |
|---|---|---|---|
| CSV | `ExportFormats.Csv` | [result_in_csv.csv](result/result_in_csv.csv) | yes |
| HTML | `ExportFormats.Html` | [result_in_html.html](result/result_in_html.html) | yes |
| JSON | `ExportFormats.Json` | [result_in_json.json](result/result_in_json.json) | yes |
| Markdown | `ExportFormats.Markdown` (`"MD"`) | [result_in_md.md](result/result_in_md.md) | yes |
| XML | `ExportFormats.Xml` | [result_in_xml.xml](result/result_in_xml.xml) | no |
| YAML | `ExportFormats.Yaml` | [result_in_yaml.yaml](result/result_in_yaml.yaml) | yes |

Format names are case-insensitive: `"json"`, `"JSON"` and `ExportFormats.Json` select the same exporter.

The export functions are on `ExporterRegistry` (signature only):

```csharp
using System.Collections.Generic;
using System.IO;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Models;

namespace RzR.Core.CodeSource.Services
{
    public static class ExporterRegistry
    {
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, string savePath);
        public static void Export(string format, IEnumerable<CodeSourceObjectsResult> items, Stream stream);

        public static void Register(ICodeSourceExporter exporter);
        public static bool Unregister(string format);
        public static IEnumerable<string> GetRegisteredFormats();
    }
}
```

More examples are in [`src\tests\Tests\Export\`](../src/tests/Tests/Export/) (start with `ScanToExportTests.cs` and `ExporterRegistryTests.cs`).

This program exports to a file, to a stream you own, and to a string:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Samples.Library;

public static class Program
{
    public static void Main()
    {
        var items = CodeSourceScanner.Instance
            .FindAnnotations(new[] { typeof(RetryPolicy).Assembly })
            .ToList();

        // 1. To a file: created or overwritten, then closed.
        ExporterRegistry.Export(ExportFormats.Markdown, items, "code-sources.md");

        // 2. To a stream you own: the exporter flushes it and leaves it open.
        using (var stream = new FileStream("code-sources.json", FileMode.Create))
        {
            ExporterRegistry.Export(ExportFormats.Json, items, stream);
            Console.WriteLine("Stream still open: " + stream.CanWrite + ", bytes written: " + stream.Length);
        }

        // 3. To a string.
        var csv = ExportToString(ExportFormats.Csv, items);
        Console.WriteLine(csv);
    }

    // StreamReader drops the UTF-8 BOM.
    private static string ExportToString(string format, IEnumerable<CodeSourceObjectsResult> results)
    {
        using (var buffer = new MemoryStream())
        {
            ExporterRegistry.Export(format, results, buffer);
            buffer.Position = 0;

            using (var reader = new StreamReader(buffer, Encoding.UTF8, true))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
```

How the two `Export` overloads handle output:

- **Stream overload:** the built-in exporters write and flush, then **leave your stream open**. You own the stream and must dispose it.
- **`savePath` overload:** creates the file (or overwrites an existing one), writes it and closes it. It checks every argument before it opens the file, so an invalid call never creates or truncates the target file. This overload is not available on netstandard1.0. The file and the stream overloads produce the same bytes.
- **Byte order mark:** CSV, HTML, JSON, Markdown and YAML write a UTF-8 BOM; XML does not. They skip the BOM only when the stream is seekable and its `Position` is greater than 0. A non-seekable stream (for example an HTTP response body, a network stream or a `GZipStream`) always gets a BOM, so do not export twice into one such stream.
- **Strings:** read the stream with a `StreamReader`, as above. Do not use `Encoding.UTF8.GetString(buffer.ToArray())` on a format that writes a BOM: it keeps the BOM as a `U+FEFF` character.

Argument errors, all raised before anything is written:

| Argument | Exception |
|---|---|
| `format` is `null` | `ArgumentNullException` (`ParamName` `format`), both overloads |
| no exporter registered for `format` | `CodeSourceUndefinedExportFormat` (namespace `RzR.Core.CodeSource.Exceptions`), message `Missing exporter for '<format>'` |
| `items` is `null` | `ArgumentNullException` (`ParamName` `items`) |
| stream is `null` | `ArgumentNullException` (`ParamName` `outputStream`) |
| `savePath` is `null` | `ArgumentNullException` (`ParamName` `savePath`) |
| `savePath` is empty or blank | `ArgumentException` (`ParamName` `savePath`) |

On the stream overload, the registry checks only the format. The `items` and stream checks are done by the built-in exporters. A failure while a built-in exporter writes is wrapped in `CodeSourceExporterException` (`ExporterFormat` is the exporter's own format, `InnerException` holds the cause).

#### Export format constants

Instead of raw strings, use the `ExportFormats` constants: `ExportFormats.Csv`, `ExportFormats.Html`, `ExportFormats.Json`, `ExportFormats.Markdown`, `ExportFormats.Xml`, `ExportFormats.Yaml`.

#### Custom exporter registration

Implement `ICodeSourceExporter` and register it:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Models;

public sealed class TextExporter : ICodeSourceExporter
{
    public string Format
    {
        get { return "TEXT"; }
    }

    public void Export(IEnumerable<CodeSourceObjectsResult> items, Stream outputStream)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
        if (outputStream == null)
            throw new ArgumentNullException(nameof(outputStream));

        // leaveOpen: true keeps the caller's stream open, like the built-in exporters (net45+).
        using (var writer = new StreamWriter(outputStream, new UTF8Encoding(false), 1024, true))
        {
            foreach (var item in items)
            {
                if (item == null || item.Parent == null)
                    continue;

                foreach (var history in item.Parent.History)
                    writer.WriteLine(history.CodePath + " <- " + history.SourceUrl);

                foreach (var member in item.Children)
                {
                    foreach (var history in member.History)
                        writer.WriteLine(history.CodePath + " <- " + history.SourceUrl);
                }
            }
        }
    }
}
```

Register it, use it and remove it:

```csharp
using System;
using System.IO;
using System.Linq;
using System.Text;
using RzR.Core.CodeSource.Services;
using Samples.Library;

public static class Program
{
    public static void Main()
    {
        var items = CodeSourceScanner.Instance
            .FindAnnotations(new[] { typeof(RetryPolicy).Assembly })
            .ToList();

        ExporterRegistry.Register(new TextExporter());

        using (var buffer = new MemoryStream())
        {
            ExporterRegistry.Export("text", items, buffer); // format names are case-insensitive
            Console.Write(Encoding.UTF8.GetString(buffer.ToArray())); // safe here: TextExporter writes no BOM
        }

        ExporterRegistry.Unregister("TEXT");
    }
}
```

Output for `Samples.Library` (`RetryPolicy` and `LegacyClient`):

```
Samples.Library.LegacyClient.Send <- https://example.com/legacy-client
Samples.Library.RetryPolicy <- https://learn.microsoft.com/azure/architecture/patterns/retry
Samples.Library.RetryPolicy <- https://example.com/blog/retry-with-jitter
Samples.Library.RetryPolicy.ctor <- https://example.com/blog/retry-options
Samples.Library.RetryPolicy.GetDelay <- https://en.wikipedia.org/wiki/Exponential_backoff
```

The contract for a custom exporter:

- **The registry passes `items` and the stream to your exporter unchecked.** Check them yourself, as above. Stream ownership and exception wrapping are built-in exporter behaviour, not registry behaviour.
- **Leave the caller's stream open** so that callers see the same behaviour as with the built-in exporters. On net40, `StreamWriter` has no `leaveOpen` overload: create the writer, call `writer.Flush()`, and do not dispose it.
- **Return a non-null `Format`.** `Register` throws `ArgumentNullException` for an exporter whose `Format` is `null`.
- **Keep the exporter stateless.** The registry keeps one instance per format and shares it across all callers and threads, so do not keep per-export state in fields.
- If your callers catch `CodeSourceExporterException`, wrap write failures in it yourself.

Registry behaviour:

- `Register` replaces any exporter already registered for the same format, including a built-in one; the last registration wins.
- `Unregister` returns `true` if it removed an exporter and `false` if none was registered. Exporting to a removed format throws `CodeSourceUndefinedExportFormat`.
- `GetRegisteredFormats()` returns a snapshot of the registered format names. By default: `CSV, HTML, JSON, MD, XML, YAML`.

---

## Upgrading

Upgrading from an earlier version, or from the old `CodeSource` package? See the [migration guide](migration-guide.md).
