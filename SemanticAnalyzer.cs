using System;
using System.Collections.Generic;

namespace Компилятор
{
    // Перечисление всех типов данных в нашей системе
    public enum DataType : byte
    {
        Unknown,
        Integer,
        Real,
        Boolean,
        Char,
        String,
        Enumerate,
        Range
    }

    // Категория идентификатора
    public enum SymbolKind
    {
        Variable,
        Constant,
        Function
    }

    // Структура для хранения информации об идентификаторе
    public class Symbol
    {
        public string Name { get; set; }
        public DataType Type { get; set; }
        public SymbolKind Kind { get; set; }
        
        // Для функций: типы параметров и возвращаемый тип
        public List<DataType> ParameterTypes { get; set; } = new List<DataType>();
        public DataType ReturnType { get; set; }

        public Symbol(string name, DataType type, SymbolKind kind)
        {
            Name = name.ToLower();
            Type = type;
            Kind = kind;
        }
    }

    public static class SemanticAnalyzer
    {
        // Глобальная таблица символов
        private static Dictionary<string, Symbol> _globalTable = new Dictionary<string, Symbol>();
        
        // Локальная таблица символов (активна, когда мы парсим внутренности функции)
        private static Dictionary<string, Symbol> _localTable = null;
        
        // Текущая функция, которую мы анализируем (для проверки возвращаемого значения)
        public static Symbol CurrentFunction { get; set; } = null;

        public static void Init()
        {
            _globalTable.Clear();
            _localTable = null;
            CurrentFunction = null;
        }

        // Вход в локальную область видимости функции
        public static void EnterFunctionScope()
        {
            _localTable = new Dictionary<string, Symbol>();
        }

        // Выход из локальной области видимости
        public static void ExitFunctionScope()
        {
            _localTable = null;
            CurrentFunction = null;
        }
        
        public static bool AddSymbol(string name, DataType type, SymbolKind kind, out byte errorCode)
        {
            name = name.ToLower();
            errorCode = 0;
            
            var currentTable = (_localTable != null) ? _localTable : _globalTable;
            
            if (currentTable.ContainsKey(name))
            {
                errorCode = 80; 
                return false;
            }
            
            if (_localTable != null && CurrentFunction != null && CurrentFunction.Name == name)
            {
                if (kind == SymbolKind.Variable) return true; 
                
                errorCode = 80;
                return false;
            }

            currentTable.Add(name, new Symbol(name, type, kind));
            return true;
        }

        // Поиск символа (сначала локально, затем глобально)
        public static Symbol FindSymbol(string name)
        {
            name = name.ToLower();
            if (_localTable != null && _localTable.ContainsKey(name))
            {
                return _localTable[name];
            }
            if (_globalTable.ContainsKey(name))
            {
                return _globalTable[name];
            }
            
            // Если мы внутри функции и имя совпадает с именем функции, возвращаем её объект
            if (CurrentFunction != null && CurrentFunction.Name == name)
            {
                return CurrentFunction;
            }

            return null; // Идентификатор не найден
        }

        // Вспомогательный конвертер из кодов токенов лексера в DataType семантики
        public static DataType GetDataTypeFromToken(byte tokenCode)
        {
            if (tokenCode == LexicalAnalyzer.GetTokenCode("integer")) return DataType.Integer;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("real")) return DataType.Real;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("boolean")) return DataType.Boolean;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("char")) return DataType.Char;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("string")) return DataType.String;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("enumerate")) return DataType.Enumerate;
            if (tokenCode == LexicalAnalyzer.GetTokenCode("range")) return DataType.Range;
            return DataType.Unknown;
        }
    }
}