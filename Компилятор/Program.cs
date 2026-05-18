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

            Console.WriteLine("Выберите тест: ");
            string option = Console.ReadLine();;
            int cur_option = int.Parse(option);
            
            switch (cur_option)
            {
                case 1: 
                    InputOutput.Init(path1);
                    while (!InputOutput.IsEndOfFile)
                    {
                        if (InputOutput.Ch == '@')
                        {
                            InputOutput.Error(1, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '#')
                        {
                            InputOutput.Error(2, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '^')
                        {
                            InputOutput.Error(3, InputOutput.PositionNow);
                        }

                        InputOutput.NextCh();
                    }
                    break;
                case 2: InputOutput.Init(path2); 
                    while (!InputOutput.IsEndOfFile)
                    {
                        if (InputOutput.Ch == '@')
                        {
                            InputOutput.Error(1, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '#')
                        {
                            InputOutput.Error(2, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '^')
                        {
                            InputOutput.Error(3, InputOutput.PositionNow);
                        }

                        InputOutput.NextCh();
                    }
                    break;
                case 3: InputOutput.Init(path3); 
                    while (!InputOutput.IsEndOfFile)
                    {
                        if (InputOutput.Ch == '@')
                        {
                            InputOutput.Error(1, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '#')
                        {
                            InputOutput.Error(2, InputOutput.PositionNow);
                        }
                        if (InputOutput.Ch == '^')
                        {
                            InputOutput.Error(3, InputOutput.PositionNow);
                        }

                        InputOutput.NextCh();
                    }
                    break;
                default: Console.WriteLine("Заданного теста не существует"); break;
            }
            
        }
    }
}
