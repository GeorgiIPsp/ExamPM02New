using app.Classes;
using System.Threading.Channels;

class Program
{
    static int n;
    // Добавление комментария для проерки работоспособности коммитов
    static void Main(string[] args)
    {
        Console.WriteLine("=== Карта города Кольчугино ===");
        Console.WriteLine("Поиск минимального времени в пути\n");


        double[,] distanceMatrix = LoadDistancesFromFile();
        if (distanceMatrix == null)
        {
            Console.WriteLine("Не удалось загрузить данные. Программа завершена.");
            return;
        }


        double[,] speedMatrix = GenerateSpeeds();

        // Вывод информации о скоростях
        PrintSpeedInfo(distanceMatrix, speedMatrix);

        ProcessQueries(distanceMatrix, speedMatrix);

        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }




    /// <summary>
    /// Метод загрузки файла
    /// </summary>
    /// <returns></returns>
    static double[,] LoadDistancesFromFile()
    {
        string fileName = "map.txt";

        if (!File.Exists($"../../../{fileName}"))
        {
            Console.WriteLine($"Файл {fileName} не найден!");
            return null;
        }

        try
        {
            string[] lines = File.ReadAllLines($"../../../{fileName}");

            int maxPoint = 0;
            List<(int from, int to, double distance)> edges = new List<(int, int, double)>();

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                    continue;

                string[] parts = trimmedLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 3)
                {
                    int from = int.Parse(parts[0]);
                    int to = int.Parse(parts[1]);
                    double distance = double.Parse(parts[2]);

                    edges.Add((from, to, distance));

                    maxPoint = Math.Max(maxPoint, Math.Max(from, to));
                }
            }

            if (maxPoint == 0)
            {
                Console.WriteLine("Файл не содержит корректных данных!");
                return null;
            }

            n = maxPoint;
            Console.WriteLine($"Загружено {edges.Count} дорог между {n} точками");


