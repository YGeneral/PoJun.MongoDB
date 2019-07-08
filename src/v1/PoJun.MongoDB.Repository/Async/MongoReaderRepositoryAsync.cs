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
	/// 异步读取仓储
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public class MongoReaderRepositoryAsync<TEntity, TKey> : MongoBaseRepository<TEntity, TKey>, IReaderRepositoryAsync<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
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
        public MongoReaderRepositoryAsync(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null) : base(connString, dbName, collectionName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 创建自增长ID
        /// </summary>
        /// <returns></returns>
        public async Task<long> CreateIncIDAsync(long inc = 1L, int iteration = 0)
        {
            long num = 1L;
            IMongoCollection<BsonDocument> collection = base.Database.GetCollection<BsonDocument>(this._sequence.SequenceName, null);
            string name = typeof(TEntity).Name;
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq<string>(this._sequence.CollectionName, name);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Inc<long>(this._sequence.IncrementID, inc);
            BsonDocument bsonDocument = await collection.FindOneAndUpdateAsync<BsonDocument>(filter, update, new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            }, default(CancellationToken)).ConfigureAwait(false);
            long result;
            if (bsonDocument != null)
            {
                num = bsonDocument[this._sequence.IncrementID].AsInt64;
                result = num;
            }
            else
            {
                if (iteration > 1)
                {
                    throw new Exception("Failed to get on the IncID");
                }
                result = await this.CreateIncIDAsync(inc, ++iteration);
            }
            return result;
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public Task CreateIncIDAsync(TEntity entity)
        {
            return this.CreateIncIDAsync(1L, 0).ContinueWith(delegate (Task<long> x)
            {
                long result = x.Result;
                this.AssignmentEntityID(entity, result);
            });
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
        public async Task<TEntity> GetAsync(TKey id, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq<TKey>((TEntity x) => x.ID, id);
            ProjectionDefinition<TEntity, TEntity> projection = null;
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields<TEntity>(includeFieldExp);
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sortExp, sortType, 1, 0, hint);
            IAsyncCursor<TEntity> cursor = await base.GetCollection(readPreference).FindAsync<TEntity>(filter, options, default(CancellationToken)).ConfigureAwait(false);
            return await cursor.FirstOrDefaultAsync(default(CancellationToken)).ConfigureAwait(false);
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
        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = null;
            ProjectionDefinition<TEntity, TEntity> projection = null;
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
            IAsyncCursor<TEntity> cursor = await base.GetCollection(readPreference).FindAsync<TEntity>(filter, options, default(CancellationToken)).ConfigureAwait(false);
            return await cursor.FirstOrDefaultAsync(default(CancellationToken)).ConfigureAwait(false);
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
        public async Task<TEntity> GetAsync(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TEntity> projection = null, SortDefinition<TEntity> sort = null, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, 1, 0, hint);
            IAsyncCursor<TEntity> cursor = await base.GetCollection(readPreference).FindAsync<TEntity>(filter, options, default(CancellationToken)).ConfigureAwait(false);
            return await cursor.FirstOrDefaultAsync(default(CancellationToken)).ConfigureAwait(false);
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
        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filterExp = null, Expression<Func<TEntity, object>> includeFieldExp = null, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            FilterDefinition<TEntity> filter = null;
            ProjectionDefinition<TEntity, TEntity> projection = null;
            SortDefinition<TEntity> sort = null;
            if (filterExp != null)
            {
                filter = Builders<TEntity>.Filter.Where(filterExp);
            }
            else
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            if (includeFieldExp != null)
            {
                projection = base.IncludeFields<TEntity>(includeFieldExp);
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, limit, skip, hint);
            IAsyncCursor<TEntity> source = await base.GetCollection(readPreference).FindAsync<TEntity>(filter, options, default(CancellationToken)).ConfigureAwait(false);
            return await source.ToListAsync(default(CancellationToken)).ConfigureAwait(false);
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
        public async Task<List<TEntity>> GetListAsync(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TEntity> projection = null, SortDefinition<TEntity> sort = null, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            FindOptions<TEntity, TEntity> options = base.CreateFindOptions<TEntity>(projection, sort, limit, skip, hint);
            IAsyncCursor<TEntity> source = await base.GetCollection(readPreference).FindAsync<TEntity>(filter, options, default(CancellationToken)).ConfigureAwait(false);
            return await source.ToListAsync(default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filterExp"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public Task<List<TField>> DistinctAsync<TField>(Expression<Func<TEntity, TField>> fieldExp, Expression<Func<TEntity, bool>> filterExp, ReadPreference readPreference = null)
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
            return this.DistinctAsync<TField>(fieldExp, filter, null);
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldExp"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public async Task<List<TField>> DistinctAsync<TField>(Expression<Func<TEntity, TField>> fieldExp, FilterDefinition<TEntity> filter, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAsyncCursor<TField> source = await base.GetCollection(readPreference).DistinctAsync(fieldExp, filter, null, default(CancellationToken));
            return await source.ToListAsync(default(CancellationToken));
        }

        /// <summary>
        /// Distinct
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="filter"></param>
        /// <param name="readPreference"></param>
        /// <returns></returns>
        public async Task<List<TField>> DistinctAsync<TField>(FieldDefinition<TEntity, TField> field, FilterDefinition<TEntity> filter, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            IAsyncCursor<TField> source = await base.GetCollection(readPreference).DistinctAsync<TField>(field, filter, null, default(CancellationToken));
            return await source.ToListAsync(default(CancellationToken));
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
        public Task<long> CountAsync(FilterDefinition<TEntity> filter, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            CountOptions options = base.CreateCountOptions(limit, skip, hint);
            return base.GetCollection(readPreference).CountAsync(filter, options, default(CancellationToken));
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
        public Task<long> CountAsync(Expression<Func<TEntity, bool>> filterExp, int limit = 0, int skip = 0, BsonValue hint = null, ReadPreference readPreference = null)
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
            return this.CountAsync(filter, limit, skip, hint, readPreference);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(FilterDefinition<TEntity> filter, BsonValue hint = null, ReadPreference readPreference = null)
        {
            if (filter == null)
            {
                filter = Builders<TEntity>.Filter.Empty;
            }
            base.CreateCountOptions(1, 0, hint);
            return this.GetAsync(filter, MongoBaseRepository<TEntity, TKey>.Projection.Include((TEntity x) => (object)x.ID), null, hint, readPreference).ContinueWith<bool>((Task<TEntity> x) => x.Result != null);
        }

        /// <summary>
        /// 数量
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="hint">hint索引</param>
        /// <param name="readPreference">访问设置</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filterExp, BsonValue hint = null, ReadPreference readPreference = null)
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
            return this.ExistsAsync(filter, hint, readPreference);
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
        public Task<List<TResult>> AggregateAsync<TResult, TID>(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
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
            return this.AggregateAsync<TResult, TID>(filter, id, group, sortExp, sortType, limit, skip, readPreference);
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
        public Task<List<TResult>> AggregateAsync<TResult, TID>(FilterDefinition<TEntity> filter, Expression<Func<TEntity, TID>> id, Expression<Func<IGrouping<TID, TEntity>, TResult>> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
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
            return aggregateFluent.ToListAsync(default(CancellationToken));
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
        public Task<List<TResult>> AggregateAsync<TResult, TID>(Expression<Func<TEntity, bool>> filterExp, ProjectionDefinition<TEntity, TResult> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
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
            return this.AggregateAsync<TResult, TID>(filter, group, sortExp, sortType, limit, skip, readPreference);
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
        public Task<List<TResult>> AggregateAsync<TResult, TID>(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TResult> group, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, int limit = 0, int skip = 0, ReadPreference readPreference = null)
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
            return aggregateFluent2.ToListAsync(default(CancellationToken));
        }
    }
}
