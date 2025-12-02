using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoffeeCat.Utils.Defines;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace CoffeeCat.Editor {
    public class CoffeeCatEditor : UnityEditor.Editor {
        private static readonly string csvFolderPath = Application.dataPath + "/08_Entities/CSV";
        private static readonly string jsonFolderPath = Application.dataPath + "/08_Entities/JSON";
        private static readonly string jsonResourcePath = Application.dataPath + "/Resources/Entity/Json";
        private static readonly char[] CSV_TEXT_DELIMITERS = { ';', ',' };
        
        [MenuItem("CoffeeCat/Remove PlayerPrefs", false, 2)]
        public static void RemoveAllPlayerPrefs() {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("CoffeeCat/Captrue GameView/1X")]
        public static void Capture1XScreenShot() {
            CaptureGameView(1);
        }

        [MenuItem("CoffeeCat/Captrue GameView/2X")]
        public static void Capture2XScreenShot() {
            CaptureGameView(2);
        }

        [MenuItem("CoffeeCat/Captrue GameView/3X")]
        public static void Capture3XScreenShot() {
            CaptureGameView(3);
        }

        private static void CaptureGameView(int size) {
            string imgName = "IMG-" + DateTime.Now.Year.ToString() +
                             DateTime.Now.Month.ToString("00") +
                             DateTime.Now.Day.ToString("00") + "-" +
                             DateTime.Now.Hour.ToString("00") +
                             DateTime.Now.Minute.ToString("00") +
                             DateTime.Now.Second.ToString("00") + ".png";
            ScreenCapture.CaptureScreenshot((Application.dataPath + "/" + imgName), size);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        [MenuItem("CONTEXT/Component/Move To Top", priority = 2)]
        private static void MoveToTop(MenuCommand menuCommand) {
            Component component = menuCommand.context as Component;
            if (component == null) return;
            // Move the component to the top of the list
            for (int i = component.gameObject.GetComponents<Component>().Length - 1; i >= 0; i--) {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            }
        }

        [MenuItem("Window/Toggle Inspector Lock %q")] // %q is Meaning 'Ctrl + Q'
        private static void ToggleLock() {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            
            // Ensure the Inspector is updated to reflect the lock state change
            EditorWindow.focusedWindow.Repaint();

            // var delta = new Vector2(0.15f, 0.15f);
            // var distanceSqr = Vector2.SqrMagnitude(delta);
            // CatLog.Log("Disatance Sqrt: " + distanceSqr.ToString());
        }
        
        #region CSV To Json

        [MenuItem("CoffeeCat/CSV/Convert All CSV to Encrypted JSON")]
        private static void ConvertAllCSVToEncryptedJSON() {
            // delete all legacy json files
            DeleteAllFilesInDirectory(jsonFolderPath);
            DeleteAllFilesInDirectory(jsonResourcePath);
            // convert all csv files to json
            ConvertAllCSVToJson();
            JsonToResourcePathWithEncrypt();
        }
        
        // [MenuItem("CoffeeCat/CSV/Convert All CSV to JSON")]
        private static void ConvertAllCSVToJson()
        {
            // Lock 파일 존재 여부 확인 (수정중인 파일이 존재하는지 체크)
            if (Directory.GetFiles(csvFolderPath, ".~lock.*.csv#").Any() || Directory.GetFiles(csvFolderPath, "*.csv#").Any())
            {
                CatLog.WLog("Lock file found. CSV Conversion aborted.");
                return;
            }

            // CSV 파일 목록 가져오기
            int totalFilesCount = 0;
            string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");
            foreach (string csvFilePath in csvFiles)
            {
                // 각 CSV 파일을 JSON으로 변환
                string jsonFilePath =
                    Path.Combine(jsonFolderPath, Path.GetFileNameWithoutExtension(csvFilePath) + ".json");
                ConvertCsvToJson(csvFilePath, jsonFilePath);
                totalFilesCount++;
            }

            CatLog.Log($"All CSV to JSON conversion completed. (Total: {totalFilesCount.ToString()} Processed.)");
        }

        // [MenuItem("CoffeeCat/CSV/Json To Resource Path")]
        private static void JsonToResourcePathWithEncrypt()
        {
            // JSON 파일 목록 가져오기
            int totalProcessFileCount = 0;
            string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json");
            foreach (string jsonFilePath in jsonFiles)
            {
                // JSON 파일을 읽어서 암호화
                string json = File.ReadAllText(jsonFilePath);
                string encryptedJson = Cryptor.Encrypt2(json);

                // 암호화된 JSON 파일을 Resources 폴더로 저장
                string encryptedJsonFilePath =
                    Path.Combine(jsonResourcePath, Path.GetFileNameWithoutExtension(jsonFilePath) + ".enc");
                File.WriteAllText(encryptedJsonFilePath, encryptedJson);
                totalProcessFileCount++;
            }

            CatLog.Log($"All JSON to Copy Resource Path With Encrpyt Completed. (Total: {totalProcessFileCount.ToString()} Processed.)");
        }
        
        private static void ConvertCsvToJson(string csvPath, string jsonPath)
        {
            var records = new List<Dictionary<string, string>>();

            using (var reader = new StreamReader(csvPath))
            {
                bool isFirstLine = true;
                string[] headers = null;
                int lineCount = 0;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineCount++;

                    if (isFirstLine)
                    {
                        // 첫 번째 줄은 헤더로 저장
                        headers = ParseCsvLine(line);
                        isFirstLine = false;
                        continue;
                    }

                    // 첫 세 줄 건너뛰기 (4번째 줄부터 읽기)
                    if (lineCount <= 3)
                    {
                        continue;
                    }

                    var values = ParseCsvLine(line);
                    var record = new Dictionary<string, string>();

                    for (int i = 1; i < headers.Length; i++)
                    {
                        record[headers[i]] = values[i];
                    }

                    records.Add(record);
                }
            }

            // JSON으로 변환
            string json = JsonConvert.SerializeObject(records, Formatting.Indented);

            // JSON 파일로 저장
            File.WriteAllText(jsonPath, json);
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                    {
                        // 이스케이프된 큰따옴표
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (CSV_TEXT_DELIMITERS.Contains(line[i]) && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(line[i]);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }
        
        private static void DeleteAllFilesInDirectory(string path) {
            if (Directory.Exists(path)) {
                try {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files) {
                        File.Delete(file);
                        // CatLog.Log($"fileName: {file.ToString()}");
                    }
                    CatLog.Log($"Deleted All Files -> '{path}'");
                }
                catch (IOException e) {
                    CatLog.ELog($"IOException Occurs -> {e.Message}");
                }
            }
            else {
                CatLog.WLog($"Invalid Path -> '{path}'");
            }
        }
        
        #endregion
        
        [MenuItem("CoffeeCat/Remove SaveData")]
        private static void RemoveUserSaveData() {
            if (!File.Exists(Defines.USER_DATA_PATH)) {
                return;
            }
            File.Delete(Defines.USER_DATA_PATH);
            CatLog.Log("User Data Removed.");
        }
    }
}