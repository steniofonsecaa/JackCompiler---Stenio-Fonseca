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

        // Escreve um comando de goto (desvio incondicional) no arquivo .vm
        public void WriteGoto(string label)
        {
            _writer.WriteLine($"goto {label}");
        }

        // Escreve um comando de if-goto (desvio condicional) no arquivo .vm
        public void WriteIf(string label)
        {
            _writer.WriteLine($"if-goto {label}");
        }

        // Escreve um comando de chamada de função no arquivo .vm
        public void WriteCall(string functionName, int numArgs)
        {
            _writer.WriteLine($"call {functionName} {numArgs}");
        }

        // Escreve um comando de definição de função no arquivo .vm
        public void WriteFunction(string functionName, int numLocals)
        {
            _writer.WriteLine($"function {functionName} {numLocals}");
        }

        // Escreve um comando de retorno no arquivo .vm
        public void WriteReturn()
        {
            _writer.WriteLine("return");
        }

        // Fecha o arquivo .vm, garantindo que todos os dados sejam gravados corretamente
        public void Close()
        {
            _writer.Close();
        }

        // Helper para converter o enum Segment em sua representação de string usada na linguagem VM
        private string SegmentToString(Segment segment)
        {
            switch (segment)
            {
                case Segment.CONST: return "constant";
                case Segment.ARG: return "argument";
                case Segment.LOCAL: return "local";
                case Segment.STATIC: return "static";
                case Segment.THIS: return "this";
                case Segment.THAT: return "that";
                case Segment.POINTER: return "pointer";
                case Segment.TEMP: return "temp";
                default: throw new ArgumentException("Segmento desconhecido");
            }
        }
    }
}