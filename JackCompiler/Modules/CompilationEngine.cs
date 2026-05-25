using JackCompiler.Enums;
using System;

namespace JackCompiler.Modules
{
    public class CompilationEngine
    {
        private JackTokenizer _tokenizer;
        private VMWriter _vmWriter;
        private SymbolTable _symbolTable;
        private string _className; // Para armazenar o nome da classe atual, útil para gerar código VM

        private int _ifCount = 0;
        private int _whileCount = 0;

        // Metodo construtor que agora inicializa o VMWriter e o SymbolTable
        public CompilationEngine(JackTokenizer tokenizer, string outputPath)
        {   
            _tokenizer = tokenizer;
            _vmWriter = new VMWriter(outputPath);
            _symbolTable = new SymbolTable();
        }

        // Metodo Close para fechar o VMWriter e liberar recursos
        public void Close() => _vmWriter.Close();

        // ProcessToken
        private void ProcessToken()
        {
            var type = _tokenizer.GetTokenType();
            string tag = type.ToString().ToLower().Replace("_const", "Constant");
            
            string valor = _tokenizer.CurrentToken;

            // Substitui caracteres especiais para a saída XML
            if (valor == "<") valor = "&lt;";
            else if (valor == ">") valor = "&gt;";
            else if (valor == "&") valor = "&amp;";
            else if (valor == "\"") valor = "&quot;";
        }

        // Método para compilar uma classe, para nao escrever XML e salvar o nome da classe  
        public void CompileClass()
        {
            _tokenizer.Advance(); // 'class'
            ProcessToken();

            _tokenizer.Advance(); // className
            _className = _tokenizer.CurrentToken; // Salva o nome da classe para uso futuro
            ProcessToken();

            _tokenizer.Advance(); // '{'
            ProcessToken();

            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                string token = _tokenizer.CurrentToken.Trim();

                if (token == "}") break;

                if (token == "static" || token == "field")
                {
                    CompileClassVarDec(); 
                }
                else if (token == "constructor" || token == "function" || token == "method")
                {
                    CompileSubroutine(); 
                }
                else
                {
                    ProcessToken();
                }
            }
        }

