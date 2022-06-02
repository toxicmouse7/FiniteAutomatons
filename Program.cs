using FiniteAutomatons;


// for (var i = 0; i < 1024; ++i)
// {
//     var binaryInt = Convert.ToString(i, 2);
//     var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
//     
//     var machine = new NondeterministicFiniteAutomaton(@"/Users/aleksejgladkov/RiderProjects/FiniteAutomatons/test3.txt");
//     var detMachine = machine.ToDetFinAut();
//
//     var b1 = machine.Accept(sequence);
//     var b2 = detMachine.Accept(sequence);
//     
//     if (b1 != b2)
//     {
//         Console.WriteLine(sequence);
//         Console.WriteLine(b1);
//         Console.WriteLine(b2);
//         Console.WriteLine(false);
//         Environment.Exit(-1);
//     }
// }
//
// Console.WriteLine(true);
//((0+1*)+(11+010+0*))*

// 0const 1const * + 1concat
//var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("0+1*0");
var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("(0+11*00)*1*");
var detAutomaton = regAutomaton.ToDetFinAut();
//var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("11*");
//var dfa = regAutomaton.ToDetFinAut();
//Console.WriteLine(regAutomaton.Accept("11111111110"));
Console.WriteLine(regAutomaton.Accept("01100101001"));
Console.WriteLine(detAutomaton.Accept("01100101001"));
//Console.WriteLine(regAutomaton.Accept("1011111111"));