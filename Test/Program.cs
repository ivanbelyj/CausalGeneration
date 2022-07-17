using System.Text.Json;
using System.Text.Json.Serialization;
using CausalGeneration;
using System.Text.Encodings.Web;
using System.Text.Unicode;

CausalModel<string> model = new CausalModel<string>();
CausalModelNode<string> hobbyRoot = model.AddRootNode("Хобби", 0);

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

JsonSerializerOptions options = new JsonSerializerOptions() {
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица
};
Serialize(model, "model");

model.Generate();

Serialize(model, "generated_model");

Console.ReadKey(true);

void Serialize<TNodeValue>(CausalModel<TNodeValue> model, string fileName)
{
    string jsonString = JsonSerializer.Serialize(model, options);
    if (!fileName.EndsWith(".json"))
    {
        fileName += ".json";
    }
    File.WriteAllText(fileName, jsonString);
    // Console.WriteLine(File.ReadAllText(fileName));
}