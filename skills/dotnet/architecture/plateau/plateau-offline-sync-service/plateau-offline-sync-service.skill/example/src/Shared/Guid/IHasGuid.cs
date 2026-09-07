namespace Shared.Guid;

// A create command for an external-created entity carries the client-generated Guid.
public interface IHasGuid
{
    System.Guid Guid { get; }
}
