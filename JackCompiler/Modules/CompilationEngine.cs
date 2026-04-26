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

            // Ler apenas os valores
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == "}") break;
                ProcessToken();
            }

            // Fechar a chave '}'
            ProcessToken();

            _writer.WriteLine("</class>");
        }

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
    }
}