using System;
using System.IO;
using System.Collections.Generic;

namespace Компилятор
{
    class Program
    {
        static void Main(string[] args)
        {
            string path1 = "source_test1.pas";
            string path2 = "source_test2.pas";
            string path3 = "source_test3.pas";
            string resultPath = "codes.txt";
            
            Console.WriteLine("=== Тестирование Лексического анализатора ===\n");

            Console.WriteLine("Выберите тест (1, 2, 3): ");
            string option = Console.ReadLine();
            int cur_option = int.Parse(option);
            
            string selectedPath = "";

            switch (cur_option)
            {
                case 1: selectedPath = path1; break;
                case 2: selectedPath = path2; break;
                case 3: selectedPath = path3; break;
                default: 
                    Console.WriteLine("Заданного теста не существует"); 
                    return;
            }

            if (!File.Exists(selectedPath))
            {
                Console.WriteLine($"Ошибка: Файл {selectedPath} не найден.");
                Console.ReadKey();
                return;
            }

            InputOutput.Init(selectedPath);
            LexicalAnalyzer.Init();

            List<byte> tokenCodes = new List<byte>();

            while (!InputOutput.IsEndOfFile)
            {
                byte code = LexicalAnalyzer.Scan();
                if (code != 0)
                {
                    tokenCodes.Add(code);
                }
            }

            InputOutput.End();

            try
            {
                using (StreamWriter sw = new StreamWriter(resultPath))
                {
                    for (int i = 0; i < tokenCodes.Count; i++)
                    {
                        sw.Write(tokenCodes[i]);
                        if (i < tokenCodes.Count - 1)
                        {
                            sw.Write(" ");
                        }
                    }
                }
                Console.WriteLine($"\nКоды лексем успешно сохранены в файл: {resultPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи файла результатов: {ex.Message}");
            }
        }
    }
}