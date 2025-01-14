﻿using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;
using System;

namespace Type.Base
{
    public abstract class GameObject : IUpdatable, IPositionable, IRotatable
    {
        /// <summary> Sprite for the game object </summary>
        protected Sprite _Sprite;
        /// <summary> Position of the object </summary>
        protected Vector2 _Position;
        /// <summary> Rotation of the object </summary>
        protected Double _Rotation;
        /// <summary> White pixels drawn over the collidible area of the object </summary>
        private Sprite _CollideArea;

        /// <summary> Whether the object is </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the object is updated </summary>
        public Boolean UpdateEnabled { get; protected set; }

        protected GameObject(Boolean updateEnabled = true)
        {
            UpdateEnabled = updateEnabled;
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary> Position of the object </summary>
        public virtual Vector2 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                if (_Sprite != null) _Sprite.Position = _Position;
                if (_CollideArea != null) _CollideArea.Position = _Position;
            }
        }

        public virtual Double Rotation
        {
            get => _Rotation;
            set
            {
                _Rotation = value;
                if (_Sprite != null) _Sprite.Rotation = _Rotation;
                if (_CollideArea != null) _CollideArea.Rotation = _Rotation;
            }
        }

        /// <summary>
        /// Returns the sprite for this object
        /// </summary>
        public Sprite GetSprite()
        {
            return _Sprite;
        }

        /// <summary>
        /// Returns the rectangle of this game object
        /// </summary>
        public Vector4 GetRect()
        {
            return new Vector4(_Sprite.Position.X - _Sprite.Offset.X * _Sprite.Scale.X, _Sprite.Position.Y - _Sprite.Offset.Y * _Sprite.Scale.Y,
                _Sprite.Width * _Sprite.Scale.X, _Sprite.Height * _Sprite.Scale.Y);
        }

        /// <summary>
        /// Whether the object will be updated
        /// </summary>
        public Boolean CanUpdate()
        {
            return UpdateEnabled;
        }

        /// <summary>
        /// Updates the sprites position with this game object
        /// </summary>
        protected void AddSprite(Sprite sprite)
        {
            _Sprite = sprite;
            _Sprite.Position = Position;

            if (Constants.Global.SHOW_SPRITE_AREAS)
            {
                _CollideArea = new Sprite(Game.MainCanvas, Int32.MaxValue, Texture.GetPixel())
                {
                    Colour = new Vector4(255, 255, 255, 255),
                    Position = _Sprite.Position,
                    Offset = _Sprite.Offset,
                    RotationOrigin = _Sprite.RotationOrigin,
                    Rotation = _Sprite.Rotation,
                    Scale = new Vector2(_Sprite.Width, _Sprite.Height),
                    Visible = true,
                };
            }
        }

        public virtual void Update(TimeSpan timeTilUpdate)
        {

        }

        public virtual void Dispose()
        {
            _Sprite?.Dispose();
            _CollideArea?.Dispose();
            _Sprite = null;
            UpdateEnabled = false;
            UpdateManager.Instance.RemoveUpdatable(this);
            IsDisposed = true;
        }
    }
}
