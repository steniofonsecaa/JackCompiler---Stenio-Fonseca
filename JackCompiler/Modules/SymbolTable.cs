using System.Collections.Generic;

namespace JackCompiler.Modules
{
    public enum VarKind { STATIC, FIELD, ARG, VAR, NONE }

    // Representa os dados de uma variável
    public class Symbol
    {
        public string Type { get; set; } = "";
        public VarKind Kind { get; set; }
        public int Index { get; set; }
    }

    public class SymbolTable
    {
        // Tabelas de escopo
        private Dictionary<string, Symbol> classScope;
        private Dictionary<string, Symbol> subroutineScope;

        // Contadores para saber qual o índice da próxima variável (ex: local 0, local 1...)
        private Dictionary<VarKind, int> indices;

        public SymbolTable()
        {
            classScope = new Dictionary<string, Symbol>();
            subroutineScope = new Dictionary<string, Symbol>();
            indices = new Dictionary<VarKind, int>
            {
                { VarKind.STATIC, 0 },
                { VarKind.FIELD, 0 },
                { VarKind.ARG, 0 },
                { VarKind.VAR, 0 }
            };
        }

        // Chamado sempre que começamos a compilar uma nova função/método
        public void StartSubroutine()
        {
            subroutineScope.Clear();
            indices[VarKind.ARG] = 0;
            indices[VarKind.VAR] = 0;
        }

        // Define uma nova variável na tabela
        public void Define(string name, string type, VarKind kind)
        {
            Symbol newSymbol = new Symbol { Type = type, Kind = kind, Index = indices[kind] };
            
            if (kind == VarKind.STATIC || kind == VarKind.FIELD)
            {
                classScope[name] = newSymbol;
            }
            else if (kind == VarKind.ARG || kind == VarKind.VAR)
            {
                subroutineScope[name] = newSymbol;
            }
            
            indices[kind]++; // Aumenta o índice para a próxima variável desse tipo
        }

        // Retorna a quantidade de variáveis de um certo tipo
        public int VarCount(VarKind kind)
        {
            return indices[kind];
        }

        // Busca o Kind de uma variável pelo nome
        public VarKind KindOf(string name)
        {
            if (subroutineScope.ContainsKey(name)) return subroutineScope[name].Kind;
            if (classScope.ContainsKey(name)) return classScope[name].Kind;
            return VarKind.NONE;
        }

        // Busca o Tipo de uma variável pelo nome
        public string? TypeOf(string name)
        {
            if (subroutineScope.ContainsKey(name)) return subroutineScope[name].Type;
            if (classScope.ContainsKey(name)) return classScope[name].Type;
            return null;
        }

        // Busca o Índice de uma variável pelo nome
        public int IndexOf(string name)
        {
            if (subroutineScope.ContainsKey(name)) return subroutineScope[name].Index;
            if (classScope.ContainsKey(name)) return classScope[name].Index;
            return -1;
        }
    }
}