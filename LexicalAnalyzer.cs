using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class LexicalAnalyzer
    {
        private static Dictionary<string, byte> _keywords;
        private static Dictionary<string, byte> _tokenCodes; // Единый словарь для кодов всех лексем
        
        private const int MAX_PASCAL_INT = 32767;
        private const int MIN_PASCAL_INT = -32768;

        public static void Init()
        {
            // 1. Справочник ключевых слов
            _keywords = new Dictionary<string, byte>();
            _keywords.Add("program", 3);
            _keywords.Add("var", 3);
            _keywords.Add("const", 3);
            _keywords.Add("begin", 3);
            _keywords.Add("end", 3);
            _keywords.Add("integer", 3);

            // 2. Все коды символов и лексем теперь берутся из этого словаря
            _tokenCodes = new Dictionary<string, byte>();
            _tokenCodes.Add("identifier", 100); // Код для пользовательских идентификаторов
            _tokenCodes.Add("number", 200);     // Код для целых чисел
            
            // Коды символов пунктуации и операций (из ASCII или по вашей спецификации)
            _tokenCodes.Add("*", 21);
            _tokenCodes.Add("/", 60);
            _tokenCodes.Add("=", 16);
            _tokenCodes.Add(",", 20);
            _tokenCodes.Add(";", 14);
            _tokenCodes.Add(":", 5);
            _tokenCodes.Add(".", 61);
            _tokenCodes.Add(">", 62);
            _tokenCodes.Add("(", 40);
            _tokenCodes.Add(")", 41);
            _tokenCodes.Add(":=", 99);          // Код для составного оператора присваивания
        }

        public static byte Scan()
        {
            while (!InputOutput.IsEndOfFile)
            {
                // Пропускаем пробельные символы
                if (InputOutput.Ch == ' ' || InputOutput.Ch == '\r' || InputOutput.Ch == '\n' || InputOutput.Ch == '\t')
                {
                    InputOutput.NextCh();
                    continue;
                }

                // --- АНАЛИЗ ВСЕХ ВИДОВ КОММЕНТАРИЕВ ---

                // 1. Однострочный комментарий '//'
                if (InputOutput.Ch == '/')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '/')
                    {
                        // Обнаружен '//', считываем всё до конца текущей строки
                        uint currentLine = pos.lineNumber;
                        while (!InputOutput.IsEndOfFile && InputOutput.PositionNow.lineNumber == currentLine)
                        {
                            InputOutput.NextCh();
                        }
                        continue;
                    }
                    else
                    {
                        // Это не комментарий, а одиночный слэш. Возвращаем позицию назад логически
                        // Так как мы уже вызвали NextCh(), обработаем текущий слэш как знак деления ниже
                        if (_tokenCodes.ContainsKey("/")) return _tokenCodes["/"];
                    }
                }

                // 2. Многострочный комментарий '{ }'
                if (InputOutput.Ch == '{')
                {
                    TextPosition commentStart = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    
                    while (!InputOutput.IsEndOfFile && InputOutput.Ch != '}')
                    {
                        InputOutput.NextCh();
                    }

                    if (InputOutput.IsEndOfFile)
                    {
                        InputOutput.Error(5, commentStart); // Не закрыт '{'
                        return 0;
                    }

                    InputOutput.NextCh(); // Пропускаем '}'
                    continue;
                }

                // Ошибка: встретили '}' без открывающей '{'
                if (InputOutput.Ch == '}')
                {
                    InputOutput.Error(6, InputOutput.PositionNow);
                    InputOutput.NextCh();
                    continue;
                }

                // 3. Многострочный комментарий '(* *)'
                if (InputOutput.Ch == '(')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '*')
                    {
                        // Обнаружено начало комментария '(*'
                        TextPosition commentStart = pos;
                        InputOutput.NextCh(); // Шаг за '*'
                        
                        bool closed = false;
                        while (!InputOutput.IsEndOfFile)
                        {
                            if (InputOutput.Ch == '*')
                            {
                                InputOutput.NextCh();
                                if (InputOutput.Ch == ')')
                                {
                                    InputOutput.NextCh();
                                    closed = true;
                                    break;
                                }
                            }
                            else
                            {
                                InputOutput.NextCh();
                            }
                        }

                        if (!closed)
                        {
                            InputOutput.Error(7, commentStart); // Не закрыт '(*'
                            return 0;
                        }
                        continue;
                    }
                    else
                    {
                        // Это была просто открывающая круглая скобка '('
                        if (_tokenCodes.ContainsKey("(")) return _tokenCodes["("];
                    }
                }

                // Ошибка: встретили '*)' без открывающего '(*'
                if (InputOutput.Ch == '*')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == ')')
                    {
                        InputOutput.Error(8, pos); // Одиночный '*)'
                        InputOutput.NextCh();
                        continue;
                    }
                    else
                    {
                        // Это просто одиночная звездочка '*'
                        if (_tokenCodes.ContainsKey("*")) return _tokenCodes["*"];
                    }
                }

                break;
            }

            if (InputOutput.IsEndOfFile) return 0;

            // --- ОБРАБОТКА ЛЕКСЕМ ---

            // 1. ИДЕНТИФИКАТОРЫ И КЛЮЧЕВЫЕ СЛОВА
            if (char.IsLetter(InputOutput.Ch))
            {
                string word = "";
                while (!InputOutput.IsEndOfFile && char.IsLetterOrDigit(InputOutput.Ch))
                {
                    word += InputOutput.Ch;
                    InputOutput.NextCh();
                }

                word = word.ToLower();

                // Если это ключевое слово — у него свой код из таблицы ключевых слов
                if (_keywords.ContainsKey(word))
                {
                    return _keywords[word];
                }

                // Иначе — это пользовательский идентификатор (код берем из словаря)
                if (_tokenCodes.ContainsKey("identifier")) return _tokenCodes["identifier"];
                return 0;
            }

            // 2. ЦЕЛОЧИСЛЕННЫЕ ЗНАЧЕНИЯ
            if (char.IsDigit(InputOutput.Ch))
            {
                string numStr = "";
                TextPosition startPos = InputOutput.PositionNow;

                while (!InputOutput.IsEndOfFile && char.IsDigit(InputOutput.Ch))
                {
                    numStr += InputOutput.Ch;
                    InputOutput.NextCh();
                }

                long longValue;
                if (long.TryParse(numStr, out longValue))
                {
                    if (longValue < MIN_PASCAL_INT || longValue > MAX_PASCAL_INT)
                    {
                        InputOutput.Error(4, startPos); 
                    }
                }
                else
                {
                    InputOutput.Error(4, startPos);
                }

                if (_tokenCodes.ContainsKey("number")) return _tokenCodes["number"];
                return 0;
            }

            // 3. ЗНАКИ ОПЕРАЦИЙ И СИМВОЛЫ ИЗ СЛОВАРЯ
            char currentCh = InputOutput.Ch;

            if (currentCh == ':')
            {
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    InputOutput.NextCh();
                    if (_tokenCodes.ContainsKey(":=")) return _tokenCodes[":="];
                }
                if (_tokenCodes.ContainsKey(":")) return _tokenCodes[":"];
            }

            // Для всех базовых одиночных символов
            string chStr = currentCh.ToString();
            if (_tokenCodes.ContainsKey(chStr))
            {
                byte code = _tokenCodes[chStr];
                InputOutput.NextCh();
                return code;
            }

            InputOutput.NextCh();
            return 0;
        }
    }
}
