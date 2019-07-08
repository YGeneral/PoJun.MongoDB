using MongoDB.Bson.Serialization.Attributes;
using PoJun.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.UnitTest.Core.Entity
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class User : IAutoIncr<long>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [BsonId]
        public long ID { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }


        
    }
}
