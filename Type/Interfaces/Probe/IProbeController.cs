﻿using AmosShared.Interfaces;
using OpenTK;
using System;

namespace Type.Interfaces.Probe
{
    /// <summary>
    /// Creates and controls <see cref="IProbe"/>'s
    /// </summary>
    public interface IProbeController : IUpdatable
    {
        /// <summary>
        /// Whether the probes should be shooting
        /// </summary>
        Boolean Shoot { get; set; }

        /// <summary>
        /// Current probe count
        /// </summary>
        Int32 CurrentProbes { get; }

        /// <summary>
        /// Whether probes are at maximum
        /// </summary>
        Boolean WeaponsAtMax { get; }

        /// <summary>
        /// Add a probe to the controller
        /// </summary>
        /// <param name="id"> The id of the probe to add </param>
        void AddProbe(Int32 id);

        /// <summary>
        /// Remove all probes from the controller
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Provide position data to the controller
        /// </summary>
        /// <param name="position"> The position to provide </param>
        /// <param name="deltaTime"></param>
        void UpdatePosition(Vector2 position, Single deltaTime);
    }
}
