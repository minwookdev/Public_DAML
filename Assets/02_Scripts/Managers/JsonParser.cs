using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CoffeeCat.Utils.JsonParser {
    // Load Json From Addressables 

    public class JsonParser {
        private JsonExtension jsonExtension = new JsonExtension();

        private string LoadJsonFromResourcesToString(string resourcesJsonPath) {
            return Resources.Load<TextAsset>(resourcesJsonPath).ToString();
        }

        public T[] LoadFromJsonInResources<T>(string resourcesJsonPath) {
            return jsonExtension.FromJson<T>(LoadJsonFromResourcesToString(resourcesJsonPath));
        }
    }

    public class JsonExtension {
        [Serializable]
        private class Wrapper<T> {
            public T[] array;
        }

        public T[] FromJson<T>(string jsonStr) {
            Wrapper<T> wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(jsonStr);
            return wrapper.array;
        }

        public string ToJson<T>(params T[] array) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.array = array;
            return JsonConvert.SerializeObject(wrapper, Formatting.Indented);
        }

        public string ToJsonByPath<T>(string path, params T[] array) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.array = array;
            string result = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            System.IO.File.WriteAllText(path, result);
            return result;
            //System.IO.File.WriteAllText(path, result, System.Text.Encoding.ASCII);
            //System.IO.File.WriteAllTextAsync(path, result).Wait();
        }

        public T[] FromJsonByPath<T>(string path) { 
            var readAllText = System.IO.File.ReadAllText(path);
            Wrapper<T> wrapper = JsonConvert.DeserializeObject<Wrapper<T>>(readAllText);
            return wrapper.array;
        }
    }
}
