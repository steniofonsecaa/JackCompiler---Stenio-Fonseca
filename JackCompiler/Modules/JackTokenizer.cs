using System.Text.RegularExpressions;
using JackCompiler.Enums;
using System.Collections.Generic;


namespace JackCompiler.Modules
{
    public class JackTokenizer
    {
        private int _currentIndex = -1;
        private string _currentToken = "";
        
        public string CurrentToken => _currentToken;

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
            //Console.WriteLine($"\nTokens encontrados: {_tokens.Count}");
            //foreach (var t in _tokens)
            //{
            //    Console.WriteLine($"Token: [{t}]");
            //}
        }

        // Procura por mais tokens no código
        public bool HasMoreTokens()
        {
            return _currentIndex + 1 < _tokens.Count;
        }

        // Avança para o próximo token
        public void Advance()
        {
            if (HasMoreTokens())
            {
                _currentIndex++;
                _currentToken = _tokens[_currentIndex];
            }
        }

        // Retorna o tipo do token atual
        public TokenType GetTokenType()
        {
            if (Keywords.Contains(_currentToken))
                return TokenType.KEYWORD;
            
            if (Regex.IsMatch(_currentToken, @"[{}()\[\].,;+\-*/&|<>=~]"))
                return TokenType.SYMBOL;
                
            if (Regex.IsMatch(_currentToken, @"^\d+$"))
                return TokenType.INT_CONST;
                
            if (_currentToken.StartsWith("\""))
                return TokenType.STRING_CONST;
                
            return TokenType.IDENTIFIER;
        }

        // Retorna o token formatado para saída XML
        public string GetTokenTag()
        {
            var tipo = GetTokenType();
            string tag = tipo.ToString().ToLower().Replace("_const", "Constant");
            
            // Substituição de caracteres especiais para a saída XML
            string valor = CurrentToken;
            if (valor == "<") valor = "&lt;";
            else if (valor == ">") valor = "&gt;";
            else if (valor == "\"") valor = "&quot;";
            else if (valor == "&") valor = "&amp;";

            return $"<{tag}> {valor} </{tag}>";
        }
    }
}