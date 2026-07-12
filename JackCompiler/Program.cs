using System;
using System.IO;
using JackCompiler.Modules;

namespace JackCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Erro: Por favor, forneça o caminho de um ficheiro .jack");
                Console.WriteLine("Uso: dotnet run -- <caminho/para/ficheiro.jack>");
                return;
            }

            string inputPath = args[0];

            if (File.Exists(inputPath) && inputPath.EndsWith(".jack"))
            {
                ProcessFile(inputPath);
            }
            else
            {
                Console.WriteLine($"Erro: O ficheiro '{inputPath}' não é um ficheiro .jack válido.");
            }
        }

        static void ProcessFile(string filePath)
        {
            // O analisador sintático gera o ficheiro com a extensão final .xml
            string outputPath = Path.ChangeExtension(filePath, ".xml"); 
            
            Console.WriteLine($"A iniciar a análise sintática de: {Path.GetFileName(filePath)}...");

            JackTokenizer tokenizer = new JackTokenizer(filePath);
            
            // O CompilationEngine assume o controle total e gera o XML estruturado
            CompilationEngine engine = new CompilationEngine(tokenizer, outputPath);
            engine.Close(); // Fecha o arquivo garantindo que foi salvo corretamente
            
            Console.WriteLine($"Ficheiro XML de sintaxe gerado com sucesso: {Path.GetFileName(outputPath)}");
        }
    }
}