using BackerUp.Editor.UI.Components;

namespace BackerUp.Editor.UI.Windows
{
    public class DirListingWindow : Window
    {
        public List<string> Dirs;
        public Action OnTab { get; set; }
        public Action OnEscape { get; set; }
        public Action OnEnter { get; set; }

        public DirListingWindow(List<string> dirs)
        {
            Dirs = dirs;
        }

        public void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            Components.Clear();
            SelectedComponent = 0;
            ScrollOffset = 0;

            foreach (var dir in Dirs)
            {
                Components.Add(new SelectionField { Text = dir });
            }

            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.UpArrow, () => { MoveSelection(SelectedComponent - 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.DownArrow, () => { MoveSelection(SelectedComponent + 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.Delete, () => RemoveSelected() },
                { ConsoleKey.Tab, () => OnTab?.Invoke() },
                { ConsoleKey.Escape, () => OnEscape?.Invoke() },
                { ConsoleKey.Enter, () => OnEnter?.Invoke() }
            };
        }

        private void RemoveSelected()
        {
            if (Dirs.Count == 0 || SelectedComponent < 0 || SelectedComponent >= Dirs.Count)
                return;

            Dirs.RemoveAt(SelectedComponent);
            Refresh();

            if (SelectedComponent >= Dirs.Count)
                SelectedComponent = Math.Max(0, Dirs.Count - 1);
        }
    }
}
