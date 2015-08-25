using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Ocr;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage.FileProperties;
using Windows.Graphics.Imaging;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleOcr10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            ResultsBox.Text = "";
            StatusBlock.Text = "Running...";
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

            List<Task<string>> taskList = new List<Task<string>>();
            foreach (StorageFile file in files)
            {
                try
                {
                    taskList.Add(ProcessImage(file));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            await Task.WhenAll<string>(taskList);

            foreach (var task in taskList)
            {
                ResultsBox.Text += $"Results:\n{task.Result}---------------------------------------------------\n";
            }
            StatusBlock.Text = "Ready";

        }

        private async Task<string> ProcessImage(StorageFile file)
        {            
            SoftwareBitmap bitmap;          
            using (var imgStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(imgStream);
                bitmap = await decoder.GetSoftwareBitmapAsync();                                
            }

            if(bitmap == null)
            {
                return "No text found.\n";
            }

            OcrEngine engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));
            OcrResult result = await engine.RecognizeAsync(bitmap);
            StringBuilder sb = new StringBuilder();
            if (result.Lines == null)
            {
                return "No text found.";
            }
            foreach (var line in result.Lines)
            {
                foreach (var word in line.Words)
                {
                    sb.Append(word.Text + " ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
