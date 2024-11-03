using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public static class SaveLoadManager
{
    //private const string levelPath = "/Resources";

    public static void SaveAnswers(List<Answer> answers)
    {
        if(File.Exists(Application.persistentDataPath + "/Answers.json"))
            File.Copy(Application.persistentDataPath + "/Answers.json", Application.persistentDataPath + "/AnswersBackup.json", true);

        string serializedAnswers = JsonConvert.SerializeObject(answers);
        File.WriteAllText(Application.persistentDataPath + "/Answers.json", serializedAnswers);

        Debug.Log("Answers Saved");
    }

    public static List<Answer> LoadAnswers()
    {
        string answers = null;
        try
        {
            answers = File.ReadAllText(Application.persistentDataPath + "/Answers.json");
        }
        catch
        {
            try
            {
                answers = File.ReadAllText(Application.persistentDataPath + "/AnswersBackup.json");
            }
            catch
            {
                return new List<Answer>();
            }
            SaveAnswers(JsonConvert.DeserializeObject<List<Answer>>(answers));
        }
        return JsonConvert.DeserializeObject<List<Answer>>(answers);
    }


    public static void ChangeAnswersFromWindows()
    {
        string path = Application.persistentDataPath;
        path = path.Replace("/files", "/WindowsAnswers.json");

        string windowsAnswers = File.ReadAllText(path);
        SaveAnswers(JsonConvert.DeserializeObject<List<Answer>>(windowsAnswers));

        File.Delete(path);
    }

}
