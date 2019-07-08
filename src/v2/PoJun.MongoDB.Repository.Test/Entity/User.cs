﻿using MongoDB.Bson.Serialization.Attributes;
using PoJun.MongoDB.Repository.IEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository.Test
{
    [BsonIgnoreExtraElements]
    public class User : IAutoIncr<long>
    {
        [BsonId]
        public long ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int Age
        { get; set; }



        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime
        {
            get;
            set;
        }

        //public string Desc
        //{
        //    get;
        //    set;
        //}
        
        public string Stamp { get; set; }

        public User()
        {
            CreateTime = DateTime.Now;
        }
    }


    [BsonIgnoreExtraElements]
    public class MallCard : IAutoIncr<long>
    {
        /// <summary>
        ///自增主键
        /// </summary>
        [BsonId]
        public long ID { get; set; }

        /// <summary>
        ///// 商场id
        ///// </summary>
        public long MallID { get; set; }

        ///// <summary>
        ///// 用户UID[为0表示未绑定猫酷用户]
        ///// </summary>
        public long UID { get; set; }

        ///// <summary>
        ///// 会员卡类型ID
        ///// </summary>
        public long? CardTypeID { get; set; }
    }

}
