using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using E33Randomizer.AotValidators;

namespace E33Randomizer;

public class StringFloatKeyValuePairViewModel(string key, float value, Func<string, bool> existsCheck)
    : KeyValuePairViewModel<string, float>(key, value, existsCheck);

public class StringByteKeyValuePairViewModel(string key, byte value, Func<string, bool> existsCheck)
    : KeyValuePairViewModel<string, byte>(key, value, existsCheck);

public class StringDictionaryKeyValuePairViewModel<TSubType> : KeyValuePairViewModel<string, ObservableCollectionWithChildListener<TSubType>> 
    where TSubType : INotifyPropertyChanged
{
    public StringDictionaryKeyValuePairViewModel(string key,
        ObservableCollectionWithChildListener<TSubType> value,
        Func<string, bool> existsCheck) : base(key, value, existsCheck)
    {
        value.ItemPropertyChanged += (sender, args) =>
        {
            this.OnPropertyChanged(nameof(Value));
        };
    }
}

public partial class KeyValuePairViewModel<TKey, TValue> : ObservableValidator, ISiblingCheck<string>
{
    [AotMinLength(1)]
    [AotSiblingNotExistsValidator<string>]
    public TKey Key { get; 
        set
        {
            if (value is null) return;
            
            if (EqualityComparer<TKey>.Default.Equals(field, value)) return;
            
            OnPropertyChanging();
            ValidateProperty(value);
            if (HasErrors)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    OnPropertyChanged();
                });
                return;
            }
            field = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    public partial TValue Value { get; set; }

    public IEnumerable<ValidationResult> KeyErrors => GetErrors(nameof(Key));
    
    public KeyValuePairViewModel(TKey key, TValue value, Func<string, bool> existsCheck)
    {
        CheckFunc = existsCheck;
        Key = key;
        Value = value;
    }
    
    public Func<string, bool> CheckFunc { get; }
}