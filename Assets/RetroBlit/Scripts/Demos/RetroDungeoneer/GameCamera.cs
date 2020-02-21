namespace RetroBlitDemoRetroDungeoneer
{
    using UnityEngine;

    /// <summary>
    /// Game camera, follows the player around with some lag
    /// </summary>
    public class GameCamera
    {
        private Vector2 mPos;
        private float mSpeed = 0.1f;

        /// <summary>
        /// Set position based on entity. This immediately snaps the camera to the given entities position.
        /// </summary>
        /// <param name="entity">Entity</param>
        public void SetPos(EntityID entity)
        {
            var e = EntityStore.Get(entity);
            if (e == null)
            {
                return;
            }

            mPos = new Vector2(e.pos.x * RB.SpriteSize(0).width, e.pos.y * RB.SpriteSize(0).height);
        }

        /// <summary>
        /// Get current position
        /// </summary>
        /// <returns>Position</returns>
        public Vector2i GetPos()
        {
            return mPos;
        }

        /// <summary>
        /// Follow the given entity
        /// </summary>
        /// <param name="entity">Entity to follow</param>
        public void Follow(EntityID entity)
        {
            var e = EntityStore.Get(entity);
            if (e == null)
            {
                return;
            }

            mPos.x += ((e.pos.x * RB.SpriteSize(0).width) - mPos.x) * mSpeed;
            mPos.y += ((e.pos.y * RB.SpriteSize(0).height) - mPos.y) * mSpeed;
        }

        /// <summary>
        /// Apply the camera position
        /// </summary>
        public void Apply()
        {
            Vector2i pos = mPos;
            pos.x -= (RB.DisplaySize.width / 2) - (RB.SpriteSize(0).width / 2);
            pos.y -= (RB.DisplaySize.height / 2) - (RB.SpriteSize(0).height / 2);

            RB.CameraSet(pos);
        }
    }
}
