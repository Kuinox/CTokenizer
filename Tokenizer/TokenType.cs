using System;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
    public enum TokenType
    {
        None,
        Keyword,
        Word,
        Operator,
        BlockOpen,
        BlockClose,
        Whitespace,
        Comment,
        StringDeclaration,
        Semicolon,
        Number,
        PreprocessorDirective,
        Unknown,
    }
}
