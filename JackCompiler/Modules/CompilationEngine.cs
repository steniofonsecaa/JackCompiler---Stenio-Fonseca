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

        // Método para compilar uma classe, seguindo a estrutura da gramática do Jack

        public void CompileClass()
        {
            _writer.WriteLine("<class>");
            
            // Primeiro componente: 'class' (keyword)
            _tokenizer.Advance();
            WriteTag("keyword", _tokenizer.CurrentToken);

            // Segundo componente: nome da classe (identifier)
            _tokenizer.Advance();
            WriteTag("identifier", _tokenizer.CurrentToken);

            // Terceiro componente: '{' (symbol)    
            _tokenizer.Advance();
            WriteTag("symbol", _tokenizer.CurrentToken);

            // TODO: Aqui virão as chamadas para CompileClassVarDec e CompileSubroutine
            
            // Teste para verificar se o tokenizador está avançando corretamente até o final da classe
            _tokenizer.Advance(); // Esperando o '}'
            WriteTag("symbol", _tokenizer.CurrentToken);

            _writer.WriteLine("</class>");
        }
    }
}