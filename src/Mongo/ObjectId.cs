using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SharpCompress.Compressors.Xz;
using UCode.Extensions;

namespace UCode.Mongo
{

    public abstract class TenantClassBase : TenantClassBase<string, string>, IObjectBaseTenant, IObjectBase
    {

    }

    public abstract class TenantClassBase<TObjectId> : TenantClassBase<TObjectId, string>, IObjectBaseTenant, IObjectBase<TObjectId>
        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    public abstract class TenantClassBase<TObjectId, TUser> : ObjectIdClassBase, IObjectBaseTenant, IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        public TObjectId Id
        {
            get;
            set;
        }

        public TUser CreatedBy
        {
            get;
            set;
        } = default!;

        
        public DateTime CreatedAt
        {
            get;
            set;
        }

        
        public TUser? UpdatedBy
        {
            get;
            set;
        }

        
        public DateTime? UpdatedAt
        {
            get;
            set;
        }

        public Dictionary<string, object?>? ExtraElements
        {
            get;
            set;
        } = null;

        public bool Disabled
        {
            get;
            set;
        } = false;

        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);

        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;

        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);


    }




    public abstract record TenantRecordBase : TenantRecordBase<string, string>, IObjectBase, IObjectBaseTenant
    {

    }

    public abstract record TenantRecordBase<TObjectId> : TenantRecordBase<TObjectId, string>, IObjectBase<TObjectId>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

    }

    public abstract record TenantRecordBase<TObjectId, TUser> : ObjectIdRecordBase, IObjectBaseTenant, IObjectBase<TObjectId, TUser>
            where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
    {

        public TObjectId Id
        {
            get;
            set;
        }

        public TUser CreatedBy
        {
            get;
            set;
        } = default!;

        public DateTime CreatedAt
        {
            get;
            set;
        }

        
        public TUser? UpdatedBy
        {
            get;
            set;
        }

        
        public DateTime? UpdatedAt
        {
            get;
            set;
        }

        
        public Dictionary<string, object?>? ExtraElements
        {
            get;
            set;
        } = null;


        public bool Disabled
        {
            get;
            set;
        } = false;



        public delegate void IdGeneratorCompletedEventHandler(IObjectBase<TObjectId, TUser> sender, IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs);


        public event IdGeneratorCompletedEventHandler IdGeneratorCompleted;


        public virtual void OnProcessCompleted(IdGeneratorCompletedEventArgs<TObjectId, TUser> eventArgs) => IdGeneratorCompleted?.Invoke(this, eventArgs);

    }





    public abstract record ObjectIdRecordBase: IObjectBaseTenant
    {
        public Guid Ref
        {
            get;
            set;
        } = Guid.NewGuid();

        public Guid Tenant
        {
            get;
            set;
        } = Guid.NewGuid();

    }

    public abstract class ObjectIdClassBase : IObjectBaseTenant
    {

        //public void WriteRef() => this.Ref = UCode.Extensions.GuidHelpers.CreateVersion8(this.Tenant.ToByteArray());

        //[JsonPropertyName("ref")]
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[BsonElement("ref")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonGuidRepresentation(GuidRepresentation.Standard)]
        //[BsonRepresentation(BsonType.String)]
        public Guid Ref
        {
            get;
            set;
        } = Guid.NewGuid();

        //[JsonPropertyName("tenant")]
        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[BsonElement("tenant")]
        //[BsonSerializer(typeof(GuidAsStringSerializer))]
        //[BsonRepresentation(BsonType.String)]
        public Guid Tenant
        {
            get;
            set;
        } = Guid.NewGuid();



    }


}
