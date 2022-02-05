using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;

namespace QuickPrototype
{
   class Program
   {
      static void Main()
      {
         Game.Create(80, 60, window: new(new(800, 600), "QuickPrototype"));
         Game.Instance.OnStart = Init;
         Game.Instance.Run();
         Game.Instance.Dispose();
      }
      static void Init()
      {
         var window = new Window(20, 10);
         var listBox = new ListBox(18, 8);

         window.Show();
         window.Controls.Add(listBox);
         listBox.Position = new(1, 1);
			for (int i = 0; i < 20; i++)
			{
            listBox.Items.Add(i);
            listBox.SelectedIndex = listBox.Items.Count - 1;
            listBox.ScrollToSelectedItem();
			}
      }
   }
}
