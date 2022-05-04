using FiniteAutomatons;


/*for (var i = 0; i < 1024; ++i)
{
    var binaryInt = Convert.ToString(i, 2);
    var sequence = string.Concat(Enumerable.Repeat("0", 10 - binaryInt.Length)) + binaryInt;
    
    var machine = new NondeterministicFiniteAutomaton(@"C:\Users\Aleksej\RiderProjects\FiniteAutomatons\test2.txt");
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

Console.WriteLine(true);*/
//((0+1*)+(11+010+0*))*
NondeterministicFiniteAutomaton.FromRegularExpression("0+1*");
