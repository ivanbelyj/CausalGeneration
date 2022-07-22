using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Groups
{
    // Todo:  Можно ли распространить функциональность групп на другие узлы?
    // Узлы вне групп тоже валидируются, определяются, могут быть удалены
    abstract public class NodesGroup<TNodeValue>
    {
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Проверяет валидность узлов группы перед применением правил, <br />
        /// согласно своим собственным требованиям <br />
        /// </summary>
        public abstract ValidationResult ValidateNodes();

        /// <summary>
        /// Определяет каждый узел или его причинные связи на этапе генерации каузальной <br />
        /// модели, как правило, при отсутствии каких либо необходимых свойств. <br />
        /// Предполагается, что перед определением требования валидации выполнены. <br />
        /// Если правила группы предполагают удаление элементов, то все узлы, <br />
        /// подлежащие удалению, должны быть выявлены на этом этапе
        /// </summary>
        public abstract void Define();

        /// <summary>
        /// Вызывается для каждого узла группы на этапе отбрасывания
        /// </summary>
        /// <param name="node">Один из элементов группы</param>
        /// <returns>true, если элемент должен быть отброшен</returns>
        public abstract bool ShouldBeDiscarded(CausalModelNode<TNodeValue> node);
    }
}
