using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Edges
{
    /// <summary>
    /// Класс, реализующий данный интерфейс, представляет ребро каузальной модели,
    /// по которому можно проходить, ссылаясь по id на узел-причину.
    /// Ссылка осуществляется по id для обеспечения связи объектов при сериализации в json
    /// </summary>
    public class CausalEdge
    {
        /// <summary>
        /// Guid узла, представляющего причину. null для корневых узлов
        /// </summary>
        public Guid? CauseId { get; set; }

        /// <summary>
        /// Узел, представляющий причину.
        /// Устанавливается на подготовительном этапе, чтобы не получать узел по id 
        /// на каждой генерации.
        /// Тип TNodeValue неизвестен, а добавление обобщения для CausalEdge
        /// загромождает код, поэтому выбран тип object
        /// </summary>
        internal object? Cause { get; set; }
    }
}
