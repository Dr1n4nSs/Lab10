   using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    public struct TextPosition
    {
        public uint lineNumber;
        public byte charNumber;

        public TextPosition(uint ln = 0, byte c = 0)
        {
            lineNumber = ln;
            charNumber = c;
        }
    }

    public struct Err
    {
        public TextPosition errorPosition;
        public byte errorCode;
        public string errorDescription;

        public Err(TextPosition errorPosition, byte errorCode, string errorDescription)
        {
            this.errorPosition = errorPosition;
            this.errorCode = errorCode;
            this.errorDescription = errorDescription;
        }
    }

    public static class InputOutput
    {
        private const byte ERRMAX = 9;
        
        private static Dictionary<byte, string> _errorRules;
        private static char _ch;
        private static TextPosition _positionNow;
        private static string _line;
        private static int _lastInLine;
        private static List<Err> _err;
        private static List<Err> _allErrors;
        private static StreamReader _fileReader;
        private static uint _errCount = 0;
        private static bool _isEndOfFile;
        
        public static char Ch
        {
            get { return _ch; }
        }

        public static TextPosition PositionNow
        {
            get { return _positionNow; }
        }

        public static bool IsEndOfFile
        {
            get { return _isEndOfFile; }
        }

        public static List<Err> ErrList
        {
            get { return _err; }
        }

        public static void Init(string filePath)
        {
            _positionNow = new TextPosition(0, 0);
            _errCount = 0;
            _isEndOfFile = false;
            _err = new List<Err>();
            _allErrors = new List<Err>();

            _errorRules = new Dictionary<byte, string>();
            _errorRules.Add(4, "Выход целого числа за пределы допустимого диапазона [-32768..32767]");
            _errorRules.Add(5, "Ошибка: Открытая фигурная скобка '{' не закрыта до конца файла");
            _errorRules.Add(6, "Ошибка: Одиночная закрывающая фигурная скобка '}' без открывающей");
            _errorRules.Add(7, "Ошибка: Комментарий '(*' не закрыт до конца файла");
            _errorRules.Add(8, "Ошибка: Одиночный закрывающий символ комментария '*)' без открывающего");

            try
            {
                string directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath)) { } 
                }

                _fileReader = new StreamReader(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException($"Не удалось подготовить файл или директорию: {ex.Message}", ex);
            }

            ReadNextLine();

            if (_line.Length > 0)
            {
                _ch = _line[0];
                _lastInLine = _line.Length - 1;
            }
            else
            {
                _ch = ' ';
            }
        }

        public static void NextCh()
        {
            if (_isEndOfFile) return;

            if (_positionNow.charNumber >= _lastInLine)
            {
                ListThisLine();
                if (_err.Count > 0)
                {
                    ListErrors();
                }

                ReadNextLine();

                if (_isEndOfFile) return;

                _positionNow.lineNumber++;
                _positionNow.charNumber = 0;
                _lastInLine = _line.Length - 1;
            }
            else
            {
                _positionNow.charNumber++;
            }

            if (_line.Length > 0)
            {
                _ch = _line[_positionNow.charNumber];
            }
            else
            {
                _ch = ' ';
            }
        }

        private static void ListThisLine()
        {
            Console.WriteLine($"{_positionNow.lineNumber + 1, 4} | {_line}");
        }

        private static void ReadNextLine()
        {
            if (!_fileReader.EndOfStream)
            {
                _line = _fileReader.ReadLine();
                _line += " "; 
                _err = new List<Err>();
            }
            else
            {
                End();
            }
        }

        private static void End()
        {
            _isEndOfFile = true;
            _ch = '\0';
            _fileReader.Close();
            
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine($"Компиляция завершена. Всего ошибок выведено на листинг: {_errCount}");
            Console.WriteLine("----------------------------------------");
            
            if (_allErrors.Count > 0)
            {
                int i = 0;
                Console.WriteLine("Список всех зарегистрированных ошибок:");
                foreach (Err error in _allErrors)
                {
                    Console.WriteLine($"**{i:D2}** Ошибка {error.errorCode} ({error.errorDescription}) " +
                                      $"в строке {error.errorPosition.lineNumber + 1} " +
                                      $"на позиции {error.errorPosition.charNumber + 1}");
                    i++;
                }
                Console.WriteLine("----------------------------------------");
            }
        }

        private static void ListErrors()
        {
            foreach (Err item in _err)
            {
                _errCount++;
                string prefix = $"**{_errCount:D2}**";
                int spacesCount = 7 + (int)item.errorPosition.charNumber;
                
                string arrows = "";
                for (int i = 0; i < spacesCount; i++)
                {
                    arrows += " ";
                }

                Console.WriteLine($"{prefix} {arrows}^ ошибка с кодом {item.errorCode}");
            }
            _err.Clear();
        }

        public static void Error(byte errorCode, TextPosition position)
        {
            if (_allErrors.Count < ERRMAX)
            {
                string description = "Неизвестная ошибка";
                if (_errorRules.ContainsKey(errorCode))
                {
                    description = _errorRules[errorCode];
                }

                Err e = new Err(position, errorCode, description);
                _err.Add(e);
                _allErrors.Add(e); 
            }
        }
    }
}                 
