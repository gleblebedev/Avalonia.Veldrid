using System;
using System.Numerics;
using System.Windows.Input;

namespace Avalonia.Veldrid.Sample
{
    public class MainViewModel
    {
        private readonly Random rnd = new Random();

        public MainViewModel()
        {
            TestCommand = new ActionCommand(Test);
        }

        public ICommand TestCommand { get; set; }

        private void Test()
        {
            var w = new PopupWindow();
            var pos = new Vector3(((float) rnd.NextDouble() - 0.5f) * 5, ((float) rnd.NextDouble() - 0.5f) * 5,
                ((float) rnd.NextDouble() - 0.5f) * 5);
            WorldTransformProperty.SetValue(w, Matrix4x4.CreateTranslation(pos));
            w.Show();
        }
    }
}