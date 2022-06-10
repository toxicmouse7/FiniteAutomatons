using System.Text.RegularExpressions;

namespace FiniteAutomatons;

public class NondeterministicFiniteAutomaton
{
    private readonly Dictionary<string, Dictionary<string, List<string>>> _states = new();

    private List<string> _finalStates = new();
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

        foreach (var finalState in arg1._finalStates)
        {
            states[finalState] = new Dictionary<string, List<string>>
            {
                ["e"] = arg2._currentStates.ToList()
            };
        }

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
                    var arg2 = automatons.Pop();
                    var arg1 = automatons.Pop();

                    automatons.Push(Concatenation(arg1, arg2));

                    break;
                }
                case Tokenizer.Token.TokenType.KleeneStar:
                {
                    var argument = automatons.Pop();

                    automatons.Push(KleeneStart(argument, ref count));

                    break;
                }
                case Tokenizer.Token.TokenType.OpenBracket:
                case Tokenizer.Token.TokenType.CloseBracket:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(regex));
            }
        }

        foreach (var symbol in automatons.Peek()._states.Keys
                     .SelectMany(fromState => automatons.Peek()._states[fromState].Keys))
        {
            if (symbol == "e") continue;
            alphabet.Add(symbol);
        }

        automatons.Peek()._alphabet = alphabet.ToArray();

        automatons.Peek()._states[automatons.Peek()._finalStates.First()] = new Dictionary<string, List<string>>();

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

    private static int GetIndexOf<T>(IReadOnlyList<HashSet<T>> collection,
        IReadOnlyCollection<T> item)
    {
        for (var i = 0; i < collection.Count; ++i)
        {
            if (collection[i].SequenceEqual(item))
                return i;
        }

        return -1;
    }

    public DeterministicFiniteAutomaton ToDetFinAutV2()
    {
        var queue = new Queue<HashSet<string>>();
        var start = e_BFS(_currentStates.First());
        queue.Enqueue(start.ToHashSet());
        var dStates = new List<HashSet<string>>();
        var dTransitions = new Dictionary<string, Dictionary<string, string>>();
        var dFinish = new List<string>();
        dStates.Add(queue.Peek());
        while (queue.Any())
        {
            var newState = queue.Dequeue();
            foreach (var symbol in _alphabet)
            {
                var newStateTarget = new HashSet<string>();
                foreach (var state in newState)
                {
                    if (_states[state].ContainsKey(symbol))
                        newStateTarget.UnionWith(_states[state][symbol]);
                }

                var targetEClose = new HashSet<string>();

                foreach (var state in newStateTarget)
                    targetEClose.UnionWith(e_BFS(state, false));
                newStateTarget.UnionWith(targetEClose);
                
                if (newStateTarget.Contains(""))
                    newStateTarget.Remove("");

                if (dStates.All(x => !x.SequenceEqual(newStateTarget)))
                {
                    queue.Enqueue(newStateTarget);
                    dStates.Add(newStateTarget);
                }
                
                var newStateName = "Q" + dStates.IndexOf(newState);
                if (!dTransitions.ContainsKey(newStateName))
                    dTransitions[newStateName] = new Dictionary<string, string>();

                var indexOf1 = "Q" + GetIndexOf(dStates, newStateTarget);
                dTransitions["Q" + GetIndexOf(dStates, newState)][symbol] = indexOf1;
            }


            foreach (var fState in _finalStates)
            {
                if (newState.Contains(fState))
                {
                    dFinish.Add("Q" + dStates.IndexOf(newState));
                    break;
                }
            }
        }

        return new DeterministicFiniteAutomaton(dTransitions, _alphabet, dFinish, "Q0");
    }
}