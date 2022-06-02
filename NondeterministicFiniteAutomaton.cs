﻿using System.Text.RegularExpressions;

namespace FiniteAutomatons;

public class NondeterministicFiniteAutomaton
{
    private readonly Dictionary<string, Dictionary<string, List<string>>> _states = new();

    private readonly List<string> _finalStates = new();
    private string[] _alphabet;
    private HashSet<string> _currentStates = new();

    public NondeterministicFiniteAutomaton(string filename)
    {
        var lines = File.ReadAllLines(filename);
        var statesCount = int.Parse(lines[0]);

        lines[1] = lines[1].Substring(1, lines[1].Length - 2);
        _alphabet = lines[1].Split(',');

        var stateNumberRegex = new Regex("Q(?<stateNumber>[0-9]*)=.*",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);
        var allowedTransitionsRegex = new Regex(@"Q[0-9]*=\{(?<transitions>.*)\}",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);
        var transitionRegex = new Regex(@"(?<letter>[0-9e]*):\[(?<states>.*)]",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);
        var statesRegex = new Regex(@"Q(?<state>[0-9]*)",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);
        var replaceCommaRegex = new Regex(@"\],",
            RegexOptions.IgnoreCase & RegexOptions.Compiled);

        for (var i = 2; i < 2 + statesCount; i++)
        {
            var state = stateNumberRegex.Match(lines[i]).Groups["stateNumber"].Value;
            _states[state] = new Dictionary<string, List<string>>();

            var allowedTransitionsString = allowedTransitionsRegex.Match(lines[i]).Groups["transitions"].Value;
            if (allowedTransitionsString == "")
            {
                continue;
            }

            var allowedTransitions = replaceCommaRegex.Replace(allowedTransitionsString, "];")
                .Split(';');
            foreach (var transition in allowedTransitions)
            {
                var matches = transitionRegex.Match(transition).Groups;
                _states[state][matches["letter"].Value] = new List<string>();
                var states = matches["states"].Value.Split(',');
                foreach (var transitionState in states)
                {
                    if (transitionState.Length == 0)
                    {
                        _states[state][matches["letter"].Value].Add(state);
                        break;
                    }

                    var matchedStates = statesRegex.Matches(transitionState);
                    foreach (Match matchedState in matchedStates)
                    {
                        _states[state][matches["letter"].Value]
                            .Add(matchedState.Groups["state"].Value);
                    }
                }
            }
        }

        _currentStates.Add(lines[2 + statesCount].Substring(1));
        var allowedStates = lines.Last().Substring(1, lines.Last().Length - 2)
            .Split(',');
        foreach (var allowedState in allowedStates) _finalStates.Add(allowedState.Substring(1));
    }

    private NondeterministicFiniteAutomaton(Dictionary<string, Dictionary<string, List<string>>> states,
        IEnumerable<string> finalStates, IEnumerable<string> alphabet, IEnumerable<string> currentStates)
    {
        _states = states;
        _finalStates = finalStates.ToList();
        _alphabet = alphabet.ToArray();
        _currentStates = currentStates.ToHashSet();
    }

    private static NondeterministicFiniteAutomaton Constant(Tokenizer.Token token, ref int count)
    {
        var currentStates = new HashSet<string> {count.ToString()};
        var states = new Dictionary<string, Dictionary<string, List<string>>>
        {
            [count++.ToString()] = new()
            {
                [token.Expression!] = new List<string>()
                {
                    count.ToString()
                }
            }
        };


        var finalStates = new List<string> {count++.ToString()};

        return new NondeterministicFiniteAutomaton(
            states, finalStates, ArraySegment<string>.Empty, currentStates
        );
    }

    private static NondeterministicFiniteAutomaton Union(NondeterministicFiniteAutomaton arg1,
        NondeterministicFiniteAutomaton arg2, ref int count)
    {
        var currentStates = new HashSet<string> {count.ToString()};
        var states = new Dictionary<string, Dictionary<string, List<string>>>
        {
            [count.ToString()] = new()
            {
                ["e"] = new List<string>()
            }
        };

        states[count.ToString()]["e"].AddRange(arg1._currentStates);
        states[count++.ToString()]["e"].AddRange(arg2._currentStates);

        arg1._states[arg1._finalStates.First()] = new Dictionary<string, List<string>>
        {
            ["e"] = new()
            {
                count.ToString()
            }
        };

        arg2._states[arg2._finalStates.First()] = new Dictionary<string, List<string>>
        {
            ["e"] = new()
            {
                count.ToString()
            }
        };

        arg1._states.ToList().ForEach(x => states.Add(x.Key, x.Value));
        arg2._states.ToList().ForEach(x => states.Add(x.Key, x.Value));

        var finalStates = new List<string>
        {
            count++.ToString()
        };

        return new NondeterministicFiniteAutomaton(
            states, finalStates, ArraySegment<string>.Empty, currentStates
        );
    }

