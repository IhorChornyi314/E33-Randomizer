using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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

// TODO: At some point would love to just replace this with source generators that would handle creating these.  This is mostly because you can't use generic types in extension expressions.
// TODO: Revisit possibly creating a ObservableDictionary that doesn't suck.
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
        
        public bool TryGetValue(string key, [NotNullWhen(true)] out float? result)
        {
            result = collection.FirstOrDefault(x => x.Key == key)?.Value;
            return result != null;
        }

        public void Remove(string key)
        {
            var item = collection.FirstOrDefault(x => x.Key == key);
            if (item != null) collection.Remove(item);
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
        
        public bool TryGetValue(string key, [NotNullWhen(true)] out byte? result)
        {
            result = collection.FirstOrDefault(x => x.Key == key)?.Value;
            return result != null;
        }
        
        public void Remove(string key)
        {
            var item = collection.FirstOrDefault(x => x.Key == key);
            if (item != null) collection.Remove(item);
        }

        private bool ExistsCheck(string key)
        {
            return collection.Any(x => x.Key == key);
        }
    }
    
    extension(ObservableCollectionWithChildListener<StringDictionaryKeyValuePairViewModel<StringByteKeyValuePairViewModel>> collection)
    {
        public void Add(string key, IEnumerable<KeyValuePair<string, byte>> value)
        {
            var subCollection = new ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel>();
            subCollection.AddRange(value);
            
            var item = new StringDictionaryKeyValuePairViewModel<StringByteKeyValuePairViewModel>(key, subCollection, collection.ExistsCheck);
            collection.Add(item);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, byte>>>> items)
        {
            foreach (var item in items)
            {
                collection.Add(item.Key, item.Value);
            }
        }

        public bool TryGetValue(string key,  [NotNullWhen(true)] out ObservableCollectionWithChildListener<StringByteKeyValuePairViewModel>? result)
        {
            result = collection.FirstOrDefault(x => x.Key == key)?.Value;
            return result != null;
        }

        private bool ExistsCheck(string key)
        {
            return collection.Any(x => x.Key == key);
        }
    }
}

public static class ObservableCollectionExtensions
{
    extension(ObservableCollection<string> collection)
    {
        public void AddRange(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                collection.Add(value);
            }
        }
        
    }
}