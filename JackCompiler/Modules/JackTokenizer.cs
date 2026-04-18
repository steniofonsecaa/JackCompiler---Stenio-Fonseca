using System.Text.RegularExpressions;

namespace JackCompiler.Modules
{
    public class JackTokenizer
    {
        private string _rawContent;

        public JackTokenizer(string filePath)
        {
            // Leitura de todo o conteúdo do arquivo .jack
            _rawContent = File.ReadAllText(filePath);
            
            // Remoção de comentários do código
            _rawContent = RemoveComments(_rawContent);
            
            // Teste para verificar o conteúdo limpo
            Console.WriteLine("--- Código Limpo ---");
            Console.WriteLine(_rawContent);
        }

        private string RemoveComments(string input)
        {
            // Regex para comentários de bloco /* ... */ e /** ... */
            // O RegexOptions.Singleline faz o '.' capturar quebras de linha também
            string blockComments = @"/\*.*?\*/";
            
            // Regex para comentários de linha //
            string lineComments = @"//.*";

            string clean = Regex.Replace(input, blockComments, "", RegexOptions.Singleline);
            clean = Regex.Replace(clean, lineComments, "");

            return clean.Trim();
        }
    }
}