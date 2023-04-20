using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TDTools.ResourceScanner {
    //TODO： 更新表示名字！
    public class FavoriatedItems{
        public List<ResourceReferenceInspector.SearchOption> List;

        public FavoriatedItems() { 
            List = new List<ResourceReferenceInspector.SearchOption>();
            Load();
        }

        public void Add(ResourceReferenceInspector.SearchOption option) {
            option.IsFavorite = true;
            List.Add(option);
            Save();
        }

        public void Remove(NodeType type, string id) {
            for (int i = 0; i < List.Count; i++) {
                if (List[i].type == type && List[i].ID.Equals(id)) {
                    List.RemoveAt(i);
                    break;
                }
            }
            Save();
        }

        void Save() {
            var stream = new FileStream($@"{Application.dataPath}\Editor\TDTools\ResourceReferenceScanner\Inspector\Favoriate.txt", FileMode.Create);
            using var sw = new StreamWriter(stream, System.Text.Encoding.UTF8);

            for (int i = 0; i < List.Count; i++) { 
                sw.WriteLine($"{List[i].ID}@{List[i].type}@{List[i].DisplayedName}");
            }
        }

        void Load() {
            if (!File.Exists($@"{Application.dataPath}\Editor\TDTools\ResourceReferenceScanner\Inspector\Favoriate.txt"))
                return;
            var stream = new FileStream($@"{Application.dataPath}\Editor\TDTools\ResourceReferenceScanner\Inspector\Favoriate.txt", FileMode.Open);
            using var sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            while (!sr.EndOfStream) {
                try {
                    string[] line = sr.ReadLine().Split('@');
                    List.Add(new ResourceReferenceInspector.SearchOption(0, line[2], line[0], (NodeType)Enum.Parse(typeof(NodeType), line[1]), true));
                } catch { 
                }
            }
        }
    }
}