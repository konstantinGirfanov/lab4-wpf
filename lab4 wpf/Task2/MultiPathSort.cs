
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace lab4_wpf
{
    public class MultipathSort
    {
        public string FileInput { get; set; } = "data.csv";
        private static int PathCount { get; set; }
        private static int column { get; set; }
        private static int iterations { get; set; } = 1;
        private static readonly List<int> segmentsLength = new();
        public List<Step> Steps = new();
        public static List<SelectInfo> Selects = new();
        public string sortInfo { get; set; }

        public void Sort()
        {
            string[] input = $"../../../{sortInfo.Split(" ")[0]} {sortInfo.Split(" ")[1]} {sortInfo.Split(" ")[2]}".Split(" ");
            File.Delete("data.csv");
            File.Copy(input[0], "data.csv");
            column = int.Parse(input[1]);
            PathCount = int.Parse(input[2]);
            Steps.Add(new Step($"Начинается внешняя сортировка файла {input[0]} по {column} столбцу.",
                Array.Empty<int>(), Operations.None));

            while (true)
            {
                segmentsLength.Clear();
                Steps.Add(new Step($"Длины сегментов(отсортированных подпоследовательностей) равны {iterations}",
                    Array.Empty<int>(), Operations.None));
                SplitToFiles();

                if (segmentsLength.Count == 1)
                {
                    File.Delete("../../../sortedData.csv");
                    File.Copy("data.csv", "../../../sortedData.csv");
                    Steps.Add(new Step($"Конец сортировки.",
                        Array.Empty<int>(), Operations.None));
                    break;
                }
                Console.WriteLine("\r\n\r\n\r\n\r\n\r\n");
                MergePairs();
                Console.WriteLine("\r\n\r\n\r\n\r\n\r\n");
            }
        }

        private static string Input()
        {
            Console.Write("Введите имя файла: ");
            string fileName = Console.ReadLine();
            while (!File.Exists($"../../../{fileName}"))
            {
                Console.Write("Файла не существует. Введите имя файла: ");
                fileName = Console.ReadLine();
            }

            StreamReader reader = new StreamReader($"../../../{fileName}");
            int maxColumn = reader.ReadLine().Split(";").Length;
            reader.Close();
            Console.Write("Введите номер столбца: ");
            string input = Console.ReadLine();
            int col;
            while (!int.TryParse(input, out col) || col < 0 || col >= maxColumn)
            {
                Console.Write("Неверный ввод. Введите номер столбца: ");
                input = Console.ReadLine();
            }

            Console.Write("Введите количество путей: ");
            string inputPaths = Console.ReadLine();
            int paths;
            while (!int.TryParse(inputPaths, out paths) || paths <= 1)
            {
                Console.Write("Неверный ввод. Введите количество путей: ");
                inputPaths = Console.ReadLine();
            }
            return $"../../../{fileName} {col} {paths}";
        }

        private void SplitToFiles()
        {
            StreamReader sr = new(File.Open(FileInput, FileMode.Open));
            FileWriter[] writers = new FileWriter[PathCount];
            int[] countElements = new int[PathCount + 1];
            for (int i = 0; i < PathCount; i++)
            {
                writers[i] = new FileWriter($"a{i}.csv");
            }

            int counter = 0;
            int flag = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                if(counter == iterations)
                {
                    var tempFlag = flag;
                    SwitchFlag(ref flag);
                    
                    Steps.Add(new Step($"В файл {writers[tempFlag].FileName} записан сегмент длиной {iterations}. " +
                            $"Следующий сегмент будет записан в файл {writers[flag].FileName}",
                            Array.Empty<int>(), Operations.None));

                    segmentsLength.Add(counter);
                    counter = 0;
                }

                writers[flag].Writer.WriteLine(line);
                counter++;

                Value value = new(line, line.Split(";")[column]);
                Steps.Add(new Step($"в файл {writers[flag].FileName} записана строка {line}",
                    Array.Empty<int>(), Operations.WriteToTemp,
                    new SwapDataInformation(value, countElements[flag + 1], countElements[0], flag + 1)));

                countElements[flag + 1]++;
                countElements[0]++;
            }
            segmentsLength.Add(counter);

            Steps.Add(new Step($"Исходный файл разделен.", Array.Empty<int>(), Operations.None));

            sr.Close();
            foreach(FileWriter sw in writers)
            {
                sw.Writer.Close();
            }
        }

        private static void SwitchFlag(ref int flag)
        {
            if (flag == PathCount - 1)
            {
                flag = 0;
            }
            else
            {
                flag++;
            }
        }

        private void MergePairs()
        {
            StreamWriter bw = new(File.Create(FileInput));
            int segmentNumber = 0;
            int[] countElements = new int[PathCount + 1];
            Segment[] segments = new Segment[PathCount];
            for (int i = 0; i < PathCount; i++)
            {
                segments[i] = new Segment(GetSegmentLength(ref segmentNumber), $"a{i}.csv", false, false, "", column, i, 0);
            }

            while (true)
            {
                bool needForCount = true;
                foreach (Segment segment in segments)
                {
                    needForCount &= segment.Counter == 0;
                }
                if (needForCount)
                {
                    Steps.Add(new Step($"Сегменты из всех файлов слились в 1 сегмент исходного файла.",
                        Array.Empty<int>(), Operations.None));

                    foreach (Segment segment in segments)
                    {
                        segment.Counter = GetSegmentLength(ref segmentNumber);
                    }
                }

                for(int  i = 0; i < segments.Length; i++)
                {
                    if (!segments[i].Reader.EndOfStream)
                    {
                        if (segments[i].Counter > 0)
                        {
                            if (!segments[i].Picked)
                            {
                                segments[i].Value = segments[i].Reader.ReadLine();
                                segments[i].Picked = true;

                                Selects.Add(new SelectInfo(segments[i].FileNumber + 1, segments[i].FileIndex));
                                segments[i].FileIndex++;




                                /*Selects.Add(new SelectInfo(i + 1, countElements[i + 1]));
                                countElements[i + 1]++;*/

                            }
                        }
                    }
                    else
                    {
                        segments[i].End = true;
                    }
                }

                bool end = true;
                foreach (Segment segment in segments)
                {
                    end &= segment.End && segment.Picked == false;
                }
                if (end)
                {
                    Steps.Add(new Step($"Все файлы закончились, нет ни одного считанного элемента из файлов. Слияние завершено.",
                        Array.Empty<int>(), Operations.None));
                    break;
                }

                Segment temp = null;
                foreach (Segment segment in segments)
                {
                    if (segment.Picked)
                    {
                        temp = segment;
                        break;
                    }
                }

                foreach (Segment segment in segments)
                {
                    if (segment.Picked)
                    {
                        if (temp.Compare(segment))
                        {
                            temp = segment;
                        }
                    }
                }
                SelectInfo[] tempSelects = new SelectInfo[Selects.Count];
                Array.Copy(Selects.ToArray(), tempSelects, Selects.Count);
                Steps.Add(new Step($"Сравниваем строки из файлов.", Operations.SelectInTemp, tempSelects.ToList()));

                bw.WriteLine(temp.Value);
                temp.Picked = false;
                temp.Counter--;

                Value value = new(temp.Value, temp.Value.Split(";")[column]);
                Steps.Add(new Step($"В исходный файл записана строка: {temp.Value}",
                    Array.Empty<int>(), Operations.WriteFromTemp,
                    new SwapDataInformation(value, countElements[0], temp.FileIndex - 1, temp.FileNumber + 1)));
                Selects.Remove(SelectInfo.FindInfo(Selects, temp.FileNumber  + 1));
                countElements[0]++;
            }

            bw.Close();
            foreach (Segment segment in segments)
            {
                segment.Reader.Close();
            }

            Steps.Add(new Step($"Длина сегментов умножается на {PathCount}.", Array.Empty<int>(), Operations.None));
            iterations *= PathCount;
        }

        private static int GetSegmentLength(ref int segmentNumber)
        {
            if (segmentsLength.Count - 1 >= segmentNumber)
            {
                int temp = segmentsLength[segmentNumber];
                segmentNumber++;
                return temp;
            }

            return 0;
        }

        private static bool Compare(string firstLine, string secondLine)
        {
            if (double.TryParse(firstLine.Split(';')[column], out double fDouble))
            {
                double sDouble = double.Parse(secondLine.Split(';')[column]);
                return fDouble <= sDouble;
            }

            if (DateTime.TryParse(firstLine.Split(';')[column], out DateTime fDate))
            {
                DateTime sDate = DateTime.Parse(secondLine.Split(';')[column]);
                return fDate <= sDate;
            }

            return string.Compare(firstLine.Split(';')[column], secondLine) <= 0;
        }
    }
}