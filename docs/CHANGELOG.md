### **v4.0.0.2838** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 27-11-2025
* [14ddc6a] (RzR) -> Auto commit uncommited files
* [e8a9f13] (RzR) -> [#11],[#12],[#13] - Adjust documentation and add result examples.
* [f63b733] (RzR) -> Add some exporter tests.
* [da2803f] (RzR) -> [#13] - AAdd exporter registry entry point.
* [f1d35de] (RzR) -> [#13] - Add XML export functionality.
* [8e9cc9c] (RzR) -> [#13] - Add JSON export functionality.
* [05e20aa] (RzR) -> [#13] - Add YAML export functionality.
* [ccecdd0] (RzR) -> [#13] - Add MD export functionality.
* [7d6d74b] (RzR) -> [#13] - Add HTML export functionality.
* [f4e79a8] (RzR) -> [#13] - Add CSV export functionality.
* [4b07ad2] (RzR) -> [#13] - Add new internal extension methods.
* [e382f2a] (RzR) -> [#13] - Add common exception.
* [34d3858] (RzR) -> [#13] - Add code source history export to a Markdown (MD) file.
* [86e7025] (RzR) -> [#12] - Exclude the obsolete method in the history generation helper and adjust the code accordingly.
* [dee79ae] (RzR) -> [#12] - Fix the application of the children's code history generation.
* [f8b34cf] (RzR) -> [#11] - Fix `InternalAppliedOn` value set/get.
* [f4347b0] (RzR) -> [#12] - Adjust the default value for `Version` to `1.0`.
* [0d9a1ab] (RzR) -> [#12] - Fix `CodePath` generation in history.
* [f7096f8] (RzR) -> [#11] - Add `SourceUrl` validator.

### **v3.0.0.6590** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 17-11-2025
* [69e6313] (RzR) -> Adjust code generation flow.
* [15ce35f] (RzR) -> Auto commit uncommited files
* [65da579] (RzR) -> Adjust readme, using, version generate code.
* [fc658b1] (RzR) -> Adjust the processing annotation and the result.
* [a1de5c2] (RzR) -> Add code scanner service.
* [11e48be] (RzR) -> Add new fields: `Tags`, `RelatedTaskId`.
* [0f19be6] (RzR) -> Add multiple internal string extension methods.

### **v2.0.0.0** 
-> Change visibility for set action in CodeSourceAttribute.<br />
-> Recreate the code source history generation.<br />

### **v1.0.6.0933** 
-> Fix wrong modification.<br />

### **v1.0.5.1933** 
-> Add sign key and adjust attribute model.<br />

### **v1.0.4.1723** 
-> Was added change version on attribute.<br />

### **v1.0.3.0959** 
-> Removed `System.Runtime` reference to use current installed in project. In special for old projects where was added another references with old version (ex. 4.1.0 and in current package uses 4.3.0 -> on adding reference to another project, on publish will be error).<br />

### **v1.0.2.1848** 
-> Cleaned up code, and added `System.Runtime` reference.<br />

### **v1.0.2.1659** 
-> Added support for net framework 4.0+.<br />
