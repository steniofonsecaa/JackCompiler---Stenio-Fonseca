using JackCompiler.Modules;

if (args.Length == 0)
{
    Console.WriteLine("Erro: Por favor, forneça o caminho do arquivo .jack ou diretório.");
    return;
}

//var tokenizer = new JackTokenizer(args[0]);
string inputPath = args[0];

if (!File.Exists(inputPath))
{
    Console.WriteLine($"Erro: Arquivo não encontrado em {inputPath}");
    return;
}

// Processamento do arquivo .jack e geração do arquivo XML de tokens
try
{
    var tokenizer = new JackTokenizer(inputPath);
    string outputPath = inputPath.Replace(".jack", "T.xml");
    
    using (StreamWriter writer = new StreamWriter(outputPath))
    {
        writer.WriteLine("<tokens>");
        
        while (tokenizer.HasMoreTokens())
        {
            tokenizer.Advance();
            // Escreve a tag XML no arquivo
            writer.WriteLine(tokenizer.GetTokenTag());
        }
        
        writer.WriteLine("</tokens>");
    }

    Console.WriteLine($"\nSucesso! Arquivo XML gerado em: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro fatal: {ex.Message}");
}