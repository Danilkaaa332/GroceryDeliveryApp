using System;

namespace GroceryDeliveryApp.Core
{
    /// <summary>
    /// Продукт магазина
    /// </summary>
    public class GroceryProduct
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название продукта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Категория
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Срок годности
        /// </summary>
        public DateTime ExprirationDate { get; set; }

        /// <summary>
        /// Проверка срока годности
        /// </summary>
        public bool IsExpired()
        {
            return ExprirationDate < DateTime.Now;
        }
    }
}
