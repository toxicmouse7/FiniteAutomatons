using System.Text.RegularExpressions;

namespace FiniteAutomatons;

public class DeterministicFiniteAutomaton
{
    private readonly Dictionary<string, Dictionary<string, string>> _states = new();

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
        }

        return _finalStates.Contains(_currentState);
    }
}