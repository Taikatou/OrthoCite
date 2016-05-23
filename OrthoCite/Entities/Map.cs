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


namespace OrthoCite.Entities
{
    

    class Map : IEntity
    {
        RuntimeData _runtimeData;
        TiledMap textMap;
        TiledTileLayer _collisionLayer;
        TiledTileLayer _upLayer;


        Helpers.Player _player;
  
        int _gidStart;
        const int _gidSpawn = 1151;

        const int _fastSpeed = 8;
        const int _lowSpeed = 13;
        
        int _separeFrame;
        int _actualFrame;
        int _fastFrame;
        int _lowFrame;
        const int _zoom = 3;
        bool _firstUpdate;
       
        
        public Map(RuntimeData runtimeData)
        {
            _runtimeData = runtimeData;
            _gidStart = _runtimeData.gidLast;

            _separeFrame = 0;
            _lowFrame = _lowSpeed;
            _fastFrame = _fastSpeed;

            _firstUpdate = true;

            _player = new Helpers.Player(Helpers.TypePlayer.WithSpriteSheet, new Vector2(0, 0), _runtimeData, "animations/walking");

        }

        void IEntity.LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            textMap = content.Load<TiledMap>("map/Map");
            
            foreach (TiledTileLayer e in textMap.TileLayers)
            {
                if (e.Name == "Collision") _collisionLayer = e;
                else if (e.Name == "Up") _upLayer = e;
            }
            _collisionLayer.IsVisible = false;
            
            if (_gidStart != 0)
            {
                foreach (TiledTile i in _collisionLayer.Tiles)
                {
                    if (i.Id == _gidStart) _player.positionVirt = new Vector2(i.X, i.Y + 1);
                }
            }

            if(_player.positionVirt.Length() == 0)
            {
                foreach (TiledTile i in _collisionLayer.Tiles)
                {
                    if (i.Id == _gidSpawn) _player.positionVirt = new Vector2(i.X, i.Y);
                }
            }
            _runtimeData.gidLast = 0;


            _player.spriteFactory.Add(Helpers.Direction.NONE, new SpriteSheetAnimationData(new[] { 0 }));
            _player.spriteFactory.Add(Helpers.Direction.DOWN, new SpriteSheetAnimationData(new[] { 5, 0, 10, 0 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.LEFT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.RIGHT, new SpriteSheetAnimationData(new[] { 32, 26, 37, 26 }, isLooping: false));
            _player.spriteFactory.Add(Helpers.Direction.UP, new SpriteSheetAnimationData(new[] { 19, 13, 24, 13 }, isLooping: false));

            _player.LoadContent(content);
        }

        void IEntity.UnloadContent()
        {
            
        }

        void IEntity.Update(GameTime gameTime, KeyboardState keyboardState, Camera2D camera)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            checkMove(keyboardState, camera);
            
            _player.heroAnimations.Update(deltaSeconds);
            _player.heroSprite.Position = new Vector2(_player.position.X + textMap.TileWidth / 2, _player.position.Y + textMap.TileHeight / 2);

            checkCamera(camera);

            //Console.WriteLine($"X : {_positionVirt.X} Y : {_positionVirt.Y} ");
        }

        void IEntity.Draw(SpriteBatch spriteBatch, Matrix frozenMatrix, Matrix cameraMatrix)
        {
            spriteBatch.Begin(transformMatrix: cameraMatrix);

            _upLayer.IsVisible = false;
            spriteBatch.Draw(textMap, gameTime: _runtimeData.GameTime);

            _player.Draw(spriteBatch);

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

        private void checkMove(KeyboardState keyboardState, Camera2D camera)
        {
            if (_firstUpdate)
            {
                camera.Zoom = _zoom;
                _player.position = new Vector2(_player.positionVirt.X * textMap.TileWidth, _player.positionVirt.Y * textMap.TileHeight);
                _firstUpdate = !_firstUpdate;
            }

            if (_separeFrame == 0 && keyboardState.GetPressedKeys().Length != 0 && _player.actualDir == Helpers.Direction.NONE)
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift)) _actualFrame = _fastFrame;
                else _actualFrame = _lowFrame;

                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    if (!ColDown()) _player.actualDir = Helpers.Direction.DOWN;
                    _player.lastDir = Helpers.Direction.DOWN;
                    _player.heroAnimations.Play(Helpers.Direction.DOWN.ToString());

                }
                else if (keyboardState.IsKeyDown(Keys.Up))
                {
                    if (!ColUp()) _player.actualDir = Helpers.Direction.UP;
                    _player.lastDir = Helpers.Direction.UP;
                    _player.heroAnimations.Play(Helpers.Direction.UP.ToString());
                }
                else if (keyboardState.IsKeyDown(Keys.Left))
                {
                    if (!ColLeft()) _player.actualDir = Helpers.Direction.LEFT;
                    _player.lastDir = Helpers.Direction.LEFT;
                    _player.heroAnimations.Play(Helpers.Direction.LEFT.ToString());
                }
                else if (keyboardState.IsKeyDown(Keys.Right))
                {
                    if (!ColRight()) _player.actualDir = Helpers.Direction.RIGHT;
                    _player.lastDir = Helpers.Direction.RIGHT;
                    _player.heroAnimations.Play(Helpers.Direction.RIGHT.ToString());
                }


