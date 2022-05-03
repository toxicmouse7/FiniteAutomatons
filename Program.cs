using FiniteAutomatons;

// 01110 - fail
// 0000010000 - fail
// var machine = new NondeterministicFiniteAutomaton(@"C:\Users\Aleksej\RiderProjects\FiniteAutomatons\test3.txt");
// var detMachine = machine.ToDetFinAut();
//
// for (var i = 0; i < 1024; ++i)
// {
//     var binaryInt = Convert.ToString(i, 2);
//     var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
//
//     if (machine.Accept(sequence) != detMachine.Accept(sequence))
//     {
//         Console.WriteLine(false);
//         break;
//     }
// }
//
// Console.WriteLine(true);

NondeterministicFiniteAutomaton.FromRegularExpression("((0+1*)+(11+010+0*))*");
