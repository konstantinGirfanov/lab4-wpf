using System.IO;
using System;

namespace lab4_wpf
{
    public class Segment
    {
        public StreamReader Reader { get; set; }
        public int Counter { get; set; }
        public string FilePath { get; set; }
        public bool Picked { get; set; }
        public bool End { get; set; }
        public string Value { get; set; }
        public int Column { get; set; }
        public int FileNumber { get; set; }
        public int FileIndex {  get; set; }
        public Segment(int counter, string path, bool picked, bool end, string value, int columnn)
        {
            Counter = counter;
            FilePath = path;
            Reader = new StreamReader(path);
            Picked = picked;
            End = end;
            Value = value;
            Column = columnn;
        }
        public Segment(int counter, string path, bool picked, bool end, string value, int columnn, int number, int fileIndex)
        {
            Counter = counter;
            FilePath = path;
            Reader = new StreamReader(path);
            Picked = picked;
            End = end;
            Value = value;
            Column = columnn;
            FileNumber = number;
            FileIndex = fileIndex;
        }

        public bool Compare(Segment second)
        {
            if (double.TryParse(Value.Split(';')[Column], out double fDouble))
            {
                double sDouble = double.Parse(second.Value.Split(';')[Column]);
                return fDouble >= sDouble;
            }

            if (DateTime.TryParse(Value.Split(';')[Column], out DateTime fDate))
            {
                DateTime sDate = DateTime.Parse(second.Value.Split(';')[Column]);
                return fDate >= sDate;
            }

            return string.Compare(Value.Split(';')[Column], second.Value.Split(';')[Column]) >= 0;
        }
    }
}