        // Método para escrever a declaração de variáveis locais, seguindo a estrutura da gramática do Jack
        public void CompileVarDec()
        {
            ProcessToken(); // 'var'
            VarKind kind = VarKind.VAR; // No Jack, 'var' significa variável local

            // Tipo
            _tokenizer.Advance();
            string type = _tokenizer.CurrentToken;
            ProcessToken();

            // Nome
            _tokenizer.Advance();
            string name = _tokenizer.CurrentToken;
            
            // Registra a variável na tabela de símbolos, associando o nome, tipo e kind (local)
            _symbolTable.Define(name, type, kind);
            ProcessToken();

            // Múltiplas variáveis (ex: var int i, j, k;)
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // vírgula
                    
                    _tokenizer.Advance();
                    name = _tokenizer.CurrentToken;
                    
                    _symbolTable.Define(name, type, kind);
                    ProcessToken();
                }
            }

            ProcessToken(); // Processa ';'
        }

        // Metodo para compilar as declarações de variáveis de classe (static ou field), seguindo a estrutura da gramática do Jack
        public void CompileClassVarDec()
        {
            // Identifica se é STATIC ou FIELD
            string keyword = _tokenizer.CurrentToken; // 'static' ou 'field'
            VarKind kind = keyword == "static" ? VarKind.STATIC : VarKind.FIELD;
            ProcessToken();

            // Identifica o TIPO (int, char, boolean ou nome da classe)
            _tokenizer.Advance();
            string type = _tokenizer.CurrentToken;
            ProcessToken();

            // Identifica o NOME da variável
            _tokenizer.Advance();
            string name = _tokenizer.CurrentToken;
            
            // Guarda a variável na tabela de símbolos, associando o nome, tipo e kind (static ou field)
            _symbolTable.Define(name, type, kind);
            ProcessToken();

            // Tratamento para múltiplas variáveis na mesma linha: static int x, y;
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == ";") break;
                
                if (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // Processa a vírgula
                    
                    _tokenizer.Advance();
                    name = _tokenizer.CurrentToken; // Pega o próximo nome
                    
                    // Guarda a variável na tabela de símbolos
                    _symbolTable.Define(name, type, kind);
                    ProcessToken();
                }
            }

            ProcessToken(); // Processa o ';'
        }

        // Metodo para compilar sub-rotinas (constructor, function ou method), seguindo a estrutura da gramática do Jack
        public void CompileSubroutine()
        {
            // Limpa a tabela de símbolos para a nova sub-rotina
            _symbolTable.StartSubroutine();

            ProcessToken(); // 'constructor', 'function' ou 'method'
            string subroutineType = _tokenizer.CurrentToken; // Guardamos o tipo

            // Se for um método, o argumento 0 é a própria instância (this)
            if (subroutineType == "method")
            {
                _symbolTable.Define("this", _className, VarKind.ARG);
            }

            _tokenizer.Advance();
            ProcessToken(); // Tipo de retorno (void, int, etc.)

            _tokenizer.Advance();
            string subroutineName = _tokenizer.CurrentToken; // Nome da função
            ProcessToken(); 

            _tokenizer.Advance();
            ProcessToken(); // '('

            CompileParameterList();
            
            // O CompileParameterList já para no ')'
            ProcessToken(); // ')'

            // Compilaçao do corpo da sub-rotina, passando o nome e tipo para gerar código VM específico
            CompileSubroutineBody(subroutineName, subroutineType);
        }
        
        // Metodo para compilar a lista de parâmetros de uma sub-rotina, seguindo a estrutura da gramática do Jack
        public void CompileParameterList()
        {
            _tokenizer.Advance();
            
            // Se o token for ')', a lista está vazia
            if (_tokenizer.CurrentToken == ")") return;

            // 1º Parâmetro
            string type = _tokenizer.CurrentToken;
            ProcessToken(); // Tipo

            _tokenizer.Advance();
            string name = _tokenizer.CurrentToken;
            
            // Regista na tabela como argumento (ARG)
            _symbolTable.Define(name, type, VarKind.ARG);
            ProcessToken(); // Nome

            // Parâmetros adicionais separados por vírgula
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken != ",") break;
                
                ProcessToken(); // Vírgula
                
                _tokenizer.Advance();
                type = _tokenizer.CurrentToken;
                ProcessToken(); // Tipo
                
                _tokenizer.Advance();
                name = _tokenizer.CurrentToken;
                
                _symbolTable.Define(name, type, VarKind.ARG);
                ProcessToken(); // Nome
            }
        }

        // Metodo para compilar o corpo de uma sub-rotina, seguindo a estrutura da gramática do Jack
        private void CompileSubroutineBody(string subroutineName, string subroutineType)
        {
            _tokenizer.Advance(); 
            ProcessToken(); // '{'

            // Processa TODAS as variáveis locais primeiro
            while (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                if (_tokenizer.CurrentToken == "var")
                {
                    CompileVarDec();
                }
                else
                {
                    break; // Se não for 'var', saímos do loop e partimos para os statements
                }
            }

            // Processo de todas as variáveis locais
            int nLocals = _symbolTable.VarCount(VarKind.VAR);

            // Escreve a instrução de função no VM
            _vmWriter.WriteFunction($"{_className}.{subroutineName}", nLocals);

            // Configurações especiais para 'method' e 'constructor'
            if (subroutineType == "method")
            {
                // Um método precisa de carregar o 'this' (argumento 0) para o ponteiro base
                _vmWriter.WritePush(Segment.ARG, 0);
                _vmWriter.WritePop(Segment.POINTER, 0);
            }
            else if (subroutineType == "constructor")
            {
                // Um construtor precisa de alocar memória para os 'fields'
                int nFields = _symbolTable.VarCount(VarKind.FIELD);
                _vmWriter.WritePush(Segment.CONST, nFields);
                _vmWriter.WriteCall("Memory.alloc", 1);
                _vmWriter.WritePop(Segment.POINTER, 0);
            }

            // Processa os Statements (comandos)
            if (_tokenizer.CurrentToken != "}")
            {
                CompileStatements();
            }

            ProcessToken(); // '}'
        }

        // Metodo para compilar os comandos dentro do corpo de uma sub-rotina, seguindo a estrutura da gramática do Jack
        public void CompileStatements()
        {
            _writer.WriteLine("<statements>");

            // Processa os comandos enquanto não encontrar o fechamento do bloco (})
            while (true)
            {
                string token = _tokenizer.CurrentToken;
                
                if (token == "let") CompileLet();
                else if (token == "if") CompileIf();
                else if (token == "while") CompileWhile();
                else if (token == "do") CompileDo();
                else if (token == "return") CompileReturn();
                else break; // Se não for nenhum comando, sai do loop (provavelmente achou '}')

                // Após processar um comando, precisamos avançar para ver o próximo
                //if (_tokenizer.HasMoreTokens()) 
                //{
                //    _tokenizer.Advance();
                //}
                //else break;
            }

            _writer.WriteLine("</statements>");
        }

        // Método para compilar uma expressão, seguindo a estrutura da gramática do Jack
        public void CompileExpression()
        {
            CompileTerm(); // Compila o primeiro valor
            
            // Enquanto houver operadores matemáticos ou lógicos...
            while (_tokenizer.CurrentToken == "+" || _tokenizer.CurrentToken == "-" || 
                _tokenizer.CurrentToken == "*" || _tokenizer.CurrentToken == "/" || 
                _tokenizer.CurrentToken == "&" || _tokenizer.CurrentToken == "|" || 
                _tokenizer.CurrentToken == "<" || _tokenizer.CurrentToken == ">" || 
                _tokenizer.CurrentToken == "=")
            {
                string op = _tokenizer.CurrentToken;
                
                _tokenizer.Advance(); // Avança o operador
                CompileTerm();        // Compila o segundo valor
                
                // Empurra o comando aritmético correspondente ao operador para o arquivo .vm
                switch (op)
                {
                    case "+": _vmWriter.WriteArithmetic(Command.ADD); break;
                    case "-": _vmWriter.WriteArithmetic(Command.SUB); break;
                    case "*": _vmWriter.WriteCall("Math.multiply", 2); break;
                    case "/": _vmWriter.WriteCall("Math.divide", 2); break;  
                    case "&": _vmWriter.WriteArithmetic(Command.AND); break;
                    case "|": _vmWriter.WriteArithmetic(Command.OR); break;
                    case "<": _vmWriter.WriteArithmetic(Command.LT); break;
                    case ">": _vmWriter.WriteArithmetic(Command.GT); break;
                    case "=": _vmWriter.WriteArithmetic(Command.EQ); break;
                }
            }
        }

        public void CompileTerm()
        {
            string token = _tokenizer.CurrentToken;
            
            // Números inteiros
            if (int.TryParse(token, out int val))
            {
                _vmWriter.WritePush(Segment.CONST, val);
                _tokenizer.Advance();
            }
            // Strings
            else if (token.StartsWith("\""))
            {
                string str = token.Replace("\"", ""); // Remove as aspas
                _vmWriter.WritePush(Segment.CONST, str.Length);
                _vmWriter.WriteCall("String.new", 1);
                
                foreach (char c in str)
                {
                    _vmWriter.WritePush(Segment.CONST, (int)c);
                    _vmWriter.WriteCall("String.appendChar", 2);
                }
                _tokenizer.Advance();
            }
            // Constantes Palavra-Chave (true, false, null, this)
            else if (token == "true")
            {
                _vmWriter.WritePush(Segment.CONST, 1);
                _vmWriter.WriteArithmetic(Command.NEG); // No Jack, true é -1
                _tokenizer.Advance();
            }
            else if (token == "false" || token == "null")
            {
                _vmWriter.WritePush(Segment.CONST, 0); // False e Null são 0
                _tokenizer.Advance();
            }
            else if (token == "this")
            {
                _vmWriter.WritePush(Segment.POINTER, 0);
                _tokenizer.Advance();
            }
            // Operadores Unários
            else if (token == "-" || token == "~")
            {
                _tokenizer.Advance();
                CompileTerm(); // Recursividade para resolver o termo a seguir
                
                if (token == "-") _vmWriter.WriteArithmetic(Command.NEG);
                else _vmWriter.WriteArithmetic(Command.NOT);
            }
            // Parênteses agrupados (ex: (2 + 3) )
            else if (token == "(")
            {
                ProcessToken(); // Lê o '('
                _tokenizer.Advance();
                CompileExpression();
                ProcessToken(); // Lê o ')'
                _tokenizer.Advance();
            }
            // Variáveis, Arrays ou Chamadas de Função
            else
            {
                string identifier = token;
                _tokenizer.Advance();

                // Verifica se é uma atribuição de array (ex: arr[5])
                if (_tokenizer.CurrentToken == "[") 
                {
                    ProcessToken(); // '['
                    _tokenizer.Advance();
                    CompileExpression(); // Índice
                    ProcessToken(); // ']'
                    _tokenizer.Advance();

                    // Lógica VM para ler valor de um array
                    VarKind kind = _symbolTable.KindOf(identifier);
                    int index = _symbolTable.IndexOf(identifier);
                    _vmWriter.WritePush(GetSegment(kind), index);
                    
                    _vmWriter.WriteArithmetic(Command.ADD); // Endereço base + índice
                    
                    _vmWriter.WritePop(Segment.POINTER, 1); // Põe no THAT
                    _vmWriter.WritePush(Segment.THAT, 0);   // Recupera o valor
                }
                // Verifica se é uma chamada de função (ex: myFunction() ou obj.method())
                else if (_tokenizer.CurrentToken == "(" || _tokenizer.CurrentToken == ".") 
                {
                    CompileSubroutineCall(identifier);
                }
                // Caso seja apenas uma variável simples, empurra seu valor para a pilha
                else 
                {
                    VarKind kind = _symbolTable.KindOf(identifier);
                    int index = _symbolTable.IndexOf(identifier);
                    _vmWriter.WritePush(GetSegment(kind), index);
                }
            }
        }

        // Método para compilar um comando de retorno, seguindo a estrutura da gramática do Jack
        public void CompileReturn()
        {
            ProcessToken();
            _tokenizer.Advance(); // Avança para ver o que vem a seguir

            // Se não for ';', significa que há um valor a ser retornado (ex: return x;)
            if (_tokenizer.CurrentToken != ";")
            {
                CompileExpression(); 
            }
            else
            {
                // Se for void (return;), a máquina virtual exige que empurremos 0 para a pilha
                _vmWriter.WritePush(Segment.CONST, 0);
            }

            _vmWriter.WriteReturn(); // Escreve 'return' no .vm

            ProcessToken(); // Processa o ';' que encerra o comando
        }

        // Método para compilar um comando de let, seguindo a estrutura da gramática do Jack
        public void CompileLet()
        {
            ProcessToken(); // 'let'

            _tokenizer.Advance();
            string varName = _tokenizer.CurrentToken; // Nome da variável
            ProcessToken(); 

            bool isArray = false;
            _tokenizer.Advance();

            // Verifica se é uma atribuição de array
            if (_tokenizer.CurrentToken == "[")
            {
                isArray = true;
                ProcessToken();
                
                _tokenizer.Advance();
                CompileExpression();
                
                // Empurra o endereço base do array para a pilha
                VarKind kind = _symbolTable.KindOf(varName);
                int index = _symbolTable.IndexOf(varName);
                _vmWriter.WritePush(GetSegment(kind), index);
                
                // Soma a base com o índice (base + índice)
                _vmWriter.WriteArithmetic(Command.ADD); 
                
                ProcessToken();
                _tokenizer.Advance();
            }

            ProcessToken();
            _tokenizer.Advance();
            
            // Compila o valor que vai ser atribuído
            CompileExpression();
            
            // Guarda o valor
            if (isArray)
            {
                // Lógica da VM para guardar valor num array
                // Guardar temporariamente o valor que queremos atribuir
                _vmWriter.WritePop(Segment.TEMP, 0); 
                // Apontar o THAT (pointer 1) para o endereço de memória (base + índice)
                _vmWriter.WritePop(Segment.POINTER, 1);
                // Recuper o valor
                _vmWriter.WritePush(Segment.TEMP, 0);
                // Guardar o valor no array
                _vmWriter.WritePop(Segment.THAT, 0);
            }
            else
            {
                // Fazer pop para a variável correspondente
                VarKind kind = _symbolTable.KindOf(varName);
                int index = _symbolTable.IndexOf(varName);
                _vmWriter.WritePop(GetSegment(kind), index);
            }

            ProcessToken(); // ';'
        }
        // Método para compilar um comando de do, seguindo a estrutura da gramática do Jack
        public void CompileDo()
        {
            ProcessToken();
            _tokenizer.Advance();

            // Pega o nome principal (pode ser da classe, do objeto ou da função)
            string identifier = _tokenizer.CurrentToken;
            _tokenizer.Advance();

            // Chama o auxiliar para compilar a chamada de sub-rotina, passando o identificador principal
            CompileSubroutineCall(identifier);

            // O comando 'do' sempre ignora o retorno da função, mandando para o lixo
            _vmWriter.WritePop(Segment.TEMP, 0); 
            
            ProcessToken(); // Lê o ';'
        }

        public void CompileIf()
        {
            string ifTrue = $"IF_TRUE{_ifCount}";
            string ifFalse = $"IF_FALSE{_ifCount}";
            string ifEnd = $"IF_END{_ifCount}";
            _ifCount++; // Aumenta para o próximo if

            ProcessToken(); 
            _tokenizer.Advance();
            ProcessToken();

            // Compila a condição
            CompileExpression();
            
            // Inverte a condição
            _vmWriter.WriteArithmetic(Command.NOT);
            
            // Se a condição original era falsa, salta para o rótulo FALSE
            _vmWriter.WriteIf(ifFalse);

            ProcessToken(); 
            _tokenizer.Advance();
            ProcessToken();

            // Executa os comandos do bloco IF (verdadeiro)
            _tokenizer.Advance();
            CompileStatements();
            
            // Salta para o fim do IF para não executar o ELSE por acidente
            _vmWriter.WriteGoto(ifEnd);

            ProcessToken(); // '}'

            // Marca o rótulo FALSE (onde o else começa)
            _vmWriter.WriteLabel(ifFalse);

            // Verifica se existe um 'else'
            if (_tokenizer.HasMoreTokens())
            {
                _tokenizer.Advance();
                
                if (_tokenizer.CurrentToken == "else")
                {
                    ProcessToken();
                    _tokenizer.Advance();
                    ProcessToken();
                    
                    // Executa os comandos do bloco ELSE
                    _tokenizer.Advance();
                    CompileStatements();
                    
                    ProcessToken(); 
                }
                else
                {
                    
                }
            }
            
            // Marca o fim absoluto do bloco IF/ELSE
            _vmWriter.WriteLabel(ifEnd);
        }


        public void CompileWhile()
        {
            // Cria os nomes únicos para os rótulos deste while específico
            string whileExp = $"WHILE_EXP{_whileCount}";
            string whileEnd = $"WHILE_END{_whileCount}";
            _whileCount++; // Aumenta o contador para o próximo while

            ProcessToken();

            // Marca o início do loop
            _vmWriter.WriteLabel(whileExp);

            _tokenizer.Advance();
            ProcessToken();

            // Compila a condição
            CompileExpression();
            
            // Inverte a condição (se era verdadeiro, fica falso, e vice-versa)
            _vmWriter.WriteArithmetic(Command.NOT);
            
            // Se for verdadeiro, salta para o fim
            _vmWriter.WriteIf(whileEnd);

            ProcessToken();
            _tokenizer.Advance();
            ProcessToken();

            // Executa os comandos dentro do while
            _tokenizer.Advance();
            CompileStatements();

            ProcessToken();
            
            // Salta de volta para avaliar a condição novamente
            _vmWriter.WriteGoto(whileExp);
            
            // Marca o fim do loop
            _vmWriter.WriteLabel(whileEnd);
        }

        private Segment GetSegment(VarKind kind)
        {
            switch (kind)
            {
                case VarKind.STATIC: return Segment.STATIC;
                case VarKind.FIELD: return Segment.THIS;
                case VarKind.ARG: return Segment.ARG;
                case VarKind.VAR: return Segment.LOCAL;
                default: return Segment.LOCAL;
            }
        }

        public int CompileExpressionList()
        {
            int nArgs = 0;

            // Se o token atual for ')', a lista está vazia
            if (_tokenizer.CurrentToken != ")")
            {
                CompileExpression(); // Compila o primeiro argumento
                nArgs++;

                // Enquanto houver vírgulas, há mais argumentos
                while (_tokenizer.CurrentToken == ",")
                {
                    ProcessToken(); // Lê a vírgula
                    _tokenizer.Advance();
                    
                    CompileExpression(); // Compila o próximo argumento
                    nArgs++;
                }
            }

            return nArgs;
        }

        private void CompileSubroutineCall(string identifier)
        {
            int nArgs = 0;
            string functionName = identifier;

            if (_tokenizer.CurrentToken == ".")
            {
                string objName = identifier;
                ProcessToken();
                _tokenizer.Advance();
                
                string methodName = _tokenizer.CurrentToken;
                _tokenizer.Advance();

                // Verifica na Tabela de Símbolos se é um objeto (instância)
                VarKind kind = _symbolTable.KindOf(objName);
                
                if (kind != VarKind.NONE) 
                {
                    // Descobre o tipo da variável (que é a classe do objeto) para formar o nome completo da função
                    string type = _symbolTable.TypeOf(objName);
                    functionName = $"{type}.{methodName}";
                    
                    // O primeiro argumento (0) de um método é a própria instância
                    _vmWriter.WritePush(GetSegment(kind), _symbolTable.IndexOf(objName));
                    nArgs++;
                }
                else 
                {
                    // Se não está na tabela, é uma classe
                    functionName = $"{objName}.{methodName}";
                }
            }
            else if (_tokenizer.CurrentToken == "(")
            {
                functionName = $"{_className}.{identifier}";
                
                _vmWriter.WritePush(Segment.POINTER, 0); 
                nArgs++;
            }

            ProcessToken();
            _tokenizer.Advance();

            // Compila e soma a quantidade de argumentos dentro dos parênteses
            nArgs += CompileExpressionList();

            ProcessToken();
            _tokenizer.Advance();

            // Por ultimo, chama a função
            _vmWriter.WriteCall(functionName, nArgs);
        }
    }
}