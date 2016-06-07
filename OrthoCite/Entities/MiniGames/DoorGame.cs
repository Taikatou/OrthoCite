﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System;
using OrthoCite.Helpers;
using System.Collections.Generic;

namespace OrthoCite.Entities.MiniGames
{
    public class DoorGame : MiniGame
    {
        RuntimeData _runtimeData;
        public TiledMap textMap;
        Helpers.Player _player;
        SpriteFont _font;

        int _gidStart;
        const int _gidSpawn = 46;

        const int _fastSpeedPlayer = 8;
        const int _lowSpeedPlayer = 13;
        const int _zoom = 3;
        bool _firstUpdate;

        Dictionary<int, List<object>> _world;

        struct Word
        {
            public string Value;
            public bool IsValid;
        }

        struct WordCollection
        {
            public Word Valid;
            public List<Word> Invalid;

            public WordCollection(string valid)
            {
                Valid = new Word { IsValid = true, Value = valid };
                Invalid = new List<Word>();
            }

            public void AddInvalid(string invalid)
            {
                Invalid.Add(new Word { Value = invalid });
            }
        }

        


        public DoorGame(RuntimeData runtimeData)
        {
            _runtimeData = runtimeData;
            _gidStart = _runtimeData.gidLast;

            _firstUpdate = true;

            _player = new Helpers.Player(Helpers.TypePlayer.WithSpriteSheet, new Vector2(0, 0), _runtimeData, "animations/walking");
            _runtimeData.Player = _player;
            _player.separeFrame = 0;
            _player.lowFrame = _lowSpeedPlayer;
            _player.fastFrame = _fastSpeedPlayer;
            _player.typeDeplacement = TypeDeplacement.WithKey;
            
            _runtimeData.DoorGame = this;

            _world = new Dictionary<int, List<object>>();

            _world.Add(563, new List<object>());
            _world.Add(564, new List<object>());
            _world.Add(573, new List<object>());
            _world.Add(565, new List<object>());
            _world.Add(566, new List<object>());
            _world.Add(574, new List<object>());
        }

        public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            textMap = content.Load<TiledMap>("minigames/DoorGame/sallePorte");
            _font = content.Load<SpriteFont>("minigames/platformer/font");
            foreach (TiledTileLayer e in textMap.TileLayers)
            {
                if (e.Name == "collision") _player.collisionLayer = e;
            }
            _player.collisionLayer.IsVisible = false;

            if (_gidStart != 0)
            {
                foreach (TiledTile i in _player.collisionLayer.Tiles)
                {
                    if (i.Id == _gidStart) _player.positionVirt = new Vector2(i.X, i.Y + 1);
                }
            }

            if (_player.positionVirt.Length() == 0)
            {
                foreach (TiledTile i in _player.collisionLayer.Tiles)
                {
                    if (i.Id == _gidSpawn) _player.positionVirt = new Vector2(i.X, i.Y);
                }
            }
            _runtimeData.gidLast = 0;

            _player.gidCol = 633;
            _player.spriteFactory.Add(Helpers.Direction.NONE, new SpriteSheetAnimationData(new[] { 0 }));
            _player.spriteFactory.Add(Helpers.Direction.DOWN, new SpriteSheetAnimationData(new[] { 5,10 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.LEFT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.RIGHT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.UP, new SpriteSheetAnimationData(new[] { 19, 13, 24, 13 }, isLooping: false));

            _player.LoadContent(content);
            
            

        }

        public override void UnloadContent()
        {
        }

        public override void Update(GameTime gameTime, KeyboardState keyboardState, Camera2D camera)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_firstUpdate)
            {
                camera.Zoom = _zoom;
                _player.position = new Vector2(_player.positionVirt.X * textMap.TileWidth, _player.positionVirt.Y * textMap.TileHeight);
                _firstUpdate = !_firstUpdate;
                _runtimeData.DialogBox.SetText("Totografe");
                _runtimeData.DialogBox.Show();
            }

            _player.checkMove(keyboardState);

            _player.heroAnimations.Update(deltaSeconds);
            _player.heroSprite.Position = new Vector2(_player.position.X + textMap.TileWidth / 2, _player.position.Y + textMap.TileHeight / 2);

            checkCamera(camera);

            if (keyboardState.IsKeyDown(Keys.F9)) _player.collisionLayer.IsVisible = !_player.collisionLayer.IsVisible;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix frozenMatrix, Matrix cameraMatrix)
        {
            spriteBatch.Begin(transformMatrix: cameraMatrix);

            spriteBatch.Draw(textMap, gameTime: _runtimeData.GameTime);

            _player.Draw(spriteBatch);


            spriteBatch.End();
        }

        

        public override void Execute(params string[] param)
        {
            switch (param[0])
            {
                case "movePlayer":
                    try { MoveTo(new Vector2(Int32.Parse(param[1]), Int32.Parse(param[2]))); }
                    catch { Console.WriteLine("use : movePlayer {x] {y}"); }
                    break;
                default:
                    Console.WriteLine("Can't find method to invoke in Map Class");
                    break;
            }
        }

