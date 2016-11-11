namespace TT_Network_Framework
{
    public class PlayerData: ClientData
    {
        public float xPosition = 0.0f;
        public float yPosition = 0.0f;
        public float zPosition = 0.0f;

        public float xRotation = 0.0f;
        public float yRotation = 0.0f;
        public float zRotation = 0.0f;

        public float powerInput = 0.0f;
        public float turnInput = 0.0f;

        public bool isJumping = false;
    }
}