    private static NondeterministicFiniteAutomaton Concatenation(Tokenizer.Token token,
        NondeterministicFiniteAutomaton argument, ref int count)
    {
        --count;

        var states = new Dictionary<string, Dictionary<string, List<string>>>
        {
            [count++.ToString()] = new()
            {
                [token.Expression!] = new List<string>()
                {
                    count.ToString()
                }
            }
        };

        var finalStates = new List<string> {count++.ToString()};

        var currentStates = argument._currentStates;

        argument._states.ToList().ForEach(x => states.Add(x.Key, x.Value));

        return new NondeterministicFiniteAutomaton(
            states, finalStates, ArraySegment<string>.Empty, currentStates
        );
    }

    private static NondeterministicFiniteAutomaton Concatenation(NondeterministicFiniteAutomaton arg1,
        NondeterministicFiniteAutomaton arg2)
    {
        var states = arg1._states;
        
        foreach (var state in states.Keys)
        {
            foreach (var symbol in states[state].Keys
                         .Where(symbol => states[state][symbol].First() == arg1._finalStates.First()))
            {
                states[state][symbol] = arg2._currentStates.ToList();
            }
        }

        arg2._states.ToList().ForEach(x => states.Add(x.Key, x.Value));

        // foreach (var finalState in arg1._finalStates)
        // {
        //     states[finalState] = new Dictionary<string, List<string>>
        //     {
        //         ["e"] = arg2._currentStates.ToList()
        //     };
        // }

        var currentStates = arg1._currentStates;
        var finalStates = arg2._finalStates;

        return new NondeterministicFiniteAutomaton(
            states, finalStates, ArraySegment<string>.Empty, currentStates
        );
    }

    private static NondeterministicFiniteAutomaton KleeneStart(NondeterministicFiniteAutomaton argument, ref int count)
    {
        var currentStates = new HashSet<string> {count.ToString()};
        var states = new Dictionary<string, Dictionary<string, List<string>>>
        {
            [count.ToString()] = new()
            {
                ["e"] = new List<string>()
            }
        };

        states[count.ToString()]["e"].AddRange(argument._currentStates);
        states[count.ToString()]["e"].Add((++count).ToString());
        argument._states[argument._finalStates.First()] = new Dictionary<string, List<string>>
        {
            ["e"] = new()
            {
                argument._currentStates.First(),
                count.ToString()
            }
        };

        argument._states.ToList().ForEach(x => states.Add(x.Key, x.Value));

        var finalStates = new List<string> {count++.ToString()};

        return new NondeterministicFiniteAutomaton(
            states, finalStates, ArraySegment<string>.Empty, currentStates
        );
    }

