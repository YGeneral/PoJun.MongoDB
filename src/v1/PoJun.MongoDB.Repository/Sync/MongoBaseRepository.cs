using MongoDB.Bson;
using MongoDB.Driver;
using PoJun.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
	/// 仓储Base
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public abstract class MongoBaseRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// Mongo自增长ID数据序列
        /// </summary>
        protected MongoSequence _sequence;

        /// <summary>
        ///
        /// </summary>
        protected MongoSession _mongoSession;

        /// <summary>
        /// MongoDatabase
        /// </summary>
        public IMongoDatabase Database
        {
            get
            {
                return this._mongoSession.Database;
            }
        }

        /// <summary>
        /// 集合名称
        /// </summary>
        protected string CollectionName
        {
            get;
            private set;
        }

        /// <summary>
        /// get Filter
        /// </summary>
        public static FilterDefinitionBuilder<TEntity> Filter
        {
            get
            {
                return Builders<TEntity>.Filter;
            }
        }

        /// <summary>
        /// get Sort
        /// </summary>
        public static SortDefinitionBuilder<TEntity> Sort
        {
            get
            {
                return Builders<TEntity>.Sort;
            }
        }

        /// <summary>
        /// get Update
        /// </summary>
        public static UpdateDefinitionBuilder<TEntity> Update
        {
            get
            {
                return Builders<TEntity>.Update;
            }
        }

        /// <summary>
        /// get Projection
        /// </summary>
        public static ProjectionDefinitionBuilder<TEntity> Projection
        {
            get
            {
                return Builders<TEntity>.Projection;
            }
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(MongoCollectionSettings settings = null)
        {
            return this.Database.GetCollection<TEntity>(this.CollectionName, settings);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(WriteConcern writeConcern)
        {
            MongoCollectionSettings mongoCollectionSettings = null;
            if (writeConcern != null)
            {
                mongoCollectionSettings = new MongoCollectionSettings();
                mongoCollectionSettings.WriteConcern = writeConcern;
            }
            return this.Database.GetCollection<TEntity>(this.CollectionName, mongoCollectionSettings);
        }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<TEntity> GetCollection(ReadPreference readPreference)
        {
            MongoCollectionSettings mongoCollectionSettings = null;
            if (readPreference != null)
            {
                mongoCollectionSettings = new MongoCollectionSettings();
                mongoCollectionSettings.ReadPreference = readPreference;
            }
            return this.Database.GetCollection<TEntity>(this.CollectionName, mongoCollectionSettings);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="collectionName">集合名称</param>
        /// <param name="writeConcern"></param>
        /// <param name="readPreference"></param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        public MongoBaseRepository(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
        {
            this._sequence = (sequence ?? new MongoSequence());
            this._mongoSession = new MongoSession(connString, dbName, writeConcern, false, readPreference);
            this.CollectionName = (collectionName ?? typeof(TEntity).Name);
        }

        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fieldsExp"></param>
        /// <returns></returns>
        public ProjectionDefinition<T> IncludeFields<T>(Expression<Func<T, object>> fieldsExp) where T : class, new()
        {
            ProjectionDefinitionBuilder<T> projection = Builders<T>.Projection;
            if (fieldsExp == null)
            {
                return null;
            }
            List<ProjectionDefinition<T>> list = new List<ProjectionDefinition<T>>();
            NewExpression newExpression = fieldsExp.Body as NewExpression;
            if (newExpression == null || newExpression.Members == null)
            {
                throw new Exception("fieldsExp is invalid expression format， eg: x => new { x.Field1, x.Field2 }");
            }
            foreach (MemberInfo current in newExpression.Members)
            {
                list.Add(projection.Include(current.Name));
            }
            return projection.Combine(list);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public SortDefinition<T> CreateSortDefinition<T>(Expression<Func<T, object>> sortExp, SortType sortType = SortType.Ascending)
        {
            SortDefinition<T> result = null;
            if (sortExp != null)
            {
                if (sortType == SortType.Ascending)
                {
                    result = Builders<T>.Sort.Ascending(sortExp);
                }
                else
                {
                    result = Builders<T>.Sort.Descending(sortExp);
                }
            }
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null, SortDefinition<T> sort = null, int limit = 0, int skip = 0, BsonValue hint = null)
        {
            FindOptions<T, T> findOptions = new FindOptions<T, T>();
            if (limit > 0)
            {
                findOptions.Limit = new int?(limit);
            }
            if (skip > 0)
            {
                findOptions.Skip = new int?(skip);
            }
            if (projection != null)
            {
                findOptions.Projection = projection;
            }
            if (sort != null)
            {
                findOptions.Sort = sort;
            }
            if (hint != null)
            {
                FindOptionsBase arg_5C_0 = findOptions;
                BsonDocument bsonDocument = new BsonDocument();
                bsonDocument.Add("$hint", hint);
                arg_5C_0.Modifiers = bsonDocument;
            }
            return findOptions;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projection"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public FindOptions<T, T> CreateFindOptions<T>(ProjectionDefinition<T, T> projection = null, Expression<Func<T, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, BsonValue hint = null)
        {
            FindOptions<T, T> findOptions = new FindOptions<T, T>();
            if (limit > 0)
            {
                findOptions.Limit = new int?(limit);
            }
            if (skip > 0)
            {
                findOptions.Skip = new int?(skip);
            }
            if (projection != null)
            {
                findOptions.Projection = projection;
            }
            SortDefinition<T> sortDefinition = this.CreateSortDefinition<T>(sortExp, sortType);
            if (sortDefinition != null)
            {
                findOptions.Sort = sortDefinition;
            }
            if (hint != null)
            {
                FindOptionsBase arg_67_0 = findOptions;
                BsonDocument bsonDocument = new BsonDocument();
                bsonDocument.Add("$hint", hint);
                arg_67_0.Modifiers = bsonDocument;
            }
            return findOptions;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint"></param>
        /// <returns></returns>
        public CountOptions CreateCountOptions(int limit = 0, int skip = 0, BsonValue hint = null)
        {
            CountOptions countOptions = new CountOptions();
            if (limit > 0)
            {
                countOptions.Limit = new long?((long)limit);
            }
            if (skip > 0)
            {
                countOptions.Skip = new long?((long)skip);
            }
            if (hint != null)
            {
                countOptions.Hint = hint;
            }
            return countOptions;
        }

        /// <summary>
        /// ID赋值
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        protected void AssignmentEntityID(TEntity entity, long id)
        {
            IEntity<TKey> entity2 = entity;
            if (entity2.ID is int)
            {
                (entity as IEntity<int>).ID = (int)id;
                return;
            }
            if (entity2.ID is long)
            {
                (entity as IEntity<long>).ID = id;
                return;
            }
            if (entity2.ID is short)
            {
                (entity as IEntity<short>).ID = (short)id;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        protected IAggregateFluent<TEntity> CreateAggregate(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort, ReadPreference readPreference = null)
        {
            IAggregateFluent<TEntity> aggregateFluent = this.GetCollection(readPreference).Aggregate(null);
            aggregateFluent = aggregateFluent.Match(filter);
            if (sort != null)
            {
                aggregateFluent = aggregateFluent.Sort(sort);
            }
            return aggregateFluent;
        }
    }
}
