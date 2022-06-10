using static FiniteAutomatons.Tokenizer.Token;

namespace FiniteAutomatons;

public static class Tokenizer
{
    private const string OperatorsString = "()+*|";

    private static Dictionary<TokenType, int> _priority = new()
    {
        {TokenType.Concatenation, 1},
        {TokenType.Union, 0}
    };

    public class Token
    {
        public enum TokenType
        {
            OpenBracket,
            CloseBracket,
            Union,
            Concatenation,
            KleeneStar,
            Constant
        }

        public TokenType Type { get; set; }
        public string? Expression { get; init; }
    }

    private static IEnumerable<Token> GetTokensFromString(string? expr)
    {
        // надо сделать операцию конкатенации отдельной
        List<Token> tokens = new();
        if (expr == null)
            return tokens;
        foreach (var symbol in expr)
        {
            var type = symbol switch
            {
                '(' => TokenType.OpenBracket,
                ')' => TokenType.CloseBracket,
                '*' => TokenType.KleeneStar,
                '+' => TokenType.Union,
                '|' => TokenType.Union,
                _ => TokenType.Constant
            };

            char? expression = type switch
            {
                TokenType.Constant => symbol,
                _ => null
            };

            if (tokens.LastOrDefault()?.Type is TokenType.Constant or TokenType.KleeneStar
                && type is TokenType.Constant or TokenType.OpenBracket
                || (tokens.LastOrDefault()?.Type == TokenType.CloseBracket && type == TokenType.OpenBracket))
            {
                tokens.Add(
                    new Token
                    {
                        Type = TokenType.Concatenation,
                        Expression = null
                    }
                );
            }


            tokens.Add(
                new Token()
                {
                    Type = type,
                    Expression = expression.ToString()
                }
            );
        }

        return tokens;
    }

    private static IEnumerable<Token> MakePostfix(IEnumerable<Token> tokens)
    {
        var result = new List<Token>();
        var operations = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.OpenBracket:
                    operations.Push(token);
                    break;
                case TokenType.CloseBracket:
                    while (operations.Peek().Type != TokenType.OpenBracket)
                    {
                        result.Add(operations.Pop());
                    }

                    operations.Pop();
                    break;
                case TokenType.Union:
                case TokenType.Concatenation:
                    while (operations.Any() 
                           && operations.Peek().Type is TokenType.Concatenation or TokenType.Union
                           && _priority[operations.Peek().Type] >= _priority[token.Type])
                    {
                        result.Add(operations.Pop());
                    }

                    operations.Push(token);
                    break;
                case TokenType.Constant:
                    result.Add(token);
                    break;
                case TokenType.KleeneStar:
                    result.Add(token);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tokens));
            }
        }

        while (operations.Any())
        {
            result.Add(operations.Pop());
        }

        return result;
    }

    public static IEnumerable<Token> GetPostfixTokens(string? expr)
    {
        return MakePostfix(GetTokensFromString(expr));
    }
}