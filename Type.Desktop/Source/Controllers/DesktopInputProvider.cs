﻿using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using Type.Buttons;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.Desktop.Source.Controllers
{
    public class DesktopInputProvider : IInputProvider, INotifier<IInputListener>, IUpdatable
    {
        private readonly List<IInputListener> _Listeners = new List<IInputListener>();

        private Vector2 _Velocity;

        private Single _VelocityMagnitude;

        private Single _PositiveDeadZone = 0.2f;

        private Single _NegativeDeadZone = -0.2f;

        private Boolean _NukePressed;

        public DesktopInputProvider()
        {
            UpdateManager.Instance.AddUpdatable(this);
        }

        #region Implementation of IUpdatable

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            // Analog input
            if (OpenTK.Input.GamePad.GetState(0).ThumbSticks.Left.Y > _PositiveDeadZone ||
                OpenTK.Input.GamePad.GetState(0).ThumbSticks.Left.Y < _NegativeDeadZone ||
                OpenTK.Input.GamePad.GetState(0).ThumbSticks.Left.X > _PositiveDeadZone ||
                OpenTK.Input.GamePad.GetState(0).ThumbSticks.Left.X < _NegativeDeadZone)
            {
                _Velocity = GamePad.GetState(0).ThumbSticks.Left;
                _VelocityMagnitude = GamePad.GetState(0).ThumbSticks.Left.Length;
            }
            else
            {
                _Velocity = Vector2.Zero;
            }

            if (OpenTK.Input.GamePad.GetState(0).Buttons.A == ButtonState.Pressed)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.FireButtonPressed();
                }
            }
            else if (OpenTK.Input.GamePad.GetState(0).Buttons.A == ButtonState.Released)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.FireButtonReleased();
                }
            }
            if (OpenTK.Input.GamePad.GetState(0).Buttons.B == ButtonState.Pressed && !_NukePressed)
            {
                foreach (IInputListener listener in _Listeners)
                {
                    listener.OnNukeButtonPressed();
                }

                _NukePressed = true;
            }
            if (OpenTK.Input.GamePad.GetState(0).Buttons.B == ButtonState.Released)
            {
                _NukePressed = false;
            }

            foreach (IInputListener listener in _Listeners)
            {
                listener.UpdateDirectionData(_Velocity, _VelocityMagnitude);
            }
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        #endregion


        #region Implementation of INotifier<IInputListener>

        /// <summary> Virtual analog stick </summary>
        public VirtualAnalogStick VirtualAnalogStick { get; set; }

        /// <summary> Virtual nuke button </summary>
        public NukeButton NukeButton { get; set; }

        /// <summary> Virtual firebutton </summary>
        public FireButton FireButton { get; set; }

        /// <summary> Virtual pausebutton </summary>
        public PauseButton PauseButton { get; set; }

        /// <summary> Virtual resumebutton </summary>
        public ResumeButton ResumeButton { get; set; }

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(IInputListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(IInputListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion


        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            UpdateManager.Instance.RemoveUpdatable(this);
        }

        #endregion
    }
}
