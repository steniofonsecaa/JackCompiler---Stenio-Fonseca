using System;
using System.IO;
using System.Collections.Generic;
using JackCompiler.Modules;

if (args.Length == 0)
{
    Console.WriteLine("Erro: Por favor, forneça o caminho do arquivo .jack ou diretório.");
    return;
}

string inputPath = args[0];
List<string> jackFiles = new List<string>();

// Verifica se a entrada é um diretório ou um arquivo único
if (Directory.Exists(inputPath))
{
    // Pega todos os arquivos .jack dentro do diretório
    jackFiles.AddRange(Directory.GetFiles(inputPath, "*.jack", SearchOption.AllDirectories));
}
else if (File.Exists(inputPath) && inputPath.EndsWith(".jack"))
{
    // É um arquivo único
    jackFiles.Add(inputPath);
}
else
{
    Console.WriteLine($"Erro: Caminho inválido ou arquivo não encontrado: {inputPath}");
    return;
}

if (jackFiles.Count == 0)
{
    Console.WriteLine("Aviso: Nenhum arquivo .jack encontrado no caminho especificado.");
    return;
}

Console.WriteLine($"--- INICIANDO COMPILAÇÃO ({jackFiles.Count} arquivo(s)) ---");

// Compila cada arquivo encontrado
foreach (string file in jackFiles)
{
    try
    {
        var tokenizer = new JackTokenizer(file);
        
        // Saida do arquivo .vm (mesmo nome do .jack, mas com extensão .vm)
        string vmOutput = file.Replace(".jack", ".vm"); 
        
        // Cria o motor de compilação e inicia a compilação
        // O CompilationEngine é responsável por gerar o código VM a partir dos tokens do JackTokenizer
        var engine = new CompilationEngine(tokenizer, vmOutput);
        
        engine.CompileClass();
        engine.Close();
        
        Console.WriteLine($"[OK] {Path.GetFileName(file)} -> {Path.GetFileName(vmOutput)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERRO] Falha ao compilar {Path.GetFileName(file)}: {ex.Message}");
    }
}

Console.WriteLine("\n--- COMPILAÇÃO CONCLUÍDA COM SUCESSO ---");