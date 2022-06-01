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
var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("(0+11*00)*1*");
//var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("01*");
//var dfa = regAutomaton.ToDetFinAut();
Console.WriteLine(regAutomaton.Accept("0100"));
//Console.WriteLine(regAutomaton.Accept("0110011001"));
//Console.WriteLine(regAutomaton.Accept("011"));