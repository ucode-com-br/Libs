using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a generic class for sending messages of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the message being sent.</typeparam>
    public class SendMessage<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the session should be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if the session should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseSession
        {
            get; set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether to use partitioning.
        /// </summary>
        /// <value>
        /// <c>true</c> if partitioning is to be used; otherwise, <c>false</c>.
        /// </value>
        public bool UsePartition
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the SendMessage class with the specified parameters.
        /// </summary>
        /// <param name="instance">The instance of type T that will be sent as the body of the message.</param>
        /// <param name="session">A boolean indicating whether the message uses sessions. Defaults to false.</param>
        /// <param name="partitioned">A boolean indicating whether the message is partitioned. Defaults to false.</param>
        /// <returns>
        /// A new instance of SendMessage class.
        /// </returns>
        public SendMessage(T instance, bool session = false, bool partitioned = false)
        {
            this.Body = instance;
            this.UseSession = session;
            this.UsePartition = partitioned;

            this.VerifyMembers(typeof(T), instance);
        }


        /// <summary>
        /// Verifies the members of the specified type and extracts relevant attribute values
        /// into the instance's properties. It checks for known attributes such as MessageId, 
        /// PartitionKey, SessionId, CorrelationId, ScheduledEnqueueTime, and ApplicationProperties.
        /// </summary>
        /// <param name="type">The type whose members are to be verified.</param>
        /// <param name="instance">The instance from which to retrieve member values.</param>
        private void VerifyMembers(Type type, object instance)
        {
            foreach (var memberInfo in type.GetMembers())
            {
                var isProperty = memberInfo.MemberType == MemberTypes.Property;
                var isField = memberInfo.MemberType == MemberTypes.Field;

                if (!isProperty && !isField)
                {
                    continue;
                }

                var attributes = memberInfo.GetCustomAttributes().ToArray();

                if (SendMessage<T>.TypeIdEquals<MessageIdAttribute>(attributes))
                {
                    this.MessageId = SendMessage<T>.GetValue(memberInfo, instance)?.ToString();
                }
                else
                {
                    if (SendMessage<T>.TypeIdEquals<PartitionKeyAttribute>(attributes))
                    {
                        this.PartitionKey = SendMessage<T>.GetValue(memberInfo, instance)?.ToString();
                    }
                    else
                    {
                        if (SendMessage<T>.TypeIdEquals<SessionIdAttribute>(attributes))
                        {
                            this.SessionId = SendMessage<T>.GetValue(memberInfo, instance)?.ToString();
                        }
                        else
                        {
                            if (SendMessage<T>.TypeIdEquals<CorrelationIdAttribute>(attributes))
                            {
                                this.CorrelationId = SendMessage<T>.GetValue(memberInfo, instance)?.ToString();
                            }
                            else
                            {
                                if (SendMessage<T>.TypeIdEquals<ScheduledEnqueueTimeAttribute>(attributes))
                                {
                                    var val = SendMessage<T>.GetValue(memberInfo, instance);

                                    if (val != null)
                                    {
                                        if (val is DateTimeOffset dateTimeOffset)
                                        {
                                            this.ScheduledEnqueueTime = dateTimeOffset;
                                        }
                                        else if (val is DateTime dateTime)
                                        {
                                            this.ScheduledEnqueueTime = new DateTimeOffset(dateTime, TimeSpan.Zero);
                                        }
                                        else if (val is long l)
                                        {
                                            this.ScheduledEnqueueTime = new DateTimeOffset(new DateTime(l), TimeSpan.Zero);
                                        }
                                        else
                                        {
                                            this.ScheduledEnqueueTime = DateTimeOffset.Parse(val.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    if (SendMessage<T>.TypeIdEquals<ApplicationPropertyAttribute>(attributes))
                                    {
                                        var apa = SendMessage<T>.GetValue(memberInfo, instance);
                                        var tup = apa.GetType();

                                        if (tup == typeof(Dictionary<string, object>))
                                        {
                                            this.ApplicationProperties.Add(memberInfo.Name, apa);
                                        }
                                        else
                                        {
                                            if (tup == typeof(Dictionary<string, object?>))
                                            {
                                                this.ApplicationProperties.Add(memberInfo.Name, apa);
                                            }
                                            else
                                            {
                                                if (tup == typeof(Dictionary<string, string>))
                                                {
                                                    this.ApplicationProperties.Add(memberInfo.Name, apa);
                                                }
                                                else
                                                {
                                                    this.ApplicationProperties.Add(memberInfo.Name, apa);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if any of the given attributes has a TypeId that matches
        /// the TypeId of a newly created instance of the specified attribute type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute to check against.</typeparam>
        /// <param name="attributes">An array of attributes to search through.</param>
        /// <returns>True if any attribute in the array has a matching TypeId; otherwise, false.</returns>
        private static bool TypeIdEquals<TAttribute>(Attribute[] attributes) where TAttribute : Attribute => attributes.Any(a => a.TypeId == Activator.CreateInstance<TAttribute>().TypeId);

        /// <summary>
        /// Gets the value of a member (property or field) from a given instance.
        /// </summary>
        /// <param name="memberInfo">The member information of the property or field to retrieve the value from.</param>
        /// <param name="instance">The instance of the object from which to retrieve the value.</param>
        /// <returns>
        /// The value of the specified member from the given instance. Returns null if the member is not found or if 
        /// the instance is not valid.
        /// </returns>
        private static object? GetValue(MemberInfo memberInfo, object instance)
        {
            var isProperty = memberInfo.MemberType == MemberTypes.Property;
            return isProperty ? ((PropertyInfo)memberInfo).GetValue(instance) : ((FieldInfo)memberInfo).GetValue(instance);
        }

        /// <summary>
        /// Gets or sets the application properties.
        /// This property holds a collection of key-value pairs where 
        /// the key is of type string and the value is of type object.
        /// It is initialized to a new instance of the Dictionary class 
        /// when the containing object is created.
        /// </summary>
        /// <returns>
        /// A dictionary containing the application properties as key-value pairs,
        /// where each key is a string and each value is an object.
        /// </returns>
        public Dictionary<string, object> ApplicationProperties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the unique identifier for a message.
        /// </summary>
        /// <remarks>
        /// The <c>MessageId</c> property is a string that contains the unique identifier associated with a message.
        /// This identifier can be used to track or reference the specific message in various contexts.
        /// </remarks>
        public string MessageId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the Partition Key for the entity.
        /// This property is used to define the partitioning scheme for the data
        /// stored in a distributed data system, allowing for optimized queries
        /// and data management.
        /// </summary>
        /// <value>
        /// A string representing the Partition Key. This key is essential for 
        /// partitioning the data effectively across different storage nodes.
        /// </value>
        public string PartitionKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the session.
        /// </summary>
        /// <value>
        /// A string representing the session ID.
        /// </value>
        public string SessionId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the correlation identifier for tracking related requests or operations.
        /// This is useful for logging and tracing related events throughout the system.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the correlation identifier.
        /// </value>
        public string CorrelationId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the duration of time that an item can live before it should be 
        /// considered expired. If a TimeSpan value is provided, it defines the lifespan 
        /// of the item.
        /// </summary>
        /// <value>
        /// A nullable <see cref="TimeSpan"/> representing the time to live for the item. 
        /// If <c>null</c>, it indicates that there is no expiration time set.
        /// </value>
        public TimeSpan? TimeToLive
        {
            get; set;
        }

        /// <summary>
        /// Represents the content type of the data being handled. 
        /// By default, it is set to "application/json".
        /// </summary>
        /// <value>
        /// A string that specifies the media type of the content. 
        /// The default value is "application/json".
        /// </value>
        public string ContentType { get; } = "application/json";

        /// <summary>
        /// Represents the scheduled enqueue time for a message.
        /// This property is nullable and can be used to specify when
        /// a message should be enqueued. If no specific time is set, 
        /// the value will be null.
        /// </summary>
        /// <remarks>
        /// Use this property in scenarios where you need to delay 
        /// the processing of a message until a specified time.
        /// </remarks>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing the date and time 
        /// at which the message is scheduled to be enqueued.
        /// </value>
        /// <example>
        /// <code>
        /// var message = new Message();
        /// message.ScheduledEnqueueTime = DateTimeOffset.Now.AddMinutes(10);
        /// </code>
        /// </example>
        public DateTimeOffset? ScheduledEnqueueTime
        {
            get; set;
        }

        /// <summary>
        /// Represents the body of a response or request, encapsulating the data type specified by T.
        /// </summary>
        /// <typeparam name="T">The type of the body content.</typeparam>
        /// <value>
        /// The body content of type T. The value can be both retrieved and set.
        /// </value>
        public T Body
        {
            get; set;
        }

        public static implicit operator ServiceBusMessage(SendMessage<T> source)
        {
            var binaryData = new BinaryData(source.Body);

            var result = new ServiceBusMessage(binaryData)
            {
                ContentType = source.ContentType,
                PartitionKey = source.PartitionKey,
                SessionId = source.SessionId
            };

            foreach (var applicationProperty in source.ApplicationProperties)
            {
                result.ApplicationProperties.Add(applicationProperty);
            }


            if (!string.IsNullOrWhiteSpace(source.MessageId))
            {
                try
                {
                    result.MessageId = source.MessageId;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("MessageId set fail.", ex);
                }
            }


            if (!string.IsNullOrWhiteSpace(source.CorrelationId))
            {
                try
                {
                    result.CorrelationId = source.CorrelationId;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("CorrelationId set fail.", ex);
                }
            }

            if (source.ScheduledEnqueueTime.HasValue)
            {
                try
                {
                    result.ScheduledEnqueueTime = source.ScheduledEnqueueTime.Value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException("TimeToLive value is out of range.", ex);
                }
            }

            if (source.TimeToLive.HasValue)
            {
                try
                {
                    result.TimeToLive = source.TimeToLive.Value;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("TimeToLive set fail.", ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a string representation of the current object, 
        /// which is the JSON string generated by the <see cref="JsonString"/> method.
        /// </summary>
        /// <returns>
        /// A string that represents the current object in JSON format.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the JsonString method returns null.
        /// </exception>
        public override string ToString() => this.JsonString()!;
    }
}
