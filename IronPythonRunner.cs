public TResult Execute<TResult>(string file, string className, string methodName)
{
  var engine = Python.CreateEngine();
  var source = engine.CreateScriptSourceFromFile(file);
  var scope = engine.CreateScope();
  var operations = engine.Operations;
  
  source.Execute(scope);
  var classObj = scope.GetVariable(className);
  var instance = operations.Call(classObj);
  var m = operations.GetMember(instance, methodName);
  var results = (TResult) operations.Call(m);
  
  return results;
}
