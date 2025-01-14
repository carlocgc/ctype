﻿using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Interfaces.Weapons;

namespace Type.Controllers
{
    /// <summary>
    /// Checks if registered objects have collided and handles those collisions
    /// </summary>
    public class CollisionController : IUpdatable
    {
        /// <summary> The instance of the Collision Controller </summary>
        private static CollisionController _Instance;
        /// <summary> The instance of the Collision Controller </summary>
        public static CollisionController Instance => _Instance ?? (_Instance = new CollisionController());

        /// <summary> List of all active player projectiles </summary>
        private readonly List<IProjectile> _PlayerProjectiles;
        /// <summary> List of all active enemy projectiles </summary>
        private readonly List<IProjectile> _EnemyProjectiles;
        /// <summary> List of all the uncollected powerups </summary>
        private readonly List<IPowerup> _Powerups;
        /// <summary> The players ship </summary>
        private IPlayer _Player;

        /// <summary> List of all active enemies </summary>
        public List<IEnemy> Enemies { get; private set; }
        /// <summary> Whether the collision controller is disposed </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the collision controller is active </summary>
        public Boolean IsActive { get; set; }

        private CollisionController()
        {
            _PlayerProjectiles = new List<IProjectile>();
            _EnemyProjectiles = new List<IProjectile>();
            Enemies = new List<IEnemy>();
            _Powerups = new List<IPowerup>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        #region Register

        /// <summary>
        /// Adds enemy to the collision list
        /// </summary>
        /// <param name="enemy"></param>
        public void RegisterEnemy(IEnemy enemy)
        {
            Enemies.Add(enemy);
        }

        /// <summary>
        /// Adds enemy projectile to the enemy projectile list
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterEnemyProjectile(IProjectile projectile)
        {
            _EnemyProjectiles.Add(projectile);
        }

        /// <summary>
        /// Adds player projectile to the player projectile list
        /// </summary>
        /// <param name="projectile"></param>
        public void RegisterPlayerProjectile(IProjectile projectile)
        {
            _PlayerProjectiles.Add(projectile);
        }

        /// <summary>
        /// Adds a powerup to the controller
        /// </summary>
        /// <param name="powerup"></param>
        public void RegisterPowerup(IPowerup powerup)
        {
            _Powerups.Add(powerup);
        }

        /// <summary>
        /// Registers the player object to the collision controller
        /// </summary>
        /// <param name="player"></param>
        public void RegisterPlayer(IPlayer player)
        {
            _Player = player;
        }

        #endregion

        #region Checking

        /// <summary>
        /// Performs Intersection checks for each group of objects that will collide with each other and handles those collisions
        /// </summary>
        private void CheckCollisions()
        {
            CheckProjectilesToEnemies();
            CheckProjectilesToPlayer();
            CheckPlayerToEnemies();
            CheckPowerupsToPlayer();
        }

        /// <summary>
        /// Checks for enemy projectiles hitting the player
        /// </summary>
        private void CheckProjectilesToPlayer()
        {
            foreach (IProjectile projectile in _EnemyProjectiles.Where(p => !p.IsDisposed).ToList())
            {
                if (Intersects(projectile.HitBox, _Player.HitBox))
                {
                    HandlePlayerHit(projectile);
                }
            }
        }

        /// <summary>
        /// Checks for player projectiles hitting enemies
        /// </summary>
        private void CheckProjectilesToEnemies()
        {
            foreach (IProjectile projectile in _PlayerProjectiles.Where(p => !p.IsDisposed).ToList())
            {
                foreach (IEnemy enemy in Enemies.ToList())
                {
                    if (Intersects(projectile.HitBox, enemy.HitBox))
                    {
                        HandleEnemyHit(projectile, enemy);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the player ship is colliding with an enemy ship
        /// </summary>
        private void CheckPlayerToEnemies()
        {
            if (_Player == null) return;

            foreach (IEnemy enemy in Enemies.Where(p => !p.IsDisposed).ToList())
            {
                if (Intersects(_Player.HitBox, enemy.HitBox))
                {
                    HandleEnemyPlayerCollision(enemy);
                }
            }
        }

        /// <summary>
        /// Check collision between player and powerups
        /// </summary>
        private void CheckPowerupsToPlayer()
        {
            if (_Player == null) return;

            foreach (IPowerup powerup in _Powerups.Where(p => !p.IsDisposed).ToList())
            {
                if (Intersects(_Player.HitBox, powerup.HitBox))
                {
                    HandlePowerupCollision(powerup);
                }
            }

        }

        /// <summary>
        /// returns true if two rectangles intersects
        /// </summary>
        private Boolean Intersects(Vector4 rectA, Vector4 rectB)
        {
            return rectA.X < rectB.X + rectB.Z &&
                   rectA.X + rectA.Z > rectB.X &&
                   rectA.Y < rectB.Y + rectB.W &&
                   rectA.W + rectA.Y > rectB.Y;
        }

        #endregion

        #region Handling

        /// <summary>
        /// Handles collisions with the player ship
        /// </summary>
        private void HandlePlayerHit(IProjectile projectile = null)
        {
            Int32 damage = projectile?.Damage ?? 1;
            _Player.Hit(damage);

            if (projectile == null) return;
            _EnemyProjectiles.Remove(projectile);
            projectile.Destroy();
        }

        /// <summary>
        /// Handles collisions between player projectiles and enemies
        /// </summary>
        private void HandleEnemyHit(IProjectile projectile, IEnemy enemy)
        {
            _PlayerProjectiles.Remove(projectile);
            enemy.Hit(projectile.Damage);
            projectile?.Destroy();
        }

        /// <summary>
        /// Handles enemy collisions with the player
        /// </summary>
        /// <param name="enemy"></param>
        private void HandleEnemyPlayerCollision(IEnemy enemy)
        {
            HandlePlayerHit();
            if (enemy.IsDisposed) return;
            if (enemy.CanBeRoadKilled) enemy.Destroy();
        }

        /// <summary>
        /// Handles collecting of powerups
        /// </summary>
        /// <param name="powerup"></param>
        private void HandlePowerupCollision(IPowerup powerup)
        {
            _Player.ApplyPowerup(powerup);
            powerup.Dispose();
        }

        #endregion

        #region CleanUp

        /// <summary>
        /// Clears all lists and deactivates the controller
        /// </summary>
        public void ClearObjects()
        {
            foreach (IEnemy enemy in Enemies.ToList())
            {
                enemy.Dispose();
            }
            Enemies.Clear();
            ClearProjectiles();
        }

        /// <summary>
        /// Disposes all alive projectiles
        /// </summary>
        public void ClearProjectiles()
        {
            foreach (IProjectile projectile in _EnemyProjectiles.ToList())
            {
                projectile.Dispose();
            }
            _EnemyProjectiles.Clear();
            foreach (IProjectile projectile in _PlayerProjectiles.ToList())
            {
                projectile.Dispose();
            }
            _PlayerProjectiles.Clear();
        }

        /// <summary>
        /// Removes the given enemy from the enenmy list
        /// </summary>
        /// <param name="enemy"></param>
        public void DeregisterEnemy(IEnemy enemy)
        {
            if (Enemies.Contains(enemy)) Enemies.Remove(enemy);
        }

        /// <summary>
        /// Removes the given projectile from the collision controller
        /// </summary>
        /// <param name="projectile"></param>
        public void DeregisterProjectile(IProjectile projectile)
        {
            if (_EnemyProjectiles.Contains(projectile))
            {
                _EnemyProjectiles.Remove(projectile);
            }
            else if (_PlayerProjectiles.Contains(projectile))
            {
                _PlayerProjectiles.Remove(projectile);
            }
        }

        /// <summary>
        /// Removes the given powerup from the controller
        /// </summary>
        /// <param name="powerup"></param>
        public void DeregisterPowerup(IPowerup powerup)
        {
            if (_Powerups.Contains(powerup)) _Powerups.Remove(powerup);
        }

        #endregion

        /// <summary>
        /// Returns whether the controller can update
        /// </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        public void Update(TimeSpan timeTilUpdate)
        {
            if (IsActive)
            {
                CheckCollisions();
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            UpdateManager.Instance.RemoveUpdatable(this);
            ClearProjectiles();
            foreach (IEnemy enemy in Enemies)
            {
                enemy.Dispose();
            }
            Enemies.Clear();
            _Player.Dispose();
        }
    }
}
