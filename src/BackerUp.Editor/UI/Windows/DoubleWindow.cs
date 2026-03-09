using BackerUp.Editor.UI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Windows
{

    public class DoubleWindow : Window
    {
        public Window LeftWindow { get; set; }
        public Window RightWindow { get; set; }
        public Pane ActivePane { get; set; } = Pane.Left;

        public int SplitX => Console.WindowWidth / 2;

        public override void Draw()
        {
            DrawSplitBorder();

            if (LeftWindow != null)
            {
                LeftWindow.Application = Application;
                LeftWindow.IsActive = ActivePane == Pane.Left;
                LeftWindow.DrawsBorder = false;
                LeftWindow.OffsetX = 0;
                LeftWindow.OffsetY = 0;
                LeftWindow.Draw();
            }

            if (RightWindow != null)
            {
                RightWindow.Application = Application;
                RightWindow.IsActive = ActivePane == Pane.Right;
                RightWindow.DrawsBorder = false;
                RightWindow.OffsetX = SplitX;
                RightWindow.OffsetY = 0;
                RightWindow.Draw();
            }
        }

        private void DrawSplitBorder()
        {
            int w = Console.WindowWidth;
            int h = Console.WindowHeight;
            int split = SplitX;

            Console.SetCursorPosition(0, 0);
            Console.Write("┌");
            for (int i = 1; i < w - 1; i++)
                Console.Write(i == split ? "╥" : "─");
            Console.Write("┐");

            for (int row = 1; row < h - 1; row++)
            {
                Console.SetCursorPosition(0, row);
                Console.Write("│");
                for (int col = 1; col < w - 1; col++)
                    Console.Write(col == split ? "║" : " ");
                Console.Write("│");
            }

            Console.SetCursorPosition(0, h - 1);
            Console.Write("└");
            for (int i = 1; i < w - 1; i++)
                Console.Write(i == split ? "╨" : "─");
            Console.Write("┘");
        }

        public override void HandleKey(ConsoleKeyInfo key)
        {
            var active = ActivePane == Pane.Left ? LeftWindow : RightWindow;
            if (active != null)
            {
                active.Application = Application;
                active.HandleKey(key);
            }
        }

        public void SetRightWindow(Window window)
        {
            window.Application = this.Application;
            RightWindow = window;
        }
    }
}
