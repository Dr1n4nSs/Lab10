using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class LexicalAnalyzer
    {
        private static Dictionary<string, byte> _keywords;
        
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
        }

        public static byte Scan()
        {
            // Пропуск пробелов и невидимых символов
            while (!InputOutput.IsEndOfFile && (InputOutput.Ch == ' ' || InputOutput.Ch == '\r' || InputOutput.Ch == '\n' || InputOutput.Ch == '\t'))
            {
                InputOutput.NextCh();
            }

            if (InputOutput.IsEndOfFile) return 0;

            // 1. ОБРАБОТКА ИДЕНТИФИКАТОРОВ И КЛЮЧЕВЫХ СЛОВ
            if (char.IsLetter(InputOutput.Ch))
            {
                string word = "";

                // Накапливаем буквы и цифры в одну строку
                while (!InputOutput.IsEndOfFile && char.IsLetterOrDigit(InputOutput.Ch))
                {
                    word += InputOutput.Ch;
                    InputOutput.NextCh();
                }
                
                word = word.ToLower();

                // Выполняем поиск по таблице ключевых слов (без LINQ)
                if (_keywords.ContainsKey(word))
                {
                    return _keywords[word]; // Если нашли, возвращаем код 3 (Ключевое слово)
                }

                return 1; // Если не нашли в таблице — это код 1 (Идентификатор пользователя)
            }

            // 2. ОБРАБОТКА ЦЕЛЫХ ЧИСЕЛ
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
                        InputOutput.Error(4, startPos, $"Выход целого числа '{numStr}' за пределы диапазона [{MIN_PASCAL_INT}..{MAX_PASCAL_INT}]");
                    }
                }
                else
                {
                    InputOutput.Error(4, startPos, "Число слишком велико для обработки");
                }

                return 2;
            }

            // 3. ОБРАБОТКА ЗНАКОВ ОПЕРАЦИЙ И СИМВОЛОВ
            TextPosition currentPos = InputOutput.PositionNow;
            char currentCh = InputOutput.Ch;

            if (currentCh == ':')
            {
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    InputOutput.NextCh();
                    return 4; // Код 4 (:=)
                }
                return 5; // Код 5 (:)
            }

            if (currentCh == ';') { InputOutput.NextCh(); return 6; }
            if (currentCh == '=') { InputOutput.NextCh(); return 7; }
            if (currentCh == '.') { InputOutput.NextCh(); return 8; }
            if (currentCh == ',') { InputOutput.NextCh(); return 9; }
            
            foreach (char triggerChar in InputOutput.ErrorRules.Keys)
            {
                if (currentCh == triggerChar)
                {
                    var rule = InputOutput.ErrorRules[triggerChar];
                    InputOutput.Error(rule.Code, currentPos, rule.Desc);
                    InputOutput.NextCh();
                    return 0; 
                }
            }

            InputOutput.NextCh();
            return 0;
        }
    }
}