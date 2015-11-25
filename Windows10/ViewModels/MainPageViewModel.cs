using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SimpleOcr10.Common;
using SimpleOcr10.Models;

namespace SimpleOcr10.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private DelegateCommand<IList<object>> _removeCommand;
        public DelegateCommand<IList<object>> RemoveCommand 
            => _removeCommand ?? (_removeCommand = new DelegateCommand<IList<object>>(RemoveItems));

        private DelegateCommand _toggleSelectionCommand;
        public DelegateCommand ToggleSelectionCommand => _toggleSelectionCommand ?? (new DelegateCommand(ToggleSelection));

        private DelegateCommand _browseCommand;
        public DelegateCommand BrowseCommand => _browseCommand ?? (_browseCommand = new DelegateCommand(BrowseForImages));               

        private ObservableCollection<OcrResultDisplay> _resultsList = new ObservableCollection<OcrResultDisplay>();
        public ObservableCollection<OcrResultDisplay> ResultsList
        {
            get { return _resultsList; }
            set
            {
                if (value != _resultsList)
                {
                    _resultsList = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChanged();
            }
        }

        private bool _isMultiSelectCheckboxEnabled = false;
        public bool IsMultiSelectCheckboxEnabled
        {
            get { return _isMultiSelectCheckboxEnabled; }
            set
            {
                _isMultiSelectCheckboxEnabled = value;
                RaisePropertyChanged();
            }
        }

        private async void BrowseForImages()
        {            
            StatusText = "Running...";
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.CommitButtonText = "Open";
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpe");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

            if (files.Count > 0)
            {
                ResultsList.Clear();
            }
            foreach (StorageFile file in files)
            {
                try
                {
                    var result = await ProcessImage(file);
                    ResultsList.Add(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            StatusText = "Ready";
        }

        private async Task<OcrResultDisplay> ProcessImage(StorageFile file)
        {
            SoftwareBitmap bitmap;
            ImageSource source;
            using (var imgStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(imgStream);
                bitmap = await decoder.GetSoftwareBitmapAsync();
            }

            if (bitmap == null)
            {
                source = new BitmapImage();
                return new OcrResultDisplay
                {
                    OcrString = "No text found.\n",
                    OcrImage = source
                };
            }

            OcrEngine engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
            OcrResult result = await engine.RecognizeAsync(bitmap);
            StringBuilder sb = new StringBuilder();
            if (result.Lines == null)
            {
                source = new SoftwareBitmapSource();
                bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                await ((SoftwareBitmapSource)source).SetBitmapAsync(bitmap);
                return new OcrResultDisplay
                {
                    OcrString = "No text found.\n",
                    OcrImage = source
                };
            }
            foreach (var line in result.Lines)
            {
                foreach (var word in line.Words)
                {
                    sb.Append(word.Text + " ");
                }
                sb.AppendLine();
            }

            source = new SoftwareBitmapSource();
            bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            await ((SoftwareBitmapSource)source).SetBitmapAsync(bitmap);
            return new OcrResultDisplay { OcrString = sb.ToString(), OcrImage = source };
        }

        private void RemoveItems(IList<object> selectedItems)
        {
            if (selectedItems?.Count == ResultsList.Count)
            {
                ResultsList.Clear();
            }
            else
            {
                List<int> indicesToRemove = selectedItems
                    .Select(x => ResultsList.IndexOf(x as OcrResultDisplay))
                    .OrderByDescending(x => x)
                    .ToList();

                foreach (int i in indicesToRemove)
                {
                    ResultsList.RemoveAt(i);
                }
            }

        }

        private void ToggleSelection()
        {
            IsMultiSelectCheckboxEnabled = !IsMultiSelectCheckboxEnabled;
        }
    }
}