using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIObjGroup : UIObj {

	protected List<UIObj> containedUIObjs = new List<UIObj>();

	public virtual void AddUIObj(UIObj obj) {
		containedUIObjs.Add(obj);
		if(screen != null) {
			screen.AddUIObj(obj);
		}
	}

	public virtual void RemoveUIObj(UIObj obj) {
		containedUIObjs.Remove(obj);
		if(screen != null) {
			screen.RemoveUIObj(obj);
		}
	}

	public override void OnAddedToScreen(Screen screen) {
		containedUIObjs.ForEach(screen.AddUIObj);
	}

	public override void OnRemovedFromScreen(Screen screen) {
		containedUIObjs.ForEach(screen.RemoveUIObj);
	}

	public override void Update() {
		base.Update();
		containedUIObjs.ForEach(uiObj => {
			uiObj.isVisible = this.isVisible; // 1 frame delayed, better add SetVisible method and override
		});
	}

	public override void Render() {
		// Intentionally empty
	}
}
