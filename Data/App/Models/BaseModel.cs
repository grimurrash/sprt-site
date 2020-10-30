using System;

namespace NewSprt.Data.App.Models
{
    /// <summary>
    /// Базовая модель для добавления и реализации времени создания и изменения
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// Время добавления в таблицу (базу)
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Время изменения строки в таблицце
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}