using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf
{
    public class Value
    {
        public string Content { get; set; }
        public string ComparisonValue { get; set; }
        public Value(string content, string value)
        {
            Content = content;
            ComparisonValue = value;
        }
    }
}
