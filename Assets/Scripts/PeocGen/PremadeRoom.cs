using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGen
{
    public class PremadeRoom : MonoBehaviour
    {
        public Vector3Int Size;
        public List<Vector3> Exits;

        [System.NonSerialized]
        public bool Editable = false;
        [System.NonSerialized]
        public GameObject MergeTarget;
        // public List<Dir> Dirs;
        // Start is called before the first frame update
        private void OnDrawGizmosSelected()
        {
            Size.y = 1;
            var sizePos = transform.position;
            sizePos.y = 0;
            Gizmos.DrawWireCube(sizePos, Size);
            foreach (var exit in Exits)
            {
                var exitPos = transform.position + exit;
                exitPos.y = 0;
                Gizmos.DrawWireCube(exitPos, new Vector3(4, 4, 4));
            }

        }

        public Room GetRoom()
        {
            var room = new Room(Size);
            room.GO = gameObject;
            return room;

        }


        // Vector3 GetWorldSize()
        // {
        //     return transform.TransformVector((Vector3)Size);
        // }
        // Vector3 GetWorldEnterance()
        // {
        //     return transform.TransformVector((Vector3)Enterance);
        // }
    }
}
