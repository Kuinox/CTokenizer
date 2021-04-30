using System;
using System.Buffers;

namespace Tokenizer
{
    public readonly struct Token
    {
        public Token( TokenType tokenType, ReadOnlySequence<char> value )
        {
            TokenType = tokenType;
            Value = value;
        }
        public Token( TokenType tokenType, string value ) : this( tokenType, new ReadOnlySequence<char>( value.AsMemory() ) )
        {
        }

        public readonly TokenType TokenType;
        public readonly ReadOnlySequence<char> Value;
        public static Token None => new Token( TokenType.None, ReadOnlySequence<char>.Empty );
        //public static Token Space => new Token( TokenType.Whitespace, CommonStrings.Space );
        //public static Token Semicolon => new Token( TokenType.Semicolon, CommonStrings.Semicolon );
        //public static Token BitwiseOr => new Token( TokenType.Operator, CommonStrings.Pipe );
        //public static Token BitwiseAnd => new Token( TokenType.Operator, CommonStrings.Ampersand );
        //public static Token LessThan => new Token( TokenType.Operator, CommonStrings.AngleOpen );
        //public static Token MoreThan => new Token( TokenType.Operator, CommonStrings.AngleClose );
        //public static Token Assign => new Token( TokenType.Operator, CommonStrings.Equal );
        //public static Token Dot => new Token( TokenType.Operator, CommonStrings.Dot );
        //public static Token ParenthesesOpen => new Token( TokenType.BlockOpen, CommonStrings.ParenthesesOpen );
        //public static Token AngleOpen => new Token( TokenType.BlockOpen, CommonStrings.AngleOpen );
        //public static Token CurlyOpen => new Token( TokenType.BlockOpen, CommonStrings.CurlyOpen );
        //public static Token BracketOpen => new Token( TokenType.BlockOpen, CommonStrings.BracketOpen );
        //public static Token ParenthesesClose => new Token( TokenType.BlockClose, CommonStrings.ParenthesesClose );
        //public static Token AngleClose => new Token( TokenType.BlockClose, CommonStrings.AngleClose );
        //public static Token CurlyClose => new Token( TokenType.BlockClose, CommonStrings.CurlyClose );
        //public static Token BracketClose => new Token( TokenType.BlockClose, CommonStrings.BracketClose );
        //public static Token Not => new Token( TokenType.Operator, CommonStrings.ExclamationMark );
        //public static Token Comma => new Token( TokenType.Operator, CommonStrings.Comma );
        //public static Token Plus => new Token( TokenType.Operator, CommonStrings.Plus );
        //public static Token Increment => new Token( TokenType.Operator, CommonStrings.DoublePlus );
        //public static Token PlusAssign => new Token( TokenType.Operator, CommonStrings.PlusEqual );
        //public static Token MinusAssign => new Token( TokenType.Operator, CommonStrings.MinusEqual );
        //public static Token Minus => new Token( TokenType.Operator, CommonStrings.Minus );
        //public static Token Multiply => new Token( TokenType.Operator, CommonStrings.Minus );
        //public static Token MultiplyAssign => new Token( TokenType.Operator, CommonStrings.StarEqual );
        //public static Token Divide => new Token( TokenType.Operator, CommonStrings.Slash );
        //public static Token DivideAssign => new Token( TokenType.Operator, CommonStrings.SlashEqual );
        //public static Token Colon => new Token( TokenType.Operator, CommonStrings.Colon );
        //public static Token DotAssign => new Token( TokenType.Operator, CommonStrings.DotEqual );
        //public static Token Equal => new Token( TokenType.Operator, CommonStrings.DoubleEqual );
        //public static Token Arrow => new Token( TokenType.Operator, CommonStrings.EqualSuperior );
        //public static Token LessOrEqual => new Token( TokenType.Operator, CommonStrings.LessOrEqual );
        //public static Token MoreOrEqual => new Token( TokenType.Operator, CommonStrings.MoreOrEqual );
        //public static Token LeftShift => new Token( TokenType.Operator, CommonStrings.DoubleAngleOpen );
        //public static Token LeftShiftAssign => new Token( TokenType.Operator, CommonStrings.DoubleAngleOpenEqual );
        //public static Token RightShift => new Token( TokenType.Operator, CommonStrings.DoubleAngleClose );
        //public static Token RightShiftAssign => new Token( TokenType.Operator, CommonStrings.DoubleAngleCloseEqual );
        //public static Token Modulo => new Token( TokenType.Operator, CommonStrings.Percent );
        //public static Token NotEqual => new Token( TokenType.Operator, CommonStrings.NotEqual );
        //public static Token Or => new Token( TokenType.Operator, CommonStrings.DoublePipe );
        //public static Token And => new Token( TokenType.Operator, CommonStrings.DoubleAmpersand );
        //public static Token BitwiseOrAssign => new Token( TokenType.Operator, CommonStrings.PipeEqual );
        //public static Token BitwiseAndAssign => new Token( TokenType.Operator, CommonStrings.AndEqual );
        //public static Token ColonAssign => new Token( TokenType.Operator, CommonStrings.ColonAndEqual );
        //public static Token DoubleColon => new Token( TokenType.Operator, CommonStrings.DoubleColon );
        //public static Token Ternary => new Token( TokenType.Operator, CommonStrings.QuestionMark );
        //public static Token ConditionalCall => new Token( TokenType.Operator, CommonStrings.QuestionMarkDot );
        //public static Token NullCoalescing => new Token( TokenType.Operator, CommonStrings.DoubleQuestionMark );
        //public static Token NullForgiving => new Token( TokenType.Operator, CommonStrings.ExclamationMarkDot );
        //public static Token NullCoalescingAssign => new Token( TokenType.Operator, CommonStrings.DoubleQuestionMarkEqual );
        //public static Token Unary => new Token( TokenType.Operator, CommonStrings.Tilde );
        //public static Token Xor => new Token( TokenType.Operator, CommonStrings.Caret );

    }
}
