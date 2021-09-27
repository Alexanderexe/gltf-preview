using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace prvw
{
    class Program
    {
        static void Main(string[] args)
        {




            var nativeWindowSettings = new NativeWindowSettings()
            {
                //StartVisible = false,
                Size = new Vector2i(800, 600),
                Title = "LearnOpenTK - Textures",
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                
                //window.IsVisible = false;
                window.Run();
            }


        }
    }
}
