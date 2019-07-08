using System;
using System.Collections.Generic;
using System.Text;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
    /// 框架异常
    /// </summary>
    public class MongoFrameException : Exception
    {
        public MongoFrameException() : base()
        {
        }

        public MongoFrameException(string message) : base(message)
        {
        }
    }
}
