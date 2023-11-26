using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf
{
    public class SwapDataInformation
    {
        public Value Data { get; set; }
        public int DestinationIndex {  get; set; }
        public int SourceIndex {  get; set; }
        public int SourceFileNumber {  get; set; }
        public SwapDataInformation(Value data, int destinationIndex, int sourceIndex, int fileNumber)
        {
            Data = data;
            DestinationIndex = destinationIndex;
            SourceIndex = sourceIndex;
            SourceFileNumber = fileNumber;
        }
    }
}
