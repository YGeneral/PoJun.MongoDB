using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoJun.MongoDB.UnitTest.Core.Repository;
using PoJun.MongoDB.Repository;
using PoJun.MongoDB.UnitTest.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoJun.MongoDB.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        
        private UserRepositoryAsync userRepAsync => RepositoryContainer.Resolve<UserRepositoryAsync>();

        [TestMethod]
        public async Task Add()
        {
            List<User> userlist = new List<User>();
            User user = null;
            for(var i = 0; i < 50000; i++)
            {
                user = new User();
                user.Age = new Random().Next(100);
                user.Name = i.ToString();
                userlist.Add(user);
                //await userRepAsync.InsertAsync(user);
            }
            await userRepAsync.InsertBatchAsync(userlist);
        }
    }
}
