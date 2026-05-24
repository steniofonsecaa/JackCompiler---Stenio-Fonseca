using System;
using System.IO;

namespace JackCompiler.Modules
{
    // Enum para os segmentos de memória usados na linguagem VM
    public enum Segment
    {
        CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP
    }

    // Enum para os comandos aritméticos usados na linguagem VM
    public enum Command
    {
        ADD, SUB, NEG, EQ, GT, LT, AND, OR, NOT
    }

    public class VMWriter   
    {
        private StreamWriter _writer;
        
        // Construtor que recebe o caminho do arquivo de saída (.vm) e inicializa o StreamWriter
        public VMWriter(string outputPath)
        {
            _writer = new StreamWriter(outputPath);
        }

        // Escreve um comando de push no arquivo .vm
        public void WritePush(Segment segment, int index)
        {
            _writer.WriteLine($"push {SegmentToString(segment)} {index}");
        }   

        // Escreve um comando de pop no arquivo .vm
        public void WritePop(Segment segment, int index)
        {
            _writer.WriteLine($"pop {SegmentToString(segment)} {index}");
        }

        // Escreve um comando aritmético no arquivo .vm
        public void WriteArithmetic(Command command)
        {
            _writer.WriteLine(command.ToString().ToLower());
        }

        // Escreve um comando de label no arquivo .vm
        public void WriteLabel(string label)
        {
            _writer.WriteLine($"label {label}");
        }
    }
}