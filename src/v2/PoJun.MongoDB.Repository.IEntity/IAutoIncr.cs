using System;
using System.Collections.Generic;
using System.Text;

namespace PoJun.MongoDB.Repository.IEntity
{
    /// <summary>
    /// 自增实体
    /// </summary>
    public interface IAutoIncr
    {
    }

    /// <summary>
    /// 自增实体
    /// </summary>
    public interface IAutoIncr<T> : IAutoIncr, IEntity<T>
    {
    }
}
