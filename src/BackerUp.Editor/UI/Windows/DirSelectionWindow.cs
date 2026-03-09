using BackerUp.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class DirSelectionWindow : Window
    {
        public List<string> SelectedDirs;
        public string CurrentPath { get; set; }
        public Action OnTab { get; set; }
        public Action OnEscape { get; set; }
        public Action OnDirsChanged { get; set; }

        public DirSelectionWindow(List<string> selectedDirs)
        {
            SelectedDirs = selectedDirs;
        }

        public void Initialize()
        {
            CurrentPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";
            LoadDirectory();
        }

        public void LoadDirectory()
        {
            Components.Clear();
            SelectedComponent = 0;
            ScrollOffset = 0;

            Components.Add(new SelectionField { Text = ".." });

            try
            {
                foreach (var dir in Directory.EnumerateDirectories(CurrentPath))
                {
                    Components.Add(new SelectionField { Text = Path.GetFileName(dir) });
                }
            }
            catch { }

            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.UpArrow, () => { MoveSelection(SelectedComponent - 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.DownArrow, () => { MoveSelection(SelectedComponent + 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.Enter, () => NavigateInto() },
                { ConsoleKey.Spacebar, () => AddSelected() },
                { ConsoleKey.Tab, () => OnTab?.Invoke() },
                { ConsoleKey.Escape, () => OnEscape?.Invoke() }
            };
        }

        private void NavigateInto()
        {
            if (SelectedComponent == 0)
            {
                var parent = Directory.GetParent(CurrentPath);
                if (parent != null)
                {
                    CurrentPath = parent.FullName;
                    LoadDirectory();
                }
            }
            else
            {
                var fullPath = Path.Combine(CurrentPath, Components[SelectedComponent].Text);
                if (Directory.Exists(fullPath))
                {
                    CurrentPath = fullPath;
                    LoadDirectory();
                }
            }
        }

        private void AddSelected()
        {
            if (SelectedComponent == 0) return;

            var fullPath = Path.Combine(CurrentPath, Components[SelectedComponent].Text);
            if (!SelectedDirs.Contains(fullPath))
            {
                SelectedDirs.Add(fullPath);
                OnDirsChanged?.Invoke();
            }
        }

        public override void Draw()
        {
            Console.SetCursorPosition(OffsetX + 2, OffsetY + 1);
            Console.Write(CurrentPath);

            int firstRow = OffsetY + 3;
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

        protected override int GetContentFirstRow() => OffsetY + 3;
    }
}
