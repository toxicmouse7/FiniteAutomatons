using FiniteAutomatons;

var machine = new NondeterministicFiniteAutomaton(@"C:\Users\Aleksej\RiderProjects\FiniteAutomatons\test2.txt");
var detMachine = machine.ToDetFinAut();

for (var i = 0; i < 32; ++i)
{
    var binaryInt = Convert.ToString(i, 2);
    var sequence = string.Concat(Enumerable.Repeat("0", 5 - binaryInt.Length)) + binaryInt;
    Console.Write(machine.Accept(sequence));
    Console.WriteLine(' ' + sequence);
    Console.Write(machine.Accept(sequence));
    Console.WriteLine(' ' + sequence);
    Console.WriteLine();
}