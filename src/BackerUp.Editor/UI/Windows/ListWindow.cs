using BackerUp.Core.Models;
using BackerUp.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class ListWindow : Window
    {
        public List<BackupJob> Jobs;
        public Action<BackupJob> OnSelectionChanged { get; set; }
        public Action OnEnter { get; set; }
        public Action OnDelete { get; set; }
        public Action OnCreate { get; set; }

        public ListWindow(List<BackupJob> jobs)
        {
            Jobs = jobs;
        }

        public void Initialize()
        {
            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.UpArrow, () => {
                    MoveSelection(SelectedComponent - 1);
                    OnSelectionChanged?.Invoke(Jobs[SelectedComponent]);
                }},
                { ConsoleKey.DownArrow, () => {
                    MoveSelection(SelectedComponent + 1);
                    OnSelectionChanged?.Invoke(Jobs[SelectedComponent]);
                }},
                { ConsoleKey.Enter, () => OnEnter?.Invoke() },
                { ConsoleKey.Delete, () => OnDelete?.Invoke() },
                { ConsoleKey.Insert, () => OnCreate?.Invoke() }
            };

            for (int i = 0; i < Jobs.Count; i++)
            {
                // TODO: replace job id with name
                Components.Add(new SelectionField { Text = Jobs[i].Id.ToString() });
            }
        }
    }
}
