namespace RetroBlitDemoBrickBust
{
    using UnityEngine;

    /// <summary>
    /// Laser powerup, paddle gets two laser turrets
    /// </summary>
    public class PowerUpLaser : PowerUp
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">Position</param>
        public PowerUpLaser(Vector2i pos) : base("L", C.COLOR_PINK_BRICK, pos)
        {
        }

        /// <summary>
        /// Activate the power up
        /// </summary>
        protected override void Activate()
        {
            base.Activate();

            BrickBustGame game = (BrickBustGame)RB.Game;
            var paddle = game.Level.Paddle;

            paddle.Laser();

            // Make sure all balls stuck to the paddle are released
            var balls = game.Level.Balls;
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].StuckToPaddle = false;
            }
        }
    }
}