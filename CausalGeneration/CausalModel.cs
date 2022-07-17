using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>  // : IEnumerable<TNodeValue>
    {
        /// <summary>
        /// Id корневых узлов
        /// </summary>
        public ISet<Guid> Roots { get; set; }
        public List<CausalModelNode<TNodeValue>> Nodes { get; set; }

        public CausalModelNode<TNodeValue>? FindNodeById(Guid id)
        {
            return Nodes.FirstOrDefault(x => x.Id == id);
        }

        // Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            Roots = new HashSet<Guid>();
            Nodes = new List<CausalModelNode<TNodeValue>>();
        }

        public CausalModelNode<TNodeValue> AddNode(CausalModelNode<TNodeValue> node)
        {
            Nodes.Add(node);
            return node;
        }
        public CausalModelNode<TNodeValue> AddNode(CausesNest causesNest,
            TNodeValue? value = default(TNodeValue))
        {
            var node = new CausalModelNode<TNodeValue>(causesNest, value);
            return AddNode(node);
        }

        public void AddRoot(Guid id)
        {
            Roots.Add(id);
        }

        public CausalModelNode<TNodeValue> AddRootNode(TNodeValue value, double probability)
        {
            var node = AddNode(new CausesNest(null, probability), value);
            AddRoot(node.Id);
            return node;
        }

        #endregion

        #region Json
        public CausalModel(string json) : this()
        {
            throw new NotImplementedException();
        }
        public string ToJson()
        {
            throw new NotImplementedException();
        }

        // Todo: Какой тип модели?
        public static CausalModel<TNodeValue> FromJson()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void Generate()
        {
            DiscardAllNotHappened();
            DiscardGarbageNodes();
        }

        private void DiscardAllNotHappened()
        {
            Random rnd = new Random();
            foreach (CausalModelNode<TNodeValue> node in Nodes.Select(x => x).ToList())
            {
                foreach (CausalModelEdge edge in node.CausesNest.Edges())
                {
                    if (!edge.ActualProbability.HasValue)
                    {
                        edge.ActualProbability = rnd.NextDouble();
                    }

                }
                // Можно вызывать IsHappened
                bool? isHappened = node.CausesNest.IsHappened();
                // Если такое вообще возможно
                if (!isHappened.HasValue)
                {
                    throw new Exception("Ошибка на этапе отбрасывания непроизошедшего." +
                        " Не определено, произошло ли событие.");
                }
                if (!isHappened.Value)
                {
                    DiscardNode(node);
                    continue;
                }

                // Для того, что, возможно, произошло, собираются следствия
                // для дальнейшего обхода.
                foreach (CausalModelEdge edge in node.CausesNest.Edges())
                {
                    // Если у узла есть причина, значит узел - ее следствие
                    if (edge.CauseId.HasValue)
                    {
                        CausalModelNode<TNodeValue>? cause =
                            FindNodeById(edge.CauseId.Value);
                        if (cause != null)
                        {
                            if (cause.Effects == null)
                            {
                                cause.Effects = new List<CausalModelNode<TNodeValue>>();
                            }
                            cause.Effects.Add(node);
                        }
                    }
                }
            }
        }

        private void DiscardNode(CausalModelNode<TNodeValue> node)
        {
            // 1. Для каждого следствия удалить node из гнезда причин,
            // чтобы не учитывать непроизошедшее событие в структуре
            // сгенерированной модели

            if (node.Effects != null)
            {
                node.Effects.ForEach(effect =>
                {
                    effect.CausesNest.DiscardCause(node.Id);
                });
            } else
            {
                // Иначе это лист графа, он не имеет следствий
            }

            // 2. Для каждой причины узла удалять его из Effects, чтобы
            // в дальнейшем обходе по Effects узел не учитывался
            foreach (CausalModelEdge edge in node.CausesNest.Edges())
            {
                if (edge.CauseId == null)
                    continue;
                var cause = FindNodeById(edge.CauseId.Value);
                cause?.Effects?.Remove(node);
            }

            // 3. Удалить из _nodes, т.к. событие больше не нужно
            Nodes.Remove(node);

            // 4. Если данный узел - корневой, то также удалить из корней
            if (Roots.Contains(node.Id))
            {
                Roots.Remove(node.Id);
            }
        }

        /// <summary>
        /// Путем прохода по модели от причин к следствиям, оставляет в модели только
        /// актуальные узлы
        /// </summary>
        private void DiscardGarbageNodes()
        {
            Nodes = ActualNodes();
        }

        private List<CausalModelNode<TNodeValue>> ActualNodes()
        {
            var res = new List<CausalModelNode<TNodeValue>>();
            foreach (Guid rootId in Roots)
            {
                AddNodeAndEffects(res, FindNodeById(rootId));
            }
            return res;
        }
        private void AddNodeAndEffects(List<CausalModelNode<TNodeValue>> list,
            CausalModelNode<TNodeValue>? node)
        {
            if (node != null)
            {
                if (node.Effects != null)
                {
                    foreach (CausalModelNode<TNodeValue> effect in node.Effects)
                    {
                        AddNodeAndEffects(list, effect);
                    }
                }
                list.Add(node);
            }
        }
    }
}
