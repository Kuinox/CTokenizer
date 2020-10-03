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
