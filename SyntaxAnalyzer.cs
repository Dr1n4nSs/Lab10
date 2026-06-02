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
            if (_currentToken == LexicalAnalyzer.GetTokenCode("program"))
            {
                GetNextToken();
                if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(50, InputOutput.PositionNow); 
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
                }
            }
            
            if (_currentToken == LexicalAnalyzer.GetTokenCode("const"))
            {
                ConstDeclaration();
            }
            
            if (_currentToken == LexicalAnalyzer.GetTokenCode("var"))
            {
                VarDeclaration();
            }
            
            if (_currentToken == LexicalAnalyzer.GetTokenCode("const"))
            {
                InputOutput.Error(62, InputOutput.PositionNow);
                InputOutput.ListErrors();
                ConstDeclaration(); 
            }
            
            while (_currentToken == LexicalAnalyzer.GetTokenCode("function"))
            {
                FunctionDeclaration();
                
                if (_currentToken == LexicalAnalyzer.GetTokenCode("const") || _currentToken == LexicalAnalyzer.GetTokenCode("var"))
                {
                    InputOutput.Error(62, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode("const"))
                    {
                        ConstDeclaration();
                    }
                    else
                    {
                        VarDeclaration();
                    }
                }
            }

            CompoundStatement();

            if (_currentToken == LexicalAnalyzer.GetTokenCode(".")) GetNextToken();
            else { InputOutput.Error(52, InputOutput.PositionNow); InputOutput.ListErrors(); }
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

        private static void VarDeclaration()
        {
            GetNextToken();

            while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
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

                if (_currentToken == LexicalAnalyzer.GetTokenCode(":"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(54, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                    Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode(";"), LexicalAnalyzer.GetTokenCode("begin") });
                }

                if (IsTypeToken(_currentToken))
                {
                    ParseType();
                }
                else
                {
                    InputOutput.Error(55, InputOutput.PositionNow); 
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
                    Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode("identifier"), LexicalAnalyzer.GetTokenCode("begin"), LexicalAnalyzer.GetTokenCode("function") });
                }
            }
        }

        private static void FunctionDeclaration()
        {
            GetNextToken();

            if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
            {
                GetNextToken();
            }
            else
            {
                InputOutput.Error(56, InputOutput.PositionNow);
                InputOutput.ListErrors();
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
            {
                GetNextToken();
                while (_currentToken == LexicalAnalyzer.GetTokenCode("identifier"))
                {
                    GetNextToken();
                    if (_currentToken == LexicalAnalyzer.GetTokenCode(":"))
                    {
                        GetNextToken();
                    }

                    if (IsTypeToken(_currentToken))
                    {
                        ParseType();
                    }

                    if (_currentToken == LexicalAnalyzer.GetTokenCode(";"))
                    {
                        GetNextToken();
                    }
                }

                if (_currentToken == LexicalAnalyzer.GetTokenCode(")"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(57, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                }
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode(":"))
            {
                GetNextToken();
                if (IsTypeToken(_currentToken))
                {
                    ParseType();
                }
                else
                {
                    InputOutput.Error(55, InputOutput.PositionNow);
                    InputOutput.ListErrors();
                }
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode(";"))
            {
                GetNextToken();
            }
            else
            {
                InputOutput.Error(51, InputOutput.PositionNow); 
                InputOutput.ListErrors();
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("const"))
            {
                ConstDeclaration();
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("var"))
            {
                VarDeclaration();
            }

            if (_currentToken == LexicalAnalyzer.GetTokenCode("const"))
            {
                InputOutput.Error(62, InputOutput.PositionNow); InputOutput.ListErrors();
                ConstDeclaration();
            }

            CompoundStatement();

            if (_currentToken == LexicalAnalyzer.GetTokenCode(";"))
            {
                GetNextToken();
            }
            else
            {
                InputOutput.Error(51, InputOutput.PositionNow); 
                InputOutput.ListErrors();
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

        private static void AssignmentStatement()
        {
            GetNextToken();
            if (_currentToken == LexicalAnalyzer.GetTokenCode(":="))
            {
                GetNextToken();
                Expression();
            }
            else
            {
                InputOutput.Error(60, InputOutput.PositionNow); InputOutput.ListErrors();
                Neutralize(new List<byte> { LexicalAnalyzer.GetTokenCode(";"), LexicalAnalyzer.GetTokenCode("end") });
            }
        }

        private static void Expression()
        {
            Term();
            while (_currentToken == LexicalAnalyzer.GetTokenCode("+") || _currentToken == LexicalAnalyzer.GetTokenCode("-"))
            {
                GetNextToken();
                Term();
            }
        }

        private static void Term()
        {
            Factor();
            while (_currentToken == LexicalAnalyzer.GetTokenCode("*") || _currentToken == LexicalAnalyzer.GetTokenCode("/"))
            {
                GetNextToken();
                Factor();
            }
        }

        private static void Factor()
        {
            if (_currentToken == LexicalAnalyzer.GetTokenCode("identifier") || 
                _currentToken == LexicalAnalyzer.GetTokenCode("number") || 
                _currentToken == LexicalAnalyzer.GetTokenCode("string_literal"))
            {
                GetNextToken();
            }
            else if (_currentToken == LexicalAnalyzer.GetTokenCode("("))
            {
                GetNextToken();
                Expression();
                if (_currentToken == LexicalAnalyzer.GetTokenCode(")"))
                {
                    GetNextToken();
                }
                else
                {
                    InputOutput.Error(41, InputOutput.PositionNow); 
                    InputOutput.ListErrors();
                }
            }
            else
            {
                InputOutput.Error(61, InputOutput.PositionNow); 
                InputOutput.ListErrors();
            }
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