using BackerUp.Core.Models;

namespace BackerUp.Editor.UI.Components
{
    public class SelectionField : IComponent
    {
        public string Text { get; set; }
        public BackupJob? Job { get; set; }

        Dictionary<ConsoleKeyInfo, Action> IComponent.Pairs => throw new NotImplementedException();

        public void Draw()
        {
            Console.Write(Text);
        }

        public void HandleKey(ConsoleKeyInfo key) { }
    }
}
