using System.Text.RegularExpressions;

namespace JackCompiler.Modules
{
    public class JackTokenizer
    {
        // Conjunto de palavras-chave do Jack
        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "class", "constructor", "function", "method", "field", "static", "var",
            "int", "char", "boolean", "void", "true", "false", "null", "this",
            "let", "do", "if", "else", "while", "return"
        };

        private string _rawContent;

        private List<string> _tokens = new List<string>();

        public JackTokenizer(string filePath)
        {
            // Leitura de todo o conteúdo do arquivo .jack
            _rawContent = File.ReadAllText(filePath);
            
            // Remoção de comentários do código
            _rawContent = RemoveComments(_rawContent);

            // Chamada para tokenizar o conteúdo limpo
            Tokenize();
            
            // Teste para verificar o conteúdo limpo
            // Console.WriteLine("--- Código Limpo ---");
            // Console.WriteLine(_rawContent);
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
        private void Tokenize()
        {
            // Regex para identificar tokens: palavras-chave, símbolos, identificadores, números e strings
            string pattern = @"("".*?""|\d+|[a-zA-Z_][a-zA-Z0-9_]*|[{}()\[\].,;+\-*/&|<>=~])";
            
            var matches = Regex.Matches(_rawContent, pattern);

            foreach (Match match in matches)
            {
                _tokens.Add(match.Value);
            }
            
            // Teste para verificar os tokens encontrados
            Console.WriteLine($"\nTokens encontrados: {_tokens.Count}");
            foreach (var t in _tokens)
            {
                Console.WriteLine($"Token: [{t}]");
            }
        }
    }
}