using Scrblr.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scrblr.Console
{
    [Sketch(Name = "Trial Sketch")]
    public class TrialSketch : AbstractSketch20220317
    {
        public TrialSketch()
            : base(3, 3)
        {
            LoadAction += Load;
            UnLoadAction += UnLoad;
            RenderAction += Render;
            UpdateAction += Update;
        }

        public void Load()
        {
            ClearColor(1f, 1f, 1f, 1f);
            ClearColor(1f, 1f, 1f);
            ClearColor(255, 255, 255, 255);
            ClearColor(255, 255, 255);
        }

        public void UnLoad()
        {
            
        }

        public void Render()
        {
            Clear(ClearFlag.Color);

            PushTransform();
           
            Rectangle()
                .Position(0, 0)
                .Size(1, 1)
                .Translate(10f, 10f)
                .Fill(255, 0, 0);

            PopTransform();
        }

        public void Update()
        {
            
        }
    }
}
