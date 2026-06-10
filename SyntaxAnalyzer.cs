using System;
using System.Collections.Generic;

namespace Компилятор
{
    public static class SyntaxAnalyzer
    {
        private static byte _currentToken;

        private static void GetNextToken()
        {
            _currentToken = LexicalAnalyzer.Scan();
        }

        private static void Neutralize(List<byte> syncTokens)
        {
            while (!InputOutput.IsEndOfFile && !syncTokens.Contains(_currentToken) && _currentToken != 0)
            {
                GetNextToken();
            }
        }

        public static void Parse()
        {
            GetNextToken();
            ProgramBlock();
        }

        private static void ProgramBlock()
        {
            // 1. Заголовок
            if (_currentToken == LexicalAnalyzer.GetTokenCode("program"))
            {
                GetNextToken();
                if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier")) GetNextToken();
                if (_currentToken == LexicalAnalyzer.GetTokenCode(";")) GetNextToken();
            }
            
            SemanticAnalyzer.Init();
            
            while (_currentToken == LexicalAnalyzer.GetTokenCode("const") || _currentToken == LexicalAnalyzer.GetTokenCode("var"))
            {
                if (_currentToken == LexicalAnalyzer.GetTokenCode("const")) ConstDeclaration();
                if (_currentToken == LexicalAnalyzer.GetTokenCode("var")) VarDeclaration();
            }

            while (_currentToken == LexicalAnalyzer.GetTokenCode("function"))
            {
                FunctionDeclaration();
            }
            
            CompoundStatement();

            if (_currentToken == LexicalAnalyzer.GetTokenCode(".")) GetNextToken();
            else InputOutput.Error(52, InputOutput.PositionNow);
        }
        
        private static void ConstDeclaration()
        {
            GetNextToken(); 

            while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                GetNextToken();

                if (_currentToken == LexicalAnalyzer.GetTokenCode("="))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(63, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode(";"), LexicalAnalyzer.GetTokenCode("identifier"), LexicalAnalyzer.GetTokenCode("var") });
                }
                
