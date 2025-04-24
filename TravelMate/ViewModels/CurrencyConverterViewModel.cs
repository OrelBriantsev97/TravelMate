using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelMate.Services;
using Microsoft.Maui.Controls;

namespace TravelMate.ViewModels
{
    public class CurrencyConverterViewModel : BindableObject
    {
        // ViewModel responsible for loading exchange rates and converting between currencies.
        private Dictionary<string, decimal> exchangeRates;
        private decimal amount;
        private string selectedFrom;
        private string selectedTo;
        private string convertedResult;
        public ObservableCollection<string> CurrencyList { get; set; } = new();

        // The amount to be converted. Changing this value will trigger an immediate conversion.
        public decimal Amount
        {
            get => amount;
            set
            {
                amount = value;
                OnPropertyChanged();
                ConvertCurrency();
            }
        }

        // The source currency
        public string SelectedFrom
        {
            get => selectedFrom;
            set
            {
                selectedFrom = value;
                OnPropertyChanged();
                ConvertCurrency();
            }
        }

        // The target currency
        public string SelectedTo
        {
            get => selectedTo;
            set
            {
                selectedTo = value;
                OnPropertyChanged();
                ConvertCurrency();
            }
        }

        public string ConvertedResult
        {
            get => convertedResult;
            set { convertedResult = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        // The result of the currency conversion formatted as "{value} {currencyCode}".
        public CurrencyConverterViewModel()
        {
            RefreshCommand = new Command(async () => await LoadExchangeRates());
            _ = LoadExchangeRates();
        }

        // Asynchronously loads the latest exchange rates (base USD) from the service,
        //populates the currency list, and sets default selections.
        private async Task LoadExchangeRates()
        {
            exchangeRates = await CurrencyConverterService.GetExchangeRates("USD");

            if (exchangeRates != null)
            {
                CurrencyList.Clear();
                foreach (var key in exchangeRates.Keys)
                    CurrencyList.Add(key);

                SelectedFrom = "USD";
                SelectedTo = "EUR";
            }
        }

        // Performs the currency conversion using the currently selected rates
        // and updates the ConvertedResult
        private void ConvertCurrency()
        {
            if (exchangeRates == null || string.IsNullOrEmpty(SelectedFrom) || string.IsNullOrEmpty(SelectedTo))
                return;

            if (exchangeRates.TryGetValue(SelectedFrom, out decimal fromRate) &&
                exchangeRates.TryGetValue(SelectedTo, out decimal toRate))
            {
                var usdAmount = Amount / fromRate;
                var converted = usdAmount * toRate;
                ConvertedResult = $"{converted:F2} {SelectedTo}";
            }
        }
    }
}
