using BackerUp.Core.Models;
using BackerUp.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class EditWindow : Window
    {
        public BackupJob Job;
        public Action OnSave { get; set; }
        public Action OnCancel { get; set; }

        private TextBox _idBox;
        private TextBox _methodBox;
        private TextBox _timingBox;
        private TextBox _retentionCountBox;
        private TextBox _retentionSizeBox;

        public EditWindow(BackupJob job)
        {
            Job = job;
        }

        public void Initialize()
        {
            _idBox = new TextBox { Label = "ID", Value = Job.Id.ToString() };
            _methodBox = new TextBox { Label = "Method", Value = Job.Method.ToString() };
            _timingBox = new TextBox { Label = "Timing", Value = Job.Timing };
            _retentionCountBox = new TextBox { Label = "Retention Count", Value = Job.BackupRetention.Count.ToString() };
            _retentionSizeBox = new TextBox { Label = "Retention Size", Value = Job.BackupRetention.Size.ToString() };

            Components.Add(_idBox);
            Components.Add(_methodBox);
            Components.Add(_timingBox);
            Components.Add(_retentionCountBox);
            Components.Add(_retentionSizeBox);

            Components.Add(new Button
            {
                Text = "OK",
                OnPress = () =>
                {
                    if (int.TryParse(_idBox.Value, out int id))
                        Job.Id = id;

                    if (Enum.TryParse<BackupMethod>(_methodBox.Value, true, out var method))
                        Job.Method = method;

                    Job.Timing = _timingBox.Value;

                    if (int.TryParse(_retentionCountBox.Value, out int count))
                        Job.BackupRetention.Count = count;

                    if (int.TryParse(_retentionSizeBox.Value, out int size))
                        Job.BackupRetention.Size = size;

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
                { ConsoleKey.UpArrow, () => this.SelectedComponent = Math.Max(0, this.SelectedComponent - 1) },
                { ConsoleKey.DownArrow, () => this.SelectedComponent = Math.Min(this.Components.Count - 1, this.SelectedComponent + 1) },
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
                else
                {
                    component.HandleKey(key);
                }
            }
        }
    }
}
