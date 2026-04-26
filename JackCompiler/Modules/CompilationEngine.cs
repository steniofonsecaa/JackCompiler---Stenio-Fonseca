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

                // Se achar '}', a classe acabou
                if (_tokenizer.CurrentToken == "}") break;

                // Se encontrar 'static' ou 'field', é chamado a regra específica de variáveis de classe
                if (_tokenizer.CurrentToken == "static" || _tokenizer.CurrentToken == "field")
                {
                    CompileClassVarDec(); 
                }
                // Se encontrar 'constructor', 'function' ou 'method', é chamado a regra específica de sub-rotinas
                else if (_tokenizer.CurrentToken == "constructor" || 
                        _tokenizer.CurrentToken == "function" || 
                        _tokenizer.CurrentToken == "method")
                {
                    CompileSubroutine();
                }
                // Se encontrar 'var', e chamado a regra específica de variáveis
                else if (_tokenizer.CurrentToken == "var")
                {
                    CompileVarDec(); 
                }
                else
                {
                    // Se não for var, sera processado normalmente
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
            
            // Escreve 'function', 'method' ou 'constructor'
            ProcessToken();

            // Tipo de retorno (void, int, etc.)
            _tokenizer.Advance();
            ProcessToken();

            // Nome da sub-rotina
            _tokenizer.Advance();
            ProcessToken();

            // Abre parênteses '('
            _tokenizer.Advance();
            ProcessToken();

            // Chamada da lista de paramentros (mesmo que seja vazia)
            CompileParameterList();
            
            // Fecha parênteses ')'
            _tokenizer.Advance();
            ProcessToken();

            // Corpo da sub-rotina
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
                    ProcessToken();
                }
            }
            
            // Fecha chave '}'
            _tokenizer.Advance();
            ProcessToken();

            _writer.WriteLine("</subroutineBody>");
        }
    }
}