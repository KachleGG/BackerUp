using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Components
{
    public class OptionBox : IComponent
    {
        public string Label { get; set; } = "";
        public List<string> Options { get; set; } = new();
        public int SelectedIndex { get; set; } = 0;

        public string Value => Options.Count > 0 ? Options[SelectedIndex] : "";
        public string Text => $"{Label}: {Value}";

        public Dictionary<ConsoleKeyInfo, Action> Pairs => new();

        public void Draw()
        {
            Console.Write($"{Label}: < {Value} >");
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            if (Options.Count == 0) return;

            if (key.Key == ConsoleKey.LeftArrow)
            {
                SelectedIndex = (SelectedIndex - 1 + Options.Count) % Options.Count;
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                SelectedIndex = (SelectedIndex + 1) % Options.Count;
            }
        }
    }
}
