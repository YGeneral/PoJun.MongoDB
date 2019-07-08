using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PoJun.MongoDB.Repository;
using PoJun.Test.API.Reposiory;
using PoJun.Test.Entity;

namespace PoJun.Test.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Index")]
    public class IndexController : Controller
    {
        [Route("SingleInsert")]
        [HttpGet, HttpPost]
        public async Task<APILog> SingleInsert()
        {
            var logRep = RepositoryContainer.Resolve<APILogRepository>();
            var entity = CreateEntity();
            await logRep.InsertAsync(entity);
            return entity;
        }

        [Route("BatchInsert")]
        [HttpGet,HttpPost]
        public async Task<List<APILog>> BatchInsert()
        {
            var logRep = RepositoryContainer.Resolve<APILogRepository>();
            var logs = new List<APILog>();
            logs.Add(CreateEntity());
            logs.Add(CreateEntity());
            logs.Add(CreateEntity());
            await logRep.InsertBatchAsync(logs);
            return logs;
        }

        private APILog CreateEntity()
        {
            return new APILog()
            {
                APIName = Guid.NewGuid().ToString("N"),
                CreateTime = DateTime.Now
            };
        }
    }
}