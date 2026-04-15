using System.Text.RegularExpressions;
using JackCompiler.Enums;

namespace JackCompiler.Modules
{
    public class JackTokenizer
    {
        // Regex para Símbolos da linguagem Jack
        private static readonly string SymbolPattern = @"[{}()\[\].,;+\-*/&|<>=~]";
        
        // Regex para Números (0 a 32767)
        private static readonly string IntPattern = @"\d+";

        public void ProcessarLinha(string linha)
        {
            // Exemplo simples: verificando se a linha contém um símbolo
            if (Regex.IsMatch(linha, SymbolPattern))
            {
                var match = Regex.Match(linha, SymbolPattern);
                Console.WriteLine($"Símbolo encontrado: {match.Value}");
            }
        }
    }
}