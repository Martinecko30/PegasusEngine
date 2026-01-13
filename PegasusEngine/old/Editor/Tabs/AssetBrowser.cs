#region

using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PegasusEngine.Core.InputSystem;
using PegasusEngine.Editor.Utils;
using PegasusEngine.Modules.Rendering.Textures;

#endregion

namespace PegasusEngine.Editor.Tabs;

public class AssetBrowser : TabPanel
{
    private string resourseFolder = "\\Resources";
    private string currentFolder;
    
    private Texture folderIcon, scriptIcon, fileIcon;
    
    private float padding = 16.0f;
    private float thumbnailSize = 80.0f; // 64 + 16

    private bool renaming;
    private string newName = "";
    private string selected = "";
    
    private bool requestDeletePopup;
    
    public override void Start()
    {
        Title = "Assets";
        resourseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        currentFolder = resourseFolder;
        
        folderIcon = new Texture(
            Path.Combine(resourseFolder, "Images", "FolderIcon.png"), 
            "FolderIcon"
        );
        
        scriptIcon = new Texture(
            Path.Combine(resourseFolder, "Images", "BehaviourScriptIcon.png"), 
            "ScriptIcon"
        );
        
        fileIcon = new Texture(
            Path.Combine(resourseFolder, "Images", "FileIcon.png"), 
            "FileIcon"
        );
    }

    public override void Render()
    {
        ImGui.Begin(Title);

        DeletePopup();

        if (ImGui.BeginPopup("ManipulationPopup"))
            ManipulationPopup();
        
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsWindowHovered())
            ImGui.OpenPopup("ManipulationPopup");
        

        if ((Input.KeyboardState.IsKeyPressed(Keys.Enter) ||
             ImGui.IsKeyPressed(ImGuiKey.Enter)) &&
            renaming &&
            !string.IsNullOrEmpty(selected)
            )
            RenameAsset();

        
        ShowAssets();
        
        ImGui.End();
    }

    public override void Update()
    {
        
    }

    private void ShowAssets()
    {
        if (currentFolder != resourseFolder)
            if (ImGui.Button("<- Back"))
                currentFolder = Directory.GetParent(currentFolder).FullName;
        
        float cellSize = thumbnailSize + padding;
        
        float panelWidth = ImGui.GetContentRegionAvail().X;
        int columntCount = (int) (panelWidth / cellSize);
        if (columntCount < 1)
            columntCount = 1;
        
        ImGui.Columns(columntCount, "Assets", false);
        
        string[] directories = Directory.GetDirectories(currentFolder);
        string[] files = Directory.GetFiles(currentFolder);
        
        string[] contents = directories.Concat(files).ToArray();

        foreach (var content in contents)
        {
            string contentName = Path.GetFileName(content);

            int iconId = folderIcon.textureID;
            if (File.Exists(content))
            {
                if (contentName.Contains(".cs"))
                    iconId = scriptIcon.textureID;
                else
                    iconId = fileIcon.textureID;
            }

            ImGui.ImageButton(contentName,
                iconId,
                new Vector2(thumbnailSize, thumbnailSize), 
                new Vector2(0, 1), 
                new Vector2(1, 0));
            
            if (ImGui.IsItemClicked() || ImGui.IsItemClicked(ImGuiMouseButton.Right))
                selected = contentName;
            
            if (ImGui.IsItemHovered() && 
                ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && 
                Directory.Exists(content)
                )
                currentFolder = Path.Combine(resourseFolder, contentName);


            if (renaming && selected == contentName)
            {
                ImGui.InputText($"##{contentName}", ref newName, 100);
            }
            else
                ImGui.Text(contentName);

            if (ImGui.IsItemClicked() || ImGui.IsItemClicked(ImGuiMouseButton.Right))
                selected = contentName;
            
            
            ImGui.NextColumn();
        }
        
        ImGui.Columns(1);
        
        ImGui.SliderFloat("Thumbnail Size", ref thumbnailSize, 16.0f, 512.0f);
        ImGui.SliderFloat("Padding", ref padding, 0.0f, 32.0f);
    }

    private void CreateNewScript(string name)
    {
        if (ScriptFactory.CreateScript(resourseFolder, "NewScript"))
            Console.WriteLine("Script created!");
        else
            Console.WriteLine("Script creation failed!");
        
        ImGui.CloseCurrentPopup();
    }

    private void ManipulationPopup()
    {
        if (ImGui.BeginMenu("New"))
        {
            if (ImGui.MenuItem("C# Script"))
            {
                string name = "NewBehaviourScript";
                
                CreateNewScript(name);
                    
                selected = name;
            }

            if (ImGui.MenuItem("Folder"))
            {
                string name = "NewFolder";

                Directory.CreateDirectory(Path.Combine(currentFolder, name));
                
                selected = name;
            }
                
            ImGui.EndMenu();
        }

        if (ImGui.MenuItem("Refresh"))
        {
            // TODO: Refreshing
        }

        if (ImGui.MenuItem("Show in explorer"))
        {
            Process.Start("explorer.exe", $"\"{currentFolder}\"");
        }

        if (ImGui.MenuItem("Rename"))
        {
            if (!string.IsNullOrEmpty(selected))
            {
                renaming = !renaming;
                newName = selected;
            }
        }

        if (ImGui.MenuItem("Delete"))
        {
            if (!string.IsNullOrEmpty(selected))
                requestDeletePopup = true; // Trigger next frame
        }

        ImGui.EndPopup();

        if (requestDeletePopup)
        {
            ImGui.OpenPopup("Delete?");
            requestDeletePopup = false;
        }
    }
    
    private void DeletePopup()
    {
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        
        if (ImGui.BeginPopupModal("Delete?", ImGuiWindowFlags.AlwaysAutoResize))
        {
            // Italic or bold doesn't work like this 
            // will need to change font
            
            // Bold text: "Are you sure you want to delete this?"
            ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]); // Optional: use different font if needed
            ImGui.TextColored(new Vector4(1, 1, 1, 1), "Are you sure you want to delete this?");
            ImGui.PopFont();

            // Italic-like text: show filename/folder name
            ImGui.Spacing();
            ImGui.Text("Deleting: ");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1.0f, 0.6f, 0.3f, 1.0f), selected); // Orange color for emphasis

            ImGui.Separator();

            if (ImGui.Button("Delete"))
            {
                string path = Path.Combine(currentFolder, selected);
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path, true);

                selected = "";
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                ImGui.CloseCurrentPopup();
            
            ImGui.EndPopup();
        }
    }

    private void RenameAsset()
    {
        string oldPath = Path.Combine(currentFolder, selected);
        string extension = Path.GetExtension(selected);
        string newPath = Path.Combine(currentFolder, Path.GetFileNameWithoutExtension(newName) + extension);
    
        if (!File.Exists(newPath) && !Directory.Exists(newPath))
        {
            if (File.Exists(oldPath))
            {
                if (oldPath.Contains(".cs"))
                {
                    string oldScriptName = Path.GetFileNameWithoutExtension(oldPath);
                    if (ScriptFactory.ChangeName(oldPath, oldScriptName, newName.Replace(".cs", "")))
                        Console.WriteLine("Renamed script successfully");
                    else
                        Console.WriteLine("Script renaming failed!");
                }
                File.Move(oldPath, newPath);
            }
            else if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
            selected = Path.GetFileName(newPath); // update selection
        }

        renaming = false;
    }
}