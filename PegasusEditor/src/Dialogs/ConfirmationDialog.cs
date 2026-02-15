using System.Numerics;
using ImGuiNET;

namespace PegasusEditor.Dialogs;

public class ConfirmationDialog
{
    public static void ConfirmAndExecute(
        ref bool shouldExecute,
        string popupTitle,
        string popupMessage,
        Action onConfirm,
        EditorState state
        )
    {
        if (!shouldExecute)
            return;

        var theme = state.Temp.EditorTheme;
        ImGui.OpenPopup(popupTitle);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));
        
        ImGui.SetNextWindowSizeConstraints(new(300.0f, 0.0f), new(400.0f, float.MaxValue));
        theme.PushColor(ImGuiCol.PopupBg, EditorCol.Background1);

        if (ImGui.BeginPopupModal(popupTitle, ref shouldExecute, ImGuiWindowFlags.AlwaysAutoResize))
        {
            theme.PushColor(ImGuiCol.Text, EditorCol.Warning);
            ImGui.TextWrapped(popupMessage);
            theme.PopColor();
            
            float panelWidth = ImGui.GetContentRegionAvail().X;
            float buttonWidth = panelWidth * 0.5f - 5.0f;
            ImGui.Dummy(new(5.0f, 5.0f));
            
            theme.PushColor(ImGuiCol.Button, EditorCol.Primary3);
            if (ImGui.Button("Yes", new(buttonWidth, 0)))
            {
                onConfirm();
                shouldExecute = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.SameLine();
            if (ImGui.Button("No", new(buttonWidth, 0)))
            {
                shouldExecute = false;
                ImGui.CloseCurrentPopup();
            }
            theme.PopColor();
            ImGui.EndPopup();
        }
        theme.PopColor();
        ImGui.PopStyleVar();
    }
}