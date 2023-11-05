namespace Code.util
{
    public class KalmanFilter
    {
        public float Q { get; set; } // 过程噪声协方差
        public float R { get; set; } // 测量噪声协方差
        public float P { get; private set; } // 估计误差协方差
        public float X { get; private set; } // 估计值

        public KalmanFilter(float q, float r, float initialP, float initialX)
        {
            Q = q;
            R = r;
            P = initialP;
            X = initialX;
        }

        public float Update(float measurement)
        {
            // 预测更新
            P = P + Q;

            // 测量更新
            var K = P / (P + R); // 卡尔曼增益
            X = X + K * (measurement - X);
            P = (1 - K) * P;

            return X;
        }
    }
}