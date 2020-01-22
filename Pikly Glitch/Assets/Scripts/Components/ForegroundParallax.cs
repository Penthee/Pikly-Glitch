using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using UnityEngine;

public class ForegroundParallax : MonoBehaviour {

    [Range(0.01f, 2f)] 
    public float strength = 1, range = 0.5f; 
    public float updateRange = 15, updateInterval = 0.1f;
    
    static Transform _cam;
    
    Transform _t;
    Vector2 _camPosLastFrame, _newPos, _startPosition, _delta;

    void Awake() {
        if (!_cam)
            if (Camera.main != null)
                _cam = Camera.main.transform;

        _t = transform;
        _startPosition = _t.position;
        
        InvokeRepeating("UpdatePosition", updateInterval, updateInterval);    
    }
//get velocity of camera
//move against velocity * strength
//clamp max movement from original position
    void UpdatePosition() {
        if (Vector2.Distance(_cam.position, _t.position) > updateRange)
            return;
        
        _delta = (_camPosLastFrame - (_cam ? _cam : _t).position.To2DXY());
        
        _newPos = (Time.deltaTime * strength * _delta) + _t.position.To2DXY();

        if (Vector2.Distance(_newPos, _startPosition) < range)
            _t.position = _newPos; 
        
        _camPosLastFrame = _cam.position;
    }
}
