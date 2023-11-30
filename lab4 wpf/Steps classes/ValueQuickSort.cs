using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf.Steps_classes
{
    class ValueQuickSort
    {
        public string Content { get; set; }
        public string ComparisonValue { get; set; }
        public string CurrentArray { get; set; } = "";
        public ValueQuickSort(string content, string value)
        {
            Content = content;
            ComparisonValue = value;
        }
    }
}
