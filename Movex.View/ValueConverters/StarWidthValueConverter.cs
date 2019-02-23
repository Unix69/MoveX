using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Movex.View
{
    class StarWidthValueConverter : BaseValueConverter<StarWidthValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var listview = value as ListView;
            var width = listview.Width;
            var gv = listview.View as GridView;
            var margin = 10;

            for (var i = 0; i < gv.Columns.Count; i++)
            {
                if (!double.IsNaN(gv.Columns[i].Width))
                    width -= gv.Columns[i].Width;
            }
            return width - margin;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
