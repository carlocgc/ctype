﻿using AmosShared.Graphics.Drawables;
using AmosShared.State;
using System;
using AmosShared.Audio;
using Type.Controllers;
using Type.Data;
using Type.Factories;
using Type.Interfaces.Enemies;
using Type.Interfaces.Player;
using Type.Interfaces.Powerups;
using Type.Scenes;
using Type.UI;

namespace Type.States
{
    /// <summary>
    /// Game play state
    /// </summary>
    public class PlayingState : State, IPlayerListener, IEnemyListener, IEnemyFactoryListener, IPowerupListener, IPowerupFactoryListener
    {
        /// <summary> Max level of the game </summary>
        private readonly Int32 _MaxLevel = 2;
        /// <summary> THe type of player craft </summary>
        private readonly Int32 _PlayerType;

        /// <summary> Scene for game objects </summary>
        private GameScene _GameScene;
        /// <summary> Scene for UI objects </summary>
        private UIScene _UIScene;
        /// <summary> Factory that will create enemies </summary>
        private EnemyFactory _EnemyFactory;
        /// <summary> Factory that creates power ups </summary>
        private PowerupFactory _PowerupFactory;
        /// <summary> The player </summary>
        private IPlayer _Player;
        /// <summary> The current level </summary>
        private Int32 _CurrentLevel;
        /// <summary> Whether the game is over </summary>
        private Boolean _GameOver;
        /// <summary> Whether the game is complete </summary>
        private Boolean _GameComplete;
        /// <summary> Displays the current level as text on the screen </summary>
        private LevelDisplay _LevelDisplay;
        /// <summary> Text to display the score </summary>
        private TextDisplay _ScoreDisplay;
        /// <summary> Displays the players current lives </summary>
        private LifeMeter _LifeMeter;

        public PlayingState(Int32 type)
        {
            _PlayerType = type;
        }

        protected override void OnEnter()
        {
            _CurrentLevel = 2;

            _EnemyFactory = new EnemyFactory();
            _EnemyFactory.RegisterListener(this);
            _EnemyFactory.ParentState = this;

            _PowerupFactory = new PowerupFactory();
            _PowerupFactory.RegisterListener(this);

            _GameScene = new GameScene(_PlayerType) { Visible = true };

            _Player = _GameScene.Player;
            _Player.RegisterListener(this);
            CollisionController.Instance.RegisterPlayer(_Player);

            _UIScene = new UIScene(_PlayerType);
            _UIScene.RegisterListener(_Player);
            _UIScene.AnalogStick.RegisterListener(_Player);
            _ScoreDisplay = _UIScene.ScoreDisplay;
            _LifeMeter = _UIScene.LifeMeter;
            _LevelDisplay = _UIScene.LevelDisplay;
            _UIScene.Active = true;

            _GameScene.StartBackgroundScroll();

            GameStats.Instance.Score = 0;
            GameStats.Instance.GameStart();

            _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
            {
                _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                CollisionController.Instance.IsActive = true;
            });
        }

        public override Boolean IsComplete()
        {
            if (_GameOver) ChangeState(new GameOverState());
            else if (_GameComplete) ChangeState(new GameCompleteState());

            Boolean gameEnded = _GameOver || _GameComplete;
            return gameEnded;
        }

        #region Player

        /// <inheritdoc />
        public void OnLifeAdded(IPlayer player, Int32 points)
        {
            if (_LifeMeter.PlayerLives == 5)
            {
                UpdateScore(points);
                new AudioPlayer("Content/Audio/points_instead.wav", false, AudioManager.Category.EFFECT, 1);
                return;
            }
            _LifeMeter.AddLife();
        }

        /// <inheritdoc />
        public void OnPointPickup(Int32 value)
        {
            UpdateScore(value);
        }

        /// <inheritdoc />
        public void OnPlayerHit(IPlayer player)
        {

        }

        /// <inheritdoc />
        public void OnPlayerDeath(IPlayer player)
        {
            CollisionController.Instance.ClearObjects();
            _GameScene.RemoveEnemies();
            _GameScene.RemovePowerUps();

            _LifeMeter.LoseLife();

            if (_LifeMeter.PlayerLives > 0)
            {
                _Player.Spawn();
                _EnemyFactory.RestartWave();
            }
            else
            {
                GameOver();
            }
        }

        #endregion

        #region Enemy

        /// <inheritdoc />
        public void EnemyCreated(IEnemy enemy)
        {
            _GameScene.Enemies.Add(enemy);
        }

        /// <inheritdoc />
        public void OnEnemyDestroyed(IEnemy enemy)
        {
            UpdateScore(enemy.Points);
            if (!_EnemyFactory.Creating && CollisionController.Instance.Enemies == 0)
            {
                LevelComplete();
                return;
            }
            _PowerupFactory.Create(0, enemy.Position, _CurrentLevel);
        }

        /// <inheritdoc />
        public void OnEnemyOffscreen(IEnemy enemy)
        {
            enemy.Dispose();
            if (!_EnemyFactory.Creating && CollisionController.Instance.Enemies == 0) LevelComplete();
        }

        #endregion


        #region  Powerups

        /// <inheritdoc />
        public void OnPowerupCreated(IPowerup powerup)
        {
            _GameScene.Powerups.Add(powerup);
            CollisionController.Instance.RegisterPowerup(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupApplied(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        /// <inheritdoc />
        public void OnPowerupExpired(IPowerup powerup)
        {
            _GameScene.Powerups.Remove(powerup);
        }

        #endregion

        #region Game_Logic

        /// <summary>
        /// Adds the value to the players current score
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateScore(Int32 amount)
        {
            GameStats.Instance.Score += amount;
            _ScoreDisplay.Text = GameStats.Instance.Score.ToString();
        }

        /// <summary>
        /// Sets the next level data and displays the current level, ends the game if complete
        /// </summary>
        private void LevelComplete()
        {
            if (_CurrentLevel >= _MaxLevel) GameCompleted();
            else
            {
                _CurrentLevel++;
                _LevelDisplay.ShowLevel(_CurrentLevel, TimeSpan.FromSeconds(2), () =>
                {
                    _EnemyFactory.Start(LevelLoader.GetWaveData(_CurrentLevel));
                });
            }
        }

        /// <summary>
        /// Ends the playing state and set the next state to be <see cref="GameCompleteState"/>
        /// </summary>
        private void GameCompleted()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _EnemyFactory.Stop();
            _UIScene.Active = false;
            _GameComplete = true;
        }

        /// <summary>
        /// Ends the game and sets the next state to Game over state
        /// </summary>
        private void GameOver()
        {
            GameStats.Instance.GameEnd();
            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _EnemyFactory.Stop();
            _UIScene.Active = false;
            _GameOver = true;
        }

        #endregion

        protected override void OnExit()
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();

            CollisionController.Instance.IsActive = false;
            CollisionController.Instance.ClearObjects();
            _EnemyFactory.Dispose();
            _GameScene.Dispose();
            _UIScene.Dispose();
        }
    }
}
