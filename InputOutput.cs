using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{

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
        private static List<string> _fileLines; 
        private static uint _errCount = 0;
        private static bool _isEndOfFile;

        public static char Ch
        {
            get
            {
                return _ch;
            }
        }

        public static TextPosition PositionNow
        {
            get
            {
                return _positionNow;
            }
        }

        public static bool IsEndOfFile
        {
            get
            {
                return _isEndOfFile;
            }
        }

        public static List<Err> ErrList
        {
            get
            {
                return _err;
            }
        }

        public static List<string> FileLines
        {
            get
            {
                return _fileLines;
            }
        }

        public static void Init(string filePath)
        {
            _positionNow = new TextPosition(0, 0);
            _errCount = 0;
            _isEndOfFile = false;
            _err = new List<Err>();
            _allErrors = new List<Err>();
            _fileLines = new List<string>();

            _errorRules = new Dictionary<byte, string>();
            _errorRules.Add(1, "Нахождение недопустимого символа '@'");
            _errorRules.Add(2, "Нахождение недопустимого символа '#'");
            _errorRules.Add(3, "Нахождение недопустимого символа '^'");
            _errorRules.Add(4, "Выход целого числа за пределы допустимого диапазона [-32768..32767]");
            _errorRules.Add(5, "Открытая фигурная скобка '{' не закрыта до конца файла");
            _errorRules.Add(6, "Одиночная закрывающая фигурная скобка '}' без открывающей");
            _errorRules.Add(7, "Комментарий '(*' не закрыт до конца файла");
            _errorRules.Add(8, "Одиночный закрывающий символ комментария '*)' без открывающего");
            _errorRules.Add(9, "Одиночная закрывающая круглая скобка ')' без открывающей");
            _errorRules.Add(11, "Одиночная открывающая круглая скобка '(' без закрывающей");
            _errorRules.Add(12, "Символьная строка не закрыта до конца текущей строки");

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream)
                    {
                        _fileLines.Add(sr.ReadLine() + " ");
                    }
                }
                if (_fileLines.Count == 0)
                {
                    _fileLines.Add(" ");
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Не удалось прочитать файл: {ex.Message}", ex);
            }

            _line = _fileLines[0];
            _ch = _line[0];
            _lastInLine = _line.Length - 1;
            
            ListThisLine();
        }

        public static void NextCh()
        {
            if (_isEndOfFile) return;

            if (_positionNow.CharNumber >= _lastInLine)
            {
                if (_err.Count > 0)
                {
                    ListErrors();
                }

                _positionNow.LineNumber++;
                if (_positionNow.LineNumber < _fileLines.Count)
                {
                    _line = _fileLines[(int)_positionNow.LineNumber];
                    _positionNow.CharNumber = 0;
                    _lastInLine = _line.Length - 1;
                    ListThisLine();
                }
                else
                {
                    _isEndOfFile = true;
                    _ch = '\0';
                    return;
                }
            }
            else
            {
                _positionNow.CharNumber++;
            }

            _ch = _line[_positionNow.CharNumber];
        }

        private static void ListThisLine()
        {
            Console.WriteLine($"{_positionNow.LineNumber + 1, 4} | {_line}");
        }

        public static void End()
        {
            if (_err.Count > 0)
            {
                ListErrors();
            }
            
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine($"Компиляция завершена. Всего ошибок выведено на листинг: {_errCount}");
            Console.WriteLine("----------------------------------------");
            
            if (_allErrors.Count > 0)
            {
                int i = 0;
                Console.WriteLine("Список всех зарегистрированных ошибок:");
                foreach (Err error in _allErrors)
                {
                    Console.WriteLine($"**{i:D2}** Ошибка {error.ErrorCode} ({error.ErrorDescription}) " +
                                      $"в строке {error.ErrorPosition.LineNumber + 1} " +
                                      $"на позиции {error.ErrorPosition.CharNumber + 1}");
                    i++;
                }
                Console.WriteLine("----------------------------------------");
            }
        }

        public static void ListErrors()
        {
            foreach (Err item in _err)
            {
                _errCount++;
                string prefix = $"**{_errCount:D2}**";
                int spacesCount = (int)item.ErrorPosition.CharNumber;
                
                string arrows = "";
                for (int i = 0; i < spacesCount; i++)
                {
                    arrows += " ";
                }

                Console.WriteLine($"{prefix} {arrows}^ ошибка с кодом {item.ErrorCode}");
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
