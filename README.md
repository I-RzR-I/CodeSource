> **Note** This repository is developed for .netstandard1.0 and .net framework 4.0+

[![NuGet Version](https://img.shields.io/nuget/v/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/CodeSource.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/CodeSource/)

The primary purpose of this repository/library is to make an easy, accurate, and organized solution for storing data in your source code about some ideas, comments, or code references, which was an inspiration for realizing your current functionality.

From the box is provided an attribute with multiple input parameters such as: `SourceUrl`, `AuthorName`, `Copyright`, `AppliedOn`, `Comment`, `Tags`, `RelatedTaskId`.

Also, it was implemented a method that can return a list with every place where was applied attribute grouped by class with user-specified details.

In addition to generating code source history, the current implementation can export possibilities.
From the box, built-in export formats are: `CSV`, `HTML`, `JSON`, `YAML`, `XML`, and `MD`(Markdown).

No additional components or packs are required for use. So, it only needs to be added/installed in the project and can be used instantly.

**In case you wish to use it in your project, u can install the package from <a href="https://www.nuget.org/packages/CodeSource" target="_blank">nuget.org</a>** or specify what version you want:


> `Install-Package CodeSource -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)