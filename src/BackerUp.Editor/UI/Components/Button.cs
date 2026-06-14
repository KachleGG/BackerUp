namespace BackerUp.Editor.UI.Components
{
    public class Button : IComponent
    {
        public string Text { get; set; } = "";
        public Action OnPress { get; set; }

        public Dictionary<ConsoleKeyInfo, Action> Pairs { get; } = new();

        public void Draw()
        {
            Console.Write($"[ {Text} ]");
        }

        public void HandleKey(ConsoleKeyInfo key) { }
    }
}
