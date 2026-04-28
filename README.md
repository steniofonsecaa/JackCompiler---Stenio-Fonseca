# JackAnalyzer - Compilador Jack (Análise Sintática)

Este projeto consiste no desenvolvimento de um Analisador Lexico Analisador Sintático (Parser) para a linguagem Jack, como parte dos requisitos da disciplina de Compiladores e do currículo **Nand2Tetris**. O programa realiza a leitura de arquivos `.jack` e gera uma estrutura hierárquica em formato XML que representa a gramática da linguagem.

## Grupo
* **Nome**: Stenio Moraes Fonseca
* **Matrícula**: 2025

## Linguagem Utilizada
* **C#** 

## Instruções para Compilar e Executar

O projeto utiliza o SDK do .NET. Verificar se possui o instalado em sua maquina

1. **Compilação**:
   Navega até à pasta raiz do projeto (onde se encontra o ficheiro `.slnx` ou o diretório `JackCompiler`) e executa:
   ```bash
   dotnet build

2. **Execução**:
    Para processar um ficheiro .jack específico, utiliza o seguinte comando:
    '''bash
    dotnet run -- JackCompiler/Atributos.jack

    **Exemplo de Uso**
    ''bash
    dotnet run -- caminho/do/teu_ficheiro.jack
        Entrada: Ficheiro.jack
        Saída: Ficheiro.xml

    **Nome do Ficheiro de Saída**
    O analisador gera um ficheiro de saída com o mesmo nome do ficheiro de entrada, mas com a extensão .xml, localizado no mesmo diretório de origem.

## Status da Validação

* **Status:** Funcional, com suporte a estruturas recursivas.
* O analisador processa corretamente:
    * Declaração de classes e variáveis de classe (`static`, `field`).
    * Sub-rotinas (`function`, `method`, `constructor`) e listas de parâmetros.
    * Blocos de comandos (`let`, `do`, `if`, `while`, `return`) de forma aninhada.
* **Validação:** O motor de compilação foi ajustado para garantir que a hierarquia de tags (como `<statements>` e `<subroutineBody>`) respeite rigorosamente a gramática Jack.

## Desafios Enfrentados na Unidade

O maior desafio técnico nesta unidade foi a implementação da descida recursiva para o processamento de comandos. Garantir que estruturas como um `if` dentro de um `while` fossem fechadas corretamente exigiu um controlo minucioso do ponteiro do `Tokenizer`.

A sincronização entre o método `CompileStatements` e as regras individuais de cada comando foi crítica; pequenas falhas no avanço dos tokens (`Advance()`) causavam o "atropelamento" de símbolos de fechamento como `}` ou `;`. 