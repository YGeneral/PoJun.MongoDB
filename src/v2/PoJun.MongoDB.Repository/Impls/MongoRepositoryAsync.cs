﻿using MongoDB.Bson;
using MongoDB.Driver;
using PoJun.MongoDB.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
    /// 异步仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class MongoRepositoryAsync<TEntity, TKey> : MongoReaderRepositoryAsync<TEntity, TKey>, IMongoRepositoryAsync<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
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
        public MongoRepositoryAsync(string connString, string dbName, string collectionName = null, WriteConcern writeConcern = null, ReadPreference readPreference = null, MongoSequence sequence = null)
            : base(connString, dbName, collectionName, writeConcern, readPreference, sequence)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public MongoRepositoryAsync(MongoRepositoryOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// 创建自增长ID
        /// </summary>
        /// <returns></returns>
        public async Task<long> CreateIncIDAsync(long inc = 1, int iteration = 0)
        {
            long id = 1;
            var collection = Database.GetCollection<BsonDocument>(base._sequence.SequenceName);
            var typeName = typeof(TEntity).Name;

            var query = Builders<BsonDocument>.Filter.Eq(base._sequence.CollectionName, typeName);
            var update = Builders<BsonDocument>.Update.Inc(base._sequence.IncrementID, inc);
            var options = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;

            var result = await collection.FindOneAndUpdateAsync(query, update, options).ConfigureAwait(false);
            if (result != null)
            {
                id = result[base._sequence.IncrementID].AsInt64;
                return id;
            }
            else if (iteration <= 1)
            {
                return await CreateIncIDAsync(inc, ++iteration);
            }
            else
            {
                throw new MongoFrameException("Failed to get on the IncID");
            }
        }

        /// <summary>
        /// 创建自增ID
        /// </summary>
        /// <param name="entity"></param>
        public Task CreateIncIDAsync(TEntity entity)
        {
            return this.CreateIncIDAsync().ContinueWith(x =>
            {
                long _id = x.Result;
                AssignmentEntityID(entity, _id);
            });
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="entitys">待添加数据集合</param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task InsertBatchAsync(IEnumerable<TEntity> entitys
            , WriteConcern writeConcern = null)
        {
            //需要自增的实体
            if (isAutoIncrType)
            {
                int count = entitys.Count();
                //自增ID值
                long id = await CreateIncIDAsync(count).ConfigureAwait(false);
                id = id - count;

                foreach (var entity in entitys)
                {
                    AssignmentEntityID(entity, ++id);
                }
            }

            //await base.InsertBatchAsync(entitys);
            await base.GetCollection(writeConcern).InsertManyAsync(entitys).ConfigureAwait(false);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="entity">待添加数据</param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public async Task<TEntity> InsertAsync(TEntity entity
            , WriteConcern writeConcern = null)
        {
            if (isAutoIncrType)
            {
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            await base.GetCollection(writeConcern).InsertOneAsync(entity).ConfigureAwait(false);
            return entity;
        }

       
        
        /// <summary>
        /// 根据实体创建UpdateDefinition
        /// </summary>
        /// <param name="updateEntity"></param>
        /// <param name="isUpsert"></param>
        /// <returns></returns>
        protected async Task<UpdateDefinition<TEntity>> CreateUpdateDefinitionAsync(TEntity updateEntity, bool isUpsert = false)
        {
            long id = 0;
            BsonDocument bsDoc = updateEntity.ToBsonDocument();
            bsDoc.Remove(PoJun.MongoDB.Repository.Util.PRIMARY_KEY_NAME);

            UpdateDefinition<TEntity> update = new UpdateDocument("$set", bsDoc);// string.Concat("{$set:", bsDoc.ToJson(), "}");
            if (isUpsert && isAutoIncrType)
            {
                id = await CreateIncIDAsync();
                update = update.SetOnInsert(PoJun.MongoDB.Repository.Util.PRIMARY_KEY_NAME, id);
            }

            return update;
        }

        /// <summary>
        /// 修改单条数据
        /// 如果isUpsert 为 true ，且updateEntity继承IAutoIncr，则ID内部会自增
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var filter = Filter.Eq(x => x.ID, updateEntity.ID);
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="updateEntity">更新实体（不是replace，updateEntity不会减少原实体字段）</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinitionAsync(updateEntity, isUpsert);
            return await base.GetCollection(writeConcern).UpdateOneAsync(filter, update, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filterExp, update, option);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="isUpsert">如果文档不存在，是否插入数据</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            UpdateOptions option = new UpdateOptions();
            option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateOneAsync(filter, update, option);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="update">更新内容</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update
            //, bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            //UpdateOptions option = new UpdateOptions();
            //option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filterExp, update);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filterExp">查询表达式</param>
        /// <param name="updateExp">更新内容表达式</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp
            //  , bool isUpsert = false
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            //UpdateOptions option = new UpdateOptions();
            //option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filterExp, update);
        }

        /// <summary>
        /// 修改多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="update">更新内容</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update
            //, bool isUpsert = false   ///  <param name="isUpsert">如果文档不存在，是否插入数据</param>            
            , WriteConcern writeConcern = null)
        {
            //UpdateOptions option = new UpdateOptions();
            //option.IsUpsert = isUpsert;
            return base.GetCollection(writeConcern).UpdateManyAsync(filter, update);
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
        public Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, UpdateDefinition<TEntity> update, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;

            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync(filterExp, update, option);
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
        public async Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, TEntity updateEntity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinitionAsync(updateEntity, isUpsert);

            return await base.GetCollection(writeConcern).FindOneAndUpdateAsync(filterExp, update, option).ConfigureAwait(false);
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
        public Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync(filter, update, option);
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
        public Task<TEntity> FindOneAndUpdateAsync(Expression<Func<TEntity, bool>> filterExp, Func<UpdateDefinitionBuilder<TEntity>, UpdateDefinition<TEntity>> updateExp, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            var update = updateExp(Update);

            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;
            return base.GetCollection(writeConcern).FindOneAndUpdateAsync(filterExp, update, option);
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
        public async Task<TEntity> FindOneAndUpdateAsync(FilterDefinition<TEntity> filter, TEntity updateEntity, bool isUpsert = false
            , SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndUpdateOptions<TEntity> option = new FindOneAndUpdateOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            UpdateDefinition<TEntity> update = await CreateUpdateDefinitionAsync(updateEntity, isUpsert);

            return await base.GetCollection(writeConcern).FindOneAndUpdateAsync(filter, update, option).ConfigureAwait(false);
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
        public async Task<TEntity> FindOneAndReplaceAsync(Expression<Func<TEntity, bool>> filterExp, TEntity entity, bool isUpsert = false
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && isAutoIncrType)
            {
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await base.GetCollection(writeConcern).FindOneAndReplaceAsync(filterExp, entity, option).ConfigureAwait(false);
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
        public async Task<TEntity> FindOneAndReplaceAsync(FilterDefinition<TEntity> filter, TEntity entity, bool isUpsert = false, SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndReplaceOptions<TEntity> option = new FindOneAndReplaceOptions<TEntity>();
            option.IsUpsert = isUpsert;
            option.Sort = sort;
            option.ReturnDocument = ReturnDocument.After;

            if (isUpsert && isAutoIncrType)
            {
                await CreateIncIDAsync(entity).ConfigureAwait(false);
            }
            return await base.GetCollection(writeConcern).FindOneAndReplaceAsync(filter, entity, option).ConfigureAwait(false);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndDeleteAsync(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null
            , WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = sort;
            return base.GetCollection(writeConcern).FindOneAndDeleteAsync(filter, option);
        }

        /// <summary>
        /// 找到并替换
        /// </summary>
        /// <param name="filterExp"></param>
        /// <param name="sortExp"></param>
        /// <param name="sortType"></param>
        /// <param name="writeConcern">访问设置</param>
        /// <returns></returns>
        public Task<TEntity> FindOneAndDeleteAsync(Expression<Func<TEntity, bool>> filterExp
            , Expression<Func<TEntity, object>> sortExp = null, SortType sortType = SortType.Ascending
            , WriteConcern writeConcern = null)
        {
            FindOneAndDeleteOptions<TEntity> option = new FindOneAndDeleteOptions<TEntity>();
            option.Sort = base.CreateSortDefinition(sortExp, sortType);
            return base.GetCollection(writeConcern).FindOneAndDeleteAsync(filterExp, option);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(TKey id
            , WriteConcern writeConcern = null)
        {
            var filter = Builders<TEntity>.Filter.Eq(x => x.ID, id);
            return base.GetCollection(writeConcern).DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除单条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<TEntity> filter
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOneAsync(filter);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteOneAsync(Expression<Func<TEntity, bool>> filterExp
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteOneAsync(filterExp);
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteManyAsync(filter);
        }

        /// <summary>
        /// 修改单条数据
        /// </summary>
        /// <param name="filterExp">查询条件</param>
        /// <param name="writeConcern">访问设置</param>
        public Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> filterExp
            , WriteConcern writeConcern = null)
        {
            return base.GetCollection(writeConcern).DeleteManyAsync(filterExp);
        }

    }
}
