﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scrblr.Core
{
    public interface IEventComponent : IComponent
    {
        double ElapsedTime { get; set; }

        KeyboardState KeyboardState { get; set; }

        MouseState MouseState { get; set; }

        //event Action LoadAction;
        //event Action UnLoadAction;
        //event Action<FrameEventArgs> UpdateAction;
        //event Action<ResizeEventArgs> ResizeAction;
        //event Action<MouseWheelEventArgs> MouseWheelAction;
        //event Action<MouseButtonEventArgs> MouseUpAction;
        //event Action<MouseButtonEventArgs> MouseDownAction;
        //event Action<MouseMoveEventArgs> MouseMoveAction;
        //event Action MouseLeaveAction;
        //event Action MouseEnterAction;
        //event Action<KeyboardKeyEventArgs> KeyDownAction;
        //event Action<KeyboardKeyEventArgs> KeyUpAction;

        void Update(FrameEventArgs a);

        void Resize(ResizeEventArgs a);

        void KeyUp(KeyboardKeyEventArgs a);

        void KeyDown(KeyboardKeyEventArgs a);

        void MouseDown(MouseButtonEventArgs a);

        void MouseUp(MouseButtonEventArgs a);

        void MouseEnter();

        void MouseLeave();

        void MouseMove(MouseMoveEventArgs a);

        void MouseWheel(MouseWheelEventArgs a);
    }
}
