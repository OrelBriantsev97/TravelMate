using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelMate.Services;
using Microsoft.Maui.Controls;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class CurrencyConverterPage : ContentPage
    {
        private Dictionary<string, decimal> exchangeRates;
        private string baseCurrency = "USD";
        private readonly int userId;

        public CurrencyConverterPage(int UserId, string destination)
        {
            InitializeComponent();
            userId = UserId;
            NavigationBar navBar = new NavigationBar(userId,destination);
            NavigationContainer.Content = navBar;
            LoadExchangeRates();
        }

        private async void LoadExchangeRates()
        {
            exchangeRates = await CurrencyConverterService.GetExchangeRates("USD"); // Base currency doesn't matter here
            if (exchangeRates != null)
            {
                var currencyList = new List<string>(exchangeRates.Keys);

                FromCurrencyPicker.ItemsSource = currencyList;
                ToCurrencyPicker.ItemsSource = currencyList;

                FromCurrencyPicker.SelectedIndex = 0; // Default selection
                ToCurrencyPicker.SelectedIndex = 1; // Default to another currency
            }
        }


        private void OnAmountChanged(object sender, TextChangedEventArgs e)
        {
            ConvertCurrency();
        }

        private void OnCurrencyChanged(object sender, EventArgs e)
        {
            ConvertCurrency();
        }


        private void ConvertCurrency()
        {
            if (exchangeRates == null || string.IsNullOrEmpty(AmountEntry.Text)) return;

            if (decimal.TryParse(AmountEntry.Text, out decimal amount))
            {
                string fromCurrency = FromCurrencyPicker.SelectedItem?.ToString();
                string toCurrency = ToCurrencyPicker.SelectedItem?.ToString();

                if (fromCurrency != null && toCurrency != null &&
                    exchangeRates.ContainsKey(fromCurrency) && exchangeRates.ContainsKey(toCurrency))
                {
                    // Convert amount to USD first
                    decimal amountInUSD = amount / exchangeRates[fromCurrency];

                    // Convert from USD to target currency
                    decimal convertedAmount = amountInUSD * exchangeRates[toCurrency];

                    ConvertedAmountLabel.Text = $"{convertedAmount:F2} {toCurrency}";
                }
            }
        }

        private async void OnRefreshRatesClicked(object sender, EventArgs e)
        {
            LoadExchangeRates();
        }

    }
}
