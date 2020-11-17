using pdftron.PDF;
using PDFViewerWPFDemo.ViewModel;
using System;
using System.IO;
using System.Windows;

namespace PDFViewerWPFDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}
