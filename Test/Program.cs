using CausalGeneration;
using CausalGeneration.CausesExpressionTree;
using CausalGeneration.Edges;
using CausalGeneration.Nests;
using CausalGeneration.Tests;

Test1();

bool endDebug = true;

void Test1()
{
    // Следующая модель генерации персонажа используется исключительно для теста и демонстрации
    CausalModel<string> model = new CausalModel<string>();
    CausalModelNode<string> hobbyRoot = model.AddNode(new ProbabilityNest(null, 0.9),
        "Хобби");

    foreach (string hobbyName in new string[] { "Рисование",
        "Музыка", "Ворлдбилдинг", "Программирование", "Писательство" })
    {
        model.AddNode(new ProbabilityNest(hobbyRoot.Id, 0.5), hobbyName);
    }

    var educationNode = NodeUtils.CreateNode(1, "Образование", null);
    model.AddVariantsGroup(educationNode, "компьютерные науки", "история", "математика");
    var linguisticsNode = NodeUtils.CreateImplementation(educationNode.Id, 2, "лингвистика");
    model.Nodes.Add(linguisticsNode);

    var conlangHobby = model.AddNode(new ProbabilityNest(hobbyRoot.Id, 0.3),
        "Создание языков");

    foreach (string nodeValue in new string[] {  "Разбирается в лингвистике",
        "Говорит на нескольких языках" })
    {
        // model.AddNode(new ProbabilityNest(conlangHobby.Id, 0.4), nodeValue);
        // Если персонаж - лингвист, вероятность повышается
        var linguisticsEdge = new ProbabilityEdge(1, linguisticsNode.Id);
        // Хобби создание языков тоже повышает вероятность
        var conlangEdge = new ProbabilityEdge(0.5, conlangHobby.Id);
        var node = NodeUtils.CreateNodeWithOr(nodeValue, linguisticsEdge, conlangEdge);
        model.Nodes.Add(node);
    }

    // Раса напрямую связана с бытием существа,
    // представляет собой абстрактную сущность, реализуемую одним из конкретных вариантов
    //CausalModelNode<string> raceNode = new CausalModelNode<string>(
    //    new ProbabilityNest(null, 1), "Раса");
    CausalModelNode<string> raceNode = NodeUtils.CreateNode(1, "Раса", null);
    model.AddVariantsGroup(raceNode, "тшэайская", "мэрайская", "мйеурийская", "эвойская");

    // Generate(model, "new-model");

    //JsonSerializerOptions options = new JsonSerializerOptions()
    //{
    //    WriteIndented = true,
    //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица
    //};

    Generate(model, "new-model");
    //string modelStr = ToFile(model, "serialized-model");
    //var deserializedModel = CausalModel<string>.FromJson(modelStr);
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

string ToFile<TNodeValue>(CausalModel<TNodeValue> model, string fileName)
{
    string jsonString = model.ToJson(true);
    if (!fileName.EndsWith(".json"))
    {
        fileName += ".json";
    }
    File.WriteAllText(fileName, jsonString);
    return jsonString;
}
