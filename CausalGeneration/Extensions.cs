using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CausalGeneration
{
    internal static class Extensions
    {
        /// <summary>
        /// Генерирует случайное число от 0 до max (не включительно)
        /// </summary>
        /// <returns></returns>
        public static double NextDouble(this Random random, double min, double max)
            => (max - min) * random.NextDouble() + min;
    }
}
