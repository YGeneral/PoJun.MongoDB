using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoJun.MongoDB.Repository
{
    /// <summary>
	/// 容器
	/// </summary>
	public static class RepositoryContainer
    {
        private static ConcurrentDictionary<string, Lazy<object>> Repositorys
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        static RepositoryContainer()
        {
            RepositoryContainer.Repositorys = new ConcurrentDictionary<string, Lazy<object>>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void Register<T>(T service)
        {
            Type typeFromHandle = typeof(T);
            Lazy<object> lazy = new Lazy<object>(() => service);
            RepositoryContainer.Repositorys.AddOrUpdate(RepositoryContainer.GetKey(typeFromHandle), lazy, (string x, Lazy<object> y) => lazy);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>() where T : new()
        {
            Type typeFromHandle = typeof(T);
            Lazy<object> lazy = new Lazy<object>(() => (default(T) == null) ? Activator.CreateInstance<T>() : default(T));
            RepositoryContainer.Repositorys.AddOrUpdate(RepositoryContainer.GetKey(typeFromHandle), lazy, (string x, Lazy<object> y) => lazy);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        public static void Register<T>(Func<object> function)
        {
            Type typeFromHandle = typeof(T);
            Lazy<object> lazy = new Lazy<object>(function);
            RepositoryContainer.Repositorys.AddOrUpdate(RepositoryContainer.GetKey(typeFromHandle), lazy, (string x, Lazy<object> y) => lazy);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>() where T : new()
        {
            Type typeFromHandle = typeof(T);
            string key = RepositoryContainer.GetKey(typeFromHandle);
            Lazy<object> orAdd = RepositoryContainer.Repositorys.GetOrAdd(key, (string x) => new Lazy<object>(() => (default(T) == null) ? Activator.CreateInstance<T>() : default(T)));
            return (T)((object)orAdd.Value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string GetKey(Type t)
        {
            return t.FullName;
        }
    }
}
