using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia.Veldrid.Sample
{
    public class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = new PopupViewModel {CloseCommand = new ActionCommand(Close)};
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}