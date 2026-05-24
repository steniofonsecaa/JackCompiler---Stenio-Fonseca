using JackCompiler.Enums;
using System;

namespace JackCompiler.Modules
{
    public class CompilationEngine
    {
        private JackTokenizer _tokenizer;
        private VMWriter _vmWriter;
        private SymbolTable _symbolTable;
        //private StreamWriter _writer;
        private string _className; // Para armazenar o nome da classe atual, útil para gerar código VM

        // Metodo construtor que agora inicializa o VMWriter e o SymbolTable
        public CompilationEngine(JackTokenizer tokenizer, string outputPath)
        {   
            _tokenizer = tokenizer;
            _vmWriter = new VMWriter(outputPath);
            _symbolTable = new SymbolTable();
        }

        // Metodo Close para fechar o VMWriter e liberar recursos
        public void Close() => _vmWriter.Close();

        // ProcessToken
        private void ProcessToken()
        {
            var type = _tokenizer.GetTokenType();
            string tag = type.ToString().ToLower().Replace("_const", "Constant");
            
            string valor = _tokenizer.CurrentToken;

            // Substitui caracteres especiais para a saída XML
            if (valor == "<") valor = "&lt;";
            else if (valor == ">") valor = "&gt;";
            else if (valor == "&") valor = "&amp;";
            else if (valor == "\"") valor = "&quot;";
        }

        // Método para compilar uma classe, para nao escrever XML e salvar o nome da classe  
        public void CompileClass()
        {
            _tokenizer.Advance(); // 'class'
            ProcessToken();

            _tokenizer.Advance(); // className
            _className = _tokenizer.CurrentToken; // Salva o nome da classe para uso futuro
            ProcessToken();

            _tokenizer.Advance(); // '{'
            ProcessToken();

            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                string token = _tokenizer.CurrentToken.Trim();

                if (token == "}") break;

                if (token == "static" || token == "field")
                {
                    CompileClassVarDec(); 
                }
                else if (token == "constructor" || token == "function" || token == "method")
                {
                    CompileSubroutine(); 
                }
                else
                {
                    ProcessToken();
                }
            }
        }

