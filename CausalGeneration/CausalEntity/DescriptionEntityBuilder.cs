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
            foreach (var node in _model.Nodes)
            {
                if (node.Value is null || node.Value.Text is null)
                    continue;
                string textToAdd = node.Value.Text.Trim();
                sb.Append(textToAdd);
                if (textToAdd != "")
                    sb.Append(" ");
            }
            return new DescriptionEntity() { EntityDescription = sb.ToString() };
        }
    }
}
