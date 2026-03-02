using BackerUp.Core.Models;
using BackerUp.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class CreateConfigWindow : Window
    {
        public Action OnClose { get; set; }

        private TextBox _idBox;
        private OptionBox _methodBox;
        private TextBox _timingBox;
        private TextBox _retentionCountBox;
        private TextBox _retentionSizeBox;

        public CreateConfigWindow(int jobCount)
        {
            _idBox = new TextBox { Label = "ID", Value = jobCount.ToString() };
            _methodBox = new OptionBox
            {
                Label = "Method",
                Options = Enum.GetNames<BackupMethod>().ToList(),
                SelectedIndex = 0
            };
            _timingBox = new TextBox { Label = "Timing", Value = "* */0 * * *" };
            _retentionCountBox = new TextBox { Label = "Retention Count", Value = "3" };
            _retentionSizeBox = new TextBox { Label = "Retention Size", Value = "1" };

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
                    var job = new BackupJob();

                    if (int.TryParse(_idBox.Value, out int id))
                        job.Id = id;

                    if (Enum.TryParse<BackupMethod>(_methodBox.Value, true, out var method))
                        job.Method = method;

                    job.Timing = _timingBox.Value;

                    if (int.TryParse(_retentionCountBox.Value, out int count))
                        job.BackupRetention.Count = count;

                    if (int.TryParse(_retentionSizeBox.Value, out int size))
                        job.BackupRetention.Size = size;

                    Application.Jobs.Add(job);
                    Config.SaveJobs(Application.Jobs);
                    OnClose?.Invoke();
                }
            });

            Components.Add(new Button
            {
                Text = "Cancel",
                OnPress = () => OnClose?.Invoke()
            });

            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.UpArrow, () => SelectedComponent = Math.Max(0, SelectedComponent - 1) },
                { ConsoleKey.DownArrow, () => SelectedComponent = Math.Min(Components.Count - 1, SelectedComponent + 1) },
                { ConsoleKey.Escape, () => OnClose?.Invoke() }
            };
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
