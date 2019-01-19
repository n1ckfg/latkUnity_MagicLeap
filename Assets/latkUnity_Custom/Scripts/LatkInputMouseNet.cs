using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LatkInputMouseNet : MonoBehaviour {

	public LightningArtist latk;
    public LatkNetwork latkNetwork;
	public LatkInputButtons brushInputButtons;
	public float zPos = 1f;

	private void Awake() {
		if (latk == null) latk = GetComponent<LightningArtist>();
		if (brushInputButtons == null) brushInputButtons = GetComponent<LatkInputButtons>();
	}

	private void Update() {
        // draw
        if (Input.GetMouseButton(0) && GUIUtility.hotControl == 0) {
            Vector3 mousePos = Vector3.zero;
            mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, zPos));
            latk.target.transform.position = mousePos;
            latk.clicked = true;
            if (latk.clicked && !latk.isDrawing) {
                try {
                    List<Vector3> points = latk.getLastStroke().points;
                    latkNetwork.sendStrokeData(points);
                } catch (Exception e) {  };
            }
        } else {
			latk.clicked = false;

        }
	}

}
