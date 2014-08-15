code-examples
=============

string of python code -> IronPython => Expression tree -> DLR => CIL bytecode
                                                       -> Interpreter Iron Python => Interpreter instrcutions => Stack                                                               based interpreter

The DLR provides :
 ->helpers to produce extended expression trees (with if, try, ...)
 ->generates the byte code from the expression tree
 
 The DLR does not always compile the expression tree to byte code, it runs the interpreter first and if called several times (>32) compiles the tree to the CIL bytecode.
 
DLR Hosting :
-The ScriptRuntime is generally shared amongst all dynamic languages in an application.The runtime handles all of the current assembly references that are presented to the loaded languages.
-The ScriptEngine is language specific. One instance by language.
  Engines are thread-safe, and can execute multiple scripts in parallel, as long as each thread has its own scope.
-The ScriptScope is used to hold all of script's variables. It providex isolation, so that multiple scripts can be loaded and executed at the same time without interfering with each other

https://mail.python.org/pipermail/ironpython-users/2010-June/012979.html
https://code.google.com/p/ironpython/source/browse/#svn%2Ftrunk%2Fironpythoninaction%2Fchapter15
