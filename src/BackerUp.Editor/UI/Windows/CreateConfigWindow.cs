using BackerUp.Core.Models;
using BackerUp.Editor.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{
    public class CreateConfigWindow : EditWindow
    {

        public CreateConfigWindow()
        {
            IdBox = new TextBox { Label = "ID", Value = Config.GetJobs().Count.ToString() };

            SourcesList = new ListSelection { Label = "Sources", Values = new List<string>() };
            TargetsList = new ListSelection { Label = "Targets", Values = new List<string>() };
            MethodBox = new OptionBox
            {
                Label = "Method",
                Options = Enum.GetNames<BackupMethod>().ToList(),
                SelectedIndex = 0
            };
            TimingBox = new TextBox { Label = "Timing", Value = "* */0 * * *" };
            RetentionCountBox = new TextBox { Label = "Retention Count", Value = "3" };
            RetentionSizeBox = new TextBox { Label = "Retention Size", Value = "1" };

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
                    var job = new BackupJob();

                    job.Id = IdBox.Value;

                    if (Enum.TryParse<BackupMethod>(MethodBox.Value, true, out var method))
                        job.Method = method;

                    job.Timing = TimingBox.Value;

                    if (int.TryParse(RetentionCountBox.Value, out int count))
                        job.BackupRetention.Count = count;

                    if (int.TryParse(RetentionSizeBox.Value, out int size))
                        job.BackupRetention.Size = size;

                    Application.Jobs.Add(job);
                    Config.SaveJobs(Application.Jobs);
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
                { ConsoleKey.UpArrow, () => { MoveSelection(SelectedComponent - 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.DownArrow, () => { MoveSelection(SelectedComponent + 1); Application.NeedsRedraw = false; } },
                { ConsoleKey.Escape, () => OnCancel?.Invoke() }
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
