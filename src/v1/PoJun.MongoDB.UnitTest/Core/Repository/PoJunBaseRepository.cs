using PoJun.MongoDB.Repository;
using PoJun.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.UnitTest.Core.Repository
{
    /// <summary>
    /// 数据仓储-异步
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class PoJunBaseRepositoryAsync<TEntity, TKey> : MongoRepositoryAsync<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        private static readonly string dbName = "PoJun";

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        private static readonly string dbConnetcion = "mongodb://127.0.0.1:27017/?socketTimeoutMS=30000;maxPoolSize=100;minPoolSize=50";

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PoJunBaseRepositoryAsync() : base(dbConnetcion, dbName)
        {

        }
    }

    /// <summary>
    /// 数据仓储-同步
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class PoJunBaseRepository<TEntity, TKey> : MongoRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        private static readonly string dbName = "PoJun";

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        private static readonly string dbConnetcion = "mongodb://muser:1qaz2wsx@218.244.136.30:27018/?socketTimeoutMS=30000;maxPoolSize=100;minPoolSize=50";

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PoJunBaseRepository() : base(dbConnetcion, dbName)
        {

        }
    }
}
