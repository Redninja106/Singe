using ImGuiNET;
using Singe.Debugging;
using Singe.Debugging.Windows;
using Singe.Messaging;
using Singe.Platforms;
using Singe.Rendering;
using Commander;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Singe
{
    public abstract class Application
    {
        public Renderer Renderer { get; private set; }
        public WindowManager WindowManager { get; private set; }
        public IRenderingOutput Output { get; private set; }
        public Dispatcher Dispatcher { get; private set; }
        public bool IsRunning { get; private set; } = false;
        public Scene Scene { get; private set; }

        public Application()
        {

        }

        internal Scene Run()
        {
            Renderer = Renderer.Create(GraphicsApi.Direct3D11);

            WindowManager = WindowManager.Create();
            
            var factoryApis = WindowManager.GetSupportedApis();

            if (!factoryApis.Contains(Renderer.Api))
                throw new Exception("Rendering output factory doesnt not support this api!");

            Output = WindowManager.CreateOutput(Renderer);

            Renderer.SetRenderingOutput(Output);

            Input.SetDevice(WindowManager.CreateInputDevice());

            Dispatcher = new Dispatcher();

            Dispatcher.BroadcastMessage(MessageType.Init, null);

            GuiRenderer.Initialize(Renderer);

            IsRunning = true;

            // init everything
            while (IsRunning)
            {
                // this is everything to do:
                // - update window
                // - update time
                // - update input
                // - update gui input
                // - draw
                // - update
                // - physics update
                // - post update
                // - draw gui
                // - present

                Time.Update();
                Input.Update();
                WindowManager.HandleEvents();

                // update input
                
                Gui.Update();

                Gui.Begin();

                Input.CallKeyCommandBindings();

                // render game

                Scene.Render();

                Renderer.SetRenderTarget(Output.GetRenderTarget());
                Renderer.Clear(Color.FromKnownColor(KnownColor.CornflowerBlue));

                Dispatcher.BroadcastMessage(MessageType.OnGui, null);

                // update game
                Scene.Update();

                Gui.End();
                GuiRenderer.Render();


                Output.Present(1);
            }

            Exit(0);
        }
        static Application()
        {
            Service.RegisterAssembly(typeof(Application).Assembly);
        }
        public static void Exit(int code)
        {
            // dispose everything
            GuiRenderer.Uninitialize();
            Renderer.Dispose();
            WindowManager.Dispose();
            Output.Dispose();
            Dispatcher.BroadcastMessage(MessageType.Destroy, null);
            Environment.Exit(code);
        }
    }
}