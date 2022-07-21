using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    abstract public class NodesGroup<TNodeValue>
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        // public abstract IEnumerable<CausalModelNode<TNodeValue>> GetNodes();

        /// <summary>
        /// Проверяет валидность узлов группы перед применением правил, <br />
        /// согласно своим собственным требованиям <br />
        /// </summary>
        /// <param name="nodes">Узлы группы</param>
        /// <returns>Результат валидации</returns>
        public abstract ValidationResult ValidateNodes();

        /// <summary>
        /// Определяет каждый узел или его причинные связи на этапе генерации каузальной <br />
        /// модели, как правило, при отсутствии каких либо необходимых свойств. <br />
        /// Предполагается, что перед определением требования валидации выполнены. <br />
        /// Если правила группы предполагают удаление элементов, то все узлы, <br />
        /// подлежащие удалению, должны быть выявлены на этом этапе
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public abstract void Define();

        public virtual bool ShouldBeDiscarded(CausalModelNode<TNodeValue> node)
        {
            // Todo: Логика дублирует определение узлов, не состоящих в группах.
            // Можно ли это избежать?
            // Можно ли распространить функциональность групп на другие узлы?
            // Узлы вне групп тоже валидируются, определяются, могут быть удалены
            Random rnd = new Random();
            foreach (CausalModelEdge edge in node.CausesNest.Edges())
            {
                // Если не определена актуальная вероятность причинной связи
                if (edge.ActualProbability == null)
                {
                    edge.ActualProbability = rnd.NextDouble();
                }
            }
#pragma warning disable CS8629 // Nullable value type may be null.
            return node.CausesNest.IsHappened().Value;
#pragma warning restore CS8629 // Nullable value type may be null.
        }
    }
}
