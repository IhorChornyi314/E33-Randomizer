using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace E33Randomizer.UIControls;

public static class AutoCompleteExtensions
{
    extension(AutoCompleteBox box)
    {
        /// <summary>
        /// Causes the <see cref="AutoCompleteBox"/> to open it's dropdown on click or focus, regardless of if it was already focused before or not.
        /// </summary>
        public void AddAutoDropDownOnFocusAndClickHandler()
        {
            box.GotFocus += (_, _) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    box.IsDropDownOpen = true;
                });
            };
            box.AddHandler(InputElement.PointerPressedEvent, (_, _) =>
                {
                    var internalTextboxForAutoComplete = box.GetVisualDescendants().First(x => x is TextBox);
                
                    if (!box.IsDropDownOpen && internalTextboxForAutoComplete is TextBox { IsFocused: true })
                    {
                        box.IsDropDownOpen = true;
                    }
                }, RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                handledEventsToo: true);
        }
    }
}