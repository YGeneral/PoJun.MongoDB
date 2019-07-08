using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization.Attributes;
using PoJun.MongoDB.Repository.Test.Repositroy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task Insert()
        {
            UserRepAsync userRep = new UserRepAsync();

            var a = await userRep.InsertAsync(new User() { Name = "ggg" });
            var b = await userRep.InsertAsync(new User() { Name = "BBB" });
            var c = await userRep.InsertAsync(new User() { Name = "CCC" });

            APILogRepository apilog = new APILogRepository();

            var d = await apilog.InsertAsync(new Entity.APILog() { APIName = "sdfsafd", CreateTime=DateTime.Now });

            var list = await userRep.GetListAsync(x => x.Name == "ggg");
            UserRepAsync up = new UserRepAsync();
            list = await up.GetListAsync(x => x.Name == "ggg");
            Assert.AreNotEqual(list.Count, 0);
        }
    }

}
