using UnityEngine;

namespace RandomDungeonWithBluePrint
{
    // Room이 존재하지 않는 Section에 좌표를 찍어 길의 연결부위를 만듦
    public class Relay
    {
        public int Section { get; set; }
        public Vector2Int Point { get; set; }

        public Relay(int section, Vector2Int point)
        {
            Section = section;
            Point = point;
        }
    }
}
