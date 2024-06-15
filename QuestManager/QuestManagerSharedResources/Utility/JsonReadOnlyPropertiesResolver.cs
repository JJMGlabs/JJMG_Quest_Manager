using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace QuestManagerSharedResources.Utility
{
    public class JsonReadOnlyPropertiesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (!property.Writable && property.Readable)
            {
                property.ShouldSerialize = obj => true; // Include read-only properties
            }

            return property;
        }
    }
}