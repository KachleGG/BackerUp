using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackerUp_Editor.UI
{
    internal class MainMenu
    {
        public static List<NavHelper> NavHelpers = new List<NavHelper>()
        {
            new NavHelper() { Key = "Enter", Description = "Select option" },
            new NavHelper() { Key = "Arrows", Description = "Navigate" },
            new NavHelper() { Key = "Delete", Description = "Delete selected job" },
        };

        public static void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.Title = "BackerUp Editor";
            Console.ResetColor();
        }

        public static void Show()
        {
            Initialize();
            while (true)
            {
                DrawTable(helperRows: 1);
                CreateHelper();
            }
        }

        private static void DrawTable(int helperRows = 1)
        {
            int width = Math.Max(Console.WindowWidth, 1);
            int height = Math.Max(Console.WindowHeight, 1);

            int innerLines = Math.Max(0, height - helperRows - 2); // -2 for top and bottom border

            // Top border with corners
            string topBorder;
            if (width >= 2)
            {
                topBorder = "+" + new string('-', width - 2) + "+";
            }
            else
            {
                topBorder = "+";
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(topBorder);

            // Middle (sides + inner space)
            string middleLine;
            if (width >= 2)
            {
                middleLine = "|" + new string(' ', width - 2) + "|";
            }
            else
            {
                middleLine = "|";
            }

            for (int i = 0; i < innerLines; i++)
            {
                Console.WriteLine(middleLine);
            }

            // Bottom border with corners (placed just above helperRows)
            string bottomBorder;
            if (width >= 2)
            {
                bottomBorder = "+" + new string('-', width - 2) + "+";
            }
            else
            {
                bottomBorder = "+";
            }

            Console.WriteLine(bottomBorder);
        }

        public static void CreateHelper()
        {
            // Build helper text
            string helperText = string.Join("   ", NavHelpers.Select(h => $"{h.Key} - {h.Description}"));

            int row = Math.Max(0, Console.WindowHeight - 1);
            int maxWidth = Math.Max(1, Console.WindowWidth - 1);

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            try
            {
                Console.SetCursorPosition(0, row);
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            Console.Write(helperText.PadRight(maxWidth));

            Console.ResetColor();
        }
    }

    public class NavHelper
    {
        public string Key { get; set; }
        public string Description { get; set; }
    }
}
