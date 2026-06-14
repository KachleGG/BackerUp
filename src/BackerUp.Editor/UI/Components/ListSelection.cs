namespace BackerUp.Editor.UI.Components
{
    public class ListSelection : IComponent
    {
        public string Label { get; set; } = "";
        public List<string> Values { get; set; } = new List<string>();
        public Action OnPress { get; set; }

        public string Text => $"{Label}: {string.Join(", ", Values).Substring(0, Math.Min(20, string.Join(", ", Values).Length))}...";

        public Dictionary<ConsoleKeyInfo, Action> Pairs { get; } = new();

        public void Draw()
        {
            Console.Write($"{Label}: {string.Join(", ", Values).Substring(0, Math.Min(20, string.Join(", ", Values).Length))}...");
        }

        public void HandleKey(ConsoleKeyInfo key) { }
    }
}
