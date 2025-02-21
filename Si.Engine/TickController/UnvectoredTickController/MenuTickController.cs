﻿using SharpDX.Direct2D1;
using Si.Engine.Menu._Superclass;
using Si.Engine.TickController._Superclass;
using System.Collections.Generic;

namespace Si.Engine.TickController.UnvectoredTickController
{
    public class MenuTickController : UnvectoredTickControllerBase<MenuBase>
    {
        public delegate void CollectionAccessor(List<MenuBase> sprites);
        public delegate T CollectionAccessorT<T>(List<MenuBase> sprites);

        private MenuBase? _current = null;
        public MenuBase? Current { get => _current; }

        public MenuTickController(EngineCore engine)
            : base(engine) { }

        public void Render(RenderTarget renderTarget) => _current?.Render(renderTarget);

        public void Show(MenuBase menu)
        {
            Unload(_current);
            _current = menu;
        }

        public void Unload(MenuBase? menu)
        {
            if (_current == menu)
            {
                //QueuedForDeletion is set in MenuBase.Close, so if it is true, then MenuBase.Close has already been called.
                if (_current?.QueuedForDeletion == false)
                {
                    _current.Close();
                }
                _current = null;
            }
        }

        public override void ExecuteWorldClockTick() => _current?.HandleInput();
    }
}
