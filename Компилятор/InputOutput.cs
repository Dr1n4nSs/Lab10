using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    public static class InputOutput
    {
        private const byte ERRMAX = 9;
        
        private static Dictionary<char, (byte Code, string Desc)> _errorRules;

        private static char _ch;
        private static TextPosition _positionNow;
        private static string _line;
        private static int _lastInLine;
        private static List<Err> _err;
        private static List<Err> _allErrors;
        private static StreamReader _fileReader;
        private static uint _errCount;
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

        public static Dictionary<char, (byte Code, string Desc)> ErrorRules
        {
            get
            {
                return _errorRules;
            }
        }

        public static void Init(string filePath)
        {
            _positionNow = new TextPosition(0, 0);
            _errCount = 0;
            _isEndOfFile = false;
            _err = new List<Err>();
            _allErrors = new List<Err>();

            _errorRules = new Dictionary<char, (byte Code, string Desc)>();
            _errorRules.Add('@', (1, "Нахождение недопустимого символа '@'"));
            _errorRules.Add('#', (2, "Нахождение недопустимого символа '#'"));
            _errorRules.Add('^', (3, "Нахождение недопустимого символа '^'"));

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

            if (PositionNow.CharNumber >= _lastInLine)
            {
                ListThisLine();
                if (_err.Count > 0)
                {
                    ListErrors();
                }

                ReadNextLine();

                if (_isEndOfFile) return;

                _positionNow.LineNumber++;
                _positionNow.CharNumber = 0;
                _lastInLine = _line.Length - 1;
            }
            else
            {
                _positionNow.CharNumber++;
            }

            if (_line.Length > 0)
            {
                _ch = _line[_positionNow.CharNumber];
            }
            else
            {
                _ch = ' ';
            }
        }

        private static void ListThisLine()
        {
            Console.WriteLine($"{_positionNow.LineNumber, 4} | {_line}");
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
            Console.WriteLine($"Компиляция завершена. Всего ошибок: {_errCount}");
            Console.WriteLine("----------------------------------------");
            
            if (_allErrors.Count > 0)
            {
                int i = 0;
                Console.WriteLine("Список всех зарегистрированных ошибок:");
                foreach (Err error in _allErrors)
                {
                    Console.WriteLine($"**{i:D2}** Ошибка {error.ErrorCode} ({error.ErrorDescription}) " +
                                      $"в строке {error.ErrorPosition.LineNumber} " +
                                      $"на позиции {error.ErrorPosition.CharNumber}");
                    i++;
                }
                Console.WriteLine("----------------------------------------");
            }
        }

        private static void ListErrors()
        {
            string prefix = "";
            string arrows = "";
            int spacesCount = 0;
            
            foreach (Err item in _err)
            {
                _errCount++;
                prefix = $"**{_errCount - 1 :D2}**";
                spacesCount = 7 + (int)item.ErrorPosition.CharNumber;
                
                arrows = "";
                for (int i = 0; i < spacesCount; i++)
                {
                    arrows += " ";
                }

                Console.WriteLine($"{prefix} {arrows}^ ошибка с кодом {item.ErrorCode}");
            }
            _err.Clear();
        }

        public static void Error(byte errorCode, TextPosition position, string description)
        {
            if (_allErrors.Count < ERRMAX)
            {
                Err e = new Err(position, errorCode, description);
                _err.Add(e);
                _allErrors.Add(e); 
            }
        }
    }
}
