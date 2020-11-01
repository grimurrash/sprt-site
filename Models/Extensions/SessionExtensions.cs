using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace NewSprt.Models.Extensions
{
    /// <summary>
    /// Расширение для sessions для хранения в сессии объектов
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Запись в сессию сериализованного объекта
        /// </summary>
        /// <param name="session">Ссылка на сессии</param>
        /// <param name="key">Ключ в сессии</param>
        /// <param name="value">Объект типа Т</param>
        /// <typeparam name="T"></typeparam>
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
 
        /// <summary>
        /// Получение из сессии объекта
        /// </summary>
        /// <param name="session">Ссылка на сессии</param>
        /// <param name="key">Ключ в сессии</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Объект типа Т</returns>
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}