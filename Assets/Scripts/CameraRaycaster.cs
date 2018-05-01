using System;
using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    public Layer[] layerPriorities = {
        Layer.Walkable,
        Layer.Unwalkable
    };

    float distanceToBackground = 100f;
    Camera viewCamera;
	Grid grid;
	bool pollHit;

    RaycastHit m_hit;
    public RaycastHit hit {
        get { return m_hit; }
    }

    Layer m_layerHit;
    public Layer layerHit {
        get { return m_layerHit; }
    }
    
    Node m_nodeHit;
    public Node nodeHit {
    	get { return m_nodeHit; }
    }

	public delegate void OnLayerChange(Layer layerHit);
	public event OnLayerChange layerChangeObservers;
	private void TriggerLayerChangeObservers(Layer layerHit) {
		if (layerChangeObservers != null) {
			layerChangeObservers(layerHit);
		}
	}
	public delegate void OnNodeHoverChange(Node nodeHit);
	public event OnNodeHoverChange nodeHoverChangeObservers;
	private void TriggerNodeHoverChangeObservers(Node nodeHit) {
		if (nodeHoverChangeObservers != null) {
			nodeHoverChangeObservers(nodeHit);
		}
	}
	
    void Start() {
        viewCamera = Camera.main;
		grid = FindObjectOfType<Grid>();
		pollHit = false;
    }

    void Update() {
    	handleLayerHits();
    	if (Input.GetMouseButton(1)) {
    		pollHit = true;
    	}
    }
    
    void handleLayerHits() {
		foreach (Layer layer in layerPriorities) {
			var hit = RaycastForLayer(layer);
			if (hit.HasValue) {
				m_hit = hit.Value;
				if (m_layerHit != layer) {
					m_layerHit = layer;
					TriggerLayerChangeObservers(m_layerHit);
				}
				
				Node tmp;
				try {
					tmp = grid.NodeFromWorldPoint(hit.Value.point);
				} catch (Exception e) {
					return;
				}
				if (pollHit) {
					Debug.Log ("node poll: " + tmp.ToString());
					pollHit = false;
				}
				if (m_nodeHit != tmp) {
					m_nodeHit = tmp;
					TriggerNodeHoverChangeObservers(m_nodeHit);
				}
				
				return;
			}
		}
		
		// Otherwise return background hit
		m_hit.distance = distanceToBackground;
		m_layerHit = Layer.RaycastEndStop;
		TriggerLayerChangeObservers(m_layerHit);
    }

    RaycastHit? RaycastForLayer(Layer layer) {
        int layerMask = 1 << (int)layer; // See Unity docs for mask formation
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit; // used as an out parameter
        bool hasHit = Physics.Raycast(ray, out hit, distanceToBackground, layerMask);
        if (hasHit) {
            return hit;
        }
        return null;
    }
}
