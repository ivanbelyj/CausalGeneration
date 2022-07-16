using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    public class CausalModel<TNodeValue>  // : IEnumerable<TNodeValue>
    {
        /// <summary>
        /// Id корневых узлов
        /// </summary>
        private List<Guid> _roots;
        public List<CausalModelNode<TNodeValue>> _nodes;

        public CausalModelNode<TNodeValue>? FindNodeById(Guid id)
        {
            return _nodes.FirstOrDefault(x => x.Id == id);
        }

        // Методы для определения и построения модели
        #region ModelCreation
        public CausalModel()
        {
            _roots = new List<Guid>();
            _nodes = new List<CausalModelNode<TNodeValue>>();
        }

        public CausalModelNode<TNodeValue> AddNode(CausalModelNode<TNodeValue> node)
        {
            _nodes.Add(node);
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
            _roots.Add(id);
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
            // Подготовка. 1 этап
            Random rnd = new Random();
            foreach (CausalModelNode<TNodeValue> node in _nodes.Select(x => x).ToList())
            {
                foreach (CausalModelEdge edge in node.CausesNest)
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
                foreach (CausalModelEdge edge in node.CausesNest)
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

        // Todo: IsHappened свидетельствует только о выполнении необходимого условия
        // происшествия события.
        // ActualProbability, строго говоря, - не вероятность

        //private void DiscardAllNotHappened()
        //{
        //    foreach (var node in _nodes)
        //    {
        //        if ((!node.CausesNest.IsHappened()) ?? false)
        //        {
        //            DiscardNode(node);
        //        }
        //    }
        //}

        private void DiscardNode(CausalModelNode<TNodeValue> node)
        {
            // 1. Для каждого следствия удалить node из гнезда причин,
            // чтобы не учитывать непроизошедшее событие в сгенерированной модели

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

            // 2. По видимому, не требуется для каждой причины удалять node из Effects

            // 3. Удалить из _nodes, т.к. событие больше не нужно
            _nodes.Remove(node);
        }
    }
}
