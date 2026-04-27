using JackCompiler.Enums;

namespace JackCompiler.Modules
{
    public class CompilationEngine
    {
        private JackTokenizer _tokenizer;
        private StreamWriter _writer;

        // Metodo construtor que recebe o tokenizer e o caminho do arquivo de saída para inicializar o StreamWriter
        public CompilationEngine(JackTokenizer tokenizer, string outputPath)
        {
            _tokenizer = tokenizer;
            _writer = new StreamWriter(outputPath);
        }

        // Fecha o StreamWriter para garantir que o arquivo seja salvo corretamente
        public void Close() => _writer.Close();

        // Escreve uma tag XML formatada no arquivo de saída
        private void WriteTag(string tag, string value)
        {
            _writer.WriteLine($"<{tag}> {value} </{tag}>");
        }

        // Processa o token atual do tokenizer e escreve a tag XML correspondente
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

            _writer.WriteLine($"<{tag}> {valor} </{tag}>");
        }

        // Método para compilar uma classe, seguindo a estrutura da gramática do Jack   
        public void CompileClass()
        {
            _writer.WriteLine("<class>");
            
            // 1. 'class'
            _tokenizer.Advance();
            ProcessToken();

            // 2. className
            _tokenizer.Advance();
            ProcessToken();

            // 3. '{'
            _tokenizer.Advance();
            ProcessToken();

            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                
                // Remove espaços em branco para evitar erro de comparação
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
                else if (token == "var")
                {
                    CompileVarDec(); 
                }
                else
                {
                    ProcessToken();
                }
            }

            ProcessToken(); // Escreve o '}' final
            _writer.WriteLine("</class>");
        }

        // Método para compilar variáveis locais dentro de sub-rotinas, seguindo a estrutura da gramática do Jack
        public void CompileVarDec()
        {
            _writer.WriteLine("<varDec>");
            
            // Processa a palavra-chave 'var'
            ProcessToken(); 

            // Tipo (int, char, boolean ou className)
            _tokenizer.Advance();
            ProcessToken();

            // Nome da variável
            _tokenizer.Advance();
            ProcessToken();

            // Suporte para múltiplas variáveis declaradas na mesma linha (ex: var int x, y, z;)
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // Escreve a vírgula
                    _tokenizer.Advance();
                    ProcessToken(); // Escreve o próximo nome
                }
            }

            // Escreve o ';'
            ProcessToken();
    
            _writer.WriteLine("</varDec>");
        }

        // Metodo para compilar variáveis de classe (static ou field), seguindo a estrutura da gramática do Jack
        public void CompileClassVarDec()
        {
            _writer.WriteLine("<classVarDec>");
            
            // Processa 'static' ou 'field'
            ProcessToken();

            // Tipo (int, char, boolean ou className)
            _tokenizer.Advance();
            ProcessToken();

            // Apresenta o nome da variável
            _tokenizer.Advance();
            ProcessToken();

            // Tratamento para múltiplas variáveis na mesma linha: static int x, y;
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // Para escrever a vírgula
                    _tokenizer.Advance();
                    ProcessToken(); // Para escrever o próximo nome
                }
            }

            // Finaliza a declaração com ';'
            ProcessToken();

            _writer.WriteLine("</classVarDec>");
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

            // Enquanto houver variáveis locais 'var', chama CompileVarDec
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                
                if (_tokenizer.CurrentToken == "var")
                {
                    CompileVarDec();
                }
                else if (_tokenizer.CurrentToken == "}")
                {
                    break;
                }
                else
                {
                    // Se não é var nem }, começam os statements!
                    CompileStatements();
                    // O CompileStatements para no '}', então saímos do loop
                    if (_tokenizer.CurrentToken == "}") break;
                }
            }
            
            // Fecha chave '}'
            ProcessToken();
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
                if (_tokenizer.HasMoreTokens()) 
                {
                    _tokenizer.Advance();
                }
                else break;
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

            // Expression para o valor a ser atribuído
            while (_tokenizer.CurrentToken != ";")
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                ProcessToken();
            }

            // Adiciona ';'
            ProcessToken();

            _writer.WriteLine("</letStatement>");
        }

        // Método para compilar um comando de do
        public void CompileDo()
        {
            _writer.WriteLine("<doStatement>");
            ProcessToken(); // 'do'

            // No Jack, após o 'do' vem uma chamada de função
            // Processar os tokens até o ';'
            while (_tokenizer.CurrentToken != ";")
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                ProcessToken();
            }

            ProcessToken(); // ';'
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
            while (_tokenizer.CurrentToken != ")")
            {
                _tokenizer.Advance();
                ProcessToken();
            }

            // Verifica '{' (Início do bloco if)
            _tokenizer.Advance();
            ProcessToken();

            // Chamada RECURSIVA para processar os comandos dentro do if
            CompileStatements();

            // '}' (Fim do bloco if)
            ProcessToken();

            // Tratamento do 'else'
            _tokenizer.Advance();
            if (_tokenizer.CurrentToken == "else")
            {
                ProcessToken(); // 'else'
                
                _tokenizer.Advance();
                ProcessToken(); // '{'
                
                CompileStatements(); // Comandos dentro do else
                
                ProcessToken(); // '}'
            }
            else
            {
            
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
            while (_tokenizer.CurrentToken != ")")
            {
                _tokenizer.Advance();
                ProcessToken();
            }

            //'{' (Início do bloco de repetição)
            _tokenizer.Advance();
            ProcessToken();

            // Chamada RECURSIVA para os comandos dentro do while
            CompileStatements();

            // '}' (Fim do bloco while)
            ProcessToken();

            _writer.WriteLine("</whileStatement>");
        }
    }
}