                if (keyboardState.IsKeyDown(Keys.F9)) _collisionLayer.IsVisible = !_collisionLayer.IsVisible;

                _separeFrame++;
            }
            else if (_separeFrame != 0)
            {

                if (_separeFrame >= _actualFrame)
                {
                    if (_player.actualDir == Helpers.Direction.DOWN)
                    {
                        _player.heroAnimations.Play(Helpers.Direction.DOWN.ToString());
                        _player.MoveDownChamp();

                    }
                    if (_player.actualDir == Helpers.Direction.UP)
                    {
                        _player.MoveUpChamp();
                        _player.heroAnimations.Play(Helpers.Direction.UP.ToString());
                    }
                    if (_player.actualDir == Helpers.Direction.LEFT)
                    {
                        _player.MoveLeftChamp();
                        _player.heroAnimations.Play(Helpers.Direction.LEFT.ToString());
                    }
                    if (_player.actualDir == Helpers.Direction.RIGHT)
                    {
                        _player.MoveRightChamp();
                        _player.heroAnimations.Play(Helpers.Direction.RIGHT.ToString());
                    }

                    _player.position = new Vector2(_player.positionVirt.X * textMap.TileWidth, _player.positionVirt.Y * textMap.TileHeight);


                    _player.actualDir = Helpers.Direction.NONE;
                    _separeFrame = 0;
                }
                else
                {
                    if (_player.actualDir == Helpers.Direction.DOWN)
                    {
                        _player.position += new Vector2(0, textMap.TileHeight / _actualFrame);
                        _player.heroAnimations.Play(Helpers.Direction.DOWN.ToString());
                    }
                    if (_player.actualDir == Helpers.Direction.UP)
                    {
                        _player.position += new Vector2(0,-(textMap.TileHeight / _actualFrame));
                        _player.heroAnimations.Play(Helpers.Direction.UP.ToString());
                    }
                    if (_player.actualDir == Helpers.Direction.LEFT)
                    {
                        _player.position += new Vector2(-(textMap.TileWidth / _actualFrame),0);
                        _player.heroAnimations.Play(Helpers.Direction.LEFT.ToString());
                    }
                    if (_player.actualDir == Helpers.Direction.RIGHT)
                    {
                       
                        _player.position += new Vector2(textMap.TileWidth / _actualFrame, 0);
                        _player.heroAnimations.Play(Helpers.Direction.RIGHT.ToString());
                    }
                    _separeFrame++;
                }

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


        private void MoveTo(Vector2 vec)
        {
            _player.positionVirt = vec;
        }

        private bool ColUp()
        {
            if (_player.positionVirt.Y <= 0) return true;
            foreach (TiledTile i in _collisionLayer.Tiles)
            {
                if (i.X == _player.positionVirt.X && i.Y == _player.positionVirt.Y - 1 && i.Id == 889) return true;
                checkIfWeLaunchInstance(i);
            }
            
            return false;
        }

        private bool ColDown()
        {

            if (_player.positionVirt.Y >= textMap.Height - 1) return true;
            foreach (TiledTile i in _collisionLayer.Tiles)
            {
                if (i.X == _player.positionVirt.X && i.Y == _player.positionVirt.Y + 1 && i.Id == 889) return true;
            }
            return false;
        }

        private bool ColLeft()
        {
            if (_player.positionVirt.X <= 0) return true;
            foreach (TiledTile i in _collisionLayer.Tiles)
            {
                if (i.X == _player.positionVirt.X - 1 && i.Y == _player.positionVirt.Y && i.Id == 889) return true;
            }
            return false;
        }

        private bool ColRight()
        {
            if (_player.positionVirt.X >= textMap.Width - 1) return true;
            foreach (TiledTile i in _collisionLayer.Tiles)
            {
                if (i.X == _player.positionVirt.X + 1 && i.Y == _player.positionVirt.Y && i.Id == 889) return true;
            }
            return false;
        }

        private void checkIfWeLaunchInstance(TiledTile i)
        {
            if (i.X == _player.positionVirt.X && i.Y == _player.positionVirt.Y - 1 && i.Id == 1165)
            {
                _runtimeData.gidLast = 1165;
                _runtimeData.OrthoCite.ChangeGameContext(GameContext.MINIGAME_PLATFORMER);
            }
        }

    }
}
