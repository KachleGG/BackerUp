using BackerUp.Editor.UI.Components;
using BackerUp.Editor.UI.Windows;

namespace BackerUp.Editor.UI.Dialogues
{
    public class DeletionWarningDialogue : Window, IDialogue
    {
        public Action OnConfirm { get; set; }
        public Action OnCancel { get; set; }

        private const string Message = "Are you sure you want to delete?";
        private const int DialogWidth = 40;
        private const int DialogHeight = 7;

        public DeletionWarningDialogue()
        {
            DrawsBorder = false;
            SelectedComponent = 1;

            Components.Add(new Button { Text = "OK", OnPress = () => OnConfirm?.Invoke() });
            Components.Add(new Button { Text = "Cancel", OnPress = () => OnCancel?.Invoke() });

            Pairs = new Dictionary<ConsoleKey, Action>
            {
                { ConsoleKey.LeftArrow, () => SelectedComponent = Math.Max(0, SelectedComponent - 1) },
                { ConsoleKey.RightArrow, () => SelectedComponent = Math.Min(Components.Count - 1, SelectedComponent + 1) },
                { ConsoleKey.Escape, () => OnCancel?.Invoke() }
            };
        }

        public override void Draw()
        {
            int startX = (Console.WindowWidth - DialogWidth) / 2;
            int startY = (Console.WindowHeight - DialogHeight) / 2;

            Console.SetCursorPosition(startX, startY);
            Console.Write("┌" + "─".PadRight(DialogWidth - 2) + "┐");

            for (int i = 1; i < DialogHeight - 1; i++)
            {
                Console.SetCursorPosition(startX, startY + i);
                Console.Write("│" + " ".PadRight(DialogWidth - 2) + "│");
            }

            Console.SetCursorPosition(startX, startY + DialogHeight - 1);
            Console.Write("└" + "─".PadRight(DialogWidth - 2) + "┘");

            int msgX = startX + (DialogWidth - Message.Length) / 2;
            Console.SetCursorPosition(msgX, startY + 2);
            Console.Write(Message);

            string okText = "[ OK ]";
            string cancelText = "[ Cancel ]";
            int buttonsWidth = okText.Length + 4 + cancelText.Length;
            int btnStartX = startX + (DialogWidth - buttonsWidth) / 2;
            int btnY = startY + 4;

            Console.SetCursorPosition(btnStartX, btnY);
            if (SelectedComponent == 0)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write(okText);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(btnStartX + okText.Length + 4, btnY);
            if (SelectedComponent == 1)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write(cancelText);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            if (Pairs.TryGetValue(key.Key, out var action))
            {
                action.Invoke();
                return;
            }

            if (key.Key == ConsoleKey.Enter && SelectedComponent >= 0 && SelectedComponent < Components.Count)
            {
                if (Components[SelectedComponent] is Button button)
                    button.OnPress?.Invoke();
            }
        }
    }
}
