using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace E33Randomizer;

public class ObservableCollectionWithChildListener<T> : ObservableCollection<T> where T:INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? ItemPropertyChanged;

    protected override void InsertItem(int index, T item) {
        base.InsertItem(index, item);
        item.PropertyChanged += OnItemPropertyChanged;
    }

    protected override void RemoveItem(int index) {
        Items[index].PropertyChanged -= OnItemPropertyChanged;
        base.RemoveItem(index);
    }

    protected override void ClearItems() {
        foreach (var item in Items) item.PropertyChanged -= OnItemPropertyChanged;
        base.ClearItems();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INotifyDataErrorInfo.HasErrors)) return;
        
        ItemPropertyChanged?.Invoke(sender, e);
        
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}


public static class ObservableCollectionWithChildListenerExtensions
{
    extension(ObservableCollectionWithChildListener<StringFloatKeyValuePairViewModel> collection)
    {
        public void Add(string key, float value)
        {
            var item = new StringFloatKeyValuePairViewModel(key, value, collection.ExistsCheck);
            collection.Add(item);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, float>> items)
        {
            foreach (var item in items)
            {
                collection.Add(new StringFloatKeyValuePairViewModel(item.Key, item.Value, collection.ExistsCheck));
            }
        }

        private bool ExistsCheck(string key)
        {
            return collection.Any(x => x.Key == key);
        }
    }
    
    extension(ObservableCollectionWithChildListener<MenuItemViewModel> collection)
    {
        public void Add(string key, string value) =>
            collection.Add(new MenuItemViewModel(key, value));
    }
    
    extension(ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel> collection)
    {
        public void Add(string key, byte value)
        {
            var item = new StringByteKeyValuePairViewModel(key, value, collection.ExistsCheck);
            collection.Add(item);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, byte>> items)
        {
            foreach (var item in items)
            {
                collection.Add(new StringByteKeyValuePairViewModel(item.Key, item.Value, collection.ExistsCheck));
            }
        }

        private bool ExistsCheck(string key)
        {
            return collection.Any(x => x.Key == key);
        }
    }
}