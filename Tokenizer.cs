using static FiniteAutomatons.Tokenizer.Token;

namespace FiniteAutomatons;

public static class Tokenizer
{
    private const string OperatorsString = "()+*|";

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

            if (type == TokenType.KleeneStar && tokens.Last().Type == TokenType.Concatenation)
            {
                tokens.Last().Type = TokenType.Constant;
            }

            if (expression != null)
            {
                if (tokens.Any())
                {
                    switch (tokens.Last().Type)
                    {
                        case TokenType.KleeneStar:
                        case TokenType.CloseBracket:
                        case TokenType.Constant:
                        case TokenType.Concatenation:
                            type = TokenType.Concatenation;
                            break;
                        case TokenType.OpenBracket:
                            break;
                        case TokenType.Union:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(expr));
                    }
                }
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
                case TokenType.Constant:
                case TokenType.Concatenation:
                    result.Add(token);
                    break;
                case TokenType.Union:
                    while (operations.Any() && operations.Peek().Type == TokenType.Union)
                    {
                        result.Add(operations.Pop());
                    }

                    operations.Push(token);
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