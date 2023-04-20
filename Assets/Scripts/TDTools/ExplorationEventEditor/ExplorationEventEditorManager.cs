using System.Collections.Generic;
using UnityEngine;

namespace TDTools.ExplorationEventEditor {

    [System.Serializable]
    public class ExplorationEventEditorManager : MonoBehaviour {
        public int AreaID { 

            get => _areaID;

            set {
                _areaID = value;
                //for (int i = 0; i < Events.Count; i++)
                //    Events[i].AreaID = AreaID;
            }
        }
        private int _areaID;

        [SerializeField]
        public List<ExplorationEventHelper> Events = new List<ExplorationEventHelper>();
    }
}