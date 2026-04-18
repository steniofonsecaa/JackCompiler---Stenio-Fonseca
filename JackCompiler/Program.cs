using JackCompiler.Modules;

if (args.Length == 0)
{
    Console.WriteLine("Erro: Por favor, forneça o caminho do arquivo .jack ou diretório.");
    return;
}

var tokenizer = new JackTokenizer(args[0]);
string inputPath = args[0];

try 
{
    // Aviso de qual arquivo está sendo processado
    Console.WriteLine($"Lendo o arquivo: {inputPath}");
    
 
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao processar: {ex.Message}");
}