using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class LexicalAnalyzer
    {
        private static Dictionary<string, byte> _keywords;
        private static Dictionary<string, byte> _tokenCodes;
        private static int _roundBracketBalance; // Баланс для контроля одиночных закрывающих скобок

        private const int MAX_PASCAL_INT = 32767;
        private const int MIN_PASCAL_INT = -32768;

        public static void Init()
        {
            _keywords = new Dictionary<string, byte>();
            _keywords.Add("program", 3);
            _keywords.Add("var", 3);
            _keywords.Add("const", 3);
            _keywords.Add("begin", 3);
            _keywords.Add("end", 3);
            _keywords.Add("integer", 3);

            _tokenCodes = new Dictionary<string, byte>();
            _tokenCodes.Add("identifier", 100); 
            _tokenCodes.Add("number", 200);     
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
            _tokenCodes.Add(":=", 99);          

            _roundBracketBalance = 0;
        }

        /// <summary>
        /// Виртуальный просмотр сохраненных строк вперед без нарушения позиции чтения файла.
        /// </summary>
        private static bool LookaheadForClosing(string openType)
        {
            uint rLine = InputOutput.PositionNow.LineNumber;
            int rChar = InputOutput.PositionNow.CharNumber;

            // Пропускаем саму открывающую скобку/символ
            if (openType == "(*") rChar += 2;
            else rChar += 1;

            while (rLine < InputOutput.FileLines.Count)
            {
                string line = InputOutput.FileLines[(int)rLine];
                while (rChar < line.Length)
                {
                    char ch = line[rChar];

                    if (openType == "{")
                    {
                        if (ch == '}') return true;
                    }
                    else if (openType == "(*")
                    {
                        if (ch == '*' && rChar + 1 < line.Length && line[rChar + 1] == ')') return true;
                    }
                    else if (openType == "(")
                    {
                        if (ch == ')') return true;
                    }

                    rChar++;
                }
                rLine++;
                rChar = 0;
            }

            return false;
        }

        public static byte Scan()
        {
            while (!InputOutput.IsEndOfFile)
            {
                if (InputOutput.Ch == ' ' || InputOutput.Ch == '\r' || InputOutput.Ch == '\n' || InputOutput.Ch == '\t')
                {
                    InputOutput.NextCh();
                    continue;
                }

                // 1. Однострочный комментарий '//'
                if (InputOutput.Ch == '/')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '/')
                    {
                        uint currentLine = pos.LineNumber;
                        while (!InputOutput.IsEndOfFile && InputOutput.PositionNow.LineNumber == currentLine)
                        {
                            InputOutput.NextCh();
                        }
                        continue;
                    }
                    else
                    {
                        if (_tokenCodes.ContainsKey("/")) return _tokenCodes["/"];
                    }
                }

                // 2. Многострочный комментарий '{ }'
                if (InputOutput.Ch == '{')
                {
                    TextPosition commentStart = InputOutput.PositionNow;
                    
                    if (!LookaheadForClosing("{"))
                    {
                        InputOutput.Error(5, commentStart);
                        InputOutput.ListErrors(); // Выводим ТУТ ЖЕ под текущей строкой листинга
                        InputOutput.NextCh();
                        continue;
                    }

                    InputOutput.NextCh();
                    while (!InputOutput.IsEndOfFile && InputOutput.Ch != '}')
                    {
                        InputOutput.NextCh();
                    }
                    InputOutput.NextCh(); 
                    continue;
                }

                if (InputOutput.Ch == '}')
                {
                    InputOutput.Error(6, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    InputOutput.NextCh();
                    continue;
                }

                // 3. Круглые скобки и комментарии '(* *)'
                if (InputOutput.Ch == '(')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    
                    if (InputOutput.Ch == '*')
                    {
                        TextPosition commentStart = pos;

                        if (!LookaheadForClosing("(*"))
                        {
                            InputOutput.Error(7, commentStart);
                            InputOutput.ListErrors(); // Мгновенный вывод под строкой
                            InputOutput.NextCh();
                            continue;
                        }

                        InputOutput.NextCh();
                        while (!InputOutput.IsEndOfFile)
                        {
                            if (InputOutput.Ch == '*')
                            {
                                InputOutput.NextCh();
                                if (InputOutput.Ch == ')')
                                {
                                    InputOutput.NextCh();
                                    break;
                                }
                            }
                            else
                            {
                                InputOutput.NextCh();
                            }
                        }
                        continue;
                    }
                    else
                    {
                        // Обычная открывающая скобка '('
                        if (!LookaheadForClosing("("))
                        {
                            InputOutput.Error(11, pos);
                            InputOutput.ListErrors(); // Мгновенный вывод под строкой
                            if (_tokenCodes.ContainsKey("(")) return _tokenCodes["("];
                        }

                        _roundBracketBalance++;
                        if (_tokenCodes.ContainsKey("(")) return _tokenCodes["("];
                    }
                }

                if (InputOutput.Ch == '*')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == ')')
                    {
                        InputOutput.Error(8, pos); 
                        InputOutput.ListErrors();
                        InputOutput.NextCh();
                        continue;
                    }
                    else
                    {
                        if (_tokenCodes.ContainsKey("*")) return _tokenCodes["*"];
                    }
                }

                break;
            }

            if (InputOutput.IsEndOfFile) return 0;

            // --- ОБРАБОТКА ИДЕНТИФИКАТОРОВ И ЧИСЕЛ ---

            if (char.IsLetter(InputOutput.Ch))
            {
                string word = "";
                while (!InputOutput.IsEndOfFile && char.IsLetterOrDigit(InputOutput.Ch))
                {
                    word += InputOutput.Ch;
                    InputOutput.NextCh();
                }
                word = word.ToLower();

                if (_keywords.ContainsKey(word)) return _keywords[word];
                if (_tokenCodes.ContainsKey("identifier")) return _tokenCodes["identifier"];
                return 0;
            }

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

            // --- ЗНАКИ ОПЕРАЦИЙ ---
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

            if (currentCh == ')')
            {
                _roundBracketBalance--;
                
                // Если баланс ушел в минус — значит, встретилась одиночная закрывающая скобка ) без пары!
                if (_roundBracketBalance < 0)
                {
                    InputOutput.Error(9, InputOutput.PositionNow);
                    InputOutput.ListErrors(); // Выводим ошибку СТРОГО под текущим знаком )
                    _roundBracketBalance = 0; // Сбрасываем баланс для дальнейшего разбора
                }

                if (_tokenCodes.ContainsKey(")"))
                {
                    byte code = _tokenCodes[")"];
                    InputOutput.NextCh();
                    return code;
                }
            }

            if (currentCh == '@') { InputOutput.Error(1, InputOutput.PositionNow); InputOutput.ListErrors(); InputOutput.NextCh(); return 0; }
            if (currentCh == '#') { InputOutput.Error(2, InputOutput.PositionNow); InputOutput.ListErrors(); InputOutput.NextCh(); return 0; }
            if (currentCh == '^') { InputOutput.Error(3, InputOutput.PositionNow); InputOutput.ListErrors(); InputOutput.NextCh(); return 0; }

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