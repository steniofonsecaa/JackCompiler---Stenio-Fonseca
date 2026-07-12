using System;
using System.IO;
using JackCompiler.Modules;

namespace JackCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Validação da entrada
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
            // O validador do Nand2Tetris espera que o ficheiro de tokens tenha o sufixo "T.xml"
            string outputPath = Path.ChangeExtension(filePath, "T.xml"); 
            
            Console.WriteLine($"A iniciar a análise léxica de: {Path.GetFileName(filePath)}...");

            JackTokenizer tokenizer = new JackTokenizer(filePath);
            
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                // Abre a tag principal exigida pelo validador
                writer.WriteLine("<tokens>");
                
                while (tokenizer.HasMoreTokens())
                {
                    tokenizer.Advance();
                    // Escreve a tag formatada de cada token
                    writer.WriteLine(tokenizer.GetTokenTag());
                }
                
                // Fecha a tag principal
                writer.WriteLine("</tokens>");
            }
            
            Console.WriteLine($"Ficheiro XML de tokens gerado com sucesso: {Path.GetFileName(outputPath)}");
        }
    }
}