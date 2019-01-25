﻿using AmosShared.Interfaces;
using System;
using Type.Interfaces.Collisions;
using Type.Interfaces.Player;

namespace Type.Interfaces.Powerups
{
    /// <summary>
    /// Powerup that can collected
    /// </summary>
    public interface IPowerup : ICollidable, IUpdatable
    {
        /// <summary>
        /// The ID of the powerup type
        /// </summary>
        Int32 ID { get; }

        /// <summary>
        /// Apply the powerup to the player
        /// </summary>
        /// <param name="player"> The player ship</param>
        void Apply(IPlayer player);
    }
}
