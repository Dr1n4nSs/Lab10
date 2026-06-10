using System;
using System.Collections.Generic;
using System.IO;

namespace Компилятор
{
    public static class InputOutput
    {
        private const byte ERRMAX = 50;
        
        private static Dictionary<byte, string> _errorRules;
        private static char _ch;
        private static TextPosition _positionNow;
        private static string _line;
        private static int _lastInLine;
        private static List<Err> _err;
        private static List<Err> _allErrors;
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
            _errorRules.Add(5, "Ошибка: Открытая фигурная скобка '{' не закрыта до конца файла");
            _errorRules.Add(6, "Ошибка: Одиночная закрывающая фигурная скобка '}' без открывающей");
            _errorRules.Add(7, "Ошибка: Комментарий '(*' не закрыт до конца файла");
            _errorRules.Add(8, "Ошибка: Одиночный закрывающий символ комментария '*)' без открывающего");
            _errorRules.Add(9, "Ошибка: Одиночная закрывающая круглая скобка ')' без открывающей");
            _errorRules.Add(11, "Ошибка: Одиночная открывающая круглая скобка '(' без закрывающей");
            _errorRules.Add(12, "Ошибка: Символьная строка не закрыта до конца строки");
            
            _errorRules.Add(50, "Синтаксическая ошибка: Ожидался идентификатор (имя программы)");
            _errorRules.Add(51, "Синтаксическая ошибка: Ожидался символ ';'");
            _errorRules.Add(52, "Синтаксическая ошибка: Ожидалась точка '.' в конце программы");
            _errorRules.Add(53, "Синтаксическая ошибка: Некорректный список переменных в var");
            _errorRules.Add(54, "Синтаксическая ошибка: Ожидался символ двоеточия ':'");
            _errorRules.Add(55, "Синтаксическая ошибка: Неизвестный или недопустимый тип данных");
            _errorRules.Add(56, "Синтаксическая ошибка: Ожидался идентификатор в описании функции");
            _errorRules.Add(57, "Синтаксическая ошибка: Ожидалась закрывающая скобка ')' параметров");
            _errorRules.Add(58, "Синтаксическая ошибка: Ожидалось ключевое слово 'begin'");
            _errorRules.Add(59, "Синтаксическая ошибка: Ожидалось ключевое слово 'end'");
            _errorRules.Add(60, "Синтаксическая ошибка: Ожидался оператор присваивания ':='");
            _errorRules.Add(61, "Синтаксическая ошибка: Ошибка в выражении (ожидался операнд)");
            _errorRules.Add(62, "Синтаксическая ошибка: Секция 'const' должна идти строго до секции 'var'");
            _errorRules.Add(63, "Синтаксическая ошибка: Ожидался знак равенства '=' при объявлении константы");
            _errorRules.Add(64, "Синтаксическая ошибка: Недопустимое значение константы (ожидалось число, строка или имя)");
            _errorRules.Add(65, "Синтаксическая ошибка: Ожидалась открывающая скобка '(' в описании перечисления (enumerate)");
            _errorRules.Add(66, "Синтаксическая ошибка: Ожидался идентификатор внутри перечисления (enumerate)");
            _errorRules.Add(67, "Синтаксическая ошибка: Ожидалась закрывающая скобка ')' в описании перечисления (enumerate)");
            _errorRules.Add(68, "Синтаксическая ошибка: Некорректный формат диапазона (range), ожидалось: range [мин : макс]");
            _errorRules.Add(69, "Синтаксическая ошибка: Некорректное ограничение длины строки, ожидалось: string[число]");
            _errorRules.Add(70, "Синтаксическая ошибка: Недопустимый формат константы типа range (ожидалось [мин : макс])");
            _errorRules.Add(71, "Синтаксическая ошибка: Недопустимый формат константы типа enumerate (ожидалось (id1, id2))");
            _errorRules.Add(72, "Синтаксическая ошибка: Минимальное значение диапазона не может быть больше максимального");
            _errorRules.Add(73, "Синтаксическая ошибка: Ожидался оператор диапазона '..'");
            _errorRules.Add(80, "Семантическая ошибка: Идентификатор с таким именем уже объявлен в текущей области видимости");
            _errorRules.Add(81, "Семантическая ошибка: Использование необъявленного идентификатора");
            _errorRules.Add(82, "Семантическая ошибка: Несоответствие типов при присваивании");
            _errorRules.Add(83, "Семантическая ошибка: Ожидалась переменная в левой части оператора присваивания");
            _errorRules.Add(84, "Семантическая ошибка: Несовместимые типы операндов в выражении");

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream) _fileLines.Add(sr.ReadLine() + " ");
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
            if (_isEndOfFile)
            {
                return;
            }

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
            Console.WriteLine($"Синтаксический анализ завершен. Всего зарегистрировано ошибок: {_allErrors.Count}");
            Console.WriteLine("----------------------------------------");
            
            if (_allErrors.Count > 0)
            {
                int i = 0;
                Console.WriteLine("Список всех зарегистрированных синтаксических и лексических ошибок:");
                foreach (Err error in _allErrors)
                {
                    Console.WriteLine($"**{i:D2}** Код {error.ErrorCode} ({error.ErrorDescription}) " +
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
                for (int i = 0; i < spacesCount; i++) arrows += " ";

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