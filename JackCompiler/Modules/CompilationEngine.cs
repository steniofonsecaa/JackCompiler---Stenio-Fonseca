using System.IO;
using JackCompiler.Enums;

namespace JackCompiler.Modules
{
    public class CompilationEngine
    {
        private JackTokenizer _tokenizer;
        private StreamWriter _writer;

        public CompilationEngine(JackTokenizer tokenizer, string outputPath)
        {
            _tokenizer = tokenizer;
            _writer = new StreamWriter(outputPath);

            // Avança para o primeiro token antes de iniciar a compilação
            if (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                CompileClass();
            }
        }

        public void Close()
        {
            _writer.Close();
        }

        // Método auxiliar que escreve o token atual (já formatado em XML) e avança
        private void ProcessToken()
        {
            _writer.WriteLine(_tokenizer.GetTokenTag());
            if (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
            }
        }

        public void CompileClass()
        {
            _writer.WriteLine("<class>");
            ProcessToken(); // 'class'
            ProcessToken(); // className
            ProcessToken(); // '{'

            while (_tokenizer.CurrentToken == "static" || _tokenizer.CurrentToken == "field")
            {
                CompileClassVarDec();
            }

            while (_tokenizer.CurrentToken == "constructor" || _tokenizer.CurrentToken == "function" || _tokenizer.CurrentToken == "method")
            {
                CompileSubroutine();
            }

            ProcessToken(); // '}'
            _writer.WriteLine("</class>");
        }

        public void CompileClassVarDec()
        {
            _writer.WriteLine("<classVarDec>");
            ProcessToken(); // 'static' ou 'field'
            ProcessToken(); // tipo
            ProcessToken(); // nomeDaVariavel

            while (_tokenizer.CurrentToken == ",")
            {
                ProcessToken(); // ','
                ProcessToken(); // nomeDaVariavel
            }

            ProcessToken(); // ';'
            _writer.WriteLine("</classVarDec>");
        }

        public void CompileSubroutine()
        {
            _writer.WriteLine("<subroutineDec>");
            ProcessToken(); // 'constructor', 'function', ou 'method'
            ProcessToken(); // tipo de retorno ('void' ou outro)
            ProcessToken(); // nomeDaSubrotina
            ProcessToken(); // '('
            
            CompileParameterList();
            
            ProcessToken(); // ')'
            CompileSubroutineBody();
            _writer.WriteLine("</subroutineDec>");
        }

        public void CompileParameterList()
        {
            _writer.WriteLine("<parameterList>");
            if (_tokenizer.CurrentToken != ")")
            {
                ProcessToken(); // tipo
                ProcessToken(); // nomeDaVariavel

                while (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // ','
                    ProcessToken(); // tipo
                    ProcessToken(); // nomeDaVariavel
                }
            }
            _writer.WriteLine("</parameterList>");
        }

        public void CompileSubroutineBody()
        {
            _writer.WriteLine("<subroutineBody>");
            ProcessToken(); // '{'

            while (_tokenizer.CurrentToken == "var")
            {
                CompileVarDec();
            }

            CompileStatements();

            ProcessToken(); // '}'
            _writer.WriteLine("</subroutineBody>");
        }

        public void CompileVarDec()
        {
            _writer.WriteLine("<varDec>");
            ProcessToken(); // 'var'
            ProcessToken(); // tipo
            ProcessToken(); // nomeDaVariavel

            while (_tokenizer.CurrentToken == ",")
            {
                ProcessToken(); // ','
                ProcessToken(); // nomeDaVariavel
            }

            ProcessToken(); // ';'
            _writer.WriteLine("</varDec>");
        }

        public void CompileStatements()
        {
            _writer.WriteLine("<statements>");
            while (_tokenizer.CurrentToken == "let" || _tokenizer.CurrentToken == "if" || 
                   _tokenizer.CurrentToken == "while" || _tokenizer.CurrentToken == "do" || 
                   _tokenizer.CurrentToken == "return")
            {
                if (_tokenizer.CurrentToken == "let") CompileLet();
                else if (_tokenizer.CurrentToken == "if") CompileIf();
                else if (_tokenizer.CurrentToken == "while") CompileWhile();
                else if (_tokenizer.CurrentToken == "do") CompileDo();
                else if (_tokenizer.CurrentToken == "return") CompileReturn();
            }
            _writer.WriteLine("</statements>");
        }

        public void CompileLet()
        {
            _writer.WriteLine("<letStatement>");
            ProcessToken(); // 'let'
            ProcessToken(); // nomeDaVariavel

            if (_tokenizer.CurrentToken == "[")
            {
                ProcessToken(); // '['
                CompileExpression();
                ProcessToken(); // ']'
            }

            ProcessToken(); // '='
            CompileExpression();
            ProcessToken(); // ';'
            _writer.WriteLine("</letStatement>");
        }

