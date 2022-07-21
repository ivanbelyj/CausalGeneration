using System.Text.Json;
using System.Text.Json.Serialization;
using CausalGeneration;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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

// Тест групп

// Раса напрямую связана с бытием существа,
// представляет собой абстрактную сущность, реализуемую конкретным вариантом
CausalModelNode<string> raceRoot = model.AddRootNode("Раса", 1);
NodesGroup<string> raceGroup = new VariantsGroup<string>(raceRoot.Id, model);
model.Groups.Add(raceGroup);
foreach (string nodeValue in new string[] { "тшэайская", "мэрайская",
    "мйеурийская", "эвойская"})
{
    var node =
        new CausalModelNode<string>(new CausesNest(raceRoot.Id, 1), nodeValue);
    model.AddNode(node);
    node.AddToGroup(raceGroup);
}

JsonSerializerOptions options = new JsonSerializerOptions()
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица
};
ToFile(model, "model");

model.Generate();

ToFile(model, "generated_model");

//var deserializedModel = CausalModel<string>.FromJson(model.ToJson());
//deserializedModel?.Generate();
//if (deserializedModel != null)
//    ToFile(deserializedModel, "generated_model2");

// Console.ReadKey(true);
int endDebug = 0;

void ToFile<TNodeValue>(CausalModel<TNodeValue> model, string fileName)
{
    string jsonString = model.ToJson(true);
    if (!fileName.EndsWith(".json"))
    {
        fileName += ".json";
    }
    File.WriteAllText(fileName, jsonString);
}