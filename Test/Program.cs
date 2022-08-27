using System.Text.Json;
using System.Text.Json.Serialization;
using CausalGeneration;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using CausalGeneration.Nests;
using CausalGeneration.Tests;

Test1();

bool endDebug = true;

void Test1()
{
    CausalModel<string> model = new CausalModel<string>();
    CausalModelNode<string> hobbyRoot = model.AddNode(new ProbabilityNest(null, 0.9),
        "Хобби");

    foreach (string hobbyName in new string[] { "Рисование",
        "Музыка", "Ворлдбилдинг", "Программирование", "Писательство" })
    {
        model.AddNode(new ProbabilityNest(hobbyRoot.Id, 0.5), hobbyName);
    }

    var conlangHobby = model.AddNode(new ProbabilityNest(hobbyRoot.Id, 1),
        "Создание языков");

    foreach (string nodeValue in new string[] { "Создал 1 язык",
        "Разбирается в лингвистике", "Говорит на нескольких языках" })
    {
        model.AddNode(new ProbabilityNest(conlangHobby.Id, 0.4), nodeValue);
    }

    // Раса напрямую связана с бытием существа,
    // представляет собой абстрактную сущность, реализуемую одним из конкретных вариантов
    //CausalModelNode<string> raceNode = new CausalModelNode<string>(
    //    new ProbabilityNest(null, 1), "Раса");
    CausalModelNode<string> raceNode = NodeUtils.CreateNode(1, "Раса", null);
    model.AddVariantsGroup(raceNode, "тшэайская", "мэрайская", "мйеурийская", "эвойская");

    Generate(model, "new-model");

    //JsonSerializerOptions options = new JsonSerializerOptions()
    //{
    //    WriteIndented = true,
    //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица
    //};

    //Generate(model, "model");
    //var deserializedModel = CausalModel<string>.FromJson(model.ToJson());
    //if (deserializedModel == null)
    //    throw new Exception("Не удалось десериализовать модель.");
    //Generate(deserializedModel, "deserialized-model");
}

void Generate<TNodeValue>(CausalModel<TNodeValue> model, string fileName)
{
    ToFile(model, fileName);

    ValidationResult res = model.Generate();
    if (!res.Succeeded)
        throw new Exception("Ошибки валидации.");

    string formatString = ".json";
    if (fileName.Contains(formatString))
    {
        fileName = fileName.Replace(formatString, "");
    }
    ToFile(model, fileName + ".generated");
}

void ToFile<TNodeValue>(CausalModel<TNodeValue> model, string fileName)
{
    string jsonString = model.ToJson(true);
    if (!fileName.EndsWith(".json"))
    {
        fileName += ".json";
    }
    File.WriteAllText(fileName, jsonString);
}