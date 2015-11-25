using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.System;
using SimpleOcr10.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleOcr10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private IList<object> _selectedListItems;
        public IList<object> SelectedListItems
        {
            get { return _selectedListItems; }
            set
            {
                _selectedListItems = value;
                RaisePropertyChanged();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as MainPageViewModel;
            };

            DataContext = new MainPageViewModel();
        }       

        public MainPageViewModel ViewModel { get; set; }
       
        private void OcrListItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }
            var flyout = FlyoutBase.GetAttachedFlyout(element) as MenuFlyout;
            flyout?.ShowAt(this, e.GetPosition(null));
        }

        private void FlyoutRemove_Click(object sender, RoutedEventArgs e)
        {
            (ViewModel).RemoveCommand.Execute(this.OcrResultsList.SelectedItems);
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Delete)
            {
                return;
            }
            if (OcrResultsList.SelectedItems.Count > 0)
            {
                (ViewModel).RemoveCommand.Execute(this.OcrResultsList.SelectedItems);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OcrResultsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedListItems = e.AddedItems;
        }
    }
}
