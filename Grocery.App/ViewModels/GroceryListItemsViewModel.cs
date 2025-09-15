using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();

            var producten = _productService.GetAll().ToList();

            int[] groceryListItems = new int[MyGroceryListItems.Count()];

            for (int i = 0; i < MyGroceryListItems.Count(); i++)
            {
                groceryListItems[i] = MyGroceryListItems[i].ProductId;
            }

            foreach (var product in producten)
            {
                if (!groceryListItems.Contains(product.Id) && product.Stock != 0)
                {
                    AvailableProducts.Add(product);
                }
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null || product.Id <= 0) return;

            int amount = 1;

            GroceryListItem groceryListItem = new(0, GroceryList.Id, product.Id, amount);
            _groceryListItemsService.Add(groceryListItem);

            product.Stock -= amount;
            _productService.Update(product);

            OnGroceryListChanged(GroceryList);
        }
    }
}
