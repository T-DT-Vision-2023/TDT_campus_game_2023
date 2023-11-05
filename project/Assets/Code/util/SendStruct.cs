namespace Code.util
{
    public class SendStruct
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public byte[] Img { get; set; }
        public float TimeStamp { get; set; }
        public float RedHp { get; set; }
        public float BlueHP { get; set; }
        public int RestBullets { get; set; }
        public int RestTime { get; set; }
        public int BuffOverTime { get; set; }
    }
}