            double[,] matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = double.MaxValue;
                }
                matrix[i, i] = 0;
            }


            foreach (var edge in edges)
            {
                int fromIndex = edge.from - 1;
                int toIndex = edge.to - 1;

                matrix[fromIndex, toIndex] = edge.distance;
                matrix[toIndex, fromIndex] = edge.distance;
            }

            return matrix;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            return null;
        }
    }





    /// <summary>
    /// Метод генерации рандомом скоростей
    /// </summary>
    /// <returns></returns>
    static double[,] GenerateSpeeds()
    {
        Random random = new Random();
        double[,] speeds = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                    speeds[i, j] = 0;
                else
                    speeds[i, j] = random.Next(30, 81); // от 30 до 80 км/ч
            }
        }

        return speeds;
    }
    /// <summary>
    /// Вывод информации о скоростях
    /// </summary>
    /// <param name="distances"></param>
    /// <param name="speeds"></param>
    static void PrintSpeedInfo(double[,] distances, double[,] speeds)
    {
        Console.WriteLine("\n=== Средние скорости на участках ===");

        int count = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (distances[i, j] != double.MaxValue)
                {
                    Console.WriteLine($"Путь {i + 1} <-> {j + 1}: {speeds[i, j]} км/ч");
                    count++;
                }
            }
        }
        Console.WriteLine($"Всего путей: {count}");
    }
    
    /// <summary>
    /// Основные операции с точками и маршрутами
    /// </summary>
    /// <param name="distanceMatrix"></param>
    /// <param name="speedMatrix"></param>
    static void ProcessQueries(double[,] distanceMatrix, double[,] speedMatrix)
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("Введите номера двух точек (Q для выхода):");

        while (true)
        {
            Console.Write("\nТочка 1: ");
            string input1 = Console.ReadLine();

            if (input1.ToUpper() == "Q")
            {
                Console.WriteLine("Выход из программы.");
                break;
            }

            Console.Write("Точка 2: ");
            string input2 = Console.ReadLine();

            if (input2.ToUpper() == "Q")
            {
                Console.WriteLine("Выход из программы.");
                break;
            }

            if (!int.TryParse(input1, out int point1))
            {
                Console.WriteLine("Ошибка: введите числа или Q для выхода!");
                continue;
            }
            if (!int.TryParse(input2, out int point2))
            {
                Console.WriteLine("Ошибка: введите числа или Q для выхода!");
                continue;
            }

            if (point1 < 1 || point1 > n || point2 < 1 || point2 > n)
            {
                Console.WriteLine($"Ошибка: точки должны быть от 1 до {n}!");
                continue;
            }

            if (point1 == point2)
            {
                Console.WriteLine("Точки должны быть разными!");
                continue;
            }


            double[,] timeMatrix = CreateTimeMatrix(distanceMatrix, speedMatrix);

            FindAndPrintShortestTime(point1 - 1, point2 - 1, distanceMatrix, speedMatrix, timeMatrix);
        }
    }

    /// <summary>
    /// Создание матрицы времени(определение времени прохждения на путях)
    /// </summary>
    /// <param name="distances"></param>
    /// <param name="speeds"></param>
    /// <returns></returns>
    static double[,] CreateTimeMatrix(double[,] distances, double[,] speeds)
    {
        double[,] times = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    times[i, j] = 0;
                }
                else if (distances[i, j] != double.MaxValue)
                {
                    // Время = расстояние / скорость (в часах)
                    times[i, j] = distances[i, j] / speeds[i, j];
                }
                else
                {
                    times[i, j] = double.MaxValue;
                }
            }
        }

        return times;
    }

    /// <summary>
    /// Поиск минимального времени прохождения пути и вывод данной инфрмации о путях и времени
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="distanceMatrix"></param>
    /// <param name="speedMatrix"></param>
    /// <param name="timeMatrix"></param>
    static void FindAndPrintShortestTime(int start, int end, double[,] distanceMatrix,
                                        double[,] speedMatrix, double[,] timeMatrix)
    {
        //Алгориттм декстры
        double[] minTimes = Dekstra.Dijkstra(timeMatrix, start);

        Console.WriteLine("\n------------------------------------");
        Console.WriteLine($"РЕЗУЛЬТАТ: {start + 1} -> {end + 1}");

        if (minTimes[end] == double.MaxValue)
        {
            Console.WriteLine("Путь не существует!");
            return;
        }


        List<int> path = ReconstructPath(timeMatrix, start, end, minTimes);

        // Вывод времени
        double totalTime = minTimes[end];
        Console.WriteLine($"Время в пути: {totalTime:F2} ч ({totalTime * 60:F1} мин)");

        // Вывод маршрута
        Console.Write("Маршрут: ");
        for (int i = 0; i < path.Count; i++)
        {
            Console.Write(path[i] + 1);
            if (i < path.Count - 1)
                Console.Write(" -> ");
        }
        Console.WriteLine();


        Console.WriteLine("\nДетали по участкам:");
        for (int i = 0; i < path.Count - 1; i++)
        {
            int from = path[i];
            int to = path[i + 1];

            double distance = distanceMatrix[from, to];
            double speed = speedMatrix[from, to];
            double time = timeMatrix[from, to];

            Console.WriteLine($"  {from + 1} -> {to + 1}: {distance} км / {speed} км/ч = {time:F2} ч");
        }
    }


    /// <summary>
    /// Восстановление историйи путей
    /// </summary>
    /// <param name="timeMatrix"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="distances"></param>
    /// <returns></returns>
    static List<int> ReconstructPath(double[,] timeMatrix, int start, int end, double[] distances)
    {
        List<int> path = new List<int>();


        if (distances[end] == double.MaxValue)
            return path;

        int current = end;
        path.Add(current);

        bool[] visited = new bool[n];

        while (current != start)
        {
            visited[current] = true;
            bool found = false;

            for (int i = 0; i < n; i++)
            {
                // Проверка на посещение вершиныы
                if (!visited[i] && timeMatrix[i, current] != double.MaxValue)
                {
                    // Проверк оптимальный ли данный путьы
                    if (Math.Abs(distances[current] - (distances[i] + timeMatrix[i, current])) < 0.001)
                    {
                        path.Insert(0, i);
                        current = i;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine("Ошибка: не удалось восстановить путь!");
                break;
            }

            if (path.Count > n * 2)
            {
                Console.WriteLine("Ошибка: слишком длинный путь (возможно, зацикливание)");
                break;
            }
        }

        return path;
    }
}