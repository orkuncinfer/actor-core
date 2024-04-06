using System;

namespace Core.Editor
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = true, Inherited = false)]
    public class TitleAttribute : Attribute
    {
        public string[] Title;

        public TitleAttribute(params string[] title)
        {
            this.Title = title;
        }
    }
}