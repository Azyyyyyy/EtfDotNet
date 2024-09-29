using EtfDotNet.Example;

start:
Console.WriteLine("What example do you want to run? [1 - 3]");
string? exampleNumberS = Console.ReadLine();

if (!int.TryParse(exampleNumberS, out int exampleNumber))
{
    Console.WriteLine("Please enter a valid number.");
    goto start;
}

if (exampleNumber == 1)
{
    SecondExample.RunExample();
}
else if (exampleNumber == 2)
{
    FirstExample.RunExample();
}
else if (exampleNumber == 3)
{
    ThirdExample.RunExample();
}
else
{
    Console.WriteLine("Number range is 1 - 3");
}