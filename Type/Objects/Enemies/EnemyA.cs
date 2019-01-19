﻿using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Audio;
using OpenTK;
using Type.Objects.Projectiles;

namespace Type.Objects.Enemies
{
    public class EnemyA : BaseEnemy
    {
        public EnemyA(String assetPath, Vector2 spawnPos, Single rotation, Vector2 direction, Single speed, TimeSpan fireRate, Int32 hitPoints)
            : base(assetPath, spawnPos, rotation, direction, speed, fireRate, hitPoints)
        {
            PointValue = 10;
        }

        protected override void Fire()
        {
            if (!_IsHostile) return;
            new Bullet("Content/Graphics/bullet.png", Position, new Vector2(-1, 0), 800, Math.PI, false, new Vector4(255, 0, 0, 1));
            _IsWeaponLocked = true;
            new AudioPlayer("Content/Audio/laser2.wav", false, AudioManager.Category.EFFECT, 1);
        }
    }
}
