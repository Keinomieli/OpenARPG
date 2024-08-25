using Godot;
using Env = System.Environment;
using System.IO;

namespace OpenARPG.Tools
{
    public partial class ScreenshotTaker : Node
    {
        [Export] private Key key = Key.None;

        public override void _Input(InputEvent @event)  
        {
            if (@event is InputEventKey keyEvent)
            {
                if (@event.IsPressed() && keyEvent.Keycode == key) 
                {
                    TakeScreenshot();
                }
            }
        }

        private void TakeScreenshot()
        {
            Image capture = GetViewport().GetTexture().GetImage();

            int index = 1;
            string filename() => $"{Env.GetFolderPath(Env.SpecialFolder.Desktop)}/Screenshot{index}.png";

            while (File.Exists(filename()))
                index++;

            capture.SavePng(filename());

            Logger.Log($"Sceenshot{index}.png saved");
        }
    }
}