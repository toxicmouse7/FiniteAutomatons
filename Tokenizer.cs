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
            KleeneStar
        }

        public TokenType Type { get; set; }
        public string? Expression { get; init; }
    }

    private static IEnumerable<Token> GetTokensFromString(string? expr)
    {
        List<Token> tokens = new();
        if (expr == null)
            return tokens;
        var i = 0;
        while (i < expr.Length)
        {
            var st = expr[i..].Select((x, j) => new {Val = x, Idx = j})
                .Where(x => OperatorsString.Contains(x.Val))
                .Select(x => x.Idx)
                .FirstOrDefault();


            if (st == 0)
            {
                ++st;
            }

            tokens.Add(new Token()
            {
                Type = Token.TokenType.Concatenation,
                Expression = expr.Substring(i, st)
            });

            i += st;
        }

        foreach (var token in tokens)
        {
            token.Type = token.Expression switch
            {
                "(" => Token.TokenType.OpenBracket,
                ")" => Token.TokenType.CloseBracket,
                "*" => Token.TokenType.KleeneStar,
                "+" => Token.TokenType.Union,
                "|" => Token.TokenType.Union,
                _ => token.Type
            };
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
                case Token.TokenType.OpenBracket:
                    operations.Push(token);
                    break;
                case Token.TokenType.CloseBracket:
                    while (operations.Peek().Type != Token.TokenType.OpenBracket)
                    {
                        result.Add(operations.Pop());
                    }

                    operations.Pop();
                    break;
                case Token.TokenType.Concatenation:
                    result.Add(token);
                    break;
                case Token.TokenType.Union:
                    while (operations.Any() && operations.Peek().Type == Token.TokenType.Union)
                    {
                        result.Add(operations.Pop());
                    }

                    operations.Push(token);
                    break;
                case Token.TokenType.KleeneStar:
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