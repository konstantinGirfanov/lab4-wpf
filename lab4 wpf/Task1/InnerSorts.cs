using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup.Localizer;
using System.Windows.Media.Media3D;

namespace lab4_wpf;

public static class InnerSorts
{
    public static Logger SortLogger = Logger.GetLogger(0, 1000);
    public static List<Step> Steps = new();

    public static void BubbleSort<T>(this List<T> list) where T : IComparable
    {
        var n = list.Count;
        Steps.Add(new Step($"Начинается сортировка (Метод: Bubble Sort) массива длинной: {list.Count}.",
            Array.Empty<int>(), Operations.None, null));
        
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                Steps.Add(new Step($"Сравниваем {j} элемент ({list[j]}) и {j + 1} элемент ({list[j + 1]})",
                new int[2] {j, j + 1}, Operations.Select, null));
                if (list[j].CompareTo(list[j + 1]) > 0)
                {
                    Steps.Add(new Step($"{j} элемент ({list[j]}) больше, чем {j + 1} элемент ({list[j + 1]}), меняем их местами",
                    new int[2] { j, j + 1 }, Operations.Switch, null));
                    (list[j], list[j + 1]) = (list[j + 1], list[j]);
                }
            }
        }
        Steps.Add(new Step($"Конец сортировки.",
            Array.Empty<int>(), Operations.None, null));
    }
    
    public static void ExchangeSort<T>(this List<T> list) where T : IComparable
    {
        Steps.Add(new Step($"Начинается сортировка (Метод: Exchange Sort) массива длинной: {list.Count}.",
            Array.Empty<int>(), Operations.None, null));

        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                Steps.Add(new Step($"Сравниваем {i} элемент ({list[i]}) и {j} элемент ({list[j]})",
                    new int[2] {i, j}, Operations.Select, null));
                if (list[j].CompareTo(list[i]) < 0)
                {
                    Steps.Add(new Step($"{j} элемент ({list[j]}) меньше, чем {i} элемент ({list[i]}), меняем их местами",
                        new int[2] { i, j }, Operations.Switch, null));
                    (list[j], list[i]) = (list[i], list[j]);
                }
            }
        }
        Steps.Add(new Step($"Конец сортировки.",
            Array.Empty<int>(), Operations.None, null));
    }
    
    public static void QuickSort<T>(this IList<T> collection) where T : IComparable
    {
        Steps.Add(new Step($"Начинается сортировка (Метод: Quick Sort) массива длинной: {collection.Count}.",
            Array.Empty<int>(), Operations.None, null));

        collection.InsideQuickSort(0, collection.Count - 1);

        Steps.Add(new Step($"Конец сортировки.",
            Array.Empty<int>(), Operations.None, null));
    }

    private static void InsideQuickSort<T>(this IList<T> collection, int left, int right) where T : IComparable
    {
        if (left < right)
        {
            Step step = new Step($"Начинается поиск разделителя для под массива: array[{left}:{right}].",
                CreateIndexes(left, right), Operations.Select, null);
            step.Left = left;
            step.Right = right;
            Steps.Add(step);

            var pivotIndex = collection.Partition(left, right);
            collection.InsideQuickSort(left, pivotIndex);
            collection.InsideQuickSort(pivotIndex + 1, right);
        }
    }

    private static int[] CreateIndexes(int left, int right)
    {
        int[] result = new int[right - left + 1];
        for(int i = 0; i < right - left + 1; i++)
        {
            result[i] = i + left;
        }
        return result;
    }
    
    private static int Partition<T>(this IList<T> collection, int left, int right) where T : IComparable
    {
        var pivot = collection[left]; // В качестве опорного элемента выбирается самый левый элемент
        Steps.Add(new Step($"Опорный элемент: {pivot}.",
            new int[1] {left}, Operations.Select));

        var i = left - 1;
        var j = right + 1;

        while (true)
        {
            /*Steps.Add(new Step($"Левый указатель I: {i}.",
                new int[1] { i }, Operations.Select));*/
            do
            {
                i++;
                Steps.Add(new Step($"Левый указатель сдвигаем вправо, I++: {i}.",
                    new int[1] { i }, Operations.Select));

                Steps.Add(new Step($"Сравниваем элементы {i} ({collection[i]}) и {pivot}.",
                    new int[1] {i}, Operations.Select));
            } while (pivot.CompareTo(collection[i]) > 0); // > заменить на < для сортировки по убыванию

            Steps.Add(new Step($"Левый указатель достиг цели (pivot ({pivot}) <= элемент {i} ({collection[i]}) - выполняется).",
                new int[1] { i }, Operations.Select));

            /*Steps.Add(new Step($"Правый указатель j: {j}.",
                new int[1] { j }, Operations.Select));*/
            do
            {
                j--;
                Steps.Add(new Step($"Правый указатель сдвигаем влево, j--: {j}.",
                    new int[1] { j }, Operations.Select));

                Steps.Add(new Step($"Сравниваем элементы {j} ({collection[j]}) и pivot ({pivot}).",
                    new int[1] {j}, Operations.Select));
            } while (collection[j].CompareTo(pivot) > 0); // > заменить на < для сортировки по убыванию

            Steps.Add(new Step($"Правый указатель достиг цели (элемент {j} ({collection[j]}) <= pivot ({pivot}) - выполняется).",
                new int[1] { j }, Operations.Select));

            if (i >= j)
            {
                Steps.Add(new Step($"Разделитель для под массива array[{left}:{right}] найден.",
                    CreateIndexes(left, right), Operations.Select));
                Steps.Add(new Step($"Разделитель: {j}.",
                    new int[1] {j}, Operations.Select));

                return j;
            }

            Steps.Add(new Step($"{j} элемент ({collection[j]}) и {i} элемент ({collection[i]}), меняем их местами",
                new int[2] { j, i }, Operations.Switch));

            (collection[i], collection[j]) = (collection[j], collection[i]);
        }
    }

    public static void MergeSort<T>(this List<T> array) where T : IComparable
    {
        Steps.Add(new Step($"Начинается сортировка (Метод: Bubble Sort) массива длинной: {array.Count}.",
            Array.Empty<int>(), Operations.None, null));

        SplitAndMergeArray(array, 0, array.Count - 1);
        
        Steps.Add(new Step($"Конец сортировки.",
            Array.Empty<int>(), Operations.None, null));
    }
    
    private static void SplitAndMergeArray<T>(List<T> array, int left, int right) where T : IComparable
    {
        if (left < right)
        {
            int middle = left + (right - left) / 2;
            SplitAndMergeArray(array, left, middle);
            SplitAndMergeArray(array, middle + 1, right);
            
            Steps.Add(new Step($"Разделяем массив с {left} элемента по {right} элемент на временные подмассивы ",
            CreateIndexes(left, right), Operations.Select, null));

            MergeArray(array, left, middle, right);
        }
    }
    
    private static void MergeArray<T>(List<T> array, int left, int middle, int right) where T : IComparable
    {
        var leftArrayLength = middle - left + 1;
        var rightArrayLength = right - middle;
        var leftTempArray = new T[leftArrayLength];
        var rightTempArray = new T[rightArrayLength];
        int i, j;
        
        for (i = 0; i < leftArrayLength; ++i)
        {
            Value value = new(array[left + i].ToString(), array[left + i].ToString());
            Steps.Add(new Step($"Записываем {left + i} ({array[left + i]})элемент во временный массив",
            Array.Empty<int>(), Operations.WriteToTemp,
            new SwapDataInformation(value, i, left + i, 1)));

            leftTempArray[i] = array[left + i];
        }

        for (j = 0; j < rightArrayLength; ++j)
        {
            Value value = new(array[middle + 1 + j].ToString(), array[middle + 1 + j].ToString());
            Steps.Add(new Step($"Записываем {middle + 1 + j} ({array[middle + 1 + j]})элемент во временный массив",
            Array.Empty<int>(), Operations.WriteToTemp,
            new SwapDataInformation(value, j, middle + 1 + j, 2)));

            rightTempArray[j] = array[middle + 1 + j];
        }
        
        i = 0;
        j = 0;
        int k = left;
        
        while (i < leftArrayLength && j < rightArrayLength)
        {
            Steps.Add(new Step($"Сравниваем {i} элемент в левом подмассиве ({leftTempArray[i]}) и {j} элемент в правом подмассиве ({rightTempArray[j]})",
                Operations.SelectInTemp,
                new List<SelectInfo>() { new SelectInfo(1, i), new SelectInfo(2, j) }));
            if (leftTempArray[i].CompareTo(rightTempArray[j]) <= 0)
            {
                Value value = new(leftTempArray[i].ToString(), leftTempArray[i].ToString());
                Steps.Add(new Step($"присваиваем {k} элементу исходного массива значение ({leftTempArray[i]})",
                    Array.Empty<int>(), Operations.WriteFromTemp, 
                    new SwapDataInformation(value, k, i, 1)));
                array[k++] = leftTempArray[i++];
            }
            else
            {
                Value value = new(rightTempArray[j].ToString(), rightTempArray[j].ToString());
                Steps.Add(new Step($"присваиваем {k} элементу исходного массива значение ({rightTempArray[j]})",
                    Array.Empty<int>(), Operations.WriteFromTemp,
                    new SwapDataInformation(value, k, j, 2)));

                array[k++] = rightTempArray[j++];
            }
        }
        
        while (i < leftArrayLength)
        {
            Value value = new(leftTempArray[i].ToString(), leftTempArray[i].ToString());
            Steps.Add(new Step($"Присваиваем {k} элементу исходного массива {i} элемент левого подмассива ({leftTempArray[i]})",
                Array.Empty<int>(), Operations.WriteFromTemp,
                new SwapDataInformation(value, k, i, 1)));

            array[k++] = leftTempArray[i++];
        }
        
        while (j < rightArrayLength)
        {
            Value value = new(rightTempArray[j].ToString(), rightTempArray[j].ToString());
            Steps.Add(new Step($"Присваиваем {k} элементу исходного массива {j} элемент правого подмассива ({rightTempArray[j]})",
                Array.Empty<int>(), Operations.WriteFromTemp,
                new SwapDataInformation(value, k, j, 2)));

            array[k++] = rightTempArray[j++];
        }
    }
}