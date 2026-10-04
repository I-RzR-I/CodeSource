# Publishing a library that uses CodeSource

This page is for authors of NuGet packages whose code applies `[CodeSource]`. It applies to `RzR.Core.CodeSource` 7.0 and later.

A library used only through a project reference inside your own solution is not affected. Use the `DefineConstants` line from [Setting up CODESOURCE](../README.md#setting-up-codesource) for it.

## Why a library needs more than the DefineConstants line

**The risk:** if your dll references `RzR.Core.CodeSource` but your package does not list it as a dependency, consumers build fine and then get `FileNotFoundException` when anything reads attributes from your types.

Two settings control this:

- **Is `CODESOURCE` defined?** Then the compiler writes the attribute data into your dll, together with an assembly reference to `RzR.Core.CodeSource` (the entry in your dll's metadata that tells the runtime to load that dll).
- **Does the dependency flow?** The package reference flows when it appears as a dependency in your package's `.nuspec`, so consumers restore and deploy `RzR.Core.CodeSource` too. `PrivateAssets="all"` stops it from flowing.

| | Dependency flows (listed in your .nuspec) | Dependency private (`PrivateAssets="all"`) |
|---|---|---|
| **`CODESOURCE` defined** | OK: your dll carries data, consumers get the package. **The `EmitCodeSource` block with `EmitCodeSource=true`.** | **Broken:** consumers get `FileNotFoundException` at run time. |
| **`CODESOURCE` not defined** | OK but wasteful: consumers restore a package your dll does not use. | OK: no data, no dependency. **The `EmitCodeSource` block default (`EmitCodeSource=false`).** |

The `EmitCodeSource` block below always lands in one of the two cells that name it (top left with `EmitCodeSource=true`, bottom right by default), because one switch sets both values.

## The EmitCodeSource block

Use this block in the library's csproj instead of a plain `PackageReference` and the `DefineConstants` line:

<!-- Keep in sync with README.md#using-codesource-in-a-library -->
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

- `EmitCodeSource` is a property this block defines in your csproj. The package does not provide it, so `-p:EmitCodeSource=true` does nothing in a project without the block.
- Replace `7.0.0.x` with the exact 4-part version from [nuget.org](https://www.nuget.org/packages/RzR.Core.CodeSource). Do not shorten it to `7.0.0`: NuGet then reports `warning NU1603: ... depends on RzR.Core.CodeSource (>= 7.0.0) but RzR.Core.CodeSource 7.0.0 was not found` on every restore, which fails builds that treat warnings as errors.
- With Central Package Management, remove `Version="..."` from the block (a version there causes error `NU1008`) and set the version in `Directory.Packages.props`. Keep the `PrivateAssets` element.

| How you pack | Your dll | Your nuspec | Consumers |
|---|---|---|---|
| `dotnet pack -c Release` (`EmitCodeSource=false`, the default) | No `[CodeSource]` data, no reference to `RzR.Core.CodeSource` | No dependency on `RzR.Core.CodeSource` | Do not restore or deploy CodeSource. Reflection and the scanner find nothing in your dll. |
| `dotnet pack -c Release -p:EmitCodeSource=true` | Attribute data and a reference to `RzR.Core.CodeSource` | Dependency on `RzR.Core.CodeSource` | Get the package. Reflection and the scanner work on your dll. |

With `EmitCodeSource=true`, everything in your `[CodeSource]` attributes (author names, URLs, comments, ticket ids) can be read by anyone who installs the package, and cannot be withdrawn from copies already published. Do not put e-mail addresses, internal hostnames or private ticket links in them, or keep the default `EmitCodeSource=false`.

## Rules

1. **Change only `EmitCodeSource`.** In a library that uses the block, `CODESOURCE` must come only from `EmitCodeSource=true`. Do not set it in `Directory.Build.props` or other shared files, do not pass `-p:DefineConstants=...` on the command line or in CI, and do not use a per-file `#define CODESOURCE` (C#) or `#Const CODESOURCE = True` (VB). Each of these defines the symbol while the dependency is private.
2. **Never add `PrivateAssets` or `ExcludeAssets` yourself.** The block sets `PrivateAssets` when it is safe. `ExcludeAssets="runtime"` keeps the dependency in the nuspec but removes the dll, and consumers fail the same way.
3. **Use the same value for restore, build and pack.** Prefer a single `dotnet pack -p:EmitCodeSource=true`, which restores and builds itself. A separate `dotnet restore` without the property, followed by `dotnet build --no-restore -p:EmitCodeSource=true` and `dotnet pack --no-build`, produces a dll with data and a nuspec with no dependency.
4. **Do not use CodeSource types in library code unless you build with `EmitCodeSource=true`.** `typeof(CodeSourceAttribute)`, scanner calls and any other use of CodeSource types add the reference to your dll even when the attribute is omitted. `nameof`, `cref` and `using` alone do not.
5. **Keep the `PackageReference`.** Omitted attributes are still type-checked, so warnings such as `CS0618` still appear, and without the reference the build fails with `CS0246`.

## What a broken package looks like

Every mistake above builds green in your library and in its consumers, and normal method calls into your library keep working. The failure appears when a consumer reads attributes from your types:

- `GetCustomAttributes`, any overload, even one that asks for a different attribute type;
- `IsDefined` and `GetCustomAttributesData`;
- `inherit: true` lookups on types derived from yours;
- assembly scans, and serializers and DI containers that read attributes.

On .NET 8 the exception is:

```
System.IO.FileNotFoundException: Could not load file or assembly 'RzR.Core.CodeSource, Version=<assembly version>, Culture=neutral, PublicKeyToken=null'. The system cannot find the file specified.
```

`<assembly version>` is the assembly version of the `RzR.Core.CodeSource` dll you compiled against. On .NET Framework the message adds `or one of its dependencies`. If library code uses CodeSource types (rule 4), the same exception is thrown when that code runs.

### Tested failure cases

| Built with | Nuspec dependency | Consumer result |
|---|---|---|
| `-p:DefineConstants=CODESOURCE` (symbol with a private dependency) | none | `FileNotFoundException` |
| Separate restore, then `build`/`pack` with `-p:EmitCodeSource=true` | none | `FileNotFoundException` |
| Default build, `typeof(CodeSourceAttribute)` in library code | none | `FileNotFoundException` when that code runs |
| Plain `$(DefineConstants);CODESOURCE` with `ExcludeAssets="runtime"` | present, excludes Runtime | `FileNotFoundException` |


## Testing a library that uses the block

The [guard test](../README.md#add-a-guard-test) needs two changes when the library under test uses the `EmitCodeSource` block:

- **Give the test project its own package reference**, for example `<PackageReference Include="RzR.Core.CodeSource" Version="7.0.0.x" />`. The block keeps the library's reference private, so it does not flow through the project reference, and the test fails to compile with `CS0246` on the `RzR` namespace in its `using` lines.
- **Run the tests with the switch:** `dotnet test -p:EmitCodeSource=true`. The library's default build has no data, so without the switch the guard always fails.
