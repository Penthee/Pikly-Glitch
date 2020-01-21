using Pikl.Profile;
using UnityEngine;

namespace Pikl.Components
{
    public class FaceInput2D : FaceVector2D
    {

        public Player.Player player;
        float smoothValue;

        internal override void Start()
        {
            base.Start();

            //smoothValue = ProfileMgr.I.profile.stickSmoothing.Value ? 0.35f : origRotateSpeed;
        }

        internal override Vector3 GetDir()
        {
            //if (Input.PlayerOneControlScheme.Name == "KeyboardAndMouse") {
                //rotateSpeed = origRotateSpeed;
                //return player.input.IsAiming ? (player.input.MoveAxis.magnitude != 0 ? player.input.MoveAxis : player.input.MouseDir.normalized) : lastDir;

                /*if (player.knife.swiping)
                    return player.input.MouseDirFromWeapon.normalized;

                if (player.input.MoveAxis.magnitude != 0) {
                    if (player.input.AimAxis) {
                        return player.input.MouseDirFromWeapon.normalized;
                    } else {
                        rotateSpeed = origRotateSpeed;
                        return player.input.MoveAxis;
                    }
                } else {
                    //if not moving or aiming, face last direction, if aiming, face aim direction
                    return !player.input.AimAxis ? lastDir : player.input.MouseDirFromWeapon.normalized;
                }*/

            /*} else {
                rotateSpeed = smoothValue;

                //if moving, then face move direction, if moving and aiming, face aim direction
                if (player.input.MoveAxis.magnitude != 0) {
                    if (player.input.StickDir.magnitude == 0) {
                        rotateSpeed = origRotateSpeed;
                        return player.input.MoveAxis;
                    } else {
                        return player.input.StickDir.normalized;
                    }
                } else {
                    //if not moving or aiming, face last direction, if aiming, face aim direction
                    return !player.input.AimAxis && player.input.StickDir.magnitude == 0 ? lastDir : player.input.StickDir.normalized;
                }
            }*/

        }
    }
}
