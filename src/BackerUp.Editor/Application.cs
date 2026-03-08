using BackerUp.Core.Models;
using BackerUp.Editor.UI.Dialogues;
using BackerUp.Editor.UI.Enums;
using BackerUp.Editor.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor
{
    public class Application
    {
        public List<BackupJob> Jobs { get; set; }
        public Stack<Window> windows { get; set; }

        public Application()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Jobs = Config.GetJobs() ?? new List<BackupJob>();

            windows = new Stack<Window>();

            var doubleWindow = new DoubleWindow();
            doubleWindow.Application = this;

            var listWindow = new ListWindow(Jobs);
            listWindow.Application = this;

            if (Jobs.Count > 0)
            {
                var editWindow = new EditWindow(Jobs[0]);
                editWindow.Application = this;
                editWindow.OnSave = () => doubleWindow.ActivePane = Pane.Left;
                editWindow.OnCancel = () => doubleWindow.ActivePane = Pane.Left;
                editWindow.Initialize();

                doubleWindow.RightWindow = editWindow;
                listWindow.OnSelectionChanged = (job) => editWindow.LoadJob(job);
                listWindow.OnEnter = () => doubleWindow.ActivePane = Pane.Right;
            }

            listWindow.Initialize();

            listWindow.OnCreate = () =>
            {
                var createWindow = new CreateConfigWindow();
                createWindow.OnSave = () =>
                {
                    CloseWindow();
                    Initialize();
                };
                OpenWindow(createWindow);
                createWindow.OnCancel = () =>
                {
                    CloseWindow();
                    Initialize();
                };
            };

            listWindow.OnDelete = () =>
            {
                if (Jobs.Count == 0) return;

                var dialogue = new DeletionWarningDialogue();
                dialogue.OnConfirm = () =>
                {
                    int index = listWindow.SelectedComponent;
                    if (index >= 0 && index < Jobs.Count)
                    {
                        Jobs.RemoveAt(index);
                        Config.SaveJobs(Jobs);
                    }
                    Initialize();
                };
                dialogue.OnCancel = () => CloseWindow();
                OpenWindow(dialogue);
            };

            doubleWindow.LeftWindow = listWindow;
            windows.Push(doubleWindow);
        }

        public void Run()
        {
            while (true)
            {
                if (windows.Count == 0)
                    break;

                windows.Peek().Draw();
                this.HandleKey(Console.ReadKey());
            }
        }

        public void HandleKey(ConsoleKeyInfo key)
        {
            windows.Peek().HandleKey(key);
        }

        public void OpenWindow(Window window)
        {
            window.Application = this;
            windows.Push(window);
        }

        public void CloseWindow()
        {
            if (windows.Count > 0)
                windows.Pop();
        }
    }
}
