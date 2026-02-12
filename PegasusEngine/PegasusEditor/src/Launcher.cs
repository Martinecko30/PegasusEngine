using System.Numerics;
using FontAwesome;
using ImGuiNET;
using PegasusEngine.Pegasus.Core;
using PegasusEngine.Pegasus.Project;
using PegasusEngine.PegasusEditor.Dialogs;
using PegasusEngine.PegasusEditor.ImGuiContext;

namespace PegasusEngine.PegasusEditor;

public class Launcher
{
    private readonly EditorState _editorState;
    private readonly ProjectManager _projectManager;

    private bool _createProjectWindowOpen = false;
    
    private string projectName = string.Empty;
    private string folderPath = string.Empty;
    private bool folderPathSelected = false;
    
    public Launcher(EditorState editorState, ProjectManager projectManager)
    {
        _editorState = editorState;
        _projectManager = projectManager;
    }

    public void OnImGuiRender(ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None)
    {
        var theme = _editorState.Temp.EditorTheme;
        
        theme.PushColor(ImGuiCol.WindowBg, EditorCol.Background2);
        theme.PushColor(ImGuiCol.Button, EditorCol.Secondary2);
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.Begin("##Launcher", windowFlags);
        ImGui.PopStyleVar(2);
        
        Vector2 windowSize = ImGui.GetContentRegionAvail();
        float buttonWidth = windowSize.X / 8.0f;
        float buttonHeight = 40.0f;
        float spacingY = 16.0f;
        
        float totalHeight = buttonHeight * 2 + spacingY;
        ImGui.SetCursorPosY((windowSize.Y - totalHeight) / 2.0f);
        
        float centerX = (windowSize.X - buttonWidth) / 2.0f;
        ImGui.SetCursorPosX(centerX);

        theme.PushColor(ImGuiCol.Button, EditorCol.Accent1);
        
        ImGuiFonts.PushFont(Fonts.WumpusMono);
        if (ImGui.Button($"New Project {FontAwesomeIcons.DiagramProject}", new Vector2(buttonWidth, buttonHeight)))
            _createProjectWindowOpen = true;
        ImGuiFonts.PopFont();
        
        theme.PopColor();
        if (_createProjectWindowOpen)
        {
            DrawCreateProjectWindow();
        }

        ImGui.Spacing();
        ImGui.SetCursorPosX(centerX);
        ImGuiFonts.PushFont(Fonts.WumpusMono);
        if (ImGui.Button($"Open Project {FontAwesomeIcons.Share}", new Vector2(buttonWidth, buttonHeight)))
        {
            // Assuming your FilePickerDialog returns string.Empty on cancel
            string projectFilePath = DialogSystem.FilePickerDialog(ProjectManager.ProjectFileExtension, "Select Project File:");
            if (!string.IsNullOrEmpty(projectFilePath))
                if (_projectManager.OpenProject(projectFilePath))
                    _createProjectWindowOpen = false;
        }
        ImGuiFonts.PopFont();

        ImGui.End();
        theme.PopColor(2);
    }
    
    private void DrawCreateProjectWindow()
    {
        var theme = _editorState.Temp.EditorTheme;
        theme.PushColor(ImGuiCol.WindowBg, EditorCol.Background3);
        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 170), new Vector2(float.MaxValue, float.MaxValue));

        if (ImGui.Begin($"New Project " + FontAwesomeIcons.DiagramProject, ref _createProjectWindowOpen))
        {
            float margin = 3.0f;

            // Project Name
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.Text("Project Name");
            theme.PopColor();
            ImGui.SameLine(150.0f);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - margin);
            ImGui.InputTextWithHint("##projectName-input", "Project name", ref projectName, 250);

            // Location Picker
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.Text("Location");
            theme.PopColor();
            ImGui.SameLine(150.0f);

            string buttonLabel = string.IsNullOrEmpty(folderPath) ? "Select folder..." : folderPath;
            EditorCol textCol = folderPathSelected ? EditorCol.Text1 : EditorCol.Text2;
            
            theme.PushColor(ImGuiCol.Button, EditorCol.Primary3);
            theme.PushColor(ImGuiCol.Text, textCol);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));
            
            if (ImGui.Button(buttonLabel + "##folder-button", new Vector2(ImGui.GetContentRegionAvail().X - margin, 0)))
            {
                string path = DialogSystem.FolderPickerDialog("Select Project Folder");
                if (!string.IsNullOrEmpty(path))
                {
                    folderPath = path;
                    folderPathSelected = true;
                }
            }
            ImGui.PopStyleVar();
            theme.PopColor(2);

            // Validation
            string validationMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(projectName))
                validationMsg = "Project name cannot be empty.";
            else if (projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                validationMsg = "Project name contains invalid characters.";
            else if (!folderPathSelected || !Directory.Exists(folderPath))
                validationMsg = "Please select a valid folder location.";
            else
            {
                string fullPath = Path.Combine(folderPath, projectName);
                if (Directory.Exists(fullPath))
                    validationMsg = "Project folder already exists.";
            }

            if (!string.IsNullOrEmpty(validationMsg))
            {
                theme.PushColor(ImGuiCol.Text, EditorCol.Warning);
                ImGui.TextWrapped(validationMsg);
                theme.PopColor();
                ImGui.Dummy(new Vector2(0, 5));
            }

            // Create Button
            float createBtnHeight = 30.0f;
            Vector2 avail = ImGui.GetContentRegionAvail();
            ImGui.Dummy(new Vector2(0.0f, (avail.Y - createBtnHeight) / 2.0f));

            string createProjectLabel = "Create Project " + FontAwesomeIcons.Share;
            float centerX = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(createProjectLabel).X) / 2.0f;
            ImGui.SetCursorPosX(centerX);

            bool canCreate = string.IsNullOrEmpty(validationMsg);
            if (!canCreate) ImGui.BeginDisabled();

            if (ImGui.Button(createProjectLabel, new Vector2(0, createBtnHeight)) && canCreate)
            {
                string fullPath = Path.Combine(folderPath, projectName);
                if (_projectManager.NewProject(fullPath))
                {
                    _createProjectWindowOpen = false;
                }
            }

            if (!canCreate) ImGui.EndDisabled();
        }
        ImGui.End();
        theme.PopColor();
    }
}