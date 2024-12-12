using System.Collections.Generic;
using System.Collections.Frozen;
using MongoDB.Driver.Core.Events;

namespace UCode.Mongo.OpenTelemetry
{
    /// <summary>
    /// Provides extension methods for the <see cref="CommandStartedEvent"/> class.
    /// </summary>
    /// <remarks>
    /// These extension methods allow for additional functionality and easier integration 
    /// of command started events in various scenarios.
    /// </remarks>
    public static class CommandStartedEventExtensions
    {
        private static readonly FrozenSet<string> CommandsWithCollectionNameAsValue =
            new HashSet<string>
            {
                "aggregate",
                "count",
                "distinct",
                "mapReduce",
                "geoSearch",
                "delete",
                "find",
                "killCursors",
                "findAndModify",
                "insert",
                "update",
                "create",
                "drop",
                "createIndexes",
                "listIndexes"
            }.ToFrozenSet();

        /// <summary>
        /// Retrieves the name of the collection from a given CommandStartedEvent.
        /// This method checks if the command is a "getMore" command or if the command name 
        /// exists in a predefined list of commands that include a collection name as a value.
        /// If so, it extracts and returns the collection name as a string. 
        /// If no collection name is found, it returns an empty string.
        /// </summary>
        /// <param name="@event">
        /// The CommandStartedEvent instance that contains the command information
        /// used to extract the collection name.
        /// </param>
        /// <returns>
        /// A string representing the name of the collection, or an empty string if 
        /// no collection name can be found in the command.
        /// </returns>
        public static string? GetCollectionName(this CommandStartedEvent @event)
        {
            if (@event.CommandName == "getMore")
            {
                if (@event.Command.Contains("collection"))
                {
                    var collectionValue = @event.Command.GetValue("collection");
                    if (collectionValue.IsString)
                    {
                        return collectionValue.AsString;
                    }
                }
            }
            else if (CommandsWithCollectionNameAsValue.Contains(@event.CommandName))
            {
                var commandValue = @event.Command.GetValue(@event.CommandName);
                if (commandValue != null && commandValue.IsString)
                {
                    return commandValue.AsString;
                }
            }

            return null;
        }
    }
}
