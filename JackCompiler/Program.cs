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
                Console.WriteLine("Erro: Por favor, forneça o caminho de um ficheiro .jack ou uma pasta.");
                Console.WriteLine("Uso: dotnet run -- <caminho>");
                return;
            }

            string inputPath = args[0];

            // Verifica se o caminho passado é um arquivo específico
            if (File.Exists(inputPath) && inputPath.EndsWith(".jack"))
            {
                ProcessFile(inputPath);
            }
            // Verifica se o caminho passado é um diretório (pasta)
            else if (Directory.Exists(inputPath))
            {
                Console.WriteLine($"A analisar diretório: {inputPath}");
                string[] jackFiles = Directory.GetFiles(inputPath, "*.jack");
                
                if (jackFiles.Length == 0)
                {
                    Console.WriteLine("Nenhum ficheiro .jack encontrado no diretório.");
                    return;
                }

                foreach (string file in jackFiles)
                {
                    ProcessFile(file);
                }
            }
            else
            {
                Console.WriteLine($"Erro: Caminho '{inputPath}' não encontrado ou inválido.");
            }
        }

        static void ProcessFile(string filePath)
        {
            // O gerador de código final emite arquivos .vm
            string outputPath = Path.ChangeExtension(filePath, ".vm"); 
            
            Console.WriteLine($"Compilando: {Path.GetFileName(filePath)} -> {Path.GetFileName(outputPath)}");

            JackTokenizer tokenizer = new JackTokenizer(filePath);
            
            // O CompilationEngine assume o controle e gera as instruções da máquina virtual
            CompilationEngine engine = new CompilationEngine(tokenizer, outputPath);
            engine.Close(); 
        }
    }
}