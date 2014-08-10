// Include the following assemblies :
// (1) Microsoft.Scripting.dll
// (2) Microsoft.Scripting.Core.dll
// (3) IronPython.dll
// (4) IronPython.Modules.dll
// (5) Microsoft.Scripting.ExtensionAttribute.dll

// (1) and (2) Dynamic Language Runtime
// (3) Python interpreter
// (4) Built-in modules (written in C in CPython)

// DLR specs :
//https://dlr.codeplex.com/wikipage?title=Docs%20and%20specs
public TResult Execute<TResult>(string file, string className, string methodName, params object[] parameters)
{
  // sandboxing with appdomains : http://msdn.microsoft.com/en-us/library/bb763046.aspx
  //var sandbox = AppDomain.CreateDomain("sandbox";
  //var engine = Python.CreateEngine(sandbox);
  var engine = Python.CreateEngine();
  
  //Setting the engine import paths
  string path = Assembly.GetExecutingAssembly().Location;
  string rootDir = Directory.GetParent(path).FullName;
  string path = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
  engine.SetSearchPaths(new[]{rootDir}.Concat(path.Split(';').Where(s => !s.IsNullOrEmpty())).ToArray());

  var source = engine.CreateScriptSourceFromFile(file);
  var scope = engine.CreateScope();//represents a Python namespace, provides variables binding isolation
  var operations = engine.Operations;
  
  //To redirect the standard output / error to a memorystream
  //var stream = new MemoryStream();
  //engine.RunTime.IO.SetOutput(stream, Encoding.UTF8);
  //engine.Runtime.IO.SetErrorOutput(stream, Encoding.UTF8);
  try
  {
    source.Execute(scope);
    var classObj = scope.GetVariable(className);
    var instance = operations.Call(classObj);
    var m = operations.GetMember(instance, methodName);
    var results = (TResult) operations.Call(m, parameters);
    
    return results;
  }
  catch (Exception e)
  {
    //To get the python traceback instead of an incomprehensible CLR traceback
    ExceptionOperations eo = engine.GetService<ExceptionOperations>();
    throw eo.FormatException(e)
  }
}