        public void CompileIf()
        {
            _writer.WriteLine("<ifStatement>");
            ProcessToken(); // 'if'
            ProcessToken(); // '('
            CompileExpression();
            ProcessToken(); // ')'
            ProcessToken(); // '{'
            CompileStatements();
            ProcessToken(); // '}'

            if (_tokenizer.HasMoreTokens() && _tokenizer.CurrentToken == "else")
            {
                ProcessToken(); // 'else'
                ProcessToken(); // '{'
                CompileStatements();
                ProcessToken(); // '}'
            }
            _writer.WriteLine("</ifStatement>");
        }

        public void CompileWhile()
        {
            _writer.WriteLine("<whileStatement>");
            ProcessToken(); // 'while'
            ProcessToken(); // '('
            CompileExpression();
            ProcessToken(); // ')'
            ProcessToken(); // '{'
            CompileStatements();
            ProcessToken(); // '}'
            _writer.WriteLine("</whileStatement>");
        }

        public void CompileDo()
        {
            _writer.WriteLine("<doStatement>");
            ProcessToken(); // 'do'
            CompileSubroutineCall();
            ProcessToken(); // ';'
            _writer.WriteLine("</doStatement>");
        }

        public void CompileReturn()
        {
            _writer.WriteLine("<returnStatement>");
            ProcessToken(); // 'return'
            
            if (_tokenizer.CurrentToken != ";")
            {
                CompileExpression();
            }
            
            ProcessToken(); // ';'
            _writer.WriteLine("</returnStatement>");
        }

        public void CompileExpression()
        {
            _writer.WriteLine("<expression>");
            CompileTerm();

            while (IsOp(_tokenizer.CurrentToken))
            {
                ProcessToken(); // operador matemático/lógico
                CompileTerm();
            }
            
            _writer.WriteLine("</expression>");
        }

        public void CompileTerm()
        {
            _writer.WriteLine("<term>");
            var token = _tokenizer.CurrentToken;
            var type = _tokenizer.GetTokenType();

            if (type == TokenType.INT_CONST || type == TokenType.STRING_CONST || type == TokenType.KEYWORD)
            {
                ProcessToken(); // const int, const string ou keyword
            }
            else if (token == "-" || token == "~")
            {
                ProcessToken(); // operador unário
                CompileTerm();
            }
            else if (token == "(")
            {
                ProcessToken(); // '('
                CompileExpression();
                ProcessToken(); // ')'
            }
            else if (type == TokenType.IDENTIFIER)
            {
                ProcessToken(); // Identificador (variável ou nome da função/classe)

                // Olha para o próximo token para ver se era array ou chamada de função
                if (_tokenizer.CurrentToken == "[")
                {
                    ProcessToken(); // '['
                    CompileExpression();
                    ProcessToken(); // ']'
                }
                else if (_tokenizer.CurrentToken == "(" || _tokenizer.CurrentToken == ".")
                {
                    if (_tokenizer.CurrentToken == "(")
                    {
                        ProcessToken(); // '('
                        CompileExpressionList();
                        ProcessToken(); // ')'
                    }
                    else if (_tokenizer.CurrentToken == ".")
                    {
                        ProcessToken(); // '.'
                        ProcessToken(); // nomeDaSubrotina
                        ProcessToken(); // '('
                        CompileExpressionList();
                        ProcessToken(); // ')'
                    }
                }
            }
            _writer.WriteLine("</term>");
        }

        public void CompileExpressionList()
        {
            _writer.WriteLine("<expressionList>");
            if (_tokenizer.CurrentToken != ")")
            {
                CompileExpression();
                while (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // ','
                    CompileExpression();
                }
            }
            _writer.WriteLine("</expressionList>");
        }

        // Helper para processar a chamada de subrotina dentro do 'do'
        private void CompileSubroutineCall()
        {
            ProcessToken(); // Identificador inicial
            
            if (_tokenizer.CurrentToken == "(")
            {
                ProcessToken(); // '('
                CompileExpressionList();
                ProcessToken(); // ')'
            }
            else if (_tokenizer.CurrentToken == ".")
            {
                ProcessToken(); // '.'
                ProcessToken(); // nomeDaSubrotina
                ProcessToken(); // '('
                CompileExpressionList();
                ProcessToken(); // ')'
            }
        }

        private bool IsOp(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" ||
                   token == "&" || token == "|" || token == "<" || token == ">" || token == "=";
        }
    }
}