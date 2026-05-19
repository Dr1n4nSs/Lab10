using System;
using System.IO;

namespace Компилятор
{
    class Program
    {
        static void Main(string[] args)
        {
            string path1 = "source_test1.pas";
            string path2 = "source_test2.pas";
            string path3 = "source_test3.pas";
            
            Console.WriteLine("=== Тестирование модуля ввода-вывода ===\n");

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

            InputOutput.Init(selectedPath);
            
            while (!InputOutput.IsEndOfFile)
            {
                foreach (char triggerChar in InputOutput.ErrorRules.Keys)
                {
                    if (InputOutput.Ch == triggerChar)
                    {
                        var rule = InputOutput.ErrorRules[triggerChar];
                        InputOutput.Error(rule.Code, InputOutput.PositionNow, rule.Desc);
                        break; 
                    }
                }

                InputOutput.NextCh();
            }
        }
    }
}
