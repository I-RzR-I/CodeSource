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
[CodeSource(sourceUrl: null, authorName: "Company User", copyright: "Company INC")]
   public class Foo
   {}
```

```csharp
[CodeSource(sourceUrl: "https://company.com", authorName: "User", copyright: "Company INC")]
  public class Foo
  {
      [CodeSource("LocalHost/use-async", "User", "Company INC", "2022-12-12", "This is how to use async")]
      public async Task RunAsync()
      {
          await Task.CompletedTask;
      }
  }
```


### Generate code source history

Available code sourc esearch methods are:
```csharp
public interface ICodeSourceScanner
{
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly);
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName);
    IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies);
}
```

So to see all code source references added to your code, just make a method call for every assembly you have.
```csharp
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
public class ExporterRegistry
{
    void Export(string format, IEnumerable<CodeSourceObjectsResult> items, string savePath);
    
    void Export(string format, IEnumerable<CodeSourceObjectsResult> items, Stream stream);
}
```




