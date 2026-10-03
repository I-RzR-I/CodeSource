# Migrating from 6.0 to 6.1

This guide is for projects that use `RzR.Core.CodeSource` 6.0.x (last release: 6.0.0.94) and move to 6.1. Section 1 lists every change that can break a build or change what your code sees at run time. The later sections explain each change and how to adapt.

---

## 1. Breaking and behaviour changes

Read this section before you upgrade.

### 1.1 Unloadable referenced assemblies are skipped silently

**Before (6.0):** when `FindAnnotations(Assembly)` or `FindAnnotations(string)` could not load a referenced assembly, the exception escaped from `FindAnnotations` and you got no results.

**After (6.1):** the scanner skips that reference and keeps scanning. If you do not pass an error callback, **you get fewer results and no signal**. The same applies to every other recoverable failure listed in [section 3.4](#34-which-failures-are-recoverable).

If your build or report depends on a complete scan, pass a callback and decide what to do with each failure:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;

var errors = new List<CodeSourceScanError>();
var options = new CodeSourceScanOptions
{
    OnError = error => errors.Add(error)
};

var results = CodeSourceScanner.Instance.FindAnnotations(assembly, options).ToList();

if (errors.Count > 0)
{
    // Log, fail the build, or ignore: your choice.
    foreach (var error in errors)
        Console.WriteLine(error.Message);
}
```

To get the 6.0 "stop on the first failure" behaviour, throw from the callback. An exception thrown by `OnError` ends the scan and propagates to the caller:

```csharp
var options = new CodeSourceScanOptions
{
    OnError = error => throw new InvalidOperationException(error.Message, error.Exception)
};
```

**Always pass `OnError` in tooling and CI.** Without it, "not annotated" and "could not be read" look the same in the results.

The options overloads exist on the `CodeSourceScanner` class only. `ICodeSourceScanner` is unchanged, so if you depend on the interface you cannot pass options through it.

### 1.2 Changes that can break the build

| Change | Effect | Details |
|---|---|---|
| `CodeSourceScanner.Instance` is `static readonly` | Assigning it is error `CS0198`. Reading it is unaffected. | [3.1](#31-codesourcescannerinstance-is-readonly) |
| `(sourceUrl, version)` and `(sourceUrl, authorName, version)` constructors are obsolete | Warning `CS0618`; an error if you treat warnings as errors | [2.1](#21-what-changed), [2.8](#28-builds-with-treatwarningsaserrors) |
| `CS0618` also fires on calls that bind correctly, such as `("url", version: "1.5")` and `("url", authorName: "a", version: "2.0")` | Same as above | [2.3](#23-correct-calls-that-now-warn) |

### 1.3 Changes in run-time behaviour

These apply as soon as the 6.1 library is loaded, including by assemblies compiled against 6.0.

| Change | 6.0 | 6.1 | Details |
|---|---|---|---|
| `null` `items` or stream passed to an exporter | wrapped in `CodeSourceExporterException` | `ArgumentNullException` before any work | [4.2](#42-null-arguments-throw-argumentnullexception) |
| `AppliedOn` with surrounding spaces, such as `" 2022-12-12 "` | not parsed, no date exported | trimmed, parsed and exported | [2.7](#27-attribute-value-changes) |
| `Copyright` from the `copyright:` constructor argument, value missing or blank | `""` | `null` (export unchanged) | [2.7](#27-attribute-value-changes) |
| Markdown link destination | the value as written | the normalised absolute URI, for example `http://local.host/` | [5](#5-output-format-changes) |
| Exported `AppliedOn` dates | current culture | invariant culture (differs only under non-Gregorian cultures such as `th-TH`) | [5](#5-output-format-changes) |
| `FindAnnotations(Assembly)` | reloaded the assembly by name | scans the `Assembly` object you pass, so byte-loaded and plugin assemblies work | [3.3](#33-what-each-overload-scans) |
| Exported types of the scanned assembly itself cannot be listed (for example a dynamic assembly, or one exported type derives from a type in a missing dependency) | exception out of `FindAnnotations` | the whole assembly is skipped; reported through `OnError`, silent without it | [3.4](#34-which-failures-are-recoverable) |
| Attributes of a class cannot be read, but one of its members is annotated | the failure was silently ignored | the class appears as `Parent` with an empty history; reported through `OnError`, silent without it | [3.4](#34-which-failures-are-recoverable) |
| `null` arguments to `FindAnnotations` | `NullReferenceException`, or a loader exception for a name | `ArgumentNullException` for a `null` assembly, list or name; `ArgumentException` for an empty or blank name; `null` elements in a list are skipped | [3.2](#32-argument-checks) |
| `Parent.History` for a type annotated only on its members | `null` | empty list (export unchanged) | [3.6](#36-parenthistory-for-member-only-annotations) |
| Duplicate results | netstandard1.0 returned each result many times; a reference that resolved to an assembly already in the list was scanned again | each assembly is scanned once | [3.3](#33-what-each-overload-scans) |
| `Copyright` that already starts with `©`, passed to the constructor | `"© © MS"` | `"© MS"`, in the property and in exports | [2.7](#27-attribute-value-changes) |
| Public `AppliedOn` after the full (8-parameter) constructor | `null` | the value you passed | [2.7](#27-attribute-value-changes) |
| Exceptions during a scan that are not loader or reflection failures | often swallowed | propagate | [3.5](#35-unexpected-exceptions-propagate) |
| Stream passed to an exporter | closed by the exporter | left open; you dispose it | [4.1](#41-exporters-leave-your-stream-open) |
| Invalid call to `Export(format, items, savePath)` | `null` `items` truncated an existing file, then failed | all arguments checked first; the file is not touched | [4.5](#45-the-savepath-overload-checks-its-arguments-before-opening-the-file) |
| `ExporterRegistry` start-up | loaded its own assembly by name to find the built-in exporters | registers them directly, so it keeps working if the assembly is renamed or merged (for example with ILMerge) | [4.3](#43-registry) |
| CSV, Markdown, YAML and XML escaping | minimal | values are neutralised or escaped; CSV inserts `'` before formula triggers at every cell-start position | [5](#5-output-format-changes), [5.1](#51-csv-formula-protection) |
| JSON export under concurrent use | could produce malformed JSON | valid JSON | [4.4](#44-json-export-is-thread-safe) |

---

## 2. Attribute constructors and warning CS0618

### 2.1 What changed

| Constructor | 6.0 | 6.1 |
|---|---|---|
| `()` | available | unchanged |
| `(sourceUrl)` | did not exist | **new**, recommended; sets `Version = "1.0"` |
| `(sourceUrl, version)` | `version` defaulted to `"1.0"` | **obsolete** (`CS0618`); no default |
| `(sourceUrl, authorName, version)` | `authorName` and `version` had defaults | **obsolete** (`CS0618`); no defaults |
| `(sourceUrl, authorName, copyright, version)` | all but `sourceUrl` had defaults | not obsolete; no defaults |
| `(sourceUrl, authorName, copyright, appliedOn, comment, version, workItemId, tags)` | all but `sourceUrl` optional | unchanged signature; now also sets the public `AppliedOn` property |

The warning text is:

```
'CodeSourceAttribute.CodeSourceAttribute(string, string)' is obsolete: 'Use CodeSource(sourceUrl) with named properties, e.g. [CodeSource("url", AuthorName = "...", Version = "...")]. Positional arguments bind by position.'
```

The obsolete constructors still work. The warning exists because they are easy to misuse (next section).

### 2.2 Positional arguments bind by position

C# binds positional attribute arguments by position, not by what the value looks like. These calls compile, but the values land in the wrong property. **Do not write them.**

| Do not write | What you get | Write instead |
|---|---|---|
| `[CodeSource("https://example.com", "John")]` | `Version = "John"`, `AuthorName = null` | `[CodeSource("https://example.com", AuthorName = "John", Version = "2.0")]` |
| `[CodeSource("https://example.com", "John", "MIT")]` | `AuthorName = "John"`, `Version = "MIT"`, `Copyright = null` | `[CodeSource("https://example.com", AuthorName = "John", Copyright = "MIT")]` |

Even a positional version that binds correctly now warns:

| Warns | Binds to | Write instead |
|---|---|---|
| `[CodeSource("https://example.com", "2.0")]` | `Version = "2.0"` | `[CodeSource("https://example.com", Version = "2.0")]` |

The same trap exists with four or more positional arguments, without a warning. With four values, the fourth is `Version`. Add a fifth and the call switches to the 8-parameter constructor, where the fourth value becomes `AppliedOn`:

| Positional call | Fourth value lands in |
|---|---|
| `[CodeSource("https://example.com", "John", "MIT", "2.0")]` | `Version` |
| `[CodeSource("https://example.com", "John", "MIT", "2.0", "Adapted")]` | `AppliedOn` (not a date, so it is not exported); `"Adapted"` becomes `Comment` and `Version` falls back to `"1.0"` |

Use named properties to avoid both problems.

### 2.3 Correct calls that now warn

Some calls with named arguments bind correctly but still resolve to an obsolete constructor, so they raise `CS0618`. The values are right; only the style is flagged.

| Warns | Binds to | Write instead |
|---|---|---|
| `[CodeSource("https://example.com", version: "1.5")]` | `Version = "1.5"` | `[CodeSource("https://example.com", Version = "1.5")]` |
| `[CodeSource("https://example.com", authorName: "John", version: "2.0")]` | `AuthorName = "John"`, `Version = "2.0"` | `[CodeSource("https://example.com", AuthorName = "John", Version = "2.0")]` |
| `[CodeSource("https://example.com", "John", version: "2.0")]` | `AuthorName = "John"`, `Version = "2.0"` | `[CodeSource("https://example.com", AuthorName = "John", Version = "2.0")]` |

### 2.4 Named argument to named property

To fix a warning, move each named constructor argument to the matching property. Keep `sourceUrl` as the only constructor argument.

| 6.0 named argument | 6.1 named property | Same value? |
|---|---|---|
| `version: "1.5"` | `Version = "1.5"` | yes |
| `authorName: "John"` | `AuthorName = "John"` | yes |
| `copyright: "Company INC"` | `Copyright = "Company INC"` | **no**: the property has no `©` prefix. Write `Copyright = "© Company INC"` to keep the exported value. |
| `appliedOn: "2022-12-12"` | `AppliedOn = "2022-12-12"` | yes |
| `comment: "..."` | `Comment = "..."` | yes |
| `workItemId: "#123"` | `RelatedTaskId = "#123"` | yes; the property has a different name |
| `tags: "a;b"` | `Tags = "a;b"` | yes |

```csharp
using RzR.Core.CodeSource;

// 6.0 (warns in 6.1)
// [CodeSource("https://example.com", authorName: "John", version: "2.0")]

// 6.1
[CodeSource("https://example.com", AuthorName = "John", Version = "2.0")]
public class Foo
{
}
```

### 2.5 Calls that do not change

A call that supplies `copyright:`, `appliedOn:`, `comment:`, `workItemId:` or `tags:` as a named argument resolves to a non-obsolete constructor and raises no warning. You do not need to change it:

```csharp
using RzR.Core.CodeSource;

// No warning. Copyright is "© Company INC".
[CodeSource(sourceUrl: "https://example.com", authorName: "User", copyright: "Company INC", version: "2.0")]
public class Foo
{
}
```

If you move such a call to the named-property style anyway, remember the `©` difference in the table above.

### 2.6 Calls that did not compile in 6.0 now compile

In 6.0 these calls failed with `CS0121` (ambiguous call). In 6.1 they compile, raise no warning, and use the full constructor:

```csharp
using RzR.Core.CodeSource;

// Copyright is "© Company INC", Version is "1.0".
[CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC")]
public class Foo
{
}

// Copyright is null, Version is "1.0".
[CodeSource("https://example.com", authorName: "John")]
public class Bar
{
}
```

### 2.7 Attribute value changes

These do not raise warnings. Most matter only if you read attribute properties directly; the `©` change also shows in exports.

| Property | 6.0 | 6.1 |
|---|---|---|
| `Copyright` from the `copyright:` constructor argument, value missing or blank | `""` | `null`. Exported output is unchanged. |
| `Copyright` from the `copyright:` constructor argument, value already starts with `©` | `"© © MS"` | `"© MS"`. Exported output changes accordingly. |
| `AppliedOn` from the `appliedOn:` constructor argument | public property stayed `null` | public property holds the value you passed |
| `AppliedOn` with spaces, for example `" 2022-12-12 "` | not parsed, no date exported | parsed, date exported |
| `AppliedOn` invalid, for example `"12/12/2022"` | no date exported | no date exported; reported as `AttributeValue` when you pass `OnError` |

`AppliedOn` is parsed with the invariant culture and the exact format `yyyy-MM-dd`.

### 2.8 Builds with TreatWarningsAsErrors

If your project sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (or lists `CS0618` in `<WarningsAsErrors>`), the new warnings **fail the build**. Options, best first:

1. Move the affected attributes to named properties (sections 2.2 to 2.4).
2. Keep the warning as a warning while you migrate: add `<WarningsNotAsErrors>CS0618</WarningsNotAsErrors>` to the project. This also affects other obsolete APIs.
3. Suppress it around a specific attribute with `#pragma warning disable CS0618` / `#pragma warning restore CS0618`.

---

## 3. Scanner changes

### 3.1 `CodeSourceScanner.Instance` is readonly

`Instance` is now `public static readonly`. Reading it works as before. Code that assigns it no longer compiles (`CS0198`), and an assembly compiled against 6.0 that assigns it may fail at run time with `FieldAccessException`; remove the assignment and recompile.

The scanner holds no state, so there is no reason to replace it. Create your own `new CodeSourceScanner()` if you need a separate instance.

### 3.2 Argument checks

| Call | 6.0 | 6.1 |
|---|---|---|
| `FindAnnotations((Assembly)null)` | `NullReferenceException` | `ArgumentNullException` (`ParamName` `assembly`) |
| `FindAnnotations((IEnumerable<Assembly>)null)` | `NullReferenceException` | `ArgumentNullException` (`ParamName` `assemblies`) |
| `FindAnnotations(list)` where `list` contains `null` | `NullReferenceException` | the `null` element is skipped |
| `FindAnnotations(string)` with a `null` name | an exception from the assembly loader | `ArgumentNullException` (`ParamName` `assemblyName`) |
| `FindAnnotations(string)` with an empty or blank name | an exception from the assembly loader | `ArgumentException` (`ParamName` `assemblyName`) |

To handle every argument error from `FindAnnotations`, catch `ArgumentException`; `ArgumentNullException` derives from it.

`FindAnnotations(string)` expects an assembly display name such as `"MyApp.Core"`, not a file path.

### 3.3 What each overload scans

- `FindAnnotations(Assembly)` scans the `Assembly` object you pass. It no longer reloads it by name, so assemblies loaded from bytes, from a path or from a plugin load context now return results. Direct references are scanned, each once.
  - Assemblies loaded for reflection only (`ReflectionOnlyLoad` or a `MetadataLoadContext`) are still loaded again by their full name, as in 6.0. There is no fallback to a file path.
  - Dynamic assemblies are skipped and reported as `TypeEnumeration`.
  - Custom `Assembly` subclasses that do not implement the reflection APIs are not supported.
- `FindAnnotations(string)` loads the named assembly and scans it and its direct references, each once. If the named assembly itself cannot be loaded, the exception propagates.
- `FindAnnotations(IEnumerable<Assembly>)` scans exactly the assemblies you pass: no references, `null` elements skipped, and an assembly passed twice is scanned twice.
- **Duplicates:** a reference that resolves to an assembly already in the scan list is not scanned again.
- **netstandard1.0:** only the given assembly is scanned. On this target 6.0 added the given assembly to the scan list once more for each of its exported types, so every result came back many times. 6.1 returns each result once.

### 3.4 Which failures are recoverable

A recoverable failure is reported to `OnError` (if set) and the scan continues. Without `OnError`, it is skipped silently.

In the table, **file failures** means `FileNotFoundException`, `FileLoadException` and `BadImageFormatException`.

| `Stage` | Recoverable exceptions | Scan continues by |
|---|---|---|
| `AssemblyLoad` | file failures, `NotSupportedException` | Skipping that reference |
| `TypeEnumeration` | file failures, `ReflectionTypeLoadException`, `TypeLoadException`, `NotSupportedException` (for example a dynamic assembly) | Skipping that assembly, including the one you passed |
| `MemberEnumeration` | file failures, `TypeLoadException` | Skipping the constructors or methods of that type |
| `AttributeRead` | file failures, `TypeLoadException`, `NotSupportedException`, `CustomAttributeFormatException`, `MemberAccessException`, `TargetInvocationException`, `InvalidOperationException` | Skipping that type or member |
| `AttributeValue` | None; reported for an invalid value such as `AppliedOn = "12/12/2022"` (`Exception` is `null`) | Returning the annotation without that value |

`OutOfMemoryException`, `StackOverflowException` and `ThreadAbortException` are never recoverable.

Two consequences to know about:

- If the exported types of the assembly you pass cannot be listed, for example because one exported type derives from a type in a dependency that is missing at run time, the **whole assembly** is skipped, not just that type.
- If the attributes of a class cannot be read but one of its members is annotated, the class still appears as a `Parent`, with an empty history. Without `OnError`, you cannot tell this apart from a class that has no `[CodeSource]` of its own.

`CodeSourceScanError.Message` has the form `<Stage>: <exception type>: <exception message> [assembly: ...] [type: ...] [member: ...]`. It never contains a stack trace. The `<exception message>` part is the runtime's own text, which on .NET Core can include a file path the loader probed. Log it, but do not show it verbatim to end users.

### 3.5 `Parent.History` for member-only annotations

For a type that has `[CodeSource]` only on its members, `Parent.History` is now an empty list instead of `null`. Exported output is unchanged. Code that checked `History == null` should check `History.Count == 0`.

---

## 4. Exporter changes

### 4.1 Exporters leave your stream open

**Before (6.0):** `ExporterRegistry.Export(format, items, stream)` and `ICodeSourceExporter.Export(items, stream)` closed the stream when they finished.

**After (6.1):** the exporter writes, flushes and leaves the stream open. You own it.

```csharp
using System.IO;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Services;

// 6.0: the stream was closed after Export, so it could not be read back.
// 6.1: dispose the stream yourself.
using (var stream = new MemoryStream())
{
    ExporterRegistry.Export(ExportFormats.Csv, items, stream);
    stream.Position = 0; // works in 6.1
}
```

If you passed a stream without disposing it because the exporter closed it, add a `using` block. The `savePath` overload still creates the file, writes it and closes it; it now checks its arguments first (section 4.5).

### 4.2 `null` arguments throw `ArgumentNullException`

**Before (6.0):** a `null` `items` or stream failed inside the exporter, and the error surfaced wrapped in `CodeSourceExporterException`.

**After (6.1):** the exporter throws `ArgumentNullException` (`ParamName` `items` or `outputStream`) before it writes anything. `ArgumentNullException` does not derive from `CodeSourceExporterException`, so a `catch (CodeSourceExporterException)` no longer catches this case.

### 4.3 Registry

- The six built-in exporters are registered directly when the registry starts. It no longer loads its own assembly by name to find them, so it keeps working if the assembly is renamed or merged into another one.
- Format names are case-insensitive (unchanged).
- `Register` replaces an exporter already registered for the same format, including a built-in one. The last registration wins (unchanged, now documented).

### 4.4 JSON export is thread-safe

6.0 could write malformed JSON when several threads exported at the same time. 6.1 fixes this. If you added your own locking around JSON export, you can remove it.

The registry shares one exporter instance per format across all threads. A custom `ICodeSourceExporter` must not keep per-export state in fields.

### 4.5 The `savePath` overload checks its arguments before opening the file

**Before (6.0):** `Export(format, items, savePath)` opened the file with `FileMode.Create` before the exporter checked `items`. A call with `null` `items` truncated an existing file and then failed.

**After (6.1):** the overload checks the format, `items` and the path before it opens the file, so an invalid call never creates or truncates the target file.

| Argument | Exception |
|---|---|
| `format` is `null` | `ArgumentNullException` (`ParamName` `format`) |
| no exporter registered for `format` | `CodeSourceUndefinedExportFormat` |
| `items` is `null` | `ArgumentNullException` (`ParamName` `items`) |
| `savePath` is `null` | `ArgumentNullException` (`ParamName` `savePath`) |
| `savePath` is empty or blank | `ArgumentException` (`ParamName` `savePath`) |

The stream overload `Export(format, items, stream)` also checks for a `null` format first and throws `ArgumentNullException` (`ParamName` `format`).

---

## 5. Output format changes

These changes protect tools that open the exported files (spreadsheets, Markdown renderers, YAML and XML parsers). They can change the bytes of the output, so review them if you parse, diff or snapshot-test exported files.

**All formats**

- Values are trimmed before they are escaped, so leading and trailing tabs, carriage returns and line feeds are removed.
- `AppliedOn` is written with the invariant culture, so it stays a Gregorian `yyyy-MM-dd` date under cultures such as `th-TH`. Output changes only if your process ran under a non-Gregorian culture.
- A copyright set through the constructor no longer gets a second `©` (section 2.7).
- The byte order mark (BOM) is unchanged: CSV, HTML, JSON, Markdown and YAML write one to an empty stream, XML does not, and no format writes one to a stream that already holds data.

| Format | 6.1 behaviour | Example |
|---|---|---|
| CSV | A `'` is inserted wherever a spreadsheet could start a formula inside a data cell. See [section 5.1](#51-csv-formula-protection). | `=HYPERLINK(...)` is written as `"'=HYPERLINK(...)"`; `x;=1+1` as `"x;'=1+1"` |
| Markdown | `Name` and `FullName` are code spans whose backtick fence is longer than any backtick run in the value. The content is verbatim, except that line breaks become spaces; an empty name renders nothing. In table cells, `&`, `<` and `>` are entity-encoded; `\`, `` ` ``, `*`, `[` and `]` are backslash-escaped; `\|` becomes `\\|`; a newline becomes a space. | `a\|b` in a cell becomes `a\\|b` |
| Markdown links | Only absolute `http` and `https` URLs become links. The link destination is the normalised absolute URI (scheme and host lower-case, `/` added after a bare host), with spaces, control characters and characters that would break the link percent-encoded; the link text keeps your value. Other URLs are written as plain escaped text, and an empty URL gives an empty cell instead of `[]()`. | `[http://local.host](http://local.host/)`; `LocalHost/use-async` is plain text |
| YAML | Every value is double-quoted, including `fullName` and `name`, with full escaping. `fullName` and `name` are also trimmed. | `name: "OwnClassData"` |
| XML | Characters that are not allowed in XML 1.0 become `U+FFFD`. A carriage return is written as `&#xD;`. Attribute values are fully escaped. | |
| HTML | Unchanged. | |
| JSON | Unchanged for single-threaded use; valid under concurrent use (section 4.4). | |

### 5.1 CSV formula protection

Spreadsheets run a cell as a formula when it starts with `=`, `+`, `-` or `@`. Some, such as Excel with a `;` list separator, also split a value at `;` or `,` and treat each part as a cell. To block formula injection, the CSV exporter inserts a `'` at every position where a cell could start, if a trigger follows.

After the value is trimmed, a **cell-start position** is:

1. the start of the value;
2. the position after a `;` or `,` inside the value, after any whitespace that follows it;
3. the position after a carriage return or line feed inside the value, after any whitespace that follows it. A line break inside the whitespace of case 2 also counts.

At a cell-start position, the exporter looks past any run of `"` characters. If the next character is `=`, `+`, `-`, `@` or a full-width `＝`, `＋`, `－`, `＠`, it inserts `'` at the cell-start position, before the quotes. Tab, carriage return and line feed are never triggers.

Every data field is wrapped in double quotes, and quotes inside it are doubled, as before (RFC 4180). The flag and placeholder cells are not changed.

| Value | CSV field |
|---|---|
| `=HYPERLINK("http://x")` | `"'=HYPERLINK(""http://x"")"` |
| `x;=1+1;y` | `"x;'=1+1;y"` |
| `x;"=1+1";y` | `"x;'""=1+1"";y"` |
| `x; "+1` | `"x; '""+1"` |
| `a, @b` | `"a, '@b"` |
| `=a;=b,-c` | `"'=a;'=b,'-c"` |
| `x`, line feed, `=cmd` | `x`, line feed, `'=cmd` (quoted) |
| `step1;`, line feed, `step2` | no `'` added (still wrapped in quotes) |
| `a,`, tab, `b` | no `'` added (still wrapped in quotes) |
| `bug;security` | `"bug;security"` |
| `x;"hello"` | `"x;""hello"""` (the quote is not followed by a trigger) |
| `-beta` | `"'-beta"` |
| `1,-1` | `"1,'-1"` |

**Trade-off:** the `'` becomes part of the stored value. An RFC 4180 reader that parses the file gets `'=1+1`, not `=1+1`, so values that contain a trigger at a cell-start position do not round-trip. There is no exception for numbers: `-1` and `1,-1` are also prefixed. If you need the exact values back in code, read the JSON or XML export instead of the CSV.

---

## 6. Binary compatibility

Assemblies compiled against 6.0.x run against 6.1 without recompiling:

- Compiled attribute usages keep binding to the same constructors, and every 6.0 public constructor and method still exists with the same signature.
- Removing default values and adding `[Obsolete]` affect only the compiler, so the new warnings appear only when you recompile.
- The run-time behaviour changes in section 1.3 apply to precompiled assemblies as well.

Two exceptions:

- An assembly that assigns `CodeSourceScanner.Instance` may fail at run time with `FieldAccessException` (section 3.1).
- Late-bound calls that rely on the optional parameters of the 2-, 3- or 4-parameter constructors, for example through `Type.Missing` with `BindingFlags.OptionalParamBinding`, no longer get default values. Pass every argument, or use the `(sourceUrl)` constructor and set properties.

---

## 7. Checklist

1. Search for `FindAnnotations(` calls whose results must be complete, and pass `CodeSourceScanOptions` with `OnError`.
2. Remove any assignment to `CodeSourceScanner.Instance`.
3. Build and fix every `CS0618` with the tables in section 2. Watch `copyright:`: the property form drops the `©` prefix.
4. Find code that passes a stream to `Export` without disposing it, and add a `using` block.
5. Replace `catch (CodeSourceExporterException)` blocks that were meant to catch `null` arguments, and `NullReferenceException` handlers around `FindAnnotations`.
6. Re-run any snapshot or parsing tests over exported CSV, Markdown, YAML or XML files.
