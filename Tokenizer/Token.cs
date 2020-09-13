using System;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
    public readonly struct Token
    {
        public Token( TokenType tokenType, string value )
        {
            TokenType = tokenType;
            Value = value;
        }
        public readonly TokenType TokenType;
        public readonly string Value;
        public override string ToString() => Value;
        public static Token None => new Token( TokenType.None, string.Empty );
        public static Token Space => new Token( TokenType.Whitespace, CommonStrings.Space );
        public static Token Semicolon => new Token( TokenType.Semicolon, CommonStrings.Semicolon );
        public static Token BitwiseOr => new Token( TokenType.Operator, CommonStrings.Pipe );
        public static Token BitwiseAnd => new Token( TokenType.Operator, CommonStrings.Ampersand );
        public static Token LessThan => new Token( TokenType.Operator, CommonStrings.AngleOpen );
        public static Token MoreThan => new Token( TokenType.Operator, CommonStrings.AngleClose );
        public static Token Assign => new Token( TokenType.Operator, CommonStrings.Equal );
        public static Token Dot => new Token( TokenType.Operator, CommonStrings.Dot );
        public static Token ParenthesesOpen => new Token( TokenType.BlockOpen, CommonStrings.ParenthesesOpen );
        public static Token AngleOpen => new Token( TokenType.BlockOpen, CommonStrings.AngleOpen );
        public static Token CurlyOpen => new Token( TokenType.BlockOpen, CommonStrings.CurlyOpen );
        public static Token BracketOpen => new Token( TokenType.BlockOpen, CommonStrings.BracketOpen );
        public static Token ParenthesesClose => new Token( TokenType.BlockClose, CommonStrings.ParenthesesClose );
        public static Token AngleClose => new Token( TokenType.BlockClose, CommonStrings.AngleClose );
        public static Token CurlyClose => new Token( TokenType.BlockClose, CommonStrings.CurlyClose );
        public static Token BracketClose => new Token( TokenType.BlockClose, CommonStrings.BracketClose );
        public static Token Not => new Token( TokenType.Operator, CommonStrings.ExclamationMark );
        public static Token Comma => new Token( TokenType.Operator, CommonStrings.Comma );
        public static Token Plus => new Token( TokenType.Operator, CommonStrings.Plus );
        public static Token Increment => new Token( TokenType.Operator, CommonStrings.DoublePlus );
        public static Token PlusAssign => new Token( TokenType.Operator, CommonStrings.PlusEqual );
        public static Token Minus => new Token( TokenType.Operator, CommonStrings.Minus );
        public static Token Colon => new Token( TokenType.Operator, CommonStrings.Colon );
        public static Token DotAssign => new Token( TokenType.Operator, CommonStrings.DotEqual );
        public static Token Equal => new Token( TokenType.Operator, CommonStrings.EqualEqual );
        public static Token LessOrEqual => new Token( TokenType.Operator, CommonStrings.LessOrEqual );
        public static Token MoreOrEqual => new Token( TokenType.Operator, CommonStrings.MoreOrEqual );
        public static Token NotEqual => new Token( TokenType.Operator, CommonStrings.NotEqual );
        public static Token Or => new Token( TokenType.Operator, CommonStrings.DoublePipe );
        public static Token And => new Token( TokenType.Operator, CommonStrings.DoubleAmpersand );
        public static Token BitwiseOrAssign => new Token( TokenType.Operator, CommonStrings.PipeEqual );
        public static Token BitwiseAndAssign => new Token( TokenType.Operator, CommonStrings.AndEqual );
        public static Token ColonAssign => new Token( TokenType.Operator, CommonStrings.ColonAndEqual );
        public static Token DoubleColon => new Token( TokenType.Operator, CommonStrings.DoubleColon );
    }
}
