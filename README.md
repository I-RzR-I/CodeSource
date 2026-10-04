[![NuGet Version](https://img.shields.io/nuget/v/RzR.Core.CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Core.CodeSource/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.Core.CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Core.CodeSource/)

<details>

  <summary>Legacy package (`CodeSource`)</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)

</details>

<br />

# RzR.Core.CodeSource

Record where borrowed or inspired code came from, right next to the code, then scan your assemblies and export a report.

You put a `[CodeSource]` attribute on a class, constructor or method. It holds the source URL, author, copyright, date, version, tags and a work-item id. You can then scan the compiled assemblies and export what was recorded as CSV, HTML, JSON, Markdown, XML or YAML. Typical uses:

- Keep attribution and licence information for code adapted from blogs, Stack Overflow, documentation or other repositories.
- Produce a "third-party code" report for an audit or a release. Read [Is the report complete?](#is-the-report-complete) first.
- Link a piece of code to the ticket or design note that explains it.

The package targets net40, net45, netstandard1.0, netstandard1.5, netstandard2.0 and netstandard2.1. It has no dependencies on net40, net45 and netstandard2.x; on netstandard1.x it pulls in `NETStandard.Library`.

> **Breaking change in 7.0: define the `CODESOURCE` symbol, or the attribute is dropped.**
>
> - `[CodeSource]` is now `[Conditional("CODESOURCE")]`. The compiler keeps it only in projects that define `CODESOURCE`.
> - Without the symbol the build succeeds with no warning, and the scanner and reflection find nothing.
> - **Fix:** define `CODESOURCE` in every project that applies `[CodeSource]` ([Setting up CODESOURCE](#setting-up-codesource)). Publishing a NuGet package? Use the [`EmitCodeSource` block](#using-codesource-in-a-library) instead, in the same change as the upgrade to 7.0.
> - Assemblies already compiled against 6.x keep their data. Upgrading from 6.x? Read [Migrating from 6.x to 7.0](docs/migration-guide.md#migrating-from-6x-to-70).

## Contents

- [Install](#install)
- [Quick start](#quick-start)
- [Setting up CODESOURCE](#setting-up-codesource)
  - [Language support](#language-support)
- [Verify it works](#verify-it-works)
- [Usage](#usage)
  - [Is the report complete?](#is-the-report-complete)
- [Using CodeSource in a library](#using-codesource-in-a-library)
- [Documentation](#documentation)

## Install

Use the exact 4-part version from [nuget.org](https://www.nuget.org/packages/RzR.Core.CodeSource) (versions look like `6.1.0.5877`). This README writes the 7.0 version as `7.0.0.x`.

```bash
dotnet add package RzR.Core.CodeSource --version 7.0.0.x
```

```powershell
Install-Package RzR.Core.CodeSource -Version 7.0.0.x
```

Moving from the old `CodeSource` package? Change the package reference and the namespaces together; see [Package ID rename](docs/migration-guide.md#package-id-rename).

## Quick start

**1. Define `CODESOURCE` in the project that applies the attribute.** Add the `PropertyGroup` below to its `.csproj`, with no `Condition` on it. The `ItemGroup` is the package reference; skip it if `dotnet add package` already added one.

```xml
<ItemGroup>
  <PackageReference Include="RzR.Core.CodeSource" Version="7.0.0.x" />
</ItemGroup>
<PropertyGroup>
  <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
</PropertyGroup>
```

This form is for one SDK-style C# or F# project. For VB, old-style csproj files, a shared setting across many projects, or a NuGet package you publish, see [Setting up CODESOURCE](#setting-up-codesource).

**2. Annotate the code.** Pass the source URL as the only constructor argument and set everything else with named properties:

```csharp
using System;
using RzR.Core.CodeSource;

namespace Samples.Library
{
    [CodeSource("https://learn.microsoft.com/azure/architecture/patterns/retry",
        AuthorName = "Jane Doe",
        AppliedOn = "2026-01-15",
        Comment = "Retry pattern from the Azure Architecture Center",
        RelatedTaskId = "#101")]
    public class RetryPolicy
    {
        [CodeSource("https://en.wikipedia.org/wiki/Exponential_backoff", AuthorName = "Jane Doe")]
        public TimeSpan GetDelay(int attempt)
        {
            return TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));
        }
    }
}
```

**3. Scan and export.** In any app or tool that references the annotated assembly:

```csharp
using System.Linq;
using RzR.Core.CodeSource;
using RzR.Core.CodeSource.Services;
using Samples.Library;

public static class Program
{
    public static void Main()
    {
        var results = CodeSourceScanner.Instance
            .FindAnnotations(typeof(RetryPolicy).Assembly)
            .ToList();

        ExporterRegistry.Export(ExportFormats.Markdown, results, "code-sources.md");
    }
}
```

`code-sources.md` now lists `RetryPolicy` with its history and the `GetDelay` method. For what a complete report looks like (generated from a different sample library), see [docs/result/result_in_md.md](docs/result/result_in_md.md).

- Passing one `Assembly` also scans its direct references. Pass an array (`new[] { assembly }`) to scan only that assembly.
- The app that scans does not need `CODESOURCE`. Only the project that applies the attribute does.
- If the report is empty, the symbol did not reach the compiler. Go to [Verify it works](#verify-it-works).

## Setting up CODESOURCE

Three rules apply to every setup:

1. **Define it unconditionally.** Put it in a `PropertyGroup` with no `Configuration` or `TargetFramework` condition, so Debug, Release and every target framework get it. A Debug-only group builds green with 0 warnings, and the Release build silently has no data.
2. **Define it in every project that applies `[CodeSource]`.** The symbol counts only in the compilation that contains the attribute. In C# and F# it is case-sensitive: `codesource` has no effect. Use upper case everywhere.
3. **The reading app's symbol does not matter.** Defining `CODESOURCE` in an app or test project does not add data to a library it references, and an app that only scans does not need it.

Find your situation:

| Your project | What to add | Section |
|---|---|---|
| SDK-style C# or F# app, test project, or library you do not publish | The `DefineConstants` line `$(DefineConstants);CODESOURCE` | [SDK-style C# and F#](#sdk-style-c-and-f) |
| SDK-style VB project | `$(DefineConstants),CODESOURCE=True` | [VB](#vb) |
| Many SDK-style projects, any mix of C#, VB and F# | One `Directory.Build.props` at the solution root | [Many projects](#many-projects-directorybuildprops-and-targets) |
| Solution that also contains old-style (non-SDK) csproj files | `Directory.Build.props` for VB, `Directory.Build.targets` for C# and F# | [Many projects](#many-projects-directorybuildprops-and-targets) |
| One old-style csproj (packages.config, typical for net40/net45) | `;CODESOURCE` in each configuration group, or one group after them | [Old-style csproj](#old-style-csproj) |
| A library you publish as a NuGet package | The `EmitCodeSource` block, instead of the `DefineConstants` line | [Using CodeSource in a library](#using-codesource-in-a-library) |

In C#, to emit the attributes of a single file only, put `#define CODESOURCE` at the top of that file (VB: `#Const CODESOURCE = True`; F# has no per-file option). See [Enabling CODESOURCE](docs/usage.md#enabling-codesource-required-since-70) for when that is safe.

### Language support

C#, VB and F# are verified with the .NET 9 SDK: in all three, the compiler omits `[CodeSource]` without the symbol and emits it with the symbol. Old-style vbproj files were not tested.

| Language | Attribute syntax |
|---|---|
| C# | `[CodeSource("https://example.com/blog/retry", AuthorName = "Jane Doe")]` |
| F# | `[<CodeSource("https://example.com/blog/retry", AuthorName = "Jane Doe")>]` |
| VB | `<CodeSource("https://example.com/blog/retry", AuthorName:="Jane Doe")>` |

### SDK-style C# and F#

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
</PropertyGroup>
```

Do not put the symbol in a configuration-specific group. This is the trap:

```xml
<!-- Wrong: Release builds succeed but contain no [CodeSource] data -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
</PropertyGroup>
```

### VB

VB separates symbols with commas and gives each a value. The C# form fails the build with `error BC31030: Conditional compilation constant ... is not valid: Identifier expected.` `=True` is optional, but keep it for clarity.

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants),CODESOURCE=True</DefineConstants>
</PropertyGroup>
```

### Many projects: Directory.Build.props and .targets

If the solution also contains a library that you pack with the [`EmitCodeSource` block](#using-codesource-in-a-library), do not use these files for the symbol: they would define it in that library too. Add the `DefineConstants` line to each of the other projects instead.

#### All projects are SDK-style

Put one `Directory.Build.props` at the solution root. It picks the right syntax per language:

```xml
<Project>
  <!-- VB uses comma-separated name=value pairs; C# and F# use semicolon-separated names -->
  <PropertyGroup Condition="'$(MSBuildProjectExtension)' == '.vbproj'">
    <DefineConstants>$(DefineConstants),CODESOURCE=True</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition="'$(MSBuildProjectExtension)' != '.vbproj'">
    <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
  </PropertyGroup>
</Project>
```

#### The solution also has old-style csproj files

Put VB in `Directory.Build.props` and C#/F# in `Directory.Build.targets`. Swapping them fails silently:

- **The C# line in `Directory.Build.props` is lost in an old-style csproj.** Its per-configuration groups assign `DefineConstants` later and overwrite it.
- **The VB line in `Directory.Build.targets` is lost.** MSBuild still lists `CODESOURCE=True` in `DefineConstants`, but VB has already built the list it passes to the compiler (`FinalDefineConstants`), so the dll has no data.

`Directory.Build.props` (VB only):

```xml
<Project>
  <!-- VB: must be set in .props; VB computes FinalDefineConstants before Directory.Build.targets is imported -->
  <PropertyGroup Condition="'$(MSBuildProjectExtension)' == '.vbproj'">
    <DefineConstants>$(DefineConstants),CODESOURCE=True</DefineConstants>
  </PropertyGroup>
</Project>
```

`Directory.Build.targets` (C# and F#):

```xml
<Project>
  <!-- C# and F#: set in .targets so that a project which assigns DefineConstants
       (for example per-configuration groups in an old-style csproj) cannot overwrite it -->
  <PropertyGroup Condition="'$(MSBuildProjectExtension)' != '.vbproj'">
    <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
  </PropertyGroup>
</Project>
```

### Old-style csproj

Old-style (non-SDK) csproj files assign `DefineConstants` inside each configuration group, without `$(DefineConstants)`. A group placed above them, or a line in `Directory.Build.props`, is overwritten. Use one of these two fixes.

Fix A: add `CODESOURCE` to each configuration group.

```xml
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
  <DefineConstants>DEBUG;TRACE;CODESOURCE</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
  <DefineConstants>TRACE;CODESOURCE</DefineConstants>
</PropertyGroup>
```

Fix B: one unconditional group placed after all configuration groups. It also covers configurations you add later.

```xml
<!-- after the last <PropertyGroup Condition="'$(Configuration)|$(Platform)' == ..."> -->
<PropertyGroup>
  <DefineConstants>$(DefineConstants);CODESOURCE</DefineConstants>
</PropertyGroup>
```

Fix B survives edits in the Visual Studio property pages. Fix A may not, so check that `;CODESOURCE` is still there after you change "Conditional compilation symbols". A [`Directory.Build.targets` file](#many-projects-directorybuildprops-and-targets) also works.

## Verify it works

A missing symbol never fails the build, so check it in one of these ways.

### Check the effective symbols

This needs .NET SDK 8.0.100 or later, or Visual Studio 2022 17.8+ `MSBuild.exe`. SDK 7 fails with `MSB1001: Unknown switch`.

```bash
dotnet msbuild MyProject.csproj -getProperty:DefineConstants -p:Configuration=Release -p:TargetFramework=net8.0
```

Replace `net8.0` with one of your project's target frameworks, for example `netstandard2.0` or `net48`. The output is the raw value, for example `TRACE;CODESOURCE;RELEASE;NET;NET8_0;NETCOREAPP`. Check both Debug and Release.

- On a multi-targeted project, always pass `-p:TargetFramework`. Without it you see only the project-level part.
- For an old-style csproj, run the same switch from a Developer Command Prompt: `MSBuild.exe MyProject.csproj -getProperty:DefineConstants -p:Configuration=Release`.
- For VB, query `-getProperty:FinalDefineConstants`. `DefineConstants` can show `CODESOURCE=True` while the compiler does not receive it.
- Do not add `-p:DefineConstants=...` to this or any build command. A global property replaces the whole list, so `TRACE`, `RELEASE` and the target-framework symbols disappear too.

This proves only what MSBuild evaluates. For an end-to-end proof, scan the built assembly with a guard test.

### Add a guard test

Keep a test that fails when an assembly you expect to carry annotations scans to an empty result. NUnit:

```csharp
using System.Linq;
using NUnit.Framework;
using RzR.Core.CodeSource.Services;
using Samples.Library;

[TestFixture]
public class CodeSourceGuardTests
{
    [Test]
    public void SamplesLibrary_ContainsCodeSourceAnnotations()
    {
        var assembly = typeof(RetryPolicy).Assembly;

        var results = CodeSourceScanner.Instance.FindAnnotations(new[] { assembly }).ToList();

        Assert.That(results, Is.Not.Empty,
            assembly.GetName().Name + " has no [CodeSource] data. Is CODESOURCE defined in its project?");
    }
}
```

When the library loses the symbol, the test fails with:

```
  Error Message:
     Samples.Library has no [CodeSource] data. Is CODESOURCE defined in its project?
  Expected: not <empty>
  But was:  <empty>
```

- With xUnit, use the same scan and `Assert.True(results.Count > 0, "<message>")`.
- The test project does not need `CODESOURCE` unless it applies `[CodeSource]` itself.
- If the guarded library uses the `EmitCodeSource` block, the test needs its own package reference and a switch; see [Testing a library that uses the block](docs/emitcodesource.md#testing-a-library-that-uses-the-block).
- Related checks: a [test that fails on loader errors](docs/usage.md#fail-a-test-on-loader-errors), a [startup check](docs/migration-guide.md#keep-a-guard-against-empty-scans), and [checking which CodeSource version you run](docs/usage.md#check-which-version-you-run).

## Usage

A short tour. The full reference is [docs/usage.md](docs/usage.md).

### Attribute properties

All properties are `string`.

| Property | What to put in it |
|---|---|
| `SourceUrl` | URL of the original code or idea. Usually the only constructor argument. |
| `AuthorName` | Author of the original code |
| `Copyright` | Copyright holder |
| `AppliedOn` | Date you applied it, in `yyyy-MM-dd` format |
| `Comment` | Free text |
| `Version` | Version of this change, for example `"1.1"` |
| `Tags` | Free text; by convention `;`-separated, for example `"resilience;http"` |
| `RelatedTaskId` | Work-item id, for example `"#123"` |

Edge cases (the `copyright:` prefix, invalid dates, the default version, export column names): see [Applying the attribute](docs/usage.md#applying-the-attribute).

Avoid extra positional arguments. `[CodeSource("https://example.com", "Jane Doe")]` sets `Version` to `"Jane Doe"`. Since 6.1 that form raises warning `CS0618`.

### A complete example

The attribute can be applied several times to the same element, so one class can carry its whole history:

```csharp
using System;
using RzR.Core.CodeSource;

namespace Samples.Library
{
    [CodeSource("https://learn.microsoft.com/azure/architecture/patterns/retry",
        AuthorName = "Jane Doe",
        Copyright = "Example Ltd",
        AppliedOn = "2026-01-15",
        Comment = "Retry pattern from the Azure Architecture Center",
        Version = "1.0",
        Tags = "resilience;http",
        RelatedTaskId = "#101")]
    [CodeSource("https://example.com/blog/retry-with-jitter",
        AuthorName = "John Roe",
        AppliedOn = "2026-09-29",
        Comment = "Added jitter",
        Version = "1.1",
        RelatedTaskId = "#142")]
    public class RetryPolicy
    {
        [CodeSource("https://example.com/blog/retry-options", AuthorName = "Jane Doe", Version = "1.0")]
        public RetryPolicy(int maxAttempts)
        {
            MaxAttempts = maxAttempts;
        }

        [CodeSource("https://example.com/blog/max-attempts", Comment = "Kept in metadata, not reported by the scanner")]
        public int MaxAttempts { get; }

        [CodeSource("https://en.wikipedia.org/wiki/Exponential_backoff",
            AuthorName = "Jane Doe",
            AppliedOn = "2026-02-01",
            Tags = "algorithm",
            RelatedTaskId = "#117")]
        public TimeSpan GetDelay(int attempt)
        {
            return TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));
        }
    }
}
```

### Scanning

```csharp
using System;
using System.Globalization;
using System.Linq;
using RzR.Core.CodeSource.Models;
using RzR.Core.CodeSource.Services;
using Samples.Library;

public static class Program
{
    public static void Main()
    {
        var results = CodeSourceScanner.Instance
            .FindAnnotations(typeof(RetryPolicy).Assembly)
            .ToList();

        foreach (var result in results)
        {
            Console.WriteLine("Type: " + result.Parent.FullName + " (" + result.Parent.History.Count + " entries)");
            foreach (var history in result.Parent.History)
                Print(history);

            foreach (var member in result.Children)
            {
                Console.WriteLine("  Member: " + member.FullName + " (" + member.History.Count + " entries)");
                foreach (var history in member.History)
                    Print(history);
            }
        }
    }

    private static void Print(CodeSourceObjectHistory history)
    {
        var appliedOn = history.AppliedOn.HasValue
            ? history.AppliedOn.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "-";

        Console.WriteLine("    " + history.CodePath + " v" + history.Version + " by " + history.AuthorName
            + " on " + appliedOn + " | " + history.SourceUrl
            + " | copyright: " + history.Copyright + " | tags: " + history.Tags
            + " | work item: " + history.RelatedTaskId + " | comment: " + history.Comment);
    }
}
```

Output for a `Samples.Library` that contains only `RetryPolicy`:

```
Type: Samples.Library.RetryPolicy (2 entries)
    Samples.Library.RetryPolicy v1.0 by Jane Doe on 2026-01-15 | https://learn.microsoft.com/azure/architecture/patterns/retry | copyright: Example Ltd | tags: resilience;http | work item: #101 | comment: Retry pattern from the Azure Architecture Center
    Samples.Library.RetryPolicy v1.1 by John Roe on 2026-09-29 | https://example.com/blog/retry-with-jitter | copyright:  | tags:  | work item: #142 | comment: Added jitter
  Member: Samples.Library.RetryPolicy.ctor (1 entries)
    Samples.Library.RetryPolicy.ctor v1.0 by Jane Doe on - | https://example.com/blog/retry-options | copyright:  | tags:  | work item:  | comment: 
  Member: Samples.Library.RetryPolicy.GetDelay (1 entries)
    Samples.Library.RetryPolicy.GetDelay v1.0 by Jane Doe on 2026-02-01 | https://en.wikipedia.org/wiki/Exponential_backoff | copyright:  | tags: algorithm | work item: #117 | comment: 
```

What the scanner reports, in short (details in [usage.md](docs/usage.md#generate-code-source-history)):

- Public types, their constructors and their methods. **Not** properties, fields, events or non-public types: `MaxAttempts` is missing from the output above for that reason.
- The attribute is inherited, so a public derived class is reported with its base class's annotations.
- `FindAnnotations(Assembly)` also scans direct references; `FindAnnotations(IEnumerable<Assembly>)` scans exactly what you pass. The scanned assemblies are loaded into your process, so scan only assemblies you trust.
- Without an error callback, assemblies and types that fail to load are skipped silently. In tooling and CI, always pass `CodeSourceScanOptions` with `OnError`; see [Reporting scan errors](docs/usage.md#reporting-scan-errors).

### Is the report complete?

If you use the report as attribution or licence evidence, know what it can silently leave out:

- annotations in projects built without `CODESOURCE` (7.0+), including projects that moved to 7.0 through a transitive upgrade, or that define the symbol for Debug only;
- annotations on properties, fields, events and non-public types;
- assemblies and types that fail to load, unless you pass `OnError`.

Treat the `[CodeSource]` attributes in source as the record. Cross-check the report against a source search for `[CodeSource` (`<CodeSource` in VB), and run the [guard test](#add-a-guard-test) in the build that produces the report.

### Exporting

Pass the scan results and a format to `ExporterRegistry.Export`, with a file path (as in the [Quick start](#quick-start)) or a stream you own:

| Format | Constant | Sample output |
|---|---|---|
| CSV | `ExportFormats.Csv` | [result_in_csv.csv](docs/result/result_in_csv.csv) |
| HTML | `ExportFormats.Html` | [result_in_html.html](docs/result/result_in_html.html) |
| JSON | `ExportFormats.Json` | [result_in_json.json](docs/result/result_in_json.json) |
| Markdown | `ExportFormats.Markdown` (`"MD"`) | [result_in_md.md](docs/result/result_in_md.md) |
| XML | `ExportFormats.Xml` | [result_in_xml.xml](docs/result/result_in_xml.xml) |
| YAML | `ExportFormats.Yaml` | [result_in_yaml.yaml](docs/result/result_in_yaml.yaml) |

Exporting to a stream or a string, the byte order mark rules, and argument errors: see [Export generated code source history](docs/usage.md#export-generated-code-source-history). To add your own format, see [Custom exporter registration](docs/usage.md#custom-exporter-registration).

## Using CodeSource in a library

This section is for authors of NuGet packages whose code applies `[CodeSource]`. A library used only through a project reference inside your own solution is not affected; use the [`DefineConstants` line](#sdk-style-c-and-f).

**The risk:** if your dll references `RzR.Core.CodeSource` but your package does not list it as a dependency, consumers build fine and then get `FileNotFoundException` when anything reads attributes from your types. Defining `CODESOURCE` makes your dll reference `RzR.Core.CodeSource`, so the symbol and the package dependency must change together.

Use this `EmitCodeSource` block in the library's csproj instead of a plain `PackageReference` and the `DefineConstants` line:

<!-- Keep in sync with docs/emitcodesource.md#the-emitcodesource-block -->
```xml
<PropertyGroup>
  <EmitCodeSource Condition="'$(EmitCodeSource)' == ''">false</EmitCodeSource>
  <DefineConstants Condition="'$(EmitCodeSource)' == 'true'">$(DefineConstants);CODESOURCE</DefineConstants>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="RzR.Core.CodeSource" Version="7.0.0.x">
    <PrivateAssets Condition="'$(EmitCodeSource)' != 'true'">all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

> **Warning: the block works only with 7.0 or later.** With a 6.x version the attribute is always emitted, so the default `EmitCodeSource=false` makes the package reference private while your dll still references `RzR.Core.CodeSource`, and consumers get `FileNotFoundException`. Upgrade the version and add the block in the same change, never before.

| How you pack | Your package |
|---|---|
| `dotnet pack -c Release` (`EmitCodeSource=false`, the default) | No `[CodeSource]` data and no dependency on `RzR.Core.CodeSource` |
| `dotnet pack -c Release -p:EmitCodeSource=true` | Attribute data, and a dependency on `RzR.Core.CodeSource` |

`EmitCodeSource` is a property this block defines; the package does not provide it. Replace `7.0.0.x` with the exact 4-part version: `7.0.0` causes warning `NU1603`.

Before you publish, read [Publishing a library that uses CodeSource](docs/emitcodesource.md). It covers the rules that keep the package safe, what a broken package looks like, how to test the library, and a [CI gate](docs/emitcodesource.md#recommended-ci-gate) that checks every package before you push it.

## Documentation

1. [Usage reference](docs/usage.md): full reference for the attribute, the scanner and the exporters.
2. [Publishing a library](docs/emitcodesource.md): publishing a NuGet package that applies `[CodeSource]`.
3. [Migration guide](docs/migration-guide.md): upgrading from 6.x to 7.0, from 6.0 to 6.1, and from the old `CodeSource` package.
4. [Changelog](docs/CHANGELOG.md)
5. [Branch guide](docs/branch-guide.md)

