namespace PegasusEngine.old.Editor.Utils;

public class ScriptFactory
{
    public static bool CreateScript(string path, string name)
    {
        string script = "using PegasusEngine.Engine.Scripting;\n\n"+
                        $"public class {name} : Behaviour\n" +
                        "{\n" +
                        "   public override void Update()\n" +
                        "   {\n" +
                        "       \n" +
                        "   }\n" +
                        "}"
                        ;

        try
        {
            string filePath = Path.Combine(path, $"{name}.cs");
            File.WriteAllText(filePath, script);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public static bool ChangeName(string path, string oldName, string newName)
    {
        try
        {
            string content = File.ReadAllText(path);
            content = content.Replace(oldName, newName);
            File.WriteAllText(path, content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }
}