                if (_currentToken == LexicalAnalyzer.GetTokenCode("number") || _currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    int minVal = LexicalAnalyzer.LastNumericValue;
                    bool isMinANumber = (_currentToken == LexicalAnalyzer.GetTokenCode("number"));

                    GetNextToken();
                    
                    if (_currentToken == LexicalAnalyzer.GetTokenCode(".."))
                    {
                        GetNextToken();

                        if (_currentToken == LexicalAnalyzer.GetTokenCode("number"))
                        {
                            int maxVal = LexicalAnalyzer.LastNumericValue;
                            if (isMinANumber && minVal > maxVal)
                            {
                                InputOutput.Error(72, InputOutput.PositionNow); 
                                InputOutput.ListErrors();
                            }
                            GetNextToken();
                        }
                        else if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                        {
                            GetNextToken();
                        }
                        else
                        {
                            InputOutput.Error(70, InputOutput.PositionNow);
                            InputOutput.ListErrors();
                        }
                    }
                }
                else if (_currentToken == LexicalAnalyzer.GetTokenCode("string_literal"))
                {
                    GetNextToken(); 
                }
                else if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
                {
                    GetNextToken();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                    {
                        GetNextToken();
                    }
                    else
                    {
                        InputOutput.Error(71, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }

                    while (_currentToken == LexicalAnalyzer.GetTokenCode(","))
                    {
                        GetNextToken();
                        if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                        {
                            GetNextToken();
                        }
                        else
                        {
                            InputOutput.Error(71, InputOutput.PositionNow); 
                            InputOutput.ListErrors();
                        }
                    }

                    if (_currentToken == LexicalAnalyzer.GetTokenCode(")"))
                    {
                        GetNextToken();
                    }
                    else
                    {
                        InputOutput.Error(71, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }
                }
                else
                {
                    InputOutput.Error(64, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(";"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(51, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode("identifier"), LexicalAnalyzer.GetTokenCode("var"), LexicalAnalyzer.GetTokenCode("begin") });
                }
            }
        }
        
        private static void ParseType()
        {
            if (_currentToken == LexicalAnalyzer.GetTokenCode("string"))
            {
                GetNextToken();
                if (_currentToken == LexicalAnalyzer.GetTokenCode("["))
                {
                    GetNextToken();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode("number"))
                    {
                        GetNextToken();
                    }
                    else
                    {
                        InputOutput.Error(69, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }

                    if (_currentToken == LexicalAnalyzer.GetTokenCode("]"))
                    {
                        GetNextToken();
                    }
                    else
                    {
                        InputOutput.Error(69, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }
                }
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("enumerate"))
            {
                GetNextToken();
                if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(65, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(66, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }

                while (_currentToken == LexicalAnalyzer.GetTokenCode(","))
                {
                    GetNextToken();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                    {
                        GetNextToken();
                    }
                    else
                    {
                        InputOutput.Error(66, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(")"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(67, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("range"))
            {
                GetNextToken(); // Пропускаем 'range'
                int minVal = 0;
                int maxVal = 0;
                bool isMinANumber = false;

                if (_currentToken == LexicalAnalyzer.GetTokenCode("number"))
                {
                    minVal = LexicalAnalyzer.LastNumericValue;
                    isMinANumber = true;
                    GetNextToken();
                }
                else if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(68, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(".."))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(73, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode("number"))
                {
                    maxVal = LexicalAnalyzer.LastNumericValue;
                    if (isMinANumber && minVal > maxVal)
                    {
                        InputOutput.Error(72, InputOutput.PositionNow);
                        InputOutput.ListErrors();
                    }
                    GetNextToken();
                }
                else if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(68, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }
            }
            else
            {
                GetNextToken();
            }
        }
        

        private static void CompoundStatement()
        {
            if (_currentToken == LexicalAnalyzer.GetTokenCode("begin"))
            {
                GetNextToken();
            }
            else
            {
                InputOutput.Error(58, InputOutput.PositionNow); 
                InputOutput.ListErrors();
            }

            while (_currentToken != LexicalAnalyzer.GetTokenCode("end") && !InputOutput.IsEndOfFile && _currentToken != 0)
            {
                Statement();
                if (_currentToken == LexicalAnalyzer.GetTokenCode(";"))
                {
                    GetNextToken();
                }
                else if (_currentToken != LexicalAnalyzer.GetTokenCode("end"))
                {
                    InputOutput.Error(51, InputOutput.PositionNow); InputOutput.ListErrors();
                    Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode(";"), LexicalAnalyzer.GetTokenCode("end") });
                }
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("end"))
            {
                GetNextToken();
            }
            else
            {
                InputOutput.Error(59, InputOutput.PositionNow);
                InputOutput.ListErrors();
            }
        }

        private static void Statement()
        {
            if (_currentToken == LexicalAnalyzer.GetTokenCode("begin"))
            {
                CompoundStatement();
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                AssignmentStatement();
            }
            else Neutralize(new List<byte>
            {
                LexicalAnalyzer.GetTokenCode(";"), 
                LexicalAnalyzer.GetTokenCode("end")
            });
        }
        
        
        private static void VarDeclaration()
        {
            GetNextToken();
            while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                List<string> varNames = new List<string>();

                while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {

                    varNames.Add(LexicalAnalyzer.LastIdentName);

                    GetNextToken();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode(","))
                    {
                        GetNextToken();
                        if (_currentToken != LexicalAnalyzer.GetTokenCode("identifier"))
                        {
                            InputOutput.Error(53, InputOutput.PositionNow); 
                            InputOutput.ListErrors();
                        }
                    }
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(":")) GetNextToken();
                else { InputOutput.Error(54, InputOutput.PositionNow); InputOutput.ListErrors(); }

                DataType currentType = DataType.Unknown;
                if (IsTypeToken(_currentToken))
                {
                    currentType = SemanticAnalyzer.GetDataTypeFromToken(_currentToken);
                    ParseType(); 
                }
                else { InputOutput.Error(55, InputOutput.PositionNow); InputOutput.ListErrors(); }
                
                foreach (string varName in varNames)
                {
                    byte err;
                    if (!SemanticAnalyzer.AddSymbol(varName, currentType, SymbolKind.Variable, out err))
                    {
                        InputOutput.Error(err, InputOutput.PositionNow);
                        InputOutput.ListErrors();
                    }
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(";")) GetNextToken();
                else { InputOutput.Error(51, InputOutput.PositionNow); InputOutput.ListErrors(); }
            }
        }
        private static void FunctionDeclaration()
        {
            GetNextToken(); 
            
            string funcName = LexicalAnalyzer.LastIdentName; 
    
            Symbol funcSymbol = new Symbol(funcName, DataType.Unknown, SymbolKind.Function);

            if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier")) 
            {
                GetNextToken(); 
            }
            else 
            { 
                InputOutput.Error(56, InputOutput.PositionNow); 
                InputOutput.ListErrors(); 
            }

            SemanticAnalyzer.EnterFunctionScope();
            SemanticAnalyzer.CurrentFunction = funcSymbol;

            if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
            {
                GetNextToken();
                while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    // 1. ЗАПОМИНАЕМ имя и ТОЧНУЮ позицию токена до сдвига лексера
                    string paramName = LexicalAnalyzer.LastIdentName;
                    var errorPos = InputOutput.PositionNow; // Сохраняем структуру позиции x
                    
                    GetNextToken(); // Уходим на ':'
                    
                    if (_currentToken == LexicalAnalyzer.GetTokenCode(":")) GetNextToken();
                    
                    DataType paramType = DataType.Unknown;
                    if (IsTypeToken(_currentToken))
                    {
                        paramType = SemanticAnalyzer.GetDataTypeFromToken(_currentToken);
                        ParseType(); // Уходим на ';' или ')'
                    }

                    // 2. При добавлении передаем именно сохраненную позицию errorPos
                    byte err;
                    if (!SemanticAnalyzer.AddSymbol(paramName, paramType, SymbolKind.Variable, out err))
                    {
                        InputOutput.Error(err, errorPos); // Ошибка отобразится точно над вторым 'x'
                        InputOutput.ListErrors();
                    }
                    
                    funcSymbol.ParameterTypes.Add(paramType);

                    if (_currentToken == LexicalAnalyzer.GetTokenCode(";")) GetNextToken();
                }
                if (_currentToken == LexicalAnalyzer.GetTokenCode(")")) GetNextToken();
                else { InputOutput.Error(57, InputOutput.PositionNow); InputOutput.ListErrors(); }
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode(":"))
            {
                GetNextToken();
                if (IsTypeToken(_currentToken))
                {
                    funcSymbol.ReturnType = SemanticAnalyzer.GetDataTypeFromToken(_currentToken);
                    funcSymbol.Type = funcSymbol.ReturnType;
                    ParseType();
                }
                else { InputOutput.Error(55, InputOutput.PositionNow); InputOutput.ListErrors(); }
            }
            
            byte globalErr;
            SemanticAnalyzer.ExitFunctionScope();
            if (!SemanticAnalyzer.AddSymbol(funcName, funcSymbol.Type, SymbolKind.Function, out globalErr))
            {
                InputOutput.Error(globalErr, InputOutput.PositionNow);
                InputOutput.ListErrors();
            }
            SemanticAnalyzer.EnterFunctionScope();
            SemanticAnalyzer.CurrentFunction = funcSymbol;

            if (_currentToken == LexicalAnalyzer.GetTokenCode(";")) GetNextToken();
            else { InputOutput.Error(51, InputOutput.PositionNow); InputOutput.ListErrors(); }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("const")) ConstDeclaration();
            if (_currentToken == LexicalAnalyzer.GetTokenCode("var")) VarDeclaration();

            CompoundStatement();
            SemanticAnalyzer.ExitFunctionScope();

            if (_currentToken == LexicalAnalyzer.GetTokenCode(";")) GetNextToken();
            else { InputOutput.Error(51, InputOutput.PositionNow); InputOutput.ListErrors(); }
        }
        
        private static void AssignmentStatement()
        {
            string targetName = LexicalAnalyzer.LastIdentName; 
            Symbol targetSymbol = SemanticAnalyzer.FindSymbol(targetName);
            
            GetNextToken();
            
            if (targetSymbol == null)
            {
                InputOutput.Error(81, InputOutput.PositionNow); 
                InputOutput.ListErrors();
            }
            else if (targetSymbol.Kind == SymbolKind.Constant)
            {
                InputOutput.Error(83, InputOutput.PositionNow); 
                InputOutput.ListErrors();
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode(":="))
            {
                GetNextToken();
                
                DataType exprType = Expression(); 
                
                if (targetSymbol != null && targetSymbol.Type != exprType)
                {
                    if (!(targetSymbol.Type == DataType.Real && exprType == DataType.Integer))
                    {
                        InputOutput.Error(82, InputOutput.PositionNow); 
                        InputOutput.ListErrors();
                    }
                }
            }
            else
            {
                InputOutput.Error(60, InputOutput.PositionNow); InputOutput.ListErrors();
                Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode(";"), LexicalAnalyzer.GetTokenCode("end") });
            }
        }
        
