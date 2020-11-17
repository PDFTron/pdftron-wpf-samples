using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PDFViewerWPFDemo.Converters
{
    class TextToImageConverter : IValueConverter
    {
        public static TextToImageConverter Instance = new TextToImageConverter();

         public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.ToString())
            {
                case "fileopen":
                    return new BitmapImage(new Uri($"pack://application:,,,/resources/fileopen.gif"));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
