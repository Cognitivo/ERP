﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Cognitivo.Converters
{
    class salesorderdetailtoEnable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            entity.sales_order_detail sales_order_detail = (entity.sales_order_detail)value;
            if (sales_order_detail!=null)
            {
                if (sales_order_detail.id_sales_order_detail.ToString() == 0.ToString())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
          
            //throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
