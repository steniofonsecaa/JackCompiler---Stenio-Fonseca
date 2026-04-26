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

        private void proceessToken()
        {
            var type = _tokenizer.GetTokenType();
            // Converte o tipo do token para a tag XML correspondente
            string tag = type.ToString().ToLower().Replace("_const", "Constant"); 

            // Tratamento de caracteres especiais para a saída XML
            string valor = _tokenizer.CurrentToken;
            if (valor == "<") valor = "&lt;";
            else if (valor == ">") valor = "&gt;";
            else if (valor == "\"") valor = "&quot;";
            else if (valor == "&") valor = "&amp;";

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
                if (_tokenizer.CurrentToken == "}") break;''
                ProcessToken();
            }

            // Fechar a chave '}'
            ProcessToken();

            _writer.WriteLine("</class>");
        }
    }
}