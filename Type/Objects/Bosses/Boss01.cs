﻿using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Controllers;
using Type.Interfaces;
using Type.Interfaces.Collisions;
using Type.Interfaces.Enemies;

namespace Type.Objects.Bosses
{
    /// <summary>
    /// Boss that has three destroyable cannons
    /// </summary>
    public sealed class Boss01 : GameObject, IEnemy, IEnemyListener
    {
        /// <summary> List of the destroyable cannons on the boss </summary>
        private readonly List<BossCannon> _Cannons;
        /// <summary> Movement speed </summary>
        private readonly Single _Speed;
        /// <summary> List of <see cref="IEnemyListener"/>'s </summary>
        private readonly List<IEnemyListener> _Listeners;
        /// <summary> Move direction </summary>
        private readonly Vector2 _MoveDirection;
        /// <summary> Sprite for the boss base </summary>
        private readonly Sprite _Sprite;

        /// <summary> Whether the boss is moving onto screen </summary>
        private Boolean _IsAdvancing;
        /// <summary> Whether the boss is moving off the screen </summary>
        private Boolean _IsRetreating;
        /// <summary> Where the boss should stop when moving onto screen</summary>
        private Vector2 _StopPosition;
        /// <summary> The players current position </summary>
        private Vector2 _PlayerPosition;

        /// <summary> The position of the object </summary>
        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;

                _Sprite.Position = value;
                foreach (BossCannon cannon in _Cannons)
                {
                    cannon.Position = value;
                }
            }
        }

        /// <summary> The rotation of the object </summary>
        public override Double Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                foreach (BossCannon cannon in _Cannons)
                {
                    cannon.Rotation = value;
                }
            }
        }

        /// <inheritdoc />
        public Boolean AutoFire { get; set; }

        /// <inheritdoc />
        public Vector4 HitBox { get; set; }

        /// <summary> The hitpoints of the <see cref="IHitable"/> </summary>
        public Int32 HitPoints { get; }

        /// <summary> Amount of points this object is worth </summary>
        public Int32 Points { get; }

        public Boss01()
        {
            _Listeners = new List<IEnemyListener>();
            _Cannons = new List<BossCannon>();

            Points = 20000;
            _MoveDirection = new Vector2(-1, 0);

            _Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BOSS_BASE, Texture.GetTexture("Content/Graphics/Bosses/boss01.png"))
            {
                Visible = true,
            };
            Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 + _Sprite.Width / 2, 0);
            _Sprite.Offset = _Sprite.Size / 2;

            BossCannon topMostCannon = new BossCannon(10, TimeSpan.FromMilliseconds(2000)) {Offset = new Vector2(113, -200)};
            BossCannon topCannon = new BossCannon(10, TimeSpan.FromMilliseconds(1500)) {Offset = new Vector2(102, -130)};
            BossCannon middleCannon = new BossCannon(10, TimeSpan.FromMilliseconds(1000)) {Offset = new Vector2(-149, 0)};
            BossCannon bottomCannon = new BossCannon(10, TimeSpan.FromMilliseconds(1500)) {Offset = new Vector2(102, 130)};
            BossCannon bottomMostCannon = new BossCannon(10, TimeSpan.FromMilliseconds(2000)) {Offset = new Vector2(113, 200)};

            _Cannons.Add(topMostCannon);
            _Cannons.Add(topCannon);
            _Cannons.Add(middleCannon);
            _Cannons.Add(bottomCannon);
            _Cannons.Add(bottomMostCannon);

            foreach (BossCannon cannon in _Cannons)
            {
                cannon.RegisterListener(this);
                cannon.Position = Position;
                cannon.Visible = true;
            }

            _Speed = 250f;
            _StopPosition = new Vector2(Renderer.Instance.TargetDimensions.X / 4, 0);
            _IsAdvancing = true;

            PositionRelayer.Instance.AddRecipient(this);
        }

        /// <summary>
        /// Update position data
        /// </summary>
        /// <param name="position"> The received position data </param>
        public void UpdatePositionData(Vector2 position)
        {
            _PlayerPosition = position;
            UpdateRotation();
        }

        /// <summary>
        /// Updates the ship rotation so it is facing the players poistion
        /// </summary>
        private void UpdateRotation()
        {
            foreach (BossCannon cannon in _Cannons)
            {
                cannon.DirectionTowardsPlayer = _PlayerPosition - cannon.Position;
                cannon.Rotation = (Single)Math.Atan2(cannon.DirectionTowardsPlayer.Y, cannon.DirectionTowardsPlayer.X);
            }
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (_IsAdvancing)
            {
                Position += _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X <= _StopPosition.X)
                {
                    _IsAdvancing = false;
                    foreach (BossCannon cannon in _Cannons)
                    {
                        CollisionController.Instance.RegisterEnemy(cannon);
                    }
                }
            }
            if (_IsRetreating)
            {
                Position -= _MoveDirection * _Speed * (Single)timeTilUpdate.TotalSeconds;

                if (Position.X - _Sprite.Width / 2 > Renderer.Instance.TargetDimensions.X / 2 && _IsRetreating)
                {
                    _IsRetreating = false;
                    for (var i = _Listeners.Count - 1; i >= 0; i--)
                    {
                        IEnemyListener listener = _Listeners[i];
                        listener.OnEnemyDestroyed(this);
                    }
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Invoked when an enemy is Destroyed
        /// </summary>
        /// <param name="enemy"> The enemy that has been destroyed </param>
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            _Cannons.Remove(enemy as BossCannon);
            if (_Cannons.Count != 0) return;
            _IsRetreating = true;
        }

        #region Unusued Interfaces

        /// <summary>
        /// Invoked when an enemy leaves the screen
        /// </summary>
        /// <param name="enemy"> The enemy that has left the screen </param>
        public void OnEnemyOffscreen(IEnemy enemy)
        {
        }

        /// <summary>
        /// Hit the <see cref="IHitable"/>
        /// </summary>
        public void Hit(Int32 damage)
        {
        }

        /// <summary>
        /// Destroy the object
        /// </summary>
        public void Destroy()
        {
        }

        /// <summary>
        /// Shoot a projectile
        /// </summary>
        public void Shoot()
        {
        }

        #endregion

        /// <inheritdoc />
        public void RegisterListener(IEnemyListener listener)
        {
            _Listeners.Add(listener);
        }

        /// <inheritdoc />
        public void DeregisterListener(IEnemyListener listener)
        {
            _Listeners.Remove(listener);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _Cannons.Clear();
            _Sprite.Dispose();
            PositionRelayer.Instance.RemoveRecipient(this);
        }
    }
}
