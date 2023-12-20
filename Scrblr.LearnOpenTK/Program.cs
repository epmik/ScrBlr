using System.Reflection;

namespace Scrblr.LearnOpenTK
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAndDisplaySketches();

            var input = string.Empty;

            while (true)
            {
                var keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.Q)
                {
                    break;
                }

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (!int.TryParse(input, out int index))
                    {
                        index = 0;
                    }

                    // index = 1 based, offset by -1
                    RunAndDisplaySketches(--index);

                    input = string.Empty;
                }

                if (Char.IsNumber(keyInfo.KeyChar))
                {
                    input += keyInfo.KeyChar;
                }
            }

            Console.WriteLine("Closing in 1 second...");

            Thread.Sleep(1000);
        }

        private static void RunAndDisplaySketches(int index = -1)
        {
            var types = EnumerateAbstractSketchImplementationTypes().ToArray();

            if (index < 0 || index >= types.Length)
            {
                index = types.Length - 1;
            }

            Console.Clear();

            if (types[index].IsSubclassOf(typeof(OpenTK.Windowing.Desktop.GameWindow)))
            {
                var nativeWindowSettings = new OpenTK.Windowing.Desktop.NativeWindowSettings()
                {
                    ClientSize = new OpenTK.Mathematics.Vector2i(800, 600),
                    Title = "LearnOpenTK - Creating a Window",
                    // This is needed to run on macos
                    Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible,
                };

                using (var window = (OpenTK.Windowing.Desktop.GameWindow)Activator.CreateInstance(types[index], OpenTK.Windowing.Desktop.GameWindowSettings.Default, nativeWindowSettings))
                {
                    window.Run();
                }
            }
            else
            {
                using (var window = new Learn003Silk())
                {
                    window.Run();
                }
            }

            Console.Clear();

            var count = 1;
            foreach (var t in types)
            {
                Console.WriteLine($"{count++} - {t.Name}");
            }

            Console.WriteLine("Press 'Q' or 'Esc' to quit or start a sketch");
            Console.WriteLine("by typing a number and pressing 'ENTER'");
        }

        private static List<Type>? _abstractSketchImplementationTypeList = null;

        private static IEnumerable<Type> EnumerateAbstractSketchImplementationTypes()
        {
            if (_abstractSketchImplementationTypeList == null)
            {
                _abstractSketchImplementationTypeList = new List<Type>();

                foreach (Type type in
                    Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && (t.IsSubclassOf(typeof(OpenTK.Windowing.Desktop.GameWindow))) || t.IsSubclassOf(typeof(SilkSketch))))
                {
                    _abstractSketchImplementationTypeList.Add(type);
                }
            }

            return _abstractSketchImplementationTypeList;
        }
    }
}
