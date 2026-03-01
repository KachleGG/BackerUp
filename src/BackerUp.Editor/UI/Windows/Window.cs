
namespace BackerUp.Editor.UI.Windows;

public abstract class Window
{
    public Application Application { get; set; }
    public List<Components.IComponent> Components { get; set; } = new List<Components.IComponent>();
    public int SelectedComponent { get; set; } = 0;

    public Dictionary<ConsoleKey, Action> Pairs {  get; set; }

    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 0;
    public bool DrawsBorder { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public virtual void Draw()
    {
        // ─│┌┐└┘├┤
        if (DrawsBorder)
            this.DrawBorder();

        for (int i = 0; i < Components.Count; i++) {
            if (IsActive && i == SelectedComponent)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            Console.SetCursorPosition(OffsetX + 2, OffsetY + 2 + i);
            Components[i].Draw();
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    protected void DrawBorder()
    {
        Console.Write("┌");
        for (int i = 0; i < Console.WindowWidth - 2; i++) {
            Console.Write("─");
        }
        Console.WriteLine("┐");
        for (int i = 0; i < Console.WindowHeight - 2; i++) {
            Console.Write("│");
            for (int j = 0; j < Console.WindowWidth - 2; j++) {
                Console.Write(" ");
            }
            Console.WriteLine("│");
        }
        Console.Write("└");
        for (int i = 0; i < Console.WindowWidth - 2; i++) {
            Console.Write("─");
        }
        Console.WriteLine("┘");
    }

    public virtual void HandleKey(ConsoleKeyInfo key)
    {
        Pairs.TryGetValue(key.Key, out var action);
        action?.Invoke();
    }
}
