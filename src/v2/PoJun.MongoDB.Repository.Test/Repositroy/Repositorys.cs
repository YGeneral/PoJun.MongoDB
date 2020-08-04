using MongoDB.Driver;
using PoJun.MongoDB.Repository.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository.Test
{
    public class Repositorys
    {
        public static string dbName = "RepositoryTest";
        public static string connString = "mongodb://127.0.0.1:27017/";
    }

    //public class UserRep : MongoRepository<User, long>
    //{

    //    public UserRep()
    //        : base(Repositorys.connString, Repositorys.dbName)
    //    {

    //    }
    //}

    public class UserRepAsync : MongoRepositoryAsync<User, long>
    {
        //public UserRepAsync()
        //    : base(Repositorys.connString, Repositorys.dbName, null, null, new MongoSequence { IncrementID = "IncrementID", CollectionName = "CollectionName", SequenceName = "Sequence" })
        //{

        //}

        public UserRepAsync()
            : base(Repositorys.connString, Repositorys.dbName, null, null)
        {

        }

    }

    public class APILogRepository : MongoRepositoryAsync<APILog, string>
    {
        public APILogRepository() :
            base(Repositorys.connString, Repositorys.dbName, null, null)
        {

        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <returns></returns>
        public Task CreateIndex()
        {
            var builder = Builders<APILog>.IndexKeys;
            var indexModel = new CreateIndexModel<APILog>(builder.Descending(x => x.CreateTime));
            return this.Database.GetCollection<APILog>(nameof(APILog)).Indexes.CreateOneAsync(indexModel);
        }
    }

    //public class MallCardRep : MongoRepository<MallCard, long>
    //{
    //    public MallCardRep()
    //        : base(Repositorys.connString, Repositorys.dbName)
    //    {

    //    }
    //}
}
