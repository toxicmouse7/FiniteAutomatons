using FiniteAutomatons;
//
// for (var i = 0; i < 1024; ++i)
// {
//     var binaryInt = Convert.ToString(i, 2);
//     var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
//     var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("(0+10)*(1+11)(0+01)*");
//     var dfa = regAutomaton.ToDetFinAut();
//     var dfaToMin = regAutomaton.ToDetFinAut();
//     dfaToMin.MinimizeV2();
//
//     // if (regAutomaton.Accept(sequence) != dfa.Accept(sequence))
//     // {
//     //     Console.WriteLine("Error. " + sequence);
//     //     break;
//     // }
//
//     var b1 = regAutomaton.Accept(sequence);
//     var b2 = dfa.Accept(sequence);
//     var b3 = dfaToMin.Accept(sequence);
//                 
//     if (b1 != b2)
//     {
//         Console.WriteLine(sequence + " nka->dka error");
//     }
//                 
//     if (b3 != b2)
//     {
//         Console.WriteLine(sequence + " minimalize error");
//     }
// }

var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("(0+10)*(1+11)(0+01)*");
//var regAutomaton = NondeterministicFiniteAutomaton.FromRegularExpression("11*");
var detAutomaton = regAutomaton.ToDetFinAutV2();
detAutomaton.Minimize();

var sequence = "0000000111";

Console.WriteLine(regAutomaton.Accept(sequence));
Console.WriteLine(detAutomaton.Accept(sequence));

