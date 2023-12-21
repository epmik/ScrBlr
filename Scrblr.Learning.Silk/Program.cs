using Scrblr.Core;
using System.Reflection;

namespace Scrblr.Learning
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


            if(types[index].IsSubclassOf(typeof(SilkSketch)))
            {
                using (var window = (SilkSketch)Activator.CreateInstance(types[index]))
                {
                    window.Run();
                }
            }
            else
            {
                using (var window = (ISketch)Activator.CreateInstance(types[index]))
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
                    .Where(t => t.IsClass && !t.IsAbstract && (t.IsSubclassOf(typeof(SilkSketch)) || t.IsSubclassOf(typeof(AbstractSketch)))))
                {
                    _abstractSketchImplementationTypeList.Add(type);
                }
            }

            return _abstractSketchImplementationTypeList;
        }
    }
}
