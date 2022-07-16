using CausalGeneration;

CausalModel<string> model = new CausalModel<string>();
CausalModelNode<string> hobbyRoot = model.AddRootNode("Хобби", 0.9);

foreach (string hobbyName in new string[] { "рисование",
    "музыка", "ворлдбилдинг", "программирование",
    "писательство"
})
{
    var hobbyNode =
        new CausalModelNode<string>(new CausesNest(hobbyRoot.Id, 0.5), hobbyName);
    model.AddNode(hobbyNode);
}

var conlangHobby
    = new CausalModelNode<string>(new CausesNest(hobbyRoot.Id, 0.3), "Создание языков");
model.AddNode(conlangHobby);

foreach (string nodeValue in new string[] { "создал 1 язык",
    "разбирается в лингвистике", "говорит на нескольких языках"
})
{
    var node =
        new CausalModelNode<string>(new CausesNest(conlangHobby.Id, 0.7), nodeValue);
    model.AddNode(node);
}

foreach (CausalModelEdge edge in conlangHobby.CausesNest)
{
    Console.WriteLine(edge.CauseId + " " + edge.Probability);
    Console.WriteLine(hobbyRoot.Id + " " + 0.3);
}

model.GenerateModel();
foreach (var node in model._nodes)
{
    Console.WriteLine(node.ToString());
}
int endDebug = 0;
