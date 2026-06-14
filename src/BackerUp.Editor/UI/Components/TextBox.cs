namespace BackerUp.Editor.UI.Components
{
    public class TextBox : IComponent
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";

        public string Text => $"{Label}: {Value}";

        public Dictionary<ConsoleKeyInfo, Action> Pairs { get; } = new();

        public void Draw()
        {
            Console.Write($"{Label}: {Value}");
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (Value.Length > 0)
                    Value = Value.Substring(0, Value.Length - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                Value += key.KeyChar;
            }
        }
    }
}
