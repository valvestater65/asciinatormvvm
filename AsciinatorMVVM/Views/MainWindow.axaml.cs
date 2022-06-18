using System.Reactive.Disposables;
using AsciinatorMVVM.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace AsciinatorMVVM.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private Button ButnProcessImage => this.FindControl<Button>("BtnProcessImage");
        private Button ButnCopyToClipboard => this.FindControl<Button>("BtnCopyToClipboard");
        
        
        public MainWindow()
        {
            this.WhenActivated(disposable =>
                {
                    this.BindCommand(ViewModel, vm => vm.ProcessImage, view => view.ButnProcessImage)
                        .DisposeWith(disposable);
                    this.BindCommand(ViewModel, vm => vm.CopyToClipboard, view => view.ButnCopyToClipboard)
                        .DisposeWith(disposable);
                }
            );
            InitializeComponent();
        }

        private async void BtnSelectFile_OnClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                AllowMultiple = false
            };
            var result = await dialog.ShowAsync(this);
            ViewModel.FilePath = result[0];
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}