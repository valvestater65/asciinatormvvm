using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using ImageMagick;
using ReactiveUI;

namespace AsciinatorMVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _filePath;
        private string _generatedAscii;
        private double _lineHeight;
        private double _fontSize;


        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        public double LineHeight
        {
            get => _lineHeight;
            set => this.RaiseAndSetIfChanged(ref _lineHeight, value);
        }

        public string GeneratedAscii
        {
            get => _generatedAscii;
            set => this.RaiseAndSetIfChanged(ref _generatedAscii, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => this.RaiseAndSetIfChanged(ref _filePath, value);
        }

        public ReactiveCommand<Unit, Unit> ProcessImage { get; }
        public ReactiveCommand<Unit,Unit> CopyToClipboard { get; }

        public MainWindowViewModel()
        {
            _filePath = "";
            _generatedAscii = "";
            FontSize = 8;
            
            var processCanExecute = this.WhenAnyValue(x => x.FilePath, (path) => !string.IsNullOrEmpty(path));
            ProcessImage = ReactiveCommand.CreateFromTask(ExecuteProcessImage, processCanExecute);
            CopyToClipboard = ReactiveCommand.CreateFromTask(ExecuteCopyToClipboard);
            
        }


        private async Task ExecuteProcessImage()
        {
            ConvertImageToAscii();
        }

        private async Task ExecuteCopyToClipboard()
        {
            var clipboard = Application.Current.Clipboard;
            clipboard.SetTextAsync(GeneratedAscii);
        }

        private void ConvertImageToAscii()
        {
            var stringSet = new ConcurrentBag<string>();
            GeneratedAscii = "";
            
            if (string.IsNullOrEmpty(_filePath))
                throw new ArgumentException("File cannot be null");
            
            using var image = new MagickImage(_filePath);

            var pixCol = image.GetPixels();

            foreach (var pix in pixCol.GroupBy(p => p.Y).OrderByDescending(p=> p.Key))
            {
                var builder = new StringBuilder();
                foreach (var p in pix)
                {
                    builder.Append(PixelToCharacter(p).ToString());
                }

                builder.Append(Environment.NewLine);
                stringSet.Add(builder.ToString());
            }

            //Aspect Ratio
            var ratio = (double)image.Width / (double)image.Height;
            GeneratedAscii = string.Join("",stringSet);
            LineHeight = FontSize * ratio;

        }


        private char PixelToCharacter(IPixel<byte> pixel)
        {
            //@%8WM#*+~=-():. "
            var brightchar = "@%8WM#*+~=-():. ";
            //var brightchar = "@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvuxrjft|()1{}[]?-_+~<>i!lI:,. ";
            var pixColor = pixel.ToColor();
            var avgBrigthness = Math.Floor((double)(pixColor.R + pixColor.G + pixColor.B) / 3);

            var charIndex = (avgBrigthness * brightchar.Length / 255);

            if (charIndex >= brightchar.Length)
                charIndex = brightchar.Length - 1;

            return brightchar[(int)charIndex];
        }


    }
}