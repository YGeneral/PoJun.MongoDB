using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using PoJun.MongoDB.Repository.UnitTest.Entity;
using PoJun.MongoDB.Repository.UnitTest.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository.UnitTest
{
    /// <summary>
    /// 新增比较单元测试
    /// </summary>
    [TestClass]
    public class InsertCompared_UnitTest
    {
        public InsertCompared_UnitTest()
        {
            string assemblyName = GetType().Assembly.GetName().ToString();
            RepositoryContainer.RegisterAll(assemblyName);
        }
       private static  APILogRepository logRep = RepositoryContainer.Resolve<APILogRepository>();

        [TestMethod]
        public async Task BatchInsertToContainer()
        {
            var logs = new List<APILog>();
            logs.Add(CreateEntity());
            logs.Add(CreateEntity());
            logs.Add(CreateEntity());
            await logRep.InsertBatchAsync(logs);
        }

        [TestMethod]
        public async Task SingleInsertToContainer()
        {
            var aa = new APILog()
            {
                APIName = "1",
                CreateTime = DateTime.Now
            };
            await logRep.InsertAsync(aa);
        }

       

        public APILog CreateEntity()
        {
            return new APILog()
            {
                APIName = Guid.NewGuid().ToString("N"),
                CreateTime = DateTime.Now
            };
        }

    }
}
