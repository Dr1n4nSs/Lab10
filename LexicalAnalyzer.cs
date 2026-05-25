using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class LexicalAnalyzer
    {
        private static Dictionary<string, byte> _keywords;
        private static Dictionary<char, byte> _asciiRules;
        
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

            _asciiRules = new Dictionary<char, byte>();
            _asciiRules.Add('*', 21);
            _asciiRules.Add('/', 60);
            _asciiRules.Add('=', 16);
            _asciiRules.Add(',', 20);
            _asciiRules.Add(';', 14);
            _asciiRules.Add(':', 5);
            _asciiRules.Add('.', 61);
            _asciiRules.Add('>', 62);
            _asciiRules.Add('(', 40); // Открывающая круглая скобка
            _asciiRules.Add(')', 41); // Закрывающая круглая скобка
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

                // 1. ЛОГИКА КОММЕНТАРИЕВ И ИХ СКОБОК
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
                        InputOutput.Error(5, commentStart);
                        return 0;
                    }

                    InputOutput.NextCh(); // Пропускаем закрывающую скобку '}'
                    continue; 
                }

                // Если встретили '}' вне блока комментариев — это ошибка (одиночная закрывающая скобка)
                if (InputOutput.Ch == '}')
                {
                    InputOutput.Error(6, InputOutput.PositionNow);
                    InputOutput.NextCh();
                    continue;
                }

                break;
            }

            if (InputOutput.IsEndOfFile) return 0;

            // 2. ИДЕНТИФИКАТОРЫ И КЛЮЧЕВЫЕ СЛОВА
            if (char.IsLetter(InputOutput.Ch))
            {
                string word = "";
                while (!InputOutput.IsEndOfFile && char.IsLetterOrDigit(InputOutput.Ch))
                {
                    word += InputOutput.Ch;
                    InputOutput.NextCh();
                }

                word = word.ToLower();

                if (_keywords.ContainsKey(word))
                {
                    return _keywords[word];
                }

                return 1;
            }

            // 3. ЦЕЛОЧИСЛЕННЫЕ ЗНАЧЕНИЯ
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

                return 2;
            }

            // 4. ЗНАКИ ОПЕРАЦИЙ И ASCII СИМВОЛЫ
            TextPosition currentPos = InputOutput.PositionNow;
            char currentCh = InputOutput.Ch;

            if (currentCh == ':')
            {
                InputOutput.NextCh();
                if (InputOutput.Ch == '=')
                {
                    InputOutput.NextCh();
                    return 4; // Присваивание
                }
                if (_asciiRules.ContainsKey(':')) return _asciiRules[':'];
            }

            // Логика проверки одиночной закрывающей КРУГЛОЙ скобки.
            // Примечание: на данном этапе мы ловим её как ошибку, так как структуры выражений (парсера) ещё нет.
            if (currentCh == ')')
            {
                InputOutput.Error(7, currentPos);
                // Код символа все равно отдаем в поток (41), чтобы зафиксировать в codes.txt
                if (_asciiRules.ContainsKey(')'))
                {
                    byte asciiCode = _asciiRules[currentCh];
                    InputOutput.NextCh();
                    return asciiCode;
                }
            }

            // Проверка ручных тестовых кодов
            if (currentCh == '@') { InputOutput.Error(1, currentPos); InputOutput.NextCh(); return 0; }
            if (currentCh == '#') { InputOutput.Error(2, currentPos); InputOutput.NextCh(); return 0; }
            if (currentCh == '^') { InputOutput.Error(3, currentPos); InputOutput.NextCh(); return 0; }

            if (_asciiRules.ContainsKey(currentCh))
            {
                byte asciiCode = _asciiRules[currentCh];
                InputOutput.NextCh();
                return asciiCode;
            }

            InputOutput.NextCh();
            return 0;
        }
    }
}