        internal override void Start()
        {
            WordCollection words = new WordCollection("orthographe");
            words.AddInvalid("ortographe");
            words.AddInvalid("ortograf");
        }

        private void checkCamera(Camera2D camera)
        {
            camera.LookAt(new Vector2(_player.position.X, _player.position.Y));
            if (OutOfScreenTop(camera)) camera.LookAt(new Vector2(_player.position.X, -_runtimeData.Scene.Height / _zoom + _runtimeData.Scene.Height / 2));
            if (OutOfScreenLeft(camera)) camera.LookAt(new Vector2(-_runtimeData.Scene.Width / _zoom + _runtimeData.Scene.Width / 2, _player.position.Y));
            if (OutOfScreenRight(camera)) camera.LookAt(new Vector2(textMap.WidthInPixels - (_runtimeData.Scene.Width / _zoom) * 2 + _runtimeData.Scene.Width / 2, _player.position.Y));
            if (OutOfScreenBottom(camera)) camera.LookAt(new Vector2(_player.position.X, textMap.HeightInPixels - (_runtimeData.Scene.Height / _zoom) * 2 + _runtimeData.Scene.Height / 2));

            if (OutOfScreenLeft(camera) && OutOfScreenBottom(camera)) camera.LookAt(new Vector2(-_runtimeData.Scene.Width / _zoom + _runtimeData.Scene.Width / 2, textMap.HeightInPixels - (_runtimeData.Scene.Height / _zoom) * 2 + _runtimeData.Scene.Height / 2));
            if (OutOfScreenLeft(camera) && OutOfScreenTop(camera)) camera.LookAt(new Vector2(-_runtimeData.Scene.Width / _zoom + _runtimeData.Scene.Width / 2, -_runtimeData.Scene.Height / _zoom + _runtimeData.Scene.Height / 2));

            if (OutOfScreenRight(camera) && OutOfScreenTop(camera)) camera.LookAt(new Vector2(textMap.WidthInPixels - (_runtimeData.Scene.Width / _zoom) * 2 + _runtimeData.Scene.Width / 2, textMap.HeightInPixels - (_runtimeData.Scene.Height / _zoom) * 2 + _runtimeData.Scene.Height / 2));
            if (OutOfScreenRight(camera) && OutOfScreenBottom(camera)) camera.LookAt(new Vector2(textMap.WidthInPixels - (_runtimeData.Scene.Width / _zoom) * 2 + _runtimeData.Scene.Width / 2, textMap.HeightInPixels - (_runtimeData.Scene.Height / _zoom) * 2 + _runtimeData.Scene.Height / 2));

        }

        private bool OutOfScreenTop(Camera2D camera)
        {
            if (camera.Position.Y < -_runtimeData.Scene.Height / _zoom) return true;
            return false;
        }
        private bool OutOfScreenLeft(Camera2D camera)
        {
            if (camera.Position.X <= -_runtimeData.Scene.Width / _zoom) return true;
            return false;
        }
        private bool OutOfScreenRight(Camera2D camera)
        {
            if (camera.Position.X >= textMap.WidthInPixels - (_runtimeData.Scene.Width / _zoom) * 2) return true;
            return false;
        }
        private bool OutOfScreenBottom(Camera2D camera)
        {
            if (camera.Position.Y >= textMap.HeightInPixels - (_runtimeData.Scene.Height / _zoom) * 2) return true;
            return false;
        }


        public void MoveTo(Vector2 vec)
        {
            _player.positionVirt = vec;
        }

        internal bool CheckColUp(TiledTile i)
        {
            foreach(KeyValuePair<int, List<object>> e in _world)
            {
                if (i.Y == _player.positionVirt.Y - 1 && i.X == _player.positionVirt.X && i.Id == e.Key)
                {
                    _runtimeData.DialogBox.SetText(e.Key.ToString());
                    _runtimeData.DialogBox.Show();
                    return true;
                }
            }
            
            
            return false;
        }

        internal bool CheckColRight(TiledTile i)
        {
            foreach (KeyValuePair<int, List<object>> e in _world)
            {
                if (i.Y == _player.positionVirt.Y && i.X == _player.positionVirt.X  + 1 && i.Id == e.Key)
                {
                    _runtimeData.DialogBox.SetText(e.Key.ToString());
                    _runtimeData.DialogBox.Show();
                    return true;
                }
            }
            return false;
        }

        internal bool CheckColLeft(TiledTile i)
        {
            foreach (KeyValuePair<int, List<object>> e in _world)
            {
                if (i.Y == _player.positionVirt.Y && i.X == _player.positionVirt.X - 1 && i.Id == e.Key)
                {
                    _runtimeData.DialogBox.SetText(e.Key.ToString());
                    _runtimeData.DialogBox.Show();
                    return true;
                }
            }
            return false;   
        }
    }
}
