namespace UCode.Mongo
{
    /// <summary>
    /// Represents a unique identifier for an object of type string.
    /// This interface extends a generic interface IObjectId with a type parameter of string.
    /// </summary>
    /// <remarks>
    /// The IObjectId interface is typically used in scenarios where objects need to be uniquely identified,
    /// such as in data storage or retrieval. By extending IObjectId<string>, it provides a strongly-typed
    /// contract ensuring that the identifier is consistently of the string type.
    /// </remarks>
    public interface IObjectId : IObjectId<string>
    {
    }
}
