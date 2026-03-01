using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Editor.UI.Components;

public interface IComponent
{
    public string Text { get; }

    public Dictionary<ConsoleKeyInfo, Action> Pairs { get; }

    public void Draw();
    public void HandleKey(ConsoleKeyInfo key);
}
