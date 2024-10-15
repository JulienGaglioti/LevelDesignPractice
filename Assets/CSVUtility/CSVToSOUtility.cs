using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class CSVToSOUtility : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private IdDatabaseSO _allElements;
    [SerializeField] private TextAsset _textAssetData;
    [SerializeField] private string _path = "Assets/CSVUtility/Example/Cards/";//NewSO.asset"; // la cartella deve essere già presente
    private ExampleCardSO _castedSO;
    private bool _logEnable = false;
    int _numberOfUpdatedEntries;
    int _numberOfCreatedEntries;

    [Button]
    private void FillData()
    {
        // legge il file e lo suddivide per ogni riga. Attenzione a non inserire break line involontari nella sheet con CTRL+ENTER
        _numberOfUpdatedEntries = 0;
        _numberOfCreatedEntries = 0;
        string[] rows = _textAssetData.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        EditorUtility.SetDirty(_allElements);
        _allElements.MakeDictionary();

        // parte da 1 perchè la riga 0 contiene gli header
        for (int i = 1; i < rows.Length; i++)
        {
            // suddivide la singola riga in più celle e ripristina le virgole precedentemente rimosse nella sheet con la funzione ARRAYFORMULA
            string[] data = rows[i].Split(new string[] { "," }, System.StringSplitOptions.None);
            string[] dataWithCommas = new string[data.Length];
            for (int j = 0; j < data.Length; j++)
            {
                dataWithCommas[j] = data[j].Replace(';', ',');
                dataWithCommas[j] = dataWithCommas[j].Replace("\"", "");
                dataWithCommas[j] = ReplaceWithHTMLTags(dataWithCommas[j]);
            }


            if (_allElements.Dictionary.TryGetValue(dataWithCommas[0], out IdSO scriptableObject))
            {
                // elemento già presente nel database: viene aggiornato             
                if (_logEnable)
                {
                    print("Updating SO: " + dataWithCommas[0]);
                }
                UpdateScriptableObject(scriptableObject, dataWithCommas);
                _numberOfUpdatedEntries++;
            }
            else if (rows[i] != "")
            {
                // elemento già presente nel database: viene creato 
                if (_logEnable)
                {
                    print("Creating new SO: " + dataWithCommas[0]);
                }

                IdSO presentScriptableObject = (IdSO)AssetDatabase.LoadAssetAtPath(_path + dataWithCommas[0] + ".asset", typeof(IdSO));
                if (presentScriptableObject != null)
                {
                    // elemento già esistente ma non aggiunto al database 
                    if (_logEnable)
                    {
                        print("Updating SO: " + dataWithCommas[0]);
                    }

                    _allElements.List.Add(presentScriptableObject);
                    UpdateScriptableObject(presentScriptableObject, dataWithCommas);
                    _numberOfUpdatedEntries++;
                }
                else
                {
                    // elemento non presente nel database: viene creato 
                    if (_logEnable)
                    {
                        print("Creating new SO: " + dataWithCommas[0]);
                    }

                    ExampleCardSO newScriptableObject = ScriptableObject.CreateInstance<ExampleCardSO>();
                    newScriptableObject.Id = dataWithCommas[0];
                    AssetDatabase.CreateAsset(newScriptableObject, _path + "NewSO.asset");
                    AssetDatabase.RenameAsset(_path + "NewSO.asset", dataWithCommas[0]);

                    _allElements.List.Add(newScriptableObject);
                    UpdateScriptableObject(newScriptableObject, dataWithCommas);
                    _numberOfCreatedEntries++;
                }
            }
        }

        if (_logEnable)
        {
            print("Updated " + _numberOfUpdatedEntries + " objects");
            print("Created " + _numberOfCreatedEntries + " objects");
        }

        _allElements.MakeDictionary();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void UpdateScriptableObject(IdSO scriptableObject, string[] dataArray)
    {
        // in questo metodo, specifico come aggiornare ogni campo dello scriptable object

        _castedSO = (ExampleCardSO)scriptableObject;
        EditorUtility.SetDirty(_castedSO);

        _castedSO.Name = dataArray[1];
        _castedSO.Cost = int.Parse(dataArray[2]);
    }

    private string ReplaceWithHTMLTags(string input)
    {
        string newString = input;
        newString = newString.Replace("(Triangle)", "<sprite name=Triangle>");
        newString = newString.Replace("(Heart)", "<sprite name=Heart>");
        newString = newString.Replace("(Star)", "<sprite name=Star>");
        newString = newString.Replace("(Diamond)", "<sprite name=Diamond>");
        newString = newString.Replace("(Jolly)", "<sprite name=Jolly>");

        newString = newString.Replace("(N)", "<sprite name=N>");
        newString = newString.Replace("(B)", "<sprite name=B>");
        newString = newString.Replace("(G)", "<sprite name=G>");
        newString = newString.Replace("(Y)", "<sprite name=Y>");
        newString = newString.Replace("(R)", "<sprite name=R>");

        newString = newString.Replace("(C)", "<sprite name=Coin>");
        newString = newString.Replace("(E)", "<sprite name=Energy>");

        return newString;
    }
#endif
}
