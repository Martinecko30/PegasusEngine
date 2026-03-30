namespace PegasusEngine.Debug.Exceptions;

public class WrongSceneException : Exception
{
    public WrongSceneException(string message) : base(message)
    {}
    
    public WrongSceneException() : base() {}
}