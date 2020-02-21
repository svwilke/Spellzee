namespace RetroBlitDemoBrickBust
{
    using UnityEngine;

    /// <summary>
    /// Extend power up, paddle grows wider
    /// </summary>
    public class PowerUpExtend : PowerUp
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">Position</param>
        public PowerUpExtend(Vector2i pos) : base("E", C.COLOR_BLUE_BRICK, pos)
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

            paddle.Extend();

            // Make sure all balls stuck to the paddle are released
            var balls = game.Level.Balls;
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].StuckToPaddle = false;
            }
        }
    }
}
