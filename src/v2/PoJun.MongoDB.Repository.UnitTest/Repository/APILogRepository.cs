using PoJun.MongoDB.Repository.UnitTest.Entity;
using PoJun.Test.Repository.MongoDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoJun.MongoDB.Repository.UnitTest.Repository
{
    /// <summary>
    /// 接口日志仓储
    /// </summary>
    public class APILogRepository : LogBaseReposotory<APILog, string>
    //public class APILogRepository : MongoRepositoryAsync<APILog, string>
    {
        ///// <summary>
        ///// 默认构造函数
        ///// </summary>
        //public APILogRepository()
        //    : base("mongodb://127.0.0.1:27017/", "Log_Test")
        //{

        //}
    }
}
