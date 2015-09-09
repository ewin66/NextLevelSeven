﻿using NextLevelSeven.Core;
using NextLevelSeven.Native;
using NextLevelSeven.Routing;

namespace NextLevelSeven.Test.Routing
{
    public class NullRouter : IRouter
    {
        public NullRouter(bool success)
        {
            Success = success;
        }

        public bool Route(INativeMessage message)
        {
            LastMessage = message;
            Routed = Routed || Success;
            Checked = true;
            return Success;
        }

        public bool Checked { get; private set; }

        public INativeMessage LastMessage { get; private set; }

        public bool Routed { get; private set; }

        public readonly bool Success;
    }
}
