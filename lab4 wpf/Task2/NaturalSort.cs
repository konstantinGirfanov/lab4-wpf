using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Shapes;

namespace lab4_wpf
{
    public class NaturalSort
    {
        public string FileInput { get; set; } = "data.csv";
        private static int column;
        private readonly List<int> segmentsLength = new();
        public List<Step> Steps = new();
        public static List<SelectInfo> Selects = new();
        public string sortInfo { get; set; }

        public void Sort()
        {
            string[] input = $"../../../{sortInfo.Split(" ")[0]} {sortInfo.Split(" ")[1]}".Split(" ");
            File.Delete("data.csv");
            File.Copy(input[0], "data.csv");
            column = int.Parse(input[1]);
            Steps.Add(new Step($"Начинается внешняя сортировка файла {input[0]} по {column} столбцу.",
                Array.Empty<int>(), Operations.None));

            while (true)
            {
                segmentsLength.Clear();
                Steps.Add(new Step($"Начинается разделение файла на сегменты(отсортированные подпоследовательности).",
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
            return $"../../../{fileName} {col}";
        }

        private void SplitToFiles()
        {
            StreamReader sr = new StreamReader(File.Open(FileInput, FileMode.Open));
            StreamWriter writerA = new StreamWriter(File.Create("a.csv"));
            StreamWriter writerB = new StreamWriter(File.Create("b.csv"));
            int counter = 0;
            bool flag = true;
            var firstStr = sr.ReadLine();
            var secondStr = sr.ReadLine();
            int countElementsA = 0, countElementsB = 0, countElementsOriginal = 0;

            while (true)
            {
                bool tempFlag = flag;

                if (secondStr is not null)
                {
                    Steps.Add(new Step($"Из исходного файла считано 2 идущих подряд элемента.",
                        new int[2] { countElementsOriginal, countElementsOriginal + 1 }, Operations.Select));
                    countElementsOriginal++;

                    if (Compare(firstStr, secondStr))
                    {
                        if (flag)
                        {
                            Steps.Add(new Step("Значение в 1 строке меньше 2. длина сегмента для файла a.csv увеличивается на 1.",
                                Array.Empty<int>(), Operations.None));
                        }
                        else
                        {
                            Steps.Add(new Step($"Значение в 1 строке меньше 2. длина сегмента для файла b.csv увеличивается на 1.",
                                Array.Empty<int>(), Operations.None));
                        }
                        counter++;
                    }
                    else
                    {
                        tempFlag = !tempFlag;
                        segmentsLength.Add(counter + 1);
                        Steps.Add(new Step($"Значение в 1 строке больше 2, конец сегмента, его длина {counter + 1}",
                            Array.Empty<int>(), Operations.None));
                        counter = 0;
                    }
                }
                else
                {
                    if (firstStr != null)
                    {
                        Steps.Add(new Step($"считана единственная строка из исходного файла.",
                            new int[1] { countElementsOriginal }, Operations.Select));
                        countElementsOriginal++;
                    }
                }

                if (firstStr == null)
                {
                        Steps.Add(new Step($"не считано никаих строк. конец исходного файла.",
                            Array.Empty<int>(), Operations.None));
                    break;
                }

                if (flag)
                {
                    writerA.WriteLine(firstStr);

                    Value value = new(firstStr, firstStr.Split(";")[column]);
                    Steps.Add(new Step($"Записываем {firstStr} в файл a.csv",
                        Array.Empty<int>(), Operations.WriteToTemp,
                        new SwapDataInformation(value, countElementsA, countElementsOriginal - 1, 1)));
                    countElementsA++;
                }
                else
                {
                    writerB.WriteLine(firstStr);

                    Value value = new(firstStr, firstStr.Split(";")[column]);
                    Steps.Add(new Step($"Записываем {firstStr} в файл b.csv",
                        Array.Empty<int>(), Operations.WriteToTemp,
                        new SwapDataInformation(value, countElementsB, countElementsOriginal - 1, 2)));
                    countElementsB++;
                }

                firstStr = secondStr;
                secondStr = sr.ReadLine();
                flag = tempFlag;
            }
            segmentsLength.Add(counter + 1);

            sr.Close();
            writerA.Close();
            writerB.Close();
        }

        private void MergePairs()
        {
            StreamReader readerA = new StreamReader(File.Open("a.csv", FileMode.Open));
            StreamReader readerB = new StreamReader(File.Open("b.csv", FileMode.Open));
            StreamWriter bw = new StreamWriter(File.Create(FileInput));
            int segmentNumber = 0;
            int counterA = segmentsLength[segmentNumber];
            segmentNumber++;
            int counterB = segmentsLength[segmentNumber];
            segmentNumber++;
            string elementA = "", elementB = "";
            bool pickedA = false, pickedB = false, endA = false, endB = false;

            int countElementsA = 0, countElementsB = 0, countElementsOriginal = 0;
            while (true)
            {
                if (endA && endB)
                {
                    Steps.Add(new Step($"Все строки обоих файлов переписаны в исходный. Слияние завершено.",
                        Array.Empty<int>(), Operations.None));

                    break;
                }

                if (counterA == 0 && counterB == 0)
                {
                    Steps.Add(new Step($"Сегменты из обоих файлов слились в 1 сегмент исходного файла.",
                        Array.Empty<int>(), Operations.None));
                    if (segmentsLength.Count - 1 >= segmentNumber)
                    {
                        counterA = segmentsLength[segmentNumber];
                        segmentNumber++;
                    }
                    if (segmentsLength.Count - 1 >= segmentNumber)
                    {
                        counterB = segmentsLength[segmentNumber];
                        segmentNumber++;
                    }
                }

                if (!readerA.EndOfStream)
                {
                    if (counterA > 0)
                    {
                        if (!pickedA)
                        {
                            elementA = readerA.ReadLine();
                            pickedA = true;

                            Selects.Add(new SelectInfo(1, countElementsA));
                            countElementsA++;
                        }
                    }
                }
                else
                {
                    endA = true;
                }

                if (!readerB.EndOfStream)
                {
                    if (counterB > 0)
                    {
                        if (!pickedB)
                        {
                            elementB = readerB.ReadLine();
                            pickedB = true;

                            Selects.Add(new SelectInfo(2, countElementsB));
                            countElementsB++;
                        }
                    }
                }
                else
                {
                    endB = true;
                }

                if (endA && endB && pickedA == false && pickedB == false)
                {
                    Steps.Add(new Step($"Слияние завершено.",
                        Array.Empty<int>(), Operations.None));
                    break;
                }

                if (pickedA)
                {
                    if (pickedB)
                    {
                        SelectInfo[] temp = new SelectInfo[Selects.Count];
                        Array.Copy(Selects.ToArray(), temp, Selects.Count);
                        Steps.Add(new Step($"Сравниваем строки из файлов.", Operations.SelectInTemp, temp.ToList()));

                        if (Compare(elementA, elementB))
                        {
                            bw.WriteLine(elementA);
                            counterA--;
                            pickedA = false;

                            Value value = new(elementA, elementA.Split(";")[column]);
                            Steps.Add(new Step($"В исходный файл записана строка: {elementA}",
                                Array.Empty<int>(), Operations.WriteFromTemp,
                                new SwapDataInformation(value, countElementsOriginal, countElementsA - 1, 1)));
                            Selects.Remove(SelectInfo.FindInfo(Selects, 1));
                            countElementsOriginal++;
                        }
                        else
                        {
                            bw.WriteLine(elementB);
                            counterB--;
                            pickedB = false;

                            Value value = new(elementB, elementB.Split(";")[column]);
                            Steps.Add(new Step($"В исходный файл записана строка: {elementB}",
                                Array.Empty<int>(), Operations.WriteFromTemp,
                                new SwapDataInformation(value, countElementsOriginal, countElementsB - 1, 2)));
                            Selects.Remove(SelectInfo.FindInfo(Selects, 2));
                            countElementsOriginal++;
                        }
                    }
                    else
                    {
                        bw.WriteLine(elementA);
                        counterA--;
                        pickedA = false;

                        Value value = new(elementA, elementA.Split(";")[column]);
                        Steps.Add(new Step($"В исходный файл записана строка: {elementA}",
                            Array.Empty<int>(), Operations.WriteFromTemp,
                            new SwapDataInformation(value, countElementsOriginal, countElementsA - 1, 1)));
                        Selects.Remove(SelectInfo.FindInfo(Selects, 1));
                        countElementsOriginal++;
                    }
                }
                else if (pickedB)
                {
                    bw.WriteLine(elementB);
                    counterB--;
                    pickedB = false;

                    Value value = new(elementB, elementB.Split(";")[column]);
                    Steps.Add(new Step($"В исходный файл записана строка: {elementB}",
                        Array.Empty<int>(), Operations.WriteFromTemp,
                        new SwapDataInformation(value, countElementsOriginal, countElementsB - 1, 2)));
                    Selects.Remove(SelectInfo.FindInfo(Selects, 2));
                    countElementsOriginal++;
                }
            }

            Steps.Add(new Step("Слияние завершено.", Array.Empty<int>(), Operations.None));

            bw.Close();
            readerA.Close();
            readerB.Close();
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