        private static DataType Expression()
        {
            DataType type1 = Term();
            while (_currentToken == LexicalAnalyzer.GetTokenCode("+") || _currentToken == LexicalAnalyzer.GetTokenCode("-"))
            {
                GetNextToken();
                DataType type2 = Term();
                
                if ((type1 != DataType.Integer && type1 != DataType.Real) || 
                    (type2 != DataType.Integer && type2 != DataType.Real))
                {
                    InputOutput.Error(84, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                    type1 = DataType.Unknown;
                }
                else if (type1 == DataType.Real || type2 == DataType.Real)
                {
                    type1 = DataType.Real; 
                }
            }
            return type1;
        }

        private static DataType Term()
        {
            DataType type1 = Factor();
            while (_currentToken == LexicalAnalyzer.GetTokenCode("*") || _currentToken == LexicalAnalyzer.GetTokenCode("/"))
            {
                byte op = _currentToken;
                GetNextToken();
                DataType type2 = Factor();

                if ((type1 != DataType.Integer && type1 != DataType.Real) || 
                    (type2 != DataType.Integer && type2 != DataType.Real))
                {
                    InputOutput.Error(84, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    type1 = DataType.Unknown;
                }
                else if (op == LexicalAnalyzer.GetTokenCode("/"))
                {
                    type1 = DataType.Real; 
                }
                else if (type1 == DataType.Real || type2 == DataType.Real)
                {
                    type1 = DataType.Real;
                }
            }
            return type1;
        }

        private static DataType Factor()
        {
            if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                string name = LexicalAnalyzer.LastIdentName; 
                Symbol s = SemanticAnalyzer.FindSymbol(name);
                GetNextToken();

                if (s == null)
                {
                    InputOutput.Error(81, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                    return DataType.Unknown;
                }
                return s.Type;
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("number"))
            {
                GetNextToken();
                return DataType.Integer; 
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("string_literal"))
            {
                GetNextToken();
                return DataType.String;
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
            {
                GetNextToken();
                DataType t = Expression();
                if (_currentToken == LexicalAnalyzer.GetTokenCode(")")) GetNextToken();
                else { InputOutput.Error(41, InputOutput.PositionNow); InputOutput.ListErrors(); }
                return t;
            }
            else
            {
                InputOutput.Error(61, InputOutput.PositionNow); InputOutput.ListErrors();
                return DataType.Unknown;
            }
        }
        
        private static string GetCurrentTokenString()
        {
            try
            {
                string line = InputOutput.FileLines[(int)InputOutput.PositionNow.LineNumber];
                int start = InputOutput.PositionNow.CharNumber;
                int len = 0;
                while (start + len < line.Length && char.IsLetterOrDigit(line[start + len])) len++;
                return line.Substring(start, len);
            }
            catch { return "unknown"; }
        }
        
        private static bool IsTypeToken(byte token)
        {
            return token == LexicalAnalyzer.GetTokenCode("integer") ||
                   token == LexicalAnalyzer.GetTokenCode("real") ||
                   token == LexicalAnalyzer.GetTokenCode("boolean") ||
                   token == LexicalAnalyzer.GetTokenCode("char") ||
                   token == LexicalAnalyzer.GetTokenCode("string") ||
                   token == LexicalAnalyzer.GetTokenCode("enumerate") ||
                   token == LexicalAnalyzer.GetTokenCode("range");
        }
    }
}