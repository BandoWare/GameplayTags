using System;

namespace BandoWare.GameplayTags
{
   [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
   public class GameplayTagAttribute : Attribute
   {
      public string TagName { get; set; }
      public string Description { get; set; }
      public GameplayTagFlags Flags { get; set; }

      public GameplayTagAttribute(string tagName, string description = null, GameplayTagFlags flags = GameplayTagFlags.None)
      {
         TagName = tagName;
         Description = description;
         Flags = flags;
      }
   }
}