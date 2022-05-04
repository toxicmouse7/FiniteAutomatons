using FiniteAutomatons;

var test = new NondeterministicFiniteAutomaton(@"/Users/aleksejgladkov/RiderProjects/FiniteAutomatons/test3.txt");
var test2 = test.ToDetFinAut();
Console.WriteLine(test.Accept("0000000001"));


for (var i = 0; i < 1024; ++i)
{
    var binaryInt = Convert.ToString(i, 2);
    var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
    
    var machine = new NondeterministicFiniteAutomaton(@"/Users/aleksejgladkov/RiderProjects/FiniteAutomatons/test3.txt");
    var detMachine = machine.ToDetFinAut();

    var b1 = machine.Accept(sequence);
    var b2 = detMachine.Accept(sequence);
    
    if (b1 != b2)
    {
        Console.WriteLine(sequence);
        Console.WriteLine(b1);
        Console.WriteLine(b2);
        Console.WriteLine(false);
        break;
    }
}

Console.WriteLine(true);

//NondeterministicFiniteAutomaton.FromRegularExpression("((0+1*)+(11+010+0*))*");
