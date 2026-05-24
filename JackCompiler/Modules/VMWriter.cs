using System;
using System.IO;

namespace JackCompiler.Modules
{
    // Enum para os segmentos de memória usados na linguagem VM
    public enum Segment
    {
        CONST, ARG, LOCAL, STATIC, THIS, THAT, POINTER, TEMP
    }
}