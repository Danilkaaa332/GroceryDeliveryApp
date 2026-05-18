using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroceryDeliveryApp.Core
{
    public class DeliveryOrder
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Пользователь
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Список продуктов
        /// </summary>
        public List<GroceryProduct> Products { get; set; }
        /// <summary>
        /// Стоимость доставки
        /// </summary>
        public decimal DeliveryPrice { get; set; }
        /// <summary>
        /// Расчет стоимости корзины
        /// </summary>
        /// <returns></returns>
        public decimal CalculateBasketPrice()
        {
            return Products.Sum(x => x.Price * x.Quantity);
        }
        /// <summary>
        /// Формирование итогового заказа
        /// </summary>
        /// <returns></returns>
        public decimal GetTotalPrice()
        {
            return CalculateBasketPrice() + DeliveryPrice;
        }
        /// <summary>
        /// Расчет стоимости доставки
        /// </summary>
        /// <param name="basketPrice"></param>
        /// <returns></returns>
        public decimal CalculateDeliveryPrice(decimal basketPrice)
        {
            if (basketPrice > 1000) return 0;
            else return 250;
        }
    }
}
