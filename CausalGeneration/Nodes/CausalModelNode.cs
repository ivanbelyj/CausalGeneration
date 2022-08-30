using CausalGeneration.Edges;
using CausalGeneration.CausesExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CausalGeneration.Nests;
using CausalGeneration.Nodes;

namespace CausalGeneration
{
    /// <summary>
    /// Узел каузальной модели, моделирующий существование факта, событие, свойство, и т.п.
    /// Находится в причинно-следственных отношениях с другими узлами посредством
    /// каузальных связей.
    /// Причинные вероятностные связи структурируются в соотв. гнезде причин.
    /// Смысл вероятностных гнезд для узлов-реализаций абстрактных сущностей (АС),
    /// так называемых вариантов, которые представлены производным классом,
    /// заключается в том, что реализации АС могут появляться
    /// в зависимости от логической комбинации факторов, а не только избираться посредством
    /// весов.
    /// </summary>
    /// <typeparam name = "TNodeValue">Тип значения, которое узел содержит</typeparam>
    public class CausalModelNode<TNodeValue> : IHappenable
    {
        // Для десериализации
        public CausalModelNode() : this(new ProbabilityNest()) { }
        public CausalModelNode(Guid id, ProbabilityNest probabilityNest,
            TNodeValue? value = default(TNodeValue))
        {
            ProbabilityNest = probabilityNest;
            Id = id;
            Value = value;
        }
        public CausalModelNode(ProbabilityNest probabilityNest,
            TNodeValue? value = default(TNodeValue))
                : this(Guid.NewGuid(), probabilityNest, value) { }

        public Guid Id { get; set; }

        public ProbabilityNest ProbabilityNest { get; set; }

        /// <summary>
        /// Все исходящие причинные ребра. Гнезд разного рода может быть несколько,
        /// поэтому метод можно переопределять
        /// </summary>
        public virtual IEnumerable<CausalEdge> GetEdges() => ProbabilityNest.GetEdges();

        public virtual bool IsRootNode() => ProbabilityNest.IsRootNest();

        /// <summary>
        /// Если null, то данное звено – только связующее
        /// </summary>
        public TNodeValue? Value { get; set; }

        // Члены класса, используемые и определяемые на этапе подготовки и генерации
        // в CausalModel, по сути, не относящиеся к ответственности класса,
        // однако хранить их где-то в другом месте было бы проблематично и
        // не так быстро
        #region Generation
        /// <summary>
        /// true, если событие входит в результирующую модель
        /// </summary>
        bool IHappenable.IsHappened { get; set; }

        /// <summary>
        /// true, если для узла уже определена глубина. Используется при подготовке
        /// к генерации для оптимизации
        /// </summary>
        internal bool HasLevel { get; set; }
        #endregion

        public override string? ToString() => Value?.ToString() ?? Id.ToString();
    }
}
