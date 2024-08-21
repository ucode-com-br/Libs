using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using UCode.Extensions;

namespace UCode.ServiceBus
{
    /// <summary>
    /// Represents a message to be sent to an Azure Service Bus.
    /// </summary>
    /// <typeparam name="T">The type of the message body.</typeparam>
    public class SendMessage<T>
    {
        public bool UseSession
        {
            get; set;
        }
        public bool UsePartition
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendMessage{T}"/> class.
        /// </summary>
        /// <param name="instance">The message body.</param>
        /// <param name="session">Indicates whether the message should be sent using a session.</param>
        /// <param name="partitioned">Indicates whether the message should be sent to a partitioned queue.</param>
        public SendMessage(T instance, bool session = false, bool partitioned = false)
        {
            this.Body = instance;
            this.UseSession = session;
            this.UsePartition = partitioned;

            this.VerifyMembers(typeof(T), instance);
        }


        /// <summary>
        /// Verifies the members of the message and sets the corresponding properties.
        /// </summary>
        /// <param name="type">The type of the message body.</param>
        /// <param name="instance">The message body instance.</param>
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
        /// Checks if any of the attributes match the specified type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="attributes">The attributes to check.</param>
        /// <returns>True if any of the attributes match the specified type, false otherwise.</returns>
        private static bool TypeIdEquals<TAttribute>(Attribute[] attributes) where TAttribute : Attribute => attributes.Any(a => a.TypeId == Activator.CreateInstance<TAttribute>().TypeId);

        /// <summary>
        /// Gets the value of a property or field.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="instance">The instance containing the property or field.</param>
        /// <returns>The value of the property or field, or null if it does not exist.</returns>
        private static object? GetValue(MemberInfo memberInfo, object instance)
        {
            var isProperty = memberInfo.MemberType == MemberTypes.Property;
            return isProperty ? ((PropertyInfo)memberInfo).GetValue(instance) : ((FieldInfo)memberInfo).GetValue(instance);
        }

        /// <summary>
        /// Gets or sets the application properties for the message.
        /// </summary>
        public Dictionary<string, object> ApplicationProperties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the message ID.
        /// </summary>
        public string MessageId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the partition key for the message.
        /// </summary>
        public string PartitionKey
        {
            get; set;
        }

        /// <summary>
        /// Usado para agrupar multiplas mensagens
        /// </summary>
        public string SessionId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the correlation ID for the message.
        /// </summary>
        public string CorrelationId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the time-to-live (TTL) for the message.
        /// </summary>
        public TimeSpan? TimeToLive
        {
            get; set;
        }

        /// <summary>
        /// Gets the content type of the message.
        /// </summary>
        public string ContentType { get; } = "application/json";

        /// <summary>
        /// Gets or sets the scheduled enqueue time for the message.
        /// </summary>
        public DateTimeOffset? ScheduledEnqueueTime
        {
            get; set;
        }

        public T Body
        {
            get; set;
        }

        /// <summary>
        /// Converts the message to a ServiceBusMessage.
        /// </summary>
        /// <param name="source">The source message.</param>
        /// <returns>The converted ServiceBusMessage.</returns>
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
        /// Returns a string representation of the message.
        /// </summary>
        /// <returns>The JSON string representation of the message body.</returns>
        public override string ToString() => this.JsonString()!;
    }
}
