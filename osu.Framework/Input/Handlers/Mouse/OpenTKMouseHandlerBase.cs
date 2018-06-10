﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Drawing;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Input;

namespace osu.Framework.Input.Handlers.Mouse
{
    internal class OpenTKMouseHandlerBase : InputHandler
    {
        protected GameHost Host;
        protected bool MouseInWindow;

        public override bool Initialize(GameHost host)
        {
            this.Host = host;

            MouseInWindow = host.Window.CursorInWindow;
            Host.Window.MouseLeave += (s, e) => MouseInWindow = false;
            Host.Window.MouseEnter += (s, e) => MouseInWindow = true;

            return true;
        }

        Vector2 currentPosition;
        protected void HandleState(OpenTKMouseState state)
        {
            if (!state.HasLastPosition || (state.RawState.Flags & OpenTK.Input.MouseStateFlags.MoveAbsolute) > 0)
            {
                PendingInputs.Enqueue(new MousePositionAbsoluteInput { Position = state.Position });
                currentPosition = state.Position;
            }
            else
            {
                PendingInputs.Enqueue(new MousePositionRelativeInput { Delta = state.Delta });
                currentPosition += state.Delta;
            }
            PendingInputs.Enqueue(new MouseScrollRelativeInput { Delta = state.ScrollDelta });
            for (var i = 0; i <= (int)MouseButton.LastButton; ++ i)
                PendingInputs.Enqueue(new MouseButtonInput { Button = (MouseButton)i, IsPressed = state.IsPressed((MouseButton)i) });
            FrameStatistics.Increment(StatisticsCounterType.MouseEvents);
            
            // update the windows cursor to match our raw cursor position.
            // this is important when sensitivity is decreased below 1.0, where we need to ensure the cursor stays within the window.
            var screenPoint = Host.Window.PointToScreen(new Point((int)currentPosition.X, (int)currentPosition.Y));
            OpenTK.Input.Mouse.SetPosition(screenPoint.X, screenPoint.Y);
        }

        /// <summary>
        /// This input handler is always active, handling the cursor position if no other input handler does.
        /// </summary>
        public override bool IsActive => true;

        /// <summary>
        /// Lowest priority. We want the normal mouse handler to only kick in if all other handlers don't do anything.
        /// </summary>
        public override int Priority => 0;
    }
}

