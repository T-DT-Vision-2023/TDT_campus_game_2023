namespace Code.util
{
    public class SendStruct
    {
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public byte[] Img { get; set; }
        public double TimeStamp { get; set; }
        public double EnemyHp { get; set; }
        public double MyHp { get; set; }
        public int RestBullets { get; set; }
        public int RestTime { get; set; }
        public int BuffOverTime { get; set; }
    }
}