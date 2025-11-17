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

So to see all code source references added to your code, just make a method call for every assembly you have.
```csharp
// Obsolete method
var codeSource = CodeSourceHelper.GetCodeSourceAssembly("AssemblyName");

// New method that can be used
var codeSource = new CodeSourceScanner().FindAnnotations("AssemblyName");
```

