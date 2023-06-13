using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProcGen
{
    public class PremadeRoom : MonoBehaviour
    {
        public Vector3Int Size;
        Vector3Int Entrance;
        Vector3Int Exit;
        public int Index;

        // public List<Dir> Dirs;
        // Start is called before the first frame update
        private void OnDrawGizmosSelected()
        {
            Size.y = 1;
            var sizePos = transform.position;
            sizePos.y = 0;
            Gizmos.DrawWireCube(sizePos, Size);
            DrawDoor(Entrance);
            DrawDoor(Exit);
        }
        void DrawDoor(Vector3Int pos) {
            var exitPos = transform.position + pos;
            exitPos.y = 0;
            Gizmos.DrawWireCube(exitPos, new Vector3(1, 1, 1) * Generator.BlockSize);
        }

        public Room GetRoom()
        {
            var room = new Room(Size);
            room.GO = gameObject;
            room.Exit = Exit;
            room.Entrance = Entrance;
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
