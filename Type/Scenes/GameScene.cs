﻿using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using Type.Glide;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Objects.Player;
using Type.Objects.World;

namespace Type.Scenes
{
    public class GameScene : Scene
    {
        private Tweener _Tweener = new Tweener();

        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundFar;

        /// <summary> Scrolling background </summary>
        private readonly ScrollingBackground _BackgroundNear;

        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsNear;

        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _PlanetsFar;

        /// <summary> Scrolling object </summary>
        private readonly ScrollingObject _Clusters;

        /// <summary> Screen sized sprite that will be shown when a nuke is detonated </summary>
        private readonly Sprite _NukeEffect;

        private Single _NukeEffectAlpha;

        /// <summary> Main game background music </summary>
        public readonly AudioPlayer _Music;

        /// <summary> List of all enemies </summary>
        public List<IEnemy> Enemies { get; }

        /// <summary> List of all the power ups </summary>
        public List<IPowerup> Powerups { get; }

        /// <summary> The players ship </summary>
        public IPlayer Player { get; }

        public GameScene(Int32 playertype)
        {
            _BackgroundFar = new ScrollingBackground(100, "Content/Graphics/Background/stars-1.png");
            _BackgroundNear = new ScrollingBackground(200, "Content/Graphics/Background/stars-2.png");
            _Clusters = new ScrollingObject(100, 200, "Content/Graphics/Background/Clusters/cluster-", 7, 20, 40, Constants.ZOrders.CLUSTERS);
            _PlanetsFar = new ScrollingObject(200, 250, "Content/Graphics/Background/Planets/planet-far-", 9, 10, 20, Constants.ZOrders.PLANETS_FAR);
            _PlanetsNear = new ScrollingObject(250, 350, "Content/Graphics/Background/Planets/planet-near-", 9, 10, 30, Constants.ZOrders.PLANETS_NEAR);

            _NukeEffect = new Sprite(Game.MainCanvas, Constants.ZOrders.NUKE_EFFECT, Texture.GetTexture("Content/Graphics/Engine/engine_background.png"))
            {
                Position = new Vector2(0, 0),
            };
            _NukeEffect.Offset = _NukeEffect.Size / 2;

            switch (playertype)
            {
                case 0:
                    {
                        Player = new PlayerAlpha();
                        break;
                    }
                case 1:
                    {
                        Player = new PlayerBeta();
                        break;
                    }
                case 2:
                    {
                        Player = new PlayerGamma();
                        break;
                    }
                case 3:
                    {
                        Player = new PlayerOmega();
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("Player ship type does not exist");
            }

            Enemies = new List<IEnemy>();
            Powerups = new List<IPowerup>();

            _Music = new AudioPlayer("Content/Audio/bgm-1.wav", true, AudioManager.Category.MUSIC, 1);
        }

        /// <summary>
        /// Starts the background moving
        /// </summary>
        public void StartBackgroundScroll()
        {
            _BackgroundNear.Start();
            _BackgroundFar.Start();
            _PlanetsFar.Start();
            _PlanetsNear.Start();
            _Clusters.Start();
        }

        /// <summary>
        /// Removes enemies from the scene, used when the player dies to clean up explosions and effects
        /// </summary>
        public void RemoveEnemies()
        {
            foreach (IEnemy enemy in Enemies.Where(e => !e.IsDisposed).ToList())
            {
                enemy.Dispose();
            }
        }

        /// <summary>
        /// Removes disposed powerups from the scene, used when the layer dies to clean up the scene
        /// </summary>
        public void RemovePowerUps()
        {
            foreach (IPowerup powerup in Powerups.Where(e => !e.IsDisposed).ToList())
            {
                powerup.Dispose();
            }
        }

        /// <summary>
        /// Show the nuke effect
        /// </summary>
        public void ShowNukeEffect()
        {
            _Tweener.CancelAndComplete();
            _NukeEffect.Colour = new Vector4(1, 1, 1, 1);
            _NukeEffect.Visible = true;
            _NukeEffectAlpha = 1;
            _Tweener.Tween(this, new {_NukeEffectAlpha = 0}, 0.5f).OnUpdate(() =>
            {
                _NukeEffect.Colour = new Vector4(1, 1, 1, _NukeEffectAlpha);
            }).OnComplete(() =>
            {
                _NukeEffect.Visible = false;
                _NukeEffect.Colour = new Vector4(1, 1, 1, 1);
            });
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {
            _Tweener.Update((Single)timeSinceUpdate.TotalSeconds);

            foreach (IEnemy enemy in Enemies.Where(e => e.IsDisposed).ToList())
            {
                Enemies.Remove(enemy);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _Tweener.CancelAndComplete();
            _Tweener = null;
            _Music.Stop();

            foreach (IPowerup powerup in Powerups)
            {
                powerup.Dispose();
            }
            Powerups.Clear();
            foreach (IEnemy enemy in Enemies)
            {
                enemy.Dispose();
            }
            Enemies.Clear();

            Player.Dispose();
            _NukeEffect.Dispose();
            _BackgroundNear.Dispose();
            _BackgroundFar.Dispose();
            _PlanetsNear.Dispose();
            _PlanetsFar.Dispose();
            _Clusters.Dispose();
        }
    }
}
