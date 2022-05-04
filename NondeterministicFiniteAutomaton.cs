﻿using System.Text.RegularExpressions;

namespace FiniteAutomatons;

public class NondeterministicFiniteAutomaton
{
    private readonly Dictionary<string, Dictionary<string, List<string>>> _states = new();

    private readonly List<string> _finalStates = new();
    private readonly string[] _alphabet;
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

    public static NondeterministicFiniteAutomaton FromRegularExpression(string regex)
    {
        var tokens = Tokenizer.GetPostfixTokens(regex);
        var automatons = new Stack<NondeterministicFiniteAutomaton>();
        var count = 0;

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case Tokenizer.Token.TokenType.Union:
                {
                    var secondArgument = automatons.Pop();
                    var firstArgument = automatons.Pop();

                    var currentStates = new HashSet<string>() {count.ToString()};
                    var states = new Dictionary<string, Dictionary<string, List<string>>>
                    {
                        [count.ToString()] = new()
                        {
                            ["e"] = new List<string>()
                        }
                    };

                    states[count.ToString()]["e"].AddRange(firstArgument._currentStates);
                    states[count++.ToString()]["e"].AddRange(secondArgument._currentStates);

                    firstArgument._states[firstArgument._finalStates.First()] = new Dictionary<string, List<string>>
                    {
                        ["e"] = new()
                        {
                            count.ToString()
                        }
                    };

                    secondArgument._states[secondArgument._finalStates.First()] = new Dictionary<string, List<string>>
                    {
                        ["e"] = new()
                        {
                            count.ToString()
                        }
                    };

                    firstArgument._states.ToList().ForEach(x => states.Add(x.Key, x.Value));
                    secondArgument._states.ToList().ForEach(x => states.Add(x.Key, x.Value));

                    var finalStates = new List<string>
                    {
                        count++.ToString()
                    };

                    automatons.Push(
                        new NondeterministicFiniteAutomaton(
                            states, finalStates, ArraySegment<string>.Empty, currentStates
                        )
                    );

                    break;
                }
                case Tokenizer.Token.TokenType.Concatenation:
                {
                    var currentStates = new HashSet<string> {count.ToString()};
                    var states = new Dictionary<string, Dictionary<string, List<string>>>();
                    foreach (var symbol in token.Expression!.Select(character => character.ToString()))
                    {
                        states[count.ToString()] = new Dictionary<string, List<string>>
                        {
                            [symbol] = new()
                            {
                                (++count).ToString()
                            }
                        };
                    }

                    var finalStates = new List<string> {count++.ToString()};
                    automatons.Push(
                        new NondeterministicFiniteAutomaton(
                            states, finalStates, ArraySegment<string>.Empty, currentStates
                        )
                    );
                    break;
                }
                case Tokenizer.Token.TokenType.KleeneStar:
                {
                    var argument = automatons.Pop();
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

                    automatons.Push(
                        new NondeterministicFiniteAutomaton(
                            states, finalStates, ArraySegment<string>.Empty, currentStates
                        )
                    );

                    break;
                }
                case Tokenizer.Token.TokenType.OpenBracket:
                    break;
                case Tokenizer.Token.TokenType.CloseBracket:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(regex));
            }
        }

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
            if (!_states[v].ContainsKey("e")) continue;
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
        foreach (var symbol in input)
        {
            if (!_alphabet.Contains(symbol.ToString())) return false;

            var newStates = new HashSet<string>();
            foreach (var state in _currentStates
                         .Where(state => _states[state].ContainsKey(symbol.ToString())))
            {
                foreach (var newState in _states[state][symbol.ToString()])
                {
                    newStates.Add(newState);
                }

                if (newStates.All(newState => !_states[newState].ContainsKey("e"))) continue;
                var eClose = newStates.Where(
                    newState => _states[newState].ContainsKey("e")
                ).ToArray();

                foreach (var eState in eClose)
                {
                    var eStates = e_BFS(eState, false);
                    newStates.UnionWith(eStates);
                }
            }

            if (!newStates.Any()) continue;
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


                foreach (var state in q.Where(state => _states[state].ContainsKey(symbol)))
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