using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BandoWare.GameplayTags
{
   public class GameplayTagJsonConverter : JsonConverter<GameplayTag>
   {
      public override void WriteJson(JsonWriter writer, GameplayTag value, JsonSerializer serializer)
      {
         JObject obj = new JObject
         {
            { "tagName", value.IsNone ? "" : value.Name }
         };
         obj.WriteTo(writer);
      }

      public override GameplayTag ReadJson(JsonReader reader, Type objectType, GameplayTag existingValue, bool hasExistingValue, JsonSerializer serializer)
      {
         JObject jObject = JObject.Load(reader);
         string tagName = jObject["tagName"]?.Value<string>();

         if (string.IsNullOrEmpty(tagName))
            return GameplayTag.None;

         return GameplayTagManager.RequestTag(tagName);
      }
   }

   public class GameplayTagContainerJsonConverter : JsonConverter<GameplayTagContainer>
   {
      public override void WriteJson(JsonWriter writer, GameplayTagContainer value, JsonSerializer serializer)
      {
         if (value == null)
         {
            writer.WriteNull();
            return;
         }

         JArray tagsArray = new JArray();

         foreach (GameplayTag tag in value.GetExplicitTags())
         {
            JObject tagObj = new JObject
            {
               { "tagName", tag.ToString() }
            };
            tagsArray.Add(tagObj);
         }

         JObject containerObj = new JObject
         {
            { "gameplayTags", tagsArray }
         };

         containerObj.WriteTo(writer);
      }

      public override GameplayTagContainer ReadJson(JsonReader reader, Type objectType, GameplayTagContainer existingValue, bool hasExistingValue, JsonSerializer serializer)
      {
         JObject jObject = JObject.Load(reader);
         JArray tagsArray = jObject["gameplayTags"] as JArray;

         GameplayTagContainer container = new GameplayTagContainer();

         if (tagsArray == null)
            return container;

         foreach (JObject tagObj in tagsArray)
         {
            string tagName = tagObj["tagName"]?.Value<string>();

            if (string.IsNullOrEmpty(tagName))
               continue;

            GameplayTag tag = GameplayTagManager.RequestTag(tagName);
            if (tag.IsValid)
               container.AddTag(tag);
         }

         return container;
      }
   }

   public static class GameplayTagJsonSettings
   {
      private static JsonSerializerSettings s_Settings;

      public static JsonSerializerSettings Settings
      {
         get
         {
            if (s_Settings == null)
            {
               s_Settings = new JsonSerializerSettings
               {
                  Converters = new JsonConverter[]
                  {
                     new GameplayTagJsonConverter(),
                     new GameplayTagContainerJsonConverter()
                  }
               };
            }
            return s_Settings;
         }
      }

      /// <summary>
      /// Serializes an object to JSON using GameplayTag converters.
      /// </summary>
      public static string Serialize(object obj)
      {
         return JsonConvert.SerializeObject(obj, Settings);
      }

      /// <summary>
      /// Deserializes JSON to an object using GameplayTag converters.
      /// </summary>
      public static T Deserialize<T>(string json)
      {
         return JsonConvert.DeserializeObject<T>(json, Settings);
      }
   }
}
