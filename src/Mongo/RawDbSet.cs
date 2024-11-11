//using System;
//using System.Diagnostics.CodeAnalysis;
//using MongoDB.Bson;
//using MongoDB.Driver;

//namespace UCode.Mongo
//{
//    //https://mongodb.github.io/mongo-csharp-driver/2.11/reference/driver/crud/writing/



//    public class RawDbSet<TDocument, TObjectId, TUser> : DbSet<TDocument, TObjectId>
//        where TDocument : IObjectBase<TObjectId>, IObjectBaseTenant
//        where TObjectId : IComparable<TObjectId>, IEquatable<TObjectId>
//    {

//        public RawDbSet([NotNull] ContextBase contextBase,
//            string? collectionName = null,
//            Action<CreateCollectionOptions>? createCollectionOptionsAction = null,
//            Action<MongoCollectionSettings>? mongoCollectionSettingsAction = null,
//            bool useTransaction = false) : base(contextBase, collectionName, createCollectionOptionsAction, mongoCollectionSettingsAction, useTransaction)
//        {

//        }

//        public override string? ToString() => base.ToString();
//    }
//}
