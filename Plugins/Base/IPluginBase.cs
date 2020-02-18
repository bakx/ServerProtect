﻿using System.Threading.Tasks;
using SP.Models;

namespace SP.Plugins
{
    public interface IPluginBase
    {
        Task<bool> Initialize(PluginOptions options);
        Task<bool> Configure();

        Task<bool> RegisterLoginAttemptHandler(LoginAttempt eventHandler);
        Task<bool> RegisterBlockHandler(Block eventHandler);

        // Delegates for events
        public delegate void LoginAttempt(LoginAttempts loginAttempt);
        public delegate void Block(LoginAttempts loginAttempt);

        // Invoked Event methods
        Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt);
        Task<bool> BlockEvent(Blocks block);
    }
}