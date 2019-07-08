using PoJun.MongoDB.Repository;
using PoJun.MongoDB.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoJun.Test.Repository.MongoDB
{
    public class LogBaseReposotory<TEntity, TKey> : MongoRepositoryAsync<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public LogBaseReposotory()
            : base("mongodb://127.0.0.1:27017/", "Log_Test")
        {

        }
    }
}
