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
}