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


namespace OrthoCite.Entities
{
    public enum ListPnj
    {
        QUARTIER_1,
        QUARTIER_2,
        QUARTIER_3,
        QUARTIER_4
    }

    public class Map : IEntity
    {
        

        RuntimeData _runtimeData;
        public TiledMap textMap;
        TiledTileLayer _upLayer;
        Helpers.Player _player;
  
        int _gidStart;
        const int _gidSpawn = 1151;
        const int _gidCol = 889;
        const int _fastSpeedPlayer = 8;
        const int _lowSpeedPlayer = 13;
        const int _zoom = 3;
        bool _firstUpdate;

        
        public Map(RuntimeData runtimeData)
        {
            _runtimeData = runtimeData;
            _gidStart = _runtimeData.gidLast;

            _firstUpdate = true;

            
            _player = new Helpers.Player(Helpers.TypePlayer.WithSpriteSheet, new Vector2(0, 0), _runtimeData, "animations/walking");
            
            _player.separeFrame = 0;
            _player.lowFrame = _lowSpeedPlayer;
            _player.fastFrame = _fastSpeedPlayer;
            _player.typeDeplacement = TypeDeplacement.WithKey;

            _runtimeData.Map = this;
            _runtimeData.Player = _player;

        }

        void IEntity.LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            textMap = content.Load<TiledMap>("map/Map");

            


            foreach (TiledTileLayer e in textMap.TileLayers)
            {
                if (e.Name == "Collision") _player.collisionLayer = e;
                else if (e.Name == "Up") _upLayer = e;
            }
            _player.collisionLayer.IsVisible = false;
            
            if (_gidStart != 0)
            {
                Console.Write("ok");
                foreach (TiledTile i in _player.collisionLayer.Tiles)
                {
                    if (i.Id == _gidStart) _player.positionVirt = new Vector2(i.X, i.Y + 1); 
                }
            }

            if(_player.positionVirt.Length() == 0)
            {
                foreach (TiledTile i in _player.collisionLayer.Tiles)
                {
                    if (i.Id == _gidSpawn) _player.positionVirt = new Vector2(i.X, i.Y);
                }
            }
            _runtimeData.gidLast = 0;
            _player.gidCol = _gidCol;

            _player.spriteFactory.Add(Helpers.Direction.NONE, new SpriteSheetAnimationData(new[] { 0 }));
            _player.spriteFactory.Add(Helpers.Direction.DOWN, new SpriteSheetAnimationData(new[] { 5, 0, 10, 0 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.LEFT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.RIGHT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.UP, new SpriteSheetAnimationData(new[] { 19, 13, 24, 13 }, isLooping: false));

            _player.LoadContent(content);

            addAllPnj(content, graphicsDevice);


        }

        void IEntity.UnloadContent()
        {
            
        }

        void IEntity.Update(GameTime gameTime, KeyboardState keyboardState, Camera2D camera)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
             

            if (_firstUpdate)
            {
                _runtimeData.DialogBox.Hide();
                camera.Zoom = _zoom;
                _player.position = new Vector2(_player.positionVirt.X * textMap.TileWidth, _player.positionVirt.Y * textMap.TileHeight);
                _firstUpdate = !_firstUpdate;
            }

            bool _stop = false;

            foreach (KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                if (i.Value.stop) _stop = true;
            }

            if(!_stop)_player.checkMove(keyboardState, camera);

            _player.heroAnimations.Update(deltaSeconds);
            _player.heroSprite.Position = new Vector2(_player.position.X + textMap.TileWidth / 2, _player.position.Y + textMap.TileHeight / 2);

            foreach(KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                i.Value.Update(gameTime, keyboardState, camera, deltaSeconds);
            }


            checkCamera(camera);

            if (keyboardState.IsKeyDown(Keys.F9)) _player.collisionLayer.IsVisible = !_player.collisionLayer.IsVisible;

            //Console.WriteLine($"X : {_positionVirt.X} Y : {_positionVirt.Y} ");
        }

