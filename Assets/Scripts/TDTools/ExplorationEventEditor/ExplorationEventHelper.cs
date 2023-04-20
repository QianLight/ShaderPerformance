using System.Collections.Generic;
using UnityEngine;


namespace TDTools.ExplorationEventEditor {

    public class ExplorationEventHelper : MonoBehaviour {

        public enum TriggerType {
            NearTrigger,//ÇøÓò¿¿½ü¼¤»î
            LandingTrigger, //µÇµºµÇ´¬¼¤»î
            SpawnTrigger,//µº´¬³öÉú¼¤»î,
            PreReqTrigger//Ç°ÖÃÒÀÀµ¼¤»î
        }

        public int ID { 
            get => _id;

            set {
                _id = value;
                gameObject.name = $"Event ID {_id} {_comment}";
            }
        }
        private int _id;

        public string Comment {
            get => _comment;

            set {
                _comment = value;
                gameObject.name = $"Event ID {_id} {_comment}";
            }
        }
        private string _comment;

        public int Row;

        public int AreaID;

        public float Radius = 15f;

        public string EntityBinded = "";

        public string Island = "";

        public string Script = "";

        public TriggerType Trigger;

        public bool IsNew = false;

        public string Effect;

        public Dictionary<string, string> UnsupportedColumn = new Dictionary<string, string>();

        void OnEnable() {
            gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        }

        void OnDrawGizmos() {
#if UNITY_EDITOR
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 2);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, Radius);

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 10.0f, gameObject.name);
#endif
        }

        public Dictionary<string, string> ToDic() {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["ID"] = $"{ID}";
            result["Comment"] = Comment;
            result["SeaAreaID"] = $"{AreaID}";
            result["TriggerPos"] = $"{transform.position.x}={transform.position.y}={transform.position.z}={Radius}";
            result["XEntityBinded"] = EntityBinded;
            result["IslandBinded"] = Island;
            result["Script"] = Script?.Replace("\n", "");
            result["ActiveRule"] = $"{(int)Trigger + 1}";
            result["Effect"] = $"{Effect}";

            foreach (var pair in UnsupportedColumn) {
                result[pair.Key] = pair.Value;
            }
            return result;
        }

        //public string ToTableRow() {
        //    return $"{ID}\t{Comment}\t{AreaID}\t{transform.position.x}={transform.position.y}={transform.position.z}={Radius}\t{Island}\t{Script}\t{((int)Trigger)}\t{Effect}";
        //}
    }

}