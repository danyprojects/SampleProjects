using System.Xml;

public static class ScriptConfigs
{
    private static readonly string XmlPath = "Assets/Editor/script_config.xml";

    public static string ServerFieldsPath = "";

    public static void LoadConfigs()
    {
        XmlDocument doc = new XmlDocument();
        doc.PreserveWhitespace = false;
        doc.Load(XmlPath);

        var mapGenerator = doc.GetElementsByTagName("map_generator")[0];
        for (int i = 0; i < mapGenerator.ChildNodes.Count; i++)
        {
            if (mapGenerator.ChildNodes[i].Name == "server_fields_path")
                ServerFieldsPath = mapGenerator.ChildNodes[i].InnerText;
        }
    }
}

