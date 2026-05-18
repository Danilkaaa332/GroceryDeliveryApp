using GroceryDeliveryApp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GroceryDeliveryApp.Tests
{
    [TestClass]
    public class DeliveryTests
    {
        [TestMethod]
        public void BasketPriceTest()
        {
            DeliveryOrder order = new DeliveryOrder
            {
                Products = new List<GroceryProduct>
                {
                    new GroceryProduct
                    {
                        Price = 100,
                        Quantity = 2
                    }
                }
            };

            decimal result = order.CalculateBasketPrice();

            Assert.AreEqual(200, result);
        }

        [TestMethod]
        public void DeliveryPriceTest()
        {
            DeliveryOrder order = new DeliveryOrder();

            decimal result = order.CalculateDeliveryPrice(500);

            Assert.AreEqual(250, result);
        }

        [TestMethod]
        public void ExpirationTest()
        {
            GroceryProduct product = new GroceryProduct
            {
                ExprirationDate = DateTime.Now.AddDays(-1)
            };

            Assert.IsTrue(product.IsExpired());
        }

        [TestMethod]
        public void TotalOrderTest()
        {
            DeliveryOrder order = new DeliveryOrder
            {
                Products = new List<GroceryProduct>
                {
                    new GroceryProduct
                    {
                        Price = 100,
                        Quantity = 3
                    }
                },
                DeliveryPrice = 250
            };

            decimal result = order.GetTotalPrice();

            Assert.AreEqual(550, result);
        }
    }
}
