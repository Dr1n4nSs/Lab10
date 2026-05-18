using System;
using System.IO;

namespace Компилятор
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "source_test.pas";
            
            string[] mockCode = new string[] {
                "prog@ram Test;@@@",
                "var a: integer;@@@",
                "begin@@@",
                "  a := 10; @@@",
                "end."
            };
            
            File.WriteAllLines(path, mockCode);
            Console.WriteLine("=== Тестирование модуля ввода-вывода ===\n");
            
            InputOutput.Init(path);

            while (!InputOutput.IsEndOfFile)
            {
                if (InputOutput.Ch == '@')
                {
                    InputOutput.Error(1, InputOutput.PositionNow);
                }

                InputOutput.NextCh();
            }

            Console.ReadKey();
        }
    }
}