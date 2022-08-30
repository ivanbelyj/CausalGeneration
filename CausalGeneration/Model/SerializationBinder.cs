using CausalGeneration.CausesExpressionTree;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Model
{
    public class SerializationBinder<TNodeValue> : ISerializationBinder
    {
        private static readonly List<(Type type, string name)> _knownTypeNames = new List<(Type, string)>() {
            (typeof(ConjunctionOperation), "and"),
            (typeof(DisjunctionOperation), "or"),
            (typeof(EdgeLeaf), "edge"),
            (typeof(InversionOperation), "not"),

            (typeof(CausalModelNode<TNodeValue>), "node"),
            (typeof(ImplementationNode<TNodeValue>), "implementation"),

            // Функциональность работала и без следующих двух строчек
            (typeof(CausalGenerationModel<TNodeValue>), "generation-model"),
            (typeof(CausalResultModel<TNodeValue>), "result-model"),
        };
        public static List<(Type type, string name)> KnownTypeNames => _knownTypeNames;

        public void BindToName(Type serializedType, out string? assemblyName,
            out string? typeName)
        {
            typeName = _knownTypeNames.FirstOrDefault(x => x.type == serializedType).name;
            if (typeName is null)
            {
                throw new ArgumentException("Незарегистрированный тип данных для привязки - "
                    + serializedType.FullName);
            }
            assemblyName = null;
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            Type? type = _knownTypeNames.FirstOrDefault(x => x.name == typeName).type;
            if (type is null)
            {
                throw new ArgumentException("Некорректное значение типа данных для привязки - "
                    + typeName);
            }
            return type;
        }
    }
}
