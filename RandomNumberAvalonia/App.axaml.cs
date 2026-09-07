using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using RandomNumberAvalonia.Services;
using RandomNumberAvalonia.ViewModels;
using RandomNumberAvalonia.Views;

namespace RandomNumberAvalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                // 启动时读取配置和历史记录
                var settings = AppSettings.LoadOrDefault(AppPaths.ConfigPath);
                var history = new HistoryStore(AppPaths.HistoryPath);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(settings, history)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}