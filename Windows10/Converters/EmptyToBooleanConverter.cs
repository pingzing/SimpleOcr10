using System;
using System.Collections;
using Windows.UI.Xaml.Data;
using System.Linq;

namespace SimpleOcr10.Converters
{
    public class EmptyToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converter that returns false for items that are null, or empty collections.
        /// </summary>        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                return value;
            }

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                var enumerator = enumerable.GetEnumerator();
                return enumerator.MoveNext();
            }
            
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}