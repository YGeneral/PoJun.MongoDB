using System;
using System.Collections.Generic;
using System.Text;

namespace PoJun.MongoDB.Repository.IEntity
{
    /// <summary>
    /// 非自增实体
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// 主键
        /// </summary>
        TKey ID { get; set; }
    }
}
