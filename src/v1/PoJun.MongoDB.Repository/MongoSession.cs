using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
	/// MongoSessionAsync
	/// </summary>
	public class MongoSession
    {
        /// <summary>
        /// MongoDB WriteConcern
        /// </summary>
        private WriteConcern _writeConcern;

        /// <summary>
        /// MongoClient
        /// </summary>
        private MongoClient _mongoClient;

        /// <summary>
        /// MongoDatabase
        /// </summary>
        public IMongoDatabase Database
        {
            get;
            set;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">数据库链接字符串</param>
        /// <param name="dbName">数据库名称</param>
        /// <param name="writeConcern">WriteConcern选项</param>
        /// <param name="isSlaveOK"></param>
        /// <param name="readPreference"></param>
        public MongoSession(string connString, string dbName, WriteConcern writeConcern = null, bool isSlaveOK = false, ReadPreference readPreference = null)
        {
            this._writeConcern = (writeConcern ?? WriteConcern.Unacknowledged);
            MongoDatabaseSettings mongoDatabaseSettings = new MongoDatabaseSettings();
            mongoDatabaseSettings.WriteConcern = this._writeConcern;
            mongoDatabaseSettings.ReadPreference = (readPreference ?? ReadPreference.SecondaryPreferred);
            this._mongoClient = new MongoClient(connString);
            this.Database = this._mongoClient.GetDatabase(dbName, mongoDatabaseSettings);
        }
    }
}