    public static NondeterministicFiniteAutomaton FromRegularExpression(string regex)
    {
        var tokens = Tokenizer.GetPostfixTokens(regex);
        var automatons = new Stack<NondeterministicFiniteAutomaton>();
        var alphabet = new HashSet<string>();
        var count = 0;

        var used = new Stack<Tokenizer.Token>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case Tokenizer.Token.TokenType.Constant:
                {
                    automatons.Push(Constant(token, ref count));
                    break;
                }
                case Tokenizer.Token.TokenType.Union:
                {
                    var secondArgument = automatons.Pop();
                    var firstArgument = automatons.Pop();

                    automatons.Push(Union(firstArgument, secondArgument, ref count));

                    break;
                }
                case Tokenizer.Token.TokenType.Concatenation:
                {
                    var argument = automatons.Pop();

                    automatons.Push(Concatenation(token, argument, ref count));

                    break;
                }
                case Tokenizer.Token.TokenType.KleeneStar:
                {
                    var argument = automatons.Pop();

                    if (used.Peek().Type == Tokenizer.Token.TokenType.Constant)
                    {
                        used.Pop();
                    }

                    automatons.Push(KleeneStart(argument, ref count));

                    if (used.Any() && used.Peek().Type is Tokenizer.Token.TokenType.Constant
                            or Tokenizer.Token.TokenType.Concatenation)
                    {
                        var arg2 = automatons.Pop();
                        var arg1 = automatons.Pop();

                        automatons.Push(Concatenation(arg1, arg2));
                    }

                    break;
                }
                case Tokenizer.Token.TokenType.OpenBracket:
                case Tokenizer.Token.TokenType.CloseBracket:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(regex));
            }

            used.Push(token);
        }

        foreach (var symbol in automatons.Peek()._states.Keys
                     .SelectMany(fromState => automatons.Peek()._states[fromState].Keys))
        {
            if (symbol == "e") continue;
            alphabet.Add(symbol);
        }

        automatons.Peek()._alphabet = alphabet.ToArray();

        return automatons.Peek();
    }

    private IEnumerable<string> e_BFS(string start, bool includeFirst = true)
    {
        var q = new Queue<string>();
        q.Enqueue(start);
        var used = new List<string>();
        if (includeFirst) used.Add(start);

        while (q.Any())
        {
            var v = q.Dequeue();
            if (!_states.ContainsKey(v) || !_states[v].ContainsKey("e")) continue;
            foreach (var to in _states[v]["e"].Where(to => !used.Contains(to)))
            {
                used.Add(to);
                q.Enqueue(to);
            }
        }

        return used.ToHashSet();
    }

    public bool Accept(in string input)
    {
        _currentStates = e_BFS(_currentStates.First()).ToHashSet();

        foreach (var symbol in input)
        {
            if (!_alphabet.Contains(symbol.ToString())) return false;

            var newStates = new HashSet<string>();

            foreach (var state in _currentStates
                         .Where(
                             state => _states.ContainsKey(state) && _states[state].ContainsKey(symbol.ToString())
                         )
                    )
            {
                foreach (var newState in _states[state][symbol.ToString()])
                {
                    newStates.Add(newState);
                }

                var eClose = newStates.Where(
                    newState => _states.ContainsKey(newState) && _states[newState].ContainsKey("e")
                ).ToArray();

                foreach (var eState in eClose)
                {
                    var eStates = e_BFS(eState, false);
                    newStates.UnionWith(eStates);
                }
            }

            if (!newStates.Any()) return false;
            _currentStates = newStates;
        }

        return _currentStates.Any(s => _finalStates.Contains(s));
    }

    public DeterministicFiniteAutomaton ToDetFinAut()
    {
        var consideredVertices = new Queue<List<string>>();
        var used = new HashSet<List<string>>();
        var d = new Dictionary<string, Dictionary<string, List<string>>>();
        var newStates = new Dictionary<string, Dictionary<string, string>>();
        var newFinalStates = new List<string>();

        var startVertices = e_BFS(_currentStates.First()).ToList();
        consideredVertices.Enqueue(startVertices);
        used.Add(startVertices);

        var newInitialState = string.Join(" ", consideredVertices.First());

        while (consideredVertices.Any())
        {
            var q = consideredVertices.Dequeue();
            if (q.Any(s => _finalStates.Contains(s)))
            {
                newFinalStates.Add(string.Join(" ", q));
            }

            var questionVertex = string.Join(" ", q);

            foreach (var symbol in _alphabet)
            {
                if (!d.ContainsKey(questionVertex))
                    d[questionVertex] = new Dictionary<string, List<string>>();
                if (!d[questionVertex].ContainsKey(symbol))
                    d[questionVertex][symbol] = new List<string>();


                foreach (var state in q.Where(state =>
                             _states.ContainsKey(state) && _states[state].ContainsKey(symbol)))
                {
                    d[questionVertex][symbol].AddRange(_states[state][symbol]);
                    var eClosure = new HashSet<string>();
                    foreach (var eState in _states[state][symbol]
                                 .Where(eState => _states[eState].ContainsKey("e")))
                    {
                        eClosure.UnionWith(e_BFS(eState));
                    }

                    d[questionVertex][symbol].AddRange(eClosure);
                }

                if (!d[questionVertex][symbol].Any())
                {
                    d[questionVertex].Remove(symbol);
                    continue;
                }

                d[questionVertex][symbol] = d[questionVertex][symbol].Distinct().OrderBy(t => t).ToList();

                if (used.Any(l => d[questionVertex][symbol].SequenceEqual(l))) continue;

                consideredVertices.Enqueue(d[questionVertex][symbol]);
                used.Add(d[questionVertex][symbol]);
            }
        }

        foreach (var (fromState, transition) in d)
        {
            newStates[fromState] = new Dictionary<string, string>();
            foreach (var (symbol, toStates) in transition)
            {
                newStates[fromState][symbol] = string.Join(" ", toStates);
            }
        }

        return new DeterministicFiniteAutomaton(newStates, _alphabet, newFinalStates, newInitialState);
    }
}