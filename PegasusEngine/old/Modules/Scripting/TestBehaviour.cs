namespace PegasusEngine.Modules.Scripting;

public class TestBehaviour : Behaviour
{
    public string Name = "TestBehaviour";
    public float TestValue = 16f;
    public override void Update()
    {
        Console.WriteLine("TestBehaviour");
    }
}