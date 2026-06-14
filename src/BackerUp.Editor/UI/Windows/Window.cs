
namespace BackerUp.Editor.UI.Windows;

public abstract class Window
{
    public Application Application { get; set; }
    public List<Components.IComponent> Components { get; set; } = new List<Components.IComponent>();
    public int SelectedComponent { get; set; } = 0;

    public Dictionary<ConsoleKey, Action> Pairs { get; set; }

    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
    public bool DrawsBorder { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int ScrollOffset { get; set; } = 0;

    public virtual void Draw()
    {
        // ─│┌┐└┘├┤
        if (DrawsBorder)
            this.DrawBorder();

        int firstRow = OffsetY + 2;
        int lastRow = Console.WindowHeight - 2;
        int totalSlots = lastRow - firstRow + 1;

        if (totalSlots <= 0 || Components.Count == 0) return;

        if (Components.Count <= totalSlots)
        {
            ScrollOffset = 0;
            for (int i = 0; i < Components.Count; i++)
            {
                if (IsActive && i == SelectedComponent)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.SetCursorPosition(OffsetX + 2, firstRow + i);
                Components[i].Draw();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            return;
        }

        int maxVisible = Math.Max(1, totalSlots - 2);
        if (SelectedComponent < ScrollOffset)
            ScrollOffset = SelectedComponent;
        if (SelectedComponent >= ScrollOffset + maxVisible)
            ScrollOffset = SelectedComponent - maxVisible + 1;
        if (ScrollOffset < 0) ScrollOffset = 0;

        int row = firstRow;

        // ↓↑
        if (ScrollOffset > 0)
        {
            Console.SetCursorPosition(OffsetX + 2, row);
            Console.Write("↑");
            row++;
        }

        int idx = ScrollOffset;
        while (idx < Components.Count && row <= lastRow)
        {
            if (row == lastRow && idx + 1 < Components.Count)
            {
                Console.SetCursorPosition(OffsetX + 2, row);
                Console.Write("↓");
                break;
            }

            if (IsActive && idx == SelectedComponent)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            Console.SetCursorPosition(OffsetX + 2, row);
            Components[idx].Draw();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;

            idx++;
            row++;
        }
    }

    protected void DrawBorder()
    {
        Console.Write("┌");
        for (int i = 0; i < Console.WindowWidth - 2; i++)
        {
            Console.Write("─");
        }
        Console.WriteLine("┐");
        for (int i = 0; i < Console.WindowHeight - 2; i++)
        {
            Console.Write("│");
            for (int j = 0; j < Console.WindowWidth - 2; j++)
            {
                Console.Write(" ");
            }
            Console.WriteLine("│");
        }
        Console.Write("└");
        for (int i = 0; i < Console.WindowWidth - 2; i++)
        {
            Console.Write("─");
        }
        Console.WriteLine("┘");
    }

    public virtual void HandleKey(ConsoleKeyInfo key)
    {
        Pairs.TryGetValue(key.Key, out var action);
        action?.Invoke();
    }

    protected virtual int GetContentFirstRow() => OffsetY + 2;

    public void MoveSelection(int newIndex)
    {
        if (Components.Count == 0) return;

        int oldIndex = SelectedComponent;
        SelectedComponent = Math.Clamp(newIndex, 0, Components.Count - 1);

        if (oldIndex == SelectedComponent) return;

        int firstRow = GetContentFirstRow();
        int lastRow = Console.WindowHeight - 2;
        int totalSlots = lastRow - firstRow + 1;

        if (totalSlots <= 0) return;

        bool needsScroll = Components.Count > totalSlots
            && (SelectedComponent < ScrollOffset
                || SelectedComponent >= ScrollOffset + Math.Max(1, totalSlots - 2));

        if (needsScroll)
        {
            Draw();
            return;
        }

        RedrawComponent(oldIndex, false, firstRow);
        RedrawComponent(SelectedComponent, true, firstRow);
    }

    private void RedrawComponent(int index, bool selected, int firstRow)
    {
        int rowOffset = index - ScrollOffset + (ScrollOffset > 0 ? 1 : 0);
        int row = firstRow + rowOffset;
        int lastRow = Console.WindowHeight - 2;

        if (row < firstRow || row > lastRow) return;

        if (IsActive && selected)
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.SetCursorPosition(OffsetX + 2, row);
        Components[index].Draw();
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.ForegroundColor = ConsoleColor.White;
    }
}
