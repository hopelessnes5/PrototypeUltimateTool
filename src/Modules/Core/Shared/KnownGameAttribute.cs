using System;

namespace Nixson.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KnownGameAttribute : Attribute
    {
        public KnownGameAttribute(PrototypeGame game)
        {
            this.Game = game;
        }
        public PrototypeGame Game;
    }
}
