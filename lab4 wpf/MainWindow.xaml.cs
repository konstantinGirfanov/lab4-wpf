using lab4_wpf.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab4_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BubbleSort(object sender, RoutedEventArgs e)
        {
            Window1 win = new();
            win.Show();
        }

        private void ExchangeSort(object sender, RoutedEventArgs e)
        {
            ExchangeSort exchangeSortWindow = new();
            exchangeSortWindow.Show();
        }

        private void QuickSort(object sender, RoutedEventArgs e)
        {
            QuickSortWindow window = new();
            window.Show();
        }

        private void MergeSort(object sender, RoutedEventArgs e)
        {
            MergeSort mergeSortWindow = new();
            mergeSortWindow.Show();
            mergeSortWindow.WindowState = WindowState.Maximized;
        }

        private void ExtSort(object sender, RoutedEventArgs e)
        {
            ExternalSortWindow window = new();
            window.Show();
            window.WindowState = WindowState.Maximized;
        }

        private void NaturalSort(object sender, RoutedEventArgs e)
        {
            NaturalSortWindow window = new();
            window.Show();
            window.WindowState = WindowState.Maximized;
        }

        private void MultiSort(object sender, RoutedEventArgs e)
        {
            MultipathSortWindow window = new();
            window.Show();
            window.WindowState = WindowState.Maximized;
        }
    }
}
