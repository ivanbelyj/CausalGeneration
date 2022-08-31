using CausalGeneration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CausalGeneration.CausalEntity
{
    public class DescriptionEntityBuilder
    {
        private CausalResultModel<DescriptionEntityProperty> _model;
        public DescriptionEntityBuilder(CausalResultModel<DescriptionEntityProperty> model)
        {
            _model = model;
        }
        public virtual DescriptionEntity Build()
        {
            StringBuilder sb = new StringBuilder();

            // Описание абстрактной сущности всегда идет перед описнием реализации
            foreach (var node in _model.Nodes)
            {
                // Если дан абстрактный узел - он будет добавлен позже
                if (_model.IsAbstract(node))
                    continue;

                // Добавляем описание абстрактной сущности только в тот момент,
                // когда встречается ее реализация
                if (node is ImplementationNode<DescriptionEntityProperty> implNode)
                {
                    var abstractNode = _model.GetAbstractByImplmentation(implNode);
                    AddToSB(sb, abstractNode);
                }
                AddToSB(sb, node);
            }

            // Абстрактные узлы могут не иметь реализации, поэтому выбираются и
            // добавляются
            foreach ((var aNode, _) in _model.AbstractsAndImpls.Where(pair
                => pair.Value is null))
            {
                AddToSB(sb, aNode);
            }

            return new DescriptionEntity() { EntityDescription = sb.ToString() };

            void AddToSB(StringBuilder sb,
                CausalModelNode<DescriptionEntityProperty> node)
            {
                if (node.Value is not null && node.Value.Text is not null)
                {
                    string textToAdd = node.Value.Text.Trim();
                    sb.Append(textToAdd);
                    if (textToAdd != "")
                        sb.Append(" ");
                }
            }
        }
    }
}
