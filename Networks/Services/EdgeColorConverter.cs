using GraphSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Networks
{
    public class MyEdge : TypedEdge<Object>
    {
        public String Id { get; set; }

        public Color EdgeColor { get; set; }

        public MyEdge(Object source, Object target) : base(source, target, EdgeTypes.General) { }
    }
    public class EdgeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
