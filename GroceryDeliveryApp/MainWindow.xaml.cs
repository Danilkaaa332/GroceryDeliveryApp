using GroceryDeliveryApp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GroceryDeliveryApp
{
    public partial class MainWindow : Window
    {
        private Database db = new Database();
        List<GroceryProduct> cartProducts = new List<GroceryProduct>();
        private int currentUserId = 1;

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
            LoadOrders();
        }

        /// <summary>
        /// Загрузка продуктов
        /// </summary>
        private void LoadProducts()
        {
            ProductsList.ItemsSource = db.LoadProducts();
        }

        /// <summary>
        /// Загрузка заказов с адресами
        /// </summary>
        private void LoadOrders()
        {
            OrdersList.Items.Clear();
            var orders = db.LoadOrders(currentUserId);
            foreach (var order in orders)
            {
                OrdersList.Items.Add(order);
            }
        }

        /// <summary>
        /// Добавить в корзину
        /// </summary>
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт");
                return;
            }

            GroceryProduct product = (GroceryProduct)ProductsList.SelectedItem;

            if (product.IsExpired())
            {
                MessageBox.Show("Продукт просрочен");
                return;
            }

            if (product.Quantity <= 0)
            {
                MessageBox.Show("Товара нет в наличии");
                return;
            }

            var existingProduct = cartProducts.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Quantity++;
                UpdateCartDisplay();
            }
            else
            {
                GroceryProduct cartProduct = new GroceryProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ExprirationDate = product.ExprirationDate,
                    Category = product.Category
                };
                cartProducts.Add(cartProduct);
                UpdateCartDisplay();
            }

            UpdateTotalPrice();
        }

        /// <summary>
        /// Обновление отображения корзины
        /// </summary>
        private void UpdateCartDisplay()
        {
            CartList.Items.Clear();
            foreach (var product in cartProducts)
            {
                CartList.Items.Add($"{product.Name} - {product.Price} ₽ x {product.Quantity} = {product.Price * product.Quantity} ₽");
            }
        }

        /// <summary>
        /// Удалить продукт из корзины
        /// </summary>
        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (CartList.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт в корзине для удаления");
                return;
            }

            int selectedIndex = CartList.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < cartProducts.Count)
            {
                cartProducts.RemoveAt(selectedIndex);
                UpdateCartDisplay();
                UpdateTotalPrice();
                MessageBox.Show("Продукт удалён из корзины");
            }
        }

        /// <summary>
        /// Обновление суммы
        /// </summary>
        private void UpdateTotalPrice()
        {
            DeliveryOrder order = new DeliveryOrder
            {
                Products = cartProducts
            };
            decimal basketTotal = order.CalculateBasketPrice();
            decimal deliveryPrice = order.CalculateDeliveryPrice(basketTotal);
            decimal total = basketTotal + deliveryPrice;
            TotalPriceText.Text = $"Товары: {basketTotal} ₽ | Доставка: {deliveryPrice} ₽ | Итого: {total} ₽";
        }

        /// <summary>
        /// Оформление заказа
        /// </summary>
        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (cartProducts.Count == 0)
            {
                MessageBox.Show("Корзина пустая");
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Введите адрес доставки");
                return;
            }

            var result = db.CreateOrder(currentUserId, AddressTextBox.Text, cartProducts);
            if (result.Success)
            {
                OrdersList.Items.Add($"Заказ #{result.OrderId} | {DateTime.Now:dd.MM.yyyy HH:mm} | {result.FinalPrice} ₽ | Адрес: {AddressTextBox.Text}");

                MessageBox.Show($"Заказ #{result.OrderId} оформлен!\nСумма: {result.FinalPrice} ₽\nАдрес доставки: {AddressTextBox.Text}\nСтоимость доставки: {result.DeliveryPrice} ₽", "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                cartProducts.Clear();
                CartList.Items.Clear();
                AddressTextBox.Clear();
                UpdateTotalPrice();

                LoadProducts();
                LoadOrders();
            }
            else
            {
                MessageBox.Show(result.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}