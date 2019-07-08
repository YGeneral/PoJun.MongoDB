using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.Repository.IEntity
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
