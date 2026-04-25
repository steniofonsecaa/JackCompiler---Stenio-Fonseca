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

// Verificação para garantir que o arquivo é um .jack
try
{
    // Instanciação do tokenizador com o caminho do arquivo .jack
    var tokenizer = new JackTokenizer(inputPath);
    
    Console.WriteLine("--- EXECUTANDO ANALISADOR ---");
    
    while (tokenizer.HasMoreTokens())
    {
        tokenizer.Advance();
        var tipo = tokenizer.GetTokenType();
        
        // Formatação para saída dos tokens e seus tipos
        Console.WriteLine(string.Format("Token: {0,-15} | Tipo: {1}", 
            tokenizer.CurrentToken, tipo));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro fatal: {ex.Message}");
}