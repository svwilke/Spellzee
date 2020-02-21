namespace RetroBlitDemoBrickBust
{
    using UnityEngine;

    /// <summary>
    /// Catch powerup, ball sticks to paddle until released
    /// </summary>
    public class PowerUpCatch : PowerUp
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">Position</param>
        public PowerUpCatch(Vector2i pos) : base("C", C.COLOR_BROWN_BRICK, pos)
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

            paddle.Catch();
        }
    }
}
