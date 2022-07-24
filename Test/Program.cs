using System.Text.Json;
using System.Text.Json.Serialization;
using CausalGeneration;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using CausalGeneration.Nests;
using CausalGeneration.Groups;
using CausalGeneration.Edges;

CausalModel<string> model = new CausalModel<string>();
// CausalModelNode<string> hobbyRoot = model.AddRootNode("Хобби", 0.9);
CausalModelNode<string> hobbyRoot = model.AddNode(new CausesNest(null, 0.9), "Хобби");
model.AddRoot(hobbyRoot.Id);

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
CausalModelNode<string> raceNode = new CausalModelNode<string>(new CausesNest(null, 1), "Раса");
model.AddRoot(raceNode.Id);
model.AddVariantsGroup(raceNode, "тшэайская", "мэрайская", "мйеурийская", "эвойская");

JsonSerializerOptions options = new JsonSerializerOptions()
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица
};
ToFile(model, "model");

ValidationResult res = model.Generate();
if (!res.Succeeded)
    throw new Exception("Ошибки валидации.");

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