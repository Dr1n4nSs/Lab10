using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class LexicalAnalyzer
    {
        private static Dictionary<string, byte> _keywords;
        private static Dictionary<string, byte> _tokenCodes;
        private static int _roundBracketBalance;
        private static int _lastNumericValue; 
        private static string _lastIdentName = "";
        public static string LastIdentName { get { return _lastIdentName; } }

        private const int MAX_PASCAL_INT = 32767;
        private const int MIN_PASCAL_INT = -32768;

        public static int LastNumericValue { get { return _lastNumericValue; } }

        public static byte GetTokenCode(string key)
        {
            if (_tokenCodes.ContainsKey(key)) return _tokenCodes[key];
            if (_keywords.ContainsKey(key)) return _keywords[key];
            return 0;
        }

        public static void Init()
        {
            _keywords = new Dictionary<string, byte>();
            _keywords.Add("program", 3);
            _keywords.Add("var", 4);
            _keywords.Add("const", 5);
            _keywords.Add("begin", 6);
            _keywords.Add("end", 7);
            _keywords.Add("function", 8);
            
            _keywords.Add("integer", 11);
            _keywords.Add("real", 12);
            _keywords.Add("boolean", 13);
            _keywords.Add("char", 14);
            _keywords.Add("string", 15);
            _keywords.Add("enumerate", 16); 
            _keywords.Add("range", 17);     

            _tokenCodes = new Dictionary<string, byte>();
            _tokenCodes.Add("identifier", 100); 
            _tokenCodes.Add("number", 200);     
            _tokenCodes.Add("string_literal", 201); 
            
            _tokenCodes.Add("*", 21);
            _tokenCodes.Add("/", 60);
            _tokenCodes.Add("+", 22);
            _tokenCodes.Add("-", 23);
            _tokenCodes.Add("=", 16);
            _tokenCodes.Add(",", 20);
            _tokenCodes.Add(";", 14);
            _tokenCodes.Add(":", 5);
            _tokenCodes.Add(".", 61);
            _tokenCodes.Add("..", 64); // Токен диапазона Паскаля
            _tokenCodes.Add(">", 62);
            _tokenCodes.Add("<", 63);
            _tokenCodes.Add("(", 40);
            _tokenCodes.Add(")", 41);
            _tokenCodes.Add("[", 42); // Для строк и старых квадратных скобок
            _tokenCodes.Add("]", 43);
            _tokenCodes.Add(":=", 99);          

            _roundBracketBalance = 0;
            _lastNumericValue = 0;
        }

        private static bool LookaheadForClosing(string openType)
        {
            uint rLine = InputOutput.PositionNow.LineNumber;
            int rChar = InputOutput.PositionNow.CharNumber;

            if (openType == "(*") rChar += 2;
            else rChar += 1;

            while (rLine < InputOutput.FileLines.Count)
            {
                string line = InputOutput.FileLines[(int)rLine];
                if (openType == "'")
                {
                    while (rChar < line.Length - 1)
                    {
                        if (line[rChar] == '\'')
                        {
                            return true;
                        }
                        rChar++;
                    }
                    return false;
                }

                while (rChar < line.Length)
                {
                    char ch = line[rChar];
                    if (openType == "{")
                    {
                        if (ch == '}')
                        {
                            return true;
                        }
                    }
                    else if (openType == "(*")
                    {
                        if (ch == '*' && rChar + 1 < line.Length && line[rChar + 1] == ')')
                        {
                            return true;
                        }
                    }
                    else if (openType == "(") {
                        if (ch == ')')
                        {
                            return true;
                        } }
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

                if (InputOutput.Ch == '\'')
                {
                    TextPosition stringStart = InputOutput.PositionNow;
                    if (!LookaheadForClosing("'"))
                    {
                        InputOutput.Error(12, stringStart);
                        InputOutput.ListErrors();
                        InputOutput.NextCh(); 
                        continue;
                    }
                    InputOutput.NextCh();
                    while (!InputOutput.IsEndOfFile && InputOutput.Ch != '\'') InputOutput.NextCh();
                    InputOutput.NextCh();
                    if (_tokenCodes.ContainsKey("string_literal")) return _tokenCodes["string_literal"];
                    return 0;
                }

                if (InputOutput.Ch == '/')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '/')
                    {
                        uint currentLine = pos.LineNumber;
                        while (!InputOutput.IsEndOfFile && InputOutput.PositionNow.LineNumber == currentLine) InputOutput.NextCh();
                        continue;
                    }
                    else
                    {
                        if (_tokenCodes.ContainsKey("/")) return _tokenCodes["/"];
                    }
                }

                if (InputOutput.Ch == '{')
                {
                    TextPosition commentStart = InputOutput.PositionNow;
                    if (!LookaheadForClosing("{"))
                    {
                        InputOutput.Error(5, commentStart);
                        InputOutput.ListErrors();
                        InputOutput.NextCh();
                        continue;
                    }
                    InputOutput.NextCh();
                    while (!InputOutput.IsEndOfFile && InputOutput.Ch != '}') InputOutput.NextCh();
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

                if (InputOutput.Ch == '(')
                {
                    TextPosition pos = InputOutput.PositionNow;
                    InputOutput.NextCh();
                    if (InputOutput.Ch == '*')
                    {
                        if (!LookaheadForClosing("(*"))
                        {
                            InputOutput.Error(7, pos);
                            InputOutput.ListErrors();
                            InputOutput.NextCh();
                            continue;
                        }
                        InputOutput.NextCh();
                        while (!InputOutput.IsEndOfFile)
                        {
                            if (InputOutput.Ch == '*')
                            {
                                InputOutput.NextCh();
                                if (InputOutput.Ch == ')') { InputOutput.NextCh(); break; }
                            }
                            else InputOutput.NextCh();
                        }
                        continue;
                    }
                    else
                    {
                        if (!LookaheadForClosing("("))
                        {
                            InputOutput.Error(11, pos);
                            InputOutput.ListErrors(); 
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

            if (InputOutput.IsEndOfFile)
            {
                return 0;
            }

            if (char.IsLetter(InputOutput.Ch))
            {
                string word = "";
                while (!InputOutput.IsEndOfFile && char.IsLetterOrDigit(InputOutput.Ch))
                {
                    word += InputOutput.Ch;
                    InputOutput.NextCh();
                }
                
                _lastIdentName = word; 

                word = word.ToLower();
                if (_keywords.ContainsKey(word)) return _keywords[word];
                if (_tokenCodes.ContainsKey("identifier")) return _tokenCodes["identifier"];
                return 0;
            }
            
            if (char.IsDigit(InputOutput.Ch))
            {
                string numStr = "";
                TextPosition startPos = InputOutput.PositionNow;
                bool isReal = false;

                while (!InputOutput.IsEndOfFile && (char.IsDigit(InputOutput.Ch) || InputOutput.Ch == '.'))
                {
                    if (InputOutput.Ch == '.')
                    {
                        // Защита: Смотрим вперед, чтобы не забрать точку из оператора '..' или 'end.'
                        uint nextLine = InputOutput.PositionNow.LineNumber;
                        int nextChar = InputOutput.PositionNow.CharNumber + 1;

                        if (nextLine < InputOutput.FileLines.Count && nextChar < InputOutput.FileLines[(int)nextLine].Length)
                        {
                            char lookahead = InputOutput.FileLines[(int)nextLine][nextChar];
                            if (!char.IsDigit(lookahead))
                            {
                                break; // Это точка конца программы или часть '..'
                            }
                        }
                        else
                        {
                            break;
                        }
                        isReal = true;
                    }

                    numStr += InputOutput.Ch;
                    InputOutput.NextCh();
                }

                if (!isReal)
                {
                    long longValue;
                    if (long.TryParse(numStr, out longValue))
                    {
                        if (longValue < MIN_PASCAL_INT || longValue > MAX_PASCAL_INT) InputOutput.Error(4, startPos); 
                        _lastNumericValue = (int)longValue;
                    }
                    else InputOutput.Error(4, startPos);
                }

                if (_tokenCodes.ContainsKey("number")) return _tokenCodes["number"];
                return 0;
            }

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

            // Чтение '.' или составного '..'
            if (currentCh == '.')
            {
                InputOutput.NextCh();
                if (InputOutput.Ch == '.')
                {
                    InputOutput.NextCh();
                    if (_tokenCodes.ContainsKey("..")) return _tokenCodes[".."];
                }
                if (_tokenCodes.ContainsKey(".")) return _tokenCodes["."];
            }

            if (currentCh == ')')
            {
                _roundBracketBalance--;
                if (_roundBracketBalance < 0)
                {
                    InputOutput.Error(9, InputOutput.PositionNow);
                    InputOutput.ListErrors(); 
                    _roundBracketBalance = 0; 
                }
                if (_tokenCodes.ContainsKey(")")) { InputOutput.NextCh(); return _tokenCodes[")"]; }
            }

            if (currentCh == '+') { InputOutput.NextCh(); return _tokenCodes["+"]; }
            if (currentCh == '-') { InputOutput.NextCh(); return _tokenCodes["-"]; }

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