        void IEntity.Draw(SpriteBatch spriteBatch, Matrix frozenMatrix, Matrix cameraMatrix)
        {
            spriteBatch.Begin(transformMatrix: cameraMatrix);

            _upLayer.IsVisible = false;
            spriteBatch.Draw(textMap, gameTime: _runtimeData.GameTime);

            _player.Draw(spriteBatch);

            foreach (KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                i.Value.Draw(spriteBatch);
            }
           

            _upLayer.IsVisible = true;
            _upLayer.Draw(spriteBatch);
            
            spriteBatch.End();

        }
        
        void IEntity.Execute(params string[] param)
        {
            
            switch(param[0])
            {
                case "movePlayer":
                    try{ MoveTo(new Vector2(Int32.Parse(param[1]), Int32.Parse(param[2]))); }
                    catch { Console.WriteLine("use : movePlayer {x] {y}"); }
                    break;
                default:
                    Console.WriteLine("Can't find method to invoke in Map Class");
                    break;
            }
        }



        private void addAllPnj(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _runtimeData.PNJ.Add(ListPnj.QUARTIER_1, new PNJ(TypePNJ.Static, new Vector2(120, 59), new List<ItemList>(), _runtimeData, "animations/walking"));

            foreach(KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                i.Value.PNJPlayer.collisionLayer = _player.collisionLayer;
                i.Value.PNJPlayer.separeFrame = _player.separeFrame;
                i.Value.PNJPlayer.lowFrame = _player.lowFrame;
                i.Value.PNJPlayer.fastFrame = _player.fastFrame;
                i.Value.PNJPlayer.typeDeplacement = TypeDeplacement.WithDirection;
            }

            _runtimeData.PNJ[ListPnj.QUARTIER_1].spriteFactory(Helpers.Direction.NONE, new SpriteSheetAnimationData(new[] { 0 }));
            _runtimeData.PNJ[ListPnj.QUARTIER_1].spriteFactory(Helpers.Direction.DOWN, new SpriteSheetAnimationData(new[] { 5, 0, 10, 0 }, isLooping: false));
            _runtimeData.PNJ[ListPnj.QUARTIER_1].spriteFactory(Helpers.Direction.LEFT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _runtimeData.PNJ[ListPnj.QUARTIER_1].spriteFactory(Helpers.Direction.RIGHT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _runtimeData.PNJ[ListPnj.QUARTIER_1].spriteFactory(Helpers.Direction.UP, new SpriteSheetAnimationData(new[] { 19, 13, 24, 13 }, isLooping: false));

            _runtimeData.PNJ[ListPnj.QUARTIER_1]._talk.Add("Bienvenue sur Orhtocité");
            _runtimeData.PNJ[ListPnj.QUARTIER_1]._talk.Add("Tu es notre sauveur ! ! !");
         
            foreach(KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                i.Value.LoadContent(content, graphicsDevice);
            }
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
            if (OutOfScreenRight(camera) && OutOfScreenBottom(camera)) camera.LookAt(new Vector2(textMap.WidthInPixels - (_runtimeData.Scene.Width / _zoom) * 2 + _runtimeData.Scene.Width / 2, -_runtimeData.Scene.Height / _zoom + _runtimeData.Scene.Height / 2));

        }

        private bool OutOfScreenTop(Camera2D camera)
        {
            if(camera.Position.Y < -_runtimeData.Scene.Height / _zoom) return true;
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

        public void checkIfWeLaunchInstance(TiledTile i)
        {
            if (i.X == _player.positionVirt.X && i.Y == _player.positionVirt.Y - 1 && i.Id == 1165)
            {
                _runtimeData.gidLast = 1165;
                _runtimeData.OrthoCite.ChangeGameContext(GameContext.MINIGAME_PLATFORMER);
            }
        }

    }
}
