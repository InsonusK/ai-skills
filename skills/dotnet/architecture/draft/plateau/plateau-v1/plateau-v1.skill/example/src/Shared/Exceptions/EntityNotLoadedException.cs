namespace Shared.Exceptions;

public sealed class EntityNotLoadedException : Exception
{
    public EntityNotLoadedException(string entityName, string navigationName)
        : base($"{entityName} required navigation '{navigationName}' was not loaded before this operation.")
    {
    }
}
