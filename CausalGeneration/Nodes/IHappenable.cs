using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration.Nodes
{
    /// <summary>
    /// Сущность, которая может произойти, либо не произойти. Используется для выделения
    /// необходимой части функциональности, необходимой на определенных этапах генерации
    /// </summary>
    internal interface IHappenable
    {
        /// <summary>
        /// true, если сущность включена в окончательную выборку произошедшего
        /// </summary>
        bool IsHappened { get; set; }
    }
}
