﻿using System;
using OpenTK.Platform.iPhoneOS;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Framework.Platform.iOS
{
    public class iOSGameWindow : GameWindow
    {
        public iOSGameWindow(iOSPlatformGameView gameView)
            : base(new iOSPlatformGameWindow(gameView))
        {
        }

        public override void SetupWindow(FrameworkConfigManager config)
        {
            //throw new NotImplementedException();
        }
    }
}
