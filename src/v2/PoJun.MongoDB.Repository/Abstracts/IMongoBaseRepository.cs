using MongoDB.Driver;

namespace PoJun.MongoDB.Repository
{
    public interface IMongoBaseRepository
    { }

    public interface IMongoBaseRepository<TEntity> : IMongoBaseRepository
    {
        /// <summary>
        /// MongoDatabase
        /// </summary>
        IMongoDatabase Database { get; }

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        IMongoCollection<TEntity> GetCollection(MongoCollectionSettings settings = null);

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        IMongoCollection<TEntity> GetCollection(WriteConcern writeConcern);

        /// <summary>
        /// 根据数据类型得到集合
        /// </summary>
        /// <returns></returns>
        IMongoCollection<TEntity> GetCollection(ReadPreference readPreference);
    }
}
