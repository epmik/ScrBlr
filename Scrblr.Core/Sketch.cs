using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public class Sketch
    {
        /// <summary>
        /// Creates a sketch of type TSketch and calls the Run function. The Sketch is disposed afterwards
        /// </summary>
        /// <typeparam name="TSketch">The type of ISketch to create</typeparam>
        public static void Run<TSketch>() where TSketch : ISketch, new()
        {
            Run(new TSketch(), true);
        }

        /// <summary>
        /// Calls the Run function on the sketch of type TSketch disposed the sketch if <paramref name="dispose"/> is true.
        /// </summary>
        /// <typeparam name="TSketch"></typeparam>
        /// <param name="sketch">The sketch of type TSketch to run</param>
        /// <param name="dispose">Dispose <paramref name="sketch"/>: true or false</param>
        public static void Run<TSketch>(TSketch sketch, bool dispose = true) where TSketch : ISketch
        {
            if(!BindDelegates(sketch))
            {
                if (dispose)
                {
                    sketch.Dispose();
                }

                return;
            }

            Diagnostics.Log($"Calling Run().");

            sketch.Run();

            if (dispose)
            {
                sketch.Dispose();
            }
        }

        private static bool BindDelegates<TSketch>(TSketch sketch) where TSketch : ISketch
        {
            Diagnostics.Log($"Binding delegetes.");

            var sketchType = sketch.GetType();

            var eventInfoArray = sketchType.GetEvents(BindingFlags.Public | BindingFlags.Instance);

            foreach(var eventInfo in eventInfoArray)
            {
                if(!eventInfo.Name.EndsWith("Action"))
                {
                    continue;
                }

                var methodName = eventInfo.Name.Substring(0, eventInfo.Name.Length - 6);

                if(!BindDelegete(sketch, sketchType, eventInfo, methodName) && methodName.Equals("Render"))
                {
                    Diagnostics.Error($"Could not bind the Render delegate. Cannot run and quiting.");

                    return false;
                }
            }

            return true;
        }

        private static bool BindDelegete<TSketch>(TSketch sketch, Type sketchType, EventInfo eventInfo, string methodName) where TSketch : ISketch
        {
            var existingField = sketchType.BaseType.GetField(eventInfo.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (existingField != null)
            {
                var existingDelegate = existingField.GetValue(sketch) as Delegate;

                if (existingDelegate != null)
                {
                    Diagnostics.Log($"Found existing delegete {existingDelegate.Method.Name} for event: {eventInfo.Name}.");

                    return true;
                }
            }

            var methodInfo = sketchType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                Diagnostics.Log($"Could not find method: {methodName} in {sketchType}.");

                return false;
            }

            var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, sketch, methodInfo);

            eventInfo.GetAddMethod().Invoke(sketch, new object[] { d });

            return true;
        }
    }
}
