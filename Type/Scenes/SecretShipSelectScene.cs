﻿using AmosShared.Audio;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.UI;

namespace Type.Scenes
{
    public class SecretShipSelectScene : Scene
    {
        private readonly TextDisplay _Title;

        private readonly Sprite _Background;

        private readonly AudioPlayer _Music;

        private Boolean _Active;

        public ShipSelectButton OmegaButton { get; }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                OmegaButton.Active = _Active;
            }
        }

        public SecretShipSelectScene()
        {
            _Title = new TextDisplay(Game.MainCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "SECRET SHIP",
                Position = new Vector2(0, 450),
                Scale = new Vector2(4, 4),
                Visible = true,
            };
            _Title.Offset = new Vector2(_Title.Size.X * _Title.Scale.X, _Title.Size.Y * _Title.Scale.Y) / 2;
            _Background = new Sprite(Game.UiCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/Background/stars-2.png"))
            {
                Position = new Vector2(0, 0),
                Visible = true,
            };
            _Background.Offset = _Background.Size / 2;

            OmegaButton = new ShipSelectButton(3, new Vector2(658, 50), "Content/Graphics/Player/player_omega.png", "OMEGA", 1, 200, 120);

            _Music = new AudioPlayer("Content/Audio/mainMenuBgm.wav", true, AudioManager.Category.MUSIC, 1);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();
            _Music.Stop();
            _Title.Dispose();
            _Background.Dispose();
            OmegaButton.Dispose();
        }
    }
}
