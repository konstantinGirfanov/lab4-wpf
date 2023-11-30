using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4_wpf
{
    public class Step
    {
        public string StepDecription {  get; set; }
        public int[] Indexes {  get; set; }
        public Operations Operation {  get; set; }
        public SwapDataInformation? DataInfo { get; set; }
        public List<SelectInfo>? SelectInfo { get; set; }
        public int Left {  get; set; }
        public int Right {  get; set; }

        public Step(string StepDesc, int[] Ind, Operations op, SwapDataInformation info)
        {
            StepDecription = StepDesc;
            Indexes = new int[Ind.Length];
            Array.Copy(Ind, Indexes, Ind.Length);
            Operation = op;
            DataInfo = info;
        }

        public Step(string StepDesc, Operations op, List<SelectInfo> selectInfos)
        {
            StepDecription = StepDesc;
            Indexes = Array.Empty<int>();
            Operation = op;
            DataInfo = null;
            SelectInfo = selectInfos;
        }

        public Step(string StepDesc, int[] Ind, Operations op)
        {
            StepDecription = StepDesc;
            Indexes = new int[Ind.Length];
            Array.Copy(Ind, Indexes, Ind.Length);
            Operation = op;
            DataInfo = null;
            SelectInfo = null;
        }
    }

    public enum Operations
    {
        Select,
        Switch,
        WriteToTemp,
        WriteFromTemp,
        SelectInTemp,
        None
    }
}
