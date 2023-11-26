using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace lab4_wpf
{
    public class ExternalSort
    {
        public string FileInput { get; set; } = "data.csv";
        private ulong iterations = 1, segments;
        private static int column = 1;
        public List<Step> Steps = new();
        public static List<SelectInfo> Selects = new();
        public string sortInfo {  get; set; }


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
                Steps.Add(new Step($"Длины сегментов(отсортированных подпоследовательностей) равны {iterations}",
                    Array.Empty<int>(), Operations.None));
                SplitToFiles();

                if (segments == 1)
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
            segments = 1;
            StreamReader br = new (File.Open(FileInput, FileMode.Open));
            StreamWriter writerA = new (File.Create("a.csv"));
            StreamWriter writerB = new (File.Create("b.csv"));
            ulong counter = 0;
            bool flag = true;
            int countElementsA = 0, countElementsB = 0, countElementsOriginal = 0;

            while (!br.EndOfStream)
            {
                if (counter == iterations)
                {
                    if (flag)
                    {
                        Steps.Add(new Step($"В файл a.csv записан сегмент длиной {iterations}. " +
                            $"Следующий сегмент будет записан в файл b.csv",
                            Array.Empty<int>(), Operations.None));
                    }
                    else
                    {
                        Steps.Add(new Step($"В файл b.csv записан сегмент длиной {iterations}. " +
                            $"Следующий сегмент будет записан в файл a.csv",
                            Array.Empty<int>(), Operations.None));
                    }

                    flag = !flag;
                    counter = 0;
                    segments++;
                }

                if (flag)
                {
                    string line = br.ReadLine();
                    writerA.WriteLine(line);
                    counter++;

                    Value value = new(line, line.Split(";")[column]);
                    Steps.Add(new Step($"Записываем {line} в файл a.csv",
                        Array.Empty<int>(), Operations.WriteToTemp,
                        new SwapDataInformation(value, countElementsA, countElementsOriginal, 1)));
                    countElementsA++;
                }
                else
                {
                    string line = br.ReadLine();
                    writerB.WriteLine(line);
                    counter++;

                    Value value = new(line, line.Split(";")[column]);
                    Steps.Add(new Step($"Записываем {line} в файл b.csv",
                        Array.Empty<int>(), Operations.WriteToTemp,
                        new SwapDataInformation(value, countElementsB, countElementsOriginal, 2)));
                    countElementsB++;
                }
                countElementsOriginal++;
            }
            Steps.Add(new Step($"Исходный файл разделен.", Array.Empty<int>(), Operations.None));

            br.Close();
            writerA.Close();
            writerB.Close();
        }

        private void MergePairs()
        {
            StreamReader readerA = new StreamReader(File.Open("a.csv", FileMode.Open));
            StreamReader readerB = new StreamReader(File.Open("b.csv", FileMode.Open));
            StreamWriter bw = new StreamWriter(File.Create(FileInput));
            ulong counterA = iterations, counterB = iterations;
            string elementA = "", elementB = "";
            bool pickedA = false, pickedB = false, endA = false, endB = false;

            int countElementsA = 0, countElementsB = 0, countElementsOriginal = 0;

            while (true)
            {
                //Selects.Clear();
                if (endA && endB)
                {
                    Steps.Add(new Step($"Все строки обоих файлов переписаны в исходный. Слияние завершено.",
                        Array.Empty<int>(), Operations.None));
                    break;
                }

                if (counterA == 0 && counterB == 0)
                {
                    Steps.Add(new Step($"Сегменты длиной {iterations} обоих файлов слились в 1 сегмент исходного файла.",
                        Array.Empty<int>(), Operations.None));
                    counterA = iterations;
                    counterB = iterations;
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


            Steps.Add(new Step("Длина сегментов удваивается.", Array.Empty<int>(), Operations.None));
            iterations *= 2;

            bw.Close();
            readerA.Close();
            readerB.Close();
        }

        private static bool Compare(string firstLine, string secondLine)
        {
            if (double.TryParse(firstLine.Split(';')[column], out double fDouble))
            {
                double sDouble = double.Parse(secondLine.Split(';')[column]);
                return fDouble < sDouble;
            }

            if (DateTime.TryParse(firstLine.Split(';')[column], out DateTime fDate))
            {
                DateTime sDate = DateTime.Parse(secondLine.Split(';')[column]);
                return fDate < sDate;
            }

            return string.Compare(firstLine.Split(';')[column], secondLine.Split(';')[column]) < 0;
        }
    }
}