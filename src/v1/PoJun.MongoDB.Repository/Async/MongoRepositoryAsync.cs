using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
	/// 异步仓储
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public class MongoRepositoryAsync<TEntity, TKey> : MongoReaderRepositoryAsync<TEntity, TKey>, IRepositoryAsync<TEntity, TKey>, IReaderRepositoryAsync<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
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
        public MongoRepositoryAsync(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null) : base(connString, dbName, collectionName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entity">待添加数据</param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task InsertAsync(TEntity entity, WriteConcern writeConcern = null)
        {
            if (entity is IAutoIncr)
            {
                await base.CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            await base.GetCollection(writeConcern).InsertOneAsync(entity, null, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task InsertBatchAsync(IEnumerable<TEntity> entitys, WriteConcern writeConcern = null)
        {
            if (entitys.First<TEntity>() is IAutoIncr)
            {
                int num = entitys.Count<TEntity>();
                long num2 = await base.CreateIncIDAsync((long)num, 0).ConfigureAwait(false);
                num2 -= (long)num;
                foreach (TEntity current in entitys)
                {
                    base.AssignmentEntityID(current, num2 += 1L);
                }
            }
            await base.GetCollection(writeConcern).InsertManyAsync(entitys, null, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 根据实体创建UpdateDefinition
        /// </summary>
        /// <param name="updateEntity"></param>
        /// <param name="isUpsert"></param>
        /// <returns></returns>
        public async Task<UpdateDefinition<TEntity>> CreateUpdateDefinitionAsync(TEntity updateEntity, bool isUpsert = false)
        {
            long value = 0L;
            BsonDocument bsonDocument = updateEntity.ToBsonDocument(null, null, default(BsonSerializationArgs));
            bsonDocument.Remove("_id");
            if (isUpsert && updateEntity is IAutoIncr)
            {
                value = await base.CreateIncIDAsync(1L, 0);
            }
            UpdateDefinition<TEntity> updateDefinition = new UpdateDocument("$set", bsonDocument);
            if (isUpsert && updateEntity is IAutoIncr)
            {
                updateDefinition = updateDefinition.SetOnInsert("_id", value);
            }
            return updateDefinition;
        }

        /// <summary>
        /// 修改单条数据
        /// 如果isUpsert 为 true ，且updateEntity继承IAutoIncr，则ID内部会自增
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            MongoBaseRepository<TEntity, TKey>.Filter.Eq<TKey>((TEntity x) => x.ID, updateEntity.ID);
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            UpdateDefinition<TEntity> update = await this.CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, updateOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            UpdateDefinition<TEntity> update = await this.CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).UpdateOneAsync(filter, update, updateOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateDefinition<TEntity> update = updateExp(MongoBaseRepository<TEntity, TKey>.Update);
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filter, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filterExp, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateDefinition<TEntity> update = updateExp(MongoBaseRepository<TEntity, TKey>.Update);
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filterExp, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false, WriteConcern writeConcern = null)
        {
            UpdateOptions updateOptions = new UpdateOptions();
            updateOptions.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filter, update, updateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> findOneAndUpdateOptions = new FindOneAndUpdateOptions<TEntity>();
            findOneAndUpdateOptions.IsUpsert = isUpsert;
            findOneAndUpdateOptions.Sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync<TEntity>(filterExp, update, findOneAndUpdateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> findOneAndUpdateOptions = new FindOneAndUpdateOptions<TEntity>();
            findOneAndUpdateOptions.IsUpsert = isUpsert;
            findOneAndUpdateOptions.Sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            UpdateDefinition<TEntity> update = await this.CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).FindOneAndUpdateAsync<TEntity>(filterExp, update, findOneAndUpdateOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false, SortDefinition<TEntity> sort = null, WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> findOneAndUpdateOptions = new FindOneAndUpdateOptions<TEntity>();
            findOneAndUpdateOptions.IsUpsert = isUpsert;
            findOneAndUpdateOptions.Sort = sort;
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync<TEntity>(filter, update, findOneAndUpdateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="updateExp"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false, SortDefinition<TEntity> sort = null, WriteConcern writeConcern = null)
        {
            UpdateDefinition<TEntity> update = updateExp(MongoBaseRepository<TEntity, TKey>.Update);
            FindOneAndUpdateOptions<TEntity> findOneAndUpdateOptions = new FindOneAndUpdateOptions<TEntity>();
            findOneAndUpdateOptions.IsUpsert = isUpsert;
            findOneAndUpdateOptions.Sort = sort;
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync<TEntity>(filterExp, update, findOneAndUpdateOptions, default(CancellationToken));
        }

        /// <summary>
        /// 找到并更新
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="updateEntity">更新实体</param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false, SortDefinition<TEntity> sort = null, WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> findOneAndUpdateOptions = new FindOneAndUpdateOptions<TEntity>();
            findOneAndUpdateOptions.IsUpsert = isUpsert;
            findOneAndUpdateOptions.Sort = sort;
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            UpdateDefinition<TEntity> update = await this.CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).FindOneAndUpdateAsync<TEntity>(filter, update, findOneAndUpdateOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> findOneAndReplaceOptions = new FindOneAndReplaceOptions<TEntity>();
            findOneAndReplaceOptions.IsUpsert = isUpsert;
            findOneAndReplaceOptions.Sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            findOneAndReplaceOptions.ReturnDocument = ReturnDocument.After;
            if (isUpsert && entity is IAutoIncr)
            {
                await base.CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await base.GetCollection(writeConcern).FindOneAndReplaceAsync<TEntity>(filterExp, entity, findOneAndReplaceOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <param name="isUpsert"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> FindOneAndReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null, WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> findOneAndReplaceOptions = new FindOneAndReplaceOptions<TEntity>();
            findOneAndReplaceOptions.IsUpsert = isUpsert;
            findOneAndReplaceOptions.Sort = sort;
            findOneAndReplaceOptions.ReturnDocument = ReturnDocument.After;
            if (isUpsert && entity is IAutoIncr)
            {
                await base.CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await base.GetCollection(writeConcern).FindOneAndReplaceAsync<TEntity>(filter, entity, findOneAndReplaceOptions, default(CancellationToken)).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null, WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> findOneAndDeleteOptions = new FindOneAndDeleteOptions<TEntity>();
            findOneAndDeleteOptions.Sort = sort;
            return base.GetCollection(writeConcern).FindOneAndDeleteAsync<TEntity>(filter, findOneAndDeleteOptions, default(CancellationToken));
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> filterExp, Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending, WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> findOneAndDeleteOptions = new FindOneAndDeleteOptions<TEntity>();
            findOneAndDeleteOptions.Sort = base.CreateSortDefinition<TEntity>(sortExp, sortType);
            return base.GetCollection(writeConcern).FindOneAndDeleteAsync<TEntity>(filterExp, findOneAndDeleteOptions, default(CancellationToken));
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(TKey id, WriteConcern writeConcern = null)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq<TKey>((TEntity x) => x.ID, id);
            return base.GetCollection(writeConcern).DeleteOneAsync(filter, default(CancellationToken));
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter, WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOneAsync(filter, default(CancellationToken));
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(Expression<Func<TEntity, bool>> filterExp, WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOneAsync(filterExp, default(CancellationToken));
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter, WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteManyAsync(filter, default(CancellationToken));
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> filterExp, WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteManyAsync(filterExp, default(CancellationToken));
        }
    }
}
