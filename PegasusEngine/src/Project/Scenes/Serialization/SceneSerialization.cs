namespace PegasusEngine.Project.Scenes.Serialization;

public sealed class SceneFileDto
{
    public ulong SceneGuid { get; set; }
    public string SceneName { get; set; } = string.Empty;

    public ulong SkyboxGuid { get; set; }
    public string SkyboxName { get; set; } = string.Empty;

    public List<EntityDto> Entities { get; set; } = new();
}

public sealed class EntityDto
{
    public ulong Guid { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public List<ComponentDto> Components { get; set; } = new();
}

public sealed class ComponentDto
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object?> Data { get; set; } = new();
}