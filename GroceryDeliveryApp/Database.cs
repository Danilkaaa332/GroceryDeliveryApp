using GroceryDeliveryApp.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GroceryDeliveryApp
{
    public class Database
    {
        string connectionString = @"Server=172.16.1.101,33678;Database=GroceryDeliveryDB;User Id=Zelenin;Password=V7)797";

        /// <summary>
        /// Загрузка продуктов
        /// </summary>
        public List<GroceryProduct> LoadProducts()
        {
            var products = new List<GroceryProduct>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT Products.Id, Products.Name, Products.Price, Products.Quantity, Products.ExprirationDate, ProductCategories.Name AS CategoryName
                                    FROM Products
                                    INNER JOIN ProductCategories
                                    ON Products.CategoryId = ProductCategories.Id
                                    WHERE Products.Quantity > 0";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    products.Add(new GroceryProduct
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = row["Name"].ToString(),
                        Price = Convert.ToDecimal(row["Price"]),
                        Quantity = Convert.ToInt32(row["Quantity"]),
                        ExprirationDate = Convert.ToDateTime(row["ExprirationDate"]),
                        Category = row["CategoryName"].ToString()
                    });
                }
            }
            return products;
        }

        /// <summary>
        /// Загрузка заказов с адресами
        /// </summary>
        public List<string> LoadOrders(int currentUserId)
        {
            var orders = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT o.Id, o.TotalPrice, o.OrderDate, da.AddressText
                                    FROM Orders o
                                    JOIN DeliveryAddresses da ON o.AddressId = da.Id
                                    WHERE o.UserId = @UserId
                                    ORDER BY o.OrderDate DESC";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", currentUserId);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int orderId = reader.GetInt32(0);
                    decimal totalPrice = reader.GetDecimal(1);
                    DateTime orderDate = reader.GetDateTime(2);
                    string address = reader.GetString(3);

                    orders.Add($"Заказ #{orderId} | {orderDate:dd.MM.yyyy HH:mm} | {totalPrice} ₽ | Адрес: {address}");
                }
            }
            return orders;
        }

        /// <summary>
        /// Получение или создание адреса доставки
        /// </summary>
        private int GetOrCreateAddressId(string addressText, SqlConnection connection, SqlTransaction transaction, int currentUserId)
        {
            string checkAddressQuery = "SELECT Id FROM DeliveryAddresses WHERE UserId = @UserId AND AddressText = @AddressText";
            SqlCommand checkCommand = new SqlCommand(checkAddressQuery, connection, transaction);
            checkCommand.Parameters.AddWithValue("@UserId", currentUserId);
            checkCommand.Parameters.AddWithValue("@AddressText", addressText);

            object result = checkCommand.ExecuteScalar();
            if (result != null)
            {
                return Convert.ToInt32(result);
            }

            string insertAddressQuery = @"INSERT INTO DeliveryAddresses (UserId, AddressText) 
                                         VALUES (@UserId, @AddressText);
                                         SELECT SCOPE_IDENTITY();";
            SqlCommand insertCommand = new SqlCommand(insertAddressQuery, connection, transaction);
            insertCommand.Parameters.AddWithValue("@UserId", currentUserId);
            insertCommand.Parameters.AddWithValue("@AddressText", addressText);

            return Convert.ToInt32(insertCommand.ExecuteScalar());
        }

        /// <summary>
        /// Оформление заказа
        /// </summary>
        public (bool Success, int OrderId, decimal FinalPrice, decimal DeliveryPrice, string ErrorMessage) CreateOrder(int currentUserId, string addressText, List<GroceryProduct> cartProducts)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                DeliveryOrder order = new DeliveryOrder
                {
                    Products = cartProducts
                };
                decimal basketTotal = order.CalculateBasketPrice();
                decimal deliveryPrice = order.CalculateDeliveryPrice(basketTotal);
                decimal finalPrice = basketTotal + deliveryPrice;

                int addressId = GetOrCreateAddressId(addressText, connection, transaction, currentUserId);

                string insertOrderQuery = @"INSERT INTO Orders (UserId, AddressId, DeliveryPrice, TotalPrice, OrderDate) 
                                                   VALUES (@UserId, @AddressId, @DeliveryPrice, @TotalPrice, @OrderDate);
                                                   SELECT SCOPE_IDENTITY();";

                SqlCommand orderCommand = new SqlCommand(insertOrderQuery, connection, transaction);
                orderCommand.Parameters.AddWithValue("@UserId", currentUserId);
                orderCommand.Parameters.AddWithValue("@AddressId", addressId);
                orderCommand.Parameters.AddWithValue("@DeliveryPrice", deliveryPrice);
                orderCommand.Parameters.AddWithValue("@TotalPrice", finalPrice);
                orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);

                int orderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                string insertItemQuery = @"INSERT INTO OrderItems (OrderId, ProductId, Quantity) 
                                                  VALUES (@OrderId, @ProductId, @Quantity)";

                string updateProductQuery = @"UPDATE Products SET Quantity = Quantity - @Quantity 
                                                     WHERE Id = @ProductId";

                foreach (var product in cartProducts)
                {
                    SqlCommand itemCommand = new SqlCommand(insertItemQuery, connection, transaction);
                    itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                    itemCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    itemCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
                    itemCommand.ExecuteNonQuery();

                    SqlCommand updateCommand = new SqlCommand(updateProductQuery, connection, transaction);
                    updateCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
                    updateCommand.Parameters.AddWithValue("@ProductId", product.Id);
                    updateCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                return (true, orderId, finalPrice, deliveryPrice, null);
            }
        }
    }
}