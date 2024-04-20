using System;

namespace SOM
{
    public class SOMException : Exception
    {
        public SOMException() : base() { }
        public SOMException(string message) : base(message) { }
        public SOMException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class DatabaseNotExistException : SOMException
    {
        public DatabaseNotExistException() : base("The database does not exist") { }
    }
    public class XmlDocumentDoesNotExistException : SOMException
    {
        public XmlDocumentDoesNotExistException() : base("The document does not exist") { }
    }
    public class NoModulesFoundException : SOMException
    {
        public NoModulesFoundException() : base(string.Format("No modules found in the database")) { }
    }
    public class ModuleDoesNotExistException : SOMException
    {
        public ModuleDoesNotExistException(string path) : base(string.Format("The module at path \'{0}\' does not exist", path)) { }
    }
    public class ModuleAlreadyExistsException : SOMException
    {
        public ModuleAlreadyExistsException(string path) : base(string.Format("The module at path \'{0}\' already exists", path)) { }
    }
    public class ModulePathShortException : SOMException
    {
        public ModulePathShortException(string path) : base(string.Format("The module path \'{0}\' does not contain enough elements for constant extraction.", path)) { }
    }
    public class ModulePathLongException : SOMException
    {
        public ModulePathLongException(string path) : base(string.Format("The module path \'{0}\' contains too many elements for constant extraction.", path)) { }
    }
    public class ConstantDoesNotExistException : SOMException
    {
        public ConstantDoesNotExistException(string path, string constant) : base(string.Format("The module at path \'{0}\' does not have the constant \'{1}\'", path, constant)) { }
    }
    public class ConstantDoesNotAStringException : SOMException
    {
        public ConstantDoesNotAStringException(string path, string constant) : base(string.Format("The constant \'{1}\' at path \'{0}\' is not a string", path, constant)) { }
    }
    public class ConstantAlreadyExistsException : SOMException
    {
        public ConstantAlreadyExistsException(string path, string constant) : base(string.Format("The module at path \'{0}\' already has the constant \'{1}\'", path, constant)) { }
    }
    public class InvalidModuleNameException : SOMException
    {
        public InvalidModuleNameException(string name) : base(string.Format("\'{0}\' is not a valid module name", name)) { }
    }
    public class InvalidConstantNameException : SOMException
    {
        public InvalidConstantNameException(string constant) : base(string.Format("\'{0}\' is not a valid constant name", constant)) { }
    }
}