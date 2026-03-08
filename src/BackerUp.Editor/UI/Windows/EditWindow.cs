using BackerUp.Core.Models;
using BackerUp.Editor.UI.Components;
using BackerUp.Editor.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class EditWindow : Window
    {
        public BackupJob? Job;
        public Action OnSave { get; set; }
        public Action OnCancel { get; set; }

        protected TextBox IdBox;
        protected ListSelection SourcesList;
        protected ListSelection TargetsList;
        protected OptionBox MethodBox;
        protected TextBox TimingBox;
        protected TextBox RetentionCountBox;
        protected TextBox RetentionSizeBox;

        public EditWindow() { }
        public EditWindow(BackupJob job)
        {
            Job = job;
        }

        public void Initialize()
        {
            IdBox = new TextBox { Label = "ID", Value = Job.Id.ToString() };
            SourcesList = new ListSelection { Label = "Sources", Values = new List<string>(Job.Sources), OnPress = () => {
                var doubleWindow = new DoubleWindow();
                var dirSelectionWindow = new DirSelectionWindow(SourcesList.Values);
                var dirListingWindow = new DirListingWindow(SourcesList.Values);

                dirSelectionWindow.Initialize();
                dirListingWindow.Initialize();

                dirSelectionWindow.OnTab = () => doubleWindow.ActivePane = Pane.Right;
                dirListingWindow.OnTab = () => doubleWindow.ActivePane = Pane.Left;
                dirSelectionWindow.OnDirsChanged = () => dirListingWindow.Refresh();
                dirSelectionWindow.OnEscape = () => Application.CloseWindow();
                dirListingWindow.OnEscape = () => Application.CloseWindow();
                dirListingWindow.OnEnter = () => Application.CloseWindow();

                doubleWindow.LeftWindow = dirSelectionWindow;
                doubleWindow.RightWindow = dirListingWindow;
                doubleWindow.ActivePane = Pane.Left;

                Application.OpenWindow(doubleWindow);
            } };
            TargetsList = new ListSelection { Label = "Targets", Values = new List<string>(Job.Targets), OnPress = () => {
                var doubleWindow = new DoubleWindow();
                var dirSelectionWindow = new DirSelectionWindow(TargetsList.Values);
                var dirListingWindow = new DirListingWindow(TargetsList.Values);

                dirSelectionWindow.Initialize();
                dirListingWindow.Initialize();

                dirSelectionWindow.OnTab = () => doubleWindow.ActivePane = Pane.Right;
                dirListingWindow.OnTab = () => doubleWindow.ActivePane = Pane.Left;
                dirSelectionWindow.OnDirsChanged = () => dirListingWindow.Refresh();
                dirSelectionWindow.OnEscape = () => Application.CloseWindow();
                dirListingWindow.OnEscape = () => Application.CloseWindow();
                dirListingWindow.OnEnter = () => Application.CloseWindow();

                doubleWindow.LeftWindow = dirSelectionWindow;
                doubleWindow.RightWindow = dirListingWindow;
                doubleWindow.ActivePane = Pane.Left;

                Application.OpenWindow(doubleWindow);
            } };
            var methodNames = Enum.GetNames<BackupMethod>().ToList();
            MethodBox = new OptionBox
            {
                Label = "Method",
                Options = methodNames,
                SelectedIndex = methodNames.IndexOf(Job.Method.ToString())
            };
            TimingBox = new TextBox { Label = "Timing", Value = Job.Timing };
            RetentionCountBox = new TextBox { Label = "Retention Count", Value = Job.BackupRetention.Count.ToString() };
            RetentionSizeBox = new TextBox { Label = "Retention Size", Value = Job.BackupRetention.Size.ToString() };

            Components.Add(IdBox);
            Components.Add(SourcesList);
            Components.Add(TargetsList);
            Components.Add(MethodBox);
            Components.Add(TimingBox);
            Components.Add(RetentionCountBox);
            Components.Add(RetentionSizeBox);

            Components.Add(new Button
            {
                Text = "OK",
                OnPress = () =>
                {
                    Job.Id = IdBox.Value;

                    if (Enum.TryParse<BackupMethod>(MethodBox.Value, true, out var method))
                        Job.Method = method;


                    Job.Timing = TimingBox.Value;

                    if (int.TryParse(RetentionCountBox.Value, out int count))
                        Job.BackupRetention.Count = count;

                    if (int.TryParse(RetentionSizeBox.Value, out int size))
                        Job.BackupRetention.Size = size;

                    Job.Sources = new List<string>(SourcesList.Values);
                    Job.Targets = new List<string>(TargetsList.Values);

                    Config.SaveJobs(this.Application.Jobs);
                    OnSave?.Invoke();
                }
            });

            Components.Add(new Button
            {
                Text = "Cancel",
                OnPress = () => OnCancel?.Invoke()
            });

            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.UpArrow, () => MoveSelection(SelectedComponent - 1) },
                { ConsoleKey.DownArrow, () => MoveSelection(SelectedComponent + 1) },
                { ConsoleKey.Escape, () => OnCancel?.Invoke() }
            };
        }

        public void LoadJob(BackupJob job)
        {
            Job = job;
            Components.Clear();
            SelectedComponent = 0;
            Initialize();
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (Pairs.TryGetValue(key.Key, out var action))
            {
                action.Invoke();
                return;
            }

            if (SelectedComponent >= 0 && SelectedComponent < Components.Count)
            {
                var component = Components[SelectedComponent];
                if (key.Key == ConsoleKey.Enter && component is Button button)
                {
                    button.OnPress?.Invoke();
                } 
                else if (key.Key == ConsoleKey.Enter && component is ListSelection listSelection)
                {
                    listSelection.OnPress?.Invoke();
                }
                else
                {
                    component.HandleKey(key);
                }
            }
        }
    }
}
