using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Model
{
    public class CausalModelSerializer
    {
        public CausalModelSerializer() { }
        // Полиморфная сериализация/десериализация необходима для
        // CausesExpression => And, Or, Edge, Not
        // (Nest - нет)
        // (CausalEdge - нет)
        // CausalModelNode => ImplementationNode

        public string ToJson<TNodeValue>(ICausalModel<TNodeValue> causalMode,
            bool writeIndented = false)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = writeIndented ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new SerializationBinder<TNodeValue>()
            };
            return JsonConvert.SerializeObject(causalMode, settings);
        }

        public TModel FromJson<TModel, TNodeValue>(string jsonString)
        // where TModel<TNodeValue> : ICausalModel<TNodeValue>
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new SerializationBinder<TNodeValue>()
            };

            // var model = JsonConvert.DeserializeObject<TModel>(jsonString, settings);
            var model = JsonConvert.DeserializeObject<TModel>(jsonString, settings);
            return model;
        }
    }
}
