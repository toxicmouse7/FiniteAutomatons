using FiniteAutomatons;

// 01110 - fail
// 0000010000 - fail
var machine = new NondeterministicFiniteAutomaton(@"/Users/aleksejgladkov/RiderProjects/FiniteAutomatons/test.txt");
var detMachine = machine.ToDetFinAut();

for (var i = 0; i < 1024; ++i)
{
    var binaryInt = Convert.ToString(i, 2);
    var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
    
    // Console.Write(machine.Accept(sequence));
    // Console.WriteLine(' ' + sequence);
    // Console.Write(detMachine.Accept(sequence));
    // Console.WriteLine(' ' + sequence);
    // Console.WriteLine();
    
    if (machine.Accept(sequence) != detMachine.Accept(sequence))
    {
        Console.WriteLine(false);
        break;
    }
}