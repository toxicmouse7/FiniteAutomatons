using System.Text.RegularExpressions;

namespace FiniteAutomatons;

public class DeterministicFiniteAutomaton
{
    private Dictionary<string, Dictionary<string, string>> _states = new();

    private readonly string[] _alphabet;
    private readonly List<string> _finalStates = new();
    private string _currentState;

    public DeterministicFiniteAutomaton(Dictionary<string, Dictionary<string, string>> states,
        string[] alphabet,
        List<string> finalStates,
        string initialState)
    {
        _states = states;
        _alphabet = alphabet;
        _finalStates = finalStates;
        _currentState = initialState;
    }

    public DeterministicFiniteAutomaton(in string filename)
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
            _states[state] = new Dictionary<string, string>();

            var allowedTransitionsString = allowedTransitionsRegex.Match(lines[i]).Groups["transitions"].Value;
            var allowedTransitions = replaceCommaRegex.Replace(allowedTransitionsString, "];")
                .Split(';');
            foreach (var transition in allowedTransitions)
            {
                var matches = transitionRegex.Match(transition).Groups;
                var states = matches["states"].Value;
                var matchedStates = statesRegex.Matches(states);
                _states[state][matches["letter"].Value] = matchedStates[0].Groups["state"].Value;
            }
        }

        _currentState = lines[2 + statesCount].Substring(1);
        var allowedStates = lines.Last().Substring(1, lines.Last().Length - 2)
            .Split(',');
        foreach (var allowedState in allowedStates) _finalStates.Add(allowedState.Substring(1));
    }

    public bool Accept(in string input)
    {
        foreach (var symbol in input)
        {
            if (!_alphabet.Contains(symbol.ToString())) return false;

            if (_states[_currentState].ContainsKey(symbol.ToString()))
                _currentState = _states[_currentState][symbol.ToString()];
            else
                return false;
        }

        return _finalStates.Contains(_currentState);
    }
    
    private void MoveStates(Dictionary<string, List<string>> components)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        var newStatesList = components.Keys.ToList();

        foreach (var (key, _) in components)
        {
            result[key] = new Dictionary<string, string>();
            foreach (var (transitionKey, transitionValue) in _states[key])
            {
                var state = transitionValue;
                if (newStatesList.Contains(state) == false)
                {
                    foreach (var (s, _) in components.Where(i => i.Value.Contains(state)))
                    {
                        state = s;
                    }
                }
                
                if (newStatesList.Contains(state))
                {
                    result[key].Add(transitionKey, state);
                }
                else
                {
                    if (components.Any(i => i.Value.Contains(state)))
                    {
                        result[key].Add(transitionKey, state);
                    }
                }
            }
        }

        _states = result;
    }
    
    private Dictionary<(string, string), List<string>> GetListReversEdges()
    {
        var statesList = _states.Keys.ToList();
        var reversEdges = new Dictionary<(string, string), List<string>>();
        foreach (var state in statesList)
        {
            foreach (var letter in _alphabet)
            {
                var tmp = new List<string>();
                reversEdges.Add((state, letter), tmp);
            }
        }

        foreach (var (reversState, value) in _states)
        {
            foreach (var (transitionKey, transitionValue) in value)
            {
                var letter = transitionKey;
                reversEdges[(transitionValue, letter)].Add(reversState);
            }
        }

        return reversEdges;
    }
    
    private bool[][] BuildTable()
    {
        var n = _states.Count;
        var statesList = _states.Keys.ToList();
        var reversEdges = GetListReversEdges();

        var q = new Queue<(int, int)>();


        var marked = new bool[n][];
        for (var i = 0; i < n; i++)
        {
            marked[i] = new bool[n];
            for (var j = 0; j < n; j++)
                marked[i][j] = false;
        }

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                if (marked[i][j] ||
                    _finalStates.Contains(statesList[i]) == _finalStates.Contains(statesList[j])) continue;
                marked[i][j] = true;
                marked[j][i] = true;
                q.Enqueue((i, j));
            }
        }

        while (q.Count > 0)
        {
            var (u, v) = q.Dequeue();
            foreach (var c in _alphabet)
            {
                foreach (var r in reversEdges[(statesList[u], c)])
                {
                    foreach (var s in reversEdges[(statesList[v], c)])
                    {
                        var indexR = statesList.IndexOf(r);
                        var indexS = statesList.IndexOf(s);
                        if (marked[indexR][indexS]) continue;
                        
                        marked[indexR][indexS] = true;
                        marked[indexS][indexR] = true;
                        q.Enqueue((indexR, indexS));
                    }
                }
            }
        }

        return marked;
    }
    
    public void Minimize()
    {
        var n = _states.Count;
        var statesList = _states.Keys.ToList();
        var marked = BuildTable();
        
        var components = new Dictionary<string, List<string>>();

        var k = 0;
        while (k < marked.Length)
        {
            if (marked[k].Length == 1)
            {
                k++;
                continue;
            }

            components[statesList[k]] = new List<string>();
            for (var i = k + 1; i < n; i++)
            {
                if (!marked[i].ToList().SequenceEqual(marked[k].ToList())) continue;
                marked[i] = new bool[1];
                components[statesList[k]].Add(statesList[i]);
            }

            k++;
        }

        MoveStates(components);
    }
}