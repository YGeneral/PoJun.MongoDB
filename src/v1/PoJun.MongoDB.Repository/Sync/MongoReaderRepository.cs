using MongoDB.Bson;
using MongoDB.Driver;
using PoJun.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
	/// 读取仓储
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public class MongoReaderRepository<TEntity, TKey> : MongoBaseRepository<TEntity, TKey>, IReaderRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库连接节点</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="collectionName">集合名称</param>
        /// <param name="writeConcern"></param>
        /// <param name="readPreference"></param>
        /// <param name="sequence">Mongo自增长ID数据序列对象</param>
        public MongoReaderRepository(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null) : base(connString, dbName, collectionName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 创建自增长ID
        /// </summary>
        /// <returns></returns>
        public long CreateIncID(long inc = 1L, int iteration = 0)
        {
            IMongoCollection<BsonDocument> collection = base.Database.GetCollection<BsonDocument>(this._sequence.SequenceName, null);
            string collectionName = base.CollectionName;
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq<string>(this._sequence.CollectionName, collectionName);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Inc<long>(this._sequence.IncrementID, inc);
            BsonDocument bsonDocument = collection.FindOneAndUpdate<BsonDocument>(filter, update, new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }, default(CancellationToken));
            if (bsonDocument != null)
            {
                return bsonDocument[this._sequence.IncrementID].AsInt64;
            }
            if (iteration <= 1)
            {
                return this.CreateIncID(inc, ++iteration);
            }
            throw new Exception("Failed to get on the IncID");
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public void CreateIncID(TEntity entity)
        {
            long id = this.CreateIncID(1L, 0);
            base.AssignmentEntityID(entity, id);
        }

        /// <summary>
        /// 根据id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public TEntity Get(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq<TKey>((TEntity x) => x.ID, id);
            ProjectionDefinition<TEntity, TEntity> projection = null;
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields<TEntity>(includeFieldExp);
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sortExp, sortType, 1, 0, hint);
            IAsyncCursor<TEntity> cursor = base.GetCollection(readPreference).FindSync<TEntity>(filter, options, default(CancellationToken));
            return cursor.FirstOrDefault(default(CancellationToken));
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null, ReadPreference readPreference = null)
        {
            ProjectionDefinition<TEntity, TEntity> projection = null;
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields<TEntity>(includeFieldExp);
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sortExp, sortType, 1, 0, hint);
            IAsyncCursor<TEntity> cursor = base.GetCollection(readPreference).FindSync<TEntity>(filter, options, default(CancellationToken));
            return cursor.FirstOrDefault(default(CancellationToken));
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="projection"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public TEntity Get(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TEntity> projection = null, SortDefinition<TEntity> sort = null, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, 1, 0, hint);
            IAsyncCursor<TEntity> cursor = base.GetCollection(readPreference).FindSync<TEntity>(filter, options, default(CancellationToken));
            return cursor.FirstOrDefault(default(CancellationToken));
        }

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filterExp">查询条件表达式</param>
        /// <param name="includeFieldExp">查询字段表达式</param>
        /// <param name="sortExp">排序表达式</param>
        /// <param name="sortType">排序方式</param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public List<TEntity> GetList(Expression<Func<TEntity, bool>> filterExp = null, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            ProjectionDefinition<TEntity, TEntity> projection = null;
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            SortDefinition<TEntity> sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields<TEntity>(includeFieldExp);
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, limit, skip, hint);
            IAsyncCursor<TEntity> source = base.GetCollection(readPreference).FindSync<TEntity>(filter, options, default(CancellationToken));
            return source.ToList(default(CancellationToken));
        }

        /// <summary>
        /// 根据条件获取获取列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="projection"></param>
        /// <param name="sort"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public List<TEntity> GetList(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TEntity> projection = null, SortDefinition<TEntity> sort = null, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, limit, skip, hint);
            IAsyncCursor<TEntity> source = base.GetCollection(readPreference).FindSync<TEntity>(filter, options, default(CancellationToken));
            return source.ToList(default(CancellationToken));
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filterExp"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(Expression<Func<TEntity, TField>> fieldExp, Expression<Func<TEntity, bool>> filterExp, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.Distinct<TField>(fieldExp, filter, null);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(Expression<Func<TEntity, TField>> fieldExp, FilterDefinition<TEntity> filter, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAsyncCursor<TField> source = base.GetCollection(readPreference).Distinct(fieldExp, filter, null, default(CancellationToken));
            return source.ToList(default(CancellationToken));
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TField> Distinct<TField>(FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAsyncCursor<TField> source = base.GetCollection(readPreference).Distinct<TField>(field, filter, null, default(CancellationToken));
            return source.ToList(default(CancellationToken));
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public long Count(FilterDefinition<TEntity> filter, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            CountOptions options = base.CreateCountOptions(limit, skip, hint);
            return base.GetCollection(readPreference).Count(filter, options, default(CancellationToken));
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> filterExp, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.Count(filter, limit, skip, hint, readPreference);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public bool Exists(FilterDefinition<TEntity> filter, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            base.CreateCountOptions(1, 0, hint);
            return this.Get(filter, MongoBaseRepository<TEntity, TKey>.Projection.Include((TEntity x) => (object)x.ID), null, hint, readPreference) != null;
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public bool Exists(Expression<Func<TEntity, bool>> filterExp, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.Exists(filter, hint, readPreference);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filterExp"></param>
        /// <param name="id">$group -&gt; _id</param>
        /// <param name="group">$group</param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.Aggregate<TResult, TID>(filter, id, group, sortExp, sortType, limit, skip, readPreference);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filter"></param>
        /// <param name="id">$group -&gt; _id</param>
        /// <param name="group">$group</param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(FilterDefinition<TEntity> filter, Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAggregateFluent<TEntity> aggregate = base.CreateAggregate(filter, base.CreateSortDefinition<TEntity>(sortExp, sortType), readPreference);
            IAggregateFluent<TResult> aggregateFluent = aggregate.Group(id, group);
            if (skip > 0)
            {
                aggregateFluent = aggregateFluent.Skip(skip);
            }
            if (limit > 0)
            {
                aggregateFluent = aggregateFluent.Limit(limit);
            }
            return aggregateFluent.ToList(default(CancellationToken));
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filterExp"></param>
        /// <param name="group"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(Expression<Func<TEntity, bool>> filterExp, ProjectionDefinition<TEntity, TResult> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            return this.Aggregate<TResult, TID>(filter, group, sortExp, sortType, limit, skip, readPreference);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TID"></typeparam>
        /// <param name="filter"></param>
        /// <param name="group"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public List<TResult> Aggregate<TResult, TID>(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TResult> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAggregateFluent<TEntity> aggregateFluent = base.CreateAggregate(filter, base.CreateSortDefinition<TEntity>(sortExp, sortType), readPreference);
            IAggregateFluent<TResult> aggregateFluent2 = aggregateFluent.Group<TResult>(group);
            if (skip > 0)
            {
                aggregateFluent2 = aggregateFluent2.Skip(skip);
            }
            if (limit > 0)
            {
                aggregateFluent2 = aggregateFluent2.Limit(limit);
            }
            return aggregateFluent2.ToList(default(CancellationToken));
        }
    }
}
