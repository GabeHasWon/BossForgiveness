using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace BossForgiveness.Common.Camera;

internal class ZoomModifier(NPC track, int timeToZoom, int pauseTime, int timeToUnzoom, float zoom) : ICameraModifier
{
    public string UniqueIdentity { get; private set; }
    public bool Finished { get; private set; }

    readonly int _timeToZoom = timeToZoom;
    readonly int _timeToUnzoom = timeToUnzoom;
    readonly int _timeToPause = pauseTime;
    readonly NPC _trackedNPC = track;
    readonly float _zoom = zoom;

    int _lifeTime = 0;

    public void Update(ref CameraInfo cameraPosition)
    {
        _lifeTime++;

        if (_lifeTime < _timeToZoom)
        {
            float factor = _lifeTime / (float)_timeToZoom;

            cameraPosition.CameraPosition = Vector2.Lerp(cameraPosition.CameraPosition, _trackedNPC.Center - Main.ScreenSize.ToVector2() / 2f, factor);
            Main.GameZoomTarget = MathHelper.Lerp(_zoom, 2f, factor);
        }
        else if (_lifeTime < _timeToZoom + _timeToPause)
        {
            cameraPosition.CameraPosition = _trackedNPC.Center - Main.ScreenSize.ToVector2() / 2f;
            Main.GameZoomTarget = 2f;
        }
        else
        {
            int time = _lifeTime - _timeToZoom - _timeToPause;
            float factor = time / (float)_timeToUnzoom;
            cameraPosition.CameraPosition = Vector2.Lerp(_trackedNPC.Center - Main.ScreenSize.ToVector2() / 2f, cameraPosition.OriginalCameraPosition, factor);
            Main.GameZoomTarget = MathHelper.Lerp(2f, _zoom, factor);
        }

        if (_lifeTime > _timeToZoom + _timeToPause + _timeToUnzoom)
            Finished = true;
    }
}
