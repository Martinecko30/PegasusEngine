using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using PegasusEngine.Common;
using PegasusEngine.Core;

namespace PegasusEditor.TabPanels;

public static class DNDPayloadTypes
{
    public const string Mesh = "DND_PAYLOAD_MESH";
    public const string Texture = "DND_PAYLOAD_TEXTURE";
    public const string Scene = "DND_PAYLOAD_SCENE";
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct DNDPayload
{
    public ulong GuidValue;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Title;
    
    public readonly GUID Guid => new(GuidValue);
    
    public override string ToString() => $"{Title} ({GuidValue})";
}

public static class ImGuiDndWidgets
{
    public static void DragDropWidget(
        string? label,
        string payloadType,
        string displayValue,
        Action<DNDPayload> onDrop,
        EditorTheme theme,
        string? tooltip = null,
        Vector2 widgetSize = default,
        bool selected = true
    )
    {
        bool hasLabel = !string.IsNullOrEmpty(label);

        if (hasLabel)
        {
            ImGui.AlignTextToFramePadding();
            theme.PushColor(ImGuiCol.Text, EditorCol.Text2);
            ImGui.TextUnformatted(label);
            theme.PopColor();
            ImGui.SameLine();
        }
        
        Vector2 finalSize = widgetSize;
        if (finalSize.X <= 0)
            finalSize.X = ImGui.GetContentRegionAvail().X;
        else if (hasLabel)
            finalSize.X -= ImGui.CalcTextSize(label!).X + ImGui.GetStyle().ItemSpacing.X;
        
        if (finalSize.Y <= 0)
            finalSize.Y = 0;
        
        theme.PushColor(ImGuiCol.Text, selected ? EditorCol.Text1 : EditorCol.Text2);
        theme.PushColor(ImGuiCol.Button, EditorCol.Primary3);
        theme.PushColor(ImGuiCol.ButtonHovered, EditorCol.Primary3);
        theme.PushColor(ImGuiCol.ButtonActive, EditorCol.Primary3);
        
        string selectableId = "##" + (label ?? "DND") + "Selectable";

        ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));
        ImGui.Button(displayValue + selectableId, finalSize);
        ImGui.PopStyleVar();

        theme.PopColor(4);
        
        // Tooltip
        if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
        
        // Drag-drop target
        if (ImGui.BeginDragDropTarget())
        {
            try
            {
                unsafe
                {
                    ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload(payloadType);
                    if (payload.NativePtr != null)
                    {
                        int expectedSize = Marshal.SizeOf<DNDPayload>();
                        if (payload.DataSize != expectedSize)
                        {
                            // C++ had IM_ASSERT; in C# we can just ignore or throw depending on your preference.
                            // Throwing here makes mismatches obvious during development.
                            throw new InvalidOperationException(
                                $"Unexpected payload size. Expected {expectedSize}, got {payload.DataSize}.");
                        }

                        DNDPayload dndPayload = Marshal.PtrToStructure<DNDPayload>(payload.Data);
                        onDrop(dndPayload);
                    }
                }
            }
            finally
            {
                ImGui.EndDragDropTarget();
            }
        }
    }
}