        // Método para escrever a declaração de variáveis locais, seguindo a estrutura da gramática do Jack
        public void CompileVarDec()
        {
            ProcessToken(); // 'var'
            VarKind kind = VarKind.VAR; // No Jack, 'var' significa variável local

            // Tipo
            _tokenizer.Advance();
            string type = _tokenizer.CurrentToken;
            ProcessToken();

            // Nome
            _tokenizer.Advance();
            string name = _tokenizer.CurrentToken;
            
            // Registra a variável na tabela de símbolos, associando o nome, tipo e kind (local)
            _symbolTable.Define(name, type, kind);
            ProcessToken();

            // Múltiplas variáveis (ex: var int i, j, k;)
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // vírgula
                    
                    _tokenizer.Advance();
                    name = _tokenizer.CurrentToken;
                    
                    _symbolTable.Define(name, type, kind);
                    ProcessToken();
                }
            }

            ProcessToken(); // Processa ';'
        }

        // Metodo para compilar as declarações de variáveis de classe (static ou field), seguindo a estrutura da gramática do Jack
        public void CompileClassVarDec()
        {
            // Identifica se é STATIC ou FIELD
            string keyword = _tokenizer.CurrentToken; // 'static' ou 'field'
            VarKind kind = keyword == "static" ? VarKind.STATIC : VarKind.FIELD;
            ProcessToken();

            // Identifica o TIPO (int, char, boolean ou nome da classe)
            _tokenizer.Advance();
            string type = _tokenizer.CurrentToken;
            ProcessToken();

            // Identifica o NOME da variável
            _tokenizer.Advance();
            string name = _tokenizer.CurrentToken;
            
            // Guarda a variável na tabela de símbolos, associando o nome, tipo e kind (static ou field)
            _symbolTable.Define(name, type, kind);
            ProcessToken();

            // Tratamento para múltiplas variáveis na mesma linha: static int x, y;
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // Processa a vírgula
                    
                    _tokenizer.Advance();
                    name = _tokenizer.CurrentToken; // Pega o próximo nome
                    
                    // Guarda a variável na tabela de símbolos
                    _symbolTable.Define(name, type, kind);
                    ProcessToken();
                }
            }

            ProcessToken(); // Processa o ';'
        }

        // Metodo para compilar sub-rotinas (constructor, function ou method), seguindo a estrutura da gramática do Jack
        public void CompileSubroutine()
        {
            _writer.WriteLine("<subroutineDec>");
            
            // Processa a palavra-chave da sub-rotina: constructor, function ou method
            ProcessToken(); 
            
            // Tipo de retorno: void, tipo primitivo ou nome de classe
            _tokenizer.Advance();
            ProcessToken();

            // Nome da sub-rotina
            _tokenizer.Advance();
            ProcessToken();

            // Busca o símbolo de abertura de parâmetros '('
            _tokenizer.Advance();
            ProcessToken();

            CompileParameterList();
            
            // O CompileParameterList parou no ')', então apenas processamos ele
            ProcessToken(); 

            // Chamada para compilar o corpo da sub-rotina, que inclui as variáveis locais e os comandos
            CompileSubroutineBody();

            _writer.WriteLine("</subroutineDec>");
        }
        
        // Metodo para compilar a lista de parâmetros de uma sub-rotina, seguindo a estrutura da gramática do Jack
        public void CompileParameterList()
        {
            _writer.WriteLine("<parameterList>");
            _tokenizer.Advance();
            
            // Enquanto não encontrar o fecha parênteses, processa os tipos e nomes
            while (_tokenizer.CurrentToken != ")")
            {
                ProcessToken(); // Tipo ou Vírgula
                _tokenizer.Advance();
            }

            _writer.WriteLine("</parameterList>");
        }

        // Metodo para compilar o corpo de uma sub-rotina, seguindo a estrutura da gramática do Jack
        private void CompileSubroutineBody()
        {
            _writer.WriteLine("<subroutineBody>");
            
            // Abre chave '{'
            _tokenizer.Advance();
            ProcessToken();

            _tokenizer.Advance();
            // Enquanto houver variáveis locais 'var', chama CompileVarDec
            while (_tokenizer.CurrentToken == "var")
            {
                //_tokenizer.Advance();
                CompileVarDec();
            }

            // O token atual e o primeiro statement ou '}'
            if (_tokenizer.CurrentToken != "}")
            {
                CompileStatements();
            }
            
            // Fecha chave '}'
            ProcessToken();
            _tokenizer.Advance();
            _writer.WriteLine("</subroutineBody>");
        }

        // Metodo para compilar os comandos dentro do corpo de uma sub-rotina, seguindo a estrutura da gramática do Jack
        public void CompileStatements()
        {
            _writer.WriteLine("<statements>");

            // Processa os comandos enquanto não encontrar o fechamento do bloco (})
            while (true)
            {
                string token = _tokenizer.CurrentToken;
                
                if (token == "let") CompileLet();
                else if (token == "if") CompileIf();
                else if (token == "while") CompileWhile();
                else if (token == "do") CompileDo();
                else if (token == "return") CompileReturn();
                else break; // Se não for nenhum comando, sai do loop (provavelmente achou '}')

                // Após processar um comando, precisamos avançar para ver o próximo
                //if (_tokenizer.HasMoreTokens()) 
                //{
                //    _tokenizer.Advance();
                //}
                //else break;
            }

            _writer.WriteLine("</statements>");
        }

        // Método para compilar um comando de retorno, seguindo a estrutura da gramática do Jack
        public void CompileReturn()
        {
            _writer.WriteLine("<returnStatement>");
            ProcessToken(); // chave 'return'

            _tokenizer.Advance();
            if (_tokenizer.CurrentToken != ";")
            {
                // Desenvolvimento
                ProcessToken(); 
                _tokenizer.Advance();
            }

            ProcessToken(); // simbolo ';'
            _tokenizer.Advance();
            _writer.WriteLine("</returnStatement>");
        }

        // Método para compilar um comando de let, seguindo a estrutura da gramática do Jack
        public void CompileLet()
        {
            _writer.WriteLine("<letStatement>");
            
            // Processa 'let'
            ProcessToken();

            // Verificamos varName
            _tokenizer.Advance();
            ProcessToken();

            // Verificamos se é um array: '[' expression ']'
            _tokenizer.Advance();
            if (_tokenizer.CurrentToken == "[")
            {
                ProcessToken(); // '['
                
                // Desenvolvimento
                _tokenizer.Advance();
                ProcessToken(); // Índice ou variável
                
                _tokenizer.Advance();
                ProcessToken(); // ']'
                _tokenizer.Advance();
            }

            // Busca '='
            ProcessToken();
            _tokenizer.Advance();
            // Expression para o valor a ser atribuído
            while (_tokenizer.CurrentToken != ";")
            {
                //if (_tokenizer.CurrentToken == ";") break;
                ProcessToken();
                _tokenizer.Advance();
            }

            // Adiciona ';'
            ProcessToken();
            _tokenizer.Advance();

            _writer.WriteLine("</letStatement>");
        }

        // Método para compilar um comando de do
        public void CompileDo()
        {
            _writer.WriteLine("<doStatement>");
            ProcessToken(); // 'do'

            // No Jack, após o 'do' vem uma chamada de função
            // Processar os tokens até o ';'
            _tokenizer.Advance();
            while (_tokenizer.CurrentToken != ";")
            {
                //if (_tokenizer.CurrentToken == ";") break;
                ProcessToken();
                _tokenizer.Advance();
            }

            ProcessToken(); // ';'
            _tokenizer.Advance();
            _writer.WriteLine("</doStatement>");
        }


        public void CompileIf()
        {
            _writer.WriteLine("<ifStatement>");
            
            // Veriffica se há 'if'
            ProcessToken();

            // Busca '('
            _tokenizer.Advance();
            ProcessToken();

            // Identifica a Condição (Expression)
            _tokenizer.Advance();
            while (_tokenizer.CurrentToken != ")")
            {
                ProcessToken();
                _tokenizer.Advance();
            }
            ProcessToken(); // Para processar o ')'

            // Verifica '{' (Início do bloco if)
            _tokenizer.Advance();
            ProcessToken();

            // Chamada RECURSIVA para processar os comandos dentro do if
            _tokenizer.Advance();
            CompileStatements();

            // '}' (Fim do bloco if)
            ProcessToken();
            _tokenizer.Advance();

            // Tratamento do 'else'
            if (_tokenizer.CurrentToken == "else")
            {
                ProcessToken(); // 'else'
                
                _tokenizer.Advance();
                ProcessToken(); // '{'
                
                _tokenizer.Advance();
                CompileStatements(); // Comandos dentro do else
                
                ProcessToken(); // '}'
                _tokenizer.Advance();
            }

            _writer.WriteLine("</ifStatement>");
        }


        public void CompileWhile()
        {
            _writer.WriteLine("<whileStatement>");
            
            // Busca 'while'
            ProcessToken();

            // Identifica '('
            _tokenizer.Advance();
            ProcessToken();

            // Avalia Condição (Expression)
            // Avançamos até fechar o parêntese da condição
            _tokenizer.Advance();
            while (_tokenizer.CurrentToken != ")")
            {
                ProcessToken();
                _tokenizer.Advance();
            }

            ProcessToken(); // Para processar o ')' 
            //'{' (Início do bloco de repetição)
            _tokenizer.Advance();
            ProcessToken();

            // Chamada RECURSIVA para os comandos dentro do while
            _tokenizer.Advance();
            CompileStatements();

            // '}' (Fim do bloco while)
            ProcessToken();
            _tokenizer.Advance();

            _writer.WriteLine("</whileStatement>");
